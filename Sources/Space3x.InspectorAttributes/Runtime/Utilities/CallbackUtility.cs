using System;
using Space3x.Properties.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes
{
    public static class CallbackUtility
    {
        // public static Action RegisterCallback<TEventType>(
        //     VisualElement target,
        //     EventCallback<TEventType> callback,
        //     TrickleDown useTrickleDown = TrickleDown.NoTrickleDown)
        //     where TEventType : EventBase<TEventType>, new()
        // {
        //     if (target == null) throw new ArgumentNullException(nameof(target));
        //     // if (target is IOffscreenEventHandler handler)
        //     // {
        //     //     if (typeof(TEventType) == typeof(GeometryChangedEvent))
        //     //     {
        //     //         void RemovableCallback() => callback((TEventType)null);
        //     //         handler.onElementAdded += RemovableCallback;
        //     //         return () => handler.onElementAdded -= RemovableCallback;
        //     //     }
        //     // }
        //     // Debug.LogError("FATAL!");
        //     target.RegisterCallback<TEventType>(callback, useTrickleDown);
        //     return () => target.UnregisterCallback<TEventType>(callback, useTrickleDown);
        // }
        //
        // public static Action RegisterCallbackX<TEventType>(
        //     VisualElement target,
        //     EventCallback<TEventType> callback,
        //     TrickleDown useTrickleDown = TrickleDown.NoTrickleDown)
        //     where TEventType : EventBase<TEventType>, new()
        // {
        //     if (target == null) throw new ArgumentNullException(nameof(target));
        //     if (target is IOffscreenEventHandler handler)
        //     {
        //         if (typeof(TEventType) == typeof(GeometryChangedEvent))
        //         {
        //             void RemovableCallback() => callback((TEventType)null);
        //             handler.onElementAdded += RemovableCallback;
        //             return () => handler.onElementAdded -= RemovableCallback;
        //         }
        //     }
        //     Debug.LogError("FATAL!");
        //     return () => { Debug.LogError("UNREGISTERING FATAL!"); };
        // }

        public static void RegisterCallbackOnce<TEventType>(
            VisualElement target,
            EventCallback<TEventType> callback,
            TrickleDown useTrickleDown = TrickleDown.NoTrickleDown)
            where TEventType : EventBase<TEventType>, new()
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (target is IOffscreenEventHandler handler)
            {
                if (typeof(TEventType) == typeof(GeometryChangedEvent))
                {
                    handler.delayCall += () => callback((TEventType)null);
                    return;
                }
            }
            target.RegisterCallbackOnce<TEventType>(callback, useTrickleDown);
        }
        
        public static Action RegisterOnPropertyAddedCallback(this IDrawer self, Action callback)
        // public static Action RegisterOnPropertyAddedCallback<TEventType>(
        //     VisualElement target,
        //     EventCallback<TEventType> callback,
        //     TrickleDown useTrickleDown = TrickleDown.NoTrickleDown)
        //     where TEventType : EventBase<TEventType>, new()
        {
            
            
            // if (target == null) throw new ArgumentNullException(nameof(target));
            if ((self is IDecorator decorator ? decorator.GhostContainer : self.Container)
                ?.GetCommonClosestParentEventHandler() is IOffscreenEventHandler handler)
            {
                void RemovableCallback() => callback();
                handler.onPropertyAdded += RemovableCallback;
                return () => handler.onPropertyAdded -= RemovableCallback;
            }
            Debug.LogError("FATAL! this drawer does not descend from any VisualElement implementing " + nameof(IOffscreenEventHandler) + ".");
            return () => { Debug.LogError("UNREGISTERING FATAL!"); };
        }
    }
}
