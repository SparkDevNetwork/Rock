// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//

using System;

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Cms.MediaElementDetail
{
    /// <summary>
    /// The item details for the Media Element Detail block.
    /// </summary>
    public class MediaElementBag : EntityBagBase
    {
        /// <summary>
        /// The close captioning data for the media element. This should be in the format of WebVTT for use by Rock.
        /// </summary>
        public string CloseCaption { get; set; }

        /// <summary>
        /// Gets or sets a description of the Element.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or set the duration in seconds of media element.
        /// </summary>
        public int? DurationSeconds { get; set; }

        /// <summary>
        /// Gets or sets the file data JSON content that will be stored in
        /// the database.
        /// </summary>
        public string FileDataJson { get; set; }

        /// <summary>
        /// Gets or sets the custom provider metric data for this instance.
        /// </summary>
        public string MetricData { get; set; }

        /// <summary>
        /// Gets or sets the Name of the Element. This property is required.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the thumbnail data JSON content that will stored
        /// in the database.
        /// </summary>
        public string ThumbnailDataJson { get; set; }

        /// <summary>
        /// Transcripts Text
        /// </summary>
        public string TranscriptionText { get; set; }

        /// <summary>
        /// EngagementStat
        /// </summary>
        public double EngagementStat { get; set; }

        /// <summary>
        /// PlayCountText
        /// </summary>
        public string PlayCountText {  get; set; }

        /// <summary>
        /// MinutesWatchedText
        /// </summary>
        public string MinutesWatchedText { get; set; }
    }
}
