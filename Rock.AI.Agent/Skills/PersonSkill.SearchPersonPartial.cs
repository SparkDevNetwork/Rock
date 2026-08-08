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

namespace Rock.AI.Agent.Skills;

internal sealed partial class PersonSkill
{
    #region Tool(s)

    [AgentPurpose( "Searches for matching people by a partial first name or last name." )]
    [AgentUsage( "Use this for partial name searches including searches for only last names." )]
    [AgentToolExample( "t decker would search the database for people who's first name starts with t and last name starts with decker." )]
    [AgentToolReturnDescription( "A collection of summaries about the matched people. These are not full profiles. Call `GetPersonProfile` passing the personIdKey to get a person's full profile." )]
    [Description( "Does a name search based on a partial search (e.g. 't dec')." )]
    [AgentToolGuid( "873AFC46-1872-999F-4E6C-94409654F6BC" )]
    public AgentToolResult SearchPersonPartial(
        string searchPattern,
        int maxResults = 20,
        string campusIdKey = null )
    {
        if ( searchPattern == null || searchPattern.IsNullOrWhiteSpace() )
        {
            return Error( "Search pattern is required." )
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
                return Error( "Invalid CampusIdKey provided." );
            }

            // Confirm that the campusId is valid and filter the search results.
            var campus = CampusCache.Get( campusId.Value );

            if ( campus == null )
            {
                return Error( "Invalid CampusIdKey provided." );
            }

            searchQueryable = ( IOrderedQueryable<Model.Person> ) searchQueryable
                .Where( p => p.PrimaryCampusId == campusId.Value );
        }

        // Get results
        var results = searchQueryable
            .Select( p => new PersonDetailResult
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
                IncludeProfileLink = true
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

        return Success( results )
            .WithInstructions( "This data represents results that match the search query. All results should be displayed, even if they don't match exactly what was provided." )
            .WithReferenceRoute( AgentRequestContext, "Additional Search Options", $"/Person/Search/name/?SearchTerm={searchPattern}", true )
            .WithMetadata( meta );
    }

    #endregion
}
