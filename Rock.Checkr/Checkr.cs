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
using System.ComponentModel.Composition;
using System.Linq;
using Newtonsoft.Json;
using Rock.Cache;
using Rock.Checkr.CheckrApi;
using Rock.Checkr.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;

namespace Rock.Checkr
{
    /// <summary>
    /// Checkr Background Check 
    /// </summary>
    [Description( "Checkr Background Check" )]
    [Export( typeof( BackgroundCheckComponent ) )]
    [ExportMetadata( "ComponentName", "Checkr" )]

    public class Checkr : BackgroundCheckComponent
    {
        #region BackgroundCheckComponent Implementation

        /// <summary>
        /// Sends a background request to Checkr.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="workflow">The Workflow initiating the request.</param>
        /// <param name="personAttribute">The person attribute.</param>
        /// <param name="ssnAttribute">The SSN attribute.</param>
        /// <param name="requestTypeAttribute">The request type attribute.</param>
        /// <param name="billingCodeAttribute">The billing code attribute.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns>
        /// True/False value of whether the request was successfully sent or not.
        /// </returns>
        public override bool SendRequest( RockContext rockContext, Model.Workflow workflow,
                    CacheAttribute personAttribute, CacheAttribute ssnAttribute, CacheAttribute requestTypeAttribute,
                    CacheAttribute billingCodeAttribute, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            try
            {
                // Check to make sure workflow is not null
                if ( workflow == null )
                {
                    errorMessages.Add( "The 'Checkr' background check provider requires a valid workflow." );
                    return false;
                }

                Person person;
                int? personAliasId;
                if ( !GetPerson( rockContext, workflow, personAttribute, out person, out personAliasId, errorMessages ) )
                {
                    UpdateWorkflowRequestStatus( workflow, rockContext, "FAIL" );
                    return false;
                }

                string packageName;
                if ( !GetPackageName( rockContext, workflow, requestTypeAttribute, out packageName, errorMessages ) )
                {
                    UpdateWorkflowRequestStatus( workflow, rockContext, "FAIL" );
                    return false;
                }

                string candidateId;
                if ( !CreateCandidate( person, out candidateId, errorMessages ) )
                {
                    UpdateWorkflowRequestStatus( workflow, rockContext, "FAIL" );
                    return false;
                }

                if ( !CreateInvitation( candidateId, packageName, errorMessages ) )
                {
                    UpdateWorkflowRequestStatus( workflow, rockContext, "FAIL" );
                    return false;
                }

                using ( var newRockContext = new RockContext() )
                {
                    var backgroundCheckService = new BackgroundCheckService( newRockContext );
                    var backgroundCheck = backgroundCheckService.Queryable()
                            .Where( c =>
                                c.WorkflowId.HasValue &&
                                c.WorkflowId.Value == workflow.Id )
                            .FirstOrDefault();

                    if ( backgroundCheck == null )
                    {
                        backgroundCheck = new Rock.Model.BackgroundCheck();
                        backgroundCheck.WorkflowId = workflow.Id;
                        backgroundCheckService.Add( backgroundCheck );
                    }

                    backgroundCheck.PersonAliasId = personAliasId.Value;
                    backgroundCheck.ForeignId = 2;
                    backgroundCheck.PackageName = packageName;
                    backgroundCheck.RequestDate = RockDateTime.Now;
                    backgroundCheck.RequestId = candidateId;
                    newRockContext.SaveChanges();

                    UpdateWorkflowRequestStatus( workflow, newRockContext, "SUCCESS" );
                    CacheAttribute.RemoveEntityAttributes();
                    return true;
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, null );
                errorMessages.Add( ex.Message );
                return false;
            }
        }

