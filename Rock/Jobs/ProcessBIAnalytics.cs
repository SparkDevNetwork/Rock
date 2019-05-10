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
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;

using Quartz;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// Job to take care of schema changes ( dynamic attribute value fields ) and data updates to the BI related analytic tables
    /// </summary>
    /// <seealso cref="Quartz.IJob" />
    [DisallowConcurrentExecution]
    [BooleanField( "Process Person BI Analytics", "Do the BI Analytics tasks related to the Person Analytics tables", true, "", 1 )]
    [BooleanField( "Process Family BI Analytics", "Do the BI Analytics tasks related to the Family Analytics tables", true, "", 2 )]
    [BooleanField( "Process Campus BI Analytics", "Do the BI Analytics tasks related to the Campus table", true, "", 3 )]
    [BooleanField( "Process Financial Transaction BI Analytics", "Do the BI Analytics tasks related to the Financial Transaction Analytics tables", true, "", 4 )]
    [BooleanField( "Process Attendance BI Analytics", "Do the BI Analytics tasks related to the Attendance Analytics tables", true, "", 5 )]
    [BooleanField( "Refresh Power BI Account Tokens", "Refresh any Power BI Account Tokens to prevent them from expiring.", true, "", 6 )]
    [IntegerField( "Command Timeout", "Maximum amount of time (in seconds) to wait for each SQL command to complete. Leave blank to use the default for this job (3600 seconds). Note that some of the tasks might take a while on larger databases, so you might need to set it higher.", false, 60 * 60, "General", 7, "CommandTimeout" )]
    [BooleanField( "Save SQL for Debug", "Save the SQL that is used in the Person and Family related parts of the this job to App_Data\\Logs", false, "Advanced", 8, "SaveSQLForDebug" )]
    public class ProcessBIAnalytics : IJob
    {
        #region Constructor

        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public ProcessBIAnalytics()
        {
        }

        #endregion Constructor

        #region Private Fields

        /// <summary>
        /// The person job stats
        /// </summary>
        private JobStats _personJobStats = new JobStats();

        /// <summary>
        /// The family job stats
        /// </summary>
        private JobStats _familyJobStats = new JobStats();

        /// <summary>
        /// The campus job stats
        /// </summary>
        private JobStats _campusJobStats = new JobStats();

        private int? _commandTimeout = null;

        #endregion Private Fields

        #region Shared Methods

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            // get the configured timeout, or default to 20 minutes if it is blank
            _commandTimeout = dataMap.GetString( "CommandTimeout" ).AsIntegerOrNull() ?? 1200;

            StringBuilder results = new StringBuilder();

            // Do the stuff for Person related BI Tables
            if ( dataMap.GetString( "ProcessPersonBIAnalytics" ).AsBoolean() )
            {
                ProcessPersonBIAnalytics( context, dataMap );

                results.AppendLine( "Person BI Results:" );
                results.AppendLine( _personJobStats.SummaryMessage );

                context.UpdateLastStatusMessage( results.ToString() );
            }

            // Do the stuff for Family related BI Tables
            if ( dataMap.GetString( "ProcessFamilyBIAnalytics" ).AsBoolean() )
            {
                ProcessFamilyBIAnalytics( context, dataMap );

                results.AppendLine( "Family BI Results:" );
                results.AppendLine( _familyJobStats.SummaryMessage );

                context.UpdateLastStatusMessage( results.ToString() );
            }

            // Do the stuff for Campus related BI Tables
            if ( dataMap.GetString( "ProcessCampusBIAnalytics" ).AsBoolean() )
            {
                ProcessCampusBIAnalytics( context, dataMap );

                results.AppendLine( "Campus BI Results:" );
                results.AppendLine( _campusJobStats.SummaryMessage );
                context.UpdateLastStatusMessage( results.ToString() );
            }

            // Run Stored Proc ETL for Financial Transaction BI Tables
            if ( dataMap.GetString( "ProcessFinancialTransactionBIAnalytics" ).AsBoolean() )
            {
                try
                {
                    int rows = DbService.ExecuteCommand( "EXEC [dbo].[spAnalytics_ETL_FinancialTransaction]", System.Data.CommandType.Text, null, _commandTimeout );
                    results.AppendLine( "FinancialTransaction ETL completed." );

                    context.UpdateLastStatusMessage( results.ToString() );
                }
                catch ( System.Exception ex )
                {
                    HttpContext context2 = HttpContext.Current;
                    ExceptionLogService.LogException( ex, context2 );
                    throw;
                }
            }

            // Run Stored Proc ETL for Attendance BI Tables
            if ( dataMap.GetString( "ProcessAttendanceBIAnalytics" ).AsBoolean() )
            {
                try
                {
                    int rows = DbService.ExecuteCommand( "EXEC [dbo].[spAnalytics_ETL_Attendance]", System.Data.CommandType.Text, null, _commandTimeout );
                    results.AppendLine( "Attendance ETL completed." );

                    context.UpdateLastStatusMessage( results.ToString() );
                }
                catch ( System.Exception ex )
                {
                    HttpContext context2 = HttpContext.Current;
                    ExceptionLogService.LogException( ex, context2 );
                    throw;
                }
            }

            // "Refresh Power BI Account Tokens"
            if ( dataMap.GetString( "RefreshPowerBIAccountTokens" ).AsBoolean() )
            {
                var powerBiAccountsDefinedType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.POWERBI_ACCOUNTS.AsGuid() );
                if ( powerBiAccountsDefinedType?.DefinedValues?.Any() == true )
                {
                    foreach ( var powerBiAccount in powerBiAccountsDefinedType.DefinedValues )
                    {
                        string message;
                        var token = PowerBiUtilities.GetAccessToken( powerBiAccount.Guid, out message );
                        if ( token.IsNullOrWhiteSpace() )
                        {
                            results.AppendLine( $"Unable to refresh Power BI Access token for {powerBiAccount.Value}: {message}." );
                        }
                        else
                        {
                            results.AppendLine( $"Refreshed Power BI Access token for {powerBiAccount.Value}." );
                        }
                    }
                }
            }

            context.Result = results.ToString();
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
                        // use the value that was already populated in the Analytics table since TextField's don't do any special formatting
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
                // use the value that was already populated in the Analytics table since it would be ValueAsDateTime, ValueAsNumeric, etc
                return false;
            }
        }

        /// <summary>
        /// Updates the analytics schema for the model (Family/Group, Person, Campus)
        /// </summary>
        /// <param name="analyticsSourceFields">The analytics source fields.</param>
        /// <param name="modelAnalyticAttributes">The model analytic attributes.</param>
        /// <param name="analyticsTableName">Name of the analytics table.</param>
        /// <param name="modelJobStats">The model job stats.</param>
        private void UpdateAnalyticsSchemaForModel( List<EntityField> analyticsSourceFields, List<AttributeCache> modelAnalyticAttributes, string analyticsTableName, JobStats modelJobStats )
        {
            var dataSet = DbService.GetDataSetSchema( $"SELECT * FROM [{analyticsTableName}] where 1=0", System.Data.CommandType.Text, null );
            var dataTable = dataSet.Tables[0];

            // This is a list of fields that we do not want considered as an attribute field. By default any field not in
            // analyticsSourceFields is considered an attribute which is not correct in some cases (e.g. HideFromReporting).
            var analyticsFieldNames = analyticsSourceFields.Select( a => a.Name ).ToList();
            analyticsFieldNames.Add( "ForeignId" );
            analyticsFieldNames.Add( "ForeignGuid" );
            analyticsFieldNames.Add( "ForeignKey" );
            analyticsFieldNames.Add( "Guid" );
            analyticsFieldNames.Add( "TypeId" );

            var currentDatabaseAttributeFields = dataTable.Columns.OfType<DataColumn>().Where( a =>
                !analyticsFieldNames.Contains( a.ColumnName ) ).ToList();

            const string BooleanSqlFieldType = "bit";
            const string DateTimeSqlFieldType = "datetime";
            const string NumericSqlFieldType = "[decimal](29,4)";
            const string DefaultSqlFieldType = "nvarchar(250)";

            using ( var rockContext = new RockContext() )
            {
                // add any AttributeFields that aren't already fields on Analytics table
                foreach ( var modelAttribute in modelAnalyticAttributes )
                {
                    var columnName = modelAttribute.Key.RemoveSpecialCharacters();
                    var databaseColumn = currentDatabaseAttributeFields.Where( a => a.ColumnName == columnName ).FirstOrDefault();
                    var modelAttributeValueFieldName = modelAttribute.FieldType.Field.AttributeValueFieldName;
                    string sqlFieldType;
                    if ( modelAttributeValueFieldName == "ValueAsDateTime" )
                    {
                        sqlFieldType = DateTimeSqlFieldType;
                    }
                    else if ( modelAttributeValueFieldName == "ValueAsNumeric" )
                    {
                        sqlFieldType = NumericSqlFieldType;
                    }
                    else if ( modelAttributeValueFieldName == "ValueAsBoolean" )
                    {
                        sqlFieldType = BooleanSqlFieldType;
                    }
                    else
                    {
                        sqlFieldType = DefaultSqlFieldType;
                    }

                    string addColumnSQL = $"ALTER TABLE [{analyticsTableName}] ADD [{columnName}] {sqlFieldType} null";

                    string dropColumnSql = $"ALTER TABLE [{analyticsTableName}] DROP COLUMN [{columnName}]";

                    if ( databaseColumn == null )
                    {
                        // doesn't exist as a field on the Analytics table, so create it
                        modelJobStats.ColumnsAdded++;
                        rockContext.Database.ExecuteSqlCommand( addColumnSQL );
                    }
                    else
                    {
                        // it does exist as a field on the Analytics table, but make sure the datatype is correct
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
                            modelJobStats.ColumnsModified++;
                            rockContext.Database.ExecuteSqlCommand( dropColumnSql );
                            rockContext.Database.ExecuteSqlCommand( addColumnSQL );
                        }
                    }
                }

                // remove any attribute fields that aren't attributes on model
                var modelAttributeColumnNames = modelAnalyticAttributes.Select( a => a.Key.RemoveSpecialCharacters() ).ToList();
                foreach ( var databaseAttributeField in currentDatabaseAttributeFields )
                {
                    if ( IsEntityColumn( analyticsTableName, databaseAttributeField.ColumnName ) )
                    {
                        // We don't want to accidently delete an entity column just because it's not reportable or some such thing, that would be bad.
                        ExceptionLogService.LogException( new Exception( $"The ProcessBIAnalytics job tried to delete column {analyticsTableName}.{databaseAttributeField.ColumnName} but was prevented by a check in the job." ) );
                        continue;
                    }

                    if ( !modelAttributeColumnNames.Contains( databaseAttributeField.ColumnName ) )
                    {
                        var dropColumnSql = $"ALTER TABLE [{analyticsTableName}] DROP COLUMN [{databaseAttributeField.ColumnName}]";

                        modelJobStats.ColumnsRemoved++;
                        rockContext.Database.ExecuteSqlCommand( dropColumnSql );
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether provided property name is a property of the provided entityName.
        /// </summary>
        /// <param name="entityName">Name of the entity.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>
        ///   <c>true</c> if [is entity column] [the specified entity name]; otherwise, <c>false</c>.
        ///   Also returns false if the entity does not exist in the Rock.Model namespace.
        /// </returns>
        private bool IsEntityColumn( string entityName, string propertyName )
        {
            var entityType = Type.GetType( $"Rock.Model.{entityName}, Rock" );

            if (entityType == null )
            {
                return false;
            }

            var prop = entityType.GetProperty( propertyName );
            return prop != null ? true : false;
        }

        /// <summary>
        /// Updates the model attribute values (Just for Campus and Family; Person doesn't do it this way)
        /// </summary>
        /// <param name="modelAnalyticAttributes">The model analytic attributes.</param>
        /// <param name="analyticsTableName">Name of the analytics table.</param>
        /// <param name="analyticsTableModelIdColumnName">Name of the analytics table model identifier column.</param>
        /// <param name="modelJobStats">The model job stats.</param>
        /// <param name="hasCurrentRowIndicator">if set to <c>true</c> [has current row indicator].</param>
        private void UpdateModelAttributeValues( List<AttributeCache> modelAnalyticAttributes, string analyticsTableName, string analyticsTableModelIdColumnName, JobStats modelJobStats, bool hasCurrentRowIndicator )
        {
            using ( var rockContext = new RockContext() )
            {
                var attributeValueService = new AttributeValueService( rockContext );

                foreach ( var attribute in modelAnalyticAttributes )
                {
                    var columnName = attribute.Key.RemoveSpecialCharacters();
                    {
                        var attributeValuesQry = attributeValueService.Queryable()
                                .Where( a => a.AttributeId == attribute.Id )
                                .Select( a => a.Value );

                        // get all the unique possible values that are currently being used
                        var modelAttributeValues = attributeValuesQry.Distinct().ToList();
                        foreach ( var modelAttributeValue in modelAttributeValues )
                        {
                            object attributeValue;
                            if ( UseFormatValueForUpdate( attribute ) )
                            {
                                attributeValue = attribute.FieldType.Field.FormatValue( null, modelAttributeValue, attribute.QualifierValues, false );
                            }
                            else
                            {
                                attributeValue = attribute.FieldType.Field.ValueAsFieldType( null, modelAttributeValue, attribute.QualifierValues );
                            }

                            // mass update the value for the Attribute in the Analytics table records 
                            // Note: Only update the *Current Records, even if it was just a Text change.  For example, if they changed DefinedValue "Member" to "Owner", 
                            // have the historical records say "Member" even though it is the same definedvalue id
                            var updateSql = $@"
UPDATE [{analyticsTableName}] 
    SET [{columnName}] = @attributeValue 
    WHERE [{analyticsTableModelIdColumnName}] IN (SELECT [EntityId] FROM [AttributeValue] WHERE [AttributeId] = {attribute.Id} AND [Value] = @modelAttributeValue) 
    AND isnull([{columnName}],'') != @attributeValue
    ";

                            if ( hasCurrentRowIndicator )
                            {
                                updateSql += "AND [CurrentRowIndicator] = 1";
                            }

                            var parameters = new Dictionary<string, object>();
                            parameters.Add( "@modelAttributeValue", modelAttributeValue );
                            parameters.Add( "@attributeValue", attributeValue );

                            modelJobStats.SqlLogs.Add( parameters.Select( a => $"/* {a.Key} = '{a.Value}' */" ).ToList().AsDelimited( "\n" ) + updateSql );
                            modelJobStats.AttributeFieldsUpdated += DbService.ExecuteCommand( updateSql, System.Data.CommandType.Text, parameters, _commandTimeout );
                        }
                    }
                }
            }
        }

        #endregion Shared Methods

        #region Person Analytics

        /// <summary>
        /// Processes the person bi analytics.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="dataMap">The data map.</param>
        private void ProcessPersonBIAnalytics( IJobExecutionContext context, JobDataMap dataMap )
        {
            List<EntityField> analyticsSourcePersonHistoricalFields = EntityHelper.GetEntityFields( typeof( Rock.Model.AnalyticsSourcePersonHistorical ),  false, false );
            EntityField typeIdField = analyticsSourcePersonHistoricalFields.Where( f => f.Name == "TypeId" ).FirstOrDefault();

            if ( typeIdField != null )
            {
                analyticsSourcePersonHistoricalFields.Remove( typeIdField );
            }

            typeIdField = analyticsSourcePersonHistoricalFields.Where( f => f.Name == "ForeignId" ).FirstOrDefault();
            if ( typeIdField != null )
            {
                analyticsSourcePersonHistoricalFields.Remove( typeIdField );
            }

            typeIdField = analyticsSourcePersonHistoricalFields.Where( f => f.Name == "ForeignKey" ).FirstOrDefault();
            if ( typeIdField != null )
            {
                analyticsSourcePersonHistoricalFields.Remove( typeIdField );
            }


            List<AttributeCache> personAnalyticAttributes = EntityHelper.GetEntityFields( typeof( Rock.Model.Person ) )
                .Where( a => a.FieldKind == FieldKind.Attribute && a.AttributeGuid.HasValue )
                .Select( a => AttributeCache.Get( a.AttributeGuid.Value ) )
                .Where( a => a != null )
                .Where( a => a.IsAnalytic )
                .ToList();

            try
            {
                // Ensure that the Schema of AnalyticsSourcePersonHistorical matches the current fields for Attributes that are marked as IsAnalytic
                UpdateAnalyticsSchemaForModel( analyticsSourcePersonHistoricalFields, personAnalyticAttributes, "AnalyticsSourcePersonHistorical", _personJobStats );

                // refresh the view definitions just in case the schema changed
                // NOTE: Order is important!
                using ( var rockContext = new RockContext() )
                {
                    rockContext.Database.ExecuteSqlCommand( "exec sp_refreshview [AnalyticsDimPersonHistorical]" );
                    rockContext.Database.ExecuteSqlCommand( "exec sp_refreshview [AnalyticsDimPersonCurrent]" );
                    rockContext.Database.ExecuteSqlCommand( "exec sp_refreshview [AnalyticsDimFamilyHeadOfHousehold]" );
                }

                // start the update process by marking records as History if any of the "IsAnalyticHistory" values 
                // have changed for attributes that have to use FormatValue to get the value instead of directly in the DB
                MarkPersonAsHistoryUsingFormattedValue( personAnalyticAttributes );

                // do the big ETL for stuff that can be done directly in the DB
                DoPersonMainPopulateETLs( analyticsSourcePersonHistoricalFields, personAnalyticAttributes );

                // finish up by updating Attribute Values in the Analytic tables for attributes 
                // that have to use FormatValue to get the value instead of directly in the DB
                UpdatePersonAttributeValueUsingFormattedValue( personAnalyticAttributes );
            }
            finally
            {
                if ( dataMap.GetString( "SaveSQLForDebug" ).AsBoolean() )
                {
                    LogSQL( "ProcessAnalyticsDimPerson.sql", _personJobStats.SqlLogs.AsDelimited( "\n" ).ToString() );
                }
            }
        }

        /// <summary>
        /// Marks Person Analytic rows as history if the formatted value of the attribute has changed
        /// </summary>
        /// <param name="personAnalyticAttributes">The person analytic attributes.</param>
        private void MarkPersonAsHistoryUsingFormattedValue( List<AttributeCache> personAnalyticAttributes )
        {
            List<SqlCommand> markAsHistoryUsingFormattedValueScripts = new List<SqlCommand>();

            // Compare "IsAnalyticHistory" attribute values to see if they have changed since the last ETL, using the FormatValue function
            using ( var rockContext = new RockContext() )
            {
                var attributeValueService = new AttributeValueService( rockContext );

                foreach ( var attribute in personAnalyticAttributes.Where( a => a.IsAnalyticHistory && UseFormatValueForUpdate( a ) ) )
                {
                    var columnName = attribute.Key.RemoveSpecialCharacters();

                    if ( UseFormatValueForUpdate( attribute ) )
                    {
                        var attributeValuesQry = attributeValueService.Queryable()
                                .Where( a => a.AttributeId == attribute.Id )
                                .Select( a => a.Value );

                        // get all the unique possible values that are currently being used
                        var personAttributeValues = attributeValuesQry.Distinct().ToList();
                        foreach ( var personAttributeValue in personAttributeValues )
                        {
                            var formattedValue = attribute.FieldType.Field.FormatValue( null, personAttributeValue, attribute.QualifierValues, false );

                            // mass update any AnalyticsSourcePersonHistorical records that need to be marked as History for this Attribute's Value
                            var markAsHistorySQL = $@"
DECLARE 
    @EtlDate DATE = convert( DATE, SysDateTime() )

UPDATE [AnalyticsSourcePersonHistorical] 
    SET [CurrentRowIndicator] = 0, [ExpireDate] = @EtlDate 
    WHERE [PersonId] IN (SELECT EntityId FROM AttributeValue WHERE AttributeId = {attribute.Id} AND Value = @personAttributeValue ) 
    AND isnull([{columnName}],'') != @formattedValue AND [CurrentRowIndicator] = 1
    AND PersonId NOT IN( --Ensure that there isn't already a History Record for the current EtlDate 
        SELECT PersonId
        FROM AnalyticsSourcePersonHistorical x
        WHERE CurrentRowIndicator = 0
        AND[ExpireDate] = @EtlDate
    )";

                            var parameters = new Dictionary<string, object>();
                            parameters.Add( "@personAttributeValue", personAttributeValue );
                            parameters.Add( "@formattedValue", formattedValue );

                            this._personJobStats.SqlLogs.Add( parameters.Select( a => $"/* {a.Key} = '{a.Value}' */" ).ToList().AsDelimited( "\n" ) + markAsHistorySQL );

                            this._personJobStats.RowsMarkedAsHistory += DbService.ExecuteCommand( markAsHistorySQL, System.Data.CommandType.Text, parameters, _commandTimeout );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the Person update attribute value using formatted value scripts.
        /// </summary>
        /// <param name="personAnalyticAttributes">The person analytic attributes.</param>
        /// <returns></returns>
        private void UpdatePersonAttributeValueUsingFormattedValue( List<AttributeCache> personAnalyticAttributes )
        {
            // Update Attributes using GetFormattedValue...
            using ( var rockContext = new RockContext() )
            {
                var attributeValueService = new Rock.Model.AttributeValueService( rockContext );

                foreach ( var attribute in personAnalyticAttributes.Where( a => UseFormatValueForUpdate( a ) ) )
                {
                    var columnName = attribute.Key.RemoveSpecialCharacters();

                    if ( UseFormatValueForUpdate( attribute ) )
                    {
                        var attributeValuesQry = attributeValueService.Queryable()
                                .Where( a => a.AttributeId == attribute.Id )
                                .Select( a => a.Value );

                        // get all the unique possible values that are currently being used
                        var personAttributeValues = attributeValuesQry.Distinct().ToList();
                        foreach ( var personAttributeValue in personAttributeValues )
                        {
                            var formattedValue = attribute.FieldType.Field.FormatValue( null, personAttributeValue, attribute.QualifierValues, false );

                            // mass update the value for the Attribute in the AnalyticsSourcePersonHistorical records 
                            // Don't update the Historical Records, even if it was just a Text change.  For example, if they changed DefinedValue "Member" to "Owner", 
                            // have the historical records say "Member" even though it is the same definedvalue id
                            var updateSql = $@"
UPDATE [AnalyticsSourcePersonHistorical] 
    SET [{columnName}] = @formattedValue 
    WHERE [PersonId] IN (SELECT EntityId FROM AttributeValue WHERE AttributeId = {attribute.Id} AND Value = @personAttributeValue) 
    AND isnull([{columnName}],'') != @formattedValue
    AND [CurrentRowIndicator] = 1";

                            var parameters = new Dictionary<string, object>();
                            parameters.Add( "@personAttributeValue", personAttributeValue );
                            parameters.Add( "@formattedValue", formattedValue );

                            _personJobStats.SqlLogs.Add( parameters.Select( a => $"/* {a.Key} = '{a.Value}' */" ).ToList().AsDelimited( "\n" ) + updateSql );

                            _personJobStats.AttributeFieldsUpdated += DbService.ExecuteCommand( updateSql, System.Data.CommandType.Text, parameters, _commandTimeout );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Does the main Person related ETLs for stuff that can be done in the database
        /// </summary>
        /// <param name="analyticsSourcePersonHistoricalFields">The analytics source person historical fields.</param>
        /// <param name="personAnalyticAttributes">The person analytic attributes.</param>
        private void DoPersonMainPopulateETLs( List<EntityField> analyticsSourcePersonHistoricalFields, List<AttributeCache> personAnalyticAttributes )
        {
            // columns that should be considered when determining if a new History record is needed
            List<ColumnInfo> historyColumns = new List<ColumnInfo>();
            foreach ( var analyticsSourcePersonHistoricalProperty in analyticsSourcePersonHistoricalFields.Where( a => a.PropertyInfo.GetCustomAttribute<AnalyticHistoryFieldAttribute>() != null ) )
            {
                historyColumns.Add( new ColumnInfo( analyticsSourcePersonHistoricalProperty ) );
            }

            List<string> populateAttributeValueINSERTClauses = new List<string>();
            List<ColumnInfo> attributeValueColumns = new List<ColumnInfo>();
            List<string> populateAttributeValueSELECTClauses = new List<string>();
            List<string> populatePersonValueSELECTClauses = new List<string>();
            List<ColumnInfo> populatePersonValueSELECTColumns = new List<ColumnInfo>();
            List<string> populateAttributeValueFROMClauses = new List<string>();

            var analyticSpecificColumns = new string[] { "Id", "PersonId", "CurrentRowIndicator", "EffectiveDate", "ExpireDate", "PrimaryFamilyId", "BirthDateKey", "Age", "Guid", "Count" };

            foreach ( var item in analyticsSourcePersonHistoricalFields
                .Where( a => !analyticSpecificColumns.Contains( a.Name ) ).OrderBy( a => a.Name ).ToList() )
            {
                populatePersonValueSELECTClauses.Add( item.Name );
                populatePersonValueSELECTColumns.Add( new ColumnInfo( item ) );
            }

            List<string> populatePersonValueFROMClauses = new List<string>( populatePersonValueSELECTClauses );

            const int maxAttributeValueLength = 250;

            using ( var rockContext = new RockContext() )
            {
                // add any AttributeFields that aren't already fields on AnalyticsSourcePersonHistorical
                foreach ( var personAttribute in personAnalyticAttributes.Where( a => !UseFormatValueForUpdate( a ) ) )
                {
                    var columnName = personAttribute.Key.RemoveSpecialCharacters();
                    var personAttributeValueFieldName = personAttribute.FieldType.Field.AttributeValueFieldName;

                    // each SELECT clause should look something like: attribute_1071.ValueAsDateTime as [attribute_YouthVolunteerApplication]
                    string populateAttributeValueSELECTClause =
                        $"av{personAttribute.Id}.{personAttributeValueFieldName} as [{columnName}]";

                    populateAttributeValueSELECTClauses.Add( populateAttributeValueSELECTClause );

                    populateAttributeValueINSERTClauses.Add( columnName );
                    var columnInfo = new ColumnInfo();
                    columnInfo.ColumnName = columnName;
                    switch ( personAttributeValueFieldName )
                    {
                        case "ValueAsNumeric":
                            columnInfo.IsNullDefaultValue = "0";
                            break;
                        case "ValueAsDateTime":
                            columnInfo.IsNullDefaultValue = "DateFromParts( 9999, 1, 1 )";
                            break;
                        case "ValueAsBoolean":
                            columnInfo.IsNullDefaultValue = "0";
                            break;
                        default:
                            columnInfo.IsNullDefaultValue = "''";
                            break;
                    }

                    attributeValueColumns.Add( columnInfo );
                    if ( personAttribute.IsAnalyticHistory )
                    {
                        historyColumns.Add( columnInfo );
                    }

                    string lengthCondition = personAttributeValueFieldName == "Value"
                        ? $"AND len(av{personAttribute.Id}.Value) <= {maxAttributeValueLength}"
                        : null;

                    string populateAttributeValueFROMClause =
                        $"LEFT OUTER JOIN AttributeValue av{personAttribute.Id} ON av{personAttribute.Id}.EntityId = p.Id AND av{personAttribute.Id}.AttributeId = {personAttribute.Id} {lengthCondition}";

                    populateAttributeValueFROMClauses.Add( populateAttributeValueFROMClause );
                }

                string selectSQL = GetPersonSelectSQLScript( populateAttributeValueSELECTClauses, populateAttributeValueFROMClauses, populatePersonValueFROMClauses );

                string processINSERTScript = GetPersonProcessINSERTScript( populateAttributeValueINSERTClauses, populatePersonValueSELECTClauses, selectSQL );

                // build the CTE which is used for both the "Mark as History" and "UPDATE" scripts
                string withCTEScript = @"

;with cte1 as (" + selectSQL + @")
";
                string markAsHistoryScript = GetPersonMarkAsHistoryScript( historyColumns, withCTEScript );
                string updateETLScript = GetPersonUpdateETLScript( attributeValueColumns, populatePersonValueSELECTColumns, withCTEScript );

                string scriptDeclares = @"
DECLARE 
    @EtlDate DATE = convert( DATE, SysDateTime() )
    , @MaxExpireDate DATE = DateFromParts( 9999, 1, 1 )";

                // throw script into logs in case 'Save SQL for Debug' is enabled
                _personJobStats.SqlLogs.Add( "/* MarkAsHistoryScript */\n" + scriptDeclares + markAsHistoryScript );
                _personJobStats.SqlLogs.Add( "/* UpdateETLScript */\n" + scriptDeclares + updateETLScript );
                _personJobStats.SqlLogs.Add( "/* ProcessINSERTScript */\n" + scriptDeclares + processINSERTScript );

                // Move Records To History that have changes in any of fields that trigger history
                _personJobStats.RowsMarkedAsHistory += DbService.ExecuteCommand( scriptDeclares + markAsHistoryScript, CommandType.Text, null, _commandTimeout );

                // Update existing records that have CurrentRowIndicator=1 to match what is in the live tables
                _personJobStats.RowsUpdated += DbService.ExecuteCommand( scriptDeclares + updateETLScript, CommandType.Text, null, _commandTimeout );

                // Insert new Person Records that aren't in there yet
                _personJobStats.RowsInserted += DbService.ExecuteCommand( scriptDeclares + processINSERTScript, CommandType.Text, null, _commandTimeout );
            }
        }

        /// <summary>
        /// Gets the process insert script for Person analytics tables
        /// </summary>
        /// <param name="populateAttributeValueINSERTClauses">The populate attribute value insert clauses.</param>
        /// <param name="populatePersonValueSELECTClauses">The populate person value select clauses.</param>
        /// <param name="selectSQL">The select SQL.</param>
        /// <returns></returns>
        private static string GetPersonProcessINSERTScript( List<string> populateAttributeValueINSERTClauses, List<string> populatePersonValueSELECTClauses, string selectSQL )
        {
            string processINSERTScript = @"
INSERT INTO [dbo].[AnalyticsSourcePersonHistorical] (
        [PersonId],
        [CurrentRowIndicator],
        [EffectiveDate],
        [ExpireDate],
        [PrimaryFamilyId],
        [BirthDateKey],
        [Age],
        [Count],
" + populatePersonValueSELECTClauses.Select( a => $"        [{a}]" ).ToList().AsDelimited( ",\n" ) + @",
        [Guid]";

            if ( populateAttributeValueINSERTClauses.Any() )
            {
                // only need a comma when we have more INSERT clauses to add
                processINSERTScript += ",\n";
            }
            else
            {
                processINSERTScript += "\n";
            }

            // add INSERT columns for the AttributeValue Fields
            processINSERTScript += populateAttributeValueINSERTClauses.Select( a => $"        [{a}]" ).ToList().AsDelimited( ",\n" );

            processINSERTScript += @"
)";
            processINSERTScript += selectSQL;

            processINSERTScript += @"
WHERE p.Id NOT IN (
            SELECT PersonId
            FROM [AnalyticsSourcePersonHistorical]
            WHERE CurrentRowIndicator = 1
            )";
            return processINSERTScript;
        }

        /// <summary>
        /// Gets the select SQL script for Person Analytic tables
        /// </summary>
        /// <param name="populateAttributeValueSELECTClauses">The populate attribute value select clauses.</param>
        /// <param name="populateAttributeValueFROMClauses">The populate attribute value from clauses.</param>
        /// <param name="populatePersonValueFROMClauses">The populate person value from clauses.</param>
        /// <returns></returns>
        private string GetPersonSelectSQLScript( List<string> populateAttributeValueSELECTClauses, List<string> populateAttributeValueFROMClauses, List<string> populatePersonValueFROMClauses )
        {
            // the date that the ETL ran
            DateTime etlDate = RockDateTime.Today;

            string selectSQL = @"
    SELECT 
        p.Id [PersonId],
        1 [CurrentRowIndicator],
        @EtlDate [EffectiveDate],
        @MaxExpireDate [ExpireDate],
        family.GroupId [PrimaryFamilyId],
        convert(INT, (convert(CHAR(8), BirthDate, 112))) [BirthDateKey],
        dbo.ufnCrm_GetAge(p.BirthDate) [Age], 
        1 [Count],
" + populatePersonValueFROMClauses.Select( a => $"        [{a}]" ).ToList().AsDelimited( ",\n" ) + @",
        NEWID() [Guid]";

            if ( populateAttributeValueSELECTClauses.Any() )
            {
                // only need a comma when we have more SELECT clauses to add
                selectSQL += ",\n";
            }
            else
            {
                selectSQL += "\n";
            }

            selectSQL += populateAttributeValueSELECTClauses.Select( a => "        " + a ).ToList().AsDelimited( ",\n" );

            selectSQL += @"
FROM dbo.Person p
OUTER APPLY (
        SELECT top 1 gm.GroupId [GroupId]
        FROM [GroupMember] gm
        JOIN [Group] g ON gm.GroupId = g.Id
        WHERE g.GroupTypeId = 10
            AND gm.PersonId = p.Id
			order by g.IsActive desc, g.Id desc
        ) family
";

            // add the "LEFT OUTER JOIN..." AttributeValue FROM clauses
            selectSQL += populateAttributeValueFROMClauses.Select( a => "        " + a ).ToList().AsDelimited( "\n" );
            return selectSQL;
        }

        /// <summary>
        /// Gets the mark as history script for Person Analytic Tables
        /// </summary>
        /// <param name="historyColumns">The history columns.</param>
        /// <param name="withCTEScript">The with cte script.</param>
        /// <returns></returns>
        private string GetPersonMarkAsHistoryScript( List<ColumnInfo> historyColumns, string withCTEScript )
        {
            string markAsHistoryScript = withCTEScript;
            markAsHistoryScript += @"UPDATE AnalyticsSourcePersonHistorical SET
        CurrentRowIndicator = 0,
        [ExpireDate] = @EtlDate
FROM AnalyticsSourcePersonHistorical asph
JOIN cte1 ON cte1.PersonId = asph.PersonId
WHERE asph.CurrentRowIndicator = 1 and (" + historyColumns.Select( a => $" isnull(asph.[{a.ColumnName}], {a.IsNullDefaultValue}) != isnull(cte1.[{a.ColumnName}],{a.IsNullDefaultValue})" ).ToList().AsDelimited( " OR \n" ) + @")
AND asph.PersonId NOT IN ( -- Ensure that there isn't already a History Record for the current EtlDate 
    SELECT PersonId
    FROM AnalyticsSourcePersonHistorical x
    WHERE CurrentRowIndicator = 0
        AND [ExpireDate] = @EtlDate
    )
";
            return markAsHistoryScript;
        }

        /// <summary>
        /// Gets the update etl script for Person Analytic Tables
        /// </summary>
        /// <param name="attributeValueColumns">The attribute value columns.</param>
        /// <param name="populatePersonValueSELECTColumns">The populate person value select columns.</param>
        /// <param name="withCTEScript">The with cte script.</param>
        /// <returns></returns>
        private string GetPersonUpdateETLScript( List<ColumnInfo> attributeValueColumns, List<ColumnInfo> populatePersonValueSELECTColumns, string withCTEScript )
        {
            string updateETLScript = withCTEScript;

            updateETLScript += @"UPDATE AnalyticsSourcePersonHistorical SET 
";
            updateETLScript += @"
        [PrimaryFamilyId] = cte1.[PrimaryFamilyId],
        [BirthDateKey] = cte1.[BirthDateKey],
        [Age] = cte1.[Age],
";
            updateETLScript += populatePersonValueSELECTColumns.Select( a => $"        [{a.ColumnName}] = cte1.[{a.ColumnName}]" ).ToList().AsDelimited( ",\n" );
            if ( attributeValueColumns.Any() )
            {
                updateETLScript += ",\n";
                updateETLScript += attributeValueColumns.Select( a => $"        [{a.ColumnName}] = cte1.[{a.ColumnName}]" ).ToList().AsDelimited( ",\n" );
            }

            updateETLScript += @"
FROM AnalyticsSourcePersonHistorical asph
JOIN cte1 ON cte1.PersonId = asph.PersonId
WHERE asph.CurrentRowIndicator = 1 AND (";

            updateETLScript += populatePersonValueSELECTColumns.Select( a => $"        isnull(asph.[{a.ColumnName}],{a.IsNullDefaultValue}) != isnull(cte1.[{a.ColumnName}],{a.IsNullDefaultValue})" ).ToList().AsDelimited( " OR \n" );
            updateETLScript += " OR \n        isnull(asph.[Age],-1) != isnull(cte1.[Age],-1)";
            if ( attributeValueColumns.Any() )
            {
                updateETLScript += " OR \n";
                updateETLScript += attributeValueColumns.Select( a => $"        isnull(asph.[{a.ColumnName}],{a.IsNullDefaultValue}) != isnull(cte1.[{a.ColumnName}],{a.IsNullDefaultValue})" ).ToList().AsDelimited( " OR \n" );
            }

            updateETLScript += ")";

            return updateETLScript;
        }

        #endregion

        #region Family Analytics

        /// <summary>
        /// Processes the family bi analytics.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="dataMap">The data map.</param>
        private void ProcessFamilyBIAnalytics( IJobExecutionContext context, JobDataMap dataMap )
        {
            List<EntityField> analyticsSourceFamilyHistoricalFields = EntityHelper.GetEntityFields( typeof( Rock.Model.AnalyticsSourceFamilyHistorical ), false, false );
            int groupTypeIdFamily = GroupTypeCache.GetFamilyGroupType().Id;
            string groupTypeIdFamilyQualifier = groupTypeIdFamily.ToString();

            List<AttributeCache> familyAnalyticAttributes = EntityHelper.GetEntityFields( typeof( Rock.Model.Group ), false, false )
                .Where( a => a.FieldKind == FieldKind.Attribute && a.AttributeGuid.HasValue )
                .Select( a => AttributeCache.Get( a.AttributeGuid.Value ) )
                .Where( a => a != null )
                .Where( a => a.EntityTypeQualifierColumn == "GroupTypeId" && a.EntityTypeQualifierValue == groupTypeIdFamilyQualifier )
                .Where( a => a.IsAnalytic )
                .ToList();

            try
            {
                // Ensure that the Schema of AnalyticsSourceFamilyHistorical matches the current fields for Attributes that are marked as IsAnalytic
                UpdateAnalyticsSchemaForModel( analyticsSourceFamilyHistoricalFields, familyAnalyticAttributes, "AnalyticsSourceFamilyHistorical", _familyJobStats );

                using ( var rockContext = new RockContext() )
                {
                    // refresh the view definitions just in case the schema changed
                    // NOTE: Order is important!
                    rockContext.Database.ExecuteSqlCommand( "exec sp_refreshview [AnalyticsDimFamilyHistorical]" );
                    rockContext.Database.ExecuteSqlCommand( "exec sp_refreshview [AnalyticsDimFamilyCurrent]" );
                }

                // start the update process by marking records as History if any of the "IsAnalyticHistory" values 
                // have changed for attributes 
                MarkFamilyAsHistoryUsingAttributeValues( familyAnalyticAttributes );

                // run the main spAnalytics_ETL_Family stored proc to take care of all the non-attribute related data
                var etlResult = DbService.GetDataTable( "EXEC [dbo].[spAnalytics_ETL_Family]", CommandType.Text, null, _commandTimeout );
                if ( etlResult.Rows.Count == 1 )
                {
                    _familyJobStats.RowsInserted = etlResult.Rows[0]["RowsInserted"] as int? ?? 0;
                    _familyJobStats.RowsUpdated = etlResult.Rows[0]["RowsUpdated"] as int? ?? 0;
                }

                // finish up by updating Attribute Values in the Analytic tables for attributes 
                UpdateModelAttributeValues( familyAnalyticAttributes, "AnalyticsSourceFamilyHistorical", "FamilyId", _familyJobStats, true );
            }
            finally
            {
                if ( dataMap.GetString( "SaveSQLForDebug" ).AsBoolean() )
                {
                    LogSQL( "ProcessAnalyticsDimFamily.sql", _familyJobStats.SqlLogs.AsDelimited( "\n" ).ToString() );
                }
            }
        }

        /// <summary>
        /// Marks Family Analytic rows as history if the value of the attribute has changed
        /// </summary>
        /// <param name="familyAnalyticAttributes">The family analytic attributes.</param>
        private void MarkFamilyAsHistoryUsingAttributeValues( List<AttributeCache> familyAnalyticAttributes )
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
                        this._familyJobStats.SqlLogs.Add( parameters.Select( a => $"/* {a.Key} = '{a.Value}' */" ).ToList().AsDelimited( "\n" ) + markAsHistorySQL );

                        this._familyJobStats.RowsMarkedAsHistory += DbService.ExecuteCommand( markAsHistorySQL, System.Data.CommandType.Text, parameters, _commandTimeout );
                    }
                }
            }
        }

        #endregion Family Analytics

        #region Campus Analytics

        /// <summary>
        /// Processes the campus bi analytics.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="dataMap">The data map.</param>
        private void ProcessCampusBIAnalytics( IJobExecutionContext context, JobDataMap dataMap )
        {
            List<EntityField> analyticsSourceCampusFields = EntityHelper.GetEntityFields( typeof( Rock.Model.AnalyticsSourceCampus ), false, false );

            List<AttributeCache> campusAnalyticAttributes = EntityHelper.GetEntityFields( typeof( Rock.Model.Campus ), false, false )
                .Where( a => a.FieldKind == FieldKind.Attribute && a.AttributeGuid.HasValue )
                .Select( a => AttributeCache.Get( a.AttributeGuid.Value ) )
                .Where( a => a != null )
                .Where( a => a.IsAnalytic )
                .ToList();

            try
            {
                // Ensure that the Schema of AnalyticsSourceCampus matches the current fields for Attributes that are marked as IsAnalytic
                UpdateAnalyticsSchemaForModel( analyticsSourceCampusFields, campusAnalyticAttributes, "AnalyticsSourceCampus", _campusJobStats );

                // run the main spAnalytics_ETL_Campus stored proc to take care of all the non-attribute related data
                var etlResult = DbService.GetDataTable( "EXEC [dbo].[spAnalytics_ETL_Campus]", CommandType.Text, null, _commandTimeout );
                if ( etlResult.Rows.Count == 1 )
                {
                    _campusJobStats.RowsInserted = etlResult.Rows[0]["RowsInserted"] as int? ?? 0;
                    _campusJobStats.RowsUpdated = etlResult.Rows[0]["RowsUpdated"] as int? ?? 0;
                }

                // finish up by updating Attribute Values in the Analytic tables for attributes 
                UpdateModelAttributeValues( campusAnalyticAttributes, "AnalyticsSourceCampus", "CampusId", _campusJobStats, false );
            }
            finally
            {
                if ( dataMap.GetString( "SaveSQLForDebug" ).AsBoolean() )
                {
                    LogSQL( "ProcessAnalyticsDimCampus.sql", _campusJobStats.SqlLogs.AsDelimited( "\n" ).ToString() );
                }
            }
        }

        #endregion Campus Analytics

        #region Private Classes

        /// <summary>
        /// 
        /// </summary>
        private class ColumnInfo
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ColumnInfo"/> class.
            /// </summary>
            public ColumnInfo()
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ColumnInfo"/> class.
            /// </summary>
            /// <param name="entityField">The entity field.</param>
            public ColumnInfo( EntityField entityField )
            {
                this.ColumnName = entityField.Name;

                if ( entityField.PropertyType == typeof( int? ) )
                {
                    this.IsNullDefaultValue = "0";
                }
                else if ( entityField.PropertyType == typeof( bool? ) )
                {
                    this.IsNullDefaultValue = "0";
                }
                else if ( entityField.PropertyType.IsEnum )
                {
                    this.IsNullDefaultValue = "0";
                }
                else if ( entityField.PropertyType == typeof( DateTime? ) )
                {
                    this.IsNullDefaultValue = "DateFromParts( 9999, 1, 1 )";
                }
                else
                {
                    this.IsNullDefaultValue = "''";
                }
            }

            /// <summary>
            /// Gets or sets the name of the column.
            /// </summary>
            /// <value>
            /// The name of the column.
            /// </value>
            public string ColumnName { get; set; }

            /// <summary>
            /// Gets or sets the is null default value.
            /// </summary>
            /// <value>
            /// The is null default value.
            /// </value>
            public string IsNullDefaultValue { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        private class JobStats
        {
            /// <summary>
            /// Gets or sets the columns added.
            /// </summary>
            /// <value>
            /// The columns added.
            /// </value>
            public int ColumnsAdded { get; set; }

            /// <summary>
            /// Gets or sets the columns modified.
            /// </summary>
            /// <value>
            /// The columns modified.
            /// </value>
            public int ColumnsModified { get; set; }

            /// <summary>
            /// Gets or sets the columns removed.
            /// </summary>
            /// <value>
            /// The columns removed.
            /// </value>
            public int ColumnsRemoved { get; set; }

            /// <summary>
            /// Gets a value indicating whether [schema was modified].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [schema was modified]; otherwise, <c>false</c>.
            /// </value>
            public bool SchemaWasModified
            {
                get
                {
                    return ColumnsAdded > 0 || ColumnsModified > 0 || ColumnsRemoved > 0;
                }
            }


            /// <summary>
            /// Gets or sets the rows marked as history.
            /// </summary>
            /// <value>
            /// The rows marked as history.
            /// </value>
            public int RowsMarkedAsHistory { get; set; }

            /// <summary>
            /// Gets or sets the rows updated.
            /// </summary>
            /// <value>
            /// The rows updated.
            /// </value>
            public int RowsUpdated { get; set; }

            /// <summary>
            /// Gets or sets the rows inserted.
            /// </summary>
            /// <value>
            /// The rows inserted.
            /// </value>
            public int RowsInserted { get; set; }

            /// <summary>
            /// Gets a value indicating whether [row data was modified].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [row data was modified]; otherwise, <c>false</c>.
            /// </value>
            public bool RowDataWasModified
            {
                get
                {
                    return RowsMarkedAsHistory > 0 || RowsUpdated > 0 || RowsInserted > 0 || AttributeFieldsUpdated > 0;
                }
            }

            /// <summary>
            /// Gets or sets the attribute fields updated.
            /// </summary>
            /// <value>
            /// The attribute fields updated.
            /// </value>
            public int AttributeFieldsUpdated { get; set; }

            /// <summary>
            /// The SQL logs
            /// </summary>
            public List<string> SqlLogs = new List<string>();

            /// <summary>
            /// Gets the summary message.
            /// </summary>
            /// <value>
            /// The summary message.
            /// </value>
            public string SummaryMessage
            {
                get
                {
                    StringBuilder summaryMessage = new StringBuilder();
                    if ( SchemaWasModified )
                    {
                        summaryMessage.AppendLine( $"-- Added {this.ColumnsAdded}, modified {this.ColumnsModified}, and removed {this.ColumnsRemoved} attribute columns." );
                    }

                    if ( RowDataWasModified )
                    {
                        summaryMessage.Append( $"--" );
                        if ( this.RowsMarkedAsHistory > 0)
                        {
                            summaryMessage.Append( $" Marked {this.RowsMarkedAsHistory} records as History;" ); 
                        }

                        if ( this.RowsUpdated > 0 )
                        {
                            summaryMessage.Append( $" Updated {this.RowsUpdated} records;" );
                        }

                        if ( this.AttributeFieldsUpdated > 0 )
                        {
                            summaryMessage.Append( $" Updated {this.AttributeFieldsUpdated} attribute values;" );
                        }

                        if ( this.RowsInserted > 0 )
                        {
                            summaryMessage.Append( $" Inserted {this.RowsInserted} records;" );
                        }

                        summaryMessage.AppendLine();
                    }

                    if ( summaryMessage.Length == 0 )
                    {
                        return "-- No Changes";
                    }
                    else
                    {
                        return summaryMessage.ToString();
                    }
                }
            }
        }

        #endregion
    }
}
