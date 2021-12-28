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
    public partial class AttributeValue : Model<AttributeValue>, ICacheable
    {
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
#if !NET5_0_OR_GREATER
        [Index( "IX_EntityId_AttributeId", IsUnique = true, Order = 2 )]
#endif
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
#if !NET5_0_OR_GREATER
        [Index( "IX_EntityId_AttributeId", IsUnique = true, Order = 1 )]
#endif
        public int? EntityId { get; set; }

        /// <summary>
        /// Gets or sets the value
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the value.
        /// </value>
        [DataMember]
        public string Value
        {
            get
            {
                return _value ?? string.Empty;
            }

            set
            {
                _value = value;
            }
        }

        private string _value = string.Empty;

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets the Value as a DateTime (maintained by SQL Trigger on AttributeValue)
        /// </summary>
        /// <remarks>
        /// see tgrAttributeValue_InsertUpdate                                                                                    
        /// </remarks>
        [DataMember]
        [DatabaseGenerated( DatabaseGeneratedOption.Computed )]
        [LavaHidden]
        public DateTime? ValueAsDateTime { get; internal set; }

        /// <summary>
        /// Gets the value as boolean (computed column)
        /// </summary>
        /// <value>
        /// The value as boolean.
        /// </value>
        [DataMember]
        [DatabaseGenerated( DatabaseGeneratedOption.Computed )]
        [LavaHidden]
        public bool? ValueAsBoolean { get; internal set; }

        /// <summary>
        /// Gets a person alias guid value as a PersonId (ComputedColumn).
        /// </summary>
        /// <remarks>
        /// Computed Column Spec:
        /// case 
        ///     when [Value] like '________-____-____-____-____________' 
        ///         then [dbo].[ufnUtility_GetPersonIdFromPersonAliasGuid]([Value]) 
        ///     else null 
        /// end
        /// </remarks>
        [DataMember]
        [DatabaseGenerated( DatabaseGeneratedOption.Computed )]
        [LavaHidden]
        public int? ValueAsPersonId { get; private set; }

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
