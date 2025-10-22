using System;
using System.ComponentModel;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.Core.Geography.Classes;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    internal sealed partial class PersonSkill
    {
        #region Tool(s)

        [Description( "Returns a comprehensive profile for a single person, including contact details, demographics, household, and key insights." )]
        [AgentPurpose( "Retrieves the complete profile of a person." )]
        [AgentPurpose( "Serves as the primary entry point for gaining insights into an individual." )]
        [AgentToolGuid( "2142A382-6AB2-0995-4480-69B641AE2CDC" )]
        public RockToolResult GetPersonProfile( string personIdKey )
        {
            if ( personIdKey.IsNullOrWhiteSpace() )
            {
                return RockToolResult.Error( "The personIdKey is required." );
            }

            var person = new PersonService( AgentRequestContext.RockContext ).Get( personIdKey );

            if ( person == null )
            {
                return RockToolResult.Error( "No person could be found with the provided personIdKey." );
            }

            // Get the request context
            var requestContext = AgentRequestContext.RockRequestContext;
            var currentPerson = requestContext?.CurrentPerson;

            var profileResult = new PersonResult( AgentRequestContext );

            profileResult.Id = person.Id;
            profileResult.FirstName = person.FirstName;
            profileResult.NickName = person.NickName;
            profileResult.LastName = person.LastName;
            profileResult.MiddleName = person.MiddleName;
            profileResult.Suffix = person.SuffixValue != null ? person.SuffixValue.Value : null;
            profileResult.Age = person.Age;
            profileResult.AgeClassification = person.AgeClassification;
            profileResult.Gender = person.Gender;
            profileResult.BirthMonth = person.BirthMonth;
            profileResult.BirthDay = person.BirthDay;
            profileResult.BirthYear = person.BirthYear;
            profileResult.AnniversaryDate = person.AnniversaryDate;
            profileResult.GraduationYear = person.GraduationYear;
            profileResult.MaritalStatus = person.MaritalStatusValue != null ? person.MaritalStatusValue.Value : null;
            profileResult.PhotoId = person.PhotoId;
            profileResult.PrimaryFamilyId = person.PrimaryFamilyId;
            profileResult.PrimaryFamilyId = person.PrimaryFamilyId;
            profileResult.MaritalStatusGuid = person.MaritalStatusValue != null ? person.MaritalStatusValue.Guid : Guid.Empty;
            profileResult.RecordTypeValueId = person.RecordTypeValueId;
            profileResult.RecordStatus = person.RecordStatusValue != null ? person.RecordStatusValue.Value : null;
            profileResult.ConnectionStatus = person.ConnectionStatusValue != null ? person.ConnectionStatusValue.Value : null;
            profileResult.Email = person.Email;
            profileResult.MaritalStatus = person.MaritalStatusValue != null ? person.MaritalStatusValue.Value : null;
            profileResult.Campus = person.PrimaryCampus != null ? new KeyNameResult { Id = person.PrimaryCampus.Id, Name = person.PrimaryCampus.Name } : null;
            profileResult.IncludePublicProfile = true;

            profileResult.PreviousLastNames = person.GetPreviousNames()
                .Select( p => p.LastName )
                .ToList();

            var family = person.GetFamily();

            // Add phone numbers
            profileResult.PhoneNumbers = person.PhoneNumbers
                .Select( n => new PhoneNumberResult
                {
                    IsUnlisted = n.IsUnlisted,
                    PhoneNumber = n.NumberFormatted,
                    PhoneType = new KeyNameResult
                    {
                        Id = n.NumberTypeValueId ?? 0,
                        Name = n.NumberTypeValue != null ? n.NumberTypeValue.Value : string.Empty
                    },
                    IsMessagingEnabled = n.IsMessagingEnabled
                } ).ToList();

            // Add addresses
            profileResult.Addresses = family
                .GroupLocations.Select( l => new LocationResult
                {
                    Street1 = l.Location.Street1,
                    Street2 = l.Location.Street2,
                    City = l.Location.City,
                    State = l.Location.State,
                    PostalCode = l.Location.PostalCode,
                    Country = l.Location.Country,
                    LocationType = l.GroupLocationTypeValue != null ? l.GroupLocationTypeValue.Value : string.Empty,
                    IsMailingAddress = l.IsMailingLocation,
                    IsMappedLocation = l.IsMappedLocation,
                    GeographyPoint = ( l.Location.Latitude.HasValue && l.Location.Longitude.HasValue ) ? new GeographyPoint( l.Location.Latitude.Value, l.Location.Longitude.Value ) : null
                } ).ToList();

            // Add adults
            profileResult.AdultsInFamily = family.Members.Where( m => m.Person.AgeClassification == AgeClassification.Adult )
                .Select( m => new PersonResult
                {
                    Id = m.Person.Id,
                    FirstName = m.Person.FirstName,
                    NickName = m.Person.NickName,
                    PhotoId = m.Person.PhotoId,
                    LastName = m.Person.LastName,
                    Age = m.Person.Age,
                    Suffix = m.Person.SuffixValue != null ? m.Person.SuffixValue.Value : null
                } ).ToList();

            // Add children
            profileResult.ChildrenInFamily = family.Members.Where( m => m.Person.AgeClassification == AgeClassification.Child )
                .Select( m => new PersonResult
                {
                    Id = m.Person.Id,
                    FirstName = m.Person.FirstName,
                    NickName = m.Person.NickName,
                    PhotoId = m.Person.PhotoId,
                    LastName = m.Person.LastName,
                    Age = m.Person.Age,
                    Suffix = m.Person.SuffixValue != null ? m.Person.SuffixValue.Value : null
                } ).ToList();

            // Add spouse
            var spouse = person.GetSpouse();

            if ( spouse != null )
            {
                profileResult.Spouse = new PersonResult
                {
                    Id = spouse.Id,
                    FirstName = spouse.FirstName,
                    NickName = spouse.NickName,
                    PhotoId = spouse.PhotoId,
                    Email = spouse.Email,
                    LastName = spouse.LastName,
                    Age = spouse.Age,
                    Suffix = spouse.SuffixValue != null ? spouse.SuffixValue.Value : null
                };
            }

            // Add Attributes
            person.LoadAttributes();
            profileResult.Attributes = person.AttributeValues
                        .Where( v => v.Value != null && v.Value.Value != null & v.Value.Value != string.Empty )
                        .Where( v => person.Attributes[v.Key].IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson ) )
                        .Select( a => new AttributeResult
                        {
                            Id = a.Value.AttributeId,
                            Key = a.Key,
                            Value = a.Value.PersistedTextValue,
                            Category = a.Value.AttributeCategoryIds.Select( cId => CategoryCache.Get( cId ) ).Where( c => c != null ).Select( c => c.Name ).FirstOrDefault()
                        } )
                        .ToList();

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
                        Role = new KeyNameResult { Id = m.GroupRoleId, Name = m.GroupRole.Name },
                        Person = new PersonResult { Id = person.Id, FirstName = m.Person.FirstName, LastName = m.Person.LastName, NickName = m.Person.NickName, Suffix = m.Person.SuffixValue != null ? m.Person.SuffixValue.Value : null }
                    } )
                    .ToList();
            }

            // Add latest notes
            var currentPersonId = currentPerson != null ? currentPerson.Id : 0;
            var personEntityTypeId = EntityTypeCache.GetId<Rock.Model.Person>().Value;

            profileResult.Notes = new NoteService( AgentRequestContext.RockContext ).Queryable()
                .Where( n =>
                    n.NoteType.EntityTypeId == personEntityTypeId
                    && n.EntityId == person.Id
                    && ( n.IsPrivateNote == false || n.CreatedByPersonAlias.PersonId == currentPersonId ) )
                .OrderByDescending( n => n.CreatedDateTime )
                .Take( 20 )
                .ToList()
                .Where( x => x.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson ) )
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
                    Category = new KeyNameResult { Id = pr.Category.Id, Name = pr.Category.Name },
                    PrayerCount = pr.PrayerCount,
                    EnteredDateTime = pr.EnteredDateTime
                } )
                .OrderByDescending( pr => pr.EnteredDateTime )
                .Take( 5 )
                .ToList();

            // Run security on profile result
            var securityCheckPassed = profileResult.SanitizeForSecurity( currentPerson );

            if ( !securityCheckPassed )
            {
                return RockToolResult.Error( "You do not have permission to view this person's profile." );
            }

            return RockToolResult.Success( profileResult )
                .WithReferenceRoute( requestContext, "View Profile", $"/person/{profileResult.IdKey}", false );
        }

        #endregion
    }
}
