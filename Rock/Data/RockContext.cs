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
using System.Data.Entity.Core.Objects;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Reflection;

using Rock.Configuration;
using Rock.Model;

namespace Rock.Data
{
    /// <summary>
    /// Entity Framework Context
    /// </summary>
    public class RockContext : Rock.Data.DbContext
    {
        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether timing metrics should be captured.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [metrics enabled]; otherwise, <c>false</c>.
        /// </value>
        public QueryMetricDetailLevel QueryMetricDetailLevel { get; set; } = QueryMetricDetailLevel.Off;

        /// <summary>
        /// Gets or sets the metric query count.
        /// </summary>
        /// <value>
        /// The query count.
        /// </value>
        public int QueryCount { get; set; } = 0;

        /// <summary>
        /// Gets or sets the metric details.
        /// </summary>
        /// <value>
        /// The metric details.
        /// </value>
        public List<QueryMetricDetail> QueryMetricDetails { get; private set; } = new List<QueryMetricDetail>();

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="RockContext"/> class.
        /// Use this if you need to specify a connection string other than the default
        /// </summary>
        /// <param name="nameOrConnectionString">Either the database name or a connection string.</param>
        public RockContext( string nameOrConnectionString )
            : base( nameOrConnectionString )
        {
        }

        /// <summary>
        /// Initializes a new instance of a <see cref="RockContext"/> sub-class using the same <see cref="ObjectContext"/> as regular RockContext.
        /// This is for internal use by <see cref="RockContextReadOnly"/> and <see cref="RockContextAnalytics"/>. 
        /// </summary>
        /// <param name="objectContext">The object context.</param>
        /// <param name="dbContextOwnsObjectContext">if set to <c>true</c> [database context owns object context].</param>
        /// <inheritdoc />
        internal protected RockContext( ObjectContext objectContext, bool dbContextOwnsObjectContext ) :
            base( objectContext, dbContextOwnsObjectContext )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockContext"/> class.
        /// </summary>
        public RockContext()
            : base( RockApp.Current.InitializationSettings.ConnectionString )
        {
        }

        #region Models

        /// <summary>
        /// Gets or sets the AchievementAttempts.
        /// </summary>
        /// <value>
        /// The AchievementAttempts.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<AchievementAttempt> AchievementAttempts { get; set; }

        /// <summary>
        /// Gets or sets the AchievementTypes.
        /// </summary>
        /// <value>
        /// The AchievementTypes.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<AchievementType> AchievementTypes { get; set; }

        /// <summary>
        /// Gets or sets the streak type achievement type prerequisites.
        /// </summary>
        /// <value>
        /// The streak type achievement type prerequisites.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<AchievementTypePrerequisite> AchievementTypePrerequisites { get; set; }

        /// <summary>
        /// Gets or sets the adaptive messages.
        /// </summary>
        /// <value>
        /// The adaptive messages.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<AdaptiveMessage> AdaptiveMessages { get; set; }

        /// <summary>
        /// Gets or sets the adaptive message adaptations.
        /// </summary>
        /// <value>
        /// The adaptive message adaptations.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<AdaptiveMessageAdaptation> AdaptiveMessageAdaptations { get; set; }

        /// <summary>
        /// Gets or sets the adaptive message adaptation segments.
        /// </summary>
        /// <value>
        /// The adaptive message adaptation segments.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<AdaptiveMessageAdaptationSegment> AdaptiveMessageAdaptationSegments { get; set; }

        /// <summary>
        /// Gets or sets the analytics dim attendance locations.
        /// </summary>
        /// <value>
        /// The analytics dim attendance locations.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<AnalyticsDimAttendanceLocation> AnalyticsDimAttendanceLocations { get; set; }

        /// <summary>
        /// Gets or sets the analytics source dates.
        /// </summary>
        /// <value>
        /// The analytics source dates.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<AnalyticsSourceDate> AnalyticsSourceDates { get; set; }

        /// <summary>
        /// Gets or sets the analytics dim campuses.
        /// </summary>
        /// <value>
        /// The analytics dim campuses.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<AnalyticsDimCampus> AnalyticsDimCampuses { get; set; }

        /// <summary>
        /// Gets or sets the analytics dim family currents.
        /// </summary>
        /// <value>
        /// The analytics dim family currents.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<AnalyticsDimFamilyCurrent> AnalyticsDimFamilyCurrents { get; set; }

        /// <summary>
        /// Gets or sets the analytics dim family head of households.
        /// </summary>
        /// <value>
        /// The analytics dim family head of households.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<AnalyticsDimFamilyHeadOfHousehold> AnalyticsDimFamilyHeadOfHouseholds { get; set; }

        /// <summary>
        /// Gets or sets the analytics dim family historicals.
        /// </summary>
        /// <value>
        /// The analytics dim family historicals.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<AnalyticsDimFamilyHistorical> AnalyticsDimFamilyHistoricals { get; set; }

        /// <summary>
        /// Gets or sets the analytics dim financial accounts.
        /// </summary>
        /// <value>
        /// The analytics dim financial accounts.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<AnalyticsDimFinancialAccount> AnalyticsDimFinancialAccounts { get; set; }

        /// <summary>
        /// Gets or sets the analytics dim financial batches.
        /// </summary>
        /// <value>
        /// The analytics dim financial batches.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<AnalyticsDimFinancialBatch> AnalyticsDimFinancialBatches { get; set; }

        /// <summary>
        /// Gets or sets the analytics dim person currents.
        /// </summary>
        /// <value>
        /// The analytics dim person currents.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<AnalyticsDimPersonCurrent> AnalyticsDimPersonCurrents { get; set; }

        /// <summary>
        /// Gets or sets the analytics dim person historicals.
        /// </summary>
        /// <value>
        /// The analytics dim person historicals.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<AnalyticsDimPersonHistorical> AnalyticsDimPersonHistoricals { get; set; }

        /// <summary>
        /// Gets or sets the analytics fact attendances.
        /// </summary>
        /// <value>
        /// The analytics fact attendances.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<AnalyticsFactAttendance> AnalyticsFactAttendances { get; set; }

        /// <summary>
        /// Gets or sets the analytics fact financial transactions.
        /// </summary>
        /// <value>
        /// The analytics fact financial transactions.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<AnalyticsFactFinancialTransaction> AnalyticsFactFinancialTransactions { get; set; }

        /// <summary>
        /// Gets or sets the analytics source financial transactions.
        /// </summary>
        /// <value>
        /// The analytics source financial transactions.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<AnalyticsSourceFinancialTransaction> AnalyticsSourceFinancialTransactions { get; set; }

        /// <summary>
        /// Gets or sets the analytics source attendances.
        /// </summary>
        /// <value>
        /// The analytics source attendances.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<AnalyticsSourceAttendance> AnalyticsSourceAttendances { get; set; }

        /// <summary>
        /// Gets or sets the analytics source campuses.
        /// </summary>
        /// <value>
        /// The analytics source campuses.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<AnalyticsSourceCampus> AnalyticsSourceCampuses { get; set; }

        /// <summary>
        /// Gets or sets the analytics source family historicals.
        /// </summary>
        /// <value>
        /// The analytics source family historicals.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<AnalyticsSourceFamilyHistorical> AnalyticsSourceFamilyHistoricals { get; set; }

        /// <summary>
        /// Gets or sets the analytics source person historicals.
        /// </summary>
        /// <value>
        /// The analytics source person historicals.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<AnalyticsSourcePersonHistorical> AnalyticsSourcePersonHistoricals { get; set; }

        /// <summary>
        /// Gets or sets the analytics source giving units.
        /// </summary>
        /// <value>
        /// The analytics source giving units.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<AnalyticsSourceGivingUnit> AnalyticsSourceGivingUnits { get; set; }

        /// <summary>
        /// Gets or sets the asset storage providers.
        /// </summary>
        /// <value>
        /// The asset storage providers.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<AssetStorageProvider> AssetStorageProviders { get; set; }

