using Space3x.Attributes.Types;
using Space3x.InspectorAttributes.Editor.Drawers;
using Space3x.InspectorAttributes.Editor.VisualElements;
using Space3x.UiToolkit.Types;
using UnityEngine.Internal;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.Extensions
{
    [ExcludeFromDocs]
    public static class GroupMarkerDecoratorExtensions
    {
        public static bool IsGroupMarkerUsed(this IGroupMarkerDecorator self) => 
            self.GetGroupBeginMarkerDecorator()?.GroupContainer is PropertyGroupField group && group.IsUsed;

        public static IGroupMarkerDecorator GetGroupBeginMarkerDecorator(this IGroupMarkerDecorator self)
        {
            if (self.GetGroupMarkerAttribute().IsOpen)
                return self;
            if (self.LinkedMarkerDecorator != null && self.LinkedMarkerDecorator.GetGroupMarkerAttribute().IsOpen)
                return self.LinkedMarkerDecorator;
            
            return null;
        }
        
        public static IGroupMarkerDecorator GetGroupEndMarkerDecorator(this IGroupMarkerDecorator self)
        {
            if (!self.GetGroupMarkerAttribute().IsOpen)
                return self;
            if (self.LinkedMarkerDecorator != null && !self.LinkedMarkerDecorator.GetGroupMarkerAttribute().IsOpen)
                return self.LinkedMarkerDecorator;
            
            return null;
        }
        
        public static bool TryLinkToMatchingGroupMarkerDecorator(this IGroupMarkerDecorator self)
        {
            IGroupMarkerDecorator beginDecorator = null;
            IGroupMarkerDecorator endDecorator = null;

            if (self.GetGroupMarkerAttribute().IsOpen)
            {
                beginDecorator = self;
                if (self.LinkedMarkerDecorator != null)
                    endDecorator = self.LinkedMarkerDecorator;
            }
            else
            {
                endDecorator = self;

                if (self.LinkedMarkerDecorator == null)
                {
                    // Is end marker and not linked
                    endDecorator.RebuildGroupMarkerIfRequired();
                    var beginMarker = endDecorator.Marker.GetMatchingGroupMarker();
                    if (beginMarker != null)
                    {
                        endDecorator.Marker.LinkTo(beginMarker);
                        beginDecorator = beginMarker.MarkerDecorator;
                    }
                }
                else
                {
                    beginDecorator = self.LinkedMarkerDecorator;
                }
            }

            return beginDecorator != null && endDecorator != null;
        }
        
        /// <summary>
        /// Rebuilds the group marker and it's related decorator container, if required.
        /// </summary>
        /// <param name="decorator"></param>
        /// <returns>Whether the group marker and/or it's related decorator container was rebuilt or not.</returns>
        public static bool RebuildGroupMarkerIfRequired(this IGroupMarkerDecorator decorator)
        {
            var prevContainerName = decorator.Container?.name ?? "???";
            var isValid = decorator.EnsureContainerIsProperlyAttached();
            if (!isValid)
                decorator.RebuildGroupMarker();
            else
                if (!decorator.HasValidMarker())
                {
                    isValid = false;
                    decorator.RebuildGroupMarker();
                }

            if (!isValid)
                DebugLog.Info("<color=#000000CC>  Marker REBUILT from <b>#" + prevContainerName + "</b> as: </color>" + decorator.Container.AsString());
            
            return !isValid;
        }
        
        public static GroupMarker CreateMarker(this IGroupMarkerDecorator self)
        {
            return new GroupMarker()
            {
                Type = self.GetGroupMarkerAttribute().Type,
                GroupName = self.GetGroupMarkerAttribute().Text,
                Origin = self.Container,
                MarkerDecorator = self,
                IsOpen = self.GetGroupMarkerAttribute().IsOpen,
                IsUsed = false,
                style = { display = DisplayStyle.None }
            };
        }
    }
}
