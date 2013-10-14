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
            return base.Save( item, personId );
        }

    }
}
