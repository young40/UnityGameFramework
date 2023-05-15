using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;
using System.Text;
using System.IO;
using System.Linq;

namespace UnityGameFramework.Editor
{
    [InitializeOnLoad]
    public abstract partial class DynamicMenuItem : MonoBehaviour
    {
        static DynamicMenuItem()
        {
            ReGeneratorDynamicMenuitemCSharpScript();
        }

        private static void ReGeneratorDynamicMenuitemCSharpScript()
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder sbInitCall = new StringBuilder();

            foreach (Assembly assembly in GetAssemblies())
            {
                MethodInfo[] methods = GetMethods(assembly);

                foreach (MethodInfo method in methods)
                {
                    GetMenuItemCode(method, sb, sbInitCall);
                }
            }

            string className = System.IO.Path.GetFileNameWithoutExtension(csharpScriptPath);
            string classStr = classTpl.Replace("__REPLACE_CLASS_NAME", className);

            classStr = classStr.Replace("__REPLACE_METHOD_BODY", sb.ToString());
            classStr = classStr.Replace("__REPLACE_INIT_CALL", sbInitCall.ToString());

            File.WriteAllText(csharpScriptPath, classStr);

            string relativePath = Path.GetRelativePath(csharpScriptPath, Path.GetDirectoryName(Application.dataPath));
            AssetDatabase.ImportAsset(relativePath);
        }

