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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Lava;
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

    #region Block Attributes
    // Block Properties
    [BooleanField( "Update Page",
        Description = "If True, provides fields for updating the parent page's Name and Description",
        DefaultBooleanValue = true,
        Order = 0,
        Key = AttributeKey.UpdatePage )]
    [LavaCommandsField( "Enabled Lava Commands",
        Description = "The Lava commands that should be enabled for this dynamic data block.",
        IsRequired = false,
        Order = 1,
        Key = AttributeKey.EnabledLavaCommands )]
    [BooleanField( "Enable Quick Return",
        Description = "When enabled, viewing the block will cause it to be added to the Quick Return list in the bookmarks feature.",
        DefaultBooleanValue = false,
        Key = AttributeKey.EnableQuickReturn )]

    // Custom Settings
    [CodeEditorField( "Query",
        Description = "The query to execute. Note that if you are providing SQL you can add items from the query string using Lava like {{ QueryParmName }}.",
        EditorMode = CodeEditorMode.Sql,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 400,
        IsRequired = false,
        Category = "CustomSetting",
        Key = AttributeKey.Query )]

    [TextField( "Query Params",
        Description = "Parameters to pass to query",
        IsRequired = false,
        Category = "CustomSetting",
        Key = AttributeKey.QueryParams )]

    [BooleanField( "Stored Procedure",
        Description = "Is the query a stored procedure?",
        DefaultBooleanValue = false,
        Category = "CustomSetting",
        Key = AttributeKey.StoredProcedure )]

    [TextField( "Url Mask",
        Description = "The URL to redirect to when a row is clicked",
        IsRequired = false,
        Category = "CustomSetting",
        Key = AttributeKey.UrlMask )]

    [BooleanField( "Show Columns",
        Description = "Should the 'Columns' specified below be the only ones shown (vs. the only ones hidden)",
        DefaultBooleanValue = false,
        Category = "CustomSetting",
        Key = AttributeKey.ShowColumns )]

    [TextField( "Columns",
        Description = "The columns to hide or show",
        IsRequired = false,
        Category = "CustomSetting",
        Key = AttributeKey.Columns )]

    [CodeEditorField( "Formatted Output",
        Description = "Optional formatting to apply to the returned results.  If left blank, a grid will be displayed. Example: {% for row in rows %} {{ row.FirstName }}<br/> {% endfor %}",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        Category = "CustomSetting",
        Key = AttributeKey.FormattedOutput )]

    [BooleanField( "Person Report",
        Description = "Is this report a list of people?",
        DefaultBooleanValue = false,
        Category = "CustomSetting",
        Key = AttributeKey.PersonReport )]

    [BooleanField( "Show Communicate",
        Description = "Show Communicate button in grid footer?",
        DefaultBooleanValue = true,
        Category = "CustomSetting",
        Key = AttributeKey.ShowCommunicate )]

    [BooleanField( "Show Merge Person",
        Description = "Show Merge Person button in grid footer?",
        DefaultBooleanValue = true,
        Category = "CustomSetting",
        Key = AttributeKey.ShowMergePerson )]

    [BooleanField( "Show Bulk Update",
        Description = "Show Bulk Update button in grid footer?",
        DefaultBooleanValue = true,
        Category = "CustomSetting",
        Key = AttributeKey.ShowBulkUpdate )]

    [BooleanField( "Show Excel Export",
        Description = "Show Export to Excel button in grid footer?",
        DefaultBooleanValue = true,
        Category = "CustomSetting",
        Key = AttributeKey.ShowExcelExport )]

    [BooleanField( "Show Merge Template",
        Description = "Show Export to Merge Template button in grid footer?",
        DefaultBooleanValue = true,
        Category = "CustomSetting",
        Key = AttributeKey.ShowMergeTemplate )]

    [BooleanField( "Show Launch Workflow",
        Description = "Show Launch Workflow button in grid footer?",
        DefaultBooleanValue = true,
        Category = "CustomSetting",
        Key = AttributeKey.ShowLaunchWorkflow )]

    [IntegerField( "Timeout",
        Description = "The amount of time in seconds to allow the query to run before timing out.",
        IsRequired = false,
        DefaultIntegerValue = 30,
        Category = "CustomSetting",
        Key = AttributeKey.Timeout )]

    [TextField( "Merge Fields",
        Description = "Any fields to make available as merge fields for any new communications",
        IsRequired = false,
        Category = "CustomSetting",
        Key = AttributeKey.MergeFields )]

    [TextField( "Communication Recipient Person Id Columns",
        Description = "Columns that contain a communication recipient person id.",
        IsRequired = false,
        Category = "CustomSetting",
        Key = AttributeKey.CommunicationRecipientPersonIdColumns )]

    [TextField( "Encrypted Fields",
        Description = "Any fields that need to be decrypted before displaying their value",
        IsRequired = false,
        Category = "CustomSetting",
        Key = AttributeKey.EncryptedFields )]

    [CodeEditorField( "Page Title Lava",
        Description = "Optional Lava for setting the page title. If nothing is provided then the page's title will be used.",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        Category = "CustomSetting",
        Key = AttributeKey.PageTitleLava )]

    [BooleanField( "Paneled Grid",
        Description = "Add the 'grid-panel' class to the grid to allow it to fit nicely in a block.",
        DefaultBooleanValue = false,
        Category = "Advanced",
        Key = AttributeKey.PaneledGrid )]

    [BooleanField( "Show Grid Filter",
        Description = "Show filtering controls that are dynamically generated to match the columns of the dynamic data.",
        DefaultBooleanValue = true,
        Category = "CustomSetting",
        Key = AttributeKey.ShowGridFilter )]

    [BooleanField( "Enable Sticky Header on Grid",
        Description = "Determines whether the header on the grid will be stick at the top of the page.",
        DefaultBooleanValue = false,
        Category = "CustomSetting",
        Key = AttributeKey.EnableStickyHeaderOnGrid )]

    [BooleanField( "Wrap In Panel",
        Description = "This will wrap the results grid in a panel.",
        DefaultBooleanValue = false,
        Category = "CustomSetting",
        Key = AttributeKey.WrapInPanel )]

    [TextField( "Panel Title",
        Description = "The title of the panel.",
        Category = "CustomSetting",
        Key = AttributeKey.PanelTitle )]

    [TextField( "Panel Icon CSS Class",
        Description = "The CSS Class to use in the panel title.",
        Category = "CustomSetting",
        Key = AttributeKey.PanelTitleCssClass )]

    [CodeEditorField( "Grid Header Content",
        Description = "This Lava template will be rendered above the grid. It will have access to the same dataset as the grid.",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        Category = "CustomSetting",
        Key = AttributeKey.GridHeaderContent )]

    [CodeEditorField( "Grid Footer Content",
        Description = "This Lava template will be rendered below the grid (best used for custom totaling). It will have access to the same dataset as the grid.",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        Category = "CustomSetting",
        Key = AttributeKey.GridFooterContent )]
    #endregion
    [Rock.SystemGuid.BlockTypeGuid( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126" )]
    public partial class DynamicData : RockBlockCustomSettings
    {
        #region Keys

        /// <summary>
        /// Attribute Keys
        /// </summary>
        private static class AttributeKey
        {
            public const string UpdatePage = "UpdatePage";
            public const string EnabledLavaCommands = "EnabledLavaCommands";
            public const string Query = "Query";
            public const string QueryParams = "QueryParams";
            public const string StoredProcedure = "StoredProcedure";
            public const string UrlMask = "UrlMask";
            public const string ShowColumns = "ShowColumns";
            public const string Columns = "Columns";
            public const string FormattedOutput = "FormattedOutput";
            public const string PersonReport = "PersonReport";
            public const string ShowCommunicate = "ShowCommunicate";
            public const string ShowMergePerson = "ShowMergePerson";
            public const string ShowBulkUpdate = "ShowBulkUpdate";
            public const string ShowExcelExport = "ShowExcelExport";
            public const string ShowMergeTemplate = "ShowMergeTemplate";
            public const string Timeout = "Timeout";
            public const string MergeFields = "MergeFields";
            public const string CommunicationRecipientPersonIdColumns = "CommunicationRecipientPersonIdColumns";
            public const string EncryptedFields = "EncryptedFields";
            public const string PageTitleLava = "PageTitleLava";
            public const string PaneledGrid = "PaneledGrid";
            public const string ShowGridFilter = "ShowGridFilter";
            public const string EnableStickyHeaderOnGrid = "EnableStickyHeaderOnGrid";
            public const string WrapInPanel = "WrapInPanel";
            public const string PanelTitle = "PanelTitle";
            public const string PanelTitleCssClass = "PanelTitleCssClass";
            public const string ShowLaunchWorkflow = "ShowLaunchWorkflow";
            public const string GridHeaderContent = "GridHeaderContent";
            public const string GridFooterContent = "GridFooterContent";
            public const string EnableQuickReturn = "EnableQuickReturn";
        }

        #endregion Keys

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

            _updatePage = GetAttributeValue( AttributeKey.UpdatePage ).AsBoolean( true );
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
            GridFilter.DeleteFilterPreferences();

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

                GridFilter.SetFilterPreference( key, name, value );
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
                var pageCache = PageCache.Get( RockPage.PageId );
                if ( pageCache != null &&
                        ( pageCache.PageTitle != tbName.Text || pageCache.Description != tbDesc.Text )
                        && pageCache.Guid != Rock.SystemGuid.Page.PAGE_MAP.AsGuid() // Don't allow editing the title of the page if the page is the internal page editor (Issue #5542)
                   )
                {
                    var rockContext = new RockContext();
                    var service = new PageService( rockContext );
                    var page = service.Get( pageCache.Id );
                    page.InternalName = tbName.Text;
                    page.PageTitle = tbName.Text;
                    page.BrowserTitle = tbName.Text;
                    page.Description = tbDesc.Text;
                    rockContext.SaveChanges();

                    pageCache = PageCache.Get( RockPage.PageId );

                    var breadCrumb = RockPage.BreadCrumbs.Where( c => c.Url == RockPage.PageReference.BuildUrl() ).FirstOrDefault();
                    if ( breadCrumb != null )
                    {
                        breadCrumb.Name = pageCache.BreadCrumbText;
                    }
                }
            }

            SetAttributeValue( AttributeKey.Query, ceQuery.Text );
            SetAttributeValue( AttributeKey.Timeout, nbTimeout.Text );
            SetAttributeValue( AttributeKey.StoredProcedure, cbStoredProcedure.Checked.ToString() );
            SetAttributeValue( AttributeKey.QueryParams, tbParams.Text );
            SetAttributeValue( AttributeKey.UrlMask, tbUrlMask.Text );
            SetAttributeValue( AttributeKey.Columns, tbColumns.Text );
            SetAttributeValue( AttributeKey.ShowColumns, ddlHideShow.SelectedValue );
            SetAttributeValue( AttributeKey.FormattedOutput, swLavaCustomization.Checked ? ceFormattedOutput.Text : string.Empty );
            SetAttributeValue( AttributeKey.PageTitleLava, cePageTitleLava.Text );
            SetAttributeValue( AttributeKey.GridHeaderContent, ceGridHeaderLava.Text );
            SetAttributeValue( AttributeKey.GridFooterContent, ceGridFooterLava.Text );
            SetAttributeValue( AttributeKey.PersonReport, cbPersonReport.Checked.ToString() );
            SetAttributeValue( AttributeKey.CommunicationRecipientPersonIdColumns, tbCommunicationRecipientPersonIdFields.Text );
            SetAttributeValue( AttributeKey.ShowCommunicate, ( cbPersonReport.Checked && cbShowCommunicate.Checked ).ToString() );
            SetAttributeValue( AttributeKey.ShowMergePerson, ( cbPersonReport.Checked && cbShowMergePerson.Checked ).ToString() );
            SetAttributeValue( AttributeKey.ShowBulkUpdate, ( cbPersonReport.Checked && cbShowBulkUpdate.Checked ).ToString() );
            SetAttributeValue( AttributeKey.ShowExcelExport, cbShowExcelExport.Checked.ToString() );
            SetAttributeValue( AttributeKey.ShowMergeTemplate, cbShowMergeTemplate.Checked.ToString() );
            SetAttributeValue( AttributeKey.ShowLaunchWorkflow, ( cbPersonReport.Checked && cbShowLaunchWorkflow.Checked ).ToString() );
            SetAttributeValue( AttributeKey.ShowGridFilter, cbShowGridFilter.Checked.ToString() );
            SetAttributeValue( AttributeKey.EnableStickyHeaderOnGrid, cbEnableStickyHeaderOnGrid.Checked.ToString() );
            SetAttributeValue( AttributeKey.MergeFields, tbMergeFields.Text );
            SetAttributeValue( AttributeKey.EncryptedFields, tbEncryptedFields.Text );
            SetAttributeValue( AttributeKey.WrapInPanel, swWrapInPanel.Checked.ToString() );
            SetAttributeValue( AttributeKey.PanelTitleCssClass, tbPanelIcon.Text );
            SetAttributeValue( AttributeKey.PanelTitle, tbPanelTitle.Text );

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
            string url = GetAttributeValue( AttributeKey.UrlMask );
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
                    foreach ( var grid in div.ControlsOfTypeRecursive<Grid>() )
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
        /// <param name="schemaOnly">if set to <c>true</c> [schema only].</param>
        /// <returns></returns>
        private DataSet GetData( out string errorMessage, bool schemaOnly = false )
        {
            errorMessage = string.Empty;

            string query = GetAttributeValue( AttributeKey.Query );
            string enabledLavaCommands = this.GetAttributeValue( AttributeKey.EnabledLavaCommands );
            if ( !string.IsNullOrWhiteSpace( query ) )
            {
                try
                {
                    var mergeFields = GetDynamicDataMergeFields();

                    // NOTE: there is already a PageParameters merge field from GetDynamicDataMergeFields, but for backwards compatibility, also add each of the PageParameters as plain merge fields
                    foreach ( var pageParam in PageParameters() )
                    {
                        mergeFields.AddOrReplace( pageParam.Key, pageParam.Value );
                    }

                    query = query.ResolveMergeFields( mergeFields, enabledLavaCommands );

                    var parameters = GetParameters();
                    int timeout = GetAttributeValue( AttributeKey.Timeout ).AsInteger();

                    if ( schemaOnly && new Regex( @"#\w+" ).IsMatch( query ) )
                    {
                        /*
                            5/28/2024 - JPH

                            If this query makes use of any temporary tables, bypass the loading of schema
                            only, and go straight to loading schema and data, as the underlying use of
                            `SqlDataAdapter.FillSchema()` will throw an exception when temp tables are
                            being used.

                            The pattern being matched against here will catch both local (#table_name)
                            and global (##table_name) temporary tables.

                            Reason: The use of temporary SQL tables in dynamic data block queries causes
                            cluttered Azure SQL error logs, and causes extra load on the database server.
                            https://github.com/SparkDevNetwork/Rock/issues/5868
                         */
                        schemaOnly = false;
                    }

                    if ( schemaOnly )
                    {
                        try
                        {
                            // GetDataSetSchema won't work in some cases, for example, if the SQL references a TEMP table.  So, fall back to use the regular GetDataSet if there is an exception or the schema does not return any tables
                            var dataSet = DbService.GetDataSetSchema( query, GetAttributeValue( AttributeKey.StoredProcedure ).AsBoolean( false ) ? CommandType.StoredProcedure : CommandType.Text, parameters, timeout );
                            if ( dataSet != null && dataSet.Tables != null && dataSet.Tables.Count > 0 )
                            {
                                return dataSet;
                            }
                            else
                            {
                                return DbService.GetDataSet( query, GetAttributeValue( AttributeKey.StoredProcedure ).AsBoolean( false ) ? CommandType.StoredProcedure : CommandType.Text, parameters, timeout );
                            }
                        }
                        catch
                        {
                            return DbService.GetDataSet( query, GetAttributeValue( AttributeKey.StoredProcedure ).AsBoolean( false ) ? CommandType.StoredProcedure : CommandType.Text, parameters, timeout );
                        }
                    }
                    else
                    {
                        return DbService.GetDataSet( query, GetAttributeValue( AttributeKey.StoredProcedure ).AsBoolean( false ) ? CommandType.StoredProcedure : CommandType.Text, parameters, timeout );
                    }
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
                var pageCache = PageCache.Get( RockPage.PageId );
                tbName.Text = pageCache != null ? pageCache.PageTitle : string.Empty;
                tbDesc.Text = pageCache != null ? pageCache.Description : string.Empty;
            }

            tbName.Visible = _updatePage;
            tbDesc.Visible = _updatePage;

            ceQuery.Text = GetAttributeValue( AttributeKey.Query );
            nbTimeout.Text = GetAttributeValue( AttributeKey.Timeout );
            cbStoredProcedure.Checked = GetAttributeValue( AttributeKey.StoredProcedure ).AsBoolean();
            tbParams.Text = GetAttributeValue( AttributeKey.QueryParams );
            tbUrlMask.Text = GetAttributeValue( AttributeKey.UrlMask );
            ddlHideShow.SelectedValue = GetAttributeValue( AttributeKey.ShowColumns );
            tbColumns.Text = GetAttributeValue( AttributeKey.Columns );
            ceFormattedOutput.Text = GetAttributeValue( AttributeKey.FormattedOutput );
            cePageTitleLava.Text = GetAttributeValue( AttributeKey.PageTitleLava );
            ceGridHeaderLava.Text = GetAttributeValue( AttributeKey.GridHeaderContent );
            ceGridFooterLava.Text = GetAttributeValue( AttributeKey.GridFooterContent );
            cbPersonReport.Checked = GetAttributeValue( AttributeKey.PersonReport ).AsBoolean();
            tbCommunicationRecipientPersonIdFields.Text = GetAttributeValue( AttributeKey.CommunicationRecipientPersonIdColumns );
            cbShowCommunicate.Checked = GetAttributeValue( AttributeKey.ShowCommunicate ).AsBoolean();
            cbShowMergePerson.Checked = GetAttributeValue( AttributeKey.ShowMergePerson ).AsBoolean();
            cbShowBulkUpdate.Checked = GetAttributeValue( AttributeKey.ShowBulkUpdate ).AsBoolean();
            cbShowExcelExport.Checked = GetAttributeValue( AttributeKey.ShowExcelExport ).AsBoolean();
            cbShowMergeTemplate.Checked = GetAttributeValue( AttributeKey.ShowMergeTemplate ).AsBoolean();
            cbShowLaunchWorkflow.Checked = GetAttributeValue( AttributeKey.ShowLaunchWorkflow ).AsBoolean();
            cbShowGridFilter.Checked = GetAttributeValue( AttributeKey.ShowGridFilter ).AsBoolean();
            cbEnableStickyHeaderOnGrid.Checked = GetAttributeValue( AttributeKey.EnableStickyHeaderOnGrid ).AsBoolean();
            tbMergeFields.Text = GetAttributeValue( AttributeKey.MergeFields );
            tbEncryptedFields.Text = GetAttributeValue( AttributeKey.EncryptedFields );
            swWrapInPanel.Checked = GetAttributeValue( AttributeKey.WrapInPanel ).AsBoolean();
            tbPanelIcon.Text = GetAttributeValue( AttributeKey.PanelTitleCssClass );
            tbPanelTitle.Text = GetAttributeValue( AttributeKey.PanelTitle );
            swLavaCustomization.Checked = ceFormattedOutput.Text.IsNotNullOrWhiteSpace();

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
            var showGridFilterControls = GetAttributeValue( AttributeKey.ShowGridFilter ).AsBoolean();
            string enabledLavaCommands = this.GetAttributeValue( AttributeKey.EnabledLavaCommands );
            string errorMessage = string.Empty;

            // get just the schema of the data until we actually need the data
            var dataSetSchema = GetData( out errorMessage, true );

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

                if ( dataSetSchema != null )
                {
                    var formattedOutput = GetAttributeValue( AttributeKey.FormattedOutput );
                    var pageTitleLava = GetAttributeValue( AttributeKey.PageTitleLava );
                    var gridHeaderLava = GetAttributeValue( AttributeKey.GridHeaderContent );
                    var gridFooterLava = GetAttributeValue( AttributeKey.GridFooterContent );

                    // load merge objects if needed by either for formatted output OR page title
                    var loadGridIntoMergeFields = formattedOutput.IsNotNullOrWhiteSpace()
                        || pageTitleLava.IsNotNullOrWhiteSpace()
                        || gridHeaderLava.IsNotNullOrWhiteSpace()
                        || gridFooterLava.IsNotNullOrWhiteSpace();

                    if ( loadGridIntoMergeFields )
                    {
                        int i = 1;

                        // Formatted output needs all the rows, so get the data regardless of the setData parameter
                        var dataSet = GetData( out errorMessage );

                        if ( dataSet == null || dataSet.Tables == null )
                        {

                            nbError.Text = errorMessage;
                            nbError.Visible = true;
                            return;
                        }

                        if ( LavaService.RockLiquidIsEnabled )
                        {
                            foreach ( DataTable dataTable in dataSet.Tables )
                            {
                                var lavaRows = new List<DataRowDrop>();
                                foreach ( DataRow row in dataTable.Rows )
                                {
                                    lavaRows.Add( new DataRowDrop( row ) );
                                }

                                if ( dataSet.Tables.Count > 1 )
                                {
                                    var tableField = new Dictionary<string, object>();
                                    tableField.Add( "rows", lavaRows );
                                    mergeFields.Add( "table" + i.ToString(), tableField );
                                }
                                else
                                {
                                    mergeFields.Add( "rows", lavaRows );
                                }

                                i++;
                            }
                        }
                        else
                        {
                            foreach ( DataTable dataTable in dataSet.Tables )
                            {
                                var lavaRows = new List<DataRowLavaData>();
                                foreach ( DataRow row in dataTable.Rows )
                                {
                                    lavaRows.Add( new DataRowLavaData( row ) );
                                }

                                if ( dataSet.Tables.Count > 1 )
                                {
                                    var tableField = new Dictionary<string, object>();
                                    tableField.Add( "rows", lavaRows );
                                    mergeFields.Add( "table" + i.ToString(), tableField );
                                }
                                else
                                {
                                    mergeFields.Add( "rows", lavaRows );
                                }

                                i++;
                            }
                        }
                    }

                    // set page title
                    if ( pageTitleLava.IsNotNullOrWhiteSpace() )
                    {
                        var title = pageTitleLava.ResolveMergeFields( mergeFields, enabledLavaCommands );

                        RockPage.BrowserTitle = title;
                        RockPage.PageTitle = title;
                        RockPage.Header.Title = title;
                    }

                    if ( GetAttributeValue( AttributeKey.EnableQuickReturn ).AsBoolean() && setData && RockPage.PageTitle.IsNotNullOrWhiteSpace() )
                    {
                        string quickReturnLava = "{{ Title | AddQuickReturn:'Dynamic Data', 80 }}";
                        var quickReturnMergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson, new Rock.Lava.CommonMergeFieldsOptions() );
                        quickReturnMergeFields.Add( "Title", RockPage.PageTitle );
                        quickReturnLava.ResolveMergeFields( quickReturnMergeFields );
                    }

                    if ( formattedOutput.IsNullOrWhiteSpace() )
                    {
                        bool personReport = GetAttributeValue( AttributeKey.PersonReport ).AsBoolean();

                        int tableId = 0;
                        DataSet dataSet;
                        if ( setData == false )
                        {
                            dataSet = dataSetSchema;
                        }
                        else
                        {
                            dataSet = GetData( out errorMessage );
                        }

                        if ( dataSet == null || dataSet.Tables == null )
                        {

                            nbError.Text = errorMessage;
                            nbError.Visible = true;
                            return;
                        }

                        HtmlGenericControl divPanelBody = null;
                        HtmlGenericControl divPanel = null;
                        if ( GetAttributeValue( AttributeKey.WrapInPanel ).AsBoolean() )
                        {
                            divPanel = new HtmlGenericControl( "div" );
                            divPanel.AddCssClass( "panel panel-block" );

                            var divPanelHeading = new HtmlGenericControl( "div" );
                            divPanelHeading.AddCssClass( "panel-heading" );

                            var panelIcon = GetAttributeValue( AttributeKey.PanelTitleCssClass );
                            var panelTitle = GetAttributeValue( AttributeKey.PanelTitle );
                            var hPanelTitle = new HtmlGenericControl( "h1" );
                            hPanelTitle.AddCssClass( "panel-title" );
                            divPanelHeading.Controls.Add( hPanelTitle );

                            if ( panelIcon.IsNullOrWhiteSpace() )
                            {
                                hPanelTitle.InnerText = panelTitle;
                            }
                            else
                            {
                                var iPanelHeaderIcon = new HtmlGenericControl( "i" );
                                iPanelHeaderIcon.AddCssClass( panelIcon );

                                var spanPanelHeaderText = new HtmlGenericControl( "span" );
                                spanPanelHeaderText.InnerText = " " + panelTitle;
                                hPanelTitle.Controls.Add( iPanelHeaderIcon );
                                hPanelTitle.Controls.Add( spanPanelHeaderText );
                            }


                            divPanelBody = new HtmlGenericControl( "div" );
                            divPanelBody.AddCssClass( "panel-body" );

                            divPanel.Controls.Add( divPanelHeading );

                            phContent.Controls.Add( divPanel );
                        }

                        if ( gridHeaderLava.IsNotNullOrWhiteSpace() )
                        {
                            var div = new HtmlGenericControl( "div" );
                            div.Controls.Add( new LiteralControl( gridHeaderLava.ResolveMergeFields( mergeFields, enabledLavaCommands ) ) );
                            if ( divPanel == null )
                            {
                                phContent.Controls.Add( div );
                            }
                            else
                            {
                                div.AddCssClass( "panel-heading" );
                                divPanel.Controls.Add( div );
                            }
                        }

                        foreach ( DataTable dataTable in dataSet.Tables )
                        {
                            var div = new HtmlGenericControl( "div" );
                            div.AddCssClass( "grid" );

                            if ( GetAttributeValue( AttributeKey.PaneledGrid ).AsBoolean() || GetAttributeValue( AttributeKey.WrapInPanel ).AsBoolean() )
                            {
                                div.AddCssClass( "grid-panel" );
                            }

                            if ( divPanelBody == null )
                            {
                                phContent.Controls.Add( div );
                            }
                            else
                            {
                                divPanelBody.Controls.Add( div );
                                divPanel.Controls.Add( divPanelBody );
                            }

                            GridFilter = new GridFilter()
                            {
                                ID = string.Format( "gfFilter{0}", tableId )
                            };

                            div.Controls.Add( GridFilter );
                            GridFilter.ApplyFilterClick += ApplyFilterClick;
                            GridFilter.DisplayFilterValue += DisplayFilterValue;
                            GridFilter.Visible = showGridFilterControls && ( dataSet.Tables.Count == 1 );

                            var grid = new Grid();
                            div.Controls.Add( grid );
                            grid.ID = string.Format( "dynamic_data_{0}", tableId++ );
                            grid.AllowSorting = true;
                            grid.EmptyDataText = "No Results";
                            grid.Actions.ShowCommunicate = GetAttributeValue( AttributeKey.ShowCommunicate ).AsBoolean();
                            grid.Actions.ShowMergePerson = GetAttributeValue( AttributeKey.ShowMergePerson ).AsBoolean();
                            grid.Actions.ShowBulkUpdate = GetAttributeValue( AttributeKey.ShowBulkUpdate ).AsBoolean();
                            grid.Actions.ShowExcelExport = GetAttributeValue( AttributeKey.ShowExcelExport ).AsBoolean();
                            grid.Actions.ShowMergeTemplate = GetAttributeValue( AttributeKey.ShowMergeTemplate ).AsBoolean();
                            grid.ShowWorkflowOrCustomActionButtons = GetAttributeValue( AttributeKey.ShowLaunchWorkflow ).AsBoolean();
                            grid.EnableStickyHeaders = GetAttributeValue( AttributeKey.EnableStickyHeaderOnGrid ).AsBoolean();

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

                            grid.CommunicateMergeFields = GetAttributeValue( AttributeKey.MergeFields ).SplitDelimitedValues().ToList<string>();
                            grid.CommunicationRecipientPersonIdFields = GetAttributeValue( AttributeKey.CommunicationRecipientPersonIdColumns ).SplitDelimitedValues().ToList();

                            AddGridColumns( grid, dataTable );
                            SetDataKeyNames( grid, dataTable );

                            if ( setData )
                            {
                                FilterTable( grid, dataTable );
                                SortTable( grid, dataTable );
                                grid.DataSource = dataTable;

                                if ( personReport )
                                {
                                    grid.EntityTypeId = EntityTypeCache.GetId<Person>();
                                }

                                grid.DataBind();
                            }
                        }

                        if ( gridFooterLava.IsNotNullOrWhiteSpace() )
                        {
                            var div = new HtmlGenericControl( "div" );
                            div.Controls.Add( new LiteralControl( gridFooterLava.ResolveMergeFields( mergeFields, enabledLavaCommands ) ) );
                            if ( divPanel == null )
                            {
                                phContent.Controls.Add( div );
                            }
                            else
                            {
                                div.AddCssClass( "panel-footer" );
                                divPanel.Controls.Add( div );
                            }
                        }
                    }
                    else
                    {
                        phContent.Controls.Add( new LiteralControl( formattedOutput.ResolveMergeFields( mergeFields, enabledLavaCommands ) ) );
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
            string urlMask = GetAttributeValue( AttributeKey.UrlMask );
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
            string[] queryParams = GetAttributeValue( AttributeKey.QueryParams ).SplitDelimitedValues();
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
            bool showColumns = GetAttributeValue( AttributeKey.ShowColumns ).AsBoolean();
            var columnList = GetAttributeValue( AttributeKey.Columns ).SplitDelimitedValues().ToList();
            var encryptedFields = GetAttributeValue( AttributeKey.EncryptedFields ).SplitDelimitedValues().ToList();

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

                        var value = GridFilter.GetFilterPreference( id );

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
                            if ( dateTime.TimeOfDay.TotalSeconds != 0 )
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

                        var value = GridFilter.GetFilterPreference( id );

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
                    if ( encryptedFields.Contains( dataTableColumn.ColumnName ) )
                    {
                        bf = new EncryptedField();
                    }

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
                        var value = GridFilter.GetFilterPreference( key );

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
            var showGridFilterControls = GetAttributeValue( AttributeKey.ShowGridFilter ).AsBoolean();
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
                        query.Add( string.Format( "[{0}] >= #{1}#", colName, minValue.Value.ToISO8601DateString() ) );
                    }

                    if ( maxValue.HasValue )
                    {
                        query.Add( string.Format( "[{0}] < #{1}#", colName, maxValue.Value.AddDays( 1 ).ToISO8601DateString() ) );
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
                            query.Add( string.Format( "[{0}] LIKE '%{1}%'", colName, value.Replace( "'", "''" ) ) );
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

        private class DataRowLavaData : LavaDataObject
        {
            private readonly DataRow _dataRow;

            public DataRowLavaData( DataRow dataRow )
            {
                _dataRow = dataRow;
            }

            public override List<string> AvailableKeys
            {
                get
                {
                    var keys = new List<string>();

                    foreach ( DataColumn column in _dataRow.Table.Columns )
                    {
                        keys.Add( column.ColumnName );
                    }

                    return keys;
                }
            }

            protected override bool OnTryGetValue( string key, out object result )
            {
                if ( _dataRow.Table.Columns.Contains( key ) )
                {
                    result = _dataRow[key];
                    return true;
                }

                result = null;
                return false;
            }
        }

        #region RockLiquid Lava implementation

        /// <summary>
        ///
        /// </summary>
        private class DataRowDrop : DotLiquid.Drop, ILavaDataDictionary
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

            #region ILavaDataDictionary

            public List<string> AvailableKeys
            {
                get
                {
                    var keys = new List<string>();
                    foreach ( DataColumn column in _dataRow.Table.Columns )
                    {
                        keys.Add( column.ColumnName );
                    }
                    return keys;
                }
            }

            public bool ContainsKey( string key )
            {
                return _dataRow.Table.Columns.Contains( key );
            }

            public object GetValue( string key )
            {
                if ( _dataRow.Table.Columns.Contains( key ) )
                {
                    return _dataRow[key];
                }
                return null;
            }

            #endregion
        }

        #endregion
    }
}