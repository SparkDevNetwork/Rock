// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using Rock.Model;
using Rock.Utility;
using Rock.Workflow;

using InteractivePreGeneratedViews;

namespace Rock.Data
{
    /// <summary>
    /// Helper class to set view cache
    /// </summary>
    public static class RockInteractiveViews
    {
        /// <summary>
        /// Sets the view factory.
        /// </summary>
        /// <param name="path">The path.</param>
        public static void SetViewFactory( string path )
        {
            using ( var rockContext = new RockContext() )
            {
                InteractiveViews.SetViewCacheFactory( rockContext, new FileViewCacheFactory( path ) );
            }
        }
    }

    /// <summary>
    /// Entity Framework Context
    /// </summary>
    public class RockContext : Rock.Data.DbContext
    {
        //private const string APP_LOG_FILENAME = "RockContext";
        //private static string _filePath = string.Empty;
        //private static object _threadlock;

        //private string _contextId = string.Empty;

        //static RockContext()
        //{
        //    string directory = AppDomain.CurrentDomain.BaseDirectory;
        //    directory = Path.Combine( directory, "App_Data", "Logs" );

        //    if ( !Directory.Exists( directory ) )
        //    {
        //        Directory.CreateDirectory( directory );
        //    }

        //    _filePath = Path.Combine( directory, APP_LOG_FILENAME + ".csv" );

        //    _threadlock = new object();
        //}

