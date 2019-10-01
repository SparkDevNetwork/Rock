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
using System.Linq;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Results;

using Rock.Data;
using Rock.Model;

namespace Rock.Rest
{
    /// <summary>
    /// 
    /// </summary>
    public class RockApiExceptionHandler : ExceptionHandler
    {
        /// <summary>
        /// Determines whether the exception should be handled.
        /// </summary>
        /// <param name="context">The exception handler context.</param>
        /// <returns>
        /// true if the exception should be handled; otherwise, false.
        /// </returns>
        public override bool ShouldHandle( ExceptionHandlerContext context )
        {
            return true;
        }

        /// <summary>
        /// When overridden in a derived class, handles the exception synchronously.
        /// </summary>
        /// <param name="context">The exception handler context.</param>
        public override void Handle( ExceptionHandlerContext context )
        {
            // check to see if the user is an admin, if so allow them to view the error details
            var userLogin = Rock.Model.UserLoginService.GetCurrentUser();
            GroupService service = new GroupService( new RockContext() );
            Group adminGroup = service.GetByGuid( new Guid( Rock.SystemGuid.Group.GROUP_ADMINISTRATORS ) );
            context.RequestContext.IncludeErrorDetail = userLogin != null && adminGroup.Members.Where( m => m.PersonId == userLogin.PersonId ).Count() > 0;

            ExceptionResult result = context.Result as ExceptionResult;

            // fix up context.Result.IncludeErrorMessage if it didn't get set when we set context.RequestContext.IncludeErrorDetail
            // see comments in https://aspnetwebstack.codeplex.com/workitem/1248
            if ( result != null && result.IncludeErrorDetail != context.RequestContext.IncludeErrorDetail )
            {
                
                context.Result = new ExceptionResult( result.Exception, context.RequestContext.IncludeErrorDetail, result.ContentNegotiator, context.Request, result.Formatters );
            }

            base.Handle( context );
        }
    }
}
