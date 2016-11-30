using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;

using church.ccv.SampleProject.Data;

using Rock.Data;
using Rock.Model;

namespace church.ccv.SampleProject.Model
{
    /// <summary>
    /// A Referral Agency
    /// </summary>
    [Table( "_church_ccv_SampleProject_ReferralAgency" )]
    [DataContract]
    public class ReferralAgency : NamedModel<ReferralAgency>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the name of the contact at agency.
        /// </summary>
        /// <value>
        /// The name of the contact.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string ContactName { get; set; }

        /// <summary>
        /// Gets or sets the agency's phone number.
        /// </summary>
        /// <value>
        /// The phone number.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the agency's website.
        /// </summary>
        /// <value>
        /// The website.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string Website { get; set; }

        /// <summary>
        /// Gets or sets the campus identifier.
        /// </summary>
        /// <value>
        /// The campus identifier.
        /// </value>
        [DataMember]
        public int? CampusId { get; set; }

        /// <summary>
        /// Gets or sets the agency type value identifier.
        /// </summary>
        /// <value>
        /// The agency type value identifier.
        /// </value>
        [DataMember]
        public int? AgencyTypeValueId { get; set; }

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
        /// Gets or sets the agency type value.
        /// </summary>
        /// <value>
        /// The agency type value.
        /// </value>
        [DataMember]
        public virtual DefinedValue AgencyTypeValue { get; set; }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// 
    /// </summary>
    public partial class ReferralAgencyConfiguration : EntityTypeConfiguration<ReferralAgency>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReferralAgencyConfiguration"/> class.
        /// </summary>
        public ReferralAgencyConfiguration()
        {
            this.HasOptional( r => r.Campus ).WithMany().HasForeignKey( r => r.CampusId).WillCascadeOnDelete( false );
            this.HasOptional( r => r.AgencyTypeValue ).WithMany().HasForeignKey( p => p.AgencyTypeValueId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

}
