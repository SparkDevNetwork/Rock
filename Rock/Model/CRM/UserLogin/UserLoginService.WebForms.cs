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
using System.Web;

using Rock.Attribute;
using Rock.Data;
using Rock.Net;
using Rock.Security;
using Rock.Tasks;
using Rock.Web.UI;

namespace Rock.Model
{
    public partial class UserLoginService
    {
        /// <summary>
        /// NOTE: This does much more then is sounds like! It returns the <see cref="Rock.Model.UserLogin"/> of the user who is currently logged in,
        /// but it also updates their last activity date, and will sign them out if they are not confirmed or are locked out.
        /// </summary>
        /// <param name="userIsOnline">A <see cref="System.Boolean"/> value that returns the logged in user if <c>true</c>; otherwise can return the impersonated user</param>
        /// <returns>The current <see cref="Rock.Model.UserLogin"/></returns>
        public static UserLogin GetCurrentUser( bool userIsOnline )
        {
            // Don't wrap in using since we are returning an object that needs
            // its navigation properties to work.
            return GetCurrentUser( userIsOnline, new RockContext() );
        }

        /// <summary>
        /// NOTE: This does much more then is sounds like! It returns the <see cref="Rock.Model.UserLogin"/> of the user who is currently logged in,
        /// but it also updates their last activity date, and will sign them out if they are not confirmed or are locked out.
        /// </summary>
        /// <param name="userIsOnline">A <see cref="System.Boolean"/> value that returns the logged in user if <c>true</c>; otherwise can return the impersonated user</param>
        /// <param name="rockContext">The database context to use when accessing the database.</param>
        /// <returns>The current <see cref="Rock.Model.UserLogin"/></returns>
        internal static UserLogin GetCurrentUser( bool userIsOnline, RockContext rockContext )
        {
            string userName = UserLogin.GetCurrentUserName();

            if ( userName.IsNullOrWhiteSpace() )
            {
                return null;
            }

            if ( userName.StartsWith( "rckipid=" ) )
            {
                Rock.Model.PersonTokenService personTokenService = new Model.PersonTokenService( rockContext );
                Rock.Model.PersonToken personToken = personTokenService.GetByImpersonationToken( userName.Substring( 8 ) );
                if ( personToken?.PersonAlias?.Person != null )
                {
                    return personToken.PersonAlias.Person.GetImpersonatedUser();
                }
            }
            else
            {
                var userLoginService = new UserLoginService( rockContext );
                UserLogin user = userLoginService.GetByUserName( userName );

                if ( user != null && userIsOnline )
                {
                    // Save last activity date
                    var message = new UpdateUserLastActivity.Message
                    {
                        UserId = user.Id,
                        LastActivityDate = RockDateTime.Now,
                    };

                    if ( ( user.IsConfirmed ?? true ) && !( user.IsLockedOut ?? false ) )
                    {
                        if ( HttpContext.Current != null && HttpContext.Current.Session != null )
                        {
                            HttpContext.Current.Session["RockUserId"] = user.Id;
                        }

                        message.SendIfNeeded();
                    }
                    else
                    {
                        // Even though we are in the userIsOnline == true condition,
                        // The user is either not confirmed or is locked out, so we'll mark them
                        // as offline and sign them out.

                        message.IsOnline = false;
                        message.SendIfNeeded();

                        Authorization.SignOut();
                        return null;
                    }
                }

                return user;
            }

            return null;
        }

        /// <summary>
        /// Updates an individual's last successful login date time and writes the event to the person's <see cref="HistoryLogin"/> log.
        /// </summary>
        /// <param name="userName">The user name of the individual who successfully logged in.</param>
        [RockObsolete( "17.0" )]
        [Obsolete( "Use the UpdateLastLogin method that takes a UpdateLastLoginArgs parameter instead." )]
        public static void UpdateLastLogin( string userName )
        {
            UpdateLastLogin( new UpdateLastLoginArgs { UserName = userName } );
        }

        /// <summary>
        /// Updates an individual's last successful login date time and optionally writes the event to the person's
        /// <see cref="HistoryLogin"/> log.
        /// </summary>
        /// <param name="args">The arguments to specify how an individual's last login should be updated.</param>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        [RockInternal( "17.0" )]
        public static void UpdateLastLogin( UpdateLastLoginArgs args )
        {
            if ( args?.UserName.IsNotNullOrWhiteSpace() != true )
            {
                return;
            }

            var userName = args.UserName;

            int? personId = null;
            UserLogin userLogin = null;
            bool impersonated = userName.StartsWith( "rckipid=" );

            using ( var rockContext = new RockContext() )
            {
                if ( !impersonated )
                {
                    userLogin = new UserLoginService( rockContext ).Queryable().Where( a => a.UserName == userName ).FirstOrDefault();
                    if ( userLogin != null )
                    {
                        userLogin.LastLoginDateTime = RockDateTime.Now;
                        personId = userLogin.PersonId;
                        rockContext.SaveChanges();
                    }
                }
                else if ( !args.ShouldSkipWritingHistoryLog )
                {
                    var impersonationToken = userName.Substring( 8 );
                    personId = new PersonService( rockContext ).GetByImpersonationToken( impersonationToken, false, null )?.Id;
                }

                if ( args.ShouldSkipWritingHistoryLog || personId == null )
                {
                    return;
                }

                var historyLogin = new HistoryLogin
                {
                    UserName = PersonToken.ObfuscateRockMagicToken( userName ),
                    UserLoginId = userLogin?.Id,
                    PersonAliasId = new PersonAliasService( rockContext ).GetPrimaryAliasId( personId.Value ),
                    SourceSiteId = args.SourceSiteIdOverride,
                    WasLoginSuccessful = true
                };

                if ( impersonated )
                {
                    var impersonatedByUser = HttpContext.Current?.Session?["ImpersonatedByUser"] as UserLogin;
                    if ( impersonatedByUser?.Person != null )
                    {
                        var relatedData = new HistoryLoginRelatedData
                        {
                            ImpersonatedByPersonFullName = impersonatedByUser.Person.FullName,
                            LoginContext = "Impersonation"
                        };

                        historyLogin.SetRelatedDataJson( relatedData );
                    }
                }

                historyLogin.SaveAfterDelay();
            }
        }
    }
}
