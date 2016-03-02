using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.Security;
using System.Web.UI;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_centralaz.Widgets
{
    [DisplayName("Instagram Block")]
    [Category("com_centralaz > Widgets")]
    [Description("Displays a grid of latest pictures from an Instagram feed.")]
    [TextField( "Instagram Client ID", "Instagram Client ID assigned from: instagram.com/developer", true, "c50e35a1f44b4ed3b83a5aa06536886c", Order =1 )]
    [TextField( "Instagram User ID", "Instagram Numeric User ID. Find at:  jelled.com/instagram/lookup-user-id#", true, "298256730", Order = 2 )]
    [CustomDropdownListField( "Image Resolution", "Resolution of each image in the grid.", "thumbnail^Low, low_resolution^Medium, standard_resolution^High", true, "thumbnail", Order = 3 )]
    //[CustomDropdownListField( "Picture Grid Size", "Arrangement of pictures from feed.", "wide^Wide 6x3, medium^Medium 3x3, small^Small 2x2, single^Single 1x1", true, "medium", Order = 4 )]
    [CustomDropdownListField("Picture Grid Size", "Arrangement of pictures from feed.", "medium^Medium 3x3, small^Small 2x2, single^Single 1x1", true, "medium", Order = 4)]
    [TextField("Minimum Image Height","Minimum tile image height in number of pixels.", true, "300", Order = 5)]
    public partial class InstagramBlock : RockBlock
    {

        #region Control Methods

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger(upnlContent);
        }


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        #endregion

        #region Control Events

        protected void Block_BlockUpdated(object sender, EventArgs e)
        {
            LoadContent();
        }

        #endregion

        #region Internal Methods


        public void LoadContent()
        {
            this.NavigateToPage( RockPage.Guid, new Dictionary<string, string>() );
        }

        #endregion

    }
}