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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Entity Set Item POCO Entity.
    /// </summary>
    [Table( "EntitySetItem" )]
    [DataContract]
    [NotAudited]
    public partial class EntitySetItem : Model<EntitySetItem>, IOrdered
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the entity set identifier.
        /// </summary>
        /// <value>
        /// The entity set identifier.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int EntitySetId { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        /// <value>
        /// The entity identifier.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int EntityId { get; set; }

        /// <summary>
        /// Gets or sets the AdditionalMergeValues as a Json string.
        /// </summary>
        /// <value>
        /// A Json formatted <see cref="System.String"/> containing the AdditionalMergeValues for the EntitySet Item
        /// </value>
        [DataMember]
        public string AdditionalMergeValuesJson
        {
            get
            {
                return AdditionalMergeValues.ToJson();
            }

            set
            {
                AdditionalMergeValues = value.FromJsonOrNull<Dictionary<string, string>>() ?? new Dictionary<string, string>();
            }
        }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets a dictionary containing the Additional Merge values for this EntitySet Item
        /// </summary>
        /// <value>
        ///  A <see cref="System.Collections.Generic.Dictionary&lt;String,String&gt;"/> of <see cref="System.String"/> objects containing additional merge values for the <see cref="Rock.Model.EntitySetItem"/>
        /// </value>
        [DataMember]
        public virtual Dictionary<string, string> AdditionalMergeValues
        {
            get { return _additionalMergeValues; }
            set { _additionalMergeValues = value; }
        }
        private Dictionary<string, string> _additionalMergeValues = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the metric.
        /// </summary>
        /// <value>
        /// The metric.
        /// </value>
        public virtual EntitySet EntitySet { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the parent authority.
        /// </summary>
        public override Security.ISecured ParentAuthority
        {
            get { return this.EntitySet; }
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// EntitySetItem Configuration class.
    /// </summary>
    public partial class EntitySetItemConfiguration : EntityTypeConfiguration<EntitySetItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySetItemConfiguration"/> class.
        /// </summary>
        public EntitySetItemConfiguration()
        {
            this.HasRequired( p => p.EntitySet ).WithMany( p => p.Items ).HasForeignKey( p => p.EntitySetId ).WillCascadeOnDelete( true );
        }
    }

    #endregion
}