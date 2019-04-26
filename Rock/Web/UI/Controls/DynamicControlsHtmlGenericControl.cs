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
using System.Web.UI.HtmlControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Use DynamicControlsPlaceholder as a container for controls that will be changed at runtime 
    /// and it is difficult or not possible to create the controls early enough in the page lifecycle to avoid a viewstate issue
    /// Note: there is a small performance hit 
    /// From the Help: 
    ///     The ViewStateModeByIdAttribute class is used to specify a control that requires view-state loading by ID. 
    ///     The default view-state loading behavior is for ASP.NET to load the view-state information for a control by 
    ///     its index in the control tree of the page. There is a performance cost for loading view-state information
    ///     by ID because the page control tree must be searched for the control specifically before loading its view-state information.
    /// </summary>
    [ViewStateModeById]
    public class DynamicControlsHtmlGenericControl : HtmlGenericControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicControlsHtmlGenericControl"/> class.
        /// </summary>
        /// <param name="tag">The name of the element for which this instance of the class is created.</param>
        public DynamicControlsHtmlGenericControl( string tag ) : base( tag ) { }
    }
}