        /// <summary>
        /// Gets the URL to the background check report.
        /// Note: Also used by GetBackgroundCheck.ashx.cs, ProcessRequest( HttpContext context )
        /// </summary>
        /// <param name="reportKey">The report key.</param>
        /// <returns></returns>
        public override string GetReportUrl( string reportKey )
        {
            var isAuthorized = this.IsAuthorized( Authorization.VIEW, this. GetCurrentPerson() );

            if ( isAuthorized )
            {
                GetDocumentResponse getDocumentResponse;
                List<string> errorMessages = new List<string>();

                if ( CheckrApiUtility.GetDocument( reportKey, out getDocumentResponse, errorMessages ) )
                {
                    return getDocumentResponse.DownloadUri;
                }
            }
            else
            {
                return "Unauthorized";
            }

            return null;
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Gets the person that is currently logged in.
        /// </summary>
        /// <returns></returns>
        private Person GetCurrentPerson()
        {
            using ( var rockContext = new RockContext() )
            {
                var currentUser = new UserLoginService( rockContext ).GetByUserName( UserLogin.GetCurrentUserName() );
                return currentUser != null ? currentUser.Person : null;
            }
        }

        /// <summary>
        /// Saves the attribute value.
        /// </summary>
        /// <param name="workflow">The workflow.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="fieldType">Type of the field.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="qualifiers">The qualifiers.</param>
        /// <returns>True/False value of whether the request was successfully sent or not.</returns>
        private static bool SaveAttributeValue( Rock.Model.Workflow workflow, string key, string value,
            CacheFieldType fieldType, RockContext rockContext, Dictionary<string, string> qualifiers = null )
        {
            bool createdNewAttribute = false;

            if ( workflow.Attributes.ContainsKey( key ) )
            {
                workflow.SetAttributeValue( key, value );
            }
            else
            {
                // Read the attribute
                var attributeService = new AttributeService( rockContext );
                var attribute = attributeService
                    .Get( workflow.TypeId, "WorkflowTypeId", workflow.WorkflowTypeId.ToString() )
                    .Where( a => a.Key == key )
                    .FirstOrDefault();

                // If workflow attribute doesn't exist, create it 
                // ( should only happen first time a background check is processed for given workflow type)
                if ( attribute == null )
                {
                    attribute = new Rock.Model.Attribute();
                    attribute.EntityTypeId = workflow.TypeId;
                    attribute.EntityTypeQualifierColumn = "WorkflowTypeId";
                    attribute.EntityTypeQualifierValue = workflow.WorkflowTypeId.ToString();
                    attribute.Name = key.SplitCase();
                    attribute.Key = key;
                    attribute.FieldTypeId = fieldType.Id;
                    attributeService.Add( attribute );

                    if ( qualifiers != null )
                    {
                        foreach ( var keyVal in qualifiers )
                        {
                            var qualifier = new Rock.Model.AttributeQualifier();
                            qualifier.Key = keyVal.Key;
                            qualifier.Value = keyVal.Value;
                            attribute.AttributeQualifiers.Add( qualifier );
                        }
                    }

                    createdNewAttribute = true;
                }

                // Set the value for this attribute
                var attributeValue = new Rock.Model.AttributeValue();
                attributeValue.Attribute = attribute;
                attributeValue.EntityId = workflow.Id;
                attributeValue.Value = value;
                new AttributeValueService( rockContext ).Add( attributeValue );
            }

            return createdNewAttribute;
        }

        /// <summary>
        /// Updates the workflow.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="recommendation">The recommendation.</param>
        /// <param name="reportLink">The report link.</param>
        /// <param name="reportStatus">The report status.</param>
        /// <param name="rockContext">The rock context.</param>
        private static void UpdateWorkflow( int id, string recommendation, string documentId, string reportStatus, RockContext rockContext )
        {
            bool createdNewAttribute = false;
            var workflowService = new WorkflowService( rockContext );
            var workflow = new WorkflowService( rockContext ).Get( id );
            if ( workflow != null && workflow.IsActive )
            {
                workflow.LoadAttributes();
                if ( workflow.Attributes.ContainsKey( "ReportStatus" ) )
                {
                    if ( workflow.GetAttributeValue( "ReportStatus" ).IsNotNullOrWhitespace() && reportStatus.IsNullOrWhiteSpace() )
                    {
                        // Don't override current values if Webhook is older than current values
                        return;
                    }
                }

                if ( workflow.Attributes.ContainsKey( "Report" ) )
                {
                    if ( workflow.GetAttributeValue( "Report" ).IsNotNullOrWhitespace() && documentId.IsNullOrWhiteSpace() )
                    {
                        // Don't override current values if Webhook is older than current values
                        return;
                    }
                }

                // Save the recommendation 
                if ( !string.IsNullOrWhiteSpace( recommendation ) )
                {
                    if ( SaveAttributeValue( workflow, "ReportRecommendation", recommendation,
                        CacheFieldType.Get( Rock.SystemGuid.FieldType.TEXT.AsGuid() ), rockContext,
                        new Dictionary<string, string> { { "ispassword", "false" } } ) )
                    {
                        createdNewAttribute = true;
                    }

                }
                // Save the report link 
                if ( documentId.IsNotNullOrWhitespace() )
                {
                    int entityTypeId = CacheEntityType.Get( typeof(Checkr) ).Id;
                    if ( SaveAttributeValue( workflow, "Report", $"{entityTypeId},{documentId}",
                        CacheFieldType.Get( Rock.SystemGuid.FieldType.TEXT.AsGuid() ), rockContext,
                        new Dictionary<string, string> { { "ispassword", "false" } } ) )
                    {
                        createdNewAttribute = true;
                    }
                }

                if ( !string.IsNullOrWhiteSpace( reportStatus ) )
                {
                    // Save the status
                    if ( SaveAttributeValue( workflow, "ReportStatus", reportStatus,
                    CacheFieldType.Get( Rock.SystemGuid.FieldType.SINGLE_SELECT.AsGuid() ), rockContext,
                    new Dictionary<string, string> { { "fieldtype", "ddl" }, { "values", "Pass,Fail,Review" } } ) )
                    {
                        createdNewAttribute = true;
                    }
                }

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    workflow.SaveAttributeValues( rockContext );
                    foreach ( var activity in workflow.Activities )
                    {
                        activity.SaveAttributeValues( rockContext );
                    }
                } );
            }

