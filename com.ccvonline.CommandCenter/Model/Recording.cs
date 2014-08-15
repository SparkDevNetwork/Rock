using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

using com.ccvonline.CommandCenter.Data;

using Rock.Model;

namespace com.ccvonline.CommandCenter.Model
{
    /// <summary>
    /// 
    /// </summary>
    [Table( "_com_ccvonline_CommandCenter_Recording" )]
    [DataContract]
    public class Recording : Model<Recording>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the campus id.
        /// </summary>
        /// <value>
        /// The campus id.
        /// </value>
        [DataMember]
        public int? CampusId { get; set; }

        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        /// <value>
        /// The date.
        /// </value>
        [DataMember]
        public DateTime? Date { get; set; }

        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        /// <value>
        /// The label.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the app.
        /// </summary>
        /// <value>
        /// The app.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string App { get; set; }

        /// <summary>
        /// Gets or sets the name of the stream.
        /// </summary>
        /// <value>
        /// The name of the stream.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string StreamName { get; set; }

        /// <summary>
        /// Gets or sets the name of the recording.
        /// </summary>
        /// <value>
        /// The name of the recording.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string RecordingName { get; set; }

        /// <summary>
        /// Gets or sets the start time.
        /// </summary>
        /// <value>
        /// The start time.
        /// </value>
        [DataMember]
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// Gets or sets the start response.
        /// </summary>
        /// <value>
        /// The start response.
        /// </value>
        [MaxLength( 400 )]
        [DataMember]
        public string StartResponse { get; set; }

        /// <summary>
        /// Gets or sets the stop time.
        /// </summary>
        /// <value>
        /// The stop time.
        /// </value>
        [DataMember]
        public DateTime? StopTime { get; set; }

        /// <summary>
        /// Gets or sets the stop response.
        /// </summary>
        /// <value>
        /// The stop response.
        /// </value>
        [MaxLength( 400 )]
        [DataMember]
        public string StopResponse { get; set; }

        /// <summary>
        /// Gets or sets the Venue
        /// </summary>
        /// <value>
        /// The venue.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string Venue { get; set; }


        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the campus.
        /// </summary>
        /// <value>
        /// The campus.
        /// </value>
        public virtual Campus Campus { get; set; }

        /// <summary>
        /// Gets the length.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public virtual TimeSpan Length
        {
            get
            {
                if ( StartTime.HasValue && StopTime.HasValue )
                    return StopTime.Value.Subtract( StartTime.Value );
                else
                    return new TimeSpan( 0 );
            }
        }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class RecordingConfiguration : EntityTypeConfiguration<Recording>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandCenterCompetencyConfiguration"/> class.
        /// </summary>
        public RecordingConfiguration()
        {
            this.HasOptional( r => r.Campus ).WithMany().HasForeignKey( r => r.CampusId ).WillCascadeOnDelete( false );
        }
    }
}
