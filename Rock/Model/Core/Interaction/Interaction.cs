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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;
using Rock.Lava;

namespace Rock.Model
{
    /// <summary>
    /// Represents a interaction <see cref="Rock.Model.Interaction"/>.
    /// </summary>
    [RockDomain( "Core" )]
    [NotAudited]
    [Table( "Interaction" )]
    [DataContract]
    public partial class Interaction : Model<Interaction>
    {
        /* Custom Indexes:
         *      
         *  InteractionComponentId
         *      This was added because InteractionComponent deletes have a FK reference that was not indexed.
         *      Mainly used in delete operations that also delete interaction references.
         *      See FK_dbo.Interaction_dbo.InteractionComponent_InteractionComponentId
         *      
         * InteractionComponentId, InteractionDateTime
         *      Includes InteractionTimeToServe, Operation, InteractionSessionId, PersonalDeviceId
         *      This was added for <see cref="Rock.Jobs.RockCleanup.UpdateMedianPageLoadTimes"/>
         *          and CleanupOldInteractions
         *
         *  InteractionDateTime
         *      Includes InteractionComponentId, PersonAliasId
         *
         *  PersonAliasId, InteractionSessionId
         *      Includes InteractionDateTime, InteractionComponentId
         *      This was added for RockWeb.Blocks.Reporting.InteractionSessionList
         *      
         */

        #region Entity Properties

        /// <summary>
        /// Gets or sets the interaction datetime.
        /// </summary>
        /// <value>
        /// The interaction datetime.
        /// </value>
        [DataMember]
        public DateTime InteractionDateTime { get; set; }

