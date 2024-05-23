using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Space3x.UiToolkit.SlicedText
{
    public class ExtensibleEventHandler : TextEditorKeyboardEventHandler
    {
        private static event Action OnChangeKeyMappings;

        private static Dictionary<int, string> _keyMapping;
        
        public static Dictionary<int, string> KeyMapping
        {
            get => _keyMapping;
            set
            {
                _keyMapping = value ?? new Dictionary<int, string>();
                ExtensibleEventHandler.Setup();
            }
        }

        protected static Dictionary<Event, int> Actions;

        protected static void MapAction(int action, string defaultKey = null)
        {
            if (Actions.ContainsValue(action))
                Actions
                    .Where(pair => pair.Value == action)
                    .Select(match => Actions.Remove(match.Key));
            
            var rawKey = _keyMapping.ContainsKey(action) ? _keyMapping[action] ?? defaultKey : defaultKey;

            if (string.IsNullOrEmpty(rawKey)) return;

            foreach (var key in rawKey.Split('|'))
                Actions[Event.KeyboardEvent(key)] = action;
        }

        static ExtensibleEventHandler()
        {
            _keyMapping = _keyMapping ?? new Dictionary<int, string>();
            Actions = Actions ?? new Dictionary<Event, int>();
        }
        
        public ExtensibleEventHandler() {}

        protected static void Initialize(Action setupAction)
        {
            OnChangeKeyMappings -= setupAction;
            OnChangeKeyMappings += setupAction;
            setupAction?.Invoke();
        }
        
        protected static void Setup()
        {
            OnChangeKeyMappings?.Invoke();
        }

        public virtual bool TriggerAction(int action, bool textIsReadOnly = false) => false;
        
        public override bool HandleKeyEvent(Event e, bool textIsReadOnly)
        {
            EventModifiers modifiers = e.modifiers;
            e.modifiers &= ~EventModifiers.CapsLock;

            if (Actions.ContainsKey(e))
            {
                if (TriggerAction(Actions[e], textIsReadOnly))
                {
                    e.modifiers = modifiers;
                    return true;
                }
            }

            e.modifiers = modifiers;
            return base.HandleKeyEvent(e, textIsReadOnly);
        }
    }
}
