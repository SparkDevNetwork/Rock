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

namespace Rock.Transactions
{
    /// <summary>
    /// Writes entity audits 
    /// </summary>
    public class UpdateDigitalSignatureDocumentTransaction : ITransaction
    {
        /// <summary>
        /// Gets or sets the signature document type identifier.
        /// </summary>
        /// <value>
        /// The signature document type identifier.
        /// </value>
        public int SignatureDocumentId { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateDigitalSignatureDocumentTransaction"/> class.
        /// </summary>
        /// <param name="signatureDocumentId">The signature document identifier.</param>
        public UpdateDigitalSignatureDocumentTransaction( int signatureDocumentId )
        {
            SignatureDocumentId = signatureDocumentId;
        }

        /// <summary>
        /// Execute method to write transaction to the database.
        /// </summary>
        public void Execute()
        {
            using ( var rockContext = new RockContext() )
            {
                var docTypeService = new SignatureDocumentTemplateService( rockContext );
                var docService = new SignatureDocumentService( rockContext );

                var document = docService.Get( SignatureDocumentId );
                if ( document != null )
                {
                    var status = document.Status;
                    int? binaryFileId = document.BinaryFileId;
                    string folderPath = System.Web.Hosting.HostingEnvironment.MapPath( "~/App_Data/Cache/SignNow" );
                    var updateErrorMessages = new List<string>();

                    if ( docTypeService.UpdateDocumentStatus( document, folderPath, out updateErrorMessages ) )
                    {
                        if ( status != document.Status || !binaryFileId.Equals( document.BinaryFileId ) )
                        {
                            rockContext.SaveChanges();
                        }
                    }
                }
            }
        }
    }
}