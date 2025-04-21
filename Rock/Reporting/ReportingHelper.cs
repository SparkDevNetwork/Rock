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
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using Rock.Data;
using Rock.Model;
using Rock.Reporting.DataFilter;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Reporting
{
    /// <summary>
    /// 
    /// </summary>
    public static class ReportingHelper
    {
        /// <summary>
        /// 
        /// </summary>
        public class BindGridOptions
        {
            /// <summary>
            /// Gets or sets the report database context.
            /// </summary>
            /// <value>
            /// The report database context.
            /// </value>
            public System.Data.Entity.DbContext ReportDbContext { get; set; }

            /// <summary>
            /// Gets or sets the current person.
            /// </summary>
            /// <value>
            /// The current person.
            /// </value>
            public Person CurrentPerson { get; set; }

            /// <summary>
            /// Gets or sets the data view filter overrides.
            /// </summary>
            /// <value>
            /// The data view filter overrides.
            /// </value>
            public DataViewFilterOverrides DataViewFilterOverrides { get; set; }

            /// <summary>
            /// Gets or sets the database timeout seconds.
            /// </summary>
            /// <value>
            /// The database timeout seconds.
            /// </value>
            public int? DatabaseTimeoutSeconds { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this instance is communication.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is communication; otherwise, <c>false</c>.
            /// </value>
            public bool IsCommunication { get; set; }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        /// <param name="report">The report.</param>
        /// <param name="gReport">The g report.</param>
        /// <param name="bindGridOptions">The bind grid options.</param>
        /// <exception cref="ArgumentNullException">Report must be specified</exception>
        /// <exception cref="Rock.Reporting.RockReportingException">Unable to determine EntityType type for {report.EntityType}</exception>
        public static void BindGrid( Report report, Grid gReport, BindGridOptions bindGridOptions )
        {
            if ( report == null )
            {
                throw new ArgumentNullException( "Report must be specified" );
            }

            if ( !report.EntityTypeId.HasValue )
            {
                gReport.Visible = false;
                return;
            }

            var currentPerson = bindGridOptions?.CurrentPerson;

            var rockContext = new RockContext();

            if ( !report.IsAuthorized( Authorization.VIEW, currentPerson ) )
            {
                gReport.Visible = false;
                return;
            }

            Type entityType = EntityTypeCache.Get( report.EntityTypeId.Value, rockContext ).GetEntityType();
            if ( entityType == null )
            {
                throw new RockReportException( report, $"Unable to determine EntityType type for {report.EntityType} " );
            }

            Stopwatch stopwatch = Stopwatch.StartNew();
            gReport.EntityTypeId = report.EntityTypeId;

            bool isPersonDataSet = report.EntityTypeId == EntityTypeCache.Get( typeof( Rock.Model.Person ), true, rockContext ).Id;

            if ( isPersonDataSet )
            {
                gReport.PersonIdField = "Id";
                gReport.DataKeyNames = new string[] { "Id" };
            }
            else
            {
                gReport.PersonIdField = null;
            }

            if ( report.EntityTypeId.HasValue )
            {
                gReport.RowItemText = EntityTypeCache.Get( report.EntityTypeId.Value, rockContext ).FriendlyName;
            }

            List<EntityField> entityFields = Rock.Reporting.EntityHelper.GetEntityFields( entityType, true, false );

            var selectedEntityFields = new Dictionary<int, EntityField>();
            var selectedAttributes = new Dictionary<int, AttributeCache>();
            var selectedComponents = new Dictionary<int, ReportField>();

            // if there is a selectField, keep it to preserve which items are checked
            var selectField = gReport.Columns.OfType<SelectField>().FirstOrDefault();
            gReport.Columns.Clear();
            int columnIndex = 0;

            if ( !string.IsNullOrWhiteSpace( gReport.PersonIdField ) )
            {
                // if we already had a selectField, use it (to preserve checkbox state)
                gReport.Columns.Add( selectField ?? new SelectField() );
                columnIndex++;
            }

            var reportFieldSortExpressions = new Dictionary<Guid, string>();

            gReport.CommunicateMergeFields = new List<string>();
            gReport.CommunicationRecipientPersonIdFields = new List<string>();

            foreach ( var reportField in report.ReportFields.OrderBy( a => a.ColumnOrder ) )
            {
                bool isMergeField = reportField.IsCommunicationMergeField.HasValue && reportField.IsCommunicationMergeField.Value;
                bool isRecipientField = reportField.IsCommunicationRecipientField.HasValue && reportField.IsCommunicationRecipientField.Value;

                columnIndex++;
                if ( reportField.ReportFieldType == ReportFieldType.Property )
                {
                    var entityField = entityFields.FirstOrDefault( a => a.Name == reportField.Selection );

                    if ( entityField == null )
                    {
                        // If the reportField selection refers to a non-existent entityField, ignore this field.
                        continue;
                    }

                    selectedEntityFields.Add( columnIndex, entityField );

                    BoundField boundField = entityField.GetBoundFieldType();
                    boundField.DataField = string.Format( "Entity_{0}_{1}", entityField.Name, columnIndex );
                    boundField.HeaderText = string.IsNullOrWhiteSpace( reportField.ColumnHeaderText ) ? entityField.Title : reportField.ColumnHeaderText;
                    boundField.SortExpression = boundField.DataField;
                    reportFieldSortExpressions.AddOrReplace( reportField.Guid, boundField.SortExpression );
                    boundField.Visible = reportField.ShowInGrid;
                    if ( entityField.FieldType.Guid.Equals( SystemGuid.FieldType.DATE_TIME.AsGuid() ) )
                    {
                        boundField.DataFormatString = "{0:g}";
                    }
                    gReport.Columns.Add( boundField );

                    if ( isMergeField )
                    {
                        gReport.CommunicateMergeFields.Add( $"{boundField.DataField}|{boundField.HeaderText.RemoveSpecialCharacters()}" );
                    }

                    if ( isRecipientField )
                    {
                        gReport.CommunicationRecipientPersonIdFields.Add( boundField.DataField );
                    }
                }
                else if ( reportField.ReportFieldType == ReportFieldType.Attribute )
                {
                    var fieldInfo = ReportingHelper.GetAttributeReportFieldInfo( reportField.Selection, rockContext );

                    var attributeGuid = fieldInfo.AttributeGuid;
                    if ( attributeGuid == null )
                    {
                        // if an attribute guid is not specified, just ignore this field
                        continue;
                    }

                    var attribute = AttributeCache.Get( attributeGuid.Value, rockContext );
                    if ( attribute == null || !attribute.IsActive )
                    {
                        // if the attribute doesn't exist (or no longer exists), or if the active is not active, just ignore this field
                        continue;
                    }

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
                        var maximumLength = fieldInfo.MaximumLength;

                        boundField = new CallbackField();
                        boundField.HtmlEncode = false;
                        ( boundField as CallbackField ).OnFormatDataValue += ( sender, e ) =>
                        {
                            string resultHtml = null;
                            var attributeValue = e.DataValue ?? attribute.DefaultValueAsType;
                            if ( attributeValue != null )
                            {
                                // If the field has no maximum length specified, show the default condensed value.
                                var condensed = !maximumLength.HasValue;
                                resultHtml = attribute.FieldType.Field.FormatValueAsHtml( gReport, attributeValue.ToString(), attribute.QualifierValues, condensed );
                                if ( maximumLength.HasValue )
                                {
                                    resultHtml = resultHtml.TruncateHtml( maximumLength.Value );
                                }
                            }

                            e.FormattedValue = resultHtml ?? string.Empty;
                        };
                    }

                    boundField.DataField = string.Format( "Attribute_{0}_{1}", attribute.Id, columnIndex );
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

                    if ( isMergeField )
                    {
                        gReport.CommunicateMergeFields.Add( $"{boundField.DataField}|{boundField.HeaderText.RemoveSpecialCharacters()}" );
                    }

                    if ( isRecipientField )
                    {
                        gReport.CommunicationRecipientPersonIdFields.Add( boundField.DataField );
                    }

                }
                else if ( reportField.ReportFieldType == ReportFieldType.DataSelectComponent )
                {
                    selectedComponents.Add( columnIndex, reportField );

                    DataSelectComponent selectComponent = DataSelectContainer.GetComponent( reportField.DataSelectComponentEntityType.Name );
                    if ( selectComponent == null )
                    {
                        throw new RockReportFieldExpressionException( reportField, $"Unable to determine select component for {reportField.DataSelectComponentEntityType.Name}" );
                    }

                    DataControlField columnField = selectComponent.GetGridField( entityType, reportField.Selection ?? string.Empty );
                    string fieldId = $"{selectComponent.ColumnPropertyName}_{columnIndex}";

                    if ( columnField is BoundField )
                    {
                        ( columnField as BoundField ).DataField = $"Data_{fieldId}";
                        var customSortProperties = selectComponent.SortProperties( reportField.Selection ?? string.Empty );
                        bool sortReversed = selectComponent.SortReversed( reportField.Selection ?? string.Empty );
                        if ( customSortProperties != null )
                        {
                            if ( customSortProperties == string.Empty )
                            {
                                // disable sorting if customSortExpression set to string.empty
                                columnField.SortExpression = string.Empty;
                            }
                            else
                            {
                                columnField.SortExpression = customSortProperties.Split( ',' ).Select( a => string.Format( "Sort_{0}_{1}", a, columnIndex ) ).ToList().AsDelimited( "," );
                            }
                        }
                        else
                        {
                            // use default sorting if customSortExpression was null
                            columnField.SortExpression = ( columnField as BoundField ).DataField;
                        }

                        if ( sortReversed == true && !string.IsNullOrWhiteSpace( columnField.SortExpression ) )
                        {
                            columnField.SortExpression = columnField.SortExpression + " DESC";
                        }
                    }

                    columnField.HeaderText = string.IsNullOrWhiteSpace( reportField.ColumnHeaderText ) ? selectComponent.ColumnHeaderText : reportField.ColumnHeaderText;
                    if ( !string.IsNullOrEmpty( columnField.SortExpression ) )
                    {
                        reportFieldSortExpressions.AddOrReplace( reportField.Guid, columnField.SortExpression );
                    }

                    columnField.Visible = reportField.ShowInGrid;
                    gReport.Columns.Add( columnField );

                    if ( isMergeField )
                    {
                        gReport.CommunicateMergeFields.Add( $"Data_{fieldId}|{columnField.HeaderText.RemoveSpecialCharacters()}" );
                    }
                    if ( isRecipientField )
                    {
                        string fieldName = ( selectComponent is IRecipientDataSelect ) ? $"Recipient_{fieldId}" : $"Data_{fieldId}";
                        gReport.CommunicationRecipientPersonIdFields.Add( fieldName );
                    }
                }
            }

            // if no fields are specified, show the default fields (Previewable/All) for the EntityType
            var dataColumns = gReport.Columns.OfType<object>().Where( a => a.GetType() != typeof( SelectField ) );
            if ( dataColumns.Count() == 0 )
            {
                // show either the Previewable Columns or all (if there are no previewable columns)
                bool showAllColumns = !entityFields.Any( a => a.FieldKind == FieldKind.Property && a.IsPreviewable );
                foreach ( var entityField in entityFields.Where( a => a.FieldKind == FieldKind.Property ) )
                {
                    columnIndex++;
                    selectedEntityFields.Add( columnIndex, entityField );

                    BoundField boundField = entityField.GetBoundFieldType();

                    boundField.DataField = string.Format( "Entity_{0}_{1}", entityField.Name, columnIndex );
                    boundField.HeaderText = entityField.Name;
                    boundField.SortExpression = boundField.DataField;
                    boundField.Visible = showAllColumns || entityField.IsPreviewable;
                    gReport.Columns.Add( boundField );
                }
            }

            try
            {
                gReport.Visible = true;
                gReport.ExportFilename = report.Name;
                SortProperty sortProperty = gReport.SortProperty;
                if ( sortProperty == null )
                {
                    var reportSort = new SortProperty();
                    var sortColumns = new Dictionary<string, SortDirection>();
                    foreach ( var reportField in report.ReportFields.Where( a => a.SortOrder.HasValue ).OrderBy( a => a.SortOrder.Value ) )
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

                var qryErrors = new List<string>();
                var reportDbContext = bindGridOptions?.ReportDbContext;
                if ( reportDbContext == null )
                {
                    reportDbContext = Reflection.GetDbContextForEntityType( entityType );
                }

                ReportGetQueryableArgs reportGetQueryableArgs = new ReportGetQueryableArgs
                {
                    ReportDbContext = reportDbContext as Rock.Data.DbContext,
                    EntityFields = selectedEntityFields,
                    Attributes = selectedAttributes,
                    SelectComponents = selectedComponents,
                    SortProperty = sortProperty,
                    DataViewFilterOverrides = bindGridOptions?.DataViewFilterOverrides,
                    DatabaseTimeoutSeconds = bindGridOptions?.DatabaseTimeoutSeconds ?? 180,
                    IsCommunication = bindGridOptions?.IsCommunication ?? false
                };

                dynamic qry = report.GetQueryable( reportGetQueryableArgs );

                if ( !string.IsNullOrEmpty( report.QueryHint ) && reportDbContext is RockContext )
                {
                    using ( new QueryHintScope( reportDbContext as RockContext, report.QueryHint ) )
                    {
                        gReport.SetLinqDataSource( qry );
                    }
                }
                else
                {
                    gReport.SetLinqDataSource( qry );
                }

                gReport.DataBind();
                stopwatch.Stop();

                ReportService.AddRunReportTransaction( report.Id, Convert.ToInt32( stopwatch.Elapsed.TotalMilliseconds ) );
            }
            catch ( Exception ex )
            {
                Exception exception = ex;
                ExceptionLogService.LogException( ex, HttpContext.Current );

                // recursively find if there was a SQL timeout exception.
                // A SQL Timeout would be something we might expect, so we'll give that a friendly error message.
                var sqlTimeoutException = FindSqlTimeoutException( ex );

                if ( sqlTimeoutException != null )
                {
                    // if there was a SQL Server Timeout, have the warning be a friendly message about that.
                    throw new RockReportException( report, "This report did not complete in a timely manner. You can try again or adjust the timeout setting of this block.", ex );
                }

                // if there isn't a SQL Exception, we'll throw the original exception (since it would be an exception that we wouldn't expect)
                throw;
            }
        }

        /// <summary>
        /// If there is a Sql Timeout Exception, returns a <seealso cref="System.Data.SqlClient.SqlException"/>, otherwise returns null
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <returns></returns>
        public static System.Data.SqlClient.SqlException FindSqlTimeoutException( Exception ex )
        {
            System.Data.SqlClient.SqlException sqlException = null;
            var exception = ex;
            while ( exception != null )
            {
                if ( exception is System.Data.SqlClient.SqlException )
                {
                    sqlException = exception as System.Data.SqlClient.SqlException;

                    if ( sqlException != null && sqlException.Number == -2 )
                    {
                        return sqlException;
                    }
                }

                exception = exception.InnerException;
            }

            return null;
        }

        /// <summary>
        /// Gets the filter overrides from controls.
        /// </summary>
        /// <param name="dataView">The data view.</param>
        /// <param name="phFilters">The ph filters.</param>
        /// <returns></returns>
        public static DataViewFilterOverrides GetFilterOverridesFromControls( DataView dataView, PlaceHolder phFilters )
        {
            if ( phFilters.Controls.Count > 0 )
            {
                var dataViewFilter = GetFilterFromControls( phFilters );
                var dataViewFilterOverrideList = phFilters.ControlsOfTypeRecursive<FilterField>().Select( a => new DataViewFilterOverride
                {
                    DataFilterGuid = a.DataViewFilterGuid,
                    IncludeFilter = a.ShowCheckbox ? a.CheckBoxChecked.GetValueOrDefault( true ) : true,
                    Selection = a.GetSelection()
                } ).ToList();

                List<int> ignoreDataViewPersistedValues = new List<int>();

                if ( dataView != null )
                {
                    // only include overrides that are different than the saved dataview's filter
                    var filters = GetFilterInfoList( dataView );
                    foreach ( var dataViewFilterOverride in dataViewFilterOverrideList.ToList().Where( a => a.IncludeFilter == true ) )
                    {
                        var originalFilter = filters.FirstOrDefault( a => a.Guid == dataViewFilterOverride.DataFilterGuid );
                        if ( originalFilter != null )
                        {
                            if ( dataViewFilterOverride.IncludeFilter && originalFilter.Selection == dataViewFilterOverride.Selection )
                            {
                                // the filter override is the same as the saved dataview, so no need to override it
                                dataViewFilterOverrideList.Remove( dataViewFilterOverride );
                            }
                            else
                            {
                                // if the selection has changed, and it is from a 'other data view'  filter, add the other dataview and the dataviews it impacts to the list of dataviews that should not use the persisted values
                                if ( originalFilter.ImpactedDataViews?.Any() == true )
                                {
                                    ignoreDataViewPersistedValues.AddRange( originalFilter.ImpactedDataViews.Select( a => a.Id ).ToList() );
                                }
                            }
                        }
                    }
                }

                return new DataViewFilterOverrides( dataViewFilterOverrideList ) { IgnoreDataViewPersistedValues = new HashSet<int>( ignoreDataViewPersistedValues ) };
            }

            return null;
        }

        /// <summary>
        /// Gets the dataviewfilter from the current values in the filter controls
        /// </summary>
        /// <param name="phFilters">The ph filters.</param>
        /// <returns></returns>
        public static DataViewFilter GetFilterFromControls( PlaceHolder phFilters )
        {
            if ( phFilters.Controls.Count > 0 )
            {
                return GetFilterControl( phFilters.Controls[0] );
            }

            return null;
        }

        /// <summary>
        /// Gets the filter control.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns></returns>
        private static DataViewFilter GetFilterControl( Control control )
        {
            FilterGroup filterGroupControl = control as FilterGroup;
            if ( filterGroupControl != null )
            {
                return GetFilterGroupControl( filterGroupControl );
            }

            FilterField filterFieldControl = control as FilterField;
            if ( filterFieldControl != null )
            {
                return GetFilterFieldControl( filterFieldControl );
            }

            return null;
        }

        /// <summary>
        /// Gets the filter group control.
        /// </summary>
        /// <param name="filterGroup">The filter group.</param>
        /// <returns></returns>
        private static DataViewFilter GetFilterGroupControl( FilterGroup filterGroup )
        {
            DataViewFilter filter = new DataViewFilter();
            filter.Guid = filterGroup.DataViewFilterGuid;
            filter.ExpressionType = filterGroup.FilterType;
            foreach ( Control control in filterGroup.Controls )
            {
                DataViewFilter childFilter = GetFilterControl( control );
                if ( childFilter != null )
                {
                    filter.ChildFilters.Add( childFilter );
                }
            }

            return filter;
        }

        /// <summary>
        /// Gets the filter field control.
        /// </summary>
        /// <param name="filterField">The filter field.</param>
        /// <returns></returns>
        private static DataViewFilter GetFilterFieldControl( FilterField filterField )
        {
            if ( filterField.ShowCheckbox && !filterField.CheckBoxChecked.GetValueOrDefault( true ) )
            {
                return null;
            }

            DataViewFilter filter = new DataViewFilter();
            filter.Guid = filterField.DataViewFilterGuid;
            filter.ExpressionType = FilterExpressionType.Filter;
            filter.Expanded = filterField.Expanded;
            if ( filterField.FilterEntityTypeName != null )
            {
                filter.EntityTypeId = EntityTypeCache.Get( filterField.FilterEntityTypeName ).Id;
                filter.Selection = filterField.GetSelection();
                filter.RelatedDataViewId = filterField.GetRelatedDataViewId();
            }

            return filter;
        }

        #region FilterInfo Helpers

        /// <summary>
        /// Helper class to show the configuration of which fields are shown and configurable 
        /// </summary>
        public class FilterInfo
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="FilterInfo"/> class.
            /// </summary>
            /// <param name="dataViewFilter">The data view filter.</param>
            public FilterInfo( DataViewFilter dataViewFilter )
            {
                DataViewFilter = dataViewFilter;
            }

            private DataViewFilter DataViewFilter { get; set; }

            /// <summary>
            /// Gets or sets the unique identifier.
            /// </summary>
            /// <value>
            /// The unique identifier.
            /// </value>
            public Guid Guid
            {
                get
                {
                    return this.DataViewFilter.Guid;
                }
            }

            /// <summary>
            /// Gets or sets the title.
            /// </summary>
            /// <value>
            /// The title.
            /// </value>
            public string Title { get; set; }

            /// <summary>
            /// Gets the title including the parent filter titles
            /// </summary>
            /// <value>
            /// The title path.
            /// </value>
            public string TitlePath
            {
                get
                {
                    string parentPath = this.Title;
                    var parentFilter = this.ParentFilter;
                    while ( parentFilter != null )
                    {
                        if ( parentFilter.ParentFilter == null )
                        {
                            // don't include the root group filter if it is just a 'Group All'
                            if ( parentFilter.FilterExpressionType == FilterExpressionType.GroupAll )
                            {
                                break;
                            }
                        }

                        parentPath = parentFilter.Title + " > " + parentPath;
                        parentFilter = parentFilter.ParentFilter;
                    }

                    return parentPath;
                }
            }

            /// <summary>
            /// Gets or sets the summary.
            /// </summary>
            /// <value>
            /// The summary.
            /// </value>
            public string Summary
            {
                get
                {
                    string result;
                    if ( FilterExpressionType != FilterExpressionType.Filter )
                    {
                        var childFilters = this.AllFilterList.Where( a => a.ParentFilter == this ).ToList();
                        var parentSummaries = childFilters.Select( a => a.Summary ?? string.Empty ).ToList().AsDelimited( ", ", this.FilterExpressionType == FilterExpressionType.GroupAny ? " OR " : " AND " );
                        if ( childFilters.Count > 1 )
                        {
                            result = string.Format( "( {0} )", parentSummaries );
                        }
                        else
                        {
                            result = parentSummaries;
                        }
                    }
                    else if ( this.Component != null )
                    {
                        result = this.Component.FormatSelection( this.ReportEntityTypeModel, this.Selection );
                    }
                    else
                    {
                        result = "-";
                    }

                    return result;
                }
            }

            /// <summary>
            /// Gets or sets the type of the filter expression.
            /// </summary>
            /// <value>
            /// The type of the filter expression.
            /// </value>
            public FilterExpressionType FilterExpressionType
            {
                get
                {
                    return this.DataViewFilter.ExpressionType;
                }
            }

            /// <summary>
            /// Gets or sets the parent filter.
            /// </summary>
            /// <value>
            /// The parent filter.
            /// </value>
            public FilterInfo ParentFilter
            {
                get
                {
                    return this.DataViewFilter.ParentId.HasValue ? this.AllFilterList.FirstOrDefault( a => a.Guid == this.DataViewFilter.Parent.Guid ) : null;
                }
            }

            /// <summary>
            /// Gets or sets all filter list.
            /// </summary>
            /// <value>
            /// All filter list.
            /// </value>
            internal List<FilterInfo> AllFilterList { get; set; }

            /// <summary>
            /// Gets or sets the component.
            /// </summary>
            /// <value>
            /// The component.
            /// </value>
            public DataFilterComponent Component { get; internal set; }

            /// <summary>
            /// Gets or sets the report entity type model.
            /// </summary>
            /// <value>
            /// The report entity type model.
            /// </value>
            public Type ReportEntityTypeModel { get; internal set; }

            /// <summary>
            /// If this Filter is an OtherDataViewFilter, gets the DataView that this filter has selected
            /// </summary>
            /// <value>
            /// The parent data view.
            /// </value>
            public DataView SelectedDataView
            {
                get
                {
                    var selectionDataView = ( this.Component as OtherDataViewFilter )?.GetSelectedDataView( this.Selection );
                    return selectionDataView;
                }
            }

            /// <summary>
            /// If this Filter is part of another dataview, gets the DataView that this filter is from
            /// </summary>
            /// <value>
            /// From other data view identifier.
            /// </value>
            public DataView FromDataView { get; internal set; }

            /// <summary>
            /// Gets the data views that would be impacted if this filter was customized
            /// </summary>
            /// <value>
            /// The impacted data views.
            /// </value>
            public List<DataView> ImpactedDataViews
            {
                get
                {
                    if ( FromDataView == null && !( this.Component is OtherDataViewFilter ) )
                    {
                        return new List<DataView>();
                    }

                    List<DataView> impactedDataViews = new List<DataView>();
                    impactedDataViews.Add( FromDataView );

                    GetImpactedDataViewRecursive( impactedDataViews, FromDataView );

                    return impactedDataViews;
                }
            }

            /// <summary>
            /// Gets the impacted data view recursive.
            /// </summary>
            /// <param name="impactedDataViews">The impacted data views.</param>
            /// <param name="dataView">The data view.</param>
            private void GetImpactedDataViewRecursive( List<DataView> impactedDataViews, DataView dataView )
            {
                var dataViewfiltersThatUseDataView = AllFilterList.Where( a => a.SelectedDataView?.Id == dataView.Id ).ToList();
                foreach ( var dataViewFilter in dataViewfiltersThatUseDataView )
                {
                    var impactedDataView = dataViewFilter.FromDataView;
                    if ( impactedDataView != null )
                    {
                        if ( !impactedDataViews.Any( a => a.Id == impactedDataView.Id ) )
                        {
                            impactedDataViews.Add( impactedDataView );
                            GetImpactedDataViewRecursive( impactedDataViews, impactedDataView );
                        }
                    }
                }
            }

            /// <summary>
            /// If this Filter is part of another dataview, gets Name of the DataView that this filter is from
            /// </summary>
            /// <value>
            /// The name of from other data view.
            /// </value>
            public string FromDataViewName => FromDataView?.Name;

            /// <summary>
            /// Gets or sets the selection.
            /// </summary>
            /// <value>
            /// The selection.
            /// </value>
            public string Selection
            {
                get
                {
                    return this.DataViewFilter.Selection;
                }
            }

            /// <summary>
            /// Returns a <see cref="System.String" /> that represents this instance.
            /// </summary>
            /// <returns>
            /// A <see cref="System.String" /> that represents this instance.
            /// </returns>
            public override string ToString()
            {
                return TitlePath;
            }
        }

        /// <summary>
        /// Recursively gets a list of all the Filters in a dataview, and all the filters in it's child dataviews
        /// </summary>
        /// <param name="dataView">The data view.</param>
        /// <returns></returns>
        public static List<FilterInfo> GetFilterInfoList( DataView dataView )
        {
            List<FilterInfo> filterList = new List<FilterInfo>();
            GetFilterListRecursive( filterList, dataView.DataViewFilter, dataView.EntityType );

            // now that we have the full filter list, set the AllFilterList of all the filters
            foreach ( var filter in filterList )
            {
                filter.AllFilterList = filterList;
            }

            // set FromDataView from this dataview's datafilters
            var dataViewDataFilters = filterList.Where( a => a.ParentFilter != null && a.ParentFilter.Guid == dataView.DataViewFilter.Guid ).ToList();
            foreach ( var dataViewDataFilter in dataViewDataFilters )
            {
                dataViewDataFilter.FromDataView = dataView;
            }


            return filterList;
        }

        /// <summary>
        /// Gets the filter list recursive.
        /// </summary>
        /// <param name="filterList">The filter list.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="reportEntityType">Type of the report entity.</param>
        private static void GetFilterListRecursive( List<FilterInfo> filterList, DataViewFilter filter, EntityType reportEntityType )
        {
            var result = new Dictionary<Guid, string>();

            var entityType = EntityTypeCache.Get( filter.EntityTypeId ?? 0 );
            var reportEntityTypeCache = EntityTypeCache.Get( reportEntityType );
            var reportEntityTypeModel = reportEntityTypeCache.GetEntityType();

            var filterInfo = new FilterInfo( filter );
            filterInfo.AllFilterList = filterList;

            if ( entityType != null )
            {
                var component = Rock.Reporting.DataFilterContainer.GetComponent( entityType.Name );
                filterInfo.Component = component;
                filterInfo.ReportEntityTypeModel = reportEntityTypeModel;

                if ( component != null )
                {
                    if ( component is Rock.Reporting.DataFilter.EntityFieldFilter )
                    {
                        var entityFieldFilter = component as Rock.Reporting.DataFilter.EntityFieldFilter;
                        var fieldName = entityFieldFilter.GetSelectedFieldName( filter.Selection );
                        if ( !string.IsNullOrWhiteSpace( fieldName ) )
                        {
                            var entityFields = EntityHelper.GetEntityFields( reportEntityTypeModel );
                            var entityField = entityFields.Where( a => a.Name == fieldName ).FirstOrDefault();
                            if ( entityField != null )
                            {
                                filterInfo.Title = entityField.Title;
                            }
                            else
                            {
                                filterInfo.Title = fieldName;
                            }
                        }
                    }
                    else
                    {
                        filterInfo.Title = component.GetTitle( reportEntityType.GetType() );
                    }
                }
            }

            filterList.Add( filterInfo );

            if ( filterInfo.Component is Rock.Reporting.DataFilter.OtherDataViewFilter )
            {
                Rock.Reporting.DataFilter.OtherDataViewFilter otherDataViewFilter = filterInfo.Component as Rock.Reporting.DataFilter.OtherDataViewFilter;
                var otherDataView = otherDataViewFilter.GetSelectedDataView( filterInfo.Selection );
                if ( otherDataView != null )
                {
                    var otherDataViewFilterList = new List<FilterInfo>();
                    GetFilterListRecursive( otherDataViewFilterList, otherDataView.DataViewFilter, reportEntityType );
                    foreach ( var otherFilter in otherDataViewFilterList )
                    {
                        if ( otherFilter.FromDataView == null )
                        {
                            otherFilter.FromDataView = otherDataView;
                        }
                    }

                    filterList.AddRange( otherDataViewFilterList );
                }
            }

            foreach ( var childFilter in filter.ChildFilters )
            {
                GetFilterListRecursive( filterList, childFilter, reportEntityType );
            }
        }

        #endregion

        /// <summary>
        /// Creates an Attribute report field configuration settings object from a settings string.
        /// </summary>
        /// <param name="configurationString"></param>
        /// <param name="rockContext"></param>
        /// <returns></returns>
        public static AttributeReportFieldInfo GetAttributeReportFieldInfo( string configurationString, RockContext rockContext )
        {
            var info = new AttributeReportFieldInfo();

            if ( string.IsNullOrWhiteSpace( configurationString ) )
            {
                return info;
            }

            // If this is a packed string, remove the first parameter.
            if ( configurationString.StartsWith( "Attribute|", StringComparison.OrdinalIgnoreCase ) )
            {
                configurationString = configurationString.Substring( 10 );
            }

            // Try to parse the configuration as a simple Guid.
            var attributeGuid = configurationString.AsGuidOrNull();
            if ( attributeGuid == null )
            {
                // Try to deserialize the field settings from Json.
                var config = configurationString.FromJsonOrNull<AttributeReportFieldConfig>();
                if ( config != null )
                {
                    info.MaximumLength = config.MaximumLength;
                    attributeGuid = config.AttributeGuid;
                }
            }

            if ( attributeGuid != null )
            {
                info.AttributeGuid = attributeGuid;

                var attribute = AttributeCache.Get( attributeGuid.Value, rockContext );
                if ( attribute != null && attribute.IsActive )
                {
                    var hasMaxLength = !attribute.FieldType.Guid.Equals( Rock.SystemGuid.FieldType.DEFINED_VALUE.AsGuid() )
                        && !attribute.FieldType.Guid.Equals( Rock.SystemGuid.FieldType.BOOLEAN.AsGuid() );
                    info.HasMaximumLength = hasMaxLength;
                }
            }

            return info;
        }

        /// <summary>
        /// A data entry view model for an instance of an Attribute Report Field.
        /// </summary>
        public class AttributeReportFieldInfo
        {
            /// <summary>
            /// Gets or sets the Attribute identifier.
            /// </summary>
            public Guid? AttributeGuid { get; set; }

            /// <summary>
            /// Gets or sets the column header displayed on the report.
            /// </summary>
            public string ColumnHeader { get; set; }

            /// <summary>
            /// Gets or sets if the field is visible in the report.
            /// </summary>
            public bool ShowInGrid { get; set; }

            /// <summary>
            /// Gets or sets the maxiumum length of the field text.
            /// </summary>
            /// <remarks>
            /// This flag is only supported for specific fields.
            /// </remarks>
            public int? MaximumLength { get; set; }

            /// <summary>
            /// Gets a flag indicating if the field content supports a maximum length.
            /// </summary>
            public bool HasMaximumLength { get; set; }

            /// <summary>
            /// Gets the serialized configuration for the report field settings.
            /// </summary>
            /// <returns>
            /// A simple string representing an attribute guid, or a JSON string if the field supports multiple configuration properties.
            /// </returns>
            public string SerializeConfiguration()
            {
                // If only the Attribute Guid is set, return a simple string.
                if ( this.MaximumLength == null )
                {
                    if ( this.AttributeGuid.HasValue )
                    {
                        return this.AttributeGuid.Value.ToString( "N" );
                    }
                    else
                    {
                        return null;
                    }
                }

                var config = new AttributeReportFieldConfig
                {
                    AttributeGuid = this.AttributeGuid.Value,
                    MaximumLength = this.MaximumLength.Value
                };
                var json = JsonConvert.SerializeObject( config );
                return json;
            }
        }

        /// <summary>
        /// The configuration settings for a report field containing an Attribute.
        /// </summary>
        private class AttributeReportFieldConfig
        {
            public Guid? AttributeGuid { get; set; }

            public int? MaximumLength { get; set; }
        }
    }
}