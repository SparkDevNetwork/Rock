//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class PagePicker : ItemPicker
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            ItemRestUrlExtraParams = string.Empty;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="page">The page.</param>
        public void SetValue( Rock.Model.Page page )
        {
            if ( page != null )
            {
                ItemId = page.Id.ToString();

                string parentPageIds = string.Empty;
                var parentPage = page.ParentPage;
                while ( parentPage != null )
                {
                    parentPageIds = parentPage.Id + "," + parentPageIds;
                    parentPage = parentPage.ParentPage;
                }

                InitialItemParentIds = parentPageIds.TrimEnd( new char[] { ',' } );
                ItemName = page.Name;
            }
            else
            {
                ItemId = Rock.Constants.None.IdValue;
                ItemName = Rock.Constants.None.TextHtml;
            }
        }

        /// <summary>
        /// Sets the values.
        /// </summary>
        /// <param name="pages">The pages.</param>
        public void SetValues( IEnumerable<Rock.Model.Page> pages )
        {
            var thePages = pages.ToList();

            if ( thePages.Any() )
            {
                var ids = new List<string>();
                var names = new List<string>();
                var parentPageIds = string.Empty;

                foreach ( var page in thePages )
                {
                    if ( page != null )
                    {
                        ids.Add( page.Id.ToString() );
                        names.Add( page.Name );
                        var parentPage = page.ParentPage;

                        while ( parentPage != null )
                        {
                            parentPageIds += parentPage.Id.ToString() + ",";
                            parentPage = parentPage.ParentPage;
                        }
                    }
                }

                InitialItemParentIds = parentPageIds.TrimEnd( new[] { ',' } );
                ItemIds = ids;
                ItemNames = names;
            }
            else
            {
                ItemId = Constants.None.IdValue;
                ItemName = Constants.None.TextHtml;
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSelect control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected override void SetValueOnSelect()
        {
            var page = new PageService().Get( int.Parse( ItemId ) );
            this.SetValue( page );
        }

        /// <summary>
        /// Sets the values on select.
        /// </summary>
        protected override void SetValuesOnSelect()
        {
            var pages = new PageService().Queryable().Where( p => ItemIds.Contains( p.Id.ToString() ) );
            this.SetValues( pages );
        }

        /// <summary>
        /// Gets the item rest URL.
        /// </summary>
        /// <value>
        /// The item rest URL.
        /// </value>
        public override string ItemRestUrl
        {
            get { return "~/api/pages/getchildren/"; }
        }
    }
}