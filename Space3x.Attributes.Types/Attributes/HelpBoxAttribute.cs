using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.Attributes.Types
{
    /// <summary>
    /// Displays a HelpBox over the next VisualTarget in the inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public class HelpBoxAttribute : PropertyAttribute
    {
        public string Text { get; set; } = string.Empty;

        public HelpBoxMessageType MessageType { get; set; } = HelpBoxMessageType.None;

        public HelpBoxAttribute() { }
        
        public HelpBoxAttribute(string text, HelpBoxMessageType messageType = HelpBoxMessageType.None)
        {
            Text = text;
            MessageType = messageType;
        }
    }
}
