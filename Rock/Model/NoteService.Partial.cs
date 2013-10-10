//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Linq;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Data access/service class for <see cref="Rock.Model.Note"/> entity objects.
    /// </summary>
    public partial class NoteService : Service<Note>
    {
        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Model.Note">Notes</see> for the specified <see cref="Rock.Model.NoteType"/> and entity.
        /// </summary>
        /// <param name="noteTypeId">A <see cref="System.Int32" /> representing the Id of the <see cref="Rock.Model.NoteType"/>.</param>
        /// <param name="entityId">TA <see cref="Sytem.Int32"/> representing the Id of the entity that the note belongs to.</param>
        /// <returns>A queryable collection of <see cref="Rock.Model.Note">Notes</see> for the specified <see cref="System.Mode.NoteType"/> and entity. </returns>
        public IQueryable<Note> Get( int noteTypeId, int entityId )
        {
            return Repository.AsQueryable()
                .Where( n =>
                    n.NoteTypeId == noteTypeId &&
                    n.EntityId == entityId )
                .OrderByDescending( n => n.IsAlert )
                .ThenByDescending( n => n.CreationDateTime );
        }

        /// <summary>
        /// Returns all of the <see cref="Rock.Model.Note">Notes</see> for the specified note type.
        /// </summary>
        /// <param name="noteTypeId">A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.NoteType"/>.</param>
        /// <returns>A queryable collection of <see cref="Rock.Model.Note">Notes</see> by <see cref="Rock.Model.NoteType"/>.</returns>
        public IQueryable<Note> GetByNoteTypeId( int noteTypeId )
        {
            return Repository.AsQueryable()
                .Where( n =>
                    n.NoteTypeId == noteTypeId )
                .OrderByDescending( n => n.IsAlert )
                .ThenByDescending( n => n.CreationDateTime );
        }
    }
}
