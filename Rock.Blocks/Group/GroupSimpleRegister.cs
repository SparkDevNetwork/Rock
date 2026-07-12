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
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;

using Rock.Attribute;
using Rock.Communication;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Blocks.Group.GroupSimpleRegister;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Group
{
    /// <summary>
    /// Prompts for name and email, creates a person record if none exists, and adds the person to a group.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Group Simple Register" )]
    [Category( "Group" )]
    [Description( "Prompts for name and email, creates a person record if none exists, and adds the person to a group." )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [GroupField( "Group",
        Key = AttributeKey.Group,
        Description = "The group to add people to.",
        IsRequired = true,
        Order = 0 )]

    [SystemCommunicationField( "Confirmation Email",
        Key = AttributeKey.ConfirmationEmail,
        Description = "The email to send the person to confirm their registration. If not specified, the user will not need to confirm their registration.",
        IsRequired = false,
        Order = 1 )]

    [LinkedPage( "Confirmation Page",
        Key = AttributeKey.ConfirmationPage,
        Description = "The page that the user should be directed to in order to confirm their registration.",
        IsRequired = true,
        Order = 2 )]

    [DefinedValueField( "Connection Status",
        Key = AttributeKey.ConnectionStatus,
        Description = "The connection status to use for new individuals (default: 'Prospect').",
        IsRequired = true,
        AllowMultiple = false,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS,
        DefaultValue = Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_PROSPECT,
        Order = 3 )]

    [DefinedValueField( "Record Status",
        Key = AttributeKey.RecordStatus,
        Description = "The record status to use for new individuals (default: 'Pending').",
        IsRequired = true,
        AllowMultiple = false,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS,
        DefaultValue = Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING,
        Order = 4 )]

    [DefinedValueField( "Record Source",
        Key = AttributeKey.RecordSource,
        Description = "The record source to use for new individuals (default: 'Group Registration').",
        IsRequired = true,
        AllowMultiple = false,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.RECORD_SOURCE_TYPE,
        DefaultValue = Rock.SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_GROUP_REGISTRATION,
        Order = 5 )]

    [BooleanField( "Disable Captcha Support",
        Key = AttributeKey.DisableCaptchaSupport,
        Description = "If set to 'Yes' the CAPTCHA verification step will not be performed.",
        DefaultBooleanValue = false,
        Order = 6 )]

    [BooleanField( "Load Current Person from Page",
        Key = AttributeKey.LoadPerson,
        Description = "If set to true the form will autopopulate fields from the person profile.",
        DefaultBooleanValue = false,
        Order = 7 )]

    [TextField( "Save Button Text",
        Key = AttributeKey.SaveButtonText,
        Description = "The text to use for the Save button.",
        IsRequired = false,
        DefaultValue = "Save",
        Order = 8 )]

    [TextField( "Success Message",
        Key = AttributeKey.SuccessMessage,
        Description = "The message to display when the user is successfully added to the group.",
        IsRequired = false,
        DefaultValue = "Please check your email to verify your registration",
        Order = 9 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "A36E2804-78CC-41BE-9606-A5534ABF8B09" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "60E45285-337D-44E5-8DC4-6FD0DA0AF902" )]
    [Rock.SystemGuid.BlockTypeGuid( "82A285C1-0D6B-41E0-B1AA-DD356021BDBF" )]
    public class GroupSimpleRegister : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string Group = "Group";
            public const string ConfirmationEmail = "ConfirmationEmail";
            public const string ConfirmationPage = "ConfirmationPage";
            public const string ConnectionStatus = "ConnectionStatus";
            public const string RecordStatus = "RecordStatus";
            public const string RecordSource = "RecordSource";
            public const string DisableCaptchaSupport = "DisableCaptchaSupport";
            public const string LoadPerson = "LoadPerson";
            public const string SaveButtonText = "SaveButtonText";
            public const string SuccessMessage = "SuccessMessage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var options = new GroupSimpleRegisterOptionsBag
            {
                Registrant = new GroupSimpleRegisterBag(),
                SaveButtonText = GetAttributeValue( AttributeKey.SaveButtonText ),
                DisableCaptchaSupport = Captcha.CaptchaService.ShouldDisableCaptcha( GetAttributeValue( AttributeKey.DisableCaptchaSupport ).AsBoolean() ),
                ErrorMessage = GetConfigurationError()
            };

            // Prefill the form from the current person when the block is configured to do so.
            var currentPerson = RequestContext.CurrentPerson;
            if ( GetAttributeValue( AttributeKey.LoadPerson ).AsBoolean() && currentPerson != null )
            {
                options.Registrant.FirstName = currentPerson.FirstName;
                options.Registrant.LastName = currentPerson.LastName;
                options.Registrant.Email = currentPerson.Email;
            }

            return options;
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Registers the individual by finding or creating their person record, adding them to the
        /// configured group, and optionally sending a confirmation email.
        /// </summary>
        /// <param name="bag">The information entered on the registration form.</param>
        /// <returns>
        /// The success message (HTTP 200) when the person is registered, or the confirmation page
        /// URL (HTTP 201) when the person is already an active member and should be redirected.
        /// </returns>
        [BlockAction]
        public BlockActionResult Register( GroupSimpleRegisterBag bag )
        {
            var disableCaptcha = Captcha.CaptchaService.ShouldDisableCaptcha( GetAttributeValue( AttributeKey.DisableCaptchaSupport ).AsBoolean() );
            if ( !disableCaptcha && !RequestContext.IsCaptchaValid )
            {
                return ActionBadRequest( "CAPTCHA verification failed. Please try again." );
            }

            if ( bag == null
                || bag.FirstName.IsNullOrWhiteSpace()
                || bag.LastName.IsNullOrWhiteSpace()
                || bag.Email.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Please enter a value for First Name, Last Name, and Email." );
            }

            // Re-validate the name characters on the server. The form enforces the same rules
            // client-side, but this block accepts public (often anonymous) submissions, so a
            // crafted request that bypasses the form must not be able to persist a person with
            // disallowed characters in their name.
            if ( Regex.IsMatch( bag.FirstName, RegexPatterns.SpecialCharacterRemovalPattern )
                || Regex.IsMatch( bag.LastName, RegexPatterns.SpecialCharacterRemovalPattern ) )
            {
                return ActionBadRequest( "First and Last Name cannot contain special characters such as quotes, parentheses, etc." );
            }

            if ( Regex.IsMatch( bag.FirstName, RegexPatterns.EmojiAndSpecialFontRemovalPattern )
                || Regex.IsMatch( bag.LastName, RegexPatterns.EmojiAndSpecialFontRemovalPattern ) )
            {
                return ActionBadRequest( "First and Last Name cannot contain emojis or special fonts." );
            }

            // Validate the group configuration before creating any records so that a misconfigured
            // block cannot leave an orphaned person behind.
            var group = GetConfiguredGroup();
            if ( group == null || !group.GroupType.DefaultGroupRoleId.HasValue )
            {
                return ActionBadRequest( "The configured group does not exist, or its group type does not have a default role configured." );
            }

            // Confirmation links depend on the Confirmation Page being set. The form is hidden at
            // initialization when this is missing, but guard here as well so a crafted request cannot
            // reach the email or redirect paths with an unbuildable URL. Mirrors the WebForms submit check.
            if ( GetAttributeValue( AttributeKey.ConfirmationPage ).IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "The Confirmation Page setting is not valid." );
            }

            var person = FindOrCreatePerson( bag, group );

            var confirmationEmailGuid = GetAttributeValue( AttributeKey.ConfirmationEmail ).AsGuidOrNull();

            var groupMemberService = new GroupMemberService( RockContext );
            var member = groupMemberService.Queryable()
                .FirstOrDefault( m => m.GroupId == group.Id && m.PersonId == person.Id );

            // A person who has already confirmed their registration is sent directly to the confirmation page.
            if ( member != null && member.GroupMemberStatus == GroupMemberStatus.Active )
            {
                var activeMemberUrl = this.GetLinkedPageUrl( AttributeKey.ConfirmationPage, "GM", member.UrlEncodedKey );
                return ActionContent( System.Net.HttpStatusCode.Created, activeMemberUrl );
            }

            // Add the member when they are new to the group.
            if ( member == null )
            {
                member = new GroupMember
                {
                    GroupId = group.Id,
                    PersonId = person.Id,
                    GroupRoleId = group.GroupType.DefaultGroupRoleId.Value,

                    // Require confirmation when a confirmation email is configured; otherwise activate immediately.
                    GroupMemberStatus = confirmationEmailGuid.HasValue ? GroupMemberStatus.Pending : GroupMemberStatus.Active
                };

                groupMemberService.Add( member );
                RockContext.SaveChanges();
            }

            if ( confirmationEmailGuid.HasValue )
            {
                SendConfirmationEmail( confirmationEmailGuid.Value, person, member );
            }

            return ActionOk( GetAttributeValue( AttributeKey.SuccessMessage ) );
        }

        #endregion Block Actions

        #region Private Methods

        /// <summary>
        /// Validates the block configuration and returns a message describing the first problem
        /// found, or <c>null</c> when the block is configured correctly.
        /// </summary>
        private string GetConfigurationError()
        {
            var group = GetConfiguredGroup();
            if ( group == null || !group.GroupType.DefaultGroupRoleId.HasValue )
            {
                return "The configured group does not exist, or its group type does not have a default role configured.";
            }

            if ( GetAttributeValue( AttributeKey.ConfirmationPage ).IsNullOrWhiteSpace() )
            {
                return "The Confirmation Page setting is not valid.";
            }

            if ( DefinedValueCache.Get( GetAttributeValue( AttributeKey.ConnectionStatus ).AsGuid() ) == null )
            {
                return "The Connection Status setting is not valid.";
            }

            if ( DefinedValueCache.Get( GetAttributeValue( AttributeKey.RecordStatus ).AsGuid() ) == null )
            {
                return "The Record Status setting is not valid.";
            }

            if ( DefinedValueCache.Get( GetAttributeValue( AttributeKey.RecordSource ).AsGuid() ) == null )
            {
                return "The Record Source setting is not valid.";
            }

            return null;
        }

        /// <summary>
        /// Gets the group configured on the block, or <c>null</c> when it is not set or cannot be found.
        /// </summary>
        private Rock.Model.Group GetConfiguredGroup()
        {
            var groupGuid = GetAttributeValue( AttributeKey.Group ).AsGuidOrNull();
            if ( !groupGuid.HasValue )
            {
                return null;
            }

            return new GroupService( RockContext ).Get( groupGuid.Value );
        }

        /// <summary>
        /// Finds an existing person matching the submitted information, or creates a new person
        /// (and family) when no match is found.
        /// </summary>
        /// <param name="bag">The information entered on the registration form.</param>
        /// <param name="group">The group the person is registering for; its campus is used for a new family.</param>
        private Person FindOrCreatePerson( GroupSimpleRegisterBag bag, Rock.Model.Group group )
        {
            var personService = new PersonService( RockContext );

            /*
                7/10/26 - MSE

                The matched person's primary email is intentionally not updated (updatePrimaryEmail: false).
                This block accepts public, often anonymous submissions, so allowing unauthenticated input to
                overwrite an existing person's primary email would be a data-integrity risk. This also
                preserves the original WebForms behavior, which matched on the email and never mutated it.

                Reason: Prevent anonymous input from overwriting an existing person's primary email.
            */
            var matchQuery = new PersonService.PersonMatchQuery( bag.FirstName.Trim(), bag.LastName.Trim(), bag.Email.Trim(), null );
            var person = personService.FindPerson( matchQuery, updatePrimaryEmail: false );
            if ( person != null )
            {
                return person;
            }

            var connectionStatus = DefinedValueCache.Get( GetAttributeValue( AttributeKey.ConnectionStatus ).AsGuid() );
            var recordStatus = DefinedValueCache.Get( GetAttributeValue( AttributeKey.RecordStatus ).AsGuid() );
            var recordSource = DefinedValueCache.Get( GetAttributeValue( AttributeKey.RecordSource ).AsGuid() );

            person = new Person
            {
                FirstName = bag.FirstName.Trim(),
                LastName = bag.LastName.Trim(),
                Email = bag.Email.Trim(),
                IsEmailActive = true,
                EmailPreference = EmailPreference.EmailAllowed,
                RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id,
                ConnectionStatusValueId = connectionStatus?.Id,
                RecordStatusValueId = recordStatus?.Id,
                RecordSourceValueId = recordSource?.Id,
                Gender = Gender.Unknown
            };

            PersonService.SaveNewPerson( person, RockContext, group.CampusId, false );

            return person;
        }

        /// <summary>
        /// Sends the registration confirmation email for the given group member.
        /// </summary>
        /// <param name="confirmationEmailGuid">The system communication template to send.</param>
        /// <param name="person">The person to send the confirmation to.</param>
        /// <param name="member">The group member being confirmed; supplies the confirmation link key.</param>
        private void SendConfirmationEmail( Guid confirmationEmailGuid, Person person, GroupMember member )
        {
            var mergeFields = RequestContext.GetCommonMergeFields( RequestContext.CurrentPerson );
            mergeFields.Add( "Member", member );
            mergeFields.Add( "ConfirmationPage", this.GetLinkedPageUrl( AttributeKey.ConfirmationPage, "GM", member.UrlEncodedKey ) );

            var emailMessage = new RockEmailMessage( confirmationEmailGuid );
            emailMessage.AddRecipient( new RockEmailMessageRecipient( person, mergeFields ) );
            emailMessage.AppRoot = RequestContext.ResolveRockUrl( "~/" );
            emailMessage.ThemeRoot = RequestContext.ResolveRockUrl( "~~/" );
            emailMessage.Send();
        }

        #endregion Private Methods
    }
}
