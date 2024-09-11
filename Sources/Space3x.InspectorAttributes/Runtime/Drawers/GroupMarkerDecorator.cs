using System.Runtime.CompilerServices;
using Space3x.Attributes.Types;
using Space3x.Properties.Types;
using Space3x.UiToolkit.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
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

        public GroupMarkerAttribute GetGroupMarkerAttribute() => (GroupMarkerAttribute) (((DecoratorDrawerAdapter) this).attribute);
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
           DebugLog.Warning($"[GMD!] OnUpdate()");
            var isPending = true;
            if (!DecoratorsCache.IsAutoGroupingDisabled())
            {
                this.RebuildGroupMarkerIfRequired();
                if (this.TryLinkToMatchingGroupMarkerDecorator())
                {
                    if (Target.IsOpen)
                        Debug.LogWarning($"       <color=#000000FF><b>[WARNING]</b></color> ...");
                    
                    if (!Target.IsOpen && !this.IsGroupMarkerUsed())
                    {
                        Marker.PopulateGroupMarker();
                        isPending = false;
                        DecoratorsCache.Remove(this);
                        DecoratorsCache.Remove(LinkedMarkerDecorator);
                    }
                }
            }
            if (isPending)
            {
                DecoratorsCache.MarkPending(this);
            }
            DecoratorsCache.HandlePendingDecorators();
        }

        public override void OnAttachedAndReady(VisualElement element)
        {
            EnsureContainerIsProperlyAttached();
            if (!HasValidMarker())
                RebuildGroupMarker();
            if (Target.IsOpen)
                Marker.GetOrCreatePropertyGroupFieldForMarker();

            DecoratorsCache.Add(this);
            DebugLog.Warning($"[GMD!] OnAttachedAndReady()");
        }
        
        public override bool HasValidContainer()
        {
            var isValid = Container != null && Field != null;
            if (isValid)
            {
#if UNITY_EDITOR
                if (Container.GetNextSiblingOfType<UnityEditor.UIElements.PropertyField, BindablePropertyField>() != Field)
#else
                if (Container.GetNextSiblingOfType<BindablePropertyField>() != Field)
#endif
                {
                    isValid = false;
                    // EDIT: AutoDecorator was being used instead of IAutoElement, replaced by IAutoElement but requires further in-depth testing.
                    if (Container.GetNextSibling<IAutoElement>() is PropertyGroupField pgf)
                    {
                        if (pgf.contentContainer.hierarchy.childCount > 0)
                        {
                            var firstElement = pgf.contentContainer.hierarchy.ElementAt(0);
                            if (firstElement is BindablePropertyField bpf)
                                isValid = bpf == Field;
#if UNITY_EDITOR
                            else if (firstElement is UnityEditor.UIElements.PropertyField pf)
                                isValid = pf == Field;
                            else
                                isValid = firstElement.GetNextSiblingOfType<UnityEditor.UIElements.PropertyField, BindablePropertyField>() == Field;
#else
                            else
                                isValid = firstElement.GetNextSiblingOfType<BindablePropertyField>() == Field;
#endif
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
                if (next is BindablePropertyField bpf)
                    match = bpf;
#if UNITY_EDITOR
                else if (next is UnityEditor.UIElements.PropertyField pf)
                    match = pf;
#endif
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
                    Debug.LogWarning($"<color=#FF0000FF><b>!!! [Reset]</b></color> This Group marker was used when it should have been reset.");
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
                Debug.LogWarning("<color=#FF0000FF>// TODO: FIXME: this is a hack, should be fixed in the next release</color>");
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
