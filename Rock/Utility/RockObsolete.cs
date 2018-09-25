using System;

namespace Rock
{
    /// <summary>
    /// Marks the version at which an [Obsolete] item became obsolete
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage( AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Delegate, Inherited = false )]
    public class RockObsolete : System.Attribute
    {
        private string _version;

        /// <summary>
        /// Initializes a new instance of the <see cref="RockObsolete"/> class.
        /// </summary>
        /// <param name="version">The version when this became obsolete (for example, "v7")</param>
        public RockObsolete( string version )
        {
            _version = version;
        }
    }
}
