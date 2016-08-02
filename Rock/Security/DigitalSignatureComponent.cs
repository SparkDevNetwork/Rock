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
using System.Collections.Generic;

using Rock.Data;
using Rock.Model;
using Rock.Extension;
using Rock.Web.Cache;

namespace Rock.Security
{
    /// <summary>
    /// The base class for all digital signature methods
    /// </summary>
    public abstract class DigitalSignatureComponent : Component
    {
        /// <summary>
        /// Gets the templates.
        /// </summary>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public abstract Dictionary<string, string> GetTemplates( out List<string> errors );

        /// <summary>
        /// Abstract method for requesting a document be sent to recipient for signature
        /// </summary>
        /// <param name="documentType">Type of the document.</param>
        /// <param name="appliesTo">The applies to.</param>
        /// <param name="assignedTo">The assigned to.</param>
        /// <param name="documentName">Name of the document.</param>
        /// <param name="errors">The errors.</param>
        /// <param name="sendInvite">if set to <c>true</c> [send invite].</param>
        /// <returns></returns>
        public abstract string CreateDocument( SignatureDocumentTemplate documentType, Person appliesTo, Person assignedTo, string documentName, out List<string> errors, bool sendInvite );

        /// <summary>
        /// Gets the invite link.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="recipient">The recipient.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public abstract string GetInviteLink( SignatureDocument document, Person recipient, out List<string> errors );

        /// <summary>
        /// Resends the document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public abstract bool ResendDocument( SignatureDocument document, out List<string> errors );


        /// <summary>
        /// Cancels the document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public abstract bool CancelDocument( SignatureDocument document, out List<string> errors );

        /// <summary>
        /// Gets the document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="folderPath">The folder path.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public abstract string GetDocument( SignatureDocument document, string folderPath, out List<string> errors );

        /// <summary>
        /// Determines whether [is document signed] [the specified document].
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public abstract bool IsDocumentSigned( SignatureDocument document, out List<string> errors );
    }

}
