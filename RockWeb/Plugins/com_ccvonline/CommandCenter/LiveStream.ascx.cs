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
    [TextField("Venue Type", "Only shows streams for a specfic venue. i.e. Command Center", false, order: 1)]
    public partial class LiveStream : RockBlock
    {
        #region Base Control Methods

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
            string configuredVenueType = GetAttributeValue( "VenueType" );
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

            foreach ( var campusStream in campusStreams )
            {
                string[] nameValues = campusStream.Streams.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );

                foreach ( var nameValue in nameValues )
                {
                    string[] nameAndValue = nameValue.Split( new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries );

                    if ( String.IsNullOrWhiteSpace( configuredVenueType ) || configuredVenueType.Equals( nameAndValue[0], StringComparison.OrdinalIgnoreCase ) )
                    {
                        string[] videoOptions = new string[] { nameAndValue[1] + "-" + nameAndValue[0] + "-" + campusStream.Id, nameAndValue[1], nameAndValue.Length > 2 ? nameAndValue[2] : ""};

                        datasource.Add( videoOptions );
                    }
                }
            }

            return datasource;
        }
    }
}