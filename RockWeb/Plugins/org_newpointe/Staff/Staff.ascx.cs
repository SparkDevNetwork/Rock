using System;
using System.ComponentModel;
using System.Linq;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Web.UI;

namespace RockWeb.Plugins.org_newpointe.Staff
{
    [DisplayName( "Staff" )]
    [Category( "NewPointe.org Web Blocks" )]
    [Description( "This block will display all members of the selected group" )]
    [GroupField( "Root Group", "Select the root group to use as a starting point for the tree view.", false, order: 1 )]
    public partial class Staff : RockBlock
    {
        protected void Page_Load( object sender, EventArgs e )
        {

            Guid? rootGroupGuid = GetAttributeValue( "RootGroup" ).AsGuidOrNull();

            if ( rootGroupGuid != null )
            {
                var staffGroup = new GroupService( new RockContext() ).Get( rootGroupGuid.Value );
                lblGroupName.Text = staffGroup.Name;

                rptStaff.DataSource = staffGroup
                    .Members
                    .OrderByDescending( g => g.GroupMemberStatus )
                    .ThenBy( m => m.Person.LastName )
                    .ThenBy( m => m.Person.FirstName )
                    .Select( m => m.Person )
                    .ToList().Select( person =>
                    {
                        person.LoadAttributes();
                        return new
                        {
                            Name = person.FullName,
                            PhotoUrl = person.PhotoUrl,
                            Position = person.GetAttributeValue( "StaffPosition" )
                        };
                    }
                    );
                rptStaff.DataBind();
            }

        }

        protected void rptStaff_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( ( e.Item.ItemIndex + 1 ) % 6 == 0 )
            {
                e.Item.Controls.Add( new Panel { CssClass = "clearfix visible-lg-block" } );
            }
            if ( ( e.Item.ItemIndex + 1 ) % 4 == 0 )
            {
                e.Item.Controls.Add( new Panel { CssClass = "clearfix visible-md-block" } );
            }
            if ( ( e.Item.ItemIndex + 1 ) % 3 == 0 )
            {
                e.Item.Controls.Add( new Panel { CssClass = "clearfix visible-sm-block" } );
            }
            if ( ( e.Item.ItemIndex + 1 ) % 2 == 0 )
            {
                e.Item.Controls.Add( new Panel { CssClass = "clearfix visible-xs-block" } );
            }
        }
    }
}