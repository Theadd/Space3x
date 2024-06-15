using UnityEngine;

namespace Space3x.Attributes.Types
{
    public static class DebugLog
    {
        public static void Error(string msg)
        {
            #if SPACE3X_DEBUG
            Debug.LogError(msg);
            #endif
        }
        
        public static void Info(string msg)
        {
            #if SPACE3X_DEBUG
            Debug.Log(msg);
            #endif
        }
    }
}
