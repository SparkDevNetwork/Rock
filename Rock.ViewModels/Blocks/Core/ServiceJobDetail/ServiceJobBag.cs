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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Core.ServiceJobDetail
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Utility.EntityBagBase" />
    public class ServiceJobBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the Assembly name of the .dll file that contains the job class.
        /// Set this to null to have Rock figure out the Assembly automatically.
        /// </summary>
        public string Assembly { get; set; }

        /// <summary>
        /// Gets or sets the fully qualified class name with Namespace of the Job class. This property is required.
        /// </summary>
        public string Class { get; set; }

        /// <summary>
        /// Gets or sets the Cron Expression that is used to schedule the Job. This property is required.
        /// </summary>
        public string CronExpression { get; set; }

        /// <summary>
        /// Gets or sets a user defined description of the Job.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether jobs should be logged in ServiceJobHistory
        /// </summary>
        public bool EnableHistory { get; set; }

        /// <summary>
        /// Gets or sets the history count per job.
        /// </summary>
        public int HistoryCount { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the Job is active.
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this Job is part of the Rock core system/framework
        /// </summary>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the status message that was returned the last time that the job was run. In most cases this will be used
        /// in the event of an exception to return the exception message.
        /// </summary>
        public string LastStatusMessage { get; set; }

        /// <summary>
        /// Gets or sets the friendly Name of the Job. This property is required.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a comma delimited list of email address that should receive notification emails for this job. Notification
        /// emails are sent to these email addresses based on the completion status of the Job and the Rock.Model.ServiceJob.NotificationStatus
        /// property of this job.
        /// </summary>
        public string NotificationEmails { get; set; }

        /// <summary>
        /// Gets or sets the cron description.
        /// </summary>
        public string CronDescription { get; set; }

        /// <summary>
        /// Gets or sets the notification status.
        /// </summary>
        /// <value>
        /// The notification status.
        /// </value>
        public string NotificationStatus { get; set; }
    }
}
