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
    /// File POCO Entity.
    /// </summary>
    [Table( "BinaryFileData" )]
    [DataContract]
    public partial class BinaryFileData : Model<BinaryFileData>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the content
        /// </summary>
        /// <value>
        /// content.
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
        /// Initializes a new instance of the <see cref="BinaryFileDatConfiguration"/> class.
        /// </summary>
        public BinaryFileDataConfiguration()
        {
        }
    }

    #endregion

}
