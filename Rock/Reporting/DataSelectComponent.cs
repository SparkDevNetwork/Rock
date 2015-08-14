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
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Data;
using Rock.Extension;

namespace Rock.Reporting
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class DataSelectComponent : Component
    {
        #region Properties

        /// <summary>
        /// Gets the name of the entity type. Filter should be an empty string
        /// if it applies to all entities
        /// </summary>
        /// <value>
        /// The name of the entity type.
        /// </value>
        public abstract string AppliesToEntityType { get; }

        /// <summary>
        /// Gets the section that this will appear in in the Field Selector
        /// </summary>
        /// <value>
        /// The section.
        /// </value>
        public virtual string Section
        {
            get { return "Advanced"; }
        }

        /// <summary>
        /// Gets the attribute value defaults.
        /// </summary>
        /// <value>
        /// The attribute defaults.
        /// </value>
        public override Dictionary<string, string> AttributeValueDefaults
        {
            get
            {
                var defaults = new Dictionary<string, string>();
                defaults.Add( "Active", "True" );
                return defaults;
            }
        }

        /// <summary>
        /// The PropertyName of the property in the anonymous class returned by the SelectExpression
        /// </summary>
        /// <value>
        /// The name of the column property.
        /// </value>
        public abstract string ColumnPropertyName { get; }

        /// <summary>
        /// Comma-delimited list of the Entity properties that should be used for Sorting. Normally, you should leave this as null which will make it sort on the returned field 
        /// To disable sorting for this field, return string.Empty;
        /// </summary>
        /// <value>
        /// The sort expression.
        /// </value>
        public virtual string SortProperties ( string selection )
        {
            return null;
        }

        /// <summary>
        /// Gets the type of the column field.
        /// </summary>
        /// <value>
        /// The type of the column field.
        /// </value>
        public abstract Type ColumnFieldType { get; }

        /// <summary>
        /// Gets the default column header text.
        /// </summary>
        /// <value>
        /// The default column header text.
        /// </value>
        public abstract string ColumnHeaderText { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the grid field.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public virtual System.Web.UI.WebControls.DataControlField GetGridField( Type entityType, string selection )
        {
            BoundField result = Rock.Web.UI.Controls.Grid.GetGridField( this.ColumnFieldType );
            return result;
        }

        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public abstract string GetTitle( Type entityType );

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public virtual Control[] CreateChildControls( Control parentControl )
        {
            string validationGroup = null;
            PlaceHolder phAttributes = new PlaceHolder();
            phAttributes.ID = parentControl.ID + "_phAttributes";
            parentControl.Controls.Add( phAttributes );
            Rock.Attribute.Helper.AddEditControls( this, phAttributes, true, validationGroup, new List<string>() { "Active", "Order" } );

            return new Control[1] { phAttributes };
        }

        /// <summary>
        /// Renders the controls.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="controls">The controls.</param>
        public virtual void RenderControls( Control parentControl, HtmlTextWriter writer, Control[] controls )
        {
            foreach ( var control in controls )
            {
                control.RenderControl( writer );
                writer.WriteLine();
            }
        }

        /// <summary>
        /// Gets the selection.
        /// This is typically a string that contains the values selected with the Controls
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public virtual string GetSelection( Control[] controls )
        {
            return GetAttributesSelectionValues( controls ).ToJson();
        }

        /// <summary>
        /// Gets the attributes selection values.
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> GetAttributesSelectionValues( Control[] controls )
        {
            PlaceHolder phAttributes = controls.FirstOrDefault( a => a.ID.EndsWith( "_phAttributes" ) ) as PlaceHolder;
            Dictionary<string, string> values = new Dictionary<string, string>();
            if ( this.Attributes != null )
            {
                foreach ( var attribute in this.Attributes )
                {
                    Control control = phAttributes.FindControl( string.Format( "attribute_field_{0}", attribute.Value.Id ) );
                    if ( control != null )
                    {
                        string value = attribute.Value.FieldType.Field.GetEditValue( control, attribute.Value.QualifierValues );
                        values.Add( attribute.Key, value );
                    }
                }
            }

            return values;
        }

        /// <summary>
        /// Gets the attribute value from selection.
        /// </summary>
        /// <param name="attributeKey">The attribute key.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public string GetAttributeValueFromSelection( string attributeKey, string selection )
        {
            Dictionary<string, string> values;
            try
            {
                values = Newtonsoft.Json.JsonConvert.DeserializeObject( selection, typeof( Dictionary<string, string> ) ) as Dictionary<string, string>;
            }
            catch
            {
                values = new Dictionary<string, string>();
            }

            if ( values.ContainsKey( attributeKey ) )
            {
                return values[attributeKey];
            }

            return string.Empty;
        }

        /// <summary>
        /// Sets the attributes selection values.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        private void SetAttributesSelectionValues( Control[] controls, string selection )
        {
            Dictionary<string, string> values = null;
            try
            {
                values = Newtonsoft.Json.JsonConvert.DeserializeObject( selection, typeof( Dictionary<string, string> ) ) as Dictionary<string, string>;
            }
            catch
            {
                // intentionally ignore if selection is corrupt
            }

            values = values ?? new Dictionary<string, string>();

            if ( this.Attributes != null )
            {
                var allControls = new List<Control>();
                foreach ( var control in controls )
                {
                    allControls.Add( control );
                    allControls.AddRange( control.ControlsOfTypeRecursive<Control>() );
                }

                foreach ( var attributeKey in values.Keys )
                {
                    var attribute = this.Attributes[attributeKey];
                    if ( attribute != null )
                    {
                        Control control = allControls.FirstOrDefault( a => a.ID == string.Format( "attribute_field_{0}", attribute.Id ) );
                        if ( control != null )
                        {
                            attribute.FieldType.Field.SetEditValue( control, attribute.QualifierValues, values[attribute.Key] );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public virtual void SetSelection( Control[] controls, string selection )
        {
            SetAttributesSelectionValues( controls, selection );
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="entityIdProperty">The entity identifier property.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public abstract Expression GetExpression( RockContext context, MemberExpression entityIdProperty, string selection );

        /// <summary>
        /// Gets the expression.
        /// override this for non-RockContext Expressions
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="entityIdProperty">The entity identifier property.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public virtual Expression GetExpression( System.Data.Entity.DbContext context, MemberExpression entityIdProperty, string selection )
        {
            return GetExpression( context as RockContext, entityIdProperty, selection );
        }

        #endregion
    }
}