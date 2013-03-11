//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class ItemPicker : CompositeControl, ILabeledControl
    {
        private Label label;
        private Literal literal;
        private HiddenField hfItemId;
        private HiddenField hfInitialItemParentIds;
        private HiddenField hfItemName;
        private HiddenField hfItemRestUrlExtraParams;
        private LinkButton btnSelect;
        private LinkButton btnSelectNone;

        /// <summary>
        /// The required validator
        /// </summary>
        protected HiddenFieldValidator requiredValidator;

        public abstract string ItemRestUrl { get; }

        /// <summary>
        /// Gets or sets the item rest URL extra params.
        /// </summary>
        /// <value>
        /// The item rest URL extra params.
        /// </value>
        public string ItemRestUrlExtraParams
        {
            get
            {
                EnsureChildControls();
                return hfItemRestUrlExtraParams.Value;
            }

            set
            {
                EnsureChildControls();
                hfItemRestUrlExtraParams.Value = value;
            }
        }

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
        /// Gets or sets the item id.
        /// </summary>
        /// <value>
        /// The item id.
        /// </value>
        public string ItemId
        {
            get
            {
                EnsureChildControls();
                if ( string.IsNullOrWhiteSpace( hfItemId.Value ) )
                {
                    hfItemId.Value = Rock.Constants.None.IdValue;
                }

                return hfItemId.Value;
            }

            set
            {
                EnsureChildControls();
                hfItemId.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets the initial item parent ids.
        /// </summary>
        /// <value>
        /// The initial item parent ids.
        /// </value>
        public string InitialItemParentIds
        {
            get
            {
                EnsureChildControls();
                if ( string.IsNullOrWhiteSpace( hfInitialItemParentIds.Value ) )
                {
                    hfInitialItemParentIds.Value = Rock.Constants.None.IdValue;
                }

                return hfInitialItemParentIds.Value;
            }

            set
            {
                EnsureChildControls();
                hfInitialItemParentIds.Value = value;
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
                return ItemId;
            }

            private set
            {
                ItemId = value;
            }
        }

        /// <summary>
        /// Gets the selected value as int.
        /// </summary>
        /// <param name="NoneAsNull">if set to <c>true</c> [none as null].</param>
        /// <returns></returns>
        /// <value>
        /// The selected value as int.
        ///   </value>
        public int? SelectedValueAsInt( bool NoneAsNull = true )
        {
            if ( string.IsNullOrWhiteSpace( ItemId ) )
            {
                return null;
            }
            else
            {
                int result = int.Parse( ItemId );
                if ( NoneAsNull )
                {
                    if ( result == Rock.Constants.None.Id )
                    {
                        return null;
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// Gets or sets the name of the item.
        /// </summary>
        /// <value>
        /// The name of the item.
        /// </value>
        public string ItemName
        {
            get
            {
                EnsureChildControls();
                if ( string.IsNullOrWhiteSpace( hfItemName.Value ) )
                {
                    hfItemName.Value = Rock.Constants.None.Text;
                }

                return hfItemName.Value;
            }

            set
            {
                EnsureChildControls();
                hfItemName.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ItemPicker"/> is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if required; otherwise, <c>false</c>.
        /// </value>
        [
        Bindable( true ),
        Category( "Behavior" ),
        DefaultValue( "false" ),
        Description( "Is the value required?" )
        ]
        public bool Required
        {
            get
            {
                if ( ViewState["Required"] != null )
                    return (bool)ViewState["Required"];
                else
                    return false;
            }
            set
            {
                ViewState["Required"] = value;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            string scriptFormat = @"
        $('#{0} a.rock-picker').click(function (e) {{
            e.preventDefault();
            $(this).parent().siblings('.rock-picker').first().toggle();
            updateScrollbar{0}(e);
        }});

        $('#{0} .rock-picker-select').mouseover(function (e) {{
            if ( $('#hfItemId_{0}').val() != '0' ) {{
                $('#btnSelectNone_{0}').stop().show();
            }}
        }});

        $('#{0} .rock-picker-select').mouseout(function (e) {{
            $('#btnSelectNone_{0}').fadeOut(500);
        }});

        $('#btnCancel_{0}').click(function (e) {{
            $(this).parent().slideUp();
        }});

        $('#btnSelectNone_{0}').click(function (e) {{
            $('#btnSelectNone_{0}').stop().hide();
            e.stopImmediatePropagation();

            var selectedValue = '0';
            var selectedText = '<none>';

            var selectedItemLabel = $('#selectedItemLabel_{0}');
            var hiddenItemId = $('#hfItemId_{0}');
            var hiddenItemName = $('#hfItemName_{0}');

            hiddenItemId.val(selectedValue);
            hiddenItemName.val(selectedText);
            selectedItemLabel.val(selectedValue);
            selectedItemLabel.text(selectedText);
        }});

        $('#btnSelect_{0}').click(function (e) {{
            var treeViewData = $('#treeviewItems_{0}').data('kendoTreeView');
            var selectedNode = treeViewData.select();
            var nodeData = treeViewData.dataItem(selectedNode);
            var selectedValue = '0';
            var selectedText = '<none>';

            if (nodeData) {{
                selectedValue = nodeData.Id;
                selectedText = nodeData.Name;
            }}

            var selectedItemLabel = $('#selectedItemLabel_{0}');
            var hiddenItemId = $('#hfItemId_{0}');
            var hiddenItemName = $('#hfItemName_{0}');

            hiddenItemId.val(selectedValue);
            hiddenItemName.val(selectedText);
            selectedItemLabel.val(selectedValue);
            selectedItemLabel.text(selectedText);

            $(this).parent().slideUp();
        }});
";

            string script = string.Format( scriptFormat, this.ID );

            ScriptManager.RegisterStartupScript( this, this.GetType(), "item_picker-" + this.ID.ToString(), script, true );

            string treeViewScriptFormat = @"

    function findChildItemInTree{0}(treeViewData, itemId, itemParentIds) {{
        if (itemParentIds != '') {{
            var itemParentList = itemParentIds.split(',');
            for (var i = 0; i < itemParentList.length; i++) {{
                var parentItemId = itemParentList[i];
                var parentItem = treeViewData.dataSource.get(parentItemId);
                var parentNodeItem = treeViewData.findByUid(parentItem.uid);
                if (!parentItem.expanded && parentItem.hasChildren) {{
                    // if not yet expand, expand and return null (which will fetch more data and fire the databound event)
                    treeViewData.expand(parentNodeItem);
                    return null;
                }}
            }}
        }}

        var initialItemItem = treeViewData.dataSource.get(itemId);

        return initialItemItem;
    }}

    function updateScrollbar{0}(e) {{
        $('#treeview-scroll-container_{0}').tinyscrollbar_update('relative');
        var modalDialog = $('#modal-scroll-container');
        if (modalDialog) {{
            if (modalDialog.is(':visible')) {{
                modalDialog.tinyscrollbar_update('bottom');
            }}
        }}
    }}

    function onDataBound{0}(e) {{
        // select the item specified in the item param in the treeview if there isn't one currently selected
        var treeViewData = $('#treeviewItems_{0}').data('kendoTreeView');
        var selectedNode = treeViewData.select();
        var nodeData = this.dataItem(selectedNode);

        if (!nodeData) {{
            var initialItemId = $('#hfItemId_{0}').val();
            var initialItemParentIds = $('#hfInitialItemParentIds_{0}').val();
            var initialItemItem = findChildItemInTree{0}(treeViewData, initialItemId, initialItemParentIds);
            if (initialItemId) {{
                if (initialItemItem) {{
                    var firstItem = treeViewData.findByUid(initialItemItem.uid);
                    var firstDataItem = this.dataItem(firstItem);
                    if (firstDataItem) {{
                        treeViewData.select(firstItem);
                    }}
                }}
            }}
        }}

        updateScrollbar{0}(e);        
    }}

    var itemList{0} = new kendo.data.HierarchicalDataSource({{
        transport: {{
            read: {{
                url: function (options) {{
                    var extraParams = $('#hfItemRestUrlExtraParams_{0}').val(); 
                    var requestUrl = '{1}' + (options.Id || 0) + '' + extraParams + '';
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

    $('#treeviewItems_{0}').kendoTreeView({{
        template: ""<i class='#= item.IconCssClass #'></i> #= item.Name #"",
        dataSource: itemList{0},
        dataTextField: 'Name',
        dataImageUrlField: 'IconSmallUrl',
        dataBound: onDataBound{0},
        select: updateScrollbar{0}
    }});

    $('#treeview-scroll-container_{0}').tinyscrollbar({{ size: 120 }});
";

            string treeViewScript = string.Format( treeViewScriptFormat, this.ID.ToString(), this.ResolveUrl( ItemRestUrl ) );

            ScriptManager.RegisterStartupScript( this, this.GetType(), "item_picker-treeviewscript_" + this.ID.ToString(), treeViewScript, true );

            var sm = ScriptManager.GetCurrent( this.Page );

            EnsureChildControls();
            sm.RegisterAsyncPostBackControl( btnSelect );
            sm.RegisterAsyncPostBackControl( btnSelectNone );
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsValid
        {
            get
            {
                return !Required || requiredValidator.IsValid;
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSelect control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSelect_Click( object sender, EventArgs e )
        {
            SetValueOnSelect();
            if ( SelectItem != null )
            {
                SelectItem( sender, e );
            }
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="id">The id.</param>
        public void SetValue( int? id )
        {
            ItemId = id.HasValue ? id.Value.ToString() : Rock.Constants.None.IdValue;
            SetValueOnSelect();
        }

        /// <summary>
        /// Sets the value on select.
        /// </summary>
        protected abstract void SetValueOnSelect();

        /// <summary>
        /// Occurs when [select item].
        /// </summary>
        public event EventHandler SelectItem;

        /// <summary>
        /// Shows the error message.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        public void ShowErrorMessage( string errorMessage )
        {
            requiredValidator.ErrorMessage = errorMessage;
            requiredValidator.IsValid = false;
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
            hfItemId = new HiddenField();
            hfItemId.ClientIDMode = ClientIDMode.Static;
            hfItemId.ID = string.Format( "hfItemId_{0}", this.ID );
            hfInitialItemParentIds = new HiddenField();
            hfInitialItemParentIds.ClientIDMode = ClientIDMode.Static;
            hfInitialItemParentIds.ID = string.Format( "hfInitialItemParentIds_{0}", this.ID );
            hfItemName = new HiddenField();
            hfItemName.ClientIDMode = ClientIDMode.Static;
            hfItemName.ID = string.Format( "hfItemName_{0}", this.ID );
            hfItemRestUrlExtraParams = new HiddenField();
            hfItemRestUrlExtraParams.ClientIDMode = ClientIDMode.Static;
            hfItemRestUrlExtraParams.ID = string.Format( "hfItemRestUrlExtraParams_{0}", this.ID );

            btnSelect = new LinkButton();
            btnSelect.ClientIDMode = System.Web.UI.ClientIDMode.Static;
            btnSelect.CssClass = "btn btn-mini btn-primary";
            btnSelect.ID = string.Format( "btnSelect_{0}", this.ID );
            btnSelect.Text = "Select Item";
            btnSelect.CausesValidation = false;
            btnSelect.Click += btnSelect_Click;

            btnSelectNone = new LinkButton();
            btnSelectNone.ClientIDMode = System.Web.UI.ClientIDMode.Static;
            btnSelectNone.CssClass = "rock-picker-select-none";
            btnSelectNone.ID = string.Format( "btnSelectNone_{0}", this.ID );
            btnSelectNone.Text = "<i class='icon-remove'></i>";
            btnSelectNone.CausesValidation = false;
            btnSelectNone.Visible = false;
            btnSelectNone.Click += btnSelect_Click;


            Controls.Add( label );
            Controls.Add( literal );
            Controls.Add( hfItemId );
            Controls.Add( hfInitialItemParentIds );
            Controls.Add( hfItemName );
            Controls.Add( hfItemRestUrlExtraParams );
            Controls.Add( btnSelect );
            Controls.Add( btnSelectNone );

            requiredValidator = new HiddenFieldValidator();
            requiredValidator.ID = this.ID + "_rfv";
            requiredValidator.InitialValue = "0";
            requiredValidator.ControlToValidate = hfItemId.ID;
            requiredValidator.Display = ValidatorDisplay.Dynamic;
            requiredValidator.CssClass = "validation-error";
            requiredValidator.Enabled = false;

            Controls.Add( requiredValidator );
        }

        /// <summary>
        /// Renders the <see cref="T:System.Web.UI.WebControls.TextBox" /> control to the specified <see cref="T:System.Web.UI.HtmlTextWriter" /> object.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> that receives the rendered output.</param>
        protected override void Render( HtmlTextWriter writer )
        {
            writer.AddAttribute( "class", "control-group" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            label.AddCssClass( "control-label" );

            label.RenderControl( writer );

            writer.AddAttribute( "class", "controls" );

            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            if ( Required )
            {
                requiredValidator.Enabled = true;
                requiredValidator.ErrorMessage = LabelText + " is Required.";
                requiredValidator.RenderControl( writer );
            }

            hfItemId.RenderControl( writer );
            hfInitialItemParentIds.RenderControl( writer );
            hfItemName.RenderControl( writer );
            hfItemRestUrlExtraParams.RenderControl( writer );

            if ( this.Enabled )
            {
                string controlHtmlFormatStart = @"
<div class='control-group' id='{0}'>
    <div class='controls'>
        <div class='rock-picker rock-picker-select'> 
            <a class='rock-picker' href='#'>
                <i class='icon-folder-open'></i>
                <span id='selectedItemLabel_{0}'>{1}</span>
                <b class='caret'></b>
            </a>
";
                writer.Write( string.Format( controlHtmlFormatStart, this.ID, this.ItemName ) );

                // if there is a PostBack registered, create a real LinkButton, otherwise just spit out HTML (to prevent the autopostback)
                if ( SelectItem != null )
                {
                    btnSelectNone.RenderControl( writer );
                }
                else
                {
                    writer.Write( string.Format( "<a class='rock-picker-select-none' id='btnSelectNone_{0}' href='#' style='display:none'><i class='icon-remove'></i></a>", this.ID ) );
                }

                string controlHtmlFormatMiddle = @"
        </div>
        <div class='dropdown-menu rock-picker rock-picker-item'>

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
                        <div id='treeviewItems_{0}' class='tree-view tree-view-items'></div>        
                    </div>
                </div>
            </div>

            <hr />
";
                writer.Write( string.Format( controlHtmlFormatMiddle, this.ID, this.ItemName ) );

                // if there is a PostBack registered, create a real LinkButton, otherwise just spit out HTML (to prevent the autopostback)
                if ( SelectItem != null )
                {
                    btnSelect.RenderControl( writer );
                }
                else
                {
                    writer.Write( string.Format( "<a class='btn btn-mini btn-primary' id='btnSelect_{0}'>Select</a>", this.ID ) );
                }

                string controlHtmlFormatEnd = @"
            <a class='btn btn-mini' id='btnCancel_{0}'>Cancel</a>
            
        </div>
    </div>
</div>
";
                writer.Write( string.Format( controlHtmlFormatEnd, this.ID, this.ItemName ) );
            }
            else
            {
                string controlHtmlFormatDisabled = @"
<div class='control-group' id='{0}'>
    <div class='controls'>

        <i class='icon-file-alt'></i>
        <span id='selectedItemLabel_{0}'>{1}</span>

    </div>
</div>
"; 
                writer.Write( string.Format( controlHtmlFormatDisabled, this.ID, this.ItemName ) );
            }

            writer.RenderEndTag();

            writer.RenderEndTag();
        }
    }
}