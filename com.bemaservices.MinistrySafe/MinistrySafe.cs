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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using com.bemaservices.MinistrySafe.Constants;
using com.bemaservices.MinistrySafe.MinistrySafeApi;
using com.bemaservices.MinistrySafe.Model;
using Humanizer;
using Newtonsoft.Json;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

namespace com.bemaservices.MinistrySafe
{
    /// <summary>
    /// Checkr Background Check
    /// </summary>
    [Description( "MinistrySafe Background Check" )]
    [Export( typeof( BackgroundCheckComponent ) )]
    [ExportMetadata( "ComponentName", "MinistrySafe" )]

    [EncryptedTextField( "Access Token", "MinistrySafe Access Token", true, "", "", 0, null, true )]
    [BooleanField( "Is Staging", "Is Staging Environment", false, "", 0, "IsStaging" )]
    public class MinistrySafe : BackgroundCheckComponent
    {
        #region Private Fields

        /// <summary>
        /// The objects to use when locking our use of the workflow's attribute values and the webhook's use of them.
        /// We're using a concurrent dictionary to hold small lock objects that are based on the workflow id so
        /// we don't needlessly lock two different workflow's from being worked on at the same time.
        /// Based on https://kofoedanders.com/c-sharp-dynamic-locking/
        /// </summary>
        private static ConcurrentDictionary<int, object> _lockObjects = new ConcurrentDictionary<int, object>();

        #endregion

        #region BackgroundCheck Implementation

