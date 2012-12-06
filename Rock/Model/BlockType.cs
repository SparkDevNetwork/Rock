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
using Newtonsoft.Json;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Block Type POCO Entity.
    /// </summary>
    [Table( "BlockType" )]
    public partial class BlockType : Model<BlockType>, IExportable
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
        /// Gets or sets the Path.
        /// </summary>
        /// <value>
        /// Path.
        /// </value>
        [Required]
        [MaxLength( 200 )]
        public string Path { get; set; }
        
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
        /// Gets or sets the Blocks.
        /// </summary>
        /// <value>
        /// Collection of Blocks.
        /// </value>
        [NotExportable]
        public virtual ICollection<Block> Blocks { get; set; }

        /// <summary>
        /// Gets the dto.
        /// </summary>
        /// <returns></returns>
        [NotExportable]
        public override IDto Dto
        {
            get { return this.ToDto(); }
        }

        /// <summary>
        /// Static Method to return an object based on the id
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static BlockType Read( int id )
        {
            return Read<BlockType>( id );
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

        /// <summary>
        /// Exports the object as JSON.
        /// </summary>
        /// <returns></returns>
        public string ExportJson()
        {
            return ExportObject().ToJSON();
        }

        /// <summary>
        /// Exports the object.
        /// </summary>
        /// <returns></returns>
        public object ExportObject()
        {
            return this.ToDynamic();
        }

        /// <summary>
        /// Imports the object from JSON.
        /// </summary>
        /// <param name="data">The data.</param>
        public void ImportJson(string data)
        {
            JsonConvert.PopulateObject( data, this );
        }

    }

    /// <summary>
    /// Block Type Configuration class.
    /// </summary>
    public partial class BlockTypeConfiguration : EntityTypeConfiguration<BlockType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlockTypeConfiguration"/> class.
        /// </summary>
        public BlockTypeConfiguration()
        {
        }
    }
}
