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

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    [Table( "ContentChannelItemAssociation" )]
    [DataContract]
    public partial class ContentChannelItemAssociation : Model<ContentChannelItemAssociation>, IOrdered
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the content channel item identifier.
        /// </summary>
        /// <value>
        /// The content channel item identifier.
        /// </value>
        [DataMember]
        public int ContentChannelItemId { get; set; }

        /// <summary>
        /// Gets or sets the child content channel item identifier.
        /// </summary>
        /// <value>
        /// The child content channel item identifier.
        /// </value>
        [DataMember]
        public int ChildContentChannelItemId { get; set; }

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
        /// Gets or sets the content channel item.
        /// </summary>
        /// <value>
        /// The content channel item.
        /// </value>
        public virtual ContentChannelItem ContentChannelItem { get; set; }

        /// <summary>
        /// Gets or sets the child content channel item.
        /// </summary>
        /// <value>
        /// The child content channel item.
        /// </value>
        [DataMember]
        public virtual ContentChannelItem ChildContentChannelItem { get; set; }

        #endregion
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

    #endregion
}