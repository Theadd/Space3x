//<voffset=0em><line-height=0px><b>RangeDrawer</b> <br><voffset=0em><align="right"><alpha=#7F>(in UnityEditor)<alpha=#FF>
//
//



/*

using Space3x.Attributes.Types.VisualElements;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Space3x.UiToolkit.ListView.QuickSearch.Editor.VisualElements
{
    [UxmlElement]
    public partial class QuickSearchElement : BindableElement
    {
        private SplitterContainer m_Splitter;
        private ToolbarSearchField m_SearchField;
        
        public QuickSearchElement()
        {
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            this.WithClasses("ui3-modal").WithName("ui3-modal");
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            UnregisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            (m_Splitter, m_SearchField) = CreateElementGUID();
            Add(m_Splitter);
        }
    }

    public partial class QuickSearchElement
    {
        [UxmlAttribute]
        public int FixedPaneInitialDimension { get; set; } = 175;
        
        public (SplitterContainer splitter, ToolbarSearchField searchField) CreateElementGUID(int fixedPaneInitialDimension = 175)
        {
            SplitterContainer splitter;
            ToolbarSearchField searchField;
            
            (splitter = new SplitterContainer
            {
                name = "ui3x-splitter",
                fixedPaneInitialDimension = fixedPaneInitialDimension,
                fixedPaneIndex = 1,
                orientation = TwoPaneSplitViewOrientation.Horizontal,
                style = { flexDirection = FlexDirection.Row },
                focusable = true
            }).WithClasses("ui3x-splitter").AlsoAdd(
                new VisualElement() { name = "ui3x-content-panel" }.WithClasses("ui3x-content-panel").AlsoAdd(
                        new VisualElement() { name = "ui3x-panel-header" }
                                .WithClasses("ui3x-panel-header")
                                .AlsoAdd((new Toolbar()).WithClasses("ui3x-toolbar"))
                                .AlsoAdd((searchField = new ToolbarSearchField()).WithClasses("ui3x-search-field")))
                .AlsoAdd(new VisualElement()
                    .WithClasses("ui3-panel-header")
                    .AlsoAdd(new Label("Header")))
                .AlsoAdd(new VisualElement()
                    .WithClasses("ui3-panel-body")
                    .AlsoAdd(new VisualElement()
                        .WithClasses("ui3-grid-view")
                        .AlsoAdd(new GridView()))));
            
            return (splitter, searchField);
        }
        
        
    }
}
//
        //    <ui:VisualElement name="ui3x-modal" class="ui3x-modal">
        //        <Plugins.Space3x.Space3x.Attributes.Types.VisualElements.SplitterContainer focusable="true" fixed-pane-initial-dimension="175" fixed-pane-index="1" orientation="Horizontal" is-fixed-panel-active="true" name="ui3x-splitter" class="ui3x-splitter">
        //            <ui:VisualElement name="ui3x-content-panel" class="ui3x-content-panel">
        //                <ui:VisualElement name="ui3x-panel-header" class="ui3x-panel-header">
        //                    <uie:Toolbar class="ui3x-toolbar">
//                        <uie:ToolbarSearchField name="ui3x-search-field" class="ui3x-search-field" />
//                        <uie:ToolbarSpacer class="ui3x-toolbar-spacer" />
//                        <uie:ToolbarButton text="≡" tabindex="1" tooltip="Side Panel" name="ui3x-toggle-side-panel" class="ui3x-toolbar-button" />
//                    </uie:Toolbar>
//                </ui:VisualElement>
//                <ui:ListView virtualization-method="DynamicHeight" show-alternating-row-backgrounds="ContentOnly" show-foldout-header="false" name="ui3x-list-view" fixed-item-height="26" show-border="false" selection-type="Multiple" tabindex="2" class="ui3x-list-view" />
//            </ui:VisualElement>
//            <ui:VisualElement name="ui3x-side-panel" class="ui3x-side-panel">
//                <ui:TabView name="ui3x-tab-view" class="some-styled-class ui3x-tab-view">
//                    <ui:Tab label="Assemblies" name="ui3x-tab-assemblies" class="ui3x-tab">
//                        <ui:ListView name="ui3x-assemblies-list" class="ui3x-list-view" />
//                    </ui:Tab>
//                    <ui:Tab label="Filters" name="ui3x-tab-filters" class="ui3x-tab">
//                        <Space3x.UiToolkit.ListView.QuickSearch.Editor.VisualElements.GridView name="GridView" class="ui3x-grid-view" />
//                    </ui:Tab>
//                </ui:TabView>
//            </ui:VisualElement>
//        </Plugins.Space3x.Space3x.Attributes.Types.VisualElements.SplitterContainer>
//    </ui:VisualElement>
*/
