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

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Lava;

namespace Rock.Model
{
    /// <summary>
    /// Represents author-defined Obsidian UI (a compiled Vue component and its
    /// original source) owned by a single block placement.
    /// </summary>
    /// <remarks>
    /// This is the storage behind the <c>ObsidianContentDetail</c> block. An
    /// administrator writes Vue source in place; the source is compiled to a
    /// browser-loadable SystemJS module in the administrator's browser and both
    /// the source and the compiled output are persisted here. Visitors are served
    /// only the precompiled output. The nullable <see cref="BlockId"/> reserves
    /// room for a future reusable-library record that is not tied to a block.
    /// </remarks>
    [RockDomain( "CMS" )]
    [Table( "ObsidianContent" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( "38F182A7-9FE4-4D7B-B483-59F615BDE41C" )]
    public partial class ObsidianContent : Model<ObsidianContent>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Block"/> placement that owns this content.
        /// </summary>
        /// <remarks>
        /// Per-instance content is owned by a single block placement (the <c>HtmlContent</c> pattern).
        /// A <c>null</c> value is reserved for a future reusable-library record that is not tied to a block.
        /// </remarks>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the owning <see cref="Rock.Model.Block"/>, or <c>null</c> for a reusable record.
        /// </value>
        [DataMember]
        public int? BlockId { get; set; }

        /// <summary>
        /// Gets or sets an optional name for this content.
        /// </summary>
        /// <remarks>
        /// Unused in the per-instance flow; present for the future reusable library.
        /// </remarks>
        /// <value>
        /// A <see cref="System.String"/> representing the name of the content.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the clean Vue source the author wrote.
        /// </summary>
        /// <remarks>
        /// This is the source of truth for editing and for recompiling on a future Rock upgrade.
        /// </remarks>
        /// <value>
        /// A <see cref="System.String"/> containing the authored Vue source.
        /// </value>
        [DataMember]
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the compiled SystemJS module string that is served to browsers.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> containing the compiled component module.
        /// </value>
        [DataMember]
        public string CompiledContent { get; set; }

        /// <summary>
        /// Gets or sets the Vue version the stored <see cref="CompiledContent"/> was compiled against.
        /// </summary>
        /// <remarks>
        /// Used to decide whether the content should be recompiled after a Rock (and therefore Vue) upgrade.
        /// </remarks>
        /// <value>
        /// A <see cref="System.String"/> representing the targeted Vue version.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string CompiledVueVersion { get; set; }

        /// <summary>
        /// Gets or sets the date and time the stored <see cref="CompiledContent"/> was produced.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing when the compile occurred, or <c>null</c> if it has never been compiled.
        /// </value>
        [DataMember]
        public DateTime? CompiledDateTime { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this content is active.
        /// </summary>
        /// <remarks>
        /// Present for the future reusable library; per-instance records are active.
        /// </remarks>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if the content is active; otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsActive { get; set; } = true;

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Block"/> placement that owns this content.
        /// </summary>
        /// <value>
        /// The owning <see cref="Rock.Model.Block"/>.
        /// </value>
        [LavaVisible]
        public virtual Block Block { get; set; }

        #endregion Navigation Properties

        #region Methods

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.IsNullOrWhiteSpace( Name ) ? base.ToString() : Name;
        }

        #endregion Methods
    }

    #region Entity Configuration

    /// <summary>
    /// ObsidianContent Configuration class.
    /// </summary>
    public partial class ObsidianContentConfiguration : EntityTypeConfiguration<ObsidianContent>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObsidianContentConfiguration"/> class.
        /// </summary>
        public ObsidianContentConfiguration()
        {
            /*
                7/22/2026 - CLAUDE

                The BlockId foreign key cascades on delete. This is the rare parent-child
                ownership case: a per-instance record has no meaning once its owning block
                placement is gone. Future reusable-library records carry a null BlockId and
                are therefore unaffected by any block deletion.

                Reason: Per-instance content is owned by its block; orphaned rows are useless.
            */
            this.HasOptional( c => c.Block ).WithMany().HasForeignKey( c => c.BlockId ).WillCascadeOnDelete( true );
        }
    }

    #endregion Entity Configuration
}