        /// <summary>
        /// Initializes a new instance of the <see cref="RockContext"/> class.
        /// </summary>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        public RockContext() : base()
        {
            //try
            //{
            //    _contextId = DateTime.Now.Ticks.ToString();

            //    var frames = new System.Diagnostics.StackTrace().GetFrames();

            //    var sb = new System.Text.StringBuilder();
            //    for ( int i = 1; i < frames.Length; i++ )
            //    {
            //        var method = frames[i].GetMethod();
            //        sb.AppendFormat( "{0}:{1}", method.DeclaringType.Name, method.Name );
            //        sb.Append( "; " );
            //    }

            //    LogMessage( _contextId, sb.ToString() );
            //}
            //catch { }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="RockContext"/> class.
        /// Use this if you need to specify a connection string other than the default
        /// </summary>
        /// <param name="nameOrConnectionString">Either the database name or a connection string.</param>
        public RockContext( string nameOrConnectionString )
            : base( nameOrConnectionString )
        {
        }

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
        /// Gets or sets the audit details.
        /// </summary>
        /// <value>
        /// The audit details.
        /// </value>
        public DbSet<AuditDetail> AuditDetails { get; set; }

        /// <summary>
        /// Gets or sets the Auths.
        /// </summary>
        /// <value>
        /// the Auths.
        /// </value>
        public DbSet<Auth> Auths { get; set; }

        /// <summary>
        /// Gets or sets the Benevolence Results.
        /// </summary>
        /// <value>
        /// the Benevolence Results.
        /// </value>
        public DbSet<Model.BenevolenceResult> BenevolenceResults { get; set; }      
        
        /// <summary>
        /// Gets or sets the Benevolence Requests.
        /// </summary>
        /// <value>
        /// The Benevolence Requests.
        /// </value>
        public DbSet<Model.BenevolenceRequest> BenevolenceRequests { get; set; }     
        
        /// <summary>
        /// Gets or sets the Files.
        /// </summary>
        /// <value>
        /// The Files.
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
        /// Gets or sets the communication recipient activities.
        /// </summary>
        /// <value>
        /// The communication recipient activities.
        /// </value>
        public DbSet<CommunicationRecipientActivity> CommunicationRecipientActivities { get; set; }

        /// <summary>
        /// Gets or sets the communication templates.
        /// </summary>
        /// <value>
        /// The communication templates.
        /// </value>
        public DbSet<Rock.Model.CommunicationTemplate> CommunicationTemplates { get; set; }

        /// <summary>
        /// Gets or sets the connection activity types.
        /// </summary>
        /// <value>
        /// The connection activity types.
        /// </value>
        public DbSet<ConnectionActivityType> ConnectionActivityTypes { get; set; }

        /// <summary>
        /// Gets or sets the connection opportunities.
        /// </summary>
        /// <value>
        /// The connection opportunities.
        /// </value>
        public DbSet<ConnectionOpportunity> ConnectionOpportunities { get; set; }

        /// <summary>
        /// Gets or sets the connection opportunity campuses.
        /// </summary>
        /// <value>
        /// The connection opportunity campuses.
        /// </value>
        public DbSet<ConnectionOpportunityCampus> ConnectionOpportunityCampuses { get; set; }

        /// <summary>
        /// Gets or sets the connection opportunity connector groups.
        /// </summary>
        /// <value>
        /// The connection opportunity connector groups.
        /// </value>
        public DbSet<ConnectionOpportunityConnectorGroup> ConnectionOpportunityConnectorGroups { get; set; }

        /// <summary>
        /// Gets or sets the connection opportunity groups.
        /// </summary>
        /// <value>
        /// The connection opportunity groups.
        /// </value>
        public DbSet<ConnectionOpportunityGroup> ConnectionOpportunityGroups { get; set; }

        /// <summary>
        /// Gets or sets the connection requests.
        /// </summary>
        /// <value>
        /// The connection requests.
        /// </value>
        public DbSet<ConnectionRequest> ConnectionRequests { get; set; }

        /// <summary>
        /// Gets or sets the connection request activities.
        /// </summary>
        /// <value>
        /// The connection request activities.
        /// </value>
        public DbSet<ConnectionRequestActivity> ConnectionRequestActivities { get; set; }

        /// <summary>
        /// Gets or sets the connection statuses.
        /// </summary>
        /// <value>
        /// The connection statuses.
        /// </value>
        public DbSet<ConnectionStatus> ConnectionStatuses { get; set; }

        /// <summary>
        /// Gets or sets the connection types.
        /// </summary>
        /// <value>
        /// The connection types.
        /// </value>
        public DbSet<ConnectionType> ConnectionTypes { get; set; }

        /// <summary>
        /// Gets or sets the connection workflows.
        /// </summary>
        /// <value>
        /// The connection workflows.
        /// </value>
        public DbSet<ConnectionWorkflow> ConnectionWorkflows { get; set; }
        
        /// <summary>
        /// Gets or sets the content channels.
        /// </summary>
        /// <value>
        /// The content channels.
        /// </value>
        public DbSet<ContentChannel> ContentChannels { get; set; }

        /// <summary>
        /// Gets or sets the content channel items.
        /// </summary>
        /// <value>
        /// The content channel items.
        /// </value>
        public DbSet<ContentChannelItem> ContentChannelItems { get; set; }

        /// <summary>
        /// Gets or sets the content channel types.
        /// </summary>
        /// <value>
        /// The content channel types.
        /// </value>
        public DbSet<ContentChannelType> ContentChannelTypes { get; set; }

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
        /// Gets or sets the entity sets.
        /// </summary>
        /// <value>
        /// The entity sets.
        /// </value>
        public DbSet<EntitySet> EntitySets { get; set; }

        /// <summary>
        /// Gets or sets the entity set items.
        /// </summary>
        /// <value>
        /// The entity set items.
        /// </value>
        public DbSet<EntitySetItem> EntitySetItems { get; set; }

        /// <summary>
        /// Gets or sets the entity types.
        /// </summary>
        /// <value>
        /// The entity types.
        /// </value>
        public DbSet<EntityType> EntityTypes { get; set; }

        /// <summary>
        /// Gets or sets the event calendars.
        /// </summary>
        /// <value>
        /// The event calendars.
        /// </value>
        public DbSet<EventCalendar> EventCalendars { get; set; }

        /// <summary>
        /// Gets or sets the event calendar content channels.
        /// </summary>
        /// <value>
        /// The event calendar content channels.
        /// </value>
        public DbSet<EventCalendarContentChannel> EventCalendarContentChannels { get; set; }

        /// <summary>
        /// Gets or sets the event calendar items.
        /// </summary>
        /// <value>
        /// The event calendar items.
        /// </value>
        public DbSet<EventCalendarItem> EventCalendarItems { get; set; }

        /// <summary>
        /// Gets or sets the event items.
        /// </summary>
        /// <value>
        /// The event items.
        /// </value>
        public DbSet<EventItem> EventItems { get; set; }

        /// <summary>
        /// Gets or sets the event item audiences.
        /// </summary>
        /// <value>
        /// The event item audiences.
        /// </value>
        public DbSet<EventItemAudience> EventItemAudiences { get; set; }

        /// <summary>
        /// Gets or sets the event item occurrences.
        /// </summary>
        /// <value>
        /// The event item occurrences.
        /// </value>
        public DbSet<EventItemOccurrence> EventItemOccurrences { get; set; }

        /// <summary>
        /// Gets or sets the event item occurrence channel items.
        /// </summary>
        /// <value>
        /// The event item occurrence channel items.
        /// </value>
        public DbSet<EventItemOccurrenceChannelItem> EventItemOccurrenceChannelItems { get; set; }

        /// <summary>
        /// Gets or sets the event item occurrence group maps.
        /// </summary>
        /// <value>
        /// The event item occurrence group maps.
        /// </value>
        public DbSet<EventItemOccurrenceGroupMap> EventItemOccurrenceGroupMaps { get; set; }

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
        /// The Financial Account.
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
        /// Gets or sets the financial gateways.
        /// </summary>
        /// <value>
        /// The financial gateways.
        /// </value>
        public DbSet<FinancialGateway> FinancialGateways { get; set; }

        /// <summary>
        /// Gets or sets the financial payment details.
        /// </summary>
        /// <value>
        /// The financial payment details.
        /// </value>
        public DbSet<FinancialPaymentDetail> FinancialPaymentDetails { get; set; }

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
        /// Gets or sets the pledges.
        /// </summary>
        /// <value>
        /// The pledges.
        /// </value>
        public DbSet<FinancialPledge> FinancialPledges { get; set; }

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
        /// Gets or sets the followings.
        /// </summary>
        /// <value>
        /// The followings.
        /// </value>
        public DbSet<Following> Followings { get; set; }

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
        /// Gets or sets the group member requirements.
        /// </summary>
        /// <value>
        /// The group member requirements.
        /// </value>
        public DbSet<GroupMemberRequirement> GroupMemberRequirements { get; set; }

        /// <summary>
        /// Gets or sets the group member workflow triggers.
        /// </summary>
        /// <value>
        /// The group member workflow triggers.
        /// </value>
        public DbSet<GroupMemberWorkflowTrigger> GroupMemberWorkflowTriggers { get; set; }

        /// <summary>
        /// Gets or sets the group requirements.
        /// </summary>
        /// <value>
        /// The group requirements.
        /// </value>
        public DbSet<GroupRequirement> GroupRequirements { get; set; }

        /// <summary>
        /// Gets or sets the group requirement types.
        /// </summary>
        /// <value>
        /// The group requirement types.
        /// </value>
        public DbSet<GroupRequirementType> GroupRequirementTypes { get; set; }
        
        /// <summary>
        /// Gets or sets the group schedule exclusions.
        /// </summary>
        /// <value>
        /// The group schedule exclusions.
        /// </value>
        public DbSet<GroupScheduleExclusion> GroupScheduleExclusions { get; set; }

        /// <summary>
        /// Gets or sets the Group Types.
        /// </summary>
        /// <value>
        /// the Group Types.
        /// </value>
        public DbSet<GroupType> GroupTypes { get; set; }

        /// <summary>
        /// Gets or sets the Group Roles.
        /// </summary>
        /// <value>
        /// the Group Roles.
        /// </value>
        public DbSet<GroupTypeRole> GroupTypeRoles { get; set; }

        /// <summary>
        /// Gets or sets the histories.
        /// </summary>
        /// <value>
        /// The histories.
        /// </value>
        public DbSet<History> Histories { get; set; }

        /// <summary>
        /// Gets or sets the Html Contents.
        /// </summary>
        /// <value>
        /// the Html Contents.
        /// </value>
        public DbSet<HtmlContent> HtmlContents { get; set; }

        /// <summary>
        /// Gets or sets the layouts.
        /// </summary>
        /// <value>
        /// The layouts.
        /// </value>
        public DbSet<Layout> Layouts { get; set; }

        /// <summary>
        /// Gets or sets the Location.
        /// </summary>
        /// <value>
        /// the Location.
        /// </value>
        public DbSet<Location> Locations { get; set; }

        /// <summary>
        /// Gets or sets the metaphones.
        /// </summary>
        /// <value>
        /// The metaphones.
        /// </value>
        public DbSet<Metaphone> Metaphones { get; set; }

        /// <summary>
        /// Gets or sets the Defined Types.
        /// </summary>
        /// <value>
        /// the Defined Types.
        /// </value>
        public DbSet<Metric> Metrics { get; set; }

        /// <summary>
        /// Gets or sets the metric categories.
        /// </summary>
        /// <value>
        /// The metric categories.
        /// </value>
        public DbSet<MetricCategory> MetricCategories { get; set; }

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
        /// Gets or sets the page views.
        /// </summary>
        /// <value>
        /// The page views.
        /// </value>
        public DbSet<PageView> PageViews { get; set; }

        /// <summary>
        /// Gets or sets the People.
        /// </summary>
        /// <value>
        /// the People.
        /// </value>
        public DbSet<Person> People { get; set; }

        /// <summary>
        /// Gets or sets the Person Aliases.
        /// </summary>
        /// <value>
        /// the Person aliases.
        /// </value>
        public DbSet<PersonAlias> PersonAliases { get; set; }

        /// <summary>
        /// Gets or sets the person badge types.
        /// </summary>
        /// <value>
        /// The person badge types.
        /// </value>
        public DbSet<PersonBadge> PersonBadges { get; set; }

        /// <summary>
        /// Gets or sets the person duplicates.
        /// </summary>
        /// <value>
        /// The person duplicates.
        /// </value>
        public DbSet<PersonDuplicate> PersonDuplicates { get; set; }

        /// <summary>
        /// Gets or sets the person previous names.
        /// </summary>
        /// <value>
        /// The person previous names.
        /// </value>
        public DbSet<PersonPreviousName> PersonPreviousNames { get; set; }        
        
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
        /// Gets or sets the plugin migrations.
        /// </summary>
        /// <value>
        /// The plugin migrations.
        /// </value>
        public DbSet<PluginMigration> PluginMigrations { get; set; }

        /// <summary>
        /// Gets or sets the prayer requests.
        /// </summary>
        /// <value>
        /// The prayer requests.
        /// </value>
        public DbSet<PrayerRequest> PrayerRequests { get; set; }

        /// <summary>
        /// Gets or sets the registrations.
        /// </summary>
        /// <value>
        /// The registrations.
        /// </value>
        public DbSet<Registration> Registrations { get; set; }

        /// <summary>
        /// Gets or sets the registration instances.
        /// </summary>
        /// <value>
        /// The registration instances.
        /// </value>
        public DbSet<RegistrationInstance> RegistrationInstances { get; set; }

        /// <summary>
        /// Gets or sets the registration registrants.
        /// </summary>
        /// <value>
        /// The registration registrants.
        /// </value>
        public DbSet<RegistrationRegistrant> RegistrationRegistrants { get; set; }

        /// <summary>
        /// Gets or sets the registration registrant fees.
        /// </summary>
        /// <value>
        /// The registration registrant fees.
        /// </value>
        public DbSet<RegistrationRegistrantFee> RegistrationRegistrantFees { get; set; }

        /// <summary>
        /// Gets or sets the registration templates.
        /// </summary>
        /// <value>
        /// The registration templates.
        /// </value>
        public DbSet<RegistrationTemplate> RegistrationTemplates { get; set; }

        /// <summary>
        /// Gets or sets the registration template discounts.
        /// </summary>
        /// <value>
        /// The registration template discounts.
        /// </value>
        public DbSet<RegistrationTemplateDiscount> RegistrationTemplateDiscounts { get; set; }

        /// <summary>
        /// Gets or sets the registration template fees.
        /// </summary>
        /// <value>
        /// The registration template fees.
        /// </value>
        public DbSet<RegistrationTemplateFee> RegistrationTemplateFees { get; set; }

        /// <summary>
        /// Gets or sets the registration template forms.
        /// </summary>
        /// <value>
        /// The registration template forms.
        /// </value>
        public DbSet<RegistrationTemplateForm> RegistrationTemplateForms { get; set; }

        /// <summary>
        /// Gets or sets the registration template form fields.
        /// </summary>
        /// <value>
        /// The registration template form fields.
        /// </value>
        public DbSet<RegistrationTemplateFormField> RegistrationTemplateFormFields { get; set; }

        /// <summary>
        /// Gets or sets the reports.
        /// </summary>
        /// <value>
        /// The reports.
        /// </value>
        public DbSet<Report> Reports { get; set; }

        /// <summary>
        /// Gets or sets the report fields.
        /// </summary>
        /// <value>
        /// The report fields.
        /// </value>
        public DbSet<ReportField> ReportFields { get; set; }

        /// <summary>
        /// Gets or sets the REST Actions.
        /// </summary>
        /// <value>
        /// The reports.
        /// </value>
        public DbSet<RestAction> RestActions { get; set; }

        /// <summary>
        /// Gets or sets the REST Controllers.
        /// </summary>
        /// <value>
        /// The reports.
        /// </value>
        public DbSet<RestController> RestControllers { get; set; }

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
        /// Gets or sets the system emails.
        /// </summary>
        /// <value>
        /// The system emails.
        /// </value>
        public DbSet<SystemEmail> SystemEmails { get; set; }

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
        /// Gets or sets the workflow action forms.
        /// </summary>
        /// <value>
        /// The workflow action forms.
        /// </value>
        public DbSet<WorkflowActionForm> WorkflowActionForms { get; set; }

        /// <summary>
        /// Gets or sets the workflow action form attributes.
        /// </summary>
        /// <value>
        /// The workflow action form attributes.
        /// </value>
        public DbSet<WorkflowActionFormAttribute> WorkflowActionFormAttributes { get; set; }

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

            modelBuilder.Conventions.Add( new GetAddressStoreFunctionInjectionConvention() );
            modelBuilder.Conventions.Add( new GetGeofencingGroupNamesStoreFunctionInjectionConvention() );

            try
            {
                //// dynamically add plugin entities so that queryables can use a mixture of entities from different plugins and core
                //// from http://romiller.com/2012/03/26/dynamically-building-a-model-with-code-first/, but using the new RegisterEntityType in 6.1.3
                
                // look for IRockEntity classes
                var entityTypeList = Reflection.FindTypes( typeof( Rock.Data.IRockEntity ) )
                    .Where( a => !a.Value.IsAbstract && ( a.Value.GetCustomAttribute<NotMappedAttribute>() == null ) && ( a.Value.GetCustomAttribute<System.Runtime.Serialization.DataContractAttribute>() != null ) )
                    .OrderBy( a => a.Key ).Select( a => a.Value );

                foreach ( var entityType in entityTypeList )
                {
                    modelBuilder.RegisterEntityType( entityType );
                }

                // add configurations that might be in plugin assemblies
                foreach ( var assembly in entityTypeList.Select( a => a.Assembly ).Distinct() )
                {
                    modelBuilder.Configurations.AddFromAssembly( assembly );
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( new Exception( "Exception occurred when adding Plugin Entities to RockContext", ex ), null );
            }
        }

        ///// <summary>
        ///// Disposes the context. The underlying <see cref="T:System.Data.Entity.Core.Objects.ObjectContext" /> is also disposed if it was created
        ///// is by this context or ownership was passed to this context when this context was created.
        ///// The connection to the database (<see cref="T:System.Data.Common.DbConnection" /> object) is also disposed if it was created
        ///// is by this context or ownership was passed to this context when this context was created.
        ///// </summary>
        ///// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        //protected override void Dispose( bool disposing )
        //{
        //    LogMessage( _contextId, "Disposed" );
        //    base.Dispose( disposing );
        //}

        ///// <summary>
        ///// Logs the message.
        ///// </summary>
        ///// <param name="contextId">The context identifier.</param>
        ///// <param name="message">The message.</param>
        //private static void LogMessage( string contextId, string message )
        //{
        //    try
        //    {
        //        lock ( _threadlock )
        //        {
        //            string when = RockDateTime.Now.ToString();
        //            File.AppendAllText( _filePath, string.Format( "{0},{1},{2}\r\n", contextId, when, message ) );
        //        }
        //    }
        //    catch { }
        //}

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
        public static void AddConfigurations( DbModelBuilder modelBuilder )
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Configurations.AddFromAssembly( typeof( RockContext ).Assembly );
        }
    }
}