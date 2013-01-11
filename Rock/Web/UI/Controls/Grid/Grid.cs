//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using OfficeOpenXml;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Rock Grid Control
    /// </summary>
    [ToolboxData( "<{0}:Grid runat=server></{0}:Grid>" )]
    public class Grid : System.Web.UI.WebControls.GridView, IPostBackEventHandler
    {
        const int ALL_ITEMS_SIZE = 1000000;

        private Table _table;
        private GridViewRow _actionRow;
        private GridActions _gridActions;
        private const string DefaultEmptyDataText = "No Results Found";
        private Dictionary<int, string> DataBoundColumns = new Dictionary<int, string>();

        #region Properties

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
            get
            {
                object showActionRow = this.ViewState["ShowActionRow"];
                return ( ( showActionRow == null ) || ( (bool)showActionRow ) );
            }

            set
            {
                bool showActionRow = this.ShowActionRow;
                if ( value != showActionRow )
                {
                    this.ViewState["ShowActionRow"] = value;
                }
            }
        }

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
            get
            {
                object deleteEnabled = this.ViewState["IsDeleteEnabled"];
                if ( deleteEnabled == null )
                {
                    return true;
                }
                else
                {
                    return (bool)deleteEnabled;
                }

            }

            set
            {
                this.ViewState["IsDeleteEnabled"] = value;
            }
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
            get
            {
                object showConfirmDeleteDialog = this.ViewState["ShowConfirmDeleteDialog"];
                if ( showConfirmDeleteDialog == null )
                {
                    return true;
                }
                else
                {
                    return (bool)showConfirmDeleteDialog;
                }

            }

            set
            {
                this.ViewState["ShowConfirmDeleteDialog"] = value;
            }

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
                object rowItemText = this.ViewState["RowItemText"];
                if ( string.IsNullOrWhiteSpace( rowItemText as string ) && DataSource != null )
                {
                    Type dataSourceType = DataSource.GetType();
                    Type itemType = dataSourceType.GetGenericArguments()[0];
                    if ( itemType != null )
                    {
                        rowItemText = itemType.GetFriendlyTypeName().ToLower();
                    }

                }
                return ( rowItemText as string );
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
                if ( string.IsNullOrWhiteSpace( result ) || result.Equals( DefaultEmptyDataText ) )
                {
                    if ( DataSource != null )
                    {
                        Type dataSourceType = DataSource.GetType();
                        Type itemType = dataSourceType.GetGenericArguments()[0];
                        if ( itemType != null )
                        {
                            result = string.Format( "No {0} Found", itemType.GetFriendlyTypeName().Pluralize() );
                        }
                    }
                }
                return result;
            }
            set
            {
                base.EmptyDataText = value;
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
            get
            {
                object hideDeleteButtonForIsSystem = this.ViewState["HideDeleteButtonForIsSystem"];
                if ( hideDeleteButtonForIsSystem != null )
                {
                    return (bool)hideDeleteButtonForIsSystem;
                }
                else
                {
                    // default to true
                    return true;
                }
            }

            set
            {
                this.ViewState["HideDeleteButtonForIsSystem"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [row click enabled].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [row click enabled]; otherwise, <c>false</c>.
        /// </value>
        public virtual bool RowClickEnabled
        {
            get
            {
                object rowClickEnabled = this.ViewState["RowClickEnabled"];
                if ( rowClickEnabled != null )
                {
                    return (bool)rowClickEnabled;
                }
                else
                {
                    // default to true
                    return true;
                }
            }

            set
            {
                this.ViewState["RowClickEnabled"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the export to excel action should be displayed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the eport to excel action should be displayed; otherwise, <c>false</c>.
        /// </value>
        [
        Category( "Appearance" ),
        DefaultValue( true ),
        Description( "Show Action Export to Excel" )
        ]
        public virtual bool ShowActionExcelExport
        {
            get
            {
                object showActionExcelExport = this.ViewState["ShowActionExcelExport"];
                return ( ( showActionExcelExport == null ) || ( (bool)showActionExcelExport ) );
            }

            set
            {
                bool showActionExcelExport = this.ShowActionExcelExport;
                if ( value != showActionExcelExport )
                {
                    this.ViewState["ShowActionExcelExport"] = value;
                }
            }
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

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Grid"/> class.
        /// </summary>
        public Grid()
        {
            base.CssClass = "grid-table table";
            base.AutoGenerateColumns = false;
            base.RowStyle.HorizontalAlign = System.Web.UI.WebControls.HorizontalAlign.Left;
            base.HeaderStyle.HorizontalAlign = System.Web.UI.WebControls.HorizontalAlign.Left;
            base.SelectedRowStyle.HorizontalAlign = System.Web.UI.WebControls.HorizontalAlign.Left;

            this.ShowHeaderWhenEmpty = true;
            this.EmptyDataText = DefaultEmptyDataText;

            // hack to turn off style="border-collapse: collapse"
            base.GridLines = GridLines.None;
            base.CellSpacing = -1;

            base.AllowPaging = true;
            base.PageSize = 25;
            base.PageIndex = 0;

            _gridActions = new GridActions( this );
        }

        #endregion

        #region Overridden Methods

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

            this.Sorting += new GridViewSortEventHandler( Grid_Sorting );
            this.Actions.ExcelExportClick += new EventHandler( Actions_ExcelExportClick );

            this.Actions.IsExcelExportEnabled = this.ShowActionExcelExport;

            base.OnInit( e );
        }

        /// <summary>
        /// Handles the ExcelExportClick event of the Actions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        void Actions_ExcelExportClick( object sender, EventArgs e )
        {
            OnGridRebind( e );

            // create default settings
            string filename = "export.xlsx";
            string workSheetName = "Export";
            string title = "Rock ChMS Export";

            MemoryStream ms = new MemoryStream();
            ExcelPackage excel = new ExcelPackage( ms );

            // if the grid has a caption customize on it
            if ( this.Caption != null && this.Caption != string.Empty )
            {
                excel.Workbook.Properties.Title = this.Caption;
                workSheetName = this.Caption;
                filename = this.Caption.Replace( " ", "" ) + ".xlsx";
                title = this.Caption;
            }
            else
            {
                excel.Workbook.Properties.Title = "Rock ChMS Export";
            }

            // add author info
            Rock.Model.UserLogin userLogin = Rock.Model.UserLoginService.GetCurrentUser();
            if ( userLogin != null )
            {
                excel.Workbook.Properties.Author = userLogin.Person.FullName;
            }
            else
            {
                excel.Workbook.Properties.Author = "Rock ChMS";
            }

            // add the page that created this
            excel.Workbook.Properties.SetCustomPropertyValue( "Source", this.Page.Request.Url.OriginalString );

            ExcelWorksheet worksheet = excel.Workbook.Worksheets.Add( workSheetName );

            //// write data to worksheet there are three supported data sources
            //// DataTables, DataViews and ILists

            int rowCounter = 4;
            int columnCounter = 1;

            if ( this.DataSource is DataTable || this.DataSource is DataView )
            {
                DataTable data = null;

                if ( this.DataSource is DataTable )
                {
                    data = (DataTable)this.DataSource;
                }
                else if ( this.DataSource is DataView )
                {
                    data = ( (DataView)this.DataSource ).Table;
                }

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
                        worksheet.Cells[rowCounter, i].Value = row[i].ToString();

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
                // get access to the List<> and its properties
                IList data = (IList)this.DataSource;
                Type oType = data.GetType().GetProperty( "Item" ).PropertyType;
                IList<PropertyInfo> props = new List<PropertyInfo>( oType.GetProperties() );

                // print column headings
                foreach ( PropertyInfo prop in props )
                {
                    worksheet.Cells[3, columnCounter].Value = prop.Name.SplitCase();
                    columnCounter++;
                }

                // print data
                foreach ( var item in data )
                {
                    columnCounter = 1;
                    foreach ( PropertyInfo prop in props )
                    {
                        object propValue = prop.GetValue( item, null );

                        string value = "";
                        if ( propValue != null )
                        {
                            value = propValue.ToString();
                        }

                        worksheet.Cells[rowCounter, columnCounter].Value = value;

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

                        columnCounter++;
                    }

                    rowCounter++;
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
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnPreRender( EventArgs e )
        {
            base.OnPreRender( e );

            //Rock.Web.UI.RockPage.AddCSSLink( Page, "~/CSS/grid.css" );

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
        /// Raises the <see cref="E:System.Web.UI.WebControls.BaseDataBoundControl.DataBound"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnDataBound( EventArgs e )
        {
            if ( DisplayType == GridDisplayType.Light )
            {
                this.AllowPaging = false;
                this.AllowSorting = false;
                this.Actions.IsExcelExportEnabled = false;

                this.RemoveCssClass( "table-bordered" );
                this.RemoveCssClass( "table-striped" );
                this.RemoveCssClass( "table-hover" );
                this.RemoveCssClass( "table-full" );
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
                this.AddCssClass( "table-full" );
            }

            base.OnDataBound( e );

            PagerTemplate pagerTemplate = this.PagerTemplate as PagerTemplate;
            if ( PagerTemplate != null )
            {
                pagerTemplate.SetNavigation( this.PageCount, this.PageIndex, this.PageSize );
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

                // Remove the sort css classes
                foreach ( TableCell cell in e.Row.Cells )
                {
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
                            e.Row.Cells[this.Columns.IndexOf( dcf )].AddCssClass( sortProperty.Direction.ToString().ToLower() );
                            break;
                        }
                    }
                }
            }

            if ( e.Row.DataItem != null && this.DataKeys != null && this.DataKeys.Count > 0 )
            {
                object dataKey = this.DataKeys[e.Row.RowIndex].Value as object;
                if ( dataKey != null )
                {
                    string key = dataKey.ToString();
                    e.Row.Attributes.Add( "datakey", key );
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.WebControls.GridView.RowCreated" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Web.UI.WebControls.GridViewRowEventArgs" /> that contains event data.</param>
        protected override void OnRowCreated( GridViewRowEventArgs e )
        {
            base.OnRowCreated( e );

            if ( RowSelected != null && e.Row.RowType == DataControlRowType.DataRow )
            {
                string clickUrl = Page.ClientScript.GetPostBackClientHyperlink( this, "RowSelected$" + e.Row.RowIndex );

                for ( int i = 0; i < e.Row.Cells.Count; i++ )
                {
                    if ( DataBoundColumns.ContainsKey( i ) )
                    {
                        var cell = e.Row.Cells[i];
                        cell.AddCssClass( DataBoundColumns[i] );
                        cell.AddCssClass( "grid-select-cell" );
                        cell.Attributes["onclick"] = clickUrl;
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new child table.
        /// </summary>
        /// <returns>
        /// Always returns a new <see cref="T:System.Web.UI.WebControls.Table"/> that represents the child table.
        /// </returns>
        protected override Table CreateChildTable()
        {
            _table = base.CreateChildTable();

            DataBoundColumns = new Dictionary<int, string>();
            for ( int i = 0; i < this.Columns.Count; i++ )
            {
                BoundField column = this.Columns[i] as BoundField;
                if ( column != null )
                {
                    DataBoundColumns.Add( i, column.ItemStyle.CssClass );
                }
            }

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
                }

                _actionRow = base.CreateRow( -1, -1, DataControlRowType.Footer, DataControlRowState.Normal );
                _table.Rows.Add( _actionRow );

                TableCell cell = new TableCell();
                cell.ColumnSpan = this.Columns.Count;
                cell.CssClass = "grid-footer";
                _actionRow.Cells.Add( cell );

                cell.Controls.Add( _gridActions );

                if ( !this.ShowActionRow )
                {
                    _actionRow.Visible = false;
                }
            }

            return result;
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

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.WebControls.GridView.RowCommand" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Web.UI.WebControls.GridViewCommandEventArgs" /> that contains event data.</param>
        protected override void OnRowCommand( GridViewCommandEventArgs e )
        {
        	base.OnRowCommand( e );
        	
            if ( e.CommandName == "RowSelected" )
            {
                int rowIndex = Int32.Parse( e.CommandArgument.ToString() );
                RowEventArgs a = new RowEventArgs( this.Rows[rowIndex] );
                OnRowSelected( a );
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Handles the ItemsPerPageClick event of the pagerTemplate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.NumericalEventArgs"/> instance containing the event data.</param>
        void pagerTemplate_ItemsPerPageClick( object sender, NumericalEventArgs e )
        {
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

        #region Events

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
        /// Gets or sets the property name
        /// </summary>
        /// <value>
        /// The property.
        /// </value>
        public string Property { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SortProperty"/> class.
        /// </summary>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.GridViewSortEventArgs"/> instance containing the event data.</param>
        public SortProperty( GridViewSortEventArgs e )
        {
            Direction = e.SortDirection;
            Property = e.SortExpression;
        }
    }

    #endregion

    #region Templates

    /// <summary>
    /// Template used for the pager row in the <see cref="Grid"/> control
    /// </summary>
    internal class PagerTemplate : ITemplate, IDisposable
    {
        const int ALL_ITEMS_SIZE = 1000000;

        //Literal lStatus;
        private bool IsDisposed;
        HtmlGenericControl NavigationPanel;

        HtmlGenericContainer[] PageLinkListItem = new HtmlGenericContainer[12];
        LinkButton[] PageLink = new LinkButton[12];

        HtmlGenericContainer[] ItemLinkListItem = new HtmlGenericContainer[4];
        LinkButton[] ItemLink = new LinkButton[4];

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
            HtmlGenericControl divPagination = new HtmlGenericControl( "div" );
            divPagination.Attributes.Add( "class", "pagination" );
            container.Controls.Add( divPagination );

            // RockPage Status
            //HtmlGenericControl divStatus = new HtmlGenericControl( "div" );
            //divStatus.Attributes.Add( "class", "page-status" );
            //divPagination.Controls.Add( divStatus );

            //lStatus = new Literal(); 
            //divStatus.Controls.Add( lStatus );

            // Pagination
            NavigationPanel = new HtmlGenericControl( "div" );
            NavigationPanel.Attributes.Add( "class", "page-navigation" );
            divPagination.Controls.Add( NavigationPanel );

            HtmlGenericControl ulNavigation = new HtmlGenericControl( "ul" );
            NavigationPanel.Controls.Add( ulNavigation );

            for ( var i = 0; i < PageLinkListItem.Length; i++ )
            {
                PageLinkListItem[i] = new HtmlGenericContainer( "li" );
                ulNavigation.Controls.Add( PageLinkListItem[i] );

                PageLink[i] = new LinkButton();
                PageLinkListItem[i].Controls.Add( PageLink[i] );
                PageLink[i].Click += new EventHandler( lbPage_Click );
            }

            PageLink[0].Text = "&larr; Previous";
            PageLink[PageLinkListItem.Length - 1].Text = "Next &rarr;";

            // Items Per RockPage
            HtmlGenericControl divSize = new HtmlGenericControl( "div" );
            divSize.Attributes.Add( "class", "page-size" );
            divPagination.Controls.Add( divSize );

            //Label lblPageSize = new Label();
            //lblPageSize.Text = "Items per page:";
            //divSize.Controls.Add( lblPageSize );

            HtmlGenericControl divSizeOptions = new HtmlGenericControl( "div" );
            divSizeOptions.Attributes.Add( "class", "page-size-options" );
            divSize.Controls.Add( divSizeOptions );

            HtmlGenericControl ulSizeOptions = new HtmlGenericControl( "ul" );
            divSizeOptions.Controls.Add( ulSizeOptions );

            for ( int i = 0; i < ItemLinkListItem.Length; i++ )
            {
                ItemLinkListItem[i] = new HtmlGenericContainer( "li" );
                ulSizeOptions.Controls.Add( ItemLinkListItem[i] );

                ItemLink[i] = new LinkButton();
                ItemLinkListItem[i].Controls.Add( ItemLink[i] );
                ItemLink[i].Click += new EventHandler( lbItems_Click );
            }

            ItemLink[0].Text = "25";
            ItemLink[1].Text = "100";
            ItemLink[2].Text = "1,000";
            ItemLink[3].Text = "All";
        }

        /// <summary>
        /// Set the RockPage Navigation Display
        /// </summary>
        /// <param name="pageCount">The number of total pages</param>
        /// <param name="pageIndex">The current page index</param>
        /// <param name="pageSize">The number of items on each page</param>
        public void SetNavigation( int pageCount, int pageIndex, int pageSize )
        {
            //// Set status
            //if (lStatus != null)
            //    lStatus.Text = string.Format( "RockPage {0:N0} of {1:N0}", pageIndex+1, pageCount );

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

            // Set page size controls
            if ( ItemLinkListItem[0] != null )
            {
                string pageSizeValue = pageSize == ALL_ITEMS_SIZE ? "All" : pageSize.ToString( "N0" );
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
                int itemsPerPage = ALL_ITEMS_SIZE;

                switch ( lbItems.Text )
                {
                    case "25":
                        itemsPerPage = 25;
                        break;
                    case "100":
                        itemsPerPage = 100;
                        break;
                    case "1,000":
                        itemsPerPage = 1000;
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


    #endregion
}