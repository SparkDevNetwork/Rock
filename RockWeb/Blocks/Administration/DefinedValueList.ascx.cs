//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Attribute = Rock.Model.Attribute;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// User controls for managing defined values
    /// </summary>    
    public partial class DefinedValueList : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // assign type values grid actions
            gDefinedValues.DataKeyNames = new string[] { "id" };
            gDefinedValues.Actions.ShowAdd = true;
            gDefinedValues.Actions.AddClick += gDefinedValues_Add;
            gDefinedValues.GridRebind += gDefinedValues_GridRebind;
            gDefinedValues.GridReorder += gDefinedValues_GridReorder;            
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
                string itemId = PageParameter( "definedTypeId" );
                if ( !string.IsNullOrWhiteSpace( itemId ) )
                {
                    ShowDetail( "definedTypeId", int.Parse( itemId ) );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }

            if ( pnlDefinedValueEditor.Visible )
            {
                if ( !string.IsNullOrWhiteSpace( hfDefinedTypeId.Value ) )
                {
                    var definedValue = new DefinedValue { DefinedTypeId = hfDefinedTypeId.ValueAsInt() };
                    definedValue.LoadAttributes();
                    phDefinedValueAttributes.Controls.Clear();
                    Rock.Attribute.Helper.AddEditControls( definedValue, phDefinedValueAttributes, false );
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnSaveValue control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSaveValue_Click( object sender, EventArgs e )
        {
            using ( new Rock.Data.UnitOfWorkScope() )
            {
                DefinedValueService valueService = new DefinedValueService();

                DefinedValue definedValue;

                int definedValueId = hfDefinedValueId.ValueAsInt();
                if ( definedValueId == 0 )
                {
                    definedValue = new DefinedValue();
                    definedValue.IsSystem = false;
                    definedValue.DefinedTypeId = definedValueId;
                    valueService.Add( definedValue, CurrentPersonId );
                }
                else
                {
                    Rock.Web.Cache.AttributeCache.Flush( definedValueId );
                    definedValue = valueService.Get( definedValueId );
                }

                definedValue.Name = tbValueName.Text;
                definedValue.Description = tbValueDescription.Text;

                valueService.Save( definedValue, CurrentPersonId );
            }

            BindDefinedValuesGrid();
            pnlValues.Visible = true;
        }
        
        /// <summary>
        /// Handles the Add event of the gDefinedValues control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gDefinedValues_Add( object sender, EventArgs e )
        {
            gDefinedValues_ShowEdit( 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gDefinedValues control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gDefinedValues_Edit( object sender, RowEventArgs e )
        {
            int definedValueId = (int)e.RowKeyValue;
            gDefinedValues_ShowEdit( definedValueId );
        }

        /// <summary>
        /// Handles the Delete event of the gDefinedValues control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gDefinedValues_Delete( object sender, RowEventArgs e )
        {
            var valueService = new DefinedValueService();

            DefinedValue value = valueService.Get( (int)e.RowKeyValue );

            if ( value != null )
            {
                valueService.Delete( value, CurrentPersonId );
                valueService.Save( value, CurrentPersonId );
            }

            BindDefinedValuesGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnSaveDefinedValue control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSaveDefinedValue_Click( object sender, EventArgs e )
        {
            DefinedValue definedValue;
            DefinedValueService definedValueService = new DefinedValueService();

            int definedValueId = hfDefinedValueId.ValueAsInt();

            if ( definedValueId.Equals( 0 ) )
            {
                definedValue = new DefinedValue { Id = 0 };
                definedValue.DefinedTypeId = hfDefinedTypeId.ValueAsInt();
                definedValue.IsSystem = false;
            }
            else
            {
                definedValue = definedValueService.Get( definedValueId );
            }

            definedValue.Name = tbValueName.Text;
            definedValue.Description = tbValueDescription.Text;
            definedValue.LoadAttributes();
            Rock.Attribute.Helper.GetEditValues( phDefinedValueAttributes, definedValue );

            if ( !Page.IsValid )
            {
                return;
            }

            if ( !definedValue.IsValid )
            {
                // Controls will render the error messages                    
                return;
            }

            RockTransactionScope.WrapTransaction( () =>
            {
                if ( definedValue.Id.Equals( 0 ) )
                {
                    definedValueService.Add( definedValue, CurrentPersonId );
                }

                definedValueService.Save( definedValue, CurrentPersonId );
                Rock.Attribute.Helper.SaveAttributeValues( definedValue, CurrentPersonId );
            } );

            pnlDetails.Visible = true;
            pnlDefinedValueEditor.Visible = false;
            BindDefinedValuesGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnCancelDefinedValue control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancelDefinedValue_Click( object sender, EventArgs e )
        {
            pnlDetails.Visible = true;
            pnlDefinedValueEditor.Visible = false;
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
        public void ShowDetail( string itemKey, int itemKeyValue )
        {
            if ( !itemKey.Equals( "definedTypeId" ) )
            {
                return;
            }

            pnlDetails.Visible = true;
            DefinedType definedType = null;

            if ( !itemKeyValue.Equals( 0 ) )
            {
                definedType = new DefinedTypeService().Get( itemKeyValue );
            }
            else
            {
                definedType = new DefinedType { Id = 0 };
            }

            hfDefinedTypeId.SetValue( definedType.Id );
            BindDefinedValuesGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gDefinedValues control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gDefinedValues_GridRebind( object sender, EventArgs e )
        {
            BindDefinedValuesGrid();
        }

        /// <summary>
        /// Handles the GridReorder event of the gDefinedValues control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        private void gDefinedValues_GridReorder( object sender, GridReorderEventArgs e )
        {
            int definedTypeId = hfDefinedTypeId.ValueAsInt();
            DefinedTypeCache.Flush( definedTypeId );

            using ( new UnitOfWorkScope() )
            {
                var definedValueService = new DefinedValueService();               
                var definedValues = definedValueService.Queryable().Where( a => a.DefinedTypeId == definedTypeId ).OrderBy( a => a.Order );
                definedValueService.Reorder( definedValues.ToList(), e.OldIndex, e.NewIndex, CurrentPersonId );
                BindDefinedValuesGrid();
            }
        }

        /// <summary>
        /// Binds the defined values grid.
        /// </summary>
        protected void BindDefinedValuesGrid()
        {
            AttributeService attributeService = new AttributeService();

            int definedTypeId = hfDefinedTypeId.ValueAsInt();
            
            // add attributes with IsGridColumn to grid
            string qualifierValue = hfDefinedTypeId.Value;
            var qryDefinedTypeAttributes = attributeService.GetByEntityTypeId( new DefinedValue().TypeId ).AsQueryable()
                .Where( a => a.EntityTypeQualifierColumn.Equals( "DefinedTypeId", StringComparison.OrdinalIgnoreCase )
                && a.EntityTypeQualifierValue.Equals( qualifierValue ) );

            qryDefinedTypeAttributes = qryDefinedTypeAttributes.Where( a => a.IsGridColumn );

            List<Attribute> gridItems = qryDefinedTypeAttributes.ToList();

            foreach ( var item in gDefinedValues.Columns.OfType<AttributeField>().ToList() )
            {
                gDefinedValues.Columns.Remove( item );
            }

            foreach ( var item in gridItems.OrderBy( a => a.Order ).ThenBy( a => a.Name ) )
            {
                string dataFieldExpression = item.Key;
                bool columnExists = gDefinedValues.Columns.OfType<AttributeField>().FirstOrDefault( a => a.DataField.Equals( dataFieldExpression ) ) != null;
                if ( !columnExists )
                {
                    AttributeField boundField = new AttributeField();
                    boundField.DataField = dataFieldExpression;
                    boundField.HeaderText = item.Name;
                    boundField.SortExpression = string.Empty;
                    int insertPos = gDefinedValues.Columns.IndexOf( gDefinedValues.Columns.OfType<ReorderField>().First());
                    gDefinedValues.Columns.Insert(insertPos, boundField );
                }
            }

            var queryable = new DefinedValueService().Queryable().Where( a => a.DefinedTypeId == definedTypeId ).OrderBy( a => a.Order );
            var result = queryable.ToList();

            gDefinedValues.DataSource = result;
            gDefinedValues.DataBind();
        }

        /// <summary>
        /// Shows the edit value.
        /// </summary>
        /// <param name="valueId">The value id.</param>
        protected void gDefinedValues_ShowEdit( int valueId )
        {
            pnlDetails.Visible = false;
            pnlDefinedValueEditor.Visible = true;
            var definedType = DefinedTypeCache.Read( hfDefinedTypeId.ValueAsInt() );            
            DefinedValue definedValue;
            if ( valueId.Equals( 0 ) )
            {
                definedValue = new DefinedValue { Id = 0 };
                definedValue.DefinedTypeId = hfDefinedTypeId.ValueAsInt();
                if ( definedType != null )
                {
                    lActionTitleDefinedValue.Text = ActionTitle.Add( "defined value for " + definedType.Name );
                }                
            }
            else
            {
                definedValue = new DefinedValueService().Get( valueId );
                if ( definedType != null )
                {
                    lActionTitleDefinedValue.Text = ActionTitle.Edit( "defined value for " + definedType.Name );
                }                                
            }            

            hfDefinedValueId.SetValue( definedValue.Id );
            tbValueName.Text = definedValue.Name;
            tbValueDescription.Text = definedValue.Description;
            definedValue.LoadAttributes();
            phDefinedValueAttributes.Controls.Clear();
            Rock.Attribute.Helper.AddEditControls( definedValue, phDefinedValueAttributes, true );
        }
                
        #endregion
    }
}