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
    [BooleanField( "Process Financial Transaction BI Analytics", "Do the BI Analytics tasks related to the Financial Transaction Analytics tables", true, "", 3 )]
    [BooleanField( "Process Attendance BI Analytics", "Do the BI Analytics tasks related to the Attendance Analytics tables", true, "", 4 )]
    [BooleanField( "Refresh Power BI Account Tokens", "Refresh any Power BI Account Tokens to prevent them from expiring.", true, "", 5 )]
    [IntegerField( "Command Timeout", "Maximum amount of time (in seconds) to wait for each SQL command to complete. Leave blank to use the default for this job (3600 seconds). Note that some of the tasks might take a while on larger databases, so you might need to set it higher.", false, 60 * 60, "General", 6, "CommandTimeout" )]
    [BooleanField( "Save SQL for Debug", "Save the SQL that is used in the Person and Family related parts of the this job to App_Data\\Logs", false, "Advanced", 7, "SaveSQLForDebug" )]
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

        private int? _commandTimeout = null;
        private List<string> _sqlLogs = new List<string>();

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

            int? commandTimeout = dataMap.GetString( "CommandTimeout" ).AsIntegerOrNull();

            StringBuilder results = new StringBuilder();

            // Do the stuff for Person related BI Tables
            if ( dataMap.GetString( "ProcessPersonBIAnalytics" ).AsBoolean() )
            {
                ProcessPersonBIAnalytics( context, dataMap );

                results.AppendLine( "Person BI Results:" );

                if ( _personJobStats.ColumnsAdded != 0 || _personJobStats.ColumnsModified != 0 || _personJobStats.ColumnsRemoved != 0 )
                {
                    results.AppendLine( $"-- Added {_personJobStats.ColumnsAdded}, modified {_personJobStats.ColumnsModified}, and removed {_personJobStats.ColumnsRemoved} Person attribute columns." );
                }

                results.AppendLine( $"-- Marked {_personJobStats.RowsMarkedAsHistory} records as History, updated {_personJobStats.RowsUpdated} records, updated {_personJobStats.AttributeFieldsUpdated} attribute formatted values, and inserted {_personJobStats.RowsInserted} records." );

                context.UpdateLastStatusMessage( results.ToString() );
            }

            // Do the stuff for Family related BI Tables
            if ( dataMap.GetString( "ProcessFamilyBIAnalytics" ).AsBoolean() )
            {
                ProcessFamilyBIAnalytics( context, dataMap );

                results.AppendLine( "Family BI Results:" );

                if ( _familyJobStats.ColumnsAdded != 0 || _familyJobStats.ColumnsModified != 0 || _familyJobStats.ColumnsRemoved != 0 )
                {
                    results.AppendLine( $"-- Added {_familyJobStats.ColumnsAdded}, modified {_familyJobStats.ColumnsModified}, and removed {_familyJobStats.ColumnsRemoved} attribute columns." );
                }

                results.AppendLine( $"-- Marked {_familyJobStats.RowsMarkedAsHistory} records as History, updated {_familyJobStats.RowsUpdated} records, and updated {_familyJobStats.AttributeFieldsUpdated} attribute values, and inserted {_familyJobStats.RowsInserted} records." );

                context.UpdateLastStatusMessage( results.ToString() );
            }

            // Run Stored Proc ETL for Financial Transaction BI Tables
            if ( dataMap.GetString( "ProcessFinancialTransactionBIAnalytics" ).AsBoolean() )
            {
                try
                {
                    int rows = DbService.ExecuteCommand( "EXEC [dbo].[spAnalytics_ETL_FinancialTransaction]", System.Data.CommandType.Text, null, commandTimeout );
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
                    int rows = DbService.ExecuteCommand( "EXEC [dbo].[spAnalytics_ETL_Attendance]", System.Data.CommandType.Text, null, commandTimeout );
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
                var powerBiAccountsDefinedType = DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.POWERBI_ACCOUNTS.AsGuid() );
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

        #endregion Shared Methods

        #region Person Analytics

        /// <summary>
        /// Processes the person bi analytics.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="dataMap">The data map.</param>
        private void ProcessPersonBIAnalytics( IJobExecutionContext context, JobDataMap dataMap )
        {
            List<EntityField> analyticsSourcePersonHistoricalFields = EntityHelper.GetEntityFields( typeof( Rock.Model.AnalyticsSourcePersonHistorical ), false, false );

            List<AttributeCache> personAnalyticAttributes = EntityHelper.GetEntityFields( typeof( Rock.Model.Person ) )
                .Where( a => a.FieldKind == FieldKind.Attribute && a.AttributeGuid.HasValue )
                .Select( a => AttributeCache.Read( a.AttributeGuid.Value ) )
                .Where( a => a != null )
                .Where( a => a.IsAnalytic )
                .ToList();

            try
            {
                // Ensure that the Schema of AnalyticsSourcePersonHistorical matches the current fields for Attributes that are marked as IsAnalytic
                UpdateAnalyticsSourcePersonHistoricalSchema( analyticsSourcePersonHistoricalFields, personAnalyticAttributes );

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
                    LogSQL( "ProcessAnalyticsDimPerson.sql", _sqlLogs.AsDelimited( "\n" ).ToString() );
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
                            this._personJobStats.RowsMarkedAsHistory += DbService.ExecuteCommand( markAsHistorySQL, System.Data.CommandType.Text, parameters );

                            _sqlLogs.Add( parameters.Select( a => $"/* {a.Key} = '{a.Value}' */" ).ToList().AsDelimited( "\n" ) + markAsHistorySQL );
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
                            _personJobStats.AttributeFieldsUpdated += DbService.ExecuteCommand( updateSql, System.Data.CommandType.Text, parameters );

                            _sqlLogs.Add( parameters.Select( a => $"/* {a.Key} = '{a.Value}' */" ).ToList().AsDelimited( "\n" ) + updateSql );
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

            const int MaxAttributeValueLength = 250;

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
                        ? $"AND len(av{personAttribute.Id}.Value) <= {MaxAttributeValueLength}"
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
                _sqlLogs.Add( "/* MarkAsHistoryScript */\n" + scriptDeclares + markAsHistoryScript );
                _sqlLogs.Add( "/* UpdateETLScript */\n" + scriptDeclares + updateETLScript );
                _sqlLogs.Add( "/* ProcessINSERTScript */\n" + scriptDeclares + processINSERTScript );

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
        convert(INT, (convert(CHAR(8), DateFromParts(BirthYear, BirthMonth, BirthDay), 112))) [BirthDateKey],
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

        /// <summary>
        /// Updates the analytics source person historical schema.
        /// </summary>
        /// <param name="analyticsSourcePersonHistoricalFields">The analytics source person historical fields.</param>
        /// <param name="personAnalyticAttributes">The person analytic attributes.</param>
        private void UpdateAnalyticsSourcePersonHistoricalSchema( List<EntityField> analyticsSourcePersonHistoricalFields, List<AttributeCache> personAnalyticAttributes )
        {
            var dataSet = DbService.GetDataSetSchema( "SELECT * FROM [AnalyticsSourcePersonHistorical] where 1=0", System.Data.CommandType.Text, null );
            var dataTable = dataSet.Tables[0];

            var analyticsSourcePersonHistoricalFieldNames = analyticsSourcePersonHistoricalFields.Select( a => a.Name ).ToList();
            analyticsSourcePersonHistoricalFieldNames.Add( "ForeignId" );
            analyticsSourcePersonHistoricalFieldNames.Add( "ForeignGuid" );
            analyticsSourcePersonHistoricalFieldNames.Add( "ForeignKey" );
            analyticsSourcePersonHistoricalFieldNames.Add( "Guid" );

            var currentDatabaseAttributeFields = dataTable.Columns.OfType<DataColumn>().Where( a =>
                !analyticsSourcePersonHistoricalFieldNames.Contains( a.ColumnName ) ).ToList();

            const string BooleanSqlFieldType = "bit";
            const string DateTimeSqlFieldType = "datetime";
            const string NumericSqlFieldType = "[decimal](29,4)";
            const string DefaultSqlFieldType = "nvarchar(250)";

            using ( var rockContext = new RockContext() )
            {
                // add any AttributeFields that aren't already fields on AnalyticsSourcePersonHistorical
                foreach ( var personAttribute in personAnalyticAttributes )
                {
                    var columnName = personAttribute.Key.RemoveSpecialCharacters();
                    var databaseColumn = currentDatabaseAttributeFields.Where( a => a.ColumnName == columnName ).FirstOrDefault();
                    var personAttributeValueFieldName = personAttribute.FieldType.Field.AttributeValueFieldName;
                    string sqlFieldType;
                    if ( personAttributeValueFieldName == "ValueAsDateTime" )
                    {
                        sqlFieldType = DateTimeSqlFieldType;
                    }
                    else if ( personAttributeValueFieldName == "ValueAsNumeric" )
                    {
                        sqlFieldType = NumericSqlFieldType;
                    }
                    else if ( personAttributeValueFieldName == "ValueAsBoolean" )
                    {
                        sqlFieldType = BooleanSqlFieldType;
                    }
                    else
                    {
                        sqlFieldType = DefaultSqlFieldType;
                    }

                    string addColumnSQL = $"ALTER TABLE [AnalyticsSourcePersonHistorical] ADD [{columnName}] {sqlFieldType} null";

                    string dropColumnSql = $"ALTER TABLE [AnalyticsSourcePersonHistorical] DROP COLUMN [{columnName}]";

                    if ( databaseColumn == null )
                    {
                        // doesn't exist as a field on the AnalyticsSourcePersonHistorical table, so create it
                        _personJobStats.ColumnsAdded++;
                        rockContext.Database.ExecuteSqlCommand( addColumnSQL );
                    }
                    else
                    {
                        // it does exist as a field on the AnalyticsSourcePersonHistorical table, but make sure the datatype is correct
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
                            _personJobStats.ColumnsModified++;
                            rockContext.Database.ExecuteSqlCommand( dropColumnSql );
                            rockContext.Database.ExecuteSqlCommand( addColumnSQL );
                        }
                    }
                }

                // remove any AnalyticsSourcePersonHistorical attribute fields that aren't attributes on Person
                var personAttributeColumnNames = personAnalyticAttributes.Select( a => a.Key.RemoveSpecialCharacters() ).ToList();
                foreach ( var databaseAttributeField in currentDatabaseAttributeFields )
                {
                    if ( !personAttributeColumnNames.Contains( databaseAttributeField.ColumnName ) )
                    {
                        var dropColumnSql = $"ALTER TABLE [AnalyticsSourcePersonHistorical] DROP COLUMN [{databaseAttributeField.ColumnName}]";

                        _personJobStats.ColumnsRemoved++;
                        rockContext.Database.ExecuteSqlCommand( dropColumnSql );
                    }
                }

                // refresh the view definitions just in case the schema changed
                // NOTE: Order is important!
                rockContext.Database.ExecuteSqlCommand( "exec sp_refreshview [AnalyticsDimPersonHistorical]" );
                rockContext.Database.ExecuteSqlCommand( "exec sp_refreshview [AnalyticsDimPersonCurrent]" );
                rockContext.Database.ExecuteSqlCommand( "exec sp_refreshview [AnalyticsDimFamilyHeadOfHousehold]" );
            }
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
                MarkFamilyAsHistoryUsingAttributeValues( familyAnalyticAttributes );

                // run the main spAnalytics_ETL_Family stored proc to take care of all the non-attribute related data
                var etlResult = DbService.GetDataTable( "EXEC [dbo].[spAnalytics_ETL_Family]", CommandType.Text, null );
                if ( etlResult.Rows.Count == 1 )
                {
                    _familyJobStats.RowsInserted = etlResult.Rows[0]["RowsInserted"] as int? ?? 0;
                    _familyJobStats.RowsUpdated = etlResult.Rows[0]["RowsUpdated"] as int? ?? 0;
                }

                // finish up by updating Attribute Values in the Analytic tables for attributes 
                UpdateFamilyAttributeValues( familyAnalyticAttributes );
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
                        this._familyJobStats.RowsMarkedAsHistory += DbService.ExecuteCommand( markAsHistorySQL, System.Data.CommandType.Text, parameters );

                        _sqlLogs.Add( parameters.Select( a => $"/* {a.Key} = '{a.Value}' */" ).ToList().AsDelimited( "\n" ) + markAsHistorySQL );
                    }
                }
            }
        }

        /// <summary>
        /// Updates Attribute Values for Family Analytics
        /// </summary>
        /// <param name="familyAnalyticAttributes">The family analytic attributes.</param>
        /// <returns></returns>
        private void UpdateFamilyAttributeValues( List<AttributeCache> familyAnalyticAttributes )
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
                            _familyJobStats.AttributeFieldsUpdated += DbService.ExecuteCommand( updateSql, System.Data.CommandType.Text, parameters );

                            _sqlLogs.Add( parameters.Select( a => $"/* {a.Key} = '{a.Value}' */" ).ToList().AsDelimited( "\n" ) + updateSql );
                        }
                    }
                }
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
                        _familyJobStats.ColumnsAdded++;
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
                            _familyJobStats.ColumnsModified++;
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

                        _familyJobStats.ColumnsRemoved++;
                        rockContext.Database.ExecuteSqlCommand( dropColumnSql );
                    }
                }

                // refresh the view definitions just in case the schema changed
                // NOTE: Order is important!
                rockContext.Database.ExecuteSqlCommand( "exec sp_refreshview [AnalyticsDimFamilyHistorical]" );
                rockContext.Database.ExecuteSqlCommand( "exec sp_refreshview [AnalyticsDimFamilyCurrent]" );
            }
        }

        #endregion Family Analytics

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
        private struct JobStats
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
            /// Gets or sets the attribute fields updated.
            /// </summary>
            /// <value>
            /// The attribute fields updated.
            /// </value>
            public int AttributeFieldsUpdated { get; set; }
        }

        #endregion
    }
}
