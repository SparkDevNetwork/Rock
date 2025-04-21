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
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Core
{
    [DisplayName( "Block Type Detail" )]
    [Category( "Core" )]
    [Description( "Shows the details of a selected block type." )]
    [Rock.SystemGuid.BlockTypeGuid( "A3E648CC-0F19-455F-AF1D-B70A8205802D" )]
    public partial class BlockTypeDetail : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( "BlockTypeId" ).AsInteger() );
            }

            base.OnLoad( e );
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            SetEditMode( false );
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editMode">if set to <c>true</c> [edit mode].</param>
        private void SetEditMode( bool editMode )
        {
            pnlReadOnly.Visible = !editMode;
            pnlEdit.Visible = editMode;
            btnEdit.Visible = !editMode;
            btnSave.Visible = editMode;
            btnCancel.Visible = editMode;
        }

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            SetEditMode( true );
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            BlockType blockType;
            var rockContext = new RockContext();
            BlockTypeService blockTypeService = new BlockTypeService( rockContext );

            int blockTypeId = int.Parse( hfBlockTypeId.Value );

            if ( blockTypeId == 0 )
            {
                blockType = new BlockType();
                blockTypeService.Add( blockType );
            }
            else
            {
                blockType = blockTypeService.Get( blockTypeId );
            }

            blockType.Name = tbName.Text;
            blockType.Path = tbPath.Text;
            blockType.Description = tbDescription.Text;

            if ( !blockType.IsValid )
            {
                // Controls will render the error messages                    
                return;
            }

            rockContext.SaveChanges();

            SetEditMode( false );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="blockTypeId">The block type identifier.</param>
        public void ShowDetail( int blockTypeId )
        {
            pnlDetails.Visible = true;

            // Load depending on Add(0) or Edit
            BlockType blockType = null;

            if ( !blockTypeId.Equals( 0 ) )
            {
                blockType = new BlockTypeService( new RockContext() ).Get( blockTypeId );
                lActionTitle.Text = ActionTitle.Edit( BlockType.FriendlyTypeName ).FormatAsHtmlTitle();
                lPages.Visible = true;
                lblStatus.Visible = true;
                pdAuditDetails.SetEntity( blockType, ResolveRockUrl( "~" ) );
            }

            if ( blockType == null )
            {
                blockType = new BlockType { Id = 0 };
                lActionTitle.Text = ActionTitle.Add( BlockType.FriendlyTypeName ).FormatAsHtmlTitle();
                lPages.Visible = false;
                lblStatus.Visible = false;
                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
            }

            hfBlockTypeId.Value = blockType.Id.ToString();
            tbName.Text = blockType.Name;
            tbPath.Text = blockType.Path;
            tbDescription.Text = blockType.Description;

            lReadonlySummary.Text = new DescriptionList().Add( "Name", blockType.Name ).Add( "Path", blockType.Path ).Add( "Description", blockType.Description ).Html;

            var blocks = BlockCache.All()
                .Where( b => b.BlockTypeId == blockTypeId );
            var pages = blocks
                .Where( b => b.Page != null )
                .OrderBy( b => b.Page.GetFullyQualifiedPageName() )
                .Select( b => b.Page.GetHyperLinkedPageBreadCrumbs() )
                .Select( p => $"<li>{p}</li>" )
                .ToList();
            var layouts = blocks
                .Where( b => b.Layout != null )
                .Select( b => $"<a href='/admin/cms/sites/layouts/{b.LayoutId}'>{b.Layout.Name}</a> (Layout), {b.Zone} (Zone)" )
                .Select( l => $"<li>{l}</li>" )
                .OrderBy( l => l )
                .ToList();
            var sites = blocks
                .Where( b => b.Site != null )
                .Select( b => $"<a href='/admin/cms/sites/{b.SiteId}'>{b.Site.Name}</a> (Site), {b.Zone} (Zone)" )
                .Select( s => $"<li>{s}</li>" )
                .OrderBy( s => s )
                .ToList();

            if ( pages.Any() )
            {
                lPages.Text = string.Format( $"<ul>{string.Join( "", pages )}</ul>" );
            }
            else
            {
                lPages.Text = "<span class='text-muted'><em>No pages are currently using this block</em></muted>";
            }

            if ( layouts.Any() )
            {
                lLayout.Visible = true;
                lLayout.Text = $"<ul>{string.Join( "", layouts )}</ul>";
            }
            if ( sites.Any() )
            {
                lSites.Visible = true;
                lSites.Text = $"<ul>{string.Join( "", sites )}</ul>";
            }

            string blockPath = Request.MapPath( blockType.Path );

            if ( blockType.Path == null && blockType.EntityTypeId.HasValue )
            {
                var entityType = EntityTypeCache.Get( blockType.EntityTypeId.Value );
                lblStatus.Text = string.Format( "<span class='label label-info'>{0}</span>", entityType.Name );
            }
            else if ( !System.IO.File.Exists( blockPath ) )
            {
                lblStatus.Text = string.Format( "<span class='label label-danger'>The file '{0}' [{1}] does not exist.</span>", blockType.Path, blockType.Guid );
            }
            else
            {
                lblStatus.Text = "<span class='label label-success'>Block exists on the file system.</span>";
            }

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( BlockType.FriendlyTypeName );
            }

            if ( blockType.IsSystem )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlySystem( BlockType.FriendlyTypeName );
            }

            if ( readOnly )
            {
                lActionTitle.Text = ActionTitle.View( BlockType.FriendlyTypeName ).FormatAsHtmlTitle();
                btnCancel.Text = "Close";
            }

            tbName.ReadOnly = readOnly;
            tbPath.ReadOnly = readOnly;
            tbDescription.ReadOnly = readOnly;

            btnEdit.Visible = !readOnly;
            pnlEdit.Visible = false;
            pnlReadOnly.Visible = true;
            btnSave.Visible = false;
            btnCancel.Visible = false;
        }

        #endregion
    }
}