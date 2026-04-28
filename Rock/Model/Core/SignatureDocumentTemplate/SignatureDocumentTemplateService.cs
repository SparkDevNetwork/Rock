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
using System.Data.Entity;
using System.IO;
using System.Linq;

using Rock.Data;
using Rock.Security;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data Access class for <see cref="Rock.Model.SignatureDocumentTemplate"/> entity objects.
    /// </summary>
    public partial class SignatureDocumentTemplateService
    {
        #region Obsolete Legacy Provider methods

        /// <summary>
        /// Sends the legacy provider document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="alternateEmail">The alternate email.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        [Obsolete( "Legacy signature providers are no longer supported in Rock." )]
        [RockObsolete( "19.0" )]
        public bool SendLegacyProviderDocument( SignatureDocument document, string alternateEmail, out List<string> errorMessages )
        {
            return SendLegacyProviderDocument( document, null, null, null, string.Empty, alternateEmail, out errorMessages );
        }

        /// <summary>
        /// Sends the legacy provider document.
        /// </summary>
        /// <param name="signatureDocumentTemplate">The signature document template.</param>
        /// <param name="appliesToPerson">The applies to person.</param>
        /// <param name="assignedToPerson">The assigned to person.</param>
        /// <param name="documentName">Name of the document.</param>
        /// <param name="alternateEmail">The alternate email.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        [Obsolete( "Legacy signature providers are no longer supported in Rock." )]
        [RockObsolete( "19.0" )]
        public bool SendLegacyProviderDocument( SignatureDocumentTemplate signatureDocumentTemplate, Person appliesToPerson, Person assignedToPerson, string documentName, string alternateEmail, out List<string> errorMessages )
        {
            return SendLegacyProviderDocument( null, signatureDocumentTemplate, appliesToPerson, assignedToPerson, documentName, alternateEmail, out errorMessages );
        }

        /// <summary>
        /// Sends the document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="signatureDocumentTemplate">Type of the signature document.</param>
        /// <param name="appliesToPerson">The person.</param>
        /// <param name="assignedToPerson">The assigned to person.</param>
        /// <param name="documentName">Name of the document.</param>
        /// <param name="alternateEmail">The alternate email.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        [Obsolete( "Legacy signature providers are no longer supported in Rock." )]
        [RockObsolete( "19.0" )]
        private bool SendLegacyProviderDocument( SignatureDocument document, SignatureDocumentTemplate signatureDocumentTemplate, Person appliesToPerson, Person assignedToPerson, string documentName, string alternateEmail, out List<string> errorMessages )
        {
            errorMessages = new List<string>();
            errorMessages.Add( "Legacy signature providers are no longer supported in Rock." );
            return false;
        }

        /// <summary>
        /// Cancels the document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        [Obsolete( "Legacy signature providers are no longer supported in Rock." )]
        [RockObsolete( "19.0" )]
        public bool CancelLegacyProviderDocument( SignatureDocument document, out List<string> errorMessages )
        {
            errorMessages = new List<string>();
            errorMessages.Add( "Legacy signature providers are no longer supported in Rock." );
            return false;
        }

        /// <summary>
        /// Updates the document status.
        /// </summary>
        /// <param name="signatureDocument">The signature document.</param>
        /// <param name="tempFolderPath">The temporary folder path.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        [Obsolete( "Legacy signature providers are no longer supported in Rock." )]
        [RockObsolete( "19.0" )]
        public bool UpdateLegacyProviderDocumentStatus( SignatureDocument signatureDocument, string tempFolderPath, out List<string> errorMessages )
        {
            errorMessages = new List<string>();
            errorMessages.Add( "Legacy signature providers are no longer supported in Rock." );
            return !errorMessages.Any();
        }

        #endregion Obsolete Legacy Provider methods

        /// <summary>
        /// Gets the legacy templates.
        /// </summary>
        [Obsolete( "Legacy signature providers are no longer supported in Rock." )]
        [RockObsolete( "19.0" )]
        public IQueryable<SignatureDocumentTemplate> GetLegacyTemplates()
        {
            return Queryable().AsNoTracking().Where( t => t.ProviderEntityTypeId.HasValue ).OrderBy( t => t.Name );
        }
    }
}
