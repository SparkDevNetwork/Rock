﻿// <copyright>
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
using System.ComponentModel;
using System.Linq;

using Quartz;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Jobs
{

    /// <summary>
    /// Job to process the signature documents
    /// </summary>
    [DisplayName( "Process Signature Documents" )]
    [Description( "Sends any digital signature invites that need to be sent for groups that require a signed document." )]

    [IntegerField( "Resend Invite After Number Days", "Number of days after sending last invite to sign, that a new invite should be resent.", false, 5, "", 0 )]
    [IntegerField( "Max Invites", "Maximum number of times an invite should be sent", false, 3, "", 1 )]
    [IntegerField( "Check For Signature Days", "Number of days after document was last sent to check for signature", false, 30, "", 2 )]
    [DisallowConcurrentExecution]
    public class ProcessSignatureDocuments : IJob
    {
        /// <summary> 
        /// Empty constructor for job initialization
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
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            int resendDays = dataMap.GetString( "ResendInviteAfterNumberDays" ).AsIntegerOrNull() ?? 5;
            int maxInvites = dataMap.GetString( "MaxInvites" ).AsIntegerOrNull() ?? 2;
            int checkDays = dataMap.GetString( "CheckForSignatureDays" ).AsIntegerOrNull() ?? 30;
            string folderPath = System.Web.Hosting.HostingEnvironment.MapPath( "~/App_Data/Cache/SignNow" );

            var errorMessages = new List<string>();
            int signatureRequestsSent = 0;
            int documentsUpdated = 0;

            // Send documents
            using ( var rockContext = new RockContext() )
            {
                var maxInviteDate = RockDateTime.Today.AddDays( 0 - resendDays );
                var maxCheckDays = RockDateTime.Today.AddDays( 0 - checkDays );
                var docTypeService = new SignatureDocumentTemplateService( rockContext );
                var docService = new SignatureDocumentService( rockContext );

                // Check for status updates
                foreach ( var document in new SignatureDocumentService( rockContext ).Queryable()
                    .Where( d => 
                        (
                            d.Status == SignatureDocumentStatus.Sent || 
                            ( d.Status == SignatureDocumentStatus.Signed && !d.BinaryFileId.HasValue )
                        ) &&
                        d.LastInviteDate.HasValue && 
                        d.LastInviteDate.Value > maxCheckDays )
                    .ToList() )
                {
                    var updateErrorMessages = new List<string>();
                    var status = document.Status;
                    int? binaryFileId = document.BinaryFileId;
                    if ( docTypeService.UpdateDocumentStatus( document, folderPath, out updateErrorMessages ) )
                    {
                        if ( status != document.Status || !binaryFileId.Equals( document.BinaryFileId )  )
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
                        m.Group.IsActive && !m.Group.IsArchived &&
                        m.Person.Email != null &&
                        m.Person.Email != "" &&
                        m.Group.RequiredSignatureDocumentTemplate != null &&
                        !m.Group.RequiredSignatureDocumentTemplate.Documents.Any( d =>
                            d.AppliesToPersonAlias.PersonId == m.PersonId &&
                            d.Status == SignatureDocumentStatus.Signed 
                        )
                    )
                    .Select( m => new
                    {
                        GroupName = m.Group.Name,
                        Person = m.Person,
                        DocumentType = m.Group.RequiredSignatureDocumentTemplate
                    } )
                    .ToList() )
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

                    var document = docService.Queryable()
                        .Where( d =>
                            d.SignatureDocumentTemplateId == gm.DocumentType.Id &&
                            d.AppliesToPersonAlias.PersonId == gm.Person.Id &&
                            d.AssignedToPersonAlias.PersonId == gm.Person.Id &&
                            d.Status != SignatureDocumentStatus.Signed
                        )
                        .OrderByDescending( d => d.CreatedDateTime )
                        .FirstOrDefault();

                    if ( document == null || ( document.InviteCount < maxInvites && document.LastInviteDate < maxInviteDate ) )
                    {
                        string documentName = string.Format( "{0}_{1}", gm.GroupName.RemoveSpecialCharacters(), gm.Person.FullName.RemoveSpecialCharacters() );

                        var sendErrorMessages = new List<string>();
                        if ( document != null )
                        {
                            docTypeService.SendDocument( document, gm.Person.Email, out sendErrorMessages );
                        }
                        else
                        {
                            docTypeService.SendDocument( gm.DocumentType, gm.Person, gm.Person, documentName, gm.Person.Email, out sendErrorMessages );
                        }

                        if ( !errorMessages.Any() )
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
            }

            if ( errorMessages.Any() )
            {
                throw new Exception( "One or more exceptions occurred processing signature documents..." + Environment.NewLine + errorMessages.AsDelimited( Environment.NewLine ) );
            }

            context.Result = string.Format( "{0} signature requests sent; {1} existing document's status updated", signatureRequestsSent, documentsUpdated );
        }
    }
}