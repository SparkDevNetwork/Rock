// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
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

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data Access class for <see cref="Rock.Model.SignatureDocument"/> entity objects.
    /// </summary>
    public partial class SignatureDocumentService 
    {
        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.SignatureDocument">SignatureDocuments</see> that belong to a specified <see cref="Rock.Model.SignatureDocumentTemplate"/> retrieved by the SignatureDocumentTemplate's SignatureDocumentTemplateId.
        /// </summary>
        /// <param name="definedTypeId">A <see cref="System.Int32"/> representing the SignatureDocumentTemplateId of the <see cref="Rock.Model.SignatureDocumentTemplate"/> to retrieve <see cref="Rock.Model.SignatureDocument">SignatureDocuments</see> for.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.SignatureDocument">SignatureDocuments</see> that belong to the specified <see cref="Rock.Model.SignatureDocumentTemplate"/>. The <see cref="Rock.Model.SignatureDocument">SignatureDocuments</see> will 
        /// be ordered by the <see cref="SignatureDocument">SignatureDocument's</see> Order property.</returns>
        public IOrderedQueryable<SignatureDocument> GetBySignatureDocumentTemplateId( int definedTypeId )
        {
            return Queryable()
                .Where( t => t.SignatureDocumentTemplateId == definedTypeId )
                .OrderBy( t => t.Name );
        }

        /// <summary>
        /// Gets the by document key.
        /// </summary>
        /// <param name="documentKey">The document key.</param>
        /// <returns></returns>
        public SignatureDocument GetByDocumentKey( string documentKey )
        {
            return this.Queryable().Where( d => d.DocumentKey == documentKey ).FirstOrDefault();
        }

    }
}
