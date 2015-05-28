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
using System.Collections.Generic;
using System.Linq;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Attribute = Rock.Model.Attribute;
using Rock.Security;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// User controls for managing defined types
    /// </summary>
    [DisplayName( "Defined Type Detail" )]
    [Category( "Core" )]
    [Description( "Displays the details of the given defined type." )]

    [DefinedTypeField( "Defined Type", "If a Defined Type is set, only details for it will be displayed (regardless of the querystring parameters).", required: false, defaultValue: "" )]
    public partial class DefinedTypeDetail : RockBlock, IDetailBlock
    {

        #region Fields

        // If block is being used on a stand alone page ( i.e. not navigated to through defined type list )
        bool _isStandAlone = false;

        #endregion

        #region Properties

        // used for public / protected properties

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlSettings );

            // assign attributes grid actions
            gDefinedTypeAttributes.DataKeyNames = new string[] { "Guid" };
            gDefinedTypeAttributes.Actions.ShowAdd = true;
            gDefinedTypeAttributes.Actions.AddClick += gDefinedTypeAttributes_Add;
            gDefinedTypeAttributes.GridRebind += gDefinedTypeAttributes_GridRebind;
            gDefinedTypeAttributes.GridReorder += gDefinedTypeAttributes_GridReorder;

            btnDelete.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}');", DefinedType.FriendlyTypeName );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                int? itemId = InitItemId();

                if ( itemId.HasValue )
                {
                    ShowDetail( itemId.Value );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
        }

        /// <summary>
        /// Determines which item to display based on either the configuration or the definedTypeId that was passed in.
        /// </summary>
        /// <returns>An <see cref="System.Int32"/> of the Id for a <see cref="Rock.Model.DefinedType"/> or null if it was not found.</returns>
        private int? InitItemId()
        {
            Guid? definedTypeGuid = GetAttributeValue( "DefinedType" ).AsGuidOrNull();
            int? itemId = null;

            // A configured defined type takes precedence over any definedTypeId param value that is passed in.
            if ( definedTypeGuid.HasValue )
            {
                _isStandAlone = true;
                // hide reorder, edit and delete

                gDefinedTypeAttributes.Columns[0].Visible = false;
                gDefinedTypeAttributes.Columns[2].Visible = false;
                gDefinedTypeAttributes.Columns[3].Visible = false;
                gDefinedTypeAttributes.Actions.ShowAdd = false;

                itemId = DefinedTypeCache.Read( definedTypeGuid.Value ).Id;
                var definedType = DefinedTypeCache.Read( definedTypeGuid.Value );
                if ( definedType != null )
                {
                    itemId = definedType.Id;
                }
            }
            else
            {
                gDefinedTypeAttributes.Columns[0].Visible = true;
                gDefinedTypeAttributes.Columns[2].Visible = true;
                gDefinedTypeAttributes.Columns[3].Visible = true;
                gDefinedTypeAttributes.Actions.ShowAdd = true;

                itemId = PageParameter( "definedTypeId" ).AsIntegerOrNull();
            }

            return itemId;
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            int? itemId = InitItemId();

            if ( itemId.HasValue )
            {
                ShowDetail( itemId.Value );
            }
            else
            {
                pnlDetails.Visible = false;
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSaveType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSaveType_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();

            DefinedType definedType = null;
            DefinedTypeService typeService = new DefinedTypeService( rockContext );

            int definedTypeId = hfDefinedTypeId.ValueAsInt();

            if ( definedTypeId == 0 )
            {
                definedType = new DefinedType();
                definedType.IsSystem = false;
                definedType.Order = 0;
                typeService.Add( definedType );
            }
            else
            {
                DefinedTypeCache.Flush( definedTypeId );
                definedType = typeService.Get( definedTypeId );
            }

            definedType.FieldTypeId = FieldTypeCache.Read( Rock.SystemGuid.FieldType.TEXT ).Id;
            definedType.Name = tbTypeName.Text;
            definedType.CategoryId = cpCategory.SelectedValueAsInt();
            definedType.Description = tbTypeDescription.Text;
            definedType.HelpText = tbHelpText.Text;

            if ( !definedType.IsValid )
            {
                // Controls will render the error messages                    
                return;
            }

            rockContext.SaveChanges();

            var qryParams = new Dictionary<string, string>();
            qryParams["definedTypeId"] = definedType.Id.ToString();
            NavigateToPage( RockPage.Guid, qryParams );
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            RockContext rockContext = new RockContext();
            DefinedTypeService definedTypeService = new DefinedTypeService( rockContext );
            DefinedType definedType = definedTypeService.Get( int.Parse( hfDefinedTypeId.Value ) );

            if ( definedType != null )
            {
                if ( !definedType.IsAuthorized( Authorization.EDIT, this.CurrentPerson ) )
                {
                    mdDeleteWarning.Show( "Sorry, You are not authorized to delete this Defined Type.", ModalAlertType.Information );
                    return;
                }

                string errorMessage;
                if ( !definedTypeService.CanDelete( definedType, out errorMessage ) )
                {
                    mdDeleteWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                definedTypeService.Delete( definedType );

                rockContext.SaveChanges();

            }

            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnCancelType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancelType_Click( object sender, EventArgs e )
        {
            if ( hfDefinedTypeId.IsZero() )
            {
                // Cancelling on Add.  Return to Grid
                NavigateToParentPage();
            }
            else
            {
                // Cancelling on Edit.  Return to Details
                DefinedTypeService definedTypeService = new DefinedTypeService(new RockContext());
                DefinedType definedType = definedTypeService.Get( hfDefinedTypeId.ValueAsInt() );
                ShowReadonlyDetails( definedType );
            }
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="definedType">Type of the defined.</param>
        private void ShowReadonlyDetails( DefinedType definedType )
        {
            SetEditMode( false );

            hfDefinedTypeId.SetValue( definedType.Id );
            tbTypeName.Text = definedType.Name;

            lTitle.Text = definedType.Name.FormatAsHtmlTitle();
            lDescription.Text = definedType.Description;

            if ( !string.IsNullOrWhiteSpace( definedType.HelpText ) )
            {
                lHelpText.Text = definedType.HelpText;
                rcHelpText.Visible = true;
            }
            else
            {
                rcHelpText.Visible = false;
            }

            definedType.LoadAttributes();

            if (!_isStandAlone && definedType.Category != null )
            {
                lblMainDetails.Text = new DescriptionList()
                    .Add( "Category", definedType.Category.Name )
                    .Html;
            }

        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="definedType">Type of the defined.</param>
        private void ShowEditDetails( DefinedType definedType )
        {
            if ( definedType.Id > 0 )
            {
                lTitle.Text = ActionTitle.Edit( DefinedType.FriendlyTypeName ).FormatAsHtmlTitle();
            }
            else
            {
                lTitle.Text = ActionTitle.Add( DefinedType.FriendlyTypeName ).FormatAsHtmlTitle();
            }

            SetEditMode( true );

            tbTypeName.Text = definedType.Name;
            cpCategory.SetValue( definedType.CategoryId );
            tbTypeDescription.Text = definedType.Description;
            tbHelpText.Text = definedType.HelpText;
        }

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            DefinedTypeService definedTypeService = new DefinedTypeService( new RockContext() );
            DefinedType definedType = definedTypeService.Get( hfDefinedTypeId.ValueAsInt() );
            ShowEditDetails( definedType );
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            vsDetails.Enabled = editable;
            fieldsetViewDetails.Visible = !editable;

            this.HideSecondaryBlocks( editable );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="definedTypeId">The defined type identifier.</param>
        public void ShowDetail( int definedTypeId )
        {
            pnlDetails.Visible = true;
            DefinedType definedType = null;

            if ( !definedTypeId.Equals( 0 ) )
            {
                definedType = new DefinedTypeService( new RockContext() ).Get( definedTypeId );
            }

            if ( definedType == null )
            {
                definedType = new DefinedType { Id = 0 };
            }

            hfDefinedTypeId.SetValue( definedType.Id );

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( _isStandAlone )
            {
                readOnly = true;
            }
            else
            {
                if ( !IsUserAuthorized( Authorization.EDIT ) )
                {
                    readOnly = true;
                    nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( DefinedType.FriendlyTypeName );
                }

                if ( definedType.IsSystem )
                {
                    readOnly = true;
                    nbEditModeMessage.Text = EditModeMessage.ReadOnlySystem( DefinedType.FriendlyTypeName );
                }
            }

            if ( readOnly )
            {
                btnEdit.Visible = false;
                btnDelete.Visible = false;
                ShowReadonlyDetails( definedType );
            }
            else
            {
                btnEdit.Visible = true;
                btnDelete.Visible = false;
                if ( definedType.Id > 0 )
                {
                    ShowReadonlyDetails( definedType );
                }
                else
                {
                    ShowEditDetails( definedType );
                }
            }

            BindDefinedTypeAttributesGrid();         
  
        }
                
        #endregion

        #region DefinedTypeAttributes Grid and Picker

        /// <summary>
        /// Handles the Add event of the gDefinedTypeAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gDefinedTypeAttributes_Add( object sender, EventArgs e )
        {
            gDefinedTypeAttributes_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the Edit event of the gDefinedTypeAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gDefinedTypeAttributes_Edit( object sender, RowEventArgs e )
        {
            Guid attributeGuid = (Guid)e.RowKeyValue;
            gDefinedTypeAttributes_ShowEdit( attributeGuid );
        }

        /// <summary>
        /// Gs the defined type attributes_ show edit.
        /// </summary>
        /// <param name="attributeGuid">The attribute GUID.</param>
        protected void gDefinedTypeAttributes_ShowEdit( Guid attributeGuid )
        {
            pnlDetails.Visible = false;
            vsDetails.Enabled = false;
            pnlDefinedTypeAttributes.Visible = true;

            Attribute attribute;
            if ( attributeGuid.Equals( Guid.Empty ) )
            {
                attribute = new Attribute();
                attribute.FieldTypeId = FieldTypeCache.Read( Rock.SystemGuid.FieldType.TEXT ).Id;
                edtDefinedTypeAttributes.ActionTitle = ActionTitle.Add( "attribute for defined type " + tbTypeName.Text );
            }
            else
            {
                AttributeService attributeService = new AttributeService( new RockContext() );
                attribute = attributeService.Get( attributeGuid );
                edtDefinedTypeAttributes.ActionTitle = ActionTitle.Edit( "attribute for defined type " + tbTypeName.Text );
            }

            edtDefinedTypeAttributes.ReservedKeyNames = new AttributeService( new RockContext() )
                .GetByEntityTypeId( new DefinedValue().TypeId ).AsQueryable()
                .Where( a =>
                    a.EntityTypeQualifierColumn.Equals( "DefinedTypeId", StringComparison.OrdinalIgnoreCase ) &&
                    a.EntityTypeQualifierValue.Equals( hfDefinedTypeId.Value ) &&
                    !a.Guid.Equals(attributeGuid) )
                .Select( a => a.Key )
                .Distinct()
                .ToList();

            edtDefinedTypeAttributes.SetAttributeProperties( attribute, typeof( DefinedValue ) );

            this.HideSecondaryBlocks( true );
        }

        /// <summary>
        /// Handles the GridReorder event of the gDefinedTypeAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        void gDefinedTypeAttributes_GridReorder( object sender, GridReorderEventArgs e )
        {
            string qualifierValue = hfDefinedTypeId.Value;

            var rockContext = new RockContext();
            var attributeService = new AttributeService( rockContext );

            int order = 0;
            var attributes = attributeService
                .GetByEntityTypeId( new DefinedValue().TypeId ).AsQueryable()
                .Where( a =>
                    a.EntityTypeQualifierColumn.Equals( "DefinedTypeId", StringComparison.OrdinalIgnoreCase ) &&
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

            BindDefinedTypeAttributesGrid();
        }

        /// <summary>
        /// Handles the Delete event of the gDefinedTypeAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gDefinedTypeAttributes_Delete( object sender, RowEventArgs e )
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

            AttributeCache.FlushEntityAttributes();

            BindDefinedTypeAttributesGrid();            
        }

        /// <summary>
        /// Handles the GridRebind event of the gDefinedTypeAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gDefinedTypeAttributes_GridRebind( object sender, EventArgs e )
        {
            BindDefinedTypeAttributesGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnSaveDefinedTypeAttribute control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSaveDefinedTypeAttribute_Click( object sender, EventArgs e )
        {
            var attribute = Rock.Attribute.Helper.SaveAttributeEdits( 
                edtDefinedTypeAttributes, EntityTypeCache.Read( typeof( DefinedValue ) ).Id, "DefinedTypeId", hfDefinedTypeId.Value );

            // Attribute will be null if it was not valid
            if (attribute == null)
            {
                return;
            }

            pnlDetails.Visible = true;
            pnlDefinedTypeAttributes.Visible = false;

            AttributeCache.FlushEntityAttributes();

            BindDefinedTypeAttributesGrid();

            this.HideSecondaryBlocks( false );
        }

        /// <summary>
        /// Handles the Click event of the btnCancelDefinedTypeAttribute control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancelDefinedTypeAttribute_Click( object sender, EventArgs e )
        {
            pnlDetails.Visible = true;
            pnlDefinedTypeAttributes.Visible = false;

            this.HideSecondaryBlocks( false );
        }

        /// <summary>
        /// Binds the defined type attributes grid.
        /// </summary>
        private void BindDefinedTypeAttributesGrid()
        {
            string qualifierValue = hfDefinedTypeId.Value;
            var attributes = new AttributeService( new RockContext() )
                .GetByEntityTypeId( new DefinedValue().TypeId ).AsQueryable()
                .Where( a =>
                    a.EntityTypeQualifierColumn.Equals( "DefinedTypeId", StringComparison.OrdinalIgnoreCase ) &&
                    a.EntityTypeQualifierValue.Equals( qualifierValue ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList();

            gDefinedTypeAttributes.DataSource = attributes;
            gDefinedTypeAttributes.DataBind();

            pnlAttributeTypes.Visible = !_isStandAlone || attributes.Count > 0;

        }

        #endregion       
    }
}