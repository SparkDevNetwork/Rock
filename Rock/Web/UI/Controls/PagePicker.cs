//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class PagePicker : CompositeControl, ILabeledControl
    {
        private Label label;
        private Literal literal;
        private HiddenField hfPageId;
        private HiddenField hfInitialPageParentIds;
        private HiddenField hfPageName;
        private LinkButton btnSelect;

        /// <summary>
        /// Gets or sets the label text.
        /// </summary>
        /// <value>
        /// The label text.
        /// </value>
        public string LabelText
        {
            get
            {
                EnsureChildControls();
                return label.Text;
            }

            set
            {
                EnsureChildControls();
                label.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the page id.
        /// </summary>
        /// <value>
        /// The page id.
        /// </value>
        public string PageId
        {
            get
            {
                EnsureChildControls();
                if ( string.IsNullOrWhiteSpace( hfPageId.Value ) )
                {
                    hfPageId.Value = Rock.Constants.None.IdValue;
                }

                return hfPageId.Value;
            }

            set
            {
                EnsureChildControls();
                hfPageId.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets the selected value.
        /// </summary>
        /// <value>
        /// The selected value.
        /// </value>
        public string SelectedValue
        {
            get
            {
                return PageId;
            }

            private set
            {
                PageId = value;
            }
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="page">The page.</param>
        public void SetValue( Rock.Model.Page page )
        {
            if ( page != null )
            {
                PageId = page.Id.ToString();
                
                string parentPageIds = string.Empty;
                var parentPage = page.ParentPage;
                while ( parentPage != null )
                {
                    parentPageIds += parentPage.Id + ",";
                    parentPage = parentPage.ParentPage;
                }

                hfInitialPageParentIds.Value = parentPageIds;
                PageName = page.Name;
            }
            else
            {
                PageId = Rock.Constants.None.IdValue;
                PageName = Rock.Constants.None.TextHtml;
            }
        }

        /// <summary>
        /// Gets or sets the name of the page.
        /// </summary>
        /// <value>
        /// The name of the page.
        /// </value>
        public string PageName
        {
            get
            {
                EnsureChildControls();
                if ( string.IsNullOrWhiteSpace( hfPageName.Value ) )
                {
                    hfPageName.Value = Rock.Constants.None.TextHtml;
                }

                return hfPageName.Value;
            }

            set
            {
                EnsureChildControls();
                hfPageName.Value = value;
            }
        }

        /// <summary>
        /// Occurs when [select page].
        /// </summary>
        public event EventHandler SelectPage;

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            string scriptFormat = @"
        $('a.rock-picker').click(function (e) {{
            e.preventDefault();
            $(this).next('.rock-picker').show();

            debugger 

        }});

        $('#btnCancel_{0}').click(function (e) {{
            $(this).parent().slideUp();
        }});

        $('#btnSelect_{0}').click(function (e) {{
            
            debugger
            var treeViewData = $('#treeviewPages_{0}').data('kendoTreeView');
            var selectedNode = treeViewData.select();
            var nodeData = treeViewData.dataItem(selectedNode);
            var selectedValue = '0';
            var selectedText = '&lt;none&gt;';

            if (nodeData) {{
                selectedValue = nodeData.Id;
                selectedText = nodeData.Name;
            }}

            var selectedPageLabel = $('#selectedPageLabel_{0}');

            var hiddenPageId = $('#hfPageId_{0}');
            var hiddenPageName = $('#hfPageName_{0}');

            hiddenPageId.val(selectedValue);
            hiddenPageName.val(selectedText);

            selectedPageLabel.val(selectedValue);
            selectedPageLabel.text(selectedText);

            $(this).parent().slideUp();
        }});
";

            string script = string.Format( scriptFormat, this.ID);

            ScriptManager.RegisterStartupScript( this, this.GetType(), "page_picker-" + this.ID.ToString(), script, true );

            string treeViewScriptFormat = @"

    function findChildItemInTree(treeViewData, pageId, pageParentIds) {{
        if (pageParentIds != '') {{
            var pageParentList = pageParentIds.split(',');
            for (var i = 0; i < pageParentList.length; i++) {{
                var parentPageId = pageParentList[i];
                var parentItem = treeViewData.dataSource.get(parentPageId);
                var parentNodeItem = treeViewData.findByUid(parentItem.uid);
                if (!parentItem.expanded && parentItem.hasChildren) {{
                    // if not yet expand, expand and return null (which will fetch more data and fire the databound event)
                    treeViewData.expand(parentNodeItem);
                    return null;
                }}
            }}
        }}

        var initialPageItem = treeViewData.dataSource.get(pageId);

        return initialPageItem;
    }}

    function onDataBound(e) {{
        // select the item specified in the page param in the treeview if there isn't one currently selected
        var treeViewData = $('#treeviewPages_{0}').data('kendoTreeView');
        var selectedNode = treeViewData.select();
        var nodeData = this.dataItem(selectedNode);
        if (!nodeData) {{
            var initialPageId = $('#hfInitialPageId_{0}').val();
            var initialPageParentIds = $('#hfInitialPageParentIds_{0}').val();
            var initialPageItem = findChildItemInTree(treeViewData, initialPageId, initialPageParentIds);
            if (initialPageId) {{
                if (initialPageItem) {{
                    var firstItem = treeViewData.findByUid(initialPageItem.uid);
                    var firstDataItem = this.dataItem(firstItem);
                    if (firstDataItem) {{
                        treeViewData.select(firstItem);
                    }}
                }}
            }}
        }}

        $('#treeview-scroll-container_{0}').tinyscrollbar_update('relative');
    }}

    var restUrl = '{1}';

    var pageList = new kendo.data.HierarchicalDataSource({{
        transport: {{
            read: {{
                url: function (options) {{
                    var requestUrl = restUrl + (options.id || 0);
                    return requestUrl;
                }},
                error: function (xhr, status, error) {{
                    {{
                        alert(status + ' [' + error + ']: ' + xhr.responseText);
                    }}
                }}
            }}
        }},
        schema: {{
            model: {{
                id: 'Id',
                hasChildren: 'HasChildren'
            }}
        }}
    }});

    $('#treeviewPages_{0}').kendoTreeView({{
        template: ""<i class='icon-file-alt'></i> #= item.Name #"",
        dataSource: pageList,
        dataTextField: 'Name',
        dataBound: onDataBound
    }});

    $('#treeview-scroll-container_{0}').tinyscrollbar({{ size: 100 }});
";

            string treeViewScript = string.Format( treeViewScriptFormat, this.ID.ToString(), this.ResolveUrl( "~/api/pages/getchildren/" ) );
            
            ScriptManager.RegisterStartupScript( this, this.GetType(), "page_picker-treeviewscript_" + this.ID.ToString(), treeViewScript, true );
            
            var sm = ScriptManager.GetCurrent( this.Page );

            EnsureChildControls();
            sm.RegisterAsyncPostBackControl( btnSelect );
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            Controls.Clear();

            label = new Label();
            literal = new Literal();
            hfPageId = new HiddenField();
            hfPageId.ClientIDMode = System.Web.UI.ClientIDMode.Static;
            hfPageId.ID = string.Format( "hfPageId_{0}", this.ID );
            hfInitialPageParentIds = new HiddenField();
            hfInitialPageParentIds.ClientIDMode = ClientIDMode.Static;
            hfInitialPageParentIds.ID = string.Format( "hfInitialPageParentIds_{0}", this.ID );
            hfPageName = new HiddenField();
            hfPageName.ClientIDMode = System.Web.UI.ClientIDMode.Static;
            hfPageName.ID = string.Format( "hfPageName_{0}", this.ID );

            btnSelect = new LinkButton();
            btnSelect.ClientIDMode = System.Web.UI.ClientIDMode.Static;
            btnSelect.CssClass = "btn btn-mini btn-primary";
            btnSelect.ID = string.Format( "btnSelect_{0}", this.ID );
            btnSelect.Text = "Select Page";
            btnSelect.CausesValidation = false;
            btnSelect.Click += btnSelect_Click;

            Controls.Add( label );
            Controls.Add( literal );
            Controls.Add( hfPageId );
            Controls.Add( hfInitialPageParentIds );
            Controls.Add( hfPageName );
            Controls.Add( btnSelect );
        }

        /// <summary>
        /// Handles the Click event of the btnSelect control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void btnSelect_Click( object sender, EventArgs e )
        {
            if ( SelectPage != null )
            {
                SelectPage( sender, e );
            }
        }

        /// <summary>
        /// Renders the <see cref="T:System.Web.UI.WebControls.TextBox" /> control to the specified <see cref="T:System.Web.UI.HtmlTextWriter" /> object.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> that receives the rendered output.</param>
        protected override void Render( HtmlTextWriter writer )
        {
            string controlHtmlFormatStart = @"
<div class='control-group' id='{0}'>
    <div class='controls'>
        <a class='rock-picker' href='#'>
            <i class='icon-folder-open'></i>
            <span id='selectedPageLabel_{0}'>{1}</span>
            <b class='caret'></b>
        </a>
        <div class='dropdown-menu rock-picker rock-picker-page'>

            <div id='treeview-scroll-container_{0}' class='scroll-container scroll-container-picker'>
                <div class='scrollbar'>
                    <div class='track'>
                        <div class='thumb'>
                            <div class='end'></div>
                        </div>
                    </div>
                </div>
                <div class='viewport'>
                    <div class='overview'>
                        <div id='treeviewPages_{0}' class='tree-view tree-view-pages'></div>
                    </div>
                </div>
            </div>

            <hr />
";
            string controlHtmlFormatEnd = @"
            <a class='btn btn-mini' id='btnCancel_{0}'>Cancel</a>
        </div>
    </div>
</div>
";

            writer.AddAttribute( "class", "control-group" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            label.AddCssClass( "control-label" );

            label.RenderControl( writer );

            writer.AddAttribute( "class", "controls" );

            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            hfPageId.RenderControl( writer );
            hfInitialPageParentIds.RenderControl( writer );
            hfPageName.RenderControl( writer );

            writer.Write( string.Format( controlHtmlFormatStart, this.ID, this.PageName ) );

            // if there is a PostBack registered, create a real LinkButton, otherwise just spit out HTML (to prevent the autopostback)
            if ( SelectPage != null )
            {
                btnSelect.RenderControl( writer );
            }
            else
            {
                writer.Write( string.Format( "<a class='btn btn-mini btn-primary' id='btnSelect_{0}'>Select Page</a>", this.ID ) );
            }

            writer.Write( string.Format( controlHtmlFormatEnd, this.ID, this.PageName ) );

            writer.RenderEndTag();

            writer.RenderEndTag();
        }
    }
}