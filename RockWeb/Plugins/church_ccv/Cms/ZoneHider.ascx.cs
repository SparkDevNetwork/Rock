using System.ComponentModel;
using System.Linq;
using System.Web.UI;

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
    [TextField("HTML To Hide", "Optional comma delimited list of ID tags of html to hide. Tags must be 'runat=server'. Ex: 'masthead, mainfooter'", false, "")]
    public partial class ZoneHider : RockBlock
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( System.EventArgs e )
        {
            base.OnInit( e );
            var urlParameterName = this.GetAttributeValue( "UrlParameterName" );
            var urlParameterValue = this.GetAttributeValue( "UrlParameterValue" );
            var hideBreadcrumbs = this.GetAttributeValue( "HideBreadcrumbs" ).AsBooleanOrNull() ?? false;
            var htmlToHide = this.GetAttributeValue( "HTMLToHide" );

            if ( !string.IsNullOrEmpty( urlParameterName ) && !string.IsNullOrEmpty( urlParameterValue ) )
            {
                if ( this.PageParameter( urlParameterName ) == urlParameterValue )
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

                    if ( hideBreadcrumbs )
                    {
                        var script = "$('.breadcrumb').hide();";
                        ScriptManager.RegisterStartupScript( this, this.GetType(), "HideBreadcrumbs", script, true );
                    }

                    if ( htmlToHide != "" )
                    {
                        var controlsToHide = (htmlToHide ?? string.Empty).Split(',').Select(a => a.Trim()).ToList();
                        foreach (var controlId in controlsToHide)
                        {
                            var control = this.Page.Controls[0].FindControl(controlId);
                            if (control != null) control.Visible = false;
                        }
                    }
                }
            }
        }
    }
}