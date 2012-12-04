//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Reflection;

namespace Rock.Data
{
    /// <summary>
    /// Entity Framework Context
    /// </summary>
    public partial class RockContext : DbContext
    {
        #region Cms

        /// <summary>
        /// Gets or sets the Auths.
        /// </summary>
        /// <value>
        /// the Auths.
        /// </value>
        public DbSet<Rock.Model.Auth> Auths { get; set; }

        /// <summary>
        /// Gets or sets the Block Types.
        /// </summary>
        /// <value>
        /// the Block Types.
        /// </value>
        public DbSet<Rock.Model.BlockType> BlockTypes { get; set; }

        /// <summary>
        /// Gets or sets the Blocks.
        /// </summary>
        /// <value>
        /// the Blocks.
        /// </value>
        public DbSet<Rock.Model.Block> Blocks { get; set; }

        /// <summary>
        /// Gets or sets the Files.
        /// </summary>
        /// <value>
        /// the Files.
        /// </value>
        public DbSet<Rock.Model.BinaryFile> Files { get; set; }

        /// <summary>
        /// Gets or sets the Html Contents.
        /// </summary>
        /// <value>
        /// the Html Contents.
        /// </value>
        public DbSet<Rock.Model.HtmlContent> HtmlContents { get; set; }

        /// <summary>
        /// Gets or sets the Pages.
        /// </summary>
        /// <value>
        /// the Pages.
        /// </value>
        public DbSet<Rock.Model.Page> Pages { get; set; }

        /// <summary>
        /// Gets or sets the Page Routes.
        /// </summary>
        /// <value>
        /// the Page Routes.
        /// </value>
        public DbSet<Rock.Model.PageRoute> PageRoutes { get; set; }

        /// <summary>
        /// Gets or sets the Sites.
        /// </summary>
        /// <value>
        /// the Sites.
        /// </value>
        public DbSet<Rock.Model.Site> Sites { get; set; }

        /// <summary>
        /// Gets or sets the Site Domains.
        /// </summary>
        /// <value>
        /// the Site Domains.
        /// </value>
        public DbSet<Rock.Model.SiteDomain> SiteDomains { get; set; }

        /// <summary>
        /// Gets or sets the Users.
        /// </summary>
        /// <value>
        /// the Users.
        /// </value>
        public DbSet<Rock.Model.UserLogin> Users { get; set; }

        /// <summary>
        /// Gets or sets the marketing campaigns.
        /// </summary>
        /// <value>
        /// The marketing campaigns.
        /// </value>
        public DbSet<Rock.Model.MarketingCampaign> MarketingCampaigns { get; set; }

        /// <summary>
        /// Gets or sets the marketing campaign ads.
        /// </summary>
        /// <value>
        /// The marketing campaign ads.
        /// </value>
        public DbSet<Rock.Model.MarketingCampaignAd> MarketingCampaignAds { get; set; }

        /// <summary>
        /// Gets or sets the marketing campaign ad types.
        /// </summary>
        /// <value>
        /// The marketing campaign ad types.
        /// </value>
        public DbSet<Rock.Model.MarketingCampaignAdType> MarketingCampaignAdTypes { get; set; }

        /// <summary>
        /// Gets or sets the marketing campaign audiences.
        /// </summary>
        /// <value>
        /// The marketing campaign audiences.
        /// </value>
        public DbSet<Rock.Model.MarketingCampaignAudience> MarketingCampaignAudiences { get; set; }

        /// <summary>
        /// Gets or sets the marketing campaign campuses.
        /// </summary>
        /// <value>
        /// The marketing campaign campuses.
        /// </value>
        public DbSet<Rock.Model.MarketingCampaignCampus> MarketingCampaignCampuses { get; set; }

        #endregion

        #region Core

        /// <summary>
        /// Gets or sets the Attributes.
        /// </summary>
        /// <value>
        /// the Attributes.
        /// </value>
        public DbSet<Rock.Model.Attribute> Attributes { get; set; }

        /// <summary>
        /// Gets or sets the Attribute Qualifiers.
        /// </summary>
        /// <value>
        /// the Attribute Qualifiers.
        /// </value>
        public DbSet<Rock.Model.AttributeQualifier> AttributeQualifiers { get; set; }

