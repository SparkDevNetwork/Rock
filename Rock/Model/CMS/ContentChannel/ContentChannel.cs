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
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Rock.Cms;
using Rock.Data;
using Rock.Lava;
using Rock.Tasks;
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
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.CONTENT_CHANNEL )]
    public partial class ContentChannel : Model<ContentChannel>, ICacheable, ICampusFilterable
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.ContentChannelType"/> identifier.
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
        /// Gets or sets the root image directory to use when the HTML control type is used
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

        /// <summary>
        /// Gets or sets a value indicating whether this content is structured.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this content is structured; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsStructuredContent { get; set; }

        /// <summary>
        /// Gets or sets the Structure Content Tool Id.
        /// </summary>
        /// <value>
        /// The structure content tool value identifier.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.STRUCTURED_CONTENT_EDITOR_TOOLS )]
        public int? StructuredContentToolValueId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable personalization].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable personalization]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool EnablePersonalization { get; set; }

        /// <summary>
        /// Gets or sets the Content Library configuration JSON.
        /// </summary>
        /// <value>The Content Library configuration JSON.</value>
        [DataMember]
        public string ContentLibraryConfigurationJson
        {
            get
            {
                return ContentLibraryConfiguration?.ToJson();
            }

            set
            {
                ContentLibraryConfiguration = value.FromJsonOrNull<ContentLibraryConfiguration>() ?? new ContentLibraryConfiguration();
            }
        }

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.ContentChannelType">type</see> of the content channel.
        /// </summary>
        /// <value>
        /// The type of the content channel.
        /// </value>
        [DataMember]
        public virtual ContentChannelType ContentChannelType { get; set; }

        /// <summary>
        /// Gets or sets the item tag <see cref="Rock.Model.Category"/>.
        /// </summary>
        /// <value>
        /// The item tag category.
        /// </value>
        [DataMember]
        public virtual Category ItemTagCategory { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.DefinedValue"/> representing the content channel's structure content tool.
        /// </summary>
        /// <value>
        /// A <see cref="DefinedValue"/> object representing the content channel's structure content tool.
        /// </value>
        [DataMember]
        public virtual DefinedValue StructuredContentToolValue { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.ContentChannelItem">items</see>.
        /// </summary>
        /// <value>
        /// The items.
        /// </value>
        [LavaVisible]
        public virtual ICollection<ContentChannelItem> Items { get; set; }

        /*
            08/25/2020 - MSB
            We have added the JsonIgnore attribute to address in application crash issue
            caused by a Content Channel referencing itself when the object is serialized
            to a JSON string.

            https://github.com/SparkDevNetwork/Rock/issues/4250

            Reason: Web Api Controller
        */

        /// <summary>
        /// Gets or sets the collection of ContentChannels that this ContentChannel allows as children.
        /// </summary>
        /// <value>
        /// A collection of ContentChannels that this ContentChannel allows as children.
        /// </value>
        [DataMember, JsonIgnore]
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
        /// Gets or sets the collection of <see cref="Rock.Model.Category">Categories</see> that this Content Channel is associated with.
        /// NOTE: Since changes to Categories isn't tracked by ChangeTracker, set the ModifiedDateTime if Categories are modified.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.Category">Categories</see> that this Content Channel is associated with.
        /// </value>
        [DataMember]
        public virtual ICollection<Category> Categories
        {
            get { return _categories ?? ( _categories = new Collection<Category>() ); }
            set { _categories = value; }
        }

        private ICollection<Category> _categories;

        /// <summary>
        /// Provides a <see cref="Dictionary{TKey, TValue}"/> of actions that this model supports, and the description of each.
        /// </summary>
        [NotMapped]
        public override Dictionary<string, string> SupportedActions
        {
            get
            {
                var supportedActions = base.SupportedActions;
                supportedActions.AddOrReplace( Rock.Security.Authorization.APPROVE, "The roles and/or users that have access to approve channel items." );
                supportedActions.AddOrReplace( Rock.Security.Authorization.INTERACT, "The roles and/or users that have access to interact with the channel item." );
                return supportedActions;
            }
        }

        #endregion Navigation Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentChannel"/> class.
        /// </summary>
        public ContentChannel()
        {
            Items = new Collection<ContentChannelItem>();
        }

        #endregion Constructors

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
                var deleteEntityTypeIndexMsg = new DeleteEntityTypeIndex.Message
                {
                    EntityTypeId = contentChannelItemEntityTypeId,
                    EntityId = contentChannelItemId
                };

                deleteEntityTypeIndexMsg.Send();
            }
        }

        /// <summary>
        /// Queues ContentChannelItems of this ContentChannel to have their indexes updated
        /// </summary>
        /// <param name="contentChannelId">The content channel identifier.</param>
        public void BulkIndexDocumentsByContentChannel( int contentChannelId )
        {
            // return all approved content channel items that are in content channels that should be indexed
            var contentChannelItemIds = new ContentChannelItemService( new RockContext() )
                .Queryable()
                .Where( i => i.ContentChannelId == contentChannelId
                    && ( i.ContentChannel.RequiresApproval == false || i.ContentChannel.ContentChannelType.DisableStatus || i.Status == ContentChannelItemStatus.Approved ) )
                .Select( a => a.Id )
                .ToList();

            int contentChannelItemEntityTypeId = EntityTypeCache.GetId<Rock.Model.ContentChannelItem>().Value;

            foreach ( var contentChannelItemId in contentChannelItemIds )
            {
                var indexEntityTransaction = new IndexEntityTransaction( new EntityIndexInfo() { EntityTypeId = contentChannelItemEntityTypeId, EntityId = contentChannelItemId } );
                indexEntityTransaction.Enqueue();
            }
        }

        #endregion Index Methods

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

        #endregion Methods
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
            this.HasOptional( p => p.StructuredContentToolValue ).WithMany().HasForeignKey( p => p.StructuredContentToolValueId ).WillCascadeOnDelete( false );
            this.HasMany( a => a.Categories ).WithMany().Map( a => { a.MapLeftKey( "ContentChannelId" ); a.MapRightKey( "CategoryId" ); a.ToTable( "ContentChannelCategory" ); } );
        }
    }

    #endregion Entity Configuration
}