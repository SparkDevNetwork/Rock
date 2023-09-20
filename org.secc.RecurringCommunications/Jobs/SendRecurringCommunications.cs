// <copyright>
// Copyright Southeast Christian Church
//
// Licensed under the  Southeast Christian Church License (the "License");
// you may not use this file except in compliance with the License.
// A copy of the License shoud be included with this file.
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System.Linq;
using System.Data;
using System.Data.Entity;
using Quartz;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using System.Collections.Generic;
using org.secc.RecurringCommunications.Model;
using Rock.Web.Cache;
using System;
using Rock.Reporting;
using System.Reflection;

namespace org.secc.RecurringCommunications.Jobs
{
    [DisallowConcurrentExecution]
    public class SendRecurringCommunications : IJob
    {
        public void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            RockContext rockContext = new RockContext();
            RecurringCommunicationService recurringCommunicationService = new RecurringCommunicationService( rockContext );
            var communications = recurringCommunicationService.Queryable( "Schedule" ).ToList();
            int count = 0;

            foreach ( var communication in communications )
            {
                var lastExpectedRun = communication.Schedule
                    .GetScheduledStartTimes( RockDateTime.Now.AddDays( -1 ), RockDateTime.Now )
                    .LastOrDefault();
                if ( lastExpectedRun != null && lastExpectedRun > DateTime.MinValue )
                {
                    if ( communication.LastRunDateTime == null || communication.LastRunDateTime <= lastExpectedRun )
                    {
                        communication.LastRunDateTime = RockDateTime.Now;
                        rockContext.SaveChanges();
                        EnqueRecurringCommuniation( communication.Id );
                        count++;
                    }

                }
            }
            context.Result = string.Format( "Sent {0} communication{1}", count, count == 1 ? "" : "s" );
        }


        private void EnqueRecurringCommuniation( int id )
        {
            RockContext rockContext = new RockContext();
            CommunicationService communicationService = new CommunicationService( rockContext );
            RecurringCommunicationService recurringCommunicationService = new RecurringCommunicationService( rockContext );
            var recurringCommunication = recurringCommunicationService.Get( id );
            if ( recurringCommunication == null )
            {
                return;
            }



            var communication = new Communication();
            communication.SenderPersonAlias = recurringCommunication.CreatedByPersonAlias;
            communication.Name = recurringCommunication.Name;
            communication.CommunicationType = recurringCommunication.CommunicationType;
            communication.FromName = recurringCommunication.FromName;
            communication.FromEmail = recurringCommunication.FromEmail;
            communication.Subject = recurringCommunication.Subject;
            communication.Message = recurringCommunication.EmailBody;
            communication.SMSFromDefinedValueId = recurringCommunication.PhoneNumberValueId;
            communication.SMSMessage = recurringCommunication.SMSBody;
            communication.PushTitle = recurringCommunication.PushTitle;
            communication.PushSound = recurringCommunication.PushSound;
            communication.PushMessage = recurringCommunication.PushMessage;
            communication.Status = CommunicationStatus.Approved;

            DataTransformComponent transform = null;
            if (recurringCommunication.TransformationEntityTypeId.HasValue)
            {
                transform = DataTransformContainer.GetComponent(recurringCommunication.TransformationEntityType.Name);
                communication.AdditionalMergeFields = new List<string>() { "AppliesTo" };
            }


            communicationService.Add( communication );

            var emailMediumEntityType = EntityTypeCache.Get( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid() );
            var smsMediumEntityType = EntityTypeCache.Get( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS.AsGuid() );
            var pushNotificationMediumEntityType = EntityTypeCache.Get( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_PUSH_NOTIFICATION.AsGuid() );

            var dataViewGetQueryArgs = new DataViewGetQueryArgs
            {
                DbContext = rockContext
            };
            var dataview = ( IQueryable<Person> ) recurringCommunication.DataView.GetQuery( dataViewGetQueryArgs );

            if (transform != null)
            {
                var recipients = new List<CommunicationRecipient>();
                var personService = new PersonService(rockContext);
                var paramExpression = personService.ParameterExpression;
                
                foreach (Person dvPerson in dataview)
                {
                    var whereExp = Rock.Reporting.FilterExpressionExtractor.Extract<Rock.Model.Person>(personService.Queryable().Where(p => p.Id == dvPerson.Id), paramExpression, "p");
                    var transformExp = transform.GetExpression(personService, paramExpression, whereExp);

                    MethodInfo getMethod = personService.GetType().GetMethod("Get", new Type[] { typeof(System.Linq.Expressions.ParameterExpression), typeof(System.Linq.Expressions.Expression) });

                    if (getMethod != null)
                    {
                        var getResult = getMethod.Invoke(personService, new object[] { paramExpression, transformExp });
                        var qry = getResult as IQueryable<Person>;

                        foreach (var p in qry.ToList())
                        {
                            var fieldValues = new Dictionary<string, object>();
                            fieldValues.Add("AppliesTo", dvPerson);
                            recipients.Add(new CommunicationRecipient()
                            {
                                PersonAlias = p.PrimaryAlias,
                                MediumEntityTypeId = p.CommunicationPreference == CommunicationType.SMS ? smsMediumEntityType.Id : emailMediumEntityType.Id,
                                AdditionalMergeValues = fieldValues
                            }); 

                        }
                    }

                    communication.Recipients = recipients;
                }
            }
            else
            {
                communication.Recipients = dataview
                    .ToList()
                    .Select(p =>
                       new CommunicationRecipient
                        {
                            PersonAlias = p.PrimaryAlias,
                            MediumEntityTypeId = p.CommunicationPreference == CommunicationType.SMS ? smsMediumEntityType.Id : emailMediumEntityType.Id
                        })
                    .ToList();
            }
            Dictionary<int, CommunicationType?> communicationListGroupMemberCommunicationTypeLookup = new Dictionary<int, CommunicationType?>();

            foreach ( var recipient in communication.Recipients )
            {
                if ( communication.CommunicationType == CommunicationType.Email )
                {
                    recipient.MediumEntityTypeId = emailMediumEntityType.Id;
                }
                else if ( communication.CommunicationType == CommunicationType.SMS )
                {
                    recipient.MediumEntityTypeId = smsMediumEntityType.Id;
                }
                else if ( communication.CommunicationType == CommunicationType.PushNotification )
                {
                    recipient.MediumEntityTypeId = pushNotificationMediumEntityType.Id;
                }
                else if ( communication.CommunicationType == CommunicationType.RecipientPreference )
                {
                    //Do nothing we already defaulted to the recipient's preference
                }
                else
                {
                    throw new Exception( "Unexpected CommunicationType: " + communication.CommunicationType.ConvertToString() );
                }
            }
            rockContext.SaveChanges();

            var transaction = new Rock.Transactions.SendCommunicationTransaction();
            transaction.CommunicationId = communication.Id;
            transaction.PersonAlias = recurringCommunication.CreatedByPersonAlias;
            Rock.Transactions.RockQueue.TransactionQueue.Enqueue(transaction);
        }

    }
}
