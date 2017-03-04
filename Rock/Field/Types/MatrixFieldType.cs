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
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    ///  Matrix Field Type
    ///  Value stored as AttributeMatrix.Guid
    /// </summary>
    public class MatrixFieldType : FieldType
    {
        #region Configuration

        /// <summary>
        /// The attribute matrix template Id
        /// </summary>
        private const string ATTRIBUTE_MATRIX_TEMPLATE = "attributematrixtemplate";

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            var configKeys = base.ConfigurationKeys();
            configKeys.Add( ATTRIBUTE_MATRIX_TEMPLATE );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            var controls = base.ConfigurationControls();

            // build a drop down list of attribute matrix templates
            var ddlMatrixTemplate = new RockDropDownList();
            controls.Add( ddlMatrixTemplate );
            ddlMatrixTemplate.Label = "Attribute Matrix Template";
            ddlMatrixTemplate.Help = "The Attribute Matrix Template that defines this matrix attribute";

            var list = new AttributeMatrixTemplateService( new RockContext() ).Queryable().OrderBy( a => a.Name ).Select( a => new
            {
                a.Id,
                a.Name
            } ).ToList();

            ddlMatrixTemplate.Items.Clear();
            ddlMatrixTemplate.Items.Add( new ListItem() );

            foreach ( var item in list )
            {
                ddlMatrixTemplate.Items.Add( new ListItem( item.Name, item.Id.ToString() ) );
            }

            return controls;
        }

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls )
        {
            Dictionary<string, ConfigurationValue> configurationValues = new Dictionary<string, ConfigurationValue>();
            configurationValues.Add( ATTRIBUTE_MATRIX_TEMPLATE, new ConfigurationValue( "Attribute Matrix Type", "The Attribute Matrix Template that defines this matrix attribute", string.Empty ) );

            if ( controls != null )
            {
                if ( controls.Count > 0 )
                {
                    var ddlMatrixTemplate = controls[0] as RockDropDownList;
                    configurationValues[ATTRIBUTE_MATRIX_TEMPLATE].Value = ddlMatrixTemplate?.SelectedValue;
                }
            }

            return configurationValues;
        }

        /// <summary>
        /// Sets the configuration value.
        /// </summary>
        /// <param name="controls"></param>
        /// <param name="configurationValues"></param>
        public override void SetConfigurationValues( List<Control> controls, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( controls != null && configurationValues != null )
            {
                if ( controls.Count > 0 )
                {
                    var ddlMatrixTemplate = controls[0] as RockDropDownList;
                    if ( ddlMatrixTemplate != null && configurationValues.ContainsKey( ATTRIBUTE_MATRIX_TEMPLATE ) )
                    {
                        ddlMatrixTemplate.SetValue( configurationValues[ATTRIBUTE_MATRIX_TEMPLATE]?.Value );
                    }
                }
            }
        }

        #endregion

        #region Formatting

        public override string FormatValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            // TODO
            return base.FormatValue( parentControl, value, configurationValues, condensed );
        }

        #endregion

        /// <summary>
        /// Creates the control(s) necessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The id.</param>
        /// <returns>
        /// The control
        /// </returns>
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            if ( configurationValues.ContainsKey( ATTRIBUTE_MATRIX_TEMPLATE ) )
            {
                int? attributeMatrixTemplateId = configurationValues[ATTRIBUTE_MATRIX_TEMPLATE]?.Value.AsIntegerOrNull();
                if ( attributeMatrixTemplateId.HasValue )
                {
                    var rockContext = new RockContext();
                    var attributeMatrixTemplate = new AttributeMatrixTemplateService( rockContext ).Get( attributeMatrixTemplateId.Value );
                    if ( attributeMatrixTemplate != null )
                    {
                        attributeMatrixTemplate.LoadAttributes( rockContext );
                        var phAttributes = new PlaceHolder { ID = $"phAttributes_{id}" };
                        Rock.Attribute.Helper.AddEditControls( attributeMatrixTemplate, phAttributes, false );
                        phAttributes.Controls.Add( new HiddenField { ID = "hfAttributeMatrixId" } );

                        return phAttributes;
                    }
                }

            }

            return null;

        }

        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            PlaceHolder phAttributes = control as PlaceHolder;

            if ( phAttributes != null && configurationValues.ContainsKey( ATTRIBUTE_MATRIX_TEMPLATE ) )
            {
                int? attributeMatrixTemplateId = configurationValues[ATTRIBUTE_MATRIX_TEMPLATE]?.Value.AsIntegerOrNull();
                if ( attributeMatrixTemplateId.HasValue )
                {
                    var attributeMatrix = new AttributeMatrix();
                    attributeMatrix.AttributeMatrixTemplateId = attributeMatrixTemplateId.Value;
                    attributeMatrix.LoadAttributes();
                    Rock.Attribute.Helper.GetEditValues( phAttributes, attributeMatrix );
                    HiddenField hfAttributeMatrixGuid = phAttributes.FindControl( "hfAttributeMatrixGuid" ) as HiddenField;
                    return hfAttributeMatrixGuid.Value;
                }
            }

            return null;
        }

        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            //TODO base.SetEditValue( control, configurationValues, value );
        }

    }
}