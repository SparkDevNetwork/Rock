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
using System.ComponentModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.UI.Controls;
using System.Text.RegularExpressions;
using System.Data.Entity.SqlServer;
using Rock.Data;
using Rock.Web.Cache;

namespace RockWeb.Blocks.Crm
{
    [DisplayName( "Person Search" )]
    [Category( "CRM" )]
    [Description( "Displays list of people that match a given search type and term." )]

    [LinkedPage("Person Detail Page")]
    public partial class PersonSearch : Rock.Web.UI.RockBlock
    {
        #region Fields

        private DefinedValueCache _inactiveStatus = null;

        #endregion

        #region Base Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gPeople.DataKeyNames = new string[] { "Id" };
            gPeople.Actions.ShowAdd = false;
            gPeople.GridRebind += gPeople_GridRebind;
            gPeople.RowDataBound += gPeople_RowDataBound;
            gPeople.PersonIdField = "Id";
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                BindGrid();
            }
        }

        #endregion

        #region Events

        void gPeople_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        void gPeople_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var person = e.Row.DataItem as Person;
                if ( person != null )
                {
                    if (_inactiveStatus != null && 
                        person.RecordStatusValueId.HasValue && 
                        person.RecordStatusValueId == _inactiveStatus.Id)
                    {
                        e.Row.AddCssClass( "inactive" );
                    }

                    if ( person.IsDeceased ?? false )
                    {
                        e.Row.AddCssClass( "deceased" );
                    }
                }
            }
        }

        protected void gPeople_RowSelected( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "PersonDetailPage", "PersonId", (int)e.RowKeyId );
        }

        #endregion

        #region Internal Methods

        private void BindGrid()
        {
            string type = PageParameter( "SearchType" );
            string term = PageParameter( "SearchTerm" );

            if ( !String.IsNullOrWhiteSpace( type ) && !String.IsNullOrWhiteSpace( term ) )
            {
                using ( var rockContext = new RockContext() )
                {
                    var personService = new PersonService( rockContext );
                    IQueryable<Person> people = null;

                    switch ( type.ToLower() )
                    {
                        case ( "name" ):
                            {
                                bool allowFirstNameOnly = false;
                                if ( !bool.TryParse( PageParameter( "allowFirstNameOnly" ), out allowFirstNameOnly ) )
                                {
                                    allowFirstNameOnly = false;
                                }
                                people = personService.GetByFullName( term, allowFirstNameOnly, true );
                                break;
                            }
                        case ( "phone" ):
                            {
                                var phoneService = new PhoneNumberService( rockContext );
                                var personIds = phoneService.GetPersonIdsByNumber( term );
                                people = personService.Queryable().Where( p => personIds.Contains( p.Id ) );
                                break;
                            }
                        case ( "address" ):
                            {
                                var groupMemberService = new GroupMemberService( rockContext );
                                var personIds2 = groupMemberService.GetPersonIdsByHomeAddress( term );
                                people = personService.Queryable().Where( p => personIds2.Contains( p.Id ) );
                                break;
                            }
                        case ( "email" ):
                            {
                                people = personService.Queryable().Where( p => p.Email.Contains( term ) );
                                break;
                            }
                    }

                    SortProperty sortProperty = gPeople.SortProperty;
                    if ( sortProperty != null )
                    {
                        people = people.Sort( sortProperty );
                    }
                    else
                    {
                        people = people.OrderBy( p => p.LastName ).ThenBy( p => p.FirstName );
                    }

                    var personList = people.ToList();

                    if ( personList.Count == 1 )
                    {
                        Response.Redirect( string.Format( "~/Person/{0}", personList[0].Id ), false );
                        Context.ApplicationInstance.CompleteRequest();
                    }
                    else
                    {
                        if ( type.ToLower() == "name" )
                        {
                            var similiarNames = personService.GetSimiliarNames( term,
                                personList.Select( p => p.Id ).ToList(), true );
                            if ( similiarNames.Any() )
                            {
                                var hyperlinks = new List<string>();
                                foreach ( string name in similiarNames.Distinct() )
                                {
                                    var pageRef = CurrentPageReference;
                                    pageRef.Parameters["SearchTerm"] = name;
                                    hyperlinks.Add( string.Format( "<a href='{0}'>{1}</a>", pageRef.BuildUrl(), name ) );
                                }
                                string altNames = string.Join( ", ", hyperlinks );
                                nbNotice.Text = string.Format( "Other Possible Matches: {0}", altNames );
                                nbNotice.Visible = true;
                            }
                        }

                        _inactiveStatus = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE );

                        gPeople.DataSource = personList;
                        gPeople.DataBind();
                    }
                }
            }
        }

        #endregion

}
}