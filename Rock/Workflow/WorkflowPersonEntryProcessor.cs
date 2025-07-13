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

using Rock.Attribute;
using Rock.Crm.RecordSource;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Rest.Controls;
using Rock.ViewModels.Workflow;
using Rock.Web.Cache;

namespace Rock.Workflow
{
    /// <summary>
    /// Handles processing the data from a workflow entry Person Entry form and
    /// updates the people and workflow attributes.
    /// </summary>
    internal class WorkflowPersonEntryProcessor
    {
        #region Fields

        /// <summary>
        /// The action that represents the workflow form.
        /// </summary>
        private readonly WorkflowAction _action;

        /// <summary>
        /// The database context to use when accessing and updating the database.
        /// </summary>
        private readonly RockContext _rockContext;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="WorkflowPersonEntryProcessor"/>.
        /// </summary>
        /// <param name="action">The action that represents the workflow form.</param>
        /// <param name="rockContext">The database context to use when accessing and updating the database.</param>
        public WorkflowPersonEntryProcessor( WorkflowAction action, RockContext rockContext )
        {
            if ( action == null )
            {
                throw new ArgumentNullException( nameof( action ) );
            }

            if ( rockContext == null )
            {
                throw new ArgumentNullException( nameof( rockContext ) );
            }

            _action = action;
            _rockContext = rockContext;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the workflow attribute entity.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <returns>The <see cref="WorkflowAction"/> or <see cref="WorkflowActivity"/> that owns the <paramref name="attribute"/>.</returns>
        private IHasAttributes GetWorkflowAttributeEntity( AttributeCache attribute )
        {
            IHasAttributes item = null;

            if ( attribute.EntityTypeId == _action.Activity.Workflow.TypeId )
            {
                item = _action.Activity.Workflow;
            }
            else if ( attribute.EntityTypeId == _action.Activity.TypeId )
            {
                item = _action.Activity;
            }

            return item;
        }

        /// <summary>
        /// Saves the person entry to attribute values.
        /// </summary>
        /// <param name="form">The form configuration data.</param>
        /// <param name="personEntryPersonId">The person entry person identifier.</param>
        /// <param name="personEntryPersonSpouseId">The person entry person spouse identifier.</param>
        /// <param name="primaryFamily">The primary family.</param>
        private void SavePersonEntryToAttributeValues( WorkflowActionFormCache form, int personEntryPersonId, int? personEntryPersonSpouseId, Group primaryFamily )
        {
            var workflow = _action.Activity.Workflow;
            var personAliasService = new PersonAliasService( _rockContext );

            var personEntryPersonAttribute = form.GetPersonEntryPersonAttribute( workflow );
            var personEntryFamilyAttribute = form.GetPersonEntryFamilyAttribute( workflow );
            var personEntrySpouseAttribute = form.GetPersonEntrySpouseAttribute( workflow );

            if ( personEntryPersonAttribute != null )
            {
                var item = GetWorkflowAttributeEntity( personEntryPersonAttribute );

                if ( item != null )
                {
                    var primaryAliasGuid = personAliasService.GetPrimaryAliasGuid( personEntryPersonId );
                    item.SetAttributeValue( personEntryPersonAttribute.Key, primaryAliasGuid );
                }
            }

            if ( personEntryFamilyAttribute != null )
            {
                var item = GetWorkflowAttributeEntity( personEntryFamilyAttribute );

                if ( item != null )
                {
                    item.SetAttributeValue( personEntryFamilyAttribute.Key, primaryFamily.Guid );
                }
            }

            if ( personEntrySpouseAttribute != null && personEntryPersonSpouseId.HasValue )
            {
                var item = GetWorkflowAttributeEntity( personEntrySpouseAttribute );

                if ( item != null )
                {
                    var primaryAliasGuid = personAliasService.GetPrimaryAliasGuid( personEntryPersonSpouseId.Value );
                    item.SetAttributeValue( personEntrySpouseAttribute.Key, primaryAliasGuid );
                }
            }
        }

        /// <summary>
        /// Updates the person from the values entered on the form.
        /// </summary>
        /// <param name="form">The form configuration data.</param>
        /// <param name="person">The person to be updated.</param>
        /// <param name="personBag">The bag that describes the changes to make to <paramref name="person"/>.</param>
        private void UpdatePersonFromEntryValues( WorkflowActionFormCache form, Person person, PersonBasicEditorBag personBag )
        {
            person.FirstName = personBag.FirstName;
            person.LastName = personBag.LastName;

            if ( personBag.NickName.IsNotNullOrWhiteSpace() )
            {
                person.NickName = personBag.NickName;
            }
            else if ( string.IsNullOrWhiteSpace( person.NickName ) )
            {
                // Only set the nickname in the database to the first name if the nickname is empty.
                // This prevents a differing nickname from being overwritten with the first name
                person.NickName = personBag.FirstName;
            }

            if ( form.PersonEntryEmailEntryOption != WorkflowActionFormPersonEntryOption.Hidden )
            {
                person.Email = personBag.Email;
            }

            if ( form.PersonEntryMobilePhoneEntryOption != WorkflowActionFormPersonEntryOption.Hidden )
            {
                var existingMobilePhone = person.GetPhoneNumber( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid(), _rockContext );

                var numberTypeMobile = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid(), _rockContext );
                var messagingEnabled = existingMobilePhone?.IsMessagingEnabled ?? true;
                var isUnlisted = existingMobilePhone?.IsUnlisted ?? false;

                person.UpdatePhoneNumber( numberTypeMobile.Id, personBag.MobilePhoneCountryCode, personBag.MobilePhoneNumber, messagingEnabled, isUnlisted, _rockContext );
            }

            if ( form.PersonEntryBirthdateEntryOption != WorkflowActionFormPersonEntryOption.Hidden && personBag.PersonBirthDate != null )
            {
                person.SetBirthDate( new DateTime( personBag.PersonBirthDate.Year, personBag.PersonBirthDate.Month, personBag.PersonBirthDate.Day ) );
            }

            if ( form.PersonEntryGenderEntryOption != WorkflowActionFormPersonEntryOption.Hidden )
            {
                person.Gender = personBag.PersonGender ?? Gender.Unknown;
            }
        }

