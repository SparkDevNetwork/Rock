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
using Rock.Model;

namespace Rock.Data
{
    /// <summary>
    /// Entity Framework Context
    /// </summary>
    public partial class RockContext : DbContext
    {
        #region Models

        /// <summary>
        /// Gets or sets the attendances.
        /// </summary>
        /// <value>
        /// The attendances.
        /// </value>
        public DbSet<Attendance> Attendances { get; set; }

        /// <summary>
        /// Gets or sets the attendance codes.
        /// </summary>
        /// <value>
        /// The attendance codes.
        /// </value>
        public DbSet<AttendanceCode> AttendanceCodes { get; set; }

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
        public DbSet<AttributeQualifier> AttributeQualifiers { get; set; }

        /// <summary>
        /// Gets or sets the Attribute Values.
        /// </summary>
        /// <value>
        /// the Attribute Values.
        /// </value>
        public DbSet<AttributeValue> AttributeValues { get; set; }

        /// <summary>
        /// Gets or sets the Attribute Values.
        /// </summary>
        /// <value>
        /// the Attribute Values.
        /// </value>
        public DbSet<Audit> Audits { get; set; }

        /// <summary>
        /// Gets or sets the Auths.
        /// </summary>
        /// <value>
        /// the Auths.
        /// </value>
        public DbSet<Auth> Auths { get; set; }

        /// <summary>
        /// Gets or sets the Files.
        /// </summary>
        /// <value>
        /// the Files.
        /// </value>
        public DbSet<Model.BinaryFile> BinaryFiles { get; set; }

        /// <summary>
        /// Gets or sets the Files data.
        /// </summary>
        /// <value>
        /// the Files data
        /// </value>
        public DbSet<BinaryFileData> BinaryFilesData { get; set; }

        /// <summary>
        /// Gets or sets the Binary File Types.
        /// </summary>
        /// <value>
        /// the Binary File Types.
        /// </value>
        public DbSet<BinaryFileType> BinaryFileTypes { get; set; }

        /// <summary>
        /// Gets or sets the Blocks.
        /// </summary>
        /// <value>
        /// the Blocks.
        /// </value>
        public DbSet<Block> Blocks { get; set; }

        /// <summary>
        /// Gets or sets the Block Types.
        /// </summary>
        /// <value>
        /// the Block Types.
        /// </value>
        public DbSet<BlockType> BlockTypes { get; set; }

        /// <summary>
        /// Gets or sets the Campuses.
        /// </summary>
        /// <value>
        /// the Campuses.
        /// </value>
        public DbSet<Campus> Campuses { get; set; }

        /// <summary>
        /// Gets or sets the categories.
        /// </summary>
        /// <value>
        /// The categories.
        /// </value>
        public DbSet<Category> Categories { get; set; }

        /// <summary>
        /// Gets or sets the communications.
        /// </summary>
        /// <value>
        /// The communications.
        /// </value>
        public DbSet<Rock.Model.Communication> Communications { get; set; }

        /// <summary>
        /// Gets or sets the communication recipients.
        /// </summary>
        /// <value>
        /// The communication recipients.
        /// </value>
        public DbSet<CommunicationRecipient> CommunicationRecipients { get; set; }

        /// <summary>
        /// Gets or sets the data views.
        /// </summary>
        /// <value>
        /// The data views.
        /// </value>
        public DbSet<DataView> DataViews { get; set; }

        /// <summary>
        /// Gets or sets the data view filters.
        /// </summary>
        /// <value>
        /// The data view filters.
        /// </value>
        public DbSet<DataViewFilter> DataViewFilters { get; set; }

        /// <summary>
        /// Gets or sets the Defined Types.
        /// </summary>
        /// <value>
        /// the Defined Types.
        /// </value>
        public DbSet<DefinedType> DefinedTypes { get; set; }

        /// <summary>
        /// Gets or sets the Defined Values.
        /// </summary>
        /// <value>
        /// the Defined Values.
        /// </value>
        public DbSet<DefinedValue> DefinedValues { get; set; }

        /// <summary>
        /// Gets or sets the devices.
        /// </summary>
        /// <value>
        /// The devices.
        /// </value>
        public DbSet<Device> Devices { get; set; }

        /// <summary>
        /// Gets or sets the Email Templates.
        /// </summary>
        /// <value>
        /// the Email Templates.
        /// </value>
        public DbSet<EmailTemplate> EmailTemplates { get; set; }

