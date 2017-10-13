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
using System.IO;
using System.Linq;
using Quartz;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// Job to take care of schema changes ( dynamic Attribute Value Fields ) and data updates to Family analytic tables
    /// </summary>
    /// <seealso cref="Quartz.IJob" />
    [DisallowConcurrentExecution]
    [IntegerField( "Command Timeout", "Maximum amount of time (in seconds) to wait for the SQL Query to complete. Leave blank to use the default for this job (1200 seconds). However, it could take several minutes, so you might want to set it at 1200 (20 minutes) or higher", false, 20 * 60, "General", 1, "CommandTimeout" )]
    [BooleanField( "Save SQL for Debug", "Save the SQL that is used in this job to App_Data\\Logs", false, "Advanced", 2, "SaveSQLForDebug" )]
    public class ProcessAnalyticsDimFamily : IJob
    {
        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public ProcessAnalyticsDimFamily()
        {
        }

        private int _columnsAdded = 0;
        private int _columnsModified = 0;
        private int _columnsRemoved = 0;
        private int _rowsInserted = 0;
        private int _rowsUpdated = 0;
        private int _rowsMarkedAsHistory = 0;
        private int _attributeFieldsUpdated = 0;
        private int? _commandTimeout = null;
        private List<string> _sqlLogs = new List<string>();

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            // get the configured timeout, or default to 20 minutes if it is blank
            _commandTimeout = dataMap.GetString( "CommandTimeout" ).AsIntegerOrNull() ?? 1200;

            List<EntityField> analyticsSourceFamilyHistoricalFields = EntityHelper.GetEntityFields( typeof( Rock.Model.AnalyticsSourceFamilyHistorical ), false, false );
            int groupTypeIdFamily = GroupTypeCache.GetFamilyGroupType().Id;
            string groupTypeIdFamilyQualifier = groupTypeIdFamily.ToString();

            List<AttributeCache> familyAnalyticAttributes = EntityHelper.GetEntityFields( typeof( Rock.Model.Group ), false, false )
                .Where( a => a.FieldKind == FieldKind.Attribute && a.AttributeGuid.HasValue )
                .Select( a => AttributeCache.Read( a.AttributeGuid.Value ) )
                .Where( a => a != null )
                .Where( a => a.EntityTypeQualifierColumn == "GroupTypeId" && a.EntityTypeQualifierValue == groupTypeIdFamilyQualifier )
                .Where( a => a.IsAnalytic )
                .ToList();

            try
            {
                // Ensure that the Schema of AnalyticsSourceFamilyHistorical matches the current fields for Attributes that are marked as IsAnalytic
                UpdateAnalyticsSourceFamilyHistoricalSchema( analyticsSourceFamilyHistoricalFields, familyAnalyticAttributes );

                // start the update process by marking records as History if any of the "IsAnalyticHistory" values 
                // have changed for attributes 
                MarkAsHistoryUsingAttributeValues( familyAnalyticAttributes );

                // run the main spAnalytics_ETL_Family stored proc to take care of all the non-attribute related data
                var etlResult = DbService.GetDataTable( "EXEC [dbo].[spAnalytics_ETL_Family]", CommandType.Text, null );
                if ( etlResult.Rows.Count == 1 )
                {
                    _rowsInserted = etlResult.Rows[0]["RowsInserted"] as int? ?? 0;
                    _rowsUpdated = etlResult.Rows[0]["RowsUpdated"] as int? ?? 0;
                }

                // finish up by updating Attribute Values in the Analytic tables for attributes 
                UpdateAttributeValues( familyAnalyticAttributes );

                string resultText = string.Empty;
                if ( _columnsAdded != 0 || _columnsModified != 0 || _columnsRemoved != 0 )
                {
                    resultText = $"Added {_columnsAdded}, modified {_columnsModified}, and removed {_columnsRemoved} attribute columns.\n";
                }

                resultText += $"Marked {_rowsMarkedAsHistory} records as History, updated {_rowsUpdated} records, and updated {_attributeFieldsUpdated} attribute values, and inserted {_rowsInserted} records.";

                context.Result = resultText;
            }
            finally
            {
                if ( dataMap.GetString( "SaveSQLForDebug" ).AsBoolean() )
                {
                    LogSQL( "ProcessAnalyticsDimFamily.sql", _sqlLogs.AsDelimited( "\n" ).ToString() );
                }
            }
        }

        /// <summary>
        /// Logs the SQL.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="message">The message.</param>
        private void LogSQL( string fileName, string message )
        {
            try
            {
                string directory = AppDomain.CurrentDomain.BaseDirectory;
                directory = Path.Combine( directory, "App_Data", "Logs" );

                if ( !Directory.Exists( directory ) )
                {
                    Directory.CreateDirectory( directory );
                }

                string filePath = Path.Combine( directory, fileName );

                File.WriteAllText( filePath, message );
            }
            catch
            {
                // ignore
            }
        }

        /// <summary>
        /// Marks Family Analytic rows as history if the value of the attribute has changed
        /// </summary>
        /// <param name="familyAnalyticAttributes">The family analytic attributes.</param>
        private void MarkAsHistoryUsingAttributeValues( List<AttributeCache> familyAnalyticAttributes )
        {
            using ( var rockContext = new RockContext() )
            {
                var attributeValueService = new AttributeValueService( rockContext );

                foreach ( var attribute in familyAnalyticAttributes.Where( a => a.IsAnalyticHistory ) )
                {
                    var columnName = attribute.Key.RemoveSpecialCharacters();


                    var attributeValuesQry = attributeValueService.Queryable()
                            .Where( a => a.AttributeId == attribute.Id )
                            .Select( a => a.Value );

                    // get all the unique possible values that are currently being used
                    var familyAttributeValues = attributeValuesQry.Distinct().ToList();
                    foreach ( var familyAttributeValue in familyAttributeValues )
                    {
                        object attributeValue;
                        if ( UseFormatValueForUpdate( attribute ) )
                        {
                            attributeValue = attribute.FieldType.Field.FormatValue( null, familyAttributeValue, attribute.QualifierValues, false );
                        }
                        else
                        {
                            attributeValue = attribute.FieldType.Field.ValueAsFieldType( null, familyAttributeValue, attribute.QualifierValues );
                        }

                        // mass update any AnalyticsSourceFamilyHistorical records that need to be marked as History for this Attribute's Value
                        var markAsHistorySQL = $@"
DECLARE 
    @EtlDate DATE = convert( DATE, SysDateTime() )

UPDATE [AnalyticsSourceFamilyHistorical] 
    SET [CurrentRowIndicator] = 0, [ExpireDate] = @EtlDate 
    WHERE [FamilyId] IN (SELECT EntityId FROM AttributeValue WHERE AttributeId = {attribute.Id} AND Value = @familyAttributeValue ) 
    AND isnull([{columnName}],'') != @attributeValue AND [CurrentRowIndicator] = 1
    AND FamilyId NOT IN( --Ensure that there isn't already a History Record for the current EtlDate 
        SELECT FamilyId
        FROM AnalyticsSourceFamilyHistorical x
        WHERE CurrentRowIndicator = 0
        AND[ExpireDate] = @EtlDate
    )";

                        var parameters = new Dictionary<string, object>();
                        parameters.Add( "@familyAttributeValue", familyAttributeValue );
                        parameters.Add( "@attributeValue", attributeValue );
                        this._rowsMarkedAsHistory += DbService.ExecuteCommand( markAsHistorySQL, System.Data.CommandType.Text, parameters );

                        _sqlLogs.Add( parameters.Select( a => $"/* {a.Key} = '{a.Value}' */" ).ToList().AsDelimited( "\n" ) + markAsHistorySQL );
                    }

                }
            }
        }

        /// <summary>
        /// Updates Attribute Values
        /// </summary>
        /// <param name="familyAnalyticAttributes">The family analytic attributes.</param>
        /// <returns></returns>
        private void UpdateAttributeValues( List<AttributeCache> familyAnalyticAttributes )
        {
            using ( var rockContext = new RockContext() )
            {
                var attributeValueService = new AttributeValueService( rockContext );

                foreach ( var attribute in familyAnalyticAttributes )
                {
                    var columnName = attribute.Key.RemoveSpecialCharacters();
                    {
                        var attributeValuesQry = attributeValueService.Queryable()
                                .Where( a => a.AttributeId == attribute.Id )
                                .Select( a => a.Value );

                        // get all the unique possible values that are currently being used
                        var familyAttributeValues = attributeValuesQry.Distinct().ToList();
                        foreach ( var familyAttributeValue in familyAttributeValues )
                        {
                            object attributeValue;
                            if ( UseFormatValueForUpdate( attribute ) )
                            {
                                attributeValue = attribute.FieldType.Field.FormatValue( null, familyAttributeValue, attribute.QualifierValues, false );
                            }
                            else
                            {
                                attributeValue = attribute.FieldType.Field.ValueAsFieldType( null, familyAttributeValue, attribute.QualifierValues );
                            }

                            // mass update the value for the Attribute in the AnalyticsSourceFamilyHistorical records 
                            // Don't update the Historical Records, even if it was just a Text change.  For example, if they changed DefinedValue "Member" to "Owner", 
                            // have the historical records say "Member" even though it is the same definedvalue id
                            var updateSql = $@"
UPDATE [AnalyticsSourceFamilyHistorical] 
    SET [{columnName}] = @attributeValue 
    WHERE [FamilyId] IN (SELECT [EntityId] FROM [AttributeValue] WHERE [AttributeId] = {attribute.Id} AND [Value] = @familyAttributeValue) 
    AND isnull([{columnName}],'') != @attributeValue
    AND [CurrentRowIndicator] = 1";

                            var parameters = new Dictionary<string, object>();
                            parameters.Add( "@familyAttributeValue", familyAttributeValue );
                            parameters.Add( "@attributeValue", attributeValue );
                            _attributeFieldsUpdated += DbService.ExecuteCommand( updateSql, System.Data.CommandType.Text, parameters );

                            _sqlLogs.Add( parameters.Select( a => $"/* {a.Key} = '{a.Value}' */" ).ToList().AsDelimited( "\n" ) + updateSql );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Determines if the Attribute value in the analytics tables needs to be populated using the attribute's field's FormatValue function
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <returns></returns>
        private static bool UseFormatValueForUpdate( AttributeCache attribute )
        {
            if ( attribute.FieldType.Field.AttributeValueFieldName == "Value" )
            {
                if ( attribute.FieldType.Field is Field.Types.TextFieldType )
                {
                    if ( ( attribute.FieldType.Field as Field.Types.TextFieldType ).IsPassword( attribute.QualifierValues ) )
                    {
                        // since this TextField for this attribute has IsPassword=true, let FormatValue populate the value
                        return true;
                    }
                    else
                    {
                        // use the raw value that since TextField's don't do any special formatting
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
            else
            {
                // use the raw value since it would be ValueAsDateTime, ValueAsNumeric, etc
                return false;
            }
        }

        /// <summary>
        /// Updates the analytics source family historical schema.
        /// </summary>
        /// <param name="analyticsSourceFamilyHistoricalFields">The analytics source family historical fields.</param>
        /// <param name="familyAnalyticAttributes">The family analytic attributes.</param>
        private void UpdateAnalyticsSourceFamilyHistoricalSchema( List<EntityField> analyticsSourceFamilyHistoricalFields, List<AttributeCache> familyAnalyticAttributes )
        {
            var dataSet = DbService.GetDataSetSchema( "SELECT * FROM [AnalyticsSourceFamilyHistorical] where 1=0", System.Data.CommandType.Text, null );
            var dataTable = dataSet.Tables[0];

            var analyticsSourceFamilyHistoricalFieldNames = analyticsSourceFamilyHistoricalFields.Select( a => a.Name ).ToList();
            analyticsSourceFamilyHistoricalFieldNames.Add( "ForeignId" );
            analyticsSourceFamilyHistoricalFieldNames.Add( "ForeignGuid" );
            analyticsSourceFamilyHistoricalFieldNames.Add( "ForeignKey" );
            analyticsSourceFamilyHistoricalFieldNames.Add( "Guid" );

            var currentDatabaseAttributeFields = dataTable.Columns.OfType<DataColumn>().Where( a =>
                !analyticsSourceFamilyHistoricalFieldNames.Contains( a.ColumnName ) ).ToList();

            const string BooleanSqlFieldType = "bit";
            const string DateTimeSqlFieldType = "datetime";
            const string NumericSqlFieldType = "[decimal](29,4)";
            const string DefaultSqlFieldType = "nvarchar(250)";

            using ( var rockContext = new RockContext() )
            {
                // add any AttributeFields that aren't already fields on AnalyticsSourceFamilyHistorical
                foreach ( var familyAttribute in familyAnalyticAttributes )
                {
                    var columnName = familyAttribute.Key.RemoveSpecialCharacters();
                    var databaseColumn = currentDatabaseAttributeFields.Where( a => a.ColumnName == columnName ).FirstOrDefault();
                    var familyAttributeValueFieldName = familyAttribute.FieldType.Field.AttributeValueFieldName;
                    string sqlFieldType;
                    if ( familyAttributeValueFieldName == "ValueAsDateTime" )
                    {
                        sqlFieldType = DateTimeSqlFieldType;
                    }
                    else if ( familyAttributeValueFieldName == "ValueAsNumeric" )
                    {
                        sqlFieldType = NumericSqlFieldType;
                    }
                    else if ( familyAttributeValueFieldName == "ValueAsBoolean" )
                    {
                        sqlFieldType = BooleanSqlFieldType;
                    }
                    else
                    {
                        sqlFieldType = DefaultSqlFieldType;
                    }

                    string addColumnSQL = $"ALTER TABLE [AnalyticsSourceFamilyHistorical] ADD [{columnName}] {sqlFieldType} null";

                    string dropColumnSql = $"ALTER TABLE [AnalyticsSourceFamilyHistorical] DROP COLUMN [{columnName}]";

                    if ( databaseColumn == null )
                    {
                        // doesn't exist as a field on the AnalyticsSourceFamilyHistorical table, so create it
                        _columnsAdded++;
                        rockContext.Database.ExecuteSqlCommand( addColumnSQL );
                    }
                    else
                    {
                        // it does exist as a field on the AnalyticsSourceFamilyHistorical table, but make sure the datatype is correct
                        bool dropCreate = false;

                        if ( databaseColumn.DataType == typeof( decimal ) )
                        {
                            dropCreate = sqlFieldType != NumericSqlFieldType;
                        }
                        else if ( databaseColumn.DataType == typeof( DateTime ) )
                        {
                            dropCreate = sqlFieldType != DateTimeSqlFieldType;
                        }
                        else if ( databaseColumn.DataType == typeof( bool ) )
                        {
                            dropCreate = sqlFieldType != BooleanSqlFieldType;
                        }
                        else
                        {
                            dropCreate = sqlFieldType != DefaultSqlFieldType;
                        }

                        if ( dropCreate )
                        {
                            // the attribute fieldtype must have changed. Drop and recreate the column
                            _columnsModified++;
                            rockContext.Database.ExecuteSqlCommand( dropColumnSql );
                            rockContext.Database.ExecuteSqlCommand( addColumnSQL );
                        }
                    }
                }

                // remove any AnalyticsSourceFamilyHistorical attribute fields that aren't attributes on Family
                var familyAttributeColumnNames = familyAnalyticAttributes.Select( a => a.Key.RemoveSpecialCharacters() ).ToList();
                foreach ( var databaseAttributeField in currentDatabaseAttributeFields )
                {
                    if ( !familyAttributeColumnNames.Contains( databaseAttributeField.ColumnName ) )
                    {
                        var dropColumnSql = $"ALTER TABLE [AnalyticsSourceFamilyHistorical] DROP COLUMN [{databaseAttributeField.ColumnName}]";

                        _columnsRemoved++;
                        rockContext.Database.ExecuteSqlCommand( dropColumnSql );
                    }
                }

                // refresh the view definitions just in case the schema changed
                // NOTE: Order is important!
                rockContext.Database.ExecuteSqlCommand( "exec sp_refreshview [AnalyticsDimFamilyHistorical]" );
                rockContext.Database.ExecuteSqlCommand( "exec sp_refreshview [AnalyticsDimFamilyCurrent]" );
            }
        }
    }
}
