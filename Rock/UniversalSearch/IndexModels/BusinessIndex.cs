using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.UniversalSearch.IndexModels
{
    public class BusinessIndex : IndexModelBase
    {
        public string Name { get; set; }
        
        public string Contacts { get; set; }

        public override string IconCssClass
        {
            get
            {
                return "fa fa-building";
            }
        }

        public static BusinessIndex LoadByModel(Person business )
        {
            var businessIndex = new BusinessIndex();
            businessIndex.SourceIndexModel = "Rock.Model.Person";

            businessIndex.Id = business.Id;
            businessIndex.Name = business.LastName;
            
            // do not currently index business attributes since they are shared with people
            //AddIndexableAttributes( businessIndex, person );

            var knownRelationshipGroupType = GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS.AsGuid() );
            var knownRelationshipOwnerRoleId = knownRelationshipGroupType.Roles.Where( r => r.Guid == SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER.AsGuid() ).FirstOrDefault().Id;
            var knownRelationshipBusinessContactId = knownRelationshipGroupType.Roles.Where( r => r.Guid == SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_BUSINESS_CONTACT.AsGuid() ).FirstOrDefault().Id;

            RockContext rockContext = new RockContext();
            var contactGroup = new GroupMemberService( rockContext ).Queryable()
                                        .Where( m =>
                                             m.Group.GroupTypeId == knownRelationshipGroupType.Id
                                             && m.GroupRoleId == knownRelationshipOwnerRoleId
                                             && m.PersonId == business.Id)
                                        .FirstOrDefault();

            if ( contactGroup != null )
            {
                var contacts = new GroupMemberService( rockContext ).Queryable().AsNoTracking()
                                    .Where( m =>
                                         m.Group.GroupTypeId == knownRelationshipGroupType.Id
                                         && m.GroupId == contactGroup.GroupId
                                         && m.GroupRoleId == knownRelationshipBusinessContactId )
                                    .Select( m => m.Person.NickName + " " + m.Person.LastName ).ToList();

                if ( contacts != null )
                {
                    businessIndex.Contacts = string.Join( " ", contacts );
                }
            }

            return businessIndex;
        }

        /// <summary>
        /// Formats the search result.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        public override FormattedSearchResult FormatSearchResult( Person person, Dictionary<string, object> displayOptions = null )
        {
            string url = "/Business/";

            if (displayOptions != null )
            {
                if ( displayOptions.ContainsKey( "Business.Url" ) )
                {
                    url = displayOptions["Business.Url"].ToString();
                }
            }

            return new FormattedSearchResult() { IsViewAllowed = true, FormattedResult = string.Format( "<a href='{0}{1}'>{2} <small>(Business)</small></a>", url, this.Id, this.Name ) };
        }
    }
}
