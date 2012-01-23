//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;

namespace Rock.CRM
{
    /// <summary>
    /// System Email Templates
    /// </summary>
    public static class SystemEmailTemplate
    {
        /// <summary>
        /// Gets the template guid for the Forgot Username email
        /// </summary>
        public static Guid SECURITY_FORGOT_USERNAME { get { return new Guid( "113593ff-620e-4870-86b1-7a0ec0409208" ); } }

        /// <summary>
        /// Gets the template guid for the Account Created email
        /// </summary>
        public static Guid SECURITY_ACCOUNT_CREATED { get { return new Guid( "84e373e9-3aaf-4a31-b3fb-a8e3f0666710" ); } }

        /// <summary>
        /// Gets the template guid for the Confirm Account email
        /// </summary>
        public static Guid SECURITY_CONFIRM_ACCOUNT { get { return new Guid( "17aaceef-15ca-4c30-9a3a-11e6cf7e6411" ); } }
    }
}
