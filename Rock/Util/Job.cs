//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Util
{
    /// <summary>
    /// Job POCO Entity.
    /// </summary>
    [Table( "utilJob" )]
    public partial class Job : ModelWithAttributes<Job>, IAuditable
    {
		/// <summary>
		/// Gets or sets the System.
		/// </summary>
		/// <value>
		/// Determines whether the job is a system job..
		/// </value>
		[DataMember]
		public bool IsSystem { get; set; }
		
		/// <summary>
		/// Gets or sets the Active.
		/// </summary>
		/// <value>
		/// Determines is the job is currently active..
		/// </value>
		[DataMember]
		public bool? IsActive { get; set; }
		
		/// <summary>
		/// Gets or sets the Name.
		/// </summary>
		/// <value>
		/// Friendly name for the job..
		/// </value>
		[Required]
		[MaxLength( 100 )]
		[DataMember]
		public string Name { get; set; }
		
		/// <summary>
		/// Gets or sets the Description.
		/// </summary>
		/// <value>
		/// Notes about the job..
		/// </value>
		[MaxLength( 500 )]
		[DataMember]
		public string Description { get; set; }
		
		/// <summary>
		/// Gets or sets the Assemby.
		/// </summary>
		/// <value>
		/// Assembly (.dll) that contains the job class..
		/// </value>
		[MaxLength( 100 )]
		[DataMember]
		public string Assemby { get; set; }
		
		/// <summary>
		/// Gets or sets the Class.
		/// </summary>
		/// <value>
		/// The class name of the job to run..
		/// </value>
		[Required]
		[MaxLength( 100 )]
		[DataMember]
		public string Class { get; set; }
		
		/// <summary>
		/// Gets or sets the Cron Expression.
		/// </summary>
		/// <value>
		/// The cron expression that is used to determine the schedule of the job (see http://www.quartz-scheduler.org/documentation/quartz-1.x/tutorials/crontrigger for syntax.).
		/// </value>
		[Required]
		[MaxLength( 120 )]
		[DataMember]
		public string CronExpression { get; set; }
		
		/// <summary>
		/// Gets or sets the Last Successful Run.
		/// </summary>
		/// <value>
		/// Date and time the job last completed successfully..
		/// </value>
		[DataMember]
		public DateTime? LastSuccessfulRun { get; set; }
		
		/// <summary>
		/// Gets or sets the Last Run Date.
		/// </summary>
		/// <value>
		/// Last date and time the job attempted to run..
		/// </value>
		[DataMember]
		public DateTime? LastRunDate { get; set; }
		
		/// <summary>
		/// Gets or sets the Last Run Duration.
		/// </summary>
		/// <value>
		/// Number of seconds that the last job took to finish..
		/// </value>
		[DataMember]
		public int? LastRunDuration { get; set; }
		
		/// <summary>
		/// Gets or sets the Last Status.
		/// </summary>
		/// <value>
		/// The completion status from the last time the job was run (valid values 'Success', 'Exception', 'Error Loading Job')..
		/// </value>
		[MaxLength( 50 )]
		[DataMember]
		public string LastStatus { get; set; }
		
		/// <summary>
		/// Gets or sets the Last Status Message.
		/// </summary>
		/// <value>
		/// Message from the last run.  Usually used to store the exception message..
		/// </value>
		[DataMember]
		public string LastStatusMessage { get; set; }
		
		/// <summary>
		/// Gets or sets the Last Run Scheduler Name.
		/// </summary>
		/// <value>
		/// Name of the scheduler that the job ran under.  This is used to determine if a job ran in IIS or the Windows service..
		/// </value>
		[MaxLength( 40 )]
		[DataMember]
		public string LastRunSchedulerName { get; set; }
		
		/// <summary>
		/// Gets or sets the Notification Emails.
		/// </summary>
		/// <value>
		/// Email addresses (separated with commas) to be used for notification..
		/// </value>
		[MaxLength( 1000 )]
		[DataMember]
		public string NotificationEmails { get; set; }
		
		/// <summary>
		/// Gets or sets the Notification Status.
		/// </summary>
		/// <value>
		/// Enum[JobNotificationStatus].
		/// </value>
		[Required]
		[DataMember]
        public JobNotificationStatus NotificationStatus { get; set; }

		/// <summary>
		/// Gets or sets the Created Date Time.
		/// </summary>
		/// <value>
		/// Created Date Time.
		/// </value>
		[DataMember]
		public DateTime? CreatedDateTime { get; set; }
		
		/// <summary>
		/// Gets or sets the Modified Date Time.
		/// </summary>
		/// <value>
		/// Modified Date Time.
		/// </value>
		[DataMember]
		public DateTime? ModifiedDateTime { get; set; }
		
		/// <summary>
		/// Gets or sets the Created By Person Id.
		/// </summary>
		/// <value>
		/// Created By Person Id.
		/// </value>
		[DataMember]
		public int? CreatedByPersonId { get; set; }
		
		/// <summary>
		/// Gets or sets the Modified By Person Id.
		/// </summary>
		/// <value>
		/// Modified By Person Id.
		/// </value>
		[DataMember]
		public int? ModifiedByPersonId { get; set; }

        /// <summary>
        /// Gets the auth entity.
        /// </summary>
		[NotMapped]
		public override string AuthEntity { get { return "Util.Job"; } }
        
		/// <summary>
        /// Gets or sets the Created By Person.
        /// </summary>
        /// <value>
        /// A <see cref="CRM.Person"/> object.
        /// </value>
		public virtual CRM.Person CreatedByPerson { get; set; }
        
		/// <summary>
        /// Gets or sets the Modified By Person.
        /// </summary>
        /// <value>
        /// A <see cref="CRM.Person"/> object.
        /// </value>
		public virtual CRM.Person ModifiedByPerson { get; set; }

    }

    /// <summary>
    /// Job Configuration class.
    /// </summary>
    public partial class JobConfiguration : EntityTypeConfiguration<Job>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JobConfiguration"/> class.
        /// </summary>
        public JobConfiguration()
        {
			this.HasOptional( p => p.CreatedByPerson ).WithMany().HasForeignKey( p => p.CreatedByPersonId ).WillCascadeOnDelete(false);
			this.HasOptional( p => p.ModifiedByPerson ).WithMany().HasForeignKey( p => p.ModifiedByPersonId ).WillCascadeOnDelete(false);
		}
    }

    /// <summary>
    /// Data Transformation Object
    /// </summary>
    public partial class JobDTO : DTO<Job>
    {
        /// <summary>
        /// Gets or sets the System.
        /// </summary>
        /// <value>
        /// System.
        /// </value>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Active.
        /// </summary>
        /// <value>
        /// Active.
        /// </value>
        public bool? IsActive { get; set; }

        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        /// <value>
        /// Name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Description.
        /// </summary>
        /// <value>
        /// Description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the Assemby.
        /// </summary>
        /// <value>
        /// Assemby.
        /// </value>
        public string Assemby { get; set; }

        /// <summary>
        /// Gets or sets the Class.
        /// </summary>
        /// <value>
        /// Class.
        /// </value>
        public string Class { get; set; }

        /// <summary>
        /// Gets or sets the Cron Expression.
        /// </summary>
        /// <value>
        /// Cron Expression.
        /// </value>
        public string CronExpression { get; set; }

        /// <summary>
        /// Gets or sets the Last Successful Run.
        /// </summary>
        /// <value>
        /// Last Successful Run.
        /// </value>
        public DateTime? LastSuccessfulRun { get; set; }

        /// <summary>
        /// Gets or sets the Last Run Date.
        /// </summary>
        /// <value>
        /// Last Run Date.
        /// </value>
        public DateTime? LastRunDate { get; set; }

        /// <summary>
        /// Gets or sets the Last Run Duration.
        /// </summary>
        /// <value>
        /// Last Run Duration.
        /// </value>
        public int? LastRunDuration { get; set; }

        /// <summary>
        /// Gets or sets the Last Status.
        /// </summary>
        /// <value>
        /// Last Status.
        /// </value>
        public string LastStatus { get; set; }

        /// <summary>
        /// Gets or sets the Last Status Message.
        /// </summary>
        /// <value>
        /// Last Status Message.
        /// </value>
        public string LastStatusMessage { get; set; }

        /// <summary>
        /// Gets or sets the Last Run Scheduler Name.
        /// </summary>
        /// <value>
        /// Last Run Scheduler Name.
        /// </value>
        public string LastRunSchedulerName { get; set; }

        /// <summary>
        /// Gets or sets the Notification Emails.
        /// </summary>
        /// <value>
        /// Notification Emails.
        /// </value>
        public string NotificationEmails { get; set; }

        /// <summary>
        /// Gets or sets the Notification Status.
        /// </summary>
        /// <value>
        /// Notification Status.
        /// </value>
        public int NotificationStatus { get; set; }

        /// <summary>
        /// Instantiate new DTO object
        /// </summary>
        public JobDTO()
        {
        }

        /// <summary>
        /// Instantiate new DTO object from Model
        /// </summary>
        /// <param name="auth"></param>
        public JobDTO( Job job )
        {
            CopyFromModel( job );
        }

        /// <summary>
        /// Copy DTO to Model
        /// </summary>
        /// <param name="job"></param>
        public override void CopyFromModel( Job job )
        {
            this.Id = job.Id;
            this.Guid = job.Guid;
            this.IsSystem = job.IsSystem;
            this.IsActive = job.IsActive;
            this.Name = job.Name;
            this.Description = job.Description;
            this.Assemby = job.Assemby;
            this.Class = job.Class;
            this.CronExpression = job.CronExpression;
            this.LastSuccessfulRun = job.LastSuccessfulRun;
            this.LastRunDate = job.LastRunDate;
            this.LastRunDuration = job.LastRunDuration;
            this.LastStatus = job.LastStatus;
            this.LastStatusMessage = job.LastStatusMessage;
            this.LastRunSchedulerName = job.LastRunSchedulerName;
            this.NotificationEmails = job.NotificationEmails;
            this.NotificationStatus = ( int )job.NotificationStatus;
            this.CreatedDateTime = job.CreatedDateTime;
            this.ModifiedDateTime = job.ModifiedDateTime;
            this.CreatedByPersonId = job.CreatedByPersonId;
            this.ModifiedByPersonId = job.ModifiedByPersonId;
        }

        /// <summary>
        /// Copy Model to DTO
        /// </summary>
        /// <param name="job"></param>
        public override void CopyToModel( Job job )
        {
            job.Id = this.Id;
            job.Guid = this.Guid;
            job.IsSystem = this.IsSystem;
            job.IsActive = this.IsActive;
            job.Name = this.Name;
            job.Description = this.Description;
            job.Assemby = this.Assemby;
            job.Class = this.Class;
            job.CronExpression = this.CronExpression;
            job.LastSuccessfulRun = this.LastSuccessfulRun;
            job.LastRunDate = this.LastRunDate;
            job.LastRunDuration = this.LastRunDuration;
            job.LastStatus = this.LastStatus;
            job.LastStatusMessage = this.LastStatusMessage;
            job.LastRunSchedulerName = this.LastRunSchedulerName;
            job.NotificationEmails = this.NotificationEmails;
            job.NotificationStatus = ( JobNotificationStatus )this.NotificationStatus;
            job.CreatedDateTime = this.CreatedDateTime;
            job.ModifiedDateTime = this.ModifiedDateTime;
            job.CreatedByPersonId = this.CreatedByPersonId;
            job.ModifiedByPersonId = this.ModifiedByPersonId;
        }
    }

    /// <summary>
    /// Job notification status
    /// </summary>
    public enum JobNotificationStatus
    {
        /// <summary>
        /// Notify on all status
        /// </summary>
        All = 1,

        /// <summary>
        /// Notify when successful
        /// </summary>
        /// 
        Success = 2,

        /// <summary>
        /// Notify when an error occurs
        /// </summary>
        Error = 3,

        /// <summary>
        /// Notify when a warning occurs
        /// </summary>
        None = 4
    }
}
