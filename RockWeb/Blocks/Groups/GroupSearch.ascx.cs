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
using System.IO;
using System.Linq;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Groups
{
    /// <summary>
    /// "Handles displaying group search results and redirects to the group detail page (via route ~/Group/) when only one match was found.
    /// </summary>
    [DisplayName( "Group Search" )]
    [Category( "Groups" )]
    [Description( "Handles displaying group search results and redirects to the group detail page (via route ~/Group/) when only one match was found." )]
    [CodeEditorField( "Group URL Format", "The URL to use for linking to a group. <span class='tip tip-lava'></span>", CodeEditorMode.Lava, CodeEditorTheme.Rock, 200, false, @"~/Group/{{ Group.Id }}" )]

    [Rock.SystemGuid.BlockTypeGuid( "F1E188A5-2F9D-4BA6-BCA1-82B2450DAC1C" )]
    public partial class GroupSearch : RockBlock
    {
        #region Fields

        private Dictionary<string, object> _commonMergeFields = new Dictionary<string, object>();

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            BindGrid();
            base.OnLoad( e );
        }

        #endregion

        #region Events

        #endregion

        #region Methods

        private void BindGrid()
        {
            string type = PageParameter( "SearchType" );
            string term = PageParameter( "SearchTerm" );

            var groupService = new GroupService( new RockContext() );
            var groups = new List<Group>();

            if ( !string.IsNullOrWhiteSpace( type ) && !string.IsNullOrWhiteSpace( term ) )
            {
                switch ( type.ToLower() )
                {
                    case "name":
                        {
                            groups = groupService.Queryable()
                                .Where( g =>
                                    g.GroupType.ShowInNavigation &&
                                    g.Name.Contains( term ) )
                                .OrderBy( g => g.Order )
                                .ThenBy( g => g.Name )
                                .ToList();

                            break;
                        }
                }
            }

            var _commonMergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );

            if ( groups.Count == 1 )
            {
                var mergeFields = new Dictionary<string, object>(_commonMergeFields)
                {
                    {"Group", groups[0]}
                };
                var url = GetAttributeValue("GroupURLFormat").ResolveMergeFields(mergeFields);

                Response.Redirect( url, false );
                Context.ApplicationInstance.CompleteRequest();
            }
            else
            {
                gGroups.EntityTypeId = EntityTypeCache.Get<Group>().Id;
                gGroups.DataSource = groups
                    .Select( g => new
                    {
                        g.Id,
                        GroupType = g.GroupType.Name,
                        Structure = ParentStructure( g ),
                        MemberCount = g.Members.Count,
                        Campus = g.Campus?.Name
                    } )
                    .ToList();
                gGroups.DataBind();
            }
        }

        private string ParentStructure( Group group, List<int> parentIds = null )
        {
            if ( group == null )
            {
                return string.Empty;
            }

            // Create or add this node to the history stack for this tree walk.
            if (parentIds == null)
            {
                parentIds = new List<int>();
            }
            else
            {
                // If we have encountered this node before during this tree walk, we have found an infinite recursion in the tree.
                // Truncate the path with an error message and exit.
                if (parentIds.Contains(group.Id))
                {
                    return "#Invalid-Parent-Reference#";
                }
            }

            parentIds.Add(group.Id);

            string prefix = ParentStructure( group.ParentGroup, parentIds );

            if ( !string.IsNullOrWhiteSpace( prefix ) )
            {
                prefix += " <i class='fa fa-angle-right'></i> ";
            }

            var mergeFields = new Dictionary<string, object>( _commonMergeFields )
            {
                {"Group", group }
            };
            var url = RockPage.ResolveRockUrl(GetAttributeValue("GroupURLFormat").ResolveMergeFields(mergeFields));
            return string.Format( "{0}<a href='{1}'>{2}</a>", prefix, url, group.Name );
        }

        #endregion
    }
}