        /// <summary>
        /// Gets or sets the entity types.
        /// </summary>
        /// <value>
        /// The entity types.
        /// </value>
        public DbSet<EntityType> EntityTypes { get; set; }

        /// <summary>
        /// Gets or sets the Exception Logs.
        /// </summary>
        /// <value>
        /// the Exception Logs.
        /// </value>
        public DbSet<ExceptionLog> ExceptionLogs { get; set; }

        /// <summary>
        /// Gets or sets the Field Types.
        /// </summary>
        /// <value>
        /// the Field Types.
        /// </value>
        public DbSet<FieldType> FieldTypes { get; set; }

        /// <summary>
        /// Gets or sets the accounts.
        /// </summary>
        /// <value>
        /// The fund.
        /// </value>
        public DbSet<FinancialAccount> FinancialAccounts { get; set; }

        /// <summary>
        /// Gets or sets the batches.
        /// </summary>
        /// <value>
        /// The batches.
        /// </value>
        public DbSet<FinancialBatch> FinancialBatches { get; set; }

        /// <summary>
        /// Gets or sets the pledges.
        /// </summary>
        /// <value>
        /// The pledges.
        /// </value>
        public DbSet<FinancialPledge> FinancialPledges { get; set; }

        /// <summary>
        /// Gets or sets the financial person bank account.
        /// </summary>
        /// <value>
        /// The financial person bank account.
        /// </value>
        public DbSet<FinancialPersonBankAccount> FinancialPersonBankAccounts { get; set; }

        /// <summary>
        /// Gets or sets the financial person saved account.
        /// </summary>
        /// <value>
        /// The financial person saved account.
        /// </value>
        public DbSet<FinancialPersonSavedAccount> FinancialPersonSavedAccounts { get; set; }

        /// <summary>
        /// Gets or sets the financial scheduled transactions.
        /// </summary>
        /// <value>
        /// The financial scheduled transactions.
        /// </value>
        public DbSet<FinancialScheduledTransaction> FinancialScheduledTransactions { get; set; }

        /// <summary>
        /// Gets or sets the financial scheduled transaction details.
        /// </summary>
        /// <value>
        /// The financial scheduled transaction details.
        /// </value>
        public DbSet<FinancialScheduledTransactionDetail> FinancialScheduledTransactionDetails { get; set; }

        /// <summary>
        /// Gets or sets the transactions.
        /// </summary>
        /// <value>
        /// The transactions.
        /// </value>
        public DbSet<FinancialTransaction> FinancialTransactions { get; set; }

        /// <summary>
        /// Gets or sets the transaction details.
        /// </summary>
        /// <value>
        /// The transaction details.
        /// </value>
        public DbSet<FinancialTransactionDetail> FinancialTransactionDetails { get; set; }

        /// <summary>
        /// Gets or sets the transaction details.
        /// </summary>
        /// <value>
        /// The transaction details.
        /// </value>
        public DbSet<FinancialTransactionImage> FinancialTransactionImages { get; set; }

        /// <summary>
        /// Gets or sets the financial transaction refunds.
        /// </summary>
        /// <value>
        /// The financial transaction refunds.
        /// </value>
        public DbSet<FinancialTransactionRefund> FinancialTransactionRefunds { get; set; }

        /// <summary>
        /// Gets or sets the Groups.
        /// </summary>
        /// <value>
        /// the Groups.
        /// </value>
        public DbSet<Group> Groups { get; set; }

        /// <summary>
        /// Gets or sets the Group Locations.
        /// </summary>
        /// <value>
        /// the Group Locations.
        /// </value>
        public DbSet<GroupLocation> GroupLocations { get; set; }

        /// <summary>
        /// Gets or sets the Members.
        /// </summary>
        /// <value>
        /// the Members.
        /// </value>
        public DbSet<GroupMember> GroupMembers { get; set; }

        /// <summary>
        /// Gets or sets the Group Roles.
        /// </summary>
        /// <value>
        /// the Group Roles.
        /// </value>
        public DbSet<GroupRole> GroupRoles { get; set; }

        /// <summary>
        /// Gets or sets the Group Types.
        /// </summary>
        /// <value>
        /// the Group Types.
        /// </value>
        public DbSet<GroupType> GroupTypes { get; set; }

        /// <summary>
        /// Gets or sets the Html Contents.
        /// </summary>
        /// <value>
        /// the Html Contents.
        /// </value>
        public DbSet<HtmlContent> HtmlContents { get; set; }

        /// <summary>
        /// Gets or sets the Location.
        /// </summary>
        /// <value>
        /// the Location.
        /// </value>
        public DbSet<Location> Locations { get; set; }