        /// <summary>
        /// Gets or sets the assessments.
        /// </summary>
        /// <value>
        /// The assessments
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<Assessment> Assessments { get; set; }

        /// <summary>
        /// Gets or sets the AssessmentTypes.
        /// </summary>
        /// <value>
        /// The AssessmentTypes
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<AssessmentType> AssessmentTypes { get; set; }

        /// <summary>
        /// Gets or sets the attendances.
        /// </summary>
        /// <value>
        /// The attendances.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<Attendance> Attendances { get; set; }

        /// <summary>
        /// Gets or sets the attendance check in sessions.
        /// </summary>
        /// <value>
        /// The attendance check in sessions.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<AttendanceCheckInSession> AttendanceCheckInSessions { get; set; }

        /// <summary>
        /// Gets or sets the attendance codes.
        /// </summary>
        /// <value>
        /// The attendance codes.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<AttendanceCode> AttendanceCodes { get; set; }

        /// <summary>
        /// Gets or sets the attendances data.
        /// </summary>
        /// <value>
        /// The attendances data.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<AttendanceData> AttendancesData { get; set; }

        /// <summary>
        /// Gets or sets the attendance occurrences.
        /// </summary>
        /// <value>
        /// The attendance occurrences.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<AttendanceOccurrence> AttendanceOccurrences { get; set; }

        /// <summary>
        /// Gets or sets the attribute matrices.
        /// </summary>
        /// <value>
        /// The attribute matrices.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<Rock.Model.AttributeMatrix> AttributeMatrices { get; set; }

        /// <summary>
        /// Gets or sets the attribute matrix items.
        /// </summary>
        /// <value>
        /// The attribute matrix items.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<Rock.Model.AttributeMatrixItem> AttributeMatrixItems { get; set; }

        /// <summary>
        /// Gets or sets the attribute matrix templates.
        /// </summary>
        /// <value>
        /// The attribute matrix templates.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<Rock.Model.AttributeMatrixTemplate> AttributeMatrixTemplates { get; set; }

        /// <summary>
        /// Gets or sets the attribute referenced entities.
        /// </summary>
        /// <value>The attribute referenced entities.</value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<Rock.Model.AttributeReferencedEntity> AttributeReferencedEntities { get; set; }

        /// <summary>
        /// Gets or sets the Attributes.
        /// </summary>
        /// <value>
        /// the Attributes.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<Rock.Model.Attribute> Attributes { get; set; }

        /// <summary>
        /// Gets or sets the Attribute Qualifiers.
        /// </summary>
        /// <value>
        /// the Attribute Qualifiers.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<AttributeQualifier> AttributeQualifiers { get; set; }

        /// <summary>
        /// Gets or sets the Attribute Values.
        /// </summary>
        /// <value>
        /// the Attribute Values.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<AttributeValue> AttributeValues { get; set; }

        /// <summary>
        /// Gets or sets the attribute value historicals.
        /// </summary>
        /// <value>
        /// The attribute value historicals.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<AttributeValueHistorical> AttributeValueHistoricals { get; set; }

        /// <summary>
        /// Gets or sets the attribute value referenced entities.
        /// </summary>
        /// <value>The attribute value referenced entities.</value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<Rock.Model.AttributeValueReferencedEntity> AttributeValueReferencedEntities { get; set; }

        /// <summary>
        /// Gets or sets the Attribute Values.
        /// </summary>
        /// <value>
        /// the Attribute Values.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<Audit> Audits { get; set; }

        /// <summary>
        /// Gets or sets the audit details.
        /// </summary>
        /// <value>
        /// The audit details.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<AuditDetail> AuditDetails { get; set; }

        /// <summary>
        /// Gets or sets the authentication audit log.
        /// </summary>
        /// <value>
        /// The authentication audit log.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<AuthAuditLog> AuthAuditLog { get; set; }

        /// <summary>
        /// Gets or sets the Auth Clients.
        /// </summary>
        /// <value>
        /// the Auth Clients.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<AuthClient> AuthClients { get; set; }

        /// <summary>
        /// Gets or sets the Auth Claim.
        /// </summary>
        /// <value>
        /// The Auth Claim.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<AuthClaim> AuthClaims { get; set; }

        /// <summary>
        /// Gets or sets the Auths.
        /// </summary>
        /// <value>
        /// the Auths.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<Auth> Auths { get; set; }

        /// <summary>
        /// Gets or sets the Auth Scope.
        /// </summary>
        /// <value>
        /// The Auth Scope.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<AuthScope> AuthScopes { get; set; }

        /// <summary>
        /// Gets or sets the background checks.
        /// </summary>
        /// <value>
        /// The background checks.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<BackgroundCheck> BackgroundChecks { get; set; }

        /// <summary>
        /// Gets or sets the Benevolence Requests.
        /// </summary>
        /// <value>
        /// The Benevolence Requests.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<BenevolenceRequest> BenevolenceRequests { get; set; }

        /// <summary>
        /// Gets or sets the benevolence request documents.
        /// </summary>
        /// <value>
        /// The benevolence request documents.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<BenevolenceRequestDocument> BenevolenceRequestDocuments { get; set; }

        /// <summary>
        /// Gets or sets the Benevolence Results.
        /// </summary>
        /// <value>
        /// the Benevolence Results.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<BenevolenceResult> BenevolenceResults { get; set; }

        /// <summary>
        /// Gets or sets the benevolence types.
        /// </summary>
        /// <value>The benevolence types.</value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<BenevolenceType> BenevolenceTypes { get; set; }

        /// <summary>
        /// Gets or sets the benevolence workflows.
        /// </summary>
        /// <value>The benevolence workflows.</value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<BenevolenceWorkflow> BenevolenceWorkflows { get; set; }

        /// <summary>
        /// Gets or sets the Files.
        /// </summary>
        /// <value>
        /// The Files.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<Model.BinaryFile> BinaryFiles { get; set; }

        /// <summary>
        /// Gets or sets the Files data.
        /// </summary>
        /// <value>
        /// the Files data
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<BinaryFileData> BinaryFilesData { get; set; }

        /// <summary>
        /// Gets or sets the Binary File Types.
        /// </summary>
        /// <value>
        /// the Binary File Types.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<BinaryFileType> BinaryFileTypes { get; set; }

        /// <summary>
        /// Gets or sets the Blocks.
        /// </summary>
        /// <value>
        /// the Blocks.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<Block> Blocks { get; set; }

        /// <summary>
        /// Gets or sets the Block Types.
        /// </summary>
        /// <value>
        /// the Block Types.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<BlockType> BlockTypes { get; set; }

        /// <summary>
        /// Gets or sets the Campuses.
        /// </summary>
        /// <value>
        /// the Campuses.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<Campus> Campuses { get; set; }

        /// <summary>
        /// Gets or sets the campus schedules.
        /// </summary>
        /// <value>
        /// The campus schedules.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<CampusSchedule> CampusSchedules { get; set; }

        /// <summary>
        /// Gets or sets the campus topics.
        /// </summary>
        /// <value>The campus topics.</value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<CampusTopic> CampusTopics { get; set; }

        /// <summary>
        /// Gets or sets the categories.
        /// </summary>
        /// <value>
        /// The categories.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<Category> Categories { get; set; }

        /// <summary>
        /// Gets or sets the communication attachments.
        /// </summary>
        /// <value>
        /// The communication attachments.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<Rock.Model.CommunicationAttachment> CommunicationAttachments { get; set; }

        /// <summary>
        /// Gets or sets the communications.
        /// </summary>
        /// <value>
        /// The communications.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<Rock.Model.Communication> Communications { get; set; }

        /// <summary>
        /// Gets or sets the communication recipients.
        /// </summary>
        /// <value>
        /// The communication recipients.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<CommunicationRecipient> CommunicationRecipients { get; set; }

        /// <summary>
        /// Gets or sets the communication response attachments.
        /// </summary>
        /// <value>
        /// The communication response attachments.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<CommunicationResponseAttachment> CommunicationResponseAttachments { get; set; }

