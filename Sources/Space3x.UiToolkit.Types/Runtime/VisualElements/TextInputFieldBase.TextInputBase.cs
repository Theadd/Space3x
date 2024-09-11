using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.UiToolkit.Types
{
    public abstract partial class TextInputFieldBase<TValueType>
    {
        // [UxmlElement]
        [HideInInspector]
        public abstract partial class TextInputBase : VisualElement
        {
            public ScrollView scrollView;
            public VisualElement multilineContainer;
            public static readonly string innerComponentsModifierName = "--inner-input-field-component";
            public static readonly string innerTextElementUssClassName = TextElement.ussClassName + innerComponentsModifierName;
            public static readonly string innerTextElementWithScrollViewUssClassName = TextElement.ussClassName + innerComponentsModifierName + "--scroll-view";
            public static readonly string horizontalVariantInnerTextElementUssClassName = TextElement.ussClassName + innerComponentsModifierName + "--horizontal";
            public static readonly string verticalVariantInnerTextElementUssClassName = TextElement.ussClassName + innerComponentsModifierName + "--vertical";
            public static readonly string verticalHorizontalVariantInnerTextElementUssClassName = TextElement.ussClassName + innerComponentsModifierName + "--vertical-horizontal";
            public static readonly string innerScrollviewUssClassName = ScrollView.ussClassName + innerComponentsModifierName;
            public static readonly string innerViewportUssClassName = ScrollView.viewportUssClassName + innerComponentsModifierName;
            public static readonly string innerContentContainerUssClassName = ScrollView.contentUssClassName + innerComponentsModifierName;

            public Vector2 scrollOffset = Vector2.zero;
            private bool m_ScrollViewWasClamped;
            private Vector2 lastCursorPos = Vector2.zero;
            public ScrollerVisibility verticalScrollerVisibility = ScrollerVisibility.Hidden;

            public TextValueElement textElement { get; private set; }

            private static PropertyInfo s_AcceptCharacterProperty;
            private static PropertyInfo s_UpdateScrollOffsetProperty = 
                typeof(ITextEdition).GetProperty("UpdateScrollOffset", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            private static PropertyInfo s_UpdateValueFromTextProperty = 
                typeof(ITextEdition).GetProperty("UpdateValueFromText", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            private static PropertyInfo s_UpdateTextFromValueProperty = 
                typeof(ITextEdition).GetProperty("UpdateTextFromValue", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            private static PropertyInfo s_MoveFocusToCompositeRootProperty = 
                typeof(ITextEdition).GetProperty("MoveFocusToCompositeRoot", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            private static PropertyInfo s_GetDefaultValueTypeProperty = 
                typeof(ITextEdition).GetProperty("GetDefaultValueType", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            // private string m_OriginalText;
            private static FieldInfo s_OriginalTextField = typeof(TextElement).GetField("m_OriginalText", BindingFlags.NonPublic | BindingFlags.Instance);
            // internal bool multiline { get; set; }
            private static PropertyInfo s_Multiline = typeof(ITextEdition).GetProperty("multiline", BindingFlags.NonPublic | BindingFlags.Instance);
            // ITextSelection -> internal float cursorWidth { get; set; }
            private static PropertyInfo s_CursorWidth = typeof(ITextSelection).GetProperty("cursorWidth", BindingFlags.NonPublic | BindingFlags.Instance);
            // ITextSelection -> internal float lineHeightAtCursorPosition { get; }
            private static PropertyInfo s_LineHeightAtCursorPosition = typeof(ITextSelection).GetProperty("lineHeightAtCursorPosition", BindingFlags.NonPublic | BindingFlags.Instance);
            // ITextEdition -> internal void UpdateText(string value);
            private static MethodInfo s_UpdateText = typeof(ITextEdition).GetMethod("UpdateText", BindingFlags.NonPublic | BindingFlags.Instance);
            
            private static void SetAcceptCharacter(ITextEdition target, Func<char, bool> value)
            {
                s_AcceptCharacterProperty ??= typeof(ITextEdition).GetProperty(
                    "AcceptCharacter", 
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                s_AcceptCharacterProperty?.SetValue(target, value);
            }

            private static void SetUpdateScrollOffset(ITextEdition target, Action<bool> value) =>
                s_UpdateScrollOffsetProperty?.SetValue(target, value);
            
            private static void SetUpdateValueFromText(ITextEdition target, Action value) =>
                s_UpdateValueFromTextProperty?.SetValue(target, value);
            
            private static void SetUpdateTextFromValue(ITextEdition target, Action value) =>
                s_UpdateTextFromValueProperty?.SetValue(target, value);
            
            private static void SetMoveFocusToCompositeRoot(ITextEdition target, Action value) =>
                s_MoveFocusToCompositeRootProperty?.SetValue(target, value);
            
            private static void SetGetDefaultValueType(ITextEdition target, Func<string> value) =>
                s_GetDefaultValueTypeProperty?.SetValue(target, value);

            public bool Multiline
            {
                get => (bool) s_Multiline.GetValue(this.textEdition);
                set => s_Multiline.SetValue(this.textEdition, value);
            }
            
            public float CursorWidth
            {
                get => (float) s_CursorWidth.GetValue(this.textSelection);
                set => s_CursorWidth.SetValue(this.textSelection, value);
            }
            
            public float LineHeightAtCursorPosition => (float) s_LineHeightAtCursorPosition.GetValue(this.textSelection);

            public void UpdateText(string value) => s_UpdateText.Invoke(this.textEdition, new object[] { value });
            
            public TextInputBase()
            {
                this.delegatesFocus = true;
                this.textElement = new TextValueElement();
                this.textElement.selection.isSelectable = true;
                this.textEdition.isReadOnly = false;
                this.textSelection.isSelectable = true;
                this.textSelection.selectAllOnFocus = true;
                this.textSelection.selectAllOnMouseUp = true;
                this.textElement.enableRichText = false;
                this.textElement.tabIndex = 0;
                SetAcceptCharacter(this.textEdition, new Func<char, bool>(this.AcceptCharacter));
                // this.textEdition.AcceptCharacter += new Func<char, bool>(this.AcceptCharacter);
                SetUpdateScrollOffset(this.textEdition, new Action<bool>(this.UpdateScrollOffset));
                // this.textEdition.UpdateScrollOffset += new Action<bool>(this.UpdateScrollOffset);
                SetUpdateValueFromText(this.textEdition, new Action(this.UpdateValueFromText));
                // this.textEdition.UpdateValueFromText += new Action(this.UpdateValueFromText);
                SetUpdateTextFromValue(this.textEdition, new Action(this.UpdateTextFromValue));
                // this.textEdition.UpdateTextFromValue += new Action(this.UpdateTextFromValue);
                SetMoveFocusToCompositeRoot(this.textEdition, new Action(this.MoveFocusToCompositeRoot));
                // this.textEdition.MoveFocusToCompositeRoot += new Action(this.MoveFocusToCompositeRoot);
                SetGetDefaultValueType(this.textEdition, new Func<string>(this.GetDefaultValueType));
                // this.textEdition.GetDefaultValueType = new Func<string>(this.GetDefaultValueType);
                this.AddToClassList(TextInputFieldBase<TValueType>.inputUssClassName);
                this.name = TextInputFieldBase<string>.textInputUssName;
                this.SetSingleLine();
                this.RegisterCallback<CustomStyleResolvedEvent>(
                    new EventCallback<CustomStyleResolvedEvent>(this.OnInputCustomStyleResolved));
                this.tabIndex = -1;
            }

            public ITextSelection textSelection => this.textElement.selection;

            public ITextEdition textEdition => (ITextEdition)textElement;

            public bool isDragging { get; set; }

            public string text
            {
                get => this.textElement.text;
                set
                {
                    if (this.textElement.text == value)
                        return;
                    this.textElement.text = value;
                }
            }

            public string originalText => s_OriginalTextField.GetValue(this.textElement) as string;

            protected virtual TValueType StringToValue(string str) => throw new NotSupportedException();

            public void UpdateValueFromText()
            {
                ((TextInputFieldBase<TValueType>)this.parent).UpdateValueFromText();
            }

            public void UpdateTextFromValue()
            {
                ((TextInputFieldBase<TValueType>)this.parent).UpdateTextFromValue();
            }

            public void MoveFocusToCompositeRoot()
            {
                // FIXME: this.focusController.SwitchFocus((Focusable)this.parent);
                /* FIXME: */ ((Focusable)this.parent).Focus();
                this.textEdition.keyboardType = TouchScreenKeyboardType.Default;
                this.textEdition.autoCorrection = false;
            }

            private void MakeSureScrollViewDoesNotLeakEvents(ChangeEvent<float> evt)
            {
                evt.StopPropagation();
            }

            public void SetSingleLine()
            {
                this.hierarchy.Clear();
                this.RemoveMultilineComponents();
                this.Add((VisualElement)this.textElement);
                this.AddToClassList(TextInputFieldBase<TValueType>.singleLineInputUssClassName);
                this.textElement.AddToClassList(TextInputFieldBase<TValueType>.TextInputBase
                    .innerTextElementUssClassName);
                this.textElement.RegisterCallback<GeometryChangedEvent>(
                    new EventCallback<GeometryChangedEvent>(this.TextElementOnGeometryChangedEvent));
                if (!(this.scrollOffset != Vector2.zero))
                    return;
                this.scrollOffset.y = 0.0f;
                this.UpdateScrollOffset();
            }

            public void SetMultiline()
            {
                // if (!this.textEdition.multiline)
                if (!Multiline)
                    return;
                this.RemoveSingleLineComponents();
                this.RemoveMultilineComponents();
                if (this.verticalScrollerVisibility != ScrollerVisibility.Hidden && this.scrollView == null)
                {
                    this.scrollView = new ScrollView();
                    this.scrollView.Add((VisualElement)this.textElement);
                    this.Add((VisualElement)this.scrollView);
                    this.SetScrollViewMode();
                    this.scrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
                    this.scrollView.verticalScrollerVisibility = this.verticalScrollerVisibility;
                    this.scrollView.AddToClassList(TextInputFieldBase<TValueType>.TextInputBase
                        .innerScrollviewUssClassName);
                    this.scrollView.contentViewport.AddToClassList(TextInputFieldBase<TValueType>.TextInputBase
                        .innerViewportUssClassName);
                    this.scrollView.contentContainer.AddToClassList(TextInputFieldBase<TValueType>.TextInputBase
                        .innerContentContainerUssClassName);
                    this.scrollView.contentContainer.RegisterCallback<GeometryChangedEvent>(
                        new EventCallback<GeometryChangedEvent>(this.ScrollViewOnGeometryChangedEvent));
                    this.scrollView.verticalScroller.slider.RegisterValueChangedCallback<float>(
                        new EventCallback<ChangeEvent<float>>(this.MakeSureScrollViewDoesNotLeakEvents));
                    this.scrollView.verticalScroller.slider.focusable = false;
                    this.scrollView.horizontalScroller.slider.RegisterValueChangedCallback<float>(
                        new EventCallback<ChangeEvent<float>>(this.MakeSureScrollViewDoesNotLeakEvents));
                    this.scrollView.horizontalScroller.slider.focusable = false;
                    this.AddToClassList(TextInputFieldBase<TValueType>.multilineInputWithScrollViewUssClassName);
                    this.textElement.AddToClassList(TextInputFieldBase<TValueType>.TextInputBase
                        .innerTextElementWithScrollViewUssClassName);
                }
                else
                {
                    if (this.multilineContainer != null)
                        return;
                    this.textElement.RegisterCallback<GeometryChangedEvent>(
                        new EventCallback<GeometryChangedEvent>(this.TextElementOnGeometryChangedEvent));
                    this.multilineContainer = new VisualElement() { };
                    this.multilineContainer.EnableInClassList(multilineContainerClassName, true);
                    this.multilineContainer.Add((VisualElement)this.textElement);
                    this.Add(this.multilineContainer);
                    this.SetMultilineContainerStyle();
                    this.AddToClassList(TextInputFieldBase<TValueType>.multilineInputUssClassName);
                    this.textElement.AddToClassList(TextInputFieldBase<TValueType>.TextInputBase
                        .innerTextElementUssClassName);
                }
            }

            private void ScrollViewOnGeometryChangedEvent(GeometryChangedEvent e)
            {
                Rect rect = e.oldRect;
                Vector2 size1 = rect.size;
                rect = e.newRect;
                Vector2 size2 = rect.size;
                if (size1 == size2)
                    return;
                this.UpdateScrollOffset();
            }

            private void TextElementOnGeometryChangedEvent(GeometryChangedEvent e)
            {
                Rect rect1 = e.oldRect;
                Vector2 size1 = rect1.size;
                rect1 = e.newRect;
                Vector2 size2 = rect1.size;
                if (size1 == size2)
                    return;
                Rect rect2 = e.oldRect;
                double x1 = (double)rect2.size.x;
                rect2 = e.newRect;
                double x2 = (double)rect2.size.x;
                this.UpdateScrollOffset(false, (double)Math.Abs((float)(x1 - x2)) > 1.0000000031710769E-30);
            }

            public void OnInputCustomStyleResolved(CustomStyleResolvedEvent e)
            {
                ICustomStyle customStyle = e.customStyle;
                Color color1;
                if (customStyle.TryGetValue(TextInputFieldBase<TValueType>.s_SelectionColorProperty, out color1))
                    this.textSelection.selectionColor = color1;
                Color color2;
                if (customStyle.TryGetValue(TextInputFieldBase<TValueType>.s_CursorColorProperty, out color2))
                    this.textSelection.cursorColor = color2;
                this.SetScrollViewMode();
                this.SetMultilineContainerStyle();
            }

            private string GetDefaultValueType()
            {
                TValueType valueType = default(TValueType);
                string defaultValueType;
                if ((object)valueType != null)
                {
                    ref TValueType local = ref valueType;
                    local = default(TValueType);
                    defaultValueType = local.ToString();
                }
                else
                    defaultValueType = "";

                return defaultValueType;
            }

            public virtual bool AcceptCharacter(char c)
            {
                return !this.textEdition.isReadOnly && this.enabledInHierarchy;
            }

            public void UpdateScrollOffset(bool isBackspace = false)
            {
                this.UpdateScrollOffset(isBackspace, false);
            }

            public void UpdateScrollOffset(bool isBackspace, bool widthChanged)
            {
                ITextSelection textSelection = this.textSelection;
                if (textSelection.cursorIndex < 0 || textSelection.cursorIndex <= 0 && textSelection.selectIndex <= 0 &&
                    this.scrollOffset == Vector2.zero)
                    return;
                if (this.scrollView != null)
                {
                    this.scrollOffset = this.GetScrollOffset(this.scrollView.scrollOffset.x,
                        this.scrollView.scrollOffset.y, this.scrollView.contentViewport.layout.width, isBackspace,
                        widthChanged);
                    this.scrollView.scrollOffset = this.scrollOffset;
                    this.m_ScrollViewWasClamped =
                        (double)this.scrollOffset.x > (double)this.scrollView.scrollOffset.x ||
                        (double)this.scrollOffset.y > (double)this.scrollView.scrollOffset.y;
                }
                else
                {
                    Vector3 position = this.textElement.transform.position;
                    double x = (double)this.scrollOffset.x;
                    double y1 = (double)this.scrollOffset.y;
                    Rect contentRect = this.contentRect;
                    double width = (double)contentRect.width;
                    int num1 = isBackspace ? 1 : 0;
                    int num2 = widthChanged ? 1 : 0;
                    this.scrollOffset = this.GetScrollOffset((float)x, (float)y1, (float)width, num1 != 0, num2 != 0);
                    ref Vector3 local = ref position;
                    double y2 = (double)this.scrollOffset.y;
                    contentRect = this.textElement.contentRect;
                    double height1 = (double)contentRect.height;
                    contentRect = this.contentRect;
                    double height2 = (double)contentRect.height;
                    double b = (double)Math.Abs((float)(height1 - height2));
                    double num3 = -(double)Mathf.Min((float)y2, (float)b);
                    local.y = (float)num3;
                    position.x = -this.scrollOffset.x;
                    if (!position.Equals(this.textElement.transform.position))
                        this.textElement.transform.position = position;
                }
            }

            private Vector2 GetScrollOffset(
                float xOffset,
                float yOffset,
                float contentViewportWidth,
                bool isBackspace,
                bool widthChanged)
            {
                Vector2 cursorPosition = this.textSelection.cursorPosition;
                float cursorWidth = CursorWidth;
                float x = xOffset;
                float y = yOffset;
                if ((((double)Math.Abs(this.lastCursorPos.x - cursorPosition.x) > 0.05000000074505806
                        ? 1
                        : (this.m_ScrollViewWasClamped ? 1 : 0)) | (widthChanged ? 1 : 0)) != 0)
                {
                    if ((double)cursorPosition.x >
                        (double)xOffset + (double)contentViewportWidth - (double)cursorWidth ||
                        (double)xOffset > 0.0 & widthChanged)
                        x = Mathf.Max(Mathf.Ceil(cursorPosition.x + cursorWidth - contentViewportWidth), 0.0f);
                    else if ((double)cursorPosition.x < (double)xOffset + 5.0)
                        x = Mathf.Max(cursorPosition.x - 5f, 0.0f);
                }

                // if (this.textEdition.multiline &&
                if (Multiline &&
                    ((double)Math.Abs(this.lastCursorPos.y - cursorPosition.y) > 0.05000000074505806 ||
                     this.m_ScrollViewWasClamped))
                {
                    if ((double)cursorPosition.y > (double)this.contentRect.height + (double)yOffset)
                        y = cursorPosition.y - this.contentRect.height;
                    else if ((double)cursorPosition.y < (double)LineHeightAtCursorPosition +
                             (double)yOffset + 0.05000000074505806)
                        y = cursorPosition.y - LineHeightAtCursorPosition;
                }

                this.lastCursorPos = cursorPosition;
                return (double)Math.Abs(xOffset - x) > 0.05000000074505806 ||
                       (double)Math.Abs(yOffset - y) > 0.05000000074505806
                    ? new Vector2(x, y)
                    : (this.scrollView != null ? this.scrollView.scrollOffset : this.scrollOffset);
            }

            public void SetScrollViewMode()
            {
                if (this.scrollView == null)
                    return;
                this.textElement.RemoveFromClassList(TextInputFieldBase<TValueType>.TextInputBase
                    .verticalVariantInnerTextElementUssClassName);
                this.textElement.RemoveFromClassList(TextInputFieldBase<TValueType>.TextInputBase
                    .verticalHorizontalVariantInnerTextElementUssClassName);
                this.textElement.RemoveFromClassList(TextInputFieldBase<TValueType>.TextInputBase
                    .horizontalVariantInnerTextElementUssClassName);
                // if (this.textEdition.multiline && (this.computedStyle.whiteSpace == WhiteSpace.Normal ||
                //                                    this.computedStyle.whiteSpace == WhiteSpace.PreWrap))
                if (Multiline && (this.resolvedStyle.whiteSpace == WhiteSpace.Normal
#if UNITY_6000_0_OR_NEWER
                                  || this.resolvedStyle.whiteSpace == WhiteSpace.PreWrap
#endif
                                  ))
                {
                    this.textElement.AddToClassList(TextInputFieldBase<TValueType>.TextInputBase
                        .verticalVariantInnerTextElementUssClassName);
                    this.scrollView.mode = ScrollViewMode.Vertical;
                }
                else if (Multiline)
                {
                    this.textElement.AddToClassList(TextInputFieldBase<TValueType>.TextInputBase
                        .verticalHorizontalVariantInnerTextElementUssClassName);
                    this.scrollView.mode = ScrollViewMode.VerticalAndHorizontal;
                }
                else
                {
                    this.textElement.AddToClassList(TextInputFieldBase<TValueType>.TextInputBase
                        .horizontalVariantInnerTextElementUssClassName);
                    this.scrollView.mode = ScrollViewMode.Horizontal;
                }
            }

            private void SetMultilineContainerStyle()
            {
                if (this.multilineContainer == null)
                    return;
                if (this.resolvedStyle.whiteSpace == WhiteSpace.Normal
#if UNITY_6000_0_OR_NEWER
                    || this.resolvedStyle.whiteSpace == WhiteSpace.PreWrap
#endif
                    )
                    this.style.overflow = (StyleEnum<Overflow>)Overflow.Hidden;
                else
                    this.style.overflow = (StyleEnum<Overflow>)(Overflow)2;
            }

            private void RemoveSingleLineComponents()
            {
                this.RemoveFromClassList(TextInputFieldBase<TValueType>.singleLineInputUssClassName);
                this.textElement.RemoveFromClassList(TextInputFieldBase<TValueType>.TextInputBase
                    .innerTextElementUssClassName);
                this.textElement.RemoveFromHierarchy();
                this.textElement.UnregisterCallback<GeometryChangedEvent>(
                    new EventCallback<GeometryChangedEvent>(this.TextElementOnGeometryChangedEvent));
            }

            private void RemoveMultilineComponents()
            {
                if (this.scrollView != null)
                {
                    this.scrollView.RemoveFromHierarchy();
                    this.scrollView.contentContainer.UnregisterCallback<GeometryChangedEvent>(
                        new EventCallback<GeometryChangedEvent>(this.ScrollViewOnGeometryChangedEvent));
                    this.scrollView.verticalScroller.slider.UnregisterValueChangedCallback<float>(
                        new EventCallback<ChangeEvent<float>>(this.MakeSureScrollViewDoesNotLeakEvents));
                    this.scrollView.horizontalScroller.slider.UnregisterValueChangedCallback<float>(
                        new EventCallback<ChangeEvent<float>>(this.MakeSureScrollViewDoesNotLeakEvents));
                    this.scrollView = (ScrollView)null;
                    this.textElement.RemoveFromClassList(TextInputFieldBase<TValueType>.TextInputBase
                        .verticalVariantInnerTextElementUssClassName);
                    this.textElement.RemoveFromClassList(TextInputFieldBase<TValueType>.TextInputBase
                        .verticalHorizontalVariantInnerTextElementUssClassName);
                    this.textElement.RemoveFromClassList(TextInputFieldBase<TValueType>.TextInputBase
                        .horizontalVariantInnerTextElementUssClassName);
                    this.RemoveFromClassList(TextInputFieldBase<TValueType>.multilineInputWithScrollViewUssClassName);
                    this.textElement.RemoveFromClassList(TextInputFieldBase<TValueType>.TextInputBase
                        .innerTextElementWithScrollViewUssClassName);
                }

                if (this.multilineContainer == null)
                    return;
                this.textElement.transform.position = Vector3.zero;
                this.multilineContainer.RemoveFromHierarchy();
                this.textElement.UnregisterCallback<GeometryChangedEvent>(
                    new EventCallback<GeometryChangedEvent>(this.TextElementOnGeometryChangedEvent));
                this.multilineContainer = (VisualElement)null;
                this.RemoveFromClassList(TextInputFieldBase<TValueType>.multilineInputUssClassName);
            }

            public bool SetVerticalScrollerVisibility(ScrollerVisibility sv)
            {
                if (!Multiline)
                    return false;
                this.verticalScrollerVisibility = sv;
                if (this.scrollView == null)
                    this.SetMultiline();
                else
                    this.scrollView.verticalScrollerVisibility = this.verticalScrollerVisibility;
                return true;
            }
            
            #region Obsolete
            [Obsolete("SelectAll() is deprecated. Use textSelection.SelectAll() instead.")]
            public void SelectAll() => this.textSelection.SelectAll();

            [Obsolete("isReadOnly is deprecated. Use textEdition.isReadOnly instead.")]
            public bool isReadOnly
            {
                get => this.textEdition.isReadOnly;
                set => this.textEdition.isReadOnly = value;
            }

            [Obsolete("maxLength is deprecated. Use textEdition.maxLength instead.")]
            public int maxLength
            {
                get => this.textEdition.maxLength;
                set => this.textEdition.maxLength = value;
            }

            [Obsolete("maskChar is deprecated. Use textEdition.maskChar instead.")]
            public char maskChar
            {
                get => this.textEdition.maskChar;
                set => this.textEdition.maskChar = value;
            }

            [Obsolete("isPasswordField is deprecated. Use textEdition.isPassword instead.")]
            public virtual bool isPasswordField
            {
                get => this.textEdition.isPassword;
                set => this.textEdition.isPassword = value;
            }

            [Obsolete("selectionColor is deprecated. Use textSelection.selectionColor instead.")]
            public Color selectionColor
            {
                get => this.textSelection.selectionColor;
                set => this.textSelection.selectionColor = value;
            }

            [Obsolete("cursorColor is deprecated. Use textSelection.cursorColor instead.")]
            public Color cursorColor
            {
                get => this.textSelection.cursorColor;
                set => this.textSelection.cursorColor = value;
            }

            [Obsolete("cursorIndex is deprecated. Use textSelection.cursorIndex instead.")]
            public int cursorIndex => this.textSelection.cursorIndex;

            [Obsolete("selectIndex is deprecated. Use textSelection.selectIndex instead.")]
            public int selectIndex => this.textSelection.selectIndex;

            [Obsolete("doubleClickSelectsWord is deprecated. Use textSelection.doubleClickSelectsWord instead.")]
            public bool doubleClickSelectsWord
            {
                get => this.textSelection.doubleClickSelectsWord;
                set => this.textSelection.doubleClickSelectsWord = value;
            }

            [Obsolete("tripleClickSelectsLine is deprecated. Use textSelection.tripleClickSelectsLine instead.")]
            public bool tripleClickSelectsLine
            {
                get => this.textSelection.tripleClickSelectsLine;
                set => this.textSelection.tripleClickSelectsLine = value;
            }
            #endregion Obsolete

            private TextInputKeyboardEventHandler<TValueType> m_KeyboardEventHandler;
            
            public void HandleKeyDownEvent(KeyDownEvent ev)
            {
                m_KeyboardEventHandler ??= new TextInputKeyboardEventHandler<TValueType>(this);
                m_KeyboardEventHandler.HandleKeyDownEvent(ev);
            }
        }
    }
}
