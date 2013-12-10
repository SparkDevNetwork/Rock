//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using Rock;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    public partial class HtmlContentApproval : Rock.Web.UI.RockBlock
    {
        #region Fields

        private bool _canApprove = false;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Handles the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gContentList.DataKeyNames = new string[] { "id" };
            _canApprove = IsUserAuthorized( "Approve" );
            ppApprovedByFilter.Visible = _canApprove;
            gContentListFilter.ApplyFilterClick += gContentListFilter_ApplyFilterClick;
            gContentListFilter.DisplayFilterValue += gContentListFilter_DisplayFilterValue;
            gContentList.GridRebind += gContentList_GridRebind;
        }

        /// <summary>
        /// Handles the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                BindFilter();
                BindGrid();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the gContentListFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        void gContentListFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            gContentListFilter.SaveUserPreference( "Site", ddlSiteFilter.SelectedValue.ToLower() != "-1" ? ddlSiteFilter.SelectedValue : string.Empty );
            gContentListFilter.SaveUserPreference( "Approval Status", ddlApprovedFilter.SelectedValue.ToLower() != "unapproved" ? ddlApprovedFilter.SelectedValue : string.Empty );
            if ( _canApprove )
            {
                gContentListFilter.SaveUserPreference( "Approved By", ppApprovedByFilter.PersonId.ToString() );
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the filter display for each saved user value
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        void gContentListFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Site":
                    e.Value = ddlSiteFilter.SelectedValue;
                    break;

                case "Approval Status":
                    e.Value = ddlApprovedFilter.SelectedValue;
                    break;

                case "Approved By":
                    int personId = 0;
                    if ( int.TryParse( e.Value, out personId ) && personId != 0 )
                    {
                        var personService = new PersonService();
                        var person = personService.Get( personId );
                        if ( person != null )
                        {
                            e.Value = person.FullName;
                        }
                    }

                    break;
            }
        }

        /// <summary>
        /// Handles the CheckChanged event of the gContentList IsApproved field.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gContentList_CheckedChanged( object sender, RowEventArgs e )
        {
            bool cannotApprove = true;

            if ( e.RowKeyValue != null )
            {
                var htmlContentService = new HtmlContentService();
                var htmlContent = htmlContentService.Get( (int)e.RowKeyValue );

                if ( htmlContent != null )
                {
                    cannotApprove = false;
                    // if it was approved, set it to unapproved... otherwise
                    if ( htmlContent.IsApproved )
                    {
                        htmlContent.IsApproved = false;
                        htmlContent.ApprovedByPersonId = null;
                        htmlContent.ApprovedDateTime = null;
                    }
                    else
                    {
                        htmlContent.IsApproved = true;
                        htmlContent.ApprovedByPersonId = CurrentPersonId;
                        htmlContent.ApprovedDateTime = DateTime.Now;
                    }

                    htmlContentService.Save( htmlContent, CurrentPersonId );
                }

                BindGrid();
            }

            if ( cannotApprove )
            {
                mdGridWarning.Show( "Unable to approve this HTML Content", ModalAlertType.Warning );
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gContentList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void gContentList_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            var sites = new SiteService().Queryable().OrderBy( s => s.Name ).ToList();
            ddlSiteFilter.DataSource = sites;
            ddlSiteFilter.DataBind();
            ddlSiteFilter.Items.Insert( 0, Rock.Constants.All.ListItem );
            ddlSiteFilter.Visible = sites.Any();
            ddlSiteFilter.SetValue( gContentListFilter.GetUserPreference( "Site" ) );

            var item = ddlApprovedFilter.Items.FindByValue( gContentListFilter.GetUserPreference( "Approval Status" ) );
            if ( item != null )
            {
                item.Selected = true;
            }
            else
            {
                ddlApprovedFilter.SelectedIndex = 2;
            }

            int personId = 0;
            if ( int.TryParse( gContentListFilter.GetUserPreference( "Approved By" ), out personId ) )
            {
                var personService = new PersonService();
                var person = personService.Get( personId );
                if ( person != null )
                {
                    ppApprovedByFilter.SetValue( person );
                }
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var htmlContentService = new HtmlContentService();
            var htmlContent = htmlContentService.Queryable();

            string pageName = "";
            string siteName = "";
            var htmlList = new List<HtmlApproval>();
            foreach ( var content in htmlContent )
            {
                content.Block.LoadAttributes();
                var blah = content.Block.GetAttributeValue( "RequireApproval" );
                if ( !string.IsNullOrEmpty( blah ) && blah.ToLower() == "true" )
                {
                    var pageService = new PageService();
                    if ( content.Block.PageId != null )
                    {
                        var page = pageService.Get( (int)content.Block.PageId );
                        if ( page != null )
                        {
                            pageName = page.Name;
                            while ( page.ParentPageId != null )
                            {
                                page = pageService.Get( (int)page.ParentPageId );
                            }
                            var siteService = new SiteService();
                            siteName = siteService.GetByDefaultPageId( page.Id ).Select( s => s.Name ).FirstOrDefault();
                        }
                    }

                    var htmlApprovalClass = new HtmlApproval();
                    htmlApprovalClass.SiteName = siteName;
                    htmlApprovalClass.PageName = pageName;
                    htmlApprovalClass.Block = content.Block;
                    htmlApprovalClass.BlockId = content.BlockId;
                    htmlApprovalClass.Content = content.Content;
                    htmlApprovalClass.Id = content.Id;
                    htmlApprovalClass.IsApproved = content.IsApproved;
                    htmlApprovalClass.ApprovedByPerson = content.ApprovedByPerson;
                    htmlApprovalClass.ApprovedByPersonId = content.ApprovedByPersonId;
                    htmlApprovalClass.ApprovedDateTime = content.ApprovedDateTime;

                    htmlList.Add( htmlApprovalClass );
                }
            }

            // Filter by Site
            if ( ddlSiteFilter.SelectedIndex > 0 )
            {
                if ( ddlSiteFilter.SelectedValue.ToLower() != "all" )
                {
                    htmlList = htmlList.Where( h => h.SiteName == ddlSiteFilter.SelectedValue ).ToList();
                }
            }

            // Filter by approved/unapproved
            if ( ddlApprovedFilter.SelectedIndex > -1 )
            {
                if ( ddlApprovedFilter.SelectedValue.ToLower() == "unapproved" )
                {
                    htmlList = htmlList.Where( a => a.IsApproved == false ).ToList();
                }
                else if ( ddlApprovedFilter.SelectedValue.ToLower() == "approved" )
                {
                    htmlList = htmlList.Where( a => a.IsApproved == true ).ToList();
                }
            }

            // Filter by the person that approved the content
            if ( _canApprove )
            {
                int personId = 0;
                if ( int.TryParse( gContentListFilter.GetUserPreference( "Approved By" ), out personId ) && personId != 0 )
                {
                    htmlList = htmlList.Where( a => a.ApprovedByPersonId.HasValue && a.ApprovedByPersonId.Value == personId ).ToList();
                }
            }

            SortProperty sortProperty = gContentList.SortProperty;
            if ( sortProperty != null )
            {
                gContentList.DataSource = htmlList.AsQueryable().Sort( sortProperty ).ToList();
            }
            else
            {
                gContentList.DataSource = htmlList.OrderBy( h => h.Id ).ToList();
            }

            gContentList.DataBind();

        }

        #endregion

        #region HtmlApproval Class

        /// <summary>
        /// A class to hold all the info for the HtmlContentApproval Block
        /// </summary>
        [Serializable()]
        protected class HtmlApproval
        {
            public string SiteName { get; set; }
            public string PageName { get; set; }
            public Block Block { get; set; }
            public int BlockId { get; set; }
            public string Content { get; set; }
            public int Id { get; set; }
            public bool IsApproved { get; set; }
            public Person ApprovedByPerson { get; set; }
            public int? ApprovedByPersonId { get; set; }
            public DateTime? ApprovedDateTime { get; set; }

            public HtmlApproval()
            {
                SiteName = string.Empty;
                PageName = string.Empty;
                Block = new Block();
                BlockId = 0;
                Content = string.Empty;
                Id = 0;
                IsApproved = false;
                ApprovedByPerson = new Person();
                ApprovedByPersonId = 0;
                ApprovedDateTime = new DateTime();
            }
        }

        #endregion
    }
}