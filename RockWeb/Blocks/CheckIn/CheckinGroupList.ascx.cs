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
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.Entity;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using System.Text;

namespace RockWeb.Blocks.Checkin
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Check-in Group List" )]
    [Category( "Check-in" )]
    [Description( "Lists checkin areas and their groups based off a parent checkin configuration group type." )]
    [GroupTypeField( "Check-in Type", required: false, key: "GroupTypeTemplate", groupTypePurposeValueGuid: Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE )]
    [LinkedPage("Group Detail Page", "Link to the group details page", false)]
    public partial class CheckinGroupList : Rock.Web.UI.RockBlock
    {
        #region Fields

        // used for private variables
        StringBuilder _content = new StringBuilder();
        RockContext _rockContext = null;
        #endregion

        #region Properties

        // used for public / protected properties

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

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

            _rockContext = new RockContext();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                ShowContent();
            }
        }



        #endregion

        #region Events

        // handlers called by the controls on your block

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

        #region Methods

        private GroupType GetRootGroupType(int groupId)
        {
            List<int> parentRecursionHistory = new List<int>();
            GroupTypeService groupTypeService = new GroupTypeService( _rockContext );
            var groupType = groupTypeService.Queryable().AsNoTracking().Include( t => t.ParentGroupTypes ).Where( t => t.Groups.Select( g => g.Id ).Contains( groupId ) ).FirstOrDefault();

            while (groupType != null && groupType.ParentGroupTypes.Count != 0 )
            {
                if ( parentRecursionHistory.Contains( groupType.Id ) )
                {
                    var exception = new Exception("Infinite Recursion detected in GetRootGroupType for groupId: " + groupId.ToString());
                    LogException( exception );
                    return null;
                }
                else
                {
                    var parentGroupType = GetParentGroupType( groupType );
                    if (parentGroupType != null && parentGroupType.Id == groupType.Id)
                    {
                        // the group type's parent is itself
                        return groupType;
                    }

                    groupType = parentGroupType;
                }
                
                parentRecursionHistory.Add(groupType.Id);
            }

            return groupType;
        }

        private GroupType GetParentGroupType( GroupType groupType )
        {
            GroupTypeService groupTypeService = new GroupTypeService( _rockContext );
            return groupTypeService.Queryable()
                                .Include( t => t.ParentGroupTypes )
                                .AsNoTracking()
                                .Where( t => t.ChildGroupTypes.Select(p => p.Id).Contains( groupType.Id ) ).FirstOrDefault();
        }

        private void ShowContent()
        {
            Guid configurationTemplateGuid = Guid.Empty;
            
            if ( Request["GroupTypeId"] != null )
            {
                var groupType = Rock.Web.Cache.GroupTypeCache.Read( Int32.Parse( Request["GroupTypeId"] ) );
                configurationTemplateGuid = groupType.Guid;
            }
            else if(Request["GroupId"] != null) {
                // get the root group type of this group
                int groupId = Int32.Parse( Request["GroupId"] );

                var rootGroupType = GetRootGroupType( groupId );
                if ( rootGroupType != null )
                {
                    configurationTemplateGuid = rootGroupType.Guid;
                }
            }
            else
            {
                if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "Check-inType" ) ) )
                {
                    configurationTemplateGuid = new Guid( GetAttributeValue( "Check-inType" ) );
                }
            }

            if ( configurationTemplateGuid == Guid.Empty )
            {
                lWarnings.Text = "<div class='alert alert-warning'>No check-in configuration template configured.</div>";
            }
            else
            {
                BuildHeirarchy( configurationTemplateGuid );
                lContent.Text = _content.ToString();
            } 
        }

        private void BuildHeirarchy( Guid parentGroupTypeGuid )
        {
            GroupTypeService groupTypeService = new GroupTypeService( _rockContext );

            var groupTypes = groupTypeService.Queryable( "Groups, ChildGroupTypes" ).AsNoTracking()
                            .Where( t => t.ParentGroupTypes.Select( p => p.Guid ).Contains( parentGroupTypeGuid ) && t.Guid != parentGroupTypeGuid ).ToList();

            foreach ( var groupType in groupTypes )
            {
                if (groupType.GroupTypePurposeValueId == null || groupType.Groups.Count > 0) {
                    _content.Append( "<ul>" );
                    _content.Append( string.Format( "<li><strong>{0}</strong></li>", groupType.Name ) );
                    if ( groupType.ChildGroupTypes.Count > 0 )
                    {
                        BuildHeirarchy( groupType.Guid );
                    }

                    _content.Append( "<ul>" );
                    foreach ( var group in groupType.Groups )
                    {
                        if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "GroupDetailPage" ) ) )
                        {
                            var groupPageParams = new Dictionary<string, string>();
                            if ( Request["GroupTypeId"] != null )
                            {
                                groupPageParams.Add( "GroupTypeId", Request["GroupTypeId"] );
                            }
                            groupPageParams.Add( "GroupId", group.Id.ToString() );
                            _content.Append( string.Format( "<li><a href='{0}'>{1}</a></li>", LinkedPageUrl( "GroupDetailPage", groupPageParams ), group.Name ) );
                        }
                        else
                        {
                            _content.Append( string.Format( "<li>{0}</li>", group.Name ) );
                        }
                    }
                    _content.Append( "</ul>" );

                    _content.Append( "</ul>" );
                }
                else
                {
                    BuildHeirarchy( groupType.Guid );
                }
            
            }

            
        }

        #endregion
    }
}