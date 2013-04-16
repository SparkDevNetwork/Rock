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
    /// Note POCO Service class
    /// </summary>
    public partial class NoteService : Service<Note>
    {
        /// <summary>
        /// Gets the notes for the specified note type and entity.
        /// </summary>
        /// <param name="noteTypeId">The note type id.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
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
        /// Gets the all notes for the specified note type.
        /// </summary>
        /// <param name="noteTypeId">The note type id.</param>
        /// <returns></returns>
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
