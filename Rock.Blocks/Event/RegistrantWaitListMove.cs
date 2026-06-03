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

using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Blocks.Event.RegistrantWaitListMove;
using Rock.Web.Cache;

namespace Rock.Blocks.Event
{
    /// <summary>
    /// Moves registrants from the wait list to active status for a registration
    /// instance and optionally sends them a confirmation email.
    /// </summary>
    [DisplayName( "Registrant Wait List Move" )]
    [Category( "Event" )]
    [Description( "Moves the person from the wait list to be a full registrant." )]
    [IconCssClass( "fa fa-clipboard-check" )]
    [SupportedSiteTypes( Model.SiteType.Web )]
    [Rock.SystemGuid.EntityTypeGuid( "7700303a-45e2-4fbf-9c10-c7f3726f529e" )]
    // WAS [Rock.SystemGuid.BlockTypeGuid( "7733cb25-df4d-4ffe-a898-ff6a03858799" )]
    [Rock.SystemGuid.BlockTypeGuid( "AAD07299-F30F-4DB2-8E04-5F3369CE46D2" )]
    public class RegistrantWaitListMove : RockBlockType
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string EntitySetId = "WaitListSetId";
        }

        private static class MergeFieldKey
        {
            public const string RegistrationInstance = "RegistrationInstance";
            public const string Registration = "Registration";
            public const string TransitionedRegistrants = "TransitionedRegistrants";
            public const string AdditionalFieldsNeeded = "AdditionalFieldsNeeded";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var bag = new RegistrantWaitListMoveBag();

            var registrants = GetRegistrantsFromEntitySet();

            if ( registrants == null || !registrants.Any() )
            {
                bag.UpdateMessage = "No registrants found.";
                return bag;
            }

            MoveRegistrantsOffWaitList( registrants );

            bag.UpdateMessage = registrants.Count == 1
                ? "1 individual was moved from the wait list."
                : $"{registrants.Count} individuals were moved from the wait list.";

            var firstRegistration = registrants
                .Where( r => r.Registration != null )
                .Select( r => r.Registration )
                .FirstOrDefault();

            var template = registrants.Where( r =>
                        r.Registration != null &&
                        r.Registration.RegistrationInstance != null &&
                        r.Registration.RegistrationInstance.RegistrationTemplate != null )
                    .Select( r => r.Registration.RegistrationInstance.RegistrationTemplate )
                    .FirstOrDefault();

            bag.Recipients = BuildRecipientBags( registrants );

            // build email bag based on first registration if valid email template is present
            if ( firstRegistration != null && template != null )
            {
                var movedRegistrants = registrants.Where( r => r.RegistrationId == firstRegistration.Id ).ToList();
                var mergeFields = BuildMergeFields( firstRegistration, firstRegistration.RegistrationInstance, movedRegistrants );
                bag.EmailBag = ResolveEmailTemplate( template, mergeFields );
            }


            return bag;
        }

        /// <summary>
        /// Loads the list of <see cref="RegistrationRegistrant"/> records from the
        /// entity set identified by the <see cref="PageParameterKey.EntitySetId"/> page parameter.
        /// Returns an empty list when the parameter is missing or the set cannot be found.
        /// </summary>
        private List<RegistrationRegistrant> GetRegistrantsFromEntitySet()
        {
            var entitySetId = PageParameter( PageParameterKey.EntitySetId ).AsIntegerOrNull();

            if ( !entitySetId.HasValue )
            {
                return null;
            }

            var registrantIds = new EntitySetItemService( RockContext ).Queryable().AsNoTracking()
                .Where( i => i.EntitySetId == entitySetId )
                .Select( i => i.EntityId )
                .ToList();

            var registrants = new RegistrationRegistrantService( RockContext )
                .Queryable()
                .Include( r => r.PersonAlias.Person )
                .Include( r => r.Registration.PersonAlias.Person )
                .Include( r => r.Registration.RegistrationInstance.RegistrationTemplate.Fees )
                .Include( r => r.Registration.RegistrationInstance.RegistrationTemplate.Forms.Select( f => f.Fields ) )
                .Include( r => r.Registration.Group.GroupType.Roles )
                .Include( r => r.Registration.Registrants.Select( reg => reg.Fees ) )
                .Where( r => registrantIds.Contains( r.Id ) )
                .ToList();

            return registrants;
        }

        /// <summary>
        /// Moves each registrant in <paramref name="registrants"/> from wait-list
        /// status to active, adds them to any group associated with the registration
        /// instance, and updates <c>LastPaymentReminderDateTime</c> on the parent
        /// <see cref="Registration"/>. Persists all changes via <see cref="RockContext"/>.
        /// </summary>
        private void MoveRegistrantsOffWaitList( List<RegistrationRegistrant> registrants )
        {
            foreach ( var registrant in registrants )
            {
                AddRegistrantToGroup( registrant );
                registrant.OnWaitList = false;
            }

            RockContext.SaveChanges();

            var now = RockDateTime.Now;
            var registrationIds = registrants
                .Select( r => r.RegistrationId )
                .Distinct()
                .ToList();

            // LastPaymentReminder N SQL calls per registration so we need to bulk update.
            RockContext.BulkUpdate(
                new RegistrationService( RockContext ).Queryable().Where( r => registrationIds.Contains( r.Id ) ),
                r => new Registration { LastPaymentReminderDateTime = now } );
        }

        /// <summary>
        /// Adds <paramref name="registrant"/> to the group linked to the registration
        /// instance, using the appropriate group role and member status from the template,
        /// and links the registrant to that group member. A no-op when no group is configured.
        /// </summary>
        private void AddRegistrantToGroup( RegistrationRegistrant registrant )
        {
            var template = registrant.Registration?.RegistrationInstance?.RegistrationTemplate;

            if ( !registrant.PersonAliasId.HasValue ||
                registrant.Registration?.Group == null ||
                registrant.Registration.Group.GroupType == null ||
                template == null )
            {
                return;
            }

            var group = registrant.Registration.Group;
            var groupMemberService = new GroupMemberService( RockContext );

            var personId = registrant.PersonAlias.PersonId;
            var groupMember = group.Members.FirstOrDefault( m => m.PersonId == personId );

            if ( groupMember == null )
            {
                groupMember = new GroupMember
                {
                    GroupId = group.Id,
                    PersonId = personId
                };

                if ( template.GroupTypeId.HasValue &&
                    template.GroupTypeId == group.GroupTypeId &&
                    template.GroupMemberRoleId.HasValue )
                {
                    groupMember.GroupRoleId = template.GroupMemberRoleId.Value;
                }
                else if ( group.GroupType.DefaultGroupRoleId.HasValue )
                {
                    groupMember.GroupRoleId = group.GroupType.DefaultGroupRoleId.Value;
                }
                else
                {
                    groupMember.GroupRoleId = group.GroupType.Roles.Select( r => r.Id ).FirstOrDefault();
                }

                groupMemberService.Add( groupMember );
            }

            groupMember.GroupMemberStatus = template.GroupMemberStatus;

            registrant.GroupMember = groupMember;
        }

        /// <summary>
        /// Builds the Lava merge-field dictionary for a single <paramref name="registration"/>
        /// including the <c>RegistrationInstance</c>, <c>Registration</c>,
        /// <c>TransitionedRegistrants</c>, and the <c>AdditionalFieldsNeeded</c> flag.
        /// </summary>
        private Dictionary<string, object> BuildMergeFields( Registration registration, RegistrationInstance registrationInstance, List<RegistrationRegistrant> transitionedRegistrants )
        {
            var additionalFieldsNeeded = registrationInstance.RegistrationTemplate.Forms
                .SelectMany( f => f.Fields )
                .Any( f => !f.ShowOnWaitlist );

            if ( !additionalFieldsNeeded )
            {
                additionalFieldsNeeded = registrationInstance.RegistrationTemplate.Fees.Any();
            }

            return new Dictionary<string, object>
            {
                { MergeFieldKey.RegistrationInstance, registrationInstance },
                { MergeFieldKey.Registration, registration },
                { MergeFieldKey.TransitionedRegistrants, transitionedRegistrants },
                { MergeFieldKey.AdditionalFieldsNeeded, additionalFieldsNeeded }
            };
        }

        /// <summary>
        /// Groups <paramref name="registrants"/> by their parent <see cref="Registration"/>
        /// and returns a list of <see cref="RegistrantWaitListMoveRecipientBag"/> suitable
        /// for display in the email recipient list. Email template fields are resolved
        /// against each registration's merge fields using the first registrant as a sample.
        /// </summary>
        private List<RegistrantWaitListMoveRecipientBag> BuildRecipientBags( List<RegistrationRegistrant> registrants )
        {
            var recipients = registrants
                .Where( r => r.Registration != null )
                .GroupBy( r => r.Registration )
                .Select( g => new RegistrantWaitListMoveRecipientBag
                {
                    Id = g.Key.Id,
                    RegistrantName = $"{g.Key.FirstName} {g.Key.LastName}",
                    Email = g.Key.ConfirmationEmail,
                    IsSelected = true,
                    RegisteredNames = g.Select( r => r.PersonAlias?.Person?.FullName ?? string.Empty ).ToList()
                } )
                .ToList();

            return recipients;
        }

        /// <summary>
        /// Resolves the email template fields (from name, from email, subject, body)
        /// from the <see cref="RegistrationTemplate"/> against the supplied
        /// <paramref name="mergeFields"/>.
        /// </summary>
        private RegistrantWaitListMoveEmailBag ResolveEmailTemplate( RegistrationTemplate template, Dictionary<string, object> mergeFields )
        {
            var body = template.WaitListTransitionEmailTemplate ?? string.Empty;

            return new RegistrantWaitListMoveEmailBag
            {
                FromName = template.WaitListTransitionFromName?.ResolveMergeFields( mergeFields ) ?? string.Empty,
                FromEmail = template.WaitListTransitionFromEmail?.ResolveMergeFields( mergeFields ) ?? string.Empty,
                Subject = template.WaitListTransitionSubject?.ResolveMergeFields( mergeFields ) ?? string.Empty,
                Body = body,
                PreviewHtml = body.ResolveMergeFields( mergeFields )
            };
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Resolves the supplied <paramref name="messageBody"/> against the first
        /// registration's Lava merge fields and returns the rendered HTML for the
        /// preview iframe. Re-fetches the registrants from the entity set so the
        /// action is stateless.
        /// </summary>
        [BlockAction]
        public BlockActionResult ResolvePreview( string messageBody )
        {
            if ( messageBody.IsNullOrWhiteSpace() )
            {
                return ActionOk( string.Empty );
            }

            var registrants = GetRegistrantsFromEntitySet();

            if ( registrants == null || !registrants.Any() )
            {
                return ActionOk( messageBody );
            }

            var firstRegistration = registrants
                .Where( r => r.Registration != null )
                .Select( r => r.Registration )
                .FirstOrDefault();

            if ( firstRegistration == null )
            {
                return ActionOk( messageBody );
            }

            var transitionedRegistrants = registrants
                .Where( r => r.RegistrationId == firstRegistration.Id )
                .ToList();

            var mergeFields = BuildMergeFields( firstRegistration, firstRegistration.RegistrationInstance, transitionedRegistrants );
            var resolvedHtml = messageBody.ResolveMergeFields( mergeFields );

            return ActionOk( resolvedHtml );
        }

        /// <summary>
        /// Sends confirmation emails to the selected recipients using the supplied
        /// from/subject/body overrides. Each email is sent and saved in its own
        /// <see cref="RockContext"/> to avoid change-tracker bloat on large batches.
        /// Returns a summary message with sent and error counts.
        /// </summary>
        [BlockAction]
        public BlockActionResult SendEmails( RegistrantWaitListMoveEmailBag request )
        {
            var sendCount = 0;
            var errorMessage = "";

            var registrationIds = request.RegistrationIds ?? new List<int>();

            var registrations = new RegistrationService( RockContext )
                .Queryable()
                .Include( r => r.Registrants.Select( reg => reg.Fees ) )
                .Include( r => r.RegistrationInstance.RegistrationTemplate.Forms.Select( f => f.Fields ) )
                .Include( r => r.RegistrationInstance.RegistrationTemplate.Fees )
                .Where( r => registrationIds.Contains( r.Id ) )
                .ToList();

            if ( !registrations.Any() )
            {
                return ActionBadRequest( "No valid registrations found for the selected recipients." );
            }

            var transitionedByRegistration = ( GetRegistrantsFromEntitySet() ?? new List<RegistrationRegistrant>() )
                .GroupBy( r => r.RegistrationId )
                .ToDictionary( g => g.Key, g => g.ToList() );

            var appRoot = GlobalAttributesCache.Value( "PublicApplicationRoot" );
            var themeRoot = RequestContext.ResolveRockUrl( "~~/" );

            foreach ( var registration in registrations )
            {
                if ( registration.ConfirmationEmail.IsNullOrWhiteSpace() )
                {
                    continue;
                }

                var transitionedRegistrants = transitionedByRegistration.TryGetValue( registration.Id, out var moved )
                    ? moved
                    : registration.Registrants.ToList();

                var mergeFields = BuildMergeFields( registration, registration.RegistrationInstance, transitionedRegistrants );

                var emailMessage = new RockEmailMessage
                {
                    FromName = request.FromName,
                    FromEmail = request.FromEmail,
                    Subject = request.Subject,
                    Message = request.Body,
                    AdditionalMergeFields = mergeFields,
                    AppRoot = appRoot,
                    ThemeRoot = themeRoot,
                    CreateCommunicationRecord = true
                };

                emailMessage.AddRecipient( registration.GetConfirmationRecipient( mergeFields ) );
                if ( emailMessage.Send( out var errors) )
                {
                    sendCount++;
                }
                else
                {
                    errorMessage += $"Failed to send email to {registration.ConfirmationEmail}: {string.Join( "; ", errors )}\n";
                }
            }


            if( errorMessage.IsNullOrWhiteSpace())
            {
                return ActionOk( $"Wait List Transition emails have been sent to {sendCount} individuals." );
            }
            else
            {
                return ActionOk( $"Sent {sendCount} email(s) with the following errors:\n{errorMessage}" );
            }
        }

        #endregion Block Actions
    }
}