        /// <summary>
        /// Gets or sets the operation. For example: 'Viewed', 'Opened', 'Click', 'Prayed', 'Form Viewed', 'Form Completed'
        /// </summary>
        /// <value>
        /// The operation.
        /// </value>
        [DataMember]
        [MaxLength( 25 )]
        public string Operation { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.InteractionComponent"/> Component that is associated with this Interaction.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.InteractionComponent"/> component that this Interaction is associated with.
        /// </value>
        [DataMember( IsRequired = true )]
        [Required]
        public int InteractionComponentId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the entity that this interaction component is tracking activity for.
        /// <list type="bullet">
        /// <item>
        ///     <term>Page Views</term>
        ///     <description>null, Page is the Component, Site is the Channel</description></item>
        /// <item>
        ///     <term>Communication Recipient Activity</term>
        ///     <description><see cref="Rock.Model.CommunicationRecipient" /> Id. Communication is the Component, single Channel</description></item>
        /// <item>
        ///     <term>Content Channel Activity</term>
        ///     <description>null, ContentChannel is the Component, single Channel</description></item>
        /// <item>
        ///     <term>Workflow Form Entry</term>
        ///     <description><see cref="Workflow"/> Id, WorkflowType is the Component, single Channel </description></item>
        /// </list>
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the entity (object) that this interaction component is related to.
        /// </value>
        [DataMember]
        public int? EntityId { get; set; }

        /// <summary>
        /// Gets or sets the related entity type identifier.
        /// </summary>
        /// <value>
        /// The related entity type identifier.
        /// </value>
        [DataMember]
        public int? RelatedEntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the related entity identifier.
        /// </summary>
        /// <value>
        /// The related entity identifier.
        /// </value>
        [DataMember]
        public int? RelatedEntityId { get; set; }

        /// <summary>
        /// Gets or sets the person alias identifier.
        /// </summary>
        /// <value>
        /// The person alias identifier.
        /// </value>
        [DataMember]
        public int? PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.InteractionSession"/> Session that that is associated with this Interaction.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.InteractionSession"/> session that this Interaction is associated with.
        /// </value>
        [DataMember]
        public int? InteractionSessionId { get; set; }

        /// <summary>
        /// Gets or sets the interaction summary.
        /// </summary>
        /// <value>
        /// The interaction summary.
        /// </value>
        [DataMember]
        [MaxLength( 500 )]
        public string InteractionSummary { get; set; }

        /// <summary>
        /// Gets or sets the interaction data.
        /// </summary>
        /// <value>
        /// The interaction data.
        /// </value>
        [DataMember]
        public string InteractionData { get; set; }

        /// <summary>
        /// Gets or sets the personal device identifier.
        /// </summary>
        /// <value>
        /// The personal device identifier.
        /// </value>
        [DataMember]
        public int? PersonalDeviceId { get; set; }

        /// <summary>
        /// Gets or sets the interaction end date time.
        /// </summary>
        /// <value>
        /// The interaction end date time.
        /// </value>
        [DataMember]
        public DateTime? InteractionEndDateTime { get; set; }

        /// <summary>
        /// Gets or sets the channel custom 1.
        /// </summary>
        /// <value>
        /// The channel custom 1.
        /// </value>
        [DataMember]
        [MaxLength( 500 )]
        public string ChannelCustom1 { get; set; }

        /// <summary>
        /// Gets or sets the channel custom 2.
        /// </summary>
        /// <value>
        /// The channel custom 2.
        /// </value>
        [DataMember]
        [MaxLength( 2000 )]
        public string ChannelCustom2 { get; set; }

        /// <summary>
        /// Gets or sets the channel custom indexed 1.
        /// </summary>
        /// <value>
        /// The channel custom indexed 1.
        /// </value>
        [DataMember]
        [MaxLength( 500 )]
        [Index( "IX_ChannelCustomIndexed1" )]
        public string ChannelCustomIndexed1 { get; set; }

        /// <summary>
        /// Gets or sets the length of time (or percent of time) of the interaction.
        /// The units on this depend on the InteractionChannel, which might have this be a Percent, Days, Seconds, Minutes, etc
        /// For example, if this interaction type is watching a video, this might be what percent of the video they watched
        /// </summary>
        /// <value>
        /// The length of the interaction.
        /// </value>
        [DataMember]
        public double? InteractionLength { get; set; }

        /// <summary>
        /// Gets or sets the interaction time to serve.
        /// The units on this depend on the InteractionChannel, which might have this be a Percent, Days, Seconds, Minutes, etc.
        /// For example, if this is a page view, this would be how long (in seconds) it took for Rock to generate a response.
        /// </summary>
        /// <value>
        /// The interaction time to serve.
        /// </value>
        [DataMember]
        public double? InteractionTimeToServe { get; set; }

        #endregion

        #region Campaign Meta fields

        /// <summary>
        /// Gets or sets the campaign source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        [DataMember]
        [MaxLength( 25 )]
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the campaign medium.
        /// </summary>
        /// <value>
        /// The medium.
        /// </value>
        [DataMember]
        [MaxLength( 25 )]
        public string Medium { get; set; }

        /// <summary>
        /// Gets or sets the campaign name
        /// </summary>
        /// <value>
        /// The campaign.
        /// </value>
        [DataMember]
        [MaxLength( 50 )]
        public string Campaign { get; set; }

        /// <summary>
        /// Gets or sets the campaign content.
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        [DataMember]
        [MaxLength( 50 )]
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the term(s).
        /// </summary>
        /// <value>
        /// The term.
        /// </value>
        [DataMember]
        [MaxLength( 50 )]
        public string Term { get; set; }

        /// <summary>
        /// Gets the interaction date key.
        /// </summary>
        /// <value>
        /// The interaction date key.
        /// </value>
        [DataMember]
        [FieldType( Rock.SystemGuid.FieldType.DATE )]
        public int InteractionDateKey
        {
            get => InteractionDateTime.ToString( "yyyyMMdd" ).AsInteger();
            private set { }
        }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the interaction component.
        /// </summary>
        /// <value>
        /// The interaction component.
        /// </value>
        [DataMember]
        public virtual InteractionComponent InteractionComponent { get; set; }

        /// <summary>
        /// Gets or sets the type of the related entity.
        /// </summary>
        /// <value>
        /// The type of the related entity.
        /// </value>
        [DataMember]
        public virtual EntityType RelatedEntityType { get; set; }

        /// <summary>
        /// Gets or sets the person alias.
        /// </summary>
        /// <value>
        /// The person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias PersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the interaction component.
        /// </summary>
        /// <value>
        /// The interaction component.
        /// </value>
        [DataMember]
        public virtual InteractionSession InteractionSession { get; set; }

        /// <summary>
        /// Gets or sets the personal device.
        /// </summary>
        /// <value>
        /// The personal device.
        /// </value>
        [LavaVisible]
        public virtual PersonalDevice PersonalDevice { get; set; }

        /// <summary>
        /// Sets the utm fields from URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        public void SetUTMFieldsFromURL( string url )
        {
            if ( url.IsNotNullOrWhiteSpace() && url.IndexOf( "utm_", StringComparison.OrdinalIgnoreCase ) >= 0 )
            {
                try
                {
                    var uri = new Uri( url );
                    var urlParams = System.Web.HttpUtility.ParseQueryString( uri.Query );
                    this.Source = urlParams.Get( "utm_source" ).Truncate( 25 );
                    this.Medium = urlParams.Get( "utm_medium" ).Truncate( 25 );
                    this.Campaign = urlParams.Get( "utm_campaign" ).Truncate( 50 );
                    this.Content = urlParams.Get( "utm_content" ).Truncate( 50 );
                    this.Term = urlParams.Get( "utm_term" ).Truncate( 50 );
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( new Exception( $"Error parsing '{url}' to the uri.", ex ), null );
                }
            }
        }

        /// <summary>
        /// Sets the interaction data (for example, the URL of the request), and obfuscates sensitive data that might be in the interactionData
        /// </summary>
        /// <param name="interactionData">The interaction data.</param>
        public void SetInteractionData( string interactionData )
        {
            this.InteractionData = interactionData.IsNotNullOrWhiteSpace() ? PersonToken.ObfuscateRockMagicToken( interactionData ) : string.Empty;
        }

        /// <summary>
        /// Gets or sets the interaction source date.
        /// </summary>
        /// <value>
        /// The interaction source date.
        /// </value>
        [DataMember]
        public virtual AnalyticsSourceDate InteractionSourceDate { get; set; }

        #endregion Virtual Properties
    }

