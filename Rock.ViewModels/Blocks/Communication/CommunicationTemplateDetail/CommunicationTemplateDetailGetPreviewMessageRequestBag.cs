using System;
using System.Collections.Generic;

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Communication.CommunicationTemplateDetail
{
    /// <summary>
    /// Represents the request bag for getting the preview message in the Communication Template Detail block.
    /// </summary>
    public class CommunicationTemplateDetailGetPreviewMessageRequestBag
    {
        /// <summary>
        /// Gets or sets the message content.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether CSS inlining is enabled.
        /// </summary>
        public bool IsCssInlined { get; set; }
    }
}
