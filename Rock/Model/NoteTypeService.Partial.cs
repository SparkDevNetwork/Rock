//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// NoteType POCO Service class
    /// </summary>
    public partial class NoteTypeService : Service<NoteType>
    {
        /// <summary>
        /// Gets the note type with the specified entity type id and name.
        /// </summary>
        /// <param name="entityTypeId">The entity type id.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public NoteType Get( int entityTypeId, string name )
        {
            return Repository.FirstOrDefault( n => n.EntityTypeId == entityTypeId && n.Name == name );
        }

    }
}
