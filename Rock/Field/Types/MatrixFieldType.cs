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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    ///  Matrix Field Type
    ///  Value stored as AttributeMatrix.Guid
    /// </summary>
    public class MatrixFieldType : FieldType
    {
        #region Configuration

        /// <summary>
        /// The attribute matrix template Id
        /// </summary>
        private const string ATTRIBUTE_MATRIX_TEMPLATE = "attributematrixtemplate";

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            var configKeys = base.ConfigurationKeys();
            configKeys.Add( ATTRIBUTE_MATRIX_TEMPLATE );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            var controls = base.ConfigurationControls();

            // build a drop down list of attribute matrix templates
            var ddlMatrixTemplate = new RockDropDownList();
            controls.Add( ddlMatrixTemplate );
            ddlMatrixTemplate.Label = "Attribute Matrix Template";
            ddlMatrixTemplate.Help = "The Attribute Matrix Template that defines this matrix attribute";

            var list = new AttributeMatrixTemplateService( new RockContext() ).Queryable().OrderBy( a => a.Name ).Select( a => new
            {
                a.Id,
                a.Name
            } ).ToList();

            ddlMatrixTemplate.Items.Clear();
            ddlMatrixTemplate.Items.Add( new ListItem() );

            foreach ( var item in list )
            {
                ddlMatrixTemplate.Items.Add( new ListItem( item.Name, item.Id.ToString() ) );
            }

            return controls;
        }

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls )
        {
            Dictionary<string, ConfigurationValue> configurationValues = new Dictionary<string, ConfigurationValue>();
            configurationValues.Add( ATTRIBUTE_MATRIX_TEMPLATE, new ConfigurationValue( "Attribute Matrix Type", "The Attribute Matrix Template that defines this matrix attribute", string.Empty ) );

            if ( controls != null )
            {
                if ( controls.Count > 0 )
                {
                    var ddlMatrixTemplate = controls[0] as RockDropDownList;
                    configurationValues[ATTRIBUTE_MATRIX_TEMPLATE].Value = ddlMatrixTemplate?.SelectedValue;
                }
            }

            return configurationValues;
        }

        /// <summary>
        /// Sets the configuration value.
        /// </summary>
        /// <param name="controls"></param>
        /// <param name="configurationValues"></param>
        public override void SetConfigurationValues( List<Control> controls, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( controls != null && configurationValues != null )
            {
                if ( controls.Count > 0 )
                {
                    var ddlMatrixTemplate = controls[0] as RockDropDownList;
                    if ( ddlMatrixTemplate != null && configurationValues.ContainsKey( ATTRIBUTE_MATRIX_TEMPLATE ) )
                    {
                        ddlMatrixTemplate.SetValue( configurationValues[ATTRIBUTE_MATRIX_TEMPLATE]?.Value );
                    }
                }
            }
        }

        #endregion

        #region Formatting

        /// <summary>
        /// Returns the field's current value(s)
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public override string FormatValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            var rockContext = new RockContext();
            var attributeMatrixService = new AttributeMatrixService( rockContext );
            AttributeMatrix attributeMatrix = null;
            Guid? attributeMatrixGuid = value.AsGuidOrNull();
            if ( attributeMatrixGuid.HasValue )
            {
                attributeMatrix = attributeMatrixService.Get( attributeMatrixGuid.Value );

                // make a temp attributeMatrixItem to see what Attributes they have
                AttributeMatrixItem tempAttributeMatrixItem = new AttributeMatrixItem();
                tempAttributeMatrixItem.AttributeMatrix = attributeMatrix;
                tempAttributeMatrixItem.LoadAttributes();

                var lavaTemplate = attributeMatrix.AttributeMatrixTemplate.FormattedLava;
                Dictionary<string, object> mergeFields = Lava.LavaHelper.GetCommonMergeFields( parentControl?.RockBlock()?.RockPage, null, new Lava.CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false } );
                mergeFields.Add( "AttributeMatrix", attributeMatrix );
                mergeFields.Add( "ItemAttributes", tempAttributeMatrixItem.Attributes.Select( a => a.Value ).OrderBy( a => a.Order ).ThenBy( a => a.Name ) );
                mergeFields.Add( "AttributeMatrixItems", attributeMatrix.AttributeMatrixItems.OrderBy( a => a.Order ) );
                return lavaTemplate.ResolveMergeFields( mergeFields );
            }

            return base.FormatValue( parentControl, value, configurationValues, condensed );
        }

        #endregion

        /// <summary>
        /// Gets a value indicating whether this field has a control to configure the default value
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has default control; otherwise, <c>false</c>.
        /// </value>
        public override bool HasDefaultControl => false;

        /// <summary>
        /// Creates the control(s) necessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The id.</param>
        /// <returns>
        /// The control
        /// </returns>
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            if ( !configurationValues.ContainsKey( ATTRIBUTE_MATRIX_TEMPLATE ) )
            {
                return null;
            }

            var pnlMatrixEdit = new Panel { ID = id };

            int attributeMatrixTemplateId = configurationValues[ATTRIBUTE_MATRIX_TEMPLATE].Value.AsInteger();

            // Edit Panel
            var pnlEditMatrixItem = new Panel { ID = "pnlEditMatrixItem", Visible = false, CssClass = "well" };
            HiddenField hfMatrixItemId = new HiddenField { ID = "hfMatrixItemId" };
            pnlEditMatrixItem.Controls.Add( hfMatrixItemId );

            DynamicPlaceholder phMatrixItemAttributes = new DynamicPlaceholder { ID = "phMatrixItemAttributes" };
            pnlEditMatrixItem.Controls.Add( phMatrixItemAttributes );

            // make a temp attributeMatrixItem to see what Attributes they have
            AttributeMatrixItem tempAttributeMatrixItem = new AttributeMatrixItem();
            tempAttributeMatrixItem.AttributeMatrix = new AttributeMatrix { AttributeMatrixTemplateId = attributeMatrixTemplateId };
            tempAttributeMatrixItem.LoadAttributes();

            Rock.Attribute.Helper.AddEditControls( tempAttributeMatrixItem, phMatrixItemAttributes, false );

            LinkButton btnSaveMatrixItem = new LinkButton { ID = "btnSaveMatrixItem", CssClass = "btn btn-primary", Text = "Save Matrix Item" };
            btnSaveMatrixItem.Click += btnSaveMatrixItem_Click;
            pnlEditMatrixItem.Controls.Add( btnSaveMatrixItem );

            LinkButton btnCancelMatrixItem = new LinkButton { ID = "btnCancelMatrixItem", CssClass = "btn btn-link", Text = "Cancel" };
            btnCancelMatrixItem.Click += btnCancelMatrixItem_Click;
            pnlEditMatrixItem.Controls.Add( btnCancelMatrixItem );

            pnlMatrixEdit.Controls.Add( pnlEditMatrixItem );

            // Grid with view of MatrixItems
            var gMatrixItems = new Grid { ID = "gMatrixItems", DisplayType = GridDisplayType.Light };
            pnlMatrixEdit.Controls.Add( gMatrixItems );
            gMatrixItems.DataKeyNames = new string[] { "Id" };
            gMatrixItems.Actions.AddClick += gMatrixItems_AddClick;
            gMatrixItems.Actions.ShowAdd = true;
            gMatrixItems.IsDeleteEnabled = true;
            gMatrixItems.GridReorder += gMatrixItems_GridReorder;

            gMatrixItems.Columns.Add( new ReorderField() );

            foreach ( var attribute in tempAttributeMatrixItem.Attributes.Select( a => a.Value ) )
            {
                gMatrixItems.Columns.Add( new AttributeField { DataField = attribute.Key, HeaderText = attribute.Name } );
            }

            var deleteField = new DeleteField();
            deleteField.Click += gMatrixItems_DeleteClick;
            gMatrixItems.Columns.Add( deleteField );

            gMatrixItems.RowSelected += gMatrixItems_RowSelected;

            // HiddenField to store which AttributeMatrix record we are editing
            HiddenField hfAttributeMatrixGuid = new HiddenField { ID = "hfAttributeMatrixGuid" };
            pnlMatrixEdit.Controls.Add( hfAttributeMatrixGuid );

            return pnlMatrixEdit;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            Panel pnlMatrixEdit = control as Panel;

            if ( pnlMatrixEdit != null && configurationValues.ContainsKey( ATTRIBUTE_MATRIX_TEMPLATE ) )
            {
                int? attributeMatrixTemplateId = configurationValues[ATTRIBUTE_MATRIX_TEMPLATE]?.Value.AsIntegerOrNull();
                if ( attributeMatrixTemplateId.HasValue )
                {
                    var rockContext = new RockContext();
                    HiddenField hfAttributeMatrixGuid = pnlMatrixEdit.FindControl( "hfAttributeMatrixGuid" ) as HiddenField;
                    Guid? attributeMatrixGuid = hfAttributeMatrixGuid.Value.AsGuidOrNull();
                    AttributeMatrix attributeMatrix = null;
                    if ( attributeMatrixGuid.HasValue )
                    {
                        attributeMatrix = new AttributeMatrixService( rockContext ).Get( attributeMatrixGuid.Value );
                        return attributeMatrix.Guid.ToString();
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            Panel pnlMatrixEdit = control as Panel;
            if ( pnlMatrixEdit != null && configurationValues.ContainsKey( ATTRIBUTE_MATRIX_TEMPLATE ) )
            {
                int? attributeMatrixTemplateId = configurationValues[ATTRIBUTE_MATRIX_TEMPLATE]?.Value.AsIntegerOrNull();
                if ( attributeMatrixTemplateId.HasValue )
                {
                    Grid gMatrixItems = pnlMatrixEdit.FindControl( "gMatrixItems" ) as Grid;
                    HiddenField hfAttributeMatrixGuid = pnlMatrixEdit.FindControl( "hfAttributeMatrixGuid" ) as HiddenField;

                    var rockContext = new RockContext();
                    var attributeMatrixService = new AttributeMatrixService( rockContext );
                    AttributeMatrix attributeMatrix = null;
                    Guid? attributeMatrixGuid = value.AsGuidOrNull();
                    if ( attributeMatrixGuid.HasValue )
                    {
                        attributeMatrix = attributeMatrixService.Get( attributeMatrixGuid.Value );
                    }

                    if ( attributeMatrix == null )
                    {
                        // Create the AttributeMatrix now and save it even though they haven't hit save yet. We'll need the AttributeMatrix record to exist so that we can add AttributeMatrixItems to it
                        // If this ends up creating an orphan, we can clean up it up later
                        attributeMatrix = new AttributeMatrix { Guid = Guid.NewGuid() };
                        attributeMatrix.AttributeMatrixTemplateId = attributeMatrixTemplateId.Value;
                        attributeMatrix.AttributeMatrixItems = new List<AttributeMatrixItem>();
                        attributeMatrixService.Add( attributeMatrix );
                        rockContext.SaveChanges();
                    }

                    hfAttributeMatrixGuid.Value = attributeMatrix.Guid.ToString();

                    BindMatrixItemsGrid( gMatrixItems, attributeMatrix );
                }
            }
        }

        #region EditMatrixItem

        /// <summary>
        /// Handles the AddClick event of the gMatrixItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gMatrixItems_AddClick( object sender, EventArgs e )
        {
            LinkButton lbAdd = sender as LinkButton;
            Grid gMatrixItems = lbAdd.FirstParentControlOfType<Grid>();

            EditMatrixItem( gMatrixItems, 0 );
        }

        /// <summary>
        /// Handles the RowSelected event of the gMatrixItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        private void gMatrixItems_RowSelected( object sender, RowEventArgs e )
        {
            Grid gMatrixItems = sender as Grid;
            int matrixItemId = e.RowKeyId;

            EditMatrixItem( gMatrixItems, matrixItemId );
        }

        /// <summary>
        /// Edits the matrix item.
        /// </summary>
        /// <param name="gMatrixItems">The g matrix items.</param>
        /// <param name="matrixItemId">The matrix item identifier.</param>
        private void EditMatrixItem( Grid gMatrixItems, int matrixItemId )
        {
            Panel pnlMatrixEdit = gMatrixItems.Parent as Panel;
            Panel pnlEditMatrixItem = pnlMatrixEdit.FindControl( "pnlEditMatrixItem" ) as Panel;
            HiddenField hfMatrixItemId = pnlMatrixEdit.FindControl( "hfMatrixItemId" ) as HiddenField;
            HiddenField hfAttributeMatrixGuid = pnlMatrixEdit.FindControl( "hfAttributeMatrixGuid" ) as HiddenField;
            hfMatrixItemId.Value = matrixItemId.ToString();

            PlaceHolder phMatrixItemAttributes = pnlMatrixEdit.FindControl( "phMatrixItemAttributes" ) as PlaceHolder;

            // make a temp attributeMatrixItem to see what Attributes they have
            AttributeMatrixItem attributeMatrixItem = null;

            if ( matrixItemId > 0 )
            {
                attributeMatrixItem = new AttributeMatrixItemService( new RockContext() ).Get( matrixItemId );
            }

            if ( attributeMatrixItem == null )
            {
                attributeMatrixItem = new AttributeMatrixItem();
                attributeMatrixItem.AttributeMatrix = new AttributeMatrixService( new RockContext() ).Get( hfAttributeMatrixGuid.Value.AsGuid() );
            }

            attributeMatrixItem.LoadAttributes();

            phMatrixItemAttributes.Controls.Clear();
            Rock.Attribute.Helper.AddEditControls( attributeMatrixItem, phMatrixItemAttributes, true );

            gMatrixItems.Visible = false;
            pnlEditMatrixItem.Visible = true;
        }

        /// <summary>
        /// Handles the Click event of the btnSaveMatrixItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void btnSaveMatrixItem_Click( object sender, EventArgs e )
        {
            LinkButton btnSaveMatrixItem = sender as LinkButton;
            Panel pnlEditMatrixItem = btnSaveMatrixItem.Parent as Panel;
            Panel pnlMatrixEdit = pnlEditMatrixItem.Parent as Panel;
            HiddenField hfMatrixItemId = pnlMatrixEdit.FindControl( "hfMatrixItemId" ) as HiddenField;
            Grid gMatrixItems = pnlMatrixEdit.FindControl( "gMatrixItems" ) as Grid;
            PlaceHolder phMatrixItemAttributes = pnlMatrixEdit.FindControl( "phMatrixItemAttributes" ) as PlaceHolder;
            HiddenField hfAttributeMatrixGuid = pnlMatrixEdit.FindControl( "hfAttributeMatrixGuid" ) as HiddenField;
            Guid attributeMatrixGuid = hfAttributeMatrixGuid.Value.AsGuid();

            var rockContext = new RockContext();
            var attributeMatrixItemService = new AttributeMatrixItemService( rockContext );
            AttributeMatrixItem attributeMatrixItem = null;
            int attributeMatrixItemId = hfMatrixItemId.Value.AsInteger();

            if ( attributeMatrixItemId > 0 )
            {
                attributeMatrixItem = attributeMatrixItemService.Get( attributeMatrixItemId );
            }

            if ( attributeMatrixItem == null )
            {
                attributeMatrixItem = new AttributeMatrixItem();
                attributeMatrixItem.AttributeMatrix = new AttributeMatrixService( rockContext ).Get( attributeMatrixGuid );
                attributeMatrixItemService.Add( attributeMatrixItem );
            }

            attributeMatrixItem.LoadAttributes( rockContext );
            Rock.Attribute.Helper.GetEditValues( phMatrixItemAttributes, attributeMatrixItem );
            rockContext.SaveChanges();
            attributeMatrixItem.SaveAttributeValues( rockContext );

            gMatrixItems.Visible = true;
            pnlEditMatrixItem.Visible = false;

            BindMatrixItemsGrid( gMatrixItems, attributeMatrixItem.AttributeMatrix );
        }

        /// <summary>
        /// Handles the Click event of the btnCancelMatrixItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void btnCancelMatrixItem_Click( object sender, EventArgs e )
        {
            LinkButton btnCancelMatrixItem = sender as LinkButton;
            Panel pnlEditMatrixItem = btnCancelMatrixItem.Parent as Panel;
            Panel pnlMatrixEdit = pnlEditMatrixItem.Parent as Panel;

            Grid gMatrixItems = pnlMatrixEdit.FindControl( "gMatrixItems" ) as Grid;

            gMatrixItems.Visible = true;
            pnlEditMatrixItem.Visible = false;
        }

        #endregion EditMatrixItem

        #region MatrixItemsGrid

        /// <summary>
        /// Binds the matrix items grid.
        /// </summary>
        /// <param name="gMatrixItems">The g matrix attributes view.</param>
        /// <param name="attributeMatrix">The attribute matrix.</param>
        private void BindMatrixItemsGrid( Grid gMatrixItems, AttributeMatrix attributeMatrix )
        {
            foreach ( var attributeMatrixItem in attributeMatrix.AttributeMatrixItems )
            {
                attributeMatrixItem.LoadAttributes();
            }

            gMatrixItems.DataSource = attributeMatrix.AttributeMatrixItems.ToList();
            gMatrixItems.DataBind();
        }

        /// <summary>
        /// Handles the Click event of the deleteField control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        private void gMatrixItems_DeleteClick( object sender, RowEventArgs e )
        {
            DeleteField deleteField = sender as DeleteField;
            Grid gMatrixItems = deleteField.ParentGrid;

            int attributeMatrixItemId = e.RowKeyId;
            var rockContext = new RockContext();
            AttributeMatrixItemService attributeMatrixItemService = new AttributeMatrixItemService( rockContext );
            AttributeMatrixItem attributeMatrixItem = attributeMatrixItemService.Get( attributeMatrixItemId );
            if ( attributeMatrixItem != null )
            {
                var attributeMatrix = attributeMatrixItem.AttributeMatrix;
                attributeMatrixItemService.Delete( attributeMatrixItem );
                rockContext.SaveChanges();

                BindMatrixItemsGrid( gMatrixItems, attributeMatrix );
            }
        }

        /// <summary>
        /// Handles the GridReorder event of the gMatrixItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        private void gMatrixItems_GridReorder( object sender, GridReorderEventArgs e )
        {
            Grid gMatrixItems = sender as Grid;
            Panel pnlMatrixEdit = gMatrixItems.Parent as Panel;
            HiddenField hfAttributeMatrixGuid = pnlMatrixEdit.FindControl( "hfAttributeMatrixGuid" ) as HiddenField;
            Guid attributeMatrixGuid = hfAttributeMatrixGuid.Value.AsGuid();

            var rockContext = new RockContext();
            var attributeMatrix = new AttributeMatrixService( rockContext ).Get( attributeMatrixGuid );
            var service = new AttributeMatrixItemService( rockContext );
            var items = service.Queryable().Where(a => a.AttributeMatrixId == attributeMatrix.Id).OrderBy( i => i.Order ).ToList();
            service.Reorder( items, e.OldIndex, e.NewIndex );
            rockContext.SaveChanges();

            BindMatrixItemsGrid( gMatrixItems, attributeMatrix );
        }

        #endregion MatrixItemsGrid
    }
}