using DH.Save.Attributes;
using DH.Save.Editor;
using DH.Save.Editor.Exceptions;
using DH.Save.SerializableTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DH.Save
{
#if UNITY_EDITOR
    public class EditorUtils
    {
        private static Dictionary<FoldoutsDictionary, bool> _foldoutStates = new Dictionary<FoldoutsDictionary, bool>();
        //private static Dictionary<FoldoutsDictionary, ListElement> listElements = new Dictionary<FoldoutsDictionary, ListElement>();

        public static string DrawProperties(object obj, string key, string lastName = "")
        {
            FoldoutsDictionary index = new FoldoutsDictionary(key, -33, new string[] { lastName, "main" });
            if (obj == null) return index.GetName();
            //ShowStats(index);

            if (obj.GetType().IsPrimitive || obj is string || obj is float || obj is decimal || obj is double || obj is bool) // primitive type
            {
                if (obj is int)
                {
                    int value = (int)obj;
                    EditorGUILayout.IntField(FirstLetterToUpper("Value(int)"), value, GUILayout.ExpandWidth(true));
                }
                else if (obj is float || obj is decimal || obj is double)
                {
                    float value = (float)obj;
                    EditorGUILayout.FloatField(FirstLetterToUpper("Value(float)"), value, GUILayout.ExpandWidth(true));
                }
                else if (obj is string)
                {
                    string value = (string)obj;
                    EditorGUILayout.TextField(FirstLetterToUpper("Value(string)"), value, GUILayout.ExpandWidth(true));
                }
                else if (obj is bool)
                {
                    bool value = (bool)obj;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(FirstLetterToUpper("Value(bool)"));
                    EditorGUILayout.Toggle(value);
                    EditorGUILayout.EndHorizontal();
                }
            }
            else if (obj.GetType().IsGenericType && obj.GetType().GetGenericTypeDefinition() == typeof(List<>) || obj.GetType() == typeof(List<>)) // list 
            {
                var list = obj as System.Collections.IList;
                DrawList((object)list, key, index.GetName());
            }
            else if (obj.GetType().IsGenericType && obj.GetType().GetGenericTypeDefinition().IsArray || obj.GetType().IsArray) //  array
            {
                var list = obj as System.Collections.ArrayList;
                DrawList((object)list, key, index.GetName());
            }
            else if (obj.GetType().IsGenericType && obj.GetType().GetGenericTypeDefinition() == typeof(Dictionary<,>) || obj.GetType().IsGenericType && obj.GetType().GetGenericTypeDefinition() == typeof(SerializableDictionary<,>)) //  dictionary
            {
                TypeDictionary(obj, obj.GetType(), key, index.GetName(), "");
            }
            else if (obj.GetType() == typeof(DictionaryPair)) //  dictionary pair
            {
                DictionaryPair pair = obj as DictionaryPair;
                DrawDictionaryPair(pair.Key, key, index.GetName(), "Key");
                DrawDictionaryPair(pair.Value, key, index.GetName() + "2", "Value");
            }
            else if (obj.GetType() == typeof(SerializableTransform)) //  transform
            {
                try
                {
                    SerializableTransform serializableTransform = obj as SerializableTransform;
                    EditorGUILayout.Vector3Field("Position", SerializableTransform.ToVector3(serializableTransform.position));
                    EditorGUILayout.Vector3Field("Rotation", SerializableTransform.ToQuaternion(serializableTransform.rotation).eulerAngles);
                    EditorGUILayout.Vector3Field("Scale", SerializableTransform.ToVector3(serializableTransform.localScale));
                }
                catch
                {
                    throw new InvalidSerializableTransformException();
                }

            }
            else // other, like custom class with properties, etc.
            {

                BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance;
                var typeObj = obj.GetType();
                TypeConverter typeConverter = new TypeConverter();
                var memberType1 = typeConverter.GetConvertedEditorType(typeObj);
                if (memberType1 != null)
                {
                    typeObj = memberType1;
                    obj = Activator.CreateInstance(memberType1, new object[] { obj });
                }
                var fields = typeObj.GetFields(bindingFlags);
                var properties = typeObj.GetProperties(bindingFlags);

                int next = 0;

                // fields
                foreach (FieldInfo field in fields)
                {
                    FoldoutsDictionary fouldoutIndex = new FoldoutsDictionary(key, next, new string[] { lastName });
                    ProcessMember(obj, field, ref next, key, fouldoutIndex.GetName());
                }



                // properties
                foreach (PropertyInfo property in properties)
                {
                    FoldoutsDictionary fouldoutIndex = new FoldoutsDictionary(key, next, new string[] { lastName });
                    ProcessMember(obj, property, ref next, key, fouldoutIndex.GetName());
                }


            }

            EditorGUIUtility.labelWidth = 0;
            return index.GetName();
        }

        private static string ProcessMember(object obj, MemberInfo member, ref int next, string key, string lastName)
        {
            next++;
            var value = GetValue(member, obj);
            if (value == null) { EditorGUILayout.TextField(FirstLetterToUpper(member.Name), "NULL", GUILayout.ExpandWidth(true)); return lastName; }

            var hideSaveAttribute = member.GetCustomAttributes(typeof(HideSaveDataAttribute), false);
            if (hideSaveAttribute.Length > 0 || SaveSystem.ContainsJsonIgnoreAttribute(member)) return lastName;

            FoldoutsDictionary index = new FoldoutsDictionary(key, next, new string[] { lastName });

            Type memberType = GetMemberType(member);
            //ShowStats(index);

            TypeConverter typeConverter = new TypeConverter();
            if (memberType.IsGenericType && memberType.GetGenericTypeDefinition() == typeof(SerializableDictionary<,>))
            {
                
            }
            else
            {
                Type itemType = null;
                if (memberType.IsArray)
                {
                    itemType = memberType.GetElementType(); // array
                }
                else if (memberType.IsGenericType)
                {
                    itemType = memberType.GetGenericArguments()[0]; // list
                }

                if (itemType != null)
                {
                    var convertedType = typeConverter.GetConvertedEditorType(itemType) ?? itemType;
                    if (convertedType != null)
                    {
                        if (memberType.IsArray) // array
                        {
                            var objArray = (Array)value;
                            var length = objArray.Length;
                            var newArray = Array.CreateInstance(convertedType, length);

                            for (int i = 0; i < length; i++)
                            {
                                var item = objArray.GetValue(i);
                                var instance = Activator.CreateInstance(convertedType, new object[] { item });
                                newArray.SetValue(instance, i);
                            }
                            memberType = newArray.GetType();
                            value = newArray;
                        }
                        else // list
                        {
                            var enumerable = ((IEnumerable)value).Cast<object>();
                            memberType = typeof(List<>).MakeGenericType(new[] { convertedType });
                            var newList = Activator.CreateInstance(memberType);
                            foreach (var item in enumerable)
                            {
                                var instance = Activator.CreateInstance(convertedType, new object[] { item });
                                memberType.GetMethod("Add").Invoke(newList, new[] { instance });
                            }
                            value = newList;
                        }
                    }
                }
                else
                {
                    var memberType1 = typeConverter.GetConvertedEditorType(memberType);
                    if (memberType1 != null)
                    {
                        memberType = memberType1;
                        value = Activator.CreateInstance(memberType1, new object[] { value });
                    }
                }
            }


            if (memberType == typeof(int) || memberType == typeof(float) || memberType == typeof(decimal) || memberType == typeof(double) || memberType == typeof(string))
            {
                if (memberType == typeof(int))
                {
                    int valueF = (int)value;
                    EditorGUILayout.IntField(FirstLetterToUpper(member.Name), valueF, GUILayout.ExpandWidth(true));
                }
                else if (memberType == typeof(float) || memberType == typeof(decimal) || memberType == typeof(double))
                {
                    float valueF = (float)value;
                    EditorGUILayout.FloatField(FirstLetterToUpper(member.Name), valueF, GUILayout.ExpandWidth(true));
                }
                else if (memberType == typeof(string))
                {
                    string valueF = (string)value;
                    EditorGUILayout.TextField(FirstLetterToUpper(member.Name), valueF, GUILayout.ExpandWidth(true));
                }
            }
            else if (memberType == typeof(bool))
            {
                bool valueF = (bool)value;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(FirstLetterToUpper(member.Name));
                EditorGUILayout.Toggle(valueF);
                EditorGUILayout.EndHorizontal();
            }
            else if (memberType.IsGenericType && memberType.GetGenericTypeDefinition() == typeof(List<>) || memberType == typeof(List<>))
            {
                var list = value as System.Collections.IList;

                if (list.Count > 0) DrawList(list, key, index.GetName(), FirstLetterToUpper(member.Name));
            }
            else if (memberType.IsGenericType && memberType.GetGenericTypeDefinition().IsArray || memberType.IsArray)
            {
                var list = ((Array)value);

                if (list.Length > 0) DrawList(list, key, index.GetName(), FirstLetterToUpper(member.Name));
            }
            else if (memberType.IsGenericType && memberType.GetGenericTypeDefinition() == typeof(Dictionary<,>) || memberType.IsGenericType && memberType.GetGenericTypeDefinition() == typeof(SerializableDictionary<,>))
            {
                TypeDictionary(value, memberType, key, index.GetName(), FirstLetterToUpper(member.Name));
            }
            else if (memberType.IsClass)
            {
                // recursive call for classes
                string listName = FirstLetterToUpper(member.Name);
                FoldoutsDictionary foldoutKey = new FoldoutsDictionary(key, -775, new string[] { index.GetName(), "class" });
                if (!_foldoutStates.ContainsKey(foldoutKey))
                {
                    _foldoutStates[foldoutKey] = false;
                }

                _foldoutStates[foldoutKey] = EditorGUILayout.Foldout(_foldoutStates[foldoutKey], listName != "" ? listName : key, true);

                CheckOptionMenu(foldoutKey);
                if (_foldoutStates[foldoutKey])
                {
                    EditorGUI.indentLevel++;
                    DrawProperties(value, key, foldoutKey.GetName());
                    EditorGUI.indentLevel--;
                }

            }
            else
            {
                try
                {
                    EditorGUILayout.TextField(FirstLetterToUpper(member.Name), value.ToString(), GUILayout.ExpandWidth(true));
                }
                catch { }
            }
            return index.GetName() + "member";
        }

        private static object GetValue(MemberInfo member, object obj)
        {
            switch (member)
            {
                case FieldInfo field:
                    return field.GetValue(obj);
                case PropertyInfo property:
                    return property.GetValue(obj);
                default:
                    return null;
            }
        }

        private static Type GetMemberType(MemberInfo member)
        {
            switch (member)
            {
                case FieldInfo field:
                    return field.FieldType;
                case PropertyInfo property:
                    return property.PropertyType;
                default:
                    return null;
            }
        }

        public static string DrawDictionaryPair(object obj, string key, string lastName, string listName)
        {
            FoldoutsDictionary index = new FoldoutsDictionary(key, -88, new string[] { lastName, "pair" });

            if (!_foldoutStates.ContainsKey(index))
            {
                _foldoutStates[index] = false;
            }

            string foldout = listName;

            _foldoutStates[index] = EditorGUILayout.Foldout(_foldoutStates[index], foldout, true);

            CheckOptionMenu(index);
            //ShowStats(index);

            if (_foldoutStates[index])
            {
                EditorGUI.indentLevel++;
                DrawProperties(obj, key, index.GetName());
                EditorGUI.indentLevel--;
            }

            return index.GetName() + "k";
        }

        public static string TypeDictionary(object obj, Type type, string key, string lastName, string listName)
        {
            Type[] arguments = type.GetGenericArguments();
            Type keyType = arguments[0];   // T
            Type valueType = arguments[1]; // U

            MethodInfo getDictionaryListMethod = null;
            Type serializableDictType = null;
            object serializableDict = null;

            serializableDictType = typeof(SerializableDictionary<,>).MakeGenericType(keyType, valueType);
            if (type.GetGenericTypeDefinition() == typeof(SerializableDictionary<,>))
            {
                ConstructorInfo constructorInfo = serializableDictType.GetConstructor(new Type[] { serializableDictType });
                if (constructorInfo != null)
                {
                    serializableDict = constructorInfo.Invoke(new object[] { obj });
                }
            }
            else
            {
                serializableDict = Activator.CreateInstance(serializableDictType, new object[] { obj });
            }



            getDictionaryListMethod = serializableDict.GetType().GetMethod("GetDictionaryList", new Type[] { });
            var list = getDictionaryListMethod.Invoke(serializableDict, null);

            Type listType = list.GetType();
            Type dictionaryPairType = listType.GetGenericArguments()[0];

            Type[] genericArguments = dictionaryPairType.GetGenericArguments();

            keyType = genericArguments[0];
            valueType = genericArguments[1];

            MethodInfo drawDictionaryMethod = typeof(EditorUtils)
            .GetMethod("DrawDictionary", BindingFlags.Public | BindingFlags.Static);
            MethodInfo genericDrawDictionaryMethod = drawDictionaryMethod
            .MakeGenericMethod(new Type[] { keyType, valueType });
            //var list1 = getDictionaryListMethod.Invoke(serializableDict, null);
            lastName = (string)genericDrawDictionaryMethod.Invoke(null, new object[] { list, key, lastName, listName });
            return lastName + "k";
        }

        private static void DrawListElement(FoldoutsDictionary foldoutsDictionary, int count, Rect lastRect)
        {
            //GUILayout.FlexibleSpace();
            //if (!listElements.ContainsKey(foldoutsDictionary))
            //{
            //    listElements[foldoutsDictionary] = new ListElement(true, 0, count);
            //}

            // Checkbox
            //listElements[foldoutsDictionary].checkbox = EditorGUILayout.Toggle(listElements[foldoutsDictionary].checkbox, GUILayout.Width(20));

            // If checkbox is checked, show int fields
            //GUI.enabled = !listElements[foldoutsDictionary].checkbox;
            //EditorGUILayout.LabelField("Start");
            //listElements[foldoutsDictionary].start = EditorGUILayout.IntField(listElements[foldoutsDictionary].start);
            //if (listElements[foldoutsDictionary].start < 0) listElements[foldoutsDictionary].start = 0;

            //EditorGUILayout.LabelField("End");
            //listElements[foldoutsDictionary].end = EditorGUILayout.IntField(listElements[foldoutsDictionary].end);
            //if (listElements[foldoutsDictionary].end > count) listElements[foldoutsDictionary].end = count;
            //GUI.enabled = true;
            GUILayout.EndHorizontal();
        }

        public static string DrawList(object obj, string key, string lastName, string listName = "")
        {
            var list = obj as System.Collections.IList;
            FoldoutsDictionary foldoutKey = new FoldoutsDictionary(key, -66, new string[] { lastName, "list" });
            if (!_foldoutStates.ContainsKey(foldoutKey))
            {
                _foldoutStates[foldoutKey] = false;
            }

            _foldoutStates[foldoutKey] = EditorGUILayout.Foldout(_foldoutStates[foldoutKey], listName != "" ? listName : key, true);



            CheckOptionMenu(foldoutKey);
            //ShowStats(foldoutKey);

            EditorGUI.indentLevel++;


            if (list != null && _foldoutStates[foldoutKey])
            {
                int start = 0;//!listElements[foldoutKey].checkbox ? listElements[foldoutKey].start : 0;
                int end = list.Count;// !listElements[foldoutKey].checkbox ? listElements[foldoutKey].end : list.Count;
                for (int i = start; i < end; i++)
                {
                    FoldoutsDictionary index = new FoldoutsDictionary(key, i, new string[] { foldoutKey.GetName() });

                    if (!_foldoutStates.ContainsKey(index))
                    {
                        _foldoutStates[index] = false;
                    }


                    _foldoutStates[index] = EditorGUILayout.Foldout(_foldoutStates[index], "Element " + i, true);



                    CheckOptionMenu(index);
                    //ShowStats(index);

                    if (_foldoutStates[index])
                    {
                        EditorGUI.indentLevel++;
                        DrawProperties(list[i], key, index.GetName());
                        EditorGUI.indentLevel--;
                    }
                }

            }

            EditorGUI.indentLevel--;
            return foldoutKey.GetName() + "k";
        }

        public static string DrawDictionary<TKey, TValue>(List<DictionaryPair<TKey, TValue>> obj, string key, string lastName, string listName = "")
        {
            FoldoutsDictionary foldoutKey = new FoldoutsDictionary(key, -55, new string[] { lastName, "dict" });

            if (!_foldoutStates.ContainsKey(foldoutKey))
            {
                _foldoutStates[foldoutKey] = false;
            }

            _foldoutStates[foldoutKey] = EditorGUILayout.Foldout(_foldoutStates[foldoutKey], listName != "" ? listName : key, true);


            CheckOptionMenu(foldoutKey);

            EditorGUI.indentLevel++;

            if (_foldoutStates[foldoutKey])
            {

                int start = 0;// !listElements[foldoutKey].checkbox ? listElements[foldoutKey].start : 0;
                int end = obj.Count;// !listElements[foldoutKey].checkbox ? listElements[foldoutKey].end : obj.Count;
                for (int i = start; i < end; i++)
                {
                    FoldoutsDictionary index = new FoldoutsDictionary(key, i, new string[] { foldoutKey.GetName() });

                    if (!_foldoutStates.ContainsKey(index))
                    {
                        _foldoutStates[index] = false;
                    }

                    string foldout = "Element " + i;
                    try
                    {
                        foldout = obj[i].Key.ToString();
                    }
                    catch { }

                    _foldoutStates[index] = EditorGUILayout.Foldout(_foldoutStates[index], foldout, true);

                    CheckOptionMenu(index);
                    // ShowStats(index);

                    if (_foldoutStates[index])
                    {
                        EditorGUI.indentLevel++;
                        var newValue = new DictionaryPair(obj[i].Key, obj[i].Value);
                        DrawProperties(obj[i].Key.GetType().IsPrimitive ? obj[i].Value : newValue, key, index.GetName());
                        EditorGUI.indentLevel--;
                    }
                }

            }

            EditorGUI.indentLevel--;
            return foldoutKey.GetName() + "k";
        }

        public class DictionaryPair
        {
            public object Key;
            public object Value;

            public DictionaryPair(object key, object value)
            {
                Key = key;
                Value = value;
            }
        }

        public static bool CheckOptionMenu(FoldoutsDictionary foldout)
        {
            if (Event.current.type == EventType.ContextClick)
            {
                Rect clickArea = GUILayoutUtility.GetLastRect();
                if (clickArea.Contains(Event.current.mousePosition))
                {
                    GenericMenu menu = new GenericMenu();

                    menu.AddItem(new GUIContent("Collapse"), false, () => CloseUnderAllFoldouts(foldout.Key, foldout.Id, foldout.GetName()));

                    menu.ShowAsContext();

                    Event.current.Use();
                    return true;
                }
            }
            return false;
        }

        public static bool ShowStats(FoldoutsDictionary foldout)
        {
            if (Event.current.type == EventType.ContextClick)
            {
                Rect clickArea = GUILayoutUtility.GetLastRect();
                if (clickArea.Contains(Event.current.mousePosition))
                {
                    GenericMenu menu = new GenericMenu();

                    menu.AddItem(new GUIContent("Key:" + foldout.Key), false, null);
                    menu.AddItem(new GUIContent("UniqueIdentifier:" + foldout.Id), false, null);
                    menu.AddItem(new GUIContent("Names:"), false, null);
                    foreach (var item in foldout.Names)
                    {
                        menu.AddItem(new GUIContent("-   :" + item), false, null);
                    }

                    menu.ShowAsContext();

                    Event.current.Use();
                    return true;
                }
            }
            return false;
        }

        public static void CloseUnderAllFoldouts(string startKey, int id, string names)
        {
            for (int i = 0; i < _foldoutStates.Keys.Count; i++)
            {
                FoldoutsDictionary item = _foldoutStates.Keys.ToList()[i];
                if (item.Key == startKey)
                {
                    if (item.GetName().Contains(names))
                    {
                        _foldoutStates[item] = false;
                    }
                }
            }

        }
        public static Rect CalculateBoxRect(int totalBoxes, int boxIndex, float boxHeight, float yPosition, float startX)
        {
            if (totalBoxes <= 0)
            {
                throw new System.ArgumentException("Total boxes must be greater than zero.");
            }
            if (boxIndex < 0 || boxIndex >= totalBoxes)
            {
                throw new System.ArgumentOutOfRangeException("Box index must be within the range of total boxes.");
            }


            // Get the current width of the EditorGUI window or container
            float containerWidth = (SaveSystemSettingsEditor.currentWidth != 0 ? SaveSystemSettingsEditor.currentWidth : EditorGUIUtility.currentViewWidth) - (boxIndex == 0 ? startX : 35); // Get the current width of the EditorGUI window or container
            float boxWidth = (containerWidth) / totalBoxes; // Divide the available width minus startX by the number of boxes to get the width of each box

            // Calculate the x position of the box based on its index and the startX
            float xPosition = (boxIndex == 0 ? startX : (32 / (totalBoxes - 1))) + boxWidth * boxIndex;


            // Return the calculated Rect
            return new Rect(xPosition, yPosition, boxWidth, boxHeight);
        }

        public static string FirstLetterToUpper(string str)
        {
            int indent = EditorGUI.indentLevel * 15;
            Vector2 labelSize = GUI.skin.label.CalcSize(new GUIContent(str));

            EditorGUIUtility.labelWidth = labelSize.x + 10 + indent;
            if (str == null)
                return null;

            if (str.Length > 1)
                return char.ToUpper(str[0]) + str.Substring(1);


            return str.ToUpper();
        }

        public class FoldoutsDictionary
        {
            public string Key { get; set; }
            public int Id { get; set; }
            public string[] Names = new string[3] { "", "", "" };

            public string GetName() => Id + GetNames();

            private string GetNames()
            {
                string result = "";
                foreach (var item in Names)
                {
                    if (item != "") result += item;
                }
                return result;
            }

            public FoldoutsDictionary(string key, int id, string[] lastNames)
            {
                Key = key.Length > 0 ? key.Replace("`", "")[..(key.Length > 5 ? 5 : key.Length)] : key.Replace("`", "");
                Id = id;
                for (int i = 0; i < (lastNames.Length > 5 ? 5 : lastNames.Length); i++)
                {
                    Names[i] = lastNames[i].Replace("`", "");
                }
            }

            public bool Contains(string key)
            {
                foreach (var item in Names)
                {
                    if (item.Contains(key))
                    {
                        return true;
                    }
                }
                return false;
            }


            public override bool Equals(object obj)
            {
                if (obj is FoldoutsDictionary dictionary)
                {
                    if (Names.Length != dictionary.Names.Length) return false;
                    for (int i = 0; i < Names.Length; i++)
                    {
                        if (Names[i] != dictionary.Names[i])
                        {
                            return false;
                        }
                    }
                    return Key == dictionary.Key && Id == dictionary.Id;
                }
                return false;
            }

            public override int GetHashCode()
            {
                var hash = new HashCode();
                hash.Add(Key);
                hash.Add(Id);
                foreach (var name in Names)
                {
                    hash.Add(name);
                }
                return hash.ToHashCode();
            }
        }
        private class ListElement
        {
            public bool checkbox;
            public int start;
            public int end;


            public ListElement(bool checkbox, int start, int end)
            {
                this.checkbox = checkbox;
                this.start = start;
                this.end = end;
            }
        }
    }
#endif
}