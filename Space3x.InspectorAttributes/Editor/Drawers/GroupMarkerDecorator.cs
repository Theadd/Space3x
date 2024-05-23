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
        
        bool IsResetting { get; set; }
        
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
        
        public bool IsResetting { get; set; } = false;
        
        private bool m_HasUpdated = false;
//        
//        private int m_ReattachCount = 0;

//        public bool TryApplyGrouping()
//        {
//            Debug.Log($"<color=#00c2ffff><b>[TRY APPLY GROUPING]</b></color> {ValidateAllString()} {Container.AsString()}");
//            if (!this.IsGroupMarkerUsed())
//            {
//                if (!this.TryLinkToMatchingGroupMarkerDecorator())
//                {
//                    Debug.LogError($"[GROUPING NOT DONE & UNABLE TO LINK] {Target.GetType().Name}");
//                    return false;
//                }
//            }
//        }
//
//        private void EnsureEverythingIsProperlyAttached(Action onDone)
//        {
//            EnsureContainerIsProperlyAttached(() => 
//            {
//                if (!HasValidMarker())
//                    RebuildGroupMarker();
//                
//            });
//        }

        public override void OnUpdate()
        {
            if (!m_HasUpdated)
            {
                if (!UngroupedMarkerDecorators.IsAutoGroupingDisabled())
                {
                    m_HasUpdated = true;
                    this.RebuildGroupMarkerIfRequired();
                    if (this.TryLinkToMatchingGroupMarkerDecorator())
                        if (!this.IsGroupMarkerUsed())
                            Marker.PopulateGroupMarker();
                }
            }
            return;
//            if (!m_HasUpdated)
//            {
//                var allText = ((IDrawer) this).InspectorElement.AsHierarchyString();
//                var count = RichTextViewer.Count();
//                var title = $"<b>{count}</b> <color=#FF00FFFF>[OnFirstUpdate] {ValidateAllString()}</color>";
//                RichTextViewer.AddText(title, allText);
//                Debug.Log(title);
//                m_HasUpdated = true;
//                if (LinkedMarkerDecorator == null)
//                {
//                    Debug.Log($">> DOING [UngroupedMarkerDecorators.TryGet] OVER {UngroupedMarkerDecorators.Count()} others; self: {Target.GetType().Name}");
//                    if (!UngroupedMarkerDecorators.TryGet(other =>
//                        {
//                            Debug.Log($"  IN [UngroupedMarkerDecorators.TryGet] self: {Target.GetType().Name}, other: {other.GetGroupMarkerAttribute().GetType().Name}");
//                            if (other.GetGroupMarkerAttribute().Type == Target.Type && other.GetGroupMarkerAttribute().IsOpen != Target.IsOpen)
//                            {
//                                Debug.Log("    <b>MATCH FOUND</b>");
//                                other.RemoveGroupMarker();
//                                other.OnAttachedAndReady(other.Container);
//                                return true;
//                            }
//                            else
//                                return false;
//                        }))
//                    {
//                        Debug.LogError($"<color=#FF0000FF>[OnFirstUpdate] UngroupedMarkerDecorators not found.</color>");
//                    }
//                }
//            }
        }

