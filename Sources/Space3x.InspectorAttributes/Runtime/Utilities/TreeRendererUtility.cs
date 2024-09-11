using System;
using Space3x.Properties.Types;
using UnityEngine.Internal;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes
{
    internal interface ITreeRenderer : ITreeContainer, ITreeAdd
    {
        void Render(bool shouldRender);
        // public abstract VisualElement contentContainer { get; }
    }

    public interface ITreeAdd
    {
        void Add(VisualElement child);
    }
    
    // public interface ITreeInsert
    // {
    //     void Insert(int index, VisualElement child);
    //     int IndexOf(VisualElement element);
    //     int childCount { get; }
    // }
    
    public interface ITreeContainer
    {
        VisualElement contentContainer { get; }
    }
    
    internal interface ITreeRendererUtility
    {
        VisualElement Create(IPropertyNode property);
    }
    
    [ExcludeFromDocs]
    internal static class TreeRendererUtility
    {
        private static ITreeRendererUtility s_Implementation;
        private static int s_CurrentPriority = -1;

        internal static void RegisterImplementationProvider(ITreeRendererUtility provider, int priority = 0)
        {
            s_Implementation = s_CurrentPriority <= priority ? provider : s_Implementation;
            s_CurrentPriority = Math.Max(s_CurrentPriority, priority);
        }

        public static VisualElement Create(IPropertyNode property) => s_Implementation?.Create(property);
    }
}
