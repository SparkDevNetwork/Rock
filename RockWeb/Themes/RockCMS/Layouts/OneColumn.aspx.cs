using System;

namespace RockWeb.Themes.RockCMS.Layouts
{
    public partial class OneColumn : Rock.Web.UI.Page
    {
        protected override void DefineZones()
        {
            AddZone( "Header", phHeader );
            AddZone( "Menu", Menu );
            AddZone( "Content", Content );
            AddZone( "Footer", Footer );
        }
    }
}