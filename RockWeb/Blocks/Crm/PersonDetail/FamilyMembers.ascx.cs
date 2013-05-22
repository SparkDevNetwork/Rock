//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Xsl;
using Rock;
using Rock.Attribute;
using Rock.Model;

namespace RockWeb.Blocks.Crm.PersonDetail
{
    [TextField( "Xslt File", "XSLT File to use.", false, "PersonDetail/FamilyMembers.xslt" )]
    public partial class FamilyMembers : Rock.Web.UI.PersonBlock
    {
        private XDocument xDocument = null;

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            var groupsElement = new XElement( "groups" );

            Guid familyGroupGuid = new Guid( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY );

            var memberService = new GroupMemberService();
            foreach ( dynamic memberGroup in memberService.Queryable()
                .Where( m =>
                    m.PersonId == Person.Id &&
                    m.Group.GroupType.Guid == familyGroupGuid
                )
                .Select( m => new
                {
                    Group = m.Group,
                    GroupRole = m.GroupRole,
                    GroupType = m.Group.GroupType,
                    GroupLocations = m.Group.GroupLocations
                }
                    ) )
            {
                if ( memberGroup.Group.IsAuthorized( "View", CurrentPerson ) )
                {
                    var groupElement = new XElement( "group",
                        new XAttribute( "id", memberGroup.Group.Id.ToString() ),
                        new XAttribute( "name", memberGroup.Group.Name ),
                        new XAttribute( "role", memberGroup.GroupRole.Name ),
                        new XAttribute( "type", memberGroup.GroupType.Name ),
                        new XAttribute( "type-icon-css-class", memberGroup.GroupType.IconCssClass ?? string.Empty ),
                        new XAttribute( "can-edit", memberGroup.Group.IsAuthorized( "Edit", CurrentPerson ).ToString() )
                        );
                    groupsElement.Add( groupElement );

                    var membersElement = new XElement( "members" );
                    groupElement.Add( membersElement );

                    int groupId = memberGroup.Group.Id;
                    foreach ( dynamic member in memberService.Queryable()
                        .Where( m =>
                            m.GroupId == groupId &&
                            m.PersonId != Person.Id )
                        .OrderBy( m => m.GroupRole.SortOrder )
                        // TODO Update Person.Age to be a computed column
                        //.ThenByDescending( m => m.Person.Age )
                        .Select( m => new
                        {
                            Id = m.PersonId,
                            PhotoId = m.Person.PhotoId.HasValue ? m.Person.PhotoId.Value : 0,
                            FirstName = m.Person.FirstName,
                            LastName = m.Person.LastName,
                            Role = m.GroupRole.Name,
                            SortOrder = m.GroupRole.SortOrder
                        }
                            ).ToList() )
                    {
                        string imageFormat = AppPath + "image.ashx?id={0}";

                        membersElement.Add( new XElement( "member",
                            new XAttribute( "id", member.Id.ToString() ),
                            new XAttribute( "photo-id", member.PhotoId ),
                            new XAttribute( "photo-url", member.PhotoId != 0 ? string.Format( imageFormat, member.PhotoId ) : string.Empty ),
                            new XAttribute( "first-name", member.FirstName ),
                            new XAttribute( "last-name", member.LastName ),
                            new XAttribute( "role", member.Role )
                            ) );
                    }

                    var locationsElement = new XElement( "locations" );
                    groupElement.Add( locationsElement );

                    foreach ( GroupLocation groupLocation in memberGroup.GroupLocations )
                    {
                        var locationElement = new XElement( "location",
                            new XAttribute( "id", groupLocation.LocationId.ToString() ),
                            new XAttribute( "type", groupLocation.GroupLocationTypeValueId.HasValue ?
                                Rock.Web.Cache.DefinedValueCache.Read( groupLocation.GroupLocationTypeValueId.Value ).Name : "Unknown" ) );
                        if ( groupLocation.Location != null )
                        {
                            var addressElement = new XElement( "address" );
                            if ( !String.IsNullOrWhiteSpace( groupLocation.Location.Street1 ) )
                            {
                                addressElement.Add( new XAttribute( "street1", groupLocation.Location.Street1 ) );
                            }
                            if ( !String.IsNullOrWhiteSpace( groupLocation.Location.Street2 ) )
                            {
                                addressElement.Add( new XAttribute( "street2", groupLocation.Location.Street2 ) );
                            }
                            addressElement.Add(
                                new XAttribute( "city", groupLocation.Location.City ),
                                new XAttribute( "state", groupLocation.Location.State ),
                                new XAttribute( "zip", groupLocation.Location.Zip ) );
                            locationElement.Add( addressElement );
                        }
                        locationsElement.Add( locationElement );
                    }
                }

                xDocument = new XDocument( new XDeclaration( "1.0", "UTF-8", "yes" ), groupsElement );

                xmlContent.DocumentContent = xDocument.ToString();
                xmlContent.TransformSource = Server.MapPath( "~/Themes/" + CurrentPage.Site.Theme + "/Assets/Xslt/" + GetAttributeValue( "XsltFile" ) );
            }
        }
    }
}