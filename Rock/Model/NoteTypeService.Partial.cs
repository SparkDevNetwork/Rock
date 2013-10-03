//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Data access/Service class for entities of the <see cref="Rock.Model.NoteType"/>
    /// </summary>
    public partial class NoteTypeService : Service<NoteType>
    {
        /// <summary>
        /// Gets the first <see cref="Rock.Model.NoteType"/> by Name and EntityType
        /// </summary>
        /// <param name="entityTypeId">A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.EntityType"/> to search for.</param>
        /// <param name="name">A <see cref="System.String"/> representing the Name of the </param>
        /// <returns>The first <see cref="Rock.Model.NoteType"/> matching the provided values. If a match is not found, this value will be null.</returns>
        public NoteType Get( int entityTypeId, string name )
        {
            return Repository.FirstOrDefault( n => n.EntityTypeId == entityTypeId && n.Name == name );
        }

    }
}
