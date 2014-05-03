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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// The plugin migrations that have bee run
    /// </summary>
    [Table( "PluginMigration" )]
    [DataContract]
    public partial class PluginMigration : Model<PluginMigration>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the name of the plugin type.
        /// </summary>
        /// <value>
        /// The name of the plugin type.
        /// </value>
        [Required]
        [MaxLength(200)]
        [DataMember( IsRequired = true )]
        public string PluginTypeName { get; set; }

        /// <summary>
        /// Gets or sets the plugin version.
        /// </summary>
        /// <value>
        /// The plugin version.
        /// </value>
        [Required]
        [MaxLength( 50 )]
        [DataMember]
        public string PluginVersion { get; set; }
        
        #endregion

        #region Virtual Properties

        #endregion

        #region Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this PluginMigration.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this PluginMigration.
        /// </returns>
        public override string ToString()
        {
            return string.Format( "{0} ({1})", this.PluginTypeName, this.PluginVersion );
        }

        #endregion

    }

    #region Entity Configuration
    
    /// <summary>
    /// PluginMigration Configuration class.
    /// </summary>
    public partial class PluginMigrationConfiguration : EntityTypeConfiguration<PluginMigration>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PluginMigrationConfiguration" /> class.
        /// </summary>
        public PluginMigrationConfiguration()
        {
        }
    }

    #endregion

}
