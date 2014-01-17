// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using Rock.Model;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Groups
{
    [DisplayName( "Group Search" )]
    [Category( "Groups" )]
    [Description( "Handles displaying group search results and redirects to the group detail page (via route ~/Group/) when only one match was found." )]
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
                                Where( g => ( g.Name ).Contains( term ) ).
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