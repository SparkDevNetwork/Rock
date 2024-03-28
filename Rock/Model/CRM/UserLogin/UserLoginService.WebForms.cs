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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

using Rock.Data;
using Rock.Security;
using Rock.Tasks;
using Rock.Web.Cache;

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
        /// Updates the last log in, writes to the person's history log, and saves changes to the database
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        public static void UpdateLastLogin( string userName )
        {
            if ( string.IsNullOrWhiteSpace( userName ) )
            {
                return;
            }

            int? personId = null;
            bool impersonated = userName.StartsWith( "rckipid=" );
            using ( var rockContext = new RockContext() )
            {
                if ( !impersonated )
                {
                    var userLogin = new UserLoginService( rockContext ).Queryable().Where( a => a.UserName == userName ).FirstOrDefault();
                    if ( userLogin != null )
                    {
                        userLogin.LastLoginDateTime = RockDateTime.Now;
                        personId = userLogin.PersonId;
                        rockContext.SaveChanges();
                    }
                }
                else
                {
                    var impersonationToken = userName.Substring( 8 );
                    personId = new PersonService( rockContext ).GetByImpersonationToken( impersonationToken, false, null )?.Id;
                }
            }

            if ( personId == null )
            {
                return;
            }

            var relatedDataBuilder = new System.Text.StringBuilder();
            int? relatedEntityTypeId = null;
            int? relatedEntityId = null;

            if ( impersonated )
            {
                var impersonatedByUser = HttpContext.Current?.Session?["ImpersonatedByUser"] as UserLogin;

                relatedEntityTypeId = EntityTypeCache.GetId<Rock.Model.Person>();
                relatedEntityId = impersonatedByUser?.PersonId;

                if ( impersonatedByUser != null )
                {
                    relatedDataBuilder.Append( $" impersonated by { impersonatedByUser.Person.FullName }" );
                }
            }

            if ( HttpContext.Current != null && HttpContext.Current.Request != null )
            {
                string cleanUrl = PersonToken.ObfuscateRockMagicToken( HttpContext.Current.Request.UrlProxySafe().AbsoluteUri );

                // obfuscate the URL specified in the returnurl, just in case it contains any sensitive information (like a rckipid)
                Regex returnurlRegEx = new Regex( @"returnurl=([^&]*)" );
                cleanUrl = returnurlRegEx.Replace( cleanUrl, "returnurl=XXXXXXXXXXXXXXXXXXXXXXXXXXXX" );

                string clientIPAddress;
                try
                {
                    clientIPAddress = Rock.Web.UI.RockPage.GetClientIpAddress();
                }
                catch
                {
                    // if we get an exception getting the IP Address, just ignore it
                    clientIPAddress = "";
                }

                relatedDataBuilder.AppendFormat(
                    " to <span class='field-value'>{0}</span>, from <span class='field-value'>{1}</span>",
                    cleanUrl,
                    clientIPAddress );
            }

            var historyChangeList = new History.HistoryChangeList();
            var cleanUserName = PersonToken.ObfuscateRockMagicToken( userName );
            var historyChange = historyChangeList.AddChange( History.HistoryVerb.Login, History.HistoryChangeType.Record, cleanUserName );

            if ( relatedDataBuilder.Length > 0 )
            {
                historyChange.SetRelatedData( relatedDataBuilder.ToString(), null, null );
            }

            var historyList = HistoryService.GetChanges( typeof( Rock.Model.Person ), Rock.SystemGuid.Category.HISTORY_PERSON_ACTIVITY.AsGuid(), personId.Value, historyChangeList, null, null, null, null, null );

            if ( historyList.Any() )
            {
                Task.Run( async () =>
                {
                    // Wait 1 second to allow all post save actions to complete
                    await Task.Delay( 1000 );
                    try
                    {
                        using ( var rockContext = new RockContext() )
                        {
                            rockContext.BulkInsert( historyList );
                        }
                    }
                    catch ( SystemException ex )
                    {
                        ExceptionLogService.LogException( ex, null );
                    }
                } );
            }
        }
    }
}
