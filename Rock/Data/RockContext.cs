//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace Rock.Data
{
	/// <summary>
	/// Entity Framework Context
	/// </summary>
    internal partial class RockContext : DbContext
    {
        /// <summary>
        /// Gets or sets the Auths.
        /// </summary>
        /// <value>
        /// the Auths.
        /// </value>
        public DbSet<Rock.CMS.Auth> Auths { get; set; }

        /// <summary>
        /// Gets or sets the Blocks.
        /// </summary>
        /// <value>
        /// the Blocks.
        /// </value>
        public DbSet<Rock.CMS.Block> Blocks { get; set; }

        /// <summary>
        /// Gets or sets the Block Instances.
        /// </summary>
        /// <value>
        /// the Block Instances.
        /// </value>
        public DbSet<Rock.CMS.BlockInstance> BlockInstances { get; set; }

        /// <summary>
        /// Gets or sets the Files.
        /// </summary>
        /// <value>
        /// the Files.
        /// </value>
        public DbSet<Rock.CMS.File> Files { get; set; }

        /// <summary>
        /// Gets or sets the Html Contents.
        /// </summary>
        /// <value>
        /// the Html Contents.
        /// </value>
        public DbSet<Rock.CMS.HtmlContent> HtmlContents { get; set; }

        /// <summary>
        /// Gets or sets the Pages.
        /// </summary>
        /// <value>
        /// the Pages.
        /// </value>
        public DbSet<Rock.CMS.Page> Pages { get; set; }

        /// <summary>
        /// Gets or sets the Page Routes.
        /// </summary>
        /// <value>
        /// the Page Routes.
        /// </value>
        public DbSet<Rock.CMS.PageRoute> PageRoutes { get; set; }

        /// <summary>
        /// Gets or sets the Sites.
        /// </summary>
        /// <value>
        /// the Sites.
        /// </value>
        public DbSet<Rock.CMS.Site> Sites { get; set; }

        /// <summary>
        /// Gets or sets the Site Domains.
        /// </summary>
        /// <value>
        /// the Site Domains.
        /// </value>
        public DbSet<Rock.CMS.SiteDomain> SiteDomains { get; set; }

        /// <summary>
        /// Gets or sets the Users.
        /// </summary>
        /// <value>
        /// the Users.
        /// </value>
        public DbSet<Rock.CMS.User> Users { get; set; }

        /// <summary>
        /// Gets or sets the Attributes.
        /// </summary>
        /// <value>
        /// the Attributes.
        /// </value>
        public DbSet<Rock.Core.Attribute> Attributes { get; set; }

        /// <summary>
        /// Gets or sets the Attribute Qualifiers.
        /// </summary>
        /// <value>
        /// the Attribute Qualifiers.
        /// </value>
        public DbSet<Rock.Core.AttributeQualifier> AttributeQualifiers { get; set; }

        /// <summary>
        /// Gets or sets the Attribute Values.
        /// </summary>
        /// <value>
        /// the Attribute Values.
        /// </value>
        public DbSet<Rock.Core.AttributeValue> AttributeValues { get; set; }

        /// <summary>
        /// Gets or sets the Defined Types.
        /// </summary>
        /// <value>
        /// the Defined Types.
        /// </value>
        public DbSet<Rock.Core.DefinedType> DefinedTypes { get; set; }

        /// <summary>
        /// Gets or sets the Defined Values.
        /// </summary>
        /// <value>
        /// the Defined Values.
        /// </value>
        public DbSet<Rock.Core.DefinedValue> DefinedValues { get; set; }

        /// <summary>
        /// Gets or sets the Entity Changes.
        /// </summary>
        /// <value>
        /// the Entity Changes.
        /// </value>
        public DbSet<Rock.Core.EntityChange> EntityChanges { get; set; }

        /// <summary>
        /// Gets or sets the Exception Logs.
        /// </summary>
        /// <value>
        /// the Exception Logs.
        /// </value>
        public DbSet<Rock.Core.ExceptionLog> ExceptionLogs { get; set; }

        /// <summary>
        /// Gets or sets the Field Types.
        /// </summary>
        /// <value>
        /// the Field Types.
        /// </value>
        public DbSet<Rock.Core.FieldType> FieldTypes { get; set; }

        /// <summary>
        /// Gets or sets the Service Logs.
        /// </summary>
        /// <value>
        /// the Service Logs.
        /// </value>
        public DbSet<Rock.Core.ServiceLog> ServiceLogs { get; set; }

        /// <summary>
        /// Gets or sets the Addresses.
        /// </summary>
        /// <value>
        /// the Addresses.
        /// </value>
        public DbSet<Rock.CRM.Address> Addresses { get; set; }

        /// <summary>
        /// Gets or sets the Campuses.
        /// </summary>
        /// <value>
        /// the Campuses.
        /// </value>
        public DbSet<Rock.CRM.Campus> Campuses { get; set; }

        /// <summary>
        /// Gets or sets the Email Templates.
        /// </summary>
        /// <value>
        /// the Email Templates.
        /// </value>
        public DbSet<Rock.CRM.EmailTemplate> EmailTemplates { get; set; }

        /// <summary>
        /// Gets or sets the People.
        /// </summary>
        /// <value>
        /// the People.
        /// </value>
        public DbSet<Rock.CRM.Person> People { get; set; }

        /// <summary>
        /// Gets or sets the Person Trails.
        /// </summary>
        /// <value>
        /// the Person Trails.
        /// </value>
        public DbSet<Rock.CRM.PersonMerged> PersonMerges { get; set; }

        /// <summary>
        /// Gets or sets the Person Vieweds.
        /// </summary>
        /// <value>
        /// the Person Vieweds.
        /// </value>
        public DbSet<Rock.CRM.PersonViewed> PersonVieweds { get; set; }

        /// <summary>
        /// Gets or sets the Phone Numbers.
        /// </summary>
        /// <value>
        /// the Phone Numbers.
        /// </value>
        public DbSet<Rock.CRM.PhoneNumber> PhoneNumbers { get; set; }

        /// <summary>
        /// Gets or sets the Groups.
        /// </summary>
        /// <value>
        /// the Groups.
        /// </value>
        public DbSet<Rock.Groups.Group> Groups { get; set; }

        /// <summary>
        /// Gets or sets the Group Roles.
        /// </summary>
        /// <value>
        /// the Group Roles.
        /// </value>
        public DbSet<Rock.Groups.GroupRole> GroupRoles { get; set; }

        /// <summary>
        /// Gets or sets the Group Types.
        /// </summary>
        /// <value>
        /// the Group Types.
        /// </value>
        public DbSet<Rock.Groups.GroupType> GroupTypes { get; set; }

        /// <summary>
        /// Gets or sets the Members.
        /// </summary>
        /// <value>
        /// the Members.
        /// </value>
        public DbSet<Rock.Groups.Member> Members { get; set; }

        /// <summary>
        /// Gets or sets the Jobs.
        /// </summary>
        /// <value>
        /// the Jobs.
        /// </value>
        public DbSet<Rock.Util.Job> Jobs { get; set; }

        /// <summary>
        /// Gets or sets the batches.
        /// </summary>
        /// <value>
        /// The batches.
        /// </value>
        public DbSet<Rock.Financial.Batch> Batches { get; set; }

        /// <summary>
        /// Gets or sets the fund.
        /// </summary>
        /// <value>
        /// The fund.
        /// </value>
        public DbSet<Rock.Financial.Fund> Fund { get; set; }

        /// <summary>
        /// Gets or sets the pledges.
        /// </summary>
        /// <value>
        /// The pledges.
        /// </value>
        public DbSet<Rock.Financial.Pledge> Pledges { get; set; }

        /// <summary>
        /// Gets or sets the transactions.
        /// </summary>
        /// <value>
        /// The transactions.
        /// </value>
        public DbSet<Rock.Financial.Transaction> Transactions { get; set; }

        /// <summary>
        /// Gets or sets the transaction details.
        /// </summary>
        /// <value>
        /// The transaction details.
        /// </value>
        public DbSet<Rock.Financial.TransactionDetail> TransactionDetails { get; set; }

        /// <summary>
        /// Gets or sets the person account lookups.
        /// </summary>
        /// <value>
        /// The person account lookups.
        /// </value>
        public DbSet<Rock.Financial.PersonAccountLookup> PersonAccountLookups { get; set; }

        /// <summary>
        /// Gets or sets the transaction funds.
        /// </summary>
        /// <value>
        /// The transaction funds.
        /// </value>
        public DbSet<Rock.Financial.TransactionFund> TransactionFunds { get; set; }


        /// <summary>
        /// This method is called when the context has been initialized, but
        /// before the model has been locked down and used to initialize the context. 
        /// </summary>
        /// <param name="modelBuilder">The builder that defines the model for the context being created.</param>
        protected override void OnModelCreating( DbModelBuilder modelBuilder )
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            modelBuilder.Configurations.Add( new Rock.CMS.AuthConfiguration() );
            modelBuilder.Configurations.Add( new Rock.CMS.BlockConfiguration() );
            modelBuilder.Configurations.Add( new Rock.CMS.BlockInstanceConfiguration() );
            modelBuilder.Configurations.Add( new Rock.CMS.FileConfiguration() );
            modelBuilder.Configurations.Add( new Rock.CMS.HtmlContentConfiguration() );
            modelBuilder.Configurations.Add( new Rock.CMS.PageConfiguration() );
            modelBuilder.Configurations.Add( new Rock.CMS.PageRouteConfiguration() );
            modelBuilder.Configurations.Add( new Rock.CMS.SiteConfiguration() );
            modelBuilder.Configurations.Add( new Rock.CMS.SiteDomainConfiguration() );
            modelBuilder.Configurations.Add( new Rock.CMS.UserConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Core.AttributeConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Core.AttributeQualifierConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Core.AttributeValueConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Core.DefinedTypeConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Core.DefinedValueConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Core.EntityChangeConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Core.ExceptionLogConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Core.FieldTypeConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Core.ServiceLogConfiguration() );
            modelBuilder.Configurations.Add( new Rock.CRM.AddressConfiguration() );
            modelBuilder.Configurations.Add( new Rock.CRM.CampusConfiguration() );
            modelBuilder.Configurations.Add( new Rock.CRM.EmailTemplateConfiguration() );
            modelBuilder.Configurations.Add( new Rock.CRM.PersonConfiguration() );
            modelBuilder.Configurations.Add( new Rock.CRM.PersonMergedConfiguration() );
            modelBuilder.Configurations.Add( new Rock.CRM.PersonViewedConfiguration() );
            modelBuilder.Configurations.Add( new Rock.CRM.PhoneNumberConfiguration() );
            modelBuilder.Configurations.Add(new Rock.Financial.BatchConfiguration());
            modelBuilder.Configurations.Add(new Rock.Financial.FundConfiguration());
            modelBuilder.Configurations.Add(new Rock.Financial.PledgeConfiguration());
            modelBuilder.Configurations.Add(new Rock.Financial.TransactionConfiguration());
            modelBuilder.Configurations.Add(new Rock.Financial.TransactionDetailConfiguration());
            modelBuilder.Configurations.Add(new Rock.Financial.PersonAccountLookupConfiguration());
            modelBuilder.Configurations.Add(new Rock.Financial.TransactionFundConfiguration());
            modelBuilder.Configurations.Add( new Rock.Groups.GroupConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Groups.GroupRoleConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Groups.GroupTypeConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Groups.MemberConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Util.JobConfiguration() );
		}
    }
}

