﻿using System;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
    public interface IExpandablePropertyContent
    {
        VisualElement Content { get; set; }
        VisualElement ContentContainer { get; set; }
        public bool IsExpanded { get; set; }
        public MarkerDecoratorsCache DecoratorsCache { get; }

        VisualElement CreateContentGUI();
        
        public virtual void RebuildExpandablePropertyContentGUI(Action onContentAttached = null)
        {
            ContentContainer ??= new VisualElement() { name = "ContentContainer" };
            if (Content != null)
            {
                Content.Unbind();
                ContentContainer.Clear();
            }
            Content = CreateContentGUI();
            if (onContentAttached != null) 
                Content.RegisterCallbackOnce<AttachToPanelEvent>(_ => onContentAttached.Invoke());
            /* TODO: FIXME: Uncomment: ContentContainer.SetVisible(false); */
            ContentContainer.Add(Content);
        }

        // public virtual void ReloadPropertyContentGUI(Action onContentAttached = null)
        // {
        //     Content.Unbind();
        //     Content.RemoveFromHierarchy();
        //     Content.MarkDirtyRepaint();
        //     if (onContentAttached != null) 
        //         Content.RegisterCallbackOnce<AttachToPanelEvent>(_ => onContentAttached.Invoke());
        //     ContentContainer.Add(Content);
        // }
        //
        // private static MethodInfo s_PropertyFieldReset = null;
        //
        // public virtual void CallResetForContentAsPropertyField(SerializedProperty property)
        // {
        //     s_PropertyFieldReset ??= typeof(PropertyField).GetMethod(
        //         "Reset", 
        //         BindingFlags.Instance | BindingFlags.NonPublic,
        //         null,
        //         new Type[] { typeof(SerializedProperty) },
        //         null);
        //
        //     if (s_PropertyFieldReset != null)
        //         s_PropertyFieldReset.Invoke(Content as PropertyField, new object[] { property });
        //     
        // }
        //
        // public void ExecuteDelayedUpdate();
    }
}