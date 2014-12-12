// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// A Modal Popup Dialog Window
    /// </summary>
    public class ModalIFrameDialog : Panel, INamingContainer
    {
        private Panel _contentPanel;
        private HtmlGenericControl _iFrame;

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            base.Controls.Clear();
            
            this.CssClass = "modal container modal-content rock-modal rock-modal-frame";

            _contentPanel = new Panel();
            this.Controls.Add( _contentPanel );
            _contentPanel.ID = "contentPanel";
            _contentPanel.CssClass = "iframe";

            _iFrame = new HtmlGenericControl( "iframe" );
            _iFrame.ID = "iframe";
            _iFrame.Attributes.Add( "scrolling", "no" );
            _iFrame.Style["height"] = "auto";
            _contentPanel.Controls.Add( _iFrame );
        }
    }
}