        /// <summary>
        /// Gets or sets the communication responses.
        /// </summary>
        /// <value>
        /// The communication responses.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<CommunicationResponse> CommunicationResponses { get; set; }

        /// <summary>
        /// Gets or sets the communication template attachment.
        /// </summary>
        /// <value>
        /// The communication template attachment.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<Rock.Model.CommunicationTemplateAttachment> CommunicationTemplateAttachment { get; set; }

        /// <summary>
        /// Gets or sets the communication templates.
        /// </summary>
        /// <value>
        /// The communication templates.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<Rock.Model.CommunicationTemplate> CommunicationTemplates { get; set; }

        /// <summary>
        /// Gets or sets the connection activity types.
        /// </summary>
        /// <value>
        /// The connection activity types.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<ConnectionActivityType> ConnectionActivityTypes { get; set; }

        /// <summary>
        /// Gets or sets the connection opportunities.
        /// </summary>
        /// <value>
        /// The connection opportunities.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<ConnectionOpportunity> ConnectionOpportunities { get; set; }

        /// <summary>
        /// Gets or sets the connection opportunity group configs.
        /// </summary>
        /// <value>
        /// The connection opportunity group configs.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<Rock.Model.ConnectionOpportunityGroupConfig> ConnectionOpportunityGroupConfigs { get; set; }

        /// <summary>
        /// Gets or sets the connection opportunity campuses.
        /// </summary>
        /// <value>
        /// The connection opportunity campuses.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<ConnectionOpportunityCampus> ConnectionOpportunityCampuses { get; set; }

        /// <summary>
        /// Gets or sets the connection opportunity connector groups.
        /// </summary>
        /// <value>
        /// The connection opportunity connector groups.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<ConnectionOpportunityConnectorGroup> ConnectionOpportunityConnectorGroups { get; set; }

        /// <summary>
        /// Gets or sets the connection opportunity groups.
        /// </summary>
        /// <value>
        /// The connection opportunity groups.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<ConnectionOpportunityGroup> ConnectionOpportunityGroups { get; set; }

        /// <summary>
        /// Gets or sets the connection requests.
        /// </summary>
        /// <value>
        /// The connection requests.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<ConnectionRequest> ConnectionRequests { get; set; }

        /// <summary>
        /// Gets or sets the connection request activities.
        /// </summary>
        /// <value>
        /// The connection request activities.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<ConnectionRequestActivity> ConnectionRequestActivities { get; set; }

        /// <summary>
        /// Gets or sets the connection request workflows.
        /// </summary>
        /// <value>
        /// The connection request workflows.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<ConnectionRequestWorkflow> ConnectionRequestWorkflows { get; set; }

        /// <summary>
        /// Gets or sets the connection statuses.
        /// </summary>
        /// <value>
        /// The connection statuses.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<ConnectionStatus> ConnectionStatuses { get; set; }

        /// <summary>
        /// Gets or sets the connection status automations.
        /// </summary>
        /// <value>
        /// The connection automations.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<ConnectionStatusAutomation> ConnectionStatusAutomations { get; set; }

        /// <summary>
        /// Gets or sets the connection types.
        /// </summary>
        /// <value>
        /// The connection types.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<ConnectionType> ConnectionTypes { get; set; }

        /// <summary>
        /// Gets or sets the connection workflows.
        /// </summary>
        /// <value>
        /// The connection workflows.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<ConnectionWorkflow> ConnectionWorkflows { get; set; }

        /// <summary>
        /// Gets or sets the content channels.
        /// </summary>
        /// <value>
        /// The content channels.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<ContentChannel> ContentChannels { get; set; }

        /// <summary>
        /// Gets or sets the content channel items.
        /// </summary>
        /// <value>
        /// The content channel items.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<ContentChannelItem> ContentChannelItems { get; set; }

        /// <summary>
        /// Gets or sets the content channel item associations.
        /// </summary>
        /// <value>
        /// The content channel item associations.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<ContentChannelItemAssociation> ContentChannelItemAssociations { get; set; }

        /// <summary>
        /// Gets or sets the content channel item slugs.
        /// </summary>
        /// <value>
        /// The content channel item slugs.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<ContentChannelItemSlug> ContentChannelItemSlugs { get; set; }

        /// <summary>
        /// Gets or sets the content channel types.
        /// </summary>
        /// <value>
        /// The content channel types.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<ContentChannelType> ContentChannelTypes { get; set; }

        /// <summary>
        /// Gets or sets the content collections.
        /// </summary>
        /// <value>
        /// The ccontent collections.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<ContentCollection> ContentCollections { get; set; }

        /// <summary>
        /// Gets or sets the content collection sources.
        /// </summary>
        /// <value>
        /// The content collection sources.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<ContentCollectionSource> ContentCollectionSources { get; set; }

        /// <summary>
        /// Gets or sets the content topics.
        /// </summary>
        /// <value>
        /// The content topics.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<ContentTopic> ContentTopics { get; set; }

        /// <summary>
        /// Gets or sets the content topic domains.
        /// </summary>
        /// <value>
        /// The content topic domains.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<ContentTopicDomain> ContentTopicDomains { get; set; }

        /// <summary>
        /// Gets or sets the data views.
        /// </summary>
        /// <value>
        /// The data views.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<DataView> DataViews { get; set; }

        /// <summary>
        /// Gets or sets the data view filters.
        /// </summary>
        /// <value>
        /// The data view filters.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<DataViewFilter> DataViewFilters { get; set; }

        /// <summary>
        /// Gets or sets the data view persisted values.
        /// </summary>
        /// <value>
        /// The data view persisted values.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<DataViewPersistedValue> DataViewPersistedValues { get; set; }

        /// <summary>
        /// Gets or sets the document types.
        /// </summary>
        /// <value>
        /// The document types.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<DocumentType> DocumentTypes { get; set; }

        /// <summary>
        /// Gets or sets the documents.
        /// </summary>
        /// <value>
        /// The documents.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<Document> Documents { get; set; }

        /// <summary>
        /// Gets or sets the Defined Types.
        /// </summary>
        /// <value>
        /// the Defined Types.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<DefinedType> DefinedTypes { get; set; }

        /// <summary>
        /// Gets or sets the Defined Values.
        /// </summary>
        /// <value>
        /// the Defined Values.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<DefinedValue> DefinedValues { get; set; }

        /// <summary>
        /// Gets or sets the devices.
        /// </summary>
        /// <value>
        /// The devices.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<Device> Devices { get; set; }

        /// <summary>
        /// Gets or sets the entity campus filters.
        /// </summary>
        /// <value>
        /// The entity campus filters.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<EntityCampusFilter> EntityCampusFilters { get; set; }

        /// <summary>
        /// Gets or sets the entity sets.
        /// </summary>
        /// <value>
        /// The entity sets.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<EntitySet> EntitySets { get; set; }

        /// <summary>
        /// Gets or sets the entity set items.
        /// </summary>
        /// <value>
        /// The entity set items.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<EntitySetItem> EntitySetItems { get; set; }

        /// <summary>
        /// Gets or sets the entity types.
        /// </summary>
        /// <value>
        /// The entity types.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<EntityType> EntityTypes { get; set; }

        /// <summary>
        /// Gets or sets the event calendars.
        /// </summary>
        /// <value>
        /// The event calendars.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<EventCalendar> EventCalendars { get; set; }

        /// <summary>
        /// Gets or sets the event calendar content channels.
        /// </summary>
        /// <value>
        /// The event calendar content channels.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<EventCalendarContentChannel> EventCalendarContentChannels { get; set; }

        /// <summary>
        /// Gets or sets the event calendar items.
        /// </summary>
        /// <value>
        /// The event calendar items.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<EventCalendarItem> EventCalendarItems { get; set; }

        /// <summary>
        /// Gets or sets the event items.
        /// </summary>
        /// <value>
        /// The event items.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<EventItem> EventItems { get; set; }

        /// <summary>
        /// Gets or sets the event item audiences.
        /// </summary>
        /// <value>
        /// The event item audiences.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<EventItemAudience> EventItemAudiences { get; set; }

