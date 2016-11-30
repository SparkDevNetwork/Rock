using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using church.ccv.Hr.Data;
using Rock;

namespace church.ccv.Hr.Model
{
    /// <summary>
    /// 
    /// </summary>
    [Table( "_church_ccv_Hr_TimeCardHistory" )]
    [DataContract]
    public class TimeCardHistory : Model<TimeCardHistory>, Rock.Data.IRockEntity
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
