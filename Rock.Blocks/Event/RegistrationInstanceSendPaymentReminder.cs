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

using Humanizer;

using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Blocks.Event.RegistrationInstanceSendPaymentReminder;
using Rock.Web.Cache;

namespace Rock.Blocks.Event
{
    /// <summary>
    /// Sends payment reminder emails to registrants with an outstanding
    /// balance for a specific registration instance.
    /// </summary>
    [DisplayName( "Registration Instance Send Payment Reminder" )]
    [Category( "Event" )]
    [Description( "Sends payment reminders for paid registrations that have a remaining balance." )]
    [IconCssClass( "ti ti-bell" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage(
        "Registration Instance Page",
        Key = AttributeKey.RegistrationInstancePage,
        Description = "The registration instance page to return to after reminders are sent.",
        DefaultValue = Rock.SystemGuid.Page.REGISTRATION_INSTANCE,
        IsRequired = false,
        Order = 1 )]

    [Rock.Cms.DefaultBlockRole( Rock.Enums.Cms.BlockRole.Secondary )]
    [Rock.SystemGuid.EntityTypeGuid( "1D05C3A0-2520-4C04-A588-ACC5D6D626A8" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "5D61C8FB-0D92-40BF-BA4A-F4DF99DCAE13" )]
    [Rock.SystemGuid.BlockTypeGuid( "ED56CD0A-0A8D-4758-A689-55B7BEC1B589" )]
    public class RegistrationInstanceSendPaymentReminder : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string RegistrationInstancePage = "RegistrationInstancePage";
        }

        private static class PageParameterKey
        {
            public const string RegistrationInstanceId = "RegistrationInstanceId";
        }

        private static class MergeFieldKey
        {
            public const string Registration = "Registration";
            public const string RegistrationInstance = "RegistrationInstance";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new RegistrationInstanceSendPaymentReminderInitializationBox();

            var registrationInstance = GetRegistrationInstance();

            if ( registrationInstance == null )
            {
                box.HasOutstandingBalances = false;
                box.EmptyStateMessage = "<strong>Registration Instance not found.</strong> Unable to load outstanding balances.";
                return box;
            }

            var template = registrationInstance.RegistrationTemplate;

            if ( template == null )
            {
                box.HasOutstandingBalances = false;
                box.EmptyStateMessage = "<strong>Registration Template not found.</strong> Unable to load payment reminder settings.";
                return box;
            }

            var paymentReminderTimeSpanDays = template.PaymentReminderTimeSpan;

            var outstandingRegistrations = GetOutstandingBalances( registrationInstance );

            if ( !outstandingRegistrations.Any() )
            {
                box.HasOutstandingBalances = false;
                box.EmptyStateMessage = "<strong>Good News!</strong> No registrations have an outstanding balance at this time.";
                return box;
            }

            var sampleRegistration = outstandingRegistrations.First();
            var sampleMergeFields = BuildMergeFields( sampleRegistration, registrationInstance );

            var gridData = BuildRegistrationGridData( outstandingRegistrations, paymentReminderTimeSpanDays );

            box.HasOutstandingBalances = true;
            box.FromName = template.PaymentReminderFromName?.ResolveMergeFields( sampleMergeFields ) ?? string.Empty;
            box.FromEmail = template.PaymentReminderFromEmail?.ResolveMergeFields( sampleMergeFields ) ?? string.Empty;
            box.Subject = template.PaymentReminderSubject?.ResolveMergeFields( sampleMergeFields ) ?? string.Empty;
            box.MessageBody = template.PaymentReminderEmailTemplate ?? string.Empty;
            box.PreviewHtml = box.MessageBody.ResolveMergeFields( sampleMergeFields );
            box.InstructionsHtml = BuildInstructionsHtml( paymentReminderTimeSpanDays );
            box.Registrations = gridData.Bags;
            box.PreSelectedKeys = gridData.PreSelectedKeys;
            box.RegistrationInstancePageUrl = BuildRegistrationInstancePageUrl( registrationInstance );
            box.RegistrationInstanceName = registrationInstance.Name;

