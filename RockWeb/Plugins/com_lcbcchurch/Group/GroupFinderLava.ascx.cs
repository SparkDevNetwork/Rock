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
using System.Data.Entity;
using System.Linq;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;
using System.Web.UI.WebControls;
using Rock.Web.Cache;
using Rock.Reporting;

namespace RockWeb.Plugins.com_lcbcChurch.Groups
{
    /// <summary>
    ///
    /// </summary>
    [DisplayName( "Group Finder Lava" )]
    [Category( "Groups" )]
    [Description( "Renders groups with filters using Lava." )]

    [GroupTypeField( "Group Type",
        Description = "The group type to search groups for.",
        IsRequired = true,
        Order = 0,
        Key = AttributeKeys.GroupType
        )]
    [CustomDropdownListField(
        "Filter One: Group Attribute",
        description: "The attribute key to be used for filter one.",
        listSource: @"SELECT [Id] as [Value], [Name] AS [Text] FROM [Attribute] WHERE [EntityTypeId]=(SELECT Id FROM EntityType WHERE [Name]='Rock.Model.Group')
AND [FieldTypeId] = ( SELECT Id FROM FieldType WHERE[Guid] = '59D5A94C-94A0-4630-B80A-BB25697D74C7' )",
        IsRequired = false,
        Key = AttributeKeys.FilterOneGroupAttribute,
        Order = 1 )]
    [CustomDropdownListField(
        "Filter Two: Group Attribute",
        description: "The attribute key to be used for filter two.",
        listSource: @"SELECT [Id] as [Value], [Name] AS [Text] FROM [Attribute] WHERE [EntityTypeId]=(SELECT Id FROM EntityType WHERE [Name]='Rock.Model.Group')
AND [FieldTypeId] = ( SELECT Id FROM FieldType WHERE[Guid] = '59D5A94C-94A0-4630-B80A-BB25697D74C7' )",
        IsRequired = false,
        Key = AttributeKeys.FilterTwoGroupAttribute,
        Order = 2 )]
    [BooleanField( "Show Campus Filter",
        description: "Should the Campus filter be displayed?",
        defaultValue: true,
        order: 3,
        key: AttributeKeys.ShowCampusFilter )]
    [EnumsField( "Keywords Search By",
        description: "Should the Campus filter be displayed?",
        enumSourceType: typeof( KeywordType ),
        Order = 4,
        Key = AttributeKeys.KeywordsSearchBy )]
    [CodeEditorField( "Search Form Lava",
        "The lava template for display the search form.",
        Rock.Web.UI.Controls.CodeEditorMode.Lava,
        CodeEditorTheme.Rock,
        IsRequired = false,
        DefaultValue = @"",
        Key = AttributeKeys.SearchFormLava,
        Order = 5 )]
    [CodeEditorField( "Search Results Lava",
        "The lava template for display the results of the search.",
        CodeEditorMode.Lava,
        CodeEditorTheme.Rock,
        IsRequired = false,
        DefaultValue = @"",
        Key = AttributeKeys.SearchResultsLava,
        Order = 6 )]
    [IntegerField( "Max Results to display",
        "The number of results to show per page.",
        true,
        20,
        Key = AttributeKeys.MaxResultstodisplay,
        Order = 7 )]
    public partial class GroupFinderLava : Rock.Web.UI.RockBlock
    {
        #region Attribute Keys

        /// <summary>
        ///
        /// </summary>
        private enum KeywordType
        {
            /// <summary>
            /// Group Leaders
            /// </summary>
            GroupName = 1,

            /// <summary>
            /// Group Description
            /// </summary>
            GroupDescription = 2,

            /// <summary>
            /// Group Leader Name
            /// </summary>
            LeaderName = 3,
        }

        protected static class AttributeKeys
        {
            public const string GroupType = "GroupType";
            public const string FilterOneGroupAttribute = "FilterOneAttributeKey";
            public const string FilterTwoGroupAttribute = "FilterTwoAttributeKey";
            public const string SearchFormLava = "SearchFormLava";
            public const string SearchResultsLava = "SearchResultsLava";
            public const string ShowCampusFilter = "ShowCampusFilter";
            public const string KeywordsSearchBy = "KeywordsSearchBy";
            public const string MaxResultstodisplay = "MaxResultstodisplay";
        }

        #endregion Attribute Keys
        #region PageParameterKeys

        protected static class PageParameterKey
        {
            public const string FilterOneValue = "FilterOneValue";
            public const string FilterTwoValue = "FilterTwoValue";
            public const string CampusId = "CampusId";
            public const string Keywords = "Q";
        }

        #endregion PageParameterKeys

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            RouteAction();

            if ( !IsPostBack )
            {
                BlockSetup();
                ShowContent();
            }
        }

        #endregion

