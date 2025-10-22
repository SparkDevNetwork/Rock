using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.Model;
using Rock.Net;
using Rock.SystemGuid;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    internal sealed partial class PersonSkill
    {
        #region Tool(s)

        /// <summary>
        /// Searches for a person using a partial name.
        /// </summary>
        /// <param name="searchPattern"></param>
        /// <param name="maxResults"></param>
        /// <param name="campusIdKey"></param>
        /// <returns></returns>
        [AgentPurpose( "Searches for matching people by a partial first name or last name." )]
        [AgentUsage( "Use this for partial name searches including searches for only last names." )]
        [AgentToolExample( "t decker would search the database for people who's first name starts with t and last name starts with decker." )]
        [AgentToolReturnDescription( "A collection of summaries about the matched people. These are not full profiles. Call `GetPersonProfile` passing the personIdKey to get a person's full profile." )]
        [Description( "Does a name search based on a partial search (e.g. 't dec')." )]
        [AgentToolGuid( "873AFC46-1872-999F-4E6C-94409654F6BC" )]
        public RockToolResult SearchPersonPartial( string searchPattern, int maxResults = 20, string campusIdKey = null )
        {
            if ( searchPattern == null || searchPattern.IsNullOrWhiteSpace() )
            {
                return RockToolResult.Error( "Search pattern is required." )
                    .WithInstructions( "The searchPattern parameter is required. You may also provide optional filters for CampusKey to filter by a specific campus and MaxResults to limit the results." );
            }

            var searchQueryable = new PersonService( AgentRequestContext.RockContext )
                .GetByFullNameOrdered( searchPattern, true, false, false, out _ );

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

                searchQueryable = ( IOrderedQueryable<Model.Person> ) searchQueryable
                    .Where( p => p.PrimaryCampusId == campusId.Value );
            }

            // Get results
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
                    Campus = p.PrimaryCampus != null ? new KeyNameResult { Id = p.PrimaryCampus.Id, Name = p.PrimaryCampus.Name } : null,
                    PhotoId = p.PhotoId,
                    RecordTypeValueId = p.RecordTypeValueId,
                    ConnectionStatus = p.ConnectionStatusValue.Value,
                    RecordStatus = p.RecordStatusValue != null ? p.RecordStatusValue.Value : "",
                    MaritalStatusGuid = p.MaritalStatusValue != null ? p.MaritalStatusValue.Guid : Guid.Empty,
                    MaritalStatus = p.MaritalStatusValue != null ? p.MaritalStatusValue.Value : "",
                    Age = p.Age,
                    Email = p.Email,
                    Gender = p.Gender,
                    IncludePublicProfile = true
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
                .WithInstructions( "This data represents results that match the search query. All results should be displayed, even if they don't match exactly what was provided." )
                .WithReferenceRoute( RockRequestContextAccessor.Current, "Additional Search Options", $"/Person/Search/name/?SearchTerm={searchPattern}" )
                .WithMetadata( meta );
        }

        #endregion
    }
}
