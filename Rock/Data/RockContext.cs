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
        public DbSet<Rock.Cms.Auth> Auths { get; set; }

        /// <summary>
        /// Gets or sets the Block Types.
        /// </summary>
        /// <value>
        /// the Block Types.
        /// </value>
        public DbSet<Rock.Cms.BlockType> BlockTypes { get; set; }

        /// <summary>
        /// Gets or sets the Blocks.
        /// </summary>
        /// <value>
        /// the Blocks.
        /// </value>
        public DbSet<Rock.Cms.Block> Blocks { get; set; }

        /// <summary>
        /// Gets or sets the Files.
        /// </summary>
        /// <value>
        /// the Files.
        /// </value>
        public DbSet<Rock.Cms.File> Files { get; set; }

        /// <summary>
        /// Gets or sets the Html Contents.
        /// </summary>
        /// <value>
        /// the Html Contents.
        /// </value>
        public DbSet<Rock.Cms.HtmlContent> HtmlContents { get; set; }

        /// <summary>
        /// Gets or sets the Pages.
        /// </summary>
        /// <value>
        /// the Pages.
        /// </value>
        public DbSet<Rock.Cms.Page> Pages { get; set; }

        /// <summary>
        /// Gets or sets the Page Routes.
        /// </summary>
        /// <value>
        /// the Page Routes.
        /// </value>
        public DbSet<Rock.Cms.PageRoute> PageRoutes { get; set; }

        /// <summary>
        /// Gets or sets the Sites.
        /// </summary>
        /// <value>
        /// the Sites.
        /// </value>
        public DbSet<Rock.Cms.Site> Sites { get; set; }

        /// <summary>
        /// Gets or sets the Site Domains.
        /// </summary>
        /// <value>
        /// the Site Domains.
        /// </value>
        public DbSet<Rock.Cms.SiteDomain> SiteDomains { get; set; }

        /// <summary>
        /// Gets or sets the Users.
        /// </summary>
        /// <value>
        /// the Users.
        /// </value>
        public DbSet<Rock.Cms.User> Users { get; set; }

        /// <summary>
        /// Gets or sets the marketing campaigns.
        /// </summary>
        /// <value>
        /// The marketing campaigns.
        /// </value>
        public DbSet<Rock.Cms.MarketingCampaign> MarketingCampaigns { get; set; }

        /// <summary>
        /// Gets or sets the marketing campaign ads.
        /// </summary>
        /// <value>
        /// The marketing campaign ads.
        /// </value>
        public DbSet<Rock.Cms.MarketingCampaignAd> MarketingCampaignAds { get; set; }

        /// <summary>
        /// Gets or sets the marketing campaign ad types.
        /// </summary>
        /// <value>
        /// The marketing campaign ad types.
        /// </value>
        public DbSet<Rock.Cms.MarketingCampaignAdType> MarketingCampaignAdTypes { get; set; }

        /// <summary>
        /// Gets or sets the marketing campaign audiences.
        /// </summary>
        /// <value>
        /// The marketing campaign audiences.
        /// </value>
        public DbSet<Rock.Cms.MarketingCampaignAudience> MarketingCampaignAudiences { get; set; }

        /// <summary>
        /// Gets or sets the marketing campaign campuses.
        /// </summary>
        /// <value>
        /// The marketing campaign campuses.
        /// </value>
        public DbSet<Rock.Cms.MarketingCampaignCampus> MarketingCampaignCampuses { get; set; }

        #endregion

        #region Core

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
        /// Gets or sets the Attribute Values.
        /// </summary>
        /// <value>
        /// the Attribute Values.
        /// </value>
        public DbSet<Rock.Core.Audit> Audits { get; set; }

        /// <summary>
        /// Gets or sets the categories.
        /// </summary>
        /// <value>
        /// The categories.
        /// </value>
        public DbSet<Rock.Core.Category> Categories { get; set; }

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
        /// Gets or sets the entity types.
        /// </summary>
        /// <value>
        /// The entity types.
        /// </value>
        public DbSet<Rock.Core.EntityType> EntityTypes { get; set; }

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
        /// Gets or sets the Defined Types.
        /// </summary>
        /// <value>
        /// the Defined Types.
        /// </value>
        public DbSet<Rock.Core.Metric> Metrics { get; set; }

        /// <summary>
        /// Gets or sets the Defined Values.
        /// </summary>
        /// <value>
        /// the Defined Values.
        /// </value>
        public DbSet<Rock.Core.MetricValue> MetricValues { get; set; }

        /// <summary>
        /// Gets or sets the Service Logs.
        /// </summary>
        /// <value>
        /// the Service Logs.
        /// </value>
        public DbSet<Rock.Core.ServiceLog> ServiceLogs { get; set; }

        /// <summary>
        /// Gets or sets the Tags.
        /// </summary>
        /// <value>
        /// the Tags.
        /// </value>
        public DbSet<Rock.Core.Tag> Tags { get; set; }

        /// <summary>
        /// Gets or sets the Tagged Items.
        /// </summary>
        /// <value>
        /// the Tagged Items.
        /// </value>
        public DbSet<Rock.Core.TaggedItem> TaggedItems { get; set; }

        #endregion

        #region Crm

        /// <summary>
        /// Gets or sets the Location.
        /// </summary>
        /// <value>
        /// the Location.
        /// </value>
        public DbSet<Rock.Crm.Location> Locations { get; set; }

        /// <summary>
        /// Gets or sets the Campuses.
        /// </summary>
        /// <value>
        /// the Campuses.
        /// </value>
        public DbSet<Rock.Crm.Campus> Campuses { get; set; }

        /// <summary>
        /// Gets or sets the Email Templates.
        /// </summary>
        /// <value>
        /// the Email Templates.
        /// </value>
        public DbSet<Rock.Crm.EmailTemplate> EmailTemplates { get; set; }

        /// <summary>
        /// Gets or sets the People.
        /// </summary>
        /// <value>
        /// the People.
        /// </value>
        public DbSet<Rock.Crm.Person> People { get; set; }

        /// <summary>
        /// Gets or sets the Person Trails.
        /// </summary>
        /// <value>
        /// the Person Trails.
        /// </value>
        public DbSet<Rock.Crm.PersonMerged> PersonMerges { get; set; }

        /// <summary>
        /// Gets or sets the Person Vieweds.
        /// </summary>
        /// <value>
        /// the Person Vieweds.
        /// </value>
        public DbSet<Rock.Crm.PersonViewed> PersonVieweds { get; set; }

        /// <summary>
        /// Gets or sets the Phone Numbers.
        /// </summary>
        /// <value>
        /// the Phone Numbers.
        /// </value>
        public DbSet<Rock.Crm.PhoneNumber> PhoneNumbers { get; set; }

        /// <summary>
        /// Gets or sets the Groups.
        /// </summary>
        /// <value>
        /// the Groups.
        /// </value>
        public DbSet<Rock.Crm.Group> Groups { get; set; }

        /// <summary>
        /// Gets or sets the Group Roles.
        /// </summary>
        /// <value>
        /// the Group Roles.
        /// </value>
        public DbSet<Rock.Crm.GroupRole> GroupRoles { get; set; }

        /// <summary>
        /// Gets or sets the Group Types.
        /// </summary>
        /// <value>
        /// the Group Types.
        /// </value>
        public DbSet<Rock.Crm.GroupType> GroupTypes { get; set; }

        /// <summary>
        /// Gets or sets the Group Locations.
        /// </summary>
        /// <value>
        /// the Group Locations.
        /// </value>
        public DbSet<Rock.Crm.GroupLocation> GroupLocations { get; set; }

        /// <summary>
        /// Gets or sets the Members.
        /// </summary>
        /// <value>
        /// the Members.
        /// </value>
        public DbSet<Rock.Crm.GroupMember> Members { get; set; }

        #endregion

        #region Util

        /// <summary>
        /// Gets or sets the actions.
        /// </summary>
        /// <value>
        /// The actions.
        /// </value>
        public DbSet<Rock.Util.Action> Actions { get; set; }

        /// <summary>
        /// Gets or sets the action types.
        /// </summary>
        /// <value>
        /// The action types.
        /// </value>
        public DbSet<Rock.Util.ActionType> ActionTypes { get; set; }

        /// <summary>
        /// Gets or sets the activities.
        /// </summary>
        /// <value>
        /// The activities.
        /// </value>
        public DbSet<Rock.Util.Activity> Activities { get; set; }

        /// <summary>
        /// Gets or sets the activity types.
        /// </summary>
        /// <value>
        /// The activity types.
        /// </value>
        public DbSet<Rock.Util.ActivityType> ActivityTypes { get; set; }

        /// <summary>
        /// Gets or sets the Jobs.
        /// </summary>
        /// <value>
        /// the Jobs.
        /// </value>
        public DbSet<Rock.Util.Job> Jobs { get; set; }

        /// <summary>
        /// Gets or sets the workflows.
        /// </summary>
        /// <value>
        /// The workflows.
        /// </value>
        public DbSet<Rock.Util.Workflow> Workflows { get; set; }

        /// <summary>
        /// Gets or sets the workflow logs.
        /// </summary>
        /// <value>
        /// The workflow logs.
        /// </value>
        public DbSet<Rock.Util.WorkflowLog> WorkflowLogs { get; set; }

        /// <summary>
        /// Gets or sets the workflow triggers.
        /// </summary>
        /// <value>
        /// The entity type workflow triggers.
        /// </value>
        public DbSet<Rock.Util.WorkflowTrigger> WorkflowTriggers { get; set; }

        /// <summary>
        /// Gets or sets the workflow types.
        /// </summary>
        /// <value>
        /// The workflow types.
        /// </value>
        public DbSet<Rock.Util.WorkflowType> WorkflowTypes { get; set; }

        #endregion

        #region Financial

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

            modelBuilder.Configurations.Add( new Rock.Cms.AuthConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Cms.BlockTypeConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Cms.BlockConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Cms.FileConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Cms.HtmlContentConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Cms.PageConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Cms.PageRouteConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Cms.SiteConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Cms.SiteDomainConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Cms.UserConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Cms.MarketingCampaignAdConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Cms.MarketingCampaignAdTypeConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Cms.MarketingCampaignAudienceConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Cms.MarketingCampaignCampusConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Cms.MarketingCampaignConfiguration() );

            modelBuilder.Configurations.Add( new Rock.Core.AttributeConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Core.AttributeQualifierConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Core.AttributeValueConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Core.AuditConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Core.CategoryConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Core.DefinedTypeConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Core.DefinedValueConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Core.EntityChangeConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Core.EntityTypeConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Core.ExceptionLogConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Core.FieldTypeConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Core.MetricConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Core.MetricValueConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Core.ServiceLogConfiguration() );

            modelBuilder.Configurations.Add( new Rock.Crm.CampusConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Crm.EmailTemplateConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Crm.GroupConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Crm.GroupRoleConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Crm.GroupTypeConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Crm.GroupLocationConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Crm.LocationConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Crm.MemberConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Crm.PersonConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Crm.PersonMergedConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Crm.PersonViewedConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Crm.PhoneNumberConfiguration() );

            modelBuilder.Configurations.Add( new Rock.Financial.BatchConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Financial.FundConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Financial.PledgeConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Financial.TransactionConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Financial.TransactionDetailConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Financial.PersonAccountLookupConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Financial.TransactionFundConfiguration() );

            modelBuilder.Configurations.Add( new Rock.Util.ActionConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Util.ActionTypeConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Util.ActivityConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Util.ActivityTypeConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Util.JobConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Util.WorkflowConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Util.WorkflowLogConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Util.WorkflowTriggerConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Util.WorkflowTypeConfiguration() );
        }
    }
}