        /// <summary>
        /// Gets or sets the marketing campaigns.
        /// </summary>
        /// <value>
        /// The marketing campaigns.
        /// </value>
        public DbSet<MarketingCampaign> MarketingCampaigns { get; set; }

        /// <summary>
        /// Gets or sets the marketing campaign ads.
        /// </summary>
        /// <value>
        /// The marketing campaign ads.
        /// </value>
        public DbSet<MarketingCampaignAd> MarketingCampaignAds { get; set; }

        /// <summary>
        /// Gets or sets the marketing campaign ad types.
        /// </summary>
        /// <value>
        /// The marketing campaign ad types.
        /// </value>
        public DbSet<MarketingCampaignAdType> MarketingCampaignAdTypes { get; set; }

        /// <summary>
        /// Gets or sets the marketing campaign audiences.
        /// </summary>
        /// <value>
        /// The marketing campaign audiences.
        /// </value>
        public DbSet<MarketingCampaignAudience> MarketingCampaignAudiences { get; set; }

        /// <summary>
        /// Gets or sets the marketing campaign campuses.
        /// </summary>
        /// <value>
        /// The marketing campaign campuses.
        /// </value>
        public DbSet<MarketingCampaignCampus> MarketingCampaignCampuses { get; set; }

        /// <summary>
        /// Gets or sets the Defined Types.
        /// </summary>
        /// <value>
        /// the Defined Types.
        /// </value>
        public DbSet<Metric> Metrics { get; set; }

        /// <summary>
        /// Gets or sets the Defined Values.
        /// </summary>
        /// <value>
        /// the Defined Values.
        /// </value>
        public DbSet<MetricValue> MetricValues { get; set; }

        /// <summary>
        /// Gets or sets the notes.
        /// </summary>
        /// <value>
        /// The notes.
        /// </value>
        public DbSet<Note> Notes { get; set; }

        /// <summary>
        /// Gets or sets the note types.
        /// </summary>
        /// <value>
        /// The note types.
        /// </value>
        public DbSet<NoteType> NoteTypes { get; set; }
        
        /// <summary>
        /// Gets or sets the Pages.
        /// </summary>
        /// <value>
        /// the Pages.
        /// </value>
        public DbSet<Page> Pages { get; set; }

        /// <summary>
        /// Gets or sets the page contexts.
        /// </summary>
        /// <value>
        /// The page contexts.
        /// </value>
        public DbSet<PageContext> PageContexts { get; set; } 

        /// <summary>
        /// Gets or sets the Page Routes.
        /// </summary>
        /// <value>
        /// the Page Routes.
        /// </value>
        public DbSet<PageRoute> PageRoutes { get; set; }

        /// <summary>
        /// Gets or sets the People.
        /// </summary>
        /// <value>
        /// the People.
        /// </value>
        public DbSet<Person> People { get; set; }

        /// <summary>
        /// Gets or sets the Person Trails.
        /// </summary>
        /// <value>
        /// the Person Trails.
        /// </value>
        public DbSet<PersonMerged> PersonMerges { get; set; }

        /// <summary>
        /// Gets or sets the Person Vieweds.
        /// </summary>
        /// <value>
        /// the Person Vieweds.
        /// </value>
        public DbSet<PersonViewed> PersonVieweds { get; set; }

        /// <summary>
        /// Gets or sets the Phone Numbers.
        /// </summary>
        /// <value>
        /// the Phone Numbers.
        /// </value>
        public DbSet<PhoneNumber> PhoneNumbers { get; set; }

        /// <summary>
        /// Gets or sets the prayer requests.
        /// </summary>
        /// <value>
        /// The prayer requests.
        /// </value>
        public DbSet<PrayerRequest> PrayerRequests { get; set; }

        /// <summary>
        /// Gets or sets the reports.
        /// </summary>
        /// <value>
        /// The reports.
        /// </value>
        public DbSet<Report> Reports { get; set; }

        /// <summary>
        /// Gets or sets the schedules.
        /// </summary>
        /// <value>
        /// The schedules.
        /// </value>
        public DbSet<Schedule> Schedules { get; set; }

        /// <summary>
        /// Gets or sets the Jobs.
        /// </summary>
        /// <value>
        /// the Jobs.
        /// </value>
        public DbSet<ServiceJob> ServiceJobs { get; set; }

        /// <summary>
        /// Gets or sets the Service Logs.
        /// </summary>
        /// <value>
        /// the Service Logs.
        /// </value>
        public DbSet<ServiceLog> ServiceLogs { get; set; }

