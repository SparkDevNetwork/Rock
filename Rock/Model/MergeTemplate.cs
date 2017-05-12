// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
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
using Rock.MergeTemplates;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    [Table( "MergeTemplate" )]
    [DataContract]
    public partial class MergeTemplate : Model<MergeTemplate>, ICategorized
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
        [IncludeForReporting]
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
        /// Gets or sets the merge template type entity type identifier.
        /// </summary>
        /// <value>
        /// The merge template type entity type identifier.
        /// </value>
        [DataMember]
        public int MergeTemplateTypeEntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the category id.
        /// </summary>
        /// <value>
        /// The category id.
        /// </value>
        [DataMember]
        [IncludeForReporting]
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
        [LavaInclude]
        public virtual BinaryFile TemplateBinaryFile { get; set; }

        /// <summary>
        /// Gets or sets the type of the merge template type entity.
        /// </summary>
        /// <value>
        /// The type of the merge template type entity.
        /// </value>
        [LavaInclude]
        public virtual EntityType MergeTemplateTypeEntityType { get; set; }

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
        /// Returns the Name of the MergeTemplate 
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

        /// <summary>
        /// Gets the MergeTemplateType MEF Component for the specified MergeTemplate
        /// </summary>
        /// <returns></returns>
        public MergeTemplateType GetMergeTemplateType()
        {
            var mergeTemplateTypeEntityType = EntityTypeCache.Read( this.MergeTemplateTypeEntityTypeId );
            if ( mergeTemplateTypeEntityType == null )
            {
                return null;
            }

            return MergeTemplateTypeContainer.GetComponent( mergeTemplateTypeEntityType.Name );
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// MergeTemplate configuration class
    /// </summary>
    public partial class MergeTemplateConfiguration : EntityTypeConfiguration<MergeTemplate>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MergeTemplateConfiguration"/> class.
        /// </summary>
        public MergeTemplateConfiguration()
        {
            this.HasRequired( c => c.TemplateBinaryFile ).WithMany().HasForeignKey( c => c.TemplateBinaryFileId ).WillCascadeOnDelete( false );
            this.HasRequired( c => c.MergeTemplateTypeEntityType ).WithMany().HasForeignKey( c => c.MergeTemplateTypeEntityTypeId ).WillCascadeOnDelete( false );
            this.HasRequired( c => c.Category ).WithMany().HasForeignKey( c => c.CategoryId ).WillCascadeOnDelete( false );
            this.HasOptional( c => c.PersonAlias ).WithMany().HasForeignKey( c => c.PersonAliasId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

    #region enums

    /// <summary>
    /// 
    /// </summary>
    public enum MergeTemplateOwnership
    {
        /// <summary>
        /// Show only global merge templates
        /// </summary>
        Global,

        /// <summary>
        /// Only show personal merge templates
        /// </summary>
        Personal,

        /// <summary>
        /// Show both personal and global merge templates
        /// </summary>
        PersonalAndGlobal
    }

    #endregion
}
