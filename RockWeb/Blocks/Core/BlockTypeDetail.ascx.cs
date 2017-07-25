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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Attribute = Rock.Model.Attribute;

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

            // assign attributes grid actions
            gBlockTypeAttributes.DataKeyNames = new string[] { "Guid" };
            gBlockTypeAttributes.Actions.ShowAdd = true;
            gBlockTypeAttributes.Actions.AddClick += gBlockTypeAttributes_AddClick;
            gBlockTypeAttributes.GridRebind += gBlockTypeAttributes_GridRebind;
            gBlockTypeAttributes.GridReorder += gBlockTypeAttributes_GridReorder;

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
            pnlBlockTypeAttributesGrid.Visible = !editMode && hfIsDynamicAttributesBlock.Value.AsBoolean();
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

            SetEditMode( false );
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

            try
            {
                var blockControlType = System.Web.Compilation.BuildManager.GetCompiledType( blockType.Path );
                bool dynamicAttributesBlock = typeof( Rock.Web.UI.IDynamicAttributesBlock ).IsAssignableFrom( blockControlType );
                hfIsDynamicAttributesBlock.Value = dynamicAttributesBlock.ToTrueFalse();
            }
            catch
            {
                // if the block can't compile, ignore
            }

            lReadonlySummary.Text = new DescriptionList().Add( "Name", blockType.Name ).Add( "Path", blockType.Path ).Add( "Description", blockType.Description ).Html;

            StringBuilder sb = new StringBuilder();
            foreach ( var fullPageName in blockType.Blocks.ToList().Where( a => a.Page != null ).Select( a => GetFullyQualifiedPageName( a.Page ) ).OrderBy( a => a ) )
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

            btnEdit.Visible = !readOnly;
            pnlEdit.Visible = false;
            pnlReadOnly.Visible = true;
            btnSave.Visible = false;
            btnCancel.Visible = false;

            BindBlockTypeAttributesGrid();
        }

        #endregion

        #region DynamicBlockTypeAttributes

        /// <summary>
        /// Handles the GridRebind event of the gBlockTypeAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.GridRebindEventArgs"/> instance containing the event data.</param>
        private void gBlockTypeAttributes_GridRebind( object sender, Rock.Web.UI.Controls.GridRebindEventArgs e )
        {
            BindBlockTypeAttributesGrid();
        }

        /// <summary>
        /// Handles the AddClick event of the gBlockTypeAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gBlockTypeAttributes_AddClick( object sender, EventArgs e )
        {
            gBlockTypeAttributes_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the EditClick event of the gBlockTypeAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gBlockTypeAttributes_EditClick( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            Guid attributeGuid = (Guid)e.RowKeyValue;
            gBlockTypeAttributes_ShowEdit( attributeGuid );
        }

        /// <summary>
        /// Binds the block type attributes grid.
        /// </summary>
        private void BindBlockTypeAttributesGrid()
        {
            if ( hfIsDynamicAttributesBlock.Value.AsBoolean() )
            {
                string qualifierValue = hfBlockTypeId.Value;
                List<string> blockStaticAttributeKeys = GetBlockTypeStaticAttributeKeys();

                var attributes = new AttributeService( new RockContext() )
                    .GetByEntityTypeId( new Rock.Model.Block().TypeId ).AsQueryable()
                    .Where( a =>
                        a.EntityTypeQualifierColumn.Equals( "BlockTypeId", StringComparison.OrdinalIgnoreCase ) &&
                        a.EntityTypeQualifierValue.Equals( qualifierValue ) )
                    .OrderBy( a => a.Order )
                    .ThenBy( a => a.Name )
                    .ToList()
                    .Select( a => new
                    {
                        a.Guid,
                        a.Name,
                        IsDynamicAttribute = !blockStaticAttributeKeys.Contains( a.Key )
                    } ).ToList();

                gBlockTypeAttributes.DataSource = attributes;
                gBlockTypeAttributes.DataBind();
            }
        }

        /// <summary>
        /// Gets the block type attributes that are defined in code in the blocktype
        /// </summary>
        /// <returns></returns>
        private List<string> GetBlockTypeStaticAttributeKeys()
        {
            var blockTypeCache = BlockTypeCache.Read( hfBlockTypeId.Value.AsInteger() );
            List<FieldAttribute> blockProperties = new List<FieldAttribute>(); ;
            try
            {
                var blockControlType = System.Web.Compilation.BuildManager.GetCompiledType( blockTypeCache.Path );

                foreach ( var customAttribute in blockControlType.GetCustomAttributes( typeof( FieldAttribute ), true ) )
                {
                    blockProperties.Add( (FieldAttribute)customAttribute );
                }
            }
            catch
            {
                // ignore if the block can't compile
            }

            var blockStaticAttributeKeys = blockProperties.Select( a => a.Key ).ToList();
            return blockStaticAttributeKeys;
        }

        /// <summary>
        /// gs the block type attributes show edit.
        /// </summary>
        /// <param name="attributeGuid">The attribute unique identifier.</param>
        protected void gBlockTypeAttributes_ShowEdit( Guid attributeGuid )
        {
            pnlDetails.Visible = false;
            vsDetails.Enabled = false;
            pnlBlockTypeAttributesEdit.Visible = true;

            Attribute attribute;
            if ( attributeGuid.Equals( Guid.Empty ) )
            {
                attribute = new Attribute();
                attribute.FieldTypeId = FieldTypeCache.Read( Rock.SystemGuid.FieldType.TEXT ).Id;
                edtBlockTypeAttributes.ActionTitle = ActionTitle.Add( "attribute for block type " + tbName.Text );
            }
            else
            {
                AttributeService attributeService = new AttributeService( new RockContext() );
                attribute = attributeService.Get( attributeGuid );
                edtBlockTypeAttributes.ActionTitle = ActionTitle.Edit( "attribute for block type " + tbName.Text );
            }

            edtBlockTypeAttributes.ReservedKeyNames = new AttributeService( new RockContext() )
                .GetByEntityTypeId( new Rock.Model.Block().TypeId ).AsQueryable()
                .Where( a =>
                    a.EntityTypeQualifierColumn.Equals( "BlockTypeId", StringComparison.OrdinalIgnoreCase ) &&
                    a.EntityTypeQualifierValue.Equals( hfBlockTypeId.Value ) &&
                    !a.Guid.Equals( attributeGuid ) )
                .Select( a => a.Key )
                .Distinct()
                .ToList();

            edtBlockTypeAttributes.SetAttributeProperties( attribute, typeof( BlockType ) );

            this.HideSecondaryBlocks( true );
        }

        /// <summary>
        /// Handles the GridReorder event of the gBlockTypeAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        void gBlockTypeAttributes_GridReorder( object sender, GridReorderEventArgs e )
        {
            var blockTypeStaticAttributeKeys = GetBlockTypeStaticAttributeKeys();
            string qualifierValue = hfBlockTypeId.Value;

            var rockContext = new RockContext();
            var attributeService = new AttributeService( rockContext );

            int order = 0;
            var attributes = attributeService
                .GetByEntityTypeId( new Rock.Model.Block().TypeId ).AsQueryable()
                .Where( a =>
                    a.EntityTypeQualifierColumn.Equals( "BlockTypeId", StringComparison.OrdinalIgnoreCase ) &&
                    a.EntityTypeQualifierValue.Equals( qualifierValue ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList();

            foreach ( var attribute in attributes )
            {
                attribute.Order = order++;
                AttributeCache.Flush( attribute.Id );
            }

            var movedItem = attributes.Where( a => a.Order == e.OldIndex ).FirstOrDefault();
            if ( movedItem != null )
            {
                if ( e.NewIndex < e.OldIndex )
                {
                    // Moved up
                    foreach ( var otherItem in attributes.Where( a => a.Order < e.OldIndex && a.Order >= e.NewIndex ) )
                    {
                        otherItem.Order = otherItem.Order + 1;
                    }
                }
                else
                {
                    // Moved Down
                    foreach ( var otherItem in attributes.Where( a => a.Order > e.OldIndex && a.Order <= e.NewIndex ) )
                    {
                        otherItem.Order = otherItem.Order - 1;
                    }
                }

                movedItem.Order = e.NewIndex;
                rockContext.SaveChanges();
            }

            BindBlockTypeAttributesGrid();
        }

        /// <summary>
        /// Handles the DeleteClick event of the gBlockTypeAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gBlockTypeAttributes_DeleteClick( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            Guid attributeGuid = (Guid)e.RowKeyValue;
            var rockContext = new RockContext();
            AttributeService attributeService = new AttributeService( rockContext );
            Attribute attribute = attributeService.Get( attributeGuid );

            if ( attribute != null )
            {
                string errorMessage;
                if ( !attributeService.CanDelete( attribute, out errorMessage ) )
                {
                    mdGridWarningAttributes.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                AttributeCache.Flush( attribute.Id );
                attributeService.Delete( attribute );
                rockContext.SaveChanges();
            }

            FlushBlockTypeAttributes();

            BindBlockTypeAttributesGrid();
        }

        /// <summary>
        /// Flushes the block type attributes.
        /// </summary>
        private void FlushBlockTypeAttributes()
        {
            // Flush BlockType, Block and Entity Attributes
            AttributeCache.FlushEntityAttributes();

            BlockTypeCache.Flush( hfBlockTypeId.Value.AsInteger() );
            var blockTypeCache = BlockTypeCache.Read( hfBlockTypeId.Value.AsInteger() );
            foreach ( var blockId in new BlockService( new RockContext() ).GetByBlockTypeId( hfBlockTypeId.Value.AsInteger() ).Select( a => a.Id ).ToList() )
            {
                BlockCache.Flush( blockId );
            }
        }

        /// <summary>
        /// Handles the SaveClick event of the edtBlockTypeAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void edtBlockTypeAttributes_SaveClick( object sender, EventArgs e )
        {
            var attribute = Rock.Attribute.Helper.SaveAttributeEdits(
                edtBlockTypeAttributes, EntityTypeCache.Read( typeof( Rock.Model.Block ) ).Id, "BlockTypeId", hfBlockTypeId.Value );

            // Attribute will be null if it was not valid
            if ( attribute == null )
            {
                return;
            }

            pnlDetails.Visible = true;
            pnlBlockTypeAttributesEdit.Visible = false;

            FlushBlockTypeAttributes();

            BindBlockTypeAttributesGrid();
            this.HideSecondaryBlocks( false );
        }

        /// <summary>
        /// Handles the CancelClick event of the edtBlockTypeAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void edtBlockTypeAttributes_CancelClick( object sender, EventArgs e )
        {
            pnlDetails.Visible = true;
            pnlBlockTypeAttributesEdit.Visible = false;

            this.HideSecondaryBlocks( false );
        }

        /// <summary>
        /// Handles the RowDataBound event of the gBlockTypeAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gBlockTypeAttributes_RowDataBound( object sender, System.Web.UI.WebControls.GridViewRowEventArgs e )
        {
            if ( e.Row.DataItem != null )
            {
                bool isDynamicAttribute = (bool)e.Row.DataItem.GetPropertyValue( "IsDynamicAttribute" );
                if ( !isDynamicAttribute )
                {
                    // don't allow static attributes on to be edited or deleted (but reordering them is OK)
                    e.Row.Cells.OfType<DataControlFieldCell>().First( a => a.ContainingField is EditField ).Controls[0].Visible = false;
                    e.Row.Cells.OfType<DataControlFieldCell>().First( a => a.ContainingField is DeleteField ).Controls[0].Visible = false;
                }
            }
        }



        #endregion
    }
}