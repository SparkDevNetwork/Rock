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
using System.Web;
using System.IO;

using Quartz;

using Rock.Model;
using Rock.Data;

namespace Rock.Jobs
{

    /// <summary>
    /// Job to process the signature documents
    /// </summary>
    [DisallowConcurrentExecution]
    public class ProcessSignatureDocuments : IJob
    {
        /// <summary> 
        /// Empty constructor for job initilization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public ProcessSignatureDocuments()
        {
        }

        /// <summary>
        /// Called by the <see cref="IScheduler" /> when a <see cref="ITrigger" />
        /// fires that is associated with the <see cref="IJob" />.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <remarks>
        /// The implementation may wish to set a  result object on the
        /// JobExecutionContext before this method exits.  The result itself
        /// is meaningless to Quartz, but may be informative to
        /// <see cref="IJobListener" />s or
        /// <see cref="ITriggerListener" />s that are watching the job's
        /// execution.
        /// </remarks>
        public virtual void Execute( IJobExecutionContext context )
        {
            var errorMessages = new List<string>();
            int signatureRequestsSent = 0;
            int documentsUpdated = 0;

            // Send documents
            using ( var rockContext = new RockContext() )
            {
                var docTypeService = new SignatureDocumentTemplateService( rockContext );

                // Check for status updates
                foreach ( var document in new SignatureDocumentService( rockContext ).Queryable()
                    .Where( d => d.Status == SignatureDocumentStatus.Sent ) )
                {
                    var updateErrorMessages = new List<string>();
                    var status = document.Status;
                    if ( docTypeService.UpdateDocumentStatus( document, out updateErrorMessages ) )
                    {
                        if ( status != document.Status )
                        {
                            rockContext.SaveChanges();
                            documentsUpdated++;
                        }
                    }
                    else
                    {
                        errorMessages.AddRange( updateErrorMessages );
                    }
                }

                // Send any needed signature requests
                var docsSent = new Dictionary<int, List<int>>();
                foreach ( var gm in new GroupMemberService( rockContext ).Queryable()
                    .Where( m =>
                        m.GroupMemberStatus == GroupMemberStatus.Active &&
                        m.Group.IsActive &&
                        m.Person.Email != null &&
                        m.Person.Email != "" &&
                        m.Group.RequiredSignatureDocumentTemplate != null &&
                        !m.Group.RequiredSignatureDocumentTemplate.Documents.Any( d =>
                            d.AppliesToPersonAlias.PersonId == m.PersonId )
                    )
                    .Select( m => new
                    {
                        GroupName = m.Group.Name,
                        Person = m.Person,
                        DocumentType = m.Group.RequiredSignatureDocumentTemplate
                    } ) )
                {
                    if ( docsSent.ContainsKey( gm.Person.Id ) )
                    {
                        if ( docsSent[gm.Person.Id].Contains( gm.DocumentType.Id ) )
                        {
                            continue;
                        }
                        else
                        {
                            docsSent[gm.Person.Id].Add( gm.DocumentType.Id );
                        }
                    }
                    else
                    {
                        docsSent.Add( gm.Person.Id, new List<int> { gm.DocumentType.Id } );
                    }

                    string documentName = string.Format( "{0}_{1}", gm.GroupName.RemoveSpecialCharacters(), gm.Person.FullName.RemoveSpecialCharacters() );

                    var sendErrorMessages = new List<string>();
                    if ( docTypeService.SendDocument( gm.DocumentType, gm.Person, gm.Person, documentName, gm.Person.Email, out sendErrorMessages ) )
                    {
                        rockContext.SaveChanges();
                        signatureRequestsSent++;
                    }
                    else
                    {
                        errorMessages.AddRange( sendErrorMessages );
                    }
                }
            }

            if ( errorMessages.Any() )
            {
                throw new Exception( "One or more exceptions occurred processing signature documents..." + Environment.NewLine + errorMessages.AsDelimited( Environment.NewLine ) );
            }

            context.Result = string.Format( "{0} signature requests sent; {1} existing document's status updated", signatureRequestsSent, documentsUpdated );
        }
    }
}