// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Linq;
using System.Web;
using System.IO;

using Quartz;

using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Data;
using com.centralaz.Accountability.Model;
using com.centralaz.Accountability.Data;
using Rock.Web.Cache;
using Rock.Web;
using Rock.Communication;

namespace com.centralaz.Accountability.Jobs
{

    /// <summary>
    /// Job to send reminders to accountability group members to submit a report.
    /// </summary>
    [CommunicationTemplateField( "Template", "", true, "" )]
    [DisallowConcurrentExecution]
    public class SendAccountabilityReportReminder : IJob
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="SendCommunications"/> class.
        /// </summary>
        public SendAccountabilityReportReminder()
        {
        }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public virtual void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            var emailTemplate = dataMap.Get( "Template" ).ToString().AsGuid();
            var pageId = ( new PageService( new RockContext() ).Get( "64B5B1F6-472A-4C64-85B5-1F6864FE1992".AsGuid() ) ).Id;
            foreach ( var groupType in new GroupTypeService( new RockContext() ).Queryable() )
            {
                if ( groupType.InheritedGroupType != null )
                {
                    if ( groupType.InheritedGroupType.Guid == "DC99BF69-8A1A-411F-A267-1AE75FDC2341".AsGuid() )
                    {
                        foreach ( Group group in groupType.Groups )
                        {
                            group.LoadAttributes();
                            DateTime reportStartDate = DateTime.Parse( group.GetAttributeValue( "ReportStartDate" ).ToString() );
                            if ( reportStartDate.DayOfWeek == DateTime.Now.DayOfWeek )
                            {
                                DateTime nextDueDate = NextReportDate( reportStartDate );
                                int daysUntilDueDate = ( nextDueDate - DateTime.Today ).Days;
                                foreach ( GroupMember groupMember in group.Members )
                                {
                                    ResponseSetService responseSetService = new ResponseSetService( new AccountabilityContext() );
                                    //All caught up case
                                    if ( daysUntilDueDate == 0 && !responseSetService.DoesResponseSetExistWithSubmitDate( nextDueDate, groupMember.PersonId, group.Id ) )
                                    {
                                        Send( groupMember, pageId, emailTemplate );
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }


        protected DateTime NextReportDate( DateTime reportStartDate )
        {
            DateTime today = DateTime.Now;
            DateTime reportDue = today;

            int daysElapsed = ( today.Date - reportStartDate ).Days;
            if ( daysElapsed >= 0 )
            {
                int remainder = daysElapsed % 7;
                if ( remainder != 0 )
                {
                    int daysUntil = 7 - remainder;
                    reportDue = today.AddDays( daysUntil );
                }
            }
            else
            {
                reportDue = today.AddDays( -( daysElapsed ) );
            }
            return reportDue;
        }


        private void Send( GroupMember recipient, int pageId, Guid emailTemplateGuid, string appRoot = "", string themeRoot = "" )
        {
            var rockContext = new RockContext();
            var communication = UpdateCommunication( rockContext, recipient, emailTemplateGuid, pageId );

            if ( communication != null )
            {
                string message = string.Empty;

                communication.Status = CommunicationStatus.Approved;
                communication.ReviewedDateTime = RockDateTime.Now;

                message = "Communication has been queued for sending.";
                rockContext.SaveChanges();

                var transaction = new Rock.Transactions.SendCommunicationTransaction();
                transaction.CommunicationId = communication.Id;
                //transaction.PersonAlias = CurrentPersonAlias;
                Rock.Transactions.RockQueue.TransactionQueue.Enqueue( transaction );
            }

            //var recipients = new List<RecipientData>();
            //var newRecipient = new RecipientData( recipient.Person.Email, CombineMergeFields( recipient, pageId ) );
            //recipients.Add( newRecipient );
            //var template = new SystemEmailService( new RockContext() ).Get( emailTemplateGuid );

            //var mediumEntity = EntityTypeCache.Read( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid() );
            //if ( mediumEntity != null )
            //{
            //    var medium = MediumContainer.GetComponent( mediumEntity.Name );
            //    if ( medium != null && medium.IsActive )
            //    {
            //        var transport = medium.Transport;
            //        if ( transport != null && transport.IsActive )
            //        {
            //            appRoot = GlobalAttributesCache.Read( new RockContext() ).GetValue( "InternalApplicationRoot" );
            //            transport.Send( template, recipients, appRoot, themeRoot );
            //        }
            //    }
            //}
        }


        private Dictionary<string, string> CombineMergeFields( GroupMember groupMember, int pageId )
        {
            //var mergeFields = Rock.Web.Cache.GlobalAttributesCache.GetMergeFields( groupMember.Person );
            var mergeFields = new Dictionary<string, string>();

            mergeFields.Add( "GroupName", groupMember.Group.Name );
            //string url = ( String.Format( "{0}page/{1}?GroupId={2}", System.Web.VirtualPathUtility.ToAbsolute("~"), pageId, groupMember.GroupId ) );
            String url = VirtualPathUtility.ToAbsolute( String.Format( "~/page/{0}?GroupId={1}", pageId, groupMember.GroupId ) );
            mergeFields.Add( "ReportPageUrl", url );
            return mergeFields;
        }

        /// <summary>
        /// Updates a communication model with the user-entered values
        /// </summary>
        /// <param name="communicationService">The service.</param>
        /// <returns></returns>
        private Communication UpdateCommunication( RockContext rockContext, GroupMember recipient, Guid emailTemplateGuid, int pageId )
        {
            var communicationService = new CommunicationService( rockContext );
            var recipientService = new CommunicationRecipientService( rockContext );

            Communication communication = null;

            communication = new Rock.Model.Communication();
            communication.Status = CommunicationStatus.Transient;
            // communication.SenderPersonAliasId = CurrentPersonAliasId;
            communicationService.Add( communication );

            if ( !communication.Recipients.Any( r => r.PersonAlias != null && r.PersonAlias.PersonId == recipient.PersonId ) )
            {
                var person = new PersonService( rockContext ).Get( recipient.PersonId );
                if ( person != null )
                {
                    var communicationRecipient = new CommunicationRecipient();
                    communicationRecipient.PersonAlias = person.PrimaryAlias;
                    communicationRecipient.AdditionalMergeValues = CombineMergeFields( recipient, pageId );
                    communication.Recipients.Add( communicationRecipient );
                }
            }

            // communication.IsBulkCommunication = cbBulk.Checked;

            communication.MediumEntityTypeId = ( new EntityTypeService( rockContext ).Get( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid() ) ).Id;
            communication.MediumData.Clear();
            //communication.AdditionalMergeFields.Add( recipient.Group.Name );
            //String url = VirtualPathUtility.ToAbsolute( String.Format( "~/page/{0}?GroupId={1}", pageId, recipient.GroupId ) );

            //  communication.AdditionalMergeFields.Add( url );
            Dictionary<string, string> MediumData = new Dictionary<string, string>();
            var template = new CommunicationTemplateService( new RockContext() ).Get( emailTemplateGuid );
            if ( template != null )
            {
                var mediumData = template.MediumData;
                if ( !mediumData.ContainsKey( "Subject" ) )
                {
                    mediumData.Add( "Subject", template.Subject );
                }

                foreach ( var dataItem in mediumData )
                {
                    if ( !string.IsNullOrWhiteSpace( dataItem.Value ) )
                    {
                        if ( MediumData.ContainsKey( dataItem.Key ) )
                        {
                            MediumData[dataItem.Key] = dataItem.Value;
                        }
                        else
                        {
                            MediumData.Add( dataItem.Key, dataItem.Value );
                        }
                    }
                }
            }

            foreach ( var keyVal in MediumData )
            {
                if ( !string.IsNullOrEmpty( keyVal.Value ) )
                {
                    communication.MediumData.Add( keyVal.Key, keyVal.Value );
                }
            }

            if ( communication.MediumData.ContainsKey( "Subject" ) )
            {
                communication.Subject = communication.MediumData["Subject"];
                communication.MediumData.Remove( "Subject" );
            }

            communication.FutureSendDateTime = null;

            return communication;
        }
    }
}