        /// <summary>
        /// Sends a background request to Checkr.  This method is called by the BackgroundCheckRequest action's Execute
        /// method for the Checkr component.
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
        public override bool SendRequest( RockContext rockContext, Rock.Model.Workflow workflow,
                    AttributeCache personAttribute, AttributeCache ssnAttribute, AttributeCache requestTypeAttribute,
                    AttributeCache billingCodeAttribute, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            try
            {
                // Check to make sure workflow is not null
                if ( workflow == null )
                {
                    errorMessages.Add( "The 'Checkr' background check provider requires a valid workflow." );
                    UpdateWorkflowRequestStatus( workflow, rockContext, "FAIL" );
                    return true;
                }

                // Lock the workflow until we're finished saving so the webhook can't start working on it.
                var lockObject = _lockObjects.GetOrAdd( workflow.Id, new object() );
                lock ( lockObject )
                {
                    Person person;
                    int? personAliasId;
                    if ( !GetPerson( rockContext, workflow, personAttribute, out person, out personAliasId, errorMessages ) )
                    {
                        errorMessages.Add( "Unable to get Person." );
                        UpdateWorkflowRequestStatus( workflow, rockContext, "FAIL" );
                        return true;
                    }

                    string level = null;
                    string packageCode = null;
                    string userType = null;
                    string packageName = null;
                    if ( !GetPackageName( rockContext, workflow, requestTypeAttribute, out level, out packageCode, out userType, out packageName, errorMessages ) )
                    {
                        errorMessages.Add( "Unable to get Package." );
                        UpdateWorkflowRequestStatus( workflow, rockContext, "FAIL" );
                        return true;
                    }


                    string userId;
                    string directLoginUrl;
                    if ( !GetOrCreateUser( workflow, person, personAliasId.Value, userType, out userId, out directLoginUrl, errorMessages ) )
                    {
                        errorMessages.Add( "Unable to create user." );
                        UpdateWorkflowRequestStatus( workflow, rockContext, "FAIL" );
                        return true;
                    }

                    string requestId;
                    if ( !CreateBackgroundCheck( userId, level, packageCode, out requestId, errorMessages ) )
                    {
                        errorMessages.Add( "Unable to create background check." );
                        UpdateWorkflowRequestStatus( workflow, rockContext, "FAIL" );
                        return true;
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
                            backgroundCheck = new BackgroundCheck();
                            backgroundCheck.WorkflowId = workflow.Id;
                            backgroundCheckService.Add( backgroundCheck );
                        }

                        backgroundCheck.PersonAliasId = personAliasId.Value;
                        backgroundCheck.ForeignId = 4;
                        backgroundCheck.PackageName = packageName;
                        backgroundCheck.RequestDate = RockDateTime.Now;
                        backgroundCheck.RequestId = requestId;
                        newRockContext.SaveChanges();
                    }

                    UpdateWorkflowRequestStatus( workflow, rockContext, "SUCCESS" );

                    if ( workflow.IsPersisted )
                    {
                        // Make sure the AttributeValues are saved to the database immediately because the MinistrySafe WebHook
                        // (which might otherwise get called before they are saved by the workflow processing) needs to
                        // have the correct attribute values.
                        workflow.SaveAttributeValues( rockContext );
                    }

                    _lockObjects.TryRemove( workflow.Id, out _ ); // we no longer need that lock for this workflow
                }

                return true;

            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, null );
                errorMessages.Add( ex.Message );
                UpdateWorkflowRequestStatus( workflow, rockContext, "FAIL" );
                return true;
            }
        }

        /// <summary>
        /// Gets the URL to the background check report.
        /// Note: Also used by GetBackgroundCheck.ashx.cs, ProcessRequest( HttpContext context )
        /// </summary>
        /// <param name="reportKey">The report key.</param>
        /// <returns></returns>
        public override string GetReportUrl( string backgroundCheckId )
        {
            var isAuthorized = this.IsAuthorized( Authorization.VIEW, this.GetCurrentPerson() );

            if ( isAuthorized )
            {
                BackgroundCheckResponse getDocumentResponse;
                List<string> errorMessages = new List<string>();

                if ( MinistrySafeApiUtility.GetBackgroundCheck( backgroundCheckId, out getDocumentResponse, errorMessages ) )
                {
                    return getDocumentResponse.ResultsUrl;
                }
                else
                {
                    LogErrors( errorMessages );
                }
                return backgroundCheckId;
            }
            else
            {
                return "Unauthorized";
            }

            return null;
        }

        /// <summary>
        /// Updates the workflow, closing it if the reportStatus is blank and the recommendation is "Invitation Expired".
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="recommendation">The recommendation.</param>
        /// <param name="reportLink">The report link.</param>
        /// <param name="reportStatus">The report status.</param>
        /// <param name="rockContext">The rock context.</param>
        private static void UpdateBackgroundCheckWorkflow( int id, string recommendation, string documentId, string reportStatus, RockContext rockContext )
        {
            // Make sure the workflow isn't locked (i.e., it's still being worked on by the 'SendRequest' method of the workflow
            // BackgroundCheckComponent) before we start working on it -- especially before we load the workflow's attributes.
            var lockObject = _lockObjects.GetOrAdd( id, new object() );
            lock ( lockObject )
            {
                var workflowService = new WorkflowService( rockContext );
                var workflow = workflowService.Get( id );
                if ( workflow != null && workflow.IsActive )
                {
                    workflow.LoadAttributes();
                    if ( workflow.Attributes.ContainsKey( "ReportStatus" ) )
                    {
                        if ( workflow.GetAttributeValue( "ReportStatus" ).IsNotNullOrWhiteSpace() && reportStatus.IsNullOrWhiteSpace() )
                        {
                            // Don't override current values if Webhook is older than current values
                            return;
                        }
                    }

                    if ( workflow.Attributes.ContainsKey( "Report" ) )
                    {
                        if ( workflow.GetAttributeValue( "Report" ).IsNotNullOrWhiteSpace() && documentId.IsNullOrWhiteSpace() )
                        {
                            // Don't override current values if Webhook is older than current values
                            return;
                        }
                    }

                    // Save the recommendation
                    if ( !string.IsNullOrWhiteSpace( recommendation ) )
                    {
                        if ( SaveAttributeValue( workflow, "ReportRecommendation", recommendation,
                            FieldTypeCache.Get( Rock.SystemGuid.FieldType.TEXT.AsGuid() ), rockContext,
                            new Dictionary<string, string> { { "ispassword", "false" } } ) )
                        {
                        }

                        if ( reportStatus.IsNullOrWhiteSpace() && recommendation == "Invitation Expired" )
                        {
                            workflow.CompletedDateTime = RockDateTime.Now;
                            workflow.MarkComplete( recommendation );
                        }
                    }
                    // Save the report link
                    if ( documentId.IsNotNullOrWhiteSpace() )
                    {
                        int entityTypeId = EntityTypeCache.Get( typeof( MinistrySafe ) ).Id;
                        if ( SaveAttributeValue( workflow, "Report", $"{entityTypeId},{documentId}",
                            FieldTypeCache.Get( Rock.SystemGuid.FieldType.TEXT.AsGuid() ), rockContext,
                            new Dictionary<string, string> { { "ispassword", "false" } } ) )
                        {
                        }
                    }

                    if ( !string.IsNullOrWhiteSpace( reportStatus ) )
                    {
                        // Save the status
                        if ( SaveAttributeValue( workflow, "ReportStatus", reportStatus,
                        FieldTypeCache.Get( Rock.SystemGuid.FieldType.SINGLE_SELECT.AsGuid() ), rockContext,
                        new Dictionary<string, string> { { "fieldtype", "ddl" }, { "values", "Pass,Fail,Review" } } ) )
                        {
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

                List<string> workflowErrors;
                workflowService.Process( workflow, out workflowErrors );
                _lockObjects.TryRemove( id, out _ ); // we no longer need that lock for this workflow
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
        private static bool UpdateBackgroundCheckAndWorkFlow( BackgroundCheckWebhook backgroundCheckWebhook )
        {
            using ( var rockContext = new RockContext() )
            {
                string requestId = backgroundCheckWebhook.Id;
                int? externalId = backgroundCheckWebhook.ExternalId;
                var backgroundCheck = new BackgroundCheckService( rockContext )
                    .Queryable( "PersonAlias.Person" )
                    .Where( g => ( requestId != null && g.RequestId == requestId ) || ( requestId == null && g.PersonAliasId == externalId ) )
                    .Where( g => g.ForeignId == 4 )
                    .OrderByDescending( g => g.RequestDate )
                    .FirstOrDefault();

                if ( backgroundCheck == null )
                {

                    string errorMessage = requestId != null ? "Background Check not found: Request ID: " + requestId : "Background Check not found: PersonAliasId: " + externalId;
                    ExceptionLogService.LogException( new Exception( errorMessage ), null );
                    return false;
                }

                backgroundCheck.Status = "consider";

                backgroundCheck.ResponseId = backgroundCheckWebhook.Id;
                backgroundCheck.ResponseDate = RockDateTime.Now;
                if ( backgroundCheckWebhook.ResultsUrl.IsNotNullOrWhiteSpace() )
                {
                    backgroundCheck.ResponseData = backgroundCheckWebhook.ResultsUrl;
                }

                //rockContext.SqlLogging( true );

                rockContext.SaveChanges();

                //rockContext.SqlLogging( false );

                if ( backgroundCheck.WorkflowId.HasValue && backgroundCheck.WorkflowId > 0 )
                {
                    string recommendation = null;
                    string reportStatus = null; //Pass,Fail,Review
                    switch ( backgroundCheck.Status )
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
                            recommendation = "Invitation Sent";
                            break;
                        case "InvitationCompleted":
                            recommendation = "Invitation Completed";
                            break;
                        case "InvitationExpired":
                            recommendation = "Invitation Expired";
                            break;
                    }

                    UpdateBackgroundCheckWorkflow( backgroundCheck.WorkflowId.Value, recommendation, backgroundCheck.ResponseId, reportStatus, rockContext );
                }
            }

            return true;
        }

        /// <summary>
        /// Sets the workflow RequestStatus attribute.
        /// </summary>
        /// <param name="workflow">The workflow.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="requestStatus">The request status.</param>
        private void UpdateWorkflowRequestStatus( Rock.Model.Workflow workflow, RockContext rockContext, string requestStatus )
        {
            if ( SaveAttributeValue( workflow, "RequestStatus", requestStatus,
                FieldTypeCache.Get( Rock.SystemGuid.FieldType.TEXT.AsGuid() ), rockContext, null ) )
            {
                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Get the Checkr packages and update the list on the server.
        /// </summary>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns>True/False value of whether the request was successfully sent or not.</returns>
        public static bool UpdatePackages( List<string> errorMessages )
        {
            List<PackageResponse> getPackagesResponse;
            if ( !MinistrySafeApiUtility.GetPackages( out getPackagesResponse, errorMessages ) )
            {
                return false;
            }

            if ( getPackagesResponse == null )
            {
                getPackagesResponse = new List<PackageResponse>();
            }

            for ( var level = 1; level <= 7; level++ )
            {
                var packageResponse = new PackageResponse();
                packageResponse.Name = String.Format( "Search Level {0}", level.ToWords().ToUpper() );
                packageResponse.Level = level;
                getPackagesResponse.Add( packageResponse );
            }

            if ( true )
            {
                var packageResponse = new PackageResponse();
                packageResponse.Name = "Leadership";
                packageResponse.Code = "TEST_PACKAGE_12";
                getPackagesResponse.Add( packageResponse );
            }

            Dictionary<string, DefinedValue> packages;
            using ( var rockContext = new RockContext() )
            {
                var definedType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.BACKGROUND_CHECK_TYPES.AsGuid() );

                DefinedValueService definedValueService = new DefinedValueService( rockContext );
                packages = definedValueService
                    .GetByDefinedTypeGuid( Rock.SystemGuid.DefinedType.BACKGROUND_CHECK_TYPES.AsGuid() )
                    .Where( v => v.ForeignId == 4 )
                    .ToList()
                    .Select( v => { v.LoadAttributes( rockContext ); return v; } ) // v => v.Value.Substring( CheckrConstants.TYPENAME_PREFIX.Length ) )
                    .ToDictionary( v => v.GetAttributeValue( "MinistrySafePackageName" ).ToString(), v => v );

                var userTypes = definedValueService
                     .GetByDefinedTypeGuid( "559E79C6-2EAB-4A0D-A16F-59D9B63F002F".AsGuid() )
                     .ToList();
                foreach ( var packageResponse in getPackagesResponse )
                {

                    string packageName = packageResponse.Name;
                    if ( !packages.ContainsKey( packageName ) )
                    {
                        foreach ( var userType in userTypes )
                        {
                            AddPackage( rockContext, definedType, definedValueService, packageResponse, userType );
                        }
                    }
                }

                var packageRestResponseNames = getPackagesResponse.Select( pr => pr.Name );
                foreach ( var package in packages )
                {
                    package.Value.IsActive = packageRestResponseNames.Contains( package.Key );
                }

                rockContext.SaveChanges();
            }

            DefinedValueCache.Clear();
            return true;
        }

        private static void AddPackage( RockContext rockContext, DefinedTypeCache definedType, DefinedValueService definedValueService, PackageResponse packageResponse, DefinedValue userType )
        {
            DefinedValue definedValue = null;

            definedValue = new DefinedValue()
            {
                IsActive = true,
                DefinedTypeId = definedType.Id,
                ForeignId = 4,
                Value = string.Format( "{0}{1} {2}", MinistrySafeConstants.MINISTRYSAFE_TYPENAME_PREFIX, userType.Description, packageResponse.Name.Replace( '_', ' ' ) )
            };

            definedValueService.Add( definedValue );

            rockContext.SaveChanges();

            definedValue.LoadAttributes( rockContext );

            definedValue.SetAttributeValue( "MinistrySafePackageName", packageResponse.Name );
            definedValue.SetAttributeValue( "MinistrySafePackageLevel", packageResponse.Level );
            definedValue.SetAttributeValue( "MinistrySafePackageCode", packageResponse.Code );
            definedValue.SetAttributeValue( "MinistrySafePackagePrice", packageResponse.Price );
            definedValue.SetAttributeValue( "MinistrySafeUserType", userType.Guid.ToString() );
            definedValue.SaveAttributeValues( rockContext );
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
        private bool GetPackageName( RockContext rockContext, Rock.Model.Workflow workflow, AttributeCache requestTypeAttribute, out string level, out string packageCode, out string userType, out string packageName, List<string> errorMessages )
        {
            level = null;
            packageCode = null;
            userType = null;
            packageName = null;
            if ( requestTypeAttribute == null )
            {
                errorMessages.Add( "The 'MinistrySafe' background check provider requires a background check type." );
                return false;
            }

            DefinedValueCache pkgTypeDefinedValue = DefinedValueCache.Get( workflow.GetAttributeValue( requestTypeAttribute.Key ).AsGuid() );
            if ( pkgTypeDefinedValue == null )
            {
                errorMessages.Add( "The 'MinistrySafe' background check provider couldn't load background check type." );
                return false;
            }

            if ( pkgTypeDefinedValue.Attributes == null )
            {
                // shouldn't happen since pkgTypeDefinedValue is a ModelCache<,> type 
                return false;
            }

            DefinedValueCache userTypeDefinedValue = DefinedValueCache.Get( pkgTypeDefinedValue.GetAttributeValue( "MinistrySafeUserType" ).AsGuid() );
            if ( userTypeDefinedValue == null )
            {
                errorMessages.Add( "The 'MinistrySafe' background check type does not have an associated user type." );
                return false;
            }

            level = pkgTypeDefinedValue.GetAttributeValue( "MinistrySafePackageLevel" );
            packageCode = pkgTypeDefinedValue.GetAttributeValue( "MinistrySafePackageCode" );
            userType = userTypeDefinedValue.Value;
            packageName = pkgTypeDefinedValue.Value;
            return true;
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
        public static bool CreateBackgroundCheck( string userId, string level, string packageCode, out string requestId, List<string> errorMessages )
        {
            requestId = null;
            BackgroundCheckResponse backgroundCheckResponse;
            if ( MinistrySafeApiUtility.CreateBackgroundCheck( userId, level, packageCode, out backgroundCheckResponse, errorMessages ) )
            {
                userId = backgroundCheckResponse.UserId.ToString();
                requestId = backgroundCheckResponse.Id;
                return true;
            }

            return false;
        }

        #endregion

        #region Training Implementation

        public bool SendTraining( RockContext rockContext, Rock.Model.Workflow workflow,
                 AttributeCache personAttribute, AttributeCache userTypeAttribute, AttributeCache surveyTypeAttribute, AttributeCache directLoginUrlAttribute,
                 out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            try
            {
                // Check to make sure workflow is not null
                if ( workflow == null )
                {
                    errorMessages.Add( "The 'MinistrySafe' provider requires a valid workflow." );
                    UpdateWorkflowTrainingStatus( workflow, rockContext, "FAIL" );
                    return true;
                }

                // Lock the workflow until we're finished saving so the webhook can't start working on it.
                var lockObject = _lockObjects.GetOrAdd( workflow.Id, new object() );
                lock ( lockObject )
                {
                    Person person;
                    int? personAliasId;
                    if ( !GetPerson( rockContext, workflow, personAttribute, out person, out personAliasId, errorMessages ) )
                    {
                        errorMessages.Add( "Unable to get Person." );
                        UpdateWorkflowTrainingStatus( workflow, rockContext, "FAIL" );
                        return true;
                    }

                    string surveyTypeName;
                    if ( !GetSurveyTypeName( rockContext, workflow, surveyTypeAttribute, out surveyTypeName, errorMessages ) )
                    {
                        errorMessages.Add( "Unable to get Survey Type." );
                        UpdateWorkflowTrainingStatus( workflow, rockContext, "FAIL" );
                        return true;
                    }

                    string userTypeName;
                    if ( !GetUserTypeName( rockContext, workflow, userTypeAttribute, out userTypeName, errorMessages ) )
                    {
                        errorMessages.Add( "Unable to get User Type." );
                        UpdateWorkflowTrainingStatus( workflow, rockContext, "FAIL" );
                        return true;
                    }

                    string userId;
                    string directLoginUrl;
                    if ( !GetOrCreateUser( workflow, person, personAliasId.Value, userTypeName, out userId, out directLoginUrl, errorMessages ) )
                    {
                        errorMessages.Add( "Unable to create user." );
                        UpdateWorkflowTrainingStatus( workflow, rockContext, "FAIL" );
                        return true;
                    }

                    if ( !AssignTraining( userId, surveyTypeName, errorMessages ) )
                    {
                        errorMessages.Add( "Unable to assign training." );
                        UpdateWorkflowTrainingStatus( workflow, rockContext, "FAIL" );
                        return true;
                    }

                    using ( var newRockContext = new RockContext() )
                    {
                        var ministrySafeUserService = new MinistrySafeUserService( newRockContext );
                        var ministrySafeUser = ministrySafeUserService.Queryable()
                                .Where( c =>
                                    c.WorkflowId.HasValue &&
                                    c.WorkflowId.Value == workflow.Id )
                                .FirstOrDefault();

                        if ( ministrySafeUser == null )
                        {
                            ministrySafeUser = new MinistrySafeUser();
                            ministrySafeUser.WorkflowId = workflow.Id;
                            ministrySafeUserService.Add( ministrySafeUser );
                        }

                        ministrySafeUser.PersonAliasId = personAliasId.Value;
                        ministrySafeUser.ForeignId = 4;
                        ministrySafeUser.SurveyCode = surveyTypeName;
                        ministrySafeUser.UserType = userTypeName;
                        ministrySafeUser.RequestDate = RockDateTime.Now;
                        ministrySafeUser.DirectLoginUrl = directLoginUrl;
                        ministrySafeUser.UserId = userId.AsInteger();
                        newRockContext.SaveChanges();
                    }

                    UpdateWorkflowTrainingStatus( workflow, rockContext, "SUCCESS" );

                    if ( SaveAttributeValue( workflow, directLoginUrlAttribute.Key, directLoginUrl,
                        FieldTypeCache.Get( Rock.SystemGuid.FieldType.TEXT.AsGuid() ), rockContext, null ) )
                    {
                        rockContext.SaveChanges();
                    }

                    if ( workflow.IsPersisted )
                    {
                        // Make sure the AttributeValues are saved to the database immediately because the MinistrySafe WebHook
                        // (which might otherwise get called before they are saved by the workflow processing) needs to
                        // have the correct attribute values.
                        workflow.SaveAttributeValues( rockContext );
                    }

                    _lockObjects.TryRemove( workflow.Id, out _ ); // we no longer need that lock for this workflow
                }

                return true;

            }
            catch ( Exception ex )
            {
                Rock.Model.ExceptionLogService.LogException( ex, null );
                errorMessages.Add( ex.Message );
                UpdateWorkflowTrainingStatus( workflow, rockContext, "FAIL" );
                return true;
            }
        }

        /// <summary>
        /// Updates the workflow, closing it if the reportStatus is blank and the recommendation is "Invitation Expired".
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="recommendation">The recommendation.</param>
        /// <param name="reportLink">The report link.</param>
        /// <param name="reportStatus">The report status.</param>
        /// <param name="rockContext">The rock context.</param>
        private static void UpdateTrainingWorkflow( int id, int? score, DateTime completedDateTime, RockContext rockContext )
        {
            // Make sure the workflow isn't locked (i.e., it's still being worked on by the 'SendRequest' method of the workflow
            // BackgroundCheckComponent) before we start working on it -- especially before we load the workflow's attributes.
            var lockObject = _lockObjects.GetOrAdd( id, new object() );
            lock ( lockObject )
            {
                var workflowService = new WorkflowService( rockContext );
                var workflow = workflowService.Get( id );
                if ( workflow != null && workflow.IsActive )
                {
                    workflow.LoadAttributes();
                    if ( workflow.Attributes.ContainsKey( "TrainingScore" ) )
                    {
                        if ( workflow.GetAttributeValue( "TrainingScore" ).IsNotNullOrWhiteSpace() && score == null )
                        {
                            // Don't override current values if Webhook is older than current values
                            return;
                        }
                    }

                    if ( workflow.Attributes.ContainsKey( "TrainingDate" ) )
                    {
                        if ( workflow.GetAttributeValue( "TrainingDate" ).IsNotNullOrWhiteSpace() && completedDateTime == null )
                        {
                            // Don't override current values if Webhook is older than current values
                            return;
                        }
                    }

                    // Save the score
                    if ( score != null )
                    {
                        if ( SaveAttributeValue( workflow, "TrainingScore", score.ToString(),
                            FieldTypeCache.Get( Rock.SystemGuid.FieldType.INTEGER.AsGuid() ), rockContext ) )
                        {
                        }
                    }
                    if ( completedDateTime != null )
                    {
                        // Save the training date
                        if ( SaveAttributeValue( workflow, "TrainingDate", completedDateTime.ToString(),
                        FieldTypeCache.Get( Rock.SystemGuid.FieldType.SINGLE_SELECT.AsGuid() ), rockContext ) )
                        {
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

                List<string> workflowErrors;
                workflowService.Process( workflow, out workflowErrors );
                _lockObjects.TryRemove( id, out _ ); // we no longer need that lock for this workflow
            }
        }

        /// <summary>
        /// Updates the user and workflow values.
        /// </summary>
        /// <param name="candidateId">The candidate identifier.</param>
        /// <param name="webhookTypes">The webhook types.</param>
        /// <param name="packageName">Name of the package.</param>
        /// <param name="status">The status.</param>
        /// <param name="documentId">The document identifier.</param>
        /// <returns>True/False value of whether the request was successfully sent or not.</returns>
        private static bool UpdateUserAndWorkFlow( TrainingWebhook trainingWebhook )
        {
            var externalId = trainingWebhook.ExternalId.AsInteger();
            using ( var rockContext = new RockContext() )
            {
                var ministrySafeUserService = new MinistrySafeUserService( rockContext );

                var ministrySafeUsers = ministrySafeUserService
                    .Queryable( "PersonAlias.Person" )
                    .Where( m => m.CompletedDateTime == null && ( m.WorkflowId == externalId && m.ForeignId == 3 ) || ( m.PersonAliasId == externalId && ( m.ForeignId == 2 || m.ForeignId == 4 ) ) )
                    .ToList();

                if ( ministrySafeUsers == null || ministrySafeUsers.Count <= 0 )
                {
                    string errorMessage = "MinistrySafe User not found: External ID: " + trainingWebhook.ExternalId;
                    Rock.Model.ExceptionLogService.LogException( new Exception( errorMessage ), null );
                    return false;
                }

                foreach ( var ministrySafeUser in ministrySafeUsers )
                {
                    ministrySafeUser.Score = trainingWebhook.Score;
                    ministrySafeUser.CompletedDateTime = trainingWebhook.CompleteDateTime;
                    ministrySafeUser.ResponseDate = RockDateTime.Now;
                    ministrySafeUser.SurveyCode = trainingWebhook.SurveyCode ?? ministrySafeUser.SurveyCode;

                    rockContext.SaveChanges();

                    if ( ministrySafeUser.WorkflowId.HasValue && ministrySafeUser.WorkflowId > 0 )
                    {
                        UpdateTrainingWorkflow( ministrySafeUser.WorkflowId.Value, trainingWebhook.Score, trainingWebhook.CompleteDateTime, rockContext );
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Sets the workflow RequestStatus attribute.
        /// </summary>
        /// <param name="workflow">The workflow.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="requestStatus">The request status.</param>
        private void UpdateWorkflowTrainingStatus( Rock.Model.Workflow workflow, RockContext rockContext, string requestStatus )
        {
            if ( SaveAttributeValue( workflow, "RequestStatus", requestStatus,
                FieldTypeCache.Get( Rock.SystemGuid.FieldType.TEXT.AsGuid() ), rockContext, null ) )
            {
                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Get the survey type that the request is for.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="workflow">The Workflow initiating the request.</param>
        /// <param name="requestTypeAttribute">The request type attribute.</param>
        /// <param name="packageName"></param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns>True/False value of whether the request was successfully sent or not.</returns>
        private bool GetSurveyTypeName( RockContext rockContext, Rock.Model.Workflow workflow, AttributeCache surveyTypeAttribute, out string packageName, List<string> errorMessages )
        {
            packageName = null;
            if ( surveyTypeAttribute == null )
            {
                errorMessages.Add( "The 'MinistrySafe' provider requires a survey type." );
                return false;
            }

            var definedValueGuid = workflow.GetAttributeValue( surveyTypeAttribute.Key ).AsGuid();
            DefinedValueCache surveyTypeDefinedValue = DefinedValueCache.Get( definedValueGuid );
            if ( surveyTypeDefinedValue == null )
            {
                errorMessages.Add( "The 'MinistrySafe' provider couldn't load survey type." );
                return false;
            }

            if ( surveyTypeDefinedValue.Attributes == null )
            {
                // shouldn't happen since pkgTypeDefinedValue is a ModelCache<,> type 
                return false;
            }

            packageName = surveyTypeDefinedValue.Value;
            return true;
        }

        private bool GetUserTypeName( RockContext rockContext, Rock.Model.Workflow workflow, AttributeCache userTypeAttribute, out string userTypeName, List<string> errorMessages )
        {
            userTypeName = null;
            if ( userTypeAttribute == null )
            {
                errorMessages.Add( "The 'MinistrySafe' provider requires a user type." );
                return false;
            }

            DefinedValueCache userTypeDefinedValue = DefinedValueCache.Get( workflow.GetAttributeValue( userTypeAttribute.Key ).AsGuid() );
            if ( userTypeDefinedValue == null )
            {
                errorMessages.Add( "The 'MinistrySafe' provider couldn't load user type." );
                return false;
            }

            if ( userTypeDefinedValue.Attributes == null )
            {
                // shouldn't happen since pkgTypeDefinedValue is a ModelCache<,> type 
                return false;
            }

            userTypeName = userTypeDefinedValue.Value;
            return true;
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
        public static bool AssignTraining( string candidateId, string surveyCode, List<string> errorMessages )
        {
            TrainingResponse assignTrainingResponse;
            if ( MinistrySafeApiUtility.AssignTraining( candidateId, surveyCode, out assignTrainingResponse, errorMessages ) )
            {
                candidateId = assignTrainingResponse.Id;
                return true;
            }

            return false;
        }

        #endregion

        #region Shared Methods

        /// <summary>
        /// Logs the errors.
        /// </summary>
        /// <param name="errorMessages">The error messages.</param>
        private static void LogErrors( List<string> errorMessages )
        {
            if ( errorMessages.Any() )
            {
                foreach ( string errorMsg in errorMessages )
                {
                    ExceptionLogService.LogException( new Exception( "Checkr Error: " + errorMsg ), null );
                }
            }
        }

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
            FieldTypeCache fieldType, RockContext rockContext, Dictionary<string, string> qualifiers = null )
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
                            var qualifier = new AttributeQualifier();
                            qualifier.Key = keyVal.Key;
                            qualifier.Value = keyVal.Value;
                            attribute.AttributeQualifiers.Add( qualifier );
                        }
                    }

                    createdNewAttribute = true;
                }

                // Set the value for this attribute
                var attributeValue = new AttributeValue();
                attributeValue.Attribute = attribute;
                attributeValue.EntityId = workflow.Id;
                attributeValue.Value = value;
                new AttributeValueService( rockContext ).Add( attributeValue );
            }

            return createdNewAttribute;
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
        private bool GetPerson( RockContext rockContext, Rock.Model.Workflow workflow, AttributeCache personAttribute, out Person person, out int? personAliasId, List<string> errorMessages )
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
                errorMessages.Add( "The 'MinistrySafe' provider requires the workflow to have a 'Person' attribute that contains the person who the training is for." );
                return false;
            }

            personAliasId = person.PrimaryAliasId;
            if ( !personAliasId.HasValue )
            {
                errorMessages.Add( "The 'MinistrySafe' provider requires the workflow to have a 'Person' attribute that contains the person who the training is for." );
                return false;
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
        /// <summary>
        /// Creates the candidate.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="candidateId">The candidate identifier.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns>True/False value of whether the request was successfully sent or not.</returns>
        public static bool GetOrCreateUser( Rock.Model.Workflow workflow, Person person, int personAliasId, string userTypeName, out string candidateId, out string directLoginUrl, List<string> errorMessages )
        {
            UserResponse userResponse;
            candidateId = null;
            directLoginUrl = null;
            if ( MinistrySafeApiUtility.GetUser( workflow, person, personAliasId, userTypeName, out userResponse, errorMessages ) )
            {
                candidateId = userResponse.Id;
                directLoginUrl = userResponse.DirectLoginUrl;
                return true;
            }
            else
            {
                if ( MinistrySafeApiUtility.CreateUser( workflow, person, personAliasId, userTypeName, out userResponse, errorMessages ) )
                {
                    candidateId = userResponse.Id;
                    directLoginUrl = userResponse.DirectLoginUrl;
                    return true;
                }
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
            TrainingWebhook trainingWebhook = JsonConvert.DeserializeObject<TrainingWebhook>( postedData, new JsonSerializerSettings()
            {
                Error = ( sender, errorEventArgs ) =>
                {
                    errorEventArgs.ErrorContext.Handled = true;
                    Rock.Model.ExceptionLogService.LogException( new Exception( errorEventArgs.ErrorContext.Error.Message ), null );
                }
            } );

            if ( trainingWebhook.CertificateUrl == null )
            {
                BackgroundCheckWebhook backgroundCheckWebhook = JsonConvert.DeserializeObject<BackgroundCheckWebhook>( postedData, new JsonSerializerSettings()
                {
                    Error = ( sender, errorEventArgs ) =>
                    {
                        errorEventArgs.ErrorContext.Handled = true;
                        Rock.Model.ExceptionLogService.LogException( new Exception( errorEventArgs.ErrorContext.Error.Message ), null );
                    }
                } );

                if ( backgroundCheckWebhook == null )
                {
                    string errorMessage = "Webhook data is not valid: " + postedData;
                    Rock.Model.ExceptionLogService.LogException( new Exception( errorMessage ), null );
                    return false;
                }

                return UpdateBackgroundCheckAndWorkFlow( backgroundCheckWebhook );
            }

            return UpdateUserAndWorkFlow( trainingWebhook );
        }

        #endregion
    }
}