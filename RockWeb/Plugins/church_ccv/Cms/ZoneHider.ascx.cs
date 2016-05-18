using System.ComponentModel;
using System.Linq;
using System.Web.UI.HtmlControls;

using Rock;
using Rock.Attribute;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.church_ccv.Cms
{
    [DisplayName( "Zone Hider" )]
    [Category( "CCV > Cms" )]
    [Description( "Block that can be used to hide Zones based on a URL parameter" )]

    [TextField( "UrlParameterName", "The URL Parameter Key to look at that will hide the zones", true, "ZoneLayout" )]
    [TextField( "UrlParameterValue", "The value to look for that will hide the zones. ", true, "Mobile" )]
    [TextField( "ZonesToHide", "Comma Delimited List of Zones that should be hidden", true, "Header, Navigation, Login, Full Menu, Feature, Sidebar 1, Main, Section A, Section B, Section C, Section D, Footer Left, Footer Right, Footer" )]
    [BooleanField( "Hide Breadcrumbs", "", true )]
    [TextField( "HTML To Hide", "Optional comma delimited list of ID tags of html to hide. Tags must be 'runat=server'. Ex: 'masthead, mainfooter'", false, "" )]
    public partial class ZoneHider : RockBlock
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( System.EventArgs e )
        {
            base.OnInit( e );

            if ( IsHidingEnabled() )
            {
                var zonesToHide = ( this.GetAttributeValue( "ZonesToHide" ) ?? string.Empty ).Split( ',' ).Select( a => a.Trim() ).ToList();
                var allZones = this.Page.ControlsOfTypeRecursive<Zone>().ToList();
                foreach ( var zone in allZones )
                {
                    if ( zonesToHide.Contains( zone.Name ) )
                    {
                        zone.Visible = false;
                    }
                }

                var htmlToHide = this.GetAttributeValue( "HTMLToHide" );

                if ( !string.IsNullOrEmpty( htmlToHide ) )
                {
                    var controlIdsToHide = ( htmlToHide ?? string.Empty ).Split( ',' ).Select( a => a.Trim() ).ToList();
                    var controlsToHide = this.Page.ControlsOfTypeRecursive<HtmlGenericControl>().Where( a => controlIdsToHide.Contains( a.ID ) );

                    foreach ( var control in controlsToHide )
                    {
                        control.Visible = false;
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether [is hiding enabled].
        /// </summary>
        /// <returns></returns>
        private bool IsHidingEnabled()
        {
            var urlParameterName = this.GetAttributeValue( "UrlParameterName" );
            var urlParameterValue = this.GetAttributeValue( "UrlParameterValue" );

            bool hidingEnabled = false;

            if ( !string.IsNullOrEmpty( urlParameterName ) && !string.IsNullOrEmpty( urlParameterValue ) )
            {
                if ( this.PageParameter( urlParameterName ) == urlParameterValue )
                {
                    hidingEnabled = true;
                }
            }

            return hidingEnabled;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( System.EventArgs e )
        {
            base.OnLoad( e );

            if ( IsHidingEnabled() )
            {
                var hideBreadcrumbs = this.GetAttributeValue( "HideBreadcrumbs" ).AsBooleanOrNull() ?? false;
                if ( hideBreadcrumbs )
                {
                    sBreadCrumbStyleHidden.Visible = true;
                    this.RockPage.BreadCrumbs.Clear();
                }
            }
        }
    }
}