        /// <summary>
        /// Gets or sets the event item occurrences.
        /// </summary>
        /// <value>
        /// The event item occurrences.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<EventItemOccurrence> EventItemOccurrences { get; set; }

        /// <summary>
        /// Gets or sets the event item occurrence channel items.
        /// </summary>
        /// <value>
        /// The event item occurrence channel items.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<EventItemOccurrenceChannelItem> EventItemOccurrenceChannelItems { get; set; }

        /// <summary>
        /// Gets or sets the event item occurrence group maps.
        /// </summary>
        /// <value>
        /// The event item occurrence group maps.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<EventItemOccurrenceGroupMap> EventItemOccurrenceGroupMaps { get; set; }

        /// <summary>
        /// Gets or sets the Exception Logs.
        /// </summary>
        /// <value>
        /// the Exception Logs.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<ExceptionLog> ExceptionLogs { get; set; }

        /// <summary>
        /// Gets or sets the Field Types.
        /// </summary>
        /// <value>
        /// the Field Types.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<FieldType> FieldTypes { get; set; }

        /// <summary>
        /// Gets or sets the accounts.
        /// </summary>
        /// <value>
        /// The Financial Account.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<FinancialAccount> FinancialAccounts { get; set; }

        /// <summary>
        /// Gets or sets the batches.
        /// </summary>
        /// <value>
        /// The batches.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<FinancialBatch> FinancialBatches { get; set; }

        /// <summary>
        /// Gets or sets the financial gateways.
        /// </summary>
        /// <value>
        /// The financial gateways.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<FinancialGateway> FinancialGateways { get; set; }

        /// <summary>
        /// Gets or sets the financial payment details.
        /// </summary>
        /// <value>
        /// The financial payment details.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<FinancialPaymentDetail> FinancialPaymentDetails { get; set; }

        /// <summary>
        /// Gets or sets the financial person bank account.
        /// </summary>
        /// <value>
        /// The financial person bank account.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<FinancialPersonBankAccount> FinancialPersonBankAccounts { get; set; }

        /// <summary>
        /// Gets or sets the financial person saved account.
        /// </summary>
        /// <value>
        /// The financial person saved account.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<FinancialPersonSavedAccount> FinancialPersonSavedAccounts { get; set; }

        /// <summary>
        /// Gets or sets the pledges.
        /// </summary>
        /// <value>
        /// The pledges.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<FinancialPledge> FinancialPledges { get; set; }

        /// <summary>
        /// Gets or sets the financial scheduled transactions.
        /// </summary>
        /// <value>
        /// The financial scheduled transactions.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<FinancialScheduledTransaction> FinancialScheduledTransactions { get; set; }

        /// <summary>
        /// Gets or sets the financial scheduled transaction details.
        /// </summary>
        /// <value>
        /// The financial scheduled transaction details.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<FinancialScheduledTransactionDetail> FinancialScheduledTransactionDetails { get; set; }

        /// <summary>
        /// Gets or sets the financial statement templates.
        /// </summary>
        /// <value>
        /// The financial statement templates.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<FinancialStatementTemplate> FinancialStatementTemplates { get; set; }

        /// <summary>
        /// Gets or sets the transactions.
        /// </summary>
        /// <value>
        /// The transactions.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<FinancialTransaction> FinancialTransactions { get; set; }

        /// <summary>
        /// Gets or sets the financial transaction alert.
        /// </summary>
        /// <value>
        /// The financial transaction alert.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<FinancialTransactionAlert> FinancialTransactionAlerts { get; set; }

        /// <summary>
        /// Gets or sets the type of the financial transaction alert.
        /// </summary>
        /// <value>
        /// The type of the financial transaction alert.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<FinancialTransactionAlertType> FinancialTransactionAlertTypes { get; set; }

        /// <summary>
        /// Gets or sets the transaction details.
        /// </summary>
        /// <value>
        /// The transaction details.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<FinancialTransactionDetail> FinancialTransactionDetails { get; set; }

        /// <summary>
        /// Gets or sets the transaction details.
        /// </summary>
        /// <value>
        /// The transaction details.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<FinancialTransactionImage> FinancialTransactionImages { get; set; }

        /// <summary>
        /// Gets or sets the financial transaction refunds.
        /// </summary>
        /// <value>
        /// The financial transaction refunds.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<FinancialTransactionRefund> FinancialTransactionRefunds { get; set; }

        /// <summary>
        /// Gets or sets the followings.
        /// </summary>
        /// <value>
        /// The followings.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<Following> Followings { get; set; }

        /// <summary>
        /// Gets or sets the following event notifications.
        /// </summary>
        /// <value>
        /// The following event notifications.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<FollowingEventNotification> FollowingEventNotifications { get; set; }

        /// <summary>
        /// Gets or sets the following event subscriptions.
        /// </summary>
        /// <value>
        /// The following event subscriptions.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<FollowingEventSubscription> FollowingEventSubscriptions { get; set; }

        /// <summary>
        /// Gets or sets the following event types.
        /// </summary>
        /// <value>
        /// The following event types.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<FollowingEventType> FollowingEventTypes { get; set; }

        /// <summary>
        /// Gets or sets the following suggestions.
        /// </summary>
        /// <value>
        /// The following suggestions.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<FollowingSuggested> FollowingSuggesteds { get; set; }

        /// <summary>
        /// Gets or sets the following suggestion types.
        /// </summary>
        /// <value>
        /// The following suggestion types.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<FollowingSuggestionType> FollowingSuggestionTypes { get; set; }

        /// <summary>
        /// Gets or sets the Groups.
        /// </summary>
        /// <value>
        /// the Groups.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<Group> Groups { get; set; }

        /// <summary>
        /// Gets or sets the group demographic types.
        /// </summary>
        /// <value>
        /// The group demographic types.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<GroupDemographicType> GroupDemographicTypes { get; set; }

        /// <summary>
        /// Gets or sets the group demographic values.
        /// </summary>
        /// <value>
        /// The group demographic values.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<GroupDemographicValue> GroupDemographicValues { get; set; }

        /// <summary>
        /// Gets or sets the group historicals.
        /// </summary>
        /// <value>
        /// The group historicals.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<GroupHistorical> GroupHistoricals { get; set; }

        /// <summary>
        /// Gets or sets the Group Locations.
        /// </summary>
        /// <value>
        /// the Group Locations.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<GroupLocation> GroupLocations { get; set; }

        /// <summary>
        /// Gets or sets the group location historicals.
        /// </summary>
        /// <value>
        /// The group location historicals.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<GroupLocationHistorical> GroupLocationHistoricals { get; set; }

        /// <summary>
        /// Gets or sets the group location historical schedules.
        /// </summary>
        /// <value>
        /// The group location historical schedules.
        /// </value>
        [RockObsolete( "1.16" )]
        [Obsolete( "Consider using 'History' entity instead." )]
        public DbSet<GroupLocationHistoricalSchedule> GroupLocationHistoricalSchedules { get; set; }

        /// <summary>
        /// Gets or sets the group location schedule configs.
        /// </summary>
        /// <value>
        /// The group location schedule configs.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<GroupLocationScheduleConfig> GroupLocationScheduleConfigs { get; set; }

        /// <summary>
        /// Gets or sets the Members.
        /// </summary>
        /// <value>
        /// the Members.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<GroupMember> GroupMembers { get; set; }

        /// <summary>
        /// Gets or sets the group member assignments.
        /// </summary>
        /// <value>
        /// The group member assignments.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<GroupMemberAssignment> GroupMemberAssignments { get; set; }

        /// <summary>
        /// Gets or sets the group member historicals.
        /// </summary>
        /// <value>
        /// The group member historicals.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<GroupMemberHistorical> GroupMemberHistoricals { get; set; }

        /// <summary>
        /// Gets or sets the group member requirements.
        /// </summary>
        /// <value>
        /// The group member requirements.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<GroupMemberRequirement> GroupMemberRequirements { get; set; }