//        public override void OnAttachedAndReady(VisualElement element)
//        {
////            Debug.Log($"OnAttachedAndReady TO: {Field.name}, Attribute: {Target.GetType().Name}, " +
////                      $"Marker: {Marker == null} (Must be True); ELEMENT: {element.name} ({element.GetHashCode()})");
//            if (Marker != null && HasValidContainer() && HasValidMarker())
//            {
//                Debug.LogError($"<color=#FF0000CC>[OnAttachedAndReady] Marker already set:</color> {ValidateAllString()} {Container?.AsString()}<br>{Marker.AsString()}");
//                // throw new Exception($"[OnAttachedAndReady] Marker already set: {Marker.AsString()}");
//            }
//            else
//            {
//                EnsureContainerIsProperlyAttached(() => {
//                    if (!HasValidMarker())
//                        RebuildGroupMarker();
//                    if (!IsResetting)
//                    {
//                        if (LinkedMarkerDecorator != null && Target.IsOpen)
//                        {
//                            LinkedMarkerDecorator.RemoveGroupMarker();
//                            LinkedMarkerDecorator.EnsureContainerIsProperlyAttached(() =>
//                            {
//                                Container.LogThis("ENSURE PROPER BEGIN CONTAINER CALLBACK");
//                                LinkedMarkerDecorator.Container.LogThis("ENSURE PROPER END CONTAINER CALLBACK");
//                                CloseGroupIfPossible();
//                            });
//                        }
//                        else
//                            CloseGroupIfPossible();
//                    }
//                    else
//                    {
//                        if (LinkedMarkerDecorator == null)
//                            throw new Exception("<color=#FF0000E0><b>[UNEXPECTED]</b> LinkedMarkerDecorator is null when IsResetting is true.</color>");
//                        
//                        if (!Target.IsOpen)
//                        {
//                            LinkedMarkerDecorator.EnsureContainerIsProperlyAttached(() =>
//                            {
//                                if (!LinkedMarkerDecorator.HasValidMarker())
//                                    LinkedMarkerDecorator.RebuildGroupMarker();
//                                LinkedMarkerDecorator.Container.LogThis("ENSURE PROPER BEGIN CONTAINER CALLBACK");
//                                Container.LogThis("ENSURE PROPER END CONTAINER CALLBACK");
//                                IsResetting = false;
//                                LinkedMarkerDecorator.IsResetting = false;
//                                CloseGroupIfPossible();
//                            });
//                        }
//                        else
//                        {
//                            m_ReattachCount++;
//                            if (m_ReattachCount > 1)
//                            {
//                                Container.LogThis($"<color=#00FFCCFF><b>[SKIP REATTACHING] {m_ReattachCount}</b></color>");
//                            }
//                            else
//                            {
//                                Container.LogThis($"<color=#00FFCCFF><b>[REATTACHING] {m_ReattachCount}</b></color>");
//                                LinkedMarkerDecorator.Container.RemoveFromHierarchy();
//                                LinkedMarkerDecorator.EnsureContainerIsProperlyAttached(() =>
//                                {
//                                    LinkedMarkerDecorator.Container.LogThis("REATTACHED");
//                                    LinkedMarkerDecorator.OnAttachedAndReady(LinkedMarkerDecorator.Container);
//                                });
//                            }
//                        }
//                    }
//                });
//            }
//        }
        
        public override void OnAttachedAndReady(VisualElement element)
        {
            Container.LogThis($"<color=#486979EE><b>READY & ATTACHING PROPERLY...</b> {this.GetType().Name}-{RuntimeHelpers.GetHashCode(this)}</color>");
            EnsureContainerIsProperlyAttached();
            if (!HasValidMarker())
                RebuildGroupMarker();
            if (Target.IsOpen)
                Marker.GetOrCreatePropertyGroupFieldForMarker();
            
            if (!UngroupedMarkerDecorators.IsAutoGroupingDisabled())
            {
                UngroupedMarkerDecorators.TryRebuildAll(); // MOD_ME
                if (this.TryLinkToMatchingGroupMarkerDecorator())
                {
                    Container.LogThis($"  <color=#486979FF><b>READY, ¡LINKED! & PROPERLY ATTACHED WITH MARKER</b></color>");
                    UngroupedMarkerDecorators.Remove(this);
                    UngroupedMarkerDecorators.Remove(LinkedMarkerDecorator);
                }
                else
                {
                    Container.LogThis($"  <color=#486979CC><b>READY & PROPERLY ATTACHED WITH MARKER (NOT LINKED)</b></color>");
                    UngroupedMarkerDecorators.Add(this);
                }
            }
            else
            {
                UngroupedMarkerDecorators.Add(this);
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

//        private void CloseGroupIfPossible()
//        {
//            Debug.Log(ValidateAllString());
//            if (LinkedMarkerDecorator != null)
//            {
//                // This Group was already closed (before resetting), so we need to close it again.
//                if (LinkedMarkerDecorator.Marker == null)
//                    LinkedMarkerDecorator.RebuildGroupMarker();
//                if (Marker == null)
//                    RebuildGroupMarker();
//
//                if (Marker?.IsOpen == false)
//                    CloseGroupOf(this);
//                else if (LinkedMarkerDecorator.Marker?.IsOpen == false)
//                    CloseGroupOf(LinkedMarkerDecorator);
//                else
//                    throw new Exception("Both group markers are open.");
//            }
//            else if (Marker?.IsOpen == false) 
//                CloseGroupOf(this);
//        }
//
////        private void CloseGroupOf(GroupMarkerDecorator<TDecorator, TGroupAttribute> decorator)
//        private void CloseGroupOf(IGroupMarkerDecorator decorator)
//        {
//            var success = decorator.Marker.CloseGroupMarker();
//
//            if (this.GetGroupBeginMarkerDecorator()?.GroupContainer is PropertyGroupField group)
//                group.IsUsed = success;
//            
//            if (!success)
//            {
////                UngroupedMarkerDecorators.Add(decorator);
//            }
////            else
////            {
////                UngroupedMarkerDecorators.Remove(decorator);
////                UngroupedMarkerDecorators.Remove(decorator.LinkedMarkerDecorator);
////            }
//        }

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
            if (!disposing)
            {
                if (this.IsGroupMarkerUsed())
                {
                    Debug.LogError($"<color=#FF0000FF><b>!!! [Reset]</b></color> This Group marker was used when it should have been reset.");
                }
                else
                {
                    Container.LogThis($"<color=#FF6979FF><b>SOFT RESET...</b> {this.GetType().Name}-{RuntimeHelpers.GetHashCode(this)}</color>");
                    if (Target.IsOpen && GroupContainer != null)
                    {
                        GroupContainer.RemoveFromHierarchy();
                        GroupContainer = null;
                    }
                    
                    IsResetting = true;
                    RemoveGroupMarker();
                    if (LinkedMarkerDecorator != null)
                    {
                        LinkedMarkerDecorator.IsResetting = true;
                        LinkedMarkerDecorator.RemoveGroupMarker();
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
