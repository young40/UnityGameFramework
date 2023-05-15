using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;
using System.Text;
using System.IO;

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

            foreach (Assembly assembly in GetAssemblies())
            {
                MethodInfo[] methods = GetMethods(assembly);

                foreach (MethodInfo method in methods)
                {
                    GetMenuItemCode(method, sb);
                }
            }

            string className = System.IO.Path.GetFileNameWithoutExtension(csharpScriptPath);
            string classStr = classTpl.Replace("__REPLACE_CLASS_NAME", className);

            classStr = classStr.Replace("__REPLACE_METHOD_BODY", sb.ToString());

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

        private static void GetMenuItemCode(MethodInfo method, StringBuilder sb)
        {
            DynamicMenuItemAttribute arrt = method.GetCustomAttribute<DynamicMenuItemAttribute>();

            string code = methodTpl.Replace("__REPLACE_MENU_PATH", $"{menuPrex}{arrt.ItemName}");

            code = code.Replace("__REPLACE_MENU_PRIORITY", arrt.Priority.ToString());

            code = code.Replace("__REPLACE_MENU_METHOD_NAME", $"{method.DeclaringType.FullName}_{method.Name}".Replace(".", "_"));
            code = code.Replace("__REPLACE_MENU_CALL", $"{method.DeclaringType.FullName}.{method.Name}()");

            sb.AppendLine(code);
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