        /// <summary>
        /// Gets or sets the group member schedule templates.
        /// </summary>
        /// <value>
        /// The group member schedule templates.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<GroupMemberScheduleTemplate> GroupMemberScheduleTemplates { get; set; }

        /// <summary>
        /// Gets or sets the group member workflow triggers.
        /// </summary>
        /// <value>
        /// The group member workflow triggers.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<GroupMemberWorkflowTrigger> GroupMemberWorkflowTriggers { get; set; }

        /// <summary>
        /// Gets or sets the group requirements.
        /// </summary>
        /// <value>
        /// The group requirements.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<GroupRequirement> GroupRequirements { get; set; }

        /// <summary>
        /// Gets or sets the group requirement types.
        /// </summary>
        /// <value>
        /// The group requirement types.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<GroupRequirementType> GroupRequirementTypes { get; set; }

        /// <summary>
        /// Gets or sets the group schedule exclusions.
        /// </summary>
        /// <value>
        /// The group schedule exclusions.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<GroupScheduleExclusion> GroupScheduleExclusions { get; set; }

        /// <summary>
        /// Gets or sets the group syncs.
        /// </summary>
        /// <value>
        /// The group syncs.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<GroupSync> GroupSyncs { get; set; }

        /// <summary>
        /// Gets or sets the group type location types.
        /// </summary>
        /// <value>
        /// The group type location types.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<Rock.Model.GroupTypeLocationType> GroupTypeLocationTypes { get; set; }

        /// <summary>
        /// Gets or sets the Group Types.
        /// </summary>
        /// <value>
        /// the Group Types.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<GroupType> GroupTypes { get; set; }

        /// <summary>
        /// Gets or sets the Group Roles.
        /// </summary>
        /// <value>
        /// the Group Roles.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<GroupTypeRole> GroupTypeRoles { get; set; }

        /// <summary>
        /// Gets or sets the histories.
        /// </summary>
        /// <value>
        /// The histories.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<History> Histories { get; set; }

        /// <summary>
        /// Gets or sets the Html Contents.
        /// </summary>
        /// <value>
        /// the Html Contents.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<HtmlContent> HtmlContents { get; set; }

        /// <summary>
        /// Gets or sets the SMS actions.
        /// </summary>
        /// <value>
        /// The SMS actions.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<SmsAction> SmsActions { get; set; }

        /// <summary>
        /// Gets or sets the SMS pipeline.
        /// </summary>
        /// <value>
        /// The SMS pipelines.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<SmsPipeline> SmsPipelines { get; set; }

        /// <summary>
        /// Gets or sets the Interactive Experiences.
        /// </summary>
        /// <value>
        /// the Interactive Experiences.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<InteractiveExperience> InteractiveExperiences { get; set; }

        /// <summary>
        /// Gets or sets the Interactive Experience Actions.
        /// </summary>
        /// <value>
        /// the Interactive Experience Actions.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<InteractiveExperienceAction> InteractiveExperienceActions { get; set; }

        /// <summary>
        /// Gets or sets the Interactive Experience Answers.
        /// </summary>
        /// <value>
        /// the Interactive Experience Answers.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<InteractiveExperienceAnswer> InteractiveExperienceAnswers { get; set; }

        /// <summary>
        /// Gets or sets the Interactive Experience Occurrences.
        /// </summary>
        /// <value>
        /// the Interactive Experience Occurrences.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<InteractiveExperienceOccurrence> InteractiveExperienceOccurrences { get; set; }

        /// <summary>
        /// Gets or sets the Interactive Experience Schedules.
        /// </summary>
        /// <value>
        /// the Interactive Experience Schedules.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<InteractiveExperienceSchedule> InteractiveExperienceSchedules { get; set; }

        /// <summary>
        /// Gets or sets the Interactive Experience Campuses.
        /// </summary>
        /// <value>
        /// the Interactive Experience Campuses.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<InteractiveExperienceScheduleCampus> InteractiveExperienceScheduleCampuses { get; set; }

        /// <summary>
        /// Gets or sets the Interactions.
        /// </summary>
        /// <value>
        /// the Interactions.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<Interaction> Interactions { get; set; }

        /// <summary>
        /// Gets or sets the Interaction Components.
        /// </summary>
        /// <value>
        /// the Interaction Components.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<InteractionComponent> InteractionComponents { get; set; }

        /// <summary>
        /// Gets or sets the Interaction Device Types.
        /// </summary>
        /// <value>
        /// the Interaction Device Types.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<InteractionDeviceType> InteractionDeviceTypes { get; set; }

        /// <summary>
        /// Gets or sets the Interaction Services.
        /// </summary>
        /// <value>
        /// the Interaction Services.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<InteractionChannel> InteractionServices { get; set; }

        /// <summary>
        /// Gets or sets the Interaction Sessions.
        /// </summary>
        /// <value>
        /// the Interaction Sessions.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<InteractionSession> InteractionSessions { get; set; }

        /// <summary>
        /// Gets or sets the Interaction Session Locations.
        /// </summary>
        /// <value>
        /// the Interaction Sessions Locations.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<InteractionSessionLocation> InteractionSessionLocations { get; set; }

        /// <summary>
        /// Gets or sets the lava shortcodes.
        /// </summary>
        /// <value>
        /// The lava shortcodes.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<Rock.Model.LavaShortcode> LavaShortcodes { get; set; }

        /// <summary>
        /// Gets or sets the layouts.
        /// </summary>
        /// <value>
        /// The layouts.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<Layout> Layouts { get; set; }

        /// <summary>
        /// Gets or sets the Location.
        /// </summary>
        /// <value>
        /// the Location.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<Location> Locations { get; set; }

        /// <summary>
        /// Gets or sets the media accounts.
        /// </summary>
        /// <value>
        /// The media accounts.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<MediaAccount> MediaAccounts { get; set; }

        /// <summary>
        /// Gets or sets the media elements.
        /// </summary>
        /// <value>
        /// The media elements.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<MediaElement> MediaElements { get; set; }

        /// <summary>
        /// Gets or sets the media folders.
        /// </summary>
        /// <value>
        /// The media folders.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<MediaFolder> MediaFolders { get; set; }

        /// <summary>
        /// Gets or sets the merge templates.
        /// </summary>
        /// <value>
        /// The merge templates.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<MergeTemplate> MergeTemplates { get; set; }

        /// <summary>
        /// Gets or sets the meta first name gender lookups.
        /// </summary>
        /// <value>
        /// The meta first name gender lookups.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<Rock.Model.MetaFirstNameGenderLookup> MetaFirstNameGenderLookups { get; set; }

        /// <summary>
        /// Gets or sets the meta last name lookups.
        /// </summary>
        /// <value>
        /// The meta last name lookups.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<Rock.Model.MetaLastNameLookup> MetaLastNameLookups { get; set; }

        /// <summary>
        /// Gets or sets the meta nick name lookups.
        /// </summary>
        /// <value>
        /// The meta nick name lookups.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<Rock.Model.MetaNickNameLookup> MetaNickNameLookups { get; set; }

        /// <summary>
        /// Gets or sets the metaphones.
        /// </summary>
        /// <value>
        /// The metaphones.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<Metaphone> Metaphones { get; set; }

        /// <summary>
        /// Gets or sets the Defined Types.
        /// </summary>
        /// <value>
        /// the Defined Types.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<Metric> Metrics { get; set; }

        /// <summary>
        /// Gets or sets the metric partitions.
        /// </summary>
        /// <value>
        /// The metric partitions.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<MetricPartition> MetricPartitions { get; set; }

        /// <summary>
        /// Gets or sets the metric categories.
        /// </summary>
        /// <value>
        /// The metric categories.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<MetricCategory> MetricCategories { get; set; }

        /// <summary>
        /// Gets or sets the Defined Values.
        /// </summary>
        /// <value>
        /// the Defined Values.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<MetricValue> MetricValues { get; set; }

        /// <summary>
        /// Gets or sets the metric value partitions.
        /// </summary>
        /// <value>
        /// The metric value partitions.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<MetricValuePartition> MetricValuePartitions { get; set; }

