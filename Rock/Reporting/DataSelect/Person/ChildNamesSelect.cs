﻿// <copyright>
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

using Rock.Data;
using Rock.Model;
using Rock.Net;
using Rock.ViewModels.Controls;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataSelect.Person
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Select the names of the Person's Children" )]
    [Export( typeof( DataSelectComponent ) )]
    [ExportMetadata( "ComponentName", "Select Person's Children's Names" )]
    [Rock.SystemGuid.EntityTypeGuid( "8DE06919-D68C-4A29-989B-359A0379F1F3" )]
    public class ChildNamesSelect : DataSelectComponent, IRecipientDataSelect
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
            get { return typeof( IEnumerable<KidInfo> ); }
        }

        /// <summary>
        /// Comma-delimited list of the Entity properties that should be used for Sorting. Normally, you should leave this as null which will make it sort on the returned field
        /// To disable sorting for this field, return string.Empty;
        /// </summary>
        /// <param name="selection"></param>
        /// <returns></returns>
        /// <value>
        /// The sort expression.
        /// </value>
        public override string SortProperties( string selection )
        {
            // disable sorting on this column since it is an IEnumerable
            return string.Empty;
        }

        /// <summary>
        /// little class so that we only need to fetch the columns that we need from Person
        /// </summary>
        private class KidInfo
        {
            public string NickName { get; set; }

            public string LastName { get; set; }

            public int? SuffixValueId { get; set; }

            public Gender Gender { get; set; }

            public DateTime? BirthDate { get; set; }

            public DateTime? DeceasedDate { get; set; }

            public int? GraduationYear { get; set; }
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
            bool includeGrade = selectionParts.Length > 2 && selectionParts[2].AsBoolean();

            callbackField.OnFormatDataValue += ( sender, e ) =>
            {
                var personList = e.DataValue as IEnumerable<KidInfo>;
                if ( personList != null )
                {
                    var formattedList = new List<string>();
                    foreach ( var person in personList )
                    {
                        var formattedPerson = Rock.Model.Person.FormatFullName( person.NickName, person.LastName, person.SuffixValueId );
                        var formattedGenderAgeGrade = string.Empty;

                        if ( includeGender && person.Gender != Gender.Unknown )
                        {
                            formattedGenderAgeGrade = person.Gender == Gender.Female ? "F" : "M";
                        }

                        int? age = Rock.Model.Person.GetAge( person.BirthDate, person.DeceasedDate );

                        if ( includeAge && age.HasValue )
                        {
                            formattedGenderAgeGrade += " " + age.Value.ToString();
                        }

                        string grade = Rock.Model.Person.GradeAbbreviationFromGraduationYear( person.GraduationYear );

                        if ( includeGrade && person.GraduationYear.HasValue )
                        {
                            formattedGenderAgeGrade += " " + grade.ToString();
                        }

                        if ( !string.IsNullOrWhiteSpace( formattedGenderAgeGrade ) )
                        {
                            formattedPerson += " (" + formattedGenderAgeGrade.Trim() + ")";
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

        #region Configuration

        /// <inheritdoc/>
        public override DynamicComponentDefinitionBag GetComponentDefinition( Type entityType, string selection, RockContext rockContext, RockRequestContext requestContext )
        {
            return new DynamicComponentDefinitionBag
            {
                Url = requestContext.ResolveRockUrl( "~/Obsidian/Reporting/DataSelects/Person/childNamesSelect.obs" ),
            };
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetObsidianComponentData( Type entityType, string selection, RockContext rockContext, RockRequestContext requestContext )
        {
            string[] selectionValues = selection.Split( '|' );

            if ( selectionValues.Length >= 3 )
            {
                return new Dictionary<string, string>
                {
                    ["includeGender"] = selectionValues[0],
                    ["includeAge"] = selectionValues[1],
                    ["includeGrade"] = selectionValues[2]
                };
            }

            return new Dictionary<string, string>();
        }

        /// <inheritdoc/>
        public override string GetSelectionFromObsidianComponentData( Type entityType, Dictionary<string, string> data, RockContext rockContext, RockRequestContext requestContext )
        {
            var includeGender = data.GetValueOrDefault( "includeGender", "False" );
            var includeAge = data.GetValueOrDefault( "includeAge", "False" );
            var includeGrade = data.GetValueOrDefault( "includeGrade", "False" );

            return $"{includeGender}|{includeAge}|{includeGrade}";
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

            // this returns Enumerable of KidInfo per row. See this.GetGridField to see how it is processed 
            var personChildrenQuery = new PersonService( context ).Queryable()
                .Select( p => familyGroupMembers.Where( s => s.PersonId == p.Id && s.GroupRole.Guid == adultGuid )
                    .SelectMany( m => m.Group.Members )
                    .Where( m => m.GroupRole.Guid == childGuid )
                    .OrderBy( m => m.Group.Members.FirstOrDefault( x => x.PersonId == p.Id ).GroupOrder ?? int.MaxValue )
                    .ThenBy( m => m.Person.BirthYear ).ThenBy( m => m.Person.BirthMonth ).ThenBy( m => m.Person.BirthDay )
                    .Select( m => new KidInfo
                    {
                        NickName = m.Person.NickName,
                        LastName = m.Person.LastName,
                        SuffixValueId = m.Person.SuffixValueId,
                        Gender = m.Person.Gender,
                        BirthDate = m.Person.BirthDate,
                        DeceasedDate = m.Person.DeceasedDate,
                        GraduationYear = m.Person.GraduationYear
                    } ).AsEnumerable() );

            var selectChildrenExpression = SelectExpressionExtractor.Extract( personChildrenQuery, entityIdProperty, "p" );

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

            RockCheckBox cbIncludeGrade = new RockCheckBox();
            cbIncludeGrade.ID = parentControl.ID + "_cbIncludeGrade";
            cbIncludeGrade.Text = "Include Grade";
            parentControl.Controls.Add( cbIncludeGrade );

            return new System.Web.UI.Control[] { cbIncludeGender, cbIncludeAge, cbIncludeGrade };
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
            if ( controls.Count() == 3 )
            {
                RockCheckBox cbIncludeGender = controls[0] as RockCheckBox;
                RockCheckBox cbIncludeAge = controls[1] as RockCheckBox;
                RockCheckBox cbIncludeGrade = controls[2] as RockCheckBox;
                if ( cbIncludeGender != null && cbIncludeAge != null && cbIncludeGrade != null )
                {
                    return string.Format( "{0}|{1}|{2}", cbIncludeGender.Checked.ToTrueFalse(), cbIncludeAge.Checked.ToTrueFalse(), cbIncludeGrade.Checked.ToTrueFalse() );
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
            if ( controls.Count() == 3 )
            {
                string[] selectionValues = selection.Split( '|' );
                if ( selectionValues.Length >= 3 )
                {
                    RockCheckBox cbIncludeGender = controls[0] as RockCheckBox;
                    RockCheckBox cbIncludeAge = controls[1] as RockCheckBox;
                    RockCheckBox cbIncludeGrade = controls[2] as RockCheckBox;
                    if ( cbIncludeGender != null && cbIncludeAge != null && cbIncludeGrade != null )
                    {
                        cbIncludeGender.Checked = selectionValues[0].AsBoolean();
                        cbIncludeAge.Checked = selectionValues[1].AsBoolean();
                        cbIncludeGrade.Checked = selectionValues[2].AsBoolean();
                    }
                }
            }
        }

        #endregion

        #region IRecipientDataSelect implementation

        /// <summary>
        /// Gets the type of the recipient column field.
        /// </summary>
        /// <value>
        /// The type of the recipient column field.
        /// </value>
        public Type RecipientColumnFieldType
        {
            get { return typeof( IEnumerable<int> ); }
        }

        /// <summary>
        /// Gets the recipient person identifier expression.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="entityIdProperty">The entity identifier property.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public Expression GetRecipientPersonIdExpression( System.Data.Entity.DbContext dbContext, MemberExpression entityIdProperty, string selection )
        {
            var rockContext = dbContext as RockContext;
            if ( rockContext != null )
            {
                Guid adultGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid();
                Guid childGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid();
                Guid familyGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid();

                var familyGroupMembers = new GroupMemberService( rockContext ).Queryable()
                    .Where( m => m.Group.GroupType.Guid == familyGuid );

                // this returns Enumerable of KidInfo per row. See this.GetGridField to see how it is processed 
                var personChildrenQuery = new PersonService( rockContext ).Queryable()
                    .Select( p => familyGroupMembers.Where( s => s.PersonId == p.Id && s.GroupRole.Guid == adultGuid )
                        .SelectMany( m => m.Group.Members )
                        .Where( m => m.GroupRole.Guid == childGuid )
                        .OrderBy( m => m.Group.Members.FirstOrDefault( x => x.PersonId == p.Id ).GroupOrder ?? int.MaxValue )
                        .ThenBy( m => m.Person.BirthYear ).ThenBy( m => m.Person.BirthMonth ).ThenBy( m => m.Person.BirthDay )
                        .Select( m => m.Person.Id ).AsEnumerable() );

                var selectChildrenExpression = SelectExpressionExtractor.Extract( personChildrenQuery, entityIdProperty, "p" );

                return selectChildrenExpression;
            }

            return null;
        }

        #endregion
    }
}
