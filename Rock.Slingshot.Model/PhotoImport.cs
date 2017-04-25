namespace Rock.Slingshot.Model
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.Slingshot.Model.PhotoImport" />
    public class PhotoImport
    {
        /// <summary>
        /// Gets or sets the type of the photo.
        /// </summary>
        /// <value>
        /// The type of the photo.
        /// </value>
        public PhotoImportType PhotoType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public enum PhotoImportType
        {
            Person = 1,
            Family = 2
        }

        /// <summary>
        /// Gets or sets the person foreign identifier.
        /// </summary>
        /// <value>
        /// The person foreign identifier.
        /// </value>
        public int ForeignId { get; set; }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the type of the MIME.
        /// </summary>
        /// <value>
        /// The type of the MIME.
        /// </value>
        public string MimeType { get; set; }

        /// <summary>
        /// Gets or sets the photo data as Base64
        /// </summary>
        /// <value>
        /// The photo data.
        /// </value>
        public string PhotoData { get; set; }
    }
}