        /// <summary>
        /// Gets or sets the ncoa history.
        /// </summary>
        /// <value>
        /// The ncoa historys.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<NcoaHistory> NcoaHistorys { get; set; }

        /// <summary>
        /// Gets or sets the notes.
        /// </summary>
        /// <value>
        /// The notes.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<Note> Notes { get; set; }

        /// <summary>
        /// Gets or sets the note attachments.
        /// </summary>
        /// <value>
        /// The note attachments.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<NoteAttachment> NoteAttachments { get; set; }

        /// <summary>
        /// Gets or sets the note types.
        /// </summary>
        /// <value>
        /// The note types.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<NoteType> NoteTypes { get; set; }

        /// <summary>
        /// Gets or sets the note watches.
        /// </summary>
        /// <value>
        /// The note watches.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<NoteWatch> NoteWatches { get; set; }

        /// <summary>
        /// Gets or sets the notification message types.
        /// </summary>
        /// <value>The notification message types.</value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<NotificationMessageType> NotificationMessageTypes { get; set; }

        /// <summary>
        /// Gets or sets the notification messages.
        /// </summary>
        /// <value>The notification messages.</value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<NotificationMessage> NotificationMessages { get; set; }

        /// <summary>
        /// Gets or sets the notifications.
        /// </summary>
        /// <value>
        /// The notifications.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<Notification> Notifications { get; set; }

        /// <summary>
        /// Gets or sets the notification recipients.
        /// </summary>
        /// <value>
        /// The notification recipients.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<NotificationRecipient> NotificationRecipients { get; set; }

        /// <summary>
        /// Gets or sets the Pages.
        /// </summary>
        /// <value>
        /// the Pages.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<Page> Pages { get; set; }

        /// <summary>
        /// Gets or sets the page contexts.
        /// </summary>
        /// <value>
        /// The page contexts.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<PageContext> PageContexts { get; set; }

        /// <summary>
        /// Gets or sets the Page Routes.
        /// </summary>
        /// <value>
        /// the Page Routes.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<PageRoute> PageRoutes { get; set; }

        /// <summary>
        /// Gets or sets the persisted datasets.
        /// </summary>
        /// <value>
        /// The persisted datasets.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<PersistedDataset> PersistedDatasets { get; set; }

        /// <summary>
        /// Gets or sets the People.
        /// </summary>
        /// <value>
        /// the People.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<Person> People { get; set; }

        /// <summary>
        /// Gets or sets the personal devices.
        /// </summary>
        /// <value>
        /// The personal devices.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<Rock.Model.PersonalDevice> PersonalDevices { get; set; }

        /// <summary>
        /// Gets or sets the Person Aliases.
        /// </summary>
        /// <value>
        /// the Person aliases.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<PersonAlias> PersonAliases { get; set; }

        /// <summary>
        /// Gets or sets the Person Preferences.
        /// </summary>
        /// <value>
        /// The Person Preferences.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<PersonPreference> PersonPreferences { get; set; }

        /// <summary>
        /// Gets or sets the badges.
        /// </summary>
        /// <value>
        /// The person badge types.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<Model.Badge> Badges { get; set; }

        /// <summary>
        /// Gets or sets the person duplicates.
        /// </summary>
        /// <value>
        /// The person duplicates.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<PersonDuplicate> PersonDuplicates { get; set; }

        /// <summary>
        /// Gets or sets the person previous names.
        /// </summary>
        /// <value>
        /// The person previous names.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<PersonPreviousName> PersonPreviousNames { get; set; }

        /// <summary>
        /// Gets or sets the person schedule exclusions.
        /// </summary>
        /// <value>
        /// The person schedule exclusions.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<PersonScheduleExclusion> PersonScheduleExclusions { get; set; }

        /// <summary>
        /// Gets or sets the Person Signals.
        /// </summary>
        /// <value>
        /// the Person Vieweds.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<PersonSignal> PersonSignals { get; set; }

        /// <summary>
        /// Gets or sets the Person Vieweds.
        /// </summary>
        /// <value>
        /// the Person Vieweds.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<PersonViewed> PersonVieweds { get; set; }

        /// <summary>
        /// Gets or sets the person search keys.
        /// </summary>
        /// <value>
        /// The person search keys.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<PersonSearchKey> PersonSearchKeys { get; set; }

        /// <summary>
        /// Gets or sets the person tokens.
        /// </summary>
        /// <value>
        /// The person tokens.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<PersonToken> PersonTokens { get; set; }

        /// <summary>
        /// Gets or sets the Phone Numbers.
        /// </summary>
        /// <value>
        /// the Phone Numbers.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<PhoneNumber> PhoneNumbers { get; set; }

        /// <summary>
        /// Gets or sets the phone verifications.
        /// </summary>
        /// <value>
        /// The phone verifications.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<IdentityVerification> IdentityVerifications { get; set; }

        /// <summary>
        /// Gets or sets the phone verification codes.
        /// </summary>
        /// <value>
        /// The phone verification codes.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<IdentityVerificationCode> IdentityVerificationCodes { get; set; }

        /// <summary>
        /// Gets or sets the personal links.
        /// </summary>
        /// <value>
        /// The personal links.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<PersonalLink> PersonalLinks { get; set; }

        /// <summary>
        /// Gets or sets the personal link sections.
        /// </summary>
        /// <value>
        /// The personal link sections.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<PersonalLinkSection> PersonalLinkSections { get; set; }

        /// <summary>
        /// Gets or sets the personal link section orders.
        /// </summary>
        /// <value>
        /// The personal link section orders.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<PersonalLinkSectionOrder> PersonalLinkSectionOrders { get; set; }

        /// <summary>
        /// Gets or sets the plugin migrations.
        /// </summary>
        /// <value>
        /// The plugin migrations.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<PluginMigration> PluginMigrations { get; set; }

        /// <summary>
        /// Gets or sets the prayer requests.
        /// </summary>
        /// <value>
        /// The prayer requests.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<PrayerRequest> PrayerRequests { get; set; }

        /// <summary>
        /// Gets or sets the registrations.
        /// </summary>
        /// <value>
        /// The registrations.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<Registration> Registrations { get; set; }

        /// <summary>
        /// Gets or sets the registration instances.
        /// </summary>
        /// <value>
        /// The registration instances.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<RegistrationInstance> RegistrationInstances { get; set; }

        /// <summary>
        /// Gets or sets the registration registrants.
        /// </summary>
        /// <value>
        /// The registration registrants.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<RegistrationRegistrant> RegistrationRegistrants { get; set; }

        /// <summary>
        /// Gets or sets the registration registrant fees.
        /// </summary>
        /// <value>
        /// The registration registrant fees.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<RegistrationRegistrantFee> RegistrationRegistrantFees { get; set; }

        /// <summary>
        /// Gets or sets the registration sessions.
        /// </summary>
        /// <value>
        /// The registration sessions.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<RegistrationSession> RegistrationSessions { get; set; }

        /// <summary>
        /// Gets or sets the registration templates.
        /// </summary>
        /// <value>
        /// The registration templates.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<RegistrationTemplate> RegistrationTemplates { get; set; }

        /// <summary>
        /// Gets or sets the registration template discounts.
        /// </summary>
        /// <value>
        /// The registration template discounts.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<RegistrationTemplateDiscount> RegistrationTemplateDiscounts { get; set; }

        /// <summary>
        /// Gets or sets the registration template fees.
        /// </summary>
        /// <value>
        /// The registration template fees.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<RegistrationTemplateFee> RegistrationTemplateFees { get; set; }

        /// <summary>
        /// Gets or sets the registration template fee items.
        /// </summary>
        /// <value>
        /// The registration template fee items.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<RegistrationTemplateFeeItem> RegistrationTemplateFeeItems { get; set; }

        /// <summary>
        /// Gets or sets the registration template forms.
        /// </summary>
        /// <value>
        /// The registration template forms.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<RegistrationTemplateForm> RegistrationTemplateForms { get; set; }

