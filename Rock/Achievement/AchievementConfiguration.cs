using Rock.Web.Cache;

namespace Rock.Achievement
{
    /// <summary>
    /// Achievement Configuration
    /// </summary>
    public sealed class AchievementConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AchievementConfiguration"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="achiever">The achiever.</param>
        public AchievementConfiguration(EntityTypeCache source, EntityTypeCache achiever)
        {
            SourceEntityTypeCache = source;
            AchieverEntityTypeCache = achiever;
        }

        /// <summary>
        /// Gets or sets the source entity type cache.
        /// </summary>
        /// <value>
        /// The source entity type cache.
        /// </value>
        public EntityTypeCache SourceEntityTypeCache { get; private set; }

        /// <summary>
        /// Gets or sets the achiever entity type cache.
        /// </summary>
        /// <value>
        /// The achiever entity type cache.
        /// </value>
        public EntityTypeCache AchieverEntityTypeCache { get; private set; }
    }
}