        /// <summary>
        /// Gets or sets the Sites.
        /// </summary>
        /// <value>
        /// the Sites.
        /// </value>
        public DbSet<Site> Sites { get; set; }

        /// <summary>
        /// Gets or sets the Site Domains.
        /// </summary>
        /// <value>
        /// the Site Domains.
        /// </value>
        public DbSet<SiteDomain> SiteDomains { get; set; }

        /// <summary>
        /// Gets or sets the Tags.
        /// </summary>
        /// <value>
        /// the Tags.
        /// </value>
        public DbSet<Tag> Tags { get; set; }

        /// <summary>
        /// Gets or sets the Tagged Items.
        /// </summary>
        /// <value>
        /// the Tagged Items.
        /// </value>
        public DbSet<TaggedItem> TaggedItems { get; set; }

        /// <summary>
        /// Gets or sets the Users.
        /// </summary>
        /// <value>
        /// the Users.
        /// </value>
        public DbSet<UserLogin> UserLogins { get; set; }

        /// <summary>
        /// Gets or sets the workflows.
        /// </summary>
        /// <value>
        /// The workflows.
        /// </value>
        public DbSet<Rock.Model.Workflow> Workflows { get; set; }

        /// <summary>
        /// Gets or sets the workflow actions.
        /// </summary>
        /// <value>
        /// The workflow actions.
        /// </value>
        public DbSet<WorkflowAction> WorkflowActions { get; set; }

        /// <summary>
        /// Gets or sets the workflow action types.
        /// </summary>
        /// <value>
        /// The workflow action types.
        /// </value>
        public DbSet<WorkflowActionType> WorkflowActionTypes { get; set; }

        /// <summary>
        /// Gets or sets the workflow activities.
        /// </summary>
        /// <value>
        /// The workflow activities.
        /// </value>
        public DbSet<WorkflowActivity> WorkflowActivities { get; set; }

        /// <summary>
        /// Gets or sets the workflow activity types.
        /// </summary>
        /// <value>
        /// The workflow activity types.
        /// </value>
        public DbSet<WorkflowActivityType> WorkflowActivityTypes { get; set; }

        /// <summary>
        /// Gets or sets the workflow logs.
        /// </summary>
        /// <value>
        /// The workflow logs.
        /// </value>
        public DbSet<WorkflowLog> WorkflowLogs { get; set; }

        /// <summary>
        /// Gets or sets the workflow triggers.
        /// </summary>
        /// <value>
        /// The entity type workflow triggers.
        /// </value>
        public DbSet<WorkflowTrigger> WorkflowTriggers { get; set; }

        /// <summary>
        /// Gets or sets the workflow types.
        /// </summary>
        /// <value>
        /// The workflow types.
        /// </value>
        public DbSet<WorkflowType> WorkflowTypes { get; set; }

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

