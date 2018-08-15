using System;
using System.Linq;

using Rock.Model;
using Rock.Web.UI;
using Rock.Data;
using System.ComponentModel;
using Rock;

[DisplayName( "Custom Menu" )]
[Category( "NewPointe.org Web Blocks" )]
[Description( "Main menu" )]
public partial class Plugins_org_newpointe_CustomMenu_CustomMenu : RockBlock
{

    private static readonly Guid MessageSeriesContentChannelGuid = "06e29efc-a70a-4636-b558-c1ad6f46cd2b".AsGuid();

    protected void Page_Load( object sender, EventArgs e )
    {
        var replacedId = "";
        var replacedImage = "";

        var lastMessageSeries = new ContentChannelService(new RockContext())
            .Get( MessageSeriesContentChannelGuid )
            .Items
            .Where(i => i.StartDateTime < DateTime.Now)
            .OrderByDescending(i => i.StartDateTime)
            .FirstOrDefault();

        if (lastMessageSeries != null)
        {
            replacedId = lastMessageSeries.Id.ToString();
            lastMessageSeries.LoadAttributes();
            replacedImage = lastMessageSeries.GetAttributeValue( "SmallSeriesFeatureImage" ) ?? "";
        }

        var rootPageId = RockPage.Site.DefaultPageId;
        var menuData = new HtmlContentService( new RockContext() ).Queryable()
            .Where( h => h.Block.Page.ParentPageId == rootPageId && h.Block.Page.InternalName != "support-pages" )
            .Select( h => new
            {
                h.Block.Page.Id,
                h.Block.Page.PageTitle,
                HtmlContent = h.Content.Replace( "{{seriesid}}", replacedId ).Replace( "{{seriesimage}}", replacedImage )
            } );

        rptMenuLinks.DataSource = rptMenuDivs.DataSource = menuData.ToList();
        rptMenuLinks.DataBind();
        rptMenuDivs.DataBind();
    }
}