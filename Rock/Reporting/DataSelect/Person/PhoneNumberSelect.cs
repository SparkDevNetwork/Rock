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
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.UI.WebControls;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataSelect.Person
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Select a Phone Number of the Person" )]
    [Export( typeof( DataSelectComponent ) )]
    [ExportMetadata( "ComponentName", "Select Person's Phone Number" )]
    public class PhoneNumberSelect : DataSelectComponent
    {
        #region
        Rock.Model.Person _currentPerson = null;
        #endregion

        #region Properties

        /// <summary>
        /// Gets the name of the entity type. Filter should be an empty string
        /// if it applies to all entities
        /// </summary>
        /// <value>
        /// The name of the entity type.
        /// </value>
        public override string AppliesToEntityType
        {
            get
            {
                return typeof( Rock.Model.Person ).FullName;
            }
        }

        /// <summary>
        /// Gets the section that this will appear in in the Field Selector
        /// </summary>
        /// <value>
        /// The section.
        /// </value>
        public override string Section
        {
            get
            {
                return base.Section;
            }
        }

        /// <summary>
        /// The PropertyName of the property in the anonymous class returned by the SelectExpression
        /// </summary>
        /// <value>
        /// The name of the column property.
        /// </value>
        public override string ColumnPropertyName
        {
            get
            {
                return "PhoneNumber";
            }
        }

        /// <summary>
        /// Gets the type of the column field.
        /// </summary>
        /// <value>
        /// The type of the column field.
        /// </value>
        public override Type ColumnFieldType
        {
            get { return typeof( Rock.Model.PhoneNumber ); }
        }

        /// <summary>
        /// Gets the default column header text.
        /// </summary>
        /// <value>
        /// The default column header text.
        /// </value>
        public override string ColumnHeaderText
        {
            get
            {
                return "Phone Number";
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        /// <value>
        /// The title.
        /// </value>
        public override string GetTitle( Type entityType )
        {
            return "Phone Number";
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="entityIdProperty">The entity identifier property.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override Expression GetExpression( RockContext context, MemberExpression entityIdProperty, string selection )
        {
            string[] selectionValues = selection.Split( '|' );
            Guid? phoneNumberTypeValueGuid = null;
            if ( selectionValues.Length >= 1)
            {
                phoneNumberTypeValueGuid = selectionValues[0].AsGuidOrNull();
            }

            if ( !phoneNumberTypeValueGuid.HasValue )
            {
                phoneNumberTypeValueGuid = Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid();
            }

            int phoneNumberTypeValueId = DefinedValueCache.Get( phoneNumberTypeValueGuid.Value ).Id;

            // NOTE: This actually selects the entire PhoneNumber record instead of just one field. This is done intentionally so that the Grid will call the .ToString() method of PhoneNumber which formats it correctly
            var personPhoneNumberQuery = new PersonService( context ).Queryable()
                .Select( p => p.PhoneNumbers.FirstOrDefault( a => a.NumberTypeValueId == phoneNumberTypeValueId ) );

            var selectExpression = SelectExpressionExtractor.Extract( personPhoneNumberQuery, entityIdProperty, "p" );

            return selectExpression;
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <param name="parentControl"></param>
        /// <returns></returns>
        public override System.Web.UI.Control[] CreateChildControls( System.Web.UI.Control parentControl )
        {
            RockDropDownList phoneNumberTypeList = new RockDropDownList();
            phoneNumberTypeList.Items.Clear();
            foreach (var value in DefinedTypeCache.Get(Rock.SystemGuid.DefinedType.PERSON_PHONE_TYPE.AsGuid()).DefinedValues.OrderBy( a => a.Order).ThenBy(a => a.Value))
            {
                phoneNumberTypeList.Items.Add( new ListItem( value.Value.EndsWith( "Phone" ) ? value.Value : value.Value + " Phone", value.Guid.ToString() ) );
            }

            phoneNumberTypeList.ID = parentControl.ID + "_phoneTypeList";
            phoneNumberTypeList.Label = "Phone Type";
            parentControl.Controls.Add( phoneNumberTypeList );

            // show the click to call link
            var pbxComponent = Rock.Pbx.PbxContainer.GetActiveComponentWithOriginationSupport();

            RockCheckBox enableCallOrigination = new RockCheckBox();
            enableCallOrigination.Help = "Determines if the phone numbers should be linked to enable click-to-call.";
            enableCallOrigination.Label = "Enable Call Origination";

            if ( pbxComponent == null )
            {
                enableCallOrigination.Visible = false;
            }

            parentControl.Controls.Add( enableCallOrigination );

            return new System.Web.UI.Control[] { phoneNumberTypeList, enableCallOrigination };
        }

        /// <summary>
        /// Renders the controls.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="controls">The controls.</param>
        public override void RenderControls( System.Web.UI.Control parentControl, System.Web.UI.HtmlTextWriter writer, System.Web.UI.Control[] controls )
        {
            base.RenderControls( parentControl, writer, controls );
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( System.Web.UI.Control[] controls )
        {
            RockDropDownList dropDownList = controls[0] as RockDropDownList;
            RockCheckBox enableCallOrigination = controls[1] as RockCheckBox;
            return string.Format( "{0}|{1}", dropDownList.SelectedValue, enableCallOrigination.Checked );
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( System.Web.UI.Control[] controls, string selection )
        {
            RockDropDownList dropDownList = controls[0] as RockDropDownList;
            RockCheckBox enableCallOrigination = controls[1] as RockCheckBox;
            string[] values = selection.Split( '|' );

            if ( values.Length >= 1 )
            {
                dropDownList.SetValue( values[0] );
            }

            if ( values.Length >= 2 )
            {
                enableCallOrigination.Checked = values[1].AsBoolean();
            }
        }

        /// <summary>
        /// Gets the grid field.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override System.Web.UI.WebControls.DataControlField GetGridField( Type entityType, string selection )
        {
            Guid? phoneType = null;
            bool enableOrigination = false;

            if ( _currentPerson == null )
            {
                if ( HttpContext.Current != null && HttpContext.Current.Items.Contains( "CurrentPerson" ) )
                {
                    _currentPerson = HttpContext.Current.Items["CurrentPerson"] as Rock.Model.Person;
                }
            }

            var selectionParts = selection.Split( '|' );
            if ( selectionParts.Length > 0 )
            {
                phoneType =  selectionParts[0].AsGuidOrNull();
            }

            if ( selectionParts.Length > 1 )
            {
                enableOrigination = selectionParts[1].AsBoolean();
            }

            var callbackField = new CallbackField();
            callbackField.OnFormatDataValue += ( sender, e ) =>
            {
                var phoneNumber = e.DataValue as PhoneNumber;
                if ( phoneNumber != null )
                {
                    if ( enableOrigination )
                    {
                        if ( _currentPerson == null )
                        {
                            e.FormattedValue = ( phoneNumber.NumberFormatted != null ) ? phoneNumber.NumberFormatted : phoneNumber.Number;
                            return;
                        }

                        var jsScript = string.Format( "javascript: Rock.controls.pbx.originate('{0}', '{1}', '{2}','{3}','{4}');", _currentPerson.Guid, phoneNumber.Number, _currentPerson.FullName, "", phoneNumber.ToString() );

                        e.FormattedValue = string.Format( "<a class='originate-call js-originate-call' href=\"{0}\">{1}</a>", jsScript, ( phoneNumber.NumberFormatted != null ) ? phoneNumber.NumberFormatted : phoneNumber.Number );
                    }
                    else
                    {
                        e.FormattedValue = ( phoneNumber.NumberFormatted != null ) ? phoneNumber.NumberFormatted : phoneNumber.Number;
                    }
                }
                else
                {
                    e.FormattedValue = string.Empty;
                    return;
                }
            };

            return callbackField;
        }

        #endregion
    }
}
