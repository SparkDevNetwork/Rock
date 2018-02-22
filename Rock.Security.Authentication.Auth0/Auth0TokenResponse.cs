namespace Rock.Security.Authentication.Auth0
{
    /// <summary>
    /// 
    /// </summary>
    internal class Auth0TokenResponse
    {
        /// <summary>
        /// Gets or sets the access token.
        /// </summary>
        /// <value>
        /// The access token.
        /// </value>
        public string access_token { get; set; }

        /// <summary>
        /// Gets or sets the refresh token.
        /// </summary>
        /// <value>
        /// The refresh token.
        /// </value>
        public string refresh_token { get; set; }

        /// <summary>
        /// Gets or sets the identifier token.
        /// </summary>
        /// <value>
        /// The identifier token.
        /// </value>
        public string id_token { get; set; }

        /// <summary>
        /// Gets or sets the expires in.
        /// </summary>
        /// <value>
        /// The expires in.
        /// </value>
        public int expires_in { get; set; }

        /// <summary>
        /// Gets or sets the type of the token.
        /// </summary>
        /// <value>
        /// The type of the token.
        /// </value>
        public string token_type { get; set; }
    }
}
