using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Rock.Media
{
    /// <summary>
    /// This class is used to store and retrieve media element data
    /// </summary>
    [Serializable]
    public class MediaElementData
    {
        /// <summary>
        /// Get or sets an identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [JsonIgnore]
        public Guid Guid { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the public name.
        /// </summary>
        /// <value>
        /// The public name.
        /// </value>
        public string PublicName { get; set; }

        /// <summary>
        /// Gets or sets the link.
        /// </summary>
        /// <value>
        /// The link.
        /// </value>
        public string Link { get; set; }

        /// <summary>
        /// Gets or sets the type of the quality.
        /// </summary>
        /// <value>
        /// The type of the quality.
        /// </value>
        public string Quality { get; set; }

        /// <summary>
        /// Gets or sets the format.
        /// </summary>
        /// <value>
        /// The format.
        /// </value>
        public string Format { get; set; }

        /// <summary>
        /// Gets or sets the width.
        /// </summary>
        /// <value>
        /// The width.
        /// </value>
        public int Width { get; set; }

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        /// <value>
        /// The height.
        /// </value>
        public int Height { get; set; }

        /// <summary>
        /// Gets or sets the frame per second.
        /// </summary>
        /// <value>
        /// The frame per second.
        /// </value>
        public int FPS { get; set; }

        /// <summary>
        /// Gets or sets the size in bytes.
        /// </summary>
        /// <value>
        /// The size in bytes.
        /// </value>
        public int Size { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [allow download].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow download]; otherwise, <c>false</c>.
        /// </value>
        public bool AllowDownload { get; set; }

        /// <summary>
        /// Gets the size of the formatted file size.
        /// </summary>
        /// <value>
        /// The size of the formatted file size.
        /// </value>
        [JsonIgnore]
        public string FormattedFileSize
        {
            get
            {
                return Size.FormatAsMemorySize();
            }
        }

        /// <summary>
        /// Gets the dimension.
        /// </summary>
        /// <value>
        /// The dimension.
        /// </value>
        [JsonIgnore]
        public string Dimension
        {
            get
            {
                return string.Format( "{0}X{1}", Width, Height );
            }
        }
    }
}
