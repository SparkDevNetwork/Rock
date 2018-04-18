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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataFilter.Person
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.Reporting.DataFilterComponent" />
    [Description( "Filter people on based on the phone type and messaging capability" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Phone Number" )]
    public class HasPhoneFilter : DataFilterComponent
    {
        #region Properties

        /// <summary>
        /// Gets the entity type that the filter applies to.
        /// </summary>
        /// <value>
        /// The namespace-qualified Type name of the entity that the filter applies to, or an empty string if the filter applies to all entities.
        /// </value>
        public override string AppliesToEntityType
        {
            get { return typeof( Rock.Model.Person ).FullName; }
        }

        /// <summary>
        /// Gets the user-friendly title used to identify the filter component.
        /// </summary>
        /// <param name="entityType">The System Type of the entity to which the filter will be applied.</param>
        /// <returns>
        /// The name of the filter.
        /// </returns>
        public override string GetTitle( Type entityType )
        {
            return "Phone Number";
        }

        /// <summary>
        /// Gets the name of the section in which the filter should be displayed in a browsable list.
        /// </summary>
        /// <value>
        /// The section name.
        /// </value>
        public override string Section
        {
            get { return "Additional Filters"; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Create the controls for the phone number type list and the Has SMS selection
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="filterControl"></param>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            // Create ddl to select search for selected phone type or absense of phone type
            RockDropDownList ddlHasPhoneOfType = new RockDropDownList();
            ddlHasPhoneOfType.CssClass = "js-hasphoneoftype";
            ddlHasPhoneOfType.ID = $"{filterControl.ID}_ddlHasPhoneOfType";
            ddlHasPhoneOfType.Items.Add( new ListItem( "Has Phone Type", "True" ) );
            ddlHasPhoneOfType.Items.Add( new ListItem( "Doesn't Have Phone Type", "False" ) );
            ddlHasPhoneOfType.SelectedValue = "True";
            filterControl.Controls.Add( ddlHasPhoneOfType );

            // List of phone types
            RockDropDownList ddlPhoneNumberType = new RockDropDownList();
            ddlPhoneNumberType.CssClass = "js-phonetype";
            ddlPhoneNumberType.ID = $"{filterControl.ID}_ddlPhoneNumberType";
            ddlPhoneNumberType.Items.Add( new ListItem("Any Phone", string.Empty) );
            foreach ( var value in DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.PERSON_PHONE_TYPE.AsGuid() ).DefinedValues.OrderBy( a => a.Order ).ThenBy( a => a.Value ) )
            {
                ddlPhoneNumberType.Items.Add( new ListItem( value.Value.EndsWith( "Phone" ) ? value.Value : value.Value + " Phone", value.Guid.ToString() ) );
            }

            filterControl.Controls.Add( ddlPhoneNumberType );

            // Bool val for IsMessagineEnabled
            RockDropDownList ddlHasSMS = new RockDropDownList();
            ddlHasSMS.CssClass = "js-hassms";
            ddlHasSMS.ID = $"{filterControl.ID}_ddlHasSMS";
            ddlHasSMS.Label = "SMS Enabled";
            ddlHasSMS.Items.Add( new ListItem() );
            ddlHasSMS.Items.Add( new ListItem( "Yes", "True" ) );
            ddlHasSMS.Items.Add( new ListItem( "No", "False" ) );
            filterControl.Controls.Add( ddlHasSMS );

            // Send them to the caller
            return new Control[] { ddlHasPhoneOfType, ddlPhoneNumberType, ddlHasSMS };
        }

        /// <summary>
        /// Renders the child controls used to display and edit the filter settings for HTML presentation.
        /// Implement this version of RenderControls if your DataFilterComponent works the same in all FilterModes
        /// Overriding to put two controls on one row
        /// </summary>
        /// <param name="entityType">The System Type of the entity to which the filter will be applied.</param>
        /// <param name="filterControl">The control that serves as the container for the controls being rendered.</param>
        /// <param name="writer">The writer being used to generate the HTML for the output page.</param>
        /// <param name="controls">The model representation of the child controls for this component.</param>
        public override void RenderControls( Type entityType, FilterField filterControl, HtmlTextWriter writer, Control[] controls )
        {
            DropDownList ddlHasPhoneOfType = controls[0] as DropDownList;
            DropDownList ddlPhoneNumberType = controls[1] as DropDownList;
            DropDownList ddlHasSMS = controls[2] as DropDownList;

            // Row 1
            writer.AddAttribute( "class", "row field-criteria" );
            writer.RenderBeginTag(HtmlTextWriterTag.Div );

            writer.AddAttribute( "class", "col-md-3" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            ddlHasPhoneOfType.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( "class", "col-md-4" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            ddlPhoneNumberType.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();

            // Row 2
            writer.AddAttribute( "class", "row field-criteria margin-t-sm" );
            writer.RenderBeginTag(HtmlTextWriterTag.Div );

            writer.AddAttribute( "class", "col-md-2" );
            writer.RenderBeginTag(HtmlTextWriterTag.Div );
            ddlHasSMS.RenderControl(writer );
            writer.RenderEndTag();

            writer.RenderEndTag();

            RegisterFilterCompareChangeScript( filterControl );
    }

    /// <summary>
    /// Retruns a pipe delimited string of values PhoneType|HasSMS
    /// </summary>
    /// <param name="entityType"></param>
    /// <param name="controls"></param>
    /// <returns></returns>
    public override string GetSelection( Type entityType, Control[] controls )
        {
            string hasPhoneOfType = ( controls[0] as RockDropDownList ).SelectedValue;
            string phoneType = ( controls[1] as RockDropDownList ).SelectedValue;
            string hasSms = ( controls[2] as RockDropDownList ).SelectedValue;

            return $"{hasPhoneOfType}|{phoneType}|{hasSms}";
        }

        /// <summary>
        /// Sets the filter control values from a formatted string.
        /// Implement this version of SetSelection if your DataFilterComponent supports different FilterModes
        /// </summary>
        /// <param name="entityType">The System Type of the entity to which the filter will be applied.</param>
        /// <param name="controls">The collection of controls used to set the filter values.</param>
        /// <param name="selection">A formatted string representing the filter settings.</param>
        /// <param name="filterMode">The filter mode.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection, FilterMode filterMode )
        {
            if ( controls.Count() >= 3 )
            {
                RockDropDownList ddlHasPhoneOfType = controls[0] as RockDropDownList;
                RockDropDownList ddlPhoneNumberType = controls[1] as RockDropDownList;
                RockDropDownList ddlHasSMS = controls[2] as RockDropDownList;

                string[] selections = selection.Split( '|' );
                if ( selections.Count() >= 3 )
                {
                    ddlHasPhoneOfType.SelectedValue = selections[0];
                    ddlPhoneNumberType.SelectedValue = selections[1];
                    ddlHasSMS.SelectedValue = selections[2];
                }
            }
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
            return string.Format( @"Rock.reporting.formatFilterForHasPhoneFilter($content)" );
        }

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override string FormatSelection( Type entityType, string selection )
        {
            string result = string.Empty;
            string[] selections = selection.Split( '|' );
            if ( selections.Length >= 3 )
            {
                string hasPhoneOfType = selections[0].AsBoolean() ? "Has " : "Doesn't Have ";
                Guid? phoneType = selections[1].AsGuidOrNull();

                string phoneTypeName = phoneType == null ? "Any Phone" : DefinedValueCache.Read( phoneType.Value ).Value + " Phone";
                string hasSMS = string.Empty;

                if ( !string.IsNullOrEmpty( selections[2] ) )
                {
                    if ( selections[2].AsBoolean() )
                    {
                        hasSMS = "and Has SMS Enabled";
                    }
                    else
                    {
                        hasSMS = "and Doesn't Have SMS Enabled";
                    }
                }

                return $"{hasPhoneOfType} {phoneTypeName} {hasSMS}";
            }

            return result;
        }

        /// <summary>
        /// Creates a Linq Expression that can be applied to an IQueryable to filter the result set.
        /// </summary>
        /// <param name="entityType">The type of entity in the result set.</param>
        /// <param name="serviceInstance">A service instance that can be queried to obtain the result set.</param>
        /// <param name="parameterExpression">The input parameter that will be injected into the filter expression.</param>
        /// <param name="selection">A formatted string representing the filter settings.</param>
        /// <returns>
        /// A Linq Expression that can be used to filter an IQueryable.
        /// </returns>
        public override Expression GetExpression( Type entityType, IService serviceInstance, ParameterExpression parameterExpression, string selection )
        {
            string[] selections = selection.Split( '|' );
            bool hasPhoneOfType = selections[0].AsBoolean();
            Guid? phoneTypeGuid = selections[1].AsGuidOrNull();
            int? phoneNumberTypeValueId = phoneTypeGuid.HasValue ? DefinedValueCache.Read( phoneTypeGuid.Value ).Id : ( int? ) null;
            bool? hasSMS = selections[2].AsBooleanOrNull();
            var qry = new PersonService( ( RockContext ) serviceInstance.Context ).Queryable();

            // Filter by PhoneType
            if ( phoneNumberTypeValueId.HasValue )
            {
                // Has the selected PhoneType
                if ( hasPhoneOfType )
                {
                    // Consider SMS for the phone type
                    if ( hasSMS.HasValue )
                    {
                        qry = qry.Where( p => p.PhoneNumbers.Any( n => n.NumberTypeValueId == phoneNumberTypeValueId && n.IsMessagingEnabled == hasSMS.Value ) );
                    }
                    else
                    {
                        // Do not consider SMS for the PhoneType
                        qry = qry.Where( p => p.PhoneNumbers.Any( n => n.NumberTypeValueId == phoneNumberTypeValueId ) );
                    }
                }
                else
                {
                    // Does not have the selected PhoneType
                    // No need to consider SMS
                    qry = qry.Where( p => p.PhoneNumbers.Any( n => n.NumberTypeValueId == phoneNumberTypeValueId ) == false );
                }
            }
            else
            {
                // Do not filter by PhoneType
                // Has any PhoneNumber
                if ( hasPhoneOfType )
                {
                    // Consider SMS for any phone
                    if ( hasSMS.HasValue )
                    {
                        qry = qry.Where( p => p.PhoneNumbers.Any( n => n.IsMessagingEnabled == hasSMS.Value ) );
                    }
                    else
                    {
                        // Do not consider SMS for any phone
                        qry = qry.Where( p => p.PhoneNumbers.Any() );
                    }
                }
                else
                {
                    // Has no PhoneNumber
                    // No need to consider SMS
                    qry = qry.Where( p => p.PhoneNumbers.Any() == false );
                }
            }

            return FilterExpressionExtractor.Extract<Rock.Model.Person>( qry, parameterExpression, "p" );
        }

        #endregion
    }
}
