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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Updates the built in Baptisms metric to count the previous full month.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 339, "17.8" )]
    public class FixBaptismsMetricDateRange : Migration
    {
        #region Constants

        /// <summary>
        /// The Guid of the built in Baptisms metric.
        /// </summary>
        private const string BaptismsMetricGuid = "8B63D9D5-A82D-49D4-9AED-2EDBCF60FDEE";

        /// <summary>
        /// The Baptisms metric SQL as set by the v17.0 rollup (UpdateMetricsDateComponentUp).
        /// This is the text found on current installs.
        /// </summary>
        private const string ShippedSourceSql = @"DECLARE @STARTDATE DATETIME= DATEADD(mm, DATEDIFF(mm, 0, dbo.RockGetDate()), 0)
DECLARE @ENDDATE DATETIME = DATEADD(DAY, -1, DATEADD(mm, 1, @STARTDATE));
DECLARE @BaptismDateAttributeId INT = (SELECT Id FROM dbo.[Attribute] WHERE [Guid] = 'D42763FA-28E9-4A55-A25A-48998D7D7FEF')

SELECT COUNT(*) as Baptisms, p.PrimaryCampusId FROM Person p
JOIN dbo.[AttributeValue] av
ON p.[Id] = av.[EntityId]
WHERE av.[AttributeId] = @BaptismDateAttributeId
AND CONVERT(date, av.[ValueAsDateTime]) >= @STARTDATE
AND CONVERT(date, av.[ValueAsDateTime]) < @ENDDATE
GROUP BY ALL p.[PrimaryCampusId]";

        /// <summary>
        /// The Baptisms metric SQL as first seeded (v17.0 pre-alpha plugin migration 202),
        /// before the v17.0 rollup replaced it. Matched too so an install that still has it
        /// gets the fix.
        /// </summary>
        private const string SeededSourceSql = @"DECLARE @STARTDATE DATETIME= DATEADD(mm, DATEDIFF(mm, 0, GETDATE()), 0)
DECLARE @ENDDATE DATETIME = DATEADD(DAY, -1, DATEADD(mm, 1, @STARTDATE));
DECLARE @BaptismDateAttributeId INT = (SELECT Id FROM dbo.[Attribute] WHERE [Guid] = 'D42763FA-28E9-4A55-A25A-48998D7D7FEF')

SELECT COUNT(*) as Baptisms, p.PrimaryCampusId FROM Person p
JOIN dbo.[AttributeValue] av
ON p.[Id] = av.[EntityId]
WHERE av.[AttributeId] = @BaptismDateAttributeId
AND av.[ValueAsDateTime] >= @STARTDATE
AND av.[ValueAsDateTime] < @ENDDATE
GROUP BY ALL p.[PrimaryCampusId]";

        /// <summary>
        /// The corrected Baptisms metric SQL. Only the two date lines differ from
        /// <see cref="ShippedSourceSql"/>: the range is now the previous full calendar
        /// month, from its first day up to (not including) the first day of the current month.
        /// </summary>
        private const string FixedSourceSql = @"DECLARE @ENDDATE DATETIME = DATEADD(mm, DATEDIFF(mm, 0, dbo.RockGetDate()), 0);
DECLARE @STARTDATE DATETIME = DATEADD(mm, -1, @ENDDATE);
DECLARE @BaptismDateAttributeId INT = (SELECT Id FROM dbo.[Attribute] WHERE [Guid] = 'D42763FA-28E9-4A55-A25A-48998D7D7FEF')

SELECT COUNT(*) as Baptisms, p.PrimaryCampusId FROM Person p
JOIN dbo.[AttributeValue] av
ON p.[Id] = av.[EntityId]
WHERE av.[AttributeId] = @BaptismDateAttributeId
AND CONVERT(date, av.[ValueAsDateTime]) >= @STARTDATE
AND CONVERT(date, av.[ValueAsDateTime]) < @ENDDATE
GROUP BY ALL p.[PrimaryCampusId]";

        #endregion Constants

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            /*
                10/29/2026 - MSE

                The Baptisms metric runs on the 1st of each month (Monthly Metric Schedule)
                but counted the current calendar month, so the scheduled run found almost no
                baptisms. Its end date was also the last day of the month with a "less than"
                test, so the last day of a month was never counted.

                The fixed SQL counts the previous full calendar month. This follows the other
                built in metrics: the weekly ones run on Monday and count the 7 days before
                the run, and the value is stamped with the run date. In the same way the
                value stamped on the 1st now holds the month that just ended.

                The SQL is only replaced while it still matches the shipped text, so a metric
                an administrator customized is left alone. Whitespace is ignored in the match
                (spaces, tabs, CR and LF are removed from both sides before comparing) because
                the stored text can have CRLF or LF line endings depending on how Rock was
                built. Existing metric values are not changed. Running it again does nothing
                because the SQL no longer matches the shipped text.

                Reason: The Baptisms metric counted the current month and skipped the last day of each month.
            */
            var fixedSql = FixedSourceSql.Replace( "'", "''" );
            var shippedSql = ShippedSourceSql.Replace( "'", "''" );
            var seededSql = SeededSourceSql.Replace( "'", "''" );

            Sql( $@"
DECLARE @FixedSql NVARCHAR(MAX) = N'{fixedSql}';
DECLARE @ShippedSql NVARCHAR(MAX) = N'{shippedSql}';
DECLARE @SeededSql NVARCHAR(MAX) = N'{seededSql}';

UPDATE [Metric]
SET [SourceSql] = @FixedSql
WHERE [Guid] = '{BaptismsMetricGuid}'
    AND REPLACE( REPLACE( REPLACE( REPLACE( [SourceSql], CHAR(13), '' ), CHAR(10), '' ), CHAR(9), '' ), ' ', '' ) IN (
        REPLACE( REPLACE( REPLACE( REPLACE( @ShippedSql, CHAR(13), '' ), CHAR(10), '' ), CHAR(9), '' ), ' ', '' ),
        REPLACE( REPLACE( REPLACE( REPLACE( @SeededSql, CHAR(13), '' ), CHAR(10), '' ), CHAR(9), '' ), ' ', '' )
    );" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }
    }
}
