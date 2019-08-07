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
    /// This is an interface that Rock UI controls can implement if they need additional content to be rendered
    /// prior to control-wrapper
    /// </summary>
    public interface IRockControlAdditionalRendering : IRockControl
    {
        /// <summary>
        /// Renders content after the label.
        /// </summary>
        /// <param name="writer">The writer.</param>
        void RenderAfterLabel( HtmlTextWriter writer );
    }
}
