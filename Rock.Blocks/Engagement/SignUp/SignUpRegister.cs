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
using System.Data.Entity;
using System.Linq;
using Rock.Attribute;
using Rock.Data;
using Rock.Enums.Blocks.Engagement.SignUp;
using Rock.Field.Types;
using Rock.Logging;
using Rock.Model;
using Rock.Tasks;
using Rock.ViewModels.Blocks.Engagement.SignUp.SignUpRegister;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Engagement.SignUp
{
    /// <summary>
    /// Block used to register for a sign-up group/project occurrence date time.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Sign-Up Register" )]
    [Category( "Engagement > Sign-Up" )]
    [Description( "Block used to register for a sign-up group/project occurrence date time." )]
    [IconCssClass( "fa fa-clipboard-check" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [EnumField( "Mode",
        Key = AttributeKey.Mode,
        Description = "Determines which registration mode the block is in.",
        EnumSourceType = typeof( RegisterMode ),
        DefaultEnumValue = ( int ) RegisterMode.Anonymous,
        IsRequired = true,
        Order = 0 )]

    [BooleanField( "Include Children",
        Key = AttributeKey.IncludeChildren,
        Description = "Determines if children should be displayed as options when in Family and Group modes.",
        ControlType = BooleanFieldType.BooleanControlType.Checkbox,
        DefaultBooleanValue = false,
        IsRequired = false,
        Order = 1 )]

    [WorkflowTypeField( "Workflow",
        Key = AttributeKey.Workflow,
        Description = "Workflow to launch when the sign-up is complete. In addition to the GroupMember entity, the following attribute keys will be passed to the Workflow: Registrar, Group, Location and Schedule. <span class='tip tip-lava'></span>",
        IsRequired = false,
        Order = 2 )]

    [SystemCommunicationField( "Registrant Confirmation System Communication",
        Key = AttributeKey.RegistrantConfirmationSystemCommunication,
        Description = "Confirmation email to be sent to each registrant (in Family mode, only send to adults and the child if they were the registrar). Merge fields include Registrant, ProjectName, OpportunityName, FriendlyLocation, StartDateTime, Group, Location and Schedule. <span class='tip tip-lava'></span>",
        IsRequired = false,
        Order = 3 )]

    [BooleanField( "Require Email",
        Key = AttributeKey.RequireEmail,
        Description = "When enabled, requires that a value be entered for email when registering in Anonymous mode.",
        ControlType = BooleanFieldType.BooleanControlType.Checkbox,
        DefaultBooleanValue = false,
        Order = 4 )]

    [BooleanField( "Require Mobile Phone",
        Key = AttributeKey.RequireMobilePhone,
        Description = "When enabled, requires that a value be entered for mobile phone when registering in Anonymous mode.",
        ControlType = BooleanFieldType.BooleanControlType.Checkbox,
        DefaultBooleanValue = false,
        Order = 5 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "ED7A31F2-8D4C-469A-B2D8-7E28B8717FB8" )]
    [Rock.SystemGuid.BlockTypeGuid( "161587D9-7B74-4D61-BF8E-3CDB38F16A12" )]
    public class SignUpRegister : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string Mode = "Mode";
            public const string IncludeChildren = "IncludeChildren";
            public const string Workflow = "Workflow";
            public const string RegistrantConfirmationSystemCommunication = "RegistrantConfirmationSystemCommunication";
            public const string RequireEmail = "RequireEmail";
            public const string RequireMobilePhone = "RequireMobilePhone";
        }

        private static class PageParameterKey
        {
            public const string ProjectId = "ProjectId";
            public const string LocationId = "LocationId";
            public const string ScheduleId = "ScheduleId";
            public const string GroupModeGroupId = "GroupId";
        }

        private static class WorkflowAttributeKey
        {
            public const string Registrar = "Registrar";
            public const string Group = "Group";
            public const string Location = "Location";
            public const string Schedule = "Schedule";
        }

        #endregion

        #region Fields

        private static readonly string MustBeLoggedInMessage = "This project requires you to be logged in to be able to register.";
        private static readonly string UnableToRegisterPrefix = "We are unable to register you for this project";

        private GroupMemberService _groupMemberService = null;
        private GroupMemberAssignmentService _groupMemberAssignmentService = null;
        private PersonService _personService = null;

        #endregion

        #region Properties

        public bool IsAuthenticated
        {
            get
            {
                return this.RequestContext.CurrentUser?.IsAuthenticated == true;
            }
        }

        public DefinedValueCache MobilePhoneDefinedValueCache
        {
            get
            {
                return DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE );
            }
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new SignUpRegisterInitializationBox();

            using ( var rockContext = new RockContext() )
            {
                SetBoxInitialState( box, rockContext );
            }

            return box;
        }

        /// <summary>
        /// Sets the initial state of the box.
        /// </summary>
        /// <param name="box">The box.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SetBoxInitialState( SignUpRegisterInitializationBox box, RockContext rockContext )
        {
            var block = new BlockService( rockContext ).Get( this.BlockId );
            block.LoadAttributes();

            var registrationData = GetRegistrationData( rockContext, false );
            if ( !registrationData.CanRegister )
            {
                box.ErrorMessage = registrationData.ErrorMessage ?? $"{UnableToRegisterPrefix}.";
                return;
            }

            box.Mode = registrationData.Mode;
            box.Title = registrationData.Title;
            box.ProjectHasRequiredGroupRequirements = registrationData.ProjectHasRequiredGroupRequirements;
            box.CommunicationPreferenceItems = GetCommunicationPreferenceItems( registrationData );
            box.RequireEmail = registrationData.RequireEmail;
            box.RequireMobilePhone = registrationData.RequireMobilePhone;
            box.Registrants = registrationData.Registrants;
        }

        /// <summary>
        /// Gets the registration data, built using a combination of page parameter values and existing database records.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="shouldTrack">Whether the <see cref="GroupMember"/>, <see cref="Person"/> and <see cref="GroupMemberAssignment"/>
        /// records for existing registrations should be tracked by Entity Framework.</param>
        /// <returns>The registration data, built using a combination of page parameter values and existing database records.</returns>
        private RegistrationData GetRegistrationData( RockContext rockContext, bool shouldTrack )
        {
            var registrationData = new RegistrationData();

            _groupMemberService = new GroupMemberService( rockContext );
            _groupMemberAssignmentService = new GroupMemberAssignmentService( rockContext );
            _personService = new PersonService( rockContext );

            var mode = GetAttributeValue( AttributeKey.Mode ).ConvertToEnum<RegisterMode>( RegisterMode.Anonymous );

            if ( mode == RegisterMode.Family && !IsAuthenticated )
            {
                mode = RegisterMode.Anonymous;
            }

            if ( !IsAuthenticated && mode != RegisterMode.Anonymous )
            {
                registrationData.ErrorMessage = MustBeLoggedInMessage;
                return registrationData;
            }

            registrationData.Mode = mode;
            registrationData.RequireEmail = mode == RegisterMode.Anonymous && GetAttributeValue( AttributeKey.RequireEmail ).AsBoolean();
            registrationData.RequireMobilePhone = mode == RegisterMode.Anonymous && GetAttributeValue( AttributeKey.RequireMobilePhone ).AsBoolean();

            var projectId = Rock.Utility.IdHasher.Instance.GetId( PageParameter( PageParameterKey.ProjectId ) );
            if ( !projectId.HasValue )
            {
                registrationData.ErrorMessage = "Project ID was not provided.";
                return registrationData;
            }

            var locationId = Rock.Utility.IdHasher.Instance.GetId( PageParameter( PageParameterKey.LocationId ) );
            if ( !locationId.HasValue )
            {
                registrationData.ErrorMessage = "Location ID was not provided";
                return registrationData;
            }

            var scheduleId = Rock.Utility.IdHasher.Instance.GetId( PageParameter( PageParameterKey.ScheduleId ) );
            if ( !scheduleId.HasValue )
            {
                registrationData.ErrorMessage = "Schedule ID was not provided.";
                return registrationData;
            }

            int? groupModeGroupId = null;
            if ( mode == RegisterMode.Group )
            {
                groupModeGroupId = Rock.Utility.IdHasher.Instance.GetId( PageParameter( PageParameterKey.GroupModeGroupId ) );
                if ( !groupModeGroupId.HasValue )
                {
                    registrationData.ErrorMessage = "Group ID was not provided for Group Mode registration.";
                    return registrationData;
                }
            }

            if ( !TryGetGroupLocationSchedule( rockContext, registrationData, projectId.Value, locationId.Value, scheduleId.Value ) )
            {
                // An error message will have been added.
                return registrationData;
            }

            GetParticipantCount( rockContext, registrationData );

            var groupRequirements = registrationData.Project.GetGroupRequirements( rockContext );
            if ( groupRequirements.Any( gr => gr.MustMeetRequirementToAddMember ) )
            {
                registrationData.ProjectHasRequiredGroupRequirements = true;

                // We can only determine if an Individual meets GroupRequirements if they're logged in.
                if ( !IsAuthenticated )
                {
                    registrationData.ErrorMessage = MustBeLoggedInMessage;
                    return registrationData;
                }
            }

            if ( mode == RegisterMode.Anonymous )
            {
                // Try to pre-populate the registrant info for the current Person.
                if ( this.RequestContext.CurrentPerson != null )
                {
                    var person = this.RequestContext.CurrentPerson;

                    // Check if we have an existing project GroupMember record for this Group and Person combination.
                    var existingProjectGroupMember = _groupMemberService
                        .Queryable()
                        .AsNoTracking()
                        .FirstOrDefault( gm =>
                            gm.GroupId == registrationData.Project.Id
                            && gm.PersonId == person.Id
                        );

                    var registrant = CreateRegistrant( person, existingProjectGroupMember );
                    registrant.PersonIdKey = null; // Clear this out, as registrants should always be considered anonymous in this mode.
                    registrant.WillAttend = true;

                    if ( registrationData.ProjectHasRequiredGroupRequirements )
                    {
                        registrant.UnmetGroupRequirements = GetUnmetGroupRequirementsForPerson(
                            rockContext,
                            registrationData.Project,
                            person.Id,
                            existingProjectGroupMember?.GroupRoleId ?? registrationData.GroupType.DefaultGroupRoleId
                        );
                    }

                    registrationData.Registrants.Add( registrant );
                }
            }
            else
            {
                if ( !TryGetGroupMemberRegistrants( rockContext, registrationData, groupModeGroupId, shouldTrack ) )
                {
                    // An error message will have been added.
                    return registrationData;
                }
            }

            return registrationData;
        }

        /// <summary>
        /// Tries the get the <see cref="Group"/>, <see cref="Location"/> and <see cref="Schedule"/> instances for this registration,
        /// loading them onto the <see cref="RegistrationData"/> instance.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="registrationData">The registration data.</param>
        /// <param name="projectId">The project identifier.</param>
        /// <param name="locationId">The location identifier.</param>
        /// <param name="scheduleId">The schedule identifier.</param>
        /// <returns>Whether <see cref="Group"/>, <see cref="Location"/> and <see cref="Schedule"/> instances were successfully loaded for this registration.</returns>
        private bool TryGetGroupLocationSchedule( RockContext rockContext, RegistrationData registrationData, int projectId, int locationId, int scheduleId )
        {
            // We'll filter against the allowed GroupType(s) to ensure this block isn't being misused.
            var signUpGroupTypeId = GroupTypeCache.GetId( Rock.SystemGuid.GroupType.GROUPTYPE_SIGNUP_GROUP.AsGuid() ).ToIntSafe();

            // Get the active (and valid sign-up GroupType) opportunities tied to this Group and Location.
            var groupLocationSchedules = new GroupLocationService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Include( gl => gl.Group.GroupType.Roles )
                .Where( gl =>
                    gl.Group.IsActive
                    && gl.Group.Id == projectId
                    && gl.Location.Id == locationId
                    && ( gl.Group.GroupTypeId == signUpGroupTypeId || gl.Group.GroupType.InheritedGroupTypeId == signUpGroupTypeId )
                )
                .SelectMany( gl => gl.Schedules, ( gl, s ) => new
                {
                    gl.Group,
                    gl.Group.GroupType,
                    gl.Location,
                    Schedule = s,
                    Config = gl.GroupLocationScheduleConfigs.FirstOrDefault( glsc => glsc.ScheduleId == s.Id )
                } )
                .ToList();

            // Ensure the requested schedule exists.
            var occurrence = groupLocationSchedules.FirstOrDefault( gls => gls.Schedule?.Id == scheduleId );

            if ( occurrence == null )
            {
                registrationData.ErrorMessage = "Project occurrence not found.";
                return false;
            }

            registrationData.Schedule = occurrence.Schedule;

            if ( !registrationData.ScheduleHasFutureStartDateTime )
            {
                registrationData.ErrorMessage = "Project has no upcoming occurrences.";
                return false;
            }

            registrationData.Project = occurrence.Group;
            registrationData.GroupType = occurrence.GroupType;
            registrationData.Location = occurrence.Location;
            registrationData.SlotsMin = occurrence.Config?.MinimumCapacity;
            registrationData.SlotsDesired = occurrence.Config?.DesiredCapacity;
            registrationData.SlotsMax = occurrence.Config?.MaximumCapacity;

            return true;
        }

        /// <summary>
        /// Gets the participant count for this <see cref="Group"/>, <see cref="Location"/> and <see cref="Schedule"/> occurrence,
        /// and sets it on the <see cref="RegistrationData"/> instance.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="registrationData">The registration data.</param>
        private void GetParticipantCount( RockContext rockContext, RegistrationData registrationData )
        {
            // This should be incorporated into the above [TryGetGroupLocationSchedule] query (for performance reasons) when we have more time to do so.
            var participantCount = new GroupMemberAssignmentService( rockContext )
                .Queryable()
                .Where( gma =>
                    !gma.GroupMember.Person.IsDeceased
                    && gma.GroupMember.GroupId == registrationData.Project.Id
                    && gma.LocationId == registrationData.Location.Id
                    && gma.ScheduleId == registrationData.Schedule.Id
                )
                .Count();

            registrationData.ParticipantCount = participantCount;
        }

        /// <summary>
        /// Gets the list of existing or possible registrants tied to a given Family or other Group, loading them onto the
        /// <see cref="RegistrationData"/> instance.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="registrationData">The registration data.</param>
        /// <param name="groupModeGroupId">The Group identifier to use if in Group mode.</param>
        /// <param name="shouldTrack">Whether the <see cref="GroupMember"/>, <see cref="Person"/> and <see cref="GroupMemberAssignment"/>
        /// records for existing registrations should be tracked by Entity Framework.</param>
        /// <returns>Whether registrants were successfully loaded for this registration.</returns>
        private bool TryGetGroupMemberRegistrants( RockContext rockContext, RegistrationData registrationData, int? groupModeGroupId, bool shouldTrack )
        {
            var currentPersonId = this.RequestContext.CurrentPerson?.Id;
            if ( !currentPersonId.HasValue )
            {
                registrationData.ErrorMessage = $"{UnableToRegisterPrefix}.";
                return false;
            }

            IQueryable<GroupMember> qryGroupMembers;

            var includeChildren = GetAttributeValue( AttributeKey.IncludeChildren ).AsBoolean();
            var groupTerm = "group";

            if ( !groupModeGroupId.HasValue )
            {
                // Family mode: build a list of registrants from the members of this family.
                groupTerm = "family";
                registrationData.Title = this.RequestContext.CurrentPerson.FullName;
                qryGroupMembers = this.RequestContext.CurrentPerson.GetFamilyMembers( includeSelf: true, rockContext );

                if ( !includeChildren )
                {
                    // Remove children (unless a child is the registrar, in which case we'll leave that particular child in the collection).
                    // This latter scenario might be a little odd; if the block settings dictate that children shouldn't be included, we
                    // might want to consider showing an error if the registrar is a child.
                    qryGroupMembers = qryGroupMembers
                        .Where( gm => gm.PersonId == currentPersonId || gm.Person.AgeClassification != AgeClassification.Child );
                }
            }
            else
            {
                // Group mode: build a list of registrants from the members of this Group.
                var group = new GroupService( rockContext ).GetNoTracking( groupModeGroupId.Value );
                if ( group == null )
                {
                    registrationData.ErrorMessage = "Unable to find to the specified group, for Group Mode registration.";
                    return false;
                }

                registrationData.Title = group.Name;

                var qryDirectMembers = _groupMemberService
                    .Queryable()
                    .AsNoTracking()
                    .Include( gm => gm.Person )
                    .Where( gm =>
                        gm.Group.IsActive
                        && gm.Group.Id == groupModeGroupId.Value
                        && !gm.Person.IsDeceased
                    );

                qryGroupMembers = qryDirectMembers;

                if ( includeChildren )
                {
                    // Add children, as they're likely not already direct members of this Group.
                    var qryDistinctFamilyIds = qryDirectMembers
                        .Where( gm => gm.Person.PrimaryFamilyId.HasValue )
                        .Select( gm => gm.Person.PrimaryFamilyId.Value )
                        .Distinct();

                    var qryChildren = _groupMemberService
                        .Queryable()
                        .AsNoTracking()
                        .Include( child => child.Person )
                        .Where( child =>
                            qryDistinctFamilyIds.Any( famId =>
                                child.GroupId == famId
                                && child.Person.AgeClassification == AgeClassification.Child
                                && !qryDirectMembers.Any( dm => dm.PersonId == child.PersonId )
                                && !child.Person.IsDeceased
                            )
                        );

                    qryGroupMembers = qryDirectMembers.Union( qryChildren );
                }
            }

            var groupMembers = qryGroupMembers
                .OrderBy( gm => gm.Person.LastName )
                .ThenBy( gm => gm.Person.AgeClassification )
                .ThenBy( gm => gm.Person.Gender )
                .ToList();

            // Ensure the current Person belongs to the specified Group.
            if ( !groupMembers.Any( gm => gm.PersonId == currentPersonId ) )
            {
                registrationData.ErrorMessage = $"{UnableToRegisterPrefix} as you don't belong to the specified {groupTerm}.";
                return false;
            }

            // Get any existing GroupMember records tied to this project, regardless of occurrence.
            GetExistingProjectMembers( registrationData, shouldTrack );

            // Get any existing registrations for this specific project occurrence.
            GetExistingRegistrations( registrationData, shouldTrack );

            // Create a registrant entry for each GroupMember.
            foreach ( var groupMember in groupMembers )
            {
                // Since a person might belong to a group multiple times if they have multiple roles,
                // ensure we add them to the registrants collection only once.
                if ( registrationData.Registrants.Any( r => r.PersonIdKey == groupMember.Person.IdKey ) )
                {
                    continue;
                }

                var existingProjectGroupMember = registrationData.ExistingProjectMembers
                    .FirstOrDefault( pm => pm.GroupMember.Person.Id == groupMember.PersonId )
                    ?.GroupMember;

                var existingRegistration = registrationData.ExistingRegistrations
                    .FirstOrDefault( gma => gma.GroupMember.PersonId == groupMember.PersonId );

                var registrant = CreateRegistrant( groupMember.Person, existingProjectGroupMember );
                registrant.WillAttend = existingRegistration != null;

                if ( registrationData.ProjectHasRequiredGroupRequirements )
                {
                    registrant.UnmetGroupRequirements = GetUnmetGroupRequirementsForPerson(
                        rockContext,
                        registrationData.Project,
                        groupMember.PersonId,
                        existingProjectGroupMember?.GroupRoleId ?? registrationData.GroupType.DefaultGroupRoleId
                    );
                }

                registrationData.Registrants.Add( registrant );
            }

            return true;
        }

        /// <summary>
        /// Get any existing GroupMember and associated Person records for this project (across all occurrences) + a count
        /// of any registrations each member might have for other occurrences (where a given occurrence isn't this one),
        /// loading them onto the <see cref="RegistrationData"/> instance.
        /// <para>
        /// Note that it's possible we could have GroupMember records tied to this project without any existing
        /// GroupMemberAssignment records, so we're going straight to the [GroupMember] table for this lookup.
        /// </para>
        /// <para>
        /// We'll use these records to:
        ///     1) Preselect the appropriate communication preference options on any relevant client forms.
        ///     2) Connect any new registrations to an existing GroupMember record if they already exist.
        ///     3) Know whether we can safely delete a given GroupMember record if we're deleting a registration that
        ///        previously existed for this occurrence.
        ///     4) Update communication preferences and records, if needed.
        /// </para>
        /// </summary>
        /// <param name="registrationData">The registration data.</param>
        /// <param name="shouldTrack">Whether the <see cref="GroupMember"/> and <see cref="Person"/> records should be
        /// tracked by Entity Framework.</param>
        private void GetExistingProjectMembers( RegistrationData registrationData, bool shouldTrack )
        {
            if ( registrationData.ExistingProjectMembers != null )
            {
                return;
            }

            var qryExistingProjectMembers = _groupMemberService
                .Queryable()
                .Include( gm => gm.Person )
                .Where( gm =>
                    !gm.Person.IsDeceased
                    && gm.GroupId == registrationData.Project.Id
                )
                .Select( gm => new ExistingProjectMember
                {
                    GroupMember = gm,
                    AddlRegistrationsCount = gm.GroupMemberAssignments
                        .Where( gma =>
                            gma.LocationId != registrationData.Location.Id
                            || gma.ScheduleId != registrationData.Schedule.Id
                        )
                        .Count()
                } );

            if ( !shouldTrack )
            {
                qryExistingProjectMembers = qryExistingProjectMembers.AsNoTracking();
            }

            registrationData.ExistingProjectMembers = qryExistingProjectMembers.ToList();
        }

        /// <summary>
        /// Gets the list of existing registrations (<see cref="GroupMemberAssignment"/>s), loading them onto the <see cref="RegistrationData"/> instance.
        /// </summary>
        /// <param name="registrationData">The registration data.</param>
        /// <param name="shouldTrack">Whether the <see cref="GroupMember"/> and <see cref="GroupMemberAssignment"/> records for existing registrations
        /// should be tracked by Entity Framework.</param>
        private void GetExistingRegistrations( RegistrationData registrationData, bool shouldTrack )
        {
            if ( registrationData.ExistingRegistrations != null )
            {
                return;
            }

            var qryExistingRegistrations = _groupMemberAssignmentService
                .Queryable()
                .Include( gma => gma.GroupMember )
                .Where( gma =>
                    !gma.GroupMember.Person.IsDeceased
                    && gma.GroupMember.GroupId == registrationData.Project.Id
                    && gma.LocationId == registrationData.Location.Id
                    && gma.ScheduleId == registrationData.Schedule.Id
                );

            if ( !shouldTrack )
            {
                qryExistingRegistrations = qryExistingRegistrations.AsNoTracking();
            }

            registrationData.ExistingRegistrations = qryExistingRegistrations.ToList();
        }

        /// <summary>
        /// Creates a <see cref="SignUpRegistrantBag"/> instance from the provided <see cref="Person"/> and <see cref="GroupMember"/> (if any).
        /// </summary>
        /// <param name="person">The <see cref="Person"/>.</param>
        /// <param name="projectGroupMember">The existing <see cref="GroupMember"/> for this project (if any).</param>
        /// <returns>A <see cref="SignUpRegistrantBag"/>.</returns>
        private SignUpRegistrantBag CreateRegistrant( Person person, GroupMember projectGroupMember )
        {
            var isRegistrar = person.Id == this.RequestContext.CurrentPerson?.Id;

            // Try to get the mobile phone number for the registrar so we can pre-fill the form.
            PhoneNumber mobilePhone = null;
            if ( isRegistrar )
            {
                mobilePhone = person.GetPhoneNumber( MobilePhoneDefinedValueCache.Guid );
            }

            var communicationPreference = 0;
            if ( projectGroupMember != null )
            {
                communicationPreference = ( int ) projectGroupMember.CommunicationPreference;
            }

            return new SignUpRegistrantBag
            {
                PersonIdKey = person.IdKey,
                IsRegistrar = isRegistrar,
                FirstName = person.NickName,
                LastName = person.LastName,
                FullName = person.FullName,
                IsChild = person.AgeClassification == AgeClassification.Child,
                CommunicationPreference = communicationPreference,
                Email = person.Email,
                MobilePhoneNumber = mobilePhone?.Number,
                MobilePhoneNumberFormatted = mobilePhone?.NumberFormatted,
                MobilePhoneCountryCode = mobilePhone?.CountryCode,
                AllowSms = mobilePhone?.IsMessagingEnabled ?? false
            };
        }

        /// <summary>
        /// Gets the unmet GroupRequirements - if any - for the Person.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="group">The Group.</param>
        /// <param name="personId">The Person identifier.</param>
        /// <returns>The unmet GroupRequirements - if any - for the Person.</returns>
        private List<string> GetUnmetGroupRequirementsForPerson( RockContext rockContext, Rock.Model.Group group, int personId, int? groupRoleId )
        {
            var unmetGroupRequirements = new List<string>();

            var personGroupRequirementStatuses = group.PersonMeetsGroupRequirements( rockContext, personId, groupRoleId );
            foreach ( var personGroupRequirementStatus in personGroupRequirementStatuses
                                                                .Where( s => s.GroupRequirement.MustMeetRequirementToAddMember
                                                                            && s.MeetsGroupRequirement != MeetsGroupRequirement.Meets ) )
            {
                var groupRequirementType = personGroupRequirementStatus.GroupRequirement.GroupRequirementType;
                if ( groupRequirementType == null )
                {
                    continue;
                }

                var friendlyName = groupRequirementType.NegativeLabel;
                if ( string.IsNullOrWhiteSpace( friendlyName ) )
                {
                    friendlyName = groupRequirementType.Name;
                }

                if ( !string.IsNullOrWhiteSpace( friendlyName ) )
                {
                    unmetGroupRequirements.Add( friendlyName );
                }
            }

            return unmetGroupRequirements;
        }

        /// <summary>
        /// Gets the communication preference items that should be presented for registrant to select.
        /// </summary>
        /// <returns>The communication preference items that should be presented for registrant to select.</returns>
        private List<ListItemBag> GetCommunicationPreferenceItems( RegistrationData registrationData )
        {
            if ( !registrationData.HasReminderCommunicationConfigured )
            {
                return new List<ListItemBag>();
            }

            return new List<ListItemBag>
            {
                new ListItemBag
                {
                    Value = CommunicationType.SMS.ConvertToInt().ToString(),
                    Text = CommunicationType.SMS.ConvertToString()
                },
                new ListItemBag
                {
                    Value = CommunicationType.Email.ConvertToInt().ToString(),
                    Text = CommunicationType.Email.ConvertToString()
                }
            };
        }

        /// <summary>
        /// Processes the provided registrants, comparing them against the allowed list of GroupMember registrants (if applicable),
        /// for this project occurrence.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="registrationData">The registration data, according to the project's <see cref="Group"/>, <see cref="Location"/> and <see cref="Schedule"/>
        /// parsed from the query string.</param>
        /// <param name="registrants">The registrants to process.</param>
        /// <param name="errorMessage">The friendly error message describing the problem encountered, if the attempt was unsuccessful.</param>
        /// <returns>A <see cref="SignUpRegisterResponseBag"/> containing information about registrants that were successfully registered,
        /// unregistered or unable to be registered or <see langword="null"/> if an error is encountered.</returns>
        private SignUpRegisterResponseBag TryProcessRegistrants( RockContext rockContext, RegistrationData registrationData, List<SignUpRegistrantBag> registrants, out string errorMessage )
        {
            if ( registrants?.Any() != true )
            {
                errorMessage = $"{UnableToRegisterPrefix} as no individuals were provided to be registered.";
                return null;
            }

            errorMessage = null;

            var mode = registrationData.Mode;
            var registered = new List<SignUpRegistrantBag>();
            var unregistered = new List<SignUpRegistrantBag>();
            var unsuccessful = new List<SignUpRegistrantBag>();

            // Get and track any existing GroupMember records tied to this project, regardless of occurrence.
            GetExistingProjectMembers( registrationData, true );

            // Get and track any existing registrations for this specific project occurrence.
            GetExistingRegistrations( registrationData, true );

            // We're using the term "registrar" loosely here, as there is no official registrar record saved to the database.
            // We'll pass this Person instance to any workflow defined on the block, so we know who was responsible for registering
            // a given group of registrants.
            Person registrarPerson = null;
            if ( IsAuthenticated )
            {
                registrarPerson = this.RequestContext.CurrentPerson;
            }

            ExistingProjectMember existingProjectMember = null;
            GroupMemberAssignment existingRegistration = null;

            string warningMessage = null;
            var registrantsToRegister = new List<SignUpRegistrantBag>();
            var registrantsToMessage = new List<SignUpRegistrantBag>();

            var communicationUpdates = new List<CommunicationUpdate>();

            var groupMemberAssignmentsToDelete = new List<GroupMemberAssignment>();
            var groupMembersToDelete = new List<GroupMember>();

            if ( mode == RegisterMode.Anonymous )
            {
                if ( GetAttributeValue( AttributeKey.RequireEmail ).AsBoolean() && registrants.Any( r => string.IsNullOrWhiteSpace( r.Email ) ) )
                {
                    errorMessage = $"{UnableToRegisterPrefix} as a valid email address is required for all registrants.";
                    return null;
                }

                if ( GetAttributeValue( AttributeKey.RequireMobilePhone ).AsBoolean() && registrants.Any( r => string.IsNullOrWhiteSpace( r.MobilePhoneNumber ) ) )
                {
                    errorMessage = $"{UnableToRegisterPrefix} as a valid mobile phone number is required for all registrants.";
                    return null;
                }

                // Once we determine who the registrar is (either the logged-in individual or the first person in the `registrants` list), if we come across any
                // registrants for whom we can't find an existing Person record, try to link that registrant to one of the registrar's family members to prevent
                // the creation of unneeded duplicate Person records when possible.
                List<Person> familyMemberPeople = null;

                foreach ( var registrant in registrants )
                {
                    // In Anonymous mode, there will be no "allowed" registrants to compare against, and we might not even know who this Person is.
                    //  1) If the individual is authenticated, they are the registrar. Otherwise, the registrar is the first Person in the `registrants` list.
                    //     Note that the registrar might or might not be in the `registrants` list.
                    //  2) For each registrant in the `registrants` list:
                    //      a) Create Person & [family] Group records if we can't find exact matches for the provided search criteria.
                    //      b) If we can determine that they're already registered for this project occurrence, make sure they still meet any required Group requirements,
                    //         and unregister them if not.
                    //      c) If they aren't already registered for this project occurrence, add them to the `registrantsToRegister` list (if there are no unmet Group
                    //         requirements), so we can create missing GroupMember and GroupMemberAssignment records.
                    //      d) If we create and register a new Person record for this registrant, add them to the `communicationUpdates` collection so we can ensure
                    //         we have their communication preferences saved.
                    //      e) If we find an existing Person record, we can only update their [Mobile]PhoneNumber.IsMessagingEnabled bit if:
                    //              i) The provided mobile phone number matches exactly AND
                    //             ii) They didn't provide an email address OR the provided email address matches exactly.
                    //         otherwise, Anonymous mode cannot be used to update existing Person records, as it's too risky. Someone could maliciously - or even
                    //         accidentally - update records that don't belong to them.

                    existingProjectMember = null;
                    existingRegistration = null;

                    // Note that this query and lookup will only use arguments that are defined, so it's safe to pass nulls, Etc. into this constructor.
                    var personQuery = new PersonService.PersonMatchQuery( registrant.FirstName?.Trim(), registrant.LastName?.Trim(), registrant.Email?.Trim(), registrant.MobilePhoneNumber?.Trim() );
                    var person = _personService.FindPerson( personQuery, updatePrimaryEmail: false );

                    var wasFirstNameProvided = !string.IsNullOrWhiteSpace( registrant.FirstName );
                    var wasLastNameProvided = !string.IsNullOrWhiteSpace( registrant.LastName );
                    var wasEmailProvided = !string.IsNullOrWhiteSpace( registrant.Email );
                    var wasMobilePhoneProvided = !string.IsNullOrWhiteSpace( registrant.MobilePhoneNumber );

                    var registrantFirstNameUpper = registrant.FirstName?.Trim().ToUpper();
                    var registrantLastNameUpper = registrant.LastName?.Trim().ToUpper();
                    var registrantEmailUpper = registrant.Email?.Trim().ToUpper();
                    var registrantCleanMobilePhoneNumber = wasMobilePhoneProvided
                        ? PhoneNumber.CleanNumber( registrant.MobilePhoneNumber )
                        : null;

                    // In the event that someone provides both an email address and a mobile phone number, they must both match.
                    // Otherwise, we're going to create a new Person record to play it safe; better to have duplicates to merge
                    // than to allow someone to maliciously/accidentally overwrite a Person's existing communication records.
                    if ( person != null && wasEmailProvided && wasMobilePhoneProvided )
                    {
                        if ( person.Email?.Trim().ToUpper() != registrantEmailUpper )
                        {
                            // Email doesn't match; create a new Person.
                            person = null;
                        }
                        else
                        {
                            var mobilePhoneNumber = person.GetPhoneNumber( MobilePhoneDefinedValueCache.Guid );
                            if ( mobilePhoneNumber == null || PhoneNumber.CleanNumber( mobilePhoneNumber.Number ) != registrantCleanMobilePhoneNumber )
                            {
                                // Number doesn't match; create a new Person.
                                person = null;
                            }
                        }
                    }

                    // Final attempt to prevent a duplicate person: If we didn't find a Person and neither email address
                    // nor mobile phone number were provided, look in the registrar's family members to find a match.
                    if ( person == null
                        && registrarPerson != null
                        && wasFirstNameProvided
                        && !wasEmailProvided
                        && !wasMobilePhoneProvided )
                    {
                        if ( familyMemberPeople == null )
                        {
                            familyMemberPeople = registrarPerson
                                .GetFamilyMembers( true, rockContext )
                                .Select( fm => fm.Person )
                                .ToList();
                        }

                        person = familyMemberPeople
                            .FirstOrDefault( p =>
                                (
                                    p.FirstName?.Trim().ToUpper() == registrantFirstNameUpper
                                    || p.NickName?.Trim().ToUpper() == registrantFirstNameUpper
                                ) && (
                                    !wasLastNameProvided
                                    || p.LastName?.Trim().ToUpper() == registrantLastNameUpper
                                )
                            );
                    }

                    if ( person == null )
                    {
                        // For new people: set `Person.CommunicationPreference` (and `GroupMember.CommunicationPreference`) only if we can
                        // confidently determine the preference, based on the provided values:
                        //  1) If both `registrant.Email` and `registrant.MobilePhoneNumber` are defined, we don't know which they'd prefer.
                        //  2) If only `registrant.Email` is defined, the preference is `CommunicationType.Email`.
                        //  3) If only `registrant.MobilePhoneNumber` is defined (AND `registrant.AllowSms` is true), the preference is `CommunicationType.SMS`.
                        CommunicationType communicationPreference = CommunicationType.RecipientPreference; // Default value.
                        if ( wasEmailProvided && !wasMobilePhoneProvided )
                        {
                            communicationPreference = CommunicationType.Email;
                        }
                        else if ( !wasEmailProvided && wasMobilePhoneProvided && registrant.AllowSms )
                        {
                            communicationPreference = CommunicationType.SMS;
                        }

                        person = new Person
                        {
                            FirstName = registrant.FirstName?.Trim(),
                            LastName = registrant.LastName?.Trim(),
                            Email = registrant.Email?.Trim(),
                            RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id,
                            CommunicationPreference = communicationPreference
                        };

                        if ( wasMobilePhoneProvided )
                        {
                            person.UpdatePhoneNumber
                            (
                                MobilePhoneDefinedValueCache.Id,
                                registrant.MobilePhoneCountryCode,
                                registrant.MobilePhoneNumber,
                                registrant.AllowSms,
                                isUnlisted: null,
                                rockContext
                            );
                        }

                        // This will save the new Person to the database, while also creating and saving a new [family] Group.
                        PersonService.SaveNewPerson( person, rockContext );

                        // Update the registrant with the new `Person.IdKey`.
                        registrant.PersonIdKey = person.IdKey;

                        if ( communicationPreference != CommunicationType.RecipientPreference )
                        {
                            // Set their `GroupMember.CommunicationPreference` value once we create a new GroupMember record below.
                            communicationUpdates.Add( new CommunicationUpdate
                            {
                                Registrant = registrant,
                                NewCommunicationPreference = communicationPreference,
                                Person = person
                            } );
                        }
                    }
                    else
                    {
                        // Update the registrant with the located `Person.IdKey`.
                        registrant.PersonIdKey = person.IdKey;

                        existingProjectMember = registrationData.ExistingProjectMembers
                                .FirstOrDefault( pm => pm.GroupMember.Person.IdKey == registrant.PersonIdKey );

                        existingRegistration = registrationData.ExistingRegistrations
                            .FirstOrDefault( gma => gma.GroupMember.Person.IdKey == registrant.PersonIdKey );

                        // We can only update an existing Person's [Mobile]PhoneNumber.IsMessagingEnabled bit if:
                        //  1) The provided mobile phone number matches exactly AND
                        //  2) They didn't provide an email address OR the provided email address matches exactly.
                        var mobilePhoneNumber = person.GetPhoneNumber( MobilePhoneDefinedValueCache.Guid );
                        if ( mobilePhoneNumber != null
                            && PhoneNumber.CleanNumber( mobilePhoneNumber.Number ) == registrantCleanMobilePhoneNumber
                            && (
                                !wasEmailProvided
                                || person.Email?.Trim().ToUpper() == registrantEmailUpper
                            )
                        )
                        {
                            // Note that we're only updating the IsMessagingEnabled bit from the provided registrant's
                            // AllowSms value; the rest of the values will simply be copied back from the existing mobile
                            // phone number, to ensure this anonymous form cannot be used to hijack an existing Person's
                            // communication records.
                            person.UpdatePhoneNumber(
                                MobilePhoneDefinedValueCache.Id,
                                mobilePhoneNumber.CountryCode,
                                mobilePhoneNumber.Number,
                                registrant.AllowSms,
                                mobilePhoneNumber.IsUnlisted,
                                rockContext
                            );
                        }
                    }

                    // If the individual performing this registration request is not logged in, set the "registrar" to the first Person registered.
                    if ( registrarPerson == null )
                    {
                        registrarPerson = person;
                    }

                    // Supplement registrant with formatted Person data (but don't provide any more than they offered, except for the full name).
                    registrant.FullName = person.FullName;
                    if ( wasMobilePhoneProvided )
                    {
                        var mobilePhone = person.GetPhoneNumber( MobilePhoneDefinedValueCache.Guid );
                        if ( mobilePhone != null )
                        {
                            registrant.MobilePhoneNumberFormatted = mobilePhone.NumberFormatted;
                        }
                    }

                    if ( registrationData.ProjectHasRequiredGroupRequirements )
                    {
                        var unmetRequirements = GetUnmetGroupRequirementsForPerson
                        (
                            rockContext,
                            registrationData.Project,
                            person.Id,
                            existingProjectMember?.GroupMember?.GroupRoleId ?? registrationData.GroupType.DefaultGroupRoleId
                        );

                        if ( unmetRequirements.Any() )
                        {
                            // Don't supplement the registrant with unmet requirements data, as that could be a security risk.

                            if ( existingRegistration != null )
                            {
                                groupMemberAssignmentsToDelete.Add( existingRegistration );

                                if ( existingProjectMember?.AddlRegistrationsCount == 0 )
                                {
                                    // If this project GroupMember doesn't have any more GroupMemberAssignment records, delete (or archive) it as well.
                                    // We'll need to set it aside for now, and delete it after we perform the initial save, to release the FK reference.
                                    groupMembersToDelete.Add( existingRegistration.GroupMember );
                                }
                            }

                            // This Person doesn't meet the Group requirements.
                            unsuccessful.Add( registrant );

                            // Move on to the next registrant.
                            continue;
                        }
                    }

                    if ( existingRegistration != null )
                    {
                        // This individual was previously registered, and is still registered.
                        // Add them to the `registered` collection so we can assure the registrar that they're still registered.
                        registered.Add( registrant );
                    }
                    else
                    {
                        // New or existing Person is eligible to be registered, and should be sent a confirmation communication.
                        registrantsToRegister.Add( registrant );
                        registrantsToMessage.Add( registrant );
                    }
                }
            }
            else // Family or Group mode.
            {
                if ( !IsAuthenticated )
                {
                    errorMessage = MustBeLoggedInMessage;
                    return null;
                }

                // The registrar's registrant instance is used to drive some of the logic below.
                SignUpRegistrantBag registrarRegistrant = null;
                if ( registrarPerson != null )
                {
                    registrarRegistrant = registrants.FirstOrDefault( r => r.PersonIdKey == registrarPerson.IdKey );
                }

                // Ensure all provided `registrants` are also in the allowed list of GroupMember registrants.
                var groupMemberRegistrants = registrationData.Registrants;
                if ( registrants.Any( r => !groupMemberRegistrants.Any( gmr => gmr.PersonIdKey == r.PersonIdKey ) ) )
                {
                    errorMessage = $"{UnableToRegisterPrefix} as your registration request includes individuals you're not authorized to register.";
                    return null;
                }

                // In Family mode, we'll set all registered registrant's communication preference to match that of the registrar, if defined and not the default value.
                CommunicationType registrarCommunicationPreference = CommunicationType.RecipientPreference; // Default value.
                if ( registrarRegistrant != null && Enum.IsDefined( typeof( CommunicationType ), registrarRegistrant.CommunicationPreference ) )
                {
                    registrarCommunicationPreference = ( CommunicationType ) registrarRegistrant.CommunicationPreference;
                }

                foreach ( var registrant in registrants )
                {
                    // In Family or Group mode, any [allowed] registrant in the `groupMemberRegistrants` list whose `WillAttend` property is true will
                    // already have GroupMember and GroupMemberAssignment records for this project occurrence [GroupLocationSchedule].
                    // 
                    //  1) If the corresponding, current registrant's `WillAttend` property is false (or it's true but they no longer meet required
                    //     GroupRequirements), we need to:
                    //      a) Delete their GroupMemberAssignment record.
                    //      b) Delete their GroupMember record IF there are no more GroupMemberAssignment records tied to it (they might be signed up for
                    //         other occurrences within this same project).
                    //  2) If the corresponding, current registrant's `WillAttend` property is true and they meet any required GroupRequirements, AND:
                    //      a) Mode == Family, add them to the `communicationUpdates` collection so we can ensure we have their latest communication
                    //         preferences updated.
                    //      b) Mode != Family, we have nothing to do; they were registered before, and they're still registered now.
                    // 
                    // Any [allowed] registrant in the `groupMemberRegistrants` list whose `WillAttend` property is false hasn't yet been registered
                    // for this occurrence.
                    // 
                    //  1) If the corresponding, current registrant's `WillAttend` property is false, we have nothing to do; they weren't registered
                    //     before, and they're still not registered.
                    //  2) If the corresponding, current registrant's `WillAttend` property is true:
                    //      a) AND they have no unmet GroupRequirements, add them to the `registrantsToRegister` list, so we can create missing
                    //         GroupMember and GroupMemberAssignment records.
                    //      b) If Mode == Family, add them to the `communicationUpdates` collection so we can ensure we have their latest communication
                    //         preferences updated.

                    existingProjectMember = registrationData.ExistingProjectMembers
                            .FirstOrDefault( pm => pm.GroupMember.Person.IdKey == registrant.PersonIdKey );

                    existingRegistration = registrationData.ExistingRegistrations
                            .FirstOrDefault( gma => gma.GroupMember.Person.IdKey == registrant.PersonIdKey );

                    var isRegistrar = registrant == registrarRegistrant;

                    // We're guaranteed to find a registrant match within the available GroupMembers based on the existence check performed above.
                    var groupMemberRegistrant = groupMemberRegistrants.First( r => r.PersonIdKey == registrant.PersonIdKey );

                    // Keep track of whether we need to update communication preferences/records for this registrant.
                    var addCommunicationUpdate = false;

                    if ( existingRegistration != null && ( !registrant.WillAttend || groupMemberRegistrant.UnmetGroupRequirements?.Any() == true ) )
                    {
                        // Scenario 1) This individual was previously registered, but is now being unregistered.
                        // Scenario 2) This individual was previously registered, but no longer meets [required] GroupRequirements.
                        groupMemberAssignmentsToDelete.Add( existingRegistration );

                        if ( existingProjectMember?.AddlRegistrationsCount == 0 )
                        {
                            // If this project GroupMember doesn't have any more GroupMemberAssignment records, delete (or archive) it as well.
                            // We'll need to set it aside for now, and delete it after we perform the initial save, to release the FK reference.
                            groupMembersToDelete.Add( existingRegistration.GroupMember );
                        }

                        unregistered.Add( registrant );
                    }
                    else if ( existingRegistration == null && registrant.WillAttend )
                    {
                        // This individual was not previously registered, and is now being registered.
                        if ( groupMemberRegistrant.UnmetGroupRequirements?.Any() == true )
                        {
                            // But they have unmet GroupRequirements, so they're not allowed.
                            unsuccessful.Add( registrant );
                        }
                        else
                        {
                            registrantsToRegister.Add( registrant );

                            if ( mode == RegisterMode.Family )
                            {
                                addCommunicationUpdate = true;

                                // Only send confirmation communications to family members if they're an adult or the registrar.
                                if ( isRegistrar || !groupMemberRegistrant.IsChild )
                                {
                                    registrantsToMessage.Add( registrant );
                                }
                            }
                            else
                            {
                                // Always send confirmation communications to newly-added registrants in Group mode.
                                registrantsToMessage.Add( registrant );
                            }
                        }
                    }
                    else if ( existingRegistration != null && registrant.WillAttend )
                    {
                        // This individual was previously registered, and is still registered.
                        // Add them to the `registered` collection so we can assure the registrar that they're still registered.
                        registered.Add( registrant );

                        if ( mode == RegisterMode.Family )
                        {
                            // The registrar might have changed their communication preference; make sure to keep this family member's preference in sync.
                            addCommunicationUpdate = true;
                        }
                    }
                    else
                    {
                        // This individual wasn't registered before and they're still not registered; nothing to do.
                    }

                    if ( addCommunicationUpdate || isRegistrar )
                    {
                        // A preference value of CommunicationType.RecipientPreference signifies that the registrar didn't make a preference
                        // selection; this is the default value. In this case, leave any existing communication preferences and records as is.
                        if ( mode == RegisterMode.Family && registrarCommunicationPreference != CommunicationType.RecipientPreference )
                        {
                            // Get the existing, tracked GroupMember and Person records, if any. If we don't have these yet, we'll
                            // fill them in later, when we create the registrant's GroupMember and GroupMemberAssignment records.
                            var existingProjectGroupMember = existingProjectMember?.GroupMember;
                            var existingPerson = existingProjectGroupMember?.Person;

                            // If the registrar 1) isn't an existing project member AND 2) isn't being registered for this occurrence,
                            // go get a tracked instance of their Person record so we can update their communication records.
                            // Otherwise, we'll get and update their Person record as part of the natural flow below.
                            if ( isRegistrar && existingPerson == null && !registrantsToRegister.Contains( registrant ) )
                            {
                                existingPerson = _personService
                                    .Queryable()
                                    .Include( p => p.PhoneNumbers )
                                    .FirstOrDefault( p => p.Id == registrarPerson.Id );
                            }

                            // Update this family member's communication preference to match that of the registrar, but don't update
                            // their [email and/or mobile phone] communication records, as we didn't collect this info (unless this
                            // registrant IS the registrar, in which case we did collect this info).

                            var newEmail = isRegistrar && !string.IsNullOrWhiteSpace( registrant.Email )
                                ? registrant.Email
                                : null;

                            var newMobilePhoneNumber = isRegistrar && !string.IsNullOrWhiteSpace( registrant.MobilePhoneNumber )
                                ? registrant.MobilePhoneNumber
                                : null;

                            var newMobilePhoneCountryCode = isRegistrar && !string.IsNullOrWhiteSpace( newMobilePhoneNumber )
                                ? registrant.MobilePhoneCountryCode
                                : null;

                            var newIsMessagingEnabled = registrarCommunicationPreference == CommunicationType.SMS
                                ? true
                                : ( bool? ) null;

                            communicationUpdates.Add( new CommunicationUpdate
                            {
                                Registrant = registrant,
                                ProjectGroupMember = existingProjectGroupMember,
                                NewCommunicationPreference = registrarCommunicationPreference,
                                Person = existingPerson,
                                NewEmail = newEmail,
                                NewMobilePhoneCountryCode = newMobilePhoneCountryCode,
                                NewMobilePhoneNumber = newMobilePhoneNumber,
                                NewIsMessagingEnabled = newIsMessagingEnabled
                            } );
                        }
                    }
                }
            }

            if ( groupMemberAssignmentsToDelete.Any() )
            {
                // For now, this is safe, as GroupMemberAssignment is a pretty low-level Entity with no child Entities.
                // We'll need to check `GroupMemberAssignmentService.CanDelete()` for each assignment (and abandon the bulk
                // delete approach) if this changes in the future.
                _groupMemberAssignmentService.DeleteRange( groupMemberAssignmentsToDelete );
            }

            // If they're attempting to register more individuals than this occurrence's available spots, don't register anybody.
            // Instead, give them an opportunity to choose who - if anybody - should fill the remaining spots.
            // Note that they might be swapping one-for-another, Etc., so factor out the count of any individuals that are being unregistered.
            var registrantsToRegisterCount = registrantsToRegister.Count - unregistered.Count;
            if ( registrantsToRegisterCount > registrationData.SlotsAvailable )
            {
                unsuccessful.AddRange( registrantsToRegister );
                registrantsToRegister.Clear();

                warningMessage = registrationData.SlotsAvailable == 0
                    ? $"This project doesn't have any available spots remaining."
                    : $"This project only has {registrationData.SlotsAvailable} available {"spot".PluralizeIf( registrationData.SlotsAvailable > 1 )} remaining.";
            }

            var workflowMembers = new List<GroupMember>();
            var communicationRecipients = new List<GroupMemberAssignment>();

            if ( registrantsToRegister.Any() )
            {
                // Create missing GroupMember and GroupMemberAssignment records.
                //  1) Set the `GroupMember.GroupRoleId` to `GroupType.DefaultGroupRoleId` if defined for this project, or the first (preferably
                //     non-leader) GroupTypeRole tied to this project, or return an error if no role is found, since it's a required field on the
                //     GroupMember entity.
                //  2) The GroupMember record might already exist for a given registrant, as they might already be signed up for other occurrences
                //     within this same project.
                //  3) Supplement any `CommunicationUpdate` instances that have missing GroupMember or Person references.

                var groupRoleId = registrationData.GroupType.DefaultGroupRoleId;
                if ( !groupRoleId.HasValue )
                {
                    groupRoleId = registrationData.GroupType.Roles
                        .OrderBy( r => !r.IsLeader )
                        .ThenBy( r => r.Order )
                        .Select( r => r.Id )
                        .FirstOrDefault();
                }

                if ( !groupRoleId.HasValue )
                {
                    RockLogger.Log.Warning( RockLogDomains.Group, "Unable to register {@registrantsToRegister} to {Project} sign-up project as no group roles could be found.", registrantsToRegister, registrationData.Project.Name );

                    errorMessage = $"{UnableToRegisterPrefix}.";
                    return null;
                }

                // At this point, we should have created any missing Person records and ensured this registrant is eligible to be added.
                foreach ( var registrant in registrantsToRegister )
                {
                    int? personId = null;
                    if ( !string.IsNullOrWhiteSpace( registrant.PersonIdKey ) )
                    {
                        personId = Rock.Utility.IdHasher.Instance.GetId( registrant.PersonIdKey );
                    }

                    if ( !personId.HasValue )
                    {
                        // This shouldn't ever happen, but let's safeguard against it just in case.
                        unsuccessful.Add( registrant );
                        continue;
                    }

                    // Always create a new GroupMemberAssignment record.
                    var groupMemberAssignment = new GroupMemberAssignment
                    {
                        LocationId = registrationData.Location.Id,
                        ScheduleId = registrationData.Schedule.Id,
                    };

                    // Only create a new GroupMember record if one doesn't already exist for this Group & Person combination.
                    var addGroupMember = false;
                    var projectGroupMember = registrationData.ExistingProjectMembers
                        .FirstOrDefault( pm => pm.GroupMember.Person.Id == personId.Value )
                        ?.GroupMember;

                    if ( projectGroupMember == null )
                    {
                        projectGroupMember = new GroupMember
                        {
                            GroupId = registrationData.Project.Id,
                            PersonId = personId.Value,
                            GroupRoleId = groupRoleId.Value,
                            GroupMemberStatus = GroupMemberStatus.Active,
                            GroupTypeId = registrationData.GroupType.Id
                        };

                        groupMemberAssignment.GroupMember = projectGroupMember;

                        // Don't add this new member to the service just yet; let's first make sure we can actually retrieve this Person record if we still need to.
                        addGroupMember = true;
                    }
                    else
                    {
                        groupMemberAssignment.GroupMemberId = projectGroupMember.Id;
                    }

                    // Check to see if we need to supplement a corresponding `CommunicationUpdate` instance with missing GroupMember or Person references.
                    var communicationUpdate = communicationUpdates.FirstOrDefault( cu => cu.Registrant == registrant );
                    if ( communicationUpdate != null )
                    {
                        if ( communicationUpdate.ProjectGroupMember == null )
                        {
                            communicationUpdate.ProjectGroupMember = projectGroupMember;
                        }

                        if ( communicationUpdate.Person == null )
                        {
                            var person = projectGroupMember.Person;
                            if ( person == null )
                            {
                                // We still need to go get this Person; get their phone numbers as well, since it's possible we'll be updating the mobile PhoneNumber.
                                // `personId.Value` is guaranteed to be defined based on a check performed above.
                                person = _personService
                                    .Queryable()
                                    .Include( p => p.PhoneNumbers )
                                    .FirstOrDefault( p => p.Id == personId.Value );

                                if ( person == null )
                                {
                                    // This shouldn't ever happen, but let's safeguard against it just in case.
                                    unsuccessful.Add( registrant );
                                    continue;
                                }

                                communicationUpdate.Person = person;
                            }
                        }
                    }

                    if ( addGroupMember )
                    {
                        _groupMemberService.Add( projectGroupMember );
                    }

                    _groupMemberAssignmentService.Add( groupMemberAssignment );

                    registered.Add( registrant );
                    workflowMembers.Add( projectGroupMember );

                    if ( registrantsToMessage.Contains( registrant ) )
                    {
                        communicationRecipients.Add( groupMemberAssignment );
                    }
                }
            }

            if ( communicationUpdates.Any() )
            {
                foreach ( var communicationUpdate in communicationUpdates )
                {
                    var registrant = communicationUpdate.Registrant;

                    // If for some reason this registrant had a failed registration attempt, don't update anything, as it's likely
                    // we won't have the needed records in the database.
                    if ( unsuccessful.Contains( registrant ) )
                    {
                        continue;
                    }

                    communicationUpdate.ApplyUpdates( rockContext );
                }
            }

            try
            {
                rockContext.WrapTransaction( () =>
                {
                    // Initial save to release FK constraints tied to any referenced entities we'll be deleting.
                    rockContext.SaveChanges();

                    if ( groupMembersToDelete.Any() )
                    {
                        foreach ( var projectGroupMember in groupMembersToDelete )
                        {
                            if ( !registrationData.GroupType.EnableGroupHistory && !_groupMemberService.CanDelete( projectGroupMember, out string groupMemberErrorMessage ) )
                            {
                                // The registration (GroupMemberAssignment record itself will be deleted, but we cannot delete the corresponding GroupMember record.
                                continue;
                            }

                            // We need to delete these one-by-one, as the individual Delete call will dynamically archive if necessary (whereas the bulk delete calls will not).
                            _groupMemberService.Delete( projectGroupMember );
                        }

                        // Follow-up save for deleted, referenced entities.
                        rockContext.SaveChanges();
                    }
                } );
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );

                unsuccessful.AddRange( registered );
                registered.Clear();

                communicationRecipients.Clear();
                workflowMembers.Clear();
            }

            SendConfirmationCommunications( registrationData, communicationRecipients );
            LaunchWorkflow( registrationData, workflowMembers, registrarPerson );

            return new SignUpRegisterResponseBag
            {
                RegisteredRegistrantNames = registered.Select( r => r.FullName ).ToList(),
                UnregisteredRegistrantNames = unregistered.Select( r => r.FullName ).ToList(),
                UnsuccessfulRegistrantNames = unsuccessful.Select( r => r.FullName ).ToList(),
                WarningMessage = warningMessage
            };
        }

        /// <summary>
        /// Sends sign-up confirmation SystemCommunications to the specified registrants.
        /// </summary>
        /// <param name="registrationData">The registration data.</param>
        /// <param name="communicationRecipients">The <see cref="GroupMemberAssignment"/> instances for individuals that should receive a
        /// confirmation communication.</param>
        private void SendConfirmationCommunications( RegistrationData registrationData, List<GroupMemberAssignment> communicationRecipients )
        {
            var systemCommunicationGuid = GetAttributeValue( AttributeKey.RegistrantConfirmationSystemCommunication ).AsGuidOrNull();
            if ( !systemCommunicationGuid.HasValue )
            {
                return;
            }

            foreach ( var groupMemberAssignment in communicationRecipients )
            {
                var processMessage = new ProcessSendSignUpRegistrationConfirmation.Message
                {
                    GroupId = registrationData.Project.Id,
                    LocationId = registrationData.Location.Id,
                    ScheduleId = registrationData.Schedule.Id,
                    GroupMemberAssignmentId = groupMemberAssignment.Id,
                    SystemCommunicationGuid = systemCommunicationGuid.Value,
                    AppRoot = "/",
                    ThemeRoot = this.RequestContext.ResolveRockUrl( "~~/" )
                };

                processMessage.Send();
            }
        }

        /// <summary>
        /// Launches any workflow defined within this Block's settings.
        /// </summary>
        /// <param name="registrationData">The registration data.</param>
        /// <param name="workflowMembers">The <see cref="GroupMember"/> instances for individuals that were registered.</param>
        /// <param name="registrar">The <see cref="Person"/> representing the registrar, if any.</param>
        private void LaunchWorkflow( RegistrationData registrationData, List<GroupMember> workflowMembers, Person registrar )
        {
            var workflowTypeGuid = GetAttributeValue( AttributeKey.Workflow ).AsGuidOrNull();
            if ( !workflowTypeGuid.HasValue )
            {
                return;
            }

            var workflowTypeCache = WorkflowTypeCache.Get( workflowTypeGuid.Value );
            if ( workflowTypeCache == null || !workflowTypeCache.IsActive.GetValueOrDefault() )
            {
                return;
            }

            foreach ( var groupMember in workflowMembers )
            {
                var workflowAttributeValues = new Dictionary<string, string>
                {
                    { WorkflowAttributeKey.Registrar, registrar?.PrimaryAlias?.Guid.ToString() },
                    { WorkflowAttributeKey.Group, registrationData.Project.Guid.ToString() },
                    { WorkflowAttributeKey.Location, registrationData.Location.Guid.ToString() },
                    { WorkflowAttributeKey.Schedule, registrationData.Schedule.Guid.ToString() }
                };

                groupMember.LaunchWorkflow
                (
                    workflowTypeGuid,
                    $"{registrationData.Project.Name} Sign-Up Registration",
                    workflowAttributeValues,
                    registrar?.PrimaryAliasId
                );
            }
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Registers, updates or unregisters the specified registrants.
        /// </summary>
        /// <param name="bag">The bag that contains the information about the registrants to be registered, updated or unregistered.</param>
        /// <returns>An 200 OK result containing a <see cref="SignUpRegisterResponseBag"/> if registrants were successfully registered,
        /// updated or unregistered, or an error response if the attempt failed.</returns>
        [BlockAction]
        public BlockActionResult Register( SignUpRegisterRequestBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                // Load block attributes, as we're going to double-check that each registrant is allowed according to the settings.
                var block = new BlockService( rockContext ).Get( this.BlockId );
                block.LoadAttributes();

                var registrationData = GetRegistrationData( rockContext, true );
                if ( !registrationData.CanRegister )
                {
                    return ActionBadRequest( registrationData.ErrorMessage ?? $"{UnableToRegisterPrefix}." );
                }

                var response = TryProcessRegistrants( rockContext, registrationData, bag.Registrants, out string errorMessage );
                if ( !string.IsNullOrWhiteSpace( errorMessage ) )
                {
                    return ActionBadRequest( errorMessage ?? $"{UnableToRegisterPrefix}." );
                }

                return ActionOk( response );
            }
        }

        #endregion

        #region Supporting Classes

        /// <summary>
        /// A runtime object to represent a <see cref="Group"/>, <see cref="Rock.Model.Location"/> and
        /// <see cref="Rock.Model.Schedule"/> combination, for which registrations can be saved.
        /// </summary>
        private class RegistrationData
        {
            private readonly List<SignUpRegistrantBag> _registrants = new List<SignUpRegistrantBag>();

            /// <summary>
            /// Gets or sets any error that was encountered when determining whether this block is in a state to process registration requests.
            /// </summary>
            public string ErrorMessage { get; set; }

            /// <summary>
            /// Gets or sets the <see cref="Rock.Model.Group"/> for this sign-up project.
            /// </summary>
            public Rock.Model.Group Project { get; set; }

            /// <summary>
            /// Gets or sets whether this sign-up project has any required GroupRequirements.
            /// </summary>
            public bool ProjectHasRequiredGroupRequirements { get; set; }

            /// <summary>
            /// Gets or sets the <see cref="Rock.Model.GroupType"/> for this sign-up project occurrence.
            /// </summary>
            public GroupType GroupType { get; set; }

            /// <summary>
            /// Gets or sets the <see cref="Rock.Model.Location"/> for this sign-up project occurrence.
            /// </summary>
            public Location Location { get; set; }

            /// <summary>
            /// Gets or sets the <see cref="Rock.Model.Schedule"/> for this sign-up project occurrence.
            /// </summary>
            public Schedule Schedule { get; set; }

            /// <summary>
            /// Gets or sets the minimum participant count for this sign-up project occurrence.
            /// </summary>
            public int? SlotsMin { get; set; }

            /// <summary>
            /// Gets or sets the desired participant count for this sign-up project occurrence.
            /// </summary>
            public int? SlotsDesired { get; set; }

            /// <summary>
            /// Gets or sets the maximum participant count for this sign-up project occurrence.
            /// </summary>
            public int? SlotsMax { get; set; }

            /// <summary>
            /// Gets or sets the current participant count for this sign-up project occurrence.
            /// </summary>
            public int ParticipantCount { get; set; }

            /// <summary>
            /// Gets or sets the registration mode the block is in.
            /// </summary>
            public RegisterMode Mode { get; set; }

            /// <summary>
            /// Gets or sets the optional title to display above the register form.
            /// </summary>
            public string Title { get; set; }

            /// <summary>
            /// Gets or sets whether to require that a value be entered for email when registering in Anonymous mode.
            /// </summary>
            public bool RequireEmail { get; set; }

            /// <summary>
            /// Gets or sets whether to require that a value be entered for mobile phone when registering in Anonymous mode.
            /// </summary>
            public bool RequireMobilePhone { get; set; }

            /// <summary>
            /// Gets or sets any existing [GroupMember] project members (across all occurrences) that have already been saved to the database.
            /// </summary>
            public List<ExistingProjectMember> ExistingProjectMembers { get; set; }

            /// <summary>
            /// Gets or sets any existing [GroupMemberAssignment] registrations that have already been saved to the database.
            /// </summary>
            public List<GroupMemberAssignment> ExistingRegistrations { get; set; }

            /// <summary>
            /// Gets or sets the list of already-existing or possible registrants, including the registrar.
            /// <para>
            /// Each <see cref="SignUpRegistrantBag.WillAttend"/> value indicates whether they're already registered (<see langword="true" />) or available to be registered (<see langword="false" />).
            /// </para>
            /// </summary>
            public List<SignUpRegistrantBag> Registrants => _registrants;

            /// <summary>
            /// Gets whether the specified Schedule has a future start date/time.
            /// </summary>
            public bool ScheduleHasFutureStartDateTime
            {
                get
                {
                    return this.Schedule != null
                        && this.Schedule.NextStartDateTime.HasValue
                        && this.Schedule.NextStartDateTime.Value >= RockDateTime.Now;
                }
            }

            /// <summary>
            /// Gets the remaining participant slots available for this sign-up project occurrence.
            /// </summary>
            public int SlotsAvailable
            {
                get
                {
                    if ( !this.ScheduleHasFutureStartDateTime )
                    {
                        return 0;
                    }

                    // This more complex approach uses a dynamic/floating minuend:
                    // 1) If the max value is defined, use that;
                    // 2) Else, if the desired value is defined, use that;
                    // 3) Else, if the min value is defined, use that;
                    // 4) Else, use int.MaxValue (there is no limit to the slots available).
                    //var minuend = this.SlotsMax.GetValueOrDefault() > 0
                    //    ? this.SlotsMax.Value
                    //    : this.SlotsDesired.GetValueOrDefault() > 0
                    //        ? this.SlotsDesired.Value
                    //        : this.SlotsMin.GetValueOrDefault() > 0
                    //            ? this.SlotsMin.Value
                    //            : int.MaxValue;

                    // Simple approach:
                    // 1) If the max value is defined, subtract participant count from that;
                    // 2) Otherwise, use int.MaxValue (there is no limit to the slots available).
                    var available = int.MaxValue;
                    if ( this.SlotsMax.GetValueOrDefault() > 0 )
                    {
                        available = this.SlotsMax.Value - this.ParticipantCount;
                    }

                    return available < 0 ? 0 : available;
                }
            }

            /// <summary>
            /// Gets whether this block is in a state to process registration requests.
            /// </summary>
            public bool CanRegister
            {
                get
                {
                    return string.IsNullOrEmpty( this.ErrorMessage )
                        && this.Project != null
                        && this.GroupType != null
                        && this.Location != null
                        && this.ScheduleHasFutureStartDateTime;
                }
            }

            /// <summary>
            /// Gets whether this sign-up project has a reminder system communication configured.
            /// </summary>
            public bool HasReminderCommunicationConfigured
            {
                get
                {
                    return this.Project?.ReminderSystemCommunicationId.HasValue ?? false;
                }
            }
        }

        /// <summary>
        /// A runtime object to represent a <see cref="Rock.Model.GroupMember"/> who already belongs to this sign-up project <see cref="Group"/>.
        /// </summary>
        private class ExistingProjectMember
        {
            /// <summary>
            /// Gets or sets the <see cref="Rock.Model.GroupMember"/> for this existing sign-up project member.
            /// </summary>
            public GroupMember GroupMember { get; set; }

            /// <summary>
            /// Gets or sets the count of any additional occurrences (<see cref="Rock.Model.GroupMemberAssignment"/>s) within this sign-up project
            /// for which this member is already registered.
            /// </summary>
            public int AddlRegistrationsCount { get; set; }
        }

        /// <summary>
        /// A runtime object to represent a communication update (communication preference, email, mobile phone number) that needs to take place
        /// for a given registrant.
        /// <para>
        /// The reason we're using this object to house these updates (rather than simply applying the updated values directly to the referenced
        /// <see cref="Rock.Model.GroupMember"/> and <see cref="Rock.Model.Person"/> instances), is because of a somewhat disjointed registrant
        /// processing flow, where we might not yet have <see cref="Rock.Model.GroupMember"/> or <see cref="Rock.Model.Person"/> instances at the
        /// time we realize a communication update needs to take place. This object allows us to build a collection of such updates that need to
        /// eventually happen, and we can fill in the missing entity references as we move through the processing flow, then apply all needed
        /// communication updates at the end.
        /// </para>
        /// </summary>
        private class CommunicationUpdate
        {
            /// <summary>
            /// Gets or sets the <see cref="Rock.ViewModels.Blocks.Engagement.SignUp.SignUpRegister.SignUpRegistrantBag"/> that requires a communication update.
            /// </summary>
            public SignUpRegistrantBag Registrant { get; set; }

            /// <summary>
            /// Gets or sets the project <see cref="Rock.Model.GroupMember"/> that requires a communication update.
            /// </summary>
            public GroupMember ProjectGroupMember { get; set; }

            /// <summary>
            /// If defined, calling <see cref="ApplyUpdates"/> will update the <see cref="ProjectGroupMember"/>'s communication preference to this value.
            /// </summary>
            public CommunicationType? NewCommunicationPreference { get; set; }

            /// <summary>
            /// Gets or sets the <see cref="Rock.Model.Person"/> that requires a communication update.
            /// </summary>
            public Person Person { get; set; }

            /// <summary>
            /// If defined, calling <see cref="ApplyUpdates"/> will update the <see cref="Person"/>'s email to this value.
            /// </summary>
            public string NewEmail { get; set; }

            /// <summary>
            /// If <see cref="NewMobilePhoneNumber"/> is defined, calling <see cref="ApplyUpdates"/> will update
            /// the <see cref="Person"/>'s mobile phone country code to this value.
            /// </summary>
            public string NewMobilePhoneCountryCode { get; set; }

            /// <summary>
            /// If defined, calling <see cref="ApplyUpdates"/> will update the <see cref="Person"/>'s mobile phone number to this value.
            /// </summary>
            public string NewMobilePhoneNumber { get; set; }

            /// <summary>
            /// If defined, calling <see cref="ApplyUpdates"/> will update the <see cref="Person"/>'s mobile phone number's `IsMessagingEnabled` to this value.
            /// </summary>
            public bool? NewIsMessagingEnabled { get; set; }

            /// <summary>
            /// Applies in-memory updates to the <see cref="ProjectGroupMember"/> and <see cref="Person"/> instances, if defined.
            /// <para>
            /// Changes are not saved to the database; the <see cref="RockContext"/> is only used to retrieve an existing
            /// <see cref="Person"/> mobile phone number record if necessary.
            /// </para>
            /// </summary>
            /// <param name="rockContext">The rock context.</param>
            public void ApplyUpdates( RockContext rockContext )
            {
                if ( this.ProjectGroupMember != null && this.NewCommunicationPreference.HasValue )
                {
                    this.ProjectGroupMember.CommunicationPreference = this.NewCommunicationPreference.Value;
                }

                if ( this.Person == null )
                {
                    return;
                }

                if ( !string.IsNullOrEmpty( this.NewEmail ) )
                {
                    this.Person.Email = this.NewEmail;
                }

                if ( !string.IsNullOrEmpty( this.NewMobilePhoneNumber ) || this.NewIsMessagingEnabled.HasValue )
                {
                    var mobilePhoneDefinedValueCache = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE );

                    var mobilePhoneNumber = this.NewMobilePhoneNumber;
                    var mobilePhoneCountryCode = this.NewMobilePhoneCountryCode;
                    bool? isUnlisted = null;

                    if ( string.IsNullOrEmpty( this.NewMobilePhoneNumber ) )
                    {
                        // If we're only updating the `IsMessagingEnabled` value for this registrant, try to find their existing mobile phone record.
                        var existingMobilePhone = Person.GetPhoneNumber( mobilePhoneDefinedValueCache.Guid );
                        if ( existingMobilePhone == null )
                        {
                            // No mobile phone record to update.
                            return;
                        }

                        // Existing mobile phone number record found; in this case, simply pass these same values back to the update call,
                        // so we can update only the `IsMessagingEnabled` value.
                        mobilePhoneNumber = existingMobilePhone.Number;
                        mobilePhoneCountryCode = existingMobilePhone.CountryCode;
                        isUnlisted = existingMobilePhone.IsUnlisted;
                    }

                    this.Person.UpdatePhoneNumber
                    (
                        mobilePhoneDefinedValueCache.Id,
                        mobilePhoneCountryCode,
                        mobilePhoneNumber,
                        this.NewIsMessagingEnabled,
                        isUnlisted,
                        rockContext
                    );
                }
            }
        }

        #endregion
    }
}
