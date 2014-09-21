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
    [Table( "ContentItem")]
    [DataContract]
    public partial class ContentItem : Model<ContentItem>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the content channel identifier.
        /// </summary>
        /// <value>
        /// The content channel identifier.
        /// </value>
        [DataMember]
        public int ContentChannelId { get; set; }

        /// <summary>
        /// Gets or sets the content type identifier.
        /// </summary>
        /// <value>
        /// The content type identifier.
        /// </value>
        [DataMember]
        public int ContentTypeId { get; set; }

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
        /// Gets or sets the <see cref="Rock.Model.ContentItemStatus"/> (status) of this ContentItem.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.ContentItemStatus"/> enumeration value that represents the status of this ContentItem. When <c>ContentItemStatus.PendingApproval</c> the item is 
        /// awaiting approval; when <c>ContentItemStatus.Approved</c> the item has been approved by the approver, when <c>ContentItemStatus.Denied</c> the item has been denied by the approver.
        /// </value>
        [DataMember]
        public ContentItemStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the PersonAliasId of the <see cref="Rock.Model.Person"/> who either approved or declined the ContentItem. If no approval action has been performed on this Ad, this value will be null.
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
        public DateTime StartDateTime { get; set; }

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
        /// Gets or sets the the <see cref="Rock.Model.ContentType"/> of this ad.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.ContentType"/> of this Ad.
        /// </value>
        [DataMember]
        public virtual ContentType ContentType { get; set; }

        /// <summary>
        /// Gets or sets the approved by person alias.
        /// </summary>
        /// <value>
        /// The approved by person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias ApprovedByPersonAlias { get; set; }

        #endregion

        #region Methods

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// 
    /// </summary>
    public partial class ContentItemConfiguration : EntityTypeConfiguration<ContentItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContentItemConfiguration" /> class.
        /// </summary>
        public ContentItemConfiguration()
        {
            this.HasRequired( i => i.ContentChannel ).WithMany( c => c.Items ).HasForeignKey( i => i.ContentChannelId ).WillCascadeOnDelete( false );
            this.HasRequired( i => i.ContentType ).WithMany().HasForeignKey( i => i.ContentTypeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

    #region Enumerations

    /// <summary>
    /// Represents the status of a Marketing Campaign Card
    /// </summary>
    public enum ContentItemStatus : byte
    {
        /// <summary>
        /// The <see cref="ContentItem"/> is pending approval.
        /// </summary>
        PendingApproval = 1,

        /// <summary>
        /// The <see cref="ContentItem"/> has been approved.
        /// </summary>
        Approved = 2,

        /// <summary>
        /// The <see cref="ContentItem"/> was denied.
        /// </summary>
        Denied = 3
    }

    #endregion

}