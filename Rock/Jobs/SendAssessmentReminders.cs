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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Web;
using Humanizer;
using Quartz;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace Rock.Jobs
{
    #region Job Attributes

    [IntegerField( "Reminder Every",
        Key = AttributeKeys.ReminderEveryDays,
        Description = "The number of days between reminder emails.",
        IsRequired = true,
        DefaultValue = "7",
        Order = 0)]

    [IntegerField( "Cut off Days",
        Key = AttributeKeys.CutoffDays,
        Description = "The number of days past the initial requested date to stop sending reminders. After this cut-off, reminders will need to be sent manually by a person.",
        IsRequired = true,
        DefaultValue = "60",
        Order = 1)]

    [SystemEmailField("Assessment Reminder System Email",
        Key = AttributeKeys.AssessmentSystemEmail,
        Description = "",
        IsRequired = true,
        DefaultValue = Rock.SystemGuid.SystemEmail.ASSESSMENT_REQUEST,
        Order = 2)]

    #endregion Job Attributes

    [DisallowConcurrentExecution]
    public class SendAssessmentReminders : IJob
    {
        protected static class AttributeKeys
        {
            public const string ReminderEveryDays = "ReminderEveryDays";
            public const string CutoffDays = "CutoffDays";
            public const string AssessmentSystemEmail = "AssessmentSystemEmail";
        }

        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public SendAssessmentReminders()
        {
            // Intentionally left blank
        }

        /// <summary>
        /// Job that will send out Assessment reminders
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="NotImplementedException"></exception>
        public void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            var sendreminderDateTime = DateTime.Now.Date.AddDays( -1 * dataMap.GetInt( "ReminderEveryDays" ) );
            var cutOffDateTime = DateTime.Now.Date.AddDays( dataMap.GetInt( "CutoffDays" ) );
            var assessmentSystemEmailGuid = dataMap.GetString( "AssessmentSystemEmail" ).AsGuid();

            int assessmentRemindersSent = 0;
            int errorCount = 0;
            var errorMessages = new List<string>();
            
            using ( var rockContext = new RockContext() )
            {
                // Get a list of unique PersonAliasIDs from Assessments where the CreatedDateTime is less than the cut off date and LastReminderDate is null or greater than the reminder date.
                var assessmentService = new AssessmentService( rockContext );
                var personAliasIds = assessmentService
                    .Queryable()
                    .AsNoTracking()
                    .Where( a => a.Status == AssessmentRequestStatus.Pending )
                    .Where( a => a.RequestedDateTime < cutOffDateTime )
                    .Where( a => ( a.LastReminderDate == null && sendreminderDateTime >= DbFunctions.TruncateTime( a.RequestedDateTime ) ) ||
                        ( sendreminderDateTime >= DbFunctions.TruncateTime( a.LastReminderDate ) ) )
                    .Select( a => a.PersonAliasId )
                    .Distinct()
                    .ToList();

                // Go through the list, send a reminder, and update the LastReminderDate for all pending assessments for the person alias
                foreach( var personAliasId in personAliasIds )
                {
                    var errors = SendReminderEmail( assessmentSystemEmailGuid, personAliasId );

                    if ( errors.Any() )
                    {
                        errorMessages.AddRange( errors );
                    }
                    else
                    {
                        assessmentRemindersSent++;
                    }

                    assessmentService.UpdateLastReminderDateForPersonAlias( personAliasId );
                }

                context.Result = string.Format( "{0} assessment reminders sent", assessmentRemindersSent );
                if (errorMessages.Any())
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine();
                    sb.Append( string.Format( "{0} Errors: ", errorCount ));
                    errorMessages.ForEach( e => { sb.AppendLine(); sb.Append( e ); } );
                    string errors = sb.ToString();
                    context.Result += errors;
                    var exception = new Exception( errors );
                    HttpContext context2 = HttpContext.Current;
                    ExceptionLogService.LogException( exception, context2 );
                    throw exception;
                }
            }
        }

        private List<string> SendReminderEmail( Guid assessmentSystemEmailGuid, int PersonAliasId )
        {
            var person = new PersonAliasService( new RockContext() ).GetPerson( PersonAliasId );

            var mergeObjects = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
            mergeObjects.Add( "Person", person );

            var recipients = new List<RecipientData>();
            recipients.Add( new RecipientData( person.Email, mergeObjects ) );

            var errors = new List<string>();
            var emailMessage = new RockEmailMessage( assessmentSystemEmailGuid );
            emailMessage.SetRecipients( recipients );
            emailMessage.Send(out errors);

            return errors;
        }
    }
}
