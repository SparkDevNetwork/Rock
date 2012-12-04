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
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    [Table( "EntityType" )]
    public partial class EntityType : Entity<EntityType>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [MaxLength( 100 )]
        [AlternateKey]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the assembly name.
        /// </summary>
        /// <value>
        /// The assembly name.
        /// </value>
        [MaxLength( 200 )]
        public string AssemblyName { get; set; }

        /// <summary>
        /// Gets or sets the name of the friendly.
        /// </summary>
        /// <value>
        /// The name of the friendly.
        /// </value>
        [MaxLength( 100 )]
        public string FriendlyName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this entity type implements the
        /// IEntity interface.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is an entity; otherwise, <c>false</c>.
        /// </value>
        public bool IsEntity { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this entity type implements the
        /// ISecured interface.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is secured; otherwise, <c>false</c>.
        /// </value>
        public bool IsSecured { get; set; }

        #endregion

        #region virtual Properties

        /// <summary>
        /// Gets a value indicating whether this instance is system.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is system; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsSystem
        {
            get { return IsSecured || IsEntity; }
        }

        /// <summary>
        /// Gets the dto.
        /// </summary>
        /// <returns></returns>
        public override IDto Dto
        {
            get { return this.ToDto(); }
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

        #region Static Methods

        /// <summary>
        /// Reads the specified id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static EntityType Read( int id )
        {
            return Read<EntityType>( id );
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <returns></returns>
        public static EntityType Read( Guid guid )
        {
            return Read<EntityType>( guid );
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
