using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Web;
using Rock.Data;
using Rock.Attribute;
using com.reallifeministries.GroupMatching;

namespace com.reallifeministries 
{
    [DisplayName( "Group Matcher Lookup (TEST)" )]
    [Description( "Interface with the GroupMatcher to test it's suggestions" )]
    public partial class GroupMatcherLookup : Rock.Web.UI.RockBlock
    {
        private RockContext _ctx;

        protected void Page_Load(object sender, EventArgs e)
        {
            _ctx = new RockContext();

            if (!Page.IsPostBack)
            {
                bindDefaults();
            }
        }

        protected void bindDefaults()
        {
            pkrGroupType.DataSource = (from gt in _ctx.GroupTypes select gt).ToList();
            pkrGroupType.DataBind();

            
        }

        protected void btnSubmit_Click( object sender, EventArgs e )
        {
            if (Page.IsValid)
            {

                var person_id = pkrPerson.PersonId;
                var group_type_id = Int32.Parse( pkrGroupType.SelectedValue );

                if (person_id != null && group_type_id > 0)
                {

                    var personService = new Rock.Model.PersonService( _ctx );
                    Rock.Model.Person person = personService.Get( (int)person_id );

                    var groupTypeService = new Rock.Model.GroupTypeService( _ctx );
                    Rock.Model.GroupType groupType = groupTypeService.Get( group_type_id );

                    GroupMatcher groupMatcher = new GroupMatcher( person, groupType, pkrDaysOfWeek.SelectedDaysOfWeek );

                    var matches = groupMatcher.GetMatches();

                    grdMatches.DataSource = matches;
                    grdMatches.DataBind();

                    pnlResults.Visible = true;
                }
            }
        }
    }
}
