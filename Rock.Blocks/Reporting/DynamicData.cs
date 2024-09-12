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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

using Rock.Attribute;
using Rock.Data;
using Rock.Field.Types;
using Rock.Lava;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Reporting.DynamicData;
using Rock.ViewModels.Cms;
using Rock.ViewModels.Core.Grid;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Reporting
{
    /// <summary>
    /// Block to display dynamic report, html, xml, or transformed xml based on a SQL query or stored procedure.
    /// </summary>

    [DisplayName( "Dynamic Data" )]
    [Category( "Reporting" )]
    [Description( "Block to display dynamic report, html, xml, or transformed xml based on a SQL query or stored procedure." )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #region Non-Custom Settings

    [BooleanField( "Enable Quick Return",
        Key = AttributeKey.EnableQuickReturn,
        Description = "When enabled, viewing the block will cause it to be added to the Quick Return list in the bookmarks feature.",
        ControlType = BooleanFieldType.BooleanControlType.DropDown,
        DefaultBooleanValue = false,
        Order = 0,
        IsRequired = false )]

    [BooleanField( "Update Page",
        Key = AttributeKey.UpdatePage,
        Description = "If True, provides fields for updating the parent page's Name and Description.",
        ControlType = BooleanFieldType.BooleanControlType.DropDown,
        DefaultBooleanValue = true,
        Order = 1,
        IsRequired = false )]

    [LavaCommandsField( "Enabled Lava Commands",
        Key = AttributeKey.EnabledLavaCommands,
        Description = "The Lava commands that should be enabled for this dynamic data block.",
        Order = 2,
        IsRequired = false )]

    #endregion Non-Custom Settings

    #region Shared Settings (shared between grid and Lava results formatting display modes)

    [TextField( "SQL Query",
        Key = AttributeKey.Query,
        Description = "The SQL query or stored procedure name to execute. If you are providing SQL you can add items from the query string using Lava like this: '{{ QueryParmName }}'. If SQL parameters are included they will also need to be in the Parameters field below.<br><span class='tip tip-lava'></span>",
        Category = "CustomSetting",
        IsRequired = false )]

    [BooleanField( "Query Implemented as Stored Procedure",
        Key = AttributeKey.StoredProcedure,
        Description = "When selecting this option, provide only the name of the stored procedure in the Query field. The parameters (if any) for the stored procedure should be configured using the Parameters field.",
        Category = "CustomSetting",
        DefaultBooleanValue = false,
        IsRequired = false )]

    [TextField( "[Query|Stored Procedure] Parameters",
        Key = AttributeKey.QueryParams,
        Description = "Specify the parameters required by the query or stored procedure using the format 'param1=value;param2=value'. Include an equals sign for each parameter. Omitting a value will set it to default to blank. Parameters matching query string values will automatically adopt those values. The 'CurrentPersonId' parameter will be replaced with the ID of the currently logged-in person. This field supports both standard SQL and stored procedures.",
        Category = "CustomSetting",
        IsRequired = false )]

    [IntegerField( "Timeout Length",
        Key = AttributeKey.Timeout,
        Description = "The amount of time in seconds to allow the query to run before timing out.",
        Category = "CustomSetting",
        DefaultIntegerValue = 30,
        IsRequired = false )]

    [CustomDropdownListField( "Results Display Mode",
        Key = AttributeKey.ResultsDisplayMode,
        Description = "Determines how the results should be displayed.",
        Category = "CustomSetting",
        ListSource = "grid^Grid,lavaTemplate^Lava Template",
        DefaultValue = "Grid",
        IsRequired = true )]

    [TextField( "Page Title Lava",
        Key = AttributeKey.PageTitleLava,
        Description = "Optional Lava for setting the page title. If nothing is provided then the page's title will be used. Example '{{rows[0].FullName}}' or if the query returns multiple result sets '{{table1.rows[0].FullName}}'.<br><span class='tip tip-lava'></span>",
        Category = "CustomSetting",
        IsRequired = false )]

    #endregion Shared Settings (shared between grid and Lava results formatting display modes)

    #region Results Formatting - Grid Settings

    [TextField( "Column Configurations",
        Key = AttributeKey.ColumnConfigurations,
        Description = "A JSON object describing how each column of the grid results should be displayed, as well as rules for filtering, exporting, Etc.",
        Category = "CustomSetting",
        IsRequired = false )]

    [BooleanField( "Show Checkbox Selection Column",
        Key = AttributeKey.ShowCheckboxSelectionColumn,
        Description = "Determines whether to show the checkbox select column on the grid as the first column.",
        Category = "CustomSetting",
        DefaultBooleanValue = true,
        IsRequired = false )]

    [BooleanField( "Disable Paging",
        Key = AttributeKey.DisablePaging,
        Description = "Determines whether to disable paging on the grid.",
        Category = "CustomSetting",
        DefaultBooleanValue = false,
        IsRequired = false )]

    [BooleanField( "Person Report",
        Key = AttributeKey.PersonReport,
        Description = "Does this query return a list of people? If it does, then additional options will be available from the result grid. (i.e. Communicate, etc). Note: A column named 'Id' that contains the person's Id is required for a person report.",
        Category = "CustomSetting",
        DefaultBooleanValue = false,
        IsRequired = false )]

    [BooleanField( "Enable Sticky Header",
        Key = AttributeKey.EnableStickyHeaderOnGrid,
        Description = "Determines whether the header on the grid will be sticky at the top of the page.",
        Category = "CustomSetting",
        DefaultBooleanValue = false,
        IsRequired = false )]

    [BooleanField( "Enable Export",
        Key = AttributeKey.ShowExcelExport,
        Description = "Determines whether to show 'Export to Excel' and 'Export to CSV' buttons in the grid.",
        Category = "CustomSetting",
        DefaultBooleanValue = true,
        IsRequired = false )]

    [BooleanField( "Merge Template",
        Key = AttributeKey.ShowMergeTemplate,
        Description = "Determines whether to show 'Merge Template' button in the grid.",
        Category = "CustomSetting",
        DefaultBooleanValue = true,
        IsRequired = false )]

    [BooleanField( "Communications",
        Key = AttributeKey.ShowCommunicate,
        Description = "Determines whether to show 'Communicate' button in the grid.",
        Category = "CustomSetting",
        DefaultBooleanValue = true,
        IsRequired = false )]

    [BooleanField( "Person Merge",
        Key = AttributeKey.ShowMergePerson,
        Description = "Determines whether to show 'Merge Person Records' button in the grid.",
        Category = "CustomSetting",
        DefaultBooleanValue = true,
        IsRequired = false )]

    [BooleanField( "Bulk Update",
        Key = AttributeKey.ShowBulkUpdate,
        Description = "Determines whether to show 'Bulk Update' button in the grid.",
        Category = "CustomSetting",
        DefaultBooleanValue = true,
        IsRequired = false )]

    [BooleanField( "Launch Workflow",
        Key = AttributeKey.ShowLaunchWorkflow,
        Description = "Determines whether to show 'Launch Workflow' button in the grid.",
        Category = "CustomSetting",
        DefaultBooleanValue = true,
        IsRequired = false )]

    [TextField( "Panel Title",
        Key = AttributeKey.PanelTitle,
        Description = "The title of the grid's panel.",
        Category = "CustomSetting",
        IsRequired = false )]

    [TextField( "Selection URL",
        Key = AttributeKey.UrlMask,
        Description = "The URL to redirect individual to when they click on a row in the grid. Any column's value can be used in the URL by including it in braces. For example if the grid includes an 'Id' column that contains Person IDs, you can link to the Person view by specifying a value here of '~/Person/{Id}'.",
        Category = "CustomSetting",
        IsRequired = false )]

    [TextField( "Communication Merge Fields",
        Key = AttributeKey.MergeFields,
        Description = "When creating a new communication from a person report, additional fields from the report can be used as merge fields on the communication. Enter any comma-separated column name(s) that you'd like to be available for the communication. If the same recipient has multiple results in this report, each result will be included in an 'AdditionalFields' list. These can be accessed using Lava in the communication. For example: '{% for field in AdditionalFields %}{{ field.columnName }}{% endfor %}'.",
        Category = "CustomSetting",
        IsRequired = false )]

    [TextField( "Communication Recipient Fields",
        Key = AttributeKey.CommunicationRecipientPersonIdColumns,
        Description = "The comma-separated column name(s) that contain a person ID field to use as the recipient for a communication. If left blank, it will assume a column named 'Id' contains the recipient's person Id.",
        Category = "CustomSetting",
        IsRequired = false )]

    [TextField( "Encrypted Fields",
        Key = AttributeKey.EncryptedFields,
        Description = "The comma-separated column name(s) that need to be decrypted before displaying their value.",
        Category = "CustomSetting",
        IsRequired = false )]

    [TextField( "Grid Header Content",
        Key = AttributeKey.GridHeaderContent,
        Description = "This Lava template will be rendered above the grid. It will have access to the same dataset as the grid.<br><span class='tip tip-lava'></span>",
        Category = "CustomSetting",
        IsRequired = false )]

    [TextField( "Grid Footer Content",
        Key = AttributeKey.GridFooterContent,
        Description = "This Lava template will be rendered below the grid (best used for custom totaling). It will have access to the same dataset as the grid.<br><span class='tip tip-lava'></span>",
        Category = "CustomSetting",
        IsRequired = false )]

    #endregion Results Formatting - Grid Settings

    #region Results Formatting - Lava Settings

    [TextField( "Lava Template",
        Key = AttributeKey.FormattedOutput,
        Description = "Formatting to apply to the returned results. Example: '{% for row in rows %}{{ row.FirstName }}{% endfor %}' or if the query returns multiple result sets: '{% for row in table1.rows %}{{ row.FirstName }}{% endfor %}'. Alternatively, you may iterate over all tables within the returned results. For example: '{% for table in tables %}{% for row in table.rows %}{{ row.FirstName }}{% endfor %}{% endfor %}'.<br><span class='tip tip-lava'></span>",
        Category = "CustomSetting",
        IsRequired = false )]

    #endregion Results Formatting - Lava Settings

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "4AD30955-96AE-422E-AF0A-4D25357692A1" )]
    [Rock.SystemGuid.BlockTypeGuid( "E050BDD0-4B59-4E86-AF68-18B361F76650" )]
    public class DynamicData : RockBlockType, IHasCustomActions
    {
        #region Keys

        private static class AttributeKey
        {
            // Non-Custom Settings.
            public const string EnableQuickReturn = "EnableQuickReturn";
            public const string UpdatePage = "UpdatePage";
            public const string EnabledLavaCommands = "EnabledLavaCommands";

            // Shared Settings (shared between grid and Lava results formatting display modes).
            public const string Query = "Query";
            public const string StoredProcedure = "StoredProcedure";
            public const string QueryParams = "QueryParams";
            public const string Timeout = "Timeout";
            public const string ResultsDisplayMode = "ResultsDisplayMode";

            public const string PageTitleLava = "PageTitleLava";

            // Results Formatting - Grid Settings.
            public const string ColumnConfigurations = "ColumnConfigurations";

            public const string ShowCheckboxSelectionColumn = "ShowCheckboxSelectionColumn";
            public const string DisablePaging = "DisablePaging";
            public const string PersonReport = "PersonReport";
            public const string EnableStickyHeaderOnGrid = "EnableStickyHeaderOnGrid";

            public const string ShowExcelExport = "ShowExcelExport";
            public const string ShowMergeTemplate = "ShowMergeTemplate";
            public const string ShowCommunicate = "ShowCommunicate";
            public const string ShowMergePerson = "ShowMergePerson";
            public const string ShowBulkUpdate = "ShowBulkUpdate";
            public const string ShowLaunchWorkflow = "ShowLaunchWorkflow";

            public const string PanelTitle = "PanelTitle";
            public const string UrlMask = "UrlMask";
            public const string MergeFields = "MergeFields";
            public const string CommunicationRecipientPersonIdColumns = "CommunicationRecipientPersonIdColumns";

            public const string EncryptedFields = "EncryptedFields";
            public const string GridHeaderContent = "GridHeaderContent";
            public const string GridFooterContent = "GridFooterContent";

            // Results Formatting - Lava Settings.
            public const string FormattedOutput = "FormattedOutput";
        }

        private static class NavigationUrlKey
        {
            public const string RowSelection = "RowSelection";
        }

        #endregion Keys

        #region Fields

        private List<ListItemBag> _displayModeItems;

        private List<ListItemBag> _columnTypeItems;

        private List<ListItemBag> _visiblePriorityItems;

        private List<string> _keyFieldColumnTypes = new List<string>
        {
            ColumnType.TextValue,
            ColumnType.NumberValue,
            ColumnType.PersonValue
        };

        private static Regex _selectionUrlRegex = new Regex( @"\{[\w\s]+\}" );

        #endregion Fields

        #region Properties

        private List<ListItemBag> DisplayModeItems
        {
            get
            {
                if ( _displayModeItems == null )
                {
                    _displayModeItems = new List<ListItemBag>
                    {
                        DisplayMode.Grid,
                        DisplayMode.LavaTemplate
                    };
                }

                return _displayModeItems;
            }
        }

        private List<ListItemBag> ColumnTypeItems
        {
            get
            {
                if ( _columnTypeItems == null )
                {
                    _columnTypeItems = new List<ListItemBag>
                    {
                        ColumnType.Text,
                        ColumnType.Boolean,
                        ColumnType.Currency,
                        ColumnType.Date,
                        ColumnType.DateTime,
                        ColumnType.Number,
                        ColumnType.Person
                    };
                }

                return _columnTypeItems;
            }
        }

        private List<ListItemBag> VisiblePriorityItems
        {
            get
            {
                if ( _visiblePriorityItems == null )
                {
                    _visiblePriorityItems = new List<ListItemBag>
                    {
                        VisiblePriority.ExtraSmall,
                        VisiblePriority.Small,
                        VisiblePriority.Medium,
                        VisiblePriority.Large,
                        VisiblePriority.ExtraLarge
                    };
                }

                return _visiblePriorityItems;
            }
        }

        private List<string> KeyFieldColumnTypes => _keyFieldColumnTypes;

        private static Regex SelectionUrlRegex => _selectionUrlRegex;

        #endregion Properties

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new DynamicDataInitializationBox();

            var dataResults = GetDataResultsForDisplay();
            if ( dataResults.ErrorMessage.IsNotNullOrWhiteSpace() )
            {
                box.ErrorMessage = dataResults.ErrorMessage;
                return box;
            }

            box.IsLavaTemplateDisplayMode = dataResults.Config.IsLavaTemplateDisplayMode;
            box.NavigationUrls = GetBoxNavigationUrls( dataResults );
            box.SecurityGrantToken = GetSecurityGrantToken();

            SetDynamicPageTitle( dataResults );

            return box;
        }

        /// <summary>
        /// Gets the data results for display.
        /// </summary>
        /// <param name="forceLoadDataRows">Whether to force the data rows to load, regardless of other settings.</param>
        /// <returns>The data results for display.</returns>
        private DynamicDataResults GetDataResultsForDisplay( bool forceLoadDataRows = false )
        {
            // Load the schema first, to ensure we don't encounter an error processing the query.
            var config = new DynamicDataConfig
            {
                // Set only the values required for data set loading.
                DataSetLoadingMode = DataSetLoadingMode.LoadSchemaOnly,
                Query = GetAttributeValue( AttributeKey.Query ),
                IsStoredProcedure = GetAttributeValue( AttributeKey.StoredProcedure ).AsBoolean(),
                QueryParams = GetAttributeValue( AttributeKey.QueryParams ).SplitDelimitedValues(),
                Timeout = GetAttributeValue( AttributeKey.Timeout ).AsIntegerOrNull(),
                EnabledLavaCommands = GetAttributeValue( AttributeKey.EnabledLavaCommands ),
                MergeFields = GetMergeFields()
            };

            var dataResults = GetDataResults( config );
            if ( dataResults.ErrorMessage.IsNotNullOrWhiteSpace() )
            {
                return dataResults;
            }

            // Set display-related attribute values on the outgoing results object for use when building the display.
            // Note: `config` (the DynamicDataConfig instance) was already set on the outgoing `dataResults` object
            // within the `GetDataResults()` method call above.
            config.PageTitleLava = GetAttributeValue( AttributeKey.PageTitleLava );
            config.ResultsDisplayMode = GetAttributeValue( AttributeKey.ResultsDisplayMode );

            if ( config.IsLavaTemplateDisplayMode )
            {
                config.LavaTemplate = GetAttributeValue( AttributeKey.FormattedOutput );
            }
            else
            {
                config.ColumnConfigurations = DeserializeColumnConfigurations( GetAttributeValue( AttributeKey.ColumnConfigurations ) );
                config.ShowCheckboxSelectionColumn = GetAttributeValue( AttributeKey.ShowCheckboxSelectionColumn ).AsBoolean();
                config.DisablePaging = GetAttributeValue( AttributeKey.DisablePaging ).AsBoolean();
                config.IsPersonReport = GetAttributeValue( AttributeKey.PersonReport ).AsBoolean();
                config.EnableStickyHeader = GetAttributeValue( AttributeKey.EnableStickyHeaderOnGrid ).AsBoolean();
                config.EnableExport = GetAttributeValue( AttributeKey.ShowExcelExport ).AsBoolean();
                config.EnableMergeTemplate = GetAttributeValue( AttributeKey.ShowMergeTemplate ).AsBoolean();
                config.EnableCommunications = GetAttributeValue( AttributeKey.ShowCommunicate ).AsBoolean();
                config.EnablePersonMerge = GetAttributeValue( AttributeKey.ShowMergePerson ).AsBoolean();
                config.EnableBulkUpdate = GetAttributeValue( AttributeKey.ShowBulkUpdate ).AsBoolean();
                config.EnableLaunchWorkflow = GetAttributeValue( AttributeKey.ShowLaunchWorkflow ).AsBoolean();
                config.GridTitle = GetAttributeValue( AttributeKey.PanelTitle );
                config.SelectionUrl = GetAttributeValue( AttributeKey.UrlMask );
                config.CommunicationMergeFields = SanitizeCommaSeparatedColumnNames( GetAttributeValue( AttributeKey.MergeFields ) ).SplitDelimitedValues( whitespace: false );
                config.CommunicationRecipientFields = SanitizeCommaSeparatedColumnNames( GetAttributeValue( AttributeKey.CommunicationRecipientPersonIdColumns ) ).SplitDelimitedValues( whitespace: false );
                config.EncryptedFields = SanitizeCommaSeparatedColumnNames( GetAttributeValue( AttributeKey.EncryptedFields ) ).SplitDelimitedValues( whitespace: false );
                config.GridHeaderContent = GetAttributeValue( AttributeKey.GridHeaderContent );
                config.GridFooterContent = GetAttributeValue( AttributeKey.GridFooterContent );
            }

            // Do we need to ensure the data rows are loaded?
            if ( forceLoadDataRows
                || config.PageTitleLava.IsNotNullOrWhiteSpace()
                || ( config.IsLavaTemplateDisplayMode && config.LavaTemplate.IsNotNullOrWhiteSpace() )
                || ( !config.IsLavaTemplateDisplayMode && ( config.GridHeaderContent.IsNotNullOrWhiteSpace() || config.GridFooterContent.IsNotNullOrWhiteSpace() ) ) )
            {
                // Even though we specified to load only the schema above, it's possible the full data
                // set was already loaded as a fallback. If so, there is no need to load the data again.
                if ( !dataResults.WasFullDataSetLoaded )
                {
                    config.DataSetLoadingMode = DataSetLoadingMode.LoadSchemaAndData;
                    dataResults = GetDataResults( config );

                    // Final check to ensure this latest data load didn't result in an error.
                    if ( dataResults.ErrorMessage.IsNotNullOrWhiteSpace() )
                    {
                        return dataResults;
                    }
                }

                // Supplement the merge fields with data results (tables and rows).
                AddDataResultsToMergeFields( dataResults );
            }

            // Finally, set the lava template or grid results.
            if ( config.IsLavaTemplateDisplayMode )
            {
                SetLavaTemplateResults( dataResults );
            }
            else
            {
                dataResults.ActualColumnConfigurations = LoadColumnConfigurationsFromDataSet( dataResults.DataSet, config.ColumnConfigurations, includeHiddenKeyColumns: true );
                SetGridResults( dataResults );
            }

            return dataResults;
        }

        /// <summary>
        /// Gets the merge fields.
        /// </summary>
        /// <returns>The merge fields.</returns>
        private Dictionary<string, object> GetMergeFields()
        {
            var mergeFields = this.RequestContext.GetCommonMergeFields();
            if ( this.RequestContext.CurrentPerson != null )
            {
                // TODO: When support for "Person" is not supported anymore (should use "CurrentPerson" instead), remove this line.
                mergeFields.Add( "Person", this.RequestContext.CurrentPerson );
            }

            mergeFields.Add( "RockVersion", Rock.VersionInfo.VersionInfo.GetRockProductVersionNumber() );
            mergeFields.Add( "CurrentPage", this.PageCache );

            return mergeFields;
        }

        /// <summary>
        /// Gets the dynamic data results.
        /// </summary>
        /// <param name="config">The config object to dictate how the data should be loaded.</param>
        /// <returns>The dynamic data results.</returns>
        private DynamicDataResults GetDataResults( DynamicDataConfig config )
        {
            var results = new DynamicDataResults
            {
                Config = config
            };

            if ( config.Query.IsNullOrWhiteSpace() )
            {
                results.ErrorMessage = "No query was provided.";
                return results;
            }

            try
            {
                var commandType = config.IsStoredProcedure ? CommandType.StoredProcedure : CommandType.Text;
                var sqlParameters = GetSqlParameters( config.QueryParams );
                var timeout = config.Timeout ?? 30;
                var mergeFields = config.MergeFields;

                // NOTE: There is already a PageParameters merge field within common merge fields, but for
                // backwards compatibility, also add each of the page parameters as plain merge fields.
                foreach ( var pageParam in this.RequestContext.GetPageParameters() )
                {
                    mergeFields.AddOrReplace( pageParam.Key, pageParam.Value );
                }

                var query = config.Query.ResolveMergeFields( mergeFields, config.EnabledLavaCommands );
                var schemaOnly = config.DataSetLoadingMode == DataSetLoadingMode.LoadSchemaOnly;

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

                // A local function to add the schema and data to the results object.
                void GetSchemaAndData()
                {
                    results.DataSet = DbService.GetDataSet( query, commandType, sqlParameters, timeout );
                    results.WasFullDataSetLoaded = true;
                }

                if ( schemaOnly )
                {
                    try
                    {
                        // GetDataSetSchema won't work in some cases; fall back to use the regular GetDataSet
                        // if there is an exception or the schema does not return any tables.
                        var dataSet = DbService.GetDataSetSchema( query, commandType, sqlParameters, timeout );
                        if ( dataSet != null && dataSet.Tables.Count > 0 )
                        {
                            results.DataSet = dataSet;
                        }
                        else
                        {
                            GetSchemaAndData();
                        }
                    }
                    catch
                    {
                        GetSchemaAndData();
                    }
                }
                else
                {
                    GetSchemaAndData();
                }
            }
            catch ( Exception ex )
            {
                results.ErrorMessage = ex.Message;
            }

            return results;
        }

        /// <summary>
        /// Gets the SQL parameters to be passed to the query or stored procedure.
        /// </summary>
        /// <param name="queryParams">The query parameters from block settings.</param>
        /// <returns>The SQL parameters to be passed to the query or stored procedure.</returns>
        private Dictionary<string, object> GetSqlParameters( string[] queryParams )
        {
            if ( queryParams.Length > 0 )
            {
                var sqlParameters = new Dictionary<string, object>();
                foreach ( string queryParam in queryParams )
                {
                    var paramParts = queryParam.Split( '=' );
                    if ( paramParts.Length == 2 )
                    {
                        var queryParamName = paramParts[0];
                        var queryParamValue = paramParts[1];

                        // Remove leading '@' character if was included.
                        if ( queryParamName.StartsWith( "@" ) )
                        {
                            queryParamName = queryParamName.Substring( 1 );
                        }

                        // If a page parameter (query or form) value matches, use it's value instead.
                        var pageValue = PageParameter( queryParamName );
                        if ( !pageValue.IsNullOrWhiteSpace() )
                        {
                            queryParamValue = pageValue;
                        }
                        else if ( queryParamName.ToLower() == "currentpersonid" && this.RequestContext.CurrentPerson != null )
                        {
                            // If current person ID, use the value of the current person ID.
                            queryParamValue = this.RequestContext.CurrentPerson.Id.ToString();
                        }

                        sqlParameters.Add( queryParamName, queryParamValue );
                    }
                }

                return sqlParameters;
            }

            return null;
        }

        /// <summary>
        /// Deserializes the provided JSON string into a list of column configuration objects.
        /// </summary>
        /// <returns>A list of column configuration objects.</returns>
        private List<ColumnConfigurationBag> DeserializeColumnConfigurations( string columnConfigurationsJson )
        {
            var columnConfigurations = columnConfigurationsJson.FromJsonOrNull<List<ColumnConfigurationBag>>() ?? new List<ColumnConfigurationBag>();

            SetColumnConfigurationNames( columnConfigurations );

            return columnConfigurations;
        }

        /// <summary>
        /// Serializes the provided list of column configuration objects into a JSON string.
        /// </summary>
        /// <returns>A JSON string representing the list of column configuration objects.</returns>
        private string SerializeColumnConfigurations( List<ColumnConfigurationBag> columnConfigurations )
        {
            var columnConfigurationsJson = string.Empty;

            if ( columnConfigurations?.Any() == true )
            {
                SetColumnConfigurationNames( columnConfigurations );
                columnConfigurationsJson = columnConfigurations.ToJson();
            }

            return columnConfigurationsJson;
        }

        /// <summary>
        /// Sets each column configuration's <see cref="ColumnConfigurationBag.SplitCaseName"/>
        /// and <see cref="ColumnConfigurationBag.CamelCaseName"/> property values based on its
        /// current <see cref="ColumnConfigurationBag.Name"/> property value.
        /// </summary>
        /// <param name="columnConfigurations"></param>
        private void SetColumnConfigurationNames( List<ColumnConfigurationBag> columnConfigurations )
        {
            columnConfigurations?.ForEach( c =>
            {
                c.Name = c.Name ?? string.Empty;
                c.SplitCaseName = c.Name.SplitCase().ReplaceWhileExists( "  ", " " );

                // Do not change this to use Rock's `ToCamelCase()` extension method,
                // as this block's private `GetCamelCase()` method is more thorough,
                // and will cover cases that are more likely to be encountered within
                // the the block's dynamic query value.
                c.CamelCaseName = GetCamelCase( c.SplitCaseName );
            } );
        }

        /// Gets the camelCase representation of the provided string.
        /// <para>
        /// https://code-maze.com/csharp-convert-string-titlecase-camelcase/
        /// </para>
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>A camelCase representation of the provided string.</returns>
        private string GetCamelCase( string str )
        {
            if ( str.IsNullOrWhiteSpace() )
            {
                return str;
            }

            var words = str.Split( new[] { "_", " " }, StringSplitOptions.RemoveEmptyEntries );

            var leadWord = Regex.Replace( words[0], @"([A-Z])([A-Z]+|[a-z0-9]+)($|[A-Z]\w*)",
                m =>
                {
                    return m.Groups[1].Value.ToLower() + m.Groups[2].Value.ToLower() + m.Groups[3].Value;
                } );

            var tailWords = words.Skip( 1 )
                .Select( word => char.ToUpper( word[0] ) + word.Substring( 1 ).ToLower() )
                .ToArray();

            return $"{leadWord}{string.Join( string.Empty, tailWords )}";
        }

        /// <summary>
        /// Sanitizes a comma-separated list of column names to ensure we're
        /// always working with consistent, expected values.
        /// </summary>
        /// <param name="input">The comma-separated list of column names to sanitize.</param>
        /// <returns>A sanitized version of the comma-separated column names.</returns>
        private string SanitizeCommaSeparatedColumnNames( string input )
        {
            if ( input.IsNullOrWhiteSpace() )
            {
                return string.Empty;
            }

            var sanitized = input
                .ReplaceWhileExists( "  ", " " )
                .Replace( "[", string.Empty )
                .Replace( "]", string.Empty )
                .SplitDelimitedValues( ",", System.StringSplitOptions.RemoveEmptyEntries )
                .Select( a => a.Trim() )
                .JoinStrings( "," );

            return sanitized;
        }

        /// <summary>
        /// Adds the data results (tables and rows) to the provided dynamic data results object's merge fields.
        /// </summary>
        /// <param name="dataResults">The dynamic data results onto which to add the merge fields.</param>
        private void AddDataResultsToMergeFields( DynamicDataResults dataResults )
        {
            var dataSet = dataResults?.DataSet;
            if ( dataSet == null || dataSet.Tables.Count == 0 )
            {
                return;
            }

            var mergeFields = dataResults.Config.MergeFields;
            var i = 1;

            if ( LavaService.RockLiquidIsEnabled )
            {
                var tableFields = new List<Dictionary<string, object>>();
                foreach ( DataTable dataTable in dataSet.Tables )
                {
                    var lavaRows = new List<DataRowDrop>();
                    foreach ( DataRow row in dataTable.Rows )
                    {
                        lavaRows.Add( new DataRowDrop( row ) );
                    }

                    // Add a collection of all the tables as a merge field (even if only one table),
                    // to allow the template to iterate over them.
                    var tableField = new Dictionary<string, object>
                    {
                        { "rows", lavaRows }
                    };

                    tableFields.Add( tableField );

                    // Continue to add the following merge fields to maintain compatibility with
                    // lava templates that were written before this Obsidian version of the block
                    // (and therefore before the 'tables' merge field) was added.
                    if ( dataSet.Tables.Count > 1 )
                    {
                        mergeFields.Add( "table" + i.ToString(), tableField );
                    }
                    else
                    {
                        mergeFields.Add( "rows", lavaRows );
                    }

                    i++;
                }

                mergeFields.Add( "tables", tableFields );
            }
            else
            {
                var tableFields = new List<Dictionary<string, object>>();
                foreach ( DataTable dataTable in dataSet.Tables )
                {
                    var lavaRows = new List<DataRowLavaData>();
                    foreach ( DataRow row in dataTable.Rows )
                    {
                        lavaRows.Add( new DataRowLavaData( row ) );
                    }

                    // Add a collection of all the tables as a merge field (even if only one table),
                    // to allow the template to iterate over them.
                    var tableField = new Dictionary<string, object>
                    {
                        { "rows", lavaRows }
                    };

                    tableFields.Add( tableField );

                    // Continue to add the following merge fields to maintain compatibility with
                    // lava templates that were written before this Obsidian version of the block
                    // (and therefore before the 'tables' merge field) was added.
                    if ( dataSet.Tables.Count > 1 )
                    {
                        mergeFields.Add( "table" + i.ToString(), tableField );
                    }
                    else
                    {
                        mergeFields.Add( "rows", lavaRows );
                    }

                    i++;
                }

                mergeFields.Add( "tables", tableFields );
            }
        }

        /// <summary>
        /// Sets the lava template results on the provided dynamic data results object.
        /// </summary>
        /// <param name="dataResults">The dynamic data results onto which to set the lava template results.</param>
        private void SetLavaTemplateResults( DynamicDataResults dataResults )
        {
            if ( dataResults == null )
            {
                return;
            }

            var mergeFields = dataResults.Config?.MergeFields;
            var enabledLavaCommands = dataResults.Config?.EnabledLavaCommands;

            dataResults.LavaTemplateResults = new LavaTemplateResultsBag
            {
                ResultsHtml = dataResults.Config?.LavaTemplate.ResolveMergeFields( mergeFields, enabledLavaCommands )
            };
        }

        /// <summary>
        /// Sets the grid results on the provided dynamic data results object.
        /// <para>
        /// This will set the grid definition and information about enabled grid features, but will not
        /// set grid data rows; that data should instead be loaded on demand by the Obsidian grid client.
        /// </para>
        /// </summary>
        /// <param name="dataResults">The dynamic data results onto which to set the grid results.</param>
        private void SetGridResults( DynamicDataResults dataResults )
        {
            var config = dataResults?.Config;
            if ( config == null )
            {
                if ( dataResults != null )
                {
                    dataResults.GridResults = null;
                }

                return;
            }

            string keyField = null;
            string personKeyField = null;

            var columnConfigurations = dataResults.ActualColumnConfigurations;

            if ( config.SelectionUrl.IsNotNullOrWhiteSpace() )
            {
                /*
                    5/15/2024: JPH

                    To retain the way the legacy version of this block worked, we need to set the grid's key
                    field to be the first key parsed from the Selection URL block setting. However, because
                    the Obsidian grid can only natively work with text or number key fields (and person fields,
                    with the client's use of `getValueFromPath()` logic), we'll be a little more selective in
                    our setting of the key field, to ensure the first key parsed is one of these 3 types.

                    The legacy block actually went one step further to gather ALL keys parsed from within the
                    Selection URL, and set them on the grid's `DataKeyNames` property. However, those combined
                    values were only used to build the dynamic redirection URL when a given grid row was clicked;
                    we can mimic this behavior in the Obsidian client code, as we'll have all needed data there,
                    so we don't need to worry about doing that here.

                    Reason: Retain behavior of legacy Dynamic Data block.
                    https://github.com/SparkDevNetwork/Rock/blob/7780d2aff7ae2e67d557b77565023bd796ac015c/RockWeb/Blocks/Reporting/DynamicData.ascx.cs#L963-L995
                    https://github.com/SparkDevNetwork/Rock/blob/7780d2aff7ae2e67d557b77565023bd796ac015c/Rock/Web/UI/Controls/Grid/Grid.cs#L522
                 */
                var selectionUrlMatches = config.SelectionUrlMatches;
                if ( selectionUrlMatches.Count > 0 )
                {
                    var columnName = selectionUrlMatches[0].Value.TrimStart( '{' ).TrimEnd( '}' ).Trim();
                    keyField = GetGridField( columnName, columnConfigurations, isKeyField: true );
                }
            }

            // If we didn't already find the key field, look for an [Id] column of the correct type.
            if ( keyField.IsNullOrWhiteSpace() )
            {
                keyField = GetGridField( "Id", columnConfigurations, isKeyField: true );
            }

            var isPersonReport = config.IsPersonReport;
            var communicationRecipientFields = GetGridFields( config.CommunicationRecipientFields, columnConfigurations, areKeyFields: true );

            if ( isPersonReport )
            {
                if ( keyField.IsNotNullOrWhiteSpace() )
                {
                    // Set the person key field to match the key field we already found.
                    personKeyField = keyField;
                }
                else
                {
                    // We're employing some fuzzy logic here:
                    //      If a key field wasn't found,
                    //      AND the admin told us this is a person report,
                    //      AND there is at least one column of type "Person",
                    //
                    // we'll defer to using that column as the key field.
                    var personColumn = columnConfigurations.FirstOrDefault( c => c.ColumnType == ColumnType.PersonValue );
                    if ( personColumn == null )
                    {
                        isPersonReport = false;
                    }
                    else
                    {
                        keyField = GetPersonKeyField( personColumn, columnConfigurations );
                        personKeyField = keyField;
                    }
                }

                if ( isPersonReport && !communicationRecipientFields.Any() )
                {
                    // Explicitly add the person key field if no other communication
                    // recipient fields were defined in block settings.
                    communicationRecipientFields.Add( personKeyField );
                }
            }

            var enableMergeTemplate = config.EnableMergeTemplate;
            if ( enableMergeTemplate && keyField.IsNullOrWhiteSpace() )
            {
                // A key field is required for merge templates to work.
                enableMergeTemplate = false;
            }

            var mergeFields = config.MergeFields;
            var enabledLavaCommands = config.EnabledLavaCommands;

            dataResults.GridResults = new GridResultsBag
            {
                GridDefinition = GetGridBuilder( dataResults ).BuildDefinition(),
                Title = config.GridTitle,
                KeyField = keyField,
                ShowCheckboxSelectionColumn = config.ShowCheckboxSelectionColumn,
                DisablePaging = config.DisablePaging,
                EnableStickyHeader = config.EnableStickyHeader,
                EnableExport = config.EnableExport,
                EnableMergeTemplate = enableMergeTemplate,
                IsPersonReport = isPersonReport,
                PersonKeyField = personKeyField,
                EnableCommunications = isPersonReport && config.EnableCommunications,
                EnablePersonMerge = isPersonReport && config.EnablePersonMerge,
                EnableBulkUpdate = isPersonReport && config.EnableBulkUpdate,
                EnableLaunchWorkflow = isPersonReport && config.EnableLaunchWorkflow,
                CommunicationMergeFields = GetGridFields( config.CommunicationMergeFields, columnConfigurations, areKeyFields: false ),
                CommunicationRecipientFields = communicationRecipientFields,
                GridHeaderHtml = config.GridHeaderContent.ResolveMergeFields( mergeFields, enabledLavaCommands ),
                GridFooterHtml = config.GridFooterContent.ResolveMergeFields( mergeFields, enabledLavaCommands )
            };
        }

        /// <summary>
        /// Gets the field names or paths for the provided column names.
        /// </summary>
        /// <param name="columnNames">The column names for which to get field names or paths.</param>
        /// <param name="columnConfigurations">The column configurations to search.</param>
        /// <param name="areKeyFields">Whether the fields in question are key fields and therefore MUST be of type text, number or person.</param>
        /// <returns>The field names or paths.</returns>
        private List<string> GetGridFields( IEnumerable<string> columnNames, List<ColumnConfigurationBag> columnConfigurations, bool areKeyFields )
        {
            var fields = new List<string>();

            if ( columnNames?.Any() != true )
            {
                return fields;
            }

            foreach ( var columnName in columnNames )
            {
                var field = GetGridField( columnName, columnConfigurations, isKeyField: areKeyFields );
                if ( field.IsNotNullOrWhiteSpace() && !fields.Contains( field ) )
                {
                    fields.Add( field );
                }
            }

            return fields;
        }

        /// <summary>
        /// Gets the grid field name or path for the provided column name.
        /// </summary>
        /// <param name="columnName">The column name for which to get the grid field name or path.</param>
        /// <param name="columnConfigurations">The column configurations to search.</param>
        /// <param name="isKeyField">Whether the field in question is a key field, and therefore MUST be of type text, number or person.</param>
        /// <param name="forceKeyFieldForPersonColumns">Whether to force key field requirements on columns of type person.</param>
        /// <returns>The field name or path.</returns>
        private string GetGridField( string columnName, List<ColumnConfigurationBag> columnConfigurations, bool isKeyField, bool forceKeyFieldForPersonColumns = false )
        {
            if ( columnName.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var columnNameUpper = columnName.ToUpper();
            var column = columnConfigurations
                ?.FirstOrDefault( c => c.Name.ToUpper() == columnNameUpper || c.SplitCaseName.ToUpper() == columnNameUpper );

            if ( column == null || column.CamelCaseName.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var isPersonColumn = column.ColumnType == ColumnType.PersonValue;

            if ( isKeyField || ( isPersonColumn && forceKeyFieldForPersonColumns ) )
            {
                // The Obsidian grid only natively supports key fields of type text or number
                // (and person fields, with the client's use of `getValueFromPath()` logic).
                // If this column is not one of these 3 types, return null.
                if ( !this.KeyFieldColumnTypes.Contains( column.ColumnType ) )
                {
                    return null;
                }

                if ( isPersonColumn )
                {
                    return GetPersonKeyField( column, columnConfigurations );
                }
            }

            return column.CamelCaseName;
        }

        /// <summary>
        ///  Gets the person key field from the hidden key column configuration.
        /// </summary>
        /// <param name="column">The person column configuration for which to get the key field.</param>
        /// <param name="columnConfigurations">The column configurations to search.</param>
        /// <returns>The person key field.</returns>
        private string GetPersonKeyField( ColumnConfigurationBag column, List<ColumnConfigurationBag> columnConfigurations )
        {
            if ( column?.ColumnType != ColumnType.PersonValue )
            {
                return null;
            }

            if ( columnConfigurations?.Any() == true )
            {
                // Try to find the matching, hidden key column configuration.
                var hiddenKeyColumn = columnConfigurations
                    .FirstOrDefault( c => c.Name.ToUpper() == GetHiddenKeyColumnName( c.ActualColumnName ).ToUpper() );

                if ( hiddenKeyColumn?.CamelCaseName.IsNotNullOrWhiteSpace() == true )
                {
                    return hiddenKeyColumn.CamelCaseName;
                }
            }

            if ( column.CamelCaseName.IsNullOrWhiteSpace() )
            {
                return null;
            }

            // This should never happen, but as a fallback just in case,
            // provide the path to the IdKey within a person field bag,
            // as the Obsidian grid knows how to use this path.
            return $"{column.CamelCaseName}.idKey";
        }

        /// <summary>
        /// Gets the grid builder based on the non-hidden column configurations on the provided dynamic data results.
        /// </summary>
        /// <param name="dataResults">The dynamic data results containing the column configurations.</param>
        /// <returns>The grid builder.</returns>
        private GridBuilder<DataRow> GetGridBuilder( DynamicDataResults dataResults )
        {
            if ( dataResults.GridBuilder != null )
            {
                // If we've already constructed the grid builder, no need to do so again.
                return dataResults.GridBuilder;
            }

            var gridBuilder = new GridBuilder<DataRow>()
                .WithBlock( this );

            var columnConfigurations = dataResults?.ActualColumnConfigurations.ToList();
            if ( columnConfigurations?.Any() == true )
            {
                var personService = new PersonService( this.RockContext );
                var encryptedFields = dataResults.Config
                    ?.EncryptedFields
                    ?.Where( f => f.IsNotNullOrWhiteSpace() )
                    .Select( f => f.ToUpper() )
                    .ToHashSet();

                foreach ( var columnConfiguration in columnConfigurations )
                {
                    var columnType = columnConfiguration.ColumnType;
                    var actualColumnName = columnConfiguration.ActualColumnName;
                    var splitCaseName = columnConfiguration.SplitCaseName;
                    var camelCaseName = columnConfiguration.CamelCaseName;

                    var isEncrypted = encryptedFields
                        ?.Any( f => f == actualColumnName.ToUpper() || f == splitCaseName.ToUpper() ) == true;

                    var fieldProperties = new Dictionary<string, object>();

                    switch ( columnType )
                    {
                        case ColumnType.BooleanValue:
                            gridBuilder.AddField( camelCaseName, dataRow =>
                            {
                                var value = dataRow[actualColumnName]?.ToString();

                                if ( isEncrypted )
                                {
                                    value = Rock.Security.Encryption.DecryptString( value );
                                }

                                return value.AsBoolean();
                            } );
                            break;
                        case ColumnType.PersonValue:
                            gridBuilder.AddPersonField( camelCaseName, dataRow =>
                            {
                                var personKey = dataRow[actualColumnName]?.ToString();

                                if ( isEncrypted )
                                {
                                    personKey = Rock.Security.Encryption.DecryptString( personKey );
                                }

                                if ( personKey.IsNullOrWhiteSpace() )
                                {
                                    return null;
                                }

                                return personService.Get( personKey, !PageCache.Layout.Site.DisablePredictableIds );
                            } );

                            fieldProperties.Add( "showLastNameFirst", columnConfiguration.PersonShowLastNameFirst );

                            break;
                        default:
                            gridBuilder.AddField( camelCaseName, dataRow =>
                            {
                                var value = dataRow[actualColumnName];

                                if ( isEncrypted && value is string )
                                {
                                    return Rock.Security.Encryption.DecryptString( value?.ToString() );
                                }

                                // Most values don't need to be transformed in any way; just return the object value.
                                return value;
                            } );
                            break;
                    }

                    var visiblePriority = columnConfiguration.VisiblePriority;
                    if ( !VisiblePriorityItems.Any( p => p.Value == visiblePriority ) )
                    {
                        // Default to extra-small.
                        visiblePriority = VisiblePriority.ExtraSmall.Value;
                    }

                    gridBuilder.AddDefinitionAction( definition =>
                    {
                        definition.DynamicFields.Add( new DynamicFieldDefinitionBag
                        {
                            Name = camelCaseName,
                            Title = splitCaseName,
                            ColumnType = columnType,
                            HideOnScreen = columnConfiguration.HideFromGrid,
                            VisiblePriority = visiblePriority,
                            Width = columnConfiguration.Width,
                            EnableFiltering = columnConfiguration.EnableFiltering,
                            ExcludeFromExport = columnConfiguration.ExcludeFromExport,
                            FieldProperties = fieldProperties
                        } );
                    } );
                }
            }

            // Set the builder on the data results object in case anything else in this
            // request/response cycle needs it.
            dataResults.GridBuilder = gridBuilder;

            return gridBuilder;
        }

        /// <summary>
        /// Loads column configurations from the first table in the provided data set.
        /// </summary>
        /// <param name="dataSet">The data set.</param>
        /// <param name="existingColumnConfigurations">The existing column configurations to be preserved if matching columns are found.</param>
        /// <param name="includeHiddenKeyColumns">Whether to include runtime columns representing any underlying person keys.</param>
        /// <returns>Column configurations from the first table in the provided data set.</returns>
        private List<ColumnConfigurationBag> LoadColumnConfigurationsFromDataSet( DataSet dataSet, List<ColumnConfigurationBag> existingColumnConfigurations, bool includeHiddenKeyColumns = false )
        {
            var columnConfigurations = new List<ColumnConfigurationBag>();

            if ( dataSet != null && dataSet.Tables.Count > 0 )
            {
                var firstTable = dataSet.Tables[0];

                foreach ( DataColumn dataColumn in firstTable.Columns )
                {
                    var columnName = dataColumn.ColumnName;

                    var column = existingColumnConfigurations?.FirstOrDefault( c => c.Name.ToUpper() == columnName.ToUpper() );
                    if ( column != null )
                    {
                        // Ensure this is set properly since we'll be relying on it
                        // when working with columns throughout this block.
                        column.ActualColumnName = columnName;

                        columnConfigurations.Add( column );

                        if ( includeHiddenKeyColumns && column.ColumnType == ColumnType.PersonValue )
                        {
                            // We need to add a runtime, hidden column configuration to represent the
                            // actual, underlying person key value, so person features will work as
                            // expected throughout the grid.
                            var hiddenKeyColumn = new ColumnConfigurationBag
                            {
                                ActualColumnName = columnName,
                                Name = GetHiddenKeyColumnName( columnName ),
                                ColumnType = ColumnType.TextValue,
                                HideFromGrid = true,
                                ExcludeFromExport = true
                            };

                            columnConfigurations.Add( hiddenKeyColumn );
                        }

                        continue;
                    }

                    // Existing column configuration not found; add a new one with default values.
                    column = new ColumnConfigurationBag
                    {
                        ActualColumnName = columnName,
                        Name = columnName,
                        ColumnType = ColumnType.TextValue
                    };

                    columnConfigurations.Add( column );
                }
            }

            SetColumnConfigurationNames( columnConfigurations );

            return columnConfigurations;
        }

        /// <summary>
        /// Gets a hidden key column name based on the actual column name.
        /// </summary>
        /// <param name="actualColumnName">The actual column name.</param>
        /// <returns>The hidden key column name.</returns>
        private string GetHiddenKeyColumnName( string actualColumnName )
        {
            // Append a suffix that is highly unlikely to conflict with any column names the admin will choose.
            return $"{actualColumnName}_RockDynamicDataKey";
        }

        /// <summary>
        /// Sets the dynamic page title on the provided data results object, as well as the response context. If page
        /// title lava is defined, the dynamic page title will be set by resolving the lava template using the merge
        /// fields on the data results object. Otherwise, it will be set from the page title stored in the database.
        /// </summary>
        /// <param name="dataResults">The dynamic data results containing the data rows and page title lava.</param>
        private void SetDynamicPageTitle( DynamicDataResults dataResults )
        {
            if ( dataResults == null )
            {
                return;
            }

            var pageTitleLava = dataResults.Config?.PageTitleLava;
            if ( pageTitleLava.IsNullOrWhiteSpace() )
            {
                dataResults.PageTitle = this.PageCache?.PageTitle;
                return;
            }

            var mergeFields = dataResults.Config?.MergeFields;
            var enabledLavaCommands = dataResults.Config?.EnabledLavaCommands;

            var pageTitle = pageTitleLava.ResolveMergeFields( mergeFields, enabledLavaCommands );

            dataResults.PageTitle = pageTitle;

            this.RequestContext.Response.SetPageTitle( pageTitle );
            this.RequestContext.Response.SetBrowserTitle( pageTitle );
        }

        /// <summary>
        /// Sets (or removes) page parameters on the rock request context, using the provided override values.
        /// </summary>
        /// <remarks>
        /// For each override key/value pair: if the value is defined, it will be set on the request context's page
        /// parameters collection. If the value is <see langword="null"/> or empty string, any existing parameter with
        /// a matching key will be removed from the collection.
        /// </remarks>
        /// <param name="pageParameterOverrides">The override values to use when setting page parameters.</param>
        private void ApplyPageParameterOverrides( Dictionary<string, string> pageParameterOverrides )
        {
            if ( pageParameterOverrides?.Any() != true )
            {
                return;
            }

            var pageParameters = this.RequestContext.GetPageParameters();
            foreach ( var paramOverride in pageParameterOverrides )
            {
                var overrideKey = paramOverride.Key;
                var overrideValue = paramOverride.Value;

                if ( overrideValue.IsNullOrWhiteSpace() )
                {
                    // Remove existing parameters whose override value is empty.
                    if ( pageParameters.ContainsKey( overrideKey ) )
                    {
                        pageParameters.Remove( overrideKey );
                    }

                    continue;
                }

                // Add the new value (which might override an existing value).
                pageParameters[overrideKey] = overrideValue;
            }

            /*
                10/22/2024 - JPH

                Local testing suggests it's safe to override RockRequestContext's page parameters (in order to provide
                low-level Lava templates, Etc. with the latest page parameter values). However, if we find this approach
                to be problematic in the future, an alternative approach might be:

                    1. Create a secondary instance of the RockRequestContext class and copy all property values from the
                       primary instance.

                    2. override the page parameters on that secondary instance.

                    3. Set the secondary instance on the RockRequestContextAccessor for low-level processes to grab and
                       use for Lava templates.

                Reason: Provide alternative approach for unconventional page parameter manipulation.
             */
            this.RequestContext.SetPageParameters( pageParameters );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls( DynamicDataResults dataResults )
        {
            var selectionUrl = dataResults?.Config?.SelectionUrl;
            if ( selectionUrl.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var selectionUrlMatches = dataResults.Config.SelectionUrlMatches;
            if ( selectionUrlMatches.Count == 0 )
            {
                // Nothing to replace; simply return selection URL as is.
                return new Dictionary<string, string>
                {
                    { NavigationUrlKey.RowSelection, selectionUrl }
                };
            }

            var columns = dataResults.ActualColumnConfigurations;
            if ( columns?.Any() != true )
            {
                // We don't have any columns to use to replace the matches' values.
                return null;
            }

            // Loop through the matches to ensure we have a column for each.
            foreach ( Match match in selectionUrlMatches )
            {
                var columnName = match.Value.TrimStart( '{' ).TrimEnd( '}' ).Trim();
                var gridField = GetGridField( columnName, columns, isKeyField: false, forceKeyFieldForPersonColumns: true );

                if ( gridField.IsNullOrWhiteSpace() )
                {
                    // We don't have a column to match this value; selection URL is invalid.
                    return null;
                }

                // Replace this match with expected format for the client.
                selectionUrl = selectionUrl.Replace( match.Value, $"(({gridField}))" );
            }

            return new Dictionary<string, string>
            {
                { NavigationUrlKey.RowSelection, selectionUrl }
            };
        }

        /// <summary>
        /// Gets the security grant token that will be used by UI controls on
        /// this block to ensure they have the proper permissions.
        /// </summary>
        /// <returns>A string that represents the security grant token.</string>
        private string GetSecurityGrantToken()
        {
            return new Rock.Security.SecurityGrant()
                .ToToken();
        }

        #endregion Methods

        #region IHasCustomActions

        /// <inheritdoc/>
        List<BlockCustomActionBag> IHasCustomActions.GetCustomActions( bool canEdit, bool canAdministrate )
        {
            var actions = new List<BlockCustomActionBag>();

            if ( canAdministrate )
            {
                actions.Add( new BlockCustomActionBag
                {
                    IconCssClass = "fa fa-edit",
                    Tooltip = "Settings",
                    ComponentFileUrl = "/Obsidian/Blocks/Reporting/dynamicDataCustomSettings.obs"
                } );
            }

            return actions;
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the data that drives the dynamic grid and Lava templates.
        /// </summary>
        /// <remarks>
        /// This method should be called on block startup and whenever page parameters change.
        /// </remarks>
        /// <param name="bag">The information needed to get dynamic data.</param>
        /// <returns>An object containing information about the dynamic data.</returns>
        [BlockAction]
        public BlockActionResult GetDynamicData( GetDynamicDataRequestBag bag )
        {
            if ( bag == null )
            {
                return ActionBadRequest( "Unable to load block data." );
            }

            if ( bag.PageParameterOverrides?.Any() == true )
            {
                ApplyPageParameterOverrides( bag.PageParameterOverrides );
            }

            var dataResults = GetDataResultsForDisplay();
            if ( dataResults.ErrorMessage.IsNotNullOrWhiteSpace() )
            {
                return ActionInternalServerError( dataResults.ErrorMessage );
            }

            var response = new GetDynamicDataResponseBag
            {
                GridResults = dataResults.GridResults,
                LavaTemplateResults = dataResults.LavaTemplateResults
            };

            if ( GetAttributeValue( AttributeKey.EnableQuickReturn ).AsBoolean() )
            {
                response.QuickReturnPageTitle = dataResults.PageTitle;
            }

            return ActionOk( response );
        }

        /// <summary>
        /// Gets the grid row data.
        /// </summary>
        /// <param name="bag">The information needed to get grid row data.</param>
        /// <returns>An object containing information about the grid row data.</returns>
        [BlockAction]
        public BlockActionResult GetGridRowData( GetDynamicDataRequestBag bag )
        {
            if ( bag == null )
            {
                return ActionBadRequest( "Unable to get grid row data." );
            }

            if ( bag.PageParameterOverrides?.Any() == true )
            {
                ApplyPageParameterOverrides( bag.PageParameterOverrides );
            }

            var dataResults = GetDataResultsForDisplay( forceLoadDataRows: true );
            if ( dataResults.ErrorMessage.IsNotNullOrWhiteSpace() )
            {
                return ActionInternalServerError( dataResults.ErrorMessage );
            }

            var dataRows = new List<DataRow>();
            if ( dataResults.DataSet != null && dataResults.DataSet.Tables.Count > 0 )
            {
                dataRows = dataResults.DataSet.Tables[0].Rows.OfType<DataRow>().ToList();
            }

            var gridBuilder = GetGridBuilder( dataResults );
            var gridDataBag = gridBuilder.Build( dataRows );

            return ActionOk( gridDataBag );
        }

        /// <summary>
        /// Creates an entity set for the subset of selected rows in the grid.
        /// </summary>
        /// <returns>An action result that contains identifier of the entity set.</returns>
        [BlockAction]
        public BlockActionResult CreateGridEntitySet( GridEntitySetBag entitySet )
        {
            if ( entitySet == null )
            {
                return ActionBadRequest( "No entity set data was provided." );
            }

            var rockEntitySet = GridHelper.CreateEntitySet( entitySet );

            if ( rockEntitySet == null )
            {
                return ActionBadRequest( "No entities were found to create the set." );
            }

            return ActionOk( rockEntitySet.Id.ToString() );
        }

        /// <summary>
        /// Creates a communication for the subset of selected rows in the grid.
        /// </summary>
        /// <returns>An action result that contains identifier of the communication.</returns>
        [BlockAction]
        public BlockActionResult CreateGridCommunication( GridCommunicationBag communication )
        {
            if ( communication == null )
            {
                return ActionBadRequest( "No communication data was provided." );
            }

            var rockCommunication = GridHelper.CreateCommunication( communication, RequestContext );

            if ( rockCommunication == null )
            {
                return ActionBadRequest( "Grid has no recipients." );
            }

            return ActionOk( rockCommunication.Id.ToString() );
        }

        /// <summary>
        /// Gets the values and all other required details that will be needed to display the custom settings modal.
        /// </summary>
        /// <returns>A box that contains the custom settings values and additional data.</returns>
        [BlockAction]
        public BlockActionResult GetCustomSettings()
        {
            if ( !BlockCache.IsAuthorized( Rock.Security.Authorization.ADMINISTRATE, this.RequestContext.CurrentPerson ) )
            {
                return ActionForbidden( "Not authorized to edit block settings." );
            }

            string pageName = null;
            string pageDescription = null;

            var enablePageUpdate = GetAttributeValue( AttributeKey.UpdatePage ).AsBoolean();
            if ( enablePageUpdate )
            {
                pageName = this.PageCache?.PageTitle ?? string.Empty;
                pageDescription = this.PageCache?.Description ?? string.Empty;
            }

            var options = new DynamicDataCustomSettingsOptionsBag
            {
                DisplayModeItems = this.DisplayModeItems,
                ColumnTypeItems = this.ColumnTypeItems,
                VisiblePriorityItems = this.VisiblePriorityItems
            };

            var resultsDisplayMode = GetAttributeValue( AttributeKey.ResultsDisplayMode );
            if ( !this.DisplayModeItems.Any( m => m.Value == resultsDisplayMode ) )
            {
                // Default to grid.
                resultsDisplayMode = DisplayMode.Grid.Value;
            }

            var settings = new DynamicDataCustomSettingsBag
            {
                // Shared Settings (shared between grid and Lava results formatting display modes).
                EnablePageUpdate = enablePageUpdate,
                PageName = pageName,
                PageDescription = pageDescription,
                Query = GetAttributeValue( AttributeKey.Query ),
                IsStoredProcedure = GetAttributeValue( AttributeKey.StoredProcedure ).AsBoolean(),
                Parameters = GetAttributeValue( AttributeKey.QueryParams ),
                Timeout = GetAttributeValue( AttributeKey.Timeout ).AsIntegerOrNull(),
                ResultsDisplayMode = resultsDisplayMode,
                PageTitleLava = GetAttributeValue( AttributeKey.PageTitleLava ),

                // Results Formatting - Grid Settings.
                ColumnConfigurations = DeserializeColumnConfigurations( GetAttributeValue( AttributeKey.ColumnConfigurations ) ),
                ShowCheckboxSelectionColumn = GetAttributeValue( AttributeKey.ShowCheckboxSelectionColumn ).AsBoolean(),
                DisablePaging = GetAttributeValue( AttributeKey.DisablePaging ).AsBoolean(),
                IsPersonReport = GetAttributeValue( AttributeKey.PersonReport ).AsBoolean(),
                EnableStickyHeader = GetAttributeValue( AttributeKey.EnableStickyHeaderOnGrid ).AsBoolean(),
                EnableExport = GetAttributeValue( AttributeKey.ShowExcelExport ).AsBoolean(),
                EnableMergeTemplate = GetAttributeValue( AttributeKey.ShowMergeTemplate ).AsBoolean(),
                EnableCommunications = GetAttributeValue( AttributeKey.ShowCommunicate ).AsBoolean(),
                EnablePersonMerge = GetAttributeValue( AttributeKey.ShowMergePerson ).AsBoolean(),
                EnableBulkUpdate = GetAttributeValue( AttributeKey.ShowBulkUpdate ).AsBoolean(),
                EnableLaunchWorkflow = GetAttributeValue( AttributeKey.ShowLaunchWorkflow ).AsBoolean(),
                GridTitle = GetAttributeValue( AttributeKey.PanelTitle ),
                SelectionUrl = GetAttributeValue( AttributeKey.UrlMask ),
                CommunicationMergeFields = SanitizeCommaSeparatedColumnNames( GetAttributeValue( AttributeKey.MergeFields ) ),
                CommunicationRecipientFields = SanitizeCommaSeparatedColumnNames( GetAttributeValue( AttributeKey.CommunicationRecipientPersonIdColumns ) ),
                EncryptedFields = SanitizeCommaSeparatedColumnNames( GetAttributeValue( AttributeKey.EncryptedFields ) ),
                GridHeaderContent = GetAttributeValue( AttributeKey.GridHeaderContent ),
                GridFooterContent = GetAttributeValue( AttributeKey.GridFooterContent ),

                // Results Formatting - Lava Settings.
                LavaTemplate = GetAttributeValue( AttributeKey.FormattedOutput )
            };

            return ActionOk( new CustomSettingsBox<DynamicDataCustomSettingsBag, DynamicDataCustomSettingsOptionsBag>
            {
                Settings = settings,
                Options = options,
                SecurityGrantToken = GetSecurityGrantToken()
            } );
        }

        /// <summary>
        /// Reloads the columns.
        /// </summary>
        /// <param name="bag">The information needed to reload columns.</param>
        /// <returns>An object containing information about the reloaded columns.</returns>
        [BlockAction]
        public BlockActionResult ReloadColumns( ReloadColumnsRequestBag bag )
        {
            if ( !BlockCache.IsAuthorized( Rock.Security.Authorization.ADMINISTRATE, this.RequestContext.CurrentPerson ) )
            {
                return ActionForbidden( "Not authorized to edit block settings." );
            }

            if ( bag == null )
            {
                return ActionBadRequest( "Unable to reload columns." );
            }

            var config = new DynamicDataConfig
            {
                // Set only the values required for data set loading.
                DataSetLoadingMode = DataSetLoadingMode.LoadSchemaOnly,
                Query = bag.Query,
                IsStoredProcedure = bag.IsStoredProcedure,
                QueryParams = bag.Parameters.SplitDelimitedValues(),
                Timeout = bag.Timeout,
                EnabledLavaCommands = GetAttributeValue( AttributeKey.EnabledLavaCommands ),
                MergeFields = GetMergeFields()
            };

            var dataResults = GetDataResults( config );
            if ( dataResults.ErrorMessage.IsNotNullOrWhiteSpace() )
            {
                return ActionBadRequest( dataResults.ErrorMessage );
            }

            var response = new ReloadColumnsResponseBag
            {
                ColumnConfigurations = LoadColumnConfigurationsFromDataSet( dataResults.DataSet, bag.ExistingColumnConfigurations )
            };

            return ActionOk( response );
        }

        /// <summary>
        /// Saves the updates to the custom setting values for this block, for the custom settings modal.
        /// </summary>
        /// <param name="box">The box that contains the setting values.</param>
        /// <returns>A response that indicates if the save was successful or not.</returns>
        [BlockAction]
        public BlockActionResult SaveCustomSettings( CustomSettingsBox<DynamicDataCustomSettingsBag, DynamicDataCustomSettingsOptionsBag> box )
        {
            if ( !BlockCache.IsAuthorized( Rock.Security.Authorization.ADMINISTRATE, this.RequestContext.CurrentPerson ) )
            {
                return ActionForbidden( "Not authorized to edit block settings." );
            }

            var block = new BlockService( this.RockContext ).Get( this.BlockId );
            block.LoadAttributes( this.RockContext );

            // Shared Settings (shared between grid and Lava results formatting display modes).
            if ( GetAttributeValue( AttributeKey.UpdatePage ).AsBoolean() )
            {
                var pageName = box.Settings.PageName;
                var pageDescription = box.Settings.PageDescription;

                if ( this.PageCache != null &&
                        ( this.PageCache.PageTitle != pageName || this.PageCache.Description != pageDescription )
                        && this.PageCache.Guid != Rock.SystemGuid.Page.PAGE_MAP.AsGuid() // Don't allow editing the title of the page if the page is the internal page editor (Issue #5542).
                    )
                {
                    var page = new PageService( this.RockContext ).Get( this.PageCache.Id );
                    if ( page != null )
                    {
                        page.InternalName = pageName;
                        page.PageTitle = pageName;
                        page.BrowserTitle = pageName;
                        page.Description = pageDescription;

                        this.RockContext.SaveChanges();

                        // TODO: This was done in the Web Forms block; is it doable in Obsidian?
                        // If not, the new value will be rendered on subsequent page loads, so no big deal.
                        //var breadCrumb = RockPage.BreadCrumbs.Where( c => c.Url == RockPage.PageReference.BuildUrl() ).FirstOrDefault();
                        //if ( breadCrumb != null )
                        //{
                        //    breadCrumb.Name = pageCache.BreadCrumbText;
                        //}
                    }
                }
            }

            box.IfValidProperty( nameof( box.Settings.Query ),
                () => block.SetAttributeValue( AttributeKey.Query, box.Settings.Query ) );

            box.IfValidProperty( nameof( box.Settings.IsStoredProcedure ),
                () => block.SetAttributeValue( AttributeKey.StoredProcedure, box.Settings.IsStoredProcedure.ToString() ) );

            box.IfValidProperty( nameof( box.Settings.Parameters ),
                () => block.SetAttributeValue( AttributeKey.QueryParams, box.Settings.Parameters ) );

            box.IfValidProperty( nameof( box.Settings.Timeout ),
                () => block.SetAttributeValue( AttributeKey.Timeout, box.Settings.Timeout.ToString() ) );

            box.IfValidProperty( nameof( box.Settings.ResultsDisplayMode ),
                () => block.SetAttributeValue( AttributeKey.ResultsDisplayMode, box.Settings.ResultsDisplayMode ) );

            box.IfValidProperty( nameof( box.Settings.PageTitleLava ),
                () => block.SetAttributeValue( AttributeKey.PageTitleLava, box.Settings.PageTitleLava ) );

            // Results Formatting - Grid Settings.
            box.IfValidProperty( nameof( box.Settings.ColumnConfigurations ),
                () => block.SetAttributeValue( AttributeKey.ColumnConfigurations, SerializeColumnConfigurations( box.Settings.ColumnConfigurations ) ) );

            box.IfValidProperty( nameof( box.Settings.ShowCheckboxSelectionColumn ),
                () => block.SetAttributeValue( AttributeKey.ShowCheckboxSelectionColumn, box.Settings.ShowCheckboxSelectionColumn.ToString() ) );

            box.IfValidProperty( nameof( box.Settings.DisablePaging ),
                () => block.SetAttributeValue( AttributeKey.DisablePaging, box.Settings.DisablePaging.ToString() ) );

            box.IfValidProperty( nameof( box.Settings.IsPersonReport ),
                () => block.SetAttributeValue( AttributeKey.PersonReport, box.Settings.IsPersonReport.ToString() ) );

            box.IfValidProperty( nameof( box.Settings.EnableStickyHeader ),
                () => block.SetAttributeValue( AttributeKey.EnableStickyHeaderOnGrid, box.Settings.EnableStickyHeader.ToString() ) );

            box.IfValidProperty( nameof( box.Settings.EnableExport ),
                () => block.SetAttributeValue( AttributeKey.ShowExcelExport, box.Settings.EnableExport.ToString() ) );

            box.IfValidProperty( nameof( box.Settings.EnableMergeTemplate ),
                () => block.SetAttributeValue( AttributeKey.ShowMergeTemplate, box.Settings.EnableMergeTemplate.ToString() ) );

            box.IfValidProperty( nameof( box.Settings.EnableCommunications ),
                () => block.SetAttributeValue( AttributeKey.ShowCommunicate, box.Settings.EnableCommunications.ToString() ) );

            box.IfValidProperty( nameof( box.Settings.EnablePersonMerge ),
                () => block.SetAttributeValue( AttributeKey.ShowMergePerson, box.Settings.EnablePersonMerge.ToString() ) );

            box.IfValidProperty( nameof( box.Settings.EnableBulkUpdate ),
                () => block.SetAttributeValue( AttributeKey.ShowBulkUpdate, box.Settings.EnableBulkUpdate.ToString() ) );

            box.IfValidProperty( nameof( box.Settings.EnableLaunchWorkflow ),
                () => block.SetAttributeValue( AttributeKey.ShowLaunchWorkflow, box.Settings.EnableLaunchWorkflow.ToString() ) );

            box.IfValidProperty( nameof( box.Settings.GridTitle ),
                () => block.SetAttributeValue( AttributeKey.PanelTitle, box.Settings.GridTitle ) );

            box.IfValidProperty( nameof( box.Settings.SelectionUrl ),
                () => block.SetAttributeValue( AttributeKey.UrlMask, box.Settings.SelectionUrl ) );

            box.IfValidProperty( nameof( box.Settings.CommunicationMergeFields ),
                () => block.SetAttributeValue( AttributeKey.MergeFields, SanitizeCommaSeparatedColumnNames( box.Settings.CommunicationMergeFields ) ) );

            box.IfValidProperty( nameof( box.Settings.CommunicationRecipientFields ),
                () => block.SetAttributeValue( AttributeKey.CommunicationRecipientPersonIdColumns, SanitizeCommaSeparatedColumnNames( box.Settings.CommunicationRecipientFields ) ) );

            box.IfValidProperty( nameof( box.Settings.EncryptedFields ),
                () => block.SetAttributeValue( AttributeKey.EncryptedFields, SanitizeCommaSeparatedColumnNames( box.Settings.EncryptedFields ) ) );

            box.IfValidProperty( nameof( box.Settings.GridHeaderContent ),
                () => block.SetAttributeValue( AttributeKey.GridHeaderContent, box.Settings.GridHeaderContent ) );

            box.IfValidProperty( nameof( box.Settings.GridFooterContent ),
                () => block.SetAttributeValue( AttributeKey.GridFooterContent, box.Settings.GridFooterContent ) );

            // Results Formatting - Lava Settings.
            box.IfValidProperty( nameof( box.Settings.LavaTemplate ),
                () => block.SetAttributeValue( AttributeKey.FormattedOutput, box.Settings.LavaTemplate ) );

            block.SaveAttributeValues( this.RockContext );

            return ActionOk();
        }

        #endregion Block Actions

        #region Supporting Members

        /// <summary>
        /// An enum to dictate how the data set should be loaded.
        /// </summary>
        private enum DataSetLoadingMode
        {
            /// <summary>
            /// Load only the schema.
            /// </summary>
            LoadSchemaOnly = 0,

            /// <summary>
            /// Load the schema and data.
            /// </summary>
            LoadSchemaAndData = 1
        }

        /// <summary>
        /// A POCO to represent available display modes for the block.
        /// </summary>
        private class DisplayMode
        {
            private static readonly ListItemBag _grid = new ListItemBag { Text = "Grid", Value = "grid" };
            public static ListItemBag Grid => _grid;

            private static readonly ListItemBag _lavaTemplate = new ListItemBag { Text = "Lava Template", Value = "lavaTemplate" };
            public static ListItemBag LavaTemplate => _lavaTemplate;
        }

        /// <summary>
        /// A POCO to represent available column types for the dynamic data grid.
        /// </summary>
        private class ColumnType
        {
            public const string BooleanValue = "boolean";
            public const string CurrencyValue = "currency";
            public const string DateValue = "date";
            public const string DateTimeValue = "dateTime";
            public const string NumberValue = "number";
            public const string PersonValue = "person";
            public const string TextValue = "text";

            private static readonly ListItemBag _boolean = new ListItemBag { Text = "Boolean", Value = BooleanValue };
            public static ListItemBag Boolean => _boolean;

            private static readonly ListItemBag _currency = new ListItemBag { Text = "Currency", Value = CurrencyValue };
            public static ListItemBag Currency => _currency;

            private static readonly ListItemBag _date = new ListItemBag { Text = "Date", Value = DateValue };
            public static ListItemBag Date => _date;

            private static readonly ListItemBag _dateTime = new ListItemBag { Text = "Date Time", Value = DateTimeValue };
            public static ListItemBag DateTime => _dateTime;

            private static readonly ListItemBag _number = new ListItemBag { Text = "Number", Value = NumberValue };
            public static ListItemBag Number => _number;

            private static readonly ListItemBag _person = new ListItemBag { Text = "Person", Value = PersonValue };
            public static ListItemBag Person => _person;

            private static readonly ListItemBag _text = new ListItemBag { Text = "Text", Value = TextValue };
            public static ListItemBag Text => _text;
        }

        /// <summary>
        /// A POCO to represent available visible priorities for the dynamic data grid.
        /// </summary>
        private class VisiblePriority
        {
            private static readonly ListItemBag _extraSmall = new ListItemBag { Text = "Extra-Small", Value = "xs" };
            public static ListItemBag ExtraSmall => _extraSmall;

            private static readonly ListItemBag _small = new ListItemBag { Text = "Small", Value = "sm" };
            public static ListItemBag Small => _small;

            private static readonly ListItemBag _medium = new ListItemBag { Text = "Medium", Value = "md" };
            public static ListItemBag Medium => _medium;

            private static readonly ListItemBag _large = new ListItemBag { Text = "Large", Value = "lg" };
            public static ListItemBag Large => _large;

            private static readonly ListItemBag _extraLarge = new ListItemBag { Text = "Extra-Large", Value = "xl" };
            public static ListItemBag ExtraLarge => _extraLarge;
        }

        /// <summary>
        /// A POCO to provide configuration settings when loading dynamic data.
        /// </summary>
        private class DynamicDataConfig
        {
            #region Required for Data Set Loading

            /// <summary>
            /// Gets or sets the data set loading mode.
            /// </summary>
            public DataSetLoadingMode DataSetLoadingMode { get; set; }

            /// <summary>
            /// Gets or sets the SQL query or stored procedure name to execute.
            /// </summary>
            public string Query { get; set; }

            /// <summary>
            /// Gets or sets whether the query represents the name of a stored procedure to execute.
            /// </summary>
            public bool IsStoredProcedure { get; set; }

            /// <summary>
            /// Gets or sets the parameters required by the query or stored procedure.
            /// </summary>
            public string[] QueryParams { get; set; }

            /// <summary>
            /// Gets or sets the amount of time in seconds to allow the query to run before timing out.
            /// </summary>
            public int? Timeout { get; set; }

            /// <summary>
            /// Gets or sets the enabled lava commands.
            /// </summary>
            public string EnabledLavaCommands { get; set; }

            /// <summary>
            /// Gets or sets the merge fields.
            /// </summary>
            public Dictionary<string, object> MergeFields { get; set; }

            #endregion Required for Data Set Loading

            #region Required for Displaying Results

            /// <summary>
            /// Gets or sets the page title lava.
            /// </summary>
            public string PageTitleLava { get; set; }

            /// <summary>
            /// Gets or sets the results display mode.
            /// </summary>
            public string ResultsDisplayMode { get; set; }

            /// <summary>
            /// Gets whether the results should be displayed using a lava template.
            /// </summary>
            public bool IsLavaTemplateDisplayMode => this.ResultsDisplayMode == DisplayMode.LavaTemplate.Value;

            /// <summary>
            /// Gets or sets configuration objects describing how each column of the grid
            /// results should be displayed, as well as rules for filtering, exporting, Etc.
            /// </summary>
            public List<ColumnConfigurationBag> ColumnConfigurations { get; set; }

            /// <summary>
            /// Gets or sets whether to show the checkbox select column on the grid as the first column.
            /// </summary>
            public bool ShowCheckboxSelectionColumn { get; set; }

            /// <summary>
            /// Gets or sets whether to disable paging on the grid.
            /// </summary>
            public bool DisablePaging { get; set; }

            /// <summary>
            /// Gets or sets whether the query returns a list of people.
            /// </summary>
            public bool IsPersonReport { get; set; }

            /// <summary>
            /// Gets or sets whether the header on the grid will be sticky at the top of the page.
            /// </summary>
            public bool EnableStickyHeader { get; set; }

            /// <summary>
            /// Gets or sets whether to show 'Export to Excel' and 'Export to CSV' buttons in the grid.
            /// </summary>
            public bool EnableExport { get; set; }

            /// <summary>
            /// Gets or sets whether to show 'Merge Template' button in the grid.
            /// </summary>
            public bool EnableMergeTemplate { get; set; }

            /// <summary>
            /// Gets or sets whether to show 'Communicate' button in the grid.
            /// </summary>
            public bool EnableCommunications { get; set; }

            /// <summary>
            /// Gets or sets whether to show 'Merge Person Records' button in the grid.
            /// </summary>
            public bool EnablePersonMerge { get; set; }

            /// <summary>
            /// Gets or sets whether to show 'Bulk Update' button in the grid.
            /// </summary>
            public bool EnableBulkUpdate { get; set; }

            /// <summary>
            /// Get or sets whether to show 'Launch Workflow' button in the grid.
            /// </summary>
            public bool EnableLaunchWorkflow { get; set; }

            /// <summary>
            /// Gets or sets the grid's title.
            /// </summary>
            public string GridTitle { get; set; }

            /// <summary>
            /// Gets or sets the URL to redirect individual to when they click on a row in the grid.
            /// </summary>
            public string SelectionUrl { get; set; }

            /// <summary>
            /// Gets the selection URL regex match collection.
            /// </summary>
            public MatchCollection SelectionUrlMatches => SelectionUrlRegex.Matches( this.SelectionUrl );

            /// <summary>
            /// Gets or sets the column name(s) that should be available to use as merge fields for the communication.
            /// </summary>
            public string[] CommunicationMergeFields { get; set; }

            /// <summary>
            /// Gets or sets the column name(s) that contain a person ID field to use as the recipient for a communication.
            /// </summary>
            public string[] CommunicationRecipientFields { get; set; }

            /// <summary>
            /// Gets or sets the column name(s) that need to be decrypted before displaying their value.
            /// </summary>
            public string[] EncryptedFields { get; set; }

            /// <summary>
            /// Gets or sets the Lava template to be rendered above the grid.
            /// </summary>
            public string GridHeaderContent { get; set; }

            /// <summary>
            /// Gets or set the Lava template to be rendered below the grid.
            /// </summary>
            public string GridFooterContent { get; set; }

            /// <summary>
            /// Gets or sets the formatting to apply to the returned results.
            /// </summary>
            public string LavaTemplate { get; set; }

            #endregion Required for Displaying Results
        }

        /// <summary>
        /// A POCO to hold the results of a dynamic data load attempt.
        /// </summary>
        private class DynamicDataResults
        {
            /// <summary>
            /// Gets or sets the error message.
            /// </summary>
            public string ErrorMessage { get; set; }

            /// <summary>
            /// Gets or sets the data set.
            /// </summary>
            public DataSet DataSet { get; set; }

            /// <summary>
            /// Gets or sets the actual column configurations, based on the current data set.
            /// </summary>
            public List<ColumnConfigurationBag> ActualColumnConfigurations { get; set; }

            /// <summary>
            /// Gets or sets the config values that were used to load the data set.
            /// </summary>
            public DynamicDataConfig Config { get; set; }

            /// <summary>
            /// Gets or sets whether the full data set was loaded.
            /// </summary>
            public bool WasFullDataSetLoaded { get; set; }

            /// <summary>
            /// Gets or sets the page title.
            /// </summary>
            public string PageTitle { get; set; }

            /// <summary>
            /// Gets or sets the grid builder.
            /// </summary>
            public GridBuilder<DataRow> GridBuilder { get; set; }

            /// <summary>
            /// Gets or sets the grid results.
            /// </summary>
            public GridResultsBag GridResults { get; set; }

            /// <summary>
            /// Gets or sets the lava template results.
            /// </summary>
            public LavaTemplateResultsBag LavaTemplateResults { get; set; }
        }

        /// <summary>
        /// An object to represent data table rows within the lava template, when RockLiquid lava processing is enabled.
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
        }

        /// <summary>
        /// An object to represent data table rows within the lava template.
        /// </summary>
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

        #endregion Supporting Members
    }
}
