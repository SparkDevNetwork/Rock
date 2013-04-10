using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace com.mychurch.MyFirstRockLibrary.Model
{
    // Rock requires that custom tables follow this naming convention
    [Table( "_com_mychurch_PotluckDish" )]
    public class PotluckDish : Rock.Data.Model<PotluckDish>
    {
        #region Entity Properties

        // This PotluckDish is associated with a PotluckDinner.  This is the Foreign Key column to it
        public int PotluckDinnerId { get; set; }

        [Required]
        [MaxLength( 100 )]
        public string Name { get; set; }

        public string Instructions { get; set; }

        #endregion

        #region Virtual Properties

        // any reference tables go here

        // This PotluckDish is associated with a PotluckDinner.  This is the Class reference to it
        public virtual PotluckDinner PotluckDinner { get; set; }

        #endregion
    }

    // The Context will use this so that EntityFramework knows how the tables are related
    public class PotluckDishConfiguration : EntityTypeConfiguration<PotluckDish>
    {
        public PotluckDishConfiguration()
        {
            // PotluckDinner has a list of PotluckDish records, and PotluckDish belongs to a PotluckDinner.  
            // If the PotluckDinner is deleted, automatically delete the associated PotluckDish records
            this.HasRequired( p => p.PotluckDinner ).WithMany( p => p.PotluckDishes )
                .HasForeignKey( p => p.PotluckDinnerId ).WillCascadeOnDelete( true );
        }
    }
}
