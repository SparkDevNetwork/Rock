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
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Reporting
{
    /// <summary>
    /// Provides services to generate report output data from a supplied tabular report template.
    /// </summary>
    public class ReportOutputBuilder
    {
        #region Private fields

        private Report _Report;
        private RockContext _DataContext;
        private Type _ReportEntityType;
        private List<EntityField> _EntityFields;
        private Dictionary<int, EntityField> _SelectedEntityFields;
        private Dictionary<int, AttributeCache> _SelectedAttributes;
        private Dictionary<int, ReportField> _SelectedComponents;
        private Dictionary<Guid, string> _SortExpressions;
        private List<ReportFieldMap> _ReportFieldMaps;
        private DataTable _DataTable;
        private List<string> _ErrorMessages;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportOutputBuilder"/> class.
        /// </summary>
        /// <param name="report">The report.</param>
        /// <param name="dataContext">The data context.</param>
        public ReportOutputBuilder( Report report, RockContext dataContext )
        {
            _Report = report;

            _DataContext = dataContext;
        }

        #endregion

        #region Public Properties and Methods

        /// <summary>
        /// Gets or sets the last result message.
        /// </summary>
        /// <value>
        /// The last result message.
        /// </value>
        public string LastResultMessage { get; set; }

        /// <summary>
        /// Sets the database query timeout value in seconds.
        /// </summary>
        public int? DatabaseTimeout { get; set; }

        /// <summary>
        /// If true, the result output will include a field containing a reference to the recipient of a communication if a IRecipientDataSelect column is present.
        /// </summary>
        public bool AddCommunicationRecipientField { get; set; }

        /// <summary>
        /// Gets the output of the report.
        /// </summary>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="whereExpression">A Linq Expression that represents the query filter predicate.</param>
        /// <param name="parameterExpression">The Parameter Expression required as input to the query filter predicate.</param>
        /// <param name="dataContext">The data context in which the report is being built.</param>
        /// <param name="fieldContent">Content of the field.</param>
        /// <param name="pageIndex">The index number of the data page to retrieve. If not specified, all pages will be included in the results.</param>
        /// <param name="pageSize">The number of rows of data to retrieve. If not specified, all rows will be included in the results.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Report data build failed.</exception>
        public TabularReportOutputResult GetReportData( Person currentPerson, Expression whereExpression, ParameterExpression parameterExpression, RockContext dataContext, ReportOutputBuilderFieldContentSpecifier fieldContent = ReportOutputBuilderFieldContentSpecifier.FormattedText, int? pageIndex = null, int? pageSize = null )
        {
            _DataContext = dataContext;

            this.Initialize();

            // Add Report Fields.
            int columnIndex = 0;

            var orderedReportFields = _Report.ReportFields.OrderBy( a => a.ColumnOrder );

            foreach ( var reportField in orderedReportFields )
            {
                bool mergeField = reportField.IsCommunicationMergeField.HasValue && reportField.IsCommunicationMergeField.Value;
                bool recipientField = reportField.IsCommunicationRecipientField.HasValue && reportField.IsCommunicationRecipientField.Value;

                var reportFieldGuid = reportField.Guid.ToString();

                columnIndex++;

                if ( reportField.ReportFieldType == ReportFieldType.Property )
                {
                    AddPropertyField( reportField, columnIndex );
                }
                else if ( reportField.ReportFieldType == ReportFieldType.Attribute )
                {
                    AddAttributeField( reportField, columnIndex );
                }
                else if ( reportField.ReportFieldType == ReportFieldType.DataSelectComponent )
                {
                    AddDataSelectField( reportField, columnIndex );
                }
            }

            // Construct the expression for sorting the data.
            SortProperty sortProperty = null;

            var reportSort = new SortProperty();
            var sortColumns = new Dictionary<string, SortDirection>();

            var sortedFields = _Report.ReportFields.Where( a => a.SortOrder.HasValue ).OrderBy( a => a.SortOrder.Value ).ToList();

            foreach ( var reportField in sortedFields )
            {
                if ( _SortExpressions.ContainsKey( reportField.Guid ) )
                {
                    var sortFields = _SortExpressions[reportField.Guid].Split( ',' );
                    
                    foreach ( var sortExpression in sortFields )
                    {
                        var sortField = sortExpression
                            .ReplaceCaseInsensitive( "asc", string.Empty )
                            .ReplaceCaseInsensitive( "desc", string.Empty )
                            .Trim();

                        sortColumns.Add( sortField, reportField.SortDirection );
                    }
                }
            }

            if ( sortColumns.Any() )
            {
                reportSort.Property = sortColumns.Select( a => a.Key + ( a.Value == SortDirection.Descending ? " desc" : string.Empty ) ).ToList().AsDelimited( "," );

                sortProperty = reportSort;
            }

            // Build the query definition.
            var qryErrors = new List<string>();

            dynamic qry = GetQueryableForReport( _Report, _ReportEntityType, _SelectedEntityFields, _SelectedAttributes, _SelectedComponents, whereExpression, parameterExpression, sortProperty, out qryErrors, _DataContext, pageIndex, pageSize );

            _ErrorMessages.AddRange( qryErrors );

            try
            {
                // Materialize the query results.
                if ( !string.IsNullOrEmpty( _Report.QueryHint )
                     && _DataContext is RockContext )
                {
                    using ( new QueryHintScope( _DataContext as RockContext, _Report.QueryHint ) )
                    {
                        AddDataTableRowsFromList( _DataTable, qry, _ReportFieldMaps );
                    }
                }
                else
                {
                    AddDataTableRowsFromList( _DataTable, qry, _ReportFieldMaps );
                }

                // Format the data retrieved from the data source.
                if ( fieldContent == ReportOutputBuilderFieldContentSpecifier.FormattedText )
                {
                    ApplyFormattingToOutputFields();
                }
            }
            catch ( Exception ex )
            {
                Exception exception = ex;

                throw new Exception( "Report data build failed.", ex );
            }

            // Return the result.
            var result = new TabularReportOutputResult();

            result.Data = _DataTable;
            result.ReportFieldToDataColumnMap = _ReportFieldMaps.ToDictionary( k => k.ReportFieldGuid, v => v.TableColumnName );

            return result;
        }

        /// <summary>
        /// Configures a specified grid to display the output of the current report.
        /// </summary>
        /// <param name="gReport">The target Grid control.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="isCommunication">Flag to indicate if this is a communication</param>
        /// <param name="dataContext"></param>
        /// <param name="reportFieldToDataSourceFieldMap"></param>
        /// <param name="preserveExistingColumns">Flag indicating if the existing columns in the grid should be preserved.</param>
        /// <param name="addSelectionColumn"></param>
        public void ConfigureReportOutputGrid( Grid gReport, Person currentPerson, bool isCommunication, RockContext dataContext, Dictionary<Guid, string> reportFieldToDataSourceFieldMap, bool preserveExistingColumns = true, bool addSelectionColumn = true )
        {
            if ( _Report == null )
            {
                throw new ArgumentException( "A report must be specified." );
            }

            _DataContext = dataContext;

            var errors = new List<string>();

            var entityType = EntityTypeCache.Get( _Report.EntityTypeId.Value, _DataContext ).GetEntityType();

            if ( entityType == null )
            {
                throw new Exception( string.Format( "ConfigureReportOutputGrid failed. Could not resolve EntityType \"{0}\".", _Report.EntityType ) );
            }

            gReport.EntityTypeId = _Report.EntityTypeId;

            bool isPersonDataSet = _Report.EntityTypeId == EntityTypeCache.Get( typeof( Rock.Model.Person ), true, _DataContext ).Id;

            if ( isPersonDataSet )
            {
                gReport.PersonIdField = "Id";
                gReport.DataKeyNames = new string[] { "Id" };
            }
            else
            {
                gReport.PersonIdField = null;
            }

            if ( _Report.EntityTypeId.HasValue )
            {
                gReport.RowItemText = EntityTypeCache.Get( _Report.EntityTypeId.Value, _DataContext ).FriendlyName;
            }

            List<EntityField> entityFields = Rock.Reporting.EntityHelper.GetEntityFields( entityType, true, false );

            var selectedEntityFields = new Dictionary<int, EntityField>();
            var selectedAttributes = new Dictionary<int, AttributeCache>();
            var selectedComponents = new Dictionary<int, ReportField>();

            // If a SelectField exists, preserve it to allow the selection ViewState to be restored.
            var selectField = gReport.Columns.OfType<SelectField>().FirstOrDefault();

            if ( !preserveExistingColumns )
            {
                gReport.Columns.Clear();
            }

            int columnIndex = gReport.Columns.Count;

            if ( addSelectionColumn )
            {
                // Add a selection column if it does not already exist.
                if ( selectField == null || !preserveExistingColumns )
                {
                    gReport.Columns.Insert( 0, selectField ?? new SelectField() );

                    columnIndex++;
                }
            }

            var reportFieldSortExpressions = new Dictionary<Guid, string>();

            gReport.CommunicateMergeFields = new List<string>();
            gReport.CommunicationRecipientPersonIdFields = new List<string>();

            foreach ( var reportField in _Report.ReportFields.OrderBy( a => a.ColumnOrder ) )
            {
                var reportFieldGuid = reportField.Guid;

                string dataFieldName;

                if ( reportFieldToDataSourceFieldMap.ContainsKey( reportFieldGuid ) )
                {
                    dataFieldName = reportFieldToDataSourceFieldMap[reportFieldGuid];
                }
                else
                {
                    dataFieldName = string.IsNullOrWhiteSpace( reportField.ColumnHeaderText ) ? string.Format( "Column_{0}", columnIndex ) : reportField.ColumnHeaderText;
                }

                bool mergeField = reportField.IsCommunicationMergeField.HasValue && reportField.IsCommunicationMergeField.Value;
                bool recipientField = reportField.IsCommunicationRecipientField.HasValue && reportField.IsCommunicationRecipientField.Value;

                columnIndex++;

                if ( reportField.ReportFieldType == ReportFieldType.Property )
                {
                    var entityField = entityFields.FirstOrDefault( a => a.Name == reportField.Selection );
                    if ( entityField != null )
                    {
                        selectedEntityFields.Add( columnIndex, entityField );

                        BoundField boundField = entityField.GetBoundFieldType();

                        boundField.DataField = dataFieldName;
                        boundField.HeaderText = string.IsNullOrWhiteSpace( reportField.ColumnHeaderText ) ? entityField.Title : reportField.ColumnHeaderText;
                        boundField.SortExpression = boundField.DataField;
                        reportFieldSortExpressions.AddOrReplace( reportField.Guid, boundField.SortExpression );
                        boundField.Visible = reportField.ShowInGrid;
                        gReport.Columns.Add( boundField );

                        if ( mergeField )
                        {
                            gReport.CommunicateMergeFields.Add( $"{boundField.DataField}|{boundField.HeaderText.RemoveSpecialCharacters()}" );
                        }
                        if ( recipientField )
                        {
                            gReport.CommunicationRecipientPersonIdFields.Add( boundField.DataField );
                        }
                    }
                }
                else if ( reportField.ReportFieldType == ReportFieldType.Attribute )
                {
                    var attributeGuid = reportField.Selection.AsGuidOrNull();

                    if ( attributeGuid.HasValue )
                    {
                        var attribute = AttributeCache.Get( attributeGuid.Value, _DataContext );
                        if ( attribute != null && attribute.IsActive )
                        {
                            selectedAttributes.Add( columnIndex, attribute );

                            BoundField boundField;

                            if ( attribute.FieldType.Guid.Equals( Rock.SystemGuid.FieldType.BOOLEAN.AsGuid() ) )
                            {
                                boundField = new BoolField();
                            }
                            else if ( attribute.FieldType.Guid.Equals( Rock.SystemGuid.FieldType.DEFINED_VALUE.AsGuid() ) )
                            {
                                boundField = new DefinedValueField();
                            }
                            else
                            {
                                boundField = new CallbackField();
                                boundField.HtmlEncode = false;
                                ( boundField as CallbackField ).OnFormatDataValue += ( sender, e ) =>
                                {
                                    string resultHtml = null;
                                    var attributeValue = e.DataValue ?? attribute.DefaultValueAsType;
                                    if ( attributeValue != null )
                                    {
                                        bool condensed = true;
                                        resultHtml = attribute.FieldType.Field.FormatValueAsHtml( gReport, attributeValue.ToString(), attribute.QualifierValues, condensed );

                                    }

                                    e.FormattedValue = resultHtml ?? string.Empty;
                                };
                            }

                            boundField.DataField = dataFieldName;
                            boundField.HeaderText = string.IsNullOrWhiteSpace( reportField.ColumnHeaderText ) ? attribute.Name : reportField.ColumnHeaderText;
                            boundField.SortExpression = boundField.DataField;
                            reportFieldSortExpressions.AddOrReplace( reportField.Guid, boundField.SortExpression );

                            if ( attribute.FieldType.Guid.Equals( Rock.SystemGuid.FieldType.INTEGER.AsGuid() ) ||
                                attribute.FieldType.Guid.Equals( Rock.SystemGuid.FieldType.DATE.AsGuid() ) ||
                                attribute.FieldType.Guid.Equals( Rock.SystemGuid.FieldType.FILTER_DATE.AsGuid() ) )
                            {
                                boundField.HeaderStyle.HorizontalAlign = HorizontalAlign.Right;
                                boundField.ItemStyle.HorizontalAlign = HorizontalAlign.Right;
                            }

                            boundField.Visible = reportField.ShowInGrid;

                            gReport.Columns.Add( boundField );

                            if ( mergeField )
                            {
                                gReport.CommunicateMergeFields.Add( $"{boundField.DataField}|{boundField.HeaderText.RemoveSpecialCharacters()}" );
                            }
                            if ( recipientField )
                            {
                                gReport.CommunicationRecipientPersonIdFields.Add( boundField.DataField );
                            }
                        }
                    }
                }
                else if ( reportField.ReportFieldType == ReportFieldType.DataSelectComponent )
                {
                    selectedComponents.Add( columnIndex, reportField );

                    if ( reportField.DataSelectComponentEntityType == null )
                    {
                        // If the component entity type is not loaded, populate it now.
                        var entityTypeService = new EntityTypeService( _DataContext );

                        var dataSelectEntityType = entityTypeService.Get( reportField.DataSelectComponentEntityTypeId.Value );

                        reportField.DataSelectComponentEntityType = dataSelectEntityType;
                    }

                    var selectComponent = DataSelectContainer.GetComponent( reportField.DataSelectComponentEntityType.Name );

                    if ( selectComponent != null )
                    {
                        try
                        {
                            DataControlField columnField = selectComponent.GetGridField( entityType, reportField.Selection ?? string.Empty );
                            string fieldId = $"{selectComponent.ColumnPropertyName}_{columnIndex}";

                            var boundField = columnField as BoundField;

                            if ( boundField != null )
                            {
                                boundField.DataField = dataFieldName;
                                var customSortProperties = selectComponent.SortProperties( reportField.Selection ?? string.Empty );
                                bool sortReversed = selectComponent.SortReversed( reportField.Selection ?? string.Empty );
                                if ( customSortProperties != null )
                                {
                                    if ( customSortProperties == string.Empty )
                                    {
                                        // disable sorting if customSortExpression set to string.empty
                                        boundField.SortExpression = string.Empty;
                                    }
                                    else
                                    {
                                        boundField.SortExpression = customSortProperties.Split( ',' ).Select( a => string.Format( "Sort_{0}_{1}", a, columnIndex ) ).ToList().AsDelimited( "," );
                                    }
                                }
                                else
                                {
                                    // use default sorting if customSortExpression was null
                                    boundField.SortExpression = boundField.DataField;
                                }

                                if ( sortReversed == true && !string.IsNullOrWhiteSpace( boundField.SortExpression ) )
                                {
                                    boundField.SortExpression = boundField.SortExpression + " DESC";
                                }
                            }

                            columnField.HeaderText = string.IsNullOrWhiteSpace( reportField.ColumnHeaderText ) ? selectComponent.ColumnHeaderText : reportField.ColumnHeaderText;
                            if ( !string.IsNullOrEmpty( columnField.SortExpression ) )
                            {
                                reportFieldSortExpressions.AddOrReplace( reportField.Guid, columnField.SortExpression );
                            }

                            columnField.Visible = reportField.ShowInGrid;
                            gReport.Columns.Add( columnField );

                            if ( mergeField )
                            {
                                gReport.CommunicateMergeFields.Add( $"Data_{fieldId}|{columnField.HeaderText.RemoveSpecialCharacters()}" );
                            }
                            if ( recipientField )
                            {
                                string fieldName = ( selectComponent is IRecipientDataSelect ) ? $"Recipient_{fieldId}" : $"Data_{fieldId}";
                                gReport.CommunicationRecipientPersonIdFields.Add( fieldName );
                            }
                        }
                        catch ( Exception ex )
                        {
                            ExceptionLogService.LogException( ex, HttpContext.Current );
                            errors.Add( string.Format( "{0} - {1}", selectComponent, ex.Message ) );
                        }
                    }
                }
            }

            try
            {
                gReport.ExportFilename = _Report.Name;

                SortProperty sortProperty = gReport.SortProperty;

                if ( sortProperty == null )
                {
                    var reportSort = new SortProperty();

                    var sortColumns = new Dictionary<string, SortDirection>();

                    foreach ( var reportField in _Report.ReportFields.Where( a => a.SortOrder.HasValue ).OrderBy( a => a.SortOrder.Value ) )
                    {
                        if ( reportFieldSortExpressions.ContainsKey( reportField.Guid ) )
                        {
                            var sortField = reportFieldSortExpressions[reportField.Guid];

                            if ( !string.IsNullOrWhiteSpace( sortField ) )
                            {
                                sortColumns.Add( sortField, reportField.SortDirection );
                            }
                        }
                    }

                    if ( sortColumns.Any() )
                    {
                        reportSort.Property = sortColumns.Select( a => a.Key + ( a.Value == SortDirection.Descending ? " desc" : string.Empty ) ).ToList().AsDelimited( "," );

                        sortProperty = reportSort;
                    }
                }
            }
            catch ( Exception ex )
            {
                throw new Exception( "ConfigureReportOutputGrid failed.", ex );
            }
        }

        /// <summary>
        /// Adds DataTable column values from a set of items in a supplied list, matching items by key value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataTable"></param>
        /// <param name="keyValuePairs"></param>
        public void FillDataColumnValues<T>( DataTable dataTable, Dictionary<int, T> keyValuePairs )
        {
            // Build dictionary of property getters for the items in the list
            var listItemType = typeof( T );

            var itemProperties = listItemType.GetProperties( BindingFlags.Public | BindingFlags.Instance );

            var mappings = new Dictionary<PropertyInfo, string>();

            // Map each of the item properties to a matching column in the DataTable by name.
            // Properties that do not exist in the DataTable are ignored.
            foreach ( PropertyInfo info in itemProperties )
            {
                if ( dataTable.Columns.Contains( info.Name ) )
                {
                    mappings.Add( info, info.Name );
                }
            }

            foreach ( var keyValueEntry in keyValuePairs )
            {
                var row = dataTable.Rows.Find( keyValueEntry.Key );

                if ( row == null )
                {
                    continue;
                }

                var sourceItem = keyValueEntry.Value;

                foreach ( var mapping in mappings )
                {
                    // Copy the source item property value to the column of the same name in the DataTable.
                    row[mapping.Value] = mapping.Key.GetValue( sourceItem );
                }
            }
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Add a field representing an Entity Property to the report output.
        /// </summary>
        /// <param name="reportField"></param>
        /// <param name="columnIndex"></param>
        private void AddPropertyField( ReportField reportField, int columnIndex )
        {
            var propertyName = reportField.Selection;

            var entityField = _EntityFields.FirstOrDefault( a => a.Name == propertyName );

            if ( entityField == null )
            {
                return;
            }

            _SelectedEntityFields.Add( columnIndex, entityField );

            // Add column for property.
            var queryFieldName = string.Format( "Entity_{0}_{1}", entityField.Name, columnIndex );

            var rawValueType = entityField.PropertyType;

            var column = AddDataTableColumn( _DataTable, propertyName, typeof( string ) );
            
            var boundField = entityField.GetBoundFieldType() as RockBoundField;

            _ReportFieldMaps.Add( new ReportFieldMap { ReportFieldGuid = reportField.Guid, QueryColumnName = queryFieldName, TableColumnName = column.ColumnName, TableColumnIsNullable = column.AllowDBNull, BoundField = boundField } );

            _SortExpressions.AddOrReplace( reportField.Guid, column.ColumnName );
        }

        /// <summary>
        /// Add a field representing an Entity Attribute to the report output.
        /// </summary>
        /// <param name="reportField"></param>
        /// <param name="columnIndex"></param>
        private void AddAttributeField( ReportField reportField, int columnIndex )
        {
            var attributeGuid = reportField.Selection.AsGuidOrNull();

            if ( !attributeGuid.HasValue )
            {
                return;
            }

            var attribute = AttributeCache.Get( attributeGuid.Value, _DataContext );

            if ( attribute == null || !attribute.IsActive )
            {
                return;
            }

            _SelectedAttributes.Add( columnIndex, attribute );

            // Add column for Attribute.
            var columnName = attribute.Name;
            var queryFieldName = string.Format( "Attribute_{0}_{1}", attribute.Id, columnIndex );
            var valueType = attribute.DefaultValueAsType.GetType();

            var column = AddDataTableColumn( _DataTable, columnName, typeof( string ) );

            _ReportFieldMaps.Add( new ReportFieldMap { ReportFieldGuid = reportField.Guid, QueryColumnName = queryFieldName, TableColumnName = column.ColumnName } );

            _SortExpressions.AddOrReplace( reportField.Guid, queryFieldName );
        }

        /// <summary>
        /// Add a field representing a DataSelect component to the report output.
        /// </summary>
        /// <param name="reportField"></param>
        /// <param name="columnIndex"></param>
        private void AddDataSelectField( ReportField reportField, int columnIndex )
        {
            _SelectedComponents.Add( columnIndex, reportField );

            if ( reportField.DataSelectComponentEntityType == null )
            {
                // If the component entity type is not loaded, populate it now.
                var entityTypeService = new EntityTypeService( _DataContext );

                var dataSelectEntityType = entityTypeService.Get( reportField.DataSelectComponentEntityTypeId.Value );

                reportField.DataSelectComponentEntityType = dataSelectEntityType;
            }

            var selectComponent = DataSelectContainer.GetComponent( reportField.DataSelectComponentEntityType.Name );

            if ( selectComponent == null )
            {
                return;
            }

            try
            {
                // Get the Grid field associated with this DataSelect component.
                // This is a UI component, but we need to use it here because it holds the logic to format the raw data.
                var columnField = selectComponent.GetGridField( _ReportEntityType, reportField.Selection ?? string.Empty );

                // Add a column for the formatted value of the DataSelect field.
                var boundField = columnField as RockBoundField;

                var columnName = selectComponent.ColumnPropertyName;
                var queryFieldName = string.Format( "Data_{0}_{1}", selectComponent.ColumnPropertyName, columnIndex );

                // Get the column data type.
                var rawValueType = selectComponent.ColumnFieldType;

                var column = AddDataTableColumn( _DataTable, columnName, rawValueType );

                var fieldMap = new ReportFieldMap { ReportFieldGuid = reportField.Guid, QueryColumnName = queryFieldName, TableColumnName = column.ColumnName, BoundField = boundField };

                fieldMap.RawValueType = rawValueType;

                _ReportFieldMaps.Add( fieldMap );

                // Add a sort property for the DataSelect field.
                var customSortProperties = selectComponent.SortProperties( reportField.Selection ?? string.Empty );
                bool sortReversed = selectComponent.SortReversed( reportField.Selection ?? string.Empty );

                var sortExpression = string.Empty;

                if ( customSortProperties != null )
                {
                    if ( customSortProperties == string.Empty )
                    {
                        // disable sorting if customSortExpression set to string.empty
                        sortExpression = string.Empty;
                    }
                    else
                    {
                        sortExpression = customSortProperties.Split( ',' ).Select( a => string.Format( "Sort_{0}_{1}", a, columnIndex ) ).ToList().AsDelimited( "," );
                    }
                }
                else
                {
                    // use default sorting if customSortExpression was null
                    sortExpression = queryFieldName;
                }

                if ( sortReversed == true && !string.IsNullOrWhiteSpace( sortExpression ) )
                {
                    sortExpression = sortExpression + " DESC";
                }

                if ( !string.IsNullOrEmpty( sortExpression ) )
                {
                    _SortExpressions.AddOrReplace( reportField.Guid, sortExpression );
                }
            }
            catch ( Exception ex )
            {
                _ErrorMessages.Add( string.Format( "{0} - {1}", selectComponent, ex.Message ) );
            }
        }

        /// <summary>
        /// Initialize the report builder for the current report configuration.
        /// </summary>
        private void Initialize()
        {
            if ( _Report == null )
            {
                throw new ArgumentException( "A report must be specified." );
            }

            _DataTable = new DataTable();

            _ReportEntityType = EntityTypeCache.Get( _Report.EntityTypeId.Value, _DataContext ).GetEntityType();

            if ( _ReportEntityType == null )
            {
                throw new Exception( string.Format( "Report data build failed. Cannot resolve Entity Type \"{0}\"", _Report.EntityType ) );
            }

            var idColumn = AddDataTableColumn( _DataTable, "Id", typeof( int ) );

            _DataTable.PrimaryKey = new DataColumn[] { idColumn };

            _EntityFields = Rock.Reporting.EntityHelper.GetEntityFields( _ReportEntityType, true, false );

            _SelectedEntityFields = new Dictionary<int, EntityField>();
            _SelectedAttributes = new Dictionary<int, AttributeCache>();
            _SelectedComponents = new Dictionary<int, ReportField>();
            _SortExpressions = new Dictionary<Guid, string>();
            _ReportFieldMaps = new List<ReportFieldMap>();

            _ErrorMessages = new List<string>();
        }

        /// <summary>
        /// Apply field-specific formatting to the raw data retrieved from the data store to produce the final report output.
        /// </summary>
        private void ApplyFormattingToOutputFields()
        {
            foreach ( var reportFieldMap in _ReportFieldMaps )
            {
                if ( reportFieldMap.BoundField == null )
                {
                    continue;
                }

                int sourceFieldColumnIndex;
                int targetFieldColumnIndex;

                if ( reportFieldMap.RawValueType != null
                     && reportFieldMap.RawValueType != typeof( string ) )
                {
                    // Create a new column for the formatted value and rename the column containing the unformatted data.
                    var sourceColumn = _DataTable.Columns[reportFieldMap.TableColumnName];

                    var targetColumn = new DataColumn( sourceColumn.ColumnName, typeof( string ) );

                    sourceColumn.ColumnName = sourceColumn.ColumnName + "_Raw";

                    _DataTable.Columns.Add( targetColumn );

                    targetColumn.SetOrdinal( sourceColumn.Ordinal );

                    sourceFieldColumnIndex = _DataTable.Columns.IndexOf( sourceColumn );
                    targetFieldColumnIndex = _DataTable.Columns.IndexOf( targetColumn );

                }
                else
                {
                    sourceFieldColumnIndex = _DataTable.Columns.IndexOf( reportFieldMap.TableColumnName );
                    targetFieldColumnIndex = sourceFieldColumnIndex;
                }

                foreach ( DataRow row in _DataTable.Rows )
                {
                    var rawValue = row[sourceFieldColumnIndex];

                    string formattedValue = null;

                    if ( rawValue != null )
                    {
                        var emptyControl = new LiteralControl();

                        formattedValue = reportFieldMap.BoundField.FormatDataValue( rawValue );
                    }

                    row[targetFieldColumnIndex] = formattedValue ?? string.Empty;
                }

                // Remove the raw value field.
                if ( sourceFieldColumnIndex != targetFieldColumnIndex )
                {
                    _DataTable.Columns.RemoveAt( sourceFieldColumnIndex );
                }
            }

            // Apply formatting for Attributes.
            foreach ( var attributeInfo in _SelectedAttributes )
            {
                var attributeColumnIndex = attributeInfo.Key;
                var attribute = attributeInfo.Value;

                foreach ( DataRow row in _DataTable.Rows )
                {
                    var rawValue = row[attributeColumnIndex];

                    string resultHtml = null;
                    var attributeValue = rawValue ?? attribute.DefaultValueAsType;

                    if ( attributeValue != null )
                    {
                        bool condensed = true;

                        var emptyControl = new LiteralControl();

                        resultHtml = attribute.FieldType.Field.FormatValueAsHtml( emptyControl, attributeValue.ToString(), attribute.QualifierValues, condensed );
                    }

                    row[attributeColumnIndex] = resultHtml ?? string.Empty;
                }
            }
        }

        /// <summary>
        /// Add a column to a DataTable.
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="columnName"></param>
        /// <param name="columnType"></param>
        /// <returns></returns>
        private DataColumn AddDataTableColumn( DataTable dataTable, string columnName, Type columnType )
        {
            if ( columnName == null || dataTable.Columns.Contains( columnName ) )
            {
                columnName = string.Format( "Column {0}", dataTable.Columns.Count + 1 );
            }

            if ( columnType.IsGenericType
                 && columnType.GetGenericTypeDefinition() == typeof( Nullable<> ) )
            {
                columnType = Nullable.GetUnderlyingType( columnType );
            }

            var column = new DataColumn( columnName, columnType );

            column.Caption = columnName;

            dataTable.Columns.Add( column );

            return column;
        }

        /// <summary>
        /// Adds new rows to a DataTable for each item in the supplied list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataTable">The data table.</param>
        /// <param name="items">The items.</param>
        /// <param name="reportFieldMaps">The report field maps.</param>
        private void AddDataTableRowsFromList<T>( DataTable dataTable, IEnumerable<T> items, List<ReportFieldMap> reportFieldMaps )
        {
            var listNameToColumnNameMap = reportFieldMaps.ToDictionary( k => k.QueryColumnName, v => v.TableColumnName );

            // Build dictionary of property getters for the items in the list
            var listItemType = typeof( T );

            var itemProperties = listItemType.GetProperties( BindingFlags.Public | BindingFlags.Instance );

            var mappings = new Dictionary<PropertyInfo, DataColumn>();

            // Map each of the list item properties to a matching column in the DataTable.
            // Properties that do not exist in the DataTable are ignored.
            foreach ( PropertyInfo info in itemProperties )
            {
                string columnName;

                if ( listNameToColumnNameMap != null
                     && listNameToColumnNameMap.ContainsKey( info.Name ) )
                {
                    columnName = listNameToColumnNameMap[info.Name];
                }
                else
                {
                    columnName = info.Name;
                }

                if ( dataTable.Columns.Contains( columnName ) )
                {
                    mappings.Add( info, dataTable.Columns[columnName] );
                }
            }

            foreach ( T item in items )
            {
                var row = dataTable.NewRow();

                object value;

                foreach ( var p in mappings )
                {
                    value = p.Key.GetValue( item, null );

                    if ( value == null )
                    {
                        if ( p.Value.AllowDBNull )
                        {
                            row[p.Value] = DBNull.Value;
                        }
                    }
                    else
                    {
                        row[p.Value] = p.Key.GetValue( item, null );
                    }
                }

                dataTable.Rows.Add( row );
            }
        }

        /// <summary>
        /// Gets a query expression for the Report output, filtered using a supplied filter expression.
        /// </summary>
        /// <param name="report">The report.</param>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="entityFields">The entity fields.</param>
        /// <param name="attributes">The attributes.</param>
        /// <param name="selectComponents">The select components.</param>
        /// <param name="whereExpression">The where expression.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="sortProperty">The sort property.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <param name="reportDbContext">The report database context.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="pageIndex">Index of the page.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        /// <exception cref="Exception"></exception>
        private IQueryable GetQueryableForReport( Report report, Type entityType, Dictionary<int, EntityField> entityFields, Dictionary<int, AttributeCache> attributes, Dictionary<int, ReportField> selectComponents, Expression whereExpression, ParameterExpression parameterExpression, Rock.Web.UI.Controls.SortProperty sortProperty, out List<string> errorMessages, System.Data.Entity.DbContext reportDbContext, int? pageSize, int? pageIndex )
        {
            errorMessages = new List<string>();

            if ( entityType != null )
            {
                IService serviceInstance = Reflection.GetServiceForEntityType( entityType, reportDbContext );

                if ( this.DatabaseTimeout.HasValue )
                {
                    reportDbContext.Database.CommandTimeout = this.DatabaseTimeout.Value;
                }

                if ( serviceInstance != null )
                {
                    var paramExpression = parameterExpression;

                    MemberExpression idExpression = Expression.Property( paramExpression, "Id" );

                    // Get AttributeValue queryable and parameter
                    var attributeValues = reportDbContext.Set<AttributeValue>();
                    ParameterExpression attributeValueParameter = Expression.Parameter( typeof( AttributeValue ), "v" );

                    // Create the dynamic type
                    var dynamicFields = new Dictionary<string, Type>();
                    dynamicFields.Add( "Id", typeof( int ) );
                    foreach ( var f in entityFields )
                    {
                        dynamicFields.Add( string.Format( "Entity_{0}_{1}", f.Value.Name, f.Key ), f.Value.PropertyType );
                    }

                    foreach ( var a in attributes )
                    {
                        dynamicFields.Add( string.Format( "Attribute_{0}_{1}", a.Value.Id, a.Key ), a.Value.FieldType.Field.AttributeValueFieldType );
                    }

                    foreach ( var reportField in selectComponents )
                    {
                        DataSelectComponent selectComponent = DataSelectContainer.GetComponent( reportField.Value.DataSelectComponentEntityType.Name );
                        if ( selectComponent != null )
                        {
                            dynamicFields.Add( string.Format( "Data_{0}_{1}", selectComponent.ColumnPropertyName, reportField.Key ), selectComponent.ColumnFieldType );
                            var customSortProperties = selectComponent.SortProperties( reportField.Value.Selection );
                            if ( customSortProperties != null )
                            {
                                foreach ( var customSortProperty in customSortProperties.Split( ',' ) )
                                {
                                    if ( !string.IsNullOrWhiteSpace( customSortProperty ) )
                                    {
                                        var customSortPropertyType = entityType.GetPropertyType( customSortProperty );
                                        dynamicFields.Add( string.Format( "Sort_{0}_{1}", customSortProperty, reportField.Key ), customSortPropertyType ?? typeof( string ) );
                                    }
                                }
                            }

                            if ( this.AddCommunicationRecipientField && selectComponent is IRecipientDataSelect )
                            {
                                dynamicFields.Add( $"Recipient_{selectComponent.ColumnPropertyName}_{reportField.Key}", ( ( IRecipientDataSelect ) selectComponent ).RecipientColumnFieldType );
                            }
                        }
                    }

                    if ( dynamicFields.Count == 0 )
                    {
                        errorMessages.Add( "At least one field must be defined" );
                        return null;
                    }

                    Type dynamicType = LinqRuntimeTypeBuilder.GetDynamicType( dynamicFields );
                    ConstructorInfo methodFromHandle = dynamicType.GetConstructor( Type.EmptyTypes );

                    // Bind the dynamic fields to their expressions
                    var bindings = new List<MemberAssignment>();
                    bindings.Add( Expression.Bind( dynamicType.GetField( "id" ), idExpression ) );

                    foreach ( var f in entityFields )
                    {
                        bindings.Add( Expression.Bind( dynamicType.GetField( string.Format( "entity_{0}_{1}", f.Value.Name, f.Key ) ), Expression.Property( paramExpression, f.Value.Name ) ) );
                    }

                    foreach ( var a in attributes )
                    {
                        bindings.Add( Expression.Bind( dynamicType.GetField( string.Format( "attribute_{0}_{1}", a.Value.Id, a.Key ) ), GetAttributeValueExpression( attributeValues, attributeValueParameter, idExpression, a.Value.Id ) ) );
                    }

                    foreach ( var reportField in selectComponents )
                    {
                        DataSelectComponent selectComponent = DataSelectContainer.GetComponent( reportField.Value.DataSelectComponentEntityType.Name );
                        if ( selectComponent != null )
                        {
                            try
                            {
                                var componentExpression = selectComponent.GetExpression( reportDbContext, idExpression, reportField.Value.Selection ?? string.Empty );
                                if ( componentExpression == null )
                                {
                                    componentExpression = Expression.Constant( null, typeof( string ) );
                                }

                                bindings.Add( Expression.Bind( dynamicType.GetField( string.Format( "data_{0}_{1}", selectComponent.ColumnPropertyName, reportField.Key ) ), componentExpression ) );

                                if ( this.AddCommunicationRecipientField && selectComponent is IRecipientDataSelect )
                                {
                                    var recipientPersonIdExpression = ( ( IRecipientDataSelect ) selectComponent ).GetRecipientPersonIdExpression( reportDbContext, idExpression, reportField.Value.Selection ?? string.Empty );
                                    if ( recipientPersonIdExpression != null )
                                    {
                                        bindings.Add( Expression.Bind( dynamicType.GetField( string.Format( "recipient_{0}_{1}", selectComponent.ColumnPropertyName, reportField.Key ) ), recipientPersonIdExpression ) );
                                    }
                                }

                                var customSortProperties = selectComponent.SortProperties( reportField.Value.Selection );
                                if ( !string.IsNullOrEmpty( customSortProperties ) )
                                {
                                    foreach ( var customSortProperty in customSortProperties.Split( ',' ) )
                                    {
                                        var customSortPropertyParts = customSortProperty.Split( '.' );
                                        MemberInfo memberInfo = dynamicType.GetField( string.Format( "sort_{0}_{1}", customSortProperty, reportField.Key ) );
                                        Expression memberExpression = null;
                                        foreach ( var customSortPropertyPart in customSortPropertyParts )
                                        {
                                            memberExpression = Expression.Property( memberExpression ?? paramExpression, customSortPropertyPart );
                                        }

                                        bindings.Add( Expression.Bind( memberInfo, memberExpression ) );
                                    }
                                }
                            }
                            catch ( Exception ex )
                            {
                                throw new Exception( string.Format( "Exception in {0}", selectComponent ), ex );
                            }
                        }
                    }

                    ConstructorInfo constructorInfo = dynamicType.GetConstructor( Type.EmptyTypes );
                    NewExpression newExpression = Expression.New( constructorInfo );
                    MemberInitExpression memberInitExpression = Expression.MemberInit( newExpression, bindings );
                    LambdaExpression selector = Expression.Lambda( memberInitExpression, paramExpression );

                    // NOTE: having a NULL Dataview is OK.
                    //Expression whereExpression = null;
                    //if ( whereExpression == null
                    //     && this.DataView != null )
                    //{
                    //    whereExpression = this.DataView.GetExpression( serviceInstance, paramExpression, out errorMessages );
                    //}

                    MethodInfo getMethod = serviceInstance.GetType().GetMethod( "Get", new Type[] { typeof( ParameterExpression ), typeof( Expression ), typeof( Rock.Web.UI.Controls.SortProperty ), typeof( int? ) } );
                    if ( getMethod != null )
                    {
                        var getResult = getMethod.Invoke( serviceInstance, new object[] { paramExpression, whereExpression, null, null } );
                        var qry = getResult as IQueryable<IEntity>;
                        var qryExpression = qry.Expression;

                        // apply the OrderBy clauses to the Expression from whatever columns are specified in sortProperty.Property
                        string orderByMethod = "OrderBy";
                        if ( sortProperty == null )
                        {
                            // if no sorting was specified, sort by Id
                            sortProperty = new Web.UI.Controls.SortProperty { Direction = SortDirection.Ascending, Property = "Id" };
                        }

                        /*
                         NOTE:  The sort property sorting rules can be a little confusing. Here is how it works:
                         * - SortProperty.Direction of Ascending means sort exactly as what the Columns specification says
                         * - SortProperty.Direction of Descending means sort the _opposite_ of what the Columns specification says
                         * Examples:
                         *  1) SortProperty.Property "LastName desc, FirstName, BirthDate desc" and SortProperty.Direction = Ascending
                         *     OrderBy should be: "order by LastName desc, FirstName, BirthDate desc"
                         *  2) SortProperty.Property "LastName desc, FirstName, BirthDate desc" and SortProperty.Direction = Descending
                         *     OrderBy should be: "order by LastName, FirstName desc, BirthDate"
                         */

                        foreach ( var column in sortProperty.Property.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ) )
                        {
                            string propertyName;

                            var direction = sortProperty.Direction;
                            if ( column.EndsWith( " desc", StringComparison.OrdinalIgnoreCase ) )
                            {
                                propertyName = column.Left( column.Length - 5 );

                                // if the column ends with " desc", toggle the direction if sortProperty is Descending
                                direction = sortProperty.Direction == SortDirection.Ascending ? SortDirection.Descending : SortDirection.Ascending;
                            }
                            else
                            {
                                propertyName = column;
                            }

                            string methodName = direction == SortDirection.Descending ? orderByMethod + "Descending" : orderByMethod;

                            // Call OrderBy on whatever the Expression is for that Column
                            var sortMember = bindings.FirstOrDefault( a => a.Member.Name.Equals( propertyName, StringComparison.OrdinalIgnoreCase ) );
                            LambdaExpression sortSelector = Expression.Lambda( sortMember.Expression, paramExpression );
                            qryExpression = Expression.Call( typeof( Queryable ), methodName, new Type[] { qry.ElementType, sortSelector.ReturnType }, qryExpression, sortSelector );
                            orderByMethod = "ThenBy";
                        }

                        var selectExpression = Expression.Call( typeof( Queryable ), "Select", new Type[] { qry.ElementType, dynamicType }, qryExpression, selector );

                        var query = qry.Provider.CreateQuery( selectExpression ).AsNoTracking();

                        // cast to a dynamic so that we can do a Queryable.Take (the compiler figures out the T in IQueryable at runtime)
                        dynamic dquery = query;

                        if ( pageSize != null )
                        {
                            dquery = Queryable.Skip( dquery, pageIndex.GetValueOrDefault( 0 ) * pageSize.Value );

                            dquery = Queryable.Take( dquery, pageSize.Value );
                        }

                        return dquery as IQueryable;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets an Expression that selects a set of Attribute Values from the data store.
        /// </summary>
        /// <param name="attributeValues">The attribute values.</param>
        /// <param name="attributeValueParameter">The attribute value parameter.</param>
        /// <param name="parentIdProperty">The parent identifier property.</param>
        /// <param name="attributeId">The attribute identifier.</param>
        /// <returns></returns>
        private Expression GetAttributeValueExpression( IQueryable<AttributeValue> attributeValues, ParameterExpression attributeValueParameter, Expression parentIdProperty, int attributeId )
        {
            MemberExpression attributeIdProperty = Expression.Property( attributeValueParameter, "AttributeId" );
            MemberExpression entityIdProperty = Expression.Property( attributeValueParameter, "EntityId" );
            Expression attributeIdConstant = Expression.Constant( attributeId );

            Expression attributeIdCompare = Expression.Equal( attributeIdProperty, attributeIdConstant );
            Expression entityIdCompre = Expression.Equal( entityIdProperty, Expression.Convert( parentIdProperty, typeof( int? ) ) );
            Expression andExpression = Expression.And( attributeIdCompare, entityIdCompre );

            var match = new Expression[] {
                Expression.Constant(attributeValues),
                Expression.Lambda<Func<AttributeValue, bool>>( andExpression, new ParameterExpression[] { attributeValueParameter })
            };

            Expression whereExpression = Expression.Call( typeof( Queryable ), "Where", new Type[] { typeof( AttributeValue ) }, match );

            var attributeCache = AttributeCache.Get( attributeId );
            var attributeValueFieldName = "Value";
            Type attributeValueFieldType = typeof( string );
            if ( attributeCache != null )
            {
                attributeValueFieldName = attributeCache.FieldType.Field.AttributeValueFieldName;
                attributeValueFieldType = attributeCache.FieldType.Field.AttributeValueFieldType;
            }

            MemberExpression valueProperty = Expression.Property( attributeValueParameter, attributeValueFieldName );

            Expression valueLambda = Expression.Lambda( valueProperty, new ParameterExpression[] { attributeValueParameter } );

            Expression selectValue = Expression.Call( typeof( Queryable ), "Select", new Type[] { typeof( AttributeValue ), attributeValueFieldType }, whereExpression, valueLambda );

            Expression firstOrDefault = Expression.Call( typeof( Queryable ), "FirstOrDefault", new Type[] { attributeValueFieldType }, selectValue );

            return firstOrDefault;
        }

        #endregion

        #region Support classes

        /// <summary>
        /// The available text outuput options
        /// </summary>
        public enum ReportOutputBuilderFieldContentSpecifier
        {
            /// <summary>
            /// Output fields contain formatted text data.
            /// </summary>
            FormattedText = 0,
            /// <summary>
            /// Output fields retain their data type and format as it is defined in the data model.
            /// </summary>
            RawValue = 1,
        }

        /// <summary>
        /// Contains the result of the build process for a tabular report.
        /// </summary>
        public class TabularReportOutputResult
        {
            /// <summary>
            /// A table containing the columns and rows of data representing the report output.
            /// </summary>
            public DataTable Data { get; set; }

            /// <summary>
            /// A dictionary containing entries to map Report Template Field unique identifiers to DataTable column names.
            /// </summary>
            public Dictionary<Guid, string> ReportFieldToDataColumnMap { get; set; }
        }

        /// <summary>
        /// Stores information required to map a field from a report template to an output table column or grid column.
        /// </summary>
        private class ReportFieldMap
        {
            /// <summary>
            /// The unique identifier of the ReportField in the Report template.
            /// </summary>
            public Guid ReportFieldGuid { get; set; }

            /// <summary>
            /// The temporary column name  generated for this field in the query used to retrieve the report data.
            /// </summary>
            public string QueryColumnName { get; set; }

            /// <summary>
            /// The column name assigned to this report field in the DataTable containing the final output.
            /// </summary>
            public string TableColumnName { get; set; }

            /// <summary>
            /// Flag to indicate if the DataTable column can accept a Null value.
            /// </summary>
            public bool TableColumnIsNullable { get; set; }

            /// <summary>
            /// Specifies the UI field that provides the output formatting for this report field.
            /// Note: These value-formatting functions should be moved to a non-UI component.
            /// </summary>
            public RockBoundField BoundField { get; set; }

            /// <summary>
            /// Specifies the value type of this field prior to formatting.
            /// </summary>
            public Type RawValueType { get; set; }

            public override string ToString()
            {
                return string.Format( "{0} ({1})", ReportFieldGuid, TableColumnName );
            }
        }

        #endregion
    }
}