        /// <summary>
        /// Creates or updates person from person form.
        /// </summary>
        /// <param name="form">The form configuration data.</param>
        /// <param name="existingPerson">The existing person to potentially update.</param>
        /// <param name="limitMatchToFamily">Limit matches to people in specified family</param>
        /// <param name="personBag">The person values.</param>
        /// <returns></returns>
        private Person CreateOrUpdatePersonFromEntryValues( WorkflowActionFormCache form, Person existingPerson, Group limitMatchToFamily, PersonBasicEditorBag personBag )
        {
            var personService = new PersonService( _rockContext );
            Person personEntryPerson;

            // If we have an existing person, just update and return.
            if ( existingPerson != null )
            {
                // Check if the values entered match the existing person, this determines
                // if we can use the existing person or not.
                var firstNameMatchesExistingFirstOrNickName = personBag.FirstName.Equals( existingPerson.FirstName, StringComparison.OrdinalIgnoreCase )
                        || personBag.FirstName.Equals( existingPerson.NickName, StringComparison.OrdinalIgnoreCase );
                var lastNameMatchesExistingLastName = personBag.LastName.Equals( existingPerson.LastName, StringComparison.OrdinalIgnoreCase );

                if ( !firstNameMatchesExistingFirstOrNickName || !lastNameMatchesExistingLastName )
                {
                    /*  10-07-2021 MDP

                    Special Logic if AutoFill CurrentPerson is enabled, but the Person Name fields were changed:

                    If the existing person (the one that used to auto-fill the fields) changed the FirstName or LastName PersonEditor,
                    then assume they mean they mean to create (or match) a new person. Note that if this happens, this matched or new person won't
                    be added to Ted Decker's family. PersonEntry isn't smart enough to figure that out and isn't intended to be a family editor. Here are a few examples
                    to clarify what this means:

                    Example 1: If Ted Decker is auto-filled because Ted Decker is logged in, but he changes the fields to Noah Decker, then we'll see if we have enough to make a match
                    to the existing Noah Decker. However, a match to the existing Noah Decker would need to match Noah's email and/or cell phone too, so it could easily create a new Noah Decker.

                    Example 2: If Ted Decker is auto-filled because Ted Decker is logged in, but he changes the fields to NewBaby Decker, we'll have to do the same thing as Example 1
                    even though Ted might be thinking he is adding his new baby to the family. So NewBaby Decker will probably be a new person in a new family.

                    Example 3: If Ted Decker is auto-filled because Ted Decker is logged in, but he changes the fields to Bob Smith (Ted's Neighbor), we also do the same thing as Example 1. However,
                    in this case, we are mostly doing what Ted expected to happen.

                    Summary. PersonEntry is not a family editor, it just collects data to match or create a person (and spouse if enabled).

                    Note: The logic for Spouse entry is slightly different. See notes below...

                    */

                    existingPerson = null;
                }

                // Update Person from person personValues.
                if ( existingPerson != null )
                {
                    personEntryPerson = personService.Get( existingPerson.Id );
                    UpdatePersonFromEntryValues( form, personEntryPerson, personBag );

                    return personEntryPerson;
                }
            }

            // Match or Create Person from personValues
            var personMatchQuery = new PersonService.PersonMatchQuery( personBag.FirstName, personBag.LastName, personBag.Email, $"{personBag.MobilePhoneCountryCode}{personBag.MobilePhoneNumber}" )
            {
                Gender = form.PersonEntryGenderEntryOption != WorkflowActionFormPersonEntryOption.Hidden
                    ? personBag.PersonGender
                    : null,
                BirthDate = form.PersonEntryBirthdateEntryOption != WorkflowActionFormPersonEntryOption.Hidden && personBag.PersonBirthDate != null
                    ? ( DateTime? ) new DateTime( personBag.PersonBirthDate.Year, personBag.PersonBirthDate.Month, personBag.PersonBirthDate.Day )
                    : null
            };

            bool updatePrimaryEmail = false;
            personEntryPerson = personService.FindPerson( personMatchQuery, updatePrimaryEmail );

            /*
            2020-11-06 MDP
            ** Special Logic when doing matches for Spouses**
            * See discussion on https://app.asana.com/0/0/1198971294248209/f for more details
            *
            If we are trying to find a matching person record for the Spouse, only consider matches that are in the same family as the primary person.
            If we find a matching person but they are in a different family, create a new person record instead.
            We don't want to risk causing two person records from different families to get married due to our matching logic.

            This avoids a problem such as these
            #1
            - Person1 fields match on Tom Miller (Existing Single guy)
            - Spouse fields match on Cindy Decker (married to Ted Decker)

            Instead of causing Tom Miller and the existing Cindy Decker to get married, create a new "duplicate" Cindy decker instead.

            #2
            - Person1 fields match on Tom Miller (Existing single guy)
            - Spouse fields match on Mary Smith (an unmarried female in another family)

            Even in case #2, create a duplicate Mary Smith instead.

            The exception is a situation like this
            #3
            - Person1 Fields match on Steve Rogers. Steve Rogers' family contains a Sally Rogers, but Sally isn't his spouse because
              one (or both) of them doesn't have a marital status of Married.
            - Spouse Fields match on Sally Rogers (in Steve Rogers' family)

            In case #3, use the matched Sally Rogers record, and change Steve and Sally's marital status to married

            Note that in the case of matching on an existing person that has a spouse, for example
            #4
            - Person1 Fields match Bill Hills.
            - Bill has a spouse named Jill Hills
            -

            In case #4, since Bill has a spouse, the data in the Spouse fields will be used to update Bill's spouse Jill Hills

             */

            if ( personEntryPerson != null && limitMatchToFamily != null )
            {
                if ( personEntryPerson.PrimaryFamilyId != limitMatchToFamily.Id )
                {
                    personEntryPerson = null;
                }
            }

            if ( personEntryPerson != null )
            {
                // if a match was found, update that person
                UpdatePersonFromEntryValues( form, personEntryPerson, personBag );
            }
            else
            {
                personEntryPerson = new Person();
                UpdatePersonFromEntryValues( form, personEntryPerson, personBag );
            }

            return personEntryPerson;
        }

