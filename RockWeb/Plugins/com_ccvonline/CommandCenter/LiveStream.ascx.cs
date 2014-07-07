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

    //todo: choose stream type block setting

    public partial class LiveStream : RockBlock
    {
        #region Base Control Methods

        protected void Page_Load(object sender, EventArgs e)
        {
            CampusService campusService = new CampusService( new RockContext() );
            List<Campus> campuses = new List<Campus>();

            List<string[]> dataSource = new List<string[]>();

            foreach ( var campus in campusService.Queryable().OrderBy( c => c.Name ) )
            {
                 campus.LoadAttributes();
                string attributeValue = campus.GetAttributeValue( "VenueStreams" );
                if ( !string.IsNullOrWhiteSpace( attributeValue ) )
                {
                    string[] nameValues = attributeValue.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );
                    foreach ( string nameValue in nameValues )
                    {
                        string[] nameAndValue = nameValue.Split( new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries );

                        dataSource.Add( new string[] { campus.Name.ToString(), nameAndValue[1].ToString() } );                        
                    }
                }
            }

            rptvideostreams.DataSource = dataSource.ToList();
            rptvideostreams.DataBind();
        }

        #endregion
        
    }
}