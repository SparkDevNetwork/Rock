using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Custom.CCVOnline.CommandCenter
{
    /// <summary>
    /// Blog POCO Entity.
    /// </summary>
    [Table( "ccvonlineRecording" )]
    public partial class Recording : ModelWithAttributes<Recording>, IAuditable
    {
        [Required]
        [DataMember]
        public bool System { get; set; }

        [DataMember]
        public DateTime? Date { get; set; }

        [DataMember]
        public int? CampusId { get; set; }

        [MaxLength( 100 )]
        [DataMember]
        public string Label { get; set; }

        [MaxLength( 100 )]
        [DataMember]
        public string Application { get; set; }

        [MaxLength( 100 )]
        [DataMember]
        public string Stream { get; set; }

        [MaxLength( 100 )]
        [DataMember]
        public string RecordingText { get; set; }

        [DataMember]
        public DateTime? StartTime { get; set; }

        [DataMember]
        public DateTime? StopDate { get; set; }

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
		public override string AuthEntity { get { return "Custom.CCVOnline.CommandCenter.Recording"; } }
       
    }
    /// <summary>
    /// Blog Configuration class.
    /// </summary>
    public partial class RecordingConfiguration : EntityTypeConfiguration<Recording>
    {
    }
}
