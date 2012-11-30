//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;

using Rock.Data;

namespace Rock.Cms
{
    /// <summary>
    /// File POCO Service class
    /// </summary>
    public partial class BinaryFileService : Service<BinaryFile, BinaryFileDto>
    {
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
