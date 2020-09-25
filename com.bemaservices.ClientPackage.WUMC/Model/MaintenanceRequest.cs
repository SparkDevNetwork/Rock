using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;
using Rock.Model;
namespace com.bemaservices.ClientPackage.WUMC.Model
{
    [Table( "_com_bemaservices_ClientPackage_WUMC_MaintenanceRequest" )]
    [DataContract]
    public class MaintenanceRequest : Rock.Data.Model<MaintenanceRequest>, Rock.Data.IRockEntity
    {

        #region Entity Properties

        [Required]
        [DataMember]
        public int RequestorPersonAliasId { get; set; }

        [DataMember]
        public int? AssignedPersonAliasId { get; set; }

        [DataMember]
        public int? AssignedSupervisorPersonAliasId { get; set; }

        [DataMember]
        public int? LocationId { get; set; }

        [DataMember]
        public int? StatusId { get; set; }

        #endregion

        #region Virtual Properties

        [LavaInclude]
        public virtual Rock.Model.PersonAlias RequestorPersonAlias { get; set; }

        [LavaInclude]
        public virtual Rock.Model.PersonAlias AssignedPersonAlias { get; set; }

        [LavaInclude]
        public virtual Rock.Model.PersonAlias AssignedSupervisorPersonAlias { get; set; }

        [LavaInclude]
        public virtual Rock.Model.DefinedValue Status { get; set; }

        [LavaInclude]
        public virtual Rock.Model.Location Location { get; set; }

        #endregion

    }

    #region Entity Configuration


    public partial class MaintenanceRequestConfiguration : EntityTypeConfiguration<MaintenanceRequest>
    {
        public MaintenanceRequestConfiguration()
        {
            this.HasRequired( p => p.RequestorPersonAlias ).WithMany().HasForeignKey( p => p.RequestorPersonAliasId ).WillCascadeOnDelete( true );
            this.HasOptional( p => p.AssignedPersonAlias ).WithMany().HasForeignKey( p => p.AssignedPersonAliasId ).WillCascadeOnDelete( true );
            this.HasOptional( p => p.AssignedSupervisorPersonAlias ).WithMany().HasForeignKey( p => p.AssignedSupervisorPersonAliasId ).WillCascadeOnDelete( true );
            this.HasOptional( p => p.Status ).WithMany().HasForeignKey( p => p.StatusId ).WillCascadeOnDelete( true );
            this.HasOptional( p => p.Location ).WithMany().HasForeignKey( p => p.LocationId ).WillCascadeOnDelete( true );

            // IMPORTANT!!
            this.HasEntitySetName( "MaintenanceRequest" );
        }
    }

    #endregion

}