//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Data Access Service class for <see cref="Rock.Model.BinaryFile"/> objects.
    /// </summary>
    public partial class BinaryFileService
    {
        /// <summary>
        /// Saves the specified <see cref="Rock.Model.BinaryFile"/>.
        /// </summary>
        /// <param name="item">A <see cref="Rock.Model.BinaryFile"/> to save.</param>
        /// <param name="personId">A <see cref="System.Int32"/> representing the PersonId of the <see cref="Rock.Model.Person"/> who is saving the BinaryFile..</param>
        /// <returns></returns>
        public override bool Save( BinaryFile item, int? personId )
        {
            item.LastModifiedDateTime = DateTime.Now;
            Rock.Storage.ProviderComponent storageProvider = DetermineStorageProvider( item );

            if ( storageProvider != null )
            {
                //// if this file is getting replaced, and we can determine the StorageProvider, use the provider to remove the file from the provider's 
                //// external storage medium before we save it again. This especially important in cases where the provider for this filetype has changed 
                //// since it was last saved
                storageProvider.RemoveFile( item );

                // save the file to the provider's storage medium
                storageProvider.SaveFile( item );
            }
            
            return base.Save( item, personId );
        }

        /// <summary>
        /// Deletes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        public override bool Delete( BinaryFile item, int? personId )
        {
            // if we can determine the StorageProvider, use the provider to remove the file from the provider's external storage medium
            Rock.Storage.ProviderComponent storageProvider = DetermineStorageProvider( item );

            if ( storageProvider != null )
            {
                storageProvider.RemoveFile( item );
            }

            // delete the record from the database
            return base.Delete( item, personId );
        }

        /// <summary>
        /// Determines the storage provider.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        private Storage.ProviderComponent DetermineStorageProvider( BinaryFile item )
        {
            Rock.Storage.ProviderComponent storageProvider = null;
            item.StorageEntityType = item.StorageEntityType ?? new EntityTypeService().Get( item.StorageEntityTypeId ?? 0 );
            if ( item.StorageEntityType != null )
            {
                storageProvider = Rock.Storage.ProviderContainer.GetComponent( item.BinaryFileType.StorageEntityType.Name );
            }

            return storageProvider;
        }
    }
}
