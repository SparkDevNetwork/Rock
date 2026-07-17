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
namespace Rock.Enums.Controls
{
    /// <summary>
    /// The type of the control (DropDown, Checkbox, or Toggle) to
    /// use to edit the value.
    /// </summary>
    public enum BooleanControlType
    {
        /// <summary>
        /// Use a dropdown control with TrueText and FalseText as the options
        /// </summary>
        DropDown,

        /// <summary>
        /// Use a checkbox with two states for off and on.
        /// </summary>
        Checkbox,

        /// <summary>
        /// Use a toggle control with TrueText and FalseText as the buttons text.
        /// </summary>
        Toggle
    }
}
