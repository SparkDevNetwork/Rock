﻿// <copyright>
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

using System;
using System.Linq;
using System.Web.UI;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using System.ComponentModel;
using Rock.Security;
using Rock.Web.Cache;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Personal Link List" )]
    [Category( "CMS" )]
    [Description( "Lists personal link in the system." )]

    public partial class PersonalLinkList : RockBlock, ICustomGridColumns, ISecondaryBlock
    {
        #region PageParameterKeys

        private static class PageParameterKey
        {
            public const string SectionId = "SectionId";
        }

        #endregion PageParameterKey

        #region UserPreferenceKeys

        /// <summary>
        /// Keys to use for UserPreferences
        /// </summary>
        protected static class UserPreferenceKey
        {
            /// <summary>
            /// The Name
            /// </summary>
            public const string Name = "Name";
        }

        #endregion UserPreferanceKeys

        #region Private Variables

        private PersonalLinkSection _personalLinkSection = null;

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            _personalLinkSection = GetPersonalLinkSection();
            if ( _personalLinkSection != null )
            {
                gfFilter.ApplyFilterClick += gfFilter_ApplyFilterClick;
                gfFilter.ClearFilterClick += gfFilter_ClearFilterClick;
                gfFilter.DisplayFilterValue += gfFilter_DisplayFilterValue;

                // Block Security and special attributes (RockPage takes care of View)
                var canEdit = !_personalLinkSection.IsShared || ( _personalLinkSection.IsShared && _personalLinkSection.IsAuthorized( Authorization.EDIT, CurrentPerson ) );

                gLinkList.DataKeyNames = new string[] { "Id" };
                gLinkList.Actions.ShowAdd = canEdit;
                gLinkList.IsDeleteEnabled = canEdit;
                gLinkList.Actions.AddClick += gLinkList_Add;
                gLinkList.GridReorder += gLinkList_GridReorder;
                gLinkList.GridRebind += gLinkList_GridRebind;

                var reOrderField = gLinkList.ColumnsOfType<ReorderField>().FirstOrDefault();
                if ( reOrderField != null )
                {
                    reOrderField.Visible = canEdit;
                }

                var editField = gLinkList.ColumnsOfType<EditField>().FirstOrDefault();
                if ( editField != null )
                {
                    editField.Visible = canEdit;
                }

                var deleteField = gLinkList.ColumnsOfType<DeleteField>().FirstOrDefault();
                if ( deleteField != null )
                {
                    deleteField.Visible = canEdit;
                }

                lTitle.Text = ( "Personal Links for " + _personalLinkSection.Name ).FormatAsHtmlTitle();
            }

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upPanel );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                if ( _personalLinkSection != null )
                {
                    BindGrid();
                }
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events 

        /// <summary>
        /// Handles the BlockUpdated event of the Block control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the Add event of the gLinkList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gLinkList_Add( object sender, EventArgs e )
        {
            tbName.Text = string.Empty;
            urlLink.Text = string.Empty;
            hfPersonalLinkId.Value = "0";

            mdAddPersonalLink.Title = "Add Personal Link";
            mdAddPersonalLink.Show();
        }

        /// <summary>
        /// Handles the Edit event of the gLinkList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gLinkList_Edit( object sender, RowEventArgs e )
        {
            var personalLink = new PersonalLinkService( new RockContext() ).Get( e.RowKeyId );

            if ( personalLink.PersonAliasId != CurrentPersonAliasId.Value && personalLink.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
            {
                mdGridWarning.Show( "Not authorized to make changes to this link.", ModalAlertType.Information );
                return;
            }

            tbName.Text = personalLink.Name;
            urlLink.Text = personalLink.Url;
            hfPersonalLinkId.Value = personalLink.Id.ToString();

            mdAddPersonalLink.Title = "Edit Personal Link";
            mdAddPersonalLink.Show();
        }

        /// <summary>
        /// Handles the Delete event of the gLinkList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gLinkList_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var personalLinkService = new PersonalLinkService( rockContext );
            var personalLink = personalLinkService.Get( e.RowKeyId );

            if ( personalLink != null )
            {
                string errorMessage;
                if ( !personalLinkService.CanDelete( personalLink, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                personalLinkService.Delete( personalLink );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gLinkList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gLinkList_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the GridReorder event of the gLinkList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gLinkList_GridReorder( object sender, GridReorderEventArgs e )
        {
            var rockContext = new RockContext();

            var personalLinks = GetPersonalLinks( rockContext );
            if ( personalLinks != null )
            {
                new PersonalLinkService( rockContext ).Reorder( personalLinks.ToList(), e.OldIndex, e.NewIndex );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Displays the text of the gfFilter control
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void gfFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            gfFilter.SaveUserPreference( UserPreferenceKey.Name, txtLinkName.Text );
            BindGrid();
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the gfFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfFilter_ClearFilterClick( object sender, EventArgs e )
        {
            gfFilter.DeleteUserPreferences();
            BindFilter();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdAddPersonalLink control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void mdAddPersonalLink_SaveClick( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var personalLinkService = new PersonalLinkService( rockContext );
            PersonalLink personalLink = null;

            if ( hfPersonalLinkId.Value.AsInteger() != 0 )
            {
                personalLink = personalLinkService.Get( hfPersonalLinkId.Value.AsInteger() );
            }
            else
            {
                personalLink = new PersonalLink
                {
                    PersonAliasId = CurrentPersonAliasId.Value,
                    SectionId = _personalLinkSection.Id
                };
                personalLinkService.Add( personalLink );
            }

            personalLink.Name = tbName.Text;
            personalLink.Url = urlLink.Text;
            rockContext.SaveChanges();

            mdAddPersonalLink.Hide();
            BindGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            txtLinkName.Text = gfFilter.GetUserPreference( UserPreferenceKey.Name );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var rockContext = new RockContext();
            gLinkList.EntityTypeId = EntityTypeCache.GetId<PersonalLink>();
            gLinkList.DataSource = GetPersonalLinks( rockContext ).ToList();
            gLinkList.DataBind();
        }

        /// <summary>
        /// Get the personal link section
        /// </summary>
        /// <returns></returns>
        private PersonalLinkSection GetPersonalLinkSection( RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();
            var personalLinkSectionService = new PersonalLinkSectionService( rockContext );

            var sectionId = PageParameter( PageParameterKey.SectionId ).AsIntegerOrNull();

            if ( !sectionId.HasValue )
            {
                return null;
            }

            return personalLinkSectionService.Queryable().FirstOrDefault( a => a.Id == sectionId.Value );
        }

        /// <summary>
        /// Gets the personal links.
        /// </summary>
        /// <returns></returns>
        private IQueryable<PersonalLink> GetPersonalLinks( RockContext rockContext )
        {
            var qry = new PersonalLinkService( rockContext ).Queryable().Where( a => a.SectionId == _personalLinkSection.Id );

            // Filter by: Name
            var name = gfFilter.GetUserPreference( UserPreferenceKey.Name ).ToStringSafe();

            if ( !string.IsNullOrWhiteSpace( name ) )
            {
                qry = qry.Where( a => a.Name.Contains( name ) );
            }

            return qry.OrderBy( g => g.Order ).ThenBy( g => g.Name );
        }

        #endregion

        #region ISecondaryBlock

        /// <summary>
        /// Sets the visible.
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlContent.Visible = visible;
        }

        #endregion
    }
}