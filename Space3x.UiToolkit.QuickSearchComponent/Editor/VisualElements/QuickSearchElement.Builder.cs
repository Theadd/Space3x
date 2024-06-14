using Space3x.UiToolkit.Types;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using SplitterContainer = Space3x.UiToolkit.Types.SplitterContainer;
using ToolbarSearchField = Space3x.UiToolkit.SlicedText.InputFields.ToolbarSearchField;

namespace Space3x.UiToolkit.QuickSearchComponent.Editor.VisualElements
{
    public partial class QuickSearchElement
    {
        [UxmlAttribute]
        public int FixedPaneInitialDimension { get; set; } = 175;
        [UxmlAttribute]
        public bool EnableFilterByType { get; set; } = true;
        [UxmlAttribute]
        public bool EnableFilterByVisibility { get; set; } = true;
        [UxmlAttribute]
        public bool EnableFilterByAssembly { get; set; } = true;
        [UxmlAttribute]
        public bool IsSelectionMultiple { get; set; } = false;
        
        public (SplitterContainer splitter, 
            ToolbarSearchField searchField, 
            ToolbarButton toggleSidePanelButton, 
            QuickListView listView, 
            ToggleableListView filtersTabListView,
            ToggleableListView filtersTabListView2,
            ToggleableListView assembliesTabListView
            ) CreateElementGUI(
                int fixedPaneInitialDimension = 175, 
                bool enableFilterByType = true,
                bool enableFilterByVisibility = true,
                bool enableFilterByAssembly = true,
                bool isSelectionMultiple = false)
        {
            SplitterContainer splitter;
            ToolbarSearchField searchField;
            ToolbarButton toggleSidePanelButton;
            QuickListView listView;
            ToggleableListView filtersTabListView;
            ToggleableListView filtersTabListView2;
            ToggleableListView assembliesTabListView;
            
            (splitter = new SplitterContainer
                {
                    name = "ui3x-splitter",
                    fixedPaneInitialDimension = fixedPaneInitialDimension,
                    fixedPaneIndex = 1,
                    orientation = TwoPaneSplitViewOrientation.Horizontal,
                    style = { flexDirection = FlexDirection.Row },
                    focusable = true
                }).WithClasses("ui3x-splitter")
                .AlsoAdd(new VisualElement() { name = "ui3x-content-panel" }.WithClasses("ui3x-content-panel")
                    .AlsoAdd(new VisualElement() { name = "ui3x-panel-header" }.WithClasses("ui3x-panel-header")
                        .AlsoAdd((new Toolbar()).WithClasses("ui3x-toolbar")
                            .AlsoAdd((searchField = new ToolbarSearchField() { name = "ui3x-search-field" }).WithClasses("ui3x-search-field"))
                            .AlsoAdd((new ToolbarSpacer()).WithClasses("ui3x-toolbar-spacer"))
                            .AlsoAdd((toggleSidePanelButton = new ToolbarButton() { name = "ui3x-toggle-side-panel", text = "\u2261", tooltip = "Side Panel" }).WithClasses("ui3x-toolbar-button"))))
                    .AlsoAdd((listView = new QuickListView() { IsSelectionMultiple = isSelectionMultiple }).WithClasses("ui3x-list-view")))
                .AlsoAdd(new VisualElement() { name = "ui3x-side-panel" }.WithClasses("ui3x-side-panel")
                    .AlsoAdd(new TabView() { name = "ui3x-tab-view" }.WithClasses("ui3x-tab-view")
                        .AlsoAdd((new Tab("Filters") { name = "ui3x-tab-filters" }.WithClasses("ui3x-tab"))
                            .AlsoAdd((filtersTabListView = new ToggleableListView() { FoldoutTitle = "Declared Type"}).WithClasses("ui3x-list-view").WithEnabled(enableFilterByType))
                            .AlsoAdd((filtersTabListView2 = new ToggleableListView() { FoldoutTitle = "Visibility"}).WithClasses("ui3x-list-view").WithEnabled(enableFilterByVisibility)))
                        .AlsoAdd((new Tab("Assemblies") { name = "ui3x-tab-assemblies" }.WithClasses("ui3x-tab"))
                            .AlsoAdd((assembliesTabListView = new ToggleableListView() { FoldoutTitle = "Assembly Names"}).WithClasses("ui3x-list-view").WithEnabled(enableFilterByAssembly)))
                    )
                );
            
            return (splitter, searchField, toggleSidePanelButton, listView, filtersTabListView, filtersTabListView2, assembliesTabListView);
        }
        
        
    }
}
