//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
using System.Linq;

using Rock.Data;

namespace Rock.CMS
{
	/// <summary>
	/// Page POCO Service class
	/// </summary>
    public partial class PageService : Service<Page, DTO.Page>
    {
		/// <summary>
		/// Gets Pages by Parent Page Id
		/// </summary>
		/// <param name="parentPageId">Parent Page Id.</param>
		/// <returns>An enumerable list of Page objects.</returns>
	    public IEnumerable<Page> GetByParentPageId( int? parentPageId )
        {
            return Repository.Find( t => ( t.ParentPageId == parentPageId || ( parentPageId == null && t.ParentPageId == null ) ) ).OrderBy( t => t.Order );
        }
		
		/// <summary>
		/// Gets Pages by Site Id
		/// </summary>
		/// <param name="siteId">Site Id.</param>
		/// <returns>An enumerable list of Page objects.</returns>
	    public IEnumerable<Page> GetBySiteId( int? siteId )
        {
            return Repository.Find( t => ( t.SiteId == siteId || ( siteId == null && t.SiteId == null ) ) ).OrderBy( t => t.Order );
        }

        /// <summary>
        /// Query DTO objects
        /// </summary>
        /// <returns>A queryable list of related DTO objects.</returns>
        public override IQueryable<DTO.Page> QueryableDTO()
        {
            return this.Queryable().Select( i => new DTO.Page
            {
                Id = i.Id,
                Guid = i.Guid,
				Name = i.Name,
				Title = i.Title,
				IsSystem = i.IsSystem,
				ParentPageId = i.ParentPageId,
				SiteId = i.SiteId,
				Layout = i.Layout,
				RequiresEncryption = i.RequiresEncryption,
				EnableViewState = i.EnableViewState,
				MenuDisplayDescription = i.MenuDisplayDescription,
				MenuDisplayIcon = i.MenuDisplayIcon,
				MenuDisplayChildPages = i.MenuDisplayChildPages,
				DisplayInNavWhen = (int)i.DisplayInNavWhen,
				Order = i.Order,
				OutputCacheDuration = i.OutputCacheDuration,
				Description = i.Description,
				IncludeAdminFooter = i.IncludeAdminFooter,
				CreatedDateTime = i.CreatedDateTime,
				ModifiedDateTime = i.ModifiedDateTime,
				CreatedByPersonId = i.CreatedByPersonId,
				ModifiedByPersonId = i.ModifiedByPersonId,
				IconUrl = i.IconUrl
            }
            );
        }
		
    }
}
