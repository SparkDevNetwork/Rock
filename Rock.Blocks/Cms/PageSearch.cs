using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Data;
using Rock.ViewModels.Blocks.Cms.PageSearch;
using Rock.Web.Cache;
using Rock.Web;
using System.ComponentModel;
using Rock.Model;
using Rock.Security;

namespace Rock.Blocks.Cms
{
    [DisplayName( "Page Search" )]
    [Category( "CMS" )]
    [Description( "Displays a search page to find child pages" )]

    [Rock.SystemGuid.EntityTypeGuid( "85BA51A4-41CF-4F60-9EAE-1D8B1E73C736" )]
    [Rock.SystemGuid.BlockTypeGuid( "A279A88E-D4E0-4867-A108-2AA743B3CFD0" )]
    public class PageSearch : RockBlockType
    {
        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var box = new PageSearchBag();

                var hierarchicalPages = GetHierarchicalPagesByParent( this.PageCache, rockContext, 2 );

                box.Pages = hierarchicalPages;

                return box;
            }
        }

        private List<PageSearchPageBag> GetHierarchicalPagesByParent( PageCache rootPage, RockContext rockContext, int depth )
        {
            depth--;
            var pages = new List<PageSearchPageBag>();

            foreach ( var page in rootPage.GetPages( rockContext ) )
            {
                // IsAuthorized() knows how to handle a null person argument.
                if ( page.DisplayInNavWhen == DisplayInNavWhen.WhenAllowed && !page.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                {
                    continue;
                }

                if ( page.DisplayInNavWhen == DisplayInNavWhen.Never )
                {
                    continue;
                }

                if ( depth >= 0 )
                {
                    var pageSearchPageBag = new PageSearchPageBag()
                    {
                        Id = page.Id,
                        Title = page.PageTitle,
                        ParentPageId = page.ParentPageId,
                        Children = GetHierarchicalPagesByParent( page, rockContext, depth ),
                        Icon = page.IconCssClass,
                        Description = page.Description,
                        PageDisplayDescription = page.PageDisplayDescription,
                        Url = new PageReference( page.Id, 0, null, null ).BuildUrl()
                    };
                    pages.Add( pageSearchPageBag );
                }
            }

            return pages;
        }

        #endregion
    }
}
