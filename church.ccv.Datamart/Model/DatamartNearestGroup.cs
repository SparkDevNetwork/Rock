using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using church.ccv.Datamart.Data;

namespace church.ccv.Datamart.Model
{
    /// <summary>
    /// 
    /// </summary>
    [Table( "_church_ccv_Datamart_NearestGroup" )]
    [DataContract]
    public partial class DatamartNearestGroup : Rock.Data.Entity<DatamartNearestGroup>
    {
        /// <summary>
        /// Gets or sets the family location identifier.
        /// </summary>
        /// <value>
        /// The family location identifier.
        /// </value>
        [DataMember]
        [DatabaseGenerated( DatabaseGeneratedOption.None )]
        public int FamilyLocationId { get; set; }

        /// <summary>
        /// Gets or sets the group location identifier.
        /// </summary>
        /// <value>
        /// The group location identifier.
        /// </value>
        [DataMember]
        public int GroupLocationId { get; set; }

        /// <summary>
        /// Gets or sets the distance.
        /// </summary>
        /// <value>
        /// The distance.
        /// </value>
        [DataMember]
        public double Distance { get; set; }
    }
}