        /// <summary>
        /// Gets or sets the Attribute Values.
        /// </summary>
        /// <value>
        /// the Attribute Values.
        /// </value>
        public DbSet<Rock.Model.AttributeValue> AttributeValues { get; set; }

        /// <summary>
        /// Gets or sets the Attribute Values.
        /// </summary>
        /// <value>
        /// the Attribute Values.
        /// </value>
        public DbSet<Rock.Model.Audit> Audits { get; set; }

        /// <summary>
        /// Gets or sets the categories.
        /// </summary>
        /// <value>
        /// The categories.
        /// </value>
        public DbSet<Rock.Model.Category> Categories { get; set; }

        /// <summary>
        /// Gets or sets the Defined Types.
        /// </summary>
        /// <value>
        /// the Defined Types.
        /// </value>
        public DbSet<Rock.Model.DefinedType> DefinedTypes { get; set; }

        /// <summary>
        /// Gets or sets the Defined Values.
        /// </summary>
        /// <value>
        /// the Defined Values.
        /// </value>
        public DbSet<Rock.Model.DefinedValue> DefinedValues { get; set; }

        /// <summary>
        /// Gets or sets the Entity Changes.
        /// </summary>
        /// <value>
        /// the Entity Changes.
        /// </value>
        public DbSet<Rock.Model.EntityChange> EntityChanges { get; set; }

        /// <summary>
        /// Gets or sets the entity types.
        /// </summary>
        /// <value>
        /// The entity types.
        /// </value>
        public DbSet<Rock.Model.EntityType> EntityTypes { get; set; }

        /// <summary>
        /// Gets or sets the Exception Logs.
        /// </summary>
        /// <value>
        /// the Exception Logs.
        /// </value>
        public DbSet<Rock.Model.ExceptionLog> ExceptionLogs { get; set; }

        /// <summary>
        /// Gets or sets the Field Types.
        /// </summary>
        /// <value>
        /// the Field Types.
        /// </value>
        public DbSet<Rock.Model.FieldType> FieldTypes { get; set; }

        /// <summary>
        /// Gets or sets the Defined Types.
        /// </summary>
        /// <value>
        /// the Defined Types.
        /// </value>
        public DbSet<Rock.Model.Metric> Metrics { get; set; }

        /// <summary>
        /// Gets or sets the Defined Values.
        /// </summary>
        /// <value>
        /// the Defined Values.
        /// </value>
        public DbSet<Rock.Model.MetricValue> MetricValues { get; set; }

        /// <summary>
        /// Gets or sets the Service Logs.
        /// </summary>
        /// <value>
        /// the Service Logs.
        /// </value>
        public DbSet<Rock.Model.ServiceLog> ServiceLogs { get; set; }

        /// <summary>
        /// Gets or sets the Tags.
        /// </summary>
        /// <value>
        /// the Tags.
        /// </value>
        public DbSet<Rock.Model.Tag> Tags { get; set; }

        /// <summary>
        /// Gets or sets the Tagged Items.
        /// </summary>
        /// <value>
        /// the Tagged Items.
        /// </value>
        public DbSet<Rock.Model.TaggedItem> TaggedItems { get; set; }

        #endregion

        #region Crm

        /// <summary>
        /// Gets or sets the Location.
        /// </summary>
        /// <value>
        /// the Location.
        /// </value>
        public DbSet<Rock.Model.Location> Locations { get; set; }

        /// <summary>
        /// Gets or sets the Campuses.
        /// </summary>
        /// <value>
        /// the Campuses.
        /// </value>
        public DbSet<Rock.Model.Campus> Campuses { get; set; }

        /// <summary>
        /// Gets or sets the Email Templates.
        /// </summary>
        /// <value>
        /// the Email Templates.
        /// </value>
        public DbSet<Rock.Model.EmailTemplate> EmailTemplates { get; set; }

        /// <summary>
        /// Gets or sets the People.
        /// </summary>
        /// <value>
        /// the People.
        /// </value>
        public DbSet<Rock.Model.Person> People { get; set; }

        /// <summary>
        /// Gets or sets the Person Trails.
        /// </summary>
        /// <value>
        /// the Person Trails.
        /// </value>
        public DbSet<Rock.Model.PersonMerged> PersonMerges { get; set; }

