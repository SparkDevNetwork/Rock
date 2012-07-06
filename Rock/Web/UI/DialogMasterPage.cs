using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rock.Web.UI
{
    public class DialogMasterPage : System.Web.UI.MasterPage
    {
        public void FireSave( object sender, EventArgs e )
        {
            if ( OnSave != null )
                OnSave( sender, e );
        }

        public event EventHandler<EventArgs> OnSave;
    }
}