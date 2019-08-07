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
using System.Text;

using Quartz;

using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Jobs
{

    /// <summary>
    /// Job to send an alert if communication queue is not being sent
    /// </summary>
    [IntegerField( "Alert Period", "The number of minutes to allow for communications to be sent before sending an alert.", false, 120, "", 0 )]
    [SystemEmailField( "Alert Email", "The system email to use for sending an alert", true, "2fc7d3e3-d85b-4265-8983-970345215dea", "", 1 )]
    [TextField( "Alert Recipients", "A comma-delimited list of recipients that should receive the alert", true, "", "", 2 )]
    [DisallowConcurrentExecution]
    public class CommunicationQueueAlert : IJob
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SendCommunications"/> class.
        /// </summary>
        public CommunicationQueueAlert()
        {
        }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public virtual void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            int alertPeriod = dataMap.GetInt( "AlertPeriod" );
            Guid? systemEmailGuid = dataMap.GetString( "AlertEmail" ).AsGuidOrNull();
            List<string> recipientEmails = dataMap.GetString( "AlertRecipients" ).SplitDelimitedValues().ToList();

            if ( systemEmailGuid.HasValue && recipientEmails.Any() )
            {
                var rockContext = new RockContext();

                int expirationDays = GetJobAttributeValue( "ExpirationPeriod", 3, rockContext );
                var cutoffTime = RockDateTime.Now.AddMinutes( 0 - alertPeriod );

                var communications = new CommunicationService( rockContext )
                    .GetQueued( expirationDays, alertPeriod, false, false )
                    .Where( c => !c.ReviewedDateTime.HasValue || c.ReviewedDateTime.Value.CompareTo( cutoffTime ) < 0 )     // Make sure communication wasn't just recently approved
                    .OrderBy( c => c.Id )
                    .ToList();

                if ( communications.Any() )
                {
                    var mergeFields = Lava.LavaHelper.GetCommonMergeFields( null );
                    mergeFields.Add( "Communications", communications );

                    var emailMessage = new RockEmailMessage( systemEmailGuid.Value );
                    foreach ( var email in recipientEmails )
                    {
                        emailMessage.AddRecipient( new RecipientData( email, mergeFields ) );
                    }

                    var errors = new List<string>();
                    emailMessage.Send( out errors );

                    context.Result = string.Format( "Notified about {0} queued communications. {1} errors encountered.", communications.Count, errors.Count );
                    if ( errors.Any() )
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine();
                        sb.Append( "Errors: " );
                        errors.ForEach( e => { sb.AppendLine(); sb.Append( e ); } );
                        string errorMessage = sb.ToString();
                        context.Result += errorMessage;
                        throw new Exception( errorMessage );
                    }
                }
            }
        }

        private int GetJobAttributeValue( string key, int defaultValue, RockContext rockContext )
        {
            var jobEntityType = EntityTypeCache.Get( typeof( Rock.Model.ServiceJob ) );

            int intValue = 3;
            var jobExpirationAttribute = new AttributeService( rockContext )
                .Queryable().AsNoTracking()
                .Where( a =>
                    a.EntityTypeId == jobEntityType.Id &&
                    a.EntityTypeQualifierColumn == "Class" &&
                    a.EntityTypeQualifierValue == "Rock.Jobs.SendCommunications" &&
                    a.Key == key )
                .FirstOrDefault();
            if ( jobExpirationAttribute != null )
            {
                intValue = jobExpirationAttribute.DefaultValue.AsIntegerOrNull() ?? 3;
                var attributeValue = new AttributeValueService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( v => v.AttributeId == jobExpirationAttribute.Id )
                    .FirstOrDefault();
                if ( attributeValue != null )
                {
                    intValue = attributeValue.Value.AsIntegerOrNull() ?? intValue;
                }
            }

            return intValue;
        }
    }
}