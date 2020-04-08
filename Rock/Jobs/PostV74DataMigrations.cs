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
using System.ComponentModel;

using Quartz;

using Rock.Data;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// </summary>
    /// <seealso cref="Quartz.IJob" />
    [DisallowConcurrentExecution]
    [DisplayName( "Data Migrations for v7.4" )]
    [Description( "This job will take care of any data migrations that need to occur after updating to v74. After all the operations are done, this job will delete itself." )]
    public class PostV74DataMigrations : IJob
    {
        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            CommunicationPendingCommunicationMediumEntityTypeFix();
            bool canDeleteJob = true;

            if ( canDeleteJob )
            {
                // Verify that there are not any communication records with medium data.
                using ( var rockContext = new RockContext() )
                {
                    var jobId = context.GetJobId();
                    var jobService = new ServiceJobService( rockContext );
                    var job = jobService.Get( jobId );
                    if ( job != null )
                    {
                        jobService.Delete( job );
                        rockContext.SaveChanges();
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Set any missing MediumEntityTypeId of CommunicationRecipient records based on the communication's CommunicationType for any communications that haven't been sent yet
        /// </summary>
        public static void CommunicationPendingCommunicationMediumEntityTypeFix()
        {
            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.ExecuteSqlCommand( @"
DECLARE @MediumEntityTypeIdEmail INT = (
		SELECT TOP 1 Id
		FROM EntityType
		WHERE NAME = 'Rock.Communication.Medium.Email'
		)
DECLARE @MediumEntityTypeIdSMS INT = (
		SELECT TOP 1 Id
		FROM EntityType
		WHERE NAME = 'Rock.Communication.Medium.Sms'
		)
DECLARE @MediumEntityTypeIdPushNotification INT = (
		SELECT TOP 1 Id
		FROM EntityType
		WHERE NAME = 'Rock.Communication.Medium.PushNotification'
		)

-- Email
UPDATE CR
SET CR.MediumEntityTypeId = @MediumEntityTypeIdEmail
FROM [CommunicationRecipient] CR
JOIN [Communication] c ON cr.CommunicationId = c.Id
WHERE c.CommunicationType = 1 /* Email */
	AND CR.[Status] = 0 /* Pending/NotSent */
	AND CR.[MediumEntityTypeId] IS NULL

-- SMS
UPDATE CR
SET CR.MediumEntityTypeId = @MediumEntityTypeIdSMS
FROM [CommunicationRecipient] CR
JOIN [Communication] c ON cr.CommunicationId = c.Id
WHERE c.CommunicationType = 2 /* SMS */
	AND CR.[Status] = 0  /* Pending/NotSent */
	AND CR.[MediumEntityTypeId] IS NULL

-- PushNotification
UPDATE CR
SET CR.MediumEntityTypeId = @MediumEntityTypeIdPushNotification
FROM [CommunicationRecipient] CR
JOIN [Communication] c ON cr.CommunicationId = c.Id
WHERE c.CommunicationType = 3 /* PushNotification*/
	AND CR.[Status] = 0  /* Pending/NotSent */
	--AND c.Status != 0
	AND CR.[MediumEntityTypeId] IS NULL
" );
            }
        }
    }
}
