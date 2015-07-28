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
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataFilter
{
    /// <summary>
    /// Abstract class that is used by DataFilters that let a user select a field/attribute of an entity
    /// </summary>
    public abstract class EntityFieldFilter : DataFilterComponent, IUpdateSelectionFromPageParameters
    {
        /// <summary>
        /// Renders the entity fields controls.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="filterControl">The filter control.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="entityFields">The entity fields.</param>
        /// <param name="ddlEntityField">The DDL entity field.</param>
        /// <param name="propertyControls">The property controls.</param>
        /// <param name="propertyControlsPrefix">The property controls prefix.</param>
        public void RenderEntityFieldsControls( Type entityType, FilterField filterControl, HtmlTextWriter writer, List<EntityField> entityFields,
            DropDownList ddlEntityField, List<Control> propertyControls, string propertyControlsPrefix, FilterMode filterMode )
        {
            string selectedEntityField = ddlEntityField.SelectedValue;

            writer.AddAttribute( "class", "row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            bool entityFieldPickerIsHidden = filterMode == FilterMode.SimpleFilter;

            if ( entityFieldPickerIsHidden )
            {
                ddlEntityField.Style[HtmlTextWriterStyle.Display] = "none";
            }

            if ( !entityFieldPickerIsHidden )
            {
                writer.AddAttribute( "class", "col-md-3" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                ddlEntityField.AddCssClass( "entity-property-selection" );
                ddlEntityField.RenderControl( writer );
                writer.RenderEndTag();
            }
            else
            {
                // render it as hidden (we'll need the postback value)
                ddlEntityField.RenderControl( writer );
            }

            writer.AddAttribute( "class", entityFieldPickerIsHidden ? "col-md-12" : "col-md-9" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            if ( entityFieldPickerIsHidden && ddlEntityField.SelectedItem != null )
            {
                if ( filterControl.ShowCheckbox )
                {
                    // special case when a filter is a entity field filter: render the checkbox here instead of in FilterField.cs
                    filterControl.cbIncludeFilter.Text = ddlEntityField.SelectedItem.Text;
                    filterControl.cbIncludeFilter.RenderControl( writer );
                }
                else
                {
                    writer.AddAttribute( "class", "filterfield-label" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Span );
                    writer.Write( ddlEntityField.SelectedItem.Text );
                    writer.RenderEndTag();
                }
            }

            // generate result for "none"
            StringBuilder sb = new StringBuilder();
            string lineFormat = @"
            case '{0}': {1}; break;";

            int fieldIndex = 0;
            sb.AppendFormat( lineFormat, fieldIndex, "result = ''" );
            fieldIndex++;

            // render empty row for "none"
            writer.AddAttribute( "class", "row field-criteria" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.RenderEndTag();  // "row field-criteria"

            // render the controls for the selectedEntityField
            string controlId = string.Format( "{0}_{1}", propertyControlsPrefix, selectedEntityField );
            var control = propertyControls.FirstOrDefault( c => c.ID == controlId );
            if ( control != null )
            {
                if ( control is IAttributeAccessor )
                {
                    ( control as IAttributeAccessor ).SetAttribute( "data-entity-field-name", selectedEntityField );
                }

                control.RenderControl( writer );
            }

            // create a javascript block for all the possible entityFields which will get rendered once per entityType
            foreach ( var entityField in entityFields )
            {
                string clientFormatSelection = entityField.FieldType.Field.GetFilterFormatScript( entityField.FieldConfig, entityField.Title );

                if ( clientFormatSelection != string.Empty )
                {
                    sb.AppendFormat( lineFormat, entityField.Name, clientFormatSelection );
                }

                fieldIndex++;
            }

            writer.RenderEndTag();  // col-md-9 or col-md-12

            writer.RenderEndTag();  // "row"

            string scriptFormat = @"
    function {0}PropertySelection($content){{
        var selectedFieldName = $('select.entity-property-selection', $content).find(':selected').val();
        var $selectedContent = $('[data-entity-field-name=' + selectedFieldName + ']', $content)
        var result = '';
        switch(selectedFieldName) {{
            {1}
        }}
        return result;
    }}
";

            string script = string.Format( scriptFormat, entityType.Name, sb.ToString() );
            ScriptManager.RegisterStartupScript( filterControl, typeof( FilterField ), entityType.Name + "-property-selection", script, true );

            RegisterFilterCompareChangeScript( filterControl );
        }

        /// <summary>
        /// Sets the entity field selection.
        /// </summary>
        /// <param name="entityFields">The entity fields.</param>
        /// <param name="ddlProperty">The DDL property.</param>
        /// <param name="values">The values.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="setFilterValues">if set to <c>true</c> [set filter values].</param>
        public void SetEntityFieldSelection( List<EntityField> entityFields, DropDownList ddlProperty, List<string> values, List<Control> controls, bool setFilterValues = true )
        {
            if ( values.Count > 0 && ddlProperty != null )
            {
                // Prior to v1.1 attribute.Name was used instead of attribute.Key, because of that, strip spaces to attempt matching key
                var entityField = entityFields.FirstOrDefault( f => f.Name == values[0].Replace( " ", "" ) );
                if ( entityField != null )
                {
                    string selectedProperty = entityField.Name;
                    ddlProperty.SelectedValue = selectedProperty;

                    var control = controls.ToList().FirstOrDefault( c => c.ID.EndsWith( entityField.Name ) );
                    if ( control != null )
                    {
                        if ( values.Count > 1 && setFilterValues )
                        {
                            entityField.FieldType.Field.SetFilterValues( control, entityField.FieldConfig, FixDelimination( values.Skip( 1 ).ToList() ) );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the name of the selected field.
        /// </summary>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public string GetSelectedFieldName( string selection )
        {
            if ( !string.IsNullOrWhiteSpace( selection ) )
            {
                var values = JsonConvert.DeserializeObject<List<string>>( selection );
                if ( values.Count > 0 )
                {
                    return values[0];
                }
            }

            return null;
        }

        /// <summary>
        /// Updates the selection from page parameters if there is a page parameter for the selection
        /// </summary>
        /// <param name="selection">The selection.</param>
        /// <param name="rockBlock">The rock block.</param>
        /// <returns></returns>
        public string UpdateSelectionFromPageParameters( string selection, Rock.Web.UI.RockBlock rockBlock )
        {
            if ( !string.IsNullOrWhiteSpace( selection ) )
            {
                var values = JsonConvert.DeserializeObject<List<string>>( selection );

                // selection list  is either "FieldName, Comparision, Value(s)" or "FieldName, Value(s)"
                if ( values.Count == 3 )
                {
                    var pageParamValue = rockBlock.PageParameter( values[0] );
                    if ( !string.IsNullOrEmpty( pageParamValue ) )
                    {
                        values[2] = pageParamValue;
                        return values.ToJson();
                    }
                }
                else if ( values.Count == 2 )
                {
                    var pageParamValue = rockBlock.PageParameter( values[0] );
                    if ( !string.IsNullOrEmpty( pageParamValue ) )
                    {
                        values[1] = pageParamValue;
                        return values.ToJson();
                    }
                }

            }

            return selection;
        }

        /// <summary>
        /// Updates the selection from user preference selection if the original selection is compatible with the user preference
        /// </summary>
        /// <param name="selection">The original selection value from the saved data filter</param>
        /// <param name="userPreferenceSelection">The user preference selection value</param>
        /// <param name="setFieldNameSelection">if set to <c>true</c> [set field name selection].</param>
        /// <returns></returns>
        public string UpdateSelectionFromUserPreferenceSelection( string selection, string userPreferenceSelection, bool setFieldNameSelection = false )
        {
            if ( !string.IsNullOrWhiteSpace( selection ) )
            {
                var selectionValues = JsonConvert.DeserializeObject<List<string>>( selection );
                List<string> userPreferenceValues = null;
                try
                {
                    userPreferenceValues = JsonConvert.DeserializeObject<List<string>>( userPreferenceSelection );
                }
                catch
                {
                    userPreferenceValues = null;
                }

                // only apply the UserPreferenceValues if they match the structure of the saved selection values
                if ( userPreferenceValues != null )
                {
                    if ( userPreferenceValues.Count >= 1 )
                    {
                        if ( setFieldNameSelection )
                        {
                            selectionValues[0] = userPreferenceValues[0];
                        }
                        else
                        {
                            // if the fieldname is different than the orig, don't use the user preference
                            if ( selectionValues[0] != userPreferenceValues[0] )
                            {
                                return selection;
                            }
                        }
                    }

                    // selection list  is either "FieldName, Comparision, Value(s)" or "FieldName, Value(s)", so only update the selection if it is one of those
                    if ( selectionValues.Count == 3 && userPreferenceValues.Count == 3 )
                    {
                        selectionValues[1] = userPreferenceValues[1];
                        selectionValues[2] = userPreferenceValues[2];
                        return selectionValues.ToJson();

                    }
                    else if ( selectionValues.Count == 2 && userPreferenceValues.Count == 2 )
                    {
                        selectionValues[1] = userPreferenceValues[1];
                        return selectionValues.ToJson();
                    }
                }
            }

            return selection;
        }

        /// <summary>
        /// Builds an expression for an attribute field
        /// </summary>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="entityField">The property.</param>
        /// <param name="values">The values.</param>
        /// <returns></returns>
        public Expression GetAttributeExpression( IService serviceInstance, ParameterExpression parameterExpression, EntityField entityField, List<string> values )
        {
            var service = new AttributeValueService( (RockContext)serviceInstance.Context );
            var attributeValues = service.Queryable().Where( v =>
                v.Attribute.Guid == entityField.AttributeGuid &&
                v.EntityId.HasValue &&
                v.Value != string.Empty );

            ParameterExpression attributeValueParameterExpression = Expression.Parameter( typeof( AttributeValue ), "v" );

            // Determine the appropriate comparison type to use for this Expression.
            // Attribute Value records only exist for Entities that have a value specified for the Attribute.
            // Therefore, if the specified comparison works by excluding certain values we must invert our filter logic:
            // first we find the Attribute Values that match those values and then we exclude the associated Entities from the result set.
            var comparisonType = ComparisonType.EqualTo;
            ComparisonType evaluatedComparisonType = comparisonType;

            if ( values.Count >= 2 )
            {
                string comparisonValue = values[0];
                if ( comparisonValue != "0" )
                {
                    comparisonType = comparisonValue.ConvertToEnum<ComparisonType>( ComparisonType.EqualTo );
                }

                switch ( comparisonType )
                {
                    case ComparisonType.DoesNotContain:
                        evaluatedComparisonType = ComparisonType.Contains;
                        break;
                    case ComparisonType.IsBlank:
                        evaluatedComparisonType = ComparisonType.IsNotBlank;
                        break;
                    case ComparisonType.NotEqualTo:
                        evaluatedComparisonType = ComparisonType.EqualTo;
                        break;
                    default:
                        evaluatedComparisonType = comparisonType;
                        break;
                }

                values[0] = evaluatedComparisonType.ToString();
            }

            var filterExpression = entityField.FieldType.Field.AttributeFilterExpression( entityField.FieldConfig, values, attributeValueParameterExpression );

            if ( filterExpression != null )
            {
                attributeValues = attributeValues.Where( attributeValueParameterExpression, filterExpression, null );
            }

            IQueryable<int> ids = attributeValues.Select( v => v.EntityId.Value );

            MemberExpression propertyExpression = Expression.Property( parameterExpression, "Id" );
            ConstantExpression idsExpression = Expression.Constant( ids.AsQueryable(), typeof( IQueryable<int> ) );
            Expression expression = Expression.Call( typeof( Queryable ), "Contains", new Type[] { typeof( int ) }, idsExpression, propertyExpression );

            // If we have used an inverted comparison type for the evaluation, invert the Expression so that it excludes the matching Entities.
            if ( comparisonType != evaluatedComparisonType )
            {
                return Expression.Not( expression );
            }
            else
            {
                return expression;
            }
        }

        /// <summary>
        /// Fixes the delimination of a multi-select value stored prior to v3.0
        /// </summary>
        /// <param name="values">The values.</param>
        /// <returns></returns>
        [System.Diagnostics.DebuggerStepThrough()]
        protected internal List<string> FixDelimination( List<string> values )
        {
            if ( values.Count() == 1 && values[0].Contains( "[" ) )
            {
                try
                {
                    var jsonValues = JsonConvert.DeserializeObject<List<string>>( values[0] );
                    values[0] = jsonValues.AsDelimited( "," );
                }
                catch { }
            }

            return values;
        }

    }
}