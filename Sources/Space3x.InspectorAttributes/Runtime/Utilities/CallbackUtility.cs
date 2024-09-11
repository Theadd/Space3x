using System;
using Space3x.Properties.Types;
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
    }
}
