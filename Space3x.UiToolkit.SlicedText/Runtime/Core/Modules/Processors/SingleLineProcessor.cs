using System;
using Space3x.UiToolkit.SlicedText.Iterators;
using Space3x.UiToolkit.SlicedText.VisualElements;
using UnityEngine.UIElements;

namespace Space3x.UiToolkit.SlicedText.Processors
{
    public class SingleLineProcessor : ProcessorEventHandler, ILineBlockProcessor<TextLine>
    {
        public override Action<ChangeRecord> OnReadyChangeRecord { get; set; }
        
        public IColorize Colorizer { get; set; }

        public TextLine Create(string text)
        {
            return new TextLine()
            {
                text = Colorizer.Format(text)
            };
        }
        
        public StringSliceGroup Editor { get; set; }
        public VisualElement TargetContainer { get; set; }
        
        public ContentViewBase TargetContentView = null;

        protected bool IsContentView = false;
        
        public void Clear()
        {
            if (IsContentView) TargetContentView.Clear();
            else TargetContainer.Clear();
        }

        public void Insert(int index, ref StringSlice element)
        {
            var text = element.ToStringLine().Replace("\\", "\\\u0003");
            
            if (IsContentView) TargetContentView.Insert(index, Colorizer.Format(text));
            else TargetContainer.Insert(index, Create(text));
            
            InsertBlockAt(index, index, text);
        }

        public void Insert(int index, StringSlice element)
        {
            Insert(index, ref element);
        }

        public void Replace(int index, ref StringSlice element)
        {
            var text = element.ToStringLine().Replace("\\", "\\\u0003");
            
            if (IsContentView) TargetContentView.Replace(index, Colorizer.Format(text));
            else ((TextLine) TargetContainer.ElementAt(index)).text = Colorizer.Format(text);
            
            RemoveBlockAt(index);
            InsertBlockAt(index, index, text);
        }

        public void Replace(int index, StringSlice element)
        {
            Replace(index, ref element);
        }

        public TextLine ElementAt(int index)
        {
            if (IsContentView)
                return (TextLine) TargetContentView.TextElementAt(index);
            
            return (TextLine) TargetContainer.ElementAt(index);
        }

        public void RemoveAt(int index)
        {
            if (index >= (IsContentView ? TargetContentView.childCount : TargetContainer.childCount))
                return;
            
            if (IsContentView) TargetContentView.RemoveAt(index);
            else TargetContainer.RemoveAt(index);
            
            RemoveBlockAt(index);
        }

        public void GetReadyForChanges()
        {
            TargetContentView = TargetContainer as ContentViewBase;
            IsContentView = TargetContentView != null;
        }

        public void CommitChanges()
        {
            SendChangeRecordEvent();
        }

        public int Count()
        {
            return IsContentView ? TargetContentView.childCount : TargetContainer.childCount;
        }

        public int Length()
        {
            return IsContentView ? TargetContentView.childCount : TargetContainer.childCount;
        }
    }
}
