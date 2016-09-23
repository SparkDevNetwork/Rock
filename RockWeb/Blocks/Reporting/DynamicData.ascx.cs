﻿// <copyright>
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Reporting
{
    /// <summary>
    /// Block to display dynamic report, html, xml, or transformed xml based on a SQL query or stored procedure.
    /// </summary>
    [DisplayName( "Dynamic Data" )]
    [Category( "Reporting" )]
    [Description( "Block to display dynamic report, html, xml, or transformed xml based on a SQL query or stored procedure." )]

    // Block Properties
    [BooleanField( "Update Page", "If True, provides fields for updating the parent page's Name and Description", true, "", 0 )]

    // Custom Settings
    [CodeEditorField( "Query", "The query to execute. Note that if you are providing SQL you can add items from the query string using Lava like {{ QueryParmName }}.", CodeEditorMode.Sql, CodeEditorTheme.Rock, 400, false, "", "CustomSetting" )]
    [TextField( "Query Params", "Parameters to pass to query", false, "", "CustomSetting" )]
    [BooleanField( "Stored Procedure", "Is the query a stored procedure?", false, "CustomSetting" )]
    [TextField( "Url Mask", "The Url to redirect to when a row is clicked", false, "", "CustomSetting" )]
    [BooleanField( "Show Columns", "Should the 'Columns' specified below be the only ones shown (vs. the only ones hidden)", false, "CustomSetting" )]
    [TextField( "Columns", "The columns to hide or show", false, "", "CustomSetting" )]
    [CodeEditorField( "Formatted Output", "Optional formatting to apply to the returned results.  If left blank, a grid will be displayed. Example: {% for row in rows %} {{ row.FirstName }}<br/> {% endfor %}",
        CodeEditorMode.Lava, CodeEditorTheme.Rock, 200, false, "", "CustomSetting" )]
    [BooleanField( "Person Report", "Is this report a list of people?", false, "CustomSetting" )]
    [BooleanField( "Show Communicate", "Show Communicate button in grid footer?", true, "CustomSetting" )]
    [BooleanField( "Show Merge Person", "Show Merge Person button in grid footer?", true, "CustomSetting" )]
    [BooleanField( "Show Bulk Update", "Show Bulk Update button in grid footer?", true, "CustomSetting" )]
    [BooleanField( "Show Excel Export", "Show Export to Excel button in grid footer?", true, "CustomSetting" )]
    [BooleanField( "Show Merge Template", "Show Export to Merge Template button in grid footer?", true, "CustomSetting" )]
    [IntegerField( "Timeout", "The amount of time in xxx to allow the query to run before timing out.", false, 30, Category = "CustomSetting" )]
    [TextField( "Merge Fields", "Any fields to make available as merge fields for any new communications", false, "", "CustomSetting" )]
    [CodeEditorField( "Page Title Lava", "Optional Lava for setting the page title. If nothing is provided then the page's title will be used.",
        CodeEditorMode.Lava, CodeEditorTheme.Rock, 200, false, "", "CustomSetting" )]
    [BooleanField( "Paneled Grid", "Add the 'grid-panel' class to the grid to allow it to fit nicely in a block.", false, "Advanced" )]
    [BooleanField( "Show Grid Filter", "Show filtering controls that are dynamically generated to match the columns of the dynamic data.", true, "CustomSetting" )]
    public partial class DynamicData : RockBlockCustomSettings
    {
        #region Fields

        private Dictionary<int, string> _sortExpressions = new Dictionary<int, string>();
        private bool _updatePage = true;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the GridFilter.
        /// </summary>
        public GridFilter GridFilter { get; set; }
        public Dictionary<Control, string> GridFilterColumnLookup;

        /// <summary>
        /// Gets the settings tool tip.
        /// </summary>
        /// <value>
        /// The settings tool tip.
        /// </value>
        public override string SettingsToolTip
        {
            get
            {
                return "Edit Criteria";
            }
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            BlockUpdated += DynamicData_BlockUpdated;
            AddConfigurationUpdateTrigger( upnlContent );

            BuildControls( !Page.IsPostBack );

            _updatePage = GetAttributeValue( "UpdatePage" ).AsBoolean( true );
        }

        #endregion

        #region Events

        /// <summary>
        /// Displays the text of the current filters
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            DateTime startDateTimeValue;
            DateTime endDateTimeValue;

            if ( DateRangePicker.TryParse( e.Value, out startDateTimeValue, out endDateTimeValue ) )
            {
                e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
            }
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the fDevice control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void ApplyFilterClick( object sender, EventArgs e )
        {
            GridFilter.DeleteUserPreferences();

            foreach ( Control control in GridFilter.Controls )
            {
                var key = control.ID;
                string value = null;
                string name = null;

                if ( control is DateRangePicker )
                {
                    var dateRangePicker = control as DateRangePicker;

                    if ( dateRangePicker.UpperValue.HasValue && dateRangePicker.LowerValue.HasValue )
                    {
                        value = dateRangePicker.DelimitedValues;
                        name = key.Remove( 0, 3 ).SplitCase();
                    }
                }
                else if ( control is RockDropDownList )
                {
                    var dropDownList = control as RockDropDownList;
                    value = dropDownList.SelectedValue;
                    name = key.Remove( 0, 3 ).SplitCase();
                }
                else if ( control is RockTextBox )
                {
                    var textBox = control as RockTextBox;
                    value = textBox.Text;
                    name = key.Remove( 0, 2 ).SplitCase();
                }

                GridFilter.SaveUserPreference( key, name, value );
            }

            gReport_GridRebind( sender, e );
        }

        /// <summary>
        /// Handles the BlockUpdated event of the DynamicData control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void DynamicData_BlockUpdated( object sender, EventArgs e )
        {
            BuildControls( true );
        }

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            if ( _updatePage )
            {
                var pageCache = PageCache.Read( RockPage.PageId );
                if ( pageCache != null &&
                    ( pageCache.PageTitle != tbName.Text || pageCache.Description != tbDesc.Text ) )
                {
                    var rockContext = new RockContext();
                    var service = new PageService( rockContext );
                    var page = service.Get( pageCache.Id );
                    page.InternalName = tbName.Text;
                    page.PageTitle = tbName.Text;
                    page.BrowserTitle = tbName.Text;
                    page.Description = tbDesc.Text;
                    rockContext.SaveChanges();

                    Rock.Web.Cache.PageCache.Flush( page.Id );
                    pageCache = PageCache.Read( RockPage.PageId );

                    var breadCrumb = RockPage.BreadCrumbs.Where( c => c.Url == RockPage.PageReference.BuildUrl() ).FirstOrDefault();
                    if ( breadCrumb != null )
                    {
                        breadCrumb.Name = pageCache.BreadCrumbText;
                    }
                }
            }

            SetAttributeValue( "Query", ceQuery.Text );
            SetAttributeValue( "Timeout", nbTimeout.Text );
            SetAttributeValue( "StoredProcedure", cbStoredProcedure.Checked.ToString() );
            SetAttributeValue( "QueryParams", tbParams.Text );
            SetAttributeValue( "UrlMask", tbUrlMask.Text );
            SetAttributeValue( "Columns", tbColumns.Text );
            SetAttributeValue( "ShowColumns", ddlHideShow.SelectedValue );
            SetAttributeValue( "FormattedOutput", ceFormattedOutput.Text );
            SetAttributeValue( "PageTitleLava", cePageTitleLava.Text );
            SetAttributeValue( "PersonReport", cbPersonReport.Checked.ToString() );

            SetAttributeValue( "ShowCommunicate", ( cbPersonReport.Checked && cbShowCommunicate.Checked ).ToString() );
            SetAttributeValue( "ShowMergePerson", ( cbPersonReport.Checked && cbShowMergePerson.Checked ).ToString() );
            SetAttributeValue( "ShowBulkUpdate", ( cbPersonReport.Checked && cbShowBulkUpdate.Checked ).ToString() );
            SetAttributeValue( "ShowExcelExport", cbShowExcelExport.Checked.ToString() );
            SetAttributeValue( "ShowMergeTemplate", cbShowMergeTemplate.Checked.ToString() );
            SetAttributeValue( "ShowGridFilter", cbShowGridFilter.Checked.ToString() );

            SetAttributeValue( "MergeFields", tbMergeFields.Text );
            SaveAttributeValues();

            mdEdit.Hide();
            pnlEditModel.Visible = false;
            upnlContent.Update();

            BuildControls( true );
        }

        /// <summary>
        /// Handles the RowSelected event of the gReport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gReport_RowSelected( object sender, RowEventArgs e )
        {
            Grid grid = sender as Grid;
            string url = GetAttributeValue( "UrlMask" );
            if ( grid != null && !string.IsNullOrWhiteSpace( url ) )
            {
                foreach ( string key in grid.DataKeyNames )
                {
                    url = url.Replace( "{" + key + "}", grid.DataKeys[e.RowIndex][key].ToString() );
                }

                Response.Redirect( url, false );
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gReport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gReport_GridRebind( object sender, EventArgs e )
        {
            string errorMessage = string.Empty;
            var ds = GetData( out errorMessage );
            if ( ds != null )
            {
                int i = 0;
                foreach ( Control div in phContent.Controls )
                {
                    foreach ( var grid in div.Controls.OfType<Grid>() )
                    {
                        if ( ds.Tables.Count > i )
                        {
                            FilterTable( grid, ds.Tables[i] );
                            SortTable( grid, ds.Tables[i] );
                            grid.DataSource = ds.Tables[i];
                            grid.DataBind();
                            i++;
                        }
                    }
                }
            }

            upnlContent.Update();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Gets the data.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        private DataSet GetData( out string errorMessage )
        {
            errorMessage = string.Empty;

            string query = GetAttributeValue( "Query" );
            if ( !string.IsNullOrWhiteSpace( query ) )
            {
                try
                {
                    var mergeFields = GetDynamicDataMergeFields();

                    // NOTE: there is already a PageParameters merge field from GetDynamicDataMergeFields, but for backwords compatibility, also add each of the PageParameters as plain merge fields
                    foreach ( var pageParam in PageParameters() )
                    {
                        mergeFields.AddOrReplace( pageParam.Key, pageParam.Value );
                    }

                    query = query.ResolveMergeFields( mergeFields );

                    var parameters = GetParameters();
                    int timeout = GetAttributeValue( "Timeout" ).AsInteger();

                    return DbService.GetDataSet( query, GetAttributeValue( "StoredProcedure" ).AsBoolean( false ) ? CommandType.StoredProcedure : CommandType.Text, parameters, timeout );
                }
                catch ( System.Exception ex )
                {
                    errorMessage = ex.Message;
                }
            }

            return null;
        }

        /// <summary>
        /// Shows the settings.
        /// </summary>
        protected override void ShowSettings()
        {
            pnlEditModel.Visible = true;
            upnlContent.Update();
            mdEdit.Show();

            if ( _updatePage )
            {
                var pageCache = PageCache.Read( RockPage.PageId );
                tbName.Text = pageCache != null ? pageCache.PageTitle : string.Empty;
                tbDesc.Text = pageCache != null ? pageCache.Description : string.Empty;
            }

            tbName.Visible = _updatePage;
            tbDesc.Visible = _updatePage;

            ceQuery.Text = GetAttributeValue( "Query" );
            nbTimeout.Text = GetAttributeValue( "Timeout" );
            cbStoredProcedure.Checked = GetAttributeValue( "StoredProcedure" ).AsBoolean();
            tbParams.Text = GetAttributeValue( "QueryParams" );
            tbUrlMask.Text = GetAttributeValue( "UrlMask" );
            ddlHideShow.SelectedValue = GetAttributeValue( "ShowColumns" );
            tbColumns.Text = GetAttributeValue( "Columns" );
            ceFormattedOutput.Text = GetAttributeValue( "FormattedOutput" );
            cePageTitleLava.Text = GetAttributeValue( "PageTitleLava" );
            cbPersonReport.Checked = GetAttributeValue( "PersonReport" ).AsBoolean();

            cbShowCommunicate.Checked = GetAttributeValue( "ShowCommunicate" ).AsBoolean();
            cbShowMergePerson.Checked = GetAttributeValue( "ShowMergePerson" ).AsBoolean();
            cbShowBulkUpdate.Checked = GetAttributeValue( "ShowBulkUpdate" ).AsBoolean();
            cbShowExcelExport.Checked = GetAttributeValue( "ShowExcelExport" ).AsBoolean();
            cbShowMergeTemplate.Checked = GetAttributeValue( "ShowMergeTemplate" ).AsBoolean();
            cbShowGridFilter.Checked = GetAttributeValue( "ShowGridFilter" ).AsBoolean();

            tbMergeFields.Text = GetAttributeValue( "MergeFields" );
        }

        /// <summary>
        /// Gets the dynamic data merge fields.
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, object> GetDynamicDataMergeFields()
        {
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
            if ( CurrentPerson != null )
            {
                // TODO: When support for "Person" is not supported anymore (should use "CurrentPerson" instead), remove this line
                mergeFields.Add( "Person", CurrentPerson );
            }

            mergeFields.Add( "RockVersion", Rock.VersionInfo.VersionInfo.GetRockProductVersionNumber() );
            mergeFields.Add( "CurrentPage", this.PageCache );

            return mergeFields;
        }

        /// <summary>
        /// Builds the controls.
        /// </summary>
        /// <param name="setData">if set to <c>true</c> [set data].</param>
        private void BuildControls( bool setData )
        {
            var showGridFilterControls = GetAttributeValue( "ShowGridFilter" ).AsBoolean();
            string errorMessage = string.Empty;
            var dataSet = GetData( out errorMessage );

            if ( !string.IsNullOrWhiteSpace( errorMessage ) )
            {
                phContent.Visible = false;

                nbError.Text = errorMessage;
                nbError.Visible = true;
            }
            else
            {
                phContent.Controls.Clear();

                var mergeFields = GetDynamicDataMergeFields();

                if ( dataSet != null )
                {
                    string formattedOutput = GetAttributeValue( "FormattedOutput" );

                    // load merge objects if needed by either for formatted output OR page title
                    if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "PageTitleLava" ) ) || !string.IsNullOrWhiteSpace( formattedOutput ) )
                    {
                        int i = 1;
                        foreach ( DataTable dataTable in dataSet.Tables )
                        {
                            var dropRows = new List<DataRowDrop>();
                            foreach ( DataRow row in dataTable.Rows )
                            {
                                dropRows.Add( new DataRowDrop( row ) );
                            }

                            if ( dataSet.Tables.Count > 1 )
                            {
                                var tableField = new Dictionary<string, object>();
                                tableField.Add( "rows", dropRows );
                                mergeFields.Add( "table" + i.ToString(), tableField );
                            }
                            else
                            {
                                mergeFields.Add( "rows", dropRows );
                            }
                            i++;
                        }
                    }

                    // set page title
                    if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "PageTitleLava" ) ) )
                    {
                        string title = GetAttributeValue( "PageTitleLava" ).ResolveMergeFields( mergeFields );

                        RockPage.BrowserTitle = title;
                        RockPage.PageTitle = title;
                        RockPage.Header.Title = title;
                    }

                    if ( string.IsNullOrWhiteSpace( formattedOutput ) )
                    {
                        bool personReport = GetAttributeValue( "PersonReport" ).AsBoolean();

                        int tableId = 0;
                        foreach ( DataTable dataTable in dataSet.Tables )
                        {
                            var div = new HtmlGenericControl( "div" );
                            div.AddCssClass( "grid" );

                            if ( GetAttributeValue( "PaneledGrid" ).AsBoolean() )
                            {
                                div.AddCssClass( "grid-panel" );
                            }

                            phContent.Controls.Add( div );

                            GridFilter = new GridFilter()
                            {
                                ID = string.Format("gfFilter{0}", tableId )
                            };

                            div.Controls.Add( GridFilter );
                            GridFilter.ApplyFilterClick += ApplyFilterClick;
                            GridFilter.DisplayFilterValue += DisplayFilterValue;
                            GridFilter.Visible = showGridFilterControls && (dataSet.Tables.Count == 1);
               
                            var grid = new Grid();
                            div.Controls.Add( grid );
                            grid.ID = string.Format( "dynamic_data_{0}", tableId++ );
                            grid.AllowSorting = true;
                            grid.EmptyDataText = "No Results";
                            grid.Actions.ShowCommunicate = GetAttributeValue( "ShowCommunicate" ).AsBoolean();
                            grid.Actions.ShowMergePerson = GetAttributeValue( "ShowMergePerson" ).AsBoolean();
                            grid.Actions.ShowBulkUpdate = GetAttributeValue( "ShowBulkUpdate" ).AsBoolean();
                            grid.Actions.ShowExcelExport = GetAttributeValue( "ShowExcelExport" ).AsBoolean();
                            grid.Actions.ShowMergeTemplate = GetAttributeValue( "ShowMergeTemplate" ).AsBoolean();

                            grid.GridRebind += gReport_GridRebind;
                            grid.RowSelected += gReport_RowSelected;
                            if ( personReport )
                            {
                                grid.PersonIdField = "Id";
                            }
                            else
                            {
                                grid.PersonIdField = null;
                            }
                            grid.CommunicateMergeFields = GetAttributeValue( "MergeFields" ).SplitDelimitedValues().ToList<string>();

                            AddGridColumns( grid, dataTable );
                            SetDataKeyNames( grid, dataTable );

                            if ( setData )
                            {
                                FilterTable( grid, dataTable );
                                SortTable( grid, dataTable );
                                grid.DataSource = dataTable;
                                
                                if ( personReport)
                                {
                                    grid.EntityTypeId = EntityTypeCache.GetId<Person>();
                                }

                                grid.DataBind();
                            }
                        }
                    }
                    else
                    {
                        phContent.Controls.Add( new LiteralControl( formattedOutput.ResolveMergeFields( mergeFields ) ) );
                    }
                }

                phContent.Visible = true;
                nbError.Visible = false;
            }
        }

        /// <summary>
        /// Sets the data key names.
        /// </summary>
        private void SetDataKeyNames( Grid grid, DataTable dataTable )
        {
            string urlMask = GetAttributeValue( "UrlMask" );
            if ( !string.IsNullOrWhiteSpace( urlMask ) )
            {
                Regex pattern = new Regex( @"\{[\w\s]+\}" );
                var matches = pattern.Matches( urlMask );
                if ( matches.Count > 0 )
                {
                    var keyNames = new List<string>();
                    for ( int i = 0; i < matches.Count; i++ )
                    {
                        string colName = matches[i].Value.TrimStart( '{' ).TrimEnd( '}' );
                        if ( dataTable.Columns.Contains( colName ) )
                        {
                            keyNames.Add( colName );
                        }
                    }

                    grid.DataKeyNames = keyNames.ToArray();
                }
            }
            else
            {
                if ( dataTable.Columns.Contains( "Id" ) )
                {
                    grid.DataKeyNames = new string[1] { "Id" };
                }
            }
        }

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, object> GetParameters()
        {
            string[] queryParams = GetAttributeValue( "QueryParams" ).SplitDelimitedValues();
            if ( queryParams.Length > 0 )
            {
                var parameters = new Dictionary<string, object>();
                foreach ( string queryParam in queryParams )
                {
                    string[] paramParts = queryParam.Split( '=' );
                    if ( paramParts.Length == 2 )
                    {
                        string queryParamName = paramParts[0];
                        string queryParamValue = paramParts[1];

                        // Remove leading '@' character if was included
                        if ( queryParamName.StartsWith( "@" ) )
                        {
                            queryParamName = queryParamName.Substring( 1 );
                        }

                        // If a page parameter (query or form) value matches, use it's value instead
                        string pageValue = PageParameter( queryParamName );
                        if ( !string.IsNullOrWhiteSpace( pageValue ) )
                        {
                            queryParamValue = pageValue;
                        }
                        else if ( queryParamName.ToLower() == "currentpersonid" && CurrentPerson != null )
                        {
                            // If current person id, use the value of the current person id
                            queryParamValue = CurrentPerson.Id.ToString();
                        }

                        parameters.Add( queryParamName, queryParamValue );
                    }
                }

                return parameters;
            }

            return null;
        }

        /// <summary>
        /// Adds the grid columns.
        /// </summary>
        /// <param name="dataTable">The data table.</param>
        private void AddGridColumns( Grid grid, DataTable dataTable )
        {
            bool showColumns = GetAttributeValue( "ShowColumns" ).AsBoolean();
            var columnList = GetAttributeValue( "Columns" ).SplitDelimitedValues().ToList();

            int rowsToEval = 10;
            if ( dataTable.Rows.Count < 10 )
            {
                rowsToEval = dataTable.Rows.Count;
            }

            grid.Columns.Clear();

            if ( !string.IsNullOrWhiteSpace( grid.PersonIdField ) )
            {
                grid.Columns.Add( new SelectField() );
            }

            GridFilterColumnLookup = new Dictionary<Control, string>();

            foreach ( DataColumn dataTableColumn in dataTable.Columns )
            {
                if ( columnList.Count > 0 &&
                    ( ( showColumns && !columnList.Contains( dataTableColumn.ColumnName, StringComparer.OrdinalIgnoreCase ) ) ||
                        ( !showColumns && columnList.Contains( dataTableColumn.ColumnName, StringComparer.OrdinalIgnoreCase ) ) ) )
                {
                    continue;
                }

                BoundField bf = new BoundField();
                var splitCaseName = dataTableColumn.ColumnName.SplitCase();

                if ( dataTableColumn.DataType == typeof( bool ) )
                {
                    bf = new BoolField();

                    if ( GridFilter != null )
                    {
                        var id = "ddl" + dataTableColumn.ColumnName.RemoveSpecialCharacters();

                        var filterControl = new RockDropDownList()
                        {
                            Label = splitCaseName,
                            ID = id
                        };

                        GridFilterColumnLookup.Add( filterControl, dataTableColumn.ColumnName );

                        filterControl.Items.Add( BoolToString( null ) );
                        filterControl.Items.Add( BoolToString( true ) );
                        filterControl.Items.Add( BoolToString( false ) );
                        GridFilter.Controls.Add( filterControl );

                        var value = GridFilter.GetUserPreference( id );

                        if ( value != null )
                        {
                            filterControl.SetValue( value );
                        }
                    }
                }
                else if ( dataTableColumn.DataType == typeof( DateTime ) )
                {
                    bf = new DateField();

                    for ( int i = 0; i < rowsToEval; i++ )
                    {
                        object dateObj = dataTable.Rows[i][dataTableColumn];
                        if ( dateObj is DateTime )
                        {
                            DateTime dateTime = ( DateTime ) dateObj;
                            if ( dateTime.TimeOfDay.Seconds != 0 )
                            {
                                bf = new DateTimeField();
                                break;
                            }
                        }
                    }

                    if ( GridFilter != null )
                    {
                        var id = "drp" + dataTableColumn.ColumnName.RemoveSpecialCharacters();

                        var filterControl = new DateRangePicker()
                        {
                            Label = splitCaseName,
                            ID = id,
                        };

                        GridFilterColumnLookup.Add( filterControl, dataTableColumn.ColumnName );

                        GridFilter.Controls.Add( filterControl );

                        var value = GridFilter.GetUserPreference( id );

                        if ( value != null )
                        {
                            DateTime upper;
                            DateTime lower;

                            if ( DateRangePicker.TryParse( value, out lower, out upper ) )
                            {
                                filterControl.LowerValue = lower;
                                filterControl.UpperValue = upper;
                            }
                        }
                    }
                }
                else
                {
                    bf.HtmlEncode = false;

                    if ( GridFilter != null )
                    {
                        var id = "tb" + dataTableColumn.ColumnName.RemoveSpecialCharacters();
                        var filterControl = new RockTextBox()
                        {
                            Label = splitCaseName,
                            ID = id
                        };

                        GridFilterColumnLookup.Add( filterControl, dataTableColumn.ColumnName );

                        GridFilter.Controls.Add( filterControl );
                        var key = filterControl.ID;
                        var value = GridFilter.GetUserPreference( key );

                        if ( value != null )
                        {
                            filterControl.Text = value;
                        }
                    }
                }

                bf.DataField = dataTableColumn.ColumnName;
                bf.SortExpression = dataTableColumn.ColumnName;
                bf.HeaderText = splitCaseName;
                grid.Columns.Add( bf );
            }
        }

        /// <summary>
        /// Gets the sorted view.
        /// </summary>
        /// <param name="dataTable">The data table.</param>
        /// <returns></returns>
        private void SortTable( Grid grid, DataTable dataTable )
        {
            System.Data.DataView dataView = dataTable.DefaultView;

            SortProperty sortProperty = grid.SortProperty;
            if ( sortProperty != null )
            {
                dataView.Sort = string.Format( "{0} {1}", sortProperty.Property, sortProperty.DirectionString );
            }
        }

        /// <summary>
        /// Gets the sorted view.
        /// </summary>
        /// <param name="dataTable">The data table.</param>
        /// <returns></returns>
        private void FilterTable( Grid grid, DataTable dataTable )
        {
            var showGridFilterControls = GetAttributeValue( "ShowGridFilter" ).AsBoolean();
            System.Data.DataView dataView = dataTable.DefaultView;

            if ( !showGridFilterControls )
            {
                dataView.RowFilter = null;
                return;
            }
            
            var query = new List<string>();

            foreach ( var control in GridFilter.Controls.OfType<Control>() )
            {
                if ( control is DateRangePicker )
                {
                    var dateRangePicker = control as DateRangePicker;
                    var minValue = dateRangePicker.LowerValue;
                    var maxValue = dateRangePicker.UpperValue;
                    
                    var colName = GridFilterColumnLookup[control];

                    if ( minValue.HasValue )
                    {
                        query.Add( string.Format( "[{0}] >= #{1}#", colName, minValue.Value ) );
                    }

                    if ( maxValue.HasValue )
                    {
                        query.Add( string.Format( "[{0}] < #{1}#", colName, maxValue.Value.AddDays( 1 ) ) );
                    }
                }
                else if ( control is RockDropDownList )
                {
                    var dropDownList = control as RockDropDownList;
                    var doFilter = !string.IsNullOrWhiteSpace( dropDownList.SelectedValue );

                    if ( doFilter )
                    {
                        var value = dropDownList.SelectedValue == true.ToYesNo() ? "1" : "0";
                        var colName = GridFilterColumnLookup[control];
                        query.Add( string.Format( "[{0}] = {1}", colName, value ) );
                    }
                }
                else if ( control is RockTextBox )
                {
                    var textBox = control as RockTextBox;
                    var value = textBox.Text;
                    var colName = GridFilterColumnLookup[control]; 
                    var colIndex = dataView.Table.Columns.IndexOf( colName );

                    if ( colIndex != -1 && !string.IsNullOrWhiteSpace( value ) )
                    {
                        var col = dataView.Table.Columns[colIndex];

                        if ( col.DataType.Name == "String" )
                        {
                            query.Add( string.Format( "[{0}] LIKE '%{1}%'", colName, value ) );
                        }
                        else if ( col.DataType.Name.StartsWith( "Int" ) )
                        {
                            query.Add( string.Format( "[{0}] = {1}", colName, value ) );
                        }
                    }
                }
            }

            dataView.RowFilter = string.Join( " AND ", query );
        }

        /// <summary>
        /// Converts bool to string.
        /// </summary>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        private string BoolToString( bool? b )
        {
            if ( b.HasValue )
            {
                return b.Value.ToYesNo();
            }

            return string.Empty;
        }

        /// <summary>
        /// Converts string to bool
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns></returns>
        private bool? StringToBool( string s )
        {
            if ( s == BoolToString( true ) )
            {
                return true;
            }

            if ( s == BoolToString( false ) )
            {
                return false;
            }

            return null;
        }

        #endregion

        /// <summary>
        ///
        /// </summary>
        private class DataRowDrop : DotLiquid.Drop
        {
            private readonly DataRow _dataRow;

            public DataRowDrop( DataRow dataRow )
            {
                _dataRow = dataRow;
            }

            public override object BeforeMethod( string method )
            {
                if ( _dataRow.Table.Columns.Contains( method ) )
                {
                    return _dataRow[method];
                }

                return null;
            }
        }
    }
}