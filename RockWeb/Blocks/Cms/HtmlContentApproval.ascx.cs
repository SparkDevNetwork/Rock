// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "HTML Content Approval" )]
    [Category( "CMS" )]
    [Description( "Lists HTML content blocks that need approval." )]

    [SecurityAction( Authorization.APPROVE, "The roles and/or users that have access to approve HTML content." )]
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

            gContentList.DataKeyNames = new string[] { "Id" };
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
        protected void gContentListFilter_ApplyFilterClick( object sender, EventArgs e )
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
        protected void gContentListFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
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
                        var personService = new PersonService( new RockContext() );
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
                var rockContext = new RockContext();
                var htmlContentService = new HtmlContentService( rockContext );
                var htmlContent = htmlContentService.Get( e.RowKeyId );

                if ( htmlContent != null )
                {
                    cannotApprove = false;

                    // if it was approved, set it to unapproved... otherwise
                    if ( htmlContent.IsApproved )
                    {
                        htmlContent.IsApproved = false;
                        htmlContent.ApprovedByPersonAliasId = null;
                        htmlContent.ApprovedDateTime = null;
                    }
                    else
                    {
                        htmlContent.IsApproved = true;
                        htmlContent.ApprovedByPersonAliasId = CurrentPersonAliasId;
                        htmlContent.ApprovedDateTime = RockDateTime.Now;
                    }

                    rockContext.SaveChanges();
                    HtmlContentService.FlushCachedContent( htmlContent.BlockId, htmlContent.EntityValue );
                }

                BindGrid();
            }

            if ( cannotApprove )
            {
                mdGridWarning.Show( "Unable to approve this HTML Content", ModalAlertType.Warning );
            }
        }

        /// <summary>
        /// Handles the RowSelected event of the gContentList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gContentList_RowSelected( object sender, RowEventArgs e )
        {
            var htmlContent = new HtmlContentService(new RockContext()).Get(e.RowKeyId);
            if ( htmlContent != null )
            {
                lPreviewHtml.Text = htmlContent.Content;
                mdPreview.Show();
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gContentList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gContentList_GridRebind( object sender, EventArgs e )
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
            var rockContext = new RockContext();

            var sites = new SiteService( rockContext ).Queryable().OrderBy( s => s.Name ).ToList();
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

            int? personId = gContentListFilter.GetUserPreference( "Approved By" ).AsIntegerOrNull();
            if ( personId.HasValue )
            {
                var personService = new PersonService( rockContext );
                var person = personService.Get( personId.Value );
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
            var rockContext = new RockContext();

            int entityTypeIdBlock = EntityTypeCache.Read( typeof( Rock.Model.Block ), true, rockContext ).Id;
            string entityTypeQualifier = BlockTypeCache.Read( Rock.SystemGuid.BlockType.HTML_CONTENT.AsGuid(), rockContext ).Id.ToString();
            var htmlContentService = new HtmlContentService( rockContext );
            var attributeValueQry = new AttributeValueService( rockContext ).Queryable()
                .Where( a => a.Attribute.Key == "RequireApproval" && a.Attribute.EntityTypeId == entityTypeIdBlock )
                .Where( a => a.Attribute.EntityTypeQualifierColumn == "BlockTypeId" && a.Attribute.EntityTypeQualifierValue == entityTypeQualifier )
                .Where( a => a.Value == "True" )
                .Select( a => a.EntityId );

            var qry = htmlContentService.Queryable().Where( a => attributeValueQry.Contains( a.BlockId ) );

            // Filter by approved/unapproved
            if ( ddlApprovedFilter.SelectedIndex > -1 )
            {
                if ( ddlApprovedFilter.SelectedValue.ToLower() == "unapproved" )
                {
                    qry = qry.Where( a => a.IsApproved == false );
                }
                else if ( ddlApprovedFilter.SelectedValue.ToLower() == "approved" )
                {
                    qry = qry.Where( a => a.IsApproved == true );
                }
            }

            // Filter by the person that approved the content
            if ( _canApprove )
            {
                int? personId = gContentListFilter.GetUserPreference( "Approved By" ).AsIntegerOrNull();
                if ( personId.HasValue )
                {
                    qry = qry.Where( a => a.ApprovedByPersonAliasId.HasValue && a.ApprovedByPersonAlias.PersonId == personId );
                }
            }

            SortProperty sortProperty = gContentList.SortProperty;
            if ( sortProperty != null )
            {
                qry = qry.Sort( sortProperty );
            }
            else
            {
                qry = qry.OrderByDescending( a => a.ModifiedDateTime );
            }

            var selectQry = qry.Select( a => new
            {
                a.Id,
                SiteName = a.Block.PageId.HasValue ? a.Block.Page.Layout.Site.Name : a.Block.Layout.Site.Name,
                PageName = a.Block.Page.InternalName,
                a.ModifiedDateTime,
                a.IsApproved,
                ApprovedDateTime = a.IsApproved ? a.ApprovedDateTime : null,
                ApprovedByPerson = a.IsApproved ? a.ApprovedByPersonAlias.Person : null,
                BlockPageId = a.Block.PageId,
                BlockLayoutId = a.Block.LayoutId,
            } );

            gContentList.EntityTypeId = EntityTypeCache.Read<HtmlContent>().Id;

            // Filter by Site
            if ( ddlSiteFilter.SelectedIndex > 0 )
            {
                if ( ddlSiteFilter.SelectedValue.ToLower() != "all" )
                {
                    selectQry = selectQry.Where( h => h.SiteName == ddlSiteFilter.SelectedValue );
                }
            }

            gContentList.DataSource = selectQry.ToList();
            gContentList.DataBind();
        }

        #endregion
        
}
}