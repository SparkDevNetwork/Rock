using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
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
            var dataSet = DbService.GetDataSetSchema( "SELECT * FROM [AnalyticsDimPersonHistorical] where 1=0", System.Data.CommandType.Text, null );
            var dataTable = dataSet.Tables[0];
            var currentDatabaseAttributeFields = dataTable.Columns.OfType<DataColumn>().Where( a => a.ColumnName.StartsWith( "attribute_" ) );

            var personAttributes = EntityHelper.GetEntityFields( typeof( Rock.Model.Person ) )
                .Where( a => a.FieldKind == FieldKind.Attribute && a.AttributeGuid.HasValue )
                .Select( a => AttributeCache.Read( a.AttributeGuid.Value ) )
                .Where( a => a != null )
                .ToList();

            const string dateTimeSqlFieldType = "datetime";
            const string numericSqlFieldType = "[decimal](29,4)";
            const string defaultSqlFieldType = "nvarchar(max)";

            List<string> populateAttributeValueINSERTClauses = new List<string>();
            List<string> populateAttributeValueSELECTClauses = new List<string>();
            List<string> populateAttributeValueFROMClauses = new List<string>();

            using ( var rockContext = new RockContext() )
            {
                // add any AttributeFields that aren't already fields on AnalyticsDimPersonHistorical
                // TODO: Limit to only personAttributes where 'IsAnalytic'
                foreach ( var personAttribute in personAttributes )
                {
                    var columnName = string.Format( "attribute_{0}", personAttribute.Key.RemoveSpecialCharacters() );
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
                    else
                    {
                        sqlFieldType = defaultSqlFieldType;
                    }

                    // each SELECT clause should look something like: attribute_1071.ValueAsDateTime as [attribute_YouthVolunteerApplication]
                    string populateAttributeValueSELECTClause = string.Format(
                        "attribute_{0}.{1} as [{2}]",
                        personAttribute.Id,
                        personAttributeValueFieldName,
                        columnName
                        );

                    populateAttributeValueSELECTClauses.Add( populateAttributeValueSELECTClause );


                    populateAttributeValueINSERTClauses.Add( columnName );

                    // each FROM clause should look something like: OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1167) attribute_1167
                    string populateAttributeValueFROMClause = string.Format(
                        "OUTER APPLY ( SELECT TOP 1 av.{0} FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = {1}) attribute_{1}",
                        personAttributeValueFieldName,
                        personAttribute.Id
                        );

                    populateAttributeValueFROMClauses.Add( populateAttributeValueFROMClause );

                    string addColumnSQL = string.Format(
                            "ALTER TABLE [AnalyticsDimPersonHistorical] ADD [{0}] {1} null",
                            columnName,
                            sqlFieldType );

                    string dropColumnSql = string.Format(
                            "ALTER TABLE [AnalyticsDimPersonHistorical] DROP COLUMN [{0}]",
                            columnName );

                    if ( databaseColumn == null )
                    {
                        // doesn't exist as a field on the AnalyticsDimPersonHistorical table, so create it
                        rockContext.Database.ExecuteSqlCommand( addColumnSQL );
                    }
                    else
                    {
                        // it does exist as a field on the AnalyticsDimPersonHistorical table, but make sure the datatype is correct
                        bool dropCreate = false;

                        if ( databaseColumn.DataType == typeof( int ) )
                        {
                            dropCreate = sqlFieldType != numericSqlFieldType;
                        }
                        else if ( databaseColumn.DataType == typeof( DateTime ) )
                        {
                            dropCreate = sqlFieldType != dateTimeSqlFieldType;
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

                // remove any AnalyticsDimPersonHistorical attribute fields that aren't attributes on Person
                // TODO: Limit to only personAttributes where 'IsAnalytic'
                var personAttributeColumnNames = personAttributes.Select( a => string.Format( "attribute_{0}", a.Key.RemoveSpecialCharacters() ) ).ToList();
                foreach ( var databaseAttributeField in currentDatabaseAttributeFields )
                {
                    if ( !personAttributeColumnNames.Contains( databaseAttributeField.ColumnName ) )
                    {
                        var dropColumnSql = string.Format(
                            "ALTER TABLE [AnalyticsDimPersonHistorical] DROP COLUMN [{0}]",
                            databaseAttributeField.ColumnName );

                        rockContext.Database.ExecuteSqlCommand( dropColumnSql );
                    }
                }

                // the date that the ETL ran
                DateTime etlDate = RockDateTime.Today;

                // magic date for indicating 'no expire date'
                DateTime maxExpireDate = new DateTime( 9999, 1, 1 );

                string currentEffectiveDatePrefix = etlDate.ToString( "yyyyMMdd_" );


                string populateETLScript = @"
INSERT INTO [dbo].[AnalyticsDimPersonHistorical] (
        [PersonKey],
        [PersonId],
        [CurrentRowIndicator],
        [EffectiveDate],
        [ExpireDate],
        [PrimaryFamilyId],
        [RecordTypeValueId],
        [RecordStatusValueId],
        [RecordStatusLastModifiedDateTime],
        [RecordStatusReasonValueId],
        [ConnectionStatusValueId],
        [ReviewReasonValueId],
        [IsDeceased],
        [TitleValueId],
        [FirstName],
        [NickName],
        [MiddleName],
        [LastName],
        [SuffixValueId],
        [PhotoId],
        [BirthDay],
        [BirthMonth],
        [BirthYear],
        [Gender],
        [MaritalStatusValueId],
        [AnniversaryDate],
        [GraduationYear],
        [GivingGroupId],
        [GivingId],
        [GivingLeaderId],
        [Email],
        [IsEmailActive],
        [EmailNote],
        [EmailPreference],
        [ReviewReasonNote],
        [InactiveReasonNote],
        [SystemNote],
        [ViewedCount],
        [Guid],
";

                // add INSERT columns for the AttributeValue Fields
                populateETLScript += populateAttributeValueINSERTClauses.Select( a => string.Format( "        [{0}]", a ) ).ToList().AsDelimited( ",\n" );

                populateETLScript += @"
)
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
        p.RecordTypeValueId,
        RecordStatusValueId,
        RecordStatusLastModifiedDateTime,
        RecordStatusReasonValueId,
        ConnectionStatusValueId,
        ReviewReasonValueId,
        IsDeceased,
        TitleValueId,
        FirstName,
        NickName,
        MiddleName,
        LastName,
        SuffixValueId,
        PhotoId,
        BirthDay,
        BirthMonth,
        BirthYear,
        Gender,
        MaritalStatusValueId,
        AnniversaryDate,
        GraduationYear,
        GivingGroupId,
        GivingId,
        GivingLeaderId,
        Email,
        IsEmailActive,
        EmailNote,
        EmailPreference,
        ReviewReasonNote,
        InactiveReasonNote,
        SystemNote,
        ViewedCount,
        NEWID(),
";

                populateETLScript += populateAttributeValueSELECTClauses.Select( a => "        " + a ).ToList().AsDelimited( ",\n" );

                populateETLScript += @"
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

                // add the "OUTER APPLY..." AttributeValue FROM clauses
                populateETLScript += populateAttributeValueFROMClauses.Select( a => "        " + a ).ToList().AsDelimited( "\n" );

                populateETLScript += @"
WHERE p.Id NOT IN (
            SELECT PersonId
            FROM [AnalyticsDimPersonHistorical]
            WHERE CurrentRowIndicator = 1
            )";



                // TODO: Figure out how to ETL person, especially attributes. See Rock\database\Procedures\spAnalytics_ETL_Person.sql
            }
        }
    }
}
