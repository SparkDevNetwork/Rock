using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    internal sealed partial class PersonSkill
    {
        #region Tool(s)

        /// <summary>
        /// Searches for persons by full name, returning a list of matching persons with their details.
        /// </summary>
        /// <returns></returns>
        [Description( "Does a full name sounds like search for the person." )]
        [AgentPurpose( "Searches for matching people by name. This will search by exact match as well as 'Sounds Like'." )]
        [AgentUsage( "Suffixes should be provide in the format of Sr., Jr., III, IV." )]
        [AgentUsage( "Only use this function if a full name is provided." )]
        [AgentToolReturnDescription( "A collection of summaries about the matched people. These are not full profiles. Call `GetPersonProfile` passing the personIdKey to get a person's full profile. " )]
        [AgentToolGuid( "03093B11-A02D-F794-4A5E-9AEA2C6EF63E" )]
        public RockToolResult SearchPerson( string fullName, int maxResults = 20, string campusIdKey = null )
        {
            if ( fullName == null || fullName.IsNullOrWhiteSpace() )
            {
                return RockToolResult.Error( "Full name is required." )
                    .WithInstructions( "The FullName parameter is required. You may also provide optional filters for CampusKey to filter by a specific campus and MaxResults to limit the results." );
            }

            // Get queryable with the metaphone and full name search.
            var searchQueryable = new PersonService( AgentRequestContext.RockContext )
                .GetSimilarPersons( fullName );

            // If the queryable is null that means that no individuals with that first name or last name were found
            if ( searchQueryable == null )
            {
                var specificErrorInstructions = string.Empty;

                // Provide some special instructions for senior and junior as these can commonly be used by voice ai.
                if ( fullName.EndsWith( "Senior" ) )
                {
                    specificErrorInstructions = "Please retry providing the suffix of Sr. instead of Senior";
                }

                if ( fullName.EndsWith( "Junior" ) )
                {
                    specificErrorInstructions = "Please retry providing the suffix of Jr. instead of Junior";
                }

                return RockToolResult.Error( "Could not find anyone with the name provided." )
                    .WithInstructions( "Could not find anyone with the name provided. You must now call SearchPersonPartial with a modified search term. Provide only the first two characters of the first name and three characters of the last name. Example: If the term was 'ted decker' pass 'te dec' to SearchPersonPartial. You must describe the results as possible matches." );
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
                .WithInstructions( "This data represents results that match the search query. These are both exact matches and those that are similar based on metaphone sounds like. All results should be displayed, even if they don't match exactly what was provided." )
                .WithReferenceRoute( AgentRequestContext.RockRequestContext, "Additional Search Options", $"/Person/Search/name/?SearchTerm={fullName}" )
                .WithMetadata( meta );
        }

        #endregion
    }
}
