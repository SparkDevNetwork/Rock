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
namespace Rock.Obsidian.UI.GridField
{
    /// <summary>
    /// Root-tier grid field mapped to the Obsidian Grid's <c>currency</c> column
    /// type. Right-aligned currency-formatted render.
    /// </summary>
    public class CurrencyObsidianGridField : ObsidianGridField
    {
        /// <inheritdoc/>
        public sealed override string ColumnType => "currency";
    }
}
