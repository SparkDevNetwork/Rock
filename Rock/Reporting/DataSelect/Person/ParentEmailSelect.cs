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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI.WebControls;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;
using Expression = System.Linq.Expressions.Expression;

namespace Rock.Reporting.DataSelect.Person
{
    /// <summary>
    /// 
    /// </summary>
    [Description("Select the email addresses of the parents of a person")]
    [Export(typeof(DataSelectComponent))]
    [ExportMetadata("ComponentName", "Select Person's Parent's Email Address")]
    public class ParentsEmailSelect : DataSelectComponent
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
        /// The PropertyName of the property in the anonymous class returned by the SelectExpression
        /// </summary>
        /// <value>
        /// The name of the column property.
        /// </value>
        public override string ColumnPropertyName
        {
            get
            {
                return "ParentEmailAddress";
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
            get { return typeof( IEnumerable<string> ); }
        }

        /// <summary>
        /// Gets the grid field.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override DataControlField GetGridField( Type entityType, string selection )
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
                return "Parent's Email Address";
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
            return "Parent's Email Address";
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
            var adultGuid = SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid();
            var childGuid = SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid();
            var familyGuid = SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid();

            //Contains an array of the int representation of EmailPreferences selected
            int[] selectedPreferences = null;
            if (!string.IsNullOrEmpty(selection))
            {
                selectedPreferences = Array.ConvertAll(selection.Split(','), s => int.Parse(s));
            }

            var familyGroupMembers = new GroupMemberService(context).Queryable()
                .Where(m => m.Group.GroupType.Guid == familyGuid);

            IQueryable<IEnumerable<string>> personParentsEmailQuery;
            //Done as an if statement because:
            // 1) If you try and check selectedPreferences for null in LINQ it throws an exception
            // 2) If all preferences are selected it's quicker to disregard the preference entirely
            if (selectedPreferences == null || selectedPreferences.Length == 3)
            {
                // this returns Enumerable of Email addresses for Parents per row. The Grid then uses ListDelimiterField to convert the list into Parent's Phone Numbers
                personParentsEmailQuery = new PersonService(context).Queryable()
                    .Select(p => familyGroupMembers.Where(s => s.PersonId == p.Id && s.GroupRole.Guid == childGuid)
                        .SelectMany(gm => gm.Group.Members)
                        .Where(m => m.GroupRole.Guid == adultGuid)
                        .Where(m => !string.IsNullOrEmpty(m.Person.Email) && m.Person.IsEmailActive)
                    .Select(q => q.Person.Email).AsEnumerable());
            }
            else
            {
                // this returns Enumerable of Email addresses for Parents per row. The Grid then uses ListDelimiterField to convert the list into Parent's Phone Numbers
                personParentsEmailQuery = new PersonService(context).Queryable()
                    .Select(p => familyGroupMembers.Where(s => s.PersonId == p.Id && s.GroupRole.Guid == childGuid)
                        .SelectMany(gm => gm.Group.Members)
                        .Where(m => m.GroupRole.Guid == adultGuid )
                        .Where(m => selectedPreferences.Contains((int)m.Person.EmailPreference))
                        .Where(m => !string.IsNullOrEmpty(m.Person.Email) && m.Person.IsEmailActive)
                    .Select(q => q.Person.Email).AsEnumerable());
            }

            var selectEmail = SelectExpressionExtractor.Extract( personParentsEmailQuery, entityIdProperty, "p" );            
            
            return selectEmail;
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <param name="parentControl"></param>
        /// <returns></returns>
        public override System.Web.UI.Control[] CreateChildControls( System.Web.UI.Control parentControl )
        {
            RockCheckBoxList emailPreferenceTypeList = new RockCheckBoxList();
            emailPreferenceTypeList.Items.Clear();
            foreach (var preference in Enum.GetValues(typeof(EmailPreference)))
            {
                emailPreferenceTypeList.Items.Add(new ListItem(preference.ToString().SplitCase(), ((int)preference).ToString()));
            }

            emailPreferenceTypeList.ID = parentControl.ID + "_emailPreferenceList";
            emailPreferenceTypeList.Label = "Email Preference";
            emailPreferenceTypeList.Help = "Only include a parent's email address if their email preference is one of these selected values.";
            parentControl.Controls.Add(emailPreferenceTypeList);

            return new System.Web.UI.Control[] { emailPreferenceTypeList };
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( System.Web.UI.Control[] controls )
        {
            if (controls.Count() == 1)
            {
                RockCheckBoxList checkBoxList = controls[0] as RockCheckBoxList;
                if (checkBoxList != null)
                {
                    return string.Join(",", checkBoxList.SelectedValues);
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
            if (controls.Count() == 1)
            {
                RockCheckBoxList checkBoxList = controls[0] as RockCheckBoxList;
                if (checkBoxList != null && !string.IsNullOrEmpty(selection))
                {
                    string[] values = selection.Split(',');
                    checkBoxList.SetValues(values);
                }
            }
        }

        #endregion
    }
}