        #region Control Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowContent();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// setup block
        /// </summary>
        private void BlockSetup()
        {
            hfFilterOneValues.Value = PageParameter( PageParameterKey.FilterOneValue );
            hfFilterTwoValues.Value = PageParameter( PageParameterKey.FilterTwoValue );
            hfCampusIds.Value = PageParameter( PageParameterKey.CampusId );
            hfPageNo.SetValue( 1 );
            tbKeywords.Text = PageParameter( PageParameterKey.Keywords );
        }
        /// <summary>
        /// Route the request to the correct panel
        /// </summary>
        private void RouteAction()
        {

            if ( Request.Form["__EVENTARGUMENT"] != null )
            {
                string[] eventArgs = Request.Form["__EVENTARGUMENT"].Split( '^' );

                if ( eventArgs.Length == 2 )
                {
                    string action = eventArgs[0];
                    string parameters = eventArgs[1];


                    switch ( action )
                    {
                        case "Search":
                            ShowContent();
                            break;
                    }
                }
            }
        }

        public void ShowContent()
        {
            nbNotice.Visible = false;
            Guid? groupTypeGuid = GetAttributeValue( AttributeKeys.GroupType ).AsGuidOrNull();
            if ( !groupTypeGuid.HasValue )
            {
                nbNotice.Heading = "Error";
                nbNotice.NotificationBoxType = NotificationBoxType.Danger;
                nbNotice.Text = "<p>A valid Group Type is required.</p>";
                nbNotice.Visible = true;
                return;
            }
            ShowFilter();
            var rockContext = new RockContext();
            var groupType = GroupTypeCache.Get( groupTypeGuid.Value );
            var groupService = new GroupService( rockContext );
            var groupQry = groupService
                .Queryable( "GroupLocations.Location" )
                .Where( g => g.IsActive && g.GroupTypeId == groupType.Id && g.IsPublic );

            var hasFilter = false;
            if ( GetAttributeValue( AttributeKeys.ShowCampusFilter ).AsBoolean() )
            {
                var campusIds = hfCampusIds.Value.SplitDelimitedValues().AsIntegerList();
                if ( campusIds.Count > 0 )
                {
                    hasFilter = true;
                    groupQry = groupQry.Where( c => campusIds.Contains( c.CampusId ?? -1 ) );
                }
            }


            var keywordsSearchBy = GetAttributeValue( AttributeKeys.KeywordsSearchBy ).SplitDelimitedValues().Select( a => a.ConvertToEnumOrNull<KeywordType>() ).ToList();
            if ( keywordsSearchBy != null && keywordsSearchBy.Any() && tbKeywords.Text.IsNotNullOrWhiteSpace() )
            {
                hasFilter = true;
                var text = tbKeywords.Text;
                bool isGroupName = false;
                bool isDescription = false;
                bool isLeaderName = false;
                foreach ( var keyword in keywordsSearchBy.Where( a => a.HasValue ) )
                {
                    switch ( keyword.Value )
                    {
                        case KeywordType.GroupName:
                            isGroupName = true;
                            break;
                        case KeywordType.GroupDescription:
                            isDescription = true;
                            break;
                        case KeywordType.LeaderName:
                            isLeaderName = true;
                            break;
                        default:
                            break;
                    }

                }

                groupQry = groupQry.Where( g =>
                                            ( isGroupName && g.Name.Contains( text ) ) ||
                                            ( isDescription && g.Description.Contains( text ) ) ||
                                            ( isLeaderName && g.Members.Any
                                                ( gm => gm.GroupRole.IsLeader &&
                                                        ( gm.Person.LastName.Contains( text ) || gm.Person.FirstName.Contains( text ) || gm.Person.NickName.Contains( text ) )
                                                )
                                            )
                                          );
            }


            var filterOneAttributeValue = GetAttributeValue( AttributeKeys.FilterOneGroupAttribute ).AsInteger();
            if ( filterOneAttributeValue != default( int ) )
            {
                List<string> values = new List<string>();
                foreach ( var id in hfFilterOneValues.Value.SplitDelimitedValues() )
                {
                    var definedValue = DefinedValueCache.Get( id.AsInteger() );
                    if ( definedValue != null )
                    {
                        values.Add( definedValue.Guid.ToString() );
                    }
                }

                if ( values.Any() )
                {
                    hasFilter = true;
                    var attribute = Rock.Web.Cache.AttributeCache.Get( filterOneAttributeValue );
                    var attributeEntityField = EntityHelper.GetEntityFieldForAttribute( attribute );
                    var control = attribute.FieldType.Field.FilterControl( attribute.QualifierValues, "fdf", true, FilterMode.SimpleFilter );
                    attribute.FieldType.Field.SetFilterValues( control, attribute.QualifierValues, new List<string>() { values.AsDelimited( "," ) } );
                    var filterValues = attribute.FieldType.Field.GetFilterValues( control, attribute.QualifierValues, FilterMode.SimpleFilter );
                    var parameterExpression = groupService.ParameterExpression;
                    var attributeExpression = Rock.Utility.ExpressionHelper.GetAttributeExpression( groupService, parameterExpression, attributeEntityField, filterValues );
                    groupQry = groupQry.Where( parameterExpression, attributeExpression );
                }
            }

            var filterTwoAttributeValue = GetAttributeValue( AttributeKeys.FilterTwoGroupAttribute ).AsInteger();
            if ( filterTwoAttributeValue != default( int ) )
            {
                List<string> values = new List<string>();
                foreach ( var id in hfFilterTwoValues.Value.SplitDelimitedValues() )
                {
                    var definedValue = DefinedValueCache.Get( id.AsInteger() );
                    if ( definedValue != null )
                    {
                        values.Add( definedValue.Guid.ToString() );
                    }
                }
                if ( values.Any() )
                {
                    hasFilter = true;
                    var attribute = Rock.Web.Cache.AttributeCache.Get( filterTwoAttributeValue );
                    var attributeEntityField = EntityHelper.GetEntityFieldForAttribute( attribute );
                    var control = attribute.FieldType.Field.FilterControl( attribute.QualifierValues, "fdf", true, FilterMode.SimpleFilter );
                    attribute.FieldType.Field.SetFilterValues( control, attribute.QualifierValues, new List<string>() { values.AsDelimited( "," ) } );
                    var filterValues = attribute.FieldType.Field.GetFilterValues( control, attribute.QualifierValues, FilterMode.SimpleFilter );
                    var parameterExpression = groupService.ParameterExpression;
                    var attributeExpression = Rock.Utility.ExpressionHelper.GetAttributeExpression( groupService, parameterExpression, attributeEntityField, filterValues );
                    groupQry = groupQry.Where( parameterExpression, attributeExpression );
                }
            }

            var groups = groupQry.OrderBy( g => g.Name ).ToList();
            int quantity = GetAttributeValue( AttributeKeys.MaxResultstodisplay ).AsInteger();
            var skipCount = ( hfPageNo.ValueAsInt() - 1 ) * quantity;
            var items = groups.Skip( skipCount ).Take( quantity + 1 ).Distinct().ToList();
            bool hasMore = ( quantity < items.Count );

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
            mergeFields.Add( "Groups", groups.Take( quantity ) );
            mergeFields.Add( "HasMore", hasMore );

            if ( filterOneAttributeValue != default( int ) )
            {
                var attribute = Rock.Web.Cache.AttributeCache.Get( filterOneAttributeValue );
                mergeFields.Add( AttributeKeys.FilterOneGroupAttribute, attribute );

                if ( attribute.QualifierValues.ContainsKey( "definedtype" ) )
                {
                    mergeFields.Add( "FilterOneDefinedTypeId", attribute.QualifierValues["definedtype"].Value.AsIntegerOrNull() );
                }
            }

            if ( filterTwoAttributeValue != default( int ) )
            {
                var attribute = Rock.Web.Cache.AttributeCache.Get( filterTwoAttributeValue );
                mergeFields.Add( AttributeKeys.FilterTwoGroupAttribute, attribute );

                if ( attribute.QualifierValues.ContainsKey( "definedtype" ) )
                {
                    mergeFields.Add( "FilterTwoDefinedTypeId", attribute.QualifierValues["definedtype"].Value.AsIntegerOrNull() );
                }
            }

            mergeFields.Add( "HasFilter", hasFilter );
            lResults.Text = GetAttributeValue( AttributeKeys.SearchResultsLava ).ResolveMergeFields( mergeFields );
        }

