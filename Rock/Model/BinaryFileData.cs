//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents the data/content of a <see cref="Rock.Model.BinaryFile"/> this entity can either be used to temporary store the 
    /// file content in memory or can be persisted to the database. 
    /// </summary>
    [Table( "BinaryFileData" )]
    [DataContract]
    public partial class BinaryFileData : Model<BinaryFileData>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the data/content of a <see cref="Rock.Model.BinaryFile"/>
        /// </summary>
        /// <value>
        /// A byte array that contains the data/content of a <see cref="Rock.Model.BinaryFile"/>
        /// </value>
        [DataMember]
        public byte[] Content { get; set; }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// File Configuration class.
    /// </summary>
    public partial class BinaryFileDataConfiguration : EntityTypeConfiguration<BinaryFileData>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryFileDataConfiguration"/> class.
        /// </summary>
        public BinaryFileDataConfiguration()
        {
        }
    }

    #endregion

}
