//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

namespace Rock.CMS
{
    public partial class Auth
    {
        /// <summary>
        /// The default authorization for a specific action.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public override bool DefaultAuthorization( string action )
        {
            return false;
        }
    }

    /// <summary>
    /// Authorization for a special group of users not defined by a specific role or person
    /// </summary>
    public enum SpecialRole
    {
        /// <summary>
        /// No special role
        /// </summary>
        None = 0,

        /// <summary>
        /// Authorize all users
        /// </summary>
        AllUsers = 1,

        /// <summary>
        /// Authorize all authenticated users
        /// </summary>
        AllAuthenticatedUsers = 2,

        /// <summary>
        /// Authorize all un-authenticated users
        /// </summary>
        AllUnAuthenticatedUsers = 3,
    }
}
