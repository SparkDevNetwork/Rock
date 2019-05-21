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
using System.Web.UI;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// A <see cref="T:System.Web.UI.WebControls.TextBox"/> control with numerical validation
    /// </summary>
    [ToolboxData( "<{0}:NumberBox runat=server></{0}:NumberBox>" )]
    public class NumberBox : NumberBoxBase
    {
        /// <summary>
        /// Renders the base control.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public override void RenderBaseControl( HtmlTextWriter writer )
        {

            string numberType = NumberType.ConvertToString();
            if ( numberType == "Integer" )
            {
                this.Attributes["pattern"] = "[0-9]*";
            }

            var minValue = MinimumValue.AsIntegerOrNull();
            if ( minValue.HasValue )
            {
                this.Attributes["min"] = minValue.ToString();
            }

            var maxValue = MaximumValue.AsIntegerOrNull();
            if ( maxValue.HasValue )
            {
                this.Attributes["max"] = maxValue.ToString();
            }

            base.RenderBaseControl( writer );
        }
    }
}