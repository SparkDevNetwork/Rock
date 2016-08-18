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
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    [Table( "ContentChannel" )]
    [DataContract]
    public partial class ContentChannel : Model<ContentChannel>
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
        /// Gets or sets the items.
        /// </summary>
        /// <value>
        /// The items.
        /// </value>
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

        /// <summary>
        /// Pres the save.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="state">The state.</param>
        public override void PreSaveChanges( DbContext dbContext, System.Data.Entity.EntityState state )
        {
            if ( state == System.Data.Entity.EntityState.Deleted )
            {
                ChildContentChannels.Clear();
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
            return this.Name;
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