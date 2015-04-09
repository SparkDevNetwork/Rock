using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Plugins.church_ccv.CommandCenter
{
    /// <summary>
    /// 
    /// </summary>   
    [DisplayName("Live Stream")]
    [Category( "CCV > Command Center" )]
    [Description("Used for viewing live venue streams.")]

    [CampusesField("Campus", "Only shows streams from selected campuses. If none are selected, all campuses will be shown.", false, order: 0)]
    [TextField("Venue", "Only shows streams for a specfic venue.", false, order: 1)]
    public partial class LiveStream : RockBlock
    {
        #region Base Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddScriptLink( "~/Plugins/church_ccv/CommandCenter/Scripts/flowplayer-3.2.8.min.js" );
            RockPage.AddCSSLink( "~/Plugins/church_ccv/CommandCenter/Styles/commandcenter.css" );
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
            string campusGuids = GetAttributeValue( "Campus" );

            var datasource = new List<string[]>();

            var campuses = CampusCache.All();
            var campusStreams = campuses
                .Where( c => campusGuids.Contains( c.Guid.ToString() )
                        || String.IsNullOrWhiteSpace( campusGuids ) )
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