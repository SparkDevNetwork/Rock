namespace InteractiveExperienceLoadTest
{
    /// <summary>
    /// The configuration provided by the individual.
    /// </summary>
    internal class Configuration
    {
        /// <summary>
        /// The username to use when authenticating to the server.
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// The password to use when authenticating to the server.
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// The base UI (such as http://localhost:6229/) to use when connecting
        /// to the server.
        /// </summary>
        public Uri? BaseUri { get; set; }

        /// <summary>
        /// The full URL of the live experience page in Rock.
        /// </summary>
        public Uri? LivePageUri { get; set; }

        /// <summary>
        /// The identifier of the experience occurrence to join.
        /// </summary>
        public string? OccurrenceId { get; set; }

        /// <summary>
        /// The total number of clients to initialize and connect.
        /// </summary>
        public int ClientCount { get; set; }

        /// <summary>
        /// The maximum number of network operations to perform concurrently.
        /// </summary>
        public int ConcurrencyLimit { get; set; }

        /// <summary>
        /// The maximum delay in milliseconds each client will wait before
        /// performing an operation. The client will wait a random time between
        /// 0 and this value.
        /// </summary>
        public int DelayVariance { get; set; }
    }
}