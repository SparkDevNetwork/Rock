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
}
