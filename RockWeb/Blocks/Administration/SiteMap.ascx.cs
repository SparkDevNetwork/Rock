using System;
using System.Collections.Generic;
using System.Linq;
using Rock.Cms;
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

        List<Rock.Cms.Page> pageList = pageService.Queryable().ToList();
        string treeHtml = "<ul id=\"treeview\">" + Environment.NewLine;
        foreach ( var page in pageService.Queryable().Where( a => a.ParentPageId == null ).OrderBy( a => a.Order ).ThenBy( a => a.Name) )
        {
            treeHtml += "<li>" + page.Name + Environment.NewLine;
            AddChildNodes( ref treeHtml, page, pageList );
            treeHtml += "</li>" + Environment.NewLine;
        }
        treeHtml += "</ul>" + Environment.NewLine;
        lPages.Text = treeHtml;
    }

    /// <summary>
    /// Adds the child nodes.
    /// </summary>
    /// <param name="nodeHtml">The node HTML.</param>
    /// <param name="parentPage">The parent page.</param>
    /// <param name="pageList">The page list.</param>
    protected void AddChildNodes( ref string nodeHtml, Rock.Cms.Page parentPage, List<Rock.Cms.Page> pageList )
    {
        var childPages = pageList.Where( a => a.ParentPageId.Equals( parentPage.Id ) );
        if ( childPages.Count() > 0 )
        {
            nodeHtml += "<ul>" + Environment.NewLine;

            foreach ( var childPage in childPages.OrderBy( a => a.Order ).ThenBy( a => a.Name ) )
            {
                string childNodeHtml = "<li>" + childPage.Name + Environment.NewLine;
                if ( childPage.Blocks.Count > 0 )
                {
                    childNodeHtml += "<ul>";
                    foreach ( var block in childPage.Blocks.OrderBy( b => b.Order ) )
                    {
                        childNodeHtml += string.Format( "<li><i class=\"icon-th-large\"></i>{1}:{0}</li>", block.Name, block.BlockType.Name );
                    }
                    childNodeHtml += "</ul>";
                }
                AddChildNodes( ref childNodeHtml, childPage, pageList );
                childNodeHtml += "</li>" + Environment.NewLine;
                nodeHtml += childNodeHtml;
            }
            nodeHtml += "</ul>" + Environment.NewLine;
        }
    }
}