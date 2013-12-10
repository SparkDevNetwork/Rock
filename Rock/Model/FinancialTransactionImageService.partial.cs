using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    public partial class FinancialTransactionImageService
    {
        /// <summary>
        /// Saves the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        public override bool Save( FinancialTransactionImage item, int? personId )
        {
            // ensure that the BinaryFile.IsTemporary flag is set to false for any BinaryFiles that are associated with this record
            BinaryFileService binaryFileService = new BinaryFileService( this.RockContext );
            var binaryFile = binaryFileService.Get( item.BinaryFileId );
            if ( binaryFile != null && binaryFile.IsTemporary )
            {
                binaryFile.IsTemporary = false;
            }

            return base.Save( item, personId );
        }
    }
}
