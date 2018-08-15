using System;
using System.Web.UI;

using Rock;
using Rock.Web.Cache;
using Rock.Web.UI;
using System.ComponentModel;

namespace RockWeb.Plugins.org_newpointe.LiveMenu
{

    [DisplayName( "Live Menu" )]
    [Category( "NewPointe.org Web Blocks" )]
    [Description( "Main menu" )]

    public partial class LiveMenu : RockBlock
    {
        public string LiveServiceText;

        protected void Page_Load( object sender, EventArgs e )
        {

            if ( GlobalAttributesCache.Value( "LiveService" ).AsBoolean() )
            {
                LiveServiceText = GlobalAttributesCache.Value( "LiveServiceTextLive" );
                
                if ( (string)Session["livePopup"] != "true" )
                {
                    ScriptManager.RegisterStartupScript( this, GetType(), "Pop", "openModal();", true );
                    Session["livePopup"] = "true";
                }

            }
            else
            {
                LiveServiceText = GlobalAttributesCache.Value( "LiveServiceTextNotLive" );
            }


        }
    }
}