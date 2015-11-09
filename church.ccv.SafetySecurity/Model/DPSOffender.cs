using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;

namespace church.ccv.SafetySecurity.Model
{
    /// <summary>
    /// 
    /// </summary>
    [Table( "_church_ccv_SafetySecurity_DPSOffender" )]
    [DataContract]
    public partial class DPSOffender : Rock.Data.Entity<DPSOffender>, Rock.Data.IRockEntity
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        /// <value>
        /// The last name.
        /// </value>
        [DataMember]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        /// <value>
        /// The first name.
        /// </value>
        [DataMember]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the middle initial.
        /// </summary>
        /// <value>
        /// The middle initial.
        /// </value>
        [DataMember]
        public string MiddleInitial { get; set; }

        /// <summary>
        /// Gets or sets the age.
        /// </summary>
        /// <value>
        /// The age.
        /// </value>
        [DataMember]
        public int? Age { get; set; }

        /// <summary>
        /// Gets or sets the height as a 1 to 3 digit number
        /// Examples: 
        ///     6 means 6ft
        ///     51 means 5ft 1 inches
        ///     508 means 5ft 8inches
        /// </summary>
        /// <value>
        /// The height.
        /// </value>
        [DataMember]
        public int? Height { get; set; }

        /// <summary>
        /// Gets or sets the weight in pounds
        /// </summary>
        /// <value>
        /// The weight.
        /// </value>
        [DataMember]
        public int? Weight { get; set; }

        /// <summary>
        /// Gets or sets the race as a single character such as W, B, I, etc. Might be blank
        /// </summary>
        /// <value>
        /// The race.
        /// </value>
        [DataMember]
        public string Race { get; set; }

        /// <summary>
        /// Gets or sets the gender as M or F
        /// </summary>
        /// <value>
        /// The gender.
        /// </value>
        [DataMember]
        public string Gender { get; set; }

        /// <summary>
        /// Gets or sets the hair color (BRO,BLK,GRY, etc)
        /// </summary>
        /// <value>
        /// The hair.
        /// </value>
        [DataMember]
        public string Hair { get; set; }

        /// <summary>
        /// Gets or sets the eye color (BLU,GRN,HAZ, etc)
        /// </summary>
        /// <value>
        /// The hair.
        /// </value>
        [DataMember]
        public string Eyes { get; set; }

        /// <summary>
        /// Gets or sets DPS's record of the offender's residential street address
        /// </summary>
        /// <value>
        /// The resource address.
        /// </value>
        [DataMember]
        public string ResAddress { get; set; }

        /// /// Gets or sets DPS's record of the offender's residential address (City)
        /// Gets or sets the resource city.
        /// </summary>
        /// <value>
        /// The resource city.
        /// </value>
        [DataMember]
        public string ResCity { get; set; }

        /// Gets or sets DPS's record of the offender's residential address (Zip code)
        /// Gets or sets the resource city.
        /// </summary>
        /// <value>
        /// The resource city.
        /// </value>
        [DataMember]
        public string ResZip { get; set; }

        /// <summary>
        /// Gets or sets the offense.
        /// </summary>
        /// <value>
        /// The offense.
        /// </value>
        [DataMember]
        public string Offense { get; set; }

        /// <summary>
        /// Gets or sets the date convicted.
        /// </summary>
        /// <value>
        /// The date convicted.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime DateConvicted { get; set; }

        /// <summary>
        /// Gets or sets the 2 letter State (AZ, etc) of the conviction.
        /// </summary>
        /// <value>
        /// The state of the conviction.
        /// </value>
        [DataMember]
        public string ConvictionState { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the offender is an absconder
        /// </summary>
        /// <value>
        ///   <c>true</c> if absconder; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool Absconder { get; set; }

        /// <summary>
        /// The PersonAliasId that the DPS record is matched to
        /// </summary>
        /// <value>
        /// The person alias identifier.
        /// </value>
        [DataMember]
        public int? PersonAliasId { get; set; }

        /// <summary>
        /// The LocationId that corresponds to the Address that DPS has for the person (ResAddress, ResCity, ResZip)
        /// </summary>
        /// <value>
        /// The DPS location identifier.
        /// </value>
        [DataMember]
        public int? DpsLocationId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// The PersonAlias that the DPS record is matched to
        /// </summary>
        /// <value>
        /// The person alias.
        /// </value>
        [DataMember]
        public virtual Rock.Model.PersonAlias PersonAlias { get; set; }

        /// <summary>
        /// The Location that corresponds to the Address that DPS has for the person (ResAddress, ResCity, ResZip)
        /// </summary>
        /// <value>
        /// The DPS location.
        /// </value>
        [DataMember]
        public virtual Rock.Model.Location DpsLocation { get; set; }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public class DPSOffenderConfiguration : EntityTypeConfiguration<DPSOffender>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TimeCardDayConfiguration"/> class.
        /// </summary>
        public DPSOffenderConfiguration()
        {
            this.HasOptional( a => a.PersonAlias ).WithMany().HasForeignKey( a => a.PersonAliasId ).WillCascadeOnDelete( true );
            this.HasOptional( a => a.DpsLocation ).WithMany().HasForeignKey( a => a.DpsLocationId ).WillCascadeOnDelete( true );
        }
    }
}
