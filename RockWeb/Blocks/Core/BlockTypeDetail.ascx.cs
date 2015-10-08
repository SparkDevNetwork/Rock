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
using System.Text;
using System.Web.UI;
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
    public partial class BlockTypeDetail : RockBlock, IDetailBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( "blockTypeId" ).AsInteger() );
            }
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
            NavigateToParentPage();
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
                BlockTypeCache.Flush( blockTypeId );
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

            NavigateToParentPage();
        }

        /// <summary>
        /// Gets the name of the fully qualified page.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        private string GetFullyQualifiedPageName( Rock.Model.Page page )
        {
            string result = Server.HtmlEncode( page.InternalName );

            result = string.Format( "<a href='{0}'>{1}</a>", new PageReference( page.Id ).BuildUrl(), result );

            Rock.Model.Page parent = page.ParentPage;
            while ( parent != null )
            {
                result = string.Format( "<a href='{0}'>{1}</a> / {2}", new PageReference( parent.Id ).BuildUrl(), Server.HtmlEncode( parent.InternalName ), result );
                parent = parent.ParentPage;
            }

            return string.Format( "<li>{0}</li>", result );
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
            }
            
            if (blockType == null)
            {
                blockType = new BlockType { Id = 0 };
                lActionTitle.Text = ActionTitle.Add( BlockType.FriendlyTypeName ).FormatAsHtmlTitle();
                lPages.Visible = false;
                lblStatus.Visible = false;
            }

            hfBlockTypeId.Value = blockType.Id.ToString();
            tbName.Text = blockType.Name;
            tbPath.Text = blockType.Path;
            tbDescription.Text = blockType.Description;
            StringBuilder sb = new StringBuilder();
            foreach ( var fullPageName in blockType.Blocks.ToList().Where( a => a.Page != null ).Select(a => GetFullyQualifiedPageName(a.Page)).OrderBy(a => a))
            {
                sb.Append( fullPageName );
            }

            if ( sb.Length == 0 )
            {
                lPages.Text = "<span class='text-muted'><em>No pages are currently using this block</em></muted>";
            }
            else
            {
                lPages.Text = string.Format( "<ul>{0}</ul>", sb.ToString() );
            }

            string blockPath = Request.MapPath( blockType.Path );
            if ( !System.IO.File.Exists( blockPath ) )
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

            btnSave.Visible = !readOnly;
        }

        #endregion
    }
}