        /// <summary>
        /// Gets or sets the Person Vieweds.
        /// </summary>
        /// <value>
        /// the Person Vieweds.
        /// </value>
        public DbSet<Rock.Model.PersonViewed> PersonVieweds { get; set; }

        /// <summary>
        /// Gets or sets the Phone Numbers.
        /// </summary>
        /// <value>
        /// the Phone Numbers.
        /// </value>
        public DbSet<Rock.Model.PhoneNumber> PhoneNumbers { get; set; }

        /// <summary>
        /// Gets or sets the Groups.
        /// </summary>
        /// <value>
        /// the Groups.
        /// </value>
        public DbSet<Rock.Model.Group> Groups { get; set; }

        /// <summary>
        /// Gets or sets the Group Roles.
        /// </summary>
        /// <value>
        /// the Group Roles.
        /// </value>
        public DbSet<Rock.Model.GroupRole> GroupRoles { get; set; }

        /// <summary>
        /// Gets or sets the Group Types.
        /// </summary>
        /// <value>
        /// the Group Types.
        /// </value>
        public DbSet<Rock.Model.GroupType> GroupTypes { get; set; }

        /// <summary>
        /// Gets or sets the Group Locations.
        /// </summary>
        /// <value>
        /// the Group Locations.
        /// </value>
        public DbSet<Rock.Model.GroupLocation> GroupLocations { get; set; }

        /// <summary>
        /// Gets or sets the Members.
        /// </summary>
        /// <value>
        /// the Members.
        /// </value>
        public DbSet<Rock.Model.GroupMember> Members { get; set; }

        #endregion

        #region Util

        /// <summary>
        /// Gets or sets the actions.
        /// </summary>
        /// <value>
        /// The actions.
        /// </value>
        public DbSet<Rock.Model.Action> Actions { get; set; }

        /// <summary>
        /// Gets or sets the action types.
        /// </summary>
        /// <value>
        /// The action types.
        /// </value>
        public DbSet<Rock.Model.ActionType> ActionTypes { get; set; }

        /// <summary>
        /// Gets or sets the activities.
        /// </summary>
        /// <value>
        /// The activities.
        /// </value>
        public DbSet<Rock.Model.Activity> Activities { get; set; }

        /// <summary>
        /// Gets or sets the activity types.
        /// </summary>
        /// <value>
        /// The activity types.
        /// </value>
        public DbSet<Rock.Model.ActivityType> ActivityTypes { get; set; }

        /// <summary>
        /// Gets or sets the Jobs.
        /// </summary>
        /// <value>
        /// the Jobs.
        /// </value>
        public DbSet<Rock.Model.ServiceJob> Jobs { get; set; }

        /// <summary>
        /// Gets or sets the workflows.
        /// </summary>
        /// <value>
        /// The workflows.
        /// </value>
        public DbSet<Rock.Model.Workflow> Workflows { get; set; }

        /// <summary>
        /// Gets or sets the workflow logs.
        /// </summary>
        /// <value>
        /// The workflow logs.
        /// </value>
        public DbSet<Rock.Model.WorkflowLog> WorkflowLogs { get; set; }

        /// <summary>
        /// Gets or sets the workflow triggers.
        /// </summary>
        /// <value>
        /// The entity type workflow triggers.
        /// </value>
        public DbSet<Rock.Model.WorkflowTrigger> WorkflowTriggers { get; set; }

        /// <summary>
        /// Gets or sets the workflow types.
        /// </summary>
        /// <value>
        /// The workflow types.
        /// </value>
        public DbSet<Rock.Model.WorkflowType> WorkflowTypes { get; set; }

        #endregion

        #region Financial

        /// <summary>
        /// Gets or sets the batches.
        /// </summary>
        /// <value>
        /// The batches.
        /// </value>
        public DbSet<Rock.Model.FinancialBatch> Batches { get; set; }

        /// <summary>
        /// Gets or sets the fund.
        /// </summary>
        /// <value>
        /// The fund.
        /// </value>
        public DbSet<Rock.Model.Fund> Fund { get; set; }

        /// <summary>
        /// Gets or sets the pledges.
        /// </summary>
        /// <value>
        /// The pledges.
        /// </value>
        public DbSet<Rock.Model.Pledge> Pledges { get; set; }

