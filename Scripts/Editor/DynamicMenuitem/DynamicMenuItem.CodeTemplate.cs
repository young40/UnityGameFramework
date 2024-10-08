using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityGameFramework.Editor
{
    public abstract partial class DynamicMenuItem
    {
        private static readonly string methodTpl = 
@"    [MenuItem(""__REPLACE_MENU_PATH"", priority = __REPLACE_MENU_PRIORITY)]
    private static void __REPLACE_MENU_METHOD_NAME()
    {
        __REPLACE_MENU_UPDATE
        __REPLACE_MENU_CALL;
    }
";

        private static readonly string classTpl = 
@"using UnityEditor;

[InitializeOnLoad]
public abstract class __REPLACE_CLASS_NAME
{
    static __REPLACE_CLASS_NAME()
    {
        EditorApplication.delayCall += () => { 
__REPLACE_INIT_CALL
        };
    }

__REPLACE_METHOD_BODY
}";
    }
}
