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
using System.ComponentModel;

namespace Rock.Jobs
{
    #region Job Attributes

    /// <summary>
    /// Sends reminders to persons with pending assessments if the created date/time is less than the calculated cut off date and the last reminder date is greater than the calculated reminder date.
    /// </summary>
    /// <seealso cref="Quartz.IJob" />
    [DisplayName( "Send Assessment Reminders" )]
    [Description( "Sends reminders to persons with pending assessments if the created date/time is less than the calculated cut off date and the last reminder date is greater than the calculated reminder date." )]

    [IntegerField( "Reminder Every",
            Key = AttributeKeys.ReminderEveryDays,
            Description = "The number of days between reminder emails.",
            IsRequired = true,
            DefaultValue = "7",
            Order = 0 )]

    [IntegerField( "Cut off Days",
            Key = AttributeKeys.CutoffDays,
            Description = "The number of days past the initial requested date to stop sending reminders. After this cut-off, reminders will need to be sent manually by a person.",
            IsRequired = true,
            DefaultValue = "60",
            Order = 1 )]

    [SystemCommunicationField( "Assessment Reminder System Email",
            Key = AttributeKeys.AssessmentSystemEmail,
            Description = "",
            IsRequired = true,
            DefaultValue = Rock.SystemGuid.SystemCommunication.ASSESSMENT_REQUEST,
            Order = 2 )]

    #endregion Job Attributes

    [DisallowConcurrentExecution]
    public class SendAssessmentReminders : IJob
    {
        /// <summary>
        /// 
        /// </summary>
        private static class AttributeKeys
        {
            /// <summary>
            /// The reminder every days
            /// </summary>
            public const string ReminderEveryDays = "ReminderEveryDays";

            /// <summary>
            /// The cutoff days
            /// </summary>
            public const string CutoffDays = "CutoffDays";

            /// <summary>
            /// The assessment system email
            /// </summary>
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
            DateTime sendreminderDateTime = DateTime.Now.Date.AddDays( -1 * dataMap.GetInt( AttributeKeys.ReminderEveryDays ) );
            int cutOffDays = dataMap.GetInt( AttributeKeys.CutoffDays );
            var assessmentSystemEmailGuid = dataMap.GetString( AttributeKeys.AssessmentSystemEmail ).AsGuid();

            DateTime currentDate = DateTime.Now.Date;
            var result = new SendMessageResult();

            using ( var rockContext = new RockContext() )
            {
                // Get a list of unique PersonAliasIDs from Assessments where the CreatedDateTime is less than the cut off date and LastReminderDate is null or greater than the reminder date.
                // Only the latest assessment for each type and person is considered. For example a past DISC assessment that is still pending but a newer one is complete. The past one will
                // not be considered.
                var assessmentService = new AssessmentService( rockContext );
                var personAliasIds = assessmentService
                    .GetLatestAssessments()
                    .AsNoTracking()
                    .Where( a => a.Status == AssessmentRequestStatus.Pending )
                    .Where( a => currentDate <= DbFunctions.AddDays( a.RequestedDateTime, cutOffDays ) )
                    .Where( a => ( a.LastReminderDate == null && sendreminderDateTime >= DbFunctions.TruncateTime( a.RequestedDateTime ) ) ||
                        ( sendreminderDateTime >= DbFunctions.TruncateTime( a.LastReminderDate ) ) )
                    .Select( a => a.PersonAliasId )
                    .Distinct()
                    .ToList();

                // Go through the list, send a reminder, and update the LastReminderDate for all pending assessments for the person alias
                foreach ( var personAliasId in personAliasIds )
                {
                    var sendResult = SendReminderEmail( assessmentSystemEmailGuid, personAliasId );

                    result.MessagesSent += sendResult.MessagesSent;
                    result.Warnings.AddRange( sendResult.Warnings );
                    result.Errors.AddRange( sendResult.Errors );

                    assessmentService.UpdateLastReminderDateForPersonAlias( personAliasId );
                }

                var results = new StringBuilder();
                results.AppendLine( $"{result.MessagesSent}  assessment reminders sent." );
                if ( result.Warnings.Count > 0 )
                {
                    var warning = "Warning".PluralizeIf( result.Warnings.Count > 1 );
                    results.AppendLine( $"{result.Warnings.Count} {warning}:" );
                    result.Warnings.ForEach( e => { results.AppendLine( e ); } );
                }
                context.Result = results.ToString();

                if ( result.Errors.Any() )
                {
                    var error = "Error".PluralizeIf( result.Errors.Count > 1 );

                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine();
                    sb.AppendLine( $"{result.Errors.Count()} {error}: " );
                    result.Errors.ForEach( e => { sb.AppendLine(); sb.Append( e ); } );
                    string errors = sb.ToString();
                    context.Result += errors;
                    var exception = new Exception( errors );
                    HttpContext context2 = HttpContext.Current;
                    ExceptionLogService.LogException( exception, context2 );
                    throw exception;
                }
            }
        }

        private SendMessageResult SendReminderEmail( Guid assessmentSystemEmailGuid, int PersonAliasId )
        {
            var person = new PersonAliasService( new RockContext() ).GetPerson( PersonAliasId );
            var result = new SendMessageResult();
            if ( !person.IsEmailActive )
            {
                result.Warnings.Add( $"{person.FullName.ToPossessive()} email address is inactive." );
                return result;
            }

            var mergeObjects = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
            mergeObjects.Add( "Person", person );

            var recipients = new List<RockEmailMessageRecipient>();
            recipients.Add( new RockEmailMessageRecipient( person, mergeObjects ) );

            var emailMessage = new RockEmailMessage( assessmentSystemEmailGuid );
            emailMessage.SetRecipients( recipients );

            if ( emailMessage.Send( out var errors ) )
            {
                result.MessagesSent = 1;
            }
            else
            {
                result.Errors.AddRange( errors );
            }
            return result;
        }
    }
}
