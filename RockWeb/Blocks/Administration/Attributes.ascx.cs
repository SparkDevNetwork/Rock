 //
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// User control for managing the attributes that are available for a specific entity
    /// </summary>
    [TextField( "Entity", "Entity Name", false, "", "Applies To", 0 )]
    [TextField( "Entity Qualifier Column", "The entity column to evaluate when determining if this attribute applies to the entity", false, "", "Applies To", 1 )]
    [TextField( "Entity Qualifier Value", "The entity column value to evaluate.  Attributes will only apply to entities with this value", false, "", "Applies To", 2 )]
    [BooleanField( "Allow Setting of Values", "Should UI be available for setting values of the specified Entity ID?", false, "Set Values", 0 )]
    [TextField( "Entity Id", "The entity id that values apply to", false, "", "Set Values", 1 )]
    public partial class Attributes : RockBlock
    {
        #region Fields

        protected int? _entityTypeId = null;
        protected string _entityQualifierColumn = string.Empty;
        protected string _entityQualifierValue = string.Empty;
        protected bool _displayValueEdit = false;
        protected int? _entityId = null;

        private bool _canConfigure = false;

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            string entityTypeName = GetAttributeValue( "Entity" );
            if ( string.IsNullOrWhiteSpace( entityTypeName ) )
            {
                entityTypeName = PageParameter( "Entity" );
            }
            _entityTypeId = Rock.Web.Cache.EntityTypeCache.GetId( entityTypeName );

            _entityQualifierColumn = GetAttributeValue( "EntityQualifierColumn" );
            if ( string.IsNullOrWhiteSpace( _entityQualifierColumn ) )
            {
                _entityQualifierColumn = PageParameter( "EntityQualifierColumn" );
            }

            _entityQualifierValue = GetAttributeValue( "EntityQualifierValue" );
            if ( string.IsNullOrWhiteSpace( _entityQualifierValue ) )
            {
                _entityQualifierValue = PageParameter( "EntityQualifierValue" );
            }

            _displayValueEdit = Convert.ToBoolean( GetAttributeValue( "AllowSettingofValues" ) );

            string entityIdString = GetAttributeValue( "EntityId" );
            if ( string.IsNullOrWhiteSpace( entityIdString ) )
            {
                entityIdString = PageParameter( "EntityId" );
            }

            if ( !string.IsNullOrWhiteSpace( entityIdString ) )
            {
                int entityIdint = 0;
                if ( int.TryParse( entityIdString, out entityIdint ) )
                {
                    _entityId = entityIdint;
                }
            }

            _canConfigure = CurrentPage.IsAuthorized( "Administrate", CurrentPerson );

            BindFilter();
            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;

            if ( _canConfigure )
            {
                rGrid.DataKeyNames = new string[] { "id" };
                rGrid.Actions.IsAddEnabled = true;

                rGrid.Actions.AddClick += rGrid_Add;
                rGrid.GridRebind += rGrid_GridRebind;
                rGrid.RowDataBound += rGrid_RowDataBound;

                rGrid.Columns[7].Visible = !_displayValueEdit;
                rGrid.Columns[8].Visible = _displayValueEdit;
                rGrid.Columns[9].Visible = _displayValueEdit;

                modalDetails.SaveClick += modalDetails_SaveClick;
                modalDetails.OnCancelScript = string.Format( "$('#{0}').val('');", hfIdValues.ClientID );

                string editAttributeId = Request.Form[hfIdValues.UniqueID];
                if ( Page.IsPostBack && editAttributeId != null && editAttributeId.Trim() != string.Empty )
                {
                    ShowEditValue( int.Parse( editAttributeId ), false );
                }
            }
            else
            {
                nbMessage.Text = "You are not authorized to configure this page";
                nbMessage.Visible = true;
            }
        }

        /// <summary>
        /// Handles the SaveClick event of the modalDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void modalDetails_SaveClick( object sender, EventArgs e )
        {
            if ( _displayValueEdit )
            {
                int attributeId = 0;
                if ( hfIdValues.Value != string.Empty && !int.TryParse( hfIdValues.Value, out attributeId ) )
                {
                    attributeId = 0;
                }

                if ( attributeId != 0 && phEditControl.Controls.Count > 0 )
                {
                    var attribute = Rock.Web.Cache.AttributeCache.Read( attributeId );

                    AttributeValueService attributeValueService = new AttributeValueService();
                    var attributeValue = attributeValueService.GetByAttributeIdAndEntityId( attributeId, _entityId ).FirstOrDefault();
                    if ( attributeValue == null )
                    {
                        attributeValue = new Rock.Model.AttributeValue();
                        attributeValue.AttributeId = attributeId;
                        attributeValue.EntityId = _entityId;
                        attributeValueService.Add( attributeValue, CurrentPersonId );
                    }

                    var fieldType = Rock.Web.Cache.FieldTypeCache.Read( attribute.FieldType.Id );
                    attributeValue.Value = fieldType.Field.GetEditValue( phEditControl.Controls[0], attribute.QualifierValues );

                    attributeValueService.Save( attributeValue, CurrentPersonId );

                    Rock.Web.Cache.AttributeCache.Flush( attributeId );
                    if ( !_entityTypeId.HasValue && _entityQualifierColumn == string.Empty && _entityQualifierValue == string.Empty && !_entityId.HasValue )
                    {
                        Rock.Web.Cache.GlobalAttributesCache.Flush();
                    }
                }

                hfIdValues.Value = string.Empty;

                modalDetails.Hide();
            }

            BindGrid();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack && _canConfigure )
            {
                BindGrid();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SaveUserPreference( "Category", ddlCategoryFilter.SelectedValue );

            BindGrid();
        }

        /// <summary>
        /// Handles the Edit event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void rGrid_Edit( object sender, RowEventArgs e )
        {
            ShowEdit( (int)rGrid.DataKeys[e.RowIndex]["id"] );
        }

        /// <summary>
        /// Handles the EditValue event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void rGrid_EditValue( object sender, RowEventArgs e )
        {
            if ( _displayValueEdit )
            {
                ShowEditValue( (int)rGrid.DataKeys[e.RowIndex]["id"], true );
            }
        }

        /// <summary>
        /// Handles the Delete event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void rGrid_Delete( object sender, RowEventArgs e )
        {
            var attributeService = new Rock.Model.AttributeService();

            Rock.Model.Attribute attribute = attributeService.Get( (int)rGrid.DataKeys[e.RowIndex]["id"] );
            if ( attribute != null )
            {
                Rock.Web.Cache.AttributeCache.Flush( attribute.Id );

                attributeService.Delete( attribute, CurrentPersonId );
                attributeService.Save( attribute, CurrentPersonId );
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the Add event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rGrid_Add( object sender, EventArgs e )
        {
            ShowEdit( 0 );
        }

        /// <summary>
        /// Handles the GridRebind event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rGrid_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs" /> instance containing the event data.</param>
        protected void rGrid_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                int attributeId = (int)rGrid.DataKeys[e.Row.RowIndex].Value;

                var attribute = Rock.Web.Cache.AttributeCache.Read( attributeId );
                var fieldType = Rock.Web.Cache.FieldTypeCache.Read( attribute.FieldTypeId );

                if ( _displayValueEdit )
                {
                    Literal lValue = e.Row.FindControl( "lValue" ) as Literal;
                    if ( lValue != null )
                    {
                        AttributeValueService attributeValueService = new AttributeValueService();
                        var attributeValue = attributeValueService.GetByAttributeIdAndEntityId( attributeId, _entityId ).FirstOrDefault();
                        if ( attributeValue != null && !string.IsNullOrWhiteSpace( attributeValue.Value ) )
                        {
                            lValue.Text = fieldType.Field.FormatValue( lValue, attributeValue.Value, attribute.QualifierValues, true );
                        }
                        else
                        {
                            lValue.Text = string.Format( "<span class='muted'>{0}</span>", fieldType.Field.FormatValue( lValue, attribute.DefaultValue, attribute.QualifierValues, true ) );
                        }
                    }
                }
                else
                {
                    Literal lDefaultValue = e.Row.FindControl( "lDefaultValue" ) as Literal;
                    if ( lDefaultValue != null )
                    {
                        lDefaultValue.Text = fieldType.Field.FormatValue( lDefaultValue, attribute.DefaultValue, attribute.QualifierValues, true );
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            using ( new UnitOfWorkScope() )
            {
                RockTransactionScope.WrapTransaction( () =>
                {
                    var attributeService = new AttributeService();
                    Rock.Model.Attribute attribute = null;

                    // remove old qualifier values in case they changed
                    if ( edtAttribute.AttributeId.HasValue )
                    {
                        AttributeQualifierService attributeQualifierService = new AttributeQualifierService();
                        foreach ( var oldQualifier in attributeQualifierService.GetByAttributeId( edtAttribute.AttributeId.Value ).ToList() )
                        {
                            attributeQualifierService.Delete( oldQualifier, CurrentPersonId );
                            attributeQualifierService.Save( oldQualifier, CurrentPersonId );
                        }
                        attribute = attributeService.Get( edtAttribute.AttributeId.Value );
                    }

                    if ( attribute == null )
                    {
                        attribute = new Rock.Model.Attribute();
                        attributeService.Add( attribute, CurrentPersonId );
                    }

                    edtAttribute.GetAttributeProperties( attribute );

                    attribute.EntityTypeId = _entityTypeId;
                    attribute.EntityTypeQualifierColumn = _entityQualifierColumn;
                    attribute.EntityTypeQualifierValue = _entityQualifierValue;

                    // Controls will show warnings
                    if ( !attribute.IsValid )
                    {
                        return;
                    }

                    Rock.Web.Cache.AttributeCache.Flush( attribute.Id );
                    attributeService.Save( attribute, CurrentPersonId );

                } );
            }

            BindGrid();

            pnlDetails.Visible = false;
            pnlList.Visible = true;
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            pnlDetails.Visible = false;
            pnlList.Visible = true;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            ddlCategoryFilter.Items.Clear();
            ddlCategoryFilter.Items.Add( Rock.Constants.All.Text );

            AttributeService attributeService = new AttributeService();
            var items = attributeService.Get( _entityTypeId, _entityQualifierColumn, _entityQualifierValue )
                .Where( a => a.Category != string.Empty && a.Category != null )
                .OrderBy( a => a.Category )
                .Select( a => a.Category )
                .Distinct()
                .ToList();

            foreach ( var item in items )
            {
                ListItem li = new ListItem( item );
                li.Selected = ( !Page.IsPostBack && rFilter.GetUserPreference( "Category" ) == item );
                ddlCategoryFilter.Items.Add( li );
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            AttributeService attributeService = new AttributeService();
            var queryable = attributeService.Get( _entityTypeId, _entityQualifierColumn, _entityQualifierValue );

            if ( ddlCategoryFilter.SelectedValue != Rock.Constants.All.Text )
            {
                queryable = queryable.
                    Where( a => a.Category == ddlCategoryFilter.SelectedValue );
            }

            SortProperty sortProperty = rGrid.SortProperty;
            if ( sortProperty != null )
            {
                queryable = queryable.
                    Sort( sortProperty );
            }
            else
            {
                queryable = queryable.
                    OrderBy( a => a.Category ).
                    ThenBy( a => a.Key );
            }

            rGrid.DataSource = queryable.ToList();
            rGrid.DataBind();
        }

        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="attributeId">The attribute id.</param>
        protected void ShowEdit( int attributeId )
        {
            var attributeModel = new AttributeService().Get( attributeId );

            edtAttribute.SetAttributeProperties( attributeModel );

            if ( attributeModel == null )
            {
                attributeModel.Category = ddlCategoryFilter.SelectedValue != Rock.Constants.All.Text ? ddlCategoryFilter.SelectedValue : string.Empty;
                edtAttribute.ActionTitle = Rock.Constants.ActionTitle.Edit( Rock.Model.Attribute.FriendlyTypeName );
            }
            else
            {
                edtAttribute.ActionTitle = Rock.Constants.ActionTitle.Edit( Rock.Model.Attribute.FriendlyTypeName );
            }

            pnlList.Visible = false;
            pnlDetails.Visible = true;
        }

        /// <summary>
        /// Shows the edit value.
        /// </summary>
        /// <param name="attributeId">The attribute id.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        protected void ShowEditValue( int attributeId, bool setValues )
        {
            if ( _displayValueEdit )
            {
                var attribute = Rock.Web.Cache.AttributeCache.Read( attributeId );

                hfIdValues.Value = attribute.Id.ToString();
                lCaption.Text = attribute.Name;

                AttributeValueService attributeValueService = new AttributeValueService();
                var attributeValue = attributeValueService.GetByAttributeIdAndEntityId( attributeId, _entityId ).FirstOrDefault();

                var fieldType = Rock.Web.Cache.FieldTypeCache.Read( attribute.FieldType.Id );

                Control editControl = fieldType.Field.EditControl( attribute.QualifierValues );
                editControl.ID = string.Format( "attribute_field_{0}", attribute.Id );
                editControl.ClientIDMode = ClientIDMode.AutoID;

                if ( setValues && attributeValue != null )
                {
                    fieldType.Field.SetEditValue( editControl, attribute.QualifierValues, attributeValue.Value );
                }

                phEditControl.Controls.Clear();
                phEditControl.Controls.Add( editControl );

                modalDetails.Show();
            }
        }

        #endregion
    }
}