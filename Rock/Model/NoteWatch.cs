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
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "NoteWatch" )]
    [DataContract]
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

        #region Virtual Properties

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

        #endregion Virtual Properties

        #region Public Methods

        /// <summary>
        /// Returns true if this NoteWatch has valid "Watcher" parameters 
        /// </summary>
        /// <returns></returns>
        public bool IsValidWatcher
        {
            get
            {
                if ( this.WatcherPersonAliasId.HasValue || this.WatcherGroupId.HasValue )
                {
                    return true;
                }
                else
                {
                    // only add a ValidationResult if IsValid has already been called
                    if ( ValidationResults != null )
                    {
                        ValidationResults.Add( new ValidationResult( "An Person or Group must be specified as the watcher" ) );
                    }

                    return false;
                }
            }
        }

        /// <summary>
        /// Returns true if this NoteWatch has valid "Watch Filter" parameters 
        /// </summary>
        /// <returns></returns>
        public bool IsValidWatchFilter
        {
            get
            {
                if ( this.EntityTypeId.HasValue || this.NoteTypeId.HasValue || this.NoteId.HasValue )
                {
                    return true;
                }
                else
                {
                    // only add a ValidationResult if IsValid has already been called
                    if ( ValidationResults != null )
                    {
                        ValidationResults.Add( new ValidationResult( "An EntityType, NoteType, or specific note must be specified for the watch filter" ) );
                    }

                    return false;
                }
            }
        }

        /// <summary>
        ///  Determines whether if this note watch is able to set IsWatching to False without another notewatch completely preventing it
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>
        ///   <c>true</c> if [is able to un watch] [the specified rock context]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsAbleToUnWatch( RockContext rockContext )
        {
            return !GetNonOverridableNoteWatches( rockContext ).Any();
        }

        /// <summary>
        /// Returns all Non-Overridable NoteWatches that would prevent this NoteWatch from enabling IsWatching=False
        /// Returns a Queryable of zero or more NoteWatches
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>
        ///   <c>true</c> if [is allowed to unwatch]; otherwise, <c>false</c>.
        /// </returns>
        public IQueryable<NoteWatch> GetNonOverridableNoteWatches( RockContext rockContext )
        {
            var noteWatchService = new NoteWatchService( rockContext );

            // we are only concerned about other IsWatching watches that don't allow overrides
            var noteWatchesWithOverrideNotAllowedQuery = noteWatchService.Queryable().Where( a => a.AllowOverride == false && a.IsWatching && a.Id != this.Id );

            // limit to notewatches for the same watcher person (or where the watcher person is part of the watcher group)
            if ( this.WatcherPersonAliasId.HasValue )
            {
                var watcherPerson = this.WatcherPersonAlias?.Person ?? new PersonAliasService( rockContext ).Get( this.WatcherPersonAliasId.Value ).Person;

                // limit to watch that are watched by the same person, or watched by a group that a person is an active member of
                noteWatchesWithOverrideNotAllowedQuery = noteWatchesWithOverrideNotAllowedQuery
                    .Where( a =>
                        a.WatcherPersonAliasId.HasValue && a.WatcherPersonAlias.PersonId == this.WatcherPersonAlias.PersonId
                        ||
                        a.WatcherGroup.Members.Any( gm => gm.GroupMemberStatus == GroupMemberStatus.Active && gm.Person.Aliases.Any( x => x.PersonId == watcherPerson.Id ) )
                    );
            }
            else if ( this.WatcherGroupId.HasValue )
            {
                // if the watcher is a Group, make sure it isn't trying to override another watch where the watcher is the same group
                noteWatchesWithOverrideNotAllowedQuery = noteWatchesWithOverrideNotAllowedQuery.Where( a => a.WatcherGroupId.HasValue && a.WatcherGroupId.Value == this.WatcherGroupId.Value );
            }
            else
            {
                // invalid NoteWatch so just return a false query (no records)
                return noteWatchesWithOverrideNotAllowedQuery.Where( a => false );
            }

            NoteTypeCache noteType = null;
            if ( this.NoteTypeId.HasValue )
            {
                noteType = NoteTypeCache.Get( this.NoteTypeId.Value );
            }

            var noteWatchEntityTypeId = this.EntityTypeId ?? noteType?.EntityTypeId;

            //// Find NoteWatches that could override this note watch
            //// We are mostly looking for NoteWatches that are the same or less specific than this watch

            // at a minimum, the EntityType must be the same
            noteWatchesWithOverrideNotAllowedQuery = noteWatchesWithOverrideNotAllowedQuery.Where( a =>
                ( a.EntityTypeId.HasValue && a.EntityTypeId.Value == noteWatchEntityTypeId )
                || ( a.NoteTypeId.HasValue && a.NoteType.EntityTypeId == noteWatchEntityTypeId ) );

            // look for non-overridable notewatches that not specific to a NoteType, NoteId, or Entity,etc
            var globalEntityTypeNoteWatchesWithNoOverridesQuery = noteWatchesWithOverrideNotAllowedQuery.Where( a =>
                 a.NoteTypeId == null
                 && a.NoteId == null
                 && a.EntityId == null );

            if ( globalEntityTypeNoteWatchesWithNoOverridesQuery.Any() )
            {
                // found at least one non-overridable notewatch that covers all notes for the note watch, so it any attempt to unwatch wouldn't block due to the override
                return globalEntityTypeNoteWatchesWithNoOverridesQuery;
            }

            // if this notewatch is more specific, look for ones that are the same or less specific
            if ( this.NoteTypeId.HasValue )
            {
                var noteTypeNoteWatchesWithNoOverridesQuery = noteWatchesWithOverrideNotAllowedQuery.Where( a =>
                     ( a.NoteTypeId.HasValue && a.NoteTypeId.Value == this.NoteTypeId.Value )
                     && a.NoteId == null
                     && a.EntityId == null );

                if ( noteTypeNoteWatchesWithNoOverridesQuery.Any() )
                {
                    // found at least one non-overridable notewatch that covers all notes for the note watch and NoteType, so it any attempt to unwatch wouldn't block due to the override
                    return noteTypeNoteWatchesWithNoOverridesQuery;
                }
            }

            if ( this.NoteId.HasValue )
            {
                var noteNoteWatchesWithNoOverridesQuery = noteWatchesWithOverrideNotAllowedQuery.Where( a =>
                     ( a.NoteId.HasValue && a.NoteId.Value == this.NoteId.Value )
                     && a.EntityId == null );

                if ( noteNoteWatchesWithNoOverridesQuery.Any() )
                {
                    // found at least one non-overridable notewatch that covers this exact note for the note watch, so it any attempt to unwatch wouldn't block due to the override
                    return noteNoteWatchesWithNoOverridesQuery;
                }
            }

            if ( this.EntityId.HasValue )
            {
                var entityNoteWatchesWithNoOverridesQuery = noteWatchesWithOverrideNotAllowedQuery.Where( a =>
                     a.NoteTypeId == null
                    && a.NoteId == null
                    && ( a.EntityId.HasValue && ( a.EntityId == this.EntityId.Value ) ) );

                if ( entityNoteWatchesWithNoOverridesQuery.Any() )
                {
                    // found at least one non-overridable notewatch that covers this exact same entity for the note watch, so it any attempt to unwatch wouldn't block due to the override
                    return entityNoteWatchesWithNoOverridesQuery;
                }
            }

            // no non-overridable conditions found, so return a false query (no records)
            return noteWatchesWithOverrideNotAllowedQuery.Where( a => false );
        }

        /// <summary>
        /// Returns true if ... is valid.
        /// </summary>
        /// <value>
        /// A <see cref="T:System.Boolean" /> that is <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public override bool IsValid
        {
            get
            {
                return base.IsValid && this.IsValidWatcher && this.IsValidWatchFilter;
            }
        }

        #endregion Public Methods
    }

    #region Entity Configuration

    /// <summary>
    /// 
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
