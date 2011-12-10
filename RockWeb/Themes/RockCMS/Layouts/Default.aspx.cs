using System;

namespace RockWeb.Themes.RockCMS.Layouts
{
    public partial class Default : Rock.Web.UI.Page
    {
        protected override void DefineZones()
        {
            AddZone( "Header", phHeader );
            AddZone( "Menu", Menu );
            AddZone( "ContentLeft", ContentLeft );
            AddZone( "Content", Content );
            AddZone( "ContentRight", ContentRight );
            AddZone( "UpperBand", UpperBand );
            AddZone( "LowerBand", LowerBand );
            AddZone( "LowerContentLeft", LowerContentLeft );
            AddZone( "LowerContent", LowerContent );
            AddZone( "LowerContentRight", LowerContentRight );
            AddZone( "Footer", Footer );
        }
    }
}