            return box;
        }

        /// <summary>
        /// Resolves the RegistrationInstance from the page parameter, accepting either
        /// the numeric Id or an IdKey.
        /// </summary>
        private RegistrationInstance GetRegistrationInstance()
        {
            var key = PageParameter( PageParameterKey.RegistrationInstanceId );

            if ( key.IsNullOrWhiteSpace() )
            {
                return null;
            }

            return new RegistrationInstanceService( RockContext )
                .Get( key, !PageCache.Layout.Site.DisablePredictableIds );
        }

        /// <summary>
        /// Returns registrations with an outstanding balance (after payment-plan offset), ordered by LastName, FirstName.
        /// </summary>
        private List<Registration> GetOutstandingBalances( RegistrationInstance registrationInstance )
        {
            /*
                4/24/26 - MSE

                SelectPaymentPlanPairs throws at materialization when a scheduled
                transaction has no detail rows: the SQL SUM returns NULL and won't
                hydrate into a non-nullable decimal. IncludePaymentPlanDependencies
                plus the computed PaymentPlan property sums in LINQ-to-Objects,
                which tolerates empty collections.

                AsNoTracking is intentionally NOT used here: the first outstanding
                registration is reused as the Lava merge-field sample for the email
                preview, and Lava may lazy-load nested navigation properties (e.g.
                Registrants[0].PersonAlias.Person) that aren't eager-loaded above.

                Reason: SelectPaymentPlanPairs fails on scheduled transactions with no detail rows.
            */
            var registrations = new RegistrationService( RockContext )
                .Queryable()
                .Where( r => r.RegistrationInstanceId == registrationInstance.Id )
                .IncludePaymentPlanDependencies()
                .ToList();

            return registrations
                .Where( r =>
                {
                    if ( r.BalanceDue <= 0 )
                    {
                        return false;
                    }

                    var paymentPlan = r.PaymentPlanFinancialScheduledTransaction?.PaymentPlan;
                    return paymentPlan == null || r.BalanceDue > paymentPlan.PlannedAmountRemaining;
                } )
                .OrderBy( r => r.LastName )
                .ThenBy( r => r.FirstName )
                .ToList();
        }

        /// <summary>
        /// Builds the Lava merge-field dictionary for a single registration.
        /// </summary>
        private Dictionary<string, object> BuildMergeFields( Registration registration, RegistrationInstance registrationInstance )
        {
            return new Dictionary<string, object>
            {
                { MergeFieldKey.Registration, registration },
                { MergeFieldKey.RegistrationInstance, registrationInstance }
            };
        }

        /// <summary>
        /// Builds the HTML instructions paragraph shown above the grid.
        /// </summary>
        private string BuildInstructionsHtml( int? paymentReminderTimeSpanDays )
        {
            if ( paymentReminderTimeSpanDays.HasValue )
            {
                return $"<p>Below is a list of registrations with outstanding balances. Individuals who have not been reminded of their balance in {paymentReminderTimeSpanDays.Value} days have been pre-selected. Those who have been recently added or notified of their balance are greyed out. They can still be included by either selecting them or selecting all transactions.</p>";
            }

            return "<p>Below is a list of registrations with outstanding balances. Select the individuals who should receive a payment reminder.</p>";
        }

