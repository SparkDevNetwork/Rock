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

using Rock.Enums.Controls;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Controls
{
    /// <summary>
    /// POCO to store the settings for the fields.
    /// This is copied from Rock/Mobile/JsonFields/FieldSetting.cs. If any changes are made here,
    /// they may need to be copied there as well.
    /// </summary>
    public class FieldPropertyItemBag : ListItemBag
    {
        /// <summary>
        /// Gets or sets the field source.
        /// </summary>
        /// <value>
        /// The field source.
        /// </value>
        public FieldFormat FieldFormat { get; set; }
    }
}
