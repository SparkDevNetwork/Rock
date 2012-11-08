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
using Rock.Crm;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Crm
{
    public partial class PersonSearch : Rock.Web.UI.RockBlock
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

            var people = new List<Person>();

            if ( !String.IsNullOrWhiteSpace( type ) && !String.IsNullOrWhiteSpace( term ) )
            {

                using ( var uow = new Rock.Data.UnitOfWorkScope() )
                {
                    var personService = new PersonService();

                    switch ( type.ToLower() )
                    {
                        case ( "name" ):

                            string fName = string.Empty;
                            string lName = string.Empty;

                            var names = term.SplitDelimitedValues();

                            bool lastFirst = term.Contains( "," );

                            if ( lastFirst )
                            {
                                // last, first
                                lName = names.Length >= 1 ? names[0] : string.Empty;
                                fName = names.Length >= 2 ? names[1] : string.Empty;
                            }
                            else
                            {
                                // first last
                                fName = names.Length >= 1 ? names[0] : string.Empty;
                                lName = names.Length >= 2 ? names[1] : string.Empty;
                            }

                            people = personService.Queryable().
                                Where( p => ( ( p.NickName ?? p.GivenName ).StartsWith( fName ) && p.LastName.StartsWith( lName ) ) ).
                                OrderBy( p => p.LastName ).ThenBy( p => p.NickName ?? p.GivenName ).
                                ToList();

                            break;

                        case ( "phone" ):

                            var phoneService = new PhoneNumberService();

                            var personIds = phoneService.Queryable().
                                Where( n => n.Number.Contains( term ) ).
                                Select( n => n.PersonId).Distinct();

                            people = personService.Queryable().
                                Where( p => personIds.Contains( p.Id ) ).
                                OrderBy( p => p.LastName ).ThenBy( p => ( p.NickName ?? p.GivenName ) ).
                                ToList();

                            break;

                        case ( "address" ):

                            break;

                        case ( "email" ):

                            people = personService.Queryable().
                                Where( p => p.Email.Contains( term ) ).
                                OrderBy( p => p.LastName ).ThenBy( p => ( p.NickName ?? p.GivenName ) ).
                                ToList();

                            break;
                    }
                }
            }

            if ( people.Count == 1 )
            {
                Response.Redirect( string.Format( "~/Person/{0}", people[0].Id ), false );
                Context.ApplicationInstance.CompleteRequest();
            }
            else
            {
                gPeople.DataSource = people;
                gPeople.DataBind();
            }
        }

        #endregion

    }
}