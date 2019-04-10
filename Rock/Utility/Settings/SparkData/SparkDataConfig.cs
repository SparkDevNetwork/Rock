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
using Newtonsoft.Json;

using Rock.Extension;

namespace Rock.Utility.Settings.SparkData
{
    /// <summary>
    /// Settings for Spark Data
    /// </summary>
    public class SparkDataConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SparkDataConfig"/> class.
        /// </summary>
        public SparkDataConfig()
        {
            NcoaSettings = new NcoaSettings();
            SparkDataApiKey = string.Empty;
            Messages = new FixedSizeList<string>(30); // Keep last 30 entries
        }

        /// <summary>
        /// The spark server
        /// </summary>
        [JsonIgnore]
        public static readonly string SPARK_SERVER = "http://www.rockrms.com";
        //public static readonly string SPARK_SERVER = "http://localhost:57822";

        /// <summary>
        /// The minimum addresses required to run NCOA
        /// </summary>
        [JsonIgnore]
        public static readonly int NCOA_MIN_ADDRESSES = 50;

        /// <summary>
        /// Gets or sets the NCOA settings.
        /// </summary>
        /// <value>
        /// The NCOA settings.
        /// </value>
        public NcoaSettings NcoaSettings { get; set; }

        /// <summary>
        /// Gets or sets the spark data API key.
        /// </summary>
        /// <value>
        /// The spark data API key.
        /// </value>
        public string SparkDataApiKey { get; set; }

        /// <summary>
        /// Gets or sets the global notification application group identifier.
        /// </summary>
        /// <value>
        /// The global notification application group identifier.
        /// </value>
        public int? GlobalNotificationApplicationGroupId { get; set; }

        /// <summary>
        /// Gets or sets the messages that communicate to the block.
        /// </summary>
        /// <value>
        /// The messages.
        /// </value>
        public FixedSizeList<string> Messages { get; set; }
    }
}
