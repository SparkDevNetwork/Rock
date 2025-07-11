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
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;
using Rock.Net;
using Rock.SystemGuid;
using Rock.Utility;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Web.Utilities;

namespace Rock.Reporting.DataSelect.Person
{
    /// <summary>
    ///     A Report Field that shows a list of people who have a specified type of relationship with the principal person.
    /// </summary>
    [Description( "Shows a list of people who have a specified type of relationship with a person." )]
    [Export( typeof( DataSelectComponent ) )]
    [ExportMetadata( "ComponentName", "Related People" )]
    [EntityTypeGuid( "7D2CB16A-9391-4D5A-A4B4-4DA34FD3E96E" )]
    public class RelatedPeopleSelect : DataSelectComponent, IRecipientDataSelect
    {
        #region Support Classes

        private class RelatedPersonInfo
        {
            /// <summary>
            /// Gets or sets the first name.
            /// </summary>
            /// <value>
            /// The first name.
            /// </value>
            public string FirstName { get; set; }

            /// <summary>
            /// Gets or sets the last name.
            /// </summary>
            /// <value>
            /// The last name.
            /// </value>
            public string LastName { get; set; }

            /// <summary>
            /// Gets or sets the person identifier.
            /// </summary>
            /// <value>
            /// The person identifier.
            /// </value>
            public int PersonId { get; set; }

            /// <summary>
            /// Gets or sets the related to person identifier.
            /// </summary>
            /// <value>
            /// The related to person identifier.
            /// </value>
            public int RelatedToPersonId { get; set; }

            /// <summary>
            /// Gets or sets the name of the relationship.
            /// </summary>
            /// <value>
            /// The name of the relationship.
            /// </value>
            public string RelationshipName { get; set; }

            /// <summary>
            /// Gets or sets the sort order.
            /// </summary>
            /// <value>
            /// The sort order.
            /// </value>
            public int SortOrder { get; set; }

            /// <summary>
            /// Gets or sets the suffix.
            /// </summary>
            /// <value>
            /// The suffix.
            /// </value>
            public string Suffix { get; set; }

            public override string ToString()
            {
                var description = new StringBuilder();

                description.AppendFormat( "{0} {1}", FirstName, LastName );

                if ( !string.IsNullOrWhiteSpace( Suffix ) )
                {
                    description.AppendFormat( " {0}", Suffix );
                }

                if ( !string.IsNullOrWhiteSpace( RelationshipName ) )
                {
                    description.AppendFormat( " [{0}]", RelationshipName );
                }

                return description.ToString();
            }
        }

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
            get { return typeof( Model.Person ).FullName; }
        }

        /// <summary>
        /// Gets the section that this will appear in in the Field Selector
        /// </summary>
        /// <value>
        /// The section.
        /// </value>
        public override string Section
        {
            get { return "Relationships"; }
        }

        /// <summary>
        /// The PropertyName of the property in the anonymous class returned by the SelectExpression
        /// </summary>
        /// <value>
        /// The name of the column property.
        /// </value>
        public override string ColumnPropertyName
        {
            get { return "Related People"; }
        }

        /// <summary>
        /// Gets the type of the column field.
        /// </summary>
        /// <value>
        /// The type of the column field.
        /// </value>
        public override Type ColumnFieldType
        {
            get { return typeof( IEnumerable<RelatedPersonInfo> ); }
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
            get { return "Related People"; }
        }

        #endregion

        #region Configuration

