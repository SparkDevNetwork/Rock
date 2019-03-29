// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using OfficeOpenXml;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Rock Grid Control
    /// </summary>
    [ToolboxData( "<{0}:Grid runat=server></{0}:Grid>" )]
    public class Grid : System.Web.UI.WebControls.GridView, IPostBackEventHandler
    {
        #region Constants

        private const string DEFAULT_EMPTY_DATA_TEXT = "No Results Found";
        private const string PAGE_SIZE_KEY = "grid-page-size-preference";

        #endregion

        #region Fields

        private Table _table;
        private GridViewRow _actionHeaderRow;
        private GridViewRow _actionFooterRow;
        private GridActions _footerGridActions;
        private ControlMirror _headerGridActionsMirror;
        private bool PreDataBound = true;

        private Dictionary<DataControlField, string> _columnDataPriorities;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether [delete enabled].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [delete enabled]; otherwise, <c>false</c>.
        /// </value>
        [
        Category( "Appearance" ),
        DefaultValue( true ),
        Description( "Delete Enabled" )
        ]
        public virtual bool IsDeleteEnabled
        {
            get { return this.ViewState["IsDeleteEnabled"] as bool? ?? true; }
            set { ViewState["IsDeleteEnabled"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether table is responsive.
        /// </summary>
        /// <value>
        ///   <c>true</c> if table is responsive, <c>false</c>.
        /// </value>
        [
        Category( "Appearance" ),
        DefaultValue( true ),
        Description( "Responsive Table Enabled" )
        ]
        public virtual bool EnableResponsiveTable
        {
            get { return this.ViewState["EnableResponsiveTable"] as bool? ?? true; }
            set { ViewState["EnableResponsiveTable"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [enable sticky headers].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable sticky headers]; otherwise, <c>false</c>.
        /// </value>
        public virtual bool EnableStickyHeaders
        {
            get { return this.ViewState["EnableStickyHeaders"] as bool? ?? false; }
            set { ViewState["EnableStickyHeaders"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show confirm delete dialog].
        /// </summary>
        /// <value>
        /// <c>true</c> if [show confirm delete dialog]; otherwise, <c>false</c>.
        /// </value>
        [
        Category( "Appearance" ),
        DefaultValue( true ),
        Description( "Show Confirm Delete Dialog" )
        ]
        public virtual bool ShowConfirmDeleteDialog
        {
            get { return this.ViewState["ShowConfirmDeleteDialog"] as bool? ?? true; }
            set { ViewState["ShowConfirmDeleteDialog"] = value; }
        }

        /// <summary>
        /// Gets or sets the name of the row item.
        /// </summary>
        /// <value>
        /// The name of the row item.
        /// </value>
        [
        Category( "Appearance" ),
        Description( "Item Text" )
        ]
        public string RowItemText
        {
            get
            {
                string rowItemText = this.ViewState["RowItemText"] as string;
                if ( !string.IsNullOrWhiteSpace( rowItemText ) )
                {
                    return rowItemText;
                }

                if ( DataSource != null )
                {
                    Type dataSourceType = DataSource.GetType();

                    Type[] genericArgs = dataSourceType.GetGenericArguments();
                    if ( genericArgs.Length > 0 )
                    {
                        Type itemType = genericArgs[0];
                        if ( itemType != null )
                        {
                            return itemType.GetFriendlyTypeName();
                        }
                    }
                }

                return "Item";
            }

            set
            {
                this.ViewState["RowItemText"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the text to display in the empty data row rendered when a <see cref="T:System.Web.UI.WebControls.GridView" /> control is bound to a data source that does not contain any records.
        /// </summary>
        /// <returns>The text to display in the empty data row. The default is an empty string (""), which indicates that this property is not set.</returns>
        public override string EmptyDataText
        {
            get
            {
                string result = base.EmptyDataText;
                if ( string.IsNullOrWhiteSpace( result ) || result.Equals( DEFAULT_EMPTY_DATA_TEXT ) )
                {
                    result = string.Format( "No {0} Found", RowItemText.Pluralize() );
                }

                return $"<span class='table-empty'>{result}</span>";
            }

            set
            {
                base.EmptyDataText = value;
            }
        }

        /// <summary>
        /// Gets or sets the export source.
        /// </summary>
        /// <value>
        /// The export source.
        /// </value>
        public virtual ExcelExportSource ExportSource
        {
            get
            {
                object exportSource = this.ViewState["ExportSource"];
                return exportSource != null ? (ExcelExportSource)exportSource : ExcelExportSource.DataSource;
            }

            set
            {
                this.ViewState["ExportSource"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the filename to use when exporting the grid contents.
        /// The .xlsx extension will be appended if not given. Special characters are removed
        /// automatically to prevent problems saving the file. Default filename is RockExport.xlsx.
        /// </summary>
        /// <value>
        /// The value of a the export's filename.
        /// </value>
        public string ExportFilename
        {
            get
            {
                string exportFilename = ViewState["ExportFilename"] as string;
                if ( string.IsNullOrWhiteSpace( exportFilename ) )
                {
                    exportFilename = $"{( Page as RockPage )?.PageTitle}.xlsx";
                }
                if ( string.IsNullOrWhiteSpace( exportFilename ) )
                {
                    exportFilename = "RockExport.xlsx";
                }
                else if ( !exportFilename.EndsWith( ".xlsx" ) )
                {
                    exportFilename += ".xlsx";
                }

                return exportFilename.RemoveSpecialCharacters();
            }

            set
            {
                ViewState["ExportFilename"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show the action buttons in the header.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the action buttons will be shown in the header; otherwise, <c>false</c>.
        /// </value>
        public bool ShowActionsInHeader { get; set; } = true;

        /// <summary>
        /// Gets or sets the worksheet and title name on the excel file.
        /// If this property is null then the grid will use it's
        /// caption or the page tile in that order.
        /// </summary>
        /// <value>
        /// The name of the export title.
        /// </value>
        public string ExportTitleName
        {
            get
            {
                return ViewState["ExportTitleName"] as string;
            }
            set
            {
                ViewState["ExportTitleName"] = value;
            }
        }


        /// <summary>
        /// Gets or sets a value indicating whether [hide delete button for is system].
        /// </summary>
        /// <value>
        /// <c>true</c> if [hide delete button for is system]; otherwise, <c>false</c>.
        /// </value>
        [
        Category( "Appearance" ),
        DefaultValue( true ),
        Description( "Hide the Delete button for IsSystem items" )
        ]
        public virtual bool HideDeleteButtonForIsSystem
        {
            get { return this.ViewState["HideDeleteButtonForIsSystem"] as bool? ?? true; }
            set { ViewState["HideDeleteButtonForIsSystem"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [row click enabled].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [row click enabled]; otherwise, <c>false</c>.
        /// </value>
        public virtual bool RowClickEnabled
        {
            get { return this.ViewState["RowClickEnabled"] as bool? ?? true; }
            set { ViewState["RowClickEnabled"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [merge template as person].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [merge template as person]; otherwise, <c>false</c>.
        /// </value>
        public virtual bool MergeTemplateAsPerson
        {
            get { return this.ViewState["MergeTemplateAsPerson"] as bool? ?? false; }
            set { ViewState["MergeTemplateAsPerson"] = value; }
        }

        /// <summary>
        /// Gets or sets the display type.
        /// </summary>
        /// <value>
        /// The display type.
        /// </value>
        [
        Category( "Appearance" ),
        DefaultValue( GridDisplayType.Full ),
        Description( "Display Type" )
        ]
        public virtual GridDisplayType DisplayType
        {
            get
            {
                object displayType = this.ViewState["DisplayType"];
                return displayType != null ? (GridDisplayType)displayType : GridDisplayType.Full;
            }

            set
            {
                this.ViewState["DisplayType"] = value;

                if ( DisplayType == GridDisplayType.Light )
                {
                    this.AllowPaging = false;
                    this.AllowSorting = false;
                    this.Actions.ShowExcelExport = false;
                    this.Actions.ShowMergeTemplate = false;

                    // disable showing buttons in the header of light grids
                    this.ShowActionsInHeader = false;
                }
            }
        }

        /// <summary>
        /// Gets the sort property.
        /// </summary>
        public SortProperty SortProperty
        {
            get { return ViewState["SortProperty"] as SortProperty; }
            set { ViewState["SortProperty"] = value; }
        }

        /// <summary>
        /// Gets or sets a list of datasource field/properties that can optionally be included as additional
        /// merge fields when a new communication is created from the grid.  NOTE: A side affect of using
        /// additional merge fields is that user will not be able to add additional recipients to the
        /// communication after it is created from the grid. If the data element name is different than
        /// the name of the merge field, seperate the two with a pipe (ex: "Data_NickName_3|NickName" )
        /// </summary>
        /// <value>
        /// The communicate merge fields.
        /// </value>
        public List<string> CommunicateMergeFields
        {
            get { return ViewState["CommunicateMergeFields"] as List<string> ?? new List<string>(); }
            set { ViewState["CommunicateMergeFields"] = value; }
        }

        /// <summary>
        /// Gets or sets the Person Id field.
        /// Default is NULL, which indicates that this grid does not reference Person records
        /// </summary>
        /// <value>
        /// The Person Id field.
        /// </value>
        public string PersonIdField
        {
            get
            {
                string personIdField = ViewState["PersonIdField"] as string;
                if ( string.IsNullOrWhiteSpace( personIdField ) )
                {
                    personIdField = null;
                }

                return personIdField;
            }

            set
            {
                ViewState["PersonIdField"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the fields which contain the person ids that should be used when creating a new communication.
        /// </summary>
        /// <value>
        /// The person identifier fields.
        /// </value>
        public List<string> CommunicationRecipientPersonIdFields
        {
            get
            {
                var communicationRecipientPersonIdFields = ViewState["communicationRecipientPersonIdFields"] as List<string>;
                if ( communicationRecipientPersonIdFields == null )
                {
                    communicationRecipientPersonIdFields = new List<string>();
                    ViewState["communicationRecipientPersonIdFields"] = communicationRecipientPersonIdFields;
                }

                return communicationRecipientPersonIdFields;
            }

            set
            {
                ViewState["communicationRecipientPersonIdFields"] = value;
            }
        }

        /// <summary>
        /// The EntityTypeId in cases where the EntityType of the Dataset can't be determined from the DataSource (like DynamicData, .Select( new {..}), or Report Output)
        /// </summary>
        /// <value>
        /// The entity type identifier.
        /// </value>
        public int? EntityTypeId
        {
            get
            {
                return ViewState["EntityTypeId"] as int?;
            }

            set
            {
                ViewState["EntityTypeId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the description field.  If specified, the description will be
        /// added as a tooltip (title) attribute on the row
        /// </summary>
        /// <value>
        /// The description field.
        /// </value>
        public string TooltipField
        {
            get
            {
                string tooltipField = ViewState["TooltipField"] as string;
                if ( string.IsNullOrWhiteSpace( tooltipField ) )
                {
                    tooltipField = null;
                }

                return tooltipField;
            }

            set
            {
                ViewState["TooltipField"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the person merge page route.
        /// </summary>
        /// <value>
        /// The merge page route.
        /// </value>
        public virtual string PersonMergePageRoute
        {
            get { return ViewState["PersonMergePageRoute"] as string ?? "~/PersonMerge/{0}"; }
            set { ViewState["PersonMergePageRoute"] = value; }
        }

        /// <summary>
        /// Gets or sets the bulk update page route.
        /// </summary>
        /// <value>
        /// The bulk update page route.
        /// </value>
        public virtual string BulkUpdatePageRoute
        {
            get { return ViewState["BulkUpdatePageRoute"] as string ?? "~/BulkUpdate/{0}"; }
            set { ViewState["BulkUpdatePageRoute"] = value; }
        }

        /// <summary>
        /// Gets or sets the new communication page route.
        /// </summary>
        /// <value>
        /// The new communication page route.
        /// </value>
        public virtual string CommunicationPageRoute
        {
            get { return ViewState["CommunicationPageRoute"] as string; }
            set { ViewState["CommunicationPageRoute"] = value; }
        }

        /// <summary>
        /// Gets or sets the merge template page route.
        /// </summary>
        /// <value>
        /// The merge template page route.
        /// </value>
        public virtual string MergeTemplatePageRoute
        {
            get { return ViewState["MergeTemplatePageRoute"] as string ?? "~/MergeTemplate/{0}"; }
            set { ViewState["MergeTemplatePageRoute"] = value; }
        }

        /// <summary>
        /// Gets or sets the row selected columns.
        /// </summary>
        /// <value>
        /// The row selected columns.
        /// </value>
        private Dictionary<int, string> RowSelectedColumns
        {
            get
            {
                var rowSelectedColumns = ViewState["RowSelectedColumns"] as Dictionary<int, string>;
                if ( rowSelectedColumns == null )
                {
                    rowSelectedColumns = new Dictionary<int, string>();
                    ViewState["RowSelectedColumns"] = rowSelectedColumns;
                }

                return rowSelectedColumns;
            }

            set
            {
                ViewState["RowSelectedColumns"] = value;
            }
        }

        /// <summary>
        /// Gets the selected keys for the first multiple selection mode SelectField column
        /// </summary>
        /// <value>
        /// The selected keys.
        /// </value>
        public List<object> SelectedKeys
        {
            get
            {
                foreach ( var col in this.Columns.OfType<SelectField>() )
                {
                    if ( col.SelectionMode == SelectionMode.Multiple )
                    {
                        return col.SelectedKeys;
                    }
                }

                return new List<object>();
            }
        }

        /// <summary>
        /// Gets or sets the current page rows.
        /// </summary>
        /// <value>
        /// The current page rows.
        /// </value>
        private int CurrentPageRows
        {
            get { return ViewState["CurrentPageRows"] as int? ?? 0; }
            set { ViewState["CurrentPageRows"] = value; }
        }

        /// <summary>
        /// Gets or sets a dictionary of objects that can be used independently of the actual objects that grid
        /// is bound to. This is helpful when binding to an anonymous type, but an actual known type is needed
        /// during row processing (i.e. RowDataBound events, or GetValue methods of custom grid fields)
        /// </summary>
        /// <value>
        /// The object list
        /// </value>
        public Dictionary<string, object> ObjectList { get; set; }

        #region Action Row Properties

        /// <summary>
        /// Gets the action row ( both the header and footer, which are mirrored )
        /// </summary>
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden ), Browsable( false )]
        public virtual GridViewRow ActionRow
        {
            get
            {
                if ( this._actionFooterRow == null )
                {
                    this.EnsureChildControls();
                }

                return this._actionFooterRow;
            }
        }

        /// <summary>
        /// Gets the actions control ( both the header and footer, which are mirrored )
        /// </summary>
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden ), Browsable( false )]
        public virtual GridActions Actions
        {
            get { return this._footerGridActions; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the action row should be displayed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if action row should be displayed; otherwise, <c>false</c>.
        /// </value>
        [
        Category( "Appearance" ),
        DefaultValue( true ),
        Description( "Show Action Row" )
        ]
        public virtual bool ShowActionRow
        {
            get { return this.ViewState["ShowActionRow"] as bool? ?? true; }
            set { ViewState["ShowActionRow"] = value; }
        }

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Grid"/> class.
        /// </summary>
        public Grid()
        {
            base.AutoGenerateColumns = false;
            base.RowStyle.HorizontalAlign = System.Web.UI.WebControls.HorizontalAlign.Left;
            base.HeaderStyle.HorizontalAlign = System.Web.UI.WebControls.HorizontalAlign.Left;
            base.SelectedRowStyle.HorizontalAlign = System.Web.UI.WebControls.HorizontalAlign.Left;

            this.ShowHeaderWhenEmpty = true;
            this.EmptyDataText = DEFAULT_EMPTY_DATA_TEXT;

            // hack to turn off style="border-collapse: collapse"
            base.GridLines = GridLines.None;
            base.CellSpacing = -1;

            base.AllowPaging = true;

            base.PageSize = 50;
            base.PageIndex = 0;

            _footerGridActions = new GridActions( this );
            _footerGridActions.ID = "footerGridActions";

            _headerGridActionsMirror = new ControlMirror();
            _headerGridActionsMirror.ID = "headerGridActionsMirror";
            _headerGridActionsMirror.ControlToMirror = _footerGridActions;

            // set default DisplayType
            DisplayType = GridDisplayType.Full;
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains event data.</param>
        protected override void OnInit( EventArgs e )
        {
            PagerTemplate pagerTemplate = new PagerTemplate();
            pagerTemplate.NavigateClick += pagerTemplate_NavigateClick;
            pagerTemplate.ItemsPerPageClick += pagerTemplate_ItemsPerPageClick;
            this.PagerTemplate = pagerTemplate;

            this.Sorting += Grid_Sorting;

            this.Actions.PersonMergeClick += Actions_PersonMergeClick;
            this.Actions.BulkUpdateClick += Actions_BulkUpdateClick;
            this.Actions.CommunicateClick += Actions_CommunicateClick;
            this.Actions.ExcelExportClick += Actions_ExcelExportClick;
            this.Actions.MergeTemplateClick += Actions_MergeTemplateClick;

            int pageSize = 50;

            var rockBlock = this.RockBlock();
            if ( rockBlock != null )
            {
                string preferenceKey = string.Format( "{0}_{1}", PAGE_SIZE_KEY, rockBlock.BlockCache?.Id );
                pageSize = rockBlock.GetUserPreference( preferenceKey ).AsInteger();
                if ( pageSize != 50 && pageSize != 500 && pageSize != 5000 )
                {
                    pageSize = 50;
                }
            }

            base.PageSize = pageSize;

            base.OnInit( e );
        }

        /// <summary>
        /// Handles the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( Page.IsPostBack )
            {
                if ( this.DataKeys != null && this.DataKeys.Count > 0 )
                {
                    // For each SelectField evaluate the checkbox/radiobutton to see if the cell was selected.
                    foreach ( var col in this.Columns.OfType<SelectField>() )
                    {
                        var colIndex = this.GetColumnIndex( col ).ToString();

                        col.SelectedKeys = new List<object>();

                        foreach ( GridViewRow row in this.Rows )
                        {
                            CheckBox cb = row.FindControl( "cbSelect_" + colIndex ) as CheckBox;
                            if ( col.SelectionMode == SelectionMode.Multiple )
                            {
                                if ( cb != null && cb.Checked )
                                {
                                    col.SelectedKeys.Add( this.DataKeys[row.RowIndex].Value );
                                }
                            }
                            else
                            {
                                string value = Page.Request.Form[cb.UniqueID.Replace( cb.ID, ( (RockRadioButton)cb ).GroupName )];
                                if ( value == cb.ID )
                                {
                                    col.SelectedKeys.Add( this.DataKeys[row.RowIndex].Value );
                                }
                            }
                        }
                    }
                }
            }

            base.OnLoad( e );
        }

        /// <summary>
        /// Registers the java script.
        /// </summary>
        private void RegisterJavaScript()
        {
            if ( this.ShowConfirmDeleteDialog && this.Enabled && this.IsDeleteEnabled )
            {
                string deleteButtonScript = $@"
$('#{this.ClientID} .grid-delete-button').not('.disabled').on( 'click', function (event) {{
    return Rock.dialogs.confirmDelete(event, '{this.RowItemText}');
}});";
                ScriptManager.RegisterStartupScript( this, this.GetType(), "grid-delete-confirm-script-" + this.ClientID, deleteButtonScript, true );
            }

            string gridSelectCellScript = $@"
$('#{this.ClientID} .grid-select-cell').on( 'click', function (event) {{
    if (!($(event.target).is('a') || $(event.target).parent().is('a'))) {{
        var dataRowIndexValue = $(this).closest('tr').attr('data-row-index');
        var postbackArg = 'RowSelected$' + dataRowIndexValue;
        window.location = ""javascript:__doPostBack('{this.UniqueID}', '"" +  postbackArg + ""')"";
    }}
}});";

            ScriptManager.RegisterStartupScript( this, this.GetType(), "grid-select-cell-script-" + this.ClientID, gridSelectCellScript, true );

            // render script for popovers
            string popoverScript = @"
    $('.grid-table tr').tooltip({html: true, container: 'body', delay: { show: 500, hide: 100 }});
    $('.grid-table tr').click( function(){ $(this).tooltip('hide'); });;
";

            ScriptManager.RegisterStartupScript( this, this.GetType(), "grid-popover", popoverScript, true );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnPreRender( EventArgs e )
        {
            base.OnPreRender( e );

            UseAccessibleHeader = true;

            if ( HeaderRow != null )
            {
                HeaderRow.TableSection = TableRowSection.TableHeader;
            }

            if ( FooterRow != null )
            {
                FooterRow.TableSection = TableRowSection.TableFooter;
            }

            if ( TopPagerRow != null )
            {
                TopPagerRow.TableSection = TableRowSection.TableHeader;
            }

            if ( BottomPagerRow != null )
            {
                BottomPagerRow.TableSection = TableRowSection.TableFooter;
            }

            if ( ActionRow != null )
            {
                ActionRow.TableSection = TableRowSection.TableFooter;
            }
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            var divClasses = new List<string>();
            if ( this.EnableResponsiveTable )
            {
                divClasses.Add( "table-responsive" );
            }

            if ( DisplayType == GridDisplayType.Light )
            {
                divClasses.Add( "table-no-border" );
            }

            if ( divClasses.Any() )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, divClasses.AsDelimited( " " ) );
            }

            writer.RenderBeginTag( HtmlTextWriterTag.Div );


            this.AddCssClass( "grid-table" );
            this.AddCssClass( "table" );

            if ( this.EnableStickyHeaders )
            {
                // javascript hook for sticky headers
                this.AddCssClass( "js-sticky-headers" );

                // styling hook for sticky headers
                this.AddCssClass( "sticky-headers" );
            }

            if ( DisplayType == GridDisplayType.Light )
            {
                this.RemoveCssClass( "table-bordered" );
                this.RemoveCssClass( "table-striped" );
                this.RemoveCssClass( "table-hover" );
                this.AddCssClass( "table-condensed" );
                this.AddCssClass( "table-light" );
            }
            else
            {
                this.RemoveCssClass( "table-condensed" );
                this.RemoveCssClass( "table-light" );
                this.AddCssClass( "table-bordered" );
                this.AddCssClass( "table-striped" );
                this.AddCssClass( "table-hover" );
            }

            base.RenderControl( writer );

            writer.RenderEndTag();
        }

        /// <summary>
        /// TODO: Added this override to prevent the default behavior of rending a grid with a table inside
        /// and div element.  The div may be needed for paging when grid is not used in an update panel
        /// so if wierd errors start happening, this could be the culprit.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> used to render the server control content on the client's browser.</param>
        protected override void Render( HtmlTextWriter writer )
        {
            bool renderPanel = !base.DesignMode;

            if ( this.Page != null )
            {
                this.Page.VerifyRenderingInServerForm( this );
            }

            this.PrepareControlHierarchy();

            RegisterJavaScript();

            this.RenderContents( writer );
        }

        /// <summary>
        /// Creates a new child table.
        /// </summary>
        /// <returns>
        /// Always returns a new <see cref="T:System.Web.UI.WebControls.Table" /> that represents the child table.
        /// </returns>
        protected override Table CreateChildTable()
        {
            _table = base.CreateChildTable();
            return _table;
        }

        /// <summary>
        /// Creates the set of column fields used to build the control hierarchy.
        /// </summary>
        /// <param name="dataSource">A <see cref="T:System.Web.UI.WebControls.PagedDataSource" /> that represents the data source.</param>
        /// <param name="useDataSource">true to use the data source specified by the <paramref name="dataSource" /> parameter; otherwise, false.</param>
        /// <returns>
        /// A <see cref="T:System.Collections.ICollection" /> that contains the fields used to build the control hierarchy.
        /// </returns>
        protected override ICollection CreateColumns( PagedDataSource dataSource, bool useDataSource )
        {
            if ( CustomColumns != null && CustomColumns.Any() )
            {
                var columns = base.CreateColumns( dataSource, useDataSource ).OfType<DataControlField>().ToList();
                foreach ( var columnConfig in CustomColumns )
                {
                    int insertPosition;
                    if ( columnConfig.PositionOffsetType == CustomGridColumnsConfig.ColumnConfig.OffsetType.LastColumn )
                    {
                        insertPosition = columns.Count - columnConfig.PositionOffset;
                    }
                    else
                    {
                        insertPosition = columnConfig.PositionOffset;
                    }

                    var column = columnConfig.GetGridColumn();
                    columns.Insert( insertPosition, column );
                    insertPosition++;
                }

                this.CreatedColumns = columns;
                return this.CreatedColumns as ICollection;
            }
            else
            {
                var defaultResult = base.CreateColumns( dataSource, useDataSource );
                this.CreatedColumns = defaultResult.Cast<DataControlField>().ToList();
                return defaultResult;
            }
        }

        private List<DataControlField> CreatedColumns { get; set; }

        /// <summary>
        /// The Column Index of the specified dataField in a Rock Grid.
        /// Use this instead of Columns.IndexOf (it doesn't return the correct result when grid has custom columns)
        /// </summary>
        /// <param name="dataControlField">The data control field.</param>
        /// <returns></returns>
        public int GetColumnIndex( DataControlField dataControlField )
        {
            // If the grid has custom columns and the columns have been created, get the index of the column from CreatedColumns
            if ( CustomColumns != null && CustomColumns.Any() && this.CreatedColumns != null )
            {
                return this.CreatedColumns.IndexOf( dataControlField );
            }
            else
            {
                return this.Columns.IndexOf( dataControlField );
            }
        }

        /// <summary>
        /// Gets the first grid column that matches the header text.
        /// </summary>
        /// <param name="headerText">The header text.</param>
        /// <returns></returns>
        public DataControlField GetColumnByHeaderText( string headerText )
        {
            // If the grid has custom columns and the columns have been created, get the datacontrolfield from CreatedColumns
            if ( CustomColumns != null && CustomColumns.Any() && this.CreatedColumns != null )
            {
                foreach ( DataControlField column in this.CreatedColumns )
                {
                    if ( column.HeaderText == headerText )
                    {
                        return column;
                    }
                }
            }
            else
            {
                foreach ( DataControlField column in this.Columns )
                {
                    if ( column.HeaderText == headerText )
                    {
                        return column;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets or sets the custom columns from blocks that implement ICustomGridColumns
        /// </summary>
        /// <value>
        /// The custom columns.
        /// </value>
        public List<CustomGridColumnsConfig.ColumnConfig> CustomColumns { get; set; }

        /// <summary>
        /// Creates the control hierarchy used to render the <see cref="T:System.Web.UI.WebControls.GridView" /> control using the specified data source.
        /// </summary>
        /// <param name="dataSource">An <see cref="T:System.Collections.IEnumerable" /> that contains the data source for the <see cref="T:System.Web.UI.WebControls.GridView" /> control.</param>
        /// <param name="dataBinding">true to indicate that the child controls are bound to data; otherwise, false.</param>
        /// <returns>
        /// The number of rows created.
        /// </returns>
        /// <exception cref="T:System.Web.HttpException"><paramref name="dataSource" /> returns a null <see cref="T:System.Web.UI.DataSourceView" />.-or-<paramref name="dataSource" /> does not implement the <see cref="T:System.Collections.ICollection" /> interface and cannot return a <see cref="P:System.Web.UI.DataSourceSelectArguments.TotalRowCount" />. -or-<see cref="P:System.Web.UI.WebControls.GridView.AllowPaging" /> is true and <paramref name="dataSource" /> does not implement the <see cref="T:System.Collections.ICollection" /> interface and cannot perform data source paging.-or-<paramref name="dataSource" /> does not implement the <see cref="T:System.Collections.ICollection" /> interface and <paramref name="dataBinding" /> is set to false.</exception>
        protected override int CreateChildControls( System.Collections.IEnumerable dataSource, bool dataBinding )
        {
            if ( !dataBinding && AllowCustomPaging && PreDataBound && CurrentPageRows < PageSize )
            {
                // When using a LinqDataSource (custom paging) and doing a postback from the last page of a grid that
                // has fewer rows, the default dummy data source used by Asp.Net to rebuild controls does not reflect the
                // correct number of rows. Because we add custom paging and action rows to the end of the table, this results in
                // header/body/footer ordering errors and/or viewstate errors. As a work-around a custom dummy data source
                // is used instead that has the correct number of rows.
                dataSource = new RockDummyDataSource( CurrentPageRows );
            }

            int result = base.CreateChildControls( dataSource, dataBinding );

            if ( _table != null && _table.Parent != null )
            {
                if ( this.AllowPaging && this.BottomPagerRow != null )
                {
                    this.BottomPagerRow.ID = "pagerRow";
                    this.BottomPagerRow.Visible = true;

                    // add paging style
                    if ( this.BottomPagerRow.Cells.Count > 0 )
                    {
                        this.BottomPagerRow.Cells[0].CssClass = "grid-paging";
                    }
                }

                // Footer Action Row
                _actionFooterRow = base.CreateRow( -1, -1, DataControlRowType.Footer, DataControlRowState.Normal );
                _actionFooterRow.ID = "actionFooterRow";
                _table.Rows.Add( _actionFooterRow );

                TableCell actionFooterCell = new TableCell();
                int visibleColumnCount = this.Columns.OfType<DataControlField>().Where( a => a.Visible ).Count() + ( this.CustomColumns?.Count ?? 0);
                actionFooterCell.ColumnSpan = visibleColumnCount;
                actionFooterCell.CssClass = "grid-actions";
                _actionFooterRow.Cells.Add( actionFooterCell );

                actionFooterCell.Controls.Add( _footerGridActions );

                if ( !this.ShowActionRow )
                {
                    _actionFooterRow.Visible = false;
                }

                // Header Action row (mirror of footer actions)
                _actionHeaderRow = base.CreateRow( 0, 0, DataControlRowType.Header, DataControlRowState.Normal );
                _actionHeaderRow.ID = "actionHeaderRow";
                _actionHeaderRow.TableSection = TableRowSection.TableHeader;
                _table.Rows.AddAt( 0, _actionHeaderRow );

                TableCell actionHeaderCell = new TableCell();
                actionHeaderCell.ColumnSpan = visibleColumnCount;
                actionHeaderCell.CssClass = "grid-actions";
                _actionHeaderRow.Cells.Add( actionHeaderCell );

                actionHeaderCell.Controls.Add( _headerGridActionsMirror );

                if ( !this.ShowActionsInHeader || !this.ShowActionRow )
                {
                    _actionHeaderRow.Visible = false;
                    _headerGridActionsMirror.Visible = false;
                }

            }

            return result;
        }

        #endregion

        #region Events

        #region Grid Events

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.DataBinding" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnDataBinding( EventArgs e )
        {
            // Get the css class for any column that does not implement the INotRowSelectedField
            RowSelectedColumns = new Dictionary<int, string>();
            _columnDataPriorities = new Dictionary<DataControlField, string>();

            for ( int i = 0; i < this.Columns.Count; i++ )
            {
                var column = this.Columns[i];
                if ( !( column is INotRowSelectedField ) )
                {
                    if ( !( column is HyperLinkField ) )
                    {
                        if ( column is RockTemplateField )
                        {
                            if ( ( column as RockTemplateField ).OnRowSelectedEnabled )
                            {
                                RowSelectedColumns.Add( i, this.Columns[i].ItemStyle.CssClass );
                            }
                        }
                        else
                        {
                            RowSelectedColumns.Add( i, this.Columns[i].ItemStyle.CssClass );
                        }
                    }
                }

                // get data priority from column
                if ( column is IPriorityColumn )
                {
                    _columnDataPriorities.Add( column, ( (IPriorityColumn)column ).ColumnPriority.ConvertToInt().ToString() );
                }
                else
                {
                    _columnDataPriorities.Add( column, "1" );
                }
            }

            base.OnDataBinding( e );
        }

        /// <summary>
        /// Gets the datasource SQL (if the Datasource is an IQueryable)
        /// </summary>
        /// <value>
        /// The datasource SQL.
        /// </value>
        public string DatasourceSQL { get; private set; }

        /// <summary>
        /// Sets the linq data source
        /// The grid will use it to load only the records it needs based on the current page and page size
        /// NOTE: Make sure that your query is sorted/ordered
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="qry">The qry.</param>
        public void SetLinqDataSource<T>( IQueryable<T> qry )
        {
            if ( this.AllowPaging )
            {
                this.AllowCustomPaging = true;
                var currentPageData = qry.Skip( this.PageIndex * this.PageSize ).Take( this.PageSize ).ToList();
                this.DataSource = currentPageData;
                if ( currentPageData.Count < this.PageSize )
                {
                    // if the current page has fewer records than the page.size, we are on the last page of records, so we can figure out how many records there are without requerying the database
                    this.VirtualItemCount = ( this.PageIndex * this.PageSize ) + currentPageData.Count;
                }
                else
                {
                    this.VirtualItemCount = qry.Count();
                }

                PreDataBound = false;
                CurrentPageRows = currentPageData.Count();
            }
            else
            {
                this.DataSource = qry.ToList();
            }

            DatasourceSQL = qry.ToString();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.WebControls.BaseDataBoundControl.DataBound"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnDataBound( EventArgs e )
        {
            base.OnDataBound( e );

            // Get ItemCount
            int itemCount = 0;
            if ( this.AllowCustomPaging )
            {
                itemCount = this.VirtualItemCount;
            }
            else if ( this.DataSourceAsDataTable != null )
            {
                itemCount = this.DataSourceAsDataTable.DefaultView.Count;
            }
            else if ( this.DataSourceAsList != null )
            {
                itemCount = DataSourceAsList.Count;
            }
            else
            {
                itemCount = 0;
            }

            PagerTemplate pagerTemplate = this.PagerTemplate as PagerTemplate;
            if ( PagerTemplate != null )
            {
                pagerTemplate.SetNavigation( this.PageCount, this.PageIndex, this.PageSize, itemCount, this.RowItemText );
            }
        }

        /// <summary>
        /// Handles the Sorting event of the Grid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.GridViewSortEventArgs"/> instance containing the event data.</param>
        protected void Grid_Sorting( object sender, GridViewSortEventArgs e )
        {
            SortProperty sortProperty = this.SortProperty;
            if ( sortProperty != null && sortProperty.Property == e.SortExpression )
            {
                if ( sortProperty.Direction == SortDirection.Ascending )
                {
                    sortProperty.Direction = SortDirection.Descending;
                }
                else
                {
                    sortProperty.Direction = SortDirection.Ascending;
                }

                this.SortProperty = sortProperty;
            }
            else
            {
                this.SortProperty = new SortProperty( e );
            }

            RebindGrid( e, false, false, false );
        }

        #endregion

        #region Row Events

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.WebControls.GridView.RowCreated" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Web.UI.WebControls.GridViewRowEventArgs" /> that contains event data.</param>
        protected override void OnRowCreated( GridViewRowEventArgs e )
        {
            base.OnRowCreated( e );

            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                // For each select field that is not bound to a DataSelectedField set its checkbox/radiobox from
                // the previously posted back values in the columns SelectedKeys property
                foreach ( var col in this.Columns.OfType<SelectField>() )
                {
                    if ( string.IsNullOrWhiteSpace( col.DataSelectedField ) && col.SelectedKeys.Any() )
                    {
                        var colIndex = this.GetColumnIndex( col ).ToString();
                        CheckBox cbSelect = e.Row.FindControl( "cbSelect_" + colIndex ) as CheckBox;
                        if ( cbSelect != null )
                        {
                            cbSelect.Checked = col.SelectedKeys.Contains( this.DataKeys[e.Row.RowIndex].Value );
                        }
                    }
                }

                if ( this.RowSelected != null )
                {
                    // For each column that supports the clicking to select add the css class to enable this functionality
                    foreach ( var col in RowSelectedColumns )
                    {
                        var cell = e.Row.Cells[col.Key];
                        cell.AddCssClass( col.Value );
                        cell.AddCssClass( "grid-select-cell" );
                    }
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.WebControls.GridView.RowDataBound"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Web.UI.WebControls.GridViewRowEventArgs"/> that contains event data.</param>
        protected override void OnRowDataBound( GridViewRowEventArgs e )
        {
            base.OnRowDataBound( e );

            if ( e.Row.RowType == DataControlRowType.Header )
            {
                //add data priority
                foreach( var cell in e.Row.Cells.OfType<DataControlFieldCell>())
                {
                    if ( _columnDataPriorities.ContainsKey( cell.ContainingField ) )
                    {
                        cell.Attributes.Add( "data-priority", _columnDataPriorities[cell.ContainingField] );
                    }
                }

                if ( this.AllowSorting )
                {

                    string asc = SortDirection.Ascending.ToString();
                    string desc = SortDirection.Descending.ToString();

                    // Remove the sort css classes
                    for ( int i = 0; i < e.Row.Cells.Count; i++ )
                    {
                        var cell = e.Row.Cells[i];
                        cell.RemoveCssClass( asc );
                        cell.RemoveCssClass( desc );
                    }


                    // Add the new sort css class
                    SortProperty sortProperty = this.SortProperty;
                    if ( sortProperty != null )
                    {
                        foreach ( var column in this.Columns )
                        {
                            var dcf = column as DataControlField;
                            if ( dcf != null && dcf.SortExpression == this.SortProperty.Property )
                            {
                                e.Row.Cells[this.GetColumnIndex( dcf )].AddCssClass( sortProperty.Direction.ToString().ToLower() );
                                break;
                            }
                        }
                    }
                }
            }

            if ( e.Row.RowType == DataControlRowType.DataRow || e.Row.RowType == DataControlRowType.Footer )
            {
                // add the data priority
                foreach ( var cell in e.Row.Cells.OfType<DataControlFieldCell>() )
                {
                    if ( _columnDataPriorities.ContainsKey( cell.ContainingField ) )
                    {
                        cell.Attributes.Add( "data-priority", _columnDataPriorities[cell.ContainingField] );
                    }
                }
            }

            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                if ( e.Row.DataItem != null )
                {
                    e.Row.Attributes.Add( "data-row-index", e.Row.RowIndex.ToString() );

                    if ( this.DataKeys != null && this.DataKeys.Count > 0 )
                    {
                        object dataKey = this.DataKeys[e.Row.RowIndex].Value as object;
                        if ( dataKey != null )
                        {
                            string key = dataKey.ToString();
                            e.Row.Attributes.Add( "datakey", key );
                        }
                    }

                    if ( TooltipField != null )
                    {
                        PropertyInfo pi = e.Row.DataItem.GetType().GetProperty( TooltipField );
                        if ( pi != null )
                        {
                            var piv = pi.GetValue( e.Row.DataItem );
                            if ( piv != null )
                            {
                                string description = piv.ToString();
                                if ( !string.IsNullOrWhiteSpace( description ) )
                                {
                                    e.Row.ToolTip = description;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.WebControls.GridView.RowCommand" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Web.UI.WebControls.GridViewCommandEventArgs" /> that contains event data.</param>
        protected override void OnRowCommand( GridViewCommandEventArgs e )
        {
            base.OnRowCommand( e );

            if ( e.CommandName == "RowSelected" )
            {
                int? rowIndex = e.CommandArgument?.ToString().AsIntegerOrNull();
                if ( rowIndex.HasValue )
                {
                    // The rows can have changed (the filter or data has changed) since the UI have been updated. This avoid an exception if there are now less rows than the selected row index.
                    if ( this.Rows.Count > rowIndex.Value )
                    {
                        RowEventArgs a = new RowEventArgs( this.Rows[rowIndex.Value] );
                        OnRowSelected( a );
                    }
                }
            }
        }

        #endregion

        #region Paging Events

        /// <summary>
        /// Handles the ItemsPerPageClick event of the pagerTemplate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.NumericalEventArgs"/> instance containing the event data.</param>
        internal void pagerTemplate_ItemsPerPageClick( object sender, NumericalEventArgs e )
        {
            var rockBlock = this.RockBlock();
            if ( rockBlock != null )
            {
                string preferenceKey = string.Format( "{0}_{1}", PAGE_SIZE_KEY, rockBlock.BlockCache.Id );
                rockBlock.SetUserPreference( preferenceKey, e.Number.ToString() );
            }

            this.PageSize = e.Number;
            this.PageIndex = 0;
            RebindGrid( e, false, false, false );
        }

        /// <summary>
        /// Handles the NavigateClick event of the pagerTemplate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.NumericalEventArgs"/> instance containing the event data.</param>
        public void pagerTemplate_NavigateClick( object sender, NumericalEventArgs e )
        {
            if ( e.Number < 0 )
            {
                this.PageIndex = 0;
            }
            else if ( e.Number > this.PageCount - 1 )
            {
                this.PageIndex = this.PageCount - 1;
            }
            else
            {
                this.PageIndex = e.Number;
            }

            RebindGrid( e, false, false, false );
        }

        #endregion

        #region Action Events

        /// <summary>
        /// Handles the MergeClick event of the Actions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void Actions_PersonMergeClick( object sender, EventArgs e )
        {
            int? entitySetId = GetPersonEntitySet( e );
            if ( entitySetId.HasValue )
            {
                Page.Response.Redirect( string.Format( PersonMergePageRoute, entitySetId.Value ), false );
                Context.ApplicationInstance.CompleteRequest();
            }
            else
            {
                // empty entityset ( probably because the list has 0 items)
                this.ShowModalAlertMessage( "Grid has no " + this.RowItemText.Pluralize(), ModalAlertType.Warning );
            }
        }

        /// <summary>
        /// Handles the MergeClick event of the Actions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Actions_BulkUpdateClick( object sender, EventArgs e )
        {
            int? entitySetId = GetPersonEntitySet( e );
            if ( entitySetId.HasValue )
            {
                Page.Response.Redirect( string.Format( BulkUpdatePageRoute, entitySetId.Value ), false );
                Context.ApplicationInstance.CompleteRequest();
            }
            else
            {
                // empty entityset ( probably because the list has 0 items)
                this.ShowModalAlertMessage( "Grid has no " + this.RowItemText.Pluralize(), ModalAlertType.Warning );
            }
        }

        /// <summary>
        /// Handles the CommunicateClick event of the Actions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void Actions_CommunicateClick( object sender, EventArgs e )
        {
            var rockPage = Page as RockPage;
            if ( rockPage != null )
            {
                // disable paging if no specific keys where selected
                bool selectAll = !SelectedKeys.Any();
                RebindGrid( e, selectAll, false, true );

                // Create a dictionary of the additional merge fields that were created for the communication
                var communicationMergeFields = new Dictionary<string, string>();
                foreach ( string mergeField in this.CommunicateMergeFields )
                {
                    var parts = mergeField.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
                    if ( parts.Any() )
                    {
                        communicationMergeFields.AddOrIgnore( parts.First().Replace( '.', '_' ), parts.Last().Replace('.','_') );
                    }
                }

                // Get the data for the recipients
                var recipients = GetPersonData( true, communicationMergeFields );

                if ( recipients.Any() )
                {
                    // Create communication
                    var communicationRockContext = new RockContext();
                    var communicationService = new Rock.Model.CommunicationService( communicationRockContext );
                    var communication = new Rock.Model.Communication();
                    communication.IsBulkCommunication = true;
                    communication.Status = Model.CommunicationStatus.Transient;

                    // Get a list of the mergefield names
                    List<string> mergeFields = communicationMergeFields.Select( f => f.Value ).Distinct().ToList();

                    if ( CommunicationRecipientPersonIdFields.Any() )
                    {
                        // If the grid has recipient person id fields, there could be multiple values(rows) for each merge fields.
                        // If this is the case save them as 'AdditionalMergeFields'. The communication block will add the neccessary
                        // Lava needed to access the multiple values.
                        communication.AdditionalMergeFields = new List<string>();
                        string mergeFieldList = mergeFields.AsDelimited( "^" );
                        communication.AdditionalMergeFields.Add( $"AdditionalMergeFields|{mergeFieldList}" );
                    }
                    else
                    {
                        // Otherwise just save the name
                        communication.AdditionalMergeFields = mergeFields;
                    }

                    if ( rockPage.CurrentPerson != null )
                    {
                        communication.SenderPersonAliasId = rockPage.CurrentPersonAliasId;
                    }

                    if ( rockPage.Request != null && rockPage.Request.Url != null )
                    {
                        communication.UrlReferrer = rockPage.Request.Url.AbsoluteUri.TrimForMaxLength( communication, "UrlReferrer" );
                    }

                    communicationService.Add( communication );

                    // save communication to get Id
                    communicationRockContext.SaveChanges();

                    var personIds = recipients.Select( r => r.Key ).ToList();
                    var personAliasService = new Rock.Model.PersonAliasService( new Rock.Data.RockContext() );

                    // Get the primary aliases
                    List<Rock.Model.PersonAlias> primaryAliasList = new List<Model.PersonAlias>( personIds.Count );

                    // get the data in chunks just in case we have a large list of PersonIds (to avoid a SQL Expression limit error)
                    var chunkedPersonIds = personIds.Take( 1000 );
                    int skipCount = 0;
                    while ( chunkedPersonIds.Any() )
                    {
                        var chunkedPrimaryAliasList = personAliasService.Queryable()
                            .Where( p => p.PersonId == p.AliasPersonId && chunkedPersonIds.Contains( p.PersonId ) ).AsNoTracking().ToList();
                        primaryAliasList.AddRange( chunkedPrimaryAliasList );
                        skipCount += 1000;
                        chunkedPersonIds = personIds.Skip( skipCount ).Take( 1000 );
                    }

                    // NOTE: Set CreatedDateTime, ModifiedDateTime, etc manually set we are using BulkInsert
                    var currentDateTime = RockDateTime.Now;
                    var currentPersonAliasId = rockPage.CurrentPersonAliasId;

                    var communicationRecipientList = primaryAliasList.Select( a => new Rock.Model.CommunicationRecipient
                    {
                        CommunicationId = communication.Id,
                        PersonAliasId = a.Id,
                        AdditionalMergeValues = recipients[a.PersonId],
                        CreatedByPersonAliasId = currentPersonAliasId,
                        ModifiedByPersonAliasId = currentPersonAliasId,
                        CreatedDateTime = currentDateTime,
                        ModifiedDateTime = currentDateTime
                    } ).ToList();

                    // BulkInsert to quickly insert the CommunicationRecipient records. Note: This is much faster, but will bypass EF and Rock processing.
                    var communicationRecipientRockContext = new RockContext();
                    communicationRecipientRockContext.BulkInsert( communicationRecipientList );

                    // Get the URL to communication page
                    string url = CommunicationPageRoute;
                    if ( string.IsNullOrWhiteSpace( url ) )
                    {
                        var pageRef = rockPage.Site.CommunicationPageReference;
                        if ( pageRef.PageId > 0 )
                        {
                            pageRef.Parameters.AddOrReplace( "CommunicationId", communication.Id.ToString() );
                            url = pageRef.BuildUrl();
                        }
                        else
                        {
                            url = "~/Communication/{0}";
                        }
                    }

                    if ( url.Contains( "{0}" ) )
                    {
                        url = string.Format( url, communication.Id );
                    }

                    Page.Response.Redirect( url, false );
                    Context.ApplicationInstance.CompleteRequest();
                }
                else
                {
                    // nobody in list or nobody selected
                    RebindGrid( e, false, false, false );
                    this.ShowModalAlertMessage( "Grid has no recipients", ModalAlertType.Warning );
                }
            }
        }

        /// <summary>
        /// Handles the MergeTemplateClick event of the Actions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void Actions_MergeTemplateClick( object sender, EventArgs e )
        {
            int? entitySetId = null;
            if ( MergeTemplateAsPerson && PersonIdField.IsNotNullOrWhiteSpace() )
            {
                entitySetId = GetPersonEntitySet( e );
            }
            else
            {
                // disable paging if no specific keys where selected (or if no select option is shown)
                bool selectAll = !SelectedKeys.Any();
                RebindGrid( e, selectAll, true, false );
                entitySetId = GetEntitySetFromGrid( e );
            }

            if ( entitySetId.HasValue )
            {
                Page.Response.Redirect( string.Format( MergeTemplatePageRoute, entitySetId.Value ), false );
                Context.ApplicationInstance.CompleteRequest();
            }
            else
            {
                // empty entityset ( probably because the list has 0 items)
                this.ShowModalAlertMessage( "Grid has no " + this.RowItemText.Pluralize(), ModalAlertType.Warning );
            }
        }

        /// <summary>
        /// Shows the modal alert message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="modalAlertType">Type of the modal alert.</param>
        public void ShowModalAlertMessage( string message, ModalAlertType modalAlertType )
        {
            var modalAlert = new ModalAlert();
            modalAlert.ID = this.ID + "_mdlGridAlert";
            this.Controls.Add( modalAlert );
            modalAlert.Show( message, modalAlertType );
        }

        /// <summary>
        /// Handles the ExcelExportClick event of the Actions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void Actions_ExcelExportClick( object sender, EventArgs e )
        {
            // disable paging if no specific keys where selected (or if no select option is shown)
            bool selectAll = !SelectedKeys.Any();
            RebindGrid( e, selectAll, true, false );

            // create default settings
            string filename = ExportFilename;
            string workSheetName = "Export";
            string title = "Rock Export";

            ExcelPackage excel = new ExcelPackage();

            if ( !string.IsNullOrEmpty( this.ExportTitleName ) )
            {
                // If we have a Export Title Name then use it
                workSheetName = this.ExportTitleName.ReplaceSpecialCharacters( "_" ).TrimEnd( '_' );
                title = this.ExportTitleName;
            }
            else if ( !string.IsNullOrEmpty( this.Caption ) )
            {
                // Then try the caption
                workSheetName = this.Caption.ReplaceSpecialCharacters( "_" ).TrimEnd( '_' );
                title = this.Caption;
            }
            else
            {
                // otherwise use the page title
                var pageTitle = ( Page as RockPage )?.PageTitle;

                if ( !string.IsNullOrEmpty( pageTitle ) )
                {
                    workSheetName = pageTitle.ReplaceSpecialCharacters( "_" ).TrimEnd( '_' );
                    title = pageTitle;
                }
            }

            excel.Workbook.Properties.Title = title;

            // add author info
            Rock.Model.UserLogin userLogin = Rock.Model.UserLoginService.GetCurrentUser();
            if ( userLogin != null )
            {
                excel.Workbook.Properties.Author = userLogin.Person.FullName;
            }
            else
            {
                excel.Workbook.Properties.Author = "Rock";
            }

            // add the page that created this
            excel.Workbook.Properties.SetCustomPropertyValue( "Source", this.Page.Request.Url.OriginalString );

            ExcelWorksheet worksheet = excel.Workbook.Worksheets.Add( workSheetName );

            //// write data to worksheet there are three supported data sources
            //// DataTables, DataViews and ILists

            var headerRows = 3;
            int rowCounter = headerRows;
            int columnCounter = 1;

            if ( this.ExportSource == ExcelExportSource.ColumnOutput )
            {
                // Columns to export with their column index as the key
                var gridColumns = new Dictionary<int, DataControlField>();
                for ( int i = 0; i < this.CreatedColumns.Count; i++ )
                {
                    var dataField = this.CreatedColumns[i];
                    var rockField = this.CreatedColumns[i] as IRockGridField;
                    if ( rockField != null &&
                        (
                            rockField.ExcelExportBehavior == ExcelExportBehavior.AlwaysInclude ||
                            ( rockField.ExcelExportBehavior == ExcelExportBehavior.IncludeIfVisible && rockField.Visible )
                        ) )
                    {
                        gridColumns.Add( i, dataField );
                    }
                }

                columnCounter = 1;
                foreach ( var col in gridColumns )
                {
                    worksheet.Cells[rowCounter, columnCounter].Value = col.Value.HeaderText;
                    columnCounter++;
                }

                var dataItems = this.DataSourceAsList;
                var gridViewRows = this.Rows.OfType<GridViewRow>().ToList();
                if ( gridViewRows.Count != dataItems.Count )
                {
                    return;
                }

                var selectedKeys = SelectedKeys.ToList();
                for ( int i = 0; i < dataItems.Count; i++ )
                {
                    rowCounter++;
                    var dataItem = dataItems[i];
                    var gridViewRow = gridViewRows[i];

                    if ( selectedKeys.Any() && this.DataKeyNames.Count() == 1 )
                    {
                        var dataKeyValue = dataItem.GetPropertyValue( this.DataKeyNames[0] );
                        if ( !selectedKeys.Contains( dataKeyValue ) )
                        {
                            // if there are specific rows selected, skip over rows that aren't selected
                            continue;
                        }
                    }

                    var args = new RockGridViewRowEventArgs( gridViewRow, true );
                    gridViewRow.DataItem = dataItem;
                    this.OnRowDataBound( args );
                    columnCounter = 0;
                    var gridViewRowCellLookup = gridViewRow.Cells.OfType<DataControlFieldCell>().ToDictionary( k => k.ContainingField, v => v );
                    foreach ( var col in gridColumns )
                    {
                        columnCounter++;

                        object exportValue = null;
                        if ( col.Value is RockBoundField )
                        {
                            exportValue = ( col.Value as RockBoundField ).GetExportValue( gridViewRow );
                        }
                        else if ( col.Value is RockTemplateField )
                        {
                            var fieldCell = gridViewRowCellLookup[col.Value];
                            exportValue = ( col.Value as RockTemplateField ).GetExportValue( gridViewRow, fieldCell );
                        }

                        ExcelHelper.SetExcelValue( worksheet.Cells[rowCounter, columnCounter], exportValue );

                        // Set the initial column format when processing the first row of data
                        // This is done here because a value is needed to determine the data types
                        if ( rowCounter == headerRows + 1 )
                        {
                            worksheet.Column( columnCounter ).Style.Numberformat.Format = ExcelHelper.DefaultColumnFormat( col.Value as IRockGridField, exportValue );
                        }
                    }
                }
            }
            else if ( this.DataSourceAsDataTable != null )
            {
                var gridDataFields = this.Columns.OfType<BoundField>().ToList();
                DataTable data = this.DataSourceAsDataTable;
                columnCounter = 0;

                var encryptedColumns = new List<int>();

                // Set up the columns
                foreach ( DataColumn column in data.Columns )
                {
                    columnCounter++;

                    // Print column headings
                    var gridField = gridDataFields.FirstOrDefault( a => a.DataField == column.ColumnName );
                    worksheet.Cells[rowCounter, columnCounter].Value = gridField != null ? gridField.HeaderText : column.ColumnName.SplitCase();

                    // Set the initial column format
                    worksheet.Column( columnCounter ).Style.Numberformat.Format = ExcelHelper.DefaultColumnFormat( column.DataType );

                    // Check to see if this is an encrypted column
                    if ( gridField is EncryptedField )
                    {
                        encryptedColumns.Add( columnCounter - 1 );
                    }
                }

                // print data
                int gridRowCounter = 0;
                var selectedKeys = SelectedKeys.ToList();
                foreach ( DataRowView rowView in data.DefaultView )
                {
                    if ( selectedKeys.Any() && this.DataKeyNames.Count() == 1 )
                    {
                        if ( gridRowCounter == this.Rows.Count )
                        {
                            // Stop when the counter reaches the number of rows displayed in the grid.
                            break;
                        }

                        var dataKeyValue = this.DataKeys[gridRowCounter].Value;
                        gridRowCounter++;

                        if ( !selectedKeys.Contains( dataKeyValue ) )
                        {
                            // if there are specific rows selected, skip over rows that aren't selected
                            continue;
                        }
                    }

                    rowCounter++;

                    for ( int i = 0; i < data.Columns.Count; i++ )
                    {
                        var value = encryptedColumns.Contains( i ) ? Security.Encryption.DecryptString( rowView.Row[i].ToString() ) : rowView.Row[i];
                        value = value.ReverseCurrencyFormatting();

                        int columnIndex = i + 1;
                        ExcelHelper.SetExcelValue( worksheet.Cells[rowCounter, columnIndex], value );

                        // Update column formatting based on data
                        ExcelHelper.FinalizeColumnFormat( worksheet, columnIndex, value );
                    }
                }
            }
            else
            {
                var definedValueFields = this.Columns.OfType<DefinedValueField>().ToList();
                Dictionary<PropertyInfo, bool> propIsDefinedValueLookup = new Dictionary<PropertyInfo, bool>();
                Dictionary<BoundField, PropertyInfo> boundFieldPropLookup = new Dictionary<BoundField, PropertyInfo>();
                var attributeFields = this.Columns.OfType<AttributeField>().ToList();
                var lavaFields = new List<LavaField>();
                var rockTemplateFields = new List<RockTemplateField>();
                var visibleFields = new Dictionary<int, DataControlField>();

                int fieldOrder = 0;
                foreach ( DataControlField dataField in this.Columns )
                {
                    if ( dataField is LavaField )
                    {
                        var lavaField = dataField as LavaField;
                        lavaFields.Add( lavaField );
                        visibleFields.Add( fieldOrder++, lavaField );
                    }

                    if ( dataField is BoundField )
                    {
                        var boundField = dataField as BoundField;
                        visibleFields.Add( fieldOrder++, boundField );
                    }

                    if ( dataField is RockTemplateField )
                    {
                        var rockTemplateField = dataField as RockTemplateField;
                        rockTemplateFields.Add( rockTemplateField );
                        if ( rockTemplateField.ExcelExportBehavior == ExcelExportBehavior.AlwaysInclude || ( rockTemplateField.Visible == true && rockTemplateField.ExcelExportBehavior == ExcelExportBehavior.IncludeIfVisible ) )
                        {
                            visibleFields.Add( fieldOrder++, rockTemplateField );
                        }
                    }
                }

                

                if ( CustomColumns != null && CustomColumns.Any() )
                {
                    foreach ( var columnConfig in CustomColumns )
                    {
                        var column = columnConfig.GetGridColumn();
                        lavaFields.Add( column );
                        visibleFields.Add( fieldOrder++, column );
                    }
                }



                if ( CustomColumns != null && CustomColumns.Any() )
                {
                    foreach ( var columnConfig in CustomColumns )
                    {
                        var column = columnConfig.GetGridColumn();
                        lavaFields.Add( column );
                        visibleFields.Add( fieldOrder++, column );
                    }
                }

                var oType = GetDataSourceObjectType();

                // get all properties of the objects in the grid
                List<PropertyInfo> allprops = new List<PropertyInfo>( oType.GetProperties() );

                // If this is a DotLiquid.Drop class, don't include any of the properties that are inherited from DotLiquid.Drop
                if ( typeof( DotLiquid.Drop ).IsAssignableFrom( oType ) )
                {
                    var dropProperties = typeof( DotLiquid.Drop ).GetProperties().Select( a => a.Name );
                    allprops = allprops.Where( a => !dropProperties.Contains( a.Name ) ).ToList();
                }

                // Inspect the collection of Fields that appear in the Grid and add the corresponding data item properties to the set of fields to be exported.
                // The fields are exported in the same order as they appear in the Grid.
                var props = new List<PropertyInfo>();
                foreach ( PropertyInfo prop in allprops )
                {
                    // skip over virtual properties that aren't shown in the grid since they are probably lazy loaded and it is too late to get them
                    var getMethod = prop.GetGetMethod();
                    if ( getMethod == null || ( getMethod.IsVirtual && !getMethod.IsFinal && prop.GetCustomAttributes( typeof( Rock.Data.PreviewableAttribute ) ).Count() == 0 ) )
                    {
                        continue;
                    }

                    // Skip the lava property (is added through columns)
                    if ( prop.Name.StartsWith( "Data_Lava_" ) )
                    {
                        continue;
                    }

                    props.Add( prop );
                }

                var lavaDataFields = new Dictionary<string, LavaFieldTemplate.DataFieldInfo>();

                // Grid column headings
                var boundPropNames = new List<string>();

                // Array provides slight performance improvement here over a list
                var orderedVisibleFields = visibleFields.OrderBy( f => f.Key ).Select( f => f.Value ).ToArray();
                for (int i = 0; i < orderedVisibleFields.Count(); i++ )
                {
                    DataControlField dataField = orderedVisibleFields[i];
                    if (dataField.HeaderText.IsNullOrWhiteSpace())
                    {
                        dataField.HeaderText = string.Format( "Column {0}", i );
                    }
                    else
                    {
                        worksheet.Cells[rowCounter, columnCounter].Value = dataField.HeaderText;
                    }

                    var boundField = dataField as BoundField;
                    if ( boundField != null )
                    {
                        var prop = GetPropertyFromBoundField( props, boundFieldPropLookup, boundField );
                        if ( prop != null )
                        {
                            if ( lavaFields.Any() )
                            {
                                var mergeFieldName = boundField.HeaderText.Replace( " ", string.Empty ).RemoveSpecialCharacters();
                                lavaDataFields.AddOrIgnore( mergeFieldName, new LavaFieldTemplate.DataFieldInfo { PropertyInfo = prop, GridField = boundField } );
                            }

                            boundPropNames.Add( prop.Name );

                            // Set the initial column format
                            worksheet.Column( columnCounter ).Style.Numberformat.Format = ExcelHelper.DefaultColumnFormat( prop.PropertyType );
                        }
                    }

                    columnCounter++;
                }

                // headings for data not associated with a bound field
                foreach ( var prop in props.Where( p => !boundPropNames.Contains( p.Name ) ) )
                {
                    if ( lavaFields.Any() )
                    {
                        var mergeFieldName = prop.Name;
                        lavaDataFields.AddOrIgnore( mergeFieldName, new LavaFieldTemplate.DataFieldInfo { PropertyInfo = prop, GridField = null } );
                    }

                    worksheet.Cells[rowCounter, columnCounter].Value = prop.Name.SplitCase();
                    worksheet.Column( columnCounter ).Style.Numberformat.Format = ExcelHelper.DefaultColumnFormat( prop.PropertyType );

                    columnCounter++;
                }

                string appRoot = ( (RockPage)Page ).ResolveRockUrl( "~/" );
                string themeRoot = ( (RockPage)Page ).ResolveRockUrl( "~~/" );

                // print data
                int dataIndex = 0;

                IList data = this.DataSourceAsList;

                var selectedKeys = SelectedKeys.ToList();
                foreach ( var item in data )
                {
                    if ( selectedKeys.Any() && this.DataKeyNames.Count() == 1 )
                    {
                        var dataKeyValue = item.GetPropertyValue( this.DataKeyNames[0] );
                        if ( !selectedKeys.Contains( dataKeyValue ) )
                        {
                            // if there are specific rows selected, skip over rows that aren't selected
                            dataIndex++;
                            continue;
                        }
                    }

                    IHasAttributes dataItemWithAttributes = null;
                    if ( attributeFields.Any() )
                    {
                        // First check to see if there is an object list
                        if ( ObjectList != null )
                        {
                            // If an object list exists, check to see if the associated object has attributes
                            string key = DataKeys[dataIndex].Value.ToString();
                            if ( !string.IsNullOrWhiteSpace( key ) && ObjectList.ContainsKey( key ) )
                            {
                                dataItemWithAttributes = ObjectList[key] as IHasAttributes;
                            }
                        }

                        // Then check if DataItem has attributes
                        if ( dataItemWithAttributes == null )
                        {
                            dataItemWithAttributes = item as IHasAttributes;
                        }

                        if ( dataItemWithAttributes != null )
                        {
                            if ( dataItemWithAttributes.Attributes == null )
                            {
                                dataItemWithAttributes.LoadAttributes();
                            }
                        }
                    }

                    columnCounter = 0;
                    rowCounter++;

                    foreach ( var dataField in visibleFields.OrderBy( f => f.Key ).Select( f => f.Value ) )
                    {
                        columnCounter++;

                        var attributeField = dataField as AttributeField;
                        if ( attributeField != null )
                        {
                            bool exists = dataItemWithAttributes.Attributes.ContainsKey( attributeField.DataField );
                            if ( exists )
                            {
                                var attrib = dataItemWithAttributes.Attributes[attributeField.DataField];
                                string rawValue = dataItemWithAttributes.GetAttributeValue( attributeField.DataField );
                                string resultHtml = attrib.FieldType.Field.FormatValue( null, attrib.EntityTypeId, dataItemWithAttributes.Id, rawValue, attrib.QualifierValues, false ).ReverseCurrencyFormatting().ToString();
                                if ( !string.IsNullOrEmpty( resultHtml ) )
                                {
                                    worksheet.Cells[rowCounter, columnCounter].Value = resultHtml;

                                    // Update column formatting based on data
                                    ExcelHelper.FinalizeColumnFormat( worksheet, columnCounter, resultHtml );
                                }
                            }
                            continue;
                        }

                        var boundField = dataField as BoundField;
                        if ( boundField != null )
                        {
                            var cell = worksheet.Cells[rowCounter, columnCounter];
                            var prop = GetPropertyFromBoundField( props, boundFieldPropLookup, boundField );
                            object exportValue = null;
                            if ( prop != null )
                            {
                                object propValue = prop.GetValue( item, null );

                                if ( dataField is CallbackField )
                                {
                                    propValue = ( dataField as CallbackField ).GetFormattedDataValue( propValue );
                                }

                                if ( dataField is LavaBoundField )
                                {
                                    propValue = ( dataField as LavaBoundField ).GetFormattedDataValue( propValue );
                                }

                                if ( dataField is HtmlField )
                                {
                                    propValue = ( dataField as HtmlField ).FormatDataValue( propValue );
                                }

                                if ( propValue != null )
                                {
                                    exportValue = GetExportValue( prop, propValue, IsDefinedValue( definedValueFields, propIsDefinedValueLookup, prop ), cell ).ReverseCurrencyFormatting();
                                }
                            }
                            else if ( boundField.DataField?.Contains(".") == true )
                            {
                                exportValue = item.GetPropertyValue( boundField.DataField );
                            }

                            if ( exportValue != null )
                            {
                                ExcelHelper.SetExcelValue( cell, exportValue );

                                // Update column formatting based on data
                                ExcelHelper.FinalizeColumnFormat( worksheet, columnCounter, exportValue );
                            }

                            continue;
                        }

                        var lavaField = dataField as LavaField;
                        if ( lavaField != null )
                        {
                            var mergeValues = new Dictionary<string, object>();
                            foreach ( var dataFieldItem in lavaDataFields )
                            {
                                var dataFieldValue = dataFieldItem.Value.PropertyInfo.GetValue( item, null );
                                if ( dataFieldItem.Value.GridField is DefinedValueField )
                                {
                                    var definedValue = ( dataFieldItem.Value.GridField as DefinedValueField ).GetDefinedValue( dataFieldValue );
                                    dataFieldValue = definedValue != null ? definedValue.Value : null;
                                }
                                mergeValues.Add( dataFieldItem.Key, dataFieldValue );
                            }
                            mergeValues.Add( "Row", item );

                            string resolvedValue = lavaField.LavaTemplate.ResolveMergeFields( mergeValues );
                            resolvedValue = resolvedValue.Replace( "~~/", themeRoot ).Replace( "~/", appRoot ).ReverseCurrencyFormatting().ToString();

                            if ( !string.IsNullOrEmpty( resolvedValue ) )
                            {
                                worksheet.Cells[rowCounter, columnCounter].Value = resolvedValue;

                                // Update column formatting based on data
                                ExcelHelper.FinalizeColumnFormat( worksheet, columnCounter, resolvedValue );
                            }

                            continue;
                        }

                        var rockTemplateField = dataField as RockTemplateField;
                        if ( rockTemplateField != null )
                        {
                            var row = this.Rows[dataIndex];
                            var cell = row?.Cells.OfType<DataControlFieldCell>().Where( a => a.ContainingField == rockTemplateField ).FirstOrDefault();
                            if ( cell != null )
                            {
                                var exportValue = rockTemplateField.GetExportValue( row, cell );

                                if ( exportValue != null )
                                {
                                    worksheet.Cells[rowCounter, columnCounter].Value = exportValue;

                                    // Update column formatting based on data
                                    ExcelHelper.FinalizeColumnFormat( worksheet, columnCounter, exportValue );
                                }

                                continue;
                            }
                        }
                    }

                    foreach ( var prop in props.Where( p => !boundPropNames.Contains( p.Name ) ) )
                    {
                        columnCounter++;
                        object propValue = prop.GetValue( item, null );
                        if ( propValue != null )
                        {
                            var cell = worksheet.Cells[rowCounter, columnCounter];
                            var exportValue = GetExportValue( prop, propValue, IsDefinedValue( definedValueFields, propIsDefinedValueLookup, prop ), cell ).ReverseCurrencyFormatting();
                            ExcelHelper.SetExcelValue( cell, exportValue );

                            // Update column formatting based on data
                            ExcelHelper.FinalizeColumnFormat( worksheet, columnCounter, exportValue );
                        }
                    }

                    dataIndex++;
                }
            }

            worksheet.FormatWorksheet( title, headerRows, rowCounter, columnCounter );

            // send the spreadsheet to the browser
            excel.SendToBrowser( this.Page, filename );
        }

        /// <summary>
        /// Determines whether [is defined value] [the specified defined value fields].
        /// </summary>
        /// <param name="definedValueFields">The defined value fields.</param>
        /// <param name="propIsDefinedValueLookup">The property is defined value lookup.</param>
        /// <param name="prop">The property.</param>
        /// <returns>
        ///   <c>true</c> if [is defined value] [the specified defined value fields]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsDefinedValue( List<DefinedValueField> definedValueFields, Dictionary<PropertyInfo, bool> propIsDefinedValueLookup, PropertyInfo prop )
        {
            if ( !propIsDefinedValueLookup.ContainsKey( prop ) )
            {
                var definedValueAttribute = prop.GetCustomAttributes( typeof( DefinedValueAttribute ), true ).FirstOrDefault();
                bool isDefinedValue = ( definedValueAttribute != null || definedValueFields.Any( f => f.DataField == prop.Name ) );

                propIsDefinedValueLookup.Add( prop, isDefinedValue );
            }

            return propIsDefinedValueLookup[prop];
        }

        /// <summary>
        /// Gets the property from bound field.
        /// </summary>
        /// <param name="props">The props.</param>
        /// <param name="boundFieldPropLookup">The bound field property lookup.</param>
        /// <param name="boundField">The bound field.</param>
        /// <returns></returns>
        private static PropertyInfo GetPropertyFromBoundField( List<PropertyInfo> props, Dictionary<BoundField, PropertyInfo> boundFieldPropLookup, BoundField boundField )
        {
            if ( !boundFieldPropLookup.ContainsKey( boundField ) )
            {
                var prop = props.FirstOrDefault( p => boundField.DataField == p.Name || boundField.DataField.StartsWith( p.Name + "." ) );
                boundFieldPropLookup.Add( boundField, prop );
            }

            return boundFieldPropLookup[boundField];
        }

        /// <summary>
        /// Calls OnGridRebind with an option to disable paging so the entire datasource is loaded vs just what is needed for the current page
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <param name="disablePaging">if set to <c>true</c> [disable paging].</param>
        /// <param name="isExporting">if set to <c>true</c> [is exporting].</param>
        private void RebindGrid( EventArgs e, bool disablePaging, bool isExporting = false )
        {
            RebindGrid( e, disablePaging, isExporting, false );
        }

        /// <summary>
        /// Calls OnGridRebind with an option to disable paging so the entire datasource is loaded vs just what is needed for the current page
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <param name="disablePaging">if set to <c>true</c> [disable paging].</param>
        /// <param name="isExporting">if set to <c>true</c> [is exporting].</param>
        /// <param name="isCommunication">if set to <c>true</c> [is communication].</param>
        private void RebindGrid( EventArgs e, bool disablePaging, bool isExporting, bool isCommunication )
        {
            var origPaging = this.AllowPaging;
            if ( disablePaging )
            {
                this.AllowPaging = false;
            }

            var eventArg = new GridRebindEventArgs( isExporting, isCommunication );
            OnGridRebind( eventArg );

            this.AllowPaging = origPaging;
        }

        /// <summary>
        /// Formats a raw value from the Grid DataSource so that it is suitable for export to an Excel Worksheet.
        /// </summary>
        /// <param name="prop">The property.</param>
        /// <param name="propValue">The property value.</param>
        /// <param name="isDefinedValueField">if set to <c>true</c> [is defined value field].</param>
        /// <param name="cell">The cell.</param>
        /// <returns></returns>
        private object GetExportValue( PropertyInfo prop, object propValue, bool isDefinedValueField, ExcelRange cell )
        {
            if ( propValue != null )
            {
                if ( isDefinedValueField )
                {
                    if ( prop.PropertyType == typeof( int? ) || prop.PropertyType == typeof( int ) )
                    {
                        // Attempt to parse the value as a single Defined Value Id.
                        int definedValueId;

                        if ( prop.PropertyType == typeof( int ) )
                        {
                            definedValueId = (int)propValue;
                        }
                        else
                        {
                            definedValueId = (int?)propValue ?? 0;
                        }

                        if ( definedValueId > 0 )
                        {
                            var definedValue = DefinedValueCache.Get( definedValueId );
                            if ( definedValue != null )
                            {
                                return definedValue.Value;
                            }

                            return definedValueId;
                        }
                    }
                    else if ( prop.PropertyType == typeof( string ) )
                    {
                        // Attempt to parse the value as a list of Defined Value Guids.
                        // If a value is not a Guid or cannot be matched to a Defined Value, the raw value will be shown.
                        var guids = propValue.ToString().Split( ',' );
                        var definedValues = new List<string>();

                        foreach ( var guidString in guids )
                        {
                            Guid definedValueGuid;

                            bool isGuid = Guid.TryParse( guidString, out definedValueGuid );
                            bool addRaw = true;

                            if ( isGuid )
                            {
                                var definedValue = DefinedValueCache.Get( definedValueGuid );

                                if ( definedValue != null )
                                {
                                    definedValues.Add( definedValue.Value );
                                    addRaw = false;
                                }
                            }

                            if ( addRaw )
                            {
                                definedValues.Add( guidString );
                            }
                        }

                        return definedValues.AsDelimited( ", " );
                    }
                }
                else if ( propValue is IEnumerable<object> )
                {
                    return ( propValue as IEnumerable<object> ).ToList().AsDelimited( ", " );
                }
                else
                {
                    // Is the value a single link field? (such as a PersonLinkSelect field)
                    if ( propValue.ToString().Split( new string[] { "<a href=" }, StringSplitOptions.None ).Length - 1 == 1 )
                    {
                        try
                        {
                            var aNode = HtmlAgilityPack.HtmlNode.CreateNode( propValue.ToString() );
                            if ( aNode != null && aNode.NodeType != HtmlAgilityPack.HtmlNodeType.Element )
                            {
                                aNode = aNode.NextSibling;
                            }

                            // Select the hyperlink tag
                            if ( aNode.Attributes["href"] != null )
                            {
                                string url = string.Format( "{0}{1}", this.RockBlock().RootPath, aNode.Attributes["href"].Value );
                                cell.Hyperlink = new ExcelHyperLink( url ) { Display = aNode.InnerText, ToolTip = url };
                                return aNode.InnerText;
                            }
                        }
                        catch
                        {
                            // ignore
                        }
                    }
                }
            }

            return propValue;
        }

        /// <summary>
        /// Gets the Type of the data source entity when the DataSource is a List (not a DataTable)
        /// </summary>
        /// <returns></returns>
        private Type GetDataSourceObjectType()
        {
            if ( this.DataSourceAsDataTable != null )
            {
                return null;
            }
            else
            {
                var data = this.DataSourceAsList;

                Type oType = data.GetType().GetProperty( "Item" ).PropertyType;

                // if the list is just List<object>, try to find out what the properties of specific type of object are by examining the first item in the list
                if ( oType == typeof( object ) || oType.IsInterface )
                {
                    if ( data.Count > 0 )
                    {
                        oType = data[0].GetType();
                    }
                }

                return oType;
            }
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Creates grid columns by reflecting on the properties of a type.  If any of the properties
        /// have the [Previewable] attribute, columns will only be created for those properties
        /// </summary>
        /// <param name="modelType">Type of the model.</param>
        public void CreatePreviewColumns( Type modelType )
        {
            // if there is a selectField, keep it to preserve which items are checked
            var selectField = this.Columns.OfType<SelectField>().FirstOrDefault();
            this.Columns.Clear();

            var previewColumns = GetPreviewColumns( modelType );
            foreach ( var column in previewColumns )
            {
                if ( column is SelectField )
                {
                    // if we already had a selectField, use it (to preserve checkbox state)
                    this.Columns.Add( selectField ?? column );
                }
                else
                {
                    this.Columns.Add( column );
                }
            }
        }

        /// <summary>
        /// Gets the preview columns.
        /// </summary>
        /// <param name="modelType">Type of the model.</param>
        /// <returns></returns>
        public List<DataControlField> GetPreviewColumns( Type modelType )
        {
            var displayColumns = new List<DataControlField>();
            var allColumns = new List<DataControlField>();

            // If displaying people, add select field (for merging & communication)
            if ( CommunicationRecipientPersonIdFields.Any() || PersonIdField.IsNotNullOrWhiteSpace() )
            {
                var selectField = new SelectField();
                displayColumns.Add( selectField );
                allColumns.Add( selectField );
            }

            if ( modelType != null )
            {
                foreach ( var property in modelType.GetProperties() )
                {
                    // limit to non-virtual methods to prevent lazy loading issues
                    var getMethod = property.GetGetMethod();
                    if ( ( getMethod != null && ( !getMethod.IsVirtual || getMethod.IsFinal ) ) || ( property.GetCustomAttribute<PreviewableAttribute>() != null ) )
                    {
                        if ( property.Name != "Id" )
                        {
                            BoundField boundField = GetGridField( property );
                            boundField.DataField = property.Name;
                            boundField.SortExpression = property.Name;
                            boundField.HeaderText = property.Name.SplitCase();

                            if ( property.GetCustomAttributes( typeof( Rock.Data.PreviewableAttribute ) ).Count() > 0 )
                            {
                                displayColumns.Add( boundField );
                            }
                            else if ( displayColumns.Count == 0
                                && property.GetCustomAttributes( typeof( System.Runtime.Serialization.DataMemberAttribute ) ).Count() > 0
                                && !property.GetCustomAttributes( typeof( HideFromReportingAttribute ), true ).Any() )
                            {
                                allColumns.Add( boundField );
                            }
                        }
                    }
                }
            }

            var columns = new List<DataControlField>();

            // Always add hidden id column
            var idCol = new BoundField();
            idCol.DataField = "Id";
            idCol.Visible = false;
            columns.Add( idCol );
            idCol.HeaderText = "Id";
            columns.AddRange( displayColumns.Count > 0 ? displayColumns : allColumns );

            if ( columns.Count == 1 )
            {
                // if the only column is the Id column, show it
                idCol.Visible = true;
            }

            return columns;
        }

        private Dictionary<int, Dictionary<string, object>> GetPersonData( bool isForCommunication, Dictionary<string, string> communicationMergeFields )
        {
            var personData = new Dictionary<int, Dictionary<string, object>>();

            var personIdFields = new List<string>();

            if ( isForCommunication )
            {
                // If the data is being queried for a communication, the person id fields can be configured to come from a different column or even
                // multiple columns rather than the primary id column
                if ( this.CommunicationRecipientPersonIdFields.Any() )
                {
                    personIdFields = new List<string>( this.CommunicationRecipientPersonIdFields );
                }
                else
                {
                    // If there were not any special columns for the communication, just use the column that was configured for the person id
                    if ( this.PersonIdField.IsNotNullOrWhiteSpace() )
                    {
                        personIdFields.Add( this.PersonIdField );
                    }
                }
            }
            else
            {
                if ( this.PersonIdField.IsNotNullOrWhiteSpace() )
                {
                    personIdFields.Add( this.PersonIdField );
                }
            }

            if ( personIdFields.Any() )
            {
                // The ToList() is potentially needed for Linq cases.
                var keysSelected = SelectedKeys.ToList();
                string dataKeyColumn = this.DataKeyNames.FirstOrDefault() ?? "Id";

                if ( !string.IsNullOrWhiteSpace( dataKeyColumn ) && this.DataSourceAsDataTable != null )
                {
                    DataTable data = this.DataSourceAsDataTable;

                    foreach ( DataRowView rowView in data.DefaultView )
                    {
                        DataRow row = rowView.Row;
                        object dataKey = row[dataKeyColumn];
                        if ( !keysSelected.Any() || keysSelected.Contains( dataKey ) )
                        {
                            // Distinct list of person ids
                            List<int> personIds = new List<int>();

                            // Merge values
                            var mergeValues = new Dictionary<string, object>();

                            for (  int i = 0; i < data.Columns.Count; i++ )
                            {
                                // Add any new person id values from the selected person (or recipient) column(s)
                                if ( personIdFields.Contains( data.Columns[i].ColumnName, StringComparer.OrdinalIgnoreCase ) )
                                {
                                    int? personId = row[i] as int?;
                                    if ( personId.HasValue && !personIds.Contains( personId.Value ) )
                                    {
                                        personIds.Add( personId.Value );
                                    }
                                    else
                                    {
                                        foreach ( int id in row[i].ToString().SplitDelimitedValues().AsIntegerList() )
                                        {
                                            if ( !personIds.Contains( id ) )
                                            {
                                                personIds.Add( id );
                                            }
                                        }
                                    }
                                }

                                // If this is a communication, add any merge values
                                if ( isForCommunication )
                                {
                                    var mergeField = communicationMergeFields.Where( f => f.Key.Equals( data.Columns[i].ColumnName, StringComparison.OrdinalIgnoreCase ) ).Select( f => f.Value ).FirstOrDefault();
                                    if ( mergeField.IsNotNullOrWhiteSpace() )
                                    {
                                        var boundField = this.ColumnsOfType<RockBoundField>().Where( c => c.DataField == mergeField ).FirstOrDefault();
                                        if ( boundField != null )
                                        {
                                            mergeValues.AddOrIgnore( mergeField, boundField.FormatDataValue( row[i] ) );
                                        }
                                        else
                                        {
                                            mergeValues.AddOrIgnore( mergeField, row[i] );
                                        }
                                    }
                                }
                            }

                            // Add the personId if none are selected or if it's one of the selected items.
                            foreach ( var personId in personIds )
                            {
                                // Allow calling block to add additional merge fields
                                if ( isForCommunication )
                                {
                                    var eventArg = new GetRecipientMergeFieldsEventArgs( dataKey, personId, row );
                                    OnGetRecipientMergeFields( eventArg );
                                    {
                                        if ( eventArg.MergeValues != null )
                                        {
                                            foreach ( var mergeValue in eventArg.MergeValues )
                                            {
                                                if ( !communicationMergeFields.ContainsKey( mergeValue.Key ) )
                                                {
                                                    communicationMergeFields.Add( mergeValue.Key, mergeValue.Key );
                                                }
                                                mergeValues.Add( mergeValue.Key, mergeValue.Value );
                                            }
                                        }
                                    }
                                }

                                if ( !personData.ContainsKey( personId ) )
                                {
                                    personData.Add( personId, new Dictionary<string, object>( mergeValues ) );
                                    if ( isForCommunication )
                                    {
                                        personData[personId].Add( "AdditionalFields", new List<Dictionary<string, object>>() );
                                    }
                                }

                                if ( isForCommunication )
                                {
                                    var rows = personData[personId]["AdditionalFields"] as List<Dictionary<string, object>>;
                                    if ( rows != null )
                                    {
                                        rows.Add( new Dictionary<string, object>( mergeValues ) );
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    // get access to the List<> and its properties
                    IList data = this.DataSourceAsList;
                    if ( data != null )
                    {
                        Type oType = data.GetType().GetProperty( "Item" ).PropertyType;

                        PropertyInfo idProp = !string.IsNullOrEmpty( dataKeyColumn ) ? oType.GetProperty( dataKeyColumn ) : null;

                        foreach ( string personIdField in personIdFields )
                        {
                            var personIdProp = new List<PropertyInfo>();
                            var propPath = personIdField.Split( new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries ).ToList<string>();
                            while ( propPath.Any() )
                            {
                                var property = oType.GetProperty( propPath.First() );
                                if ( property != null )
                                {
                                    personIdProp.Add( property );
                                    oType = property.PropertyType;
                                }
                                propPath = propPath.Skip( 1 ).ToList();
                            }

                            foreach ( var item in data )
                            {
                                if ( !personIdProp.Any() )
                                {
                                    while ( propPath.Any() )
                                    {
                                        var property = item.GetType().GetProperty( propPath.First() );
                                        if ( property != null )
                                        {
                                            personIdProp.Add( property );
                                        }
                                        propPath = propPath.Skip( 1 ).ToList();
                                    }
                                }

                                if ( idProp == null )
                                {
                                    idProp = item.GetType().GetProperty( dataKeyColumn );
                                }

                                if ( personIdProp.Any() && idProp != null )
                                {
                                    var personIdObjTree = new List<object>();
                                    personIdObjTree.Add( item );
                                    bool propFound = true;
                                    foreach ( var prop in personIdProp )
                                    {
                                        object obj = prop.GetValue( personIdObjTree.Last(), null );
                                        if ( obj != null )
                                        {
                                            personIdObjTree.Add( obj );
                                        }
                                        else
                                        {
                                            propFound = false;
                                            break;
                                        }
                                    }

                                    List<int> personIds = new List<int>();
                                    if ( propFound )
                                    {
                                        if ( personIdObjTree.Last() is int )
                                        {
                                            int personId = (int)personIdObjTree.Last();
                                            if ( !personIds.Contains( personId ) )
                                            {
                                                personIds.Add( personId );
                                            }
                                        }
                                        if ( personIdObjTree.Last() is IEnumerable<int> )
                                        {
                                            foreach ( int id in ( (IEnumerable<int>)personIdObjTree.Last() ) )
                                            {
                                                if ( !personIds.Contains( id ) )
                                                {
                                                    personIds.Add( id );
                                                }
                                            }
                                        }
                                    }

                                    foreach( int personId in personIds )
                                    {
                                        int id = (int)idProp.GetValue( item, null );

                                        // Add the personId if none are selected or if it's one of the selected items.
                                        if ( !keysSelected.Any() || keysSelected.Contains( id ) )
                                        {
                                            var mergeValues = new Dictionary<string, object>();
                                            if ( isForCommunication )
                                            {
                                                foreach ( var keyVal in communicationMergeFields )
                                                {
                                                    object obj = item.GetPropertyValue( keyVal.Key );
                                                    if ( obj != null )
                                                    {
                                                        var boundField = this.ColumnsOfType<RockBoundField>().Where( c => c.DataField == keyVal.Key ).FirstOrDefault();
                                                        if ( boundField != null )
                                                        {
                                                            mergeValues.AddOrIgnore( keyVal.Value, boundField.FormatDataValue( obj ) );
                                                        }
                                                        else
                                                        {
                                                            mergeValues.AddOrIgnore( keyVal.Value, obj );
                                                        }
                                                    }
                                                }

                                                // Allow calling block to add additional merge fields
                                                var eventArg = new GetRecipientMergeFieldsEventArgs( id, personId, item );
                                                OnGetRecipientMergeFields( eventArg );
                                                {
                                                    if ( eventArg.MergeValues != null )
                                                    {
                                                        foreach ( var mergeValue in eventArg.MergeValues )
                                                        {
                                                            if ( !communicationMergeFields.ContainsKey( mergeValue.Key ) )
                                                            {
                                                                communicationMergeFields.Add( mergeValue.Key, mergeValue.Key );
                                                            }
                                                            if ( !mergeValues.ContainsKey( mergeValue.Key ) )
                                                            {
                                                                mergeValues.Add( mergeValue.Key, mergeValue.Value );
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                            if ( !personData.ContainsKey( personId ) )
                                            {
                                                personData.Add( personId, new Dictionary<string, object>( mergeValues ) );
                                                if ( isForCommunication )
                                                {
                                                    personData[personId].Add( "AdditionalFields", new List<Dictionary<string, object>>() );
                                                }
                                            }

                                            if ( isForCommunication )
                                            {
                                                var rows = personData[personId]["AdditionalFields"] as List<Dictionary<string, object>>;
                                                if ( rows != null )
                                                {
                                                    rows.Add( new Dictionary<string, object>( mergeValues ) );
                                                }
                                            }

                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }


            return personData;
        }

        private int? GetPersonEntitySet( EventArgs e )
        {
            // disable paging if no specific keys where selected (or if no select option is shown)
            bool selectAll = !SelectedKeys.Any();
            RebindGrid( e, selectAll );

            var keys = GetPersonData( false, null );
            if ( keys.Any() )
            {
                var entitySet = new Rock.Model.EntitySet();
                entitySet.EntityTypeId = EntityTypeCache.Get<Rock.Model.Person>().Id;
                entitySet.ExpireDateTime = RockDateTime.Now.AddMinutes( 5 );
                List<Rock.Model.EntitySetItem> entitySetItems = new List<Rock.Model.EntitySetItem>();

                foreach ( var key in keys )
                {
                    try
                    {
                        var item = new Rock.Model.EntitySetItem();
                        item.EntityId = ( int ) key.Key;
                        entitySetItems.Add( item );
                    }
                    catch
                    {
                        // ignore
                    }
                }

                if ( entitySetItems.Any() )
                {
                    var rockContext = new RockContext();
                    var service = new Rock.Model.EntitySetService( rockContext );
                    service.Add( entitySet );
                    rockContext.SaveChanges();
                    entitySetItems.ForEach( a =>
                    {
                        a.EntitySetId = entitySet.Id;
                    } );

                    rockContext.BulkInsert( entitySetItems );

                    return entitySet.Id;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the data source as List when the DataSource is a List (not a DataTable)
        /// </summary>
        /// <value>
        /// The data source as List.
        /// </value>
        public IList DataSourceAsList
        {
            get
            {
                if ( DataSource is IList )
                {
                    return DataSource as IList;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets the data source as a DataTable.
        /// </summary>
        /// <value>
        /// The data source as data table.
        /// </value>
        public DataTable DataSourceAsDataTable
        {
            get
            {
                if ( this.DataSource is DataTable )
                {
                    return ( DataTable ) this.DataSource;
                }
                else if ( this.DataSource is DataView )
                {
                    return ( ( DataView ) this.DataSource ).Table;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the entity set from grid.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <returns></returns>
        private int? GetEntitySetFromGrid( EventArgs e )
        {
            if ( this.DataSourceAsDataTable != null )
            {
                return GetEntitySetFromGridSourceDataTable();
            }
            else if ( this.DataSourceAsList != null )
            {
                return GetEntitySetFromGridSourceList();
            }

            return null;
        }

        /// <summary>
        /// Gets the entity set from grid if its datasource is a data table.
        /// </summary>
        /// <returns></returns>
        private int? GetEntitySetFromGridSourceDataTable()
        {
            var entitySet = new Rock.Model.EntitySet();

            // if the EntityTypeId was set for the Grid, use that, otherwise, we don't know since this is a DataTable
            if ( this.EntityTypeId.HasValue )
            {
                entitySet.EntityTypeId = this.EntityTypeId;
            }
            else
            {
                entitySet.EntityTypeId = null;
            }

            bool isPersonEntitySet = this.EntityTypeId == EntityTypeCache.GetId<Rock.Model.Person>();
            string dataKeyField = this.DataKeyNames.FirstOrDefault() ?? "Id";
            if ( isPersonEntitySet && !string.IsNullOrEmpty( this.PersonIdField ) )
            {
                dataKeyField = this.PersonIdField;
            }

            DataColumn dataKeyColumn = this.DataSourceAsDataTable.Columns.OfType<DataColumn>().FirstOrDefault( a => a.ColumnName == dataKeyField );

            entitySet.ExpireDateTime = RockDateTime.Now.AddMinutes( 5 );
            List<Rock.Model.EntitySetItem> entitySetItems = new List<Rock.Model.EntitySetItem>();

            int itemOrder = 0;
            int gridRowCounter = 0;
            var selectedKeys = SelectedKeys.ToList();
            foreach ( DataRowView row in this.DataSourceAsDataTable.DefaultView )
            {
                try
                {
                    if ( selectedKeys.Any() && this.DataKeyNames.Count() == 1 )
                    {
                        if ( gridRowCounter == this.Rows.Count )
                        {
                            // Stop when the counter reaches the number of rows displayed in the grid.
                            break;
                        }

                        var dataKeyValue = this.DataKeys[gridRowCounter].Value;
                        gridRowCounter++;

                        if ( !selectedKeys.Contains( dataKeyValue ) )
                        {
                            // if there are specific rows selected, skip over rows that aren't selected
                            continue;
                        }
                    }

                    var item = new Rock.Model.EntitySetItem();

                    if ( entitySet.EntityTypeId.HasValue && dataKeyColumn != null )
                    {
                        // we know the EntityTypeId, so set the EntityId to the dataKeyColumn value
                        item.EntityId = ( row[dataKeyColumn.ColumnName] as int? ) ?? 0;
                    }
                    else
                    {
                        // the datasource is a DataTable (not an Entity), so just set it to zero.  The entire datarow will be put into AdditionalMergeValues
                        item.EntityId = 0;
                    }

                    item.Order = itemOrder++;
                    item.AdditionalMergeValues = new Dictionary<string, object>();
                    foreach ( var col in this.DataSourceAsDataTable.Columns.OfType<DataColumn>() )
                    {
                        item.AdditionalMergeValues.Add( col.ColumnName, row[col.ColumnName] );
                    }

                    entitySetItems.Add( item );
                }
                catch
                {
                    // ignore
                }
            }

            if ( entitySetItems.Any() )
            {
                var rockContext = new RockContext();
                var service = new Rock.Model.EntitySetService( rockContext );
                service.Add( entitySet );
                rockContext.SaveChanges();

                entitySetItems.ForEach( a =>
                {
                    a.EntitySetId = entitySet.Id;
                } );

                rockContext.BulkInsert( entitySetItems );

                return entitySet.Id;
            }

            return null;
        }

        /// <summary>
        /// Gets the entity set from grid when the DataSource is a List (Grid datasource is not a DataTable)
        /// </summary>
        /// <returns></returns>
        private int? GetEntitySetFromGridSourceList()
        {
            var dataSourceObjectType = this.GetDataSourceObjectType();
            if ( dataSourceObjectType == null )
            {
                // Not an IList datasource
                return null;
            }

            int? entityTypeId = null;

            if ( this.EntityTypeId.HasValue )
            {
                entityTypeId = this.EntityTypeId.Value;
            }
            else
            {
                Type entityType = null;
                if ( typeof( IEntity ).IsAssignableFrom( dataSourceObjectType ) )
                {
                    if ( dataSourceObjectType.Assembly.IsDynamic )
                    {
                        // if the grid datasource is Dynamic (for example, DynamicProxy)
                        entityType = dataSourceObjectType.BaseType;
                    }
                    else
                    {
                        entityType = dataSourceObjectType;
                    }

                    var entityTypeCache = EntityTypeCache.Get( entityType, false );
                    if ( entityTypeCache != null )
                    {
                        entityTypeId = entityTypeCache.Id;
                    }
                }
            }

            string entityIdColumn;
            bool isPersonEntityTypeDifferentToKeys = false;
            if ( entityTypeId.HasValue && entityTypeId.Value == EntityTypeCache.GetId<Model.Person>() )
            {
                entityIdColumn = this.PersonIdField ?? "Id";
                if ( this.DataKeyNames.Any() && this.DataKeyNames.First() != entityIdColumn )
                {
                    isPersonEntityTypeDifferentToKeys = true;
                }
            }
            else
            {
                entityIdColumn = this.DataKeyNames.FirstOrDefault() ?? "Id";
            }

            PropertyInfo idProp = dataSourceObjectType.GetProperty( entityIdColumn );

            // first try to get the SelectedKeys from the SelectField (if there is one)
            HashSet<int> selectedKeys = new HashSet<int>( this.SelectedKeys.Select( a => a as int? ).Where( a => a.HasValue ).Select( a => a.Value ).Distinct().ToList() );

            if ( isPersonEntityTypeDifferentToKeys && selectedKeys.Any() )
            {
                var dataKeySelectedKeys = selectedKeys.ToList();
                PropertyInfo personIdProp = dataSourceObjectType.GetProperty( entityIdColumn );
                PropertyInfo dataKeyProp = dataSourceObjectType.GetProperty( this.DataKeyNames.First() );
                if ( personIdProp != null && dataKeyProp != null )
                {
                    if ( entityTypeId.HasValue && dataSourceObjectType is IEntity )
                    {
                        // we know this is an IEntity Type so the datakey is Id
                        selectedKeys = new HashSet<int>();

                        foreach ( var item in this.DataSourceAsList.OfType<IEntity>() )
                        {
                            int? dataKeyValue = dataKeyProp.GetValue( item ) as int?;

                            if ( dataKeyValue.HasValue && dataKeySelectedKeys.Contains( dataKeyValue.Value ) )
                            {
                                int? personIdValue = personIdProp.GetValue( item ) as int?;
                                if ( personIdValue.HasValue )
                                {
                                    selectedKeys.Add( personIdValue.Value );
                                }
                            }
                        }
                    }
                    else
                    {
                        // this is something else, so try to figure it out from dataKeyColumn
                        selectedKeys = new HashSet<int>();

                        foreach ( var item in this.DataSourceAsList )
                        {
                            int? dataKeyValue = dataKeyProp.GetValue( item ) as int?;

                            if ( dataKeyValue.HasValue && dataKeySelectedKeys.Contains( dataKeyValue.Value ) )
                            {
                                int? personIdValue = personIdProp.GetValue( item ) as int?;
                                if ( personIdValue.HasValue )
                                {
                                    selectedKeys.Add( personIdValue.Value );
                                }
                            }
                        }
                    }
                }
            }

            if ( selectedKeys == null || !selectedKeys.Any() )
            {
                if ( entityTypeId.HasValue && dataSourceObjectType is IEntity )
                {
                    // we know this is an IEntity Type so the datakey is Id
                    selectedKeys = new HashSet<int>( this.DataSourceAsList.OfType<IEntity>().Select( a => a.Id ).Distinct().ToList() );
                }
                else
                {
                    // this is something else, so try to figure it out from dataKeyColumn
                    selectedKeys = new HashSet<int>();

                    foreach ( var item in this.DataSourceAsList )
                    {
                        int? idValue = null;
                        if ( idProp != null )
                        {
                            idValue = idProp.GetValue( item ) as int?;
                        }

                        if ( idValue.HasValue )
                        {
                            selectedKeys.Add( idValue.Value );
                        }
                    }
                }
            }

            List<PropertyInfo> additionalMergeProperties = null;

            if ( entityTypeId.HasValue )
            {
                var dataSourceObjectTypeEntityType = EntityTypeCache.Get( dataSourceObjectType, false );
                if ( dataSourceObjectTypeEntityType != null && dataSourceObjectTypeEntityType.Id == entityTypeId )
                {
                    // the entityType and the Datasource type are the same, so no additional merge fields
                }
                else
                {
                    // the entityType and the Datasource type are different, so figure out the extra properties and put them into AdditionalMergeFields
                    var entityType = EntityTypeCache.Get( entityTypeId.Value ).GetEntityType();
                    var entityTypePropertyNames = entityType.GetProperties().Select( a => a.Name ).ToList();

                    additionalMergeProperties = new List<PropertyInfo>();
                    foreach ( var objProp in dataSourceObjectType.GetProperties().Where( a => !entityTypePropertyNames.Contains( a.Name ) ) )
                    {
                        additionalMergeProperties.Add( objProp );
                    }
                }
            }
            else
            {
                // we don't know the EntityType, so throw all the data into the AdditionalMergeFields
                additionalMergeProperties = dataSourceObjectType.GetProperties().ToList();
            }

            // If this is a DotLiquid.Drop class, don't include any of the properties that are inherited from DotLiquid.Drop
            if ( typeof( DotLiquid.Drop ).IsAssignableFrom( dataSourceObjectType ) )
            {
                var dropProperties = typeof( DotLiquid.Drop ).GetProperties().Select( a => a.Name );
                additionalMergeProperties = additionalMergeProperties.Where( a => !dropProperties.Contains( a.Name ) ).ToList();
            }

            var gridDataFields = this.Columns.OfType<BoundField>().ToList();

            Dictionary<int, Dictionary<string, object>> itemMergeFieldsList = new Dictionary<int, Dictionary<string, object>>( this.DataSourceAsList.Count );
            bool? useHeaderNamesIfAvailable = null;
            if ( additionalMergeProperties != null && additionalMergeProperties.Any() && idProp != null )
            {
                foreach ( var item in this.DataSourceAsList )
                {
                    // since Reporting fieldnames are dynamic and can have special internal names, use the header text instead of the datafield name
                    useHeaderNamesIfAvailable = useHeaderNamesIfAvailable ?? item.GetType().Assembly.IsDynamic;

                    var idVal = idProp.GetValue( item ) as int?;
                    if ( idVal.HasValue && selectedKeys.Contains( idVal.Value ) && !itemMergeFieldsList.ContainsKey( idVal.Value ) )
                    {
                        var mergeFields = new Dictionary<string, object>();
                        foreach ( var mergeProperty in additionalMergeProperties )
                        {
                            var objValue = mergeProperty.GetValue( item );

                            BoundField boundField = null;
                            if ( useHeaderNamesIfAvailable.Value )
                            {
                                boundField = gridDataFields.FirstOrDefault( a => a.DataField == mergeProperty.Name );
                            }

                            string mergeFieldKey;
                            if ( useHeaderNamesIfAvailable.Value && boundField != null && !string.IsNullOrWhiteSpace( boundField.HeaderText ) )
                            {
                                mergeFieldKey = boundField.HeaderText.RemoveSpecialCharacters().Replace( " ", "_" );
                            }
                            else
                            {
                                mergeFieldKey = mergeProperty.Name;
                            }

                            mergeFields.AddOrIgnore( mergeFieldKey, objValue );
                        }

                        itemMergeFieldsList.AddOrIgnore( idVal.Value, mergeFields );
                    }
                }
            }

            var entitySet = new Rock.Model.EntitySet();
            if ( entityTypeId.HasValue )
            {
                entitySet.EntityTypeId = entityTypeId.Value;
            }
            else
            {
                // unable to determine EntityTypeId, create the EntitySet has a list of "Anonymous" objects, putting everything in AdditionalMergeFieldsJson
                entitySet.EntityTypeId = null;
            }

            entitySet.ExpireDateTime = RockDateTime.Now.AddMinutes( 5 );
            List<Rock.Model.EntitySetItem> entitySetItems = new List<Rock.Model.EntitySetItem>();

            int itemOrder = 0;
            foreach ( var key in selectedKeys )
            {
                try
                {
                    var item = new Rock.Model.EntitySetItem();
                    item.EntityId = key;
                    item.Order = itemOrder++;
                    if ( itemMergeFieldsList.ContainsKey( key ) )
                    {
                        item.AdditionalMergeValues = itemMergeFieldsList[key];
                    }

                    entitySetItems.Add( item );
                }
                catch
                {
                    // ignore
                }
            }

            if ( entitySetItems.Any() )
            {
                var rockContext = new RockContext();
                var service = new Rock.Model.EntitySetService( rockContext );
                service.Add( entitySet );
                rockContext.SaveChanges();
                entitySetItems.ForEach( a =>
                {
                    a.EntitySetId = entitySet.Id;
                } );

                rockContext.BulkInsert( entitySetItems );

                return entitySet.Id;
            }

            return null;
        }

        /// <summary>
        /// Determines whether this instance [can view target page] the specified route.
        /// </summary>
        /// <param name="route">The route.</param>
        /// <returns></returns>
        public bool CanViewTargetPage( string route )
        {
            try
            {
                // cast the page as a Rock Page
                var rockPage = Page as RockPage;
                if ( rockPage != null )
                {
                    // If the route contains a parameter
                    if ( route.Contains( "{0}" ) )
                    {
                        // replace it with a fake param
                        route = string.Format( route, 1 );
                    }

                    // Get a uri
                    Uri uri = new Uri( rockPage.ResolveRockUrlIncludeRoot( route ) );
                    if ( uri != null )
                    {
                        // Find a page ref based on the uri
                        var pageRef = new Rock.Web.PageReference( uri, Page.Request.ApplicationPath );
                        if ( pageRef.IsValid )
                        {
                            // if a valid pageref was found, check the security of the page
                            var page = PageCache.Get( pageRef.PageId );
                            if ( page != null )
                            {
                                return page.IsAuthorized( Rock.Security.Authorization.VIEW, rockPage.CurrentPerson );
                            }
                        }
                    }
                }
            }
            catch
            {
                // ignore
            }

            return false;
        }

        #endregion

        #region Callback Methods/Events

        /// <summary>
        /// Raises the appropriate events for the <see cref="T:System.Web.UI.WebControls.GridView"/> control when it posts back to the server.
        /// </summary>
        /// <param name="eventArgument">The event argument from which to create a <see cref="T:System.Web.UI.WebControls.CommandEventArgs"/> for the event or events that are raised.</param>
        void IPostBackEventHandler.RaisePostBackEvent( string eventArgument )
        {
            if ( eventArgument.StartsWith( "re-order:" ) )
            {
                string[] parms = eventArgument.Substring( 9 ).Split( ';' );

                string dataKey = parms[0];

                int oldIndex = parms[1].AsIntegerOrNull() ?? 0;

                int newIndex = parms[2].AsIntegerOrNull() ?? 0;

                int pageFactor = this.PageIndex * this.PageSize;
                oldIndex += pageFactor;
                newIndex += pageFactor;

                GridReorderEventArgs args = new GridReorderEventArgs( dataKey, oldIndex, newIndex );
                OnGridReorder( args );
            }
            else
            {
                base.RaisePostBackEvent( eventArgument );
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Occurs when [grid reorder].
        /// </summary>
        public event GridReorderEventHandler GridReorder;

        /// <summary>
        /// Raises the <see cref="E:GridReorder"/> event.
        /// </summary>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected virtual void OnGridReorder( GridReorderEventArgs e )
        {
            if ( GridReorder != null )
            {
                GridReorder( this, e );
            }
        }

        /// <summary>
        /// Occurs when [grid rebind].
        /// </summary>
        public event GridRebindEventHandler GridRebind;

        /// <summary>
        /// Raises the <see cref="E:GridRebind" /> event.
        /// </summary>
        /// <param name="e">The <see cref="GridRebindEventArgs"/> instance containing the event data.</param>
        protected virtual void OnGridRebind( GridRebindEventArgs e )
        {
            if ( GridRebind != null )
            {
                GridRebind( this, e );
            }
        }

        /// <summary>
        /// Occurs when [row click].
        /// </summary>
        public event EventHandler<RowEventArgs> RowSelected;

        /// <summary>
        /// Raises the <see cref="E:RowSelected" /> event.
        /// </summary>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected virtual void OnRowSelected( RowEventArgs e )
        {
            if ( RowSelected != null )
            {
                RowSelected( this, e );
            }
        }

        /// <summary>
        /// Occurs when grid gets recipient merge fields.
        /// </summary>
        public event EventHandler<GetRecipientMergeFieldsEventArgs> GetRecipientMergeFields;

        /// <summary>
        /// Raises the <see cref="E:GetRecipientMergeFields" /> event.
        /// </summary>
        /// <param name="e">The <see cref="GetRecipientMergeFieldsEventArgs"/> instance containing the event data.</param>
        protected virtual void OnGetRecipientMergeFields( GetRecipientMergeFieldsEventArgs e )
        {
            if ( GetRecipientMergeFields != null )
            {
                GetRecipientMergeFields( this, e );
            }
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Gets the most appropriate grid field for the model property
        /// </summary>
        /// <param name="propertyInfo">The property information.</param>
        /// <returns></returns>
        public static BoundField GetGridField( PropertyInfo propertyInfo )
        {
            var specifiedBoundFieldType = propertyInfo.GetCustomAttribute<BoundFieldTypeAttribute>();
            if ( specifiedBoundFieldType != null )
            {
                return Activator.CreateInstance( specifiedBoundFieldType.BoundFieldType ) as BoundField;
            }
            else
            {
                return GetGridField( propertyInfo.PropertyType );
            }
        }

        /// <summary>
        /// Gets the most appropriate grid field for the propertyType (int, bool, etc)
        /// </summary>
        /// <param name="propertyType">Type of the property.</param>
        /// <returns></returns>
        public static BoundField GetGridField( Type propertyType )
        {
            BoundField bf = new BoundField();
            Type baseType = propertyType;

            if ( baseType == typeof( bool ) || baseType == typeof( bool? ) )
            {
                bf = new BoolField();
            }
            else if ( baseType == typeof( DateTime ) || baseType == typeof( DateTime? ) )
            {
                bf = new DateField();
            }
            else if ( baseType.IsEnum )
            {
                bf = new EnumField();
            }
            else if ( baseType == typeof( decimal ) || baseType == typeof( decimal? ) ||
                baseType == typeof( int ) || baseType == typeof( int? ) )
            {
                bf = new BoundField();
                bf.HeaderStyle.HorizontalAlign = HorizontalAlign.Right;
                bf.ItemStyle.HorizontalAlign = HorizontalAlign.Right;
            }
            else if ( baseType == typeof( IEnumerable<object> ) )
            {
                bf = new ListDelimitedField();
            }

            return bf;
        }

        /// <summary>
        /// Returns a list of Columns of the specified type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IEnumerable<T> ColumnsOfType<T>() where T : DataControlField
        {
            var result = new List<T>();
            foreach ( var col in Columns )
            {
                if ( col is T )
                {
                    result.Add( col as T );
                }
            }

            return result;
        }

        /// <summary>
        /// Returns a list of Columns matching the specified DataField name
        /// </summary>
        /// <param name="dataField">The data field.</param>
        /// <returns></returns>
        public IEnumerable<BoundField> ColumnsWithDataField( string dataField )
        {
            return ColumnsOfType<BoundField>().Where( a => a.DataField == dataField );
        }

        #endregion
    }

    #region Delegates

    /// <summary>
    /// Delegate used for raising the grid reorder event
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
    public delegate void GridReorderEventHandler( object sender, GridReorderEventArgs e );

    /// <summary>
    /// Delegate used for raising the grid rebind event
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="GridRebindEventArgs"/> instance containing the event data.</param>
    public delegate void GridRebindEventHandler( object sender, GridRebindEventArgs e );

    /// <summary>
    /// Delegate used for raising the grid items per page changed event
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="Rock.Web.UI.Controls.NumericalEventArgs"/> instance containing the event data.</param>
    internal delegate void PageNavigationEventHandler( object sender, NumericalEventArgs e );

    #endregion

    #region Event Arguments

    /// <summary>
    /// Items Per RockPage Event Argument
    /// </summary>
    public class NumericalEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the items per page.
        /// </summary>
        public int Number { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NumericalEventArgs"/> class.
        /// </summary>
        /// <param name="number">The number.</param>
        public NumericalEventArgs( int number )
        {
            Number = number;
        }
    }

    /// <summary>
    /// Event argument for rebind
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class GridRebindEventArgs : EventArgs
    {
        /// <summary>
        /// Gets a value indicating whether this instance is exporting.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is exporting; otherwise, <c>false</c>.
        /// </value>
        public bool IsExporting { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is communication.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is communication; otherwise, <c>false</c>.
        /// </value>
        public bool IsCommunication { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GridRebindEventArgs"/> class.
        /// </summary>
        public GridRebindEventArgs() : base()
        {
            IsExporting = false;
            IsCommunication = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GridRebindEventArgs"/> class.
        /// </summary>
        /// <param name="isExporting">if set to <c>true</c> [is exporting].</param>
        public GridRebindEventArgs( bool isExporting ) : base()
        {
            IsExporting = isExporting;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GridRebindEventArgs"/> class.
        /// </summary>
        /// <param name="isExporting">if set to <c>true</c> [is exporting].</param>
        /// <param name="isCommunication">if set to <c>true</c> [is communication].</param>
        public GridRebindEventArgs( bool isExporting, bool isCommunication ) : base()
        {
            IsExporting = isExporting;
            IsCommunication = isCommunication;
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="System.Web.UI.WebControls.GridViewRowEventArgs" />
    public class RockGridViewRowEventArgs : GridViewRowEventArgs
    {

        /// <summary>
        /// Gets a value indicating whether this instance is exporting.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is exporting; otherwise, <c>false</c>.
        /// </value>
        public bool IsExporting { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockGridViewRowEventArgs"/> class.
        /// </summary>
        /// <param name="row">A <see cref="T:System.Web.UI.WebControls.GridViewRow" /> object that represents the row being created or data-bound.</param>
        public RockGridViewRowEventArgs( GridViewRow row ) : base( row )
        {
            IsExporting = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockGridViewRowEventArgs"/> class.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="isExporting">if set to <c>true</c> [is exporting].</param>
        public RockGridViewRowEventArgs( GridViewRow row, bool isExporting ) : base( row )
        {
            IsExporting = isExporting;
        }
    }

    /// <summary>
    /// Grid Reorder Event Argument
    /// </summary>
    public class GridReorderEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the data key.
        /// </summary>
        public string DataKey { get; private set; }

        /// <summary>
        /// Gets the old index.
        /// </summary>
        public int OldIndex { get; private set; }

        /// <summary>
        /// Gets the new index.
        /// </summary>
        public int NewIndex { get; private set; }

        /// <summary>
        ///
        /// </summary>
        private bool _cancel = false;

        /// <summary>
        /// Gets or sets a value indicating whether the reorder event should be cancelled
        /// </summary>
        /// <value>
        ///   <c>true</c> if cancelled; otherwise, <c>false</c>.
        /// </value>
        public bool Cancel
        {
            get { return _cancel; }
            set { _cancel = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GridReorderEventArgs"/> class.
        /// </summary>
        /// <param name="dataKey">The data key.</param>
        /// <param name="oldIndex">The old index.</param>
        /// <param name="newIndex">The new index.</param>
        public GridReorderEventArgs( string dataKey, int oldIndex, int newIndex )
        {
            DataKey = dataKey;
            OldIndex = oldIndex;
            NewIndex = newIndex;
        }
    }

    /// <summary>
    /// Event handler used when getting recipient merge fields for new communication
    /// </summary>
    public class GetRecipientMergeFieldsEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the data key.
        /// </summary>
        public object DataKey { get; private set; }

        /// <summary>
        /// Gets the person identifier.
        /// </summary>
        /// <value>
        /// The person identifier.
        /// </value>
        public int? PersonId { get; private set; }

        /// <summary>
        /// Gets the data item.
        /// </summary>
        /// <value>
        /// The data item.
        /// </value>
        public object DataItem { get; private set; }

        /// <summary>
        /// Gets or sets the merge values.
        /// </summary>
        /// <value>
        /// The merge values.
        /// </value>
        public Dictionary<string, object> MergeValues { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GetRecipientMergeFieldsEventArgs" /> class.
        /// </summary>
        /// <param name="dataKey">The data key.</param>
        /// <param name="personId">The person identifier.</param>
        /// <param name="dataItem">The data item.</param>
        public GetRecipientMergeFieldsEventArgs( object dataKey, int? personId, object dataItem )
        {
            DataKey = dataKey;
            PersonId = personId;
            DataItem = dataItem;
            MergeValues = new Dictionary<string, object>();
        }
    }

    #endregion

    #region Helper Classes

    /// <summary>
    /// JSON Result
    /// </summary>
    internal class JsonResult
    {
        /// <summary>
        /// Gets or sets the action.
        /// </summary>
        /// <value>
        /// The action.
        /// </value>
        public string Action { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="JsonResult"/> is cancel.
        /// </summary>
        /// <value>
        ///   <c>true</c> if cancel; otherwise, <c>false</c>.
        /// </value>
        public bool Cancel { get; set; }

        /// <summary>
        /// Gets or sets the result.
        /// </summary>
        /// <value>
        /// The result.
        /// </value>
        public object Result { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonResult" /> class.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="cancel">if set to <c>true</c> [cancel].</param>
        public JsonResult( string action, bool cancel )
        {
            Action = action;
            Cancel = cancel;
            Result = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonResult" /> class.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="cancel">if set to <c>true</c> [cancel].</param>
        /// <param name="result">The result.</param>
        public JsonResult( string action, bool cancel, object result )
        {
            Action = action;
            Cancel = cancel;
            Result = result;
        }

        /// <summary>
        /// Serializes this instance.
        /// </summary>
        /// <returns></returns>
        public string Serialize()
        {
            System.Web.Script.Serialization.JavaScriptSerializer serializer =
                new System.Web.Script.Serialization.JavaScriptSerializer();

            StringBuilder sb = new StringBuilder();

            serializer.Serialize( this, sb );

            return sb.ToString();
        }
    }

    /// <summary>
    /// Class for saving sort expression
    /// </summary>
    [Serializable]
    public class SortProperty
    {
        /// <summary>
        /// Gets or sets the direction.
        /// </summary>
        /// <value>
        /// The direction.
        /// </value>
        public System.Web.UI.WebControls.SortDirection Direction { get; set; }

        /// <summary>
        /// Gets the direction as an ASC or DESC string.
        /// </summary>
        /// <value>
        /// The direction string.
        /// </value>
        public string DirectionString
        {
            get
            {
                if ( Direction == SortDirection.Descending )
                {
                    return "DESC";
                }
                else
                {
                    return "ASC";
                }
            }
        }

        /// <summary>
        /// Gets or sets the Column(s) specification for Sorting.
        /// Specify multiple columns as "column1, column2, column3".
        /// To sort DESC append column(s) with a " desc", for example: "column1 desc, column2, column3 desc"
        /// </summary>
        /// <value>
        /// The property.
        /// </value>
        public string Property { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SortProperty"/> class.
        /// </summary>
        public SortProperty()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SortProperty"/> class.
        /// </summary>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.GridViewSortEventArgs"/> instance containing the event data.</param>
        public SortProperty( GridViewSortEventArgs e )
        {
            Direction = e.SortDirection;
            Property = e.SortExpression;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format( "{0} [{1}]", this.Property, this.Direction );
        }
    }

    #endregion

    #region Templates

    /// <summary>
    /// Template used for the pager row in the <see cref="Grid"/> control
    /// </summary>
    internal class PagerTemplate : ITemplate, IDisposable
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is disposed.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is disposed; otherwise, <c>false</c>.
        /// </value>
        internal bool IsDisposed { get; set; }

        /// <summary>
        /// Gets or sets the navigation panel.
        /// </summary>
        /// <value>
        /// The navigation panel.
        /// </value>
        internal HtmlGenericControl NavigationPanel { get; set; }

        /// <summary>
        /// Gets or sets the page link list item.
        /// </summary>
        /// <value>
        /// The page link list item.
        /// </value>
        internal HtmlGenericControl[] PageLinkListItem
        {
            get { return _pageLinkListItem; }
            set { _pageLinkListItem = value; }
        }

        private HtmlGenericControl[] _pageLinkListItem = new HtmlGenericControl[12];

        /// <summary>
        /// Gets or sets the page link.
        /// </summary>
        /// <value>
        /// The page link.
        /// </value>
        internal LinkButton[] PageLink
        {
            get { return _pageLink; }
            set { _pageLink = value; }
        }

        private LinkButton[] _pageLink = new LinkButton[12];

        /// <summary>
        /// Gets or sets the item count display.
        /// </summary>
        /// <value>
        /// The item count display.
        /// </value>
        internal Literal itemCountDisplay { get; set; }

        /// <summary>
        /// Gets or sets the item link list item.
        /// </summary>
        /// <value>
        /// The item link list item.
        /// </value>
        internal HtmlGenericControl[] ItemLinkListItem
        {
            get { return _itemLinkListItem; }
            set { _itemLinkListItem = value; }
        }

        private HtmlGenericControl[] _itemLinkListItem = new HtmlGenericControl[3];

        /// <summary>
        /// Gets or sets the item link.
        /// </summary>
        /// <value>
        /// The item link.
        /// </value>
        internal LinkButton[] ItemLink
        {
            get { return _itemLink; }
            set { _itemLink = value; }
        }

        private LinkButton[] _itemLink = new LinkButton[3];

        /// <summary>
        /// Initializes a new instance of the <see cref="PagerTemplate"/> class.
        /// </summary>
        public PagerTemplate()
        {
            IsDisposed = false;
        }

        /// <summary>
        /// When implemented by a class, defines the <see cref="T:System.Web.UI.Control"/> object that child controls and templates belong to. These child controls are in turn defined within an inline template.
        /// </summary>
        /// <param name="container">The <see cref="T:System.Web.UI.Control"/> object to contain the instances of controls from the inline template.</param>
        public void InstantiateIn( Control container )
        {
            HtmlGenericControl ulSizeOptions = new HtmlGenericControl( "ul" );
            ulSizeOptions.AddCssClass( "grid-pagesize pagination pagination-sm" );
            container.Controls.Add( ulSizeOptions );

            for ( int i = 0; i < ItemLinkListItem.Length; i++ )
            {
                ItemLinkListItem[i] = new HtmlGenericControl( "li" );
                ulSizeOptions.Controls.Add( ItemLinkListItem[i] );

                ItemLink[i] = new LinkButton();
                ItemLinkListItem[i].Controls.Add( ItemLink[i] );
                ItemLink[i].ID = string.Format( "ItemLink{0}", i );
                ItemLink[i].Text = ( 5 * Math.Pow( 10, i + 1 ) ).ToString( "N0" );
                ItemLink[i].CausesValidation = false;
                ItemLink[i].Click += new EventHandler( lbItems_Click );
            }

            // itemCount
            HtmlGenericControl divItemCount = new HtmlGenericControl( "div" );
            divItemCount.Attributes.Add( "class", "grid-itemcount" );
            container.Controls.Add( divItemCount );

            itemCountDisplay = new Literal();
            divItemCount.Controls.Add( itemCountDisplay );

            // Pagination
            NavigationPanel = new HtmlGenericControl( "ul" );
            NavigationPanel.AddCssClass( "grid-pager pagination pagination-sm" );
            container.Controls.Add( NavigationPanel );

            for ( var i = 0; i < PageLinkListItem.Length; i++ )
            {
                PageLinkListItem[i] = new HtmlGenericControl( "li" );
                NavigationPanel.Controls.Add( PageLinkListItem[i] );

                PageLink[i] = new LinkButton();
                PageLinkListItem[i].Controls.Add( PageLink[i] );
                PageLink[i].ID = string.Format( "pageLink{0}", i );
                PageLink[i].CausesValidation = false;
                PageLink[i].Command += lbPage_Command;
            }

            PageLink[0].Text = "&laquo;";
            PageLink[PageLinkListItem.Length - 1].Text = "&raquo;";
        }

        /// <summary>
        /// Set the RockPage Navigation Display
        /// </summary>
        /// <param name="pageCount">The number of total pages</param>
        /// <param name="pageIndex">The current page index</param>
        /// <param name="pageSize">The number of items on each page</param>
        /// <param name="itemCount">The item count.</param>
        /// <param name="rowItemText">The row item text.</param>
        public void SetNavigation( int pageCount, int pageIndex, int pageSize, int itemCount, string rowItemText )
        {
            // Set navigation controls
            if ( NavigationPanel != null )
            {
                if ( pageCount > 1 )
                {
                    int totalGroups = ( int ) ( ( pageCount - 1 ) / 10 ) + 1;
                    int currentGroupIndex = ( int ) ( pageIndex / 10 );

                    int prevPageIndex = ( ( currentGroupIndex - 1 ) * 10 ) + 9;
                    if ( prevPageIndex < 0 )
                    {
                        prevPageIndex = 0;
                        PageLinkListItem[0].Attributes["class"] = "prev disabled";
                        PageLink[0].Enabled = false;
                    }
                    else
                    {
                        PageLinkListItem[0].Attributes["class"] = "prev";
                        PageLink[0].Enabled = true;
                    }
                    PageLink[0].CommandName = prevPageIndex.ToString();

                    int nextPageIndex = ( currentGroupIndex + 1 ) * 10;
                    if ( nextPageIndex >= pageCount - 1 )
                    {
                        nextPageIndex = pageCount - 1;
                        PageLinkListItem[PageLinkListItem.Length - 1].Attributes["class"] = "next disabled";
                        PageLink[PageLinkListItem.Length - 1].Enabled = false;
                    }
                    else
                    {
                        PageLinkListItem[PageLinkListItem.Length - 1].Attributes["class"] = "next";
                        PageLink[PageLinkListItem.Length - 1].Enabled = true;
                    }
                    PageLink[PageLinkListItem.Length - 1].CommandName = nextPageIndex.ToString();

                    NavigationPanel.Visible = true;
                    for ( int i = 1; i < PageLink.Length - 1; i++ )
                    {
                        int buttonPageIndex = ( currentGroupIndex * 10 ) + ( i - 1 );

                        HtmlGenericControl li = PageLinkListItem[i];
                        LinkButton lb = PageLink[i];

                        if ( buttonPageIndex < pageCount )
                        {
                            li.Attributes["class"] = buttonPageIndex == pageIndex ? "active" : string.Empty;
                            li.Visible = true;
                            lb.Text = ( buttonPageIndex + 1 ).ToString( "N0" );
                            lb.Visible = true;
                            lb.CommandName = buttonPageIndex.ToString();
                        }
                        else
                        {
                            li.Visible = false;
                            lb.Visible = false;
                        }
                    }
                }
                else
                {
                    NavigationPanel.Visible = false;
                }
            }

            // Set Item Count
            if ( itemCountDisplay != null )
            {
                itemCountDisplay.Text = string.Format( "{0:N0} {1}", itemCount, itemCount == 1 ? rowItemText : rowItemText.Pluralize() );
            }

            // Set page size controls
            if ( ItemLinkListItem[0] != null )
            {
                string pageSizeValue = pageSize.ToString( "N0" );
                for ( int i = 0; i < ItemLinkListItem.Length; i++ )
                {
                    ItemLinkListItem[i].Attributes["class"] = ItemLink[i].Text == pageSizeValue ? "active" : string.Empty;
                }
            }
        }

        /// <summary>
        /// Handles the Command event of the lbPage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CommandEventArgs"/> instance containing the event data.</param>
        void lbPage_Command( object sender, CommandEventArgs e )
        {
            if ( NavigateClick != null )
            {
                LinkButton lbPage = sender as LinkButton;
                if ( lbPage != null )
                {
                    int? pageIndex = lbPage.CommandName.AsIntegerOrNull();
                    if ( pageIndex.HasValue )
                    {
                        NumericalEventArgs eventArgs = new NumericalEventArgs( pageIndex.Value );
                        NavigateClick( sender, eventArgs );
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the lbPageSize control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void lbItems_Click( object sender, EventArgs e )
        {
            LinkButton lbItems = sender as LinkButton;
            if ( lbItems != null && ItemsPerPageClick != null )
            {
                int itemsPerPage = 50;

                switch ( lbItems.Text )
                {
                    case "50":
                        itemsPerPage = 50;
                        break;
                    case "500":
                        itemsPerPage = 500;
                        break;
                    case "5,000":
                        itemsPerPage = 5000;
                        break;
                }

                NumericalEventArgs eventArgs = new NumericalEventArgs( itemsPerPage );

                ItemsPerPageClick( sender, eventArgs );
            }
        }

        /// <summary>
        /// Occurs when [navigate click].
        /// </summary>
        internal event PageNavigationEventHandler NavigateClick;

        /// <summary>
        /// Occurs when [page click].
        /// </summary>
        internal event PageNavigationEventHandler ItemsPerPageClick;

        /// <summary>
        /// Dispose object
        /// </summary>
        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose( bool disposing )
        {
            if ( !IsDisposed )
            {
                if ( disposing )
                {
                    if ( NavigationPanel != null )
                    {
                        NavigationPanel.Dispose();
                    }
                }

                NavigationPanel = null;
                IsDisposed = true;
            }
        }
    }

    /// <summary>
    ///
    /// </summary>
    public enum GridDisplayType
    {
        /// <summary>
        /// The full
        /// </summary>
        Full,

        /// <summary>
        /// The light
        /// </summary>
        Light
    }

    /// <summary>
    /// The data to export when Excel Export is selected
    /// </summary>
    public enum ExcelExportSource
    {
        /// <summary>
        /// Use the columns and formatting from the grid's data source
        /// </summary>
        DataSource,

        /// <summary>
        /// The the columns and formatting that is displayed in output
        /// </summary>
        ColumnOutput
    }

    /// <summary>
    /// Column Priority Values
    /// </summary>
    public enum ColumnPriority
    {
        /// <summary>
        /// Always Visible
        /// </summary>
        AlwaysVisible = 1,

        /// <summary>
        /// Devices Devices with screensize > 480px
        /// </summary>
        TabletSmall = 2,

        /// <summary>
        /// Devices with screensize > 640px
        /// </summary>
        Tablet = 3,

        /// <summary>
        /// Devices with screensize > 800px
        /// </summary>
        DesktopSmall = 4,

        /// <summary>
        /// Devices with screensize > 960px
        /// </summary>
        Desktop = 5,

        /// <summary>
        /// Devices with screensize > 1120px
        /// </summary>
        DesktopLarge = 6
    }

    /// <summary>
    ///
    /// </summary>
    internal class RockDummyDataSource : ICollection, IEnumerable
    {
        private int dataItemCount;

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.ICollection" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.ICollection" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        public void CopyTo( Array array, int index )
        {
            IEnumerator enumerator = this.GetEnumerator();
            while ( enumerator.MoveNext() )
            {
                int num = index;
                index = num + 1;
                array.SetValue( enumerator.Current, num );
            }
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.ICollection" />.
        /// </summary>
        public int Count
        {
            get { return dataItemCount; }
            set { dataItemCount = value; }
        }

        /// <summary>
        /// Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe).
        /// </summary>
        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.
        /// </summary>
        public object SyncRoot
        {
            get
            {
                return this;
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new RockDummyDataSource.RockDummyDataSourceEnumerator( this.dataItemCount );
        }

        /// <summary>
        ///
        /// </summary>
        private class RockDummyDataSourceEnumerator : IEnumerator
        {
            private int count;

            private int index;

            /// <summary>
            /// Gets the current element in the collection.
            /// </summary>
            public object Current
            {
                get
                {
                    return null;
                }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="RockDummyDataSourceEnumerator"/> class.
            /// </summary>
            /// <param name="count">The count.</param>
            public RockDummyDataSourceEnumerator( int count )
            {
                this.count = count;
                this.index = -1;
            }

            /// <summary>
            /// Advances the enumerator to the next element of the collection.
            /// </summary>
            /// <returns>
            /// true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.
            /// </returns>
            public bool MoveNext()
            {
                RockDummyDataSource.RockDummyDataSourceEnumerator dummyDataSourceEnumerator = this;
                dummyDataSourceEnumerator.index = dummyDataSourceEnumerator.index + 1;
                return this.index < this.count;
            }

            /// <summary>
            /// Sets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
            public void Reset()
            {
                this.index = -1;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockDummyDataSource"/> class.
        /// </summary>
        /// <param name="dataItemCount">The data item count.</param>
        internal RockDummyDataSource( int dataItemCount )
        {
            this.dataItemCount = dataItemCount;
        }
    }

    #endregion
}