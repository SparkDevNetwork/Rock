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
using System.Linq;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.TestData.Core
{
    /// <summary>
    /// Provides actions to manage Entity Attributes.
    /// </summary>
    public class NoteDataManager
    {
        private static Lazy<NoteDataManager> _dataManager = new Lazy<NoteDataManager>();
        public static NoteDataManager Instance => _dataManager.Value;

        #region Note

        /// <summary>
        /// Creates a new instance with the required fields.
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="noteTypeId"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public Rock.Model.Note NewNote( Guid guid, int noteTypeId, string text = null )
        {
            var note = new Rock.Model.Note();

            note.Guid = guid;
            note.Text = text;
            note.NoteTypeId = noteTypeId;

            return note;
        }

        /// <summary>
        /// Save a new instance to the data store.
        /// </summary>
        /// <param name="newNote"></param>
        /// <returns></returns>
        public void SaveNote( Rock.Model.Note newNote, RockContext rockContext, CreateExistingItemStrategySpecifier existingItemStrategy = CreateExistingItemStrategySpecifier.Replace )
        {
            rockContext.WrapTransaction( () =>
            {
                var entityService = new NoteService( rockContext );
                if ( newNote.Guid != Guid.Empty )
                {
                    var existingEntity = entityService.Get( newNote.Guid );
                    if ( existingEntity != null )
                    {
                        if ( existingItemStrategy == CreateExistingItemStrategySpecifier.Fail )
                        {
                            throw new Exception( "Item exists." );
                        }
                        else if ( existingItemStrategy == CreateExistingItemStrategySpecifier.Replace )
                        {
                            var isDeleted = DeleteNote( existingEntity.Guid.ToString(), rockContext );

                            if ( !isDeleted )
                            {
                                throw new Exception( "Could not replace existing item." );
                            }
                        }
                        else if ( existingItemStrategy == CreateExistingItemStrategySpecifier.Ignore )
                        {
                            return;
                        }
                    }
                }

                // Set the audit details.
                if ( newNote.CreatedDateTime == null )
                {
                    newNote.CreatedDateTime = RockDateTime.Now;
                }
                if ( newNote.EditedDateTime == null )
                {
                    newNote.EditedDateTime = newNote.ModifiedDateTime ?? newNote.CreatedDateTime;
                }

                entityService.Add( newNote );

                rockContext.SaveChanges();
            } );
        }

        public void SetNoteWatchStatusForPerson( int noteId, int watchedByPersonAliasId, bool isWatched, RockContext rockContext )
        {
            var noteWatchService = new NoteWatchService( rockContext );
            var noteWatch = noteWatchService.Queryable()
                    .FirstOrDefault( a => a.NoteId == noteId
                        && a.WatcherPersonAlias.PersonId == watchedByPersonAliasId );

            if ( noteWatch == null )
            {
                noteWatch = new NoteWatch();
                noteWatch.NoteId = noteId;
                ;
                noteWatch.WatcherPersonAliasId = watchedByPersonAliasId;

                noteWatchService.Add( noteWatch );
            }

            noteWatch.IsWatching = isWatched;

            rockContext.SaveChanges();
            var personService = new PersonService( rockContext );
        }

        public bool DeleteNote( string identifier, RockContext rockContext )
        {
            var service = new NoteService( rockContext );
            var note = service.Get( identifier );

            if ( note == null )
            {
                return false;
            }

            var notes = service.GetAllDescendents( note.Id )
                .ToList();
            notes.Add( note );

            var isDeleted = service.DeleteRange( notes );
            return isDeleted;
        }

        #endregion

        #region Note Type

        /// <summary>
        /// Creates a new instance with the required fields.
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="name"></param>
        /// <param name="entityTypeId"></param>
        /// <returns></returns>
        public Rock.Model.NoteType NewNoteType( Guid guid, string name, int entityTypeId )
        {
            var noteType = new Rock.Model.NoteType();

            noteType.Guid = guid;
            noteType.Name = name;
            noteType.EntityTypeId = entityTypeId;

            return noteType;
        }

        /// <summary>
        /// Creates a new instance with the required fields.
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="name"></param>
        /// <param name="attachToEntityType"></param>
        /// <returns></returns>
        public Rock.Model.NoteType NewNoteType( Guid guid, string name, Type attachToEntityType )
        {
            var noteType = new Rock.Model.NoteType();

            noteType.Guid = guid;
            noteType.Name = name;

            noteType.ForEntityType( attachToEntityType );

            return noteType;
        }

        /// <summary>
        /// Save a new instance to the data store.
        /// </summary>
        /// <param name="newNoteType"></param>
        /// <param name="rockContext"></param>
        /// <param name="existingItemStrategy"></param>
        /// <exception cref="Exception"></exception>
        public void SaveNoteType( Rock.Model.NoteType newNoteType, RockContext rockContext, CreateExistingItemStrategySpecifier existingItemStrategy = CreateExistingItemStrategySpecifier.Replace )
        {
            rockContext.WrapTransaction( () =>
            {
                var entityService = new NoteTypeService( rockContext );
                if ( newNoteType.Guid != Guid.Empty )
                {
                    var existingEntity = entityService.Get( newNoteType.Guid );
                    if ( existingEntity != null )
                    {
                        if ( existingItemStrategy == CreateExistingItemStrategySpecifier.Fail )
                        {
                            throw new Exception( "Item exists." );
                        }
                        else if ( existingItemStrategy == CreateExistingItemStrategySpecifier.Replace )
                        {
                            var isDeleted = DeleteNoteType( existingEntity.Guid.ToString(), rockContext );
                            if ( !isDeleted )
                            {
                                throw new Exception( "Could not replace existing item." );
                            }
                        }
                        else if (existingItemStrategy == CreateExistingItemStrategySpecifier.Ignore )
                        {
                            return;
                        }
                    }
                }

                entityService.Add( newNoteType );

                rockContext.SaveChanges();
            } );
        }

        public bool DeleteNoteType( string identifier, RockContext rockContext )
        {
            var service = new NoteTypeService( rockContext );
            var entity = service.Get( identifier );

            if ( entity == null )
            {
                return false;
            }

            var isDeleted = service.Delete( entity );
            return isDeleted;
        }

        #endregion

        #region Note Watch

        /// <summary>
        /// Creates a new instance with the required fields.
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="watchedPersonAliasId"></param>
        /// <param name="watchedByPersonAliasId"></param>
        /// <param name="noteTypeId"></param>
        /// <returns></returns>
        public Rock.Model.NoteWatch NewNoteWatchForPerson( Guid guid, int watchedPersonAliasId, int watchedByPersonAliasId, int? noteTypeId )
        {
            var noteWatch = new Rock.Model.NoteWatch();

            noteWatch.Guid = guid;
            noteWatch.WatcherPersonAliasId = watchedByPersonAliasId;

            noteWatch.ForWatchedPerson( watchedPersonAliasId );
            noteWatch.ForWatchedNoteType( noteTypeId );

            return noteWatch;
        }

        /// <summary>
        /// Creates a new instance with the required fields.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public Rock.Model.NoteWatch NewNoteWatch( Guid guid )
        {
            var noteWatch = new Rock.Model.NoteWatch();

            noteWatch.Guid = guid;

            return noteWatch;
        }

        /// <summary>
        /// Save a new instance to the data store.
        /// </summary>
        /// <param name="newNoteWatch"></param>
        /// <param name="rockContext"></param>
        /// <param name="existingItemStrategy"></param>
        public void SaveNoteWatch( Rock.Model.NoteWatch newNoteWatch, RockContext rockContext, CreateExistingItemStrategySpecifier existingItemStrategy = CreateExistingItemStrategySpecifier.Replace )
        {
            rockContext.WrapTransaction( () =>
            {
                var entityService = new NoteWatchService( rockContext );
                if ( newNoteWatch.Guid != Guid.Empty )
                {
                    var existingEntity = entityService.Get( newNoteWatch.Guid );
                    if ( existingEntity != null )
                    {
                        if ( existingItemStrategy == CreateExistingItemStrategySpecifier.Fail )
                        {
                            throw new Exception( "Item exists." );
                        }
                        else if ( existingItemStrategy == CreateExistingItemStrategySpecifier.Replace )
                        {
                            var isDeleted = DeleteNoteWatch( existingEntity.Guid.ToString(), rockContext );
                            if ( !isDeleted )
                            {
                                throw new Exception( "Could not replace existing item." );
                            }
                        }
                        else if ( existingItemStrategy == CreateExistingItemStrategySpecifier.Ignore )
                        {
                            return;
                        }
                    }
                }

                entityService.Add( newNoteWatch );

                rockContext.SaveChanges();
            } );
        }

        public bool DeleteNoteWatch( string identifier, RockContext rockContext )
        {
            var service = new NoteWatchService( rockContext );
            var entity = service.Get( identifier );

            if ( entity == null )
            {
                return false;
            }

            var isDeleted = service.Delete( entity );
            return isDeleted;
        }

        #endregion
    }

    #region Support Classes

    public static class NotesDataManagerExtensions
    {
        public static Rock.Model.Note ForEntity( this Rock.Model.Note note, int? entityId )
        {
            note.EntityId = entityId;
            return note;
        }

        public static Rock.Model.Note AsReplyTo( this Rock.Model.Note note, int? parentNoteId )
        {
            note.ParentNoteId = parentNoteId;
            return note;
        }

        public static Rock.Model.Note SetEditActionAuditInfo( this Rock.Model.Note note, DateTime? dateTimeEdited, int? editorPersonAliasId )
        {
            note.EditedDateTime = dateTimeEdited;
            if ( dateTimeEdited != null )
            {
                // If the created and modified dates are not set, apply the same values.
                note.CreatedDateTime = note.CreatedDateTime ?? dateTimeEdited;
                note.ModifiedDateTime = note.ModifiedDateTime ?? dateTimeEdited;
            }
            note.EditedByPersonAliasId = editorPersonAliasId;
            if ( editorPersonAliasId != null )
            {
                note.CreatedByPersonAliasId = note.CreatedByPersonAliasId ?? editorPersonAliasId;
                note.ModifiedByPersonAliasId = note.ModifiedByPersonAliasId ?? editorPersonAliasId;
            }

            return note;
        }

        public static T SetModified<T>( this T entity, DateTime? dateTimeEdited, int? editorPersonAliasId )
            where T : IModel
        {
            entity.ModifiedDateTime = dateTimeEdited;
            if ( dateTimeEdited != null )
            {
                // If the created and modified dates are not set, apply the same values.
                entity.CreatedDateTime = entity.CreatedDateTime ?? dateTimeEdited;
            }
            entity.ModifiedByPersonAliasId = editorPersonAliasId;
            if ( editorPersonAliasId != null )
            {
                entity.CreatedByPersonAliasId = entity.CreatedByPersonAliasId ?? editorPersonAliasId;
            }

            return entity;
        }

        public static Rock.Model.Note SetTestTextAndCaption( this Rock.Model.Note note )
        {
            note.Caption = $"Note {note.Guid}";
            note.Text = $"Here is the unique note text ({note.Guid}).";
            return note;
        }

        public static Rock.Model.NoteType ForEntityType( this Rock.Model.NoteType note, Type attachToEntityType )
        {
            var entityTypeId = EntityTypeCache.GetId( attachToEntityType );
            note.EntityTypeId = entityTypeId.GetValueOrDefault();

            return note;
        }

        public static Rock.Model.NoteWatch ForWatchingPerson( this Rock.Model.NoteWatch noteWatch, int? personAliasId )
        {
            noteWatch.WatcherPersonAliasId = personAliasId;
            return noteWatch;
        }

        public static Rock.Model.NoteWatch ForWatchedNoteType( this Rock.Model.NoteWatch noteWatch, int? noteTypeId )
        {
            noteWatch.NoteTypeId = noteTypeId;
            return noteWatch;
        }

        public static Rock.Model.NoteWatch ForWatchedNote( this Rock.Model.NoteWatch noteWatch, int? noteId )
        {
            noteWatch.NoteId = noteId;

            // Reset incompatible parameters.
            noteWatch.NoteTypeId = null;
            noteWatch.EntityId = null;
            noteWatch.EntityTypeId = null;

            return noteWatch;
        }

        public static Rock.Model.NoteWatch ForWatchedEntity( this Rock.Model.NoteWatch noteWatch, int? watchedEntityTypeId, int? watchedEntityId )
        {
            noteWatch.EntityTypeId = watchedEntityTypeId;
            noteWatch.EntityId = watchedEntityId;
            return noteWatch;
        }

        public static Rock.Model.NoteWatch ForWatchedEntityType( this Rock.Model.NoteWatch noteWatch, int? watchedEntityTypeId )
        {
            noteWatch.EntityTypeId = watchedEntityTypeId;
            noteWatch.EntityId = null;
            return noteWatch;
        }

        public static Rock.Model.NoteWatch ForWatchedPerson( this Rock.Model.NoteWatch noteWatch, int? personAliasId )
        {
            noteWatch.EntityId = personAliasId;

            var personEntityTypeId = EntityTypeCache.GetId( SystemGuid.EntityType.PERSON );
            noteWatch.ForWatchedEntity( personEntityTypeId, personAliasId );

            return noteWatch;
        }
    }

    #endregion
}