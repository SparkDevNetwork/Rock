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
    /// File POCO Service class
    /// </summary>
    public partial class BinaryFileService 
    {
        /// <summary>
        /// Gets binary file without the data field
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public BinaryFile GetWithoutData( int id )
        {
            return this.Queryable()
                .Where( f => f.Id == id )
                .Select( f => new BinaryFile
                {
                    Id = f.Id,
                    Guid = f.Guid,
                    IsTemporary = f.IsTemporary,
                    IsSystem = f.IsSystem,
                    BinaryFileTypeId = f.BinaryFileTypeId,
                    Data = null,
                    Url = f.Url,
                    FileName = f.FileName,
                    MimeType = f.MimeType,
                    LastModifiedTime = f.LastModifiedTime,
                    Description = f.Description
                } )
                .FirstOrDefault();
        }

        /// <summary>
        /// Saves the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="personId">The person id.</param>
        /// <returns></returns>
        public override bool Save( BinaryFile item, int? personId )
        {
            item.LastModifiedTime = DateTime.Now;
            return base.Save( item, personId );
        }
    }
}
