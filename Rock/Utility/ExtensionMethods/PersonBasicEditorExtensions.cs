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
        /// <param name="mediaElementId">The media element identifier.</param>
        /// <param name="mediaElementGuid">The media element unique identifier.</param>
        /// <param name="autoResumeInDays">The number of days back to look for an existing watch map to auto-resume from. Pass -1 to mean forever or 0 to disable.</param>
        /// <param name="combinePlayStatisticsInDays">The number of days back to look for an existing interaction to be updated. Pass -1 to mean forever or 0 to disable.</param>
        /// <param name="currentPerson">The person to use when searching for existing interactions.</param>
        /// <param name="personAliasId">If <paramref name="currentPerson"/> is <c>null</c> then this value will be used to optionally find an existing interaction.</param>
        internal static void UpdatePersonFromBag( this PersonBasicEditorBag bag, Person person )
        {
            //bag.IfValidProperty( nameof( bag.PersonGender ), () => person.Gender = bag.PersonGender );

            //person.gender = bag.gender

            //if ( person.AgeClassification == AgeClassification.Child )
            //{
            //    var childRoleId = GroupTypeCache.GetFamilyGroupType().Roles.FirstOrDefault( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() ).Id;
            //    this.PersonGroupRoleId = childRoleId;
            //}
            //else
            //{
            //    var adultRoleId = GroupTypeCache.GetFamilyGroupType().Roles.FirstOrDefault( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ).Id;
            //    this.PersonGroupRoleId = adultRoleId;
            //}
        }

        internal static PersonBasicEditorBag GetPersonBasicEditorBag( this Person person)
        {
            var familyRole = new ListItemBag
            {
                Text = "Adult",
                Value = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT
            };

            if (person.AgeClassification == AgeClassification.Child)
            {
                familyRole = new ListItemBag
                {
                    Text = "Child",
                    Value = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD
                };
            }

            var existingMobilePhone = person.GetPhoneNumber( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );

            return new PersonBasicEditorBag {
                FirstName = person.FirstName,
                LastName = person.LastName,
                PersonTitle = person.TitleValue != null ? new ListItemBag {
                    Text = person.TitleValue.Value,
                    Value = person.TitleValue.Guid.ToString()
                } : null,
                PersonSuffix = person.SuffixValue != null ? new ListItemBag
                {
                    Text = person.SuffixValue.Value,
                    Value = person.SuffixValue.Guid.ToString()
                } : null,
                PersonMaritalStatus = person.MaritalStatusValue != null ? new ListItemBag
                {
                    Text = person.MaritalStatusValue.Value,
                    Value = person.MaritalStatusValue.Guid.ToString()
                } : null,
                PersonGradeOffset = person.GradeOffset != null ? new ListItemBag
                {
                    Text = person.GradeFormatted,
                    Value = person.GradeOffset.ToString()
                } : null,
                PersonGroupRole = familyRole,
                PersonConnectionStatus = person.ConnectionStatusValue != null ? new ListItemBag
                {
                    Text = person.ConnectionStatusValue.Value,
                    Value = person.ConnectionStatusValue.Guid.ToString()
                } : null,
                PersonGender = person.Gender,
                PersonRace = person.RaceValue != null ? new ListItemBag
                {
                    Text = person.RaceValue.Value,
                    Value = person.RaceValue.Guid.ToString()
                } : null,
                PersonEthnicity = person.EthnicityValue != null ? new ListItemBag
                {
                    Text = person.EthnicityValue.Value,
                    Value = person.EthnicityValue.Guid.ToString()
                } : null,
                PersonBirthDate = person.BirthDate.HasValue ? new DatePartsPickerValueBag {
                    Day = person.BirthDate.Value.Day,
                    Month = person.BirthDate.Value.Month,
                    Year = person.BirthDate.Value.Year != DateTime.MinValue.Year ? person.BirthDate.Value.Year : 0
                } : null,
                Email = person.Email,
                MobilePhoneNumber = existingMobilePhone != null ? existingMobilePhone.NumberFormatted : null,
                MobilePhoneCountryCode = existingMobilePhone != null ? existingMobilePhone.CountryCode : null,
                IsMessagingEnabled = existingMobilePhone != null ? existingMobilePhone.IsMessagingEnabled : false,
            };
        }
    }
}
