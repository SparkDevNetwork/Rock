//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Linq;
using System.Text;

using Rock.Model;
using Rock.Web;
using Rock.Web.UI;

public partial class SiteMap : RockBlock
{
    /// <summary>
    /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
    /// </summary>
    /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
    protected override void OnInit( EventArgs e )
    {
        base.OnInit( e );

        PageService pageService = new PageService();

        var sb = new StringBuilder();

        sb.AppendLine("<ul id=\"treeview\">");
        foreach ( var page in pageService.Queryable().Where( a => a.ParentPageId == null ).OrderBy( a => a.Order ).ThenBy( a => a.Name) )
        {
            sb.Append( PageNode( page ) );
        }
        sb.AppendLine( "</ul>" );

        lPages.Text = sb.ToString();
    }

    /// <summary>
    /// Adds the page nodes.
    /// </summary>
    /// <param name="page">The page.</param>
    /// <returns></returns>
    protected string PageNode( Page page )
    {
        var sb = new StringBuilder();

        sb.AppendFormat( "<li data-expanded='false' data-model='Page' data-id='{0}'><span><i class=\"icon-file-alt\">&nbsp;</i> <a href='{1}'>{2}</a></span>{3}", page.Id, new PageReference( page.Id ).BuildUrl(), page.Name, Environment.NewLine );

        if ( page.Pages.Any() || page.Blocks.Any() )
        {
            sb.AppendLine("<ul>");

            foreach ( var childPage in page.Pages.OrderBy( a => a.Order ).ThenBy( a => a.Name ) )
            {
                sb.Append( PageNode( childPage ) );
            }

            foreach ( var block in page.Blocks.OrderBy( b => b.Order ) )
            {
                sb.AppendFormat("<li data-expanded='false' data-model='Block' data-id='{0}'><span>{1}{2}:{3}</span></li>{4}", block.Id, CreateConfigIcon(block), block.Name, block.BlockType.Name, Environment.NewLine );
            }

            sb.AppendLine( "</ul>" );
        }

        sb.AppendLine( "</li>" );

        return sb.ToString();
    }

    /// <summary>
    /// Creates the block config icon.
    /// </summary>
    /// <param name="block">The block.</param>
    /// <returns></returns>
    protected string CreateConfigIcon( Block block )
    {
        var blockPropertyUrl = ResolveUrl( string.Format( "~/BlockProperties/{0}?t=Block Properties", block.Id ) );

        return string.Format( "<i class=\"icon-th-large\">&nbsp;</i> <a href=\"javascript: Rock.controls.modal.show($(this), '{0}')\" title=\"Block Properties\"><i class=\"icon-cog\"></i>&nbsp;</a>",
            blockPropertyUrl );
    }
}