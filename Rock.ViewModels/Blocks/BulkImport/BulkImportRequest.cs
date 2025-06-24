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

namespace Rock.ViewModels.Blocks.BulkImport
{
    /// <summary>
    /// The request parameters for starting a bulk import.
    /// </summary>
    public class BulkImportRequest
    {
        /// <summary>
        /// Gets or sets the foreign system key.
        /// </summary>
        public string ForeignSystemKey { get; set; }

        /// <summary>
        /// Gets or sets the path to the slingshot file.
        /// </summary>
        public string SlingshotFilePath { get; set; }

        /// <summary>
        /// Gets or sets the type of import to perform (Import, Photos, All).
        /// </summary>
        public string ImportType { get; set; }

        /// <summary>
        /// Gets or sets the type of update to perform (AlwaysUpdate, AddOnly, MostRecentWins).
        /// </summary>
        public string ImportUpdateType { get; set; }

        /// <summary>
        /// Gets or sets the session ID for SignalR notifications.
        /// </summary>
        public string SessionId { get; set; }
    }
} 