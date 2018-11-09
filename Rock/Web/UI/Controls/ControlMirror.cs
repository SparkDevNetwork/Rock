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
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Mirrors the render of another control 
    /// There are several caveats and limitations
    /// https://www.codeproject.com/Articles/13663/Duplicate-WebControls-at-Runtime-for-Better-Web-Us
    /// </summary>
    [ToolboxData( "<{0}:ControlMirror runat=server></{0}:ControlMirror>" )]
    public class ControlMirror : WebControl
    {
        /// <summary>
        /// This will be automatically populated on each Page_Load
        /// with the value of the Mirror control's ControlID attribute.
        /// </summary>
        public string ControlID = null;

        /// <summary>
        /// Gets or sets the control to mirror.
        /// </summary>
        /// <value>
        /// The control to mirror.
        /// </value>
        public Control ControlToMirror { get; set; }

        /// <summary>
        /// Renders the control to the specified HTML writer.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        protected override void Render( HtmlTextWriter writer )
        {
            if ( ControlID != null )
            {
                Control c = Parent.FindControl( ControlID );

                if ( c != null )
                {
                    c.RenderControl( writer );
                    return;
                }
            }

            if ( ControlToMirror != null )
            {
                if ( ControlToMirror is GridActions )
                {
                    // if we are Mirroring a GridActions (we probably are), let it know that ControlMirror is rendering it
                    ( ControlToMirror as GridActions ).RenderControl( writer, true );
                }
                else
                {
                    ControlToMirror.RenderControl( writer );
                }

                return;
            }

            return;
        }

        /* 
         LIMITATIONS
            + The control you specify will be duplicated "precisely" as rendered elsewhere. Again, "precisely". This includes things 
            like the ID attributes. If you have JavaScript that references those ID attributes, you will probably encounter issues.

            + Avoid mirroring data-entry controls (e.g. TextBox, ListBox, CheckBox, DropDownList, ...), as only the topmost one on 
            the page will work properly.. During a postback, the ASP.NET postback handler will only pay attention to the first control 
            matching the ID, and load its values from there. Thus, if you change the contents of a lower-in-the-page instance of the 
            control, its value will be lost on postback.

            + If you use absolute positioning in your layouts, make sure that you do not use them on the mirrored controls. Otherwise both 
            sets of controls will render in the same location, one on top of the other.
       */
    }
}