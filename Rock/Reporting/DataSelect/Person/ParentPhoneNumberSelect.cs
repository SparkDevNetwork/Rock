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
using System.Web.UI.WebControls;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataSelect.Person
{
    /// <summary>
    /// 
    /// </summary>
    [Description("Select the phone numbers of the parents of a person")]
    [Export(typeof(DataSelectComponent))]
    [ExportMetadata("ComponentName", "Select Person's Parent's Phone Number")]
    public class ParentsPhoneNumberSelect : DataSelectComponent
    {
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
                return typeof(Rock.Model.Person).FullName;
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
                return "ParentPhoneNumber";
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
            get { return typeof( IEnumerable<Rock.Model.PhoneNumber> ); }
        }

        /// <summary>
        /// Gets the grid field.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override System.Web.UI.WebControls.DataControlField GetGridField( Type entityType, string selection )
        {
            return new ListDelimitedField();
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
                return "Parent's Phone Number";
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
            return "Parent's Phone Number";
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
            Guid adultGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid();
            Guid childGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid();
            Guid familyGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid();
            Guid? phoneNumberTypeValueGuid = selection.AsGuidOrNull();
            if ( !phoneNumberTypeValueGuid.HasValue )
            {
                phoneNumberTypeValueGuid = Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid();
            }

            int phoneNumberTypeValueId = DefinedValueCache.Read(phoneNumberTypeValueGuid.Value).Id;

            var familyGroupMembers = new GroupMemberService(context).Queryable()
                .Where(m => m.Group.GroupType.Guid == familyGuid);

            // this returns Enumerable of PhoneNumber for Parents per row. The Grid then uses ListDelimiterField to convert the list into Parent's Phone Numbers
            var personParentsPhoneQuery = new PersonService( context ).Queryable()
                .Select( p => familyGroupMembers.Where( s => s.PersonId == p.Id && s.GroupRole.Guid == childGuid )
                    .SelectMany( m => m.Group.Members )
                    .Where( m => m.GroupRole.Guid == adultGuid )
                    .Select( m => m.Person )
                    .Where( m => m.PhoneNumbers.Count( t => t.NumberTypeValueId == phoneNumberTypeValueId ) != 0 )
                    .Select( m => m.PhoneNumbers.FirstOrDefault( t => t.NumberTypeValueId == phoneNumberTypeValueId ) ).AsEnumerable() );

            var selectNumbersExpression = SelectExpressionExtractor.Extract<Rock.Model.Person>( personParentsPhoneQuery, entityIdProperty, "p" );            
            
            return selectNumbersExpression;
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
            foreach ( var value in DefinedTypeCache.Read(Rock.SystemGuid.DefinedType.PERSON_PHONE_TYPE.AsGuid()).DefinedValues.OrderBy(a => a.Order).ThenBy(a => a.Value) )
            {
                phoneNumberTypeList.Items.Add(new ListItem(value.Value.EndsWith("Phone") ? value.Value : value.Value + " Phone", value.Guid.ToString()));
            }

            phoneNumberTypeList.ID = parentControl.ID + "_phoneTypeList";
            phoneNumberTypeList.Label = "Phone Type";
            parentControl.Controls.Add(phoneNumberTypeList);

            return new System.Web.UI.Control[] { phoneNumberTypeList };
        }

        /// <summary>
        /// Renders the controls.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="controls">The controls.</param>
        public override void RenderControls( System.Web.UI.Control parentControl, System.Web.UI.HtmlTextWriter writer, System.Web.UI.Control[] controls )
        {
            base.RenderControls(parentControl, writer, controls);
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( System.Web.UI.Control[] controls )
        {
            if ( controls.Count() == 1 )
            {
                RockDropDownList dropDownList = controls[0] as RockDropDownList;
                if ( dropDownList != null )
                {
                    return dropDownList.SelectedValue;
                }
            }

            return null;
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( System.Web.UI.Control[] controls, string selection )
        {
            if ( controls.Count() == 1 )
            {
                RockDropDownList dropDownList = controls[0] as RockDropDownList;
                if ( dropDownList != null )
                {
                    dropDownList.SetValue(selection);
                }
            }
        }

        #endregion
    }
}
