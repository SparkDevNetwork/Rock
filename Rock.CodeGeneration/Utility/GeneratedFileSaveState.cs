namespace Rock.CodeGeneration.Utility
{
    /// <summary>
    /// The save state of a generated file after it is processed by the
    /// <see cref="Pages.GeneratedFilePreviewPage"/> page.
    /// </summary>
    public enum GeneratedFileSaveState
    {
        /// <summary>
        /// The file has not yet been processed for saving.
        /// </summary>
        NotProcessed,

        /// <summary>
        /// The file already existed and has been updated.
        /// </summary>
        Updated,

        /// <summary>
        /// The did not exist and has been created.
        /// </summary>
        Created,

        /// <summary>
        /// The file was already up to date and has not been changed.
        /// </summary>
        NoChange,

        /// <summary>
        /// The file needed to be created or updated but an error prevented the save.
        /// </summary>
        Failed
    }
}
