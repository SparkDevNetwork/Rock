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
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Dynamic.Core;

using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.Core.Geography.Classes;
using Rock.Model;
using Rock.SystemGuid;
using Rock.SystemKey;

namespace Rock.AI.Agent.Skills;

/// <summary>
/// Provides data lookup and analytics functions focused on site activity in Rock RMS,
/// particularly person-centric website analytics such as page visits, grouped by site.
/// </summary>
[Description( "This skill provides a holistic view of a person’s profile, connections, and overall engagement." )]
[AgentUsage( "Use the SearchPerson function to retrieve a person's IdKey when one is required as a function parameter." )]
[AgentSkillGuid( "DD5FA7DD-3277-4C31-848D-285CD67AC7CA" )]
[EntityTypeGuid( "12E7BDEA-B67A-48D7-8D1E-245BF8E9B555" )]
internal sealed partial class PersonSkill : AgentSkillComponent
{
    #region Fields

    /// <summary>
    /// The logger for this instance.
    /// </summary>
    private readonly ILogger _logger;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="CmsSkill"/> class.
    /// </summary>
    /// <param name="logger">Logger for diagnostics and error reporting.</param>
    public PersonSkill( ILogger<PersonSkill> logger )
    {
        _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
    }

    #endregion

    #region Shared Methods

    /// <summary>
    /// Appends family members (children, adults, and spouse) to the search results.
    /// </summary>
    /// <param name="results"></param>
    /// <returns></returns>
    private List<PersonDetailResult> AppendExtendedProperties( List<PersonDetailResult> results )
    {
        // Get configuration for the family roles and marital status
        var childGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid();
        var adultGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid();

        var marriedMaritalStatusGuid = Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED.AsGuid();

        var isBibleStrictSpouse = Rock.Web.SystemSettings.GetValue( SystemSetting.BIBLE_STRICT_SPOUSE ).AsBoolean( true );

        // Get families members for the individuals in the search results
        var familyIds = results.Select( p => p.PrimaryFamilyId ).Distinct().ToList();

        var familyMembers = new GroupMemberService( AgentRequestContext.RockContext ).Queryable()
            .Where( m => familyIds.Contains( m.GroupId ) && m.GroupMemberStatus == GroupMemberStatus.Active )
            .Select( m => new
            {
                NickName = m.Person.NickName,
                LastName = m.Person.LastName,
                GroupRoleGuid = m.GroupRole.Guid,
                PersonId = m.Person.Id,
                FamilyId = m.GroupId,
                Gender = m.Person.Gender,
                AgeClassification = m.Person.AgeClassification,
                Age = m.Person.Age,
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
                                        .Select( m => new PersonResult { NickName = m.NickName, LastName = m.LastName, Id = m.PersonId, Suffix = m.Suffix, Age = m.Age, IncludeProfileLink = true } )
                                        .ToList();

            result.AdultsInFamily = familyMembers.Where( m => m.FamilyId == result.PrimaryFamilyId
                                            && m.GroupRoleGuid == adultGuid
                                            && m.PersonId != result.Id )
                                        .Select( m => new PersonResult { NickName = m.NickName, LastName = m.LastName, Id = m.PersonId, Suffix = m.Suffix, Age = m.Age, IncludeProfileLink = true } )
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
                                         .Select( m => new PersonDetailResult { NickName = m.NickName, LastName = m.LastName, Id = m.PersonId, Suffix = m.Suffix, Age = m.Age, IncludeProfileLink = true } )
                                         .FirstOrDefault();
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

    /// <summary>
    /// Gets the <see cref="PersonDetailResult"/> object for the primary person
    /// being queried.
    /// </summary>
    /// <param name="person">The person object that was originally queried for.</param>
    /// <returns>An instance of <see cref="PersonDetailResult"/> that represents the person.</returns>
    internal static PersonDetailResult GetPrimaryPersonResult( Model.Person person, bool publicProfile = false )
    {
        var personResult = new PersonDetailResult
        {
            Id = person.Id,
            FirstName = person.FirstName,
            NickName = person.NickName,
            LastName = person.LastName,
            MiddleName = person.MiddleName,
            Suffix = person.SuffixValue?.Value,
            Age = person.Age,
            AgeClassification = person.AgeClassification,
            Gender = person.Gender,
            BirthMonth = person.BirthMonth,
            BirthDay = person.BirthDay,
            BirthYear = person.BirthYear,
            AnniversaryDate = person.AnniversaryDate,
            GraduationYear = person.GraduationYear,
            MaritalStatus = person.MaritalStatusValue?.Value,
            MaritalStatusGuid = person.MaritalStatusValue?.Guid,
            PhotoId = person.PhotoId,
            PrimaryFamilyId = person.PrimaryFamilyId,
            RecordTypeValueId = person.RecordTypeValueId,
            RecordStatus = person.RecordStatusValue?.Value,
            ConnectionStatus = person.ConnectionStatusValue?.Value,
            Email = person.Email,
            Campus = person.PrimaryCampus != null ? new KeyNameResult( person.PrimaryCampus.Id, person.PrimaryCampus.Name ) : null,
        };

        if ( !publicProfile )
        {
            personResult.IncludeProfileLink = true;
            personResult.PreviousLastNames = person.GetPreviousNames()
                .Select( p => p.LastName )
                .ToList();
        }

        return personResult;
    }

    internal static void PopulatePhoneNumbers( PersonDetailResult profileResult, Model.Person person )
    {
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
    }

    internal static void PopulateAddresses( PersonDetailResult profileResult, Model.Group family )
    {
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
    }

    internal static PersonResult GetFamilyMemberResult( Model.Person person, Action<Model.Person, PersonResult> setCustomValues = null )
    {
        var result = new PersonResult
        {
            Id = person.Id,
            FirstName = person.FirstName,
            NickName = person.NickName,
            PhotoId = person.PhotoId,
            LastName = person.LastName,
            Age = person.Age,
            Suffix = person.SuffixValue?.Value,
        };

        setCustomValues?.Invoke( person, result );

        return result;
    }

    internal static void PopulateAdults( PersonDetailResult profileResult, Model.Group family, Action<Model.Person, PersonResult> setCustomValues = null )
    {
        profileResult.AdultsInFamily = family.Members.Where( m => m.Person.AgeClassification == AgeClassification.Adult )
            .Select( m => GetFamilyMemberResult( m.Person, setCustomValues ) )
            .ToList();
    }

    internal static void PopulateChildren( PersonDetailResult profileResult, Model.Group family, Action<Model.Person, PersonResult> setCustomValues = null )
    {
        profileResult.ChildrenInFamily = family.Members.Where( m => m.Person.AgeClassification == AgeClassification.Child )
            .Select( m => GetFamilyMemberResult( m.Person, setCustomValues ) )
            .ToList();
    }

    internal static void PopulateSpouse( PersonDetailResult profileResult, Model.Person person, Action<Model.Person, PersonResult> setCustomValues = null )
    {
        var spouse = person.GetSpouse();

        if ( spouse == null )
        {
            return;
        }

        profileResult.Spouse = GetFamilyMemberResult( spouse, setCustomValues );
    }

    #endregion
}