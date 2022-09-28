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
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;
using Rock.Lava;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    [RockDomain( "CMS" )]
    [Table( "ContentChannelItemAssociation" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( "7C86EED3-C3F9-4B25-887B-F732FE3C35F0")]
    public partial class ContentChannelItemAssociation : Model<ContentChannelItemAssociation>, IOrdered
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.ContentChannelItem"/> identifier.
        /// </summary>
        /// <value>
        /// The content channel item identifier.
        /// </value>
        [DataMember]
        [IgnoreCanDelete]
        public int ContentChannelItemId { get; set; }

        /// <summary>
        /// Gets or sets the child <see cref="Rock.Model.ContentChannelItem"/> identifier.
        /// </summary>
        /// <value>
        /// The child content channel item identifier.
        /// </value>
        [DataMember]
        [IgnoreCanDelete]
        public int ChildContentChannelItemId { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int Order { get; set; }

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.ContentChannelItem"/>.
        /// </summary>
        /// <value>
        /// The content channel item.
        /// </value>
        [LavaVisible]
        public virtual ContentChannelItem ContentChannelItem { get; set; }

        /// <summary>
        /// Gets or sets the child <see cref="Rock.Model.ContentChannelItem"/>.
        /// </summary>
        /// <value>
        /// The child content channel item.
        /// </value>
        [DataMember]
        public virtual ContentChannelItem ChildContentChannelItem { get; set; }

        #endregion Navigation Properties
    }

    #region Entity Configuration

    /// <summary>
    /// ContentChannelItemAssociation Configuration class
    /// </summary>
    public partial class ContentChannelItemAssociationConfiguration : EntityTypeConfiguration<ContentChannelItemAssociation>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContentChannelItemAssociationConfiguration"/> class.
        /// </summary>
        public ContentChannelItemAssociationConfiguration()
        {
            this.HasRequired( i => i.ContentChannelItem ).WithMany( i => i.ChildItems ).HasForeignKey( i => i.ContentChannelItemId ).WillCascadeOnDelete( false );
            this.HasRequired( i => i.ChildContentChannelItem ).WithMany( i => i.ParentItems ).HasForeignKey( i => i.ChildContentChannelItemId ).WillCascadeOnDelete( false );
        }
    }

    #endregion Entity Configuration
}