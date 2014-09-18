// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
        /// Gets or sets the ContentTypeId of the <see cref="Rock.Model.ContentType"/> of this ContentChannel.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> that represents the MarketingTypeAdTypeId of the <see cref="Rock.Model.ContentType"/> of this ContentChannel.
        /// </value>
        [DataMember]
        public int ContentTypeId { get; set; }

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
        [DataMember]
        public string ChannelUrl { get; set; }

        /// <summary>
        /// Gets or sets the item URL.
        /// </summary>
        /// <value>
        /// The item URL.
        /// </value>
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

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the the <see cref="Rock.Model.ContentType"/> of this ad.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.ContentType"/> of this Ad.
        /// </value>
        [DataMember]
        public virtual ContentType ContentType { get; set; }

        /// <summary>
        /// Gets or sets the items.
        /// </summary>
        /// <value>
        /// The items.
        /// </value>
        [DataMember]
        public virtual ICollection<ContentItem> Items { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentChannel"/> class.
        /// </summary>
        public ContentChannel()
        {
            Items = new Collection<ContentItem>();
        }

        #endregion

        #region Methods

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
            this.HasRequired( c => c.ContentType ).WithMany( t => t.Channels ).HasForeignKey( c => c.ContentTypeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

    #region Enumerations

    /// <summary>
    /// Represents the status of a Marketing Campaign Card
    /// </summary>
    public enum ContentChannelStatus : byte
    {
        /// <summary>
        /// The <see cref="ContentChannel"/> is pending approval.
        /// </summary>
        PendingApproval = 1,

        /// <summary>
        /// The <see cref="ContentChannel"/> has been approved.
        /// </summary>
        Approved = 2,

        /// <summary>
        /// The <see cref="ContentChannel"/> was denied.
        /// </summary>
        Denied = 3
    }

    #endregion

}