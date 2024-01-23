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

using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Prayer.PrayerRequestEntry
{
    /// <summary>
    /// The bag containing the response information from saving a prayer request.
    /// </summary>
    public class PrayerRequestEntrySaveResponseBag
    {
        /// <summary>
        /// The list of error messages if the prayer request failed to save.
        /// </summary>
        public List<string> Errors { get; set; }

        /// <summary>
        /// The success message if the prayer request was successfully saved.
        /// </summary>
        public string SuccessMessage { get; set; }
    }
}
