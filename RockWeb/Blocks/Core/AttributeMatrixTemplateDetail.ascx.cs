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
using System.Web.UI;
using Newtonsoft.Json;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Field.Types;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Attribute = Rock.Model.Attribute;

namespace RockWeb.Blocks.Core
{
    [DisplayName( "Attribute Matrix Template Detail" )]
    [Category( "Core" )]
    [Description( "Displays the details of an attribute matrix template." )]
    public partial class AttributeMatrixTemplateDetail : RockBlock, IDetailBlock
    {
        #region States

        private List<Attribute> AttributesState { get; set; }

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
            this.AddConfigurationUpdateTrigger( upnlContent );

            gAttributes.DataKeyNames = new string[] { "Guid" };
            gAttributes.Actions.ShowAdd = true;
            gAttributes.Actions.AddClick += gAttributes_Add;
            gAttributes.GridRebind += gAttributes_GridRebind;
            gAttributes.GridReorder += gAttributes_GridReorder;

            SecurityField securityField = gAttributes.Columns.OfType<SecurityField>().FirstOrDefault();
            securityField.EntityTypeId = EntityTypeCache.GetId<Attribute>() ?? 0;
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
                ShowDetail( PageParameter( "AttributeMatrixTemplateId" ).AsInteger() );
            }
        }

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            var json = ViewState["AttributesState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                AttributesState = new List<Attribute>();
            }
            else
            {
                AttributesState = JsonConvert.DeserializeObject<List<Attribute>>( json );
            }
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

            ViewState["AttributesState"] = JsonConvert.SerializeObject( AttributesState, Formatting.None, jsonSetting );

            return base.SaveViewState();
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
            ShowDetail( hfAttributeMatrixTemplateId.Value.AsInteger() );
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            AttributeMatrixTemplate attributeMatrixTemplate;
            var rockContext = new RockContext();
            var attributeMatrixTemplateService = new AttributeMatrixTemplateService( rockContext );

            int attributeMatrixTemplateId = int.Parse( hfAttributeMatrixTemplateId.Value );

            if ( attributeMatrixTemplateId == 0 )
            {
                attributeMatrixTemplate = new AttributeMatrixTemplate();
                attributeMatrixTemplateService.Add( attributeMatrixTemplate );
            }
            else
            {
                attributeMatrixTemplate = attributeMatrixTemplateService.Get( attributeMatrixTemplateId );
            }

            attributeMatrixTemplate.Name = tbName.Text;
            attributeMatrixTemplate.IsActive = cbIsActive.Checked;
            attributeMatrixTemplate.Description = tbDescription.Text;
            attributeMatrixTemplate.MinimumRows = tbMinimumRows.Text.AsIntegerOrNull();
            attributeMatrixTemplate.MaximumRows = tbMaximumRows.Text.AsIntegerOrNull();
            attributeMatrixTemplate.FormattedLava = ceFormattedLava.Text;

            // need WrapTransaction due to Attribute saves    
            rockContext.WrapTransaction( () =>
            {
                rockContext.SaveChanges();

                /* Save Attributes */

                var entityTypeIdAttributeMatrix = EntityTypeCache.GetId<AttributeMatrixItem>();

                // Get the existing attributes for this entity type and qualifier value
                var attributeService = new AttributeService( rockContext );
                var regFieldService = new RegistrationTemplateFormFieldService( rockContext );
                var attributes = attributeService.Get( entityTypeIdAttributeMatrix, "AttributeMatrixTemplateId", attributeMatrixTemplate.Id.ToString() );

                // Delete any of those attributes that were removed in the UI
                var selectedAttributeGuids = AttributesState.Select( a => a.Guid );
                foreach ( var attr in attributes.Where( a => !selectedAttributeGuids.Contains( a.Guid ) ) )
                {
                    foreach ( var field in regFieldService.Queryable().Where( f => f.AttributeId.HasValue && f.AttributeId.Value == attr.Id ).ToList() )
                    {
                        regFieldService.Delete( field );
                    }

                    attributeService.Delete( attr );
                    rockContext.SaveChanges();
                    Rock.Web.Cache.AttributeCache.Flush( attr.Id );
                }

                // Update the Attributes that were assigned in the UI
                foreach ( var attributeState in AttributesState )
                {
                    Helper.SaveAttributeEdits( attributeState, entityTypeIdAttributeMatrix, "AttributeMatrixTemplateId", attributeMatrixTemplate.Id.ToString(), rockContext );
                }

                Rock.Web.Cache.AttributeCache.FlushEntityAttributes();
            } );

            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="attributeMatrixTemplateId">The attribute matrix template identifier.</param>
        public void ShowDetail( int attributeMatrixTemplateId )
        {
            pnlView.Visible = true;

            AttributeMatrixTemplate attributeMatrixTemplate = null;
            var rockContext = new RockContext();

            if ( !attributeMatrixTemplateId.Equals( 0 ) )
            {
                attributeMatrixTemplate = new AttributeMatrixTemplateService( rockContext ).Get( attributeMatrixTemplateId );
                lActionTitle.Text = ActionTitle.Edit( AttributeMatrixTemplate.FriendlyTypeName ).FormatAsHtmlTitle();
                pdAuditDetails.SetEntity( attributeMatrixTemplate, ResolveRockUrl( "~" ) );
            }

            if ( attributeMatrixTemplate == null )
            {
                attributeMatrixTemplate = new AttributeMatrixTemplate { Id = 0 };
                attributeMatrixTemplate.FormattedLava = AttributeMatrixTemplate.FormattedLavaDefault;

                lActionTitle.Text = ActionTitle.Add( AttributeMatrixTemplate.FriendlyTypeName ).FormatAsHtmlTitle();

                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
            }

            hfAttributeMatrixTemplateId.Value = attributeMatrixTemplate.Id.ToString();
            tbName.Text = attributeMatrixTemplate.Name;
            cbIsActive.Checked = attributeMatrixTemplate.IsActive;
            tbDescription.Text = attributeMatrixTemplate.Description;
            tbMinimumRows.Text = attributeMatrixTemplate.MinimumRows.ToString();
            tbMaximumRows.Text = attributeMatrixTemplate.MaximumRows.ToString();
            ceFormattedLava.Text = attributeMatrixTemplate.FormattedLava;

            var attributeService = new AttributeService( rockContext );
            AttributesState = attributeService.Get( new AttributeMatrixItem().TypeId, "AttributeMatrixTemplateId", attributeMatrixTemplate.Id.ToString() ).AsQueryable()
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList();

            BindAttributesGrid();

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( AttributeMatrixTemplate.FriendlyTypeName );
            }

            if ( readOnly )
            {
                lActionTitle.Text = ActionTitle.View( AttributeMatrixTemplate.FriendlyTypeName );
                btnCancel.Text = "Close";
            }

            tbName.ReadOnly = readOnly;
            cbIsActive.Enabled = !readOnly;
            tbDescription.ReadOnly = readOnly;
            tbMinimumRows.ReadOnly = readOnly;
            tbMaximumRows.ReadOnly = readOnly;
            ceFormattedLava.ReadOnly = readOnly;
            btnSave.Visible = !readOnly;
        }

        #endregion

        #region Attributes Grid and Picker

        /// <summary>
        /// Handles the Add event of the gAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gAttributes_Add( object sender, EventArgs e )
        {
            gAttributes_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the Edit event of the gAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gAttributes_Edit( object sender, RowEventArgs e )
        {
            Guid attributeGuid = (Guid)e.RowKeyValue;
            gAttributes_ShowEdit( attributeGuid );
        }

        /// <summary>
        /// Gs the group type attributes_ show edit.
        /// </summary>
        /// <param name="attributeGuid">The attribute GUID.</param>
        protected void gAttributes_ShowEdit( Guid attributeGuid )
        {
            Attribute attribute;
            if ( attributeGuid.Equals( Guid.Empty ) )
            {
                attribute = new Attribute();
                attribute.FieldTypeId = FieldTypeCache.Read( Rock.SystemGuid.FieldType.TEXT ).Id;
                edtAttributes.ActionTitle = ActionTitle.Add( "attribute for matrix attributes that use matrix template " + tbName.Text );
            }
            else
            {
                attribute = AttributesState.First( a => a.Guid.Equals( attributeGuid ) );
                edtAttributes.ActionTitle = ActionTitle.Edit( "attribute for matrix attributes that use matrix template " + tbName.Text );
            }

            var reservedKeyNames = new List<string>();
            AttributesState.Where( a => !a.Guid.Equals( attributeGuid ) ).Select( a => a.Key ).ToList().ForEach( a => reservedKeyNames.Add( a ) );
            edtAttributes.ReservedKeyNames = reservedKeyNames.ToList();

            edtAttributes.ExcludedFieldTypes = new FieldTypeCache[] { FieldTypeCache.Read<MatrixFieldType>() };
            edtAttributes.SetAttributeProperties( attribute, typeof( AttributeMatrixTemplate ) );

            dlgAttribute.Show();
        }

        /// <summary>
        /// Handles the GridReorder event of the gAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gAttributes_GridReorder( object sender, GridReorderEventArgs e )
        {
            ReorderAttributeList( AttributesState, e.OldIndex, e.NewIndex );
            BindAttributesGrid();
        }

        /// <summary>
        /// Reorders the attribute list.
        /// </summary>
        /// <param name="itemList">The item list.</param>
        /// <param name="oldIndex">The old index.</param>
        /// <param name="newIndex">The new index.</param>
        private void ReorderAttributeList( List<Attribute> itemList, int oldIndex, int newIndex )
        {
            var movedItem = itemList.Where( a => a.Order == oldIndex ).FirstOrDefault();
            if ( movedItem != null )
            {
                if ( newIndex < oldIndex )
                {
                    // Moved up
                    foreach ( var otherItem in itemList.Where( a => a.Order < oldIndex && a.Order >= newIndex ) )
                    {
                        otherItem.Order = otherItem.Order + 1;
                    }
                }
                else
                {
                    // Moved Down
                    foreach ( var otherItem in itemList.Where( a => a.Order > oldIndex && a.Order <= newIndex ) )
                    {
                        otherItem.Order = otherItem.Order - 1;
                    }
                }

                movedItem.Order = newIndex;
            }
        }

        /// <summary>
        /// Handles the Delete event of the gAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gAttributes_Delete( object sender, RowEventArgs e )
        {
            Guid attributeGuid = (Guid)e.RowKeyValue;
            AttributesState.RemoveEntity( attributeGuid );

            BindAttributesGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gAttributes_GridRebind( object sender, EventArgs e )
        {
            BindAttributesGrid();
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgAttribute control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgAttribute_SaveClick( object sender, EventArgs e )
        {
            Rock.Model.Attribute attribute = new Rock.Model.Attribute();
            edtAttributes.GetAttributeProperties( attribute );

            // Controls will show warnings
            if ( !attribute.IsValid )
            {
                return;
            }

            if ( AttributesState.Any( a => a.Guid.Equals( attribute.Guid ) ) )
            {
                attribute.Order = AttributesState.Where( a => a.Guid.Equals( attribute.Guid ) ).FirstOrDefault().Order;
                AttributesState.RemoveEntity( attribute.Guid );
            }
            else
            {
                attribute.Order = AttributesState.Any() ? AttributesState.Max( a => a.Order ) + 1 : 0;
            }

            AttributesState.Add( attribute );

            BindAttributesGrid();
            dlgAttribute.Hide();
        }

        /// <summary>
        /// Sets the attribute list order.
        /// </summary>
        /// <param name="itemList">The item list.</param>
        private void SetAttributeListOrder( List<Attribute> itemList )
        {
            int order = 0;
            itemList.OrderBy( a => a.Order ).ToList().ForEach( a => a.Order = order++ );
        }

        /// <summary>
        /// Binds the group type attributes grid.
        /// </summary>
        private void BindAttributesGrid()
        {
            nbAttributeCountWarning.Visible = hfAttributeMatrixTemplateId.Value.AsInteger() != 0 && !AttributesState.Any();

            gAttributes.AddCssClass( "attribute-grid" );
            SetAttributeListOrder( AttributesState );
            gAttributes.DataSource = AttributesState.OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList();
            gAttributes.DataBind();
        }

        #endregion
    }
}