        /// <summary>
        /// Gets the workflow form person entry values.
        /// </summary>
        /// <param name="currentPersonId">The current person identifier.</param>
        /// <param name="personEntryValues">The person entry values.</param>
        public void SetFormPersonEntryValues( int? currentPersonId, PersonEntryValuesBag personEntryValues )
        {
            var form = _action.ActionTypeCache?.WorkflowForm;

            if ( form == null || !form.AllowPersonEntry )
            {
                return;
            }

            _action.GetPersonEntryPeople( _rockContext, currentPersonId, out var existingPerson, out var existingSpouse );

            SetFormPersonEntryValues( form, currentPersonId, personEntryValues, existingPerson, existingSpouse );
        }

        /// <summary>
        /// Sets the form person entry values based on the provided
        /// <paramref name="personEntryValues"/> and existing person/spouse.
        /// </summary>
        /// <param name="form">The form configuration data.</param>
        /// <param name="currentPersonId">The identifier of the current person submitting this change.</param>
        /// <param name="personEntryValues">The values that describe the UI selections.</param>
        /// <param name="existingPerson">The existing person to potentially be updated.</param>
        /// <param name="existingSpouse">The existing spouse to potentially be updated.</param>
        internal void SetFormPersonEntryValues( WorkflowActionFormCache form, int? currentPersonId, PersonEntryValuesBag personEntryValues, Person existingPerson, Person existingSpouse )
        {
            if ( form == null )
            {
                throw new ArgumentNullException( nameof( form ) );
            }

            // If we have a person and the person entry was set to hide if known, then
            // we just store the current person and spouse values.
            if ( currentPersonId.HasValue && form.PersonEntryHideIfCurrentPersonKnown )
            {
                SavePersonEntryToAttributeValues( form, existingPerson.Id, existingSpouse?.Id, existingPerson.PrimaryFamily );
                return;
            }

            // Nothing provided by the client, so nothing more we can do.
            if ( personEntryValues == null )
            {
                return;
            }

            int? campusId = null;
            int? maritalStatusId = null;

            // Translate our Guid values into Id values.
            if ( personEntryValues.CampusGuid.HasValue )
            {
                campusId = CampusCache.Get( personEntryValues.CampusGuid.Value, _rockContext )?.Id;
            }

            if ( personEntryValues.MaritalStatusGuid.HasValue )
            {
                maritalStatusId = DefinedValueCache.Get( personEntryValues.MaritalStatusGuid.Value, _rockContext )?.Id;
            }

            var recordSourceValueId = RecordSourceHelper.GetSessionRecordSourceValueId()
                ?? form.PersonEntryRecordSourceValueId
                ?? DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_WORKFLOW.AsGuid() )?.Id;

