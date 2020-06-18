using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Rock.Data;
using Rock.Web.Cache;
using Rock.UniversalSearch;
using Rock.UniversalSearch.IndexModels;
using Rock.Security;
using Rock.Model;

namespace com.bemaservices.MailChimp.Model
{
    [RockDomain( "BEMA Services > Mail Chimp" )]
    [Table( "_com_bemaservices_MailChimp_Member" )]
    [DataContract]
    public partial class MailChimpMember : Rock.Data.Model<MailChimpMember>, Rock.Data.IRockEntity
    {
        #region Entity Properties    

        [Required]
        [DataMember( IsRequired = true )]
        public string MemberId { get; set; }

        [Required]
        [DataMember]
        public string ListId { get; set; }

        [DataMember]
        public DateTime LastUpdated { get; set; }

        [Required]
        [DataMember]
        public string Status { get; set; }

        [Required]
        [DataMember]
        public string Email { get; set; }

        #endregion

        #region Virtual Properties

        [LavaInclude]
        public virtual PersonAlias PersonAlias { get; set; }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// CareContact Configuration class.
    /// </summary>
    public partial class MailChimpMemberConfiguration : EntityTypeConfiguration<MailChimpMember>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CareContactConfiguration"/> class.
        /// </summary>
        public MailChimpMemberConfiguration()
        {
            // IMPORTANT!!
            this.HasEntitySetName( "MailChimpMember" );
        }
    }

    #endregion
}
