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
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Data;
using Rock.Field.Types;
using Rock.Model;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Web.Utilities;

namespace Rock.Reporting.DataSelect.GroupMember
{
    /// <summary>
    /// Report Field for Group Member Attribute Values.
    /// </summary>
    [Description( "Show Group Attribute Values" )]
    [Export( typeof( DataSelectComponent ) )]
    [ExportMetadata( "ComponentName", "Group Attribute Select" )]
    public class GroupAttributeSelect : DataSelectComponent
    {
        #region Private Methods

        /// <summary>
        /// Gets the Attributes associated to groups.
        /// </summary>
        /// <returns></returns>
        private List<EntityField> GetGroupAttributeEntityFields()
        {
            var _groupAttributes = new List<EntityField>();

            var rockContext = new RockContext();
            var attributeService = new AttributeService( rockContext );
            var groupTypeService = new GroupTypeService( rockContext );

            var groupEntityTypeId = EntityTypeCache.GetId( typeof( Model.Group ) );

            var groupAttributes = attributeService
                .Queryable().AsNoTracking()
                .Where( a =>
                    a.EntityTypeId == groupEntityTypeId &&
                    a.EntityTypeQualifierColumn == "GroupTypeId" )
                .Join( groupTypeService.Queryable(),
                    a => a.EntityTypeQualifierValue, gt => gt.Id.ToString(), ( a, gt ) =>
                        new
                        {
                            a.Name,
                            a.Guid,
                            GroupTypeName = gt.Name
                        } )
                .OrderBy( a => a.GroupTypeName )
                .ThenBy( a => a.Name )
                .ToList();

            int index = 0;
            foreach ( var attribute in groupAttributes )
            {
                if ( !_groupAttributes.Any( e => e.AttributeGuid == attribute.Guid ) )
                {
                    var attributeCache = AttributeCache.Get( attribute.Guid );
                    var entityField = EntityHelper.GetEntityFieldForAttribute( attributeCache, false );
                    if ( entityField != null )
                    {
                        entityField.Title = $"{attribute.GroupTypeName}: {attribute.Name}";
                        entityField.AttributeGuid = attribute.Guid;
                        entityField.Index = index++;
                        _groupAttributes.Add( entityField );
                    }
                }
            }

            return _groupAttributes;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the entity type that the filter applies to.
        /// </summary>
        /// <value>
        /// The namespace-qualified Type name of the entity that the filter applies to, or an empty string if the filter applies to all entities.
        /// </value>
        public override string AppliesToEntityType
        {
            get { return typeof( Model.GroupMember ).FullName; }
        }

        /// <summary>
        /// The PropertyName of the property in the anonymous class returned by the SelectExpression
        /// </summary>
        /// <value>
        /// The name of the column property.
        /// </value>
        public override string ColumnPropertyName
        {
            get { return "Group Attribute"; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the title of the DataSelectComponent.
        /// Override this property to specify a Title that is different from the ColumnPropertyName.
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        /// <value>
        /// The title.
        /// </value>
        public override string GetTitle( Type entityType )
        {
            return "Group Attribute";
        }

        private const string _CtlGroup = "pnlGroupAttributeFilterControls";
        private const string _CtlProperty = "ddlProperty";

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <param name="parentControl"></param>
        /// <returns></returns>
        public override Control[] CreateChildControls( Control parentControl )
        {
            var pnl = new DynamicControlsPanel();
            pnl.ID = parentControl.GetChildControlInstanceName( _CtlGroup );
            parentControl.Controls.Add( pnl );

            pnl.Controls.Clear();

            // Create the field selection dropdown
            var ddlProperty = new RockDropDownList();
            ddlProperty.ID = pnl.GetChildControlInstanceName( _CtlProperty );
            pnl.Controls.Add( ddlProperty );

            // Add empty selection as first item.
            var groupAttributeEntityFields = GetGroupAttributeEntityFields();
            ddlProperty.Items.Add( new ListItem() );
            foreach ( var entityField in groupAttributeEntityFields )
            {
                // Add the field to the dropdown of available fields
                ddlProperty.Items.Add( new ListItem( entityField.Title, entityField.AttributeGuid.ToString() ) );
            }

            return new Control[] { pnl };
        }

        /// <summary>
        /// Gets the selection.
        /// This is typically a string that contains the values selected with the Controls
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( Control[] controls )
        {
            // Get selection control instances.
            var ddlProperty = controls.GetByName<DropDownList>( _CtlProperty );
            if ( ddlProperty != null )
            {
                return ddlProperty.SelectedValue;
            }
            return null;
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Control[] controls, string selection )
        {
            // Get selection control instances.
            var ddlProperty = controls.GetByName<DropDownList>( _CtlProperty );
            if ( ddlProperty != null )
            {
                ddlProperty.SetValue( selection );
            }
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="entityIdProperty">The entity identifier property.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override Expression GetExpression( RockContext context, MemberExpression entityIdProperty, string selection )
        {
            var attributeGuid = selection.AsGuid();

            var groupAttributeEntityFields = GetGroupAttributeEntityFields();

            var entityField = groupAttributeEntityFields.FirstOrDefault( f => f.AttributeGuid == attributeGuid );
            if ( entityField != null )
            {
                var serviceInstance = new AttributeValueService( context );
                var valuesQuery = serviceInstance.Queryable()
                    .Where( x => x.Attribute.Guid == attributeGuid )
                    .Select( x => new { x.EntityId, x.Value } );

                var groupMemberService = new GroupMemberService( context );

                var resultQuery = groupMemberService.Queryable()
                    .Select( gm => valuesQuery.FirstOrDefault( v => v.EntityId == gm.GroupId ).Value );

                return SelectExpressionExtractor.Extract( resultQuery, entityIdProperty, "gm" );
            }

            return null;
        }

        /// <summary>
        /// Gets the grid field.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override DataControlField GetGridField( Type entityType, string selection )
        {
            BoundField boundField;

            var attributeGuid = selection.AsGuid();

            var groupAttributeEntityFields = GetGroupAttributeEntityFields();

            var entityField = groupAttributeEntityFields.FirstOrDefault( f => f.AttributeGuid == attributeGuid );
            if ( entityField != null )
            {
                if ( entityField.FieldType.Guid.Equals( Rock.SystemGuid.FieldType.BOOLEAN.AsGuid() ) )
                {
                    boundField = new BoolField();
                }
                else if ( entityField.FieldType.Guid.Equals( Rock.SystemGuid.FieldType.DATE.AsGuid() ) )
                {
                    boundField = new DateField();
                }
                else if ( entityField.FieldType.Guid.Equals( Rock.SystemGuid.FieldType.DATE_TIME.AsGuid() ) )
                {
                    boundField = new DateTimeField();
                }
                else if ( entityField.FieldType.Guid.Equals( Rock.SystemGuid.FieldType.DEFINED_VALUE.AsGuid() ) )
                {
                    boundField = new DefinedValueField();
                }
                else
                {
                    boundField = new BoundField();
                }

                boundField.SortExpression = boundField.DataField;

                if ( entityField.FieldType.Guid.Equals( Rock.SystemGuid.FieldType.INTEGER.AsGuid() )
                    || entityField.FieldType.Guid.Equals( Rock.SystemGuid.FieldType.DATE.AsGuid() )
                    || entityField.FieldType.Guid.Equals( Rock.SystemGuid.FieldType.FILTER_DATE.AsGuid() ) )
                {
                    boundField.HeaderStyle.HorizontalAlign = HorizontalAlign.Right;
                    boundField.ItemStyle.HorizontalAlign = HorizontalAlign.Right;
                }

                return boundField;
            }

            return null;
        }

        #endregion
    }
}