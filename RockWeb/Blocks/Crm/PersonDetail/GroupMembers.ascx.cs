//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;

using Rock;
using Rock.Groups;

namespace RockWeb.Blocks.Crm.PersonDetail
{
	[Rock.Attribute.Property( 0, "Group Type", "GroupType", "Behavior", "The type of group to display.  Any group of this type that person belongs to will be displayed", false, "0", "Rock", "Rock.Field.Types.Integer" )]
	public partial class GroupMembers : Rock.Web.UI.PersonBlock
	{
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

			var service = new MemberService();

			foreach ( dynamic groupItem in service.Queryable()
				.Where( m => m.PersonId == Person.Id && m.Group.GroupTypeId == GroupTypeId )
				.OrderByDescending( m => m.Group.CreatedDateTime )
				.Select( m => new
				{
					Id = m.GroupId,
					Name = m.Group.Name,
					Role = m.GroupRole.Name,
					Type = m.Group.GroupType.Name
				}
					) )
			{
				var section = new HtmlGenericControl( "section" );
				phGroups.Controls.Add( section );
				section.AddCssClass( ((string)groupItem.Type).ToLower() );
				section.AddCssClass( "group" );

				var header = new HtmlGenericControl( "header" );
				section.Controls.Add( header );
				header.Controls.Add( new LiteralControl( string.Format( "{0} ({1})", groupItem.Name, groupItem.Role ) ) );

				var editLink = new HtmlGenericControl( "a" );
				header.Controls.Add( editLink );
				editLink.Attributes.Add( "href", "#" );
				editLink.AddCssClass( "edit" );

				var editIcon = new HtmlGenericControl( "i" );
				editLink.Controls.Add( editIcon );
				editIcon.AddCssClass( "icon-edit" );

				var ul = new HtmlGenericControl( "ul" );
				section.Controls.Add( ul );

				int groupId = groupItem.Id;
				foreach ( dynamic memberItem in service.Queryable()
					.Where( m => m.GroupId == groupId && m.PersonId != Person.Id )
					.Select( m => new
					{
						Id = m.PersonId,
						PhotoId = m.Person.PhotoId.HasValue ? m.Person.PhotoId.Value : 0,
						Name = m.Person.NickName ?? m.Person.GivenName,
						Role = m.GroupRole.Name,
						Order = m.GroupRole.Order
					}
						).ToList().OrderBy( m => m.Order) )
				{
					var li = new HtmlGenericControl( "li" );
					ul.Controls.Add( li );

					var anchor = new HtmlAnchor();
					li.Controls.Add( anchor );
					anchor.HRef = string.Format( "~/Person/{0}", memberItem.Id );

					if ( memberItem.PhotoId != 0 )
					{
						var img = new HtmlImage();
						anchor.Controls.Add( img );
						img.Src = string.Format( "~/image.ashx?id={0}&maxwidth=38&maxheight=38", memberItem.PhotoId );
					}

					var h4 = new HtmlGenericControl( "h4" );
					anchor.Controls.Add( h4 );
					h4.InnerText = memberItem.Name;

					var small = new HtmlGenericControl( "small" );
					anchor.Controls.Add( small );
					small.InnerText = memberItem.Role;
				}
			}
		}
	}
}