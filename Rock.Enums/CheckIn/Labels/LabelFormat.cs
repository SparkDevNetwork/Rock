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
namespace Rock.Enums.CheckIn.Labels
{
    /// <summary>
    /// The format of a label stored in the database.
    /// </summary>
    public enum LabelFormat
    {
        /// <summary>
        /// The label content is in the designer JSON format.
        /// </summary>
        Designed = 0,

        /// <summary>
        /// The label content is in raw ZPL format. This may also contain
        /// Lava to customize the rendered output.
        /// </summary>
        Zpl = 1
    }
}