        private void ShowFilter()
        {
            var formMergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
            var filterOneAttributeValue = GetAttributeValue( AttributeKeys.FilterOneGroupAttribute ).AsInteger();
            if ( filterOneAttributeValue != default( int ) )
            {
                var attribute = Rock.Web.Cache.AttributeCache.Get( filterOneAttributeValue );
                formMergeFields.Add( AttributeKeys.FilterOneGroupAttribute, attribute );

                if ( attribute.QualifierValues.ContainsKey( "definedtype" ) )
                {
                    formMergeFields.Add( "FilterOneDefinedTypeId", attribute.QualifierValues["definedtype"].Value.AsIntegerOrNull() );
                }
            }

            var filterTwoAttributeValue = GetAttributeValue( AttributeKeys.FilterTwoGroupAttribute ).AsInteger();
            if ( filterTwoAttributeValue != default( int ) )
            {
                var attribute = Rock.Web.Cache.AttributeCache.Get( filterTwoAttributeValue );
                formMergeFields.Add( AttributeKeys.FilterTwoGroupAttribute, attribute );
                if ( attribute.QualifierValues.ContainsKey( "definedtype" ) )
                {
                    formMergeFields.Add( "FilterTwoDefinedTypeId", attribute.QualifierValues["definedtype"].Value.AsIntegerOrNull() );
                }
            }
            formMergeFields.Add( AttributeKeys.ShowCampusFilter, GetAttributeValue( AttributeKeys.ShowCampusFilter ) );
            formMergeFields.Add( AttributeKeys.KeywordsSearchBy, GetAttributeValue( AttributeKeys.KeywordsSearchBy ) );
            formMergeFields.Add( AttributeKeys.MaxResultstodisplay, GetAttributeValue( AttributeKeys.MaxResultstodisplay ) );
            lFilter.Text = GetAttributeValue( AttributeKeys.SearchFormLava ).ResolveMergeFields( formMergeFields ).ResolveClientIds( upnlContent.ClientID );
        }

        #endregion

    }
}