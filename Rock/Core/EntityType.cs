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
    /// <summary>
    /// 
    /// </summary>
    [Table( "coreEntityType" )]
    public partial class EntityType : Entity<EntityType>
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [MaxLength( 100 )]
        [AlternateKey]
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the friendly.
        /// </summary>
        /// <value>
        /// The name of the friendly.
        /// </value>
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
    public partial class EntityTypeConfiguration : EntityTypeConfiguration<EntityType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeConfiguration"/> class.
        /// </summary>
        public EntityTypeConfiguration()
        {
        }
    }
}
