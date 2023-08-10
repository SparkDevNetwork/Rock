namespace InteractiveExperienceLoadTest
{
    /// <summary>
    /// The parameters to send to the login API endpoint.
    /// </summary>
    internal class LoginParameters
    {
        /// <summary>
        /// The username.
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// The password.
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// If the cookie should persist between sessions.
        /// </summary>
        public bool Persisted { get; set; }
    }
}