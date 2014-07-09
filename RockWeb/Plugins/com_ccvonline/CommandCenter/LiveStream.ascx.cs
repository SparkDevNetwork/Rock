using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_ccvonline.CommandCenter
{
    /// <summary>
    /// 
    /// </summary>
    
    [DisplayName("Live Stream")]
    [Category( "CCV" )]
    [CategoryField("Command Center > Live Stream")]
    [Description("Used for viewing live streams.")]

    //todo: choose stream type block setting and add campus picker

    public partial class LiveStream : RockBlock
    {
        #region Base Control Methods

        protected void Page_Load(object sender, EventArgs e)
        {
            var rockContext = new RockContext();
            var campusService = new CampusService( rockContext );

            var theCampuses = campusService.Queryable().ToList();
            theCampuses.ForEach( c => c.LoadAttributes( rockContext ) );                

            rptvideostreams.DataSource = theCampuses.Select( c => new {
                Id = "campus-" + c.Id.ToString(),
                Name = c.Name,
                Stream = GetStream(c.GetAttributeValue("VenueStreams"))
            }).ToList();
            rptvideostreams.DataBind();
        }

        private string GetStream(string value)
        {
            if ( !string.IsNullOrWhiteSpace( value ) )
            {
                string[] nameValues = value.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );
                foreach ( string nameValue in nameValues )
                {
                    string[] nameAndValue = nameValue.Split( new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries );
                    return nameAndValue[1];
                }
            }

            return string.Empty;
        }

        #endregion
        
    }
}