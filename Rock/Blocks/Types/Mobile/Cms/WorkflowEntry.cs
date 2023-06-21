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
using System.ComponentModel;
using System.Linq;

using Rock.Attribute;
using Rock.Common.Mobile;
using Rock.Common.Mobile.Blocks.WorkflowEntry;
using Rock.Common.Mobile.Enums;
using Rock.Data;
using Rock.Mobile;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Blocks.Types.Mobile.Cms
{
    /// <summary>
    /// Allows for filling out workflows from a mobile application.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Workflow Entry" )]
    [Category( "Mobile > Cms" )]
    [Description( "Allows for filling out workflows from a mobile application." )]
    [IconCssClass( "fa fa-gears" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    #region Block Attributes

    [WorkflowTypeField( "Workflow Type",
        Description = "The type of workflow to launch when viewing this.",
        IsRequired = false,
        Key = AttributeKeys.WorkflowType,
        Order = 0 )]

    [CustomDropdownListField( "Completion Action",
        description: "What action to perform when there is nothing left for the user to do.",
        listSource: "0^Show Message from Workflow,1^Show Completion Xaml,2^Redirect to Page",
        IsRequired = true,
        DefaultValue = "0",
        Key = AttributeKeys.CompletionAction,
        Order = 1 )]

    [CodeEditorField( "Completion Xaml",
        Description = "The XAML markup that will be used if the Completion Action is set to Show Completion Xaml. <span class='tip tip-lava'></span>",
        IsRequired = false,
        DefaultValue = "",
        Key = AttributeKeys.CompletionXaml,
        Order = 2 )]

    [LavaCommandsField( "Enabled Lava Commands",
        Description = "The Lava commands that should be enabled for this block.",
        IsRequired = false,
        Key = AttributeKeys.EnabledLavaCommands,
        Order = 3 )]

    [LinkedPage( "Redirect To Page",
        Description = "The page the user will be redirected to if the Completion Action is set to Redirect to Page.",
        IsRequired = false,
        DefaultValue = "",
        Key = AttributeKeys.RedirectToPage,
        Order = 4 )]

    [CustomDropdownListField( "Scan Mode",
        description: "",
        listSource: "0^Off,1^Automatic",
        IsRequired = false,
        DefaultValue = "0",
        Key = AttributeKeys.ScanMode,
        Order = 5 )]

    [TextField( "Scan Attribute",
        Description = "",
        IsRequired = false,
        DefaultValue = "",
        Key = AttributeKeys.ScanAttribute,
        Order = 6 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_WORKFLOW_ENTRY_BLOCK_TYPE )]
    [Rock.SystemGuid.BlockTypeGuid( "9116AAD8-CF16-4BCE-B0CF-5B4D565710ED")]
    public class WorkflowEntry : RockBlockType
    {
        #region Feature Keys

        /// <summary>
        /// Features supported by both server and client.
        /// </summary>
        private static class FeatureKey
        {
            /// <summary>
            /// Client values (i.e. values converted from Rock Database to Client Native)
            /// are supported.
            /// </summary>
            public const string ClientValues = "clientValues";
        }

        #endregion

        #region Block Attributes

        /// <summary>
        /// The block setting attribute keys for the MobileWorkflowEntry block.
        /// </summary>
        public static class AttributeKeys
        {
            /// <summary>
            /// The completion action key
            /// </summary>
            public const string CompletionAction = "CompletionAction";

            /// <summary>
            /// The completion xaml key
            /// </summary>
            public const string CompletionXaml = "CompletionXaml";

            /// <summary>
            /// The enabled lava commands key
            /// </summary>
            public const string EnabledLavaCommands = "EnabledLavaCommands";

            /// <summary>
            /// The redirect to page key
            /// </summary>
            public const string RedirectToPage = "RedirectToPage";

            /// <summary>
            /// The scan mode key
            /// </summary>
            public const string ScanMode = "ScanMode";

            /// <summary>
            /// The scan attribute key
            /// </summary>
            public const string ScanAttribute = "ScanAttribute";

            /// <summary>
            /// The workflow type key
            /// </summary>
            public const string WorkflowType = "WorkflowType";
        }

        /// <summary>
        /// Gets the scan mode.
        /// </summary>
        /// <value>
        /// The scan mode.
        /// </value>
        protected int ScanMode => GetAttributeValue( AttributeKeys.ScanMode ).AsInteger();

        /// <summary>
        /// Gets the scan attribute.
        /// </summary>
        /// <value>
        /// The scan attribute.
        /// </value>
        protected string ScanAttribute => GetAttributeValue( AttributeKeys.ScanAttribute );

        /// <summary>
        /// Gets the workflow type unique identifier block setting.
        /// </summary>
        /// <value>
        /// The workflow type unique identifier block setting.
        /// </value>
        protected Guid? WorkflowType => GetAttributeValue( AttributeKeys.WorkflowType ).AsGuidOrNull();

        #endregion

        #region IRockMobileBlockType Implementation

        /// <inheritdoc/>
        public override Version RequiredMobileVersion => new Version( 1, 1 );

        /// <summary>
        /// Gets the property values that will be sent to the device in the application bundle.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public override object GetMobileConfigurationValues()
        {
            return new
            {
                ScanAttribute = ScanMode == 1 && ScanAttribute.IsNotNullOrWhiteSpace() ? ScanAttribute : null
            };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the workflow.
        /// </summary>
        /// <param name="workflowGuid">The workflow identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private Model.Workflow LoadWorkflow( Guid? workflowGuid, RockContext rockContext )
        {
            if ( workflowGuid.HasValue )
            {
                return new WorkflowService( rockContext ).Get( workflowGuid.Value );
            }
            else
            {
                WorkflowTypeCache workflowType = null;

                if ( WorkflowType.HasValue )
                {
                    workflowType = WorkflowTypeCache.Get( WorkflowType.Value );
                }
                else if ( RequestContext.PageParameters.ContainsKey( "WorkflowTypeGuid" ) )
                {
                    workflowType = WorkflowTypeCache.Get( RequestContext.PageParameters["WorkflowTypeGuid"].AsGuid() );
                }

                if ( workflowType == null )
                {
                    return null;
                }

                return Model.Workflow.Activate( workflowType, $"New {workflowType.Name}" );
            }
        }

        /// <summary>
        /// Sets the initial workflow attributes.
        /// </summary>
        /// <param name="workflow">The workflow.</param>
        /// <param name="fields">The fields.</param>
        private void SetInitialWorkflowAttributes( Model.Workflow workflow, List<MobileField> fields )
        {
            //
            // Set initial values from the page parameters.
            //
            foreach ( var pageParameter in RequestContext.PageParameters )
            {
                workflow.SetAttributeValue( pageParameter.Key, pageParameter.Value );
            }

            //
            // Set/Update initial values from what the shell sent us.
            //
            if ( fields != null )
            {
                foreach ( var field in fields )
                {
                    workflow.SetPublicAttributeValue( field.Key, field.Value, RequestContext.CurrentPerson, false );
                }
            }
        }

        /// <summary>
        /// Gets the next action with a Form attached.
        /// </summary>
        /// <param name="workflow">The workflow.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <returns></returns>
        private WorkflowAction GetNextAction( Model.Workflow workflow, Person currentPerson )
        {
            int personId = currentPerson?.Id ?? 0;
            bool canEdit = BlockCache.IsAuthorized( Authorization.EDIT, currentPerson );

            //
            // Find all the activities that this person can see.
            //
            var activities = workflow.Activities
                .Where( a =>
                    a.IsActive &&
                    (
                        canEdit ||
                        ( !a.AssignedGroupId.HasValue && !a.AssignedPersonAliasId.HasValue ) ||
                        ( a.AssignedPersonAlias != null && a.AssignedPersonAlias.PersonId == personId ) ||
                        ( a.AssignedGroup != null && a.AssignedGroup.Members.Any( m => m.PersonId == personId ) )
                    )
                )
                .OrderBy( a => a.ActivityTypeCache.Order )
                .ToList();

            //
            // Find the first action that the user is authorized to work with that has a Form
            // attached to it.
            //
            foreach ( var activity in activities )
            {
                if ( canEdit || activity.ActivityTypeCache.IsAuthorized( Authorization.VIEW, currentPerson ) )
                {
                    foreach ( var action in activity.ActiveActions )
                    {
                        if ( action.ActionTypeCache.WorkflowForm != null && action.IsCriteriaValid )
                        {
                            return action;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Sets the form values.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="formFields">The form fields.</param>
        private void SetFormValues( WorkflowAction action, List<MobileField> formFields )
        {
            var activity = action.Activity;
            var workflow = activity.Workflow;
            var form = action.ActionTypeCache.WorkflowForm;

            var values = new Dictionary<int, string>();
            foreach ( var formAttribute in form.FormAttributes.OrderBy( a => a.Order ) )
            {
                if ( formAttribute.IsVisible && !formAttribute.IsReadOnly )
                {
                    var attribute = AttributeCache.Get( formAttribute.AttributeId );
                    var formField = formFields.FirstOrDefault( f => f.AttributeGuid == formAttribute.Attribute.Guid );

                    if ( attribute != null && formField != null )
                    {
                        IHasAttributes item = null;

                        if ( attribute.EntityTypeId == workflow.TypeId )
                        {
                            item = workflow;
                        }
                        else if ( attribute.EntityTypeId == activity.TypeId )
                        {
                            item = activity;
                        }

                        if ( item != null )
                        {
                            item.SetPublicAttributeValue( attribute.Key, formField.Value, RequestContext.CurrentPerson, false );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the workflow attribute entity.
        /// </summary>
        /// <param name="action">The workflow action currently being processed.</param>
        /// <param name="attribute">The attribute.</param>
        /// <returns>The <see cref="WorkflowAction"/> or <see cref="WorkflowActivity"/> that owns the <paramref name="attribute"/>.</returns>
        private static IHasAttributes GetWorkflowAttributeEntity( WorkflowAction action, AttributeCache attribute )
        {
            Rock.Attribute.IHasAttributes item = null;

            if ( attribute.EntityTypeId == action.Activity.Workflow.TypeId )
            {
                item = action.Activity.Workflow;
            }
            else if ( attribute.EntityTypeId == action.Activity.TypeId )
            {
                item = action.Activity;
            }

            return item;
        }

        /// <summary>
        /// Saves the person entry to attribute values.
        /// </summary>
        /// <param name="action">The workflow action currently being processed.</param>
        /// <param name="personEntryPersonId">The person entry person identifier.</param>
        /// <param name="personEntryPersonSpouseId">The person entry person spouse identifier.</param>
        /// <param name="primaryFamily">The primary family.</param>
        private static void SavePersonEntryToAttributeValues( WorkflowAction action, int personEntryPersonId, int? personEntryPersonSpouseId, Group primaryFamily )
        {
            var form = action.ActionTypeCache.WorkflowForm;
            var personAliasService = new PersonAliasService( new RockContext() );

            if ( form.PersonEntryPersonAttributeGuid.HasValue )
            {
                AttributeCache personEntryPersonAttribute = form.FormAttributes.Where( a => a.Attribute.Guid == form.PersonEntryPersonAttributeGuid.Value ).Select( a => a.Attribute ).FirstOrDefault();
                var item = GetWorkflowAttributeEntity( action, personEntryPersonAttribute );

                if ( item != null )
                {
                    var primaryAliasGuid = personAliasService.GetPrimaryAliasGuid( personEntryPersonId );
                    item.SetAttributeValue( personEntryPersonAttribute.Key, primaryAliasGuid );
                }
            }

            if ( form.PersonEntryFamilyAttributeGuid.HasValue )
            {
                AttributeCache personEntryFamilyAttribute = form.FormAttributes.Where( a => a.Attribute.Guid == form.PersonEntryFamilyAttributeGuid.Value ).Select( a => a.Attribute ).FirstOrDefault();
                var item = GetWorkflowAttributeEntity( action, personEntryFamilyAttribute );

                if ( item != null )
                {
                    item.SetAttributeValue( personEntryFamilyAttribute.Key, primaryFamily.Guid );
                }
            }

            if ( form.PersonEntrySpouseAttributeGuid.HasValue && personEntryPersonSpouseId.HasValue )
            {
                AttributeCache personEntrySpouseAttribute = form.FormAttributes.Where( a => a.Attribute.Guid == form.PersonEntrySpouseAttributeGuid.Value ).Select( a => a.Attribute ).FirstOrDefault();
                var item = GetWorkflowAttributeEntity( action, personEntrySpouseAttribute );

                if ( item != null )
                {
                    var primaryAliasGuid = personAliasService.GetPrimaryAliasGuid( personEntryPersonSpouseId.Value );
                    item.SetAttributeValue( personEntrySpouseAttribute.Key, primaryAliasGuid );
                }
            }
        }

        /// <summary>
        /// Updates the person from mobile person object.
        /// </summary>
        /// <param name="action">The action currently being processed.</param>
        /// <param name="person">The person to be updated.</param>
        /// <param name="mobilePerson">The mobile person.</param>
        /// <param name="rockContext">The rock context.</param>
        private static void UpdatePersonFromMobilePerson( WorkflowAction action, Person person, MobilePerson mobilePerson, RockContext rockContext )
        {
            var form = action?.ActionTypeCache?.WorkflowForm;

            if ( form == null )
            {
                return;
            }

            person.FirstName = mobilePerson.FirstName;
            person.NickName = mobilePerson.NickName.IsNotNullOrWhiteSpace() ? mobilePerson.NickName : mobilePerson.FirstName;
            person.LastName = mobilePerson.LastName;

            if ( form.PersonEntryEmailEntryOption != WorkflowActionFormPersonEntryOption.Hidden )
            {
                person.Email = mobilePerson.Email;
            }

            if ( form.PersonEntryMobilePhoneEntryOption != WorkflowActionFormPersonEntryOption.Hidden )
            {
                var existingMobilePhone = person.GetPhoneNumber( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );

                var numberTypeMobile = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );
                var messagingEnabled = existingMobilePhone?.IsMessagingEnabled ?? true;
                var isUnlisted = existingMobilePhone?.IsUnlisted ?? false;

                person.UpdatePhoneNumber( numberTypeMobile.Id, string.Empty, mobilePerson.MobilePhone, messagingEnabled, isUnlisted, rockContext );
            }

            if ( form.PersonEntryBirthdateEntryOption != WorkflowActionFormPersonEntryOption.Hidden )
            {
                person.SetBirthDate( mobilePerson.BirthDate?.DateTime );
            }

            if ( form.PersonEntryGenderEntryOption != WorkflowActionFormPersonEntryOption.Hidden )
            {
                person.Gender = mobilePerson.Gender.ToNative();
            }
        }

        /// <summary>
        /// Creates or Updates person from person editor.
        /// </summary>
        /// <param name="action">The workflow action currently being processed.</param>
        /// <param name="existingPersonId">The existing person identifier.</param>
        /// <param name="limitMatchToFamily">Limit matches to people in specified family</param>
        /// <param name="personValues">The person values.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private static Person CreateOrUpdatePersonFromPersonValues( WorkflowAction action, int? existingPersonId, Group limitMatchToFamily, MobilePerson personValues, RockContext rockContext )
        {
            var personService = new PersonService( rockContext );
            Person personEntryPerson;
            var form = action.ActionTypeCache.WorkflowForm;

            // If we have an existing person, just update and return.
            if ( existingPersonId.HasValue )
            {
                // Update Person from person personValues.
                personEntryPerson = personService.Get( existingPersonId.Value );
                UpdatePersonFromMobilePerson( action, personEntryPerson, personValues, rockContext );

                return personEntryPerson;
            }

            // Match or Create Person from personValues
            var personMatchQuery = new PersonService.PersonMatchQuery( personValues.FirstName, personValues.LastName, personValues.Email, personValues.MobilePhone )
            {
                Gender = form.PersonEntryGenderEntryOption != WorkflowActionFormPersonEntryOption.Hidden ? ( Rock.Model.Gender? ) personValues.Gender.ToNative() : null,
                BirthDate = form.PersonEntryBirthdateEntryOption != WorkflowActionFormPersonEntryOption.Hidden ? personValues.BirthDate?.DateTime : null
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
                UpdatePersonFromMobilePerson( action, personEntryPerson, personValues, rockContext );
            }
            else
            {
                personEntryPerson = new Person();
                UpdatePersonFromMobilePerson( action, personEntryPerson, personValues, rockContext );
            }

            return personEntryPerson;
        }

        /// <summary>
        /// Gets the workflow form person entry values.
        /// </summary>
        /// <param name="personEntryRockContext">The person entry rock context.</param>
        /// <param name="action">The action currently being processed.</param>
        /// <param name="currentPersonId">The current person identifier.</param>
        /// <param name="personEntryValues">The person entry values.</param>
        private static void SetFormPersonEntryValues( RockContext personEntryRockContext, WorkflowAction action, int? currentPersonId, WorkflowFormPersonEntryValues personEntryValues )
        {
            var form = action?.ActionTypeCache?.WorkflowForm;

            if ( form == null || !form.AllowPersonEntry )
            {
                return;
            }

            int? campusId = null;
            int? maritalStatusId = null;

            action.GetPersonEntryPeople( personEntryRockContext, currentPersonId, out var existingPerson, out var existingSpouse );

            // If we have a person and the person entry was set to hide if known, then
            // we just store the current person and spouse values.
            if ( currentPersonId.HasValue && form.PersonEntryHideIfCurrentPersonKnown )
            {
                SavePersonEntryToAttributeValues( action, existingPerson.Id, existingSpouse?.Id, existingPerson.PrimaryFamily );
                return;
            }

            // Nothing provided by the client, so nothing more we can do.
            if ( personEntryValues == null )
            {
                return;
            }

            // Check if the values entered match the existing person, this determines
            // if we can use the existing person or not.
            if ( existingPerson != null )
            {
                var firstNameMatchesExistingFirstOrNickName = personEntryValues.Person.FirstName.Equals( existingPerson.FirstName, StringComparison.OrdinalIgnoreCase )
                        || personEntryValues.Person.FirstName.Equals( existingPerson.NickName, StringComparison.OrdinalIgnoreCase );
                var lastNameMatchesExistingLastName = personEntryValues.Person.LastName.Equals( existingPerson.LastName, StringComparison.OrdinalIgnoreCase );

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
                    existingSpouse = null;
                }
            }

            // Translate our Guid values into Id values.
            if ( personEntryValues.Person.CampusGuid.HasValue )
            {
                campusId = CampusCache.Get( personEntryValues.Person.CampusGuid.Value )?.Id;
            }

            if ( personEntryValues.MaritalStatusGuid.HasValue )
            {
                maritalStatusId = DefinedValueCache.Get( personEntryValues.MaritalStatusGuid.Value )?.Id;
            }

            var personEntryPerson = CreateOrUpdatePersonFromPersonValues( action, existingPerson?.Id, null, personEntryValues.Person, personEntryRockContext );
            if ( personEntryPerson.Id == 0 )
            {
                personEntryPerson.ConnectionStatusValueId = form.PersonEntryConnectionStatusValueId;
                personEntryPerson.RecordStatusValueId = form.PersonEntryRecordStatusValueId;
                PersonService.SaveNewPerson( personEntryPerson, personEntryRockContext, campusId );
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
            personEntryRockContext.SaveChanges();

            var personAliasService = new PersonAliasService( personEntryRockContext );

            int personEntryPersonId = personEntryPerson.Id;
            int? personEntryPersonSpouseId = null;

            var personService = new PersonService( personEntryRockContext );
            var primaryFamily = personService.GetSelect( personEntryPersonId, s => s.PrimaryFamily );

            if ( personEntryValues.Spouse != null )
            {
                var personEntryPersonSpouse = CreateOrUpdatePersonFromPersonValues( action, existingSpouse?.Id, primaryFamily, personEntryValues.Spouse, personEntryRockContext );
                if ( personEntryPersonSpouse.Id == 0 )
                {
                    personEntryPersonSpouse.ConnectionStatusValueId = form.PersonEntryConnectionStatusValueId;
                    personEntryPersonSpouse.RecordStatusValueId = form.PersonEntryRecordStatusValueId;

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

                    var groupRoleId = GroupTypeCache.GetFamilyGroupType().Roles
                        .First( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() )
                        .Id;
                    PersonService.AddPersonToFamily( personEntryPersonSpouse, true, primaryFamily.Id, groupRoleId, personEntryRockContext );
                }

                personEntryRockContext.SaveChanges();

                personEntryPersonSpouseId = personEntryPersonSpouse.Id;
            }

            SavePersonEntryToAttributeValues( action, personEntryPerson.Id, personEntryPersonSpouseId, primaryFamily );

            if ( form.PersonEntryCampusIsVisible )
            {
                primaryFamily.CampusId = campusId;
            }

            if ( form.PersonEntryAddressEntryOption != WorkflowActionFormPersonEntryOption.Hidden && form.PersonEntryGroupLocationTypeValueId.HasValue && personEntryValues.Address != null )
            {
                // a Person should always have a PrimaryFamilyId, but check to make sure, just in case
                if ( primaryFamily != null )
                {
                    var groupLocationService = new GroupLocationService( personEntryRockContext );
                    var familyLocation = primaryFamily.GroupLocations.Where( a => a.GroupLocationTypeValueId == form.PersonEntryGroupLocationTypeValueId.Value ).FirstOrDefault();

                    var newOrExistingLocation = new LocationService( personEntryRockContext ).Get(
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

            personEntryRockContext.SaveChanges();
        }

        /// <summary>
        /// Completes the form action based on the action selected by the user.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="formAction">The form action.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private string CompleteFormAction( WorkflowAction action, string formAction, Person currentPerson, RockContext rockContext )
        {
            var workflowService = new WorkflowService( rockContext );
            var activity = action.Activity;
            var workflow = activity.Workflow;

            var mergeFields = RequestContext.GetCommonMergeFields( currentPerson );
            mergeFields.Add( "Action", action );
            mergeFields.Add( "Activity", activity );
            mergeFields.Add( "Workflow", workflow );

            Guid activityTypeGuid = Guid.Empty;
            string responseText = "Your information has been submitted successfully.";

            //
            // Get the target activity type guid and response text from the
            // submitted form action.
            //
            foreach ( var act in action.ActionTypeCache.WorkflowForm.Actions.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries ) )
            {
                var actionDetails = act.Split( new char[] { '^' } );
                if ( actionDetails.Length > 0 && actionDetails[0] == formAction )
                {
                    if ( actionDetails.Length > 2 )
                    {
                        activityTypeGuid = actionDetails[2].AsGuid();
                    }

                    if ( actionDetails.Length > 3 && !string.IsNullOrWhiteSpace( actionDetails[3] ) )
                    {
                        responseText = actionDetails[3].ResolveMergeFields( mergeFields );
                    }
                    break;
                }
            }

            action.MarkComplete();
            action.FormAction = formAction;
            action.AddLogEntry( "Form Action Selected: " + action.FormAction );

            if ( action.ActionTypeCache.IsActivityCompletedOnSuccess )
            {
                action.Activity.MarkComplete();
            }

            //
            // Set the attribute that should contain the submitted form action.
            //
            if ( action.ActionTypeCache.WorkflowForm.ActionAttributeGuid.HasValue )
            {
                var attribute = AttributeCache.Get( action.ActionTypeCache.WorkflowForm.ActionAttributeGuid.Value );
                if ( attribute != null )
                {
                    IHasAttributes item = null;

                    if ( attribute.EntityTypeId == workflow.TypeId )
                    {
                        item = workflow;
                    }
                    else if ( attribute.EntityTypeId == activity.TypeId )
                    {
                        item = activity;
                    }

                    if ( item != null )
                    {
                        item.SetAttributeValue( attribute.Key, formAction );
                    }
                }
            }

            //
            // Activate the requested activity if there was one.
            //
            if ( !activityTypeGuid.IsEmpty() )
            {
                var activityType = workflow.WorkflowTypeCache.ActivityTypes.Where( a => a.Guid.Equals( activityTypeGuid ) ).FirstOrDefault();
                if ( activityType != null )
                {
                    WorkflowActivity.Activate( activityType, workflow );
                }
            }

            var workflowTypeCache = WorkflowTypeCache.Get( workflow.WorkflowTypeId );

            if ( workflowTypeCache.IsPersisted == false && workflowTypeCache.IsFormBuilder && action != null )
            {
                /* 3/14/2022 MP
                 If this is a FormBuilder workflow, the WorkflowType probably has _workflowType.IsPersisted == false.
                 This is because we don't want to persist the workflow until they have submitted.
                 So, in the case of FormBuilder, we'll persist when they submit regardless of the _workflowType.IsPersisted setting
                */
                workflowService.PersistImmediately( action );
            }

            // If the LastProcessedDateTime is equal to RockDateTime.Now we need to pause for a bit so the workflow will actually process here.
            // The resolution of System.DateTime.UTCNow is between .5 and 15 ms which can cause the workflow processing to not properly pick up
            // where it left off.
            // Without this you might see random failures of workflows to save automatically.
            // https://docs.microsoft.com/en-us/dotnet/api/system.datetime.utcnow?view=netframework-4.7#remarks
            while ( workflow.LastProcessedDateTime == RockDateTime.Now )
            {
                System.Threading.Thread.Sleep( 1 );
            }

            return responseText;
        }

        /// <summary>
        /// Processes the workflow and then get next action.
        /// </summary>
        /// <param name="workflow">The workflow.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        private WorkflowAction ProcessAndGetNextAction( Model.Workflow workflow, Person currentPerson, RockContext rockContext, out WorkflowFormMessage message )
        {
            message = null;

            var processStatus = new WorkflowService( rockContext ).Process( workflow, null, out var errorMessages );
            if ( !processStatus )
            {
                message = new WorkflowFormMessage
                {
                    Type = WorkflowFormMessageType.Error,
                    Title = "Workflow Error",
                    Content = string.Join( "\n", errorMessages )
                };

                return null;
            }

            return GetNextAction( workflow, currentPerson );
        }

        /// <summary>
        /// Gets the completion message to use based on the block settings.
        /// </summary>
        /// <param name="workflow">The workflow.</param>
        /// <param name="responseText">The response text from the last action.</param>
        /// <returns></returns>
        private WorkflowFormMessage GetCompletionMessage( Model.Workflow workflow, string responseText )
        {
            int completionAction = GetAttributeValue( AttributeKeys.CompletionAction ).AsInteger();
            var xaml = GetAttributeValue( AttributeKeys.CompletionXaml );
            var redirectToPage = GetAttributeValue( AttributeKeys.RedirectToPage ).AsGuidOrNull();

            if ( completionAction == 2 && redirectToPage.HasValue )
            {
                return new WorkflowFormMessage
                {
                    Type = WorkflowFormMessageType.Redirect,
                    Content = redirectToPage.ToString()
                };
            }
            else if ( completionAction == 1 && !string.IsNullOrWhiteSpace( xaml ) )
            {
                var mergeFields = RequestContext.GetCommonMergeFields();

                mergeFields.Add( "Workflow", workflow );

                return new WorkflowFormMessage
                {
                    Type = WorkflowFormMessageType.Xaml,
                    Content = xaml.ResolveMergeFields( mergeFields, null, GetAttributeValue( AttributeKeys.EnabledLavaCommands ) )
                };
            }
            else
            {
                if ( string.IsNullOrWhiteSpace( responseText ) )
                {
                    var message = workflow.WorkflowTypeCache.NoActionMessage;
                    var mergeFields = RequestContext.GetCommonMergeFields();

                    mergeFields.Add( "Workflow", workflow );

                    responseText = message.ResolveMergeFields( mergeFields, null, GetAttributeValue( AttributeKeys.EnabledLavaCommands ) );
                }

                return new WorkflowFormMessage
                {
                    Type = WorkflowFormMessageType.Information,
                    Content = responseText
                };
            }
        }

        /// <summary>
        /// Gets the person entry details to be sent to the shell.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action currently being processed.</param>
        /// <param name="currentPersonId">The current person identifier.</param>
        /// <param name="mergeFields">The merge fields to use for Lava parsing.</param>
        /// <returns>The object that will be included in the response that details the person entry part of the form.</returns>
        private static WorkflowFormPersonEntry GetPersonEntryDetails( RockContext rockContext, WorkflowAction action, int? currentPersonId, IDictionary<string, object> mergeFields )
        {
            var form = action.ActionTypeCache.WorkflowForm;

            if ( form == null || !form.AllowPersonEntry )
            {
                return null;
            }

            if ( form.PersonEntryHideIfCurrentPersonKnown && currentPersonId.HasValue )
            {
                return null;
            }

            var mobileSite = Rock.Mobile.MobileHelper.GetCurrentApplicationSite( true, rockContext );

            action.GetPersonEntryPeople( rockContext, currentPersonId, out var personEntryPerson, out var personEntrySpouse );

            var mobilePerson = personEntryPerson != null ? Rock.Mobile.MobileHelper.GetMobilePerson( personEntryPerson, mobileSite ) : null;
            var mobileSpouse = personEntrySpouse != null ? Rock.Mobile.MobileHelper.GetMobilePerson( personEntrySpouse, mobileSite ) : null;

            //
            // Get the default address if it is supposed to show.
            //
            MobileAddress mobileAddress = null;
            var promptForAddress = ( form.PersonEntryAddressEntryOption != WorkflowActionFormPersonEntryOption.Hidden ) && form.PersonEntryGroupLocationTypeValueId.HasValue;
            if ( promptForAddress && ( personEntryPerson?.PrimaryFamilyId ).HasValue )
            {
                var personEntryGroupLocationTypeValueId = form.PersonEntryGroupLocationTypeValueId.Value;

                var familyLocation = new GroupLocationService( rockContext ).Queryable()
                    .Where( a => a.GroupId == personEntryPerson.PrimaryFamilyId.Value && a.GroupLocationTypeValueId == form.PersonEntryGroupLocationTypeValueId )
                    .Select( a => a.Location )
                    .FirstOrDefault();

                mobileAddress = familyLocation != null ? Rock.Mobile.MobileHelper.GetMobileAddress( familyLocation ) : null;
            }

            Guid? maritalStatusGuid;

            if ( personEntryPerson != null )
            {
                maritalStatusGuid = personEntryPerson.MaritalStatusValue?.Guid;
            }
            else
            {
                // default to Married if this is a new person
                maritalStatusGuid = Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED.AsGuid();
            }

            return new WorkflowFormPersonEntry
            {
                PreHtml = form.PersonEntryPreHtml.ResolveMergeFields( mergeFields ),
                PostHtml = form.PersonEntryPostHtml.ResolveMergeFields( mergeFields ),
                CampusIsVisible = form.PersonEntryCampusIsVisible,
                SpouseEntryOption = GetVisibility( form.PersonEntrySpouseEntryOption ),
                GenderEntryOption = GetVisibility( form.PersonEntryGenderEntryOption ),
                EmailEntryOption = GetVisibility( form.PersonEntryEmailEntryOption ),
                MobilePhoneEntryOption = GetVisibility( form.PersonEntryMobilePhoneEntryOption ),
                BirthdateEntryOption = GetVisibility( form.PersonEntryBirthdateEntryOption ),
                AddressEntryOption = form.PersonEntryGroupLocationTypeValueId.HasValue ? GetVisibility( form.PersonEntryAddressEntryOption ) : VisibilityTriState.Hidden,
                MaritalStatusEntryOption = GetVisibility( form.PersonEntryMaritalStatusEntryOption ),
                SpouseLabel = form.PersonEntrySpouseLabel,
                Values = new WorkflowFormPersonEntryValues
                {
                    Person = mobilePerson,
                    Spouse = mobileSpouse,
                    Address = mobileAddress,
                    MaritalStatusGuid = maritalStatusGuid
                }
            };
        }

        /// <summary>
        /// Converts the <see cref="WorkflowActionFormPersonEntryOption"/> value
        /// into one understood by the mobile shell.
        /// </summary>
        /// <param name="option">The visibility option.</param>
        /// <returns>The <see cref="VisibilityTriState"/> value that shows if the value should be hidden, optional or required.</returns>
        private static VisibilityTriState GetVisibility( WorkflowActionFormPersonEntryOption option )
        {
            switch ( option )
            {
                case WorkflowActionFormPersonEntryOption.Optional:
                    return VisibilityTriState.Optional;

                case WorkflowActionFormPersonEntryOption.Required:
                    return VisibilityTriState.Required;

                case WorkflowActionFormPersonEntryOption.Hidden:
                default:
                    return VisibilityTriState.Hidden;
            }
        }

        #endregion

        #region Action Methods

        /// <summary>
        /// Gets the current configuration for this block.
        /// </summary>
        /// <param name="workflowGuid">The workflow unique identifier of the workflow being processed.</param>
        /// <param name="formAction">The form action button that was pressed.</param>
        /// <param name="formFields">The form field values.</param>
        /// <param name="personEntryValues">The person entry values.</param>
        /// <param name="supportedFeatures">The list of features that the client supports.</param>
        /// <returns>
        /// The data for the next form to be displayed.
        /// </returns>
        [BlockAction]
        public WorkflowForm GetNextForm( Guid? workflowGuid = null, string formAction = null, List<MobileField> formFields = null, WorkflowFormPersonEntryValues personEntryValues = null, List<string> supportedFeatures = null )
        {
            var rockContext = new RockContext();
            var workflowService = new WorkflowService( rockContext );

            var workflow = LoadWorkflow( workflowGuid, rockContext );
            var currentPerson = GetCurrentPerson();

            if ( workflow == null )
            {
                return new WorkflowForm
                {
                    Message = new WorkflowFormMessage
                    {
                        Type = WorkflowFormMessageType.Error,
                        Content = "No Workflow Type has been set."
                    }
                };
            }

            //
            // Set initial workflow attribute values.
            //
            if ( !workflowGuid.HasValue )
            {
                SetInitialWorkflowAttributes( workflow, formFields );
            }

            var action = ProcessAndGetNextAction( workflow, currentPerson, rockContext, out var message );
            if ( action == null )
            {
                return new WorkflowForm
                {
                    Message = message ?? GetCompletionMessage( workflow, string.Empty )
                };
            }

            //
            // If this is a form submittal, then complete the form and re-process.
            //
            if ( !string.IsNullOrEmpty( formAction ) && formFields != null )
            {
                if ( personEntryValues != null )
                {
                    using ( var personEntryRockContext = new RockContext() )
                    {
                        SetFormPersonEntryValues( personEntryRockContext, action, RequestContext.CurrentPerson?.Id, personEntryValues );
                    }
                }

                SetFormValues( action, formFields );
                var responseText = CompleteFormAction( action, formAction, currentPerson, rockContext );

                action = ProcessAndGetNextAction( workflow, currentPerson, rockContext, out message );
                if ( action == null )
                {
                    return new WorkflowForm
                    {
                        Message = message ?? GetCompletionMessage( workflow, responseText )
                    };
                }
                else
                {
                    //
                    // If there is a second form, we need to persist.
                    //
                    workflowService.PersistImmediately( action );
                }
            }

            //
            // Begin building up the response with the form data.
            //
            var activity = action.Activity;
            var form = action.ActionTypeCache.WorkflowForm;

            // Prepare the merge fields for the HTML content.
            var mergeFields = RequestContext.GetCommonMergeFields( currentPerson );
            mergeFields.Add( "Action", action );
            mergeFields.Add( "Activity", activity );
            mergeFields.Add( "Workflow", workflow );

            var mobileForm = new WorkflowForm
            {
                WorkflowGuid = workflow.Id != 0 ? ( Guid? ) workflow.Guid : null,
                HeaderHtml = form.Header.ResolveMergeFields( mergeFields ),
                FooterHtml = form.Footer.ResolveMergeFields( mergeFields ),
                PersonEntry = GetPersonEntryDetails( rockContext, action, RequestContext.CurrentPerson?.Id, mergeFields )
            };

            var useClientValues = supportedFeatures?.Contains( FeatureKey.ClientValues ) ?? false;

            //
            // Populate all the form fields that should be visible on the workflow.
            //
            foreach ( var formAttribute in form.FormAttributes.OrderBy( a => a.Order ) )
            {
                if ( formAttribute.IsVisible )
                {
                    var attribute = AttributeCache.Get( formAttribute.AttributeId );
                    string value = attribute.DefaultValue;

                    //
                    // Get the current value from either the workflow or the activity.
                    //
                    if ( workflow.AttributeValues.ContainsKey( attribute.Key ) && workflow.AttributeValues[attribute.Key] != null )
                    {
                        value = workflow.AttributeValues[attribute.Key].Value;
                    }
                    else if ( activity.AttributeValues.ContainsKey( attribute.Key ) && activity.AttributeValues[attribute.Key] != null )
                    {
                        value = activity.AttributeValues[attribute.Key].Value;
                    }

                    var mobileField = new MobileField
                    {
                        AttributeGuid = attribute.Guid,
                        Key = attribute.Key,
                        Title = attribute.Name,
                        IsRequired = formAttribute.IsRequired,
                        ConfigurationValues = useClientValues
                            ? attribute.FieldType.Field?.GetPublicConfigurationValues( attribute.ConfigurationValues, Field.ConfigurationValueUsage.Edit, null )
                            : attribute.QualifierValues.ToDictionary( v => v.Key, v => v.Value.Value ),
                        FieldTypeGuid = attribute.FieldType.Guid,
#pragma warning disable CS0618 // Type or member is obsolete: Required for Mobile Shell v2 support
                        RockFieldType = attribute.FieldType.Class,
#pragma warning restore CS0618 // Type or member is obsolete: Required for Mobile Shell v2 support
                        Value = useClientValues
                            ? attribute.FieldType.Field?.GetPublicEditValue( value, attribute.ConfigurationValues )
                            : value
                    };

                    if ( formAttribute.IsReadOnly )
                    {
                        var field = attribute.FieldType.Field;

                        string formattedValue = null;

                        // get formatted value 
                        if ( attribute.FieldType.Class == typeof( Rock.Field.Types.ImageFieldType ).FullName )
                        {
                            formattedValue = field.FormatValueAsHtml( null, attribute.EntityTypeId, activity.Id, value, attribute.QualifierValues, true );
                        }
                        else
                        {
                            formattedValue = field.FormatValueAsHtml( null, attribute.EntityTypeId, activity.Id, value, attribute.QualifierValues );
                        }

                        mobileField.Value = formattedValue;
                        mobileField.FieldTypeGuid = null;
#pragma warning disable CS0618 // Type or member is obsolete: Required for Mobile Shell v2 support
                        mobileField.RockFieldType = string.Empty;
#pragma warning restore CS0618 // Type or member is obsolete: Required for Mobile Shell v2 support

                        if ( formAttribute.HideLabel )
                        {
                            mobileField.Title = string.Empty;
                        }
                    }

                    mobileForm.Fields.Add( mobileField );
                }
            }

            //
            // Build the list of form actions (buttons) that should be presented
            // to the user.
            //
            foreach ( var btn in form.Actions.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries ) )
            {
                var actionDetails = btn.Split( new char[] { '^' } );
                if ( actionDetails.Length > 0 )
                {
                    DefinedValueCache btnType;

                    if ( !actionDetails[1].IsNullOrWhiteSpace() )
                    {
                        btnType = DefinedValueCache.Get( actionDetails[1].AsGuid() );
                    }
                    else
                    {
                        btnType = DefinedTypeCache.Get( SystemGuid.DefinedType.BUTTON_HTML )
                            .DefinedValues
                            .OrderBy( a => a.Order )
                            .FirstOrDefault();
                    }

                    if ( btnType != null )
                    {
                        mobileForm.Buttons.Add( new WorkflowFormButton
                        {
                            Text = actionDetails[0],
                            Type = btnType.Value
                        } );
                    }
                }
            }

            return mobileForm;
        }

        #endregion
    }
}
