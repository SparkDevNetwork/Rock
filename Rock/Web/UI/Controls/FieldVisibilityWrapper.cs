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

using Rock.Field;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Wraps content that is visible based on <see cref="FieldVisibilityRules"/>
    /// </summary>
    public class FieldVisibilityWrapper : DynamicPlaceholder
    {
        /// <summary>
        /// Gets or sets the attribute identifier of the Field
        /// </summary>
        /// <value>
        /// The attribute identifier.
        /// </value>
        public int AttributeId
        {
            get => ViewState["AttributeId"] as int? ?? 0;
            set => ViewState["AttributeId"] = value;
        }

        /// <summary>
        /// Sets the visibility based on the value of other attributes
        /// </summary>
        /// <param name="attributeValues">The attribute values.</param>
        public void UpdateVisibility( Dictionary<int, AttributeValueCache> attributeValues )
        {
            this.Visible = FieldVisibilityRules.Evaluate( attributeValues );
        }

        /// <summary>
        /// Gets or sets the edit control for the Attribute
        /// </summary>
        /// <value>
        /// The edit control.
        /// </value>
        public Control EditControl { get; set; }

        /// <summary>
        /// Gets the edit value from the <see cref="EditControl"/> associated with <see cref="AttributeId"/>
        /// </summary>
        /// <value>
        /// The edit value.
        /// </value>
        public string EditValue
        {
            get
            {
                var attribute = AttributeCache.Get( this.AttributeId );
                return attribute?.FieldType.Field.GetEditValue( this.EditControl, attribute.QualifierValues );
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
            var fieldVisibilityWrappers = parentControl.ControlsOfTypeRecursive<FieldVisibilityWrapper>().ToDictionary( k => k.AttributeId, v => v );
            Dictionary<int, AttributeValueCache> attributeValues = new Dictionary<int, AttributeValueCache>();

            foreach ( var fieldVisibilityWrapper in fieldVisibilityWrappers.Values )
            {
                attributeValues.Add( fieldVisibilityWrapper.AttributeId, new AttributeValueCache { AttributeId = fieldVisibilityWrapper.AttributeId, Value = fieldVisibilityWrapper.EditValue } );
            }

            foreach ( var fieldVisibilityWrapper in fieldVisibilityWrappers.Values )
            {
                fieldVisibilityWrapper.UpdateVisibility( attributeValues );
            }
        }

        #endregion
    }
}
