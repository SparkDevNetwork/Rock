//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Web.UI;

namespace Rock.Web.UI
{
    /// <summary>
    /// 
    /// </summary>
    public class DialogMasterPage : MasterPage
    {
        /// <summary>
        /// An optional subtitle
        /// </summary>        
        public string SubTitle { get; set; }
        
        /// <summary>
        /// Fires the save.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        public void FireSave( object sender, EventArgs e )
        {
            if ( OnSave != null )
            {
                OnSave( sender, e );
            }
        }

        /// <summary>
        /// Occurs when [on save].
        /// </summary>
        public event EventHandler<EventArgs> OnSave;
    }
}