        /// <summary>
        /// Builds the grid row bags and the list of IdKeys that should be pre-selected,
        /// in a single pass so the days-since-last-reminder calculation is shared.
        /// A reminder is considered "overdue" only when both the last-reminder timestamp
        /// and the template's time span are set; overdue rows drive pre-selection, and
        /// the inverse (reminded within the time span) drives the grey-out styling.
        /// </summary>
        private (List<RegistrationBalanceBag> Bags, List<string> PreSelectedKeys) BuildRegistrationGridData( IEnumerable<Registration> registrations, int? paymentReminderTimeSpanDays )
        {
            var bags = new List<RegistrationBalanceBag>();
            var preSelectedKeys = new List<string>();

            foreach ( var registration in registrations )
            {
                var daysSinceLastReminder = GetDaysSinceLastReminder( registration.LastPaymentReminderDateTime );
                var isReminderOverdue = daysSinceLastReminder.HasValue
                    && paymentReminderTimeSpanDays.HasValue
                    && daysSinceLastReminder.Value >= paymentReminderTimeSpanDays.Value;
                var isRecentlyReminded = daysSinceLastReminder.HasValue
                    && paymentReminderTimeSpanDays.HasValue
                    && !isReminderOverdue;

                bags.Add( new RegistrationBalanceBag
                {
                    IdKey = registration.IdKey,
                    Name = $"{registration.LastName}, {registration.FirstName}",
                    Email = registration.ConfirmationEmail,
                    RegisteredDateTime = registration.CreatedDateTime,
                    LastReminderDateTime = registration.LastPaymentReminderDateTime,
                    LastReminderText = FormatLastReminder( registration.LastPaymentReminderDateTime ),
                    Cost = registration.TotalCost,
                    Paid = registration.TotalPaid,
                    BalanceDue = registration.BalanceDue,
                    IsActive = !isRecentlyReminded
                } );

                if ( isReminderOverdue )
                {
                    preSelectedKeys.Add( registration.IdKey );
                }
            }

            return ( bags, preSelectedKeys );
        }

        /// <summary>
        /// Returns the integer days (ceiling) since the last reminder, or null when never reminded.
        /// </summary>
        private static int? GetDaysSinceLastReminder( DateTime? lastReminderDateTime )
        {
            if ( !lastReminderDateTime.HasValue )
            {
                return null;
            }

            return ( int ) Math.Ceiling( ( RockDateTime.Now - lastReminderDateTime.Value ).TotalDays );
        }

        /// <summary>
        /// Formats the last-reminder timestamp as "Today", "N days", or "Unknown".
        /// </summary>
        private string FormatLastReminder( DateTime? lastReminderDateTime )
        {
            if ( !lastReminderDateTime.HasValue )
            {
                return "Unknown";
            }

            var days = ( RockDateTime.Now - lastReminderDateTime.Value ).TotalDays;

            if ( days < 1 )
            {
                return "Today";
            }

            return "days".ToQuantity( ( int ) Math.Ceiling( days ) );
        }

        /// <summary>
        /// Builds the URL for the "Return to Registration Instance" link; empty when the attribute is not configured.
        /// </summary>
        private string BuildRegistrationInstancePageUrl( RegistrationInstance registrationInstance )
        {
            var queryParams = new Dictionary<string, string>
            {
                [PageParameterKey.RegistrationInstanceId] = registrationInstance.IdKey
            };

            return this.GetLinkedPageUrl( AttributeKey.RegistrationInstancePage, queryParams ) ?? string.Empty;
        }

