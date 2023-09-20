// <copyright>
// Copyright Pillars Inc.
// </copyright>
//
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;

using Quartz;

using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace rocks.pillars.Jobs
{
    /// <summary>
    /// Job that will send daily messages based on a defined type.
    /// </summary>
    /// 
    [SystemCommunicationField( "Message Email", "The system communication to use when sending message.", true, "", "", 0 )]
    [DefinedTypeField("Messages","The Defined Type that contains the message to send.", true, "", "", 1)]
    [AttributeField( Rock.SystemGuid.EntityType.PERSON, "Start Date Attribute", "The person attribute that contains the date that messages should start to be sent.", true, false, "", "", 2)]
    [AttributeField( Rock.SystemGuid.EntityType.PERSON, "Last Message Attribute", "The person attribute that contains the last message number that was sent.", true, false, "", "", 3)]
    [IntegerField("Valid Days", "The number of days after start that it is valid to send messages.", true, 50, "", 4)]

    [DisallowConcurrentExecution]
    public class SendDailyMessage : IJob
    {
        /// <summary>
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public SendDailyMessage()
        {
        }

        /// <summary>
        /// Job that will send daily messages based on a defined type.
        /// 
        /// Called by the <see cref="IScheduler" /> when a
        /// <see cref="ITrigger" /> fires that is associated with
        /// the <see cref="IJob" />.
        /// </summary>
        public virtual void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            var messageEmailGuid = dataMap.GetString( "MessageEmail" ).AsGuidOrNull();
            var messages = DefinedTypeCache.Get( dataMap.GetString( "Messages" ).AsGuid() );
            var startDateAttr = AttributeCache.Get( dataMap.GetString( "StartDateAttribute" ).AsGuid() );
            var lastMsgAttr = AttributeCache.Get( dataMap.GetString( "LastMessageAttribute" ).AsGuid() );

            if ( !messageEmailGuid.HasValue ||
                messages == null || 
                !messages.DefinedValues.Any() ||
                startDateAttr == null ||
                lastMsgAttr == null )
            {
                return;
            }


            var commonMergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );

            using ( var rockContext = new RockContext() )
            {
                int validDays = dataMap.GetString( "ValidDays" ).AsIntegerOrNull() ?? 50;
                var cutoffDate = RockDateTime.Today.AddDays( 0 - validDays );

                var personEntityTypeId = EntityTypeCache.GetId( typeof( Rock.Model.Person ) );
                var personService = new PersonService( rockContext );
                var attrValueService = new AttributeValueService( rockContext );

                var today = RockDateTime.Today;
                var personIds = attrValueService
                    .Queryable().AsNoTracking()
                    .Where( v =>
                        v.AttributeId == startDateAttr.Id &&
                        v.EntityId.HasValue &&
                        v.ValueAsDateTime.HasValue &&
                        v.ValueAsDateTime >= cutoffDate &&
                        v.ValueAsDateTime <= today )
                    .Select( v => v.EntityId.Value )
                    .ToList();

                var lastMessages = attrValueService
                    .Queryable().AsNoTracking()
                    .Where( v =>
                        v.AttributeId == lastMsgAttr.Id &&
                        v.EntityId.HasValue &&
                        v.ValueAsNumeric.HasValue )
                    .Select( v => new
                    {
                        v.EntityId,
                        v.ValueAsNumeric
                    } )
                    .ToDictionary( k => k.EntityId.Value, v => ( int ) v.ValueAsNumeric );

                var nextMessages = new Dictionary<int, int>();

                var recipients = new List<RockEmailMessageRecipient>();

                foreach ( var personId in personIds )
                {
                    DefinedValueCache nextMessage = null;
                    if ( lastMessages.ContainsKey( personId ) )
                    {
                        nextMessage = messages.DefinedValues.Where( v => v.Order > lastMessages[personId] ).FirstOrDefault();
                        if ( nextMessage == null )
                        {
                            continue;
                        }
                    }
                    else
                    {
                        nextMessage = messages.DefinedValues.First();
                    }

                    var person = personService.Get( personId );
                    if ( person == null )
                    {
                        continue;
                    }

                    var mergeFields = new Dictionary<string, object>( commonMergeFields );
                    mergeFields.Add( "Person", person );
                    mergeFields.Add( "Message", nextMessage );
                    recipients.Add( new RockEmailMessageRecipient( person, mergeFields ) );

                    nextMessages.Add( personId, nextMessage.Order );
                }

                if ( recipients.Any() )
                {
                    var emailMessage = new RockEmailMessage( messageEmailGuid.Value );
                    emailMessage.SetRecipients( recipients );
                    emailMessage.Send();
                    foreach ( var nextMessage in nextMessages )
                    {
                        var val = attrValueService
                            .Queryable()
                            .Where( v =>
                                v.AttributeId == lastMsgAttr.Id &&
                                v.EntityId.HasValue &&
                                v.EntityId.Value == nextMessage.Key )
                            .FirstOrDefault();
                        if ( val == null )
                        {
                            val = new AttributeValue
                            {
                                AttributeId = lastMsgAttr.Id,
                                EntityId = nextMessage.Key
                            };
                            attrValueService.Add( val );
                        }
                        val.Value = nextMessage.Value.ToString();
                        rockContext.SaveChanges();
                    }
                }
            }
        }
    }
}
