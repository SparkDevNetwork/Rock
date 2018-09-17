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
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// RangeValidator what works with HiddenFieldWithValidationProperty or HiddenFieldWithClass. Derived from ideas from http://stackoverflow.com/a/8761161/1755417
    /// </summary>
    public class HiddenFieldRangeValidator : RangeValidator
    {
        /// <summary>
        /// This is a check of properties to determine any errors made by the developer.
        /// </summary>
        /// <returns>
        /// true if the control properties are valid; otherwise, false.
        /// </returns>
        protected override bool ControlPropertiesValid()
        {
            return true;
        }
    }
}