        private static Assembly[] GetAssemblies()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            return assemblies;
        }

        private static MethodInfo[] GetMethods(Assembly assembly)
        {
            List<MethodInfo> methods = new List<MethodInfo>();

            foreach (System.Type type in assembly.GetTypes())
            {
                MethodInfo[] methodInfos = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);

                foreach (MethodInfo method in methodInfos)
                {
                    DynamicMenuItemAttribute attr = method.GetCustomAttribute<DynamicMenuItemAttribute>();
                    if (attr != null)
                    {
                        if (CheckMethod(method))
                        {
                            methods.Add(method);
                        }
                    }
                }
            }

            return methods.ToArray();
        }

        private static void GetMenuItemCode(MethodInfo method, StringBuilder sb, StringBuilder sbInitCall)
        {
            GroupMenuItemAttribute groupMenuItemAttribute = method.GetCustomAttribute<GroupMenuItemAttribute>();
            if (groupMenuItemAttribute != null)
            {
                GetGroupMenuItemCode(method, sb, sbInitCall);
                return;
            }

            DynamicMenuItemAttribute dynmicAttr = method.GetCustomAttribute<DynamicMenuItemAttribute>();

            string code = methodTpl.Replace("__REPLACE_MENU_PATH", $"{menuPrex}{dynmicAttr.ItemName}");

            code = code.Replace("__REPLACE_MENU_PRIORITY", dynmicAttr.Priority.ToString());

            code = code.Replace("__REPLACE_MENU_METHOD_NAME", $"{method.DeclaringType.FullName}_{method.Name}".Replace(".", "_"));
            code = code.Replace("__REPLACE_MENU_CALL", $"{method.DeclaringType.FullName}.{method.Name}()");

            string valueSettingStr = "";
            {
                BoolMenuItemAttribute attr = method.GetCustomAttribute<BoolMenuItemAttribute>();
                if (attr != null)
                {
                    if (!PrefHasKey(attr.Key))
                    {
                        PrefSetBool(attr.Key, attr.DefaultValue);
                    }

                    valueSettingStr = @$"UnityGameFramework.Editor.DynamicMenuItem.ToggleBoolMenuChecked(""{menuPrex}{dynmicAttr.ItemName}"", ""{attr.Key}"");";

                    string initStr = @$"            UnityGameFramework.Editor.DynamicMenuItem.SetBoolMenuChecked(""{menuPrex}{dynmicAttr.ItemName}"", ""{attr.Key}"");";
                    sbInitCall.AppendLine(initStr);
                }

                code = code.Replace("__REPLACE_MENU_UPDATE", valueSettingStr);
            }

            sb.AppendLine(code);
        }

        private static void GetGroupMenuItemCode(MethodInfo method, StringBuilder sb, StringBuilder sbInitCall)
        {
            GroupMenuItemAttribute group = method.GetCustomAttribute<GroupMenuItemAttribute>();
            DynamicMenuItemAttribute dynmicAttr = method.GetCustomAttribute<DynamicMenuItemAttribute>();
            Debug.Log(group);

            string nameJoin = string.Join(',', group.Names).Replace(",", "\",\"");
            string valuesJoin = string.Join(',', group.Values).Replace(",", "\",\"");

            string nameStr = $"new string[] {{ \"{nameJoin}\" }} ";
            string valueStr = $"new string[] {{ \"{valuesJoin}\" }} ";

            if (!PrefHasKey(group.Key))
            {
                PrefSetString(group.Key, group.DefaultValue);
            }

            for (int i = 0; i < group.Values.Length; i++)
            {
                string name = group.Names[i];
                string value = group.Values[i];

                string code = methodTpl.Replace("__REPLACE_MENU_PATH", $"{menuPrex}{dynmicAttr.ItemName}/{name}");

                code = code.Replace("__REPLACE_MENU_PRIORITY", (dynmicAttr.Priority + i).ToString());

                code = code.Replace("__REPLACE_MENU_METHOD_NAME",
                    $"{method.DeclaringType.FullName}_{method.Name}_{value}".Replace(".", "_"));
                code = code.Replace("__REPLACE_MENU_CALL",
                    @$"{method.DeclaringType.FullName}.{method.Name}(""{value}"")");

                string valueSettingStr = "";
                {
                    valueSettingStr = $@"
        UnityGameFramework.Editor.DynamicMenuItem.PrefSetString(""{group.Key}"", ""{value}"");
        UnityGameFramework.Editor.DynamicMenuItem.SetGroupMenuChecked(""{menuPrex}{dynmicAttr.ItemName}"", ""{group.Key}"", {valueStr}, ""{group.DefaultValue}"" , {nameStr});
";
                    code = code.Replace("__REPLACE_MENU_UPDATE", valueSettingStr);
                }

                sb.AppendLine(code);
            }

            {
                string valueSettingStr = "";
                {
                    valueSettingStr = $@"
                        UnityGameFramework.Editor.DynamicMenuItem.SetGroupMenuChecked(""{menuPrex}{dynmicAttr.ItemName}"", ""{group.Key}"", {valueStr}, ""{group.DefaultValue}"" , {nameStr});
                ";
                }

                sbInitCall.AppendLine(valueSettingStr);
            }
        }

        private static bool CheckMethod(MethodInfo method)
        {
            bool result = true;

            if (!method.IsPublic)
            {
                result = false;
                DebugError("{0} need to be public", method.Name);
            }

            if (!method.IsStatic)
            {
                result = false;
                DebugError("{0} need to be static", method.Name);
            }

            return result;
        }

        public static bool PrefHasKey(string key)
        {
            return PlayerPrefs.HasKey($"{prefPrex}{key}");
        }

        public static void PrefSetBool(string key, bool value)
        {
            PlayerPrefs.SetString($"{prefPrex}{key}", value.ToString());
            PlayerPrefs.Save();
        }

        public static bool PrefGetBool(string key, bool defaultValue)
        {
            return PlayerPrefs.GetString($"{prefPrex}{key}", defaultValue.ToString()) == true.ToString();
        }

        public static void PrefSetString(string key, string value)
        {
            PlayerPrefs.SetString($"{prefPrex}{key}", value);
            PlayerPrefs.Save();
        }

        public static string PrefGetString(string key, string defaultValue)
        {
            return PlayerPrefs.GetString($"{prefPrex}{key}", defaultValue);
        }

        public static void SetBoolMenuChecked(string menu, string key)
        {
            bool shouldChecked = PrefGetBool(key, false);

            Menu.SetChecked(menu, shouldChecked);
        }

        public static void SetGroupMenuChecked(string menuPref, string key, string[] values, string defultValue, string[] names)
        {
            string savedValue = PrefGetString(key, defultValue);

            for (int i = 0; i < values.Length; i++)
            {
                string value = values[i];
                string name = names[i];

                Menu.SetChecked($"{menuPref}/{name}", value == savedValue);
            }
        }

        public static void ToggleBoolMenuChecked(string menu, string key)
        {
            bool lastValue = Menu.GetChecked(menu);

            PrefSetBool(key, !lastValue);

            SetBoolMenuChecked(menu, key);
        }

        private static void DebugLog(string message, params object[] args)
        {
            Debug.Log(string.Format($"DynamicMenuItem: {message}", args));
        }

        private static void DebugError(string message, params object[] args)
        {
            Debug.LogError(string.Format($"DynamicMenuItem: {message}", args));
        }
    }
}