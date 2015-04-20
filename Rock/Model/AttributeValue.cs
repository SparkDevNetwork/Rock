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
using Newtonsoft.Json;
using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents a value of an <see cref="Rock.Model.Attribute"/>. 
    /// </summary>
    [Table( "AttributeValue" )]
    [DataContract]
    [JsonConverter( typeof( Rock.Utility.AttributeValueJsonConverter ) )]
    public partial class AttributeValue : Model<AttributeValue>
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
        [LavaIgnore]
        public bool IsSystem { get; set; }
        
        /// <summary>
        /// Gets or sets the AttributeId of the <see cref="Rock.Model.Attribute"/> that this AttributeValue provides a value for.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the AttributeId of the <see cref="Rock.Model.Attribute"/> that this AttributeValue provides a value for.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
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
        [LavaIgnore]
        public int? EntityId { get; set; }
        
        /// <summary>
        /// Gets or sets the value
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the value.
        /// </value>
        [DataMember]
        public string Value {get; set;}

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets the Value as a double (Computed Column)
        /// </summary>
        /// <value>
        /// </value>
        /* Computed Column Spec:
        CASE 
        WHEN len([value]) < (100)
            AND isnumeric([value]) = (1)
            AND NOT [value] LIKE '%[^0-9.]%'
            AND NOT [value] LIKE '%[.]%'
            THEN CONVERT([numeric](38, 10), [value])
        END         
         */
        [DataMember]
        [DatabaseGenerated( DatabaseGeneratedOption.Computed )]
        [LavaIgnore]
        public decimal? ValueAsNumeric { get; set; }

        /// <summary>
        /// Gets the Value as a DateTime (Computed Column)
        /// </summary>
        /// <remarks>
        /// Computed Column Spec:
        /// CASE 
        /// -- make sure it isn't a big value or a date range, etc
        /// WHEN LEN([value]) &lt;= 33
        ///    THEN CASE 
        ///            -- is it an ISO-8601
        ///            WHEN VALUE LIKE '____-__-__T__:__:__%'
        ///                THEN CONVERT(DATETIME, CONVERT(DATETIMEOFFSET, [value]))
        ///            -- is it some other value SQL Date
        ///            WHEN ISDATE([VALUE]) = 1
        ///                THEN CONVERT(DATETIME, [VALUE])
        ///            ELSE NULL
        ///            END
        /// ELSE NULL    
        /// END
        /// </remarks>
        [DataMember]
        [DatabaseGenerated( DatabaseGeneratedOption.Computed )]
        [LavaIgnore]
        public DateTime? ValueAsDateTime { get; private set; }

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
        [LavaIgnore]
        public int? ValueAsPersonId { get; private set; }

        /// <summary>
        /// Gets the <see cref="Rock.Model.FieldType"/> that represents the type of value that is being represented by the AttributeValue, and provides a UI for the user to set the value.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.FieldType"/> that is represented by this Attribute Value.
        /// </value>
        [NotMapped]
        private Rock.Field.IFieldType FieldType
        {
            get
            {

                Rock.Field.IFieldType result = null;
                Rock.Web.Cache.AttributeCache attribute = Rock.Web.Cache.AttributeCache.Read( this.AttributeId );
                if (attribute != null)
                {
                  if (attribute.FieldType != null)
                  {
                    result = attribute.FieldType.Field;
                  }
                }

                return result;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Attribute"/> that uses this AttributeValue.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Attribute"/> that uses this value.
        /// </value>
        [DataMember]
        [LavaIgnore]
        public virtual Attribute Attribute { get; set; }

        /// <summary>
        /// Gets the value formatted.
        /// </summary>
        /// <value>
        /// The value formatted.
        /// </value>
        [LavaInclude]
        public virtual string ValueFormatted
        {
            get
            {
                var attribute = AttributeCache.Read( this.AttributeId );
                if ( attribute != null )
                {
                    return attribute.FieldType.Field.FormatValue( null, Value, attribute.QualifierValues, false);
                }
                return Value;
            }
        }

        /// <summary>
        /// Gets the name of the attribute 
        /// </summary>
        /// <remarks>
        /// Note: this property is provided specifically for Lava templates when the Attribute property is not available
        /// as a navigable property
        /// </remarks>
        /// <value>
        /// The name of the attribute.
        /// </value>
        [LavaInclude]
        public virtual string AttributeName
        {
            get
            {
                var attribute = AttributeCache.Read( this.AttributeId );
                if ( attribute != null )
                {
                    return attribute.Name;
                }
                return Value;
            }
        }

        /// <summary>
        /// Gets the attribute key.
        /// </summary>
        /// <remarks>
        /// Note: this property is provided specifically for Lava templates when the Attribute property is not available
        /// as a navigable property
        /// </remarks>
        /// <value>
        /// The attribute key.
        /// </value>
        [LavaInclude]
        public virtual string AttributeKey
        {
            get
            {
                var attribute = AttributeCache.Read( this.AttributeId );
                if ( attribute != null )
                {
                    return attribute.Key;
                }
                return Value;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Pres the save.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="entry">The entry.</param>
        public override void PreSaveChanges( Rock.Data.DbContext dbContext, System.Data.Entity.Infrastructure.DbEntityEntry entry )
        {
            var attributeCache = AttributeCache.Read( this.AttributeId );
            if (attributeCache != null)
            {
                // Check to see if this attribute value if for a Field or Image field type 
                // ( we don't want BinaryFileFieldType as that type of attribute's file can be used by more than one attribute )
                var field = attributeCache.FieldType.Field;
                if ( field != null && (
                    field is Rock.Field.Types.FileFieldType ||
                    field is Rock.Field.Types.ImageFieldType ) )
                {
                    Guid? newBinaryFileGuid = null;
                    Guid? oldBinaryFileGuid = null;

                    if ( entry.State == System.Data.Entity.EntityState.Added ||
                        entry.State == System.Data.Entity.EntityState.Modified )
                    {
                        newBinaryFileGuid = Value.AsGuidOrNull();
                    }

                    if ( entry.State == System.Data.Entity.EntityState.Modified ||
                        entry.State == System.Data.Entity.EntityState.Deleted )
                    {
                        if ( entry.OriginalValues["Value"] != null )
                        {
                            oldBinaryFileGuid = entry.OriginalValues["Value"].ToString().AsGuidOrNull();
                        }
                    }

                    if ( oldBinaryFileGuid.HasValue )
                    {
                        if ( !newBinaryFileGuid.HasValue || !newBinaryFileGuid.Value.Equals( oldBinaryFileGuid.Value ) )
                        {
                            var transaction = new Rock.Transactions.DeleteAttributeBinaryFile( oldBinaryFileGuid.Value );
                            Rock.Transactions.RockQueue.TransactionQueue.Enqueue( transaction );
                        }
                    }

                    if ( newBinaryFileGuid.HasValue )
                    {
                        BinaryFileService binaryFileService = new BinaryFileService( (RockContext)dbContext );
                        var binaryFile = binaryFileService.Get( newBinaryFileGuid.Value );
                        if ( binaryFile != null && binaryFile.IsTemporary )
                        {
                            binaryFile.IsTemporary = false;
                        }
                    }
                }
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
