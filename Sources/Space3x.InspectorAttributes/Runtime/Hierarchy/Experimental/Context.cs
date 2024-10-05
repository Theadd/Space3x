namespace Space3x.InspectorAttributes
{
    public enum LocalizationMode
    {
        None = 0,
        Auto = 1,
    }
    
    public class Context
    {
        // public bool AttachDecorators { get; set; } = false;
        // public bool ShowInInspector { get; set; } = false;

        public LocalizationMode LocalizationMode { get; set; }
        
        public string LocalizationTable { get; set; } = null;
        
        internal ITreeRenderer Target { get; set; }
    }
    
    internal interface ITreeRenderer : ITreeContainer, ITreeAdd
    {
        void Render(bool shouldRender);

        ITreeRenderer GetTreeRenderer();

        NTree<Context> TreeNode { get; set; }
    }
    
    internal interface IOffscreenRenderer : ITreeRenderer { }
    
    public interface IOffscreenContainer
    {
        void Attach();
        void Detach();
        internal IOffscreenRenderer Renderer { get; set; }
    }
}
