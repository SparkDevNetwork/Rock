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
using System.Linq;
using System.Linq.Expressions;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataSelect.Person
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Select the names of the Person's Children" )]
    [Export( typeof( DataSelectComponent ) )]
    [ExportMetadata( "ComponentName", "Select Person's Children's Names" )]
    public class ChildNamesSelect : DataSelectComponent
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
                return "ChildNames";
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
            get { return typeof( IEnumerable<Rock.Model.Person> ); }
        }

        /// <summary>
        /// Gets the grid field.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override System.Web.UI.WebControls.DataControlField GetGridField( Type entityType, string selection )
        {
            var callbackField = new CallbackField();
            var selectionParts = selection.Split( '|' );
            bool includeGender = selectionParts.Length > 0 && selectionParts[0].AsBoolean();
            bool includeAge = selectionParts.Length > 1 && selectionParts[1].AsBoolean();
            callbackField.OnFormatDataValue += ( sender, e ) =>
            {
                var personList = e.DataValue as IEnumerable<Rock.Model.Person>;
                if ( personList != null )
                {
                    var formattedList = new List<string>();
                    foreach ( var person in personList )
                    {
                        var formattedPerson = person.FullName;
                        var formattedGenderAge = string.Empty;

                        if ( includeGender && person.Gender != Gender.Unknown )
                        {
                            // return F for Female, M for Male
                            if ( person.Gender == Gender.Female )
                            {
                                formattedGenderAge += "F";
                            }
                            else if ( person.Gender == Gender.Male )
                            {
                                formattedGenderAge += "M";
                            }
                        }

                        if ( includeAge && person.Age.HasValue )
                        {
                            formattedGenderAge += " " + person.Age.Value.ToString();
                        }

                        if ( !string.IsNullOrWhiteSpace( formattedGenderAge ) )
                        {
                            formattedPerson += " (" + formattedGenderAge.Trim() + ")";
                        }

                        formattedList.Add( formattedPerson );
                    }

                    e.FormattedValue = formattedList.AsDelimited( ", " );
                }
                else
                {
                    e.FormattedValue = string.Empty;
                }
            };

            return callbackField;
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
                return "Children's Names";
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
            return "Children's Names";
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

            var familyGroupMembers = new GroupMemberService( context ).Queryable()
                .Where( m => m.Group.GroupType.Guid == familyGuid );

            // this returns Enumerable of Person for Children per row. The Grid then uses ListDelimiterField to convert the list into Children's Names
            // sorted from oldest to youngest
            var personChildrenQuery = new PersonService( context ).Queryable()
                .Select( p => familyGroupMembers.Where( s => s.PersonId == p.Id && s.GroupRole.Guid == adultGuid )
                    .SelectMany( m => m.Group.Members )
                    .Where( m => m.GroupRole.Guid == childGuid )
                    .OrderBy( m => m.Person.BirthYear ).ThenBy( m => m.Person.BirthMonth ).ThenBy( m => m.Person.BirthDay )
                    .Select( m => m.Person ).AsEnumerable() );

            var selectChildrenExpression = SelectExpressionExtractor.Extract<Rock.Model.Person>( personChildrenQuery, entityIdProperty, "p" );

            return selectChildrenExpression;
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <param name="parentControl"></param>
        /// <returns></returns>
        public override System.Web.UI.Control[] CreateChildControls( System.Web.UI.Control parentControl )
        {
            RockCheckBox cbIncludeGender = new RockCheckBox();
            cbIncludeGender.ID = parentControl.ID + "_cbIncludeGender";
            cbIncludeGender.Text = "Include Gender";
            parentControl.Controls.Add( cbIncludeGender );

            RockCheckBox cbIncludeAge = new RockCheckBox();
            cbIncludeAge.ID = parentControl.ID + "_cbIncludeAge";
            cbIncludeAge.Text = "Include Age";
            parentControl.Controls.Add( cbIncludeAge );

            return new System.Web.UI.Control[] { cbIncludeGender, cbIncludeAge };
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
            if ( controls.Count() == 2 )
            {
                RockCheckBox cbIncludeGender = controls[0] as RockCheckBox;
                RockCheckBox cbIncludeAge = controls[1] as RockCheckBox;
                if ( cbIncludeGender != null && cbIncludeAge != null )
                {
                    return string.Format( "{0}|{1}", cbIncludeGender.Checked.ToTrueFalse(), cbIncludeAge.Checked.ToTrueFalse() );
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( System.Web.UI.Control[] controls, string selection )
        {
            if ( controls.Count() == 2 )
            {
                string[] selectionValues = selection.Split( '|' );
                if ( selectionValues.Length >= 2 )
                {
                    RockCheckBox cbIncludeGender = controls[0] as RockCheckBox;
                    RockCheckBox cbIncludeAge = controls[1] as RockCheckBox;
                    if ( cbIncludeGender != null && cbIncludeAge != null )
                    {
                        cbIncludeGender.Checked = selectionValues[0].AsBoolean();
                        cbIncludeAge.Checked = selectionValues[1].AsBoolean();
                    }
                }
            }
        }

        #endregion
    }
}
