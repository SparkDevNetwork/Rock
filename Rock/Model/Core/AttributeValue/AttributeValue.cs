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
using Newtonsoft.Json;
using Rock.Data;
using Rock.Web.Cache;
using Rock.Lava;

namespace Rock.Model
{
    /// <summary>
    /// Represents a value of an <see cref="Rock.Model.Attribute"/>. 
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "AttributeValue" )]
    [DataContract]
    [JsonConverter( typeof( Rock.Utility.AttributeValueJsonConverter ) )]
    [Rock.SystemGuid.EntityTypeGuid( "D2BDCCF0-D3F4-4F29-B286-DA5B7BFA41C6")]
    public partial class AttributeValue : Model<AttributeValue>, ICacheable
    {
        #region Private Fields

        /// <summary>
        /// Backing field for <see cref="Value"/>.
        /// </summary>
        private string _value = string.Empty;

        /// <summary>
        /// Backing field for <see cref="PersistedTextValue"/>.
        /// </summary>
        private string _persistedTextValue = string.Empty;

        /// <summary>
        /// Backing field for <see cref="PersistedHtmlValue"/>.
        /// </summary>
        private string _persistedHtmlValue = string.Empty;

        /// <summary>
        /// Backing field for <see cref="PersistedCondensedTextValue"/>.
        /// </summary>
        private string _persistedCondensedTextValue = string.Empty;

        /// <summary>
        /// Backing field for <see cref="PersistedCondensedHtmlValue"/>.
        /// </summary>
        private string _persistedCondensedHtmlValue = string.Empty;

        #endregion

        #region Entity Properties

        /// <summary>
        /// Gets or sets a flag indicating if this AttributeValue is part of the Rock core system/framework.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if is part of the Rock core system/framework.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        [LavaHidden]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the AttributeId of the <see cref="Rock.Model.Attribute"/> that this AttributeValue provides a value for.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the AttributeId of the <see cref="Rock.Model.Attribute"/> that this AttributeValue provides a value for.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        [Index( "IX_EntityId_AttributeId", IsUnique = true, Order = 2 )]
        public int AttributeId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the entity instance that uses this AttributeValue. An <see cref="Rock.Model.Attribute"/> is a configuration setting, so each 
        /// instance of the Entity that uses the same Attribute can have a different value.  For instance a <see cref="Rock.Model.BlockType"/> has a declared attribute, and that attribute can be configured 
        /// with a different value on each <see cref="Rock.Model.Block"/> that implements the <see cref="Rock.Model.BlockType"/>. This value will either be 0 or null for global attributes or attributes that have a 
        /// constant across all instances of an EntityType.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> that identifies the Id of the entity instance that uses this AttributeValue.
        /// </value>
        [DataMember]
        [Index( "IX_EntityId_AttributeId", IsUnique = true, Order = 1 )]
        public int? EntityId { get; set; }

        /// <summary>
        /// Gets or sets the raw value
        /// </summary>
        /// <value>
        /// A <see cref="string"/> representing the raw value.
        /// </value>
        [DataMember]
        public string Value
        {
            get => _value ?? string.Empty;
            set => _value = value;
        }

        /// <summary>
        /// Gets or sets the persisted text value.
        /// </summary>
        /// <value>The persisted text value.</value>
        [DataMember]
        public string PersistedTextValue
        {
            get => _persistedTextValue ?? string.Empty;
            set => _persistedTextValue = value;
        }

        /// <summary>
        /// Gets or sets the persisted HTML value.
        /// </summary>
        /// <value>The persisted HTML value.</value>
        [DataMember]
        public string PersistedHtmlValue
        {
            get => _persistedHtmlValue ?? string.Empty;
            set => _persistedHtmlValue = value;
        }

        /// <summary>
        /// Gets or sets the persisted condensed text value.
        /// </summary>
        /// <value>The persisted condensed text value.</value>
        [DataMember]
        public string PersistedCondensedTextValue
        {
            get => _persistedCondensedTextValue ?? string.Empty;
            set => _persistedCondensedTextValue = value;
        }

        /// <summary>
        /// Gets or sets the persisted condensed HTML value.
        /// </summary>
        /// <value>The persisted condensed HTML value.</value>
        [DataMember]
        public string PersistedCondensedHtmlValue
        {
            get => _persistedCondensedHtmlValue ?? string.Empty;
            set => _persistedCondensedHtmlValue = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the persisted values are
        /// considered dirty. If the values are dirty then it should be assumed
        /// that they are not in sync with the <see cref="Value"/> property.
        /// </summary>
        /// <value><c>true</c> if the persisted values are considered dirty; otherwise, <c>false</c>.</value>
        [DataMember]
        public bool IsPersistedValueDirty { get; set; }

        /// <summary>
        /// Gets the <see cref="Value"/> as a DateTime. This value is only updated on save.
        /// </summary>
        /// <value>The <see cref="Value"/> as a DateTime.</value>
        [DataMember]
        [LavaHidden]
        public DateTime? ValueAsDateTime { get; internal set; }

        /// <summary>
        /// Gets the Value as a decimal. This value is only updated on save.
        /// </summary>
        /// <remarks>The setter should not be called by plug-ins.</remarks>
        /// <value>The <see cref="Value"/> as a decimal.</value>
        [DataMember]
        [LavaHidden]
        public decimal? ValueAsNumeric { get; set; }

        /// <summary>
        /// Gets the <see cref="Value"/> as a PersonId. This value is only
        /// updated on save.
        /// </summary>
        /// <value>The <see cref="Value"/> as a PersonId.</value>
        [DataMember]
        [LavaHidden]
        public int? ValueAsPersonId { get; private set; }

        /// <summary>
        /// Gets the value as a boolean. This value is only updated on save.
        /// </summary>
        /// <value>
        /// The <see cref="Value"/> as a boolean.
        /// </value>
        [DataMember]
        [LavaHidden]
        public bool? ValueAsBoolean { get; internal set; }

        #endregion

        #region Calculated Properties

        /// <summary>
        /// Gets the value checksum. This is a hash of <see cref="Value"/> that
        /// is automatically calculated by the database.
        /// </summary>
        /// <value>The value checksum.</value>
        [DataMember]
        [DatabaseGenerated( DatabaseGeneratedOption.Computed )]
        [LavaHidden]
        public int ValueChecksum { get; private set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Attribute"/> that uses this AttributeValue.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Attribute"/> that uses this value.
        /// </value>
        [DataMember]
        [LavaHidden]
        public virtual Attribute Attribute { get; set; }

        /// <summary>
        /// Gets or sets the a list of previous values that this attribute value had (If Attribute.EnableHistory is enabled)
        /// </summary>
        /// <value>
        /// The attribute values historical.
        /// </value>
        [DataMember]
        [LavaHidden]
        public virtual ICollection<AttributeValueHistorical> AttributeValuesHistorical { get; set; } = new Collection<AttributeValueHistorical>();

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Value;
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Attribute Value Configuration class.
    /// </summary>
    public partial class AttributeValueConfiguration : EntityTypeConfiguration<AttributeValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeValueConfiguration"/> class.
        /// </summary>
        public AttributeValueConfiguration()
        {
            this.HasRequired( p => p.Attribute ).WithMany().HasForeignKey( p => p.AttributeId ).WillCascadeOnDelete( true );
        }
    }

    #endregion
}
