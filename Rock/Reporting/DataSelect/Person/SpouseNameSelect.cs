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

using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataSelect.Person
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Select the name of the Person's Spouse" )]
    [Export( typeof( DataSelectComponent ) )]
    [ExportMetadata( "ComponentName", "Select Person's Spouse's Name" )]
    [Rock.SystemGuid.EntityTypeGuid( "E8B3A661-5C6D-4B24-A09A-4D3ED080CE0C" )]
    public class SpouseNameSelect : DataSelectComponent, IRecipientDataSelect
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
                return "SpouseName";
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
            get { return typeof( string ); }
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
                return "Spouse Name";
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
            return "Spouse Name";
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
            //// Spouse is determined if all these conditions are met
            //// 1) Adult in the same family as Person (GroupType = Family, GroupRole = Adult, and in same Group)
            //// 2) Opposite Gender as Person, if the Bible Strict Couple is not selected.
            //// 3) Both Persons are Married

            var selectionParts = selection.Split( '|' );
            bool includeLastname = selectionParts.Length > 0 && selectionParts[0].AsBoolean();
            var isBibleStrict = SystemSettings.GetValue( Rock.SystemKey.SystemSetting.BIBLE_STRICT_SPOUSE ).AsBoolean( true );

            Guid adultGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid();
            Guid marriedGuid = Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED.AsGuid();
            int marriedDefinedValueId = DefinedValueCache.Get( marriedGuid ).Id;
            Guid familyGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid();

            var familyGroupMembers = new GroupMemberService( context ).Queryable()
                .Where( m => m.Group.GroupType.Guid == familyGuid );

            var personSpouseQuery = new PersonService( context ).Queryable()
                .Select( p => familyGroupMembers.Where( s => s.PersonId == p.Id && s.Person.MaritalStatusValueId == marriedDefinedValueId && s.GroupRole.Guid == adultGuid )
                    .SelectMany( m => m.Group.Members )
                    .Where( m =>
                        m.PersonId != p.Id &&
                        m.GroupRole.Guid == adultGuid &&
                        ( !isBibleStrict || m.Person.Gender != p.Gender ) &&
                        m.Person.MaritalStatusValueId == marriedDefinedValueId &&
                        !m.Person.IsDeceased )
                    .OrderBy( m => m.Group.Members.FirstOrDefault( x => x.PersonId == p.Id ).GroupOrder ?? int.MaxValue )
                    .Select( m => includeLastname ? m.Person.NickName + " " + m.Person.LastName : m.Person.NickName ).FirstOrDefault() );

            var selectSpouseExpression = SelectExpressionExtractor.Extract( personSpouseQuery, entityIdProperty, "p" );

            return selectSpouseExpression;
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <param name="parentControl"></param>
        /// <returns></returns>
        public override System.Web.UI.Control[] CreateChildControls( System.Web.UI.Control parentControl )
        {
            RockCheckBox cbIncludeLastname = new RockCheckBox();
            cbIncludeLastname.ID = parentControl.ID + "_cbIncludeLastname";
            cbIncludeLastname.Text = "Include Lastname";
            parentControl.Controls.Add( cbIncludeLastname );

            return new System.Web.UI.Control[] { cbIncludeLastname };
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
            if ( controls.Count() == 1 )
            {
                RockCheckBox cbIncludeLastname = controls[0] as RockCheckBox;
                if ( cbIncludeLastname != null )
                {
                    return cbIncludeLastname.Checked.ToTrueFalse();
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
            if ( controls.Count() == 1 )
            {
                string[] selectionValues = selection.Split( '|' );
                if ( selectionValues.Length >= 1 )
                {
                    RockCheckBox cbIncludeLastname = controls[0] as RockCheckBox;

                    if ( cbIncludeLastname != null )
                    {
                        cbIncludeLastname.Checked = selectionValues[0].AsBoolean();
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
            get { return typeof( int ); }
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
                Guid marriedGuid = Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED.AsGuid();
                int marriedDefinedValueId = DefinedValueCache.Get( marriedGuid ).Id;
                Guid familyGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid();
                var isBibleStrict = SystemSettings.GetValue( Rock.SystemKey.SystemSetting.BIBLE_STRICT_SPOUSE ).AsBoolean( true );

                var familyGroupMembers = new GroupMemberService( rockContext ).Queryable()
                    .Where( m => m.Group.GroupType.Guid == familyGuid );

                var personSpouseQuery = new PersonService( rockContext ).Queryable()
                    .Select( p => familyGroupMembers.Where( s => s.PersonId == p.Id && s.Person.MaritalStatusValueId == marriedDefinedValueId && s.GroupRole.Guid == adultGuid )
                        .SelectMany( m => m.Group.Members )
                        .Where( m =>
                            m.PersonId != p.Id &&
                            m.GroupRole.Guid == adultGuid &&
                             ( !isBibleStrict || m.Person.Gender != p.Gender ) &&
                            m.Person.MaritalStatusValueId == marriedDefinedValueId &&
                            !m.Person.IsDeceased )
                        .OrderBy( m => m.Group.Members.FirstOrDefault( x => x.PersonId == p.Id ).GroupOrder ?? int.MaxValue )
                        .Select( m => m.Person.Id ).FirstOrDefault() );

                var selectSpouseExpression = SelectExpressionExtractor.Extract( personSpouseQuery, entityIdProperty, "p" );

                return selectSpouseExpression;
            }

            return null;
        }

        #endregion

    }
}