        /// <summary>
        /// Gets or sets the registration template form fields.
        /// </summary>
        /// <value>
        /// The registration template form fields.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<RegistrationTemplateFormField> RegistrationTemplateFormFields { get; set; }

        /// <summary>
        /// Gets or sets the registration template placements.
        /// </summary>
        /// <value>
        /// The registration template placements.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<RegistrationTemplatePlacement> RegistrationTemplatePlacements { get; set; }

        /// <summary>
        /// Gets or sets the related entities.
        /// </summary>
        /// <value>
        /// The related entities.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<RelatedEntity> RelatedEntities { get; set; }

        /// <summary>
        /// Gets or sets the reminders.
        /// </summary>
        /// <value>
        /// The reminders.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<Reminder> Reminders { get; set; }

        /// <summary>
        /// Gets or sets the reminder types.
        /// </summary>
        /// <value>
        /// The reminder types.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<ReminderType> ReminderTypes { get; set; }

        /// <summary>
        /// Gets or sets the remote authentication sessions.
        /// </summary>
        /// <value>
        /// The remote authentication sessions.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<RemoteAuthenticationSession> RemoteAuthenticationSessions { get; set; }

        /// <summary>
        /// Gets or sets the reports.
        /// </summary>
        /// <value>
        /// The reports.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<Report> Reports { get; set; }

        /// <summary>
        /// Gets or sets the report fields.
        /// </summary>
        /// <value>
        /// The report fields.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<ReportField> ReportFields { get; set; }

        /// <summary>
        /// Gets or sets the REST Actions.
        /// </summary>
        /// <value>
        /// The reports.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<RestAction> RestActions { get; set; }

        /// <summary>
        /// Gets or sets the REST Controllers.
        /// </summary>
        /// <value>
        /// The reports.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<RestController> RestControllers { get; set; }

        /// <summary>
        /// Gets or sets the schedule category exclusions.
        /// </summary>
        /// <value>
        /// The schedule category exclusions.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<Rock.Model.ScheduleCategoryExclusion> ScheduleCategoryExclusions { get; set; }

        /// <summary>
        /// Gets or sets the schedules.
        /// </summary>
        /// <value>
        /// The schedules.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<Schedule> Schedules { get; set; }

        /// <summary>
        /// Gets or sets the service job histories.
        /// </summary>
        /// <value>
        /// The service job histories.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<ServiceJobHistory> ServiceJobHistories { get; set; }

        /// <summary>
        /// Gets or sets the Jobs.
        /// </summary>
        /// <value>
        /// the Jobs.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<ServiceJob> ServiceJobs { get; set; }

        /// <summary>
        /// Gets or sets the Service Logs.
        /// </summary>
        /// <value>
        /// the Service Logs.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<ServiceLog> ServiceLogs { get; set; }

        /// <summary>
        /// Gets or sets the signature documents.
        /// </summary>
        /// <value>
        /// The signature documents.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<SignalType> SignalTypes { get; set; }

        /// <summary>
        /// Gets or sets the signature documents.
        /// </summary>
        /// <value>
        /// The signature documents.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<SignatureDocument> SignatureDocuments { get; set; }

        /// <summary>
        /// Gets or sets the signature document templates.
        /// </summary>
        /// <value>
        /// The signature document templates.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<SignatureDocumentTemplate> SignatureDocumentTemplates { get; set; }

        /// <summary>
        /// Gets or sets the Sites.
        /// </summary>
        /// <value>
        /// the Sites.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<Site> Sites { get; set; }

        /// <summary>
        /// Gets or sets the Site Domains.
        /// </summary>
        /// <value>
        /// the Site Domains.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<SiteDomain> SiteDomains { get; set; }

        /// <summary>
        /// Gets or sets the steps.
        /// </summary>
        /// <value>
        /// The steps.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<Step> Steps { get; set; }

        /// <summary>
        /// Gets or sets the step programs.
        /// </summary>
        /// <value>
        /// The step programs.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<StepProgram> StepPrograms { get; set; }

        /// <summary>
        /// Gets or sets the step program completions.
        /// </summary>
        /// <value>
        /// The step program completions.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<StepProgramCompletion> StepProgramCompletions { get; set; }

        /// <summary>
        /// Gets or sets the step statuses.
        /// </summary>
        /// <value>
        /// The step statuses.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<StepStatus> StepStatuses { get; set; }

        /// <summary>
        /// Gets or sets the step types.
        /// </summary>
        /// <value>
        /// The step types.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<StepType> StepTypes { get; set; }

        /// <summary>
        /// Gets or sets the step type prerequisites.
        /// </summary>
        /// <value>
        /// The step type prerequisites.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<StepTypePrerequisite> StepTypePrerequisites { get; set; }

        /// <summary>
        /// Gets or sets the step workflows.
        /// </summary>
        /// <value>
        /// The step workflows.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<StepWorkflow> StepWorkflows { get; set; }

        /// <summary>
        /// Gets or sets the step workflow triggers.
        /// </summary>
        /// <value>
        /// The step workflow triggers.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<StepWorkflowTrigger> StepWorkflowTriggers { get; set; }

        /// <summary>
        /// Gets or sets the site URL maps.
        /// </summary>
        /// <value>
        /// The site URL maps.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<PageShortLink> PageShortLinks { get; set; }

        /// <summary>
        /// Gets or sets the Streaks.
        /// </summary>
        /// <value>
        /// The Streaks.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<Streak> Streaks { get; set; }

        /// <summary>
        /// Gets or sets the StreakTypes.
        /// </summary>
        /// <value>
        /// The StreakTypes.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<StreakType> StreakTypes { get; set; }

        /// <summary>
        /// Gets or sets the StreakTypeExclusions.
        /// </summary>
        /// <value>
        /// The StreakTypeExclusions.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<StreakTypeExclusion> StreakTypeExclusions { get; set; }

        /// <summary>
        /// Gets or sets the system emails.
        /// </summary>
        /// <value>
        /// The system emails.
        /// </value>
        [RockObsolete( "1.10" )]
        [Obsolete( "Use SystemCommunications instead.", true )]
        public DbSet<SystemEmail> SystemEmails { get; set; }

        /// <summary>
        /// Gets or sets the system emails.
        /// </summary>
        /// <value>
        /// The system emails.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<SystemCommunication> SystemCommunications { get; set; }

        /// <summary>
        /// Gets or sets the system phone numbers.
        /// </summary>
        /// <value>
        /// The system phone numbers.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<SystemPhoneNumber> SystemPhoneNumbers { get; set; }

        /// <summary>
        /// Gets or sets the Tags.
        /// </summary>
        /// <value>
        /// the Tags.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<Tag> Tags { get; set; }

        /// <summary>
        /// Gets or sets the Tagged Items.
        /// </summary>
        /// <value>
        /// the Tagged Items.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<TaggedItem> TaggedItems { get; set; }

        /// <summary>
        /// Gets or sets the Users.
        /// </summary>
        /// <value>
        /// the Users.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<UserLogin> UserLogins { get; set; }

        /// <summary>
        /// Gets or sets the web farm nodes.
        /// </summary>
        /// <value>
        /// The web farm nodes.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<WebFarmNode> WebFarmNodes { get; set; }

        /// <summary>
        /// Gets or sets the web farm node metrics.
        /// </summary>
        /// <value>
        /// The web farm node metrics.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<WebFarmNodeMetric> WebFarmNodeMetrics { get; set; }

        /// <summary>
        /// Gets or sets the web farm node logs.
        /// </summary>
        /// <value>
        /// The web farm node logs.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<WebFarmNodeLog> WebFarmNodeLogs { get; set; }

        /// <summary>
        /// Gets or sets the workflows.
        /// </summary>
        /// <value>
        /// The workflows.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<Rock.Model.Workflow> Workflows { get; set; }

        /// <summary>
        /// Gets or sets the workflow actions.
        /// </summary>
        /// <value>
        /// The workflow actions.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<WorkflowAction> WorkflowActions { get; set; }

        /// <summary>
        /// Gets or sets the workflow action forms.
        /// </summary>
        /// <value>
        /// The workflow action forms.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<WorkflowActionForm> WorkflowActionForms { get; set; }

