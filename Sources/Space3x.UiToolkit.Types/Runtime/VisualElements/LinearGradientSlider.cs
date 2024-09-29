using Space3x.Attributes.Types;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.UiToolkit.Types
{
    public interface IGradientSlider
    {
        Color lowValueColor { get; set; }
        Color highValueColor { get; set; }
        ColorModel colorModel { get; set; }
        int pixelsWidth { get; set; }
        void Update();
    }
    
    /// <summary>
    /// Draws a two-color linear gradient over the track of a slider field for float values.
    /// </summary>
    [UxmlElement]
    public partial class LinearGradientSlider : Slider, IGradientSlider
    {
        public static readonly BindingId lowValueColorProperty = (BindingId) nameof (lowValueColor);
        public static readonly BindingId highValueColorProperty = (BindingId) nameof (highValueColor);
        public static readonly BindingId colorModelProperty = (BindingId) nameof (colorModel);
        public static readonly BindingId pixelsWidthProperty = (BindingId) nameof (pixelsWidth);
        protected VisualElement DragContainer;
        protected VisualElement BackgroundTrack;
        
        #region Properties & Backing Values
        [SerializeField]
        [DontCreateProperty]
        private Color m_LowValueColor;
        
        [SerializeField]
        [DontCreateProperty]
        private Color m_HighValueColor;
        
        [SerializeField]
        [DontCreateProperty]
        private ColorModel m_ColorModel = ColorModel.RGB;
        
        [SerializeField]
        [DontCreateProperty]
        private int m_PixelsWidth = 100;
        
        [CreateProperty]
        [UxmlAttribute]
        public Color lowValueColor
        {
            get => m_LowValueColor;
            set
            {
                if (m_LowValueColor.Equals(value)) return;
                m_LowValueColor = value;
                NotifyPropertyChanged(in lowValueColorProperty);
                DelayedUpdateCall();
            }
        }
        
        [CreateProperty]
        [UxmlAttribute]
        public Color highValueColor
        {
            get => m_HighValueColor;
            set
            {
                if (m_HighValueColor.Equals(value)) return;
                m_HighValueColor = value;
                NotifyPropertyChanged(in highValueColorProperty);
                DelayedUpdateCall();
            }
        }
        
        [CreateProperty]
        [UxmlAttribute]
        public ColorModel colorModel
        {
            get => m_ColorModel;
            set
            {
                if (m_ColorModel.Equals(value)) return;
                m_ColorModel = value;
                NotifyPropertyChanged(in colorModelProperty);
                DelayedUpdateCall();
            }
        }
        
        [CreateProperty]
        [UxmlAttribute]
        public int pixelsWidth
        {
            get => m_PixelsWidth;
            set
            {
                if (m_PixelsWidth.Equals(value)) return;
                m_PixelsWidth = value;
                NotifyPropertyChanged(in pixelsWidthProperty);
                DelayedUpdateCall();
            }
        }
        #endregion

        public LinearGradientSlider() : base()
        {
            this.WithClasses(true, "ui3x-gradient-slider", "unity-inspector-main-container");   // "unity-inspector-element"
            DragContainer = this.Q(className: dragContainerUssClassName);
            BackgroundTrack = new VisualElement()
            {
                usageHints = UsageHints.DynamicColor,
                style =
                {
                    position = Position.Absolute,
                    height = StyleKeyword.Auto,
                    width = StyleKeyword.Auto,
                }
            };
            BackgroundTrack.AddToClassList("ui3x-gradient-slider--image");
            DragContainer.Insert(0, BackgroundTrack);
        }

        [EventInterest(typeof(AttachToPanelEvent))]
        protected override void HandleEventBubbleUp(EventBase ev)
        {
            if (ev is not AttachToPanelEvent) return;
            if (panel?.contextType == ContextType.Editor)
                EnableInClassList(UssConstants.UssEditorUI, true);
            if (labelElement != null)
            {
                labelElement.style.minWidth = new StyleLength(StyleKeyword.Null);
                labelElement.style.width = new StyleLength(StyleKeyword.Null);
            }
            Update();
        }
        
        private IVisualElementScheduledItem m_ScheduledUpdateCall = null;
        
        private void DelayedUpdateCall()
        {
            m_ScheduledUpdateCall ??= schedule.Execute(Update);
            if (!m_ScheduledUpdateCall.isActive)
                m_ScheduledUpdateCall.ExecuteLater(16);
        }

        public virtual void Update()
        {
            if (BackgroundTrack == null || panel == null) return;
            BackgroundTrack.style.backgroundImage = new StyleBackground(
                m_ColorModel == ColorModel.RGB
                    ? PixelUtility.CreateGradientTexture2D(lowValueColor, highValueColor, pixelsWidth)
                    : PixelUtility.CreateHSVGradientTexture2D(lowValueColor, highValueColor, pixelsWidth));
        }
    }
}
