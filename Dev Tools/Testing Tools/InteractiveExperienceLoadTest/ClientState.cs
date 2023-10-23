namespace InteractiveExperienceLoadTest
{
    /// <summary>
    /// The state of the simulated client.
    /// </summary>
    internal enum ClientState
    {
        /// <summary>
        /// The client is initializing and not yet started doing anything.
        /// </summary>
        Initializing = 0,

        /// <summary>
        /// The client is now connecting to the server.
        /// </summary>
        Connecting = 1,

        /// <summary>
        /// The client is fully connected to the server and waiting for commands.
        /// </summary>
        Connected = 2,

        /// <summary>
        /// The client has disconnected from the server.
        /// </summary>
        Disconnected = 3,

        /// <summary>
        /// The client has hit an error and cannot continue.
        /// </summary>
        Error = 4
    }
}