            rockContext.SaveChanges();

            if ( createdNewAttribute )
            {
                CacheAttribute.RemoveEntityAttributes();
            }
        }

        /// <summary>
        /// Updates the background check and workflow values.
        /// </summary>
        /// <param name="candidateId">The candidate identifier.</param>
        /// <param name="webhookTypes">The webhook types.</param>
        /// <param name="packageName">Name of the package.</param>
        /// <param name="status">The status.</param>
        /// <param name="documentId">The document identifier.</param>
        /// <returns>True/False value of whether the request was successfully sent or not.</returns>
        private static bool UpdateBackgroundCheckAndWorkFlow( string candidateId, Enums.WebhookTypes webhookTypes, string packageName = null, string status = null, string documentId = null )
        {
            using ( var rockContext = new RockContext() )
            {
                var backgroundCheck = new BackgroundCheckService( rockContext )
                    .Queryable( "PersonAlias.Person" )
                    .Where( g => g.RequestId == candidateId )
                    .Where( g => g.ForeignId == 2 )
                    .FirstOrDefault();

                if ( backgroundCheck == null )
                {
                    string errorMessage = "Background Check not found: Candidate ID: " + candidateId;
                    ExceptionLogService.LogException( new Exception( errorMessage ), null );
                    return false;
                }

                backgroundCheck.Status = webhookTypes.ToString();
                if ( packageName != null )
                {
                    backgroundCheck.PackageName = packageName;
                }

                if ( documentId == string.Empty )
                {
                    backgroundCheck.RecordFound = false;
                }
                else if ( documentId != null )
                {
                    backgroundCheck.ResponseId = documentId;
                    backgroundCheck.RecordFound = true;
                }

                //rockContext.SqlLogging( true );

                rockContext.SaveChanges();

                //rockContext.SqlLogging( false );

                if ( backgroundCheck.WorkflowId.HasValue && backgroundCheck.WorkflowId > 0 )
                {
                    string recommendation = null;
                    string reportStatus = null; //Pass,Fail,Review
                    switch ( status )
                    {
                        case "pending":
                            recommendation = "Report Pending";
                            break;
                        case "clear":
                            recommendation = "Candidate Pass";
                            reportStatus = "Pass";
                            break;
                        case "consider":
                            recommendation = "Candidate Review";
                            reportStatus = "Review";
                            break;
                        case "suspended":
                            recommendation = "Report Suspended";
                            break;
                        case "dispute":
                            recommendation = "Report Disputed";
                            break;
                        case "InvitationCreated":
                            recommendation = "Invitate Sent";
                            break;
                        case "InvitationCompleted":
                            recommendation = "Invitate Complete";
                            break;
                        case "InvitationExpired":
                            recommendation = "Invitate Expired";
                            break;
                    }

                    UpdateWorkflow( backgroundCheck.WorkflowId.Value, recommendation, documentId, reportStatus, rockContext );
                }
            }

            return true;
        }

