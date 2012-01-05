using System;

namespace RockWeb.Themes.RockCMS.Layouts
{
    public partial class Default : Rock.Web.UI.Page
    {
        /// <summary>
        /// Sets the page's title.
        /// </summary>
        /// <param name="title">The title.</param>
        public override void SetTitle( string title)
        {
            lPageTitle.Text = title;

            base.SetTitle( title );
        }

        /// <summary>
        /// Define's the content zones 
        /// </summary>
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