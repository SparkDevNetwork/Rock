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
    [Description( "Show Group Member Attribute Values" )]
    [Export( typeof( DataSelectComponent ) )]
    [ExportMetadata( "ComponentName", "Group Member Attribute Select" )]
    public class GroupMemberAttributeSelect : DataSelectComponent
    {
        #region Private Methods

        private List<EntityField> _GroupMemberAttributes = null;

        /// <summary>
        /// Gets the Attributes for a Group Member of a specific Group Type.
        /// </summary>
        /// <returns></returns>
        private List<EntityField> GetGroupMemberAttributes()
        {
            if (_GroupMemberAttributes == null)
            {
                var entityAttributeFields = new Dictionary<string, EntityField>();
                var context = new RockContext();

                var attributeService = new AttributeService( context );
                var groupTypeService = new GroupTypeService( context );

                var groupMemberEntityTypeId = EntityTypeCache.GetId( typeof(Model.GroupMember) );

                var groupMemberAttributes = attributeService.Queryable()
                                                            .AsNoTracking()
                                                            .Where( a => a.EntityTypeId == groupMemberEntityTypeId )
                                                            .Join( groupTypeService.Queryable(), a => a.EntityTypeQualifierValue, gt => gt.Id.ToString(),
                                                                   ( a, gt ) =>
                                                                   new
                                                                   {
                                                                       Attribute = a,
                                                                       AttributeKey = a.Key,
                                                                       FieldTypeName = a.FieldType.Name,
                                                                       a.FieldTypeId,
                                                                       AttributeName = a.Name,
                                                                       GroupTypeName = gt.Name
                                                                   } )
                                                            .GroupBy( x => x.AttributeName )
                                                            .ToList();

                foreach (var attributesByName in groupMemberAttributes)
                {
                    var attributeNameAndTypeGroups = attributesByName.GroupBy( x => x.FieldTypeId ).ToList();

                    bool requiresTypeQualifier = ( attributeNameAndTypeGroups.Count > 1 );

                    foreach (var attributeNameAndTypeGroup in attributeNameAndTypeGroups)
                    {
                        foreach (var attribute in attributeNameAndTypeGroup)
                        {
                            string fieldKey;
                            string fieldName;

                            if (requiresTypeQualifier)
                            {
                                fieldKey = attribute.AttributeName + "_" + attribute.FieldTypeId;

                                fieldName = string.Format( "{0} [{1}]", attribute.AttributeName, attribute.FieldTypeName );
                            }
                            else
                            {
                                fieldName = attribute.AttributeName;
                                fieldKey = attribute.AttributeName;
                            }

                            if (entityAttributeFields.ContainsKey( fieldKey ))
                            {
                                continue;
                            }

                            var attributeCache = AttributeCache.Read( attribute.Attribute );

                            var entityField = EntityHelper.GetEntityFieldForAttribute( attributeCache );

                            entityField.Title = fieldName;
                            entityField.AttributeGuid = null;

                            entityAttributeFields.Add( fieldKey, entityField );
                        }
                    }
                }

                int index = 0;
                var sortedFields = new List<EntityField>();
                foreach (var entityProperty in entityAttributeFields.Values.OrderBy( p => p.Title ).ThenBy( p => p.Name ))
                {
                    entityProperty.Index = index;
                    index++;
                    sortedFields.Add( entityProperty );
                }

                _GroupMemberAttributes = sortedFields;
            }

            return _GroupMemberAttributes;
        }

        #endregion

        #region Settings

        /// <summary>
        ///     Settings for the Data Filter Component.
        /// </summary>
        private class SelectSettings : SettingsStringBase
        {
            public string AttributeKey;

            public SelectSettings()
            {
            }

            public SelectSettings( string settingsString )
                : this()
            {
                FromSelectionString( settingsString );
            }

            public override bool IsValid
            {
                get
                {
                    if ( string.IsNullOrWhiteSpace( AttributeKey ) )
                    {
                        return false;
                    }

                    return true;
                }
            }

            protected override void OnSetParameters( int version, IReadOnlyList<string> parameters )
            {
                // Parameter 1: Attribute Key
                AttributeKey = DataComponentSettingsHelper.GetParameterOrEmpty( parameters, 0 );
            }

            protected override IEnumerable<string> OnGetParameters()
            {
                var settings = new List<string>();

                settings.Add( AttributeKey.ToStringSafe() );

                return settings;
            }
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
        /// Gets the name of the section in which the filter should be displayed in a browsable list.
        /// </summary>
        /// <value>
        /// The section name.
        /// </value>
        public override string Section
        {
            get { return "Common"; }
        }

        /// <summary>
        /// The PropertyName of the property in the anonymous class returned by the SelectExpression
        /// </summary>
        /// <value>
        /// The name of the column property.
        /// </value>
        public override string ColumnPropertyName
        {
            get { return "Group Member Attribute"; }
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
            return "Group Member Attribute";
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
            var pnlGroupAttributeFilterControls = new DynamicControlsPanel();
            pnlGroupAttributeFilterControls.ID = parentControl.GetChildControlInstanceName( _CtlGroup );
            parentControl.Controls.Add( pnlGroupAttributeFilterControls );

            pnlGroupAttributeFilterControls.Controls.Clear();

            // Create the field selection dropdown
            var ddlProperty = new RockDropDownList();
            ddlProperty.ID = pnlGroupAttributeFilterControls.GetChildControlInstanceName( _CtlProperty );

            pnlGroupAttributeFilterControls.Controls.Add( ddlProperty );

            // Add empty selection as first item.
            ddlProperty.Items.Add( new ListItem() );

            foreach ( var entityField in GetGroupMemberAttributes() )
            {
                // Add the field to the dropdown of available fields
                ddlProperty.Items.Add( new ListItem( entityField.Title, entityField.UniqueName ) );
            }

            return new Control[] { pnlGroupAttributeFilterControls };
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
            var pnlGroupAttributeFilterControls = controls.GetByName<Panel>( _CtlGroup );
            var ddlProperty = controls.GetByName<DropDownList>( _CtlProperty );

            if ( pnlGroupAttributeFilterControls == null )
            {
                return null;
            }

            var settings = new SelectSettings();
            settings.AttributeKey = ddlProperty.SelectedValue;

            return settings.ToSelectionString();
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Control[] controls, string selection )
        {
            // Get selection control instances.
            var pnlGroupAttributeFilterControls = controls.GetByName<Panel>( _CtlGroup );
            var ddlProperty = controls.GetByName<DropDownList>( _CtlProperty );

            if ( pnlGroupAttributeFilterControls == null )
            {
                return;
            }

            var settings = new SelectSettings( selection );

            if ( !settings.IsValid )
            {
                return;
            }

            ddlProperty.SelectedValue = settings.AttributeKey;
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
            var settings = new SelectSettings( selection );

            var entityFields = GetGroupMemberAttributes();
            var entityField = entityFields.FirstOrDefault( f => f.Name == settings.AttributeKey );
            if ( entityField == null )
            {
                return null;
            }

            var serviceInstance = new AttributeValueService( context );

            var entityTypeId = EntityTypeCache.GetId( typeof( Rock.Model.GroupMember ) );

            var valuesQuery = serviceInstance.Queryable()
                                             .Where( x => x.Attribute.Key == settings.AttributeKey && x.Attribute.EntityTypeId == entityTypeId )
                                             .Select( x => new { x.EntityId, x.Value } );

            var groupMemberService = new GroupMemberService( context );

            var resultQuery = groupMemberService.Queryable()
                                                .Select( gm => valuesQuery.FirstOrDefault( v => v.EntityId == gm.Id ).Value );

            var exp = SelectExpressionExtractor.Extract( resultQuery, entityIdProperty, "gm" );

            return exp;
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

            var settings = new SelectSettings( selection );

            var entityFields = GetGroupMemberAttributes();
            var entityField = entityFields.FirstOrDefault( f => f.Name == settings.AttributeKey );

            if ( entityField == null )
            {
                return null;
            }
            
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

        #endregion
    }
}