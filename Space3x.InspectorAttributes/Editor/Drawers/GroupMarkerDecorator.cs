using System;
using System.Runtime.CompilerServices;
using Space3x.Attributes.Types;
using Space3x.InspectorAttributes.Editor.Extensions;
using Space3x.InspectorAttributes.Editor.VisualElements;
using Space3x.UiToolkit.Types;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(GroupMarkerAttribute), true)]
    public class GroupDecorator : GroupMarkerDecorator<AutoDecorator, GroupMarkerAttribute>
    {
        public override IGroupMarkerDecorator LinkedMarkerDecorator { get; set; } = null;
        
        public override GroupMarkerAttribute Target => (GroupMarkerAttribute) attribute;
    }

    public interface IGroupMarkerDecorator : IDecorator, IDrawer
    {
        GroupMarker Marker { get; set; }
        
        IGroupMarkerDecorator LinkedMarkerDecorator { get; set; }
        
        VisualElement GroupContainer { get; set; }

        // TODO: remove
        string DebugId { get; }

        void RemoveGroupMarker();

        bool HasValidMarker();

        void RebuildGroupMarker();

        public GroupMarkerAttribute GetGroupMarkerAttribute() => (GroupMarkerAttribute) (((DecoratorDrawer) this).attribute);
    }

    public abstract class GroupMarkerDecorator<TDecorator, TGroupAttribute> : 
            Decorator<TDecorator, TGroupAttribute>, 
            IAttributeExtensionContext<TGroupAttribute>,
            IGroupMarkerDecorator
        where TDecorator : AutoDecorator, new()
        where TGroupAttribute : GroupMarkerAttribute
    {
        public GroupMarker Marker { get; set; }
        
        public virtual IGroupMarkerDecorator LinkedMarkerDecorator { get; set; } = null;
        
        public VisualElement GroupContainer { get; set; } = null;
        
        public override TGroupAttribute Target => (TGroupAttribute) attribute;
        
        // TODO: remove
        public string DebugId => this.GetType().Name + "-" + RuntimeHelpers.GetHashCode(this);

        public override void OnUpdate()
        {
            var isPending = true;
            DebugLog.Info("#1");
            if (!DecoratorsCache.IsAutoGroupingDisabled())
            {
                DebugLog.Info("#2");
                var didRebuild = this.RebuildGroupMarkerIfRequired();
                DebugLog.Info("#2.Rebuild: " + didRebuild);
                if (this.TryLinkToMatchingGroupMarkerDecorator())
                {
                    DebugLog.Info("#3");
                    if (Target.IsOpen)
                        DebugLog.Warning($"       <color=#000000FF><b>[WARNING]</b></color> ...");
                    
                    if (!Target.IsOpen && !this.IsGroupMarkerUsed())
                    {
                        DebugLog.Info("#4");
                        Marker.PopulateGroupMarker();
                        isPending = false;
                        DecoratorsCache.Remove(this);
                        DecoratorsCache.Remove(LinkedMarkerDecorator);
                    }
                }
            }
            if (isPending)
            {
                DebugLog.Info("#-5");
                DecoratorsCache.MarkPending(this);
                DecoratorsCache.PrintCachedInstances();
                DebugLog.Info("#-5 END");
            }
            DebugLog.Info("#-6");
            DecoratorsCache.HandlePendingDecorators();
            DebugLog.Info("#-7");
        }

        public override void OnAttachedAndReady(VisualElement element)
        {
            EnsureContainerIsProperlyAttached();
            if (!HasValidMarker())
                RebuildGroupMarker();
            if (Target.IsOpen)
                Marker.GetOrCreatePropertyGroupFieldForMarker();

            DecoratorsCache.Add(this);
        }
        
        public override bool HasValidContainer()
        {
            var isValid = Container != null && Field != null;
            if (isValid)
            {
                if (Container.GetNextSiblingOfType<PropertyField, BindablePropertyField>() != Field)
                {
                    isValid = false;
                    if (Container.GetNextSibling<AutoDecorator>() is PropertyGroupField pgf)
                    {
                        if (pgf.contentContainer.hierarchy.childCount > 0)
                        {
                            var firstElement = pgf.contentContainer.hierarchy.ElementAt(0);
                            if (firstElement is PropertyField pf)
                                isValid = pf == Field;
                            else if (firstElement is BindablePropertyField bpf)
                                isValid = bpf == Field;
                            else
                                isValid = firstElement.GetNextSiblingOfType<PropertyField, BindablePropertyField>() == Field;
                        }
                    }
                }
                if (!isValid)
                {
                    if (Target.IsOpen)
                    {
                        isValid = GetNextClosestPropertyFieldOf(Container) == Field;
                    }
                }
            }
            
            return isValid;
        }

        private VisualElement GetNextClosestPropertyFieldOf(VisualElement element)
        {
            var next = element;
            VisualElement match = null;
            while (next != null && match == null)
            {
                if (next is PropertyField pf)
                    match = pf;
                else if (next is BindablePropertyField bpf)
                    match = bpf;
                else if (next is PropertyGroupField pgf)
                    if (pgf.contentContainer.hierarchy.childCount > 0)
                        match = GetNextClosestPropertyFieldOf(pgf.contentContainer.hierarchy.ElementAt(0));

                next = next.GetNextSibling();
            }

            if (match != null)
                return match;
            else
            {
                if (element.hierarchy.parent is PropertyGroup pg)
                    if (pg.hierarchy.parent is PropertyGroupField pgf)
                        return GetNextClosestPropertyFieldOf(pgf.GetNextSibling());

                return null;
            }
        }
        
        public bool HasValidMarker()
        {
            var isValid = Container != null && Marker != null;
            if (isValid)
            {
                isValid = Container.GetPreviousSibling() switch
                {
                    PropertyGroupField group => group.contentContainer.hierarchy.childCount > 0 
                                                && group.contentContainer.hierarchy.ElementAt(group.contentContainer.hierarchy.childCount - 1) == Marker,
                    GroupMarker marker => marker == Marker,
                    _ => false
                };
            }

            return isValid;
        }

        public void RebuildGroupMarker()
        {
            RemoveGroupMarker();
            Marker = this.CreateMarker();
            Marker.WithClasses(Target.GetType().Name + "-" + Target.GetHashCode());
            Container.AddBefore(Marker);
        }

        public void RemoveGroupMarker()
        {
            Marker?.RemoveFromHierarchy();
            Marker = null;
        }

        private void UndoAllHierarchyGrouping()
        {
            var beginDecorator = this.GetGroupBeginMarkerDecorator();
            if (beginDecorator?.GroupContainer is PropertyGroupField group)
            {
                group.UngroupAll();
                group.RemoveFromHierarchy();
                group.IsUsed = false;
                beginDecorator.GroupContainer = null;
            }
        }

        public override void OnReset(bool disposing = false)
        {
            if (!disposing)
            {
                if (this.IsGroupMarkerUsed())
                {
                    Debug.LogError($"<color=#FF0000FF><b>!!! [Reset]</b></color> This Group marker was used when it should have been reset.");
                    // ((IDrawer) this).ForceRebuild();
                    // return; // TODO: FIXME: this is a hack, should be fixed in the next release
                    UndoAllHierarchyGrouping();
                }

                Container.LogThis($"<color=#FF6979FF><b>SOFT RESET...</b> {this.GetType().Name}-{RuntimeHelpers.GetHashCode(this)}</color>");
                CachedDecoratorsCache?.Remove(this);
                if (Target.IsOpen && GroupContainer != null)
                {
                    GroupContainer.RemoveFromHierarchy();
                    GroupContainer = null;
                }
                
                RemoveGroupMarker();
                LinkedMarkerDecorator?.RemoveGroupMarker(); // TODO: is this needed?
                DebugLog.Error("<color=#FF0000FF>// TODO: FIXME: this is a hack, should be fixed in the next release</color>");
                // HACK COMMENTED OUT: return; // TODO: FIXME: this is a hack, should be fixed in the next release
            }
            else
            {
                CachedDecoratorsCache?.Remove(this);
                if (Target.IsOpen && GroupContainer != null)
                    GroupContainer.RemoveFromHierarchy();
                RemoveGroupMarker();
                LinkedMarkerDecorator = null;
            }
            
            base.OnReset(disposing);
        }
    }
}
