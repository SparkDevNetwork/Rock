using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;

namespace RockWeb.Plugins.com_ccvonline.CommandCenter
{
    /// <summary>
    /// 
    /// </summary>   
    [DisplayName("Live Stream")]
    [Category( "CCV > Command Center" )]
    [Description("Used for viewing live venue streams.")]

    [CampusField("Campus", "Only show streams from a specific campus", false, order: 0)]
    [TextField("Venue", "Only shows streams for a specfic venue. i.e. Command Center", false, order: 1)]
    public partial class LiveStream : RockBlock
    {
        #region Base Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddScriptLink( "~/Plugins/com_ccvonline/CommandCenter/Assets/flowplayer-3.2.18.swf" );
            RockPage.AddScriptLink( "~/Plugins/com_ccvonline/CommandCenter/Assets/flowplayer.controls-3.2.16.swf" );
            RockPage.AddScriptLink( "~/Plugins/com_ccvonline/CommandCenter/Assets/flowplayer.rtmp-3.2.13.swf" );
            RockPage.AddScriptLink( "~/Plugins/com_ccvonline/CommandCenter/Assets/flowplayer.f4m-3.2.10.swf" );
            RockPage.AddScriptLink( "~/Plugins/com_ccvonline/CommandCenter/Scripts/flowplayer-3.2.13.min.js" );
            RockPage.AddCSSLink( "~/Plugins/com_ccvonline/CommandCenter/Styles/commandcenter.css" );
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            rptvideostreams.DataSource = GetDatasource();
            rptvideostreams.DataBind();

            if ( rptvideostreams.Items.Count < 1)
            {
                ntbAlert.Visible = true;
            }

            
        }

        #endregion

        private List<string[]> GetDatasource()
        {
            string configuredVenue = GetAttributeValue( "Venue" );
            int? campusId = GetAttributeValue( "Campus" ).AsIntegerOrNull();

            var datasource = new List<string[]>();
            var rockContext = new RockContext();
            var campusService = new CampusService( rockContext );

            var theCampuses = campusService.Queryable().ToList();
            theCampuses.ForEach( c => c.LoadAttributes( rockContext ) );

            var campusStreams = theCampuses
                .Where( c => 
                    !campusId.HasValue || 
                    c.Id == campusId.Value )
                .Select( c => new
                {
                    Id = c.Id.ToString(),
                    CampusName = c.Name,
                    Streams = c.GetAttributeValue( "VenueStreams" ).ToString()
                } )
                .ToList();

            int uniqueVideoId = 1;

            foreach ( var campusStream in campusStreams )
            {
                string[] nameValues = campusStream.Streams.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );

                foreach ( var nameValue in nameValues )
                {
                    uniqueVideoId += 1;
                    string[] nameAndValue = nameValue.Split( new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries );

                    if ( String.IsNullOrWhiteSpace( configuredVenue ) || configuredVenue.Equals( nameAndValue[0], StringComparison.OrdinalIgnoreCase ) )
                    {
                        string[] videoOptions = new string[] { campusStream.CampusName + "-" + nameAndValue[0] + "-" + uniqueVideoId, campusStream.CampusName, nameAndValue.Length > 1 ? nameAndValue[1] : ""};

                        datasource.Add( videoOptions );
                    }
                }
            }

            return datasource;
        }
    }
}