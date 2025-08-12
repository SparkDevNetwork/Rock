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
using System.Data.SqlClient;
using System.Linq;

using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.PersonSkill;
using Rock.Data;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Utility;

namespace Rock.AI.Agent.Skills
{
    /// <summary>
    /// Provides data lookup and analytics functions focused on site activity in Rock RMS,
    /// particularly person-centric website analytics such as page visits, grouped by site.
    /// </summary>
    [AgentSkillGuid( "DD5FA7DD-3277-4C31-848D-285CD67AC7CA" )]
    [EntityTypeGuid( "12E7BDEA-B67A-48D7-8D1E-245BF8E9B555" )]
    internal sealed class PersonSkill : AgentSkillComponent
    {
        #region Fields

        private readonly RockContext _rockContext;
        private readonly ILogger<PersonSkill> _logger;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SiteSkill"/> class.
        /// </summary>
        /// <param name="rockContext">Rock data context used for database access.</param>
        /// <param name="logger">Logger for diagnostics and error reporting.</param>
        public PersonSkill( RockContext rockContext, ILogger<PersonSkill> logger )
        {
            _rockContext = rockContext ?? throw new ArgumentNullException( nameof( rockContext ) );
            _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
        }

        #endregion

        #region Agent Functions

        /// <summary>
        /// Retrieves website analytics (page visits) for a specific person, optionally filtered by date and/or site.
        /// Results are grouped by site and include visited pages with visit counts.
        /// </summary>
        /// <param name="arguments">Query parameters including person id, optional site id, and optional start/end dates.</param>
        /// <remarks>
        /// Requires <see cref="LookupSites"/> to have been called previously to populate
        /// session context <c>"site-list"</c> for guardrail enforcement.
        /// Defaults the date range to [now - 1 year, now] if neither start nor end are provided.
        /// </remarks>
        [KernelFunction]
        [Description(
            "🎯 Purpose:\r\n" +
            "Retrieves page visits for a specific person, optionally filtered by date and/or site. \r\n" +
            "Results include the site type (web, mobile, tv), visited pages and visit counts.\r\n" +
            "The results are paginated (and the 'PageNumber' parameter is required. \r\n\r\n" +

            "🛡️ Guardrails:\r\n" +
            "1. This function depends on context set by `LookupSites`. Ensure it has been called first to set the site list.\r\n" +
            "2. Do not call this function multiple times per site, unless necessary. It supports all-site aggregation when `siteId` is null."
        )]
        [AgentFunctionGuid( "EFDBC338-CC1C-46D2-A7F6-7AE5081147AE" )]
        public RockFunctionResult ListPageVisitsForPerson( ListPageVisitsArguments arguments )
        {
            var errors = new List<string>();
            if ( arguments == null )
            {
                return RockFunctionResult.Error( "Options are required." );
            }

            var start = arguments.StartDate;
            var end = arguments.EndDate;

            var personId = IdHasher.Instance.GetId( arguments.PersonKey );
            if ( !personId.HasValue || personId <= 0 )
            {
                errors.Add( "There was an invalid key provided for the person." );
            }

            var siteId = IdHasher.Instance.GetId( arguments.SiteKey );
            if ( siteId.HasValue && siteId.Value <= 0 )
            {
                errors.Add( "Invalid site ID. Provide a value greater than zero." );
            }

            if ( start.HasValue && end.HasValue && start > end )
            {
                errors.Add( "Invalid date range. Start date cannot be after end date." );
            }

            // Defaults: past year → now
            if ( !start.HasValue && !end.HasValue )
            {
                end = RockDateTime.Now;
                start = end.Value.AddYears( -1 );
            }
            else if ( start.HasValue && !end.HasValue )
            {
                end = RockDateTime.Now;
            }

            // Paging
            var pageNumber = Math.Max( 1, arguments.PageNumber );
            var basePageSize = 100;
            var offset = ( pageNumber - 1 ) * basePageSize;
            var take = basePageSize + 1; // N+1 to compute hasMore

            if ( errors.Count > 0 )
            {
                return RockFunctionResult.Error( string.Join( " ", errors ) );
            }

            try
            {
                var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@PersonId", personId),
                    GetParameterValueOrDbNull("@SiteId", siteId),
                    GetParameterValueOrDbNull("@StartDate", start),
                    GetParameterValueOrDbNull("@EndDate", end),
                    new SqlParameter("@PageSize",  take),    // request N+1
                    new SqlParameter("@OffsetRows", offset), // offset uses base size
                };

                var rows = _rockContext.Database
                    .SqlQuery<PageVisitResult>( _websiteDataSql, parameters.ToArray() )
                    .ToList();

                var hasMore = rows.Count > basePageSize;
                if ( hasMore )
                {
                    rows.RemoveAt( rows.Count - 1 ); // drop lookahead row
                }

                var meta = new Dictionary<string, object>
                {
                    { "personKey", arguments.PersonKey },
                    { "startDate", start },
                    { "endDate", end },
                    { "pageNumber", pageNumber },
                    { "pageSize", basePageSize },
                    { "returnedRows", rows.Count },
                    { "hasMore", hasMore }
                };

                if ( !rows.Any() )
                {
                    return RockFunctionResult.NoData( meta: meta );
                }

                return RockFunctionResult.Success( rows, meta: meta );
            }
            catch ( Exception ex )
            {
                _logger.LogError( ex, "LookupSiteAnalytics failed for PersonId={PersonId}, SiteId={SiteId}", personId, siteId );
                return RockFunctionResult.Error( "Failed to retrieve site analytics. " + ex.Message );
            }
        }

