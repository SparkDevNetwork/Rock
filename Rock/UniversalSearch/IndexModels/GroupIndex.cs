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
using System.Collections.Generic;
using System.Linq;

using Rock.Model;
using Rock.UniversalSearch.IndexModels.Attributes;

namespace Rock.UniversalSearch.IndexModels
{
    /// <summary>
    /// Group Index
    /// </summary>
    /// <seealso cref="Rock.UniversalSearch.IndexModels.IndexModelBase" />
    public class GroupIndex : IndexModelBase
    {
        /// <summary>
        /// Gets or sets the group type identifier.
        /// </summary>
        /// <value>
        /// The group type identifier.
        /// </value>
        [RockIndexField]
        public int GroupTypeId { get; set; }

        /// <summary>
        /// Gets or sets the name of the group type.
        /// </summary>
        /// <value>
        /// The name of the group type.
        /// </value>
        [RockIndexField]
        public string GroupTypeName { get; set; }

        /// <summary>
        /// Gets or sets the parent group identifier.
        /// </summary>
        /// <value>
        /// The parent group identifier.
        /// </value>
        [RockIndexField( Index = IndexType.NotIndexed )]
        public int ParentGroupId { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        [RockIndexField( Index = IndexType.NotIndexed )]
        public override string IconCssClass {
            get
            {
                if ( string.IsNullOrWhiteSpace( _iconCssClass ) )
                {
                    return "fa fa-users";
                }
                return _iconCssClass;
            }
            set
            {
                _iconCssClass = value;
            }
        }
        private string _iconCssClass = "";

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [RockIndexField( Boost = 3 )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [RockIndexField]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the member list.
        /// </summary>
        /// <value>
        /// The member list.
        /// </value>
        [RockIndexField]
        public string MemberList { get; set; }

        /// <summary>
        /// Gets or sets the leader list.
        /// </summary>
        /// <value>
        /// The leader list.
        /// </value>
        [RockIndexField]
        public string LeaderList { get; set; }

        /// <summary>
        /// Loads the by model.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <returns></returns>
        public static GroupIndex LoadByModel( Group group )
        {
            var groupIndex = new GroupIndex();
            groupIndex.SourceIndexModel = "Rock.Model.Group";

            groupIndex.Id = group.Id;
            groupIndex.Name = group.Name;
            groupIndex.Description = group.Description;
            groupIndex.GroupTypeId = group.GroupTypeId;
            groupIndex.DocumentName = group.Name;

            groupIndex.ModelOrder = 5;

            if ( group.GroupType != null )
            {
                groupIndex.IconCssClass = group.GroupType.IconCssClass;
                groupIndex.GroupTypeName = group.GroupType.Name;
            }

            if (group.Members != null )
            {
                groupIndex.MemberList = string.Join( ", ", group.Members.Where( m => m.GroupRole.IsLeader != true ).Select( m => m.Person.FullName ) );
                groupIndex.LeaderList = string.Join( ", ", group.Members.Where( m => m.GroupRole.IsLeader == true ).Select( m => m.Person.FullName ) );
            }

            AddIndexableAttributes( groupIndex, group );

            return groupIndex;
        }

        /// <summary>
        /// Formats the search result.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="displayOptions">The display options.</param>
        /// <param name="mergeFields">The merge fields.</param>
        /// <returns></returns>
        public override FormattedSearchResult FormatSearchResult( Person person, Dictionary<string, object> displayOptions = null, Dictionary<string, object> mergeFields = null )
        {
            var result = base.FormatSearchResult( person, displayOptions );

            // check security on the group
            var group = new GroupService(new Data.RockContext()).Get( (int)this.Id );
            if ( group != null )
            {
                result.IsViewAllowed = group.IsAuthorized( Rock.Security.Authorization.VIEW, person );
            }
            else
            {
                result.IsViewAllowed = false;
            }

            return result;
        }
    }
}
