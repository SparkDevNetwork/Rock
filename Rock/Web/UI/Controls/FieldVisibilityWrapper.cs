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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Data;
using Rock.Field;
using Rock.Field.Types;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Wraps content that is visible based on <see cref="FieldVisibilityRules"/>
    /// </summary>
    public class FieldVisibilityWrapper : DynamicPlaceholder
    {
        /// <summary>
        /// Gets or sets the field identifier
        /// </summary>
        /// <value>
        /// The field identifier.
        /// </value>
        public int RegistrationTemplateFormFieldId
        {
            get => ViewState["RegistrationTemplateFormFieldId"] as int? ?? 0;
            set => ViewState["RegistrationTemplateFormFieldId"] = value;
        }

        /// <summary>
        /// Get the form field
        /// </summary>
        /// <returns></returns>
        public RegistrationTemplateFormFieldCache GetRegistrationTemplateFormField()
        {
            return RegistrationTemplateFormFieldCache.Get( RegistrationTemplateFormFieldId );
        }

        /// <summary>
        /// Get the attribute id
        /// </summary>
        /// <returns></returns>
        public AttributeCache GetAttributeCache()
        {
            var field = GetRegistrationTemplateFormField();
            var id = field?.AttributeId;

            if ( id.HasValue )
            {
                return AttributeCache.Get( id.Value );
            }

            return null;
        }

        /// <summary>
        /// Sets the visibility based on the value of other attributes
        /// </summary>
        /// <param name="attributeValues">The attribute values.</param>
        /// <param name="personFieldValues">The person field values.</param>
        public void UpdateVisibility( Dictionary<int, AttributeValueCache> attributeValues, Dictionary<RegistrationPersonFieldType, string> personFieldValues )
        {
            var visible = FieldVisibilityRules.Evaluate( attributeValues, personFieldValues );
            if ( visible == false && this.Visible )
            {
                // if hiding this field, set the value to null since we don't want to save values that aren't shown
                this.EditValue = null;
            }

            this.Visible = visible;
        }

        /// <summary>
        /// Gets or sets the edit control for the Attribute
        /// </summary>
        /// <value>
        /// The edit control.
        /// </value>
        public Control EditControl { get; set; }

        /// <summary>
        /// Gets the edit value from the <see cref="EditControl"/> associated with <see cref="RegistrationTemplateFormFieldId"/>
        /// </summary>
        /// <value>
        /// The edit value.
        /// </value>
        public string EditValue
        {
            get
            {
                var field = GetRegistrationTemplateFormField();
                var attribute = GetAttributeCache();

                if ( attribute != null )
                {
                    return attribute.FieldType.Field.GetEditValue( this.EditControl, attribute.QualifierValues );
                }
                else if ( FieldVisibilityRules.IsFieldSupported( field.PersonFieldType ) )
                {
                    var fieldType = FieldVisibilityRules.GetSupportedFieldTypeCache( field.PersonFieldType );
                    return fieldType.Field.GetEditValue( this.EditControl, null );
                }
                else
                {
                    throw new NotImplementedException( "The field type and source are not supported" );
                }
            }

            private set
            {
                var field = GetRegistrationTemplateFormField();
                var attribute = GetAttributeCache();

                if ( attribute != null )
                {
                    attribute.FieldType.Field.SetEditValue( this.EditControl, attribute.QualifierValues, value );
                }
                else if ( FieldVisibilityRules.IsFieldSupported( field.PersonFieldType ) )
                {
                    var fieldType = FieldVisibilityRules.GetSupportedFieldTypeCache( field.PersonFieldType );
                    fieldType.Field.SetEditValue( this.EditControl, null, value );
                }
                else
                {
                    throw new NotImplementedException( "The field type and source are not supported" );
                }
            }
        }

        /// <summary>
        /// Gets or sets the field visibility rules.
        /// </summary>
        /// <value>
        /// The field visibility rules.
        /// </value>
        public FieldVisibilityRules FieldVisibilityRules { get; set; }

        #region Event Handlers

        /// <summary>
        /// Gets called when an attributes edit control fires a EditValueUpdated
        /// </summary>
        public void TriggerEditValueUpdated( Control editControl, FieldEventArgs args )
        {
            EditValueUpdated?.Invoke( editControl, args );
        }

        /// <summary>
        /// Occurs when [edit value updated].
        /// </summary>
        public event EventHandler<FieldEventArgs> EditValueUpdated;

        /// <summary>
        /// 
        /// </summary>
        /// <seealso cref="System.EventArgs" />
        public class FieldEventArgs : EventArgs
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="FieldEventArgs" /> class.
            /// </summary>
            /// <param name="attribute">The attribute.</param>
            /// <param name="editControl">The edit control.</param>
            public FieldEventArgs( AttributeCache attribute, Control editControl )
            {
                this.AttributeId = attribute?.Id;
                this.EditControl = editControl;
            }

            /// <summary>
            /// Gets or sets the attribute identifier.
            /// </summary>
            /// <value>
            /// The attribute identifier.
            /// </value>
            public int? AttributeId { get; set; }

            /// <summary>
            /// Gets the edit control.
            /// </summary>
            /// <value>
            /// The edit control.
            /// </value>
            public Control EditControl { get; private set; }
        }

        /// <summary>
        /// Applies the field visibility rules for all FieldVisibilityWrappers contained in the parentControl
        /// </summary>
        public static void ApplyFieldVisibilityRules( Control parentControl )
        {
            var fieldVisibilityWrappers = parentControl.ControlsOfTypeRecursive<FieldVisibilityWrapper>().ToDictionary( k => k.RegistrationTemplateFormFieldId, v => v );
            var attributeValues = new Dictionary<int, AttributeValueCache>();
            var personFieldValues = new Dictionary<RegistrationPersonFieldType, string>();

            foreach ( var fieldVisibilityWrapper in fieldVisibilityWrappers.Values )
            {
                var field = fieldVisibilityWrapper.GetRegistrationTemplateFormField();

                if ( field.AttributeId.HasValue )
                {
                    var attributeId = field.AttributeId.Value;
                    attributeValues.Add( attributeId, new AttributeValueCache { AttributeId = attributeId, Value = fieldVisibilityWrapper.EditValue } );
                }
                else if ( FieldVisibilityRules.IsFieldSupported( field.PersonFieldType ) )
                {
                    personFieldValues[field.PersonFieldType] = fieldVisibilityWrapper.EditValue;
                }
            }

            // This needs to be done AFTER all of the attributeValuse for each fieldVisibilityWrapper are collected in order to work correctly.
            foreach ( var fieldVisibilityWrapper in fieldVisibilityWrappers.Values )
            {
                fieldVisibilityWrapper.UpdateVisibility( attributeValues, personFieldValues );
            }
        }

        #endregion
    }
}
