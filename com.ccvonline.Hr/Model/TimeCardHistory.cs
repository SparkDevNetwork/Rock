using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using com.ccvonline.Hr.Data;
using Rock;

namespace com.ccvonline.Hr.Model
{
    /// <summary>
    /// 
    /// </summary>
    [Table( "_com_ccvonline_Hr_TimeCardHistory" )]
    [DataContract]
    public class TimeCardHistory : Model<TimeCardHistory>
    {
        #region Entity Properties

        [Required]
        [DataMember]
        public int TimeCardId { get; set; }

        [Required]
        [DataMember]
        public DateTime HistoryDateTime { get; set; }

        [Required]
        [DataMember]
        public TimeCardStatus TimeCardStatus { get; set; }

        [Required]
        [DataMember]
        public int? StatusPersonAliasId { get; set; }

        [MaxLength( 200 )]
        [DataMember]
        public string Notes { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the time card.
        /// </summary>
        /// <value>
        /// The time card.
        /// </value>
        public virtual TimeCard TimeCard { get; set; }

        /// <summary>
        /// Gets or sets the status person alias.
        /// </summary>
        /// <value>
        /// The status person alias.
        /// </value>
        public virtual Rock.Model.PersonAlias StatusPersonAlias { get; set; }

        #endregion

        #region methods

        /// <summary>
        /// Gets the time card history status text.
        /// </summary>
        /// <param name="timeCardHistory">The time card history.</param>
        /// <returns></returns>
        public string GetStatusText()
        {
            string statusText = string.Empty;
            switch ( this.TimeCardStatus )
            {
                case TimeCardStatus.Approved:
                case TimeCardStatus.Exported:
                    statusText = string.Format( "{0} by {1}", this.TimeCardStatus.ConvertToString(), this.StatusPersonAlias );
                    break;
                case TimeCardStatus.Submitted:
                    statusText = string.Format( "{0} to {1}", this.TimeCardStatus.ConvertToString(), this.StatusPersonAlias );
                    break;
                default:
                    statusText = this.TimeCardStatus.ConvertToString();
                    break;
            }

            return statusText;
        }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public class TimeCardHistoryConfiguration : EntityTypeConfiguration<TimeCardHistory>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TimeCardDayConfiguration"/> class.
        /// </summary>
        public TimeCardHistoryConfiguration()
        {
            this.HasRequired( a => a.TimeCard ).WithMany( a => a.TimeCardHistories ).HasForeignKey( a => a.TimeCardId ).WillCascadeOnDelete( true );
            this.HasRequired( p => p.StatusPersonAlias ).WithMany().HasForeignKey( p => p.StatusPersonAliasId ).WillCascadeOnDelete( false );
        }
    }
}
