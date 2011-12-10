using System;

namespace RockWeb.Themes.RockCMS.Layouts
{
    public partial class Splash : Rock.Web.UI.Page
    {
        protected override void DefineZones()
        {
            AddZone( "Content", Content );
        }
    }
}