namespace com.bemaservices.MinistrySafe.Constants
{
    /// <summary>
    /// This class holds MinistrySafe settings.
    /// </summary>
    public static class MinistrySafeConstants
    {

        /// <summary>
        /// The URL where the token for the account is retrieved
        /// </summary>
        public const string MINISTRYSAFE_TOKEN_URL = "oauth/tokens";

        /// <summary>
        /// The typename prefix
        /// </summary>
        public const string MINISTRYSAFE_TYPENAME_PREFIX = "MinistrySafe - ";

        /// <summary>
        /// The login URL
        /// </summary>
        public const string MINISTRYSAFE_APISERVER = "https://safetysystem.abusepreventionsystems.com/api/";
        
        /// <summary>
        /// The staging login URL
        /// </summary>
        public const string MINISTRYSAFE_STAGING_APISERVER = "https://staging.abusepreventionsystems.com/api/";

        /// <summary>
        /// The candidates URL
        /// </summary>
        public const string MINISTRYSAFE_USERS_URL = "v2/users";

        /// <summary>
        /// The packages URL
        /// </summary>
        public const string MINISTRYSAFE_PACKAGES_URL = "v2/custom_background_check_packages";

        /// <summary>
        /// The report URL
        /// </summary>
        public const string MINISTRYSAFE_BACKGROUNDCHECK_URL = "v2/background_checks";

        /// <summary>
        /// The default checkr workflow type name
        /// </summary>
        public const string MINISTRYSAFE_WORKFLOW_TYPE_NAME = "MinistrySafe Safe Training";
    }
}