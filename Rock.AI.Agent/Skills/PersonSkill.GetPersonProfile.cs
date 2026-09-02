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

using System.ComponentModel;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class PersonSkill
{
    #region Tool(s)

    [Description( "Returns a comprehensive profile for a single person, including contact details, demographics, household, and key insights." )]
    [AgentPurpose( "Retrieves the complete profile of a person." )]
    [AgentPurpose( "Serves as the primary entry point for gaining insights into an individual." )]
    [AgentToolGuid( "2142A382-6AB2-0995-4480-69B641AE2CDC" )]
    public AgentToolResult GetPersonProfile( string personIdKey )
    {
        if ( personIdKey.IsNullOrWhiteSpace() )
        {
            return Error( "The personIdKey is required." );
        }

        var person = new PersonService( AgentRequestContext.RockContext ).Get( personIdKey );

        if ( person == null )
        {
            return Error( "No person could be found with the provided personIdKey." );
        }

        var profileResult = GetPrimaryPersonResult( person );
        var family = person.GetFamily();

        PopulatePhoneNumbers( profileResult, person );
        PopulateAddresses( profileResult, family );
        PopulateSpouse( profileResult, person );
        PopulateAdults( profileResult, family );
        PopulateChildren( profileResult, family );

        // Add Attributes
        person.LoadAttributes();
        profileResult.AttributeValues = person.GetAttributeValueResults( AgentRequestContext ).ToList();

        // Add Known Relationships
        var groupMemberService = new GroupMemberService( AgentRequestContext.RockContext );
        var knownRelationshipOwnerRoleGuid = Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER.AsGuid();

        // Get the known relationship group for the person (if any)
        var knownRelationshipGroup = groupMemberService.Queryable()
                        .Where( m =>
                            m.PersonId == person.Id &&
                            m.GroupRole.Guid == knownRelationshipOwnerRoleGuid )
                        .Select( m => m.Group )
                        .FirstOrDefault();

        if ( knownRelationshipGroup != null )
        {
            // Get the members of the known relationship group
            profileResult.KnownRelationships = knownRelationshipGroup.Members
                .Where( m => m.PersonId != person.Id )
                .Select( m => new GroupMemberResult
                {
                    Role = new KeyNameResult { Id = m.GroupRoleId, Guid = m.GroupRole.Guid, Name = m.GroupRole.Name },
                    Person = new PersonResult { Id = person.Id, FirstName = m.Person.FirstName, LastName = m.Person.LastName, NickName = m.Person.NickName, Suffix = m.Person.SuffixValue != null ? m.Person.SuffixValue.Value : null }
                } )
                .ToList();
        }

        // Add latest notes
        var currentPersonId = AgentRequestContext.CurrentPerson?.Id ?? 0;
        var personEntityTypeId = EntityTypeCache.GetId<Rock.Model.Person>().Value;

        profileResult.Notes = new NoteService( AgentRequestContext.RockContext ).Queryable()
            .Where( n =>
                n.NoteType.EntityTypeId == personEntityTypeId
                && n.EntityId == person.Id
                && ( n.IsPrivateNote == false || n.CreatedByPersonAlias.PersonId == currentPersonId ) )
            .OrderByDescending( n => n.CreatedDateTime )
            .Take( 20 )
            .ToList()
            .Where( x => x.IsAuthorized( Rock.Security.Authorization.VIEW, AgentRequestContext.CurrentPerson ) )
            .Select( x => new NoteResult
            {
                Id = x.Id,
                Caption = x.Caption,
                Text = x.Text,
                Author = x.CreatedByPersonAlias != null ? new PersonResult { Id = x.CreatedByPersonAlias.Person.Id, NickName = x.CreatedByPersonAlias.Person.NickName, LastName = x.CreatedByPersonAlias.Person.LastName } : null,
                CreatedDateTime = x.CreatedDateTime
            } )
            .Take( 5 )
            .ToList();

        // Add latest prayer requests
        profileResult.PrayerRequests = new PrayerRequestService( AgentRequestContext.RockContext ).Queryable()
            .Where( pr =>
                pr.RequestedByPersonAlias != null &&
                pr.RequestedByPersonAlias.PersonId == person.Id )
            .Select( pr => new PrayerRequestResult
            {
                Id = pr.Id,
                Text = pr.Text,
                IsApproved = pr.IsApproved,
                IsUrgent = pr.IsUrgent,
                IsPublic = pr.IsPublic,
                Category = new KeyNameResult { Id = pr.Category.Id, Guid = pr.Category.Guid, Name = pr.Category.Name },
                PrayerCount = pr.PrayerCount,
                EnteredDateTime = pr.EnteredDateTime
            } )
            .OrderByDescending( pr => pr.EnteredDateTime )
            .Take( 5 )
            .ToList();

        // Run security on profile result
        var securityCheckPassed = profileResult.Sanitize( AgentRequestContext );

        if ( !securityCheckPassed )
        {
            return Error( "You do not have permission to view this person's profile." );
        }

        return Success( profileResult )
            .WithReferenceRoute( AgentRequestContext, "View Profile", $"/person/{profileResult.IdKey}", false );
    }

    #endregion
}
