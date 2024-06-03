using System.Collections.Generic;
using UnityEditor;
namespace DH.Save.Editor
{
    static class SaveSystemSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {

            var provider = new SettingsProvider("Project/DHSaveSystemSetting", SettingsScope.Project)
            {
                label = "DH Save System Settings",
                guiHandler = (searchContext) =>
                {
#if UNITY_EDITOR
                    //// Automatically draw all the properties
                    SaveSystemSettingsEditor.DrawGUI(true);
#endif
                },

                // Optionally specify keywords for search functionality
                keywords = new HashSet<string>(new[] { "Custom", "DH Save System Settings" })
            };

            return provider;
        }

    }
}