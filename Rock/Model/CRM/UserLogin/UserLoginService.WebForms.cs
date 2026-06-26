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

using Microsoft.Extensions.DependencyInjection;

using Rock.Attribute;
using Rock.Configuration;
using Rock.Data;
using Rock.Net;

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
        [Obsolete( "Use RockRequestContext.PersonSession instead." )]
        [RockObsolete( "20.0" )]
        public static UserLogin GetCurrentUser( bool userIsOnline )
        {
            var personSession = RockApp.Current.GetRequiredService<IRockRequestContextAccessor>().RockRequestContext?.PersonSession;

            if ( personSession?.UserLogin == null )
            {
                return null;
            }

            // Don't wrap in using since we are returning an object that needs
            // its navigation properties to work.
            return new UserLoginService( new RockContext() )
                .Get( personSession.UserLogin.Id );
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

            if ( userName.StartsWith( "rckipid=" ) )
            {
                throw new ArgumentException( "rckipid usernames are no longer supported.", nameof( args ) );
            }

            using ( var rockContext = RockApp.Current.CreateRockContext() )
            {
                int? personId = null;

                var userLogin = new UserLoginService( rockContext ).Queryable().Where( a => a.UserName == userName ).FirstOrDefault();
                if ( userLogin != null )
                {
                    userLogin.LastLoginDateTime = RockDateTime.Now;
                    personId = userLogin.PersonId;
                    rockContext.SaveChanges();
                }

                if ( args.ShouldSkipWritingHistoryLog || personId == null )
                {
                    return;
                }

                var historyLogin = new HistoryLogin
                {
                    UserName = userName,
                    UserLoginId = userLogin?.Id,
                    PersonAliasId = new PersonAliasService( rockContext ).GetPrimaryAliasId( personId.Value ),
                    SourceSiteId = args.SourceSiteIdOverride,
                    WasLoginSuccessful = true
                };

                historyLogin.SaveAfterDelay();
            }
        }
    }
}