        /// <summary>
        /// Sets the workflow requeststatus attribute.
        /// </summary>
        /// <param name="workflow">The workflow.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="requestStatus">The request status.</param>
        private void UpdateWorkflowRequestStatus( Model.Workflow workflow, RockContext rockContext, string requestStatus )
        {
            if ( SaveAttributeValue( workflow, "RequestStatus", requestStatus,
                CacheFieldType.Get( Rock.SystemGuid.FieldType.TEXT.AsGuid() ), rockContext, null ) )
            {
                rockContext.SaveChanges();
                CacheAttribute.RemoveEntityAttributes();
            }
        }

        /// <summary>
        /// Get the background check type that the request is for.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="workflow">The Workflow initiating the request.</param>
        /// <param name="requestTypeAttribute">The request type attribute.</param>
        /// <param name="packageName"></param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns>True/False value of whether the request was successfully sent or not.</returns>
        private bool GetPackageName( RockContext rockContext, Model.Workflow workflow, CacheAttribute requestTypeAttribute, out string packageName, List<string> errorMessages )
        {
            packageName = null;
            if ( requestTypeAttribute == null )
            {
                errorMessages.Add( "The 'Checkr' background check provider requires a background check type." );
                return false;
            }

            CacheDefinedValue pkgTypeDefinedValue = CacheDefinedValue.Get( workflow.GetAttributeValue( requestTypeAttribute.Key ).AsGuid() );
            if ( pkgTypeDefinedValue == null )
            {
                errorMessages.Add( "The 'Checkr' background check provider couldn't load background check type." );
                return false;
            }

            if ( pkgTypeDefinedValue.Attributes == null )
            {
                pkgTypeDefinedValue.LoadAttributes( rockContext );
            }

            packageName = pkgTypeDefinedValue.GetAttributeValue( "PMMPackageName" );
            return true;
        }

