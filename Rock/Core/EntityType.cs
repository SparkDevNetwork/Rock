//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Core
{
    [Table( "coreEntityType" )]
    public partial class EntityType
    {
        [Key]
        [DataMember]
        public int Id { get; set; }

        /// <summary>
        /// Gets or 
        /// </summary>
        /// <value>
        /// The GUID.
        /// </value>
        [AlternateKey]
        [DataMember]
        public Guid Guid
        {
            get { return _guid; }
            set { _guid = value; }
        }
        private Guid _guid = Guid.NewGuid();

        [MaxLength( 100 )]
        [DataMember]
        public string Name { get; set; }
        
        [MaxLength( 100 )]
        [DataMember]
        public string FriendlyName { get; set; }
        
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
    }

    /// <summary>
    /// Attribute Configuration class.
    /// </summary>
    public partial class EntityTypeConfiguration : EntityTypeConfiguration<Tag>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeConfiguration"/> class.
        /// </summary>
        public EntityTypeConfiguration()
        {
        }
    }
}
