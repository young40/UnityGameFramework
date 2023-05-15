using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UnityGameFramework.Editor
{
    public abstract partial class DynamicMenuItem : MonoBehaviour
    {
        private static readonly string csharpScriptPath = Path.GetFullPath(Path.Join(Application.dataPath, "Editor", "IGNORE_ME_DynamicMenuItem_Auto_Generator.cs"));

        private static readonly string menuPrex = "Game Framework/";

        public static readonly string prefPrex = "UnityGameFramework.Editor.Debug.";
    }
}