        /// <summary>
        /// Resolves the sample registration for preview rendering. Prefers the client-supplied IdKey over a full rescan.
        /// The outstanding-balance fallback is defensive: the client always forwards the first grid row's
        /// IdKey on happy paths, so this path only trips on malformed requests or a stale sample reference.
        /// </summary>
        private Registration ResolveSampleRegistration( RegistrationInstance registrationInstance, string sampleRegistrationKey )
        {
            if ( sampleRegistrationKey.IsNotNullOrWhiteSpace() )
            {
                var candidate = new RegistrationService( RockContext )
                    .Get( sampleRegistrationKey, !PageCache.Layout.Site.DisablePredictableIds );

                if ( candidate != null && candidate.RegistrationInstanceId == registrationInstance.Id )
                {
                    return candidate;
                }
            }

            return GetOutstandingBalances( registrationInstance ).FirstOrDefault();
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Resolves the message body against a sample registration's merge fields for the preview iframe.
        /// </summary>
        [BlockAction]
        public BlockActionResult ResolvePreview( ResolvePreviewRequestBag request )
        {
            if ( request == null )
            {
                return ActionBadRequest( "Request payload is required." );
            }

            var registrationInstance = GetRegistrationInstance();

            if ( registrationInstance == null )
            {
                return ActionBadRequest( "Registration Instance not found." );
            }

            var sampleRegistration = ResolveSampleRegistration( registrationInstance, request.SampleRegistrationKey );

            if ( sampleRegistration == null )
            {
                return ActionOk( new ResolvePreviewResponseBag { PreviewHtml = request.MessageBody ?? string.Empty } );
            }

            var mergeFields = BuildMergeFields( sampleRegistration, registrationInstance );
            var html = ( request.MessageBody ?? string.Empty ).ResolveMergeFields( mergeFields );

            return ActionOk( new ResolvePreviewResponseBag { PreviewHtml = html } );
        }

        /// <summary>
        /// Sends a payment reminder to each selected registration and updates its LastPaymentReminderDateTime.
        /// Registrations without a confirmation email are silently skipped.
        /// </summary>
        [BlockAction]
        public BlockActionResult SendReminders( SendPaymentRemindersRequestBag request )
        {
            if ( request == null || request.SelectedKeys == null || !request.SelectedKeys.Any() )
            {
                return ActionBadRequest( "Select at least one registration before sending reminders." );
            }

            if ( request.FromName.IsNullOrWhiteSpace() || request.FromEmail.IsNullOrWhiteSpace() || request.Subject.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "From Name, From Email, and Subject are all required." );
            }

            if ( request.MessageBody.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Message body is required." );
            }

            var registrationInstance = GetRegistrationInstance();

            if ( registrationInstance == null )
            {
                return ActionBadRequest( "Registration Instance not found." );
            }

            var disablePredictableIds = PageCache.Layout.Site.DisablePredictableIds;
            var sendCount = 0;
            var errorCount = 0;
            var appRoot = this.RequestContext.ResolveRockUrl( "~/" );
            var themeRoot = this.RequestContext.ResolveRockUrl( "~~/" );

            foreach ( var key in request.SelectedKeys.Distinct() )
            {
                // A separate RockContext per iteration keeps the ChangeTracker from growing
                // when sending to large batches.
                using ( var rockContext = new RockContext() )
                {
                    try
                    {
                        var registrationService = new RegistrationService( rockContext );
                        var registration = registrationService.Get( key, !disablePredictableIds );

                        if ( registration == null || registration.RegistrationInstanceId != registrationInstance.Id )
                        {
                            continue;
                        }

                        if ( registration.ConfirmationEmail.IsNullOrWhiteSpace() )
                        {
                            continue;
                        }

                        var mergeFields = BuildMergeFields( registration, registrationInstance );

                        var emailMessage = new RockEmailMessage
                        {
                            FromEmail = request.FromEmail,
                            FromName = request.FromName,
                            Subject = request.Subject,
                            Message = request.MessageBody,
                            AdditionalMergeFields = mergeFields,
                            AppRoot = appRoot,
                            ThemeRoot = themeRoot,
                            CreateCommunicationRecord = true
                        };

                        emailMessage.AddRecipient( registration.GetConfirmationRecipient( mergeFields ) );

                        if ( emailMessage.Send( out var sendErrors ) )
                        {
                            registration.LastPaymentReminderDateTime = RockDateTime.Now;
                            rockContext.SaveChanges();
                            sendCount++;
                        }
                        else
                        {
                            // Log each transport error but keep processing the rest of the batch.
                            errorCount++;
                            foreach ( var sendError in sendErrors )
                            {
                                ExceptionLogService.LogException( new Exception( $"Failed to send payment reminder for registration {key}: {sendError}" ) );
                            }
                        }
                    }
                    catch ( Exception ex )
                    {
                        errorCount++;
                        ExceptionLogService.LogException( ex );
                    }
                }
            }

            var message = $"Payment reminders have been sent to {"individuals".ToQuantity( sendCount )}.";

            if ( errorCount > 0 )
            {
                message += $" {"reminder".ToQuantity( errorCount )} failed to send and were logged.";
            }

            return ActionOk( new SendPaymentRemindersResponseBag
            {
                SentCount = sendCount,
                Message = message
            } );
        }

        #endregion Block Actions
    }
}
