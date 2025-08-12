namespace Rock.AI.Agent.Classes.Common
{
    /// <summary>
    /// Lightweight model that pairs a stable <see cref="Key"/> (IdKey) with a human-readable <see cref="Name"/>.
    /// For example: { name: "John Doe", key: "" }.
    /// </summary>
    /// <remarks>
    /// Use the key for function calls and internal lookups; prefer the name for user-facing text.
    /// </remarks>
    public class KeyNameResult
    {

        /// <summary>
        /// Human-readable name for the entity.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Stable identifier for the entity (used by functions; avoid showing to end users unless requested).
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Creates a new <see cref="KeyNameResult"/> with the provided key and name.
        /// </summary>
        /// <param name="key">The stable identifier for the entity.</param>
        /// <param name="name">The display name for the entity.</param>
        public KeyNameResult( string key, string name )
        {
            Key = key;
            Name = name;
        }
    }
}