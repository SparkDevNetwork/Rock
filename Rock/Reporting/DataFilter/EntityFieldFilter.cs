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
            DropDownList ddlEntityField, List<Control> propertyControls, string propertyControlsPrefix )
        {
            string selectedEntityField = ddlEntityField.SelectedValue;

            writer.AddAttribute( "class", "row js-filter-row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            bool entityFieldPickerIsHidden = ddlEntityField.Style[HtmlTextWriterStyle.Display] == "none";

            writer.AddAttribute( "class", "col-md-3" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            
            if ( !entityFieldPickerIsHidden )
            {
                ddlEntityField.AddCssClass( "entity-property-selection" );
                ddlEntityField.RenderControl( writer );
            }
            else if ( ddlEntityField.SelectedItem != null )
            {
                writer.AddAttribute( "class", "data-view-filter-label" );
                writer.RenderBeginTag( HtmlTextWriterTag.Span );
                writer.Write( ddlEntityField.SelectedItem.Text );
                writer.RenderEndTag();
            }
            writer.RenderEndTag();

            writer.AddAttribute( "class", "col-md-9" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            // generate result for "none"
            StringBuilder sb = new StringBuilder();
            string lineFormat = @"
            case {0}: {1}; break;";

            int fieldIndex = 0;
            sb.AppendFormat( lineFormat, fieldIndex, "result = ''" );
            fieldIndex++;

            // render empty row for "none"
            writer.AddAttribute( "class", "row field-criteria" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.RenderEndTag();  // row

            foreach ( var entityField in entityFields )
            {
                string controlId = string.Format( "{0}_{1}", propertyControlsPrefix, entityField.Name );
                var control = propertyControls.FirstOrDefault( c => c.ID == controlId );
                if ( control != null )
                {
                    if ( entityField.Name != selectedEntityField )
                    {
                        if ( entityFieldPickerIsHidden )
                        {
                            // if the field can't be changed, we don't need the render the controls for the other entity fields
                            fieldIndex++;
                            continue;
                        }
                        
                        if ( control is HtmlControl )
                        {
                            ( (HtmlControl)control ).Style["display"] = "none";
                        }
                        else if ( control is WebControl )
                        {
                            ( (WebControl)control ).Style["display"] = "none";
                        }
                    }
                    control.RenderControl( writer );

                    string clientFormatSelection = entityField.FieldType.Field.GetFilterFormatScript( entityField.FieldConfig, entityField.Title );

                    if ( clientFormatSelection != string.Empty )
                    {
                        sb.AppendFormat( lineFormat, fieldIndex, clientFormatSelection );
                    }

                    fieldIndex++;
                }
            }

            writer.RenderEndTag();  // col-md-9

            writer.RenderEndTag();  // row

            string scriptFormat = @"
    function {0}PropertySelection($content){{

        var sIndex = $('select.entity-property-selection', $content).find(':selected').index();
        var $selectedContent = $('div.field-criteria', $content).eq(sIndex);
        var result = '';
        switch(sIndex) {{
            {1}
        }}
        return result;
    }}
";

            string script = string.Format( scriptFormat, entityType.Name, sb.ToString() );
            ScriptManager.RegisterStartupScript( filterControl, typeof( FilterField ), entityType.Name + "-property-selection", script, true );

            script = @"
    $('select.entity-property-selection').change(function(){
        var $parentRow = $(this).closest('.js-filter-row');
        $parentRow.find('div.field-criteria').hide();
        $parentRow.find('div.field-criteria').eq($(this).find(':selected').index()).show();
    });";

            // only need this script once per page
            ScriptManager.RegisterStartupScript( filterControl.Page, filterControl.Page.GetType(), "entity-property-selection-change-script", script, true );

            RegisterFilterCompareChangeScript( filterControl );
        }

        /// <summary>
        /// Sets the entity field selection.
        /// </summary>
        /// <param name="entityFields">The entity fields.</param>
        /// <param name="ddlProperty">The DDL property.</param>
        /// <param name="values">The values.</param>
        /// <param name="controls">The controls.</param>
        public void SetEntityFieldSelection( List<EntityField> entityFields, DropDownList ddlProperty, List<string> values, List<Control> controls )
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
                        if ( values.Count > 1 )
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
        /// Updates the selection from page parameters.
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