            var personEntryPerson = CreateOrUpdatePersonFromEntryValues( form, existingPerson, null, personEntryValues.Person );
            if ( personEntryPerson.Id == 0 )
            {
                personEntryPerson.ConnectionStatusValueId = form.PersonEntryConnectionStatusValueId;
                personEntryPerson.RecordStatusValueId = form.PersonEntryRecordStatusValueId;
                personEntryPerson.RecordSourceValueId = recordSourceValueId;
                PersonService.SaveNewPerson( personEntryPerson, _rockContext, campusId );
            }

            // if we ended up matching an existing person, get their spouse as the selected spouse
            var matchedPersonsSpouse = personEntryPerson.GetSpouse();

            if ( matchedPersonsSpouse != null )
            {
                existingSpouse = matchedPersonsSpouse;
            }

            if ( form.PersonEntryMaritalStatusEntryOption != WorkflowActionFormPersonEntryOption.Hidden )
            {
                personEntryPerson.MaritalStatusValueId = maritalStatusId;
            }

            // save person 1 to database and re-fetch to get any newly created family, or other things that would happen on PreSave changes, etc
            _rockContext.SaveChanges();

            var personAliasService = new PersonAliasService( _rockContext );

            int personEntryPersonId = personEntryPerson.Id;
            int? personEntryPersonSpouseId = null;

