using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Com.CCVOnline.Service
{
    /// <summary>
    /// Blog POCO Entity.
    /// </summary>
    [Table( "_com.ccvonline.ServiceRecording" )]
    public partial class Recording : ModelWithAttributes<Recording>, IAuditable
    {
        [DataMember]
        public DateTime? Date { get; set; }

        [DataMember]
        public int? CampusId { get; set; }

        public virtual Rock.CRM.Campus Campus { get; set; }

        [MaxLength( 100 )]
        [DataMember]
        public string Label { get; set; }

        [MaxLength( 100 )]
        [DataMember]
        public string App { get; set; }

        [MaxLength( 100 )]
        [DataMember]
        public string StreamName { get; set; }

        [MaxLength( 100 )]
        [DataMember]
        public string RecordingName { get; set; }

        [DataMember]
        public DateTime? StartTime { get; set; }

        [MaxLength( 400 )]
        [DataMember]
        public string StartResponse { get; set; }

        [DataMember]
        public DateTime? StopTime { get; set; }

        [MaxLength( 400 )]
        [DataMember]
        public string StopResponse { get; set; }

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
        public override string AuthEntity { get { return "Custom.CCV.CommandCenter.ServiceRecording"; } }

        [NotMapped]
        public TimeSpan Length
        {
            get
            {
                if ( StartTime.HasValue && StopTime.HasValue )
                    return StopTime.Value.Subtract( StartTime.Value );
                else
                    return new TimeSpan( 0 );
            }
        }

        /// <summary>
        /// Gets the data transfer object.
        /// </summary>
        public Rock.Com.CCVOnline.Service.RecordingDTO DataTransferObject
        {
            get
            {
                var dto = new Rock.Com.CCVOnline.Service.RecordingDTO();
                dto.Id = this.Id;
                dto.Guid = this.Guid;
                dto.Date = this.Date;
                dto.CampusId = this.CampusId;
                dto.Label = this.Label;
                dto.App = this.App;
                dto.StreamName = this.StreamName;
                dto.RecordingName = this.RecordingName;
                dto.StartTime = this.StartTime;
                dto.StartResponse = this.StartResponse;
                dto.StopTime = this.StopTime;
                dto.StopResponse = this.StopResponse;
                dto.CreatedDateTime = this.CreatedDateTime;
                dto.ModifiedDateTime = this.ModifiedDateTime;
                dto.CreatedByPersonId = this.CreatedByPersonId;
                dto.ModifiedByPersonId = this.ModifiedByPersonId;
                return dto;
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="Recording"/> class.
        /// </summary>
        public Recording()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Recording"/> class.
        /// </summary>
        /// <param name="dto">The dto.</param>
        public Recording( Rock.Com.CCVOnline.Service.RecordingDTO dto )
            : base()
        {
            this.Id = dto.Id;
            this.Guid = dto.Guid;
            this.Date = dto.Date;
            this.CampusId = dto.CampusId;
            this.Label = dto.Label;
            this.App = dto.App;
            this.StreamName = dto.StreamName;
            this.RecordingName = dto.RecordingName;
            this.StartTime = dto.StartTime;
            this.StartResponse = dto.StartResponse;
            this.StopTime = dto.StopTime;
            this.StopResponse = dto.StopResponse;
            this.CreatedDateTime = dto.CreatedDateTime;
            this.ModifiedDateTime = dto.ModifiedDateTime;
            this.CreatedByPersonId = dto.CreatedByPersonId;
            this.ModifiedByPersonId = dto.ModifiedByPersonId;
        }

        public bool SendRequest(string action)
        {
            Rock.Net.WebResponse response = RecordingService.SendRecordingRequest( App, StreamName, RecordingName, action.ToLower() );

            if ( response != null && response.HttpStatusCode == System.Net.HttpStatusCode.OK )
            {

                if ( action.ToLower() == "start" )
                {
                    StartTime = DateTime.Now;
                    StartResponse = RecordingService.ParseResponse( response.Message );
                }
                else
                {
                    StopTime = DateTime.Now;
                    StopResponse = RecordingService.ParseResponse( response.Message );
                }

                return true;
            }

            return false;
        }

    }
    /// <summary>
    /// Blog Configuration class.
    /// </summary>
    public partial class RecordingConfiguration : EntityTypeConfiguration<Recording>
    {
        public RecordingConfiguration()
        {
			this.HasRequired( c => c.Campus ).WithMany().HasForeignKey( c => c.CampusId ).WillCascadeOnDelete(false);
		}
    }
}
