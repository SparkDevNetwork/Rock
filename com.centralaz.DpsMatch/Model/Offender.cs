

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;
using com.centralaz.DpsMatch.Data;
using Rock;
using Rock.Data;
using Rock.Model;
namespace com.centralaz.DpsMatch.Model
{
    /// <summary>
    /// A Offender
    /// </summary>
    [Table( "_com_centralaz_DpsMatch_Offender" )]
    [DataContract]
    public class Offender : Rock.Data.Model<Offender>
    {

        #region Entity Properties

        /// <summary>
        /// The KeyString, the unique identifier. LastName+FirstName+Race+Sex+Hair+Eyes+PostalCode
        /// </summary>
        [DataMember]
        public String KeyString { get; set; }

        /// <summary>
        /// The last name of the offender
        /// </summary>
        [DataMember]
        public String LastName { get; set; }

        /// <summary>
        /// The first name of the offender
        /// </summary>
        [DataMember]
        public String FirstName { get; set; }

        /// <summary>
        /// The middle initial of the offender
        /// </summary>
        [DataMember]
        public Char? MiddleInitial { get; set; }

        /// <summary>
        /// The age of the offender
        /// </summary>
        [DataMember]
        public int? Age { get; set; }

        /// <summary>
        /// The height of the offender
        /// </summary>
        [DataMember]
        public int? Height { get; set; }

        /// <summary>
        /// The weight of the offender
        /// </summary>
        [DataMember]
        public int? Weight { get; set; }

        /// <summary>
        /// The race of the offender
        /// </summary>
        [DataMember]
        public String Race { get; set; }

        /// <summary>
        /// The sex of the offender
        /// </summary>
        [DataMember]
        public String Sex { get; set; }

        /// <summary>
        /// The hair of the offender
        /// </summary>
        [DataMember]
        public String Hair { get; set; }

        /// <summary>
        /// The eye colour of the offender
        /// </summary>
        [DataMember]
        public String Eyes { get; set; }

        /// <summary>
        /// The street address of the offender
        /// </summary>
        [DataMember]
        public String ResidentialAddress { get; set; }

        /// <summary>
        /// The city of the offender
        /// </summary>
        [DataMember]
        public String ResidentialCity { get; set; }

        /// <summary>
        /// The state of the offender
        /// </summary>
        [DataMember]
        public String ResidentialState { get; set; }

        /// <summary>
        /// The zip code of the offender
        /// </summary>
        [DataMember]
        public int? ResidentialZip { get; set; }

        /// <summary>
        /// The date the offender's address was verified
        /// </summary>
        [DataMember]
        public DateTime? VerificationDate { get; set; }

        /// <summary>
        /// The offender's offense
        /// </summary>
        [DataMember]
        public String Offense { get; set; }

        /// <summary>
        /// The offender's offense level
        /// </summary>
        [DataMember]
        public int? OffenseLevel { get; set; }

        /// <summary>
        /// Whether the offender is an absconder
        /// </summary>
        [DataMember]
        public bool? Absconder { get; set; }

        /// <summary>
        /// The convicting jurisdiction
        /// </summary>
        [DataMember]
        public String ConvictingJurisdiction { get; set; }

        /// <summary>
        /// Whether the address is unverified
        /// </summary>
        [DataMember]
        public bool? Unverified { get; set; }

        #endregion

    }

    #region Entity Configuration


    public partial class OffenderConfiguration : EntityTypeConfiguration<Offender>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OffenderConfiguration"/> class.
        /// </summary>
        public OffenderConfiguration()
        {
        }
    }

    #endregion

}
