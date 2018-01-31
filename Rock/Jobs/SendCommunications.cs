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
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.IO;

using Quartz;

using Rock.Attribute;
using Rock.Model;
using Rock.Data;

namespace Rock.Jobs
{

    /// <summary>
    /// Job to process communications
    /// </summary>
    [IntegerField( "Delay Period", "The number of minutes to wait before sending any new communication (If the communication block's 'Send When Approved' option is turned on, then a delay should be used here to prevent a send overlap).", false, 30, "", 0 )]
    [IntegerField( "Expiration Period", "The number of days after a communication was created or scheduled to be sent when it should no longer be sent.", false, 3, "", 1 )]
    [DisallowConcurrentExecution]
    public class SendCommunications : IJob
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SendCommunications"/> class.
        /// </summary>
        public SendCommunications()
        {
        }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public virtual void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            int expirationDays = dataMap.GetInt( "ExpirationPeriod" );
            int delayMinutes = dataMap.GetInt( "DelayPeriod" );

            var rockContext = new RockContext();
            var qry = new CommunicationService( rockContext )
                .GetQueued( expirationDays, delayMinutes, false, false )
                .OrderBy( c => c.Id );

            var exceptionMsgs = new List<string>();
            int communicationsSent = 0;
            
            foreach ( var comm in qry.AsNoTracking().ToList() )
            {
                try
                {
                    Rock.Model.Communication.Send( comm );
                    communicationsSent++;
                }

                catch ( Exception ex )
                {
                    exceptionMsgs.Add( string.Format( "Exception occurred sending communication ID:{0}:{1}    {2}", comm.Id, Environment.NewLine, ex.Messages().AsDelimited( Environment.NewLine + "   " ) ) );
                    ExceptionLogService.LogException( ex, System.Web.HttpContext.Current );
                }
            }

            if ( communicationsSent > 0 )
            {
                context.Result = string.Format( "Sent {0} {1}", communicationsSent, "communication".PluralizeIf( communicationsSent > 1 ) );
            }
            else
            {
                context.Result = "No communications to send";
            }

            if ( exceptionMsgs.Any() )
            {
                throw new Exception( "One or more exceptions occurred sending communications..." + Environment.NewLine + exceptionMsgs.AsDelimited( Environment.NewLine ) );
            }

            // check for communications that have not been sent but are past the expire date. Mark them as failed and set a warning.
            var beginWindow = RockDateTime.Now.AddDays( 0 - expirationDays );
            var qryExpired = new CommunicationService( rockContext ).Queryable()
                .Where( c =>
                    c.Status == CommunicationStatus.Approved &&
                    c.Recipients.Any( r => r.Status == CommunicationRecipientStatus.Pending ) &&
                    (
                        (!c.FutureSendDateTime.HasValue && c.CreatedDateTime.HasValue && c.CreatedDateTime.Value.CompareTo( beginWindow ) < 0 ) ||
                        (c.FutureSendDateTime.HasValue && c.FutureSendDateTime.Value.CompareTo( beginWindow ) < 0 )
                    ) );

            foreach ( var comm in qryExpired.ToList() )
            {
                foreach ( var recipient in comm.Recipients.Where( r => r.Status == CommunicationRecipientStatus.Pending ) )
                {
                    recipient.Status = CommunicationRecipientStatus.Failed;
                    recipient.StatusNote = "Communication was not sent before the expire window (possibly due to delayed approval).";
                    rockContext.SaveChanges();
                }
            }

        }
    }
}