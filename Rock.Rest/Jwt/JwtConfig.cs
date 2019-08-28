using Rock.Model;

namespace Rock.Rest.Jwt
{
    /// <summary>
    /// Represents a JSON Web Token Configuration. These records are persisted as JSON Web Token Configuration DefinedValues
    /// </summary>
    public class JwtConfig
    {
        /// <summary>
        /// Gets or sets the audience. If not set, Rock will not require Audience validation.
        /// </summary>
        /// <value>
        /// The audience.
        /// </value>
        public string Audience { get; set; }

        /// <summary>
        /// Gets or sets the issuer. If not set, Rock will not require Issuer validation.
        /// </summary>
        /// <value>
        /// The issuer.
        /// </value>
        public string Issuer { get; set; }

        /// <summary>
        /// Gets or sets the OpenId configuration URL.
        /// This is required for JWT validation.
        /// Looks like: https://xxxxx.auth0.com/.well-known/openid-configuration
        /// </summary>
        /// <value>
        /// The open identifier configuration URL.
        /// </value>
        public string OpenIdConfigurationUrl { get; set; }

        /// <summary>
        /// Gets or sets the search type value identifier. This is a defined value that correlates to <see cref="PersonSearchKey.SearchTypeValueId"/>.
        /// This is required for JWT validation.
        /// </summary>
        /// <value>
        /// The search type value identifier.
        /// </value>
        public int? SearchTypeValueId { get; set; }
    }
}
