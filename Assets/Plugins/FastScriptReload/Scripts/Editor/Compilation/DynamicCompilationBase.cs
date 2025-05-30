﻿using FastScriptReload.Editor.Compilation.CodeRewriting;
using FastScriptReload.Editor.Compilation.ScriptGenerationOverrides;
using FastScriptReload.Runtime;
using FastScriptReload.Scripts.Runtime;
using ImmersiveVRTools.Editor.Common.Cache;
using ImmersiveVrToolsCommon.Runtime.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace FastScriptReload.Editor.Compilation
{
    [InitializeOnLoad]
    public class DynamicCompilationBase
    {
        public static bool DebugWriteRewriteReasonAsComment;
        public static bool LogHowToFixMessageOnCompilationError;
        public static bool EnableExperimentalThisCallLimitationFix;
        public static List<string> ReferencesExcludedFromHotReload = new List<string>();

        public const string DebuggingInformationComment =
            @"// DEBUGGING READ-ME " +
#if !UNITY_2021_1_OR_NEWER
"WARN: on Unity versions prior to 2021, opening files in that manner can cause static values to be reinitialized"
#else
            ""
#endif
            +
            @"//
// To debug simply add a breakpoint in this file.
// 
// With every code change - new file is generated, currently you'll need to re-set breakpoints after each change.
// You can also:
//    - step into the function that was changed (and that will get you to correct source file)
//    - add a function breakpoint in your IDE (this way you won't have to re-add it every time)
//
// Tool can automatically open dynamically-compiled code file every time to make setting breakpoints easier.
// You can adjust that behaviour via 'Window -> FastScriptReload -> Start Screen -> Debugging -> Do not auto-open generated cs file'.
//
// You can always open generated file when needed by clicking link in console, eg.
// 'FSR: Files: FunctionLibrary.cs changed (click here to debug [in bottom details pane]) - compilation (took 240ms)'


";

        public static readonly string[] ActiveScriptCompilationDefines;
        protected static readonly string DynamicallyCreatedAssemblyAttributeSourceCode = $"[assembly: {typeof(DynamicallyCreatedAssemblyAttribute).FullName}()]";
        private static readonly string AssemblyCsharpFullPath;

        static DynamicCompilationBase()
        {
            //needs to be set from main thread
            ActiveScriptCompilationDefines = EditorUserBuildSettings.activeScriptCompilationDefines;
            AssemblyCsharpFullPath = SessionStateCache.GetOrCreateString(
                $"FSR:AssemblyCsharpFullPath",
                () => AssetDatabase.FindAssets("Microsoft.CSharp")
                                .Select(g => new System.IO.FileInfo(UnityEngine.Application.dataPath + "/../" + AssetDatabase.GUIDToAssetPath(g)))
                                .First(fi => fi.Name.ToLower() == "Microsoft.CSharp.dll".ToLower()).FullName
            );

        }

        protected static string CreateSourceCodeCombinedContents(List<string> sourceCodeFiles, List<string> definedPreprocessorSymbols)
        {
            var combinedUsingStatements = new List<string>();

            var sourceCodeWithAdjustments = sourceCodeFiles.Select(sourceCodeFile =>
            {
                var fileCode = File.ReadAllText(sourceCodeFile);
                var tree = CSharpSyntaxTree.ParseText(fileCode, new CSharpParseOptions(preprocessorSymbols: definedPreprocessorSymbols));
                var root = tree.GetRoot();

                var typeToNewFieldDeclarations = new Dictionary<string, List<string>>();
                if(FastScriptReloadManager.Instance.AssemblyChangesLoaderEditorOptionsNeededInBuild.EnableExperimentalAddedFieldsSupport)
                {
                    //WARN: needs to walk before root class name changes, otherwise it'll resolve wrong name
                    var fieldsWalker = new FieldsWalker();
                    fieldsWalker.Visit(root);

                    var typeToFieldDeclarations = fieldsWalker.GetTypeToFieldDeclarations();
                    typeToNewFieldDeclarations = typeToFieldDeclarations.ToDictionary(
                        t => t.Key,
                        t =>
                        {
                            if(!ProjectTypeCache.AllTypesInNonDynamicGeneratedAssemblies.TryGetValue(t.Key, out var existingType))
                            {
                                LoggerScoped.LogDebug($"Unable to find type: {t.Key} in loaded assemblies. If that's the class you've added field to then it may not be properly working. It's possible the class was not yet loaded / used and you can ignore that warning. If it's causing any issues please contact support");
                                return new List<string>();
                            }

                            var existingTypeMembersToReplace = NewFieldsRewriter.GetReplaceableMembers(existingType).Select(m => m.Name).ToList();

                            var newFields = t.Value.Where(fD => !existingTypeMembersToReplace.Contains(fD.FieldName)).ToList();

                            //TODO: ideally that registration would happen outside of this class
                            //TODO: to work for LSR it needs to be handled in runtime
                            TemporaryNewFieldValues.RegisterNewFields(
                                existingType,
                                newFields.ToDictionary(
                                    fD => fD.FieldName,
                                    fD => new TemporaryNewFieldValues.GetNewFieldInitialValue((Type forNewlyGeneratedType) =>
                                    {
                                        //TODO: PERF: could cache those - they run to init every new value (for every instance when accessed)
                                        return CreateNewFieldInitMethodRewriter.ResolveNewFieldsToCreateValueFn(forNewlyGeneratedType)[fD.FieldName]();
                                    })
                                ),
                                newFields.ToDictionary(
                                    fD => fD.FieldName,
                                    fD => new TemporaryNewFieldValues.GetNewFieldType((Type forNewlyGeneratedType) =>
                                    {
                                        //TODO: PERF: could cache those - they run to init every new value (for every instance when accessed)
                                        return (Type)CreateNewFieldInitMethodRewriter.ResolveNewFieldsToTypeFn(forNewlyGeneratedType)[fD.FieldName]();
                                    })
                                )
                            );

                            return newFields.Select(fD => fD.FieldName).ToList();
                        }
                    );

#if LiveScriptReload_Enabled
                    if (typeToNewFieldDeclarations.Any(kv => kv.Value.Any()))
                    {
                        LoggerScoped.LogWarning($"{nameof(FastScriptReloadManager.Instance.AssemblyChangesLoaderEditorOptionsNeededInBuild.EnableExperimentalAddedFieldsSupport)} is enabled. This is not supported in running build. Quite likely it'll crash remote client.");
                    }
#endif
                }

                //WARN: application order is important, eg ctors need to happen before class names as otherwise ctors will not be recognised as ctors
                if(FastScriptReloadManager.Instance.EnableExperimentalThisCallLimitationFix)
                {
                    root = new ThisCallRewriter(DebugWriteRewriteReasonAsComment).Visit(root);
                    root = new ThisAssignmentRewriter(DebugWriteRewriteReasonAsComment).Visit(root);
                }

                if(FastScriptReloadManager.Instance.AssemblyChangesLoaderEditorOptionsNeededInBuild.EnableExperimentalAddedFieldsSupport)
                {
                    root = new NewFieldsRewriter(typeToNewFieldDeclarations, DebugWriteRewriteReasonAsComment).Visit(root);
                    root = new CreateNewFieldInitMethodRewriter(typeToNewFieldDeclarations, DebugWriteRewriteReasonAsComment).Visit(root);
                }

                root = new ConstructorRewriter(adjustCtorOnlyForNonNestedTypes: true, DebugWriteRewriteReasonAsComment).Visit(root);

                var hotReloadCompliantRewriter = new HotReloadCompliantRewriter(DebugWriteRewriteReasonAsComment);
                root = hotReloadCompliantRewriter.Visit(root);
                combinedUsingStatements.AddRange(hotReloadCompliantRewriter.StrippedUsingDirectives);

                //processed as last step to simply rewrite all changes made before
                if(TryResolveUserDefinedOverridesRoot(sourceCodeFile, definedPreprocessorSymbols, out var userDefinedOverridesRoot))
                {
                    root = ProcessUserDefinedOverridesReplacements(sourceCodeFile, root, userDefinedOverridesRoot);
                    root = AddUserDefinedOverridenTypes(userDefinedOverridesRoot, root);
                }

                return root.ToFullString();
            }).ToList();

            var sourceCodeCombinedSb = new StringBuilder();
            sourceCodeCombinedSb.Append(DebuggingInformationComment);

            foreach(var usingStatement in combinedUsingStatements.Distinct())
            {
                sourceCodeCombinedSb.Append(usingStatement);
            }

            foreach(var sourceCodeWithAdjustment in sourceCodeWithAdjustments)
            {
                sourceCodeCombinedSb.AppendLine(sourceCodeWithAdjustment);
            }

            LoggerScoped.LogDebug("Source Code Created:\r\n\r\n" + sourceCodeCombinedSb);
            return sourceCodeCombinedSb.ToString();
        }

        private static SyntaxNode AddUserDefinedOverridenTypes(SyntaxNode userDefinedOverridesRoot, SyntaxNode root)
        {
            try
            {
                var userDefinedOverrideTypes = userDefinedOverridesRoot.DescendantNodes().OfType<TypeDeclarationSyntax>()
                    .ToDictionary(n => RoslynUtils.GetMemberFQDN(n, n.Identifier.ToString()));
                var allDefinedTypesInRecompiledFile = root.DescendantNodes().OfType<TypeDeclarationSyntax>()
                    .ToDictionary(n => RoslynUtils.GetMemberFQDN(n, n.Identifier.ToString())); //what about nested types?

                var userDefinedOverrideTypesWithoutMatchnigInRecompiledFile = userDefinedOverrideTypes.Select(overridenType =>
                    {
                        if(!allDefinedTypesInRecompiledFile.ContainsKey(overridenType.Key))
                        {
                            return overridenType;
                        }

                        return default(KeyValuePair<string, TypeDeclarationSyntax>);
                    })
                    .Where(kv => kv.Key != default(string))
                    .ToList();

                //types should be added either to root namespace or root of document
                var rootNamespace = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
                foreach(var overridenTypeToAdd in userDefinedOverrideTypesWithoutMatchnigInRecompiledFile)
                {
                    var newMember = FastScriptReloadCodeRewriterBase.AddRewriteCommentIfNeeded(overridenTypeToAdd.Value,
                        "New type defined in override file",
                        true, //always write reason so it's not easy to miss in generated file
                        true);
                    if(rootNamespace != null)
                    {
                        rootNamespace =
                            root.DescendantNodes().OfType<NamespaceDeclarationSyntax>()
                                .FirstOrDefault(); //need to search again to make sure it didn't change
                        var newRootNamespace = rootNamespace.AddMembers(newMember);
                        root = root.ReplaceNode(rootNamespace, newRootNamespace);
                    }
                    else
                    {
                        root = ((CompilationUnitSyntax)root).AddMembers(newMember);
                    }
                }
            }
            catch(Exception e)
            {
                Debug.LogError($"Unable to add user defined override types. {e}");
            }

            return root;
        }

        private static bool TryResolveUserDefinedOverridesRoot(string sourceCodeFile, List<string> definedPreprocessorSymbols, out SyntaxNode userDefinedOverridesRoot)
        {
            if(ScriptGenerationOverridesManager.TryGetScriptOverride(new FileInfo(sourceCodeFile), out var userDefinedOverridesFile))
            {
                try
                {
                    userDefinedOverridesRoot = CSharpSyntaxTree.ParseText(File.ReadAllText(userDefinedOverridesFile.FullName), new CSharpParseOptions(preprocessorSymbols: definedPreprocessorSymbols)).GetRoot();
                    return true;
                }
                catch(Exception ex)
                {
                    Debug.LogError($"Unable to resolve user defined overrides for file: '{userDefinedOverridesFile.FullName}' - please make sure it's compilable. Error: '{ex}'");
                }
            }

            userDefinedOverridesRoot = null;
            return false;
        }

        private static SyntaxNode ProcessUserDefinedOverridesReplacements(string sourceCodeFile, SyntaxNode root, SyntaxNode userDefinedOverridesRoot)
        {
            if(ScriptGenerationOverridesManager.TryGetScriptOverride(new FileInfo(sourceCodeFile), out var userDefinedOverridesFile))
            {
                try
                {
                    var userDefinedScriptOverridesRewriter = new ManualUserDefinedScriptOverridesRewriter(userDefinedOverridesRoot,
                        true); //always write rewrite reason so it's not easy to miss
                    root = userDefinedScriptOverridesRewriter.Visit(root);
                }
                catch(Exception ex)
                {
                    Debug.LogError($"Unable to resolve user defined overrides for file: '{userDefinedOverridesFile.FullName}' - please make sure it's compilable. Error: '{ex}'");
                }
            }

            return root;
        }

        protected static List<string> ResolveReferencesToAdd(List<string> excludeAssyNames)
        {
            var referencesToAdd = new List<string>();
            foreach(var assembly in AppDomain.CurrentDomain
                         .GetAssemblies() //TODO: PERF: just need to load once and cache? or get assembly based on changed file only?
                         .Where(a => excludeAssyNames.All(assyName => !a.FullName.StartsWith(assyName))
                                                && CustomAttributeExtensions.GetCustomAttribute<DynamicallyCreatedAssemblyAttribute>((Assembly)a) == null))
            {
                try
                {
                    if(string.IsNullOrEmpty(assembly.Location))
                    {
                        LoggerScoped.LogDebug($"FastScriptReload: Assembly location is null, usually dynamic assembly, harmless.");
                        continue;
                    }

                    referencesToAdd.Add(assembly.Location);
                }
                catch(Exception)
                {
                    LoggerScoped.LogDebug($"Unable to add a reference to assembly as unable to get location or null: {assembly.FullName} when hot-reloading, this is likely dynamic assembly and won't cause issues");
                }
            }

            referencesToAdd = referencesToAdd.Where(r => !ReferencesExcludedFromHotReload.Any(rTe => r.EndsWith(rTe))).ToList();

            if(EnableExperimentalThisCallLimitationFix || FastScriptReloadManager.Instance.AssemblyChangesLoaderEditorOptionsNeededInBuild.EnableExperimentalAddedFieldsSupport)
            {
                IncludeMicrosoftCsharpReferenceToSupportDynamicKeyword(referencesToAdd);
            }

            return referencesToAdd;
        }

        private static void IncludeMicrosoftCsharpReferenceToSupportDynamicKeyword(List<string> referencesToAdd)
        {
            //TODO: check .net4.5 backend not breaking?
            //ThisRewriters will cast to dynamic - if using .NET Standard 2.1 - reference is required
            referencesToAdd.Add(AssemblyCsharpFullPath);
            // referencesToAdd.Add(@"C:\Program Files\Unity\Hub\Editor\2021.3.12f1\Editor\Data\UnityReferenceAssemblies\unity-4.8-api\Microsoft.CSharp.dll");
        }
    }
}