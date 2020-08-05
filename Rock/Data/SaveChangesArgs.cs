namespace Rock.Data
{
    /// <summary>
    /// Arguments object for <see cref="DbContext.SaveChanges(SaveChangesArgs)"/>
    /// </summary>
    public sealed class SaveChangesArgs
    {
        /// <summary>
        /// if set to <c>true</c> disables
        /// the Pre and Post processing from being run. This should only be disabled
        /// when updating a large number of records at a time (e.g. importing records).
        /// </summary>
        public bool DisablePrePostProcessing { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether this instance checks for earned achievements on save.
        /// True by default.
        /// Set to false for faster performance.
        /// If <see cref="DisablePrePostProcessing"/> is true, then achievements are disabled no matter what this value is.
        /// </summary>
        public bool IsAchievementsEnabled { get; set; } = true;
    }
}
