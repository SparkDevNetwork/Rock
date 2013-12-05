//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.Collections.Generic;
using Rock.Extension;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Storage
{
    /// <summary>
    /// Base class for BinaryFile storage components
    /// </summary>
    public abstract class ProviderComponent : Component
    {
        /// <summary>
        /// Gets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
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
        /// Saves the file to the external storage medium associated with the provider.
        /// Note: This does not save the BinaryFile record to the database
        /// </summary>
        /// <param name="file">The file.</param>
        public abstract void SaveFile( BinaryFile file);

        /// <summary>
        /// Removes the file from the external storage medium associated with the provider.
        /// Note: This does not delete the BinaryFile record from the database
        /// </summary>
        /// <param name="file">The file.</param>
        public abstract void RemoveFile( BinaryFile file);

        /// <summary>
        /// Gets the URL.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns></returns>
        public abstract string GetUrl( BinaryFile file );

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
