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

namespace Rock.ViewModels.Blocks.Communication.NcoaProcess
{
    public class NcoaProcessBag
    {
        public ListItemBag InactiveReason {  get; set; }
        public bool MarkInvalidAsPrevious {  get; set; }
        public bool Mark48MonthAsPrevious { get; set; }
        public decimal MinMoveDistance { get; set; }
        public ListItemBag NcoaFileUploadReference { get; set; }
        public Dictionary<int, NcoaProcessPersonAddressItem> Addresses { get; set; }
        public string SuccessMessage { get; set; }
    }
}
