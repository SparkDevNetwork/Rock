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
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// User controls for managing defined types and their values
    /// </summary>
    [DisplayName( "Defined Type List" )]
    [Category( "Core" )]
    [Description( "Lists all the defined types and allows for managing them and their values." )]

    [LinkedPage( "Detail Page",
        Order = 0,
        Key = AttributeKey.DetailPage )]

    [CategoryField( AttributeKey.Categories,
        Description = "If block should only display Defined Types from specific categories, select the categories here.",
        AllowMultiple = true,
        EntityTypeName = "Rock.Model.DefinedType",
        Order = 1,
        IsRequired = false,
        Key = AttributeKey.Categories )]

    public partial class DefinedTypeList : RockBlock, ICustomGridColumns
    {
        public static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string Categories = "Categories";
        }

        #region Control Methods

        private List<Guid> _categoryGuids = null;

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            _categoryGuids = GetAttributeValue( AttributeKey.Categories ).SplitDelimitedValues().AsGuidList();
            if ( _categoryGuids.Any() )
            {
                tFilter.Visible = false;
                gDefinedType.ColumnsOfType<RockBoundField>().Where( c => c.DataField == "Category" ).First().Visible = _categoryGuids.Count > 1;
            }
            else
            {
                BindFilter();
                tFilter.ApplyFilterClick += tFilter_ApplyFilterClick;
                tFilter.DisplayFilterValue += tFilter_DisplayFilterValue;
                tFilter.Visible = true;
            }

            gDefinedType.DataKeyNames = new string[] { "Id" };
            gDefinedType.Actions.ShowAdd = true;
            gDefinedType.Actions.AddClick += gDefinedType_Add;
            gDefinedType.GridRebind += gDefinedType_GridRebind;

            // Block Security and special attributes (RockPage takes care of View)
            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );
            gDefinedType.Actions.ShowAdd = canAddEditDelete;
            gDefinedType.IsDeleteEnabled = canAddEditDelete;

            var securityField = gDefinedType.ColumnsOfType<SecurityField>().FirstOrDefault();
            securityField.EntityTypeId = EntityTypeCache.Get( typeof( DefinedType ) ).Id;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                gDefinedType_Bind();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Filter Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the tFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void tFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            int? categoryId = cpCategory.SelectedValueAsInt();
            tFilter.SaveUserPreference( "Category", categoryId.HasValue ? categoryId.Value.ToString() : string.Empty );

            gDefinedType_Bind();
        }

        /// <summary>
        /// ts the filter display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void tFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            if ( e.Key == "Category" )
            {
                int? categoryId = e.Value.AsIntegerOrNull();
                if ( categoryId.HasValue )
                {
                    var category = CategoryCache.Get( categoryId.Value );
                    if ( category != null )
                    {
                        e.Value = category.Name;
                    }
                }
                else
                {
                    e.Value = string.Empty;
                }
            }
            else
            {
                e.Value = string.Empty;
            }
        }

        #endregion

        #region Grid Events (main grid)

        /// <summary>
        /// Handles the Add event of the gDefinedType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gDefinedType_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, "DefinedTypeId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gDefinedType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gDefinedType_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, "DefinedTypeId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gDefinedType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gDefinedType_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var definedValueService = new DefinedValueService( rockContext );
            var definedTypeService = new DefinedTypeService( rockContext );

            DefinedType type = definedTypeService.Get( e.RowKeyId );

            if ( type != null )
            {
                string errorMessage;
                if ( !definedTypeService.CanDelete( type, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                // if this DefinedType has DefinedValues, see if they can be deleted
                var definedValues = definedValueService.GetByDefinedTypeId( type.Id ).ToList();

                foreach ( var value in definedValues )
                {
                    if ( !definedValueService.CanDelete( value, out errorMessage ) )
                    {
                        mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }
                }

                foreach ( var value in definedValues )
                {
                    definedValueService.Delete( value );
                }

                definedTypeService.Delete( type );

                rockContext.SaveChanges();
            }

            gDefinedType_Bind();
        }

        /// <summary>
        /// Handles the GridRebind event of the gDefinedType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gDefinedType_GridRebind( object sender, EventArgs e )
        {
            gDefinedType_Bind();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            int? categoryId = tFilter.GetUserPreference( "Category" ).AsIntegerOrNull();
            cpCategory.SetValue( categoryId );
        }

        /// <summary>
        /// Binds the grid for defined types.
        /// </summary>
        private void gDefinedType_Bind()
        {
            var queryable = new DefinedTypeService( new RockContext() ).Queryable();

            if ( _categoryGuids.Any() )
            {
                queryable = queryable.Where( a => a.Category != null && _categoryGuids.Contains( a.Category.Guid ) );
            }
            else
            {
                int? categoryId = tFilter.GetUserPreference( "Category" ).AsIntegerOrNull();
                if ( categoryId.HasValue )
                {
                    queryable = queryable.Where( a => a.CategoryId.HasValue && a.CategoryId.Value == categoryId.Value );
                }
            }

            SortProperty sortProperty = gDefinedType.SortProperty;
            if ( sortProperty != null )
            {
                queryable = queryable.Sort( sortProperty );
            }
            else
            {
                queryable = queryable.OrderBy( a => a.Category.Name ).ThenBy( a => a.Name );
            }

            gDefinedType.DataSource = queryable
                .Select( a =>
                    new
                    {
                        a.Id,
                        Category = a.Category.Name,
                        a.Name,
                        a.Description,
                        a.IsSystem,
                        FieldTypeName = a.FieldType.Name
                    } )
                .ToList();

            // SanitizeHtml can't be compiled into a SQL query so we have to ToList() the data and then sanitize the field in the List<T>
            //gDefinedType.DataSource = dataSource
            //    .Select( a =>
            //        new
            //        {
            //            a.Id,
            //            a.Category,
            //            a.Name,
            //            Description = a.Description.ScrubHtmlForGridDisplay(),
            //            a.IsSystem,
            //            a.FieldTypeName
            //        } )
            //    .ToList();
            gDefinedType.DataBind();
        }

        #endregion
    }
}