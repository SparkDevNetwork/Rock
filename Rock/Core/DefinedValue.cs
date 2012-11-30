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

namespace Rock.Core
{
    /// <summary>
    /// Defined Value POCO Entity.
    /// </summary>
    [Table( "DefinedValue" )]
    public partial class DefinedValue : Model<DefinedValue>, IOrdered
    {
        /// <summary>
        /// Gets or sets the System.
        /// </summary>
        /// <value>
        /// System.
        /// </value>
        [Required]
        public bool IsSystem { get; set; }
        
        /// <summary>
        /// Gets or sets the Defined Type Id.
        /// </summary>
        /// <value>
        /// Defined Type Id.
        /// </value>
        [Required]
        public int DefinedTypeId { get; set; }
        
        /// <summary>
        /// Gets or sets the Order.
        /// </summary>
        /// <value>
        /// Order.
        /// </value>
        [Required]
        public int Order { get; set; }
        
        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        /// <value>
        /// Name.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets the Description.
        /// </summary>
        /// <value>
        /// Description.
        /// </value>
        public string Description { get; set; }
        
        /// <summary>
        /// Gets or sets the Defined Type.
        /// </summary>
        /// <value>
        /// A <see cref="DefinedType"/> object.
        /// </value>
        public virtual DefinedType DefinedType { get; set; }

        /// <summary>
        /// Gets the dto.
        /// </summary>
        /// <returns></returns>
        public override IDto Dto
        {
            get { return this.ToDto(); }
        }

        /// <summary>
        /// Static Method to return an object based on the id
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static DefinedValue Read( int id )
        {
            return Read<DefinedValue>( id );
        }

        /// <summary>
        /// Gets the parent authority.
        /// </summary>
        public override Security.ISecured ParentAuthority
        {
            get { return new Security.GenericEntity( "Global" ); }
        }

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
    /// Defined Value Configuration class.
    /// </summary>
    public partial class DefinedValueConfiguration : EntityTypeConfiguration<DefinedValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefinedValueConfiguration"/> class.
        /// </summary>
        public DefinedValueConfiguration()
        {
            this.HasRequired( p => p.DefinedType ).WithMany( p => p.DefinedValues ).HasForeignKey( p => p.DefinedTypeId ).WillCascadeOnDelete(false);
        }
    }
}
