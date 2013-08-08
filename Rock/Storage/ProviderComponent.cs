//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.Collections.Generic;
using Rock.Extension;
using Rock.Web.Cache;

namespace Rock.Storage
{
    /// <summary>
    /// Base class for BinaryFile storage components
    /// </summary>
    public abstract class ProviderComponent : Component
    {
        public override bool IsActive
        {
            get
            {
                bool isActive;

                if ( bool.TryParse( GetAttributeValue( "Active" ), out isActive ) )
                {
                    return isActive;
                }

                return true;
            }
        }

        /// <summary>
        /// Saves the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="personId">The person id.</param>
        public virtual void SaveFile( Model.BinaryFile file, int? personId )
        {
            SaveFiles( new List<Model.BinaryFile> { file }, personId );
        }

        /// <summary>
        /// Saves the files.
        /// </summary>
        /// <param name="files">The files.</param>
        /// <param name="personId"></param>
        public abstract void SaveFiles( IEnumerable<Model.BinaryFile> files, int? personId );

        /// <summary>
        /// Removes the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="personId"></param>
        public abstract void RemoveFile( Model.BinaryFile file, int? personId );

        /// <summary>
        /// Gets the URL.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="height">The height.</param>
        /// <param name="width">The width.</param>
        /// <returns></returns>
        public abstract string GetUrl( Model.BinaryFile file );

        /// <summary>
        /// Gets the type of the entity.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        public EntityTypeCache EntityType
        {
            get { return EntityTypeCache.Read( this.GetType() ); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderComponent"/> class.
        /// </summary>
        public ProviderComponent()
        {
            this.LoadAttributes();
        }
    }
}
