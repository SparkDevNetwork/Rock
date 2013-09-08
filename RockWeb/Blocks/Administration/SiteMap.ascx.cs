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
    /// <param name="Page">The page.</param>
    /// <returns></returns>
    protected string PageNode( Rock.Model.Page Page )
    {
        var sb = new StringBuilder();

        sb.AppendFormat( "<li data-expanded='false'><i class=\"icon-file-alt\">&nbsp;</i> <a href='{0}'>{1}</a>{2}", new PageReference( Page.Id ).BuildUrl(), Page.Name, Environment.NewLine );

        if ( Page.Pages.Any() || Page.Blocks.Any() )
        {
            sb.AppendLine("<ul>");

            foreach ( var childPage in Page.Pages.OrderBy( a => a.Order ).ThenBy( a => a.Name ) )
            {
                sb.Append( PageNode( childPage ) );
            }

            foreach ( var block in Page.Blocks.OrderBy( b => b.Order ) )
            {
                sb.AppendFormat("<li data-expanded='false'>{0}{1}:{2}</li>{3}", CreateConfigIcon(block), block.Name, block.BlockType.Name, Environment.NewLine );
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