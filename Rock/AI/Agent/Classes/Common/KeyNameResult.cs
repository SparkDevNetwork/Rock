using System.Text.Json.Serialization;

namespace Rock.AI.Agent.Classes.Common
{
    /// <summary>
    /// Lightweight model that pairs a stable <see cref="IdKey"/> (IdKey) with a human-readable <see cref="Name"/>.
    /// For example: { name: "John Doe", key: "" }.
    /// </summary>
    /// <remarks>
    /// Use the key for tool calls and internal lookups; prefer the name for user-facing text.
    /// </remarks>
    public class KeyNameResult
    {

        /// <summary>
        /// Human-readable name for the entity.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Stable identifier for the entity (used by tools; avoid showing to end users unless requested).
        /// </summary>
        public string IdKey
        {
            get
            {
                if ( _idKey.IsNullOrWhiteSpace() && this.Id != null )
                {
                    _idKey = Id.Value.AsIdKey();
                }

                return _idKey;
            }
            set
            {
                _idKey = value;
            }
        }
        private string _idKey;

        /// <summary>
        /// The internal integer ID for the entity. This will not be show in the JSON output.
        /// </summary>
        [JsonIgnore]
        public int? Id { get; set; }

        /// <summary>
        /// Creates a new <see cref="KeyNameResult"/>.
        /// </summary>
        public KeyNameResult()
        {
        }

        /// <summary>
        /// Creates a new <see cref="KeyNameResult"/> with the provided key and name.
        /// </summary>
        /// <param name="key">The stable identifier for the entity.</param>
        /// <param name="name">The display name for the entity.</param>
        public KeyNameResult( string key, string name )
        {
            IdKey = key;
            Name = name;
        }

        /// <summary>
        /// Creates a new <see cref="KeyNameResult"/> with the provided id and name.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        public KeyNameResult( int id, string name )
        {
            Id = id;
            Name = name;
        }
    }
}