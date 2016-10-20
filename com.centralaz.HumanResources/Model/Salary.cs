using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Model;
using System;

namespace com.centralaz.HumanResources.Model
{
    /// <summary>
    /// A Room Reservation
    /// </summary>
    [Table( "_com_centralaz_HumanResources_Salary" )]
    [DataContract]
    public class Salary : Rock.Data.Model<Salary>, Rock.Data.IRockEntity
    {

        #region Entity Properties
        [Required]
        [DataMember( IsRequired = true )]
        public int PersonAliasId { get; set; }

        [Required]
        [DataMember( IsRequired = true )]
        public double Amount { get; set; }

        [DataMember]
        public bool IsSalariedEmployee { get; set; }

        [DataMember]
        public double HousingAllowance { get; set; }

        [DataMember]
        public double FuelAllowance { get; set; }

        [DataMember]
        public double PhoneAllowance { get; set; }

        [Required]
        [DataMember( IsRequired = true )]
        public DateTime EffectiveDate { get; set; }

        [DataMember]
        public DateTime? ReviewedDate { get; set; }

        [Required]
        [DataMember( IsRequired = true )]
        public bool IsActive { get; set; }
        #endregion

        #region Virtual Properties

        public virtual PersonAlias PersonAlias { get; set; }

        #endregion

    }

    #region Entity Configuration


    public partial class SalaryConfiguration : EntityTypeConfiguration<Salary>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SalaryConfiguration"/> class.
        /// </summary>
        public SalaryConfiguration()
        {
            this.HasRequired( r => r.PersonAlias ).WithMany().HasForeignKey( r => r.PersonAliasId ).WillCascadeOnDelete( false );
        }
    }

    #endregion


}