        /// <summary>
        /// Get the person that the request is for.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="workflow">The Workflow initiating the request.</param>
        /// <param name="personAttribute">The person attribute.</param>
        /// <param name="person">Return the person.</param>
        /// <param name="personAliasId">Return the person alias ID.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns>True/False value of whether the request was successfully sent or not.</returns>
        private bool GetPerson( RockContext rockContext, Model.Workflow workflow, CacheAttribute personAttribute, out Person person, out int? personAliasId, List<string> errorMessages )
        {
            person = null;
            personAliasId = null;
            if ( personAttribute != null )
            {
                Guid? personAliasGuid = workflow.GetAttributeValue( personAttribute.Key ).AsGuidOrNull();
                if ( personAliasGuid.HasValue )
                {
                    person = new PersonAliasService( rockContext ).Queryable()
                        .Where( p => p.Guid.Equals( personAliasGuid.Value ) )
                        .Select( p => p.Person )
                        .FirstOrDefault();
                    person.LoadAttributes( rockContext );
                }
            }

            if ( person == null )
            {
                errorMessages.Add( "The 'Checkr' background check provider requires the workflow to have a 'Person' attribute that contains the person who the background check is for." );
                return false;
            }

            personAliasId = person.PrimaryAliasId;
            if ( !personAliasId.HasValue )
            {
                errorMessages.Add( "The 'Checkr' background check provider requires the workflow to have a 'Person' attribute that contains the person who the background check is for." );
                return false;
            }

            return true;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Get the Checkr packages and update the list on the server.
        /// </summary>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns>True/False value of whether the request was successfully sent or not.</returns>
        public static bool UpdatePackages( List<string> errorMessages )
        {
            GetPackagesResponse getPackagesResponse;
            if ( !CheckrApiUtility.GetPackages( out getPackagesResponse, errorMessages ) )
            {
                return false;
            }

            List<string> packages;
            using ( var rockContext = new RockContext() )
            {
                var definedType = CacheDefinedType.Get( Rock.SystemGuid.DefinedType.BACKGROUND_CHECK_TYPES.AsGuid() );

                DefinedValueService definedValueService = new DefinedValueService( rockContext );
                packages = definedValueService
                    .GetByDefinedTypeGuid( Rock.SystemGuid.DefinedType.BACKGROUND_CHECK_TYPES.AsGuid() )
                    .Where( v => v.ForeignId == 2 )
                    .ToList()
                    .Select( v => { v.LoadAttributes( rockContext ); return v.GetAttributeValue( "PMMPackageName" ).ToString(); } ) // v => v.Value.Substring( CheckrConstants.TYPENAME_PREFIX.Length ) )
                    .ToList();

                foreach ( var packageRestResponse in getPackagesResponse.Data )
                {
                    string packageName = packageRestResponse.Slug;
                    if ( !packages.Contains( packageName ) )
                    {
                        DefinedValue definedValue = null;

                        definedValue = new DefinedValue();
                        definedValue.DefinedTypeId = definedType.Id;
                        definedValue.ForeignId = 2;
                        definedValueService.Add( definedValue );

                        definedValue.Value = CheckrConstants.CHECKR_TYPENAME_PREFIX + packageName.Replace( '_', ' ' ).FixCase();

                        definedValue.Description = packageRestResponse.Name == "Educatio Report" ? "Education Report" : packageRestResponse.Name;
                        rockContext.SaveChanges();

                        definedValue.LoadAttributes( rockContext );

                        definedValue.SetAttributeValue( "PMMPackageName", packageName );
                        definedValue.SetAttributeValue( "DefaultCounty", string.Empty );
                        definedValue.SetAttributeValue( "SendHomeCounty", "False" );
                        definedValue.SetAttributeValue( "DefaultState", string.Empty );
                        definedValue.SetAttributeValue( "SendHomeState", "False" );
                        definedValue.SetAttributeValue( "MVRJurisdiction", string.Empty );
                        definedValue.SetAttributeValue( "SendHomeStateMVR", "False" );
                        definedValue.SaveAttributeValues( rockContext );

                        CacheDefinedValue.Remove( definedValue.Id );
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Creates the candidate.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="candidateId">The candidate identifier.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns>True/False value of whether the request was successfully sent or not.</returns>
        public static bool CreateCandidate( Person person, out string candidateId, List<string> errorMessages )
        {
            CreateCandidateResponse createCandidateResponse;
            candidateId = null;
            if ( CheckrApiUtility.CreateCandidate( person, out createCandidateResponse, errorMessages ) )
            {
                candidateId = createCandidateResponse.Id;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Creates the invitation.
        /// </summary>
        /// <param name="candidateId">The candidate identifier.</param>
        /// <param name="package">The package.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <param name="request">The request.</param>
        /// <param name="response">The response.</param>
        /// <returns>True/False value of whether the request was successfully sent or not.</returns>
        public static bool CreateInvitation( string candidateId, string package, List<string> errorMessages )
        {
            CreateInvitationResponse createInvitationResponse;
            if ( CheckrApiUtility.CreateInvitation( candidateId, package, out createInvitationResponse, errorMessages ) )
            {
                candidateId = createInvitationResponse.Id;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Saves the webhook results.
        /// </summary>
        /// <param name="postedData">The posted data.</param>
        /// <returns>True/False value of whether the request was successfully sent or not.</returns>
        public static bool SaveWebhookResults( string postedData )
        {
            GenericWebhook genericWebhook = JsonConvert.DeserializeObject<GenericWebhook>( postedData, new JsonSerializerSettings()
            {
                Error = ( sender, errorEventArgs ) =>
                {
                    errorEventArgs.ErrorContext.Handled = true;
                    ExceptionLogService.LogException( new Exception( errorEventArgs.ErrorContext.Error.Message ), null );
                }
            } );

            if ( genericWebhook == null )
            {
                string errorMessage = "Webhook data is not valid: " + postedData;
                ExceptionLogService.LogException( new Exception( errorMessage ), null );
                return false;
            }

            if ( genericWebhook.Type == Enums.WebhookTypes.InvitationCompleted ||
                genericWebhook.Type == Enums.WebhookTypes.InvitationCreated ||
                genericWebhook.Type == Enums.WebhookTypes.InvitationExpired )
            {
                InvitationWebhook invitationWebhook = JsonConvert.DeserializeObject<InvitationWebhook>( postedData );
                if ( invitationWebhook == null )
                {
                    string errorMessage = "Invitation Webhook data is not valid: " + postedData;
                    ExceptionLogService.LogException( new Exception( errorMessage ), null );
                    return false;
                }

                return UpdateBackgroundCheckAndWorkFlow( invitationWebhook.Data.Object.CandidateId, genericWebhook.Type, invitationWebhook.Data.Object.Package, genericWebhook.Type.ConvertToString(false) );
            } else if ( genericWebhook.Type == Enums.WebhookTypes.ReportCreated ||
                genericWebhook.Type == Enums.WebhookTypes.ReportCompleted ||
                genericWebhook.Type == Enums.WebhookTypes.ReportDisputed ||
                genericWebhook.Type == Enums.WebhookTypes.ReportEngaged ||
                genericWebhook.Type == Enums.WebhookTypes.ReportPostAdverseAction ||
                genericWebhook.Type == Enums.WebhookTypes.ReportPreAdverseAction ||
                genericWebhook.Type == Enums.WebhookTypes.ReportResumed ||
                genericWebhook.Type == Enums.WebhookTypes.ReportSuspended ||
                genericWebhook.Type == Enums.WebhookTypes.ReportUpgraded )
            {
                ReportWebhook reportWebhook = JsonConvert.DeserializeObject<ReportWebhook>( postedData );
                if ( reportWebhook == null )
                {
                    string errorMessage = "Report Webhook data is not valid: " + postedData;
                    ExceptionLogService.LogException( new Exception( errorMessage ), null );
                    return false;
                }

                string documentId = null;
                if ( genericWebhook.Type == Enums.WebhookTypes.ReportCompleted )
                {
                    documentId = GetDocumentIdFromReport( reportWebhook.Data.Object.Id ) ?? string.Empty;

                }

                return UpdateBackgroundCheckAndWorkFlow( reportWebhook.Data.Object.CandidateId, genericWebhook.Type, reportWebhook.Data.Object.Package, reportWebhook.Data.Object.Status, documentId );
            }

            return true;
        }

        /// <summary>
        /// Gets the document identifier from report.
        /// </summary>
        /// <param name="reportId">The report identifier.</param>
        /// <returns>The document identifier.</returns>
        public static string GetDocumentIdFromReport( string reportId )
        {
            List<string> errorMessages = new List<string>();
            GetReportResponse getReportResponse;
            if ( !CheckrApiUtility.GetReport( reportId, out getReportResponse, errorMessages ) )
            {
                return null;
            }

            if ( getReportResponse.DocumentIds == null || getReportResponse.DocumentIds.Count == 0 )
            {
                errorMessages.Add( "No document found" );
                return null;
            }

            string documentId = getReportResponse.DocumentIds[0];
            if ( documentId.IsNullOrWhiteSpace() )
            {
                errorMessages.Add( "Empty document ID returned" );
                return null;
            }

            return documentId;
        }
       
        #endregion
    }
}