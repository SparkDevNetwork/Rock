// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Newtonsoft.Json;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Cms.PageList;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Displays a list of pages.
    /// </summary>

    [DisplayName( "Page List" )]
    [Category( "CMS" )]
    [Description( "Displays a list of pages." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "63b5aedc-01d9-43b3-949d-e5492c7ba8c0" )]
    [Rock.SystemGuid.BlockTypeGuid( "57393cef-3afd-4c99-a0ed-74af0efaf59a" )]
    [CustomizedGrid]
    public class PageList : RockBlockType
    {
        #region Keys

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var box = new PageListOptionsBag();

                var hierarchicalPages = GetHierarchicalPagesByParent(this.PageCache, rockContext, 2 );

                box.pages = hierarchicalPages;

                return box;
            }
        }

        private List<PageListSettingsPageBag> GetHierarchicalPagesByParent( PageCache rootPage, RockContext rockContext, int depth )
        {
            depth--;
            var pages = new List<PageListSettingsPageBag>();

            foreach ( var page in rootPage.GetPages( rockContext ) )
            {
                // IsAuthorized() knows how to handle a null person argument.
                //if ( page.DisplayInNavWhen == DisplayInNavWhen.WhenAllowed && !page.IsAuthorized( Authorization.VIEW, currentPerson ) )
                //{
                //    continue;
                //}

                //if ( page.DisplayInNavWhen == DisplayInNavWhen.Never )
                //{
                //    continue;
                //}

                if ( depth >= 0 )
                {
                    var pageListSettingsPage = new PageListSettingsPageBag()
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
                    pages.Add( pageListSettingsPage );
                }
            }

            return pages;
        }

        #endregion

        #region Block Actions



        #endregion
    }
}
