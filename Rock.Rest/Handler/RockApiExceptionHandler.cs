using System;
using System.Linq;
using System.Web.Http.ExceptionHandling;
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
            
            base.Handle( context );
        }
    }
}
