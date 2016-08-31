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
    /// 
    /// </summary>
    public class RockRadioButton : RadioButton
    {
        /// <summary>
        /// Gets or sets a value indicating whether [display inline].
        /// Defaults to True
        /// True will render the label with class="radio-inline"
        /// </summary>
        /// <value>
        ///   <c>true</c> if [display inline]; otherwise, <c>false</c>.
        /// </value>
        public bool DisplayInline
        {
            get
            {
                return this.ViewState["DisplayInline"] as bool? ?? true;
            }

            set
            {
                this.ViewState["DisplayInline"] = value;
            }
        }
    }
}