        /// <summary>
        /// Searches for persons by full name, returning a list of matching persons with their details.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        [AgentFunctionGuid( "03093B11-A02D-F794-4A5E-9AEA2C6EF63E" )]
        public RockFunctionResult SearchPerson( SearchPersonArguments options )
        {
            if ( options == null )
            {
                return RockFunctionResult.Error( "Options are required.", instructions: "The FullName parameter is required. You may also provide optional filters for CampusKey to filter by a specific campus and MaxResults to limit the results." );
            }

            if (options.FullName.IsNullOrWhiteSpace() )
            {
                return RockFunctionResult.Error( "Full name is required for search.", instructions: "The FullName parameter is required." );
            }

            var childGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid();
            var adultGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid();
            var familyGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid();

            var familyGroupMembers = new GroupMemberService( _rockContext ).Queryable()
                        .Where( m => m.Group.GroupType.Guid == familyGuid );

            var searchQueryable = new PersonService( _rockContext )
                .GetSimilarPersons( options.FullName );

            // Append campus filter if provided
            var campusIdFilter = options.CampusKey.IsNullOrWhiteSpace()
                ? null
                : IdHasher.Instance.GetId( options.CampusKey );

            if ( campusIdFilter.HasValue )
            {
                searchQueryable = searchQueryable
                    .Where( p => p.PrimaryCampusId == campusIdFilter.Value );
            }

            // Get search results
            var searchResults = searchQueryable
                .OrderBy( p => p.LastName )
                .ThenBy( p => p.NickName )
                .Take( options.MaxResults + 1 )
                .ToList()
                .Select( p => new SearchPersonResult
                {
                    PersonKey = p.IdKey,
                    FirstName = p.FirstName,
                    NickName = p.NickName,
                    LastName = p.LastName,
                    Suffix = p.SuffixValue?.Value ?? string.Empty,
                    AgeClassification = p.AgeClassification.ToString(),
                    Campus = p.GetCampus()?.Name ?? "",
                    ConnectionStatus = p.ConnectionStatusValue.Value ?? string.Empty,
                    RecordStatus = p.RecordStatusValue?.Value ?? string.Empty,
                    SpouseName = p.GetSpouse()?.FullName ?? string.Empty,

                    // Get list of child names
                    Children = familyGroupMembers
                        .Where( s => s.PersonId == p.Id && s.GroupRole.Guid == adultGuid )
                        .SelectMany( m => m.Group.Members )
                        .Where( m => m.GroupRole.Guid == childGuid )
                        .Select( m => new PersonResult { FirstName = m.Person.NickName, LastName = m.Person.LastName, PersonKey = m.Person.IdKey } )
                        .ToList(),

                    // Get list of parents names
                    Parents = familyGroupMembers
                        .Where( s => s.PersonId == p.Id && s.GroupRole.Guid == childGuid )
                        .SelectMany( m => m.Group.Members )
                        .Where( m => m.GroupRole.Guid == adultGuid )
                        .Select( m => new PersonResult { FirstName = m.Person.NickName, LastName = m.Person.LastName, PersonKey = m.Person.IdKey } )
                        .ToList()
                } )
                .ToList();

            var hasMore = searchResults.Count > options.MaxResults;

            if ( hasMore )
            {
                searchResults.RemoveAt( searchResults.Count - 1 );
            }

            var meta = new Dictionary<string, object>
                {
                    { "returnedRows", searchResults.Count },
                    { "hasMore", hasMore }
                };

            return RockFunctionResult.Success( searchResults, meta: meta );
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Creates a SQL parameter with the specified key and value, substituting <see cref="DBNull.Value"/> when the value is <c>null</c>.
        /// </summary>
        /// <param name="key">The parameter name (e.g., <c>@SiteId</c>).</param>
        /// <param name="value">The parameter value, or <c>null</c> to emit <see cref="DBNull.Value"/>.</param>
        /// <returns>A <see cref="SqlParameter"/> instance.</returns>
        private static SqlParameter GetParameterValueOrDbNull( string key, object value )
            => new SqlParameter( key, value ?? ( object ) DBNull.Value );

        #endregion

        #region SQL

        private const string _websiteDataSql = @"
;WITH Filtered AS (
    SELECT
        PageName = COALESCE(NULLIF(LTRIM(RTRIM(i.InteractionSummary)), ''), '(Unknown)'),
        i.InteractionDateTime,
        ich.ChannelEntityId AS SiteId
    FROM dbo.Interaction i
    INNER JOIN dbo.InteractionSession   AS inse  ON inse.Id  = i.InteractionSessionId
    INNER JOIN dbo.InteractionComponent AS icomp ON icomp.Id = i.InteractionComponentId
    INNER JOIN dbo.InteractionChannel   AS ich   ON ich.Id   = icomp.InteractionChannelId
    INNER JOIN dbo.PersonAlias          AS pa    ON pa.Id    = i.PersonAliasId
    WHERE (@SiteId IS NULL OR ich.ChannelEntityId = @SiteId)
      AND i.Operation = 'View'
      AND pa.PersonId = @PersonId
      AND (@StartDate IS NULL OR i.InteractionDateTime >= @StartDate)
      AND (@EndDate   IS NULL OR i.InteractionDateTime <= @EndDate)
),
Grouped AS (
    SELECT
        f.SiteId,
        f.PageName,
        VisitCount = COUNT(*),
        LastVisit  = MAX(f.InteractionDateTime)
    FROM Filtered f
    GROUP BY f.SiteId, f.PageName
)
SELECT
    g.SiteId,
    g.PageName,
    g.VisitCount,
    g.LastVisit
FROM Grouped g
ORDER BY
    g.LastVisit DESC,
    g.SiteId,
    g.PageName
OFFSET @OffsetRows ROWS
FETCH NEXT @PageSize ROWS ONLY
OPTION (RECOMPILE);";

        #endregion
    }
}