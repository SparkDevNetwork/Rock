//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using Rock.Model;
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

        List<Rock.Model.Page> pageList = pageService.Queryable().ToList();
        string treeHtml = "<ul id=\"treeview\">" + Environment.NewLine;
        foreach ( var page in pageService.Queryable().Where( a => a.ParentPageId == null ).OrderBy( a => a.Order ).ThenBy( a => a.Name) )
        {
            treeHtml += string.Format( "<li><i class=\"icon-file-alt\"></i><a href='{0}' >" + page.Name + "</a>" + Environment.NewLine, RockPage.BuildUrl( page.Id, new Dictionary<string, string>() ) );
            AddChildNodes( ref treeHtml, page, pageList, false );
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
    protected void AddChildNodes( ref string nodeHtml, Rock.Model.Page parentPage, List<Rock.Model.Page> pageList, bool writeLiTag )
    {
        var childPages = pageList.Where( a => a.ParentPageId.Equals( parentPage.Id ) );
        if ( childPages.Count() > 0 )
        {
            if ( writeLiTag )
            {
                nodeHtml += "<li>";
            }

            nodeHtml += "<ul>" + Environment.NewLine;

            foreach ( var childPage in childPages.OrderBy( a => a.Order ).ThenBy( a => a.Name ) )
            {
                string childNodeHtml = string.Format( "<li><i class=\"icon-file-alt\"></i><a href='{0}' class='btn'>" + childPage.Name + "</a>" + Environment.NewLine, RockPage.BuildUrl( childPage.Id, new Dictionary<string, string>() ) );
                if ( childPage.Blocks.Count > 0 )
                {
                    childNodeHtml += "<ul>";
                    foreach ( var block in childPage.Blocks.OrderBy( b => b.Order ) )
                    {
                        childNodeHtml += string.Format( "<li><i class=\"icon-th-large\"></i>{1}:{0}</li>", block.Name, block.BlockType.Name );
                    }
                    AddChildNodes( ref childNodeHtml, childPage, pageList, true );
                    childNodeHtml += "</ul>";
                }
                else
                {
                    AddChildNodes( ref childNodeHtml, childPage, pageList, false );
                }
                
                childNodeHtml += "</li>" + Environment.NewLine;
                nodeHtml += childNodeHtml;
            }

            nodeHtml += "</ul>" + Environment.NewLine;
            if ( writeLiTag )
            {
                nodeHtml += "</li>";
            }

        }
    }
}