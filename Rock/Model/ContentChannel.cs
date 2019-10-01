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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Transactions;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    [RockDomain( "CMS" )]
    [Table( "ContentChannel" )]
    [DataContract]
    public partial class ContentChannel : Model<ContentChannel>, ICacheable
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the content channel type identifier.
        /// </summary>
        /// <value>
        /// The content channel type identifier.
        /// </value>
        [DataMember]
        public int ContentChannelTypeId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [requires approval].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [requires approval]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool RequiresApproval { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether items are manually ordered or not
        /// </summary>
        /// <value>
        /// <c>true</c> if [items manually ordered]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool ItemsManuallyOrdered { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether child items are manually ordered or not
        /// </summary>
        /// <value>
        /// <c>true</c> if [child items manually ordered]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool ChildItemsManuallyOrdered { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable RSS].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable RSS]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool EnableRss { get; set; }

        /// <summary>
        /// Gets or sets the channel URL.
        /// </summary>
        /// <value>
        /// The channel URL.
        /// </value>
        [MaxLength( 200 )]
        [DataMember]
        public string ChannelUrl { get; set; }

        /// <summary>
        /// Gets or sets the item URL.
        /// </summary>
        /// <value>
        /// The item URL.
        /// </value>
        [MaxLength( 200 )]
        [DataMember]
        public string ItemUrl { get; set; }

        /// <summary>
        /// Gets or sets the number of minutes a feed can stay cached before refreshing it from the source.
        /// </summary>
        /// <value>
        /// The time to live.
        /// </value>
        [DataMember]
        public int? TimeToLive { get; set; }

        /// <summary>
        /// Gets or sets the type of the control to render when editing content for items of this type.
        /// </summary>
        /// <value>
        /// The type of the item control.
        /// </value>
        [DataMember]
        public ContentControlType ContentControlType { get; set; }

        /// <summary>
        /// Gets or sets the root image directory to use when the Html control type is used
        /// </summary>
        /// <value>
        /// The image root directory.
        /// </value>
        [MaxLength( 200 )]
        [DataMember]
        public string RootImageDirectory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is index enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is index enabled; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsIndexEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is tagging enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is tagging enabled; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsTaggingEnabled { get; set; }

        /// <summary>
        /// Gets or sets the item tag category identifier.
        /// </summary>
        /// <value>
        /// The item tag category identifier.
        /// </value>
        [DataMember]
        public int? ItemTagCategoryId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the type of the content channel.
        /// </summary>
        /// <value>
        /// The type of the content channel.
        /// </value>
        [DataMember]
        public virtual ContentChannelType ContentChannelType { get; set; }

        /// <summary>
        /// Gets or sets the item tag category.
        /// </summary>
        /// <value>
        /// The item tag category.
        /// </value>
        [DataMember]
        public virtual Category ItemTagCategory { get; set; }

        /// <summary>
        /// Gets or sets the items.
        /// </summary>
        /// <value>
        /// The items.
        /// </value>
        [LavaInclude]
        public virtual ICollection<ContentChannelItem> Items { get; set; }

        /// <summary>
        /// Gets or sets the collection of ContentChannels that this ContentChannel allows as children.
        /// </summary>
        /// <value>
        /// A collection of ContentChannels that this ContentChannel allows as children.
        /// </value>
        [DataMember, LavaIgnore]
        public virtual ICollection<ContentChannel> ChildContentChannels
        {
            get { return _childContentChannels ?? ( _childContentChannels = new Collection<ContentChannel>() ); }
            set { _childContentChannels = value; }
        }
        private ICollection<ContentChannel> _childContentChannels;

        /// <summary>
        /// Gets or sets a collection containing the ContentChannels that allow this ContentChannel as a child.
        /// </summary>
        /// <value>
        /// A collection containing the ContentChannels that allow this ContentChannel as a child.
        /// </value>
        public virtual ICollection<ContentChannel> ParentContentChannels
        {
            get { return _parentContentChannels ?? ( _parentContentChannels = new Collection<ContentChannel>() ); }
            set { _parentContentChannels = value; }
        }
        private ICollection<ContentChannel> _parentContentChannels;

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
                supportedActions.AddOrReplace( Rock.Security.Authorization.APPROVE, "The roles and/or users that have access to approve channel items." );
                supportedActions.AddOrReplace( Rock.Security.Authorization.INTERACT, "The roles and/or users that have access to intertact with the channel item." );
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
                return this.ContentChannelType != null ? this.ContentChannelType : base.ParentAuthority;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentChannel"/> class.
        /// </summary>
        public ContentChannel()
        {
            Items = new Collection<ContentChannelItem>();
        }

        #endregion

        #region Methods

        #region Index Methods

        /// <summary>
        /// Queues ContentChannelItems of this ContentChannel to have their indexes deleted
        /// </summary>
        /// <param name="contentChannelId">The content channel identifier.</param>
        public void DeleteIndexedDocumentsByContentChannel( int contentChannelId )
        {
            var contentChannelItemIds = new ContentChannelItemService( new RockContext() ).Queryable()
                                    .Where( i => i.ContentChannelId == contentChannelId ).Select( a => a.Id ).ToList();

            int contentChannelItemEntityTypeId = EntityTypeCache.GetId<Rock.Model.ContentChannelItem>().Value;

            foreach ( var contentChannelItemId in contentChannelItemIds )
            {
                var transaction = new DeleteIndexEntityTransaction { EntityId = contentChannelItemId, EntityTypeId = contentChannelItemEntityTypeId };
                transaction.Enqueue();
            }
        }

        /// <summary>
        /// Queues ContentChannelItems of this ContentChannel to have their indexes updated
        /// </summary>
        /// <param name="contentChannelId">The content channel identifier.</param>
        public void BulkIndexDocumentsByContentChannel( int contentChannelId )
        {
            // return all approved content channel items that are in content channels that should be indexed
            var contentChannelItemIds = new ContentChannelItemService( new RockContext() ).Queryable()
                                            .Where( i =>
                                                i.ContentChannelId == contentChannelId
                                                && ( i.ContentChannel.RequiresApproval == false || i.ContentChannel.ContentChannelType.DisableStatus || i.Status == ContentChannelItemStatus.Approved ) )
                                            .Select( a => a.Id ).ToList();

            int contentChannelItemEntityTypeId = EntityTypeCache.GetId<Rock.Model.ContentChannelItem>().Value;

            foreach ( var contentChannelItemId in contentChannelItemIds )
            {
                var transaction = new IndexEntityTransaction { EntityId = contentChannelItemId, EntityTypeId = contentChannelItemEntityTypeId };
                transaction.Enqueue();
            }
        }

        #endregion

        /// <summary>
        /// Pres the save.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="state">The state.</param>
        public override void PreSaveChanges( Data.DbContext dbContext, EntityState state )
        {
            if ( state == EntityState.Deleted )
            {
                ChildContentChannels.Clear();
            }

            // clean up the index
            if ( state == EntityState.Deleted && IsIndexEnabled )
            {
                this.DeleteIndexedDocumentsByContentChannel( Id );
            }
            else if ( state == EntityState.Modified )
            {
                // check if indexing is enabled
                var changeEntry = dbContext.ChangeTracker.Entries<ContentChannel>().Where( a => a.Entity == this ).FirstOrDefault();
                if ( changeEntry != null )
                {
                    var originalIndexState = (bool)changeEntry.OriginalValues["IsIndexEnabled"];

                    if ( originalIndexState == true && IsIndexEnabled == false )
                    {
                        // clear out index items
                        this.DeleteIndexedDocumentsByContentChannel( Id );
                    }
                    else if ( IsIndexEnabled == true )
                    {
                        // if indexing is enabled then bulk index - needed as an attribute could have changed from IsIndexed
                        BulkIndexDocumentsByContentChannel( Id );
                    }
                }
            }

            base.PreSaveChanges( dbContext, state );
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }
        #endregion

        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return ContentChannelCache.Get( this.Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            ContentChannelCache.UpdateCachedEntity( this.Id, entityState );
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// 
    /// </summary>
    public partial class ContentChannelConfiguration : EntityTypeConfiguration<ContentChannel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContentChannelConfiguration" /> class.
        /// </summary>
        public ContentChannelConfiguration()
        {
            this.HasMany( p => p.ChildContentChannels ).WithMany( c => c.ParentContentChannels ).Map( m => { m.MapLeftKey( "ContentChannelId" ); m.MapRightKey( "ChildContentChannelId" ); m.ToTable( "ContentChannelAssociation" ); } );
            this.HasRequired( c => c.ContentChannelType ).WithMany( t => t.Channels ).HasForeignKey( c => c.ContentChannelTypeId ).WillCascadeOnDelete( false );
            this.HasOptional( c => c.ItemTagCategory ).WithMany().HasForeignKey( c => c.ItemTagCategoryId ).WillCascadeOnDelete( false );
        }
    }

    #endregion


    #region Enumerations

    /// <summary>
    /// Represents the type of editor to use when editing content for channel item
    /// </summary>
    public enum ContentControlType
    {
        /// <summary>
        /// Code Editor control
        /// </summary>
        CodeEditor = 0,

        /// <summary>
        /// Html Editor control
        /// </summary>
        HtmlEditor = 1
    }

    #endregion

}