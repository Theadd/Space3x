using System.Linq;
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
        
        [HideInCallstack]
        public static void AllLines(string msg, int maxLines = 150)
        {
            #if SPACE3X_DEBUG
            var lines = msg.Split('\n');
            while (lines.Length > maxLines)
            {
                Debug.unityLogger.Log(LogType.Log, "<color=#FFFFFFFF>" + string.Join('\n', lines.Take(maxLines)) + "</color>");
                lines = lines.Skip(maxLines).ToArray();
            }
            if (lines.Length > 0)
                Debug.unityLogger.Log(LogType.Log, "<color=#FFFFFFFF>" + string.Join('\n', lines) + "</color>");
            #endif
        }
    }
}
