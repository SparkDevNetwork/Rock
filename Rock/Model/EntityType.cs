//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
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
    [Table( "EntityType" )]
    [DataContract]
    public partial class EntityType : Entity<EntityType>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the full name of the EntityType (including the namespace). This value is required and is an alternate key.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the full name of the EntityType.
        /// </value>
        [MaxLength( 100 )]
        [AlternateKey]
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the assembly name of the EntityType. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Assembly Name of the EntityType.
        /// </value>
        [MaxLength( 260 )]
        [DataMember]
        public string AssemblyName { get; set; }

        /// <summary>
        /// Gets or sets the friendly name of the EntityType (the class name).
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the friendly name of the Entity Type.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string FriendlyName { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating whether this entity type implements the
        /// IEntity interface.
        /// </summary>
        /// <value>
        ///  A <see cref="System.Boolean"/> value that is  <c>true</c> if this instance is an entity; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsEntity { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating whether this entity type implements the
        /// ISecured interface.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this instance is secured; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsSecured { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating whether this entity type is a commonly used entity.
        /// If so, it will grouped at the top by the entity type picker control
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this instance is common; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsCommon { get; set; }

        #endregion

        #region virtual Properties

        /// <summary>
        /// Gets a flag  indicating whether this instance is part of the RockChMS core system/framework.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this instance is system; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsSystem
        {
            get { return IsSecured || IsEntity; }
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
    /// Entity Type Configuration class.
    /// </summary>
    public partial class EntityTypeConfiguration : EntityTypeConfiguration<EntityType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityTypeConfiguration"/> class.
        /// </summary>
        public EntityTypeConfiguration()
        {
        }
    }

    #endregion

}
