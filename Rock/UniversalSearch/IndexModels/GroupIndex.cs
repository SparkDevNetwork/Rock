using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Model;
using Rock.UniversalSearch.IndexModels.Attributes;

namespace Rock.UniversalSearch.IndexModels
{
    public class GroupIndex : IndexModelBase
    {
        [RockIndexField]
        public int GroupTypeId { get; set; }

        [RockIndexField( )]
        public string GroupTypeName { get; set; }

        public int ParentGroupId { get; set; }

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

        [RockIndexField( Boost = 4 )]
        public string Name { get; set; }

        [RockIndexField( Boost = 3 )]
        public string Description { get; set; }

        [RockIndexField()]
        public string MemberList { get; set; }

        [RockIndexField( Boost = 2 )]
        public string LeaderList { get; set; }

        public static GroupIndex LoadByModel( Group group )
        {
            var groupIndex = new GroupIndex();
            groupIndex.SourceIndexModel = "Rock.Model.Group";

            groupIndex.Id = group.Id;
            groupIndex.Name = group.Name;
            groupIndex.Description = group.Description;
            groupIndex.GroupTypeId = group.GroupTypeId;

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
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        public override FormattedSearchResult FormatSearchResult( Person person, Dictionary<string, object> displayOptions = null )
        {
            return new FormattedSearchResult() { IsViewAllowed = true, FormattedResult = string.Format( "<a href='/Group/{0}'>{1} <small>({2})</small></a>", this.Id, this.Name, this.GroupTypeName ) };
        }
    }
}