        /// <summary>
        /// Gets or sets the transactions.
        /// </summary>
        /// <value>
        /// The transactions.
        /// </value>
        public DbSet<Rock.Model.FinancialTransaction> Transactions { get; set; }

        /// <summary>
        /// Gets or sets the transaction details.
        /// </summary>
        /// <value>
        /// The transaction details.
        /// </value>
        public DbSet<Rock.Model.FinancialTransactionDetail> TransactionDetails { get; set; }

        /// <summary>
        /// Gets or sets the person account lookups.
        /// </summary>
        /// <value>
        /// The person account lookups.
        /// </value>
        public DbSet<Rock.Model.PersonAccount> PersonAccountLookups { get; set; }

        /// <summary>
        /// Gets or sets the transaction funds.
        /// </summary>
        /// <value>
        /// The transaction funds.
        /// </value>
        public DbSet<Rock.Model.FinancialTransactionFund> TransactionFunds { get; set; }

        #endregion

        /// <summary>
        /// This method is called when the context has been initialized, but
        /// before the model has been locked down and used to initialize the context. 
        /// </summary>
        /// <param name="modelBuilder">The builder that defines the model for the context being created.</param>
        protected override void OnModelCreating( DbModelBuilder modelBuilder )
        {
            ContextHelper.AddConfigurations( modelBuilder );
        }

        /// <summary>
        /// Gets the name of the entity from table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns></returns>
        public static Type GetEntityFromTableName( string tableName )
        {
            var thisType = typeof( RockContext );
            var props = thisType.GetProperties();
            foreach ( PropertyInfo pi in props )
            {
                Type modelType = pi.PropertyType.GetGenericArguments()[0];
                TableAttribute tableAttribute = modelType.GetCustomAttribute<TableAttribute>();
                if ( tableAttribute.Name.Equals( tableName, StringComparison.OrdinalIgnoreCase ) )
                {
                    return modelType;
                }
            }

            return null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class ContextHelper
    {
        /// <summary>
        /// Adds the configurations.
        /// </summary>
        /// <param name="modelBuilder">The model builder.</param>
        public static void AddConfigurations(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            modelBuilder.Configurations.Add( new Rock.Model.AuthConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Model.BlockTypeConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Model.BlockConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Model.FileConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Model.HtmlContentConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Model.PageConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Model.PageRouteConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Model.SiteConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Model.SiteDomainConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Model.UserConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Model.MarketingCampaignAdConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Model.MarketingCampaignAdTypeConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Model.MarketingCampaignAudienceConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Model.MarketingCampaignCampusConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Model.MarketingCampaignConfiguration() );

            modelBuilder.Configurations.Add( new Rock.Model.AttributeConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Model.AttributeQualifierConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Model.AttributeValueConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Model.AuditConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Model.CategoryConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Model.DefinedTypeConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Model.DefinedValueConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Model.EntityChangeConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Model.EntityTypeConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Model.ExceptionLogConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Model.FieldTypeConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Model.MetricConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Model.MetricValueConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Model.ServiceLogConfiguration() );

            modelBuilder.Configurations.Add( new Rock.Model.CampusConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Model.EmailTemplateConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Model.GroupConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Model.GroupRoleConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Model.GroupTypeConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Model.GroupLocationConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Model.LocationConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Model.MemberConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Model.PersonConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Model.PersonMergedConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Model.PersonViewedConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Model.PhoneNumberConfiguration() );

            modelBuilder.Configurations.Add( new Rock.Model.BatchConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Model.FundConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Model.PledgeConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Model.TransactionConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Model.TransactionDetailConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Model.PersonAccountLookupConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Model.TransactionFundConfiguration() );

            modelBuilder.Configurations.Add( new Rock.Model.ActionConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Model.ActionTypeConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Model.ActivityConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Model.ActivityTypeConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Model.JobConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Model.WorkflowConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Model.WorkflowLogConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Model.WorkflowTriggerConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Model.WorkflowTypeConfiguration() );
        }
    }
}