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
    [Category( "CCV" )]
    [CategoryField("Command Center > Live Stream")]
    [Description("Used for viewing live venue streams.")]

    //todo: need to wire up
    [CampusField("Campus", "Only show streams from a specific campus", false)]

    [TextField("Venue Type", "Only shows streams for a specfic venue. i.e. Command Center", false)]

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
            var datasource = new List<string[]>();
            var rockContext = new RockContext();
            var campusService = new CampusService( rockContext );

            var theCampuses = campusService.Queryable().ToList();
            theCampuses.ForEach( c => c.LoadAttributes( rockContext ) );

            var campusStreams = theCampuses.Select( c => new
                {
                    Id = c.Id.ToString(),
                    Streams = c.GetAttributeValue( "VenueStreams" ).ToString()
                } ).ToList();

            if ( !String.IsNullOrWhiteSpace( GetAttributeValue( "Campus" ) ) )
            {
                campusStreams = theCampuses.Where( c => c.Id == Int32.Parse( GetAttributeValue( "Campus" ) ) ).Select( c => new
                {
                    Id = c.Id.ToString(),
                    Streams = c.GetAttributeValue( "VenueStreams" ).ToString()
                } ).ToList();
            }

            foreach ( var campusStream in campusStreams )
            {
                string[] nameValues = campusStream.Streams.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );


                foreach (var nameValue in nameValues )
                {
                    string[] nameAndValue = nameValue.Split( new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries );
                    string[] videoOptions = new string[] { nameAndValue[1] + "-" + nameAndValue[0] + "-" + campusStream.Id , nameAndValue[1], nameAndValue[2] };

                    if ( GetAttributeValue( "VenueType" ) != null && GetAttributeValue( "VenueType" ) == nameAndValue[0] )
                    {
                        datasource.Add( videoOptions );
                    }

                    if ( String.IsNullOrWhiteSpace(GetAttributeValue( "VenueType" ) ) )
                    {
                        datasource.Add( videoOptions );
                    }
                }
            }

            return datasource;
        }
    }
}