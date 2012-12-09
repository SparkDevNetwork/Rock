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
    [IntegerField( 0, "Group Type", "0", null, "Behavior", "The type of group to display.  Any group of this type that person belongs to will be displayed")]
    [TextField( 1, "Group Role Filter", "Behavior", "Delimited list of group role id's that if entered, will only show groups where selected person is one of the roles.", false, "" )]
    [BooleanField( 2, "Include Self", false, null, "Behavior", "Should the current person be included in list of group members?")]
    [BooleanField( 3, "Include Locations", false, null, "Behavior", "Should locations be included?")]
    [TextField( 4, "Xslt File", "Behavior", "XSLT File to use.", false, "GroupMembers.xslt" )]
    public partial class GroupMembers : Rock.Web.UI.PersonBlock
    {
        private XDocument xDocument = null;

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            int GroupTypeId = 0;

            if ( !Int32.TryParse( AttributeValue( "GroupType" ), out GroupTypeId ) )
                GroupTypeId = 0;

            if ( GroupTypeId == 0 )
                GroupTypeId = new GroupTypeService().Queryable()
                    .Where( g => g.Guid == Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY )
                    .Select( g => g.Id )
                    .FirstOrDefault();

            var filterRoles = new List<int>();
            foreach ( string stringRoleId in AttributeValue( "GroupRoleFilter" ).SplitDelimitedValues() )
            {
                int roleId = 0;
                if ( Int32.TryParse( stringRoleId, out roleId ) )
                {
                    filterRoles.Add( roleId );
                }
            }

            var groupsElement = new XElement( "groups" );

            var memberService = new GroupMemberService();
            foreach ( dynamic group in memberService.Queryable()
                .Where( m =>
                    m.PersonId == Person.Id &&
                    m.Group.GroupTypeId == GroupTypeId &&
                    ( filterRoles.Count == 0 || filterRoles.Contains( m.GroupRoleId ) )
                )
                .Select( m => new
                {
                    Id = m.GroupId,
                    Name = m.Group.Name,
                    Role = m.GroupRole.Name,
                    Type = m.Group.GroupType.Name,
                    Locations = m.Group.Locations,
                }
                    ) )
            {
                var groupElement = new XElement( "group",
                    new XAttribute( "id", group.Id.ToString() ),
                    new XAttribute( "name", group.Name ),
                    new XAttribute( "role", group.Role ),
                    new XAttribute( "type", group.Type ),
                    new XAttribute( "class-name", group.Type.ToLower().Replace(' ', '-') )
                    );
                groupsElement.Add( groupElement );

                var membersElement = new XElement( "members" );
                groupElement.Add( membersElement );

                bool includeSelf = false;
                if (!Boolean.TryParse(AttributeValue("IncludeSelf"), out includeSelf))
                {
                    includeSelf = false;
                }

                int groupId = group.Id;
                foreach ( dynamic member in memberService.Queryable()
                    .Where( m => 
                        m.GroupId == groupId && 
                        (includeSelf || m.PersonId != Person.Id ))
                    .Select( m => new
                    {
                        Id = m.PersonId,
                        PhotoId = m.Person.PhotoId.HasValue ? m.Person.PhotoId.Value : 0,
                        FirstName = m.Person.NickName ?? m.Person.GivenName,
                        LastName = m.Person.LastName,
                        Role = m.GroupRole.Name,
                        SortOrder = m.GroupRole.SortOrder
                    }
                        ).ToList().OrderBy( m => m.SortOrder ) )
                {
                    membersElement.Add( new XElement( "member",
                        new XAttribute( "id", member.Id.ToString() ),
                        new XAttribute( "photo-id", member.PhotoId ),
                        new XAttribute( "first-name", member.FirstName ),
                        new XAttribute( "last-name", member.LastName ),
                        new XAttribute( "role", member.Role )
                        ) );
                }

                if ( Convert.ToBoolean( AttributeValue( "IncludeLocations" ) ) )
                {
                    var locationsElement = new XElement( "locations" );
                    groupElement.Add( locationsElement );

                    foreach ( GroupLocation location in group.Locations )
                    {
                        var locationElement = new XElement( "location",
                            new XAttribute( "id", location.LocationId.ToString() ),
                            new XAttribute( "type", location.LocationTypeValueId.HasValue ?
                                Rock.Web.Cache.DefinedValueCache.Read( location.LocationTypeValueId.Value ).Name : "Unknown" ) );
                        if ( location.Location != null )
                        {
                            var addressElement = new XElement( "address" );
                            if ( !String.IsNullOrWhiteSpace( location.Location.Street1 ) )
                            {
                                addressElement.Add( new XAttribute( "street1", location.Location.Street1 ) );
                            }
                            if ( !String.IsNullOrWhiteSpace( location.Location.Street2 ) )
                            {
                                addressElement.Add( new XAttribute( "street2", location.Location.Street2 ) );
                            }
                            addressElement.Add(
                                new XAttribute( "city", location.Location.City ),
                                new XAttribute( "state", location.Location.State ),
                                new XAttribute( "zip", location.Location.Zip ) );
                            locationElement.Add( addressElement );
                        }
                        locationsElement.Add( locationElement );
                    }
                }

                xDocument = new XDocument( new XDeclaration( "1.0", "UTF-8", "yes" ), groupsElement );
            }
        }

        protected override void Render( System.Web.UI.HtmlTextWriter writer )
        {
            try
            {
                if ( xDocument != null && !String.IsNullOrEmpty( AttributeValue( "XsltFile" ) ) )
                {
                    string xsltFile = AttributeValue( "XsltFile" );
                    if ( !String.IsNullOrEmpty( xsltFile ) )
                    {
                        string xsltPath = Server.MapPath( "~/Themes/" + CurrentPage.Site.Theme + "/Assets/Xslt/" + AttributeValue( "XsltFile" ) );
                        var xslt = new XslCompiledTransform();
                        xslt.Load( xsltPath );
                        xslt.Transform( xDocument.CreateReader(), null, writer );
                    }
                }
            }
            catch ( Exception ex )
            {
                writer.Write( "Error: " + ex.Message );
            }
        }
    }
}