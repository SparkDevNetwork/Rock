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

namespace com.bemaservices.PastoralCare.Model
{
    [RockDomain( "BEMA Services > Care" )]
    [Table( "_com_bemaservices_PastoralCare_CareContact" )]
    [DataContract]
    public partial class CareContact : Rock.Data.Model<CareContact>, Rock.Data.IRockEntity
    {
        #region Entity Properties    

        [Required]
        [DataMember( IsRequired = true )]
        public int CareItemId { get; set; }

        [Required]
        [DataMember]
        public int? ContactorPersonAliasId { get; set; }

        [Required]
        [DataMember]
        public DateTime ContactDateTime { get; set; }

        [DataMember]
        public string Description { get; set; }

        #endregion

        #region Virtual Properties

        [DataMember]
        public virtual CareItem CareItem { get; set; }

        [LavaInclude]
        public virtual PersonAlias ContactorPersonAlias { get; set; }

        public override Rock.Security.ISecured ParentAuthority
        {
            get
            {
                return this.CareItem != null ? this.CareItem : base.ParentAuthority;
            }
        }

        /// <summary>
        /// A dictionary of actions that this class supports and the description of each.
        /// </summary>
        public override Dictionary<string, string> SupportedActions
        {
            get
            {
                if ( _supportedActions == null )
                {
                    _supportedActions = new Dictionary<string, string>();
                    _supportedActions.Add( Authorization.VIEW, "The roles and/or users that have access to view." );
                    _supportedActions.Add( Authorization.EDIT, "The roles and/or users that have access to edit." );
                    _supportedActions.Add( Authorization.ADMINISTRATE, "The roles and/or users that have access to administrate." );
                }

                return _supportedActions;
            }
        }
        private Dictionary<string, string> _supportedActions;

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// CareContact Configuration class.
    /// </summary>
    public partial class CareContactConfiguration : EntityTypeConfiguration<CareContact>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CareContactConfiguration"/> class.
        /// </summary>
        public CareContactConfiguration()
        {
            this.HasRequired( p => p.CareItem ).WithMany( p => p.CareContacts ).HasForeignKey( p => p.CareItemId ).WillCascadeOnDelete( true );

            // IMPORTANT!!
            this.HasEntitySetName( "CareContact" );
        }
    }

    #endregion
}
