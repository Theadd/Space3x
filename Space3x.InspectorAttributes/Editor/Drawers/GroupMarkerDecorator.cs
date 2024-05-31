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
        
//        bool IsResetting { get; set; }
        
        // TODO: remove
        string DebugId { get; }

        void RemoveGroupMarker();

        bool HasValidMarker();

        void RebuildGroupMarker();

        public GroupMarkerAttribute GetGroupMarkerAttribute() => (GroupMarkerAttribute) (((DecoratorDrawer) this).attribute);
    }
    
    // [CustomPropertyDrawer(typeof(GroupMarkerAttribute), true)]
    public abstract class GroupMarkerDecorator<TDecorator, TGroupAttribute> : 
            Decorator<TDecorator, TGroupAttribute>, 
            IAttributeExtensionContext<TGroupAttribute>,
            IGroupMarkerDecorator
        where TDecorator : AutoDecorator, new()
        where TGroupAttribute : GroupMarkerAttribute
    {
        public GroupMarker Marker { get; set; }

        // public virtual GroupMarkerDecorator<TDecorator, TGroupAttribute> LinkedMarkerDecorator { get; set; } = null;
        public virtual IGroupMarkerDecorator LinkedMarkerDecorator { get; set; } = null;
        
        public VisualElement GroupContainer { get; set; } = null;
        
        public override TGroupAttribute Target => (TGroupAttribute) attribute;

        protected override bool KeepExistingOnCreatePropertyGUI => false;   // Marker?.IsUsed == true;
        
        // TODO: remove
        public string DebugId => this.GetType().Name + "-" + RuntimeHelpers.GetHashCode(this);
        
//        public bool IsResetting { get; set; } = false;
        
        private bool m_HasUpdated = false;

        public override void OnUpdate()
        {
            var isPending = true;
            Debug.Log($"<color=#71ff70ff><b>#> OnUpdate: {DebugId}</b></color> (AutoGrouping: {(DecoratorsCache.IsAutoGroupingDisabled() ? "OFF" : "ON")})");
            if (!m_HasUpdated)
            {
                if (!DecoratorsCache.IsAutoGroupingDisabled())
                {
                    // m_HasUpdated = true; // TODO: uncomment
                    this.RebuildGroupMarkerIfRequired();
                    if (this.TryLinkToMatchingGroupMarkerDecorator())
                    {
                        if (Target.IsOpen)
                            Debug.LogWarning($"       <color=#000000FF><b>[WARNING]</b></color> ...");
                        
                        if (!Target.IsOpen && !this.IsGroupMarkerUsed())
                        {
                            Debug.Log($"<color=#71ff70ff>#>   :- Populate (@OnUpdate): {DebugId}</color>");
                            Marker.PopulateGroupMarker();
                            isPending = false;
                            DecoratorsCache.Remove(this);
                            DecoratorsCache.Remove(LinkedMarkerDecorator);
                        }
                    }
                }
                if (isPending)
                    DecoratorsCache.MarkPending(this);
            }
            DecoratorsCache.PrintCachedInstances();
            DecoratorsCache.HandlePendingDecorators();
//            if (!DecoratorsCache.IsAutoGroupingDisabled() && DecoratorsCache.HasOnlyPending())
//            {
//                IGroupMarkerDecorator pendingDecorator = null;
//                if (DecoratorsCache.TryGet(decorator =>
//                    {
//                        pendingDecorator = decorator;
//                        return true;
//                    }))
//                {
//                    Debug.Log("Calling pending decorator: " + pendingDecorator.DebugId);
//                    pendingDecorator.OnUpdate();
//                }
//            }
        }

        public override void OnAttachedAndReady(VisualElement element)
        {
            Debug.Log($"<color=#5dd1ffff><b>#> OnAttachedAndReady: {DebugId}</b></color> (AutoGrouping: {(DecoratorsCache.IsAutoGroupingDisabled() ? "OFF" : "ON")})");
            // #5dd1ff
            Container.LogThis($"<color=#486979EE><b>READY & ATTACHING PROPERLY...</b> {this.GetType().Name}-{RuntimeHelpers.GetHashCode(this)}</color>");
            EnsureContainerIsProperlyAttached();
            if (!HasValidMarker())
                RebuildGroupMarker();
            if (Target.IsOpen)
                Marker.GetOrCreatePropertyGroupFieldForMarker();
            
            // TODO: uncomment?
//            if (!UngroupedMarkerDecorators.IsAutoGroupingDisabled())
//            {
//                UngroupedMarkerDecorators.TryRebuildAll(); // MOD_ME
//                if (this.TryLinkToMatchingGroupMarkerDecorator())
//                {
//                    Container.LogThis($"  <color=#486979FF><b>READY, ¡LINKED! & PROPERLY ATTACHED WITH MARKER</b></color>");
//                    UngroupedMarkerDecorators.Remove(this);
//                    UngroupedMarkerDecorators.Remove(LinkedMarkerDecorator);
//                }
//                else
//                {
//                    Container.LogThis($"  <color=#486979CC><b>READY & PROPERLY ATTACHED WITH MARKER (NOT LINKED)</b></color>");
//                    UngroupedMarkerDecorators.Add(this);
//                }
//            }
//            else
            {
                DecoratorsCache.Add(this);
            }
        }
        
        public override bool HasValidContainer()
        {
            var isValid = Container != null && Field != null;
            if (isValid)
            {
                if (Container.GetNextSiblingOfType<PropertyField>() != Field)
                {
                    isValid = false;
                    if (Container.GetNextSibling<AutoDecorator>() is PropertyGroupField pgf)
                    {
                        if (pgf.contentContainer.hierarchy.childCount > 0)
                        {
                            var firstElement = pgf.contentContainer.hierarchy.ElementAt(0);
                            if (firstElement is PropertyField pf)
                            {
                                isValid = pf == Field;
                            }
                            else
                            {
                                isValid = firstElement.GetNextSiblingOfType<PropertyField>() == Field;
                            }
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

//                if (!isValid && !Target.IsOpen)
//                {
//                    if (Container.GetNextSibling() is GroupMarker groupMarker)
//                    {
//                        if (groupMarker.GetNextSibling() == null)
//                        {
//                            
//                            if (parent is PropertyGroup pg)
//                            {
//                                parent = pg.hierarchy.parent;
//                                if (parent is PropertyGroupField pgf)
//                                {
//                                    isValid = pgf.GetNextSiblingOfType<PropertyField>() == Field;
//                                }
//                            }
//                        }
//                    }
//                }
            }
            
            return isValid;
        }

        private PropertyField GetNextClosestPropertyFieldOf(VisualElement element)
        {
            var next = element; //.GetNextSibling();
            PropertyField match = null;
            while (next != null && match == null)
            {
                if (next is PropertyField pf)
                {
                    match = pf;
                } 
                else if (next is PropertyGroupField pgf)
                {
                    if (pgf.contentContainer.hierarchy.childCount > 0)
                    {
                        match = GetNextClosestPropertyFieldOf(pgf.contentContainer.hierarchy.ElementAt(0));
                    }
                }
                next = next.GetNextSibling();
            }

            if (match != null)
            {
                return match;
            }
            else
            {
                if (element.hierarchy.parent is PropertyGroup pg)
                {
                    if (pg.hierarchy.parent is PropertyGroupField pgf)
                    {
                        return GetNextClosestPropertyFieldOf(pgf.GetNextSibling());
                    }
                }

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
                    PropertyGroupField group => group.contentContainer.hierarchy.childCount > 0 && group.contentContainer.hierarchy.ElementAt(group.contentContainer.hierarchy.childCount - 1) == Marker,
                    GroupMarker marker => marker == Marker,
                    _ => false
                };
            }

            return isValid;
        }

        public string ValidateAllString()
        {
            var msg = "<color=#000000FF><b>[ValidateAll]</b></color> (Self) " + Target.GetType().Name + ": ";
            
            msg += HasValidContainer() ? "<color=#00FF00FF>CONTAINER</color>, " : "<color=#FF0000FF>CONTAINER</color>, ";
            msg += HasValidMarker() ? "<color=#00FF00FF>MARKER</color>" : "<color=#FF0000FF>MARKER</color>";

            if (LinkedMarkerDecorator != null)
            {
                msg += "; (Linked) " + ((DecoratorDrawer) LinkedMarkerDecorator).attribute.GetType().Name + ": ";
                msg += LinkedMarkerDecorator.HasValidContainer() ? "<color=#00FF00FF>CONTAINER</color>, " : "<color=#FF0000FF>CONTAINER</color>, ";
                msg += LinkedMarkerDecorator.HasValidMarker() ? "<color=#00FF00FF>MARKER</color>" : "<color=#FF0000FF>MARKER</color>";
            }

            return msg + " " + RuntimeHelpers.GetHashCode(this);
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
//            var group = Marker.GetClosestParentOfType<PropertyGroupField>();
//            group.UngroupAll();
//            group.RemoveFromHierarchy();
        }

        public override void OnReset(bool disposing = false)
        {
            Debug.Log($"  <color=#00000099>~OnReset(disposing: {disposing}): {DebugId}</color>");
            if (!disposing)
            {
                if (this.IsGroupMarkerUsed())
                {
                    Debug.LogError($"<color=#FF0000FF><b>!!! [Reset]</b></color> This Group marker was used when it should have been reset.");
                    UndoAllHierarchyGrouping();
                }
//                else
                {
                    Container.LogThis($"<color=#FF6979FF><b>SOFT RESET...</b> {this.GetType().Name}-{RuntimeHelpers.GetHashCode(this)}</color>");
                    CachedDecoratorsCache?.Remove(this);
                    if (Target.IsOpen && GroupContainer != null)
                    {
                        GroupContainer.RemoveFromHierarchy();
                        GroupContainer = null;
                    }
                    
//                    IsResetting = true;
                    RemoveGroupMarker();
                    if (LinkedMarkerDecorator != null)
                    {
//                        LinkedMarkerDecorator.IsResetting = true;
                        LinkedMarkerDecorator.RemoveGroupMarker(); // TODO: is this needed?
                    }
                }
//                if (LinkedMarkerDecorator != null)
//                {
//                    Debug.Log("Resetting linked group marker decorator " + Target.GetType().Name);
//                    IsResetting = true;
//                    LinkedMarkerDecorator.IsResetting = true;
//                    if (Marker?.IsUsed == true || Marker?.ClassListContains(GroupMarker.UssUsedClassName) == true)
//                    {
//                        UndoAllHierarchyGrouping();
//                        LinkedMarkerDecorator.RemoveGroupMarker();
//                    }
//                    else
//                    {
//                        LinkedMarkerDecorator.RemoveGroupMarker();
//                        Debug.LogError($"This group marker was linked but not marked as used.");
//                    }
//                }
//
//                RemoveGroupMarker();
            }
            else
            {
                CachedDecoratorsCache?.Remove(this);
                if (Target.IsOpen && GroupContainer != null)
                    GroupContainer.RemoveFromHierarchy();
//                if (Marker?.IsUsed == true || Marker?.ClassListContains(GroupMarker.UssUsedClassName) == true)
//                {
//                    if (!Target.IsOpen)
//                        Marker.GetClosestParentOfType<PropertyGroupField>().RemoveFromHierarchy();
//                }
                RemoveGroupMarker();
                LinkedMarkerDecorator = null;
            }
            
            base.OnReset(disposing);
        }
    }
}
