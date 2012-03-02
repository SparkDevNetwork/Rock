//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Web.UI.WebControls;

namespace RockWeb.Blocks
{
    [Rock.Attribute.Property( 0, "ShowColor", "ShowColor", "Display", "Show Color Div", false, "True", "Rock", "Rock.FieldTypes.Boolean" )]
    [Rock.Attribute.Property( 1, "Color", "Color", "Display", "A Color, Any color", false, "Khaki", "Rock", "Rock.FieldTypes.Color" )]
    [Rock.Attribute.Property( 2, "Border", "Border", "Display", "Border Style", false, "none", "Rock", "Rock.FieldTypes.SelectSingle" )]
    [Rock.Attribute.Property( 3, "Border Width", "BorderWidth", "width of border", false, "1", "Rock", "Rock.FieldTypes.SelectSingle" )]
    [Rock.Attribute.Property( 4, "Movie", "Movie", "Misc", "Favorite Sci-Fi Movie", false, "Star Wars", "Rock", "Rock.FieldTypes.SelectSingle" )]
    public partial class MyBlock : Rock.Web.UI.Block
    {
        protected override void OnInit( EventArgs e )
        {
            int zero = 0;
            int test = 2 / zero;
            
            
            base.OnInit( e );

            if ( CurrentPerson != null )
                lPersonName.Text = "Person Name: " + CurrentPerson.FullName;
            else
                lPersonName.Text = "Person Name: " + "???";

            lBlockDetails.Text = string.Format(
                "PageId: {0}<br/>Zone: {1}<br/>InstanceBlock: {2}<br/>Path: {3}<br/>",
                base.PageInstance.Id,
                base.BlockInstance.Zone,
                base.BlockInstance.Id,
                base.BlockInstance.Block.Path );

            lBlockTime.Text = DateTime.Now.ToLongTimeString();

            if ( !IsPostBack )
                DisplayItem();

            string itemTest = PageInstance.GetSharedItem( "itemTest" ) as string;
            if ( itemTest == null )
                itemTest = "A";
            else
                itemTest += "B";

            PageInstance.SaveSharedItem( "itemTest", itemTest );

            lItemTest.Text = itemTest;

            Rock.Groups.GroupTypeService _service = new Rock.Groups.GroupTypeService();
            Rock.Groups.GroupType groupType = _service.Get( 2 );

            foreach ( Rock.Groups.GroupType parentType in groupType.ParentGroupTypes )
                lParentGroups.Text += parentType.Name + ":";
            foreach ( Rock.Groups.GroupType childType in groupType.ChildGroupTypes )
                lChildGroups.Text += childType.Name + ":";

            this.AttributesUpdated += MyBlock_AttributesUpdated;
            this.AddAttributeUpdateTrigger(pnlAttributeValues);

            ShowAttributeValue();

        }

        void MyBlock_AttributesUpdated( object sender, EventArgs e )
        {
            ShowAttributeValue();
            pnlAttributeValues.Update();
        }

        private void ShowAttributeValue()
        {
            pnlAttribute.Visible = Boolean.Parse( AttributeValue( "ShowColor" ) ?? "False" );
            string color = AttributeValue( "Color" );
            if (color != string.Empty)
                pnlAttribute.BackColor = System.Drawing.Color.FromName( color );

            lMovie.Text = AttributeValue( "Movie" );

            pnlAttribute.BorderWidth = Convert.ToInt32( AttributeValue( "BorderWidth" ) );

            switch ( AttributeValue( "Border" ) )
            {
                case "solid": pnlAttribute.BorderStyle = BorderStyle.Solid; break;
                case "dashed": pnlAttribute.BorderStyle = BorderStyle.Dashed; break;
                default: pnlAttribute.BorderStyle = BorderStyle.None; break;
            }

        }

        private void DisplayItem()
        {
            string cachedData = GetCacheItem() as string;
            if ( cachedData != null )
                lItemCache.Text = cachedData + " (From Cache)";
            else
            {
                string text = DateTime.Now.ToString();
                AddCacheItem( text );
                lItemCache.Text = text + " (Saved to Cache)";
            }
        }

        protected void bFlushItemCache_Click( object sender, EventArgs e )
        {
            FlushCacheItem();
            DisplayItem();
        }

        //protected void Button1_Click( object sender, EventArgs e )
        //{
        //    ObjectCache cache = MemoryCache.Default;

        //    string fileContents = cache["filecontents"] as string;

        //    if ( fileContents == null )
        //    {
        //        CacheItemPolicy policy = new CacheItemPolicy();
        //        policy.AbsoluteExpiration =
        //        DateTimeOffset.Now.AddSeconds( 10.0 );

        //        List<string> filePaths = new List<string>();
        //        string cachedFilePath = Server.MapPath( "~" ) + "\\cachedText.txt";
        //        filePaths.Add( cachedFilePath );

        //        policy.ChangeMonitors.Add( new
        //        HostFileChangeMonitor( filePaths ) );

        //        // Fetch the file contents.
        //        fileContents =
        //        File.ReadAllText( cachedFilePath ) + "\n" +
        //            " Using built-in cache " + "\n" + DateTime.Now.ToString();

        //        cache.Set( "filecontents", fileContents, policy );

        //        Label1.Text = fileContents;
        //    }

        //}
    }
}