        /// <inheritdoc/>
        public override DynamicComponentDefinitionBag GetComponentDefinition( Type entityType, string selection, RockContext rockContext, RockRequestContext requestContext )
        {
            var familyRelationshipOptions = new List<ListItemBag>
            {
                new ListItemBag{ Text = "Parent", Value = FamilyRelationshipParentGuid },
                new ListItemBag{ Text = "Child", Value = FamilyRelationshipChildGuid },
                new ListItemBag{ Text = "Sibling", Value = FamilyRelationshipSiblingGuid },
                new ListItemBag{ Text = "Spouse", Value = FamilyRelationshipSpouseGuid },
            };

            var groupType = GroupTypeCache.Get( SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS.AsGuid(), rockContext );
            var knownRelationshipOptions = new List<ListItemBag>();

            if ( groupType != null )
            {
                // Exclude the Owner Role from the list of selectable Roles because a Person cannot be related to themselves.
                var ownerGuid = GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER.AsGuid();

                knownRelationshipOptions = new GroupTypeRoleService( rockContext )
                    .GetByGroupTypeId( groupType.Id )
                    .Where( r => r.Guid != ownerGuid )
                    .Select( r => new ListItemBag { Text = r.Name, Value = r.Guid.ToString() } )
                    .ToList();
            }

            var listFormatOptions = new List<ListItemBag> {
                new ListItemBag { Text = "Person Name and Relationship", Value = ListFormatSpecifier.NameAndRelationship.ToString() },
                new ListItemBag { Text = "Person Name", Value = ListFormatSpecifier.NameOnly.ToString() },
            };

            return new DynamicComponentDefinitionBag
            {
                Url = requestContext.ResolveRockUrl( "~/Obsidian/Reporting/DataSelects/Person/relatedPeopleSelect.obs" ),
                Options = new Dictionary<string, string>
                {
                    ["familyRelationshipOptions"] = familyRelationshipOptions.ToCamelCaseJson( false, true ),
                    ["knownRelationshipOptions"] = knownRelationshipOptions.ToCamelCaseJson( false, true ),
                    ["listFormatOptions"] = listFormatOptions.ToCamelCaseJson( false, true ),
                }
            };
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetObsidianComponentData( Type entityType, string selection, RockContext rockContext, RockRequestContext requestContext )
        {
            var data = new Dictionary<string, string>();
            var settings = new RelatedPeopleSelectSettings( selection );

            data.Add( "familyRelationships", settings.FamilyRelationshipTypeGuids.Select( g => g.ToString() ).ToList().ToJson() );
            data.Add( "knownRelationships", settings.KnownRelationshipTypeGuids.Select( g => g.ToString() ).ToList().ToJson() );
            data.Add( "listFormat", settings.ListFormat.ToString() );

            return data;
        }

        /// <inheritdoc/>
        public override string GetSelectionFromObsidianComponentData( Type entityType, Dictionary<string, string> data, RockContext rockContext, RockRequestContext requestContext )
        {
            var settings = new RelatedPeopleSelectSettings();

            settings.FamilyRelationshipTypeGuids.AddRange( data.GetValueOrDefault( "familyRelationships", "[]" ).FromJsonOrNull<List<Guid>>() ?? new List<Guid>() );
            settings.KnownRelationshipTypeGuids.AddRange( data.GetValueOrDefault( "knownRelationships", "[]" ).FromJsonOrNull<List<Guid>>() ?? new List<Guid>() );
            settings.ListFormat = data.GetValueOrDefault( "listFormat", string.Empty ).ConvertToEnum<ListFormatSpecifier>( ListFormatSpecifier.NameAndRelationship );

            return settings.ToSelectionString();
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
            return "Related People";
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
            var allRelatedPeopleQuery = GetAllRelatedPeopleQuery( context, entityIdProperty, selection );
            var personQuery = new PersonService( context ).Queryable().Select( p => allRelatedPeopleQuery.Where( rpi => rpi.RelatedToPersonId == p.Id ).AsEnumerable() );
            var selectExpression = SelectExpressionExtractor.Extract( personQuery, entityIdProperty, "p" );
            return selectExpression;
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="entityIdProperty">The entity identifier property.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        private IQueryable<RelatedPersonInfo> GetAllRelatedPeopleQuery( RockContext context, MemberExpression entityIdProperty, string selection )
        {
            var settings = new RelatedPeopleSelectSettings( selection );

            bool showRelationship = ( settings.ListFormat == ListFormatSpecifier.NameAndRelationship );

            // Get Support Data.
            var adultGuid = GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid();
            var childGuid = GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid();

            int familyGroupTypeId = GroupTypeCache.Get( SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() ).Id;
            int knownRelationshipGroupTypeId = GroupTypeCache.Get( SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS.AsGuid() ).Id;

            //
            // Construct a Query to return the list of Related People matching the filter conditions.
            //
            IQueryable<RelatedPersonInfo> allRelatedPeopleQuery = null;

            // If we are looking for Parents...
            // Add the Adults from the Family Group in which the Principal participates as a Child.
            if ( settings.FamilyRelationshipTypeGuids.Contains( FamilyRelationshipParentGuid.AsGuid() ) )
            {
                var familyMembersQuery = GetRelatedPeopleQuery( context, new List<int> { familyGroupTypeId }, new List<Guid> { childGuid }, new List<Guid> { adultGuid }, showRelationship ? "Parent" : null );

                allRelatedPeopleQuery = GetRelatedPeopleUnionQuery( allRelatedPeopleQuery, familyMembersQuery );
            }

            // If we are looking for Children...
            // Add the Children from the Family Group in which the Principal participates as an Adult.
            if ( settings.FamilyRelationshipTypeGuids.Contains( FamilyRelationshipChildGuid.AsGuid() ) )
            {
                var familyMembersQuery = GetRelatedPeopleQuery( context, new List<int> { familyGroupTypeId }, new List<Guid> { adultGuid }, new List<Guid> { childGuid }, showRelationship ? "Child" : null );

                allRelatedPeopleQuery = GetRelatedPeopleUnionQuery( allRelatedPeopleQuery, familyMembersQuery );
            }

            // If we are looking for Siblings...
            // Add other Children from the Family Group in which the Principal participates as a Child.
            if ( settings.FamilyRelationshipTypeGuids.Contains( FamilyRelationshipSiblingGuid.AsGuid() ) )
            {
                var familyMembersQuery = GetRelatedPeopleQuery( context, new List<int> { familyGroupTypeId }, new List<Guid> { childGuid }, new List<Guid> { childGuid }, showRelationship ? "Sibling" : null );

                allRelatedPeopleQuery = GetRelatedPeopleUnionQuery( allRelatedPeopleQuery, familyMembersQuery );
            }

            // If we are looking for a Spouse...
            // Add other Married Adult in the Family Group in which the Principal participates as a Married Adult.
            if ( settings.FamilyRelationshipTypeGuids.Contains( FamilyRelationshipSpouseGuid.AsGuid() ) )
            {
                var marriedStatusGuid = SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED.AsGuid();
                int marriedStatusId = DefinedValueCache.Get( marriedStatusGuid ).Id;

                /*
                 * 2020-03-19 - JPH
                 *
                 * If a given Report includes deceased individuals, the 'Related People' Field Type should list non-deceased
                 * people who have known relationships with the deceased individual. Note that this is a one-way street: a
                 * deceased Person should never be displayed within the 'Related People' Field Type itself, but rather: a
                 * deceased Person should only be displayed as a row within a given Report when they are the subject of that
                 * Report (the 'Include Deceased' checkbox has been checked for the underlying Data View).
                 *
                 * Reason: Issue #4120 (Reporting on Known Relationships for Deceased Individuals)
                 * https://github.com/SparkDevNetwork/Rock/issues/4120
                 */

                var familyGroupMembers = new GroupMemberService( context ).Queryable( true )
                                                                          .Where( m => m.Group.GroupTypeId == familyGroupTypeId );

                var personSpouseQuery = new PersonService( context ).Queryable( new PersonService.PersonQueryOptions { IncludeDeceased = true } )
                                                                    .SelectMany(
                                                                                p =>
                                                                                familyGroupMembers.Where( gm => gm.PersonId == p.Id && gm.Person.MaritalStatusValueId == marriedStatusId && gm.GroupRole.Guid == adultGuid )
                                                                                                  .SelectMany( gm => gm.Group.Members )
                                                                                                  .Where( gm => gm.PersonId != p.Id && gm.GroupRole.Guid == adultGuid && gm.Person.MaritalStatusValueId == marriedStatusId )
                                                                                                  .Where( gm => gm.Person.IsDeceased == false )
                                                                                                  .Select(
                                                                                                          gm =>
                                                                                                          new RelatedPersonInfo
                                                                                                          {
                                                                                                              RelatedToPersonId = p.Id,
                                                                                                              PersonId = gm.Person.Id,
                                                                                                              FirstName = gm.Person.FirstName,
                                                                                                              LastName = gm.Person.LastName,
                                                                                                              Suffix = gm.Person.SuffixValue.Value,
                                                                                                              RelationshipName = showRelationship ? "Spouse" : null
                                                                                                          } ) );

                allRelatedPeopleQuery = GetRelatedPeopleUnionQuery( allRelatedPeopleQuery, personSpouseQuery );
            }

            // If we are looking for a Known Relationship...
            // Add other People from the Known Relationship Group having the specified Roles and in which the Principal is the Owner.
            if ( settings.KnownRelationshipTypeGuids.Any() )
            {
                var ownerGuid = GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER.AsGuid();
                var principalRoleGuids = new List<Guid>();
                var targetRoleGuids = new List<Guid>( settings.KnownRelationshipTypeGuids );

                principalRoleGuids.Add( ownerGuid );

                var knownPersonsQuery = GetRelatedPeopleQuery( context, new List<int> { knownRelationshipGroupTypeId }, principalRoleGuids, targetRoleGuids, showRelationship ? "*" : null );

                allRelatedPeopleQuery = GetRelatedPeopleUnionQuery( allRelatedPeopleQuery, knownPersonsQuery );
            }

            return allRelatedPeopleQuery;
        }

        /// <summary>
        ///     Add a Query to an existing Query to create a Union Query.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="unionQuery"></param>
        private IQueryable<RelatedPersonInfo> GetRelatedPeopleUnionQuery( IQueryable<RelatedPersonInfo> query, IQueryable<RelatedPersonInfo> unionQuery )
        {
            return ( query == null ? unionQuery : query.Concat( unionQuery ) );
        }

        private IQueryable<RelatedPersonInfo> GetRelatedPeopleQuery( RockContext context, IEnumerable<int> groupTypeIds, IEnumerable<Guid> principalRoleGuids, IEnumerable<Guid> targetRoleGuids,
                                                                     string relationshipName = "*" )
        {
            /*
             * 2020-03-19 - JPH
             *
             * If a given Report includes deceased individuals, the 'Related People' Field Type should list non-deceased
             * people who have known relationships with the deceased individual. Note that this is a one-way street: a
             * deceased Person should never be displayed within the 'Related People' Field Type itself, but rather: a
             * deceased Person should only be displayed as a row within a given Report when they are the subject of that
             * Report (the 'Include Deceased' checkbox has been checked for the underlying Data View).
             *
             * Reason: Issue #4120 (Reporting on Known Relationships for Deceased Individuals)
             * https://github.com/SparkDevNetwork/Rock/issues/4120
             */

            var relationshipGroupMembersQuery = new GroupMemberService( context ).Queryable( true );

            relationshipGroupMembersQuery = relationshipGroupMembersQuery.Where( x => groupTypeIds.Contains( x.Group.GroupTypeId ) );

            var relatedPeopleQuery = new PersonService( context ).Queryable( new PersonService.PersonQueryOptions { IncludeDeceased = true } )
                                                                 .SelectMany( p => relationshipGroupMembersQuery.Where( gm => gm.PersonId == p.Id && principalRoleGuids.Contains( gm.GroupRole.Guid ) )
                                                                                                                .SelectMany( gm => gm.Group.Members )
                                                                                                                .Where( gm => targetRoleGuids.Contains( gm.GroupRole.Guid ) && gm.PersonId != p.Id )
                                                                                                                .Where( gm => gm.Person.IsDeceased == false )
                                                                                                                .OrderBy( gm => gm.GroupRole.Order )
                                                                                                                .ThenBy( gm => gm.Person.BirthDate )
                                                                                                                .ThenBy( gm => gm.Person.Gender )
                                                                                                                .Select(
                                                                                                                        gm =>
                                                                                                                        new RelatedPersonInfo
                                                                                                                        {
                                                                                                                            RelatedToPersonId = p.Id,
                                                                                                                            PersonId = gm.Person.Id,
                                                                                                                            FirstName = gm.Person.FirstName,
                                                                                                                            LastName = gm.Person.LastName,
                                                                                                                            Suffix = gm.Person.SuffixValue.Value,
                                                                                                                            RelationshipName =
                                                                                                                                ( relationshipName == "*"
                                                                                                                                      ? gm.GroupRole.Name
                                                                                                                                      : relationshipName )
                                                                                                                        } ) );

            return relatedPeopleQuery;
        }

        private const string _CtlFormat = "ddlFormat";
        private const string _CtlKnownRelationshipType = "cblKnownRelationshipType";
        private const string _CtlFamilyRelationshipType = "cblFamilyRelationshipType";

        /// <summary>
        /// The family relationship parent unique identifier
        /// </summary>
        public const string FamilyRelationshipParentGuid = "A498E9DB-4BFF-4C52-A6C0-7510EECDF6E7";

        /// <summary>
        /// The family relationship child unique identifier
        /// </summary>
        public const string FamilyRelationshipChildGuid = "3DD246AB-2DD4-4DCE-B465-FBF5E72D7FBE";

        /// <summary>
        /// The family relationship sibling unique identifier
        /// </summary>
        public const string FamilyRelationshipSiblingGuid = "7CEA6446-98C0-4E42-AE6D-6FD3F6B00416";

        /// <summary>
        /// The family relationship spouse unique identifier
        /// </summary>
        public const string FamilyRelationshipSpouseGuid = "98AD882C-39C4-4FC6-B06D-EC474117BF42";

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <param name="parentControl"></param>
        /// <returns></returns>
        public override Control[] CreateChildControls( Control parentControl )
        {
            // Define Control: Family Relationships Checkbox List
            var cblFamilyRelationships = new RockCheckBoxList();
            cblFamilyRelationships.Label = "Include Family Relationship Types";
            cblFamilyRelationships.Help = "These relationship types apply to members of the same Family.";
            cblFamilyRelationships.ID = parentControl.GetChildControlInstanceName( _CtlFamilyRelationshipType );

            var items = cblFamilyRelationships.Items;

            items.Add( new ListItem( "Parent", FamilyRelationshipParentGuid ) );
            items.Add( new ListItem( "Child", FamilyRelationshipChildGuid ) );
            items.Add( new ListItem( "Sibling", FamilyRelationshipSiblingGuid ) );
            items.Add( new ListItem( "Spouse", FamilyRelationshipSpouseGuid ) );

            parentControl.Controls.Add( cblFamilyRelationships );

            // Define Control: Known Relationships Checkbox List
            var cblKnownRelationships = new RockCheckBoxList();
            cblKnownRelationships.Label = "Include Known Relationship Types";
            cblKnownRelationships.Help = "These relationship types apply to People from another Family.";
            cblKnownRelationships.ID = parentControl.GetChildControlInstanceName( _CtlKnownRelationshipType );
            PopulateRelationshipTypesSelector( cblKnownRelationships, SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS.AsGuid() );

            parentControl.Controls.Add( cblKnownRelationships );

            // Define Control: Output Format DropDown List
            var ddlFormat = new RockDropDownList();
            ddlFormat.ID = parentControl.GetChildControlInstanceName( _CtlFormat );
            ddlFormat.Label = "Output Format";
            ddlFormat.Help = "Specifies the content and format of the items in this field.";
            ddlFormat.Items.Add( new ListItem( "Person Name and Relationship", ListFormatSpecifier.NameAndRelationship.ToString() ) );
            ddlFormat.Items.Add( new ListItem( "Person Name", ListFormatSpecifier.NameOnly.ToString() ) );
            parentControl.Controls.Add( ddlFormat );

            return new Control[] { cblFamilyRelationships, cblKnownRelationships, ddlFormat };
        }

        /// <summary>
        ///     Populates the group roles.
        /// </summary>
        /// <param name="checkboxList"></param>
        /// <param name="groupTypeGuid"></param>
        private void PopulateRelationshipTypesSelector( RockCheckBoxList checkboxList, Guid? groupTypeGuid )
        {
            bool showSelector = false;

            checkboxList.Items.Clear();

            var groupType = GroupTypeCache.Get( groupTypeGuid.GetValueOrDefault() );

            if ( groupType != null )
            {
                var selectableRoles = new GroupTypeRoleService( new RockContext() ).GetByGroupTypeId( groupType.Id );

                // Exclude the Owner Role from the list of selectable Roles because a Person cannot be related to themselves.
                var ownerGuid = GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER.AsGuid();

                selectableRoles = selectableRoles.Where( x => x.Guid != ownerGuid );

                checkboxList.Items.Clear();

                foreach ( var item in selectableRoles )
                {
                    checkboxList.Items.Add( new ListItem( item.Name, item.Guid.ToString() ) );
                }

                showSelector = checkboxList.Items.Count > 0;
            }

            checkboxList.Visible = showSelector;
        }

        /// <summary>
        /// Gets the selection.
        /// This is typically a string that contains the values selected with the Controls
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( Control[] controls )
        {
            var cblFamilyRelationshipType = controls.GetByName<RockCheckBoxList>( _CtlFamilyRelationshipType );
            var cblKnownRelationshipType = controls.GetByName<RockCheckBoxList>( _CtlKnownRelationshipType );
            var ddlFormat = controls.GetByName<RockDropDownList>( _CtlFormat );

            var settings = new RelatedPeopleSelectSettings();

            IEnumerable<Guid> selectedItems;

            // List Format
            settings.ListFormat = ddlFormat.SelectedValue.ConvertToEnum<ListFormatSpecifier>( ListFormatSpecifier.NameAndRelationship );

            // Family Relationships
            selectedItems = cblFamilyRelationshipType.Items.OfType<ListItem>().Where( x => x.Selected ).Select( x => x.Value.AsGuid() );

            settings.FamilyRelationshipTypeGuids.AddRange( selectedItems );

            // Known Relationships
            selectedItems = cblKnownRelationshipType.Items.OfType<ListItem>().Where( x => x.Selected ).Select( x => x.Value.AsGuid() );

            settings.KnownRelationshipTypeGuids.AddRange( selectedItems );

            return settings.ToSelectionString();
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Control[] controls, string selection )
        {
            var cblKnownRelationshipType = controls.GetByName<RockCheckBoxList>( _CtlKnownRelationshipType );
            var cblFamilyRelationshipType = controls.GetByName<RockCheckBoxList>( _CtlFamilyRelationshipType );
            var ddlFormat = controls.GetByName<RockDropDownList>( _CtlFormat );

            var settings = new RelatedPeopleSelectSettings( selection );

            if ( !settings.IsValid )
            {
                return;
            }

            // Family Relationships
            foreach ( var item in cblFamilyRelationshipType.Items.OfType<ListItem>() )
            {
                item.Selected = settings.FamilyRelationshipTypeGuids.Contains( item.Value.AsGuid() );
            }

            // Known Relationships
            foreach ( var item in cblKnownRelationshipType.Items.OfType<ListItem>() )
            {
                item.Selected = settings.KnownRelationshipTypeGuids.Contains( item.Value.AsGuid() );
            }

            // List Format
            ddlFormat.SelectedValue = settings.ListFormat.ToString();
        }

        #endregion

        #region Settings

        private enum ListFormatSpecifier
        {
            NameAndRelationship = 0,
            NameOnly = 1
        }

        /// <summary>
        ///     Settings for the Data Select Component "Related People".
        /// </summary>
        private class RelatedPeopleSelectSettings : SettingsStringBase
        {
            public readonly List<Guid> FamilyRelationshipTypeGuids = new List<Guid>();
            public readonly List<Guid> KnownRelationshipTypeGuids = new List<Guid>();
            public ListFormatSpecifier ListFormat = ListFormatSpecifier.NameAndRelationship;

            public RelatedPeopleSelectSettings()
            {
                //
            }

            public RelatedPeopleSelectSettings( string settingsString )
            {
                FromSelectionString( settingsString );
            }

            protected override void OnSetParameters( int version, IReadOnlyList<string> parameters )
            {
                ListFormat = DataComponentSettingsHelper.GetParameterAsEnum( parameters, 0, ListFormatSpecifier.NameAndRelationship );

                List<string> selectedRoleGuids;

                FamilyRelationshipTypeGuids.Clear();

                selectedRoleGuids = DataComponentSettingsHelper.GetParameterAsList( parameters, 1, "," );

                foreach ( var roleGuid in selectedRoleGuids )
                {
                    FamilyRelationshipTypeGuids.Add( roleGuid.AsGuid() );
                }

                KnownRelationshipTypeGuids.Clear();

                selectedRoleGuids = DataComponentSettingsHelper.GetParameterAsList( parameters, 2, "," );

                foreach ( var roleGuid in selectedRoleGuids )
                {
                    KnownRelationshipTypeGuids.Add( roleGuid.AsGuid() );
                }
            }

            protected override IEnumerable<string> OnGetParameters()
            {
                var settings = new List<string>();

                settings.Add( ( ( int ) ListFormat ).ToString() );

                settings.Add( FamilyRelationshipTypeGuids == null ? string.Empty : FamilyRelationshipTypeGuids.AsDelimited( "," ) );
                settings.Add( KnownRelationshipTypeGuids == null ? string.Empty : KnownRelationshipTypeGuids.AsDelimited( "," ) );

                return settings;
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
                var allRelatedPeopleQuery = GetAllRelatedPeopleQuery( rockContext, entityIdProperty, selection );
                var personQuery = new PersonService( rockContext ).Queryable().Select( p => allRelatedPeopleQuery.Where( rpi => rpi.RelatedToPersonId == p.Id ).Select( rpi => rpi.PersonId ).AsEnumerable() );
                var selectExpression = SelectExpressionExtractor.Extract( personQuery, entityIdProperty, "p" );
                return selectExpression;
            }

            return null;
        }

        #endregion
    }
}