using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace com.mychurch.MyFirstRockLibrary.Model
{
    // Rock requires that custom tables follow this naming convention
    [Table( "_com_mychurch_PotluckDinner" )]
    public class PotluckDinner : Rock.Data.Model<PotluckDinner>
    {
        #region Entity Properties

        // additional columns

        [Required] 
        [MaxLength( 100 )]
        public string Name { get; set; }

        public DateTime StartDateTime { get; set; }
        
        public DateTime EndDateTime { get; set; }

        #endregion

        #region Virtual Properties

        // any reference tables go here

        public virtual List<PotluckDish> PotluckDishes { get; set; }

        #endregion
    }

    // The Context will use this so that EntityFramework knows how the tables are related
    public class PotluckDinnerConfiguration : EntityTypeConfiguration<PotluckDinner>
    {
        public PotluckDinnerConfiguration()
        {
            // PotluckDinner currently has no additional configuration, but it is a still good
            // idea to create a PotluckDinnerConfiguration.  This will end up making the 
            // Context class easier to manage
        }
    }
}
