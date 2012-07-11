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

        [DataMember]
        public DateTime? StopTime { get; set; }

        [MaxLength( 100 )]
        [DataMember]
        public string RecordingPath { get; set; }

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


        public Recording()
            : base()
        {
        }

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
            this.StopTime = dto.StopTime;
            this.RecordingPath = dto.RecordingPath;
            this.CreatedDateTime = dto.CreatedDateTime;
            this.ModifiedDateTime = dto.ModifiedDateTime;
            this.CreatedByPersonId = dto.CreatedByPersonId;
            this.ModifiedByPersonId = dto.ModifiedByPersonId;
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
                dto.StopTime = this.StopTime;
                dto.RecordingPath = this.RecordingPath;
                dto.CreatedDateTime = this.CreatedDateTime;
                dto.ModifiedDateTime = this.ModifiedDateTime;
                dto.CreatedByPersonId = this.CreatedByPersonId;
                dto.ModifiedByPersonId = this.ModifiedByPersonId;
                return dto;
            }
        }


    }
    /// <summary>
    /// Blog Configuration class.
    /// </summary>
    public partial class RecordingConfiguration : EntityTypeConfiguration<Recording>
    {
    }
}
