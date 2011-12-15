using System;

namespace RockWeb.Themes.RockCMS.Layouts
{
    public partial class Dialog : Rock.Web.UI.Page
    {
        protected override void DefineZones()
        {
            AddZone( "Content", Content );
        }
    }
}