            modelBuilder.Configurations.Add( new AttendanceConfiguration() );
            modelBuilder.Configurations.Add( new AttendanceCodeConfiguration() );
            modelBuilder.Configurations.Add( new AttributeConfiguration() );
            modelBuilder.Configurations.Add( new AttributeQualifierConfiguration() );
            modelBuilder.Configurations.Add( new AttributeValueConfiguration() );
            modelBuilder.Configurations.Add( new AuditConfiguration() );
            modelBuilder.Configurations.Add( new AuthConfiguration() );
            modelBuilder.Configurations.Add( new BinaryFileConfiguration() );
            modelBuilder.Configurations.Add( new BinaryFileDataConfiguration() );
            modelBuilder.Configurations.Add( new BinaryFileTypeConfiguration() );
            modelBuilder.Configurations.Add( new BlockConfiguration() );
            modelBuilder.Configurations.Add( new BlockTypeConfiguration() );
            modelBuilder.Configurations.Add( new CampusConfiguration() );
            modelBuilder.Configurations.Add( new CategoryConfiguration() );
            modelBuilder.Configurations.Add( new CommunicationConfiguration() );
            modelBuilder.Configurations.Add( new CommunicationRecipientConfiguration() );
            modelBuilder.Configurations.Add( new DataViewConfiguration() );
            modelBuilder.Configurations.Add( new DataViewFilterConfiguration() );
            modelBuilder.Configurations.Add( new DefinedTypeConfiguration() );
            modelBuilder.Configurations.Add( new DefinedValueConfiguration() );
            modelBuilder.Configurations.Add( new DeviceConfiguration() );
            modelBuilder.Configurations.Add( new EmailTemplateConfiguration() );
            modelBuilder.Configurations.Add( new EntityTypeConfiguration() );
            modelBuilder.Configurations.Add( new ExceptionLogConfiguration() );
            modelBuilder.Configurations.Add( new FieldTypeConfiguration() );
            modelBuilder.Configurations.Add( new FinancialAccountConfiguration() );
            modelBuilder.Configurations.Add( new FinancialBatchConfiguration() );
            modelBuilder.Configurations.Add( new FinancialPledgeConfiguration() );
            modelBuilder.Configurations.Add( new FinancialPersonBankAccountConfiguration() );
            modelBuilder.Configurations.Add( new FinancialPersonSavedAccountConfiguration() );
            modelBuilder.Configurations.Add( new FinancialScheduledTransactionConfiguration() );
            modelBuilder.Configurations.Add( new FinancialScheduledTransactionDetailConfiguration() );
            modelBuilder.Configurations.Add( new FinancialTransactionConfiguration() );
            modelBuilder.Configurations.Add( new FinancialTransactionDetailConfiguration() );
            modelBuilder.Configurations.Add( new FinancialTransactionImageConfiguration() );
            modelBuilder.Configurations.Add( new FinancialTransactionRefundConfiguration() );
            modelBuilder.Configurations.Add( new GroupConfiguration() );
            modelBuilder.Configurations.Add( new GroupLocationConfiguration() );
            modelBuilder.Configurations.Add( new GroupMemberConfiguration() );
            modelBuilder.Configurations.Add( new GroupRoleConfiguration() );
            modelBuilder.Configurations.Add( new GroupTypeConfiguration() );
            modelBuilder.Configurations.Add( new HtmlContentConfiguration() );
            modelBuilder.Configurations.Add( new LocationConfiguration() );
            modelBuilder.Configurations.Add( new MarketingCampaignConfiguration() );
            modelBuilder.Configurations.Add( new MarketingCampaignAdConfiguration() );
            modelBuilder.Configurations.Add( new MarketingCampaignAdTypeConfiguration() );
            modelBuilder.Configurations.Add( new MarketingCampaignAudienceConfiguration() );
            modelBuilder.Configurations.Add( new MarketingCampaignCampusConfiguration() );
            modelBuilder.Configurations.Add( new MetricConfiguration() );
            modelBuilder.Configurations.Add( new MetricValueConfiguration() );
            modelBuilder.Configurations.Add( new NoteConfiguration() );
            modelBuilder.Configurations.Add( new NoteTypeConfiguration() );
            modelBuilder.Configurations.Add( new PageConfiguration() );
            modelBuilder.Configurations.Add( new PageContextConfiguration() );
            modelBuilder.Configurations.Add( new PageRouteConfiguration() );
            modelBuilder.Configurations.Add( new PersonConfiguration() );
            modelBuilder.Configurations.Add( new PersonMergedConfiguration() );
            modelBuilder.Configurations.Add( new PersonViewedConfiguration() );
            modelBuilder.Configurations.Add( new PhoneNumberConfiguration() );
            modelBuilder.Configurations.Add( new PrayerRequestConfiguration() );
            modelBuilder.Configurations.Add( new ReportConfiguration() );
            modelBuilder.Configurations.Add( new ScheduleConfiguration() );
            modelBuilder.Configurations.Add( new ServiceJobConfiguration() );
            modelBuilder.Configurations.Add( new ServiceLogConfiguration() );
            modelBuilder.Configurations.Add( new SiteConfiguration() );
            modelBuilder.Configurations.Add( new SiteDomainConfiguration() );
            modelBuilder.Configurations.Add( new TagConfiguration() );
            modelBuilder.Configurations.Add( new TaggedItemConfiguration() );
            modelBuilder.Configurations.Add( new UserLoginConfiguration() );
            modelBuilder.Configurations.Add( new WorkflowConfiguration() );
            modelBuilder.Configurations.Add( new WorkflowActionConfiguration() );
            modelBuilder.Configurations.Add( new WorkflowActionTypeConfiguration() );
            modelBuilder.Configurations.Add( new WorkflowActivityConfiguration() );
            modelBuilder.Configurations.Add( new WorkflowActivityTypeConfiguration() );
            modelBuilder.Configurations.Add( new WorkflowLogConfiguration() );
            modelBuilder.Configurations.Add( new WorkflowTriggerConfiguration() );
            modelBuilder.Configurations.Add( new WorkflowTypeConfiguration() );
        }
    }
}