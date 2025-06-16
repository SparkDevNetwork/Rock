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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.BulkImport
{
    public class CsvImportBlockBox
    {
        /// <summary>
        /// If data has previously been imported, a source for the data was provided. This
        /// is a list of all those sources.
        /// </summary>
        public List<ListItemBag> PreviousSourceDescriptions { get; set; } = new List<ListItemBag>();
    }
}
