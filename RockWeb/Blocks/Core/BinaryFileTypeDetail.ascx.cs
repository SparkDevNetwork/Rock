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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;
using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Attribute = Rock.Model.Attribute;

namespace RockWeb.Blocks.Core
{
    [DisplayName( "Binary File Type Detail" )]
    [Category( "Core" )]
    [Description( "Displays all details of a binary file type." )]
    [Rock.SystemGuid.BlockTypeGuid( "02D0A037-446B-403B-9719-5EF7D98239EF" )]
    public partial class BinaryFileTypeDetail : RockBlock
    {
        #region Fields

        private List<Attribute> BinaryFileAttributesState { get; set; }

        #endregion

        #region Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            string json = ViewState["BinaryFileAttributesState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                BinaryFileAttributesState = new List<Attribute>();
            }
            else
            {
                BinaryFileAttributesState = JsonConvert.DeserializeObject<List<Attribute>>( json );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gBinaryFileAttributes.DataKeyNames = new[] { "Guid" };
            gBinaryFileAttributes.Actions.ShowAdd = true;
            gBinaryFileAttributes.Actions.AddClick += gBinaryFileAttributes_Add;
            gBinaryFileAttributes.GridRebind += gBinaryFileAttributes_GridRebind;
            gBinaryFileAttributes.EmptyDataText = Server.HtmlEncode( None.Text );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( "BinaryFileTypeId" ).AsInteger() );
            }
            else
            {
                if ( pnlDetails.Visible )
                {
                    var storageEntityType = EntityTypeCache.Get( cpStorageType.SelectedValue.AsGuid() );
                    if ( storageEntityType != null )
                    {
                        var binaryFileType = new BinaryFileType { StorageEntityTypeId = storageEntityType.Id };
                        binaryFileType.LoadAttributes();
                        phAttributes.Controls.Clear();
                        Rock.Attribute.Helper.AddEditControls( binaryFileType, phAttributes, false, BlockValidationGroup );
                    }
                }
            }

            base.OnLoad( e );
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
            };

            ViewState["BinaryFileAttributesState"] = JsonConvert.SerializeObject( BinaryFileAttributesState, Formatting.None, jsonSetting );

            return base.SaveViewState();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="binaryFileTypeId">The binary file type identifier.</param>
        public void ShowDetail( int binaryFileTypeId )
        {
            pnlDetails.Visible = true;
            BinaryFileType binaryFileType = null;

            var rockContext = new RockContext();

            if ( !binaryFileTypeId.Equals( 0 ) )
            {
                binaryFileType = new BinaryFileTypeService( rockContext ).Get( binaryFileTypeId );
                lActionTitle.Text = ActionTitle.Edit( BinaryFileType.FriendlyTypeName ).FormatAsHtmlTitle();
                pdAuditDetails.SetEntity( binaryFileType, ResolveRockUrl( "~" ) );
            }

            if ( binaryFileType == null )
            {
                binaryFileType = new BinaryFileType { Id = 0 };
                lActionTitle.Text = ActionTitle.Add( BinaryFileType.FriendlyTypeName ).FormatAsHtmlTitle();

                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
            }

            BinaryFileAttributesState = new List<Attribute>();

            hfBinaryFileTypeId.Value = binaryFileType.Id.ToString();
            tbName.Text = binaryFileType.Name;
            tbDescription.Text = binaryFileType.Description;
            tbIconCssClass.Text = binaryFileType.IconCssClass;
            nbMaxFileSizeBytes.Text = binaryFileType.MaxFileSizeBytes.ToString();
            cbCacheToServerFileSystem.Checked = binaryFileType.CacheToServerFileSystem;

            if ( binaryFileType.CacheControlHeaderSettings != null )
            {
                cpCacheSettings.CurrentCacheability = JsonConvert.DeserializeObject<RockCacheability>( binaryFileType.CacheControlHeaderSettings );
            }

            cbRequiresViewSecurity.Checked = binaryFileType.RequiresViewSecurity;

            nbMaxWidth.Text = binaryFileType.MaxWidth.HasValue ? binaryFileType.MaxWidth.Value.ToString() : string.Empty;
            nbMaxHeight.Text = binaryFileType.MaxHeight.HasValue ? binaryFileType.MaxHeight.Value.ToString() : string.Empty;

            cbPreferredRequired.Checked = binaryFileType.PreferredRequired;

            if ( binaryFileType.StorageEntityType != null )
            {
                cpStorageType.SelectedValue = binaryFileType.StorageEntityType.Guid.ToString().ToUpper();
            }

            AttributeService attributeService = new AttributeService( rockContext );

            string qualifierValue = binaryFileType.Id.ToString();
            var qryBinaryFileAttributes = attributeService.GetByEntityTypeId( new BinaryFile().TypeId, true ).AsQueryable()
                .Where( a => a.EntityTypeQualifierColumn.Equals( "BinaryFileTypeId", StringComparison.OrdinalIgnoreCase )
                && a.EntityTypeQualifierValue.Equals( qualifierValue ) );

            BinaryFileAttributesState = qryBinaryFileAttributes.ToList();

            BindBinaryFileAttributesGrid();

            // render UI based on Authorized and IsSystem
            bool readOnly = false;
            bool restrictedEdit = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( BinaryFileType.FriendlyTypeName );
            }

            if ( binaryFileType.IsSystem )
            {
                restrictedEdit = true;
                nbEditModeMessage.Text = EditModeMessage.System( BinaryFileType.FriendlyTypeName );
            }

            phAttributes.Controls.Clear();
            binaryFileType.LoadAttributes();

            if ( readOnly )
            {
                lActionTitle.Text = ActionTitle.View( BinaryFileType.FriendlyTypeName ).FormatAsHtmlTitle();
                btnCancel.Text = "Close";
            }

            if ( readOnly )
            {
                Rock.Attribute.Helper.AddDisplayControls( binaryFileType, phAttributes );
            }
            else
            {
                Rock.Attribute.Helper.AddEditControls( binaryFileType, phAttributes, true, BlockValidationGroup );
            }

            // the only thing we'll restrict for restrictedEdit is the Name (plus they won't be able to remove Attributes that are marked as IsSystem
            tbName.ReadOnly = readOnly || restrictedEdit;

            gBinaryFileAttributes.Enabled = !readOnly;
            gBinaryFileAttributes.Columns.OfType<EditField>().First().Visible = !readOnly;
            gBinaryFileAttributes.Actions.ShowAdd = !readOnly;

            // allow these to be edited in restricted edit mode if not readonly
            tbDescription.ReadOnly = readOnly;
            tbIconCssClass.ReadOnly = readOnly;
            nbMaxFileSizeBytes.ReadOnly = readOnly;
            cbCacheToServerFileSystem.Enabled = !readOnly;
            cpCacheSettings.Enabled = !readOnly;
            cbRequiresViewSecurity.Enabled = !readOnly;
            cpStorageType.Enabled = !readOnly;
            nbMaxWidth.ReadOnly = readOnly;
            nbMaxHeight.ReadOnly = readOnly;
            cbPreferredRequired.Enabled = !readOnly;
            btnSave.Visible = !readOnly;
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
            BinaryFileType binaryFileType;

            var rockContext = new RockContext();
            BinaryFileTypeService binaryFileTypeService = new BinaryFileTypeService( rockContext );
            AttributeService attributeService = new AttributeService( rockContext );
            AttributeQualifierService attributeQualifierService = new AttributeQualifierService( rockContext );
            CategoryService categoryService = new CategoryService( rockContext );

            int binaryFileTypeId = int.Parse( hfBinaryFileTypeId.Value );

            if ( binaryFileTypeId == 0 )
            {
                binaryFileType = new BinaryFileType();
                binaryFileTypeService.Add( binaryFileType );
            }
            else
            {
                binaryFileType = binaryFileTypeService.Get( binaryFileTypeId );
            }

            binaryFileType.Name = tbName.Text;
            binaryFileType.Description = tbDescription.Text;
            binaryFileType.IconCssClass = tbIconCssClass.Text;
            binaryFileType.MaxFileSizeBytes = nbMaxFileSizeBytes.Text.AsIntegerOrNull();
            binaryFileType.CacheToServerFileSystem = cbCacheToServerFileSystem.Checked;
            binaryFileType.CacheControlHeaderSettings = cpCacheSettings.CurrentCacheability.ToJson();
            binaryFileType.RequiresViewSecurity = cbRequiresViewSecurity.Checked;
            binaryFileType.MaxWidth = nbMaxWidth.Text.AsIntegerOrNull();
            binaryFileType.MaxHeight = nbMaxHeight.Text.AsIntegerOrNull();
            binaryFileType.PreferredRequired = cbPreferredRequired.Checked;

            if ( !string.IsNullOrWhiteSpace( cpStorageType.SelectedValue ) )
            {
                var entityTypeService = new EntityTypeService( rockContext );
                var storageEntityType = entityTypeService.Get( new Guid( cpStorageType.SelectedValue ) );

                if ( storageEntityType != null )
                {
                    binaryFileType.StorageEntityTypeId = storageEntityType.Id;
                }
            }

            binaryFileType.LoadAttributes( rockContext );
            Rock.Attribute.Helper.GetEditValues( phAttributes, binaryFileType );

            if ( !binaryFileType.IsValid )
            {
                // Controls will render the error messages                    
                return;
            }

            rockContext.WrapTransaction( () =>
            {
                rockContext.SaveChanges();

                // get it back to make sure we have a good Id for it for the Attributes
                binaryFileType = binaryFileTypeService.Get( binaryFileType.Guid );

                /* Take care of Binary File Attributes */
                var entityTypeId = EntityTypeCache.Get( typeof( BinaryFile ) ).Id;

                // delete BinaryFileAttributes that are no longer configured in the UI
                var attributes = attributeService.GetByEntityTypeQualifier( entityTypeId, "BinaryFileTypeId", binaryFileType.Id.ToString(), true );
                var selectedAttributeGuids = BinaryFileAttributesState.Select( a => a.Guid );
                foreach ( var attr in attributes.Where( a => !selectedAttributeGuids.Contains( a.Guid ) ) )
                {
                    attributeService.Delete( attr );
                }

                rockContext.SaveChanges();

                // add/update the BinaryFileAttributes that are assigned in the UI
                foreach ( var attributeState in BinaryFileAttributesState )
                {
                    Rock.Attribute.Helper.SaveAttributeEdits( attributeState, entityTypeId, "BinaryFileTypeId", binaryFileType.Id.ToString(), rockContext );
                }

                // SaveAttributeValues for the BinaryFileType
                binaryFileType.SaveAttributeValues( rockContext );
            } );

            NavigateToParentPage();
        }

        #endregion

        #region BinaryFileAttributes Grid and Picker

        /// <summary>
        /// Handles the Add event of the gBinaryFileAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gBinaryFileAttributes_Add( object sender, EventArgs e )
        {
            gBinaryFileAttributes_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the Edit event of the gBinaryFileAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gBinaryFileAttributes_Edit( object sender, RowEventArgs e )
        {
            Guid attributeGuid = ( Guid ) e.RowKeyValue;
            gBinaryFileAttributes_ShowEdit( attributeGuid );
        }

        /// <summary>
        /// Gs the binary file attributes_ show edit.
        /// </summary>
        /// <param name="attributeGuid">The attribute GUID.</param>
        protected void gBinaryFileAttributes_ShowEdit( Guid attributeGuid )
        {
            pnlDetails.Visible = false;
            pnlBinaryFileAttribute.Visible = true;

            Attribute attribute;
            if ( attributeGuid.Equals( Guid.Empty ) )
            {
                attribute = new Attribute();
                attribute.FieldTypeId = FieldTypeCache.Get( Rock.SystemGuid.FieldType.TEXT ).Id;
                edtBinaryFileAttributes.ActionTitle = ActionTitle.Add( "attribute for binary files of type " + tbName.Text );
            }
            else
            {
                attribute = BinaryFileAttributesState.First( a => a.Guid.Equals( attributeGuid ) );
                edtBinaryFileAttributes.ActionTitle = ActionTitle.Edit( "attribute for binary files of type " + tbName.Text );
            }

            edtBinaryFileAttributes.ReservedKeyNames = BinaryFileAttributesState.Where( a => !a.Guid.Equals( attributeGuid ) ).Select( a => a.Key ).ToList();

            edtBinaryFileAttributes.SetAttributeProperties( attribute, typeof( BinaryFile ) );
        }

        /// <summary>
        /// Handles the Delete event of the gBinaryFileAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gBinaryFileAttributes_Delete( object sender, RowEventArgs e )
        {
            Guid attributeGuid = ( Guid ) e.RowKeyValue;
            BinaryFileAttributesState.RemoveEntity( attributeGuid );

            BindBinaryFileAttributesGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gBinaryFileAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gBinaryFileAttributes_GridRebind( object sender, EventArgs e )
        {
            BindBinaryFileAttributesGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnSaveBinaryFileAttribute control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSaveBinaryFileAttribute_Click( object sender, EventArgs e )
        {
            var attribute = edtBinaryFileAttributes.SaveChangesToStateCollection( BinaryFileAttributesState );

            // Controls will show warnings
            if ( !attribute.IsValid )
            {
                return;
            }

            pnlDetails.Visible = true;
            pnlBinaryFileAttribute.Visible = false;

            BindBinaryFileAttributesGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnCancelBinaryFileAttribute control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancelBinaryFileAttribute_Click( object sender, EventArgs e )
        {
            pnlDetails.Visible = true;
            pnlBinaryFileAttribute.Visible = false;
        }

        /// <summary>
        /// Binds the binary file type attributes grid.
        /// </summary>
        private void BindBinaryFileAttributesGrid()
        {
            gBinaryFileAttributes.DataSource = BinaryFileAttributesState.OrderBy( a => a.Name ).ToList();
            gBinaryFileAttributes.DataBind();
        }

        #endregion
    }
}