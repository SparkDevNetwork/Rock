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

            using ( var rockContext = new RockContext() )
            {
                // add any AttributeFields that aren't already fields on AnalyticsDimPersonHistorical
                // TODO: Limit to only personAttributes where 'IsAnalytic'
                foreach ( var personAttribute in personAttributes )
                {
                    var columnName = string.Format( "attribute_{0}", personAttribute.Key.RemoveSpecialCharacters() );
                    var databaseColumn = currentDatabaseAttributeFields.Where( a => a.ColumnName == columnName ).FirstOrDefault();
                    var personAttributeFieldName = personAttribute.FieldType.Field.AttributeValueFieldName;
                    string sqlFieldType;
                    if ( personAttributeFieldName == "ValueAsDateTime" )
                    {
                        sqlFieldType = dateTimeSqlFieldType;
                    }
                    else if ( personAttributeFieldName == "ValueAsNumeric" )
                    {
                        sqlFieldType = numericSqlFieldType;
                    }
                    else
                    {
                        sqlFieldType = defaultSqlFieldType;
                    }

                    var addColumnSQL = string.Format(
                            "ALTER TABLE [AnalyticsDimPersonHistorical] ADD [{0}] {1} null",
                            columnName,
                            sqlFieldType );

                    var dropColumnSql = string.Format(
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

                        if ( dropCreate || true)
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
            }
        }
    }
}
