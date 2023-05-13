using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityGameFramework.GameCore
{
    [InitializeOnLoad]
    public static class EditorExitPlayingCleanup
    {
        static EditorExitPlayingCleanup()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

        }

        private static void OnPlayModeStateChanged(PlayModeStateChange newState)
        {
            if (newState == PlayModeStateChange.EnteredEditMode)
            {
                Debug.Log("EditorExitPlayingCleanup: try to clean up UnityGameFramework");
                UnityGameFramework.Runtime.GameEntry.Shutdown(Runtime.ShutdownType.None);
            }
        }
    }
}