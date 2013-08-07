//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.UI.WebControls;

using Rock;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Crm
{
    public partial class GroupSearch : Rock.Web.UI.RockBlock
    {
        #region Control Methods

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            BindGrid();
        }

        #endregion

        #region Internal Methods

        private void BindGrid()
        {
            string type = PageParameter( "SearchType" );
            string term = PageParameter( "SearchTerm" );

            var groups = new List<Group>();

            if ( !String.IsNullOrWhiteSpace( type ) && !String.IsNullOrWhiteSpace( term ) )
            {
                using ( var uow = new Rock.Data.UnitOfWorkScope() )
                {
                    var groupService = new GroupService();

                    switch ( type.ToLower() )
                    {
                        case ( "name" ):

                            groups = groupService.Queryable().
                                Where( g => ( g.Name ).StartsWith( term ) ).
                                OrderBy( g => g.Name ).
                                ToList();

                            break;
                    }
                }
            }

            if ( groups.Count == 1 )
            {
                Response.Redirect( string.Format( "~/Group/{0}", groups[0].Id ), false );
                Context.ApplicationInstance.CompleteRequest();
            }
            else
            {
                gGroups.DataSource = groups;
                gGroups.DataBind();
            }
        }

        #endregion

    }
}