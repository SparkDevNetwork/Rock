﻿// <copyright>
// Copyright by the Spark Development Network
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
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;
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
        /// Shows the preview.
        /// </summary>
        /// <param name="report">The report.</param>
        /// <param name="gReport">The g report.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="databaseTimeoutSeconds">The database timeout seconds.</param>
        /// <param name="errorMessage">The error message.</param>
        public static void BindGrid( Report report, Grid gReport, Person currentPerson, int? databaseTimeoutSeconds, out string errorMessage )
        {
            errorMessage = null;
            if ( report != null )
            {
                var errors = new List<string>();

                if ( !report.EntityTypeId.HasValue )
                {
                    gReport.Visible = false;
                    return;
                }

                var rockContext = new RockContext();

                if ( !report.IsAuthorized( Authorization.VIEW, currentPerson ) )
                {
                    gReport.Visible = false;
                    return;
                }

                Type entityType = EntityTypeCache.Read( report.EntityTypeId.Value, rockContext ).GetEntityType();
                if ( entityType == null )
                {
                    errorMessage = string.Format( "Unable to determine entityType for {0}", report.EntityType );
                    return;
                }

                gReport.EntityTypeId = report.EntityTypeId;

                bool isPersonDataSet = report.EntityTypeId == EntityTypeCache.Read( typeof( Rock.Model.Person ), true, rockContext ).Id;

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
                    gReport.RowItemText = EntityTypeCache.Read( report.EntityTypeId.Value, rockContext ).FriendlyName;
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

                foreach ( var reportField in report.ReportFields.OrderBy( a => a.ColumnOrder ) )
                {
                    columnIndex++;
                    if ( reportField.ReportFieldType == ReportFieldType.Property )
                    {
                        var entityField = entityFields.FirstOrDefault( a => a.Name == reportField.Selection );
                        if ( entityField != null )
                        {
                            selectedEntityFields.Add( columnIndex, entityField );

                            BoundField boundField = entityField.GetBoundFieldType();
                            boundField.DataField = string.Format( "Entity_{0}_{1}", entityField.Name, columnIndex );
                            boundField.HeaderText = string.IsNullOrWhiteSpace( reportField.ColumnHeaderText ) ? entityField.Title : reportField.ColumnHeaderText;
                            boundField.SortExpression = boundField.DataField;
                            reportFieldSortExpressions.AddOrReplace( reportField.Guid, boundField.SortExpression );
                            boundField.Visible = reportField.ShowInGrid;
                            gReport.Columns.Add( boundField );
                        }
                    }
                    else if ( reportField.ReportFieldType == ReportFieldType.Attribute )
                    {
                        Guid? attributeGuid = reportField.Selection.AsGuidOrNull();
                        if ( attributeGuid.HasValue )
                        {
                            var attribute = AttributeCache.Read( attributeGuid.Value, rockContext );
                            if ( attribute != null )
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
                                    ( boundField as CallbackField ).OnFormatDataValue += (sender, e) => {
                                        string resultHtml = null;
                                        if (e.DataValue != null)
                                        {
                                            bool condensed = true;
                                            resultHtml = attribute.FieldType.Field.FormatValueAsHtml( gReport, e.DataValue.ToString(), attribute.QualifierValues, condensed );
                                            
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

                                // NOTE:  Additional formatting for attributes is done in the gReport_RowDataBound event
                                gReport.Columns.Add( boundField );
                            }
                        }
                    }
                    else if ( reportField.ReportFieldType == ReportFieldType.DataSelectComponent )
                    {
                        selectedComponents.Add( columnIndex, reportField );

                        DataSelectComponent selectComponent = DataSelectContainer.GetComponent( reportField.DataSelectComponentEntityType.Name );
                        if ( selectComponent != null )
                        {
                            try
                            {
                                DataControlField columnField = selectComponent.GetGridField( entityType, reportField.Selection ?? string.Empty );

                                if ( columnField is BoundField )
                                {
                                    ( columnField as BoundField ).DataField = string.Format( "Data_{0}_{1}", selectComponent.ColumnPropertyName, columnIndex );
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
                            }
                            catch ( Exception ex )
                            {
                                ExceptionLogService.LogException( ex, HttpContext.Current );
                                errors.Add( string.Format( "{0} - {1}", selectComponent, ex.Message ) );
                            }
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
                    dynamic qry = report.GetQueryable( entityType, selectedEntityFields, selectedAttributes, selectedComponents, sortProperty, databaseTimeoutSeconds ?? 180, out qryErrors );
                    errors.AddRange( qryErrors );
                    gReport.SetLinqDataSource( qry );
                    gReport.DataBind();
                }
                catch ( Exception ex )
                {
                    Exception exception = ex;
                    ExceptionLogService.LogException( ex, HttpContext.Current );
                    while ( exception != null )
                    {
                        if ( exception is System.Data.SqlClient.SqlException )
                        {
                            // if there was a SQL Server Timeout, have the warning be a friendly message about that.
                            if ( ( exception as System.Data.SqlClient.SqlException ).Number == -2 )
                            {
                                errorMessage = "This report did not complete in a timely manner. You can try again or adjust the timeout setting of this block.";
                                return;
                            }
                            else
                            {
                                errors.Add( exception.Message );
                                exception = exception.InnerException;
                            }
                        }
                        else
                        {
                            errors.Add( exception.Message );
                            exception = exception.InnerException;
                        }
                    }
                }

                if ( errors.Any() )
                {
                    errorMessage = "WARNING: There was a problem with one or more of the report's data components...<br/><br/> " + errors.AsDelimited( "<br/>" );
                }
            }
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
                filter.EntityTypeId = Rock.Web.Cache.EntityTypeCache.Read( filterField.FilterEntityTypeName ).Id;
                filter.Selection = filterField.GetSelection();
            }

            return filter;
        }

        /// <summary>
        /// Registers the javascript include needed for reporting client controls
        /// </summary>
        /// <param name="filterField">The filter field.</param>
        public static void RegisterJavascriptInclude( FilterField filterField )
        {
            ScriptManager.RegisterClientScriptInclude( filterField, filterField.GetType(), "reporting-include", filterField.RockBlock().RockPage.ResolveRockUrl( "~/Scripts/Rock/reportingInclude.js", true ) );
        }
    }
}
