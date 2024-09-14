namespace Space3x.InspectorAttributes
{
    public class Context
    {
        // public bool AttachDecorators { get; set; } = false;
        // public bool ShowInInspector { get; set; } = false;

        internal ITreeRenderer Target { get; set; }
    }
    
    internal interface ITreeRenderer : ITreeContainer, ITreeAdd
    {
        void Render(bool shouldRender);

        ITreeRenderer GetTreeRenderer();

        NTree<Context> TreeNode { get; set; }
    }
    
    internal interface IOffscreenRenderer : ITreeRenderer
    {
        
    }
    
    public interface IOffscreenContainer
    {
        void Attach();
        void Detach();
        internal IOffscreenRenderer Renderer { get; set; }
    }
}
