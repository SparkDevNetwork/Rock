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
