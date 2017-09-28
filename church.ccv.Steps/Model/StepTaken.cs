using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using Rock;
using Rock.Data;
using Rock.Model;

namespace church.ccv.Steps.Model
{
    /// <summary>
    /// 
    /// </summary>
    [Table( "_church_ccv_Steps_StepTaken" )]
    [DataContract]
    public class StepTaken : Model<StepTaken>, IRockEntity
    {
        #region Entity Properties
        /// <summary>
        /// Gets or sets the measure identifier.
        /// </summary>
        /// <value>
        /// The measure identifier.
        /// </value>
        [Required]
        [DataMember (IsRequired = true)]
        public int StepMeasureId { get; set; }

        /// <summary>
        /// Gets or sets the campus identifier.
        /// </summary>
        /// <value>
        /// The campus identifier.
        /// </value>
        [DataMember]
        public int? CampusId { get; set; }

        /// <summary>
        /// Gets or sets the pastor person alias identifier.
        /// </summary>
        /// <value>
        /// The pastor person alias identifier.
        /// </value>
        [DataMember]
        public int PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the sunday date.
        /// </summary>
        /// <value>
        /// The sunday date.
        /// </value>
        [DataMember]
        [DatabaseGenerated( DatabaseGeneratedOption.Computed )]
        [Column( TypeName = "Date" )]
        public DateTime? SundayDate{ get; set; }

        /// <summary>
        /// Gets or sets the date taken.
        /// </summary>
        /// <value>
        /// The date taken.
        /// </value>
        public DateTime DateTaken { get; set; }

        /// <summary>
        /// Gets or sets the note.
        /// </summary>
        /// <value>
        /// The note.
        /// </value>
        [DataMember]
        public string Note { get; set; }
        #endregion

        #region Virtual Properties


        /// <summary>
        /// Gets or sets the measure.
        /// </summary>
        /// <value>
        /// The measure.
        /// </value>
        public virtual StepMeasure StepMeasure { get; set; }

        /// <summary>
        /// Gets or sets the campus.
        /// </summary>
        /// <value>
        /// The campus.
        /// </value>
        public virtual Campus Campus { get; set; }

        /// <summary>
        /// Gets or sets the paster person alias.
        /// </summary>
        /// <value>
        /// The paster person alias.
        /// </value>
        public virtual PersonAlias PersonAlias { get; set; }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public class StepTakenConfiguration : EntityTypeConfiguration<StepTaken>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StepTakenConfiguration"/> class.
        /// </summary>
        public StepTakenConfiguration()
        {
            this.HasEntitySetName( "StepTaken" );

            this.HasRequired( m => m.StepMeasure ).WithMany( c => c.StepsTaken ).HasForeignKey( m => m.StepMeasureId ).WillCascadeOnDelete( true );
            this.HasOptional( m => m.Campus ).WithMany().HasForeignKey( m => m.CampusId ).WillCascadeOnDelete( false );
            this.HasRequired( m => m.PersonAlias ).WithMany().HasForeignKey( m => m.PersonAliasId ).WillCascadeOnDelete( false );
        }
    }
}
