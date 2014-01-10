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
using Rock.Attribute;
using Rock.Model;
using Rock.Web.UI.Controls;
using System.Text.RegularExpressions;

namespace RockWeb.Blocks.Crm
{
    [LinkedPage("Person Detail Page")]
    public partial class PersonSearch : Rock.Web.UI.RockBlock
    {
        #region Base Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gPeople.DataKeyNames = new string[] { "id" };
            gPeople.Actions.ShowAdd = false;
            gPeople.GridRebind += gPeople_GridRebind;
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
                            var personIds = phoneService.GetPersonIdsByNumber( term );

                            people = personService.Queryable().Where( p => personIds.Contains( p.Id ) );
                                
                            break;

                        case ( "address" ):

                            var groupMemberService = new GroupMemberService();

                            var personIds2 = groupMemberService.GetPersonIdsByHomeAddress( term );
                            people = personService.Queryable().Where( p => personIds2.Contains( p.Id ) );

                            break;

                        case ( "email" ):

                            people = personService.Queryable().Where( p => p.Email.Contains( term ) );

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