using System;
using System.Linq;

using Microsoft.AspNetCore.Http;

using Rock.Attribute;
using Rock.Data;
using Rock.Security;

namespace Rock.Model
{
    public partial class UserLoginService
    {
        public static UserLogin GetCurrentUser( bool userIsOnline )
        {
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
#if REVIEW_WEBFORMS
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
#else
                    throw new NotImplementedException();
#endif
                }

                historyLogin.SaveAfterDelay();
            }
        }
    }
}
