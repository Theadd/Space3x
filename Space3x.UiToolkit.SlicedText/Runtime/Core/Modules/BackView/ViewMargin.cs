using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.UiToolkit.SlicedText
{
    public class ViewMargin : VisualElement
    {
        public List<int> DisplayedLines = new List<int>();
        public ViewMargin()
        {
            AddToClassList("sliced-editor__margin-view");
        }

        public void Rebuild(int numLines)
        {
            hierarchy.Clear();
            
            for (var i = 0; i < numLines; i++)
            {
                hierarchy.Add(new LineNum());
            }
        }

        public void Update(int elementIndex, int lineIndex = -1, Line line = null, float y = -1f)
        {
            if (lineIndex == -1)
            {
                hierarchy.ElementAt(elementIndex).style.display = DisplayStyle.None;
                
                return;
            }

            var element = (LineNum) hierarchy.ElementAt(elementIndex);

            element.style.display = DisplayStyle.Flex;
            element.style.position = Position.Absolute;
            element.style.top = 0;
            element.style.left = 0;
            element.style.height = line.Height;
            element.transform.position = new Vector3(0, y, 0);
            element.text = (lineIndex + 1).ToString();
            element.LineIndex = lineIndex;
        }
    }
    
    public class LineNum : TextElement, ILine
    {
        public int LineIndex = -1;
        
        public LineNum()
        {
            text = string.Empty;
            ClearClassList();
            enableRichText = false;
            focusable = false;
            displayTooltipWhenElided = false;
            pickingMode = PickingMode.Position;
        }
    }
}
