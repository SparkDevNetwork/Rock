//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Caching;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Core;
using Rock.Field;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// User control for editing the value(s) of a set of attributes for a given entity and category
    /// </summary>
    public partial class AttributeInstanceValues : System.Web.UI.UserControl
    {
        #region Fields

        protected Rock.Attribute.IHasAttributes _model;
        protected Rock.Web.Cache.AttributeCache _attribute;
        protected int? _currentPersonId;

        #endregion

        public AttributeInstanceValues()
            : base()
        {
        }

        public AttributeInstanceValues( Rock.Attribute.IHasAttributes model, Rock.Web.Cache.AttributeCache attribute, int? currentPersonId )
        {
            _model = model;
            _attribute = attribute;
            _currentPersonId = currentPersonId;
        }

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            lvAttributeValues.ItemEditing += new EventHandler<ListViewEditEventArgs>( lvAttributeValues_ItemEditing );
            lvAttributeValues.ItemDeleting += new EventHandler<ListViewDeleteEventArgs>( lvAttributeValues_ItemDeleting );
            lvAttributeValues.ItemCanceling += new EventHandler<ListViewCancelEventArgs>( lvAttributeValues_ItemCanceling );
            lvAttributeValues.ItemUpdating += new EventHandler<ListViewUpdateEventArgs>( lvAttributeValues_ItemUpdating );
            lvAttributeValues.ItemDataBound += new EventHandler<ListViewItemEventArgs>( lvAttributeValues_ItemDataBound );
            lvAttributeValues.ItemInserting += new EventHandler<ListViewInsertEventArgs>( lvAttributeValues_ItemInserting );

            if ( _attribute != null )
            {
                lAttributeName.Text = _attribute.Name;

                if ( _attribute.IsMultiValue )
                    lvAttributeValues.InsertItemPosition = InsertItemPosition.LastItem;
                else
                    lvAttributeValues.InsertItemPosition = InsertItemPosition.None;
            }

        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            //if ( !RockPage.IsPostBack )
                BindData();
        }

        private void BindData()  
        {
            lvAttributeValues.DataKeyNames = new string[] { "Id" };
            if ( _model != null )
                lvAttributeValues.DataSource = _model.AttributeValues[_attribute.Key];
            else
                lvAttributeValues.DataSource = new List<Rock.Core.AttributeValueDto>();
            lvAttributeValues.DataBind();

            if ( _attribute.IsMultiValue && lvAttributeValues.InsertItem != null )
            {
                PlaceHolder phInsertValue = lvAttributeValues.InsertItem.FindControl( "phInsertValue" ) as PlaceHolder;
                if ( phInsertValue != null )
                    phInsertValue.Controls.Add(_attribute.FieldType.Field.EditControl(_attribute.QualifierValues) );
            }

        }

        void lvAttributeValues_ItemUpdating( object sender, ListViewUpdateEventArgs e )
        {
            PlaceHolder phEditValue = lvAttributeValues.EditItem.FindControl( "phEditValue" ) as PlaceHolder;
            if ( phEditValue != null && phEditValue.Controls.Count == 1 )
            {
                string value = _attribute.FieldType.Field.GetEditValue( phEditValue.Controls[0], _attribute.QualifierValues );

                var attributeValueService = new AttributeValueService();
                var attributeValue = attributeValueService.Get( ( int )e.Keys["Id"] );
                if ( attributeValue == null )
                {
                    attributeValue = new AttributeValue();
                    attributeValueService.Add( attributeValue, _currentPersonId );

                    attributeValue.AttributeId = _attribute.Id;
                    attributeValue.EntityId = _model.Id;
                }

                attributeValue.Value = value;
                attributeValueService.Save( attributeValue, _currentPersonId );

                _model.LoadAttributes();
            }

            lvAttributeValues.EditIndex = -1;
            BindData();
        }

        void lvAttributeValues_ItemInserting( object sender, ListViewInsertEventArgs e )
        {
            PlaceHolder phInsertValue = lvAttributeValues.InsertItem.FindControl( "phInsertValue" ) as PlaceHolder;
            if ( phInsertValue != null && phInsertValue.Controls.Count == 1 )
            {
                string value = _attribute.FieldType.Field.GetEditValue( phInsertValue.Controls[0], _attribute.QualifierValues );

                var attributeValueService = new AttributeValueService();
                var attributeValue = new AttributeValue();
                attributeValue.AttributeId = _attribute.Id;
                attributeValue.EntityId = _model.Id;
                attributeValue.Value = value;

                int? maxOrder = attributeValueService.Queryable().
                    Where( a => a.AttributeId == attributeValue.AttributeId &&
                        a.EntityId == attributeValue.EntityId).
                    Select( a => ( int? )a.Order ).Max();

                attributeValue.Order = maxOrder.HasValue ? maxOrder.Value + 1 : 0;

                attributeValueService.Add( attributeValue, _currentPersonId);
                attributeValueService.Save( attributeValue, _currentPersonId );
                _model.LoadAttributes();
            }

            lvAttributeValues.EditIndex = -1;
            BindData();
        }

        void lvAttributeValues_ItemDeleting( object sender, ListViewDeleteEventArgs e )
        {
            var attributeValueService = new AttributeValueService();
            var attributeValue = attributeValueService.Get( ( int )e.Keys["Id"] );
            if ( attributeValue != null )
            {
                attributeValueService.Delete( attributeValue, _currentPersonId );
                attributeValueService.Save( attributeValue, _currentPersonId );
                _model.LoadAttributes();
            }

            BindData();
        }

        void lvAttributeValues_ItemEditing( object sender, ListViewEditEventArgs e )
        {
            lvAttributeValues.EditIndex = e.NewEditIndex;
            BindData();
        }

        void lvAttributeValues_ItemCanceling( object sender, ListViewCancelEventArgs e )
        {
            lvAttributeValues.EditIndex = -1;
            BindData();
        }

        void lvAttributeValues_ItemDataBound( object sender, ListViewItemEventArgs e )
        {
            if ( e.Item.ItemType == ListViewItemType.DataItem )
            {
                var attributeValue = e.Item.DataItem as Rock.Core.AttributeValueDto;
                if ( attributeValue != null )
                {
                    PlaceHolder phDisplayValue = e.Item.FindControl( "phDisplayValue" ) as PlaceHolder;
                    if ( phDisplayValue != null  )
                        phDisplayValue.Controls.Add( new LiteralControl( _attribute.FieldType.Field.FormatValue( phDisplayValue, attributeValue.Value, _attribute.QualifierValues, false ) ) );
                    else
                    {
                        PlaceHolder phEditValue = e.Item.FindControl( "phEditValue" ) as PlaceHolder;
                        if ( phEditValue != null )
                        {
                            Control editControl = _attribute.FieldType.Field.EditControl( _attribute.QualifierValues );
                            _attribute.FieldType.Field.SetEditValue( editControl, _attribute.QualifierValues, attributeValue.Value );
                            phEditValue.Controls.Add( editControl );
                        }
                    }
                }
            }
        }
    }
}