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

            List<Person> personList = null;
            
            if ( !String.IsNullOrWhiteSpace( type ) && !String.IsNullOrWhiteSpace( term ) )
            {
                using ( var uow = new Rock.Data.UnitOfWorkScope() )
                {
                    IQueryable<Person> people = null;

                    var personService = new PersonService();

                    switch ( type.ToLower() )
                    {
                        case ( "name" ):

                            people = personService.GetByFullName( term, true );

                            break;

                        case ( "phone" ):

                            var phoneService = new PhoneNumberService();

                            var personIds = phoneService.Queryable().
                                Where( n => n.Number.Contains( term ) ).
                                Select( n => n.PersonId).Distinct();

                            people = personService.Queryable().
                                Where( p => personIds.Contains( p.Id ) ).
                                OrderBy( p => p.LastName ).ThenBy( p => ( p.FirstName ) );
                                

                            break;

                        case ( "address" ):

                            break;

                        case ( "email" ):

                            people = personService.Queryable().
                                Where( p => p.Email.Contains( term ) ).
                                OrderBy( p => p.LastName ).ThenBy( p => ( p.FirstName ) );

                            break;
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

                    personList = people.ToList();

                }
            }

            if ( personList != null )
            {
                if ( personList.Count == 1 )
                {
                    Response.Redirect( string.Format( "~/Person/{0}", personList[0].Id ), false );
                    Context.ApplicationInstance.CompleteRequest();
                }
                else
                {
                    gPeople.DataSource = personList;
                    gPeople.DataBind();
                }
            }
        }

        #endregion

    }
}