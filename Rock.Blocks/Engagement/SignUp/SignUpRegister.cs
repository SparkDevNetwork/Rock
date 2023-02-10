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
using Rock.Model;
using Rock.ViewModels.Blocks.Engagement.SignUp.SignUpRegister;
using Rock.ViewModels.Entities;
using Rock.Web.Cache;

namespace Rock.Blocks.Engagement.SignUp
{
    /// <summary>
    /// Block used to register for a sign-up group/project occurrence date time.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockObsidianBlockType" />

    [DisplayName( "Sign-Up Register" )]
    [Category( "Engagement > Sign-Up" )]
    [Description( "Block used to register for a sign-up group/project occurrence date time." )]
    [IconCssClass( "fa fa-clipboard-check" )]

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
        Description = "Workflow to launch when the sign-up is complete.",
        IsRequired = false,
        Order = 2 )]

    [SystemCommunicationField( "Registrant Confirmation System Communication",
        Key = AttributeKey.RegistrantConfirmationSystemCommunication,
        Description = "Confirmation email to be sent to each registrant (in Family mode, only send to adults and the child if they were the registrar).",
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
    public class SignUpRegister : RockObsidianBlockType
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

        #endregion

        #region Properties

        public override string BlockFileUrl => $"{base.BlockFileUrl}.obs";

        public bool IsAuthenticated
        {
            get
            {
                return RequestContext.CurrentUser?.IsAuthenticated == true;
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
            var block = new BlockService( rockContext ).Get( BlockId );
            block.LoadAttributes();

            var registrationData = GetRegistrationData( rockContext );
            if ( !registrationData.CanRegister )
            {
                box.ErrorMessage = registrationData.ErrorMessage ?? "Unable to register for this sign-up project.";
                return;
            }

            box.Mode = registrationData.Mode;
            box.DisplaySendReminderOption = registrationData.HasReminderCommunicationConfigured;
            box.RequireEmail = registrationData.RequireEmail;
            box.RequireMobilePhone = registrationData.RequireMobilePhone;
            box.Registrants = registrationData.Registrants;
        }

        /// <summary>
        /// Gets the registration data, built using a combination of page parameter values and existing database records.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The registration data, built using a combination of page parameter values and existing database records.</returns>
        private RegistrationData GetRegistrationData( RockContext rockContext )
        {
            var registrationData = new RegistrationData();

            var mode = GetAttributeValue( AttributeKey.Mode ).ConvertToEnum<RegisterMode>( RegisterMode.Anonymous );
            if ( !IsAuthenticated && mode != RegisterMode.Anonymous )
            {
                registrationData.ErrorMessage = "You must be logged in to register for this sign-up project.";
                return registrationData;
            }

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

            if ( mode == RegisterMode.Anonymous )
            {
                // Try to pre-populate the registrant info for the current person.
                if ( RequestContext.CurrentPerson != null )
                {
                    var registrant = CreateRegistrant( RequestContext.CurrentPerson );
                    registrationData.Registrants.Add( registrant );
                }
            }
            else
            {
                if ( !TryGetRegistrants( rockContext, registrationData, groupModeGroupId ) )
                {
                    // An error message will have been added.
                    return registrationData;
                }

                GetUnmetGroupRequirements( rockContext, registrationData );
            }

            return registrationData;
        }

        /// <summary>
        /// Tries the get the <see cref="Group"/>, <see cref="Location"/> & <see cref="Schedule"/> instances for this registration, loading them onto the <see cref="RegistrationData"/> instance.
        /// </summary>
        /// <param name="registrationData">The registration data.</param>
        /// <param name="projectId">The project identifier.</param>
        /// <param name="locationId">The location identifier.</param>
        /// <param name="scheduleId">The schedule identifier.</param>
        /// <returns>Whether <see cref="Group"/>, <see cref="Location"/> & <see cref="Schedule"/> instances were successfully loaded for this registration.</returns>
        private bool TryGetGroupLocationSchedule( RockContext rockContext, RegistrationData registrationData, int projectId, int locationId, int scheduleId )
        {
            var signUpGroupTypeId = GroupTypeCache.GetId( Rock.SystemGuid.GroupType.GROUPTYPE_SIGNUP_GROUP.AsGuid() ).ToIntSafe();

            // Get the active (and valid sign-up group type) opportunities tied to this group and location.
            var groupLocationSchedules = new GroupLocationService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Where( gl =>
                    gl.Group.IsActive
                    && gl.Group.Id == projectId
                    && gl.Location.Id == locationId
                    && ( gl.Group.GroupTypeId == signUpGroupTypeId || gl.Group.GroupType.InheritedGroupTypeId == signUpGroupTypeId )
                )
                .SelectMany( gl => gl.Schedules, ( gl, s ) => new
                {
                    gl.Group,
                    gl.Location,
                    Schedule = s
                } )
                .ToList();

            // Ensure the requested schedule exists.
            var schedule = groupLocationSchedules
                .Where( gls => gls.Schedule?.Id == scheduleId )
                .Select( gls => gls.Schedule )
                .FirstOrDefault();

            if ( schedule == null )
            {
                registrationData.ErrorMessage = "Project occurrence not found.";
                return false;
            }

            registrationData.Project = groupLocationSchedules.Select( gls => gls.Group ).First();
            registrationData.Location = groupLocationSchedules.Select( gls => gls.Location ).First();
            registrationData.Schedule = schedule;

            return true;
        }

        /// <summary>
        /// Gets the list of existing or possible registrants, loading them onto the <see cref="RegistrationData"/> instance.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="registrationData">The registration data.</param>
        /// <param name="groupModeGroupId">The group identifier to use if in Group mode.</param>
        /// <returns>Whether registrants were successfully loaded for this registration.</returns>
        private bool TryGetRegistrants( RockContext rockContext, RegistrationData registrationData, int? groupModeGroupId )
        {
            var currentPersonId = RequestContext.CurrentPerson?.Id;
            if ( !currentPersonId.HasValue )
            {
                registrationData.ErrorMessage = "Unable to register logged-in individual.";
                return false;
            }

            IQueryable<GroupMember> qryGroupMembers;

            if ( !groupModeGroupId.HasValue )
            {
                // Family mode: build a list of registrants from the members of this family.
                qryGroupMembers = RequestContext.CurrentPerson.GetFamilyMembers( includeSelf: true, rockContext );
            }
            else
            {
                // Group mode: build a list of registrants from the members of this group.
                qryGroupMembers = new GroupMemberService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Include( gm => gm.Person )
                    .Where( gm =>
                        gm.Group.IsActive
                        && gm.Group.Id == groupModeGroupId.Value
                        && !gm.Person.IsDeceased
                    );

                if ( !qryGroupMembers.Any( gm => gm.PersonId == currentPersonId ) )
                {
                    registrationData.ErrorMessage = "The logged-in individual does not belong to the specified group for Group Mode registration.";
                    return false;
                }
            }

            if ( !GetAttributeValue( AttributeKey.IncludeChildren).AsBoolean() )
            {
                // Remove children (unless a child is the registrar; then leave that particular child in the collection).
                qryGroupMembers = qryGroupMembers
                    .Where( gm => gm.PersonId == currentPersonId || gm.Person.AgeClassification != AgeClassification.Child );
            }

            // Execute the initial query to ensure the current person belongs to the specified group, before getting existing registrations.
            var groupMembers = qryGroupMembers.ToList();

            // Get any existing registrations for this project occurrence.
            var existingRegistrations = new GroupMemberAssignmentService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Include( gma => gma.GroupMember )
                .Where( gma =>
                    !gma.GroupMember.Person.IsDeceased
                    && gma.GroupMember.GroupId == registrationData.Project.Id
                    && gma.LocationId == registrationData.Location.Id
                    && gma.ScheduleId == registrationData.Schedule.Id
                )
                .ToList();

            // Create a registrant entry for each group member.
            foreach ( var groupMember in groupMembers )
            {
                var existingRegistration = existingRegistrations.FirstOrDefault( gma => gma.GroupMember.PersonId == groupMember.PersonId );

                var registrant = CreateRegistrant( groupMember.Person );
                registrant.CommunicationPreference = ( int ) groupMember.CommunicationPreference;
                registrant.WillAttend = existingRegistration != null;

                registrationData.Registrants.Add( registrant );
            }

            return true;
        }

        /// <summary>
        /// Creates a <see cref="SignUpRegistrantBag"/> instance from the provided <see cref="Person"/>.
        /// </summary>
        /// <param name="person">The <see cref="Person"/>.</param>
        /// <returns>A <see cref="SignUpRegistrantBag"/>.</returns>
        private SignUpRegistrantBag CreateRegistrant( Person person )
        {
            PhoneNumberBag mobilePhoneBag = null;

            var mobilePhone = person.GetPhoneNumber( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );
            if ( mobilePhone != null )
            {
                mobilePhoneBag = mobilePhone.ToViewModel();
            }

            return new SignUpRegistrantBag
            {
                PersonId = person.Id,
                FirstName = person.NickName,
                LastName = person.LastName,
                Email = person.Email,
                MobilePhone = mobilePhoneBag
            };
        }

        /// <summary>
        /// Gets the unmet group requirements - if any - for each registrant.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="registrationData">The registration data.</param>
        private void GetUnmetGroupRequirements( RockContext rockContext, RegistrationData registrationData )
        {

        }

        #endregion

        #region Supporting Classes

        /// <summary>
        /// A runtime object to represent a <see cref="Group"/>, <see cref="Location"/> & <see cref="Schedule"/> combination,
        /// for which registrations can be saved.
        /// </summary>
        private class RegistrationData
        {
            private List<SignUpRegistrantBag> _registrants = new List<SignUpRegistrantBag>();

            public string ErrorMessage { get; set; }

            public Group Project { get; set; }

            public Location Location { get; set; }

            public Schedule Schedule { get; set; }

            public RegisterMode Mode { get; set; }

            public bool RequireEmail { get; set; }

            public bool RequireMobilePhone { get; set; }

            public List<SignUpRegistrantBag> Registrants => _registrants;

            public bool CanRegister
            {
                get
                {
                    return string.IsNullOrEmpty( this.ErrorMessage )
                        && this.Project != null;
                }
            }

            public bool HasReminderCommunicationConfigured
            {
                get
                {
                    return this.Project?.ReminderSystemCommunicationId.HasValue ?? false;
                }
            }
        }

        #endregion
    }
}
