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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.ViewModels.Blocks.Crm.NcoaResults
{
    /// <summary>
    /// Represents a family group of NCOA history items for display in the NCOA Results block.
    /// </summary>
    public class NcoaFamilyGroupBag
    {
        /// <summary>
        /// The name of the family group (from the Rock family group record).
        /// </summary>
        public string FamilyName { get; set; }

        /// <summary>
        /// The list of NCOA history items belonging to this family group.
        /// </summary>
        public List<NcoaDataBag> NcoaItems { get; set; }
    }
}
