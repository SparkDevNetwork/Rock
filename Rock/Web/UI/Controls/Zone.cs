//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
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