    #region Entity Configuration

    /// <summary>
    /// Configuration class.
    /// </summary>
    public partial class InteractionConfiguration : EntityTypeConfiguration<Interaction>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InteractionConfiguration"/> class.
        /// </summary>
        public InteractionConfiguration()
        {
            this.HasOptional( r => r.PersonAlias ).WithMany().HasForeignKey( r => r.PersonAliasId ).WillCascadeOnDelete( false );
            this.HasRequired( r => r.InteractionComponent ).WithMany().HasForeignKey( r => r.InteractionComponentId ).WillCascadeOnDelete( true );
            this.HasOptional( r => r.InteractionSession ).WithMany( r => r.Interactions ).HasForeignKey( r => r.InteractionSessionId ).WillCascadeOnDelete( false );
            this.HasOptional( r => r.PersonalDevice ).WithMany().HasForeignKey( r => r.PersonalDeviceId ).WillCascadeOnDelete( false );
            this.HasOptional( r => r.RelatedEntityType ).WithMany().HasForeignKey( r => r.RelatedEntityTypeId ).WillCascadeOnDelete( false );

            // NOTE: When creating a migration for this, don't create the actual FK's in the database for this just in case there are outlier OccurrenceDates that aren't in the AnalyticsSourceDate table
            // and so that the AnalyticsSourceDate can be rebuilt from scratch as needed
            this.HasRequired( r => r.InteractionSourceDate ).WithMany().HasForeignKey( r => r.InteractionDateKey ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
