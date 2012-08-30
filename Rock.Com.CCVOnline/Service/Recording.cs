using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Com.CCVOnline.Service
{
    /// <summary>
    /// Blog POCO Entity.
    /// </summary>
    [Table( "_com.ccvonline.ServiceRecording" )]
    public partial class Recording : ModelWithAttributes<Recording>, IAuditable, IExportable
    {
        [DataMember]
        public DateTime? Date { get; set; }

        [DataMember]
        public int? CampusId { get; set; }

        public virtual Rock.Crm.Campus Campus { get; set; }

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
        /// Initializes a new instance of the <see cref="Recording"/> class.
        /// </summary>
        public Recording()
            : base()
        {
        }

		/// <summary>
		/// Exports the object as JSON.
		/// </summary>
		/// <returns></returns>
		public string ExportJson()
		{
			return ExportObject().ToJSON();
		}

		/// <summary>
		/// Exports the object.
		/// </summary>
		/// <returns></returns>
		public object ExportObject()
		{
			return this.ToDynamic();
		}

		/// <summary>
		/// Imports the object from JSON.
		/// </summary>
		/// <param name="data">The data.</param>
		public void ImportJson( string data )
		{
			throw new NotImplementedException();
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
