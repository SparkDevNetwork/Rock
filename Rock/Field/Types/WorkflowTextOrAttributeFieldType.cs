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
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type used to display a textbox and a dropdown list of attributes for a specific workflow Type
    /// </summary>
    [Serializable]
    public class WorkflowTextOrAttributeFieldType : FieldType
    {
        private const string WORKFLOW_TYPE_ATTRIBUTES_KEY = "WorkflowTypeAttributes";

        /// <summary>
        /// Returns the field's current value(s)
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public override string FormatValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            string formattedValue = value;

            Guid guid = value.AsGuid();
            if (!guid.IsEmpty())
            { 
                var workflowTypeAttributes = GetContextWorkflowTypeAttributes();
                if ( workflowTypeAttributes != null && workflowTypeAttributes.ContainsKey(guid) )
                {
                    formattedValue = workflowTypeAttributes[guid].Name;
                }

                if (string.IsNullOrWhiteSpace(formattedValue))
                {
                    var attributeCache = AttributeCache.Read( guid );
                    if (attributeCache != null)
                    {
                        formattedValue = attributeCache.Name;
                    }
                }
            }

            return base.FormatValue( parentControl, formattedValue, null, condensed );

        }

        /// <summary>
        /// Creates the control(s) neccessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id"></param>
        /// <returns>
        /// The control
        /// </returns>
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            var editControl = new Rock.Web.UI.Controls.RockTextOrDropDownList { ID = id };

            editControl.DropDownList.Items.Add( new ListItem() );

            var workflowTypeAttributes = GetContextWorkflowTypeAttributes();
            if ( workflowTypeAttributes != null )
            {
                foreach ( var attribute in workflowTypeAttributes )
                {
                    editControl.DropDownList.Items.Add( new ListItem( attribute.Value.Name, attribute.Key.ToString() ) );
                }
            }

            return editControl;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues"></param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( control != null && control is RockTextOrDropDownList )
            {
                return ( (RockTextOrDropDownList)control ).SelectedValue;
            }

            return string.Empty;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues"></param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            if ( value != null )
            {
                if ( control != null && control is RockTextOrDropDownList )
                {
                    ( (RockTextOrDropDownList)control ).SelectedValue = value;
                }
            }
        }

        private Dictionary<Guid, Rock.Model.Attribute> GetContextWorkflowTypeAttributes()
        {
            var httpContext = System.Web.HttpContext.Current;
            if ( httpContext != null && httpContext.Items != null )
            {
                return httpContext.Items[WORKFLOW_TYPE_ATTRIBUTES_KEY] as Dictionary<Guid, Rock.Model.Attribute>;
            }

            return null;
        }

    }
}