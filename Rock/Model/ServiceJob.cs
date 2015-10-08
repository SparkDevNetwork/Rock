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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents a scheduled job/routine in Rock. A job class can have multiple ServiceJob instances associated with it in the event that it has different attributes or 
    /// has multiple schedules.  For more information on how to create a job see https://github.com/SparkDevNetwork/Rock/wiki/Rock-Jobs
    /// </summary>
    [Table( "ServiceJob" )]
    [DataContract]
    public partial class ServiceJob : Model<ServiceJob>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets a flag indicating if this Job is part of the Rock core system/framework
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if the Job is part of the Rock core system/framework;
        /// otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsSystem { get; set; }
        
        /// <summary>
        /// Gets or sets a flag indicating if the Job is active.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if the Job is active; otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool? IsActive { get; set; }
        
        /// <summary>
        /// Gets or sets the friendly Name of the Job. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> containing the friendly Name of the Job.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets a user defined description of the Job.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the description of the Job.
        /// </value>
        [DataMember]
        public string Description { get; set; }
        
        /// <summary>
        /// Gets or sets the Assembly name of the .dll file that contains the job class.
        /// Set this to null to have Rock figure out the Assembly automatically.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that contains the Assembly name of the .dll file that contains the job class.
        /// </value>
        [MaxLength( 260 )]
        [DataMember]
        public string Assembly { get; set; }
        
        /// <summary>
        /// Gets or sets the fully qualified class name with Namespace of the Job class. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> containing the fully qualified class name with Namespace of the Job class.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Class { get; set; }
        
        /// <summary>
        /// Gets or sets the Cron Expression that is used to schedule the Job. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that contains the Cron expression that is used to determine the schedule for the job.
        /// </value>
        /// <remarks>
        /// See  http://www.quartz-scheduler.org/documentation/quartz-1.x/tutorials/crontrigger for the syntax.
        /// </remarks>
        [Required]
        [MaxLength( 120 )]
        [DataMember( IsRequired = true )]
        public string CronExpression { get; set; }
        
        /// <summary>
        /// Gets or sets the date and time that the Job last completed successfully.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date and time of the last time that the Job completed successfully
        /// </value>
        [DataMember]
        public DateTime? LastSuccessfulRunDateTime { get; set; }
        
        /// <summary>
        /// Gets or sets the date and time that the job last ran.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> that represents the last time that the job ran.
        /// </value>
        [DataMember]
        public DateTime? LastRunDateTime { get; set; }
        
        /// <summary>
        /// Gets or set the amount of time, in seconds, that it took the job to run the last time that it ran.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the amount of time, in seconds, that it took the job to run the last time that it ran.
        /// </value>
        [DataMember]
        public int? LastRunDurationSeconds { get; set; }
        
        /// <summary>
        /// Gets or sets the completion status that was returned by the Job the last time that it ran.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> containing the status that was returned by the Job the last time that it ran.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string LastStatus { get; set; }
        
        /// <summary>
        /// Gets or sets the status message that was returned the last time that the job was run. In most cases this will be used
        /// in the event of an exception to return the exception message.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Status Message that returned the last time that the job ran.
        /// </value>
        [DataMember]
        public string LastStatusMessage { get; set; }
        
        /// <summary>
        /// Gets or sets the name of the scheduler that the job ran under the last time that it ran. In most cases this 
        /// is used to determine if the was run by the IIS or Windows service.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the name of the Scheduler that the job ran under the last time that it was run.
        /// </value>
        [MaxLength( 40 )]
        [DataMember]
        public string LastRunSchedulerName { get; set; }
        
        /// <summary>
        /// Gets or sets a comma delimited list of email address that should receive notification emails for this job. Notification
        /// emails are sent to these email addresses based on the completion status of the Job and the <see cref="NotificationStatus"/>
        /// property of this job.
        /// </summary>
        /// <value>
        /// A <see cref="System.String" /> representing a list of email addresses that should receive notifications for this job.
        /// </value>
        [MaxLength( 1000 )]
        [DataMember]
        public string NotificationEmails { get; set; }
        
        /// <summary>
        /// Gets or sets the NotificationStatus for this job, this property determines when notification emails should be sent to the <see cref="NotificationEmails"/>
        /// that are associated with this Job
        /// </summary>
        /// <value>
        /// An <see cref="Rock.Model.JobNotificationStatus"/> that indicates when notification emails should be sent for this job. 
        /// When this value is <c>JobNotificationStatus.All</c> a notification email will be sent when the Job completes with any completion status.
        /// When this value is <c>JobNotificationStatus.Success</c> a notification email will be sent when the Job has completed successfully.
        /// When this value is <c>JobNotificationStatus.Error</c> a notification email will be sent when the Job completes with an error status.
        /// When this value is <c>JobNotificationStatus.None</c> notifications will not be sent when the Job completes with any status.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public JobNotificationStatus NotificationStatus { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this Job.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this Job.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// Job Configuration class.
    /// </summary>
    public partial class ServiceJobConfiguration : EntityTypeConfiguration<ServiceJob>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceJobConfiguration"/> class.
        /// </summary>
        public ServiceJobConfiguration()
        {
        }
    }

    #endregion

    #region Enumerations

    /// <summary>
    /// An enum that represents when a Job notification status should be sent.
    /// </summary>
    public enum JobNotificationStatus
    {
        /// <summary>
        /// Notifications should be sent when a job completes with any notification status.
        /// </summary>
        All = 1,

        /// <summary>
        /// Notification should be sent when the job has completed successfully.
        /// </summary>
        /// 
        Success = 2,

        /// <summary>
        /// Notification should be sent when the job has completed with an error status.
        /// </summary>
        Error = 3,

        /// <summary>
        /// Notifications should not be sent when this job completes with any status.
        /// </summary>
        None = 4
    }

    #endregion

}
