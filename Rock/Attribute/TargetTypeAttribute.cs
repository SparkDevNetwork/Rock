using System;

namespace Rock.Attribute
{
    /// <summary>
    /// Defines the target type for the <see cref="Rock.Blocks.RockCustomSettingsProvider"/> implementation.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage( AttributeTargets.Class )]
    public class TargetTypeAttribute : System.Attribute
    {
        /// <summary>
        /// Gets the type of the target.
        /// </summary>
        /// <value>
        /// The type of the target.
        /// </value>
        public Type TargetType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TargetTypeAttribute"/> class.
        /// </summary>
        /// <param name="targetType">Type of the target.</param>
        public TargetTypeAttribute( Type targetType )
        {
            TargetType = targetType;
        }
    }
}
