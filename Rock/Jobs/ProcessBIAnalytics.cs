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
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;

using Rock.Attribute;
using Rock.Data;
using Rock.Field;
using Rock.Financial;
using Rock.Logging;
using Rock.Model;
using Rock.Reporting;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// </summary>
    [DisplayName( "Process BI Analytics" )]
    [Description( "Job to take care of schema changes ( dynamic Attribute Value Fields ) and data updates to the BI related analytic tables." )]

    [BooleanField(
        "Process Person BI Analytics",
        Key = AttributeKey.ProcessPersonBIAnalytics,
        Description = "Do the BI Analytics tasks related to the Person Analytics tables.",
        DefaultBooleanValue = true,
        Category = "",
        Order = 1 )]

    [BooleanField(
        "Process Family BI Analytics",
        Key = AttributeKey.ProcessFamilyBIAnalytics,
        Description = "Do the BI Analytics tasks related to the Family Analytics tables.",
        DefaultBooleanValue = true,
        Category = "",
        Order = 2 )]

    [BooleanField(
        "Process Campus BI Analytics",
        Key = AttributeKey.ProcessCampusBIAnalytics,
        Description = "Do the BI Analytics tasks related to the Campus table.",
        DefaultBooleanValue = true,
        Category = "",
        Order = 3 )]

    [BooleanField(
        "Process Financial Transaction BI Analytics",
        Key = AttributeKey.ProcessFinancialTransactionBIAnalytics,
        Description = "Do the BI Analytics tasks related to the Financial Transaction Analytics tables.",
        DefaultBooleanValue = true,
        Category = "",
        Order = 4 )]

    [BooleanField(
        "Process Attendance BI Analytics",
        Key = AttributeKey.ProcessAttendanceBIAnalytics,
        Description = "Do the BI Analytics tasks related to the Attendance Analytics tables",
        DefaultBooleanValue = true,
        Category = "",
        Order = 5 )]

    [BooleanField(
        "Refresh Power BI Account Tokens",
        Key = AttributeKey.RefreshPowerBIAccountTokens,
        Description = "Refresh any Power BI Account Tokens to prevent them from expiring.",
        DefaultBooleanValue = true,
        Category = "",
        Order = 6 )]

    [BooleanField(
        "Process Giving Unit BI Analytics",
        Key = AttributeKey.ProcessGivingUnitBIAnalytics,
        Description = "Do the BI Analytics tasks related to Giving Units",
        DefaultBooleanValue = true,
        Category = "",
        Order = 7 )]

    [IntegerField(
        "Command Timeout",
        Key = AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. Leave blank to use the default for this job (3600 seconds). Note that some of the tasks might take a while on larger databases, so you might need to set it higher.",
        IsRequired = false,
        DefaultIntegerValue = 60 * 60,
        Category = "General",
        Order = 8 )]

    [BooleanField(
        "Save SQL for Debug",
        Key = AttributeKey.SaveSQLForDebug,
        Description = "Save the SQL that is used in the Person and Family related parts of the this job to App_Data\\Logs",
        DefaultBooleanValue = false,
        Category = "Advanced",
        Order = 9 )]
    public class ProcessBIAnalytics : RockJob
    {
        #region Attribute Keys

        /// <summary>
        /// Attribute Keys
        /// </summary>
        internal static class AttributeKey
        {
            public const string ProcessPersonBIAnalytics = "ProcessPersonBIAnalytics";
            public const string ProcessFamilyBIAnalytics = "ProcessFamilyBIAnalytics";
            public const string ProcessCampusBIAnalytics = "ProcessCampusBIAnalytics";
            public const string ProcessFinancialTransactionBIAnalytics = "ProcessFinancialTransactionBIAnalytics";
            public const string ProcessAttendanceBIAnalytics = "ProcessAttendanceBIAnalytics";
            public const string RefreshPowerBIAccountTokens = "RefreshPowerBIAccountTokens";
            public const string CommandTimeout = "CommandTimeout";
            public const string SaveSQLForDebug = "SaveSQLForDebug";
            public const string ProcessGivingUnitBIAnalytics = "ProcessGivingUnitBIAnalytics";
        }

        #endregion Attribute Keys

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

        #region Settings

        /// <summary>
        /// Gets or sets the effective date on which the processing is deemed to have occurred.
        /// If not specified, the current Rock application date is used.
        /// </summary>
        public DateTime? EffectiveProcessingDate { get; set; }

        #endregion
        #region Private Fields

        private const int _maxAttributeValueLength = 250;

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

        /// <summary>
        /// The giving unit job stats
        /// </summary>
        private JobStats _givingUnitJobStats = new JobStats();

        private int? _commandTimeout = null;

        #endregion Private Fields

        #region Shared Methods

        /// <inheritdoc cref="RockJob.Execute()" />
        public override void Execute()
        {
            // get the configured timeout, or default to 20 minutes if it is blank
            _commandTimeout = GetAttributeValue( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 1200;

            var processingDate = this.EffectiveProcessingDate ?? RockDateTime.Today;

            StringBuilder results = new StringBuilder();

            // Do the stuff for Person related BI Tables
            if ( GetAttributeValue( AttributeKey.ProcessPersonBIAnalytics ).AsBoolean() )
            {
                ProcessPersonBIAnalytics( processingDate );

                results.AppendLine( "Person BI Results:" );
                results.AppendLine( _personJobStats.SummaryMessage );

                this.UpdateLastStatusMessage( results.ToString() );
            }

            // Do the stuff for Family related BI Tables
            if ( GetAttributeValue( AttributeKey.ProcessFamilyBIAnalytics ).AsBoolean() )
            {
                ProcessFamilyBIAnalytics( processingDate );

                results.AppendLine( "Family BI Results:" );
                results.AppendLine( _familyJobStats.SummaryMessage );

                this.UpdateLastStatusMessage( results.ToString() );
            }

            // Do the stuff for Campus related BI Tables
            if ( GetAttributeValue( AttributeKey.ProcessCampusBIAnalytics ).AsBoolean() )
            {
                ProcessCampusBIAnalytics();

                results.AppendLine( "Campus BI Results:" );
                results.AppendLine( _campusJobStats.SummaryMessage );
                this.UpdateLastStatusMessage( results.ToString() );
            }

            // Run Stored Proc ETL for Financial Transaction BI Tables
            if ( GetAttributeValue( AttributeKey.ProcessFinancialTransactionBIAnalytics ).AsBoolean() )
            {
                try
                {
                    int rows = DbService.ExecuteCommand( "EXEC [dbo].[spAnalytics_ETL_FinancialTransaction]", System.Data.CommandType.Text, null, _commandTimeout );
                    results.AppendLine( "FinancialTransaction ETL completed." );

                    this.UpdateLastStatusMessage( results.ToString() );
                }
                catch ( System.Exception ex )
                {
                    HttpContext context2 = HttpContext.Current;
                    ExceptionLogService.LogException( ex, context2 );
                    throw;
                }
            }

            // Run Stored Proc ETL for Attendance BI Tables
            if ( GetAttributeValue( AttributeKey.ProcessAttendanceBIAnalytics ).AsBoolean() )
            {
                try
                {
                    int rows = DbService.ExecuteCommand( "EXEC [dbo].[spAnalytics_ETL_Attendance]", System.Data.CommandType.Text, null, _commandTimeout );
                    results.AppendLine( "Attendance ETL completed." );

                    this.UpdateLastStatusMessage( results.ToString() );
                }
                catch ( System.Exception ex )
                {
                    HttpContext context2 = HttpContext.Current;
                    ExceptionLogService.LogException( ex, context2 );
                    throw;
                }
            }

            // "Refresh Power BI Account Tokens"
            if ( GetAttributeValue( AttributeKey.RefreshPowerBIAccountTokens ).AsBoolean() )
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

            if ( GetAttributeValue( AttributeKey.ProcessGivingUnitBIAnalytics ).AsBoolean() )
            {
                ProcessGivingUnitAnalytics();
                results.AppendLine( "Giving Unit BI Analytic  Results:" );
                results.AppendLine( _givingUnitJobStats.SummaryMessage );
                this.UpdateLastStatusMessage( results.ToString() );
            }

            this.Result = results.ToString();
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

            var currentDatabaseAttributeFields = dataTable.Columns.OfType<DataColumn>().Where( a => !analyticsFieldNames.Contains( a.ColumnName ) ).ToList();

            const string BooleanSqlFieldType = "bit";
            const string DateTimeSqlFieldType = "datetime";
            const string NumericSqlFieldType = "[decimal](29,4)";
            string DefaultSqlFieldType = $"nvarchar({_maxAttributeValueLength})";

            using ( var rockContext = GetNewConfiguredDataContext() )
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

            if ( entityType == null )
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
            using ( var rockContext = GetNewConfiguredDataContext() )
            {
                var attributeValueService = new AttributeValueService( rockContext );

                foreach ( var attribute in modelAnalyticAttributes )
                {
                    var columnName = attribute.Key.RemoveSpecialCharacters();
                    var columnInfo = new ColumnInfo( attribute.FieldType.Field, columnName );

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
    AND isnull([{columnName}], {columnInfo.IsNullDefaultValue}) != isnull(@attributeValue, {columnInfo.IsNullDefaultValue})
    ";

                        if ( hasCurrentRowIndicator )
                        {
                            updateSql += "AND [CurrentRowIndicator] = 1";
                        }

                        var parameters = new Dictionary<string, object>();
                        parameters.Add( "@modelAttributeValue", modelAttributeValue ?? ( object ) DBNull.Value );
                        parameters.Add( "@attributeValue", attributeValue ?? ( object ) DBNull.Value );

                        modelJobStats.SqlLogs.Add( parameters.Select( a => $"/* {a.Key} = '{a.Value}' */" ).ToList().AsDelimited( "\n" ) + updateSql );
                        modelJobStats.AttributeFieldsUpdated += DbService.ExecuteCommand( updateSql, System.Data.CommandType.Text, parameters, _commandTimeout );
                    }
                }
            }
        }

        /// <summary>
        /// Get a new instance of the Rock data context that is configured for the current job settings.
        /// </summary>
        /// <returns>A configured RockContext instance.</returns>
        private RockContext GetNewConfiguredDataContext()
        {
            var dataContext = new RockContext();

            dataContext.Database.CommandTimeout = _commandTimeout;

            return dataContext;
        }

        #endregion Shared Methods

        #region Person Analytics

        /// <summary>
        /// Processes the person bi analytics.
        /// </summary>
        private void ProcessPersonBIAnalytics( DateTime processingDate )
        {
            List<EntityField> analyticsSourcePersonHistoricalFields = EntityHelper.GetEntityFields( typeof( Rock.Model.AnalyticsSourcePersonHistorical ), false, false );
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
                // Remove any analytics records that do not correspond to an existing Person.
                using ( var rockContext = GetNewConfiguredDataContext() )
                {
                    var recordsDeleted = rockContext.Database.ExecuteSqlCommand( "DELETE FROM [AnalyticsSourcePersonHistorical] WHERE [PersonId] NOT IN (SELECT [Id] FROM [Person])" );

                    if ( recordsDeleted > 0 )
                    {
                        Log( RockLogLevel.Debug, $"Removed {recordsDeleted} history records that do not correspond to a Person record." );
                    }
                }

                // Ensure that the Schema of AnalyticsSourcePersonHistorical matches the current fields for Attributes that are marked as IsAnalytic
                UpdateAnalyticsSchemaForModel( analyticsSourcePersonHistoricalFields, personAnalyticAttributes, "AnalyticsSourcePersonHistorical", _personJobStats );

                // refresh the view definitions just in case the schema changed
                // NOTE: Order is important!
                using ( var rockContext = GetNewConfiguredDataContext() )
                {
                    rockContext.Database.ExecuteSqlCommand( "exec sp_refreshview [AnalyticsDimPersonHistorical]" );
                    rockContext.Database.ExecuteSqlCommand( "exec sp_refreshview [AnalyticsDimPersonCurrent]" );
                    rockContext.Database.ExecuteSqlCommand( "exec sp_refreshview [AnalyticsDimFamilyHeadOfHousehold]" );
                }

                // start the update process by marking records as History if any of the "IsAnalyticHistory" values 
                // have changed for attributes that have to use FormatValue to get the value instead of directly in the DB
                MarkPersonAsHistoryUsingFormattedValue( personAnalyticAttributes, processingDate );

                // do the big ETL for stuff that can be done directly in the DB
                DoPersonMainPopulateETLs( analyticsSourcePersonHistoricalFields, personAnalyticAttributes, processingDate );

                // finish up by updating Attribute Values in the Analytic tables for attributes 
                // that have to use FormatValue to get the value instead of directly in the DB
                UpdatePersonAttributeValueUsingFormattedValue( personAnalyticAttributes );
            }
            finally
            {
                if ( GetAttributeValue( AttributeKey.SaveSQLForDebug ).AsBoolean() )
                {
                    LogSQL( "ProcessAnalyticsDimPerson.sql", _personJobStats.SqlLogs.AsDelimited( "\n" ).ToString() );
                }
            }
        }

        /// <summary>
        /// Marks Person Analytic rows as history if the formatted value of the attribute has changed
        /// </summary>
        /// <param name="personAnalyticAttributes">The person analytic attributes.</param>
        /// <param name="processingDate">The effective processing date.</param>
        private void MarkPersonAsHistoryUsingFormattedValue( List<AttributeCache> personAnalyticAttributes, DateTime processingDate )
        {
            List<SqlCommand> markAsHistoryUsingFormattedValueScripts = new List<SqlCommand>();

            // Compare "IsAnalyticHistory" attribute values to see if they have changed since the last ETL, using the FormatValue function
            using ( var rockContext = GetNewConfiguredDataContext() )
            {
                var attributeValueService = new AttributeValueService( rockContext );

                var attributesToProcess = personAnalyticAttributes.Where( a => a.IsAnalyticHistory && UseFormatValueForUpdate( a ) );
                foreach ( var attribute in attributesToProcess )
                {
                    var columnName = attribute.Key.RemoveSpecialCharacters();
                    var columnInfo = new ColumnInfo( attribute.FieldType.Field, columnName );

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
    @EtlDate DATE = DateFromParts( {processingDate.Year}, {processingDate.Month}, {processingDate.Day} )

UPDATE [AnalyticsSourcePersonHistorical]
    SET [CurrentRowIndicator] = 0, [ExpireDate] = @EtlDate
    WHERE [PersonId] IN (SELECT EntityId FROM AttributeValue WHERE AttributeId = {attribute.Id} AND Value = @personAttributeValue)
    AND isnull([{columnName}],{columnInfo.IsNullDefaultValue}) != isnull(@formattedValue,{columnInfo.IsNullDefaultValue})
    AND [CurrentRowIndicator] = 1
    AND PersonId NOT IN( --Ensure that there isn't already a History Record for the current EtlDate
        SELECT PersonId
        FROM AnalyticsSourcePersonHistorical x
        WHERE CurrentRowIndicator = 0
        AND[ExpireDate] = @EtlDate
    )";

                        var parameters = new Dictionary<string, object>();
                        parameters.Add( "@personAttributeValue", personAttributeValue ?? ( object ) DBNull.Value );
                        parameters.Add( "@formattedValue", formattedValue ?? ( object ) DBNull.Value );

                        _personJobStats.SqlLogs.Add( parameters.Select( a => $"/* {a.Key} = '{a.Value}' */" ).ToList().AsDelimited( "\n" ) + markAsHistorySQL );

                        _personJobStats.RowsMarkedAsHistory += DbService.ExecuteCommand( markAsHistorySQL, System.Data.CommandType.Text, parameters, _commandTimeout );
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
            using ( var rockContext = GetNewConfiguredDataContext() )
            {
                var attributeValueService = new Rock.Model.AttributeValueService( rockContext );

                foreach ( var attribute in personAnalyticAttributes.Where( a => UseFormatValueForUpdate( a ) ) )
                {
                    var columnName = attribute.Key.RemoveSpecialCharacters();
                    var columnInfo = new ColumnInfo( attribute.FieldType.Field, columnName );

                    var attributeValuesQry = attributeValueService.Queryable()
                            .Where( a => a.AttributeId == attribute.Id )
                            .Select( a => a.Value );

                    // get all the unique possible values that are currently being used
                    var personAttributeValues = attributeValuesQry.Distinct().ToList();
                    foreach ( var personAttributeValue in personAttributeValues )
                    {
                        var formattedValue = attribute.FieldType.Field.FormatValue( null, personAttributeValue, attribute.QualifierValues, false );

                        // unformatted values over this length are filtered out. Prevent truncated string SQL errors here by checking the formatted value against the max length.
                        if ( formattedValue.Length > _maxAttributeValueLength )
                        {
                            continue;
                        }

                        // mass update the value for the Attribute in the AnalyticsSourcePersonHistorical records 
                        // Don't update the Historical Records, even if it was just a Text change.  For example, 
                        // if they changed DefinedValue "Member" to "Owner", have the historical records say "Member"
                        // even though it is the same definedvalue id.
                        var updateSql = $@"
                                UPDATE [AnalyticsSourcePersonHistorical] 
                                    SET [{columnName}] = @formattedValue 
                                    WHERE [PersonId] IN (SELECT EntityId FROM AttributeValue WHERE AttributeId = {attribute.Id} AND Value = @personAttributeValue) 
                                        AND isnull([{columnName}],{columnInfo.IsNullDefaultValue}) != isnull(@formattedValue,{columnInfo.IsNullDefaultValue})
                                        AND [CurrentRowIndicator] = 1";

                        var parameters = new Dictionary<string, object>();
                        parameters.Add( "@personAttributeValue", personAttributeValue );
                        parameters.Add( "@formattedValue", formattedValue );

                        _personJobStats.SqlLogs.Add( parameters.Select( a => $"/* {a.Key} = '{a.Value}' */" ).ToList().AsDelimited( "\n" ) + updateSql );

                        try
                        {
                            _personJobStats.AttributeFieldsUpdated += DbService.ExecuteCommand( updateSql, System.Data.CommandType.Text, parameters, _commandTimeout );
                        }
                        catch ( Exception ex )
                        {
                            ExceptionLogService.LogException( new Exception( $"Error inserting Person Analytics value {columnName}. Value: {personAttributeValue}, formatted value: {formattedValue}", ex ) );

                            // Throw the exception since missing any data will not provide accurate analytics.
                            throw;
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
        /// <param name="processingDate">The effective processing date.</param>
        private void DoPersonMainPopulateETLs( List<EntityField> analyticsSourcePersonHistoricalFields, List<AttributeCache> personAnalyticAttributes, DateTime processingDate )
        {
            // columns that should be considered when determining if a new History record is needed
            var historyColumns = new List<ColumnInfo>();
            foreach ( var analyticsSourcePersonHistoricalProperty in analyticsSourcePersonHistoricalFields.Where( a => a.PropertyInfo.GetCustomAttribute<AnalyticHistoryFieldAttribute>() != null ) )
            {
                historyColumns.Add( new ColumnInfo( analyticsSourcePersonHistoricalProperty ) );
            }

            var attributeValueColumns = new List<ColumnInfo>();
            var personValueSelectColumns = new List<ColumnInfo>();

            var analyticSpecificColumns = new string[] { "Id", "PersonId", "CurrentRowIndicator", "EffectiveDate", "ExpireDate", "PrimaryFamilyId", "BirthDateKey", "Age", "Guid", "Count" };

            foreach ( var item in analyticsSourcePersonHistoricalFields
                .Where( a => !analyticSpecificColumns.Contains( a.Name ) ).OrderBy( a => a.Name ).ToList() )
            {
                var columnInfo = new ColumnInfo( item );
                columnInfo.SelectClause = item.Name;
                columnInfo.FromClause = item.Name;

                personValueSelectColumns.Add( columnInfo );
            }

            using ( var rockContext = GetNewConfiguredDataContext() )
            {
                // add any AttributeFields that aren't already fields on AnalyticsSourcePersonHistorical
                foreach ( var personAttribute in personAnalyticAttributes.Where( a => !UseFormatValueForUpdate( a ) ) )
                {
                    var columnName = personAttribute.Key.RemoveSpecialCharacters();
                    var personAttributeValueFieldName = personAttribute.FieldType.Field.AttributeValueFieldName;

                    // each SELECT clause should look something like: attribute_1071.ValueAsDateTime as [attribute_YouthVolunteerApplication]
                    var populateAttributeValueSELECTClause =
                        $"av{personAttribute.Id}.{personAttributeValueFieldName} as [{columnName}]";

                    var columnInfo = new ColumnInfo();
                    columnInfo.IsAttribute = true;
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

                    columnInfo.SelectClause = populateAttributeValueSELECTClause;
                    columnInfo.InsertClause = columnName;

                    attributeValueColumns.Add( columnInfo );
                    if ( personAttribute.IsAnalyticHistory )
                    {
                        historyColumns.Add( columnInfo );
                    }

                    var lengthCondition = personAttributeValueFieldName == "Value"
                        ? $"AND len(av{personAttribute.Id}.Value) <= {_maxAttributeValueLength}"
                        : null;

                    var populateAttributeValueFROMClause =
                        $"LEFT OUTER JOIN AttributeValue av{personAttribute.Id} ON av{personAttribute.Id}.EntityId = p.Id AND av{personAttribute.Id}.AttributeId = {personAttribute.Id} {lengthCondition}";

                    columnInfo.FromClause = populateAttributeValueFROMClause;
                }

                // Get the count of existing analytics rows that require updating.
                var countCandidateRecordsScript = GetPersonUpdateETLCandidateScript( personValueSelectColumns, attributeValueColumns );

                var modifiedRowCount = ( int ) DbService.ExecuteScalar( countCandidateRecordsScript, CommandType.Text, null, _commandTimeout );

                var markAsHistoryScript = GetPersonMarkAsHistoryScript( historyColumns );
                var updateETLScript = GetPersonUpdateETLScript( attributeValueColumns, personValueSelectColumns );
                var processINSERTScript = GetPersonProcessINSERTScript( personValueSelectColumns );

                var scriptDeclares = $@"
DECLARE 
    @EtlDate DATE = DateFromParts( {processingDate.Year}, {processingDate.Month}, {processingDate.Day} )
    , @MaxExpireDate DATE = DateFromParts( 9999, 1, 1 )";

                // throw script into logs in case 'Save SQL for Debug' is enabled
                _personJobStats.SqlLogs.Add( "/* MarkAsHistoryScript */\n" + scriptDeclares + markAsHistoryScript );
                _personJobStats.SqlLogs.Add( "/* ProcessINSERTScript */\n" + scriptDeclares + processINSERTScript );
                _personJobStats.SqlLogs.Add( "/* UpdateETLScript */\n" + scriptDeclares + updateETLScript );

                // Mark current records as history if they have changes in any fields that should trigger history.
                _personJobStats.RowsMarkedAsHistory += DbService.ExecuteCommand( scriptDeclares + markAsHistoryScript, CommandType.Text, null, _commandTimeout );

                // Insert new Person Records that don't have an existing entry in the history table.
                // For new records, only the person properties are initially populated.
                // Attribute values are updated in the next step, to allow batch processing for large numbers of attributes.
                _personJobStats.RowsInserted += DbService.ExecuteCommand( scriptDeclares + processINSERTScript, CommandType.Text, null, _commandTimeout );

                // Update the current analytics records (CurrentRowIndicator=1) with data from the source tables.
                DbService.ExecuteCommand( scriptDeclares + updateETLScript, CommandType.Text, null, _commandTimeout );

                // Get the number of analytics rows updated, excluding any rows that were moved to history.
                var updatedCount = modifiedRowCount - _personJobStats.RowsMarkedAsHistory;
                if ( updatedCount > 0 )
                {
                    _personJobStats.RowsUpdated += updatedCount;
                }
            }
        }

        /// <summary>
        /// Gets the process insert script for Person analytics tables
        /// </summary>
        /// <param name="propertyColumns">The person property columns.</param>
        /// <returns></returns>
        private static string GetPersonProcessINSERTScript( List<ColumnInfo> propertyColumns )
        {
            // Insert new records into the history table for person records that do not have a matching history record marked as current.
            // The new records are populated with person properties only, and attribute fields will be populated in a subsequent step.
            // This adds some overhead to SQL processing, but avoids scaling issues when dealing with a large number of attributes or attribute values.
            var processINSERTScript = @"
INSERT INTO [dbo].[AnalyticsSourcePersonHistorical] (
        [PersonId],
        [CurrentRowIndicator],
        [EffectiveDate],
        [ExpireDate],
        [PrimaryFamilyId],
        [BirthDateKey],
        [Age],
        [Count],
" + propertyColumns.Select( a => $"        [{a.SelectClause}]" ).ToList().AsDelimited( ",\n" ) + @",
        [Guid]
    )
";


            processINSERTScript += @"
    SELECT 
        p.Id [PersonId],
        1 [CurrentRowIndicator],
        @EtlDate [EffectiveDate],
        @MaxExpireDate [ExpireDate],
        p.PrimaryFamilyId [PrimaryFamilyId],
        convert(INT, (convert(CHAR(8), BirthDate, 112))) [BirthDateKey],
        p.Age [Age], 
        1 [Count],
" + propertyColumns.Select( a => $"        [{a.FromClause}]" ).ToList().AsDelimited( ",\n" ) + @",
        NEWID() [Guid]
FROM dbo.Person p
WHERE p.Id NOT IN (
            SELECT PersonId
            FROM [AnalyticsSourcePersonHistorical]
            WHERE CurrentRowIndicator = 1
            )";

            return processINSERTScript;
        }

        /// <summary>
        /// Gets the mark as history script for Person Analytic Tables
        /// </summary>
        /// <param name="historyColumns">The history columns.</param>
        /// <returns></returns>
        private string GetPersonMarkAsHistoryScript( List<ColumnInfo> historyColumns )
        {
            var propertyColumns = historyColumns.Where( c => !c.IsAttribute ).ToList();
            var attributeColumns = historyColumns.Where( c => c.IsAttribute ).ToList();

            string selectSQL = @"
    SELECT 
        p.Id [PersonId],
" + propertyColumns.Select( a => $"        [{a.ColumnName}]" ).ToList().AsDelimited( ",\n" );

            if ( attributeColumns.Any() )
            {
                // only need a comma when we have more SELECT clauses to add
                selectSQL += ",\n";
            }
            else
            {
                selectSQL += "\n";
            }

            selectSQL += attributeColumns.Select( a => "        " + a.SelectClause ).ToList().AsDelimited( ",\n" );

            selectSQL += @"
FROM dbo.Person p
";

            // add the "LEFT OUTER JOIN..." AttributeValue FROM clauses
            selectSQL += attributeColumns.Select( a => "        " + a.FromClause ).ToList().AsDelimited( "\n" );

            var markAsHistoryScript = @"

;with cte1 as (" + selectSQL + @")";

            markAsHistoryScript += @"
UPDATE AnalyticsSourcePersonHistorical SET
        CurrentRowIndicator = 0,
        [ExpireDate] = @EtlDate
FROM AnalyticsSourcePersonHistorical asph
JOIN cte1 ON cte1.PersonId = asph.PersonId
WHERE asph.CurrentRowIndicator = 1 and (
";

            markAsHistoryScript += historyColumns.Select( a => $" isnull(asph.[{a.ColumnName}], {a.IsNullDefaultValue}) != isnull(cte1.[{a.ColumnName}],{a.IsNullDefaultValue})" ).ToList().AsDelimited( " OR \n" );

            markAsHistoryScript += @")
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
        /// <returns></returns>
        private string GetPersonUpdateETLScript( List<ColumnInfo> attributeValueColumns, List<ColumnInfo> populatePersonValueSELECTColumns )
        {
            // Create script to update Person properties.
            var updateETLScript = GetPersonUpdateETLScriptForPersonValues( populatePersonValueSELECTColumns );
            updateETLScript += "\n";

            // Add scripts to update Person attributes.
            foreach ( var attributeValueColumn in attributeValueColumns )
            {
                updateETLScript += GetPersonUpdateETLScriptForAttributeValues( new List<ColumnInfo> { attributeValueColumn } );
            }

            return updateETLScript;
        }

        /// <summary>
        /// Gets the update etl script for Person Analytic Tables
        /// </summary>
        /// <param name="personValueColumns">The populate person value select columns.</param>
        /// <returns></returns>
        private string GetPersonUpdateETLScriptForPersonValues( List<ColumnInfo> personValueColumns )
        {
            var updateETLScript = @"
;with cte1 as (
    SELECT 
        p.Id [PersonId],
        p.PrimaryFamilyId [PrimaryFamilyId],
        convert(INT, (convert(CHAR(8), BirthDate, 112))) [BirthDateKey],
        p.Age [Age], 
";

            updateETLScript += personValueColumns.Select( a => $"        [{a.ColumnName}]" ).ToList().AsDelimited( ",\n" );
            updateETLScript += @"
    FROM dbo.Person p
)
UPDATE AnalyticsSourcePersonHistorical SET
        [PrimaryFamilyId] = cte1.[PrimaryFamilyId],
        [BirthDateKey] = cte1.[BirthDateKey],
        [Age] = cte1.[Age],
";
            updateETLScript += personValueColumns.Select( a => $"        [{a.ColumnName}] = cte1.[{a.ColumnName}]" ).ToList().AsDelimited( ",\n" );
            updateETLScript += @"
FROM AnalyticsSourcePersonHistorical asph
JOIN cte1 ON cte1.PersonId = asph.PersonId
WHERE asph.CurrentRowIndicator = 1 AND (
";
            updateETLScript += personValueColumns.Select( a => $"        isnull(asph.[{a.ColumnName}],{a.IsNullDefaultValue}) != isnull(cte1.[{a.ColumnName}],{a.IsNullDefaultValue})" ).ToList().AsDelimited( " OR \n" );
            updateETLScript += " OR \n        isnull(asph.[Age],-1) != isnull(cte1.[Age],-1)";
            updateETLScript += ");";

            return updateETLScript;
        }

        /// <summary>
        /// Gets the update etl script for Person Analytic Tables
        /// </summary>
        /// <param name="attributeValueColumns">The attribute value columns.</param>
        /// <returns></returns>
        private string GetPersonUpdateETLScriptForAttributeValues( List<ColumnInfo> attributeValueColumns )
        {
            string selectSQL = @"
    SELECT 
        p.Id [PersonId],
";
            selectSQL += attributeValueColumns.Select( a => "        " + a.SelectClause ).ToList().AsDelimited( ",\n" );
            selectSQL += @"
FROM dbo.Person p
";

            //add the "LEFT OUTER JOIN..." AttributeValue FROM clauses
            selectSQL += attributeValueColumns.Select( a => "        " + a.FromClause ).ToList().AsDelimited( "\n" );

            var withCTEScript = @"
;with cte1 as (" + selectSQL + @"
)";
            string updateETLScript = withCTEScript;

            updateETLScript += @"
UPDATE AnalyticsSourcePersonHistorical SET 
";
            updateETLScript += attributeValueColumns.Select( a => $"        [{a.ColumnName}] = cte1.[{a.ColumnName}]" ).ToList().AsDelimited( ",\n" );

            updateETLScript += @"
FROM AnalyticsSourcePersonHistorical asph
JOIN cte1 ON cte1.PersonId = asph.PersonId
WHERE asph.CurrentRowIndicator = 1 AND (
";
            updateETLScript += attributeValueColumns.Select( a => $"        isnull(asph.[{a.ColumnName}],{a.IsNullDefaultValue}) != isnull(cte1.[{a.ColumnName}],{a.IsNullDefaultValue})" ).ToList().AsDelimited( " OR \n" );
            updateETLScript += ")";

            return updateETLScript;
        }

        /// <summary>
        /// Gets a SQL script to count the number of rows in the analytics table that require updating.
        /// </summary>
        /// <param name="propertyColumns">The populate person value select columns.</param>
        /// <param name="attributeValueColumns">The attribute value columns.</param>
        /// <returns></returns>
        private string GetPersonUpdateETLCandidateScript( List<ColumnInfo> propertyColumns, List<ColumnInfo> attributeValueColumns )
        {
            var countCandidatePersonScript = @"
WITH cte1 as (
    SELECT 
        p.Id [PersonId],
        p.PrimaryFamilyId [PrimaryFamilyId],
        p.Age [Age], 
";

            countCandidatePersonScript += propertyColumns.Select( a => $"        [{a.ColumnName}]" ).ToList().AsDelimited( ",\n" );

            if ( attributeValueColumns.Any() )
            {
                // only need a comma when we have more SELECT clauses to add
                countCandidatePersonScript += ",\n";
            }
            else
            {
                countCandidatePersonScript += "\n";
            }

            countCandidatePersonScript += attributeValueColumns.Select( a => "        " + a.SelectClause ).ToList().AsDelimited( ",\n" );
            countCandidatePersonScript += "\n";

            countCandidatePersonScript += @"
    FROM dbo.Person p
";
            countCandidatePersonScript += attributeValueColumns.Select( a => "        " + a.FromClause ).ToList().AsDelimited( "\n" );
            countCandidatePersonScript += @"
)
SELECT COUNT(*)
FROM AnalyticsSourcePersonHistorical asph
JOIN cte1 ON cte1.PersonId = asph.PersonId
WHERE asph.CurrentRowIndicator = 1 AND (
";

            countCandidatePersonScript += propertyColumns.Select( a => $"        isnull(asph.[{a.ColumnName}],{a.IsNullDefaultValue}) != isnull(cte1.[{a.ColumnName}],{a.IsNullDefaultValue})" ).ToList().AsDelimited( " OR \n" );
            countCandidatePersonScript += " OR \n        isnull(asph.[Age],-1) != isnull(cte1.[Age],-1)";
            if ( attributeValueColumns.Any() )
            {
                countCandidatePersonScript += " OR \n";
                countCandidatePersonScript += attributeValueColumns.Select( a => $"        isnull(asph.[{a.ColumnName}],{a.IsNullDefaultValue}) != isnull(cte1.[{a.ColumnName}],{a.IsNullDefaultValue})" ).ToList().AsDelimited( " OR \n" );
            }

            countCandidatePersonScript += ")";

            return countCandidatePersonScript;
        }

        #endregion

        #region Family Analytics

        /// <summary>
        /// Processes the family bi analytics.
        /// </summary>
        private void ProcessFamilyBIAnalytics( DateTime processingDate )
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

                using ( var rockContext = GetNewConfiguredDataContext() )
                {
                    // refresh the view definitions just in case the schema changed
                    // NOTE: Order is important!
                    rockContext.Database.ExecuteSqlCommand( "exec sp_refreshview [AnalyticsDimFamilyHistorical]" );
                    rockContext.Database.ExecuteSqlCommand( "exec sp_refreshview [AnalyticsDimFamilyCurrent]" );
                }

                // start the update process by marking records as History if any of the "IsAnalyticHistory" values 
                // have changed for attributes 
                MarkFamilyAsHistoryUsingAttributeValues( familyAnalyticAttributes, processingDate );

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
                if ( GetAttributeValue( AttributeKey.SaveSQLForDebug ).AsBoolean() )
                {
                    LogSQL( "ProcessAnalyticsDimFamily.sql", _familyJobStats.SqlLogs.AsDelimited( "\n" ).ToString() );
                }
            }
        }

        /// <summary>
        /// Marks Family Analytic rows as history if the value of the attribute has changed
        /// </summary>
        /// <param name="familyAnalyticAttributes">The family analytic attributes.</param>
        /// <param name="processingDate">The effective processing date.</param>
        private void MarkFamilyAsHistoryUsingAttributeValues( List<AttributeCache> familyAnalyticAttributes, DateTime processingDate )
        {
            using ( var rockContext = GetNewConfiguredDataContext() )
            {
                var attributeValueService = new AttributeValueService( rockContext );

                foreach ( var attribute in familyAnalyticAttributes.Where( a => a.IsAnalyticHistory ) )
                {
                    var columnName = attribute.Key.RemoveSpecialCharacters();
                    var columnInfo = new ColumnInfo( attribute.FieldType.Field, columnName );
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
    @EtlDate DATE = DateFromParts( {processingDate.Year}, {processingDate.Month}, {processingDate.Day} )

UPDATE [AnalyticsSourceFamilyHistorical] 
    SET [CurrentRowIndicator] = 0, [ExpireDate] = @EtlDate 
    WHERE [FamilyId] IN (SELECT EntityId FROM AttributeValue WHERE AttributeId = {attribute.Id} AND Value = @familyAttributeValue )
    AND isnull([{columnName}],{columnInfo.IsNullDefaultValue}) != isnull(@attributeValue,{columnInfo.IsNullDefaultValue})
    AND [CurrentRowIndicator] = 1
    AND FamilyId NOT IN( --Ensure that there isn't already a History Record for the current EtlDate 
        SELECT FamilyId
        FROM AnalyticsSourceFamilyHistorical x
        WHERE CurrentRowIndicator = 0
        AND[ExpireDate] = @EtlDate
    )";

                        var parameters = new Dictionary<string, object>();
                        parameters.Add( "@familyAttributeValue", familyAttributeValue ?? ( object ) DBNull.Value );
                        parameters.Add( "@attributeValue", attributeValue ?? ( object ) DBNull.Value );
                        _familyJobStats.SqlLogs.Add( parameters.Select( a => $"/* {a.Key} = '{a.Value}' */" ).ToList().AsDelimited( "\n" ) + markAsHistorySQL );

                        _familyJobStats.RowsMarkedAsHistory += DbService.ExecuteCommand( markAsHistorySQL, System.Data.CommandType.Text, parameters, _commandTimeout );
                    }
                }
            }
        }

        #endregion Family Analytics

        #region Campus Analytics

        /// <summary>
        /// Processes the campus bi analytics.
        /// </summary>
        private void ProcessCampusBIAnalytics()
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
                if ( GetAttributeValue( AttributeKey.SaveSQLForDebug ).AsBoolean() )
                {
                    LogSQL( "ProcessAnalyticsDimCampus.sql", _campusJobStats.SqlLogs.AsDelimited( "\n" ).ToString() );
                }
            }
        }

        #endregion Campus Analytics

        #region GivingUnit Analytics

        private void ProcessGivingUnitAnalytics()
        {
            var rockContext = new RockContext();
            rockContext.Database.CommandTimeout = _commandTimeout;

            var givingLeaderPersonQry = new PersonService( rockContext ).Queryable().Where( p => p.GivingLeaderId == p.Id );

            var givingAttributes = new List<AttributeCache>();
            var personGivingBinAttributeId = AttributeCache.GetId( Rock.SystemGuid.Attribute.PERSON_GIVING_BIN.AsGuid() );
            var personGivingPercentileAttributeId = AttributeCache.GetId( Rock.SystemGuid.Attribute.PERSON_GIVING_PERCENTILE.AsGuid() );
            var personGivingAmountMedianAttributeId = AttributeCache.GetId( Rock.SystemGuid.Attribute.PERSON_GIVING_AMOUNT_MEDIAN.AsGuid() );
            var personGivingAmountIQRAttributeId = AttributeCache.GetId( Rock.SystemGuid.Attribute.PERSON_GIVING_AMOUNT_IQR.AsGuid() );
            var personGivingFreqencyMeanDaysAttributeId = AttributeCache.GetId( Rock.SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_MEAN_DAYS.AsGuid() );
            var personGivingFreqencyStdDevDaysAttributeId = AttributeCache.GetId( Rock.SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_STD_DEV_DAYS.AsGuid() );
            var personGivingPercentScheduledAttributeId = AttributeCache.GetId( Rock.SystemGuid.Attribute.PERSON_GIVING_PERCENT_SCHEDULED.AsGuid() );

            var personGivingFrequencyLabelAttributeId = AttributeCache.GetId( Rock.SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_LABEL.AsGuid() );
            var personGivingPreferredCurrencyAttributeId = AttributeCache.GetId( Rock.SystemGuid.Attribute.PERSON_GIVING_PREFERRED_CURRENCY.AsGuid() );
            var personGivingPreferredSourceAttributeId = AttributeCache.GetId( Rock.SystemGuid.Attribute.PERSON_GIVING_PREFERRED_SOURCE.AsGuid() );

            var attributesValuesQuery = new AttributeValueService( rockContext ).Queryable();

            var givingUnitDataQuery = givingLeaderPersonQry.Select( p => new
            {
                GivingId = p.GivingId,
                GivingLeaderPersonId = p.Id,
                GivingSalutation = p.PrimaryFamily.GroupSalutation,
                GivingSalutationFull = p.PrimaryFamily.GroupSalutationFull,

                GivingBinValue = attributesValuesQuery.Where( av => av.EntityId == p.Id && av.AttributeId == personGivingBinAttributeId ).Select( a => a.Value ).FirstOrDefault(),
                GivingPercentileValue = attributesValuesQuery.Where( av => av.EntityId == p.Id && av.AttributeId == personGivingPercentileAttributeId ).Select( a => a.Value ).FirstOrDefault(),
                GiftAmountMedianValue = attributesValuesQuery.Where( av => av.EntityId == p.Id && av.AttributeId == personGivingAmountMedianAttributeId ).Select( a => a.Value ).FirstOrDefault(),
                GiftAmountIqrValue = attributesValuesQuery.Where( av => av.EntityId == p.Id && av.AttributeId == personGivingAmountIQRAttributeId ).Select( a => a.Value ).FirstOrDefault(),

                GiftFrequencyMeanValue = attributesValuesQuery.Where( av => av.EntityId == p.Id && av.AttributeId == personGivingFreqencyMeanDaysAttributeId ).Select( a => a.Value ).FirstOrDefault(),
                GiftFrequencyStandardDeviationValue = attributesValuesQuery.Where( av => av.EntityId == p.Id && av.AttributeId == personGivingFreqencyStdDevDaysAttributeId ).Select( a => a.Value ).FirstOrDefault(),
                PercentGiftsScheduledValue = attributesValuesQuery.Where( av => av.EntityId == p.Id && av.AttributeId == personGivingPercentScheduledAttributeId ).Select( a => a.Value ).FirstOrDefault(),
                FrequencyIntValue = attributesValuesQuery.Where( av => av.EntityId == p.Id && av.AttributeId == personGivingFrequencyLabelAttributeId ).Select( a => a.Value ).FirstOrDefault(),

                PreferredCurrencyValueGuidValue = attributesValuesQuery.Where( av => av.EntityId == p.Id && av.AttributeId == personGivingPreferredCurrencyAttributeId ).Select( a => a.Value ).FirstOrDefault(),
                PreferredSourceValueGuidValue = attributesValuesQuery.Where( av => av.EntityId == p.Id && av.AttributeId == personGivingPreferredSourceAttributeId ).Select( a => a.Value ).FirstOrDefault(),
            } );

            var analyticsSourceGivingUnitQry = new AnalyticsSourceGivingUnitService( rockContext ).Queryable();

            // Figure out which GIvingId data needs to be Inserted into  AnalyticsSourceGivingUnit
            var givingIdsToInsert = givingLeaderPersonQry.Where( p => !analyticsSourceGivingUnitQry.Any( a => a.GivingId == p.GivingId ) ).Select( a => a.GivingId ).ToList();

            // delete any  AnalyticsSourceGivingUnit rows that have a GivingId that no longer exists
            var analyticsSourceGivingUnitToDeleteQuery = analyticsSourceGivingUnitQry.Where( a => !givingLeaderPersonQry.Any( p => p.GivingId == a.GivingId ) );
            _givingUnitJobStats.RowsDeleted = rockContext.BulkDelete( analyticsSourceGivingUnitToDeleteQuery );

            // Create a List of AnalyticsSourceGivingUnit records from the current giving data attribute values, etc
            // We'll use that to figure out which ones to Insert/Update/Delete in the AnalyticsSourceGivingUnit table
            var frequencyLabelLookup = Enum.GetValues( typeof( FinancialGivingAnalyticsFrequencyLabel ) ).OfType<FinancialGivingAnalyticsFrequencyLabel>()
                .ToDictionary( k => k.ConvertToInt(), v => v.GetDescription() );

            var givingUnitDataQueryList = givingUnitDataQuery.ToList();
            var analyticsSourceGivingUnitFromCurrentData = givingUnitDataQueryList
                .ToList()
                .Select( a =>
                {
                    var result = new AnalyticsSourceGivingUnit
                    {
                        GivingId = a.GivingId,
                        GivingLeaderPersonId = a.GivingLeaderPersonId,
                        GivingSalutation = a.GivingSalutation,
                        GivingSalutationFull = a.GivingSalutationFull,
                        GivingBin = a.GivingBinValue.AsInteger(),
                        GivingPercentile = a.GivingPercentileValue.AsInteger(),

                        GiftAmountMedian = decimal.Round( a.GiftAmountMedianValue.AsDecimal(), 2 ),
                        GiftAmountIqr = decimal.Round( a.GiftAmountIqrValue.AsDecimal(), 2 ),
                        GiftFrequencyMean = decimal.Round( a.GiftFrequencyMeanValue.AsDecimal(), 2 ),
                        GiftFrequencyStandardDeviation = decimal.Round( a.GiftFrequencyStandardDeviationValue.AsDecimal(), 2 ),

                        PercentGiftsScheduled = a.PercentGiftsScheduledValue.AsInteger(),
                        Frequency = frequencyLabelLookup.GetValueOrNull( a.FrequencyIntValue.AsInteger() ) ?? "Undetermined",
                    };

                    var preferredCurrencyValueGuid = a.PreferredCurrencyValueGuidValue.AsGuidOrNull();
                    if ( preferredCurrencyValueGuid.HasValue )
                    {
                        var preferredCurrencyValue = DefinedValueCache.Get( preferredCurrencyValueGuid.Value );
                        result.PreferredCurrencyValueId = preferredCurrencyValue?.Id ?? 0;
                        result.PreferredCurrency = preferredCurrencyValue?.Value;
                    }

                    var preferredSourceValueGuid = a.PreferredSourceValueGuidValue.AsGuidOrNull();

                    if ( preferredSourceValueGuid.HasValue )
                    {
                        var preferredSourceValue = DefinedValueCache.Get( preferredSourceValueGuid.Value );
                        result.PreferredSourceValueId = preferredSourceValue?.Id ?? 0;
                        result.PreferredSource = preferredSourceValue?.Value;
                    }

                    return result;
                } ).ToList();

            // using the list of givingIdsToInsert, and the current data for those givingIds, create AnalyticsSourceGivingUnit records to bulk insert into the database
            var analyticsSourceGivingUnitToInsert = analyticsSourceGivingUnitFromCurrentData.Where( a => givingIdsToInsert.Contains( a.GivingId ) ).ToList();
            if ( analyticsSourceGivingUnitToInsert.Any() )
            {
                _givingUnitJobStats.RowsInserted = analyticsSourceGivingUnitToInsert.Count();
                rockContext.BulkInsert( analyticsSourceGivingUnitToInsert );
            }

            // Join CurrentGIvingUnitData to the data in the AnalyticsSourceGivingUnit
            // Then we can use this join to compare and updated as needed
            var analyticsSourceGivingUnitAll = analyticsSourceGivingUnitQry.ToList();
            var givingUnitsJoinCompareList = analyticsSourceGivingUnitFromCurrentData
                .Join(
                    analyticsSourceGivingUnitAll,
                    currentData => currentData.GivingId,
                    biData => biData.GivingId,
                    ( currentData, biData ) => new
                    {
                        CurrentGivingUnitData = currentData,
                        BIGivingUnitData = biData
                    } );

            // now go thru and update the values on every record. If there are changes, EF will detect that and updated the record in the database.
            foreach ( var givingUnitJoin in givingUnitsJoinCompareList )
            {
                AnalyticsSourceGivingUnit analyticsSourceGivingUnit = givingUnitJoin.BIGivingUnitData;
                var currentGivingUnitData = givingUnitJoin.CurrentGivingUnitData;
                analyticsSourceGivingUnit.Frequency = currentGivingUnitData.Frequency;
                analyticsSourceGivingUnit.GiftAmountIqr = currentGivingUnitData.GiftAmountIqr;
                analyticsSourceGivingUnit.GiftAmountMedian = currentGivingUnitData.GiftAmountMedian;
                analyticsSourceGivingUnit.GiftFrequencyMean = currentGivingUnitData.GiftFrequencyMean;
                analyticsSourceGivingUnit.GiftFrequencyStandardDeviation = currentGivingUnitData.GiftFrequencyStandardDeviation;
                analyticsSourceGivingUnit.GivingBin = currentGivingUnitData.GivingBin;
                analyticsSourceGivingUnit.GivingLeaderPersonId = currentGivingUnitData.GivingLeaderPersonId;
                analyticsSourceGivingUnit.GivingPercentile = currentGivingUnitData.GivingPercentile;
                analyticsSourceGivingUnit.GivingSalutation = currentGivingUnitData.GivingSalutation;
                analyticsSourceGivingUnit.GivingSalutationFull = currentGivingUnitData.GivingSalutationFull;
                analyticsSourceGivingUnit.PercentGiftsScheduled = currentGivingUnitData.PercentGiftsScheduled;
                analyticsSourceGivingUnit.PreferredCurrency = currentGivingUnitData.PreferredCurrency;
                analyticsSourceGivingUnit.PreferredCurrencyValueId = currentGivingUnitData.PreferredCurrencyValueId;
                analyticsSourceGivingUnit.PreferredSource = currentGivingUnitData.PreferredSource;
                analyticsSourceGivingUnit.PreferredSourceValueId = currentGivingUnitData.PreferredSourceValueId;
            }

            var rowsUpdated = rockContext.SaveChanges();

            _givingUnitJobStats.RowsUpdated = rowsUpdated;
        }

        #endregion GivingUnit Analytics

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
            /// Initializes a new instance of the <see cref="ColumnInfo"/> class.
            /// </summary>
            /// <param name="fieldType">The entity field.</param>
            /// <param name="name">The entity field.</param>
            public ColumnInfo( IFieldType fieldType, string name )
            {
                this.ColumnName = name;

                switch ( fieldType.AttributeValueFieldName )
                {
                    case "ValueAsNumeric":
                        IsNullDefaultValue = "0";
                        break;
                    case "ValueAsDateTime":
                        IsNullDefaultValue = "DateFromParts( 9999, 1, 1 )";
                        break;
                    case "ValueAsBoolean":
                        IsNullDefaultValue = "0";
                        break;
                    default:
                        IsNullDefaultValue = "''";
                        break;
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


            public bool IsAttribute { get; set; }
            public string SelectClause { get; set; }
            public string InsertClause { get; set; }
            public string FromClause { get; set; }
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
            /// Gets or sets the rows deleted.
            /// </summary>
            /// <value>
            /// The rows deleted.
            /// </value>
            public int RowsDeleted { get; set; }

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
                    return RowsMarkedAsHistory > 0 || RowsUpdated > 0 || RowsInserted > 0 || AttributeFieldsUpdated > 0 || RowsDeleted > 0;
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
            public List<string> SqlLogs { get; set; } = new List<string>();

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
                        if ( this.RowsMarkedAsHistory > 0 )
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

                        if ( this.RowsDeleted > 0 )
                        {
                            summaryMessage.Append( $" Deleted {this.RowsDeleted} records;" );
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
