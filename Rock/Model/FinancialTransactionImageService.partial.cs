// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
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
        /// <param name="personAlias">The person alias.</param>
        /// <returns></returns>
        public override bool Save( FinancialTransactionImage item, PersonAlias personAlias )
        {
            // ensure that the BinaryFile.IsTemporary flag is set to false for any BinaryFiles that are associated with this record
            BinaryFileService binaryFileService = new BinaryFileService( this.RockContext );
            var binaryFile = binaryFileService.Get( item.BinaryFileId );
            if ( binaryFile != null && binaryFile.IsTemporary )
            {
                binaryFile.IsTemporary = false;
            }

            return base.Save( item, personAlias );
        }
    }
}
