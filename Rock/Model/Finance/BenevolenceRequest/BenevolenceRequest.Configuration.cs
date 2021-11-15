using System.Data.Entity.ModelConfiguration;
namespace Rock.Model
{
    #region Entity Configuration

    /// <summary>
    /// BenevolenceRequest Configuration class.
    /// </summary>
    public partial class BenevolenceRequestConfiguration : EntityTypeConfiguration<BenevolenceRequest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BenevolenceRequestConfiguration" /> class.
        /// </summary>
        public BenevolenceRequestConfiguration()
        {
            

            this.HasOptional( p => p.RequestedByPersonAlias ).WithMany().HasForeignKey( p => p.RequestedByPersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.CaseWorkerPersonAlias ).WithMany().HasForeignKey( p => p.CaseWorkerPersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.ConnectionStatusValue ).WithMany().HasForeignKey( p => p.ConnectionStatusValueId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.Campus ).WithMany().HasForeignKey( p => p.CampusId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.Location ).WithMany().HasForeignKey( p => p.LocationId ).WillCascadeOnDelete( false );

            this.HasRequired( p => p.BenevolenceType ).WithMany().HasForeignKey( p => p.BenevolenceTypeId ).WillCascadeOnDelete( false );
            this.HasRequired( p => p.RequestStatusValue ).WithMany().HasForeignKey( p => p.RequestStatusValueId ).WillCascadeOnDelete( false );
            // NOTE: When creating a migration for this, don't create the actual FK's in the database for this just in case there are outlier OccurrenceDates that aren't in the AnalyticsSourceDate table
            // and so that the AnalyticsSourceDate can be rebuilt from scratch as needed
            this.HasRequired( r => r.RequestSourceDate ).WithMany().HasForeignKey( r => r.RequestDateKey ).WillCascadeOnDelete( false );
        }
    }

    #endregion Entity Configuration
}
