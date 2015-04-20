// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using DotLiquid;
using OfficeOpenXml;
using Rock.Data;
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
        private GridViewRow _actionRow;
        private GridActions _gridActions;
        private Dictionary<int, string> _columnDataPriorities;

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
                return result;
            }
            set
            {
                base.EmptyDataText = value;
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
                }
            }
        }

        /// <summary>
        /// Gets the sort property.
        /// </summary>
        public SortProperty SortProperty
        {
            get { return ViewState["SortProperty"] as SortProperty; }
            private set { ViewState["SortProperty"] = value; }
        }

        /// <summary>
        /// Gets or sets a list of datasource field/properties that can optionally be included as additional 
        /// merge fields when a new communication is created from the grid.  NOTE: A side affect of using 
        /// additional merge fields is that user will not be able to add additional recipients to the 
        /// communication after it is created from the grid
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
        /// Gets the action row.
        /// </summary>
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden ), Browsable( false )]
        public virtual GridViewRow ActionRow
        {
            get
            {
                if ( this._actionRow == null )
                {
                    this.EnsureChildControls();
                }

                return this._actionRow;
            }
        }

        /// <summary>
        /// Gets the actions control
        /// </summary>
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden ), Browsable( false )]
        public virtual GridActions Actions
        {
            get { return this._gridActions; }
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

            _gridActions = new GridActions( this );

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

            var rockPage = this.Page as RockPage;
            if ( rockPage != null )
            {
                int pageSize = 50;
                int.TryParse( rockPage.GetUserPreference( PAGE_SIZE_KEY ), out pageSize );
                if ( pageSize != 50 && pageSize != 500 && pageSize != 5000 )
                {
                    pageSize = 50;
                }

                base.PageSize = pageSize;
            }

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
                        var colIndex = this.Columns.IndexOf( col ).ToString();

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
                                if ( value == cb.ClientID )
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
                string deleteButtonScriptFormat = @"
   $('#{0} .grid-delete-button').not('.disabled').on( 'click', function (event) {{
  return Rock.dialogs.confirmDelete(event, '{1}');
}});";
                string deleteButtonScript = string.Format( deleteButtonScriptFormat, this.ClientID, this.RowItemText );
                ScriptManager.RegisterStartupScript( this, this.GetType(), "grid-delete-confirm-script-" + this.ClientID, deleteButtonScript, true );
            }

            string clickScript = string.Format( "__doPostBack('{0}', 'RowSelected$' + dataRowIndexValue);", this.UniqueID );

            string gridSelectCellScriptFormat = @"
   $('#{0} .grid-select-cell').on( 'click', function (event) {{
  var dataRowIndexValue = $(this).closest('tr').attr('data-row-index');
  {1}
}});";
            string gridSelectCellScript = string.Format( gridSelectCellScriptFormat, this.ClientID, clickScript );
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

            if ( this.DataSource != null )
            {
                if ( this.EnableResponsiveTable )
                {
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "table-responsive" );
                }

                if ( DisplayType == GridDisplayType.Light )
                {
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "table-no-border" );
                }

                writer.RenderBeginTag( HtmlTextWriterTag.Div );
            }

            this.AddCssClass( "grid-table" );
            this.AddCssClass( "table" );

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

            if ( this.DataSource != null )
            {
                writer.RenderEndTag();
            }
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
        /// Creates the control hierarchy used to render the <see cref="T:System.Web.UI.WebControls.GridView"/> control using the specified data source.
        /// </summary>
        /// <param name="dataSource">An <see cref="T:System.Collections.IEnumerable"/> that contains the data source for the <see cref="T:System.Web.UI.WebControls.GridView"/> control.</param>
        /// <param name="dataBinding">true to indicate that the child controls are bound to data; otherwise, false.</param>
        /// <returns>
        /// The number of rows created.
        /// </returns>
        /// <exception cref="T:System.Web.HttpException">
        ///   <paramref name="dataSource"/> returns a null <see cref="T:System.Web.UI.DataSourceView"/>.-or-<paramref name="dataSource"/> does not implement the <see cref="T:System.Collections.ICollection"/> interface and cannot return a <see cref="P:System.Web.UI.DataSourceSelectArguments.TotalRowCount"/>. -or-<see cref="P:System.Web.UI.WebControls.GridView.AllowPaging"/> is true and <paramref name="dataSource"/> does not implement the <see cref="T:System.Collections.ICollection"/> interface and cannot perform data source paging.-or-<paramref name="dataSource"/> does not implement the <see cref="T:System.Collections.ICollection"/> interface and <paramref name="dataBinding"/> is set to false.</exception>
        protected override int CreateChildControls( System.Collections.IEnumerable dataSource, bool dataBinding )
        {
            int result = base.CreateChildControls( dataSource, dataBinding );

            if ( _table != null && _table.Parent != null )
            {
                if ( this.AllowPaging && this.BottomPagerRow != null )
                {
                    this.BottomPagerRow.Visible = true;

                    // add paging style
                    if ( this.BottomPagerRow.Cells.Count > 0 )
                    {
                        this.BottomPagerRow.Cells[0].CssClass = "grid-paging";
                    }
                }

                _actionRow = base.CreateRow( -1, -1, DataControlRowType.Footer, DataControlRowState.Normal );
                _table.Rows.Add( _actionRow );

                TableCell cell = new TableCell();
                cell.ColumnSpan = this.Columns.Count;
                cell.CssClass = "grid-actions";
                _actionRow.Cells.Add( cell );

                cell.Controls.Add( _gridActions );

                if ( !this.ShowActionRow )
                {
                    _actionRow.Visible = false;
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
            _columnDataPriorities = new Dictionary<int, string>();

            for ( int i = 0; i < this.Columns.Count; i++ )
            {
                var column = this.Columns[i];
                if ( !( column is INotRowSelectedField ) && !( column is HyperLinkField ) )
                {
                    RowSelectedColumns.Add( i, this.Columns[i].ItemStyle.CssClass );
                }

                // get data priority from column
                if ( column is IPriorityColumn )
                {
                    _columnDataPriorities.Add( i, ( (IPriorityColumn)column ).ColumnPriority.ConvertToInt().ToString() );
                }
                else
                {
                    _columnDataPriorities.Add( i, "1" );
                }
            }

            base.OnDataBinding( e );
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
            if ( this.DataSourceAsDataTable != null )
            {
                itemCount = this.DataSourceAsDataTable.Rows.Count;
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

            OnGridRebind( e );
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
                // For each select field that is not bound to a DataSelectedField set it's checkbox/radiobox from
                // the previously posted back values in the columns SelectedKeys property
                foreach ( var col in this.Columns.OfType<SelectField>() )
                {
                    if ( string.IsNullOrWhiteSpace( col.DataSelectedField ) && col.SelectedKeys.Any() )
                    {
                        var colIndex = this.Columns.IndexOf( col ).ToString();
                        CheckBox cbSelect = e.Row.FindControl( "cbSelect_" + colIndex ) as CheckBox;
                        if ( cbSelect != null )
                        {
                            cbSelect.Checked = ( col.SelectedKeys.Contains( this.DataKeys[e.Row.RowIndex].Value ) );
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

            if ( e.Row.RowType == DataControlRowType.Header && this.AllowSorting )
            {
                string asc = SortDirection.Ascending.ToString();
                string desc = SortDirection.Descending.ToString();

                // Remove the sort css classes and add the data priority
                for ( int i = 0; i < e.Row.Cells.Count; i++ )
                {
                    var cell = e.Row.Cells[i];
                    cell.RemoveCssClass( asc );
                    cell.RemoveCssClass( desc );
                    cell.Attributes.Add( "data-priority", _columnDataPriorities[i] );
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
                            e.Row.Cells[this.Columns.IndexOf( dcf )].AddCssClass( sortProperty.Direction.ToString().ToLower() );
                            break;
                        }
                    }
                }
            }

            if ( e.Row.RowType == DataControlRowType.DataRow || e.Row.RowType == DataControlRowType.Footer )
            {
                // add the data priority
                for ( int i = 0; i < e.Row.Cells.Count; i++ )
                {
                    e.Row.Cells[i].Attributes.Add( "data-priority", _columnDataPriorities[i] );
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
                int rowIndex = int.MinValue;
                if ( int.TryParse( e.CommandArgument.ToString(), out rowIndex ) )
                {
                    RowEventArgs a = new RowEventArgs( this.Rows[rowIndex] );
                    OnRowSelected( a );
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
        void pagerTemplate_ItemsPerPageClick( object sender, NumericalEventArgs e )
        {
            var rockPage = this.Page as RockPage;
            if ( rockPage != null )
            {
                rockPage.SetUserPreference( PAGE_SIZE_KEY, e.Number.ToString() );
            }

            this.PageSize = e.Number;
            OnGridRebind( e );
        }

        /// <summary>
        /// Handles the NavigateClick event of the pagerTemplate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.NumericalEventArgs"/> instance containing the event data.</param>
        void pagerTemplate_NavigateClick( object sender, NumericalEventArgs e )
        {
            this.PageIndex = e.Number;
            OnGridRebind( e );
        }

        #endregion

        #region Action Events

        /// <summary>
        /// Handles the MergeClick event of the Actions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void Actions_PersonMergeClick( object sender, EventArgs e )
        {
            int? entitySetId = GetPersonEntitySet( e );
            if ( entitySetId.HasValue )
            {
                Page.Response.Redirect( string.Format( PersonMergePageRoute, entitySetId.Value ), false );
                Context.ApplicationInstance.CompleteRequest();
            }
        }

        /// <summary>
        /// Handles the MergeClick event of the Actions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void Actions_BulkUpdateClick( object sender, EventArgs e )
        {
            int? entitySetId = GetPersonEntitySet( e );
            if ( entitySetId.HasValue )
            {
                Page.Response.Redirect( string.Format( BulkUpdatePageRoute, entitySetId.Value ), false );
                Context.ApplicationInstance.CompleteRequest();
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
                OnGridRebind( e );

                var recipients = GetPersonData();
                if ( recipients.Any() )
                {

                    // Create communication 
                    var rockContext = new RockContext();
                    var service = new Rock.Model.CommunicationService( rockContext );
                    var communication = new Rock.Model.Communication();
                    communication.IsBulkCommunication = true;
                    communication.Status = Model.CommunicationStatus.Transient;
                    CommunicateMergeFields.ForEach( f => communication.AdditionalMergeFields.Add( f.Replace( '.', '_' ) ) );
                    if ( rockPage.CurrentPerson != null )
                    {
                        communication.SenderPersonAliasId = rockPage.CurrentPersonAliasId;
                    }

                    service.Add( communication );

                    var personIds = recipients.Select( r => r.Key ).ToList();
                    var personAliasService = new Rock.Model.PersonAliasService( new Rock.Data.RockContext() );

                    // Get the primary aliases
                    foreach ( var personAlias in personAliasService.Queryable()
                        .Where( p => p.PersonId == p.AliasPersonId && personIds.Contains( p.PersonId ) ) )
                    {
                        var recipient = new Rock.Model.CommunicationRecipient();
                        recipient.PersonAliasId = personAlias.Id;
                        recipient.AdditionalMergeValues = recipients[personAlias.PersonId];
                        communication.Recipients.Add( recipient );
                    }

                    rockContext.SaveChanges();

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
            OnGridRebind( e );
            int? entitySetId = GetEntitySetFromGrid( e );
            if ( entitySetId.HasValue )
            {
                Page.Response.Redirect( string.Format( MergeTemplatePageRoute, entitySetId.Value ), false );
                Context.ApplicationInstance.CompleteRequest();
            }

        }

        /// <summary>
        /// Handles the ExcelExportClick event of the Actions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void Actions_ExcelExportClick( object sender, EventArgs e )
        {
            OnGridRebind( e );

            // create default settings
            string filename = ExportFilename;
            string workSheetName = "Export";
            string title = "Rock Export";

            MemoryStream ms = new MemoryStream();
            ExcelPackage excel = new ExcelPackage( ms );

            // if the grid has a caption customize on it
            if ( this.Caption != null && this.Caption != string.Empty )
            {
                excel.Workbook.Properties.Title = this.Caption;
                workSheetName = this.Caption;
                title = this.Caption;
            }
            else
            {
                excel.Workbook.Properties.Title = "Rock Export";
            }

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

            int rowCounter = 4;
            int columnCounter = 1;

            if ( this.DataSourceAsDataTable != null )
            {
                DataTable data = this.DataSourceAsDataTable;

                // print headings
                foreach ( DataColumn column in data.Columns )
                {
                    worksheet.Cells[3, columnCounter].Value = column.ColumnName.SplitCase();
                    columnCounter++;
                }

                // print data
                foreach ( DataRow row in data.Rows )
                {
                    for ( int i = 0; i < data.Columns.Count; i++ )
                    {
                        worksheet.Cells[rowCounter, i + 1].Value = row[i].ToString().ConvertBrToCrLf();

                        // format background color for alternating rows
                        if ( rowCounter % 2 == 1 )
                        {
                            worksheet.Cells[rowCounter, columnCounter].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            worksheet.Cells[rowCounter, columnCounter].Style.Fill.BackgroundColor.SetColor( Color.FromArgb( 240, 240, 240 ) );
                        }
                    }

                    rowCounter++;
                }
            }
            else
            {
                var oType = GetDataSourceObjectType();

                // get all properties of the objects in the grid
                IList<PropertyInfo> allprops = new List<PropertyInfo>( oType.GetProperties() );
                IList<PropertyInfo> props = new List<PropertyInfo>();

                var gridDataFields = this.Columns.OfType<BoundField>();

                // figure out which properties we can get data from and put those in the grid
                foreach ( PropertyInfo prop in allprops )
                {
                    if ( !gridDataFields.Any( a => a.DataField == prop.Name || a.DataField.StartsWith( prop.Name + "." ) ) && prop.GetGetMethod().IsVirtual )
                    {
                        // skip over virtual properties that aren't shown in the grid since they are probably lazy loaded and it is too late to get them
                        continue;
                    }

                    props.Add( prop );
                }

                // print column headings
                foreach ( PropertyInfo prop in props )
                {
                    var gridDataField = gridDataFields.FirstOrDefault( a => a.DataField == prop.Name || a.DataField.StartsWith( prop.Name + "." ) );
                    if ( gridDataField != null )
                    {
                        worksheet.Cells[3, columnCounter].Value = gridDataField.HeaderText;
                    }
                    else
                    {
                        worksheet.Cells[3, columnCounter].Value = prop.Name.SplitCase();
                    }

                    columnCounter++;
                }

                // Get any defined value columns
                List<DefinedValueField> definedValueFields = this.Columns.OfType<DefinedValueField>().ToList();

                // Get any attribute columns
                List<AttributeField> attributeFields = this.Columns.OfType<AttributeField>().ToList();
                foreach ( var attributeField in attributeFields )
                {
                    worksheet.Cells[3, columnCounter].Value = attributeField.HeaderText;
                    columnCounter++;
                }

                // print data
                int dataIndex = 0;

                IList data = this.DataSourceAsList;
                foreach ( var item in data )
                {
                    columnCounter = 0;
                    foreach ( PropertyInfo prop in props )
                    {
                        columnCounter++;

                        object propValue = prop.GetValue( item, null );
                        string value = "";
                        if ( propValue != null )
                        {
                            var definedValueAttribute = prop.GetCustomAttributes( typeof( Rock.Data.DefinedValueAttribute ), true ).FirstOrDefault();
                            if ( definedValueAttribute != null || definedValueFields.Any( f => f.DataField == prop.Name ) )
                            {
                                int definedValueId = 0;

                                if ( prop.PropertyType == typeof( int? ) )
                                {
                                    definedValueId = (int?)propValue ?? 0;
                                }
                                else if ( prop.PropertyType == typeof( int ) )
                                {
                                    definedValueId = (int)propValue;
                                }

                                if ( definedValueId > 0 )
                                {
                                    var definedValue = DefinedValueCache.Read( definedValueId );
                                    value = definedValue != null ? definedValue.Value : definedValueId.ToString();
                                }
                            }

                            else if ( propValue is IEnumerable<object> )
                            {
                                value = ( propValue as IEnumerable<object> ).ToList().AsDelimited( "," );
                            }
                            else if ( propValue is DateTime? && propValue != null )
                            {
                                DateTime dateTimeValue = ( propValue as DateTime? ).Value;
                                var columnAttribute = prop.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.ColumnAttribute>( true );
                                if ( ( columnAttribute != null && columnAttribute.Name == "Date" ) || prop.Name.EndsWith( "Date" ) )
                                {
                                    value = dateTimeValue.ToShortDateString();
                                }
                                else
                                {
                                    value = dateTimeValue.ToString();
                                }
                            }
                            else
                            {
                                value = propValue.ToString();
                            }
                        }

                        worksheet.Cells[rowCounter, columnCounter].Value = value.ConvertBrToCrLf();

                        // format background color for alternating rows
                        if ( rowCounter % 2 == 1 )
                        {
                            worksheet.Cells[rowCounter, columnCounter].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            worksheet.Cells[rowCounter, columnCounter].Style.Fill.BackgroundColor.SetColor( Color.FromArgb( 240, 240, 240 ) );
                        }

                        if ( propValue is DateTime )
                        {
                            worksheet.Cells[rowCounter, columnCounter].Style.Numberformat.Format = "MM/dd/yyyy hh:mm";
                        }

                    }

                    if ( attributeFields.Any() )
                    {
                        // First check if DataItem has attributes
                        var dataItem = item as Rock.Attribute.IHasAttributes;
                        if ( dataItem == null )
                        {
                            // If the DataItem does not have attributes, check to see if there is an object list
                            if ( ObjectList != null )
                            {
                                // If an object list exists, check to see if the associated object has attributes
                                string key = DataKeys[dataIndex].Value.ToString();
                                if ( !string.IsNullOrWhiteSpace( key ) && ObjectList.ContainsKey( key ) )
                                {
                                    dataItem = ObjectList[key] as Rock.Attribute.IHasAttributes;
                                }
                            }
                        }

                        if ( dataItem != null )
                        {
                            if ( dataItem.Attributes == null )
                            {
                                dataItem.LoadAttributes();
                            }

                            foreach ( var attributeField in attributeFields )
                            {
                                columnCounter++;

                                bool exists = dataItem.Attributes.ContainsKey( attributeField.DataField );
                                if ( exists )
                                {
                                    var attrib = dataItem.Attributes[attributeField.DataField];
                                    string rawValue = dataItem.GetAttributeValue( attributeField.DataField );
                                    string resultHtml = attrib.FieldType.Field.FormatValue( null, rawValue, attrib.QualifierValues, false );
                                    worksheet.Cells[rowCounter, columnCounter].Value = resultHtml;
                                }

                                // format background color for alternating rows
                                if ( rowCounter % 2 == 1 )
                                {
                                    worksheet.Cells[rowCounter, columnCounter].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                    worksheet.Cells[rowCounter, columnCounter].Style.Fill.BackgroundColor.SetColor( Color.FromArgb( 240, 240, 240 ) );
                                }

                            }
                        }
                    }

                    rowCounter++;
                    dataIndex++;
                }
            }

            // format header range
            using ( ExcelRange r = worksheet.Cells[3, 1, 3, columnCounter] )
            {
                r.Style.Font.Bold = true;
                r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                r.Style.Fill.BackgroundColor.SetColor( Color.FromArgb( 223, 223, 223 ) );
                r.Style.Font.Color.SetColor( Color.Black );
                r.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            }

            // format and set title
            worksheet.Cells[1, 1].Value = title;
            using ( ExcelRange r = worksheet.Cells[1, 1, 1, columnCounter] )
            {
                r.Merge = true;
                r.Style.Font.SetFromFont( new Font( "Calibri", 22, FontStyle.Regular ) );
                r.Style.Font.Color.SetColor( Color.White );
                r.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                r.Style.Fill.BackgroundColor.SetColor( Color.FromArgb( 34, 41, 55 ) );

                // set border
                r.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                r.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                r.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                r.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            }

            // TODO: add image to worksheet

            // freeze panes
            worksheet.View.FreezePanes( 3, 1 );

            // autofit columns for all cells
            worksheet.Cells.AutoFitColumns( 0 );

            // add the auto filter / sorting
            worksheet.Cells[3, 1, rowCounter, columnCounter].AutoFilter = true;

            // add alternating highlights

            // set some footer text
            worksheet.HeaderFooter.OddHeader.CenteredText = title;
            worksheet.HeaderFooter.OddFooter.RightAlignedText = string.Format( "Page {0} of {1}", ExcelHeaderFooter.PageNumber, ExcelHeaderFooter.NumberOfPages );

            excel.Save();

            byte[] byteArray = ms.ToArray();

            // send the spreadsheet to the browser
            this.Page.EnableViewState = false;
            this.Page.Response.Clear();
            //this.RockPage.Response.ContentType = "application/vnd.ms-excel";
            this.Page.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            this.Page.Response.AppendHeader( "Content-Disposition", "attachment; filename=" + filename );

            this.Page.Response.Charset = "";
            this.Page.Response.BinaryWrite( byteArray );
            this.Page.Response.Flush();
            this.Page.Response.End();
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
            if ( !string.IsNullOrWhiteSpace( PersonIdField ) )
            {
                var selectField = new SelectField();
                displayColumns.Add( selectField );
                allColumns.Add( selectField );
            }

            foreach ( var property in modelType.GetProperties() )
            {
                // limit to non-virtual methods to prevent lazy loading issues
                var getMethod = property.GetGetMethod();
                if ( !getMethod.IsVirtual )
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

            var columns = new List<DataControlField>();

            // Always add hidden id column
            var idCol = new BoundField();
            idCol.DataField = "Id";
            idCol.Visible = false;
            columns.Add( idCol );

            columns.AddRange( displayColumns.Count > 0 ? displayColumns : allColumns );

            if ( columns.Count == 1 )
            {
                // if the only column is the Id column, show it
                idCol.HeaderText = "Id";
                idCol.Visible = true;
            }

            return columns;
        }

        private Dictionary<int, Dictionary<string, string>> GetPersonData()
        {
            var personData = new Dictionary<int, Dictionary<string, string>>();

            if ( this.PersonIdField != null )
            {
                // The ToList() is potentially needed for Linq cases.
                var keysSelected = SelectedKeys.ToList();
                string dataKeyColumn = this.DataKeyNames.FirstOrDefault();

                if ( !string.IsNullOrWhiteSpace( dataKeyColumn ) && this.DataSourceAsDataTable != null )
                {
                    DataTable data = this.DataSourceAsDataTable;

                    foreach ( DataRow row in data.Rows )
                    {
                        if ( !keysSelected.Any() || keysSelected.Contains( row[dataKeyColumn] ) )
                        {
                            int? personId = null;
                            var mergeValues = new Dictionary<string, string>();
                            for ( int i = 0; i < data.Columns.Count; i++ )
                            {
                                if ( data.Columns[i].ColumnName == this.PersonIdField )
                                {
                                    personId = row[i] as int?;
                                }

                                if ( CommunicateMergeFields.Contains( data.Columns[i].ColumnName ) )
                                {
                                    mergeValues.Add( data.Columns[i].ColumnName, row[i].ToString() );
                                }
                            }

                            // Add the personId if none are selected or if it's one of the selected items.
                            if ( personId.HasValue )
                            {
                                personData.AddOrIgnore( personId.Value, mergeValues );
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

                        PropertyInfo personIdProp = oType.GetProperty( this.PersonIdField );
                        PropertyInfo idProp = oType.GetProperty( dataKeyColumn );

                        foreach ( var item in data )
                        {
                            if ( personIdProp == null )
                            {
                                personIdProp = item.GetType().GetProperty( this.PersonIdField );
                            }
                            if ( idProp == null )
                            {
                                idProp = item.GetType().GetProperty( dataKeyColumn );
                            }

                            if ( personIdProp != null && idProp != null )
                            {
                                int personId = (int)personIdProp.GetValue( item, null );
                                int id = (int)idProp.GetValue( item, null );

                                // Add the personId if none are selected or if it's one of the selected items.
                                if ( !keysSelected.Any() || keysSelected.Contains( id ) )
                                {
                                    var mergeValues = new Dictionary<string, string>();
                                    foreach ( string mergeField in CommunicateMergeFields )
                                    {
                                        object obj = item.GetPropertyValue( mergeField );
                                        if ( obj != null )
                                        {
                                            mergeValues.Add( mergeField.Replace( '.', '_' ), obj.ToString() );
                                        }
                                    }

                                    personData.AddOrIgnore( personId, mergeValues );
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
            OnGridRebind( e );

            var keys = GetPersonData();
            if ( keys.Any() )
            {
                var entitySet = new Rock.Model.EntitySet();
                entitySet.EntityTypeId = Rock.Web.Cache.EntityTypeCache.Read<Rock.Model.Person>().Id;
                entitySet.ExpireDateTime = RockDateTime.Now.AddMinutes( 5 );

                foreach ( var key in keys )
                {
                    try
                    {
                        var item = new Rock.Model.EntitySetItem();
                        item.EntityId = (int)key.Key;
                        entitySet.Items.Add( item );
                    }
                    catch { }
                }

                if ( entitySet.Items.Any() )
                {
                    var rockContext = new RockContext();
                    var service = new Rock.Model.EntitySetService( rockContext );
                    service.Add( entitySet );
                    rockContext.SaveChanges();
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
                    return ( DataSource as IList );
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

                    return (DataTable)this.DataSource;
                }
                else if ( this.DataSource is DataView )
                {
                    return ( (DataView)this.DataSource ).Table;
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
        /// Gets the entity set from grid if it's datasource is a data table.
        /// </summary>
        /// <returns></returns>
        private int? GetEntitySetFromGridSourceDataTable()
        {
            var entitySet = new Rock.Model.EntitySet();

            // the datasource is a DataTable so there isn't an EntityTypeId
            entitySet.EntityTypeId = null;

            entitySet.ExpireDateTime = RockDateTime.Now.AddMinutes( 5 );

            int itemOrder = 0;
            foreach ( var row in this.DataSourceAsDataTable.Rows.OfType<DataRow>() )
            {
                try
                {
                    var item = new Rock.Model.EntitySetItem();

                    // the datasource is a DataTable (not an Entity), so just set it to zero.  The entire datarow will be put into AdditionalMergeValues
                    item.EntityId = 0;

                    item.Order = itemOrder++;
                    item.AdditionalMergeValues = new Dictionary<string, object>();
                    foreach ( var col in this.DataSourceAsDataTable.Columns.OfType<DataColumn>() )
                    {
                        item.AdditionalMergeValues.Add( col.ColumnName, row[col] );
                    }

                    entitySet.Items.Add( item );
                }
                catch { }
            }

            if ( entitySet.Items.Any() )
            {
                var rockContext = new RockContext();
                var service = new Rock.Model.EntitySetService( rockContext );
                service.Add( entitySet );
                rockContext.SaveChanges();
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
            string dataKeyColumn = this.DataKeyNames.FirstOrDefault() ?? "Id";
            PropertyInfo idProp = dataSourceObjectType.GetProperty( dataKeyColumn );

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

                    var entityTypeCache = Rock.Web.Cache.EntityTypeCache.Read( entityType, false );
                    if ( entityTypeCache != null )
                    {
                        entityTypeId = entityTypeCache.Id;
                    }
                }
            }



            // first try to get the SelectedKeys from the SelectField (if there is one)
            var selectedKeys = this.SelectedKeys.Select( a => a as int? ).Where( a => a.HasValue ).Select( a => a.Value ).Distinct().ToList();
            if ( selectedKeys == null || !selectedKeys.Any() )
            {
                if ( entityTypeId.HasValue && dataSourceObjectType is IEntity )
                {
                    // we know this is an IEntity Type so the datakey is Id
                    selectedKeys = ( this.DataSourceAsList ).OfType<IEntity>().Select( a => a.Id ).Distinct().ToList();
                }
                else
                {
                    // this is something else, so try to figure it out from dataKeyColumn
                    selectedKeys = new List<int>();

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
                var dataSourceObjectTypeEntityType = EntityTypeCache.Read( dataSourceObjectType, false );
                if ( dataSourceObjectTypeEntityType != null && dataSourceObjectTypeEntityType.Id == entityTypeId )
                {
                    // the entityType and the Datasource type are the same, so no additional merge fields 
                }
                else
                {
                    // the entityType and the Datasource type are different, so figure out the extra properties and put them into AdditionalMergeFields
                    var entityType = EntityTypeCache.Read( entityTypeId.Value ).GetEntityType();
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


            var gridDataFields = this.Columns.OfType<BoundField>();

            Dictionary<int, Dictionary<string, object>> itemMergeFieldsList = new Dictionary<int, Dictionary<string, object>>();
            if ( additionalMergeProperties != null && additionalMergeProperties.Any() && idProp != null )
            {
                foreach ( var item in this.DataSourceAsList )
                {
                    var idVal = idProp.GetValue( item ) as int?;
                    if ( idVal.HasValue && selectedKeys.Contains( idVal.Value ) )
                    {
                        var mergeFields = new Dictionary<string, object>();
                        foreach ( var mergeProperty in additionalMergeProperties )
                        {
                            var objValue = mergeProperty.GetValue( item );

                            var dataField = gridDataFields.FirstOrDefault( a => a.DataField == mergeProperty.Name );
                            string mergeFieldKey;
                            if ( dataField != null && !string.IsNullOrWhiteSpace( dataField.HeaderText ) )
                            {
                                mergeFieldKey = dataField.HeaderText.RemoveSpecialCharacters().Replace( " ", "_" );
                            }
                            else
                            {
                                mergeFieldKey = mergeProperty.Name;
                            }

                            mergeFields.AddOrIgnore( mergeFieldKey, objValue );
                        }

                        itemMergeFieldsList.Add( idVal.Value, mergeFields );
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

                    entitySet.Items.Add( item );
                }
                catch { }
            }

            if ( entitySet.Items.Any() )
            {
                var rockContext = new RockContext();
                var service = new Rock.Model.EntitySetService( rockContext );
                service.Add( entitySet );
                rockContext.SaveChanges();
                return entitySet.Id;
            }

            return null;
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

                int oldIndex = 0;
                if ( !int.TryParse( parms[1], out oldIndex ) )
                {
                    oldIndex = 0;
                }

                int newIndex = 0;
                if ( !int.TryParse( parms[2], out newIndex ) )
                {
                    newIndex = 0;
                }

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
        /// Raises the <see cref="E:GridRebind"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnGridRebind( EventArgs e )
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

            if ( baseType == typeof( Boolean ) || baseType == typeof( Boolean? ) )
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
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    public delegate void GridRebindEventHandler( object sender, EventArgs e );

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
    internal class NumericalEventArgs : EventArgs
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

    #endregion

    #region Helper Classes

    /// <summary>
    /// JSON Result  
    /// </summary>
    internal class JsonResult
    {
        public string Action { get; set; }
        public bool Cancel { get; set; }
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
        //Literal lStatus;
        private bool IsDisposed;
        HtmlGenericControl NavigationPanel;

        HtmlGenericContainer[] PageLinkListItem = new HtmlGenericContainer[12];
        LinkButton[] PageLink = new LinkButton[12];

        Literal itemCountDisplay;

        HtmlGenericContainer[] ItemLinkListItem = new HtmlGenericContainer[3];
        LinkButton[] ItemLink = new LinkButton[3];

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
                ItemLinkListItem[i] = new HtmlGenericContainer( "li" );
                ulSizeOptions.Controls.Add( ItemLinkListItem[i] );

                ItemLink[i] = new LinkButton();
                ItemLinkListItem[i].Controls.Add( ItemLink[i] );
                ItemLink[i].CausesValidation = false;
                ItemLink[i].Click += new EventHandler( lbItems_Click );
            }

            ItemLink[0].Text = "50";
            ItemLink[1].Text = "500";
            ItemLink[2].Text = "5,000";

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
                PageLinkListItem[i] = new HtmlGenericContainer( "li" );
                NavigationPanel.Controls.Add( PageLinkListItem[i] );

                PageLink[i] = new LinkButton();
                PageLinkListItem[i].Controls.Add( PageLink[i] );
                PageLink[i].Click += new EventHandler( lbPage_Click );
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
                    int totalGroups = (int)( ( pageCount - 1 ) / 10 );
                    int currentGroup = (int)( pageIndex / 10 );

                    int prevPageIndex = 0;
                    if ( pageIndex <= 0 )
                    {
                        PageLinkListItem[0].Attributes["class"] = "prev disabled";
                        PageLink[0].Enabled = false;
                    }
                    else
                    {
                        prevPageIndex = pageIndex - ( currentGroup > 0 ? 10 : 1 );
                        PageLinkListItem[0].Attributes["class"] = "prev";
                        PageLink[0].Enabled = true;
                    }
                    PageLink[0].Attributes["page-index"] = prevPageIndex.ToString();

                    int nextPageIndex = pageIndex;
                    if ( pageIndex >= pageCount - 1 )
                    {
                        PageLinkListItem[PageLinkListItem.Length - 1].Attributes["class"] = "next disabled";
                        PageLink[PageLinkListItem.Length - 1].Enabled = false;
                    }
                    else
                    {
                        nextPageIndex = pageIndex + ( currentGroup < totalGroups ? 10 : 1 );
                        PageLinkListItem[PageLinkListItem.Length - 1].Attributes["class"] = "next";
                        PageLink[PageLinkListItem.Length - 1].Enabled = true;
                    }
                    PageLink[PageLinkListItem.Length - 1].Attributes["page-index"] = nextPageIndex.ToString();


                    NavigationPanel.Visible = true;
                    for ( int i = 1; i < PageLink.Length - 1; i++ )
                    {
                        int currentPage = currentGroup + ( i - 1 );

                        HtmlGenericControl li = PageLinkListItem[i];
                        LinkButton lb = PageLink[i];

                        if ( currentPage < pageCount )
                        {
                            li.Attributes["class"] = currentPage == pageIndex ? "active" : "";
                            li.Visible = true;

                            lb.Text = ( currentPage + 1 ).ToString( "N0" );
                            lb.Attributes["page-index"] = currentPage.ToString();
                            lb.Visible = true;
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
                    ItemLinkListItem[i].Attributes["class"] = ItemLink[i].Text == pageSizeValue ? "active" : "";
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the lbPage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        void lbPage_Click( object sender, EventArgs e )
        {
            if ( NavigateClick != null )
            {
                LinkButton lbPage = sender as LinkButton;
                if ( lbPage != null )
                {
                    int pageIndex = 0;
                    if ( Int32.TryParse( lbPage.Attributes["page-index"], out pageIndex ) )
                    {
                        NumericalEventArgs eventArgs = new NumericalEventArgs( pageIndex );
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
        void lbItems_Click( object sender, EventArgs e )
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
    /// Column Prioritiy Values
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

    #endregion

}