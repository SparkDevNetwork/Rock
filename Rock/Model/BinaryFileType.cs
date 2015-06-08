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
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents a type or category of binary files in Rock, and configures how binary files of this type are stored and accessed.
    /// </summary>
    [Table( "BinaryFileType" )]
    [DataContract]
    public partial class BinaryFileType : Model<BinaryFileType>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets a flag indicating if this BinaryFileType is part of the Rock core system/framework. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this is part of the core system/framework; otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the given Name of the BinaryFileType. This value is an alternate key and is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the given Name of the BinaryFileType. 
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a description of the BinaryFileType.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the description of the BinaryFileType.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the CSS class that is used for a vector/CSS icon.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the CSS class that is used for a vector/CSS based icon.
        /// </value>
        [DataMember]
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the Id of the storage service <see cref="Rock.Model.EntityType"/> that is used to store files of this type.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32" /> representing the Id of the storage service <see cref="Rock.Model.EntityType"/>.
        /// </value>
        [DataMember]
        public int? StorageEntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating whether to allow caching on any <see cref="Rock.Model.BinaryFile"/> child entities.
        /// </summary>
        /// <value>
        ///  A <see cref="System.Boolean"/> value that is <c>true</c> if caching is allowed; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool AllowCaching { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether security should be checked when displaying files of this type
        /// </summary>
        /// <value>
        /// <c>true</c> if [requires view security]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool RequiresViewSecurity { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the maximum width of a file type.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the maximum width in pixels of a file type.
        /// </value>
        [DataMember]
        public int? MaxWidth { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the maximum height of a file type.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the maximum height in pixels of a file type.
        /// </value>
        [DataMember]
        public int? MaxHeight { get; set; }

        /// <summary>
        /// Gets or sets the preferred format of the file type.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.Format"/> enum value that represents the preferred format of the file type.
        /// </value>
        [DataMember]
        public Format PreferredFormat { get; set; }

        /// <summary>
        /// Gets or sets the preferred resolution of the file type.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.Resolution"/> enum value that represents the preferred resolution of the file type.
        /// </value>
        [DataMember]
        public Resolution PreferredResolution { get; set; }

        /// <summary>
        /// Gets or sets the preferred color depth of the file type.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.ColorDepth"/> enum value that represents the preferred color depth of the file type.
        /// </value>
        [DataMember]
        public ColorDepth PreferredColorDepth { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the preferred attributes of the file type are required
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if the "preferred" attributes are required; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool PreferredRequired { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Person"/> class.
        /// </summary>
        public BinaryFileType()
            : base()
        {
            PreferredFormat = Format.Undefined;
            PreferredResolution = Resolution.Undefined;
            PreferredColorDepth = ColorDepth.Undefined;
        }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets the count of <see cref="Rock.Model.BinaryFile" /> entities that are children of this <see cref="Rock.Model.BinaryFileType"/>.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the count of <see cref="Rock.Model.BinaryFile"/> entities that are children of this <see cref="Rock.Model.BinaryFileType"/>.
        /// </value>
        public virtual int FileCount
        {
            get
            {
                return FileQuery.Count();
            }
        }

        /// <summary>
        /// Gets a queryable collection of <see cref="Rock.Model.BinaryFile" /> entities that are children of this <see cref="Rock.Model.BinaryFileType"/>.
        /// </summary>
        /// <value>
        /// A queryable collection of <see cref="Rock.Model.BinaryFile"/> entities that are children of this<see cref="Rock.Model.BinaryFileType"/>.
        /// </value>
        public virtual IQueryable<BinaryFile> FileQuery
        {
            get
            {
                var fileService = new BinaryFileService( new RockContext() );
                var qry = fileService.Queryable()
                    .Where( f => f.BinaryFileTypeId.HasValue && f.BinaryFileTypeId == this.Id );
                return qry;
            }
        }

        /// <summary>
        /// Gets or sets the storage mode <see cref="Rock.Model.EntityType"/>.
        /// </summary>
        /// <value>
        /// The storage mode <see cref="Rock.Model.EntityType"/>.
        /// </value>
        [DataMember]
        public virtual EntityType StorageEntityType { get; set; }

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
    /// File Configuration class.
    /// </summary>
    public partial class BinaryFileTypeConfiguration : EntityTypeConfiguration<BinaryFileType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryFileTypeConfiguration"/> class.
        /// </summary>
        public BinaryFileTypeConfiguration()
        {
        }
    }

    #endregion

    #region Enumerations

    /// <summary>
    /// Represents the preferred format of the binary file type.
    /// </summary>
    public enum Format
    {
        /// <summary>
        /// The undefined.
        /// </summary>
        Undefined = -1,

        /// <summary>
        /// The preferred format is as a .JPG file.
        /// </summary>
        JPG = 0,

        /// <summary>
        /// The preferred format is as a .GIF file.
        /// </summary>
        GIF = 1,

        /// <summary>
        /// The preferred format is as a .PNG file.
        /// </summary>
        PNG = 2,

        /// <summary>
        /// The preferred format is as a .PDF file.
        /// </summary>
        PDF = 3,

        /// <summary>
        /// The preferred format is as a Word document.
        /// </summary>
        Word = 4,

        /// <summary>
        /// The preferred format is as an Excel document.
        /// </summary>
        Excel = 5,

        /// <summary>
        /// The preferred format is as a text file.
        /// </summary>
        Text = 6,

        /// <summary>
        /// The preferred format is as an HTML document.
        /// </summary>
        HTML = 7
    }

    /// <summary>
    /// Represents the preferred resolution of the binary file type.
    /// </summary>
    public enum Resolution
    {
        /// <summary>
        /// The undefined.
        /// </summary>
        Undefined = -1,

        /// <summary>
        /// A preferred resolution of 72 DPI.
        /// </summary>
        DPI72 = 0,

        /// <summary>
        /// A preferred resolution of 150 DPI.
        /// </summary>
        DPI150 = 1,

        /// <summary>
        /// A preferred resolution of 300 DPI.
        /// </summary>
        DPI300 = 2,

        /// <summary>
        /// A preferred resolution of 600 DPI.
        /// </summary>
        DPI600 = 3
    }

    /// <summary>
    /// Represents the preferred color depth of the binary file type.
    /// </summary>
    public enum ColorDepth
    {
        /// <summary>
        /// An undefined color depth.
        /// </summary>
        Undefined = -1,

        /// <summary>
        /// A preferred color depth of Black and White.
        /// </summary>
        BlackWhite = 0,

        /// <summary>
        /// A preferred color depth of 8-bit Grayscale.
        /// </summary>
        Grayscale8bit = 1,

        /// <summary>
        /// A preferred color depth of 24-bit Grayscale.
        /// </summary>
        Grayscale24bit = 2,

        /// <summary>
        /// A preferred color depth of 8-bit Color.
        /// </summary>
        Color8bit = 3,

        /// <summary>
        /// A preferred color depth of 24-bit Color.
        /// </summary>
        Color24bit = 4
    }

    #endregion
}
