namespace Rock.Rest.Jwt
{
    /// <summary>
    /// Represents a JSON Web Token Configuration
    /// </summary>
    public class JwtConfig
    {
        /// <summary>
        /// Gets or sets the audience.
        /// </summary>
        /// <value>
        /// The audience.
        /// </value>
        public string Audience { get; set; }

        /// <summary>
        /// Gets or sets the issuer.
        /// </summary>
        /// <value>
        /// The issuer.
        /// </value>
        public string Issuer { get; set; }

        /// <summary>
        /// Gets or sets the JWKS json file URL.
        /// </summary>
        /// <value>
        /// The JWKS json file URL.
        /// </value>
        public string JwksJsonFileUrl { get; set; }

        /// <summary>
        /// Gets or sets the search type value identifier. This is a defined value referenced in the <see cref="Rock.Model.PersonSearchKey"/>
        /// </summary>
        /// <value>
        /// The search type value identifier.
        /// </value>
        public int? SearchTypeValueId { get; set; }
    }
}
