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
using System.Collections.Concurrent;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using Newtonsoft.Json;
using Rock.Attribute;
using Rock.Web.Cache;
using com.bemaservices.MinistrySafe.MinistrySafeApi;
using com.bemaservices.MinistrySafe.Constants;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using com.bemaservices.MinistrySafe.Model;

namespace com.bemaservices.MinistrySafe
{
    /// <summary>
    /// MinistrySafe Training
    /// </summary>
    [Description( "MinistrySafe Training" )]
    [ExportMetadata( "ComponentName", "MinistrySafe" )]

    [EncryptedTextField( "Access Token", "MinistrySafe Access Token", true, "", "", 0, null, true )]
    public class MinistrySafeTraining
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

        public bool SendRequest( RockContext rockContext, Rock.Model.Workflow workflow,
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

                    string surveyTypeName;
                    if ( !GetSurveyTypeName( rockContext, workflow, surveyTypeAttribute, out surveyTypeName, errorMessages ) )
                    {
                        errorMessages.Add( "Unable to get Survey Type." );
                        UpdateWorkflowRequestStatus( workflow, rockContext, "FAIL" );
                        return true;
                    }

                    string userTypeName;
                    if ( !GetUserTypeName( rockContext, workflow, userTypeAttribute, out userTypeName, errorMessages ) )
                    {
                        errorMessages.Add( "Unable to get User Type." );
                        UpdateWorkflowRequestStatus( workflow, rockContext, "FAIL" );
                        return true;
                    }

                    string userId;
                    string directLoginUrl;
                    if ( !GetOrCreateUser( workflow, person, personAliasId.Value, userTypeName, out userId, out directLoginUrl, errorMessages ) )
                    {
                        errorMessages.Add( "Unable to create user." );
                        UpdateWorkflowRequestStatus( workflow, rockContext, "FAIL" );
                        return true;
                    }

                    if ( !AssignTraining( userId, surveyTypeName, errorMessages ) )
                    {
                        errorMessages.Add( "Unable to assign training." );
                        UpdateWorkflowRequestStatus( workflow, rockContext, "FAIL" );
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

                    UpdateWorkflowRequestStatus( workflow, rockContext, "SUCCESS" );

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
                UpdateWorkflowRequestStatus( workflow, rockContext, "FAIL" );
                return true;
            }
        }


        #region Internal Methods

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
                    Rock.Model.ExceptionLogService.LogException( new Exception( "MinistrySafe Error: " + errorMsg ), null );
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
                // ( should only happen first time a training is processed for given workflow type)
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
        /// Updates the workflow, closing it if the reportStatus is blank and the recommendation is "Invitation Expired".
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="recommendation">The recommendation.</param>
        /// <param name="reportLink">The report link.</param>
        /// <param name="reportStatus">The report status.</param>
        /// <param name="rockContext">The rock context.</param>
        private static void UpdateWorkflow( int id, int? score, DateTime completedDateTime, RockContext rockContext )
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
                    .Where( m => m.WorkflowId == externalId && m.ForeignId == 3 )
                    .ToList();

                if ( ministrySafeUsers == null || ministrySafeUsers.Count <= 0 )
                {
                    ministrySafeUsers = ministrySafeUserService
                    .Queryable( "PersonAlias.Person" )
                    .Where( m => m.PersonAliasId == externalId && ( m.ForeignId == 2 || m.ForeignId == 4 ) )
                    .ToList();
                }

                if ( ministrySafeUsers == null || ministrySafeUsers.Count <= 0 )
                {
                    string errorMessage = "Ministry Safe User not found: External ID: " + trainingWebhook.ExternalId;
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
                        UpdateWorkflow( ministrySafeUser.WorkflowId.Value, trainingWebhook.Score, trainingWebhook.CompleteDateTime, rockContext );
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
        private void UpdateWorkflowRequestStatus( Rock.Model.Workflow workflow, RockContext rockContext, string requestStatus )
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
        #endregion

        #region Public Methods

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

            if ( trainingWebhook == null )
            {
                string errorMessage = "Webhook data is not valid: " + postedData;
                Rock.Model.ExceptionLogService.LogException( new Exception( errorMessage ), null );
                return false;
            }

            return UpdateUserAndWorkFlow( trainingWebhook );

        }

        #endregion
    }
}