        /// <summary>
        /// Gets or sets the workflow action form attributes.
        /// </summary>
        /// <value>
        /// The workflow action form attributes.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<WorkflowActionFormAttribute> WorkflowActionFormAttributes { get; set; }

        /// <summary>
        /// Gets or sets the workflow action types.
        /// </summary>
        /// <value>
        /// The workflow action types.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<WorkflowActionType> WorkflowActionTypes { get; set; }

        /// <summary>
        /// Gets or sets the workflow activities.
        /// </summary>
        /// <value>
        /// The workflow activities.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<WorkflowActivity> WorkflowActivities { get; set; }

        /// <summary>
        /// Gets or sets the workflow form builder templates.
        /// </summary>
        /// <value>
        /// The workflow form builder templates.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<WorkflowFormBuilderTemplate> WorkflowFormBuilderTemplates { get; set; }

        /// <summary>
        /// Gets or sets the workflow action form sections.
        /// </summary>
        /// <value>
        /// The workflow action form sections.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<WorkflowActionFormSection> WorkflowActionFormSections { get; set; }

        /// <summary>
        /// Gets or sets the workflow activity types.
        /// </summary>
        /// <value>
        /// The workflow activity types.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<WorkflowActivityType> WorkflowActivityTypes { get; set; }

        /// <summary>
        /// Gets or sets the workflow logs.
        /// </summary>
        /// <value>
        /// The workflow logs.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<WorkflowLog> WorkflowLogs { get; set; }

        /// <summary>
        /// Gets or sets the workflow triggers.
        /// </summary>
        /// <value>
        /// The entity type workflow triggers.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<WorkflowTrigger> WorkflowTriggers { get; set; }

        /// <summary>
        /// Gets or sets the workflow types.
        /// </summary>
        /// <value>
        /// The workflow types.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<WorkflowType> WorkflowTypes { get; set; }

        /// <summary>
        /// Gets or sets the person alias personalizations.
        /// </summary>
        /// <value>
        /// The person alias personalizations.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<PersonAliasPersonalization> PersonAliasPersonalizations { get; set; }

        /// <summary>
        /// Gets or sets the personalized entities.
        /// </summary>
        /// <value>
        /// The personalized entities.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<PersonalizedEntity> PersonalizedEntities { get; set; }

        /// <summary>
        /// Gets or sets the request filters.
        /// </summary>
        /// <value>
        /// The request filters.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<RequestFilter> RequestFilters { get; set; }

        /// <summary>
        /// Gets or sets the segments.
        /// </summary>
        /// <value>
        /// The segments.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<PersonalizationSegment> Segments { get; set; }

        /// <summary>
        /// Gets or sets the snippets.
        /// </summary>
        /// <value>
        /// The snippets.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<Snippet> Snippets { get; set; }

        /// <summary>
        /// Gets or sets the snippet types.
        /// </summary>
        /// <value>
        /// The snippet types.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<SnippetType> SnippetTypes { get; set; }

        /// <summary>
        /// Gets or sets the analytics source zip codes.
        /// </summary>
        /// <value>
        /// The analytics source zip codes.
        /// </value>
        [RockObsolete( "1.16.4" )]
        [Obsolete( "Use service instance instead." )]
        public DbSet<AnalyticsSourcePostalCode> AnalyticsSourcePostalCodes { get; set; }

        #endregion

        /// <summary>
        /// Use SqlBulkInsert to quickly insert a large number records.
        /// NOTE: This bypasses the Rock and a bunch of the EF Framework and automatically commits the changes to the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="records">The records.</param>
        /// <param name="useSqlBulkCopy">if set to <c>true</c> [use SQL bulk copy].</param>
        public void BulkInsert<T>( IEnumerable<T> records, bool useSqlBulkCopy ) where T : class, IEntity
        {
            if ( useSqlBulkCopy )
            {
                this.BulkInsert( records );
            }
            else
            {
                this.Configuration.ValidateOnSaveEnabled = false;
                this.Set<T>().AddRange( records );
                this.SaveChanges( true );
            }
        }

        /// <summary>
        /// This method is called when the context has been initialized, but
        /// before the model has been locked down and used to initialize the context.
        /// </summary>
        /// <param name="modelBuilder">The builder that defines the model for the context being created.</param>
        protected override void OnModelCreating( DbModelBuilder modelBuilder )
        {
            ContextHelper.AddConfigurations( modelBuilder );

            try
            {
                //// dynamically add plugin entities so that queryables can use a mixture of entities from different plugins and core
                //// from http://romiller.com/2012/03/26/dynamically-building-a-model-with-code-first/, but using the new RegisterEntityType in 6.1.3

                // look for IRockStoreModelConvention classes
                var modelConventionList = Reflection.FindTypes( typeof( Rock.Data.IRockStoreModelConvention<System.Data.Entity.Core.Metadata.Edm.EdmModel> ) )
                    .Where( a => !a.Value.IsAbstract )
                    .OrderBy( a => a.Key ).Select( a => a.Value );

                foreach ( var modelConventionType in modelConventionList )
                {
                    var convention = ( IConvention ) Activator.CreateInstance( modelConventionType );
                    modelBuilder.Conventions.Add( convention );
                }

                // look for IRockEntity classes
                var entityTypeList = Reflection.FindTypes( typeof( Rock.Data.IRockEntity ) )
                    .Where( a => !a.Value.IsAbstract && ( a.Value.GetCustomAttribute<NotMappedAttribute>() == null ) && ( a.Value.GetCustomAttribute<System.Runtime.Serialization.DataContractAttribute>() != null ) )
                    .OrderBy( a => a.Key ).Select( a => a.Value );

                foreach ( var entityType in entityTypeList )
                {
                    try
                    {
                        modelBuilder.RegisterEntityType( entityType );
                    }
                    catch ( Exception ex )
                    {
                        ExceptionLogService.LogException( new Exception( $"Exception occurred when Registering Entity Type {entityType} to RockContext", ex ), null );
                    }
                }

                // add configurations that might be in plugin assemblies
                foreach ( var assembly in entityTypeList.Select( a => a.Assembly ).Distinct() )
                {
                    try
                    {
                        modelBuilder.Configurations.AddFromAssembly( assembly );
                    }
                    catch ( Exception ex )
                    {
                        ExceptionLogService.LogException( new Exception( $"Exception occurred when adding Plugin Entity Configurations from {assembly} to RockContext", ex ), null );
                    }
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( new Exception( "Exception occurred when adding Plugin Entities to RockContext", ex ), null );
            }
        }
    }

    /// <summary>
    /// Enum for determining the level of query metrics to capture.
    /// </summary>
    public enum QueryMetricDetailLevel
    {
        /// <summary>
        /// No metrics will be captured (default)
        /// </summary>
        Off = 0,

        /// <summary>
        /// Just the number of queries will be captured.
        /// </summary>
        Count = 1,

        /// <summary>
        /// All metrics will the captured (count and a copy of the SQL)
        /// </summary>
        Full = 2
    }

    /// <summary>
    /// POCO for storing metrics about the context
    /// </summary>
    public class QueryMetricDetail
    {
        /// <summary>
        /// Gets or sets the SQL.
        /// </summary>
        /// <value>
        /// The SQL.
        /// </value>
        public string Sql { get; set; }

        /// <summary>
        /// Gets or sets the duration in ticks (not fleas).
        /// </summary>
        /// <value>
        /// The duration.
        /// </value>
        public long Duration { get; set; }

        /// <summary>
        /// Gets or sets the server.
        /// </summary>
        /// <value>
        /// The server.
        /// </value>
        public string Server { get; set; }

        /// <summary>
        /// Gets or sets the database.
        /// </summary>
        /// <value>
        /// The database.
        /// </value>
        public string Database { get; set; }
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
            modelBuilder.Conventions.Add<DecimalPrecisionAttributeConvention>();
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Configurations.AddFromAssembly( typeof( RockContext ).Assembly );
        }
    }
}