            var personService = new PersonService( _rockContext );
            var primaryFamily = personService.GetSelect( personEntryPersonId, s => s.PrimaryFamily );

            if ( personEntryValues.Spouse != null )
            {
                var personEntryPersonSpouse = CreateOrUpdatePersonFromEntryValues( form, existingSpouse, primaryFamily, personEntryValues.Spouse );
                if ( personEntryPersonSpouse.Id == 0 )
                {
                    personEntryPersonSpouse.ConnectionStatusValueId = form.PersonEntryConnectionStatusValueId;
                    personEntryPersonSpouse.RecordStatusValueId = form.PersonEntryRecordStatusValueId;
                    personEntryPersonSpouse.RecordSourceValueId = recordSourceValueId;

                    // if adding/editing the 2nd Person (should normally be the spouse), set both people to selected Marital Status

                    /* 2020-11-16 MDP
                     *  It is possible that the Spouse label could be something other than spouse. So, we won't prevent them 
                     *  from changing the Marital status on the two people. However, this should be considered a mis-use of this feature.
                     *  Unexpected things could happen. 
                     *  
                     *  Example of what would happen if 'Daughter' was the label for 'Spouse':
                     *  Ted Decker is Person1, and Cindy Decker gets auto-filled as Person2. but since the label is 'Daughter', he changes
                     *  Cindy's information to Alex Decker's information, then sets Marital status to Single.
                     *  
                     *  This would result in Ted Decker no longer having Cindy as his spouse (and vice-versa). This was discussed on 2020-11-13
                     *  and it was decided we shouldn't do anything to prevent this type of problem.
                     
                     */
                    personEntryPersonSpouse.MaritalStatusValueId = maritalStatusId;
                    personEntryPerson.MaritalStatusValueId = maritalStatusId;

                    var groupRoleId = GroupTypeCache.GetFamilyGroupType( _rockContext ).Roles
                        .First( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() )
                        .Id;
                    PersonService.AddPersonToFamily( personEntryPersonSpouse, true, primaryFamily.Id, groupRoleId, _rockContext );
                }

                _rockContext.SaveChanges();

                personEntryPersonSpouseId = personEntryPersonSpouse.Id;
            }

            SavePersonEntryToAttributeValues( form, personEntryPerson.Id, personEntryPersonSpouseId, primaryFamily );

            if ( form.PersonEntryCampusIsVisible )
            {
                primaryFamily.CampusId = campusId;
            }

            if ( form.PersonEntryAddressEntryOption != WorkflowActionFormPersonEntryOption.Hidden && form.PersonEntryGroupLocationTypeValueId.HasValue && personEntryValues.Address != null )
            {
                // a Person should always have a PrimaryFamilyId, but check to make sure, just in case
                if ( primaryFamily != null )
                {
                    var groupLocationService = new GroupLocationService( _rockContext );
                    var familyLocation = primaryFamily.GroupLocations.Where( a => a.GroupLocationTypeValueId == form.PersonEntryGroupLocationTypeValueId.Value ).FirstOrDefault();

                    var newOrExistingLocation = new LocationService( _rockContext ).Get(
                            personEntryValues.Address.Street1,
                            string.Empty,
                            personEntryValues.Address.City,
                            personEntryValues.Address.State,
                            personEntryValues.Address.PostalCode,
                            personEntryValues.Address.Country );

                    if ( newOrExistingLocation != null )
                    {
                        if ( familyLocation == null )
                        {
                            familyLocation = new GroupLocation
                            {
                                GroupLocationTypeValueId = form.PersonEntryGroupLocationTypeValueId.Value,
                                GroupId = primaryFamily.Id,
                                IsMailingLocation = true,
                                IsMappedLocation = true
                            };

                            groupLocationService.Add( familyLocation );
                        }

                        if ( newOrExistingLocation.Id != familyLocation.LocationId )
                        {
                            familyLocation.LocationId = newOrExistingLocation.Id;
                        }
                    }
                }
            }

            _rockContext.SaveChanges();
        }

        #endregion
    }
}
