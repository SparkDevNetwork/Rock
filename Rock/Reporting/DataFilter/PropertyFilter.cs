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
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataFilter
{
    /// <summary>
    /// Filter entities on any of it's property or attribute values
    /// </summary>
    [Description( "Filter entities on any of it's property or attribute values" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Property Filter" )]
    public class PropertyFilter : EntityFieldFilter
    {
        #region Properties

        /// <summary>
        /// Gets the entity type that filter applies to.
        /// </summary>
        /// <value>
        /// The entity that filter applies to.
        /// </value>
        public override string AppliesToEntityType
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// Gets the section.
        /// </summary>
        /// <value>
        /// The section.
        /// </value>
        public override string Section
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public override int Order
        {
            get
            {
                return int.MinValue;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        public override string GetTitle( Type entityType )
        {
            return EntityTypeCache.Read( entityType ).FriendlyName + " Fields";
        }

        /// <summary>
        /// Formats the selection on the client-side.  When the filter is collapsed by the user, the Filterfield control
        /// will set the description of the filter to whatever is returned by this property.  If including script, the
        /// controls parent container can be referenced through a '$content' variable that is set by the control before 
        /// referencing this property.
        /// </summary>
        /// <value>
        /// The client format script.
        /// </value>
        public override string GetClientFormatSelection( Type entityType )
        {
            return string.Format( "{0}PropertySelection( $content )", entityType.Name );
        }

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override string FormatSelection( Type entityType, string selection )
        {
            var values = JsonConvert.DeserializeObject<List<string>>( selection );
            string result = entityType.Name.SplitCase() + " Property";

            if ( values.Count > 0 )
            {
                // First value in array is always the name of the entity field being filtered
                string entityFieldName = values[0].Replace( " ", "" );   // Prior to v1.1 attribute.Name was used instead of attribute.Key, because of that, strip spaces to attempt matching key

                var entityField = GetEntityFields( entityType ).Where( p => p.Name == entityFieldName ).FirstOrDefault();
                string entityFieldResult = GetEntityFieldFormatSelection( values, entityField );

                result = entityFieldResult ?? result;
            }

            return result;
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            var controls = new List<Control>();

            // Create the field selection dropdown
            var ddlProperty = new RockDropDownList();
            ddlProperty.ID = string.Format( "{0}_{1}", filterControl.ID, controls.Count() );
            filterControl.Controls.Add( ddlProperty );
            controls.Add( ddlProperty );

            var entityFields = GetEntityFields( entityType );

            // add Empty option first
            ddlProperty.Items.Add(new ListItem());
                        
            foreach ( var entityField in entityFields )
            {
                // Add the field to the dropdown of availailable fields
                ddlProperty.Items.Add( new ListItem( entityField.Title, entityField.Name ) );

                AddFieldTypeControls( filterControl, controls, entityField );
            }

            return controls.ToArray();
        }

        /// <summary>
        /// Renders the controls.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="filterControl">The filter control.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="controls">The controls.</param>
        public override void RenderControls( Type entityType, FilterField filterControl, HtmlTextWriter writer, Control[] controls )
        {
            List<EntityField> entityFields = GetEntityFields( entityType );
            var groupedControls = GroupControls( entityType, controls );
            DropDownList ddlEntityField = controls[0] as DropDownList;

            RenderEntityFieldsControls( entityType, filterControl, writer, entityFields, groupedControls, ddlEntityField );
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( Type entityType, Control[] controls )
        {
            var values = new List<string>();

            var groupedControls = GroupControls( entityType, controls );
            DropDownList ddlProperty = controls[0] as DropDownList;

            GetSelectionValuesForProperty( ddlProperty.SelectedValue, groupedControls, values );

            return values.ToJson();
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            if ( !string.IsNullOrWhiteSpace( selection ) )
            {
                var values = JsonConvert.DeserializeObject<List<string>>( selection );
                var groupedControls = GroupControls( entityType, controls );
                DropDownList ddlProperty = controls[0] as DropDownList;

                SetEntityFieldSelection( ddlProperty, values, groupedControls );
            }
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override Expression GetExpression( Type entityType, IService serviceInstance, ParameterExpression parameterExpression, string selection )
        {
            if ( !string.IsNullOrWhiteSpace( selection ) )
            {
                var values = JsonConvert.DeserializeObject<List<string>>( selection );

                if ( values.Count >= 2 )
                {
                    string selectedProperty = values[0].Replace( " ", "" );   // Prior to v1.1 attribute.Name was used instead of attribute.Key, because of that, strip spaces to attempt matching key

                    var entityField = GetEntityFields( entityType ).Where( p => p.Name == selectedProperty ).FirstOrDefault();
                    if ( entityField != null )
                    {
                        if ( entityField.FieldKind == FieldKind.Property )
                        {
                            return GetPropertyExpression( serviceInstance, parameterExpression, entityField, values.Skip( 1 ).ToList() );
                        }
                        else
                        {
                            return GetAttributeExpression( serviceInstance, parameterExpression, entityField, values.Skip( 1 ).ToList() );
                        }
                    }
                }
            }

            return null;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the properties and attributes for the entity
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        private List<EntityField> GetEntityFields( Type entityType )
        {
            return EntityHelper.GetEntityFields( entityType );
        }

        /// <summary>
        /// Builds an expression for a property field
        /// </summary>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="entityField">The property.</param>
        /// <param name="values">The values.</param>
        /// <returns></returns>
        private Expression GetPropertyExpression( IService serviceInstance, ParameterExpression parameterExpression, EntityField entityField, List<string> values )
        {
            Expression trueValue = Expression.Constant( true );
            MemberExpression propertyExpression = Expression.Property( parameterExpression, entityField.Name );

            switch ( entityField.FilterFieldType )
            {
                // Date Properties
                case SystemGuid.FieldType.DATE:
                case SystemGuid.FieldType.FILTER_DATE:

                    if ( values.Count == 2 )
                    {
                        ComparisonType comparisonType = values[0].ConvertToEnum<ComparisonType>( ComparisonType.EqualTo );
                        DateTime dateValue = DateTime.Today;
                        if ( values[1] == null || ( ! values[1].Equals( "CURRENT", StringComparison.OrdinalIgnoreCase ) ) )
                        {
                            dateValue = values[1].AsDateTime() ?? DateTime.MinValue;
                        }
                        ConstantExpression constantExpression = Expression.Constant( dateValue );
                        return ComparisonHelper.ComparisonExpression( comparisonType, propertyExpression, constantExpression );
                    }

                    break;

                // Integer properties
                case SystemGuid.FieldType.INTEGER:

                    if ( values.Count == 2 )
                    {
                        ComparisonType comparisonType = values[0].ConvertToEnum<ComparisonType>( ComparisonType.EqualTo );
                        int intValue = values[1].AsIntegerOrNull() ?? int.MinValue;
                        ConstantExpression constantExpression = Expression.Constant( intValue );
                        return ComparisonHelper.ComparisonExpression( comparisonType, propertyExpression, constantExpression );
                    }

                    break;

                // Decimal properties
                case SystemGuid.FieldType.DECIMAL:

                    if ( values.Count == 2 )
                    {
                        ComparisonType comparisonType = values[0].ConvertToEnum<ComparisonType>( ComparisonType.EqualTo );
                        decimal decimalValue = values[1].AsDecimalOrNull() ?? decimal.MinValue;
                        ConstantExpression constantExpression = Expression.Constant( decimalValue );
                        return ComparisonHelper.ComparisonExpression( comparisonType, propertyExpression, constantExpression );
                    }

                    break;

                // Enumerations and Defined Value properties
                case SystemGuid.FieldType.MULTI_SELECT:

                    if ( values.Count == 1 )
                    {
                        List<string> selectedValues = JsonConvert.DeserializeObject<List<string>>( values[0] );
                        if ( selectedValues.Any() )
                        {
                            if ( entityField.PropertyType.IsEnum )
                            {
                                ConstantExpression constantExpression = Expression.Constant( Enum.Parse( entityField.PropertyType, selectedValues[0].Replace( " ", string.Empty ) ) );
                                Expression comparison = Expression.Equal( propertyExpression, constantExpression );

                                foreach ( string selectedValue in selectedValues.Skip( 1 ) )
                                {
                                    constantExpression = Expression.Constant( Enum.Parse( entityField.PropertyType, selectedValue.Replace( " ", string.Empty ) ) );
                                    comparison = Expression.Or( comparison, Expression.Equal( propertyExpression, constantExpression ) );
                                }

                                return comparison;
                            }
                            else if ( entityField.DefinedTypeGuid.HasValue )
                            {
                                List<Guid> selectedValueGuids = selectedValues.Select( v => v.AsGuid() ).ToList();
                                List<int> selectedIds = new DefinedValueService( serviceInstance.Context as RockContext ).GetByGuids( selectedValueGuids ).Select( a => a.Id ).ToList();
                                ConstantExpression constantExpression = Expression.Constant( selectedIds, typeof( List<int> ) );
                                return ComparisonHelper.ComparisonExpression( ComparisonType.Contains, propertyExpression, constantExpression );
                            }
                        }
                    }

                    break;

                // Boolean Properties
                case SystemGuid.FieldType.SINGLE_SELECT:

                    if ( values.Count == 1 )
                    {
                        if ( entityField.PropertyType == typeof( bool ) || entityField.PropertyType == typeof( bool? ) )
                        {
                            ConstantExpression constantExpression = Expression.Constant( bool.Parse( values[0] ) );
                            ComparisonType comparisonType = ComparisonType.EqualTo;
                            return ComparisonHelper.ComparisonExpression( comparisonType, propertyExpression, constantExpression );
                            
                        }
                    }

                    break;

                // String Properties
                case SystemGuid.FieldType.TEXT:

                    if ( values.Count == 2 )
                    {
                        ComparisonType comparisonType = values[0].ConvertToEnum<ComparisonType>( ComparisonType.EqualTo );
                        ConstantExpression constantExpression;
                        if ( propertyExpression.Type == typeof( Guid ) )
                        {
                            constantExpression = Expression.Constant( values[1].AsGuid() );
                        }
                        else
                        {
                            constantExpression = Expression.Constant( values[1] );
                        }

                        return ComparisonHelper.ComparisonExpression( comparisonType, propertyExpression, constantExpression );
                    }

                    break;
            }

            return null;
        }

        /// <summary>
        /// Groups all the controls for each field
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        private Dictionary<string, List<Control>> GroupControls( Type entityType, Control[] controls )
        {
            var groupedControls = new Dictionary<string, List<Control>>();

            var properties = GetEntityFields( entityType );
            foreach ( var property in properties )
            {
                if ( !groupedControls.ContainsKey( property.Name ) )
                {
                    groupedControls.Add( property.Name, new List<Control>() );
                }

                for ( int i = property.Index; i < property.Index + property.ControlCount; i++ )
                {
                    groupedControls[property.Name].Add( controls[i] );
                }
            }

            return groupedControls;
        }

        #endregion
    }
}