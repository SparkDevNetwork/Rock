using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Rock.Data;
using Rock.Reporting;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// Job to take care of schema changes ( dynamic Attribute Value Fields ) and data updates to AnalyticsDimPerson
    /// </summary>
    /// <seealso cref="Quartz.IJob" />
    [DisallowConcurrentExecution]
    public class ProcessAnalyticsDimPerson : IJob
    {
        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public ProcessAnalyticsDimPerson()
        {
        }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute( IJobExecutionContext context )
        {
            List<EntityField> analyticsSourcePersonHistoricalFields = EntityHelper.GetEntityFields( typeof( Rock.Model.AnalyticsSourcePersonHistorical ), false, false );

            var personAttributes = EntityHelper.GetEntityFields( typeof( Rock.Model.Person ) )
                .Where( a => a.FieldKind == FieldKind.Attribute && a.AttributeGuid.HasValue )
                .Select( a => AttributeCache.Read( a.AttributeGuid.Value ) )
                .Where( a => a != null )
                .ToList();

            // Ensure that the Schema of AnalyticsSourcePersonHistorical matches the current fields for Attributes that are marked as IsAnalytic
            UpdateAnalyticsSourcePersonHistoricalSchema( analyticsSourcePersonHistoricalFields, personAttributes );

            var populateETLScript = GetPopulateETLScript( analyticsSourcePersonHistoricalFields, personAttributes );

            // TODO
        }

        /// <summary>
        /// Gets the populate etl script.
        /// </summary>
        /// <param name="analyticsSourcePersonHistoricalFields">The analytics source person historical fields.</param>
        /// <param name="personAttributes">The person attributes.</param>
        /// <returns></returns>
        private string GetPopulateETLScript( List<EntityField> analyticsSourcePersonHistoricalFields, List<AttributeCache> personAttributes )
        {
            // columns that should be considered when determining if a new History record is needed
            List<string> historyHashColumns = new List<string>();
            foreach ( var analyticsSourcePersonHistoricalProperty in analyticsSourcePersonHistoricalFields.Where( a => a.PropertyInfo.GetCustomAttribute<AnalyticHistoryFieldAttribute>() != null ) )
            {
                historyHashColumns.Add( analyticsSourcePersonHistoricalProperty.Name );
            }

            List<string> populateAttributeValueINSERTClauses = new List<string>();
            List<string> populateAttributeValueSELECTClauses = new List<string>();
            List<string> populateAttributeValueFROMClauses = new List<string>();

            var analyticSpecificColumns = new string[] { "Id", "HistoryHash", "PersonKey", "PersonId", "CurrentRowIndicator", "EffectiveDate", "ExpireDate", "PrimaryFamilyId", "BirthDateKey", "Guid" };

            List<string> populatePersonValueSELECTClauses = analyticsSourcePersonHistoricalFields
                .Select( a => a.Name )
                .Where( a => !analyticSpecificColumns.Contains( a ) ).OrderBy( a => a ).ToList();

            List<string> populatePersonValueFROMClauses = new List<string>( populatePersonValueSELECTClauses );

            const int maxAttributeValueLength = 250;

            using ( var rockContext = new RockContext() )
            {
                // add any AttributeFields that aren't already fields on AnalyticsSourcePersonHistorical
                // TODO: Limit to only personAttributes where 'IsAnalytic'
                foreach ( var personAttribute in personAttributes )
                {
                    var columnName = string.Format( "{0}", personAttribute.Key.RemoveSpecialCharacters() );
                    var personAttributeValueFieldName = personAttribute.FieldType.Field.AttributeValueFieldName;
                    if ( true ) // personAttribute.IsAnalytic && personAttribute.IsAnalyticHistory )
                    {
                        historyHashColumns.Add( columnName );
                    }

                    // each SELECT clause should look something like: attribute_1071.ValueAsDateTime as [attribute_YouthVolunteerApplication]
                    string populateAttributeValueSELECTClause = string.Format(
                        "av{0}.{1} as [{2}]",
                        personAttribute.Id,
                        personAttributeValueFieldName,
                        columnName
                        );

                    populateAttributeValueSELECTClauses.Add( populateAttributeValueSELECTClause );

                    populateAttributeValueINSERTClauses.Add( columnName );
                    string lengthCondition = personAttributeValueFieldName == "Value"
                        ? string.Format( "AND len(av{1}.Value) <= {0}", maxAttributeValueLength, personAttribute.Id )
                        : null;

                    string populateAttributeValueFROMClause = string.Format(
                        "LEFT OUTER JOIN AttributeValue av{1} ON av{1}.EntityId = p.Id  AND av1167.AttributeId = {1} {2}",
                        personAttributeValueFieldName, // {0}
                        personAttribute.Id, // {1}
                        lengthCondition // {2}
                        );

                    populateAttributeValueFROMClauses.Add( populateAttributeValueFROMClause );
                }

                // the date that the ETL ran
                DateTime etlDate = RockDateTime.Today;

                // magic date for indicating 'no expire date'
                DateTime maxExpireDate = new DateTime( 9999, 1, 1 );

                string currentEffectiveDatePrefix = etlDate.ToString( "yyyyMMdd_" );

                string processINSERTScript = @"
INSERT INTO [dbo].[AnalyticsSourcePersonHistorical] (
        [PersonKey],
        [PersonId],
        [CurrentRowIndicator],
        [EffectiveDate],
        [ExpireDate],
        [PrimaryFamilyId],
        [BirthDateKey],
" + populatePersonValueSELECTClauses.Select( a => string.Format( "        [{0}]", a ) ).ToList().AsDelimited( ",\n" ) + @",
        [Guid],
";

                // add INSERT columns for the AttributeValue Fields
                processINSERTScript += populateAttributeValueINSERTClauses.Select( a => string.Format( "        [{0}]", a ) ).ToList().AsDelimited( ",\n" );

                processINSERTScript += @"
)";

                string selectSQL = @"
    SELECT 
        CONCAT (
            '" + currentEffectiveDatePrefix + @"'
            ,p.Id
            ) [PersonKey],
        p.Id [PersonId],
        1 [CurrentRowIndicator],
        @EtlDate [EffectiveDate],
        @MaxExpireDate [ExpireDate],
        family.GroupId [PrimaryFamilyId],
        convert(INT, (convert(CHAR(8), DateFromParts(BirthYear, BirthMonth, BirthDay), 112))) [BirthDateKey],
" + populatePersonValueFROMClauses.Select( a => string.Format( "        [{0}]", a ) ).ToList().AsDelimited( ",\n" ) + @",
        NEWID() [Guid],
";

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

                processINSERTScript += selectSQL;

                processINSERTScript += @"
WHERE p.Id NOT IN (
            SELECT PersonId
            FROM [AnalyticsSourcePersonHistorical]
            WHERE CurrentRowIndicator = 1
            )";


                // build the CTE which is used for both the "Mark as History" and "UPDATE" sripts
                string withCTEScript = @";with cte1 as (" + selectSQL + @")
";


                string markAsHistoryScript = withCTEScript;
                markAsHistoryScript += @"UPDATE AnalyticsSourcePersonHistorical SET
        CurrentRowIndicator = 0,
        [ExpireDate] = @EtlDate
FROM AnalyticsSourcePersonHistorical asph
JOIN cte1 ON cte1.PersonId = asph.PersonId
WHERE asph.CurrentRowIndicator = 1 and (" + historyHashColumns.Select(a => $"asph.[{a}] != cte1.[{a}]").ToList().AsDelimited( " OR \n" ) + @")
AND asph.PersonId NOT IN ( -- Ensure that there isn't already a History Record for the current EtlDate 
    SELECT PersonId
    FROM AnalyticsSourcePersonHistorical x
    WHERE CurrentRowIndicator = 0
        AND [ExpireDate] = @EtlDate
    )

";


                string updateETLScript = withCTEScript;

                updateETLScript += @"UPDATE AnalyticsSourcePersonHistorical SET 
";
                updateETLScript += @"
        [PrimaryFamilyId] = cte1.[PrimaryFamilyId],
        [BirthDateKey] = cte1.[BirthDateKey],
";
                updateETLScript += populatePersonValueSELECTClauses.Select( a => $"        [{a}] = cte1.[{a}]" ).ToList().AsDelimited( ",\n" );

                updateETLScript += @"
FROM AnalyticsSourcePersonHistorical asph
JOIN cte1 ON cte1.PersonId = asph.PersonId";

                // update [HistoryHash]
                string updateHistoryHashScript = @"
UPDATE AnalyticsSourcePersonHistorical
SET [HistoryHash] = CONVERT(VARCHAR(max), HASHBYTES('SHA2_512', (
                SELECT TOP 1 ";

                updateHistoryHashScript += historyHashColumns.Select( a => string.Format( "[{0}]", a ) ).ToList().AsDelimited( "," );

                updateHistoryHashScript += @"
                FROM AnalyticsSourcePersonHistorical
                WHERE PersonId = adp.PersonId
                FOR XML raw
                )), 2)
FROM AnalyticsSourcePersonHistorical adp
WHERE [HistoryHash] IS NULL";

                // TODO: Figure out how to ETL person, especially attributes. 

                string populateETLScript = @"DECLARE @EtlDateTime DATETIME = SysDateTime()
    , @RowsInserted INT = NULL
DECLARE @CurrentEffectiveDatePrefix CHAR( 8 ) = ( convert( CHAR( 8 ), @EtlDateTime, 112 ) )
    ,@EtlDate DATE = convert( DATE, @EtlDateTime )
    , @MaxExpireDate DATE = DateFromParts( 9999, 1, 1 )";

                populateETLScript += $@"
                    
-- Move Records To History that have changes in any of fields that trigger history
{markAsHistoryScript}

-- Update existing records that have CurrentRowIndicator=1 to match what is in the live tables
{updateETLScript}

-- Insert new Person Records that aren't in there yet
{processINSERTScript}
                    
-- Update the HistoryHash (or maybe we don't need this anymore...?)
{updateHistoryHashScript}";

                return populateETLScript;
            }
        }

        /// <summary>
        /// Updates the analytics source person historical schema.
        /// </summary>
        /// <param name="analyticsSourcePersonHistoricalFields">The analytics source person historical fields.</param>
        /// <param name="personAttributes">The person attributes.</param>
        private void UpdateAnalyticsSourcePersonHistoricalSchema( List<EntityField> analyticsSourcePersonHistoricalFields, List<AttributeCache> personAttributes)
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

            const string booleanSqlFieldType = "bit";
            const string dateTimeSqlFieldType = "datetime";
            const string numericSqlFieldType = "[decimal](29,4)";
            const string defaultSqlFieldType = "nvarchar(250)";

            using ( var rockContext = new RockContext() )
            {
                // add any AttributeFields that aren't already fields on AnalyticsSourcePersonHistorical
                // TODO: Limit to only personAttributes where 'IsAnalytic'
                foreach ( var personAttribute in personAttributes )
                {
                    var columnName = string.Format( "{0}", personAttribute.Key.RemoveSpecialCharacters() );
                    var databaseColumn = currentDatabaseAttributeFields.Where( a => a.ColumnName == columnName ).FirstOrDefault();
                    var personAttributeValueFieldName = personAttribute.FieldType.Field.AttributeValueFieldName;
                    string sqlFieldType;
                    if ( personAttributeValueFieldName == "ValueAsDateTime" )
                    {
                        sqlFieldType = dateTimeSqlFieldType;
                    }
                    else if ( personAttributeValueFieldName == "ValueAsNumeric" )
                    {
                        sqlFieldType = numericSqlFieldType;
                    }
                    else if ( personAttributeValueFieldName == "ValueAsBoolean" )
                    {
                        sqlFieldType = booleanSqlFieldType;
                    }
                    else
                    {
                        sqlFieldType = defaultSqlFieldType;
                    }

                    string addColumnSQL = string.Format(
                            "ALTER TABLE [AnalyticsSourcePersonHistorical] ADD [{0}] {1} null",
                            columnName,
                            sqlFieldType );

                    string dropColumnSql = string.Format(
                            "ALTER TABLE [AnalyticsSourcePersonHistorical] DROP COLUMN [{0}]",
                            columnName );

                    if ( databaseColumn == null )
                    {
                        // doesn't exist as a field on the AnalyticsSourcePersonHistorical table, so create it
                        rockContext.Database.ExecuteSqlCommand( addColumnSQL );
                    }
                    else
                    {
                        // it does exist as a field on the AnalyticsSourcePersonHistorical table, but make sure the datatype is correct
                        bool dropCreate = false;

                        if ( databaseColumn.DataType == typeof( decimal ) )
                        {
                            dropCreate = sqlFieldType != numericSqlFieldType;
                        }
                        else if ( databaseColumn.DataType == typeof( DateTime ) )
                        {
                            dropCreate = sqlFieldType != dateTimeSqlFieldType;
                        }
                        else if ( databaseColumn.DataType == typeof( bool ) )
                        {
                            dropCreate = sqlFieldType != booleanSqlFieldType;
                        }
                        else
                        {
                            dropCreate = sqlFieldType != defaultSqlFieldType;
                        }

                        if ( dropCreate )
                        {
                            // the attribute fieldtype must have changed. Drop and recreate the column
                            rockContext.Database.ExecuteSqlCommand( dropColumnSql );
                            rockContext.Database.ExecuteSqlCommand( addColumnSQL );
                        }
                    }
                }

                // remove any AnalyticsSourcePersonHistorical attribute fields that aren't attributes on Person
                // TODO: Limit to only personAttributes where 'IsAnalytic'
                var personAttributeColumnNames = personAttributes.Select( a => string.Format( "{0}", a.Key.RemoveSpecialCharacters() ) ).ToList();
                foreach ( var databaseAttributeField in currentDatabaseAttributeFields )
                {
                    if ( !personAttributeColumnNames.Contains( databaseAttributeField.ColumnName ) )
                    {
                        var dropColumnSql = string.Format(
                            "ALTER TABLE [AnalyticsSourcePersonHistorical] DROP COLUMN [{0}]",
                            databaseAttributeField.ColumnName );

                        rockContext.Database.ExecuteSqlCommand( dropColumnSql );
                    }
                }
            }
        }
    }
}
