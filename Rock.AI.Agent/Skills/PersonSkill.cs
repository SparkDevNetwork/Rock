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

using dotless.Core.Parser.Infrastructure;

using Microsoft.Azure.Amqp.Framing;
using Microsoft.CodeAnalysis.Semantics;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Mono.CSharp;

using OpenXmlPowerTools.HtmlToWml.CSS;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.PersonSkill;
using Rock.Data;
using Rock.Enums.AI.Agent;
using Rock.Model;
using Rock.SystemGuid;
using Rock.SystemKey;
using Rock.Utility;
using Rock.Web.Cache;

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
            "Results include the site type (web, mobile, tv), visited pages and visit counts. \r\n\r\n" +

            "🧭 Usage Guidance" +
            "The results are paginated (and the 'PageNumber' parameter is required.) \r\n" +
            "Do not call this function multiple times per site, unless necessary. It supports all-site aggregation when `siteId` is null." +

            "📋 Prerequisites:\r\n" +
            "This function depends on context set by `LookupSites`. Ensure it has been called first to set the site list.\r\n"
        )]
        [UserDescription( "Lists page visits for a specific person." )]
        [AgentFunctionGuid( "EFDBC338-CC1C-46D2-A7F6-7AE5081147AE" )]
        public RockToolResult ListPageVisitsForPerson( ListPageVisitsArguments arguments )
        {
            var errors = new List<string>();
            if ( arguments == null )
            {
                return RockToolResult.Error( "Options are required." );
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
                return RockToolResult.Error( errors );
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
                    return RockToolResult.NoData()
                        .WithMetadata( meta );
                }

                return RockToolResult.Success( rows )
                    .WithMetadata( meta );
            }
            catch ( Exception ex )
            {
                _logger.LogError( ex, "LookupSiteAnalytics failed for PersonId={PersonId}, SiteId={SiteId}", personId, siteId );
                return RockToolResult.Error( "Failed to retrieve site analytics. " + ex.Message );
            }
        }

        [KernelFunction]
        [Description(
            "🎯 Purpose:\r\n" +
            "Retrieves media views for a specific person, optionally filtered by date and/or site. \r\n\r\n" +

            "🧭 Usage Guidance" +
            "The results are paginated (and the 'PageNumber' parameter is required.)"
        )]
        [UserDescription( "Lists page visits for a specific person." )]
        [AgentFunctionGuid( "AB6CB80C-352A-F895-4233-09BA9DA69CCC" )]
        public RockToolResult ListMediaViewsForPerson( string personIdKey, int pageNumber = 1, DateTime? startDate = null, DateTime? endDate = null )
        {
            // Validate person
            var personId = IdHasher.Instance.GetId( personIdKey );
            if ( !personId.HasValue || personId <= 0 )
            {
                RockToolResult.Error( "The personIdKey is not valid. Please provide a valid value." );
            }

            // Validate date range
            if ( startDate.HasValue && endDate.HasValue && startDate > endDate )
            {
                RockToolResult.Error( "Invalid date range. Start date cannot be after end date." );
            }

            // Defaults: past year → now
            if ( !startDate.HasValue && !endDate.HasValue )
            {
                endDate = RockDateTime.Now;
                startDate = endDate.Value.AddYears( -1 );
            }
            else if ( startDate.HasValue && !endDate.HasValue )
            {
                endDate = RockDateTime.Now;
            }

            // Paging
            var basePageSize = 100;
            var offset = ( pageNumber - 1 ) * basePageSize;
            var take = basePageSize + 1; // N+1 to compute hasMore

            // Run query
            try
            {
                var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@PersonId", personId),
                    GetParameterValueOrDbNull("@StartDate", startDate),
                    GetParameterValueOrDbNull("@EndDate", endDate),
                    new SqlParameter("@PageSize",  take),    // request N+1
                    new SqlParameter("@OffsetRows", offset), // offset uses base size
                };

                var rows = _rockContext.Database
                    .SqlQuery<MediaViewResult>( _mediaViewsDataSql, parameters.ToArray() )
                    .ToList();

                var hasMore = rows.Count > basePageSize;
                if ( hasMore )
                {
                    rows.RemoveAt( rows.Count - 1 ); // drop lookahead row
                }

                var meta = new Dictionary<string, object>
                {
                    { "personKey", personIdKey },
                    { "startDate", startDate },
                    { "endDate", endDate },
                    { "pageNumber", pageNumber },
                    { "pageSize", basePageSize },
                    { "returnedRows", rows.Count },
                    { "hasMore", hasMore }
                };

                if ( !rows.Any() )
                {
                    return RockToolResult.NoData()
                        .WithMetadata( meta );
                }

                // Do some quick clean-up of the media data
                CleanMediaViews( rows );

                return RockToolResult.Success( rows )
                    .WithMetadata( meta );
            }
            catch ( Exception ex )
            {
                _logger.LogError( ex, "ListMediaViewsForPerson failed for PersonId={PersonId}", personId );
                return RockToolResult.Error( "Failed to retrieve media views. " + ex.Message );
            }
        }

        /// <summary>
        /// Searches for persons by full name, returning a list of matching persons with their details.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        [KernelFunction( "SearchPerson" )]
        [Description(
            "🎯 Purpose:\r\n" +
            "Searches for matching people by name. This will search by exact match as well as 'Sounds Like'. \\r\\n\" +" +
            "Suffixes should be provide in the format of Sr., Jr., III, IV." )]
        [AgentFunctionGuid( "03093B11-A02D-F794-4A5E-9AEA2C6EF63E" )]
        public RockToolResult SearchPerson( string fullName, int maxResults = 10, string campusIdKey = null )
        {
            if ( fullName == null || fullName.IsNullOrWhiteSpace() )
            {
                return RockToolResult.Error( "Full name is required.")
                    .WithInstructions( "The FullName parameter is required. You may also provide optional filters for CampusKey to filter by a specific campus and MaxResults to limit the results." );
            }

            // Get queryable with the metaphone and full name search.
            var searchQueryable = new PersonService( _rockContext )
                .GetSimilarPersons( fullName );

            // If the queryable is null that means that no individuals with that first name or last name were found
            if ( searchQueryable == null )
            {
                var specificErrorInstructions = string.Empty;

                // Provide some special instructions for senior and junior as these can commonly be used by voice ai.
                if ( fullName.EndsWith( "Senior") )
                {
                    specificErrorInstructions = "Please retry providing the suffix of Sr. instead of Senior";
                }

                if ( fullName.EndsWith( "Junior" ) )
                {
                    specificErrorInstructions = "Please retry providing the suffix of Jr. instead of Junior";
                }

                return RockToolResult.Error( "Could not find anyone with the name provided." )
                    .WithInstructions( "Could not find anyone with the name provided." );
            }
            
            // Append campus filter if provided
            if ( campusIdKey.IsNotNullOrWhiteSpace() )
            {
                var campusId = IdHasher.Instance.GetId( campusIdKey );

                if ( !campusId.HasValue || campusId <= 0 )
                {
                    return RockToolResult.Error( "Invalid CampusIdKey provided." );
                }

                // Confirm that the campusId is valid and filter the search results.
                var campus = CampusCache.Get( campusId.Value );

                if ( campus == null )
                {
                    return RockToolResult.Error( "Invalid CampusIdKey provided." );
                }

                searchQueryable = searchQueryable
                    .Where( p => p.PrimaryCampusId == campusId.Value );
            }

            // Get search results. Returning an anonymous type as some of the values needed will
            // not be returned to the client.
            var results = searchQueryable
                .Select( p => new PersonResult
                {
                    Id = p.Id,
                    FirstName = p.FirstName,
                    NickName = p.NickName,
                    LastName = p.LastName,
                    Suffix = p.SuffixValue != null ? p.SuffixValue.Value : "",
                    PrimaryFamilyId = p.PrimaryFamilyId,
                    AgeClassification = p.AgeClassification,
                    Campus = p.PrimaryCampus != null ? p.PrimaryCampus.Name : "",
                    CampusId = p.PrimaryCampusId,
                    PhotoId = p.PhotoId,
                    RecordTypeValueId = p.RecordTypeValueId,
                    ConnectionStatus = p.ConnectionStatusValue.Value,
                    RecordStatus = p.RecordStatusValue != null ? p.RecordStatusValue.Value : "",
                    MaritalStatusGuid = p.MaritalStatusValue != null ? p.MaritalStatusValue.Guid : Guid.Empty,
                    MaritalStatus = p.MaritalStatusValue != null ? p.MaritalStatusValue.Value : "",
                    Age = p.Age,
                    Gender = p.Gender
                } )
                .OrderBy( p => p.LastName )
                .ThenBy( p => p.NickName )
                .Take( maxResults + 1 )
                .ToList();

            // Provide indication of more results.
            var hasMore = results.Count > maxResults;

            if ( hasMore )
            {
                results.RemoveAt( results.Count - 1 );
            }

            results = AppendExtendedProperties( results );

            // Define meta data
            var meta = new Dictionary<string, object>
                {
                    { "returnedRows", results.Count },
                    { "hasMore", hasMore }
                };

            return RockToolResult.Success( results )
                .WithInstructions( "This data represents results that match the search query. These are both exact matches and those that are similar based on metaphone sounds like. All results should be displayed, even if they don't match exactly what was provided." )
                .WithMetadata( meta );
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Cleans up the media views by determining the medium and adjusting the viewing location URL.
        /// </summary>
        /// <param name="mediaViews"></param>
        private void CleanMediaViews( List<MediaViewResult> mediaViews )
        {
            foreach ( var media in mediaViews )
            {
                if ( media.ViewingLocationUrl.IsNullOrWhiteSpace() )
                {
                    continue;
                }

                if ( media.ViewingLocationUrl.StartsWith( "http", StringComparison.OrdinalIgnoreCase ) )
                {
                    // If the URL starts with http, we can assume it's a full URL.
                    media.Medium = "Web";
                }
                else
                {
                    // Otherwise, we assume it's mobile (e71a7c63-f510-434b-945b-f30c1c18df9d?CategoryGuid=24ae4f53-1bb8-4637-ae0e-5db0b06856b3).
                    var urlParts = media.ViewingLocationUrl.Split( '?' );

                    if ( urlParts.Length != 2 )
                    {
                        continue;
                    }

                    var page = PageCache.Get( urlParts[0].AsGuid() );

                    if ( page == null )
                    {
                        continue;
                    }

                    media.ViewingLocationUrl = $"{page.Site} - {page.PageTitle}";
                    media.Medium = "Mobile";
                }
            }
        }

        /// <summary>
        /// Appends family members (children, adults, and spouse) to the search results.
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        private List<PersonResult> AppendExtendedProperties( List<PersonResult> results )
        {
            // Get configuration for the family roles and marital status
            var childGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid();
            var adultGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid();

            var marriedMaritalStatusGuid = Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED.AsGuid();

            var isBibleStrictSpouse = Rock.Web.SystemSettings.GetValue( SystemSetting.BIBLE_STRICT_SPOUSE ).AsBoolean( true );

            // Get families members for the individuals in the search results
            var familyIds = results.Select( p => p.PrimaryFamilyId ).Distinct().ToList();

            var familyMembers = new GroupMemberService( _rockContext ).Queryable()
                .Where( m => familyIds.Contains( m.GroupId ) && m.GroupMemberStatus == GroupMemberStatus.Active )
                .Select( m => new
                {
                    FirstName = m.Person.NickName,
                    LastName = m.Person.LastName,
                    GroupRoleGuid = m.GroupRole.Guid,
                    PersonId = m.Person.Id,
                    FamilyId = m.GroupId,
                    Gender = m.Person.Gender,
                    MaritalStatusGuid = m.Person.MaritalStatusValue != null ? m.Person.MaritalStatusValue.Guid : Guid.Empty,
                    Suffix = m.Person.SuffixValue != null ? m.Person.SuffixValue.Value : string.Empty
                } )
                .ToList();

            // Append family members to the search results records
            foreach ( var result in results )
            {
                result.ChildrenInFamily = familyMembers.Where( m => m.FamilyId == result.PrimaryFamilyId
                                                && m.GroupRoleGuid == childGuid
                                                && m.PersonId != result.Id )
                                            .Select( m => new PersonResult { FirstName = m.FirstName, LastName = m.LastName, Id = m.PersonId, Suffix = m.Suffix } )
                                            .ToList();

                result.AdultsInFamily = familyMembers.Where( m => m.FamilyId == result.PrimaryFamilyId
                                                && m.GroupRoleGuid == adultGuid
                                                && m.PersonId != result.Id )
                                            .Select( m => new PersonResult { FirstName = m.FirstName, LastName = m.LastName, Id = m.PersonId, Suffix = m.Suffix } )
                                            .ToList();

                var personRoleInFamily = familyMembers.Where( m => m.FamilyId == result.PrimaryFamilyId && m.PersonId == result.Id )
                                            .Select( m => m.GroupRoleGuid )
                                            .FirstOrDefault();

                // Add spouse. This logic is copies from PersonService.GetSpouse()
                if ( personRoleInFamily == adultGuid && result.MaritalStatusGuid == marriedMaritalStatusGuid )
                {
                    result.Spouse = familyMembers.Where( m => m.FamilyId == result.PrimaryFamilyId
                                                && m.GroupRoleGuid == adultGuid
                                                && m.PersonId != result.Id
                                                && m.MaritalStatusGuid == marriedMaritalStatusGuid
                                                && ( !isBibleStrictSpouse || m.Gender != result.Gender || m.Gender == Gender.Unknown || result.Gender == Gender.Unknown ) )
                                             .Select( m => new PersonResult { FirstName = m.FirstName, LastName = m.LastName, Id = m.PersonId, Suffix = m.Suffix } )
                                             .FirstOrDefault();
                }

                if ( AgentRequestContext.AudienceType == AudienceType.Internal )
                {
                    result.ProfileUrl = $"{GlobalAttributesCache.Get().GetValue( "InternalApplicationRoot" )}/person/{result.PersonIdKey}";
                }
            }

            return results;
        }

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

        private const string _mediaViewsDataSql = @"
            SELECT 
                me.[Id] AS [MediaId]
                , i.[InteractionDateTime] AS [ViewDateTime]
                , i.[ChannelCustomIndexed1] AS [Medium]
                , CAST( ROUND( i.[InteractionLength], 0 ) AS int ) AS [PercentWatched]
                , me.[DurationSeconds] AS [MediaLengthInSeconds]
                , CAST( ROUND(me.[DurationSeconds] * i.[InteractionLength] / 100, 0) AS int) AS [DurationWatchedInSeconds]
                , ic.[Name] AS [MediaName]
                , i.[InteractionSummary] AS [ViewingLocationUrl]
            FROM [Interaction] i
                INNER JOIN [InteractionComponent] ic ON ic.[Id] = i.[InteractionComponentId]
                INNER JOIN [InteractionChannel] ich ON ich.[Id] = ic.[InteractionChannelId]
                INNER JOIN [PersonAlias] pa ON pa.[Id] = i.[PersonAliasId]
                INNER JOIN [Person] p ON p.[Id] = pa.[PersonId]
                INNER JOIN [MediaElement] me ON me.[Id] = ic.[EntityId]
            WHERE   
                ich.[Guid] = 'd5b9bdaf-6e52-40d5-8e74-4e23973df159'
                AND p.[Id] = @PersonId
                AND i.[InteractionDateTime] >= @StartDate
                AND i.[InteractionDateTime] <= @EndDate
            ORDER BY 
                i.[InteractionDateTime] DESC
                , i.[Id] DESC
            OFFSET @OffsetRows ROWS
            FETCH NEXT @PageSize ROWS ONLY
        ";

        #endregion
    }
}