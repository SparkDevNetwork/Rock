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
using System;
using System.ComponentModel;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Defines a Rock Zone on a page
    /// </summary>
    public class Zone : System.Web.UI.WebControls.PlaceHolder
    {
        /// <summary>
        /// Gets or sets the help tip.
        /// </summary>
        /// <value>
        /// The help tip.
        /// </value>
        [
        Bindable( true ),
        Category( "Misc" ),
        DefaultValue( "" ),
        Description( "The friendly name (will default to the ID)." )
        ]
        public string Name
        {
            get
            {
                string s = ViewState["Name"] as string;
                return s == null ? this.ID.SplitCase() : s;
            }
            set
            {
                ViewState["Name"] = value;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.ClientIDMode = System.Web.UI.ClientIDMode.Static;
        }
    }
}