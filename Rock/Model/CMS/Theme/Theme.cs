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

using Rock.Attribute;
using Rock.Data;
using Rock.Web.Cache;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    [RockDomain( "CMS" )]
    [Table( "Theme" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.THEME )]
    public partial class Theme : Model<Theme>, IHasActiveFlag, IHasAdditionalSettings
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
        /// Gets or sets the root path.
        /// </summary>
        /// <value>
        /// The root path.
        /// </value>
        [DataMember]
        public string RootPath { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets a flag indicating if this <see cref="Rock.Model.Theme"/> is a part of the Rock core system/framework. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this <see cref="Rock.Model.Theme"/>. is part of the Rock core system/framework; otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Id of the Record Status <see cref="Rock.Model.DefinedValue"/> representing the purpose of this theme.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the Record Status <see cref="Rock.Model.DefinedValue"/> representing the purpose of this theme.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.THEME_PURPOSE )]
        public int? PurposeValueId { get; set; }

        /// <summary>
        /// Gets or sets the additional settings json.
        /// </summary>
        /// <value>
        /// The additional settings json.
        /// </value>
        [DataMember]
        public string AdditionalSettingsJson { get; set; }

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.DefinedValue"/> representing the purpose of this theme.
        /// </summary>
        /// <value>
        /// A <see cref="DefinedValue"/> object representing the purpose of this theme.
        /// </value>
        [DataMember]
        public virtual DefinedValue PurposeValue { get; set; }

        #endregion Navigation Properties
    }

    #region Entity Configuration

    /// <summary>
    /// Theme Configuration class.
    /// </summary>
    public partial class ThemeConfiguration : EntityTypeConfiguration<Theme>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ThemeConfiguration"/> class.
        /// </summary>
        public ThemeConfiguration()
        {
            this.HasOptional( p => p.PurposeValue ).WithMany().HasForeignKey( p => p.PurposeValueId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
