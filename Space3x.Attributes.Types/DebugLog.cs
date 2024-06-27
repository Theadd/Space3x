using UnityEngine;

namespace Space3x.Attributes.Types
{
    public class DebugLog
    {
        [HideInCallstack]
        public static void Error(string msg)
        {
            #if SPACE3X_DEBUG
            Debug.unityLogger.Log(LogType.Error, "<color=#FF0000FF>" + msg + "</color>");
            // Debug.LogError("<color=#FF0000FF>" + msg + "</color>");
            #endif
        }
        
        [HideInCallstack]
        public static void Info(string msg)
        {
            #if SPACE3X_DEBUG
            Debug.Log(msg);
            #endif
        }
        
        [HideInCallstack]
        public static void Warning(string msg)
        {
            #if SPACE3X_DEBUG
            Debug.LogWarning(msg);
            #endif
        }
    }
}
