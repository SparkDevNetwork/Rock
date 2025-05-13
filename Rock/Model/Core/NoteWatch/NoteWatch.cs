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
using Rock.Data;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

namespace Rock.Model
{
    /// <summary>
    /// NoteWatch
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "NoteWatch" )]
    [DataContract]
    [CodeGenerateRest]
    [Rock.SystemGuid.EntityTypeGuid( "A5C129C2-E64D-4B72-B94D-DBA6DA6AC2E3")]
    public partial class NoteWatch : Model<NoteWatch>
    {
        #region Entity Properties

        /// <summary>
        /// Set NoteTypeId to watch all notes of a specific note type
        /// Set NoteTypeId and EntityId to watch all notes of a specific type as it relates to a specific entity 
        /// </summary>
        /// <value>
        /// The note type identifier.
        /// </value>
        [DataMember]
        public int? NoteTypeId { get; set; }

        /// <summary>
        /// Set EntityTypeId and EntityId to watch all notes for a specific entity
        /// </summary>
        /// <value>
        /// The entity type identifier.
        /// </value>
        [DataMember]
        public int? EntityTypeId { get; set; }

        /// <summary>
        /// Set EntityTypeId and EntityId to watch all notes for a specific entity
        /// NOTE: If EntityType is Person, make sure to watch the Person's PersonAlias' Persons
        /// </summary>
        /// <value>
        /// The entity identifier.
        /// </value>
        [DataMember]
        public int? EntityId { get; set; }

        /// <summary>
        /// Set NoteId to watch a specific note
        /// </summary>
        /// <value>
        /// The note identifier.
        /// </value>
        [DataMember]
        public int? NoteId { get; set; }

        /// <summary>
        /// Set IsWatching to False to disable this NoteWatch (or specifically don't watch based on the notewatch criteria)
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is watching; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsWatching { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether [watch replies].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [watch replies]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool WatchReplies { get; set; } = false;

        /// <summary>
        /// Set AllowOverride to False to prevent people from adding an IsWatching=False on NoteWatch with the same filter that is marked as IsWatching=True
        /// In other words, if a group is configured a NoteWatch, an individual shouldn't be able to add an un-watch if AllowOverride=False (and any un-watches that may have been already added would be ignored)
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow override]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool AllowOverride { get; set; } = true;

        /// <summary>
        /// Gets or sets the person alias id of the person watching this note watch
        /// </summary>
        /// <value>
        /// The person alias identifier.
        /// </value>
        [DataMember]
        public int? WatcherPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the group that is watching this note watch
        /// </summary>
        /// <value>
        /// The group identifier.
        /// </value>
        [DataMember]
        public int? WatcherGroupId { get; set; }

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the type of the note.
        /// </summary>
        /// <value>
        /// The type of the note.
        /// </value>
        [DataMember]
        public virtual NoteType NoteType { get; set; }

        /// <summary>
        /// Gets or sets the type of the entity.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        [DataMember]
        public virtual EntityType EntityType { get; set; }

        /// <summary>
        /// Gets or sets the note.
        /// </summary>
        /// <value>
        /// The note.
        /// </value>
        [DataMember]
        public virtual Note Note { get; set; }

        /// <summary>
        /// Gets or sets the person alias of the person watching this note watch
        /// </summary>
        /// <value>
        /// The person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias WatcherPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the group that is watching this note watch
        /// </summary>
        /// <value>
        /// The group.
        /// </value>
        [DataMember]
        public virtual Group WatcherGroup { get; set; }

        #endregion Navigation Properties
    }

    #region Entity Configuration

    /// <summary>
    /// NoteWatch Configuration Class
    /// </summary>
    public partial class NoteWatchConfiguration : EntityTypeConfiguration<NoteWatch>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NoteWatchConfiguration"/> class.
        /// </summary>
        public NoteWatchConfiguration()
        {
            this.HasOptional( a => a.NoteType ).WithMany().HasForeignKey( a => a.NoteTypeId ).WillCascadeOnDelete( false );

            this.HasOptional( a => a.EntityType ).WithMany().HasForeignKey( a => a.EntityTypeId ).WillCascadeOnDelete( true );
            this.HasOptional( a => a.Note ).WithMany().HasForeignKey( a => a.NoteId ).WillCascadeOnDelete( true );
            this.HasOptional( a => a.WatcherPersonAlias ).WithMany().HasForeignKey( a => a.WatcherPersonAliasId ).WillCascadeOnDelete( true );
            this.HasOptional( a => a.WatcherGroup ).WithMany().HasForeignKey( a => a.WatcherGroupId ).WillCascadeOnDelete( true );
        }
    }

    #endregion Entity Configuration    
}
