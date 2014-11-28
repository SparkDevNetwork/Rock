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
using System.Data.Entity.Infrastructure;
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;
using Rock.Client.ErrorHandling;
using Rock.Model;

namespace Rock.Rest.ErrorHandling
{
    /// <summary>
    /// An exception handler for Rock REST API Controllers.
    /// This filter is responsible for translating uncaught .NET Exceptions into corresponding Http Responses.
    /// The response contains information about the error in a standard detail object that can be parsed programatically.
    /// </summary>
    public class RestApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            // Determine if we should include Exception and StackTrace information in the error report.
            // These details are returned if the web service is configured to do so, or if the current user is a Rock Administrator.
            bool includeExceptionDetails = actionExecutedContext.ActionContext.RequestContext.IncludeErrorDetail
                                            || UserLoginService.IsAdministrator(UserLoginService.GetCurrentUser());

            var errorDetail = CreateErrorDetailObject(actionExecutedContext.Exception, includeExceptionDetails);

            actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(HttpStatusCode.BadRequest, errorDetail);
        }

        /// <summary>
        /// Create the error detail object to be returned in the Response.
        /// </summary>
        /// <param name="error"></param>
        /// <param name="includeExceptionDetail"></param>
        /// <returns></returns>
        private RestApiErrorDetail CreateErrorDetailObject(Exception error, bool includeExceptionDetail)
        {
            // Translate known Exceptions to provide additional information in the response.
            if (error is EntityValidationException)
                return CreateEntityValidationExceptionDetail((EntityValidationException)error, includeExceptionDetail);

            if (error is DbUpdateException)
                return CreateDbUpdateExceptionDetail((DbUpdateException)error, includeExceptionDetail);

            // If no specific processing of this exception is available, return a default error detail object.
            return CreateBaseExceptionDetail(error, includeExceptionDetail);
        }

        private RestApiErrorDetail CreateBaseExceptionDetail(Exception e, bool showAdministrativeDetails)
        {
            // Create a default detail object for this error. 
            var errorDetail = new RestApiErrorDetail();

            errorDetail.ErrorCode = RestApiConstants.ErrorCodes.Unspecified;

            if (showAdministrativeDetails)
            {
                errorDetail.Message = e.Message;
                errorDetail.Details = GetExceptionMessageSummary(e.InnerException);

                errorDetail.InternalCode = e.GetType().Name;
                errorDetail.StackTrace = e.StackTrace;                
            }
            else
            {
                errorDetail.Message = "The Rock RMS Server encountered a problem while trying to process your request.";
                errorDetail.Details = "Additional information about this error may be obtained by contacting your system administrator.";
            }

            return errorDetail;
        }

        /// <summary>
        /// Provides a summary of messages for the specified Exception and all associated inner Exceptions.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        private string GetExceptionMessageSummary(Exception ex)
        {
            string message = string.Empty;
            string lastMessage = string.Empty;

            while (ex != null)
            {
                // Add the inner exception message unless it is simply a repeat of the previous message.
                if (!ex.Message.Equals(lastMessage))
                {
                    if (message.Length > 0)
                        message += Environment.NewLine;

                    message += ex.Message;
                }

                lastMessage = ex.Message;

                ex = ex.InnerException;
            }

            return message;
        }

        /// <summary>
        /// Create an ErrorDetail object for an Entity Validation Exception.
        /// </summary>
        /// <param name="error"></param>
        /// <param name="includeExceptionDetail"></param>
        /// <returns></returns>
        private RestApiErrorDetail CreateEntityValidationExceptionDetail(EntityValidationException error, bool includeExceptionDetail)
        {
            var errorDetail = CreateBaseExceptionDetail(error, includeExceptionDetail);

            errorDetail.InternalCode = RestApiConstants.ErrorCodes.EntityValidationError;

            // If details are included, return the Validation Messages in the Details property.
            if (includeExceptionDetail)
                errorDetail.Details = String.Join(Environment.NewLine, error.Messages);

            return errorDetail;
        }

        /// <summary>
        /// Create an ErrorDetail object for an Entity Update Exception.
        /// </summary>
        /// <param name="error"></param>
        /// <param name="includeExceptionDetail"></param>
        /// <returns></returns>
        private RestApiErrorDetail CreateDbUpdateExceptionDetail(DbUpdateException error, bool includeExceptionDetail)
        {
            var errorDetail = CreateBaseExceptionDetail(error, includeExceptionDetail);

            errorDetail.ErrorCode = RestApiConstants.ErrorCodes.DataAccessError;

            string message = "An error occurred while attempting to update one or more Entities.";
            
            // Return the list of Entities that could not be updated.
            foreach (var entry in error.Entries)
            {
                var entity = entry.Entity;

                message += Environment.NewLine + entity.GetType().Name + ": \"" + entity.ToString() + "\"";
            }

            errorDetail.Message = message;

            return errorDetail;
        }
    }
}
