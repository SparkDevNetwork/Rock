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
using System.Linq;

using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Rest.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock
{
    /// <summary>
    /// Extension methods related to <see cref="PersonBasicEditorBag"/>. This allows you to create a <see cref="PersonBasicEditorBag"/> that
    /// you can send in to a PersonBasicEditor Obsidian control that it can use to pre-fill the form and edit the data of the <see cref="Person"/>
    /// the bag represents. Once you have the updated bag from the front end, you can update the <see cref="Person"/> with data from the
    /// bag and then manually save those changes to the database.
    /// </summary>
    internal static class PersonBasicEditorExtensions
    {
        /// <summary>
        /// Apply the valid properties of this bag to a given person. This does not save the changes to the database.
        /// </summary>
        /// <param name="bag">The <see cref="PersonBasicEditorBag"/> with data to apply to a person.</param>
        /// <param name="person">The <see cref="Person"/> you want to update with the bag's data.</param>
        /// <param name="rockContext"></param>
        internal static void UpdatePersonFromBag( this PersonBasicEditorBag bag, Person person, RockContext rockContext )
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
                    messagingEnabled = bag.IsMessagingEnabled ?? false;
                }

                person.UpdatePhoneNumber( numberTypeMobile.Id, bag.MobilePhoneCountryCode, bag.MobilePhoneNumber, messagingEnabled, isUnlisted, rockContext );
            } );

            bag.IfValidProperty( nameof( bag.PersonBirthDate ), () =>
            {
                if ( bag.PersonBirthDate != null )
                {
                    person.SetBirthDate( new DateTime( bag.PersonBirthDate.Year, bag.PersonBirthDate.Month, bag.PersonBirthDate.Day ) );
                }
                else
                {
                    person.SetBirthDate( null );
                }
            } );

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

            bag.IfValidProperty( nameof( bag.PersonGender ), () => person.Gender = bag.PersonGender ?? Gender.Unknown );

            bag.IfValidProperty( nameof( bag.PersonGradeOffset ), () =>
            {
                if ( bag.PersonGradeOffset == null )
                {
                    person.GradeOffset = null;
                    return;
                }

                try
                {
                    int offset = Int32.Parse( bag.PersonGradeOffset.Value );

                    if ( offset >= 0 )
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

            /*
                The original didn't actually apply changes to this, so right now we're still ignoring it.
                There are a couple reasons:
                1. We're not actually showing them the role. This value is generated based on AgeClassification.
                2. If they are part of multiple families, which one (or all of them?) get updated with this value?
            */
            //bag.IfValidProperty( nameof( bag.PersonGroupRole ), () => {} );

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

        /// <summary>
        /// Create a <see cref="PersonBasicEditorBag"/> that represents this <see cref="Person"/>.
        /// The bag is used by the PersonBasicEditor Obsidian control to edit the person's data.
        /// </summary>
        /// <param name="person">The <see cref="Person"/> you want the created <see cref="PersonBasicEditorBag"/> to represent.</param>
        internal static PersonBasicEditorBag GetPersonBasicEditorBag( this Person person )
        {
            ListItemBag familyRole = null;

            if ( person.AgeClassification == AgeClassification.Adult )
            {
                familyRole = new ListItemBag
                {
                    Text = GroupTypeCache.GetFamilyGroupType().Roles.FirstOrDefault( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ).Name,
                    Value = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT
                };
            }
            else if ( person.AgeClassification == AgeClassification.Child )
            {
                familyRole = new ListItemBag
                {
                    Text = GroupTypeCache.GetFamilyGroupType().Roles.FirstOrDefault( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() ).Name,
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
                ValidProperties = new List<string>
                {
                    ToCamelCase( nameof( PersonBasicEditorBag.FirstName ) ),
                    ToCamelCase( nameof( PersonBasicEditorBag.LastName ) ),
                    ToCamelCase( nameof( PersonBasicEditorBag.PersonTitle ) ),
                    ToCamelCase( nameof( PersonBasicEditorBag.PersonSuffix ) ),
                    ToCamelCase( nameof( PersonBasicEditorBag.PersonMaritalStatus ) ),
                    ToCamelCase( nameof( PersonBasicEditorBag.PersonGradeOffset ) ),
                    ToCamelCase( nameof( PersonBasicEditorBag.PersonGroupRole ) ),
                    ToCamelCase( nameof( PersonBasicEditorBag.PersonConnectionStatus ) ),
                    ToCamelCase( nameof( PersonBasicEditorBag.PersonGender ) ),
                    ToCamelCase( nameof( PersonBasicEditorBag.PersonRace ) ),
                    ToCamelCase( nameof( PersonBasicEditorBag.PersonEthnicity ) ),
                    ToCamelCase( nameof( PersonBasicEditorBag.PersonBirthDate ) ),
                    ToCamelCase( nameof( PersonBasicEditorBag.Email ) ),
                    ToCamelCase( nameof( PersonBasicEditorBag.MobilePhoneNumber ) ),
                    ToCamelCase( nameof( PersonBasicEditorBag.MobilePhoneCountryCode ) ),
                    ToCamelCase( nameof( PersonBasicEditorBag.IsMessagingEnabled ) )
                }
            };
        }

        private static string ToCamelCase( string str )
        {
            if ( str.IsNullOrWhiteSpace() )
            {
                return str;
            }

            return str.Substring( 0, 1 ).ToLower() + str.Substring( 1 );
        }
    }
}
