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
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.UniversalSearch;
using Rock.UniversalSearch.IndexModels;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    [RockDomain( "CMS" )]
    [Table( "ContentChannelItem" )]
    [DataContract]
    public partial class ContentChannelItem : Model<ContentChannelItem>, IOrdered, IRockIndexable
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the content channel identifier.
        /// </summary>
        /// <value>
        /// The content channel identifier.
        /// </value>
        [DataMember]
        [HideFromReporting]
        public int ContentChannelId { get; set; }

        /// <summary>
        /// Gets or sets the content channel type identifier.
        /// </summary>
        /// <value>
        /// The content channel type identifier.
        /// </value>
        [DataMember]
        [HideFromReporting]
        public int ContentChannelTypeId { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        [MaxLength( 200 )]
        [DataMember]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        [DataMember]
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the priority of this ContentItem. The lower the number, the higher the priority.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the priority of this ContentItem. The lower the number, the higher the priority of the Ad.
        /// </value>
        [DataMember]
        public int Priority { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.ContentChannelItemStatus"/> (status) of this ContentItem.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.ContentChannelItemStatus"/> enumeration value that represents the status of this ContentItem. When <c>ContentItemStatus.PendingApproval</c> the item is 
        /// awaiting approval; when <c>ContentItemStatus.Approved</c> the item has been approved by the approver, when <c>ContentItemStatus.Denied</c> the item has been denied by the approver.
        /// </value>
        [DataMember]
        public ContentChannelItemStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the PersonAliasId of the <see cref="Rock.Model.Person"/> who either approved or declined the ContentItem. If no approval action has been performed on this item, this value will be null.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the PersonAliasId of hte <see cref="Rock.Model.Person"/> who either approved or declined the ContentItem. This value will be null if no approval action has been
        /// performed on this add.
        /// </value>
        [DataMember]
        public int? ApprovedByPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the approved date.
        /// </summary>
        /// <value>
        /// The approved date.
        /// </value>
        [DataMember]
        public DateTime? ApprovedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the start date time.
        /// </summary>
        /// <value>
        /// The start date time.
        /// </value>
        [DataMember]
        public DateTime StartDateTime { get; set; } = RockDateTime.Now;

        /// <summary>
        /// Gets or sets the expire date time.
        /// </summary>
        /// <value>
        /// The expire date time.
        /// </value>
        [DataMember]
        public DateTime? ExpireDateTime { get; set; }

        /// <summary>
        /// Gets or sets the permalink.
        /// </summary>
        /// <value>
        /// The permalink.
        /// </value>
        [MaxLength( 200 )]
        [DataMember]
        public string Permalink { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int Order { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the content channel.
        /// </summary>
        /// <value>
        /// The content channel.
        /// </value>
        [DataMember]
        public virtual ContentChannel ContentChannel { get; set; }

        /// <summary>
        /// Gets or sets the type of the content channel.
        /// </summary>
        /// <value>
        /// The type of the content channel.
        /// </value>
        [DataMember]
        public virtual ContentChannelType ContentChannelType { get; set; }

        /// <summary>
        /// Gets or sets the approved by person alias.
        /// </summary>
        /// <value>
        /// The approved by person alias.
        /// </value>
        [LavaInclude]
        public virtual PersonAlias ApprovedByPersonAlias { get; set; }

        /// <summary>
        /// Gets the primary slug.
        /// </summary>
        /// <value>
        /// The primary alias.
        /// </value>
        [NotMapped]
        [LavaInclude]
        public virtual string PrimarySlug
        {
            get
            {
                return ContentChannelItemSlugs.Select( a => a.Slug ).FirstOrDefault( );
            }
        }

        /// <summary>
        /// Gets or sets the content channel item slugs.
        /// </summary>
        /// <value>
        /// The content channel item slugs.
        /// </value>
        [LavaInclude]
        public virtual ICollection<ContentChannelItemSlug> ContentChannelItemSlugs { get; set; }

        /// <summary>
        /// Gets or sets the child items.
        /// </summary>
        /// <value>
        /// The child items.
        /// </value>
        [LavaInclude]
        public virtual ICollection<ContentChannelItemAssociation> ChildItems
        {
            get { return _childItems ?? ( _childItems = new Collection<ContentChannelItemAssociation>() ); }
            set { _childItems = value; }
        }
        private ICollection<ContentChannelItemAssociation> _childItems;

        /// <summary>
        /// Gets or sets the parent items.
        /// </summary>
        /// <value>
        /// The parent items.
        /// </value>
        [LavaInclude]
        public virtual ICollection<ContentChannelItemAssociation> ParentItems
        {
            get { return _parentItems ?? ( _parentItems = new Collection<ContentChannelItemAssociation>() ); }
            set { _parentItems = value; }
        }
        private ICollection<ContentChannelItemAssociation> _parentItems;

        /// <summary>
        /// Gets or sets the content channel items.
        /// </summary>
        /// <value>
        /// The content channel items.
        /// </value>
        public virtual ICollection<EventItemOccurrenceChannelItem> EventItemOccurrences
        {
            get { return _eventItemOccurrences ?? ( _eventItemOccurrences = new Collection<EventItemOccurrenceChannelItem>() ); }
            set { _eventItemOccurrences = value; }
        }
        private ICollection<EventItemOccurrenceChannelItem> _eventItemOccurrences;

        /// <summary>
        /// Gets the supported actions.
        /// </summary>
        /// <value>
        /// The supported actions.
        /// </value>
        [NotMapped]
        public override Dictionary<string, string> SupportedActions
        {
            get
            {
                var supportedActions = base.SupportedActions;
                supportedActions.AddOrReplace( Rock.Security.Authorization.INTERACT, "The roles and/or users that have access to interact with the channel item." );
                return supportedActions;
            }
        }

        /// <summary>
        /// Gets the parent authority.
        /// </summary>
        /// <value>
        /// The parent authority.
        /// </value>
        [NotMapped]
        public override Security.ISecured ParentAuthority
        {
            get
            {
                return ContentChannel != null ? ContentChannel : base.ParentAuthority;
            }
        }

        /// <summary>
        /// Gets a value indicating whether [allows interactive bulk indexing].
        /// </summary>
        /// <value>
        /// <c>true</c> if [allows interactive bulk indexing]; otherwise, <c>false</c>.
        /// </value>
        /// <exception cref="System.NotImplementedException"></exception>
        [NotMapped]
        public bool AllowsInteractiveBulkIndexing
        {
            get
            {
                return true;
            }
        }

        #endregion

        #region Index Methods

        /// <summary>
        /// Bulks the index documents.
        /// </summary>
        public void BulkIndexDocuments()
        {
            List<ContentChannelItemIndex> indexableChannelItems = new List<ContentChannelItemIndex>();

            // return all approved content channel items that are in content channels that should be indexed
            RockContext rockContext = new RockContext();
            var contentChannelItems = new ContentChannelItemService( rockContext ).Queryable()
                                            .Where( i =>
                                                i.ContentChannel.IsIndexEnabled
                                                && ( i.ContentChannel.RequiresApproval == false || i.ContentChannel.ContentChannelType.DisableStatus || i.Status == ContentChannelItemStatus.Approved ) );

            int recordCounter = 0;

            foreach ( var item in contentChannelItems )
            {
                var indexableChannelItem = ContentChannelItemIndex.LoadByModel( item );
                indexableChannelItems.Add( indexableChannelItem );

                recordCounter++;

                if ( recordCounter > 100 )
                {
                    IndexContainer.IndexDocuments( indexableChannelItems );
                    indexableChannelItems = new List<ContentChannelItemIndex>();
                    recordCounter = 0;
                }
            }

            IndexContainer.IndexDocuments( indexableChannelItems );
        }

        /// <summary>
        /// Indexes the document.
        /// </summary>
        /// <param name="id"></param>
        public void IndexDocument( int id )
        {
            var itemEntity = new ContentChannelItemService( new RockContext() ).Get( id );

            // only index if the content channel is set to be indexed
            if ( itemEntity.ContentChannel != null && itemEntity.ContentChannel.IsIndexEnabled )
            {
                // ensure it's meant to be indexed
                if ( itemEntity.ContentChannel.IsIndexEnabled && ( itemEntity.ContentChannel.RequiresApproval == false || itemEntity.ContentChannel.ContentChannelType.DisableStatus || itemEntity.Status == ContentChannelItemStatus.Approved ) )
                {
                    var indexItem = ContentChannelItemIndex.LoadByModel( itemEntity );
                    IndexContainer.IndexDocument( indexItem );
                }
            }
        }

        /// <summary>
        /// Deletes the indexed document.
        /// </summary>
        /// <param name="id"></param>
        public void DeleteIndexedDocument( int id )
        {
            IndexContainer.DeleteDocumentById( this.IndexModelType(), id );
        }

        /// <summary>
        /// Deletes the indexed documents.
        /// </summary>
        public void DeleteIndexedDocuments()
        {
            IndexContainer.DeleteDocumentsByType<ContentChannelItemIndex>();
        }

        /// <summary>
        /// Indexes the name of the model.
        /// </summary>
        /// <returns></returns>
        public Type IndexModelType()
        {
            return typeof( ContentChannelItemIndex );
        }

        /// <summary>
        /// Gets the index filter values.
        /// </summary>
        /// <returns></returns>
        public ModelFieldFilterConfig GetIndexFilterConfig()
        {
            ModelFieldFilterConfig filterConfig = new ModelFieldFilterConfig();
            filterConfig.FilterValues = new ContentChannelService( new RockContext() ).Queryable().AsNoTracking().Where( c => c.IsIndexEnabled ).Select( c => c.Name ).ToList();
            filterConfig.FilterLabel = "Content Channels";
            filterConfig.FilterField = "contentChannel";

            return filterConfig;
        }

        /// <summary>
        /// Gets the index filter field.
        /// </summary>
        /// <returns></returns>
        public bool SupportsIndexFieldFiltering()
        {
            return true;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Pres the save.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="state">The state.</param>
        public override void PreSaveChanges( Data.DbContext dbContext, EntityState state )
        {
            var channel = this.ContentChannel;

            if ( state == EntityState.Deleted )
            {
                ChildItems.Clear();
                ParentItems.Clear();
            }

            base.PreSaveChanges( dbContext, state );
        }

        /// <summary>
        /// Posts the save changes.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public override void PostSaveChanges( Data.DbContext dbContext )
        {
            base.PostSaveChanges( dbContext );

            var rockContext = ( RockContext ) dbContext;
            var contentChannelItemSerivce = new ContentChannelItemService( rockContext );
            var contentChannelSlugSerivce = new ContentChannelItemSlugService( rockContext );

            if ( !contentChannelSlugSerivce.Queryable().Any( a => a.ContentChannelItemId == this.Id ) && contentChannelItemSerivce.Queryable().Any(a=>a.Id == Id) )
            {
                contentChannelSlugSerivce.SaveSlug( Id, Title, null );
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Title;
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// 
    /// </summary>
    public partial class ContentItemConfiguration : EntityTypeConfiguration<ContentChannelItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContentItemConfiguration" /> class.
        /// </summary>
        public ContentItemConfiguration()
        {
            this.HasRequired( i => i.ContentChannel ).WithMany( c => c.Items ).HasForeignKey( i => i.ContentChannelId ).WillCascadeOnDelete( false );
            this.HasRequired( i => i.ContentChannelType ).WithMany().HasForeignKey( i => i.ContentChannelTypeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

    #region Enumerations

    /// <summary>
    /// Represents the approval status of a content channel item
    /// </summary>
    public enum ContentChannelItemStatus
    {
        /// <summary>
        /// The <see cref="ContentChannelItem"/> is pending approval.
        /// </summary>
        PendingApproval = 1,

        /// <summary>
        /// The <see cref="ContentChannelItem"/> has been approved.
        /// </summary>
        Approved = 2,

        /// <summary>
        /// The <see cref="ContentChannelItem"/> was denied.
        /// </summary>
        Denied = 3
    }

    #endregion

}