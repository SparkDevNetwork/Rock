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
using Rock.Enums.Security;
using Rock.Lava;
using Rock.Security;
using Rock.Utility;

namespace Rock.Model
{
    /// <summary>
    /// Represents one configured module of a <see cref="Rock.Model.ModuleType"/>, whose settings are its attribute values.
    /// </summary>
    /// <remarks>
    /// A Canvas block displays an instance by referencing it from a block attribute value, so the
    /// same shareable instance can be displayed in more than one place.
    /// </remarks>
    [RockDomain( "CMS" )]
    [Table( "ModuleInstance" )]
    [DataContract]
    [CodeGenExclude( CodeGenFeature.DefaultRestController )] // Do not generate a v1 API controller.
    [CodeGenerateRest]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MODULE_INSTANCE )]
    public partial class ModuleInstance : Model<ModuleInstance>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the name of the module instance.
        /// </summary>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        [StringValidation( StringValidationProfile.Name )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.ModuleType"/> this is an instance of.
        /// </summary>
        /// <remarks>
        /// Also the qualifier for this instance's attributes, so every instance of a type has that type's settings.
        /// </remarks>
        [Required]
        [DataMember( IsRequired = true )]
        public int ModuleTypeId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the instance can be displayed in more than one place.
        /// </summary>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsShareable { get; set; }

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.ModuleType"/> this is an instance of.
        /// </summary>
        [DataMember]
        [LavaVisible]
        public virtual ModuleType ModuleType { get; set; }

        #endregion Navigation Properties

        #region Public Methods

        /// <summary>
        /// Returns the name of the module instance.
        /// </summary>
        /// <returns>The name of the module instance.</returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion Public Methods
    }

    #region Entity Configuration

    /// <summary>
    /// ModuleInstance Configuration class.
    /// </summary>
    public partial class ModuleInstanceConfiguration : EntityTypeConfiguration<ModuleInstance>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleInstanceConfiguration"/> class.
        /// </summary>
        public ModuleInstanceConfiguration()
        {
            this.HasRequired( i => i.ModuleType ).WithMany().HasForeignKey( i => i.ModuleTypeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion Entity Configuration
}
