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
using System.Linq;

using Rock.Data;
using Rock.Media;
using Rock.Model;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Rest.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock
{
    /// <summary>
    /// Extension methods related to <see cref="MediaPlayerOptions"/>.
    /// </summary>
    internal static class PersonBasicEditorExtensions
    {
        /// <summary>
        /// TODO!!
        /// </summary>
        /// <param name="options">The options to be updated.</param>
        internal static void UpdatePersonFromBag( this PersonBasicEditorBag bag, Person person )
        {
            using ( var rockContext = new RockContext() )
            {

                bag.IfValidProperty( nameof( bag.FirstName ), () => person.FirstName = bag.FirstName );

                bag.IfValidProperty( nameof( bag.LastName ), () => person.LastName = bag.LastName );

                bag.IfValidProperty( nameof( bag.Email ), () => person.Email = bag.Email );

                bag.IfValidProperty( nameof( bag.MobilePhoneNumber ), () =>
                {
                    var existingMobilePhone = person.GetPhoneNumber( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );

                    var numberTypeMobile = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );
                    var isUnlisted = existingMobilePhone?.IsUnlisted ?? false;
                    var messagingEnabled = existingMobilePhone?.IsMessagingEnabled ?? true;
                    if ( bag.IsValidProperty( nameof( bag.IsMessagingEnabled ) ) )
                    {
                        messagingEnabled = bag.IsMessagingEnabled;
                    }                    

                    person.UpdatePhoneNumber( numberTypeMobile.Id, bag.MobilePhoneCountryCode, bag.MobilePhoneNumber, messagingEnabled, isUnlisted, rockContext );
                } );

                bag.IfValidProperty( nameof( bag.PersonBirthDate ), () => person.SetBirthDate( new DateTime( bag.PersonBirthDate.Year, bag.PersonBirthDate.Month, bag.PersonBirthDate.Day ) ) );

                bag.IfValidProperty( nameof( bag.PersonConnectionStatus ), () =>
                {
                    if ( bag.PersonConnectionStatus != null )
                    {
                        var dv = DefinedValueCache.Get( bag.PersonConnectionStatus.Value.AsGuid() );

                        person.ConnectionStatusValueId = dv?.Id;
                    }
                    else
                    {
                        person.ConnectionStatusValueId = null;
                    }
                } );

                bag.IfValidProperty( nameof( bag.PersonEthnicity ), () =>
                {
                    if ( bag.PersonEthnicity != null )
                    {
                        var dv = DefinedValueCache.Get( bag.PersonEthnicity.Value.AsGuid() );

                        person.EthnicityValueId = dv?.Id;
                    }
                    else
                    {
                        person.EthnicityValueId = null;
                    }
                } );

                bag.IfValidProperty( nameof( bag.PersonGender ), () => person.Gender = bag.PersonGender );

                bag.IfValidProperty( nameof( bag.PersonGradeOffset ), () => {
                    try
                    {
                        int offset = Int32.Parse( bag.PersonGradeOffset.Value );

                        if (offset >= 0)
                        {
                            person.GradeOffset = offset;
                        }
                    }
                    catch { }
                } );

                bag.IfValidProperty( nameof( bag.PersonMaritalStatus ), () =>
                {
                    if ( bag.PersonMaritalStatus != null )
                    {
                        var dv = DefinedValueCache.Get( bag.PersonMaritalStatus.Value.AsGuid() );

                        person.MaritalStatusValueId = dv?.Id;
                    }
                    else
                    {
                        person.MaritalStatusValueId = null;
                    }
                } );

                bag.IfValidProperty( nameof( bag.PersonGroupRole ), () =>
                {
                    var ageClass = AgeClassification.Unknown;

                    if (bag.PersonGroupRole != null && bag.PersonGroupRole.Value.AsGuid().Equals( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ) )
                    {
                        ageClass = AgeClassification.Adult;
                    }
                    else if (bag.PersonGroupRole != null && bag.PersonGroupRole.Value.AsGuid().Equals( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() ) )
                    {
                        ageClass = AgeClassification.Child;
                    }

                    person.AgeClassification = ageClass;
                } );

                bag.IfValidProperty( nameof( bag.PersonRace ), () =>
                {
                    if ( bag.PersonRace != null )
                    {
                        var dv = DefinedValueCache.Get( bag.PersonRace.Value.AsGuid() );

                        person.RaceValueId = dv?.Id;
                    }
                    else
                    {
                        person.RaceValueId = null;
                    }
                } );

                bag.IfValidProperty( nameof( bag.PersonSuffix ), () =>
                {
                    if ( bag.PersonSuffix != null )
                    {
                        var dv = DefinedValueCache.Get( bag.PersonSuffix.Value.AsGuid() );

                        person.SuffixValueId = dv?.Id;
                    }
                    else
                    {
                        person.SuffixValueId = null;
                    }
                } );

                bag.IfValidProperty( nameof( bag.PersonTitle ), () =>
                {
                    if ( bag.PersonTitle != null )
                    {
                        var dv = DefinedValueCache.Get( bag.PersonTitle.Value.AsGuid() );

                        person.TitleValueId = dv?.Id;
                    }
                    else
                    {
                        person.TitleValueId = null;
                    }
                } );
            }
        }

        /// TODO
        internal static PersonBasicEditorBag GetPersonBasicEditorBag( this Person person)
        {
            ListItemBag familyRole = null;

            if ( person.AgeClassification == AgeClassification.Adult )
            {
                familyRole = new ListItemBag
                {
                    Text = "Adult",
                    Value = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT
                };
            }
            else if (person.AgeClassification == AgeClassification.Child)
            {
                familyRole = new ListItemBag
                {
                    Text = "Child",
                    Value = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD
                };
            }

            var existingMobilePhone = person.GetPhoneNumber( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );

            return new PersonBasicEditorBag
            {
                FirstName = person.FirstName,
                LastName = person.LastName,
                PersonTitle = DefinedValueCache.Get( person.TitleValueId.HasValue ? person.TitleValueId.Value : 0 ).ToListItemBag(),
                PersonSuffix = DefinedValueCache.Get( person.SuffixValueId.HasValue ? person.SuffixValueId.Value : 0 ).ToListItemBag(),
                PersonMaritalStatus = DefinedValueCache.Get( person.MaritalStatusValueId.HasValue ? person.MaritalStatusValueId.Value : 0 ).ToListItemBag(),
                PersonGradeOffset = person.GradeOffset != null ? new ListItemBag
                {
                    Text = person.GradeFormatted,
                    Value = person.GradeOffset.ToString()
                } : null,
                PersonGroupRole = familyRole,
                PersonConnectionStatus = DefinedValueCache.Get( person.ConnectionStatusValueId.HasValue ? person.ConnectionStatusValueId.Value : 0 ).ToListItemBag(),
                PersonGender = person.Gender,
                PersonRace = DefinedValueCache.Get( person.RaceValueId.HasValue ? person.RaceValueId.Value : 0 ).ToListItemBag(),
                PersonEthnicity = DefinedValueCache.Get( person.EthnicityValueId.HasValue ? person.EthnicityValueId.Value : 0 ).ToListItemBag(),
                PersonBirthDate = person.BirthDate.HasValue ? new DatePartsPickerValueBag
                {
                    Day = person.BirthDate.Value.Day,
                    Month = person.BirthDate.Value.Month,
                    Year = person.BirthDate.Value.Year
                } : null,
                Email = person.Email,
                MobilePhoneNumber = existingMobilePhone != null ? existingMobilePhone.NumberFormatted : null,
                MobilePhoneCountryCode = existingMobilePhone != null ? existingMobilePhone.CountryCode : null,
                IsMessagingEnabled = existingMobilePhone != null ? existingMobilePhone.IsMessagingEnabled : false,
            };
        }
    }
}
