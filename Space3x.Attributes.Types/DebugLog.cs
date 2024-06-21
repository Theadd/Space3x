using UnityEngine;

namespace Space3x.Attributes.Types
{
    public static class DebugLog
    {
        [HideInCallstack]
        public static void Error(string msg)
        {
            #if SPACE3X_DEBUG
            Debug.LogError(msg);
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
