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
    [Table( "MergeDoc" )]
    [DataContract]
    public partial class MergeDoc : Model<MergeDoc>, ICategorized
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the template binary file identifier.
        /// </summary>
        /// <value>
        /// The template binary file identifier.
        /// </value>
        [DataMember]
        public int TemplateBinaryFileId { get; set; }

        /// <summary>
        /// Gets or sets the merge document provider entity type identifier.
        /// </summary>
        /// <value>
        /// The merge document provider entity type identifier.
        /// </value>
        [DataMember]
        public int MergeDocProviderEntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the category id.
        /// </summary>
        /// <value>
        /// The category id.
        /// </value>
        [DataMember]
        public int CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the person alias identifier.
        /// </summary>
        /// <value>
        /// The person alias identifier.
        /// </value>
        [DataMember]
        public int? PersonAliasId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the template binary file.
        /// </summary>
        /// <value>
        /// The template binary file.
        /// </value>
        public virtual BinaryFile TemplateBinaryFile { get; set; }

        /// <summary>
        /// Gets or sets the type of the merge document provider entity.
        /// </summary>
        /// <value>
        /// The type of the merge document provider entity.
        /// </value>
        public virtual EntityType MergeDocProviderEntityType { get; set; }

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        [DataMember]
        public virtual Category Category { get; set; }

        /// <summary>
        /// Gets or sets the person alias.
        /// </summary>
        /// <value>
        /// The person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias PersonAlias { get; set; }

        #endregion

        #region ICategorized

        /// <summary>
        /// Gets or sets the category id.
        /// </summary>
        /// <value>
        /// The category id.
        /// </value>
        int? ICategorized.CategoryId
        {
            get
            {
                return this.CategoryId;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns the Name of the MergeDoc 
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion

        #region Private Methods

        #endregion

        #region Static Methods

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// MergeDoc configuration class
    /// </summary>
    public partial class MergeDocConfiguration : EntityTypeConfiguration<MergeDoc>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MergeDocConfiguration"/> class.
        /// </summary>
        public MergeDocConfiguration()
        {
            this.HasRequired( c => c.TemplateBinaryFile ).WithMany().HasForeignKey( c => c.TemplateBinaryFileId ).WillCascadeOnDelete( false );
            this.HasRequired( c => c.MergeDocProviderEntityType ).WithMany().HasForeignKey( c => c.MergeDocProviderEntityTypeId ).WillCascadeOnDelete( false );
            this.HasRequired( c => c.Category ).WithMany().HasForeignKey( c => c.CategoryId ).WillCascadeOnDelete( false );
            this.HasOptional( c => c.PersonAlias ).WithMany().HasForeignKey( c => c.PersonAliasId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
