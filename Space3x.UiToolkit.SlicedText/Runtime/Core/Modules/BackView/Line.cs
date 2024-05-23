using System.Collections.Generic;

namespace Space3x.UiToolkit.SlicedText
{
    public interface ILine { }
    
    public class Line : ILine
    {
        public int RawLength { get; set; }
        public string FormattedText { get; set; } = string.Empty;
        public string DisplayText;
        public int Height;
        public int LastBaseline = 0;
        public List<int> Breakpoints = new List<int>();

        /// <summary>
        /// Whether it is collapsed or not.
        /// </summary>
        public bool Visible = true;

        /// <summary>
        /// petty workaround for equality comparison
        /// </summary>
        public int Id;

        public static int Counter = 0;

        protected bool Equals(Line other)
        {
            return Id == other.Id;
        }

        public bool Equals(ILine other)
        {
            return Id == ((Line) other).Id;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is Line other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }
}
