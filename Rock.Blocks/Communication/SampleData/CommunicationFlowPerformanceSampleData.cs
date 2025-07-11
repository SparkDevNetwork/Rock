using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;

using Microsoft.Extensions.Azure;

using Rock.Attribute;
using Rock.Data;
using Rock.Enums.Communication;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Communication
{
    /// <summary>
    /// Displays the performance of a particular communication flow.
    /// </summary>

    [DisplayName( "Communication Flow Performance Sample Data" )]
    [Category( "Communication" )]
    [Description( "Displays the performance of a particular communication flow sample data." )]
    [IconCssClass( "fa fa-line-chart" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "C7335E83-7B93-4AC0-AC1D-348B82E936F6" )]
    [Rock.SystemGuid.BlockTypeGuid( "FBE95153-A67E-4BF4-9FA3-E4BD9EB7584A" )]
    public class CommunicationFlowPerformanceSampleData : RockBlockType
    {
        private readonly Random _random = new Random();

        public override object GetObsidianBlockInitialization()
        {
            var currentPerson = GetCurrentPerson();

            return new
            {
                smsFromSystemPhoneNumbers = SystemPhoneNumberCache.All( false )
                    .Where( spn => spn.IsAuthorized( Authorization.VIEW, currentPerson ) )
                    .OrderBy( spn => spn.Order )
                    .ThenBy( spn => spn.Name )
                    .ThenBy( spn => spn.Id )
                    .ToListItemBagList()
            };
        }

        #region Block Actions

        [BlockAction( "GenerateRecurringFlow" )]
        public BlockActionResult GenerateRecurringFlow(
            string iCalendarContent,
            ListItemBag targetAudienceDataView,
            ListItemBag smsFromSystemPhoneNumber,
            decimal approximateUnsubscribeRate,
            decimal approximateConversionRate,
            decimal targetConversionRate
        ) {
            var communications = BuildDefaultCommunications( smsFromSystemPhoneNumber );
            var targetAudienceDataViewId = targetAudienceDataView?.GetEntityId<DataView>( this.RockContext );

            var flow = CreateCommunicationFlow(
                communications,
                targetAudienceDataViewId,
                targetConversionRate,
                CommunicationFlowTriggerType.Recurring,
                schedule: new Schedule { iCalendarContent = iCalendarContent } );

            var recipientAliases = GetPersonAliases( this.RockContext, targetAudienceDataViewId );

            PopulateFlowInstances(
                flow,
                flow.Schedule.FirstStartDateTime.Value,
                d => flow.Schedule.GetNextStartDateTime( d.AddDays( 1 ) ),
                recipientAliases,
                approximateUnsubscribeRate,
                approximateConversionRate );

            return ActionOk( new { flow.Id, flow.Guid } );
        }

        [BlockAction( "GenerateOneTimeFlow" )]
        public BlockActionResult GenerateOneTimeFlow(
            DateTime startDateTime,
            ListItemBag targetAudienceDataView,
            ListItemBag smsFromSystemPhoneNumber,
            decimal approximateUnsubscribeRate,
            decimal approximateConversionRate,
            decimal targetConversionRate
        ) {
            var communications = BuildDefaultCommunications( smsFromSystemPhoneNumber );

            var targetAudienceDataViewId = targetAudienceDataView.GetEntityId<DataView>( this.RockContext );

            var flow = CreateCommunicationFlow(
                communications,
                targetAudienceDataViewId,
                targetConversionRate,
                CommunicationFlowTriggerType.OneTime,
                schedule: null );

            var recipientAliases = GetPersonAliases( this.RockContext, targetAudienceDataViewId );

            // one instance only – pass a “null-returning” iterator
            PopulateFlowInstances(
                flow,
                startDateTime,
                _ => null, // No next instances.
                recipientAliases,
                approximateUnsubscribeRate,
                approximateConversionRate );

            return ActionOk( new { flow.Id, flow.Guid } );
        }

        [BlockAction( "GenerateOnDemandFlow" )]
        public BlockActionResult GenerateOnDemandFlow(
            ListItemBag targetAudienceDataView,
            ListItemBag smsFromSystemPhoneNumber,
            decimal approximateUnsubscribeRate,
            decimal approximateConversionRate,
            decimal targetConversionRate
        ) {
            var communications = BuildDefaultCommunications( smsFromSystemPhoneNumber );

            var targetAudienceDataViewId = targetAudienceDataView.GetEntityId<DataView>( this.RockContext );

            var flow = CreateCommunicationFlow(
                communications,
                targetAudienceDataViewId,
                targetConversionRate,
                CommunicationFlowTriggerType.OnDemand,
                schedule: null );

            var recipientAliases = GetPersonAliases( this.RockContext, targetAudienceDataViewId );

            // On-demand flows are instantiated at the time they are needed.
            // In this case, we'll just pick a date a few days ago,
            // and then create a few follow-up instances.
            var startDateTime = RockDateTime.Now.AddMonths( -4 ).Date.AddHours( 8.5 );
            var i = 0;

            PopulateFlowInstances(
                flow,
                startDateTime,
                previousInstanceDateTime =>
                {
                    if ( i++ >= 10 )
                    {
                        // Stop creating instances.
                        return null;
                    }

                    // Create a new instance every 10 days
                    // and increase by a day for each instance.
                    return previousInstanceDateTime.AddDays( 10 + i );
                },
                recipientAliases,
                approximateUnsubscribeRate,
                approximateConversionRate );

            return ActionOk( new { flow.Id, flow.Guid } );
        }

        #endregion Block Actions

        #region Methods

        /// <summary>
        /// Returns the three demo communications used by every sample flow.
        /// (Nothing prevents you from passing a different list later.)
        /// </summary>
        private static List<CommunicationFlowCommunication> BuildDefaultCommunications(
            ListItemBag smsFromSystemPhoneNumber )
        {
            int? smsFromId = smsFromSystemPhoneNumber?.Value.AsGuidOrNull().HasValue == true
                ? SystemPhoneNumberCache.GetId( smsFromSystemPhoneNumber.Value.AsGuid() )
                : null;

            return new List<CommunicationFlowCommunication>
            {
                new CommunicationFlowCommunication
                {
                    CommunicationTemplate = new CommunicationTemplate
                    {
                        Name = "Sample Message 1",
                        Subject = "Welcome to the Sample Recurring Flow",
                        Message = @"!DOCTYPE html
        <html lang=""en-us"">
          <head><meta charset=""utf-8""></head>
          <body><p>Thank you for joining our sample recurring flow!</p></body>
        </html>",
                        FromEmail = "test@test.com",
                        FromName = "Sample Sender",
                        IsActive = true,
                        Version = CommunicationTemplateVersion.Beta,
                        UsageType = CommunicationTemplateUsageType.CommunicationFlows
                    },
                    CommunicationType = Model.CommunicationType.Email,
                    Name = "Sample Message 1",
                    Order = 0,
                    DaysToWait = 0,
                    TimeToSend = TimeSpan.FromMinutes( 8.5 * 60 )
                },

                new CommunicationFlowCommunication
                {
                    CommunicationTemplate = new CommunicationTemplate
                    {
                        Name = "Sample Message 2",
                        Subject = "Follow-up on the Sample Recurring Flow",
                        SMSMessage = "We hope you are enjoying the sample recurring flow! 😁",
                        SmsFromSystemPhoneNumberId = smsFromId,
                        IsActive = true,
                        Version = CommunicationTemplateVersion.Beta,
                        UsageType = CommunicationTemplateUsageType.CommunicationFlows
                    },
                    CommunicationType = Model.CommunicationType.SMS,
                    Name = "Sample Message 2",
                    Order = 1,
                    DaysToWait = 3,
                    TimeToSend = TimeSpan.FromMinutes( 9.5 * 60 )
                },

                new CommunicationFlowCommunication
                {
                    CommunicationTemplate = new CommunicationTemplate
                    {
                        Name = "Sample Message 3",
                        Subject = "Welcome to the Sample Recurring Flow",
                        Message = @"!DOCTYPE html
        <html lang=""en-us"">
          <head><meta charset=""utf-8""></head>
          <body><p>This is the final message in our sample recurring flow!</p></body>
        </html>",
                        FromEmail = "test@test.com",
                        FromName = "Sample Sender",
                        IsActive = true,
                        Version = CommunicationTemplateVersion.Beta,
                        UsageType = CommunicationTemplateUsageType.CommunicationFlows
                    },
                    CommunicationType = Model.CommunicationType.Email,
                    Name = "Sample Message 3",
                    Order = 2,
                    DaysToWait = 3,
                    TimeToSend = TimeSpan.FromMinutes( 8.5 * 60 )
                }
            };
        }

        /// <summary>
        /// Creates the CommunicationFlow shell (schedule *or* sendDateTime decide        
        /// whether it is recurring or one-time) and saves it.
        /// </summary>
        private CommunicationFlow CreateCommunicationFlow(
            IEnumerable<CommunicationFlowCommunication> communications,
            int? targetAudienceDataViewId,
            decimal targetConversionRate,
            CommunicationFlowTriggerType triggerType,
            Schedule schedule = null )
        {
            var flow = new CommunicationFlow
            {
                CommunicationFlowCommunications = communications.ToList(),
                ConversionGoalTargetPercent = targetConversionRate,
                ConversionGoalTimeframeInDays = _random.Next( 1, 14 ),
                ConversionGoalType = ConversionGoalType.JoinedGroupType,
                Description = "Sample Recurring Communication Flow",
                ExitConditionType = ExitConditionType.ConversionAchieved,
                Name = "Sample Recurring Communication Flow",
                Schedule = schedule,
                TargetAudienceDataViewId = targetAudienceDataViewId,
                TriggerType = triggerType,
            };

            flow.SetConversionGoalSettings( new CommunicationFlow.ConversionGoalSettings
            {
                JoinedGroupTypeSettings = new CommunicationFlow.JoinedGroupTypeConversionGoalSettings
                {
                    GroupTypeGuid = SystemGuid.GroupType.GROUPTYPE_SMALL_GROUP.AsGuid()
                }
            } );

            new CommunicationFlowService( RockContext ).Add( flow );
            RockContext.SaveChanges();

            return flow;
        }

        /// <summary>
        /// Builds every instance, communication, recipient and history entry
        /// for the supplied flow (recurring *or* one-time).
        /// </summary>
        private void PopulateFlowInstances(
            CommunicationFlow flow,
            DateTime firstInstanceDate,
            Func<DateTime, DateTime?> getNextInstanceDate,
            List<PersonAlias> recipientPersonAliases,
            decimal approxUnsubscribeRate,
            decimal approxConversionRate )
        {
            var maxUnsubscribeLevel = Enum.GetValues( typeof( UnsubscribeLevel ) )
                                          .Cast<UnsubscribeLevel>().Max();

            var communicationCount = flow.CommunicationFlowCommunications.Count;
            var instanceDate = firstInstanceDate;
            var nextInstanceDate = getNextInstanceDate( firstInstanceDate );

            while ( instanceDate != default )
            {
                var unsubscribed = new List<int>();
                var converted = new List<int>();

                var instance = new CommunicationFlowInstance { StartDateTime = instanceDate };
                flow.CommunicationFlowInstances.Add( instance );
                RockContext.SaveChanges();

                // seed recipients
                foreach ( var pa in recipientPersonAliases )
                {
                    instance.CommunicationFlowInstanceRecipients.Add(
                        new CommunicationFlowInstanceRecipient
                        {
                            RecipientPersonAlias = pa,
                            Status = CommunicationFlowInstanceRecipientStatus.Active
                        } );
                }
                RockContext.SaveChanges();

                var current = instanceDate;
                var flowCommunications = flow.CommunicationFlowCommunications.OrderBy( x => x.Order ).ToList();

                for ( var i = 0; i < flowCommunications.Count; i++ )
                {
                    var flowCommunication = flowCommunications[i];

                    var sendDateTime = current.AddDays( flowCommunication.DaysToWait ).Date.Add( flowCommunication.TimeToSend );
                    current = sendDateTime;

                    if ( sendDateTime > RockDateTime.Now || instance.CompletedDateTime.HasValue )
                    {
                        // future send or no more recipients, skip
                        continue;
                    }

                    var comm = flowCommunication.CommunicationTemplate.Message.IsNotNullOrWhiteSpace()
                        ? new Model.Communication
                        {
                            CommunicationTemplate = flowCommunication.CommunicationTemplate,
                            CommunicationType = flowCommunication.CommunicationType,
                            FromEmail = flowCommunication.CommunicationTemplate.FromEmail,
                            FromName = flowCommunication.CommunicationTemplate.FromName,
                            Message = flowCommunication.CommunicationTemplate.Message,
                            Name = flowCommunication.CommunicationTemplate.Name,
                            Subject = flowCommunication.CommunicationTemplate.Subject,
                            SendDateTime = sendDateTime,
                            Status = CommunicationStatus.Approved
                        }
                        : flowCommunication.CommunicationTemplate.SMSMessage.IsNotNullOrWhiteSpace()
                        ? new Model.Communication
                        {
                            CommunicationTemplate = flowCommunication.CommunicationTemplate,
                            CommunicationType = flowCommunication.CommunicationType,
                            SmsFromSystemPhoneNumberId = flowCommunication.CommunicationTemplate.SmsFromSystemPhoneNumberId,
                            SMSMessage = flowCommunication.CommunicationTemplate.SMSMessage,
                            Name = flowCommunication.CommunicationTemplate.Name,
                            SendDateTime = sendDateTime,
                            Status = CommunicationStatus.Approved
                        }
                        : null;

                    instance.CommunicationFlowInstanceCommunications.Add(
                        new CommunicationFlowInstanceCommunication
                        {
                            Communication = comm,
                            CommunicationFlowCommunication = flowCommunication
                        } );
                    RockContext.SaveChanges();

                    var eligible = instance.CommunicationFlowInstanceRecipients
                        .Where( r => !unsubscribed.Contains( r.RecipientPersonAlias.Id )
                                  && !converted.Contains( r.RecipientPersonAlias.Id ) )
                        .ToList();

                    if ( !eligible.Any() )
                    {
                        // There are no more eligible recipients. Mark the instance as completed.
                        instance.CompletedDateTime = comm.SendDateTime;
                        RockContext.SaveChanges();
                    }
                    else
                    {
                        foreach ( var r in eligible )
                        {
                            var causedUnsub = ( decimal ) _random.NextDouble() <=
                                              ( approxUnsubscribeRate / 100m / communicationCount );
                            var unsubDate = causedUnsub
                                ? GetRandomDateTime( sendDateTime, nextInstanceDate )
                                : ( DateTime? ) null;
                            var unsubLevel = causedUnsub
                                ? ( UnsubscribeLevel? ) _random.Next( maxUnsubscribeLevel.ConvertToInt() + 1 )
                                : default;

                            var commRecip = new CommunicationRecipient
                            {
                                PersonAlias = r.RecipientPersonAlias,
                                UnsubscribeDateTime = unsubDate,
                                UnsubscribeLevel = unsubLevel,
                                SendDateTime = comm.SendDateTime
                            };
                            comm.Recipients.Add( commRecip );

                            if ( causedUnsub )
                            {
                                unsubscribed.Add( r.Id );
                                r.UnsubscribeCommunicationRecipient = commRecip;
                            }
                            else if ( ( decimal ) _random.NextDouble() <= ( approxConversionRate / 100m / communicationCount ) )
                            {
                                instance.CommunicationFlowInstanceConversionHistories.Add(
                                    new CommunicationFlowInstanceConversionHistory
                                    {
                                        PersonAlias = r.RecipientPersonAlias,
                                        Date = GetRandomDateTime( instanceDate, nextInstanceDate ),
                                        CommunicationFlowCommunication = flowCommunication
                                    } );
                                converted.Add( r.Id );
                            }

                            // Save after processing each recipient.
                            RockContext.SaveChanges();
                        }
                    }
                }

                // All communications processed. Mark the 


                instanceDate = nextInstanceDate ?? default;
                nextInstanceDate = nextInstanceDate != null
                    ? getNextInstanceDate( nextInstanceDate.Value )
                    : null;
            }


        }

        private List<PersonAlias> GetPersonAliases( RockContext rockContext, int? targetAudienceDataViewId )
        {
            if ( !targetAudienceDataViewId.HasValue )
            {
                return new List<PersonAlias>();
            }

            var recipientAliasIds = DataViewCache.Get( targetAudienceDataViewId.Value ).GetEntityIds().ToList();

            if ( recipientAliasIds?.Any() != true )
            {
                return new List<PersonAlias>();
            }

            return new PersonAliasService( rockContext ).GetByIds( recipientAliasIds ).ToList();
        }

        private DateTime GetRandomDateTime( DateTime minDateTime, DateTime? maxDateTime )
        {
            if ( !maxDateTime.HasValue )
            {
                return minDateTime;
            }

            if ( minDateTime > maxDateTime.Value )
            {
                var temp = maxDateTime.Value;
                maxDateTime = minDateTime;
                minDateTime = temp;
            }

            var range = maxDateTime.Value - minDateTime;

            // Get a random number of ticks within the range
            var randTicks = ( long ) ( _random.NextDouble() * range.Ticks );
            return minDateTime.AddTicks( randTicks );
        }

        #endregion
    }
}
