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
using System.Web.UI;

using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type used to display a dropdown list of binary file types
    /// </summary>
    [Serializable]
    public class FinancialGatewayFieldType : FieldType
    {

        #region Formatting

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
            string formattedValue = string.Empty;

            Guid? financialGatewayGuid = value.AsGuidOrNull();
            if ( financialGatewayGuid.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var financialGateway = new FinancialGatewayService( rockContext ).Get( financialGatewayGuid.Value );
                    if ( financialGateway != null )
                    {
                        formattedValue = financialGateway.Name;
                    }
                }
            }

            return base.FormatValue( parentControl, formattedValue, null, condensed );

        }

        #endregion

        #region Edit Control

        /// <summary>
        /// Creates the control(s) necessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id"></param>
        /// <returns>
        /// The control
        /// </returns>
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            return new FinancialGatewayPicker { ID = id }; 
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues"></param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( control != null && control is FinancialGatewayPicker )
            {
                int id = int.MinValue;
                if ( Int32.TryParse( ( (FinancialGatewayPicker)control ).SelectedValue, out id ) )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        var financialGateway = new FinancialGatewayService( rockContext ).Get( id );
                        if ( financialGateway != null )
                        {
                            return financialGateway.Guid.ToString();
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues"></param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            Guid financialGatewayGuid = Guid.Empty;
            if (Guid.TryParse( value, out financialGatewayGuid ))
            {
                if ( control != null && control is FinancialGatewayPicker )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        var financialGateway = new FinancialGatewayService( rockContext ).Get( financialGatewayGuid );
                        if ( financialGateway != null )
                        {
                            ( (FinancialGatewayPicker)control ).SetValue( financialGateway.Id.ToString() );
                        }
                    }
                }
            }
        }

        #endregion

    }
}