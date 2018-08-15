using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.org_newpointe.NFCI
{
    /// <summary>
    /// Block for adding new families
    /// </summary>
    [DisplayName( "NFCI Edit Family" )]
    [Category( "NewPointe NFCI" )]
    [Description( "Allows for quickly editing a Family" )]

    [LinkedPage( "Person Details Page", "The page to use to show person details.", false, "", "", 0 )]
    [LinkedPage( "Workflow Entry Page", "The Workflow Entry page.", false, "", "", 1)]
    [WorkflowTypeField("Request Change Workflow", "The type of workflow to launch for a change request.", false, false, "", "", 2 )]
    public partial class EditFamily : Rock.Web.UI.RockBlock
    {

        RockContext rContext = new RockContext();
        Group family;
        Person person;

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            family = new GroupService( rContext ).Get( PageParameter( "GroupGuid" ).AsGuid() );
            person = new PersonService( rContext ).Get( PageParameter( "PersonGuid" ).AsGuid() );

            if ( !Page.IsPostBack )
            {
                if(family != null && family.GroupType.Guid.Equals(Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid()) && family.IsAuthorized( "View", CurrentPerson ))
                {
                    pFamilyInfo.Visible = true;
                    pFamilyError.Visible = false;
                    BindControls();
                }
                else
                {
                    pFamilyInfo.Visible = false;
                    pFamilyError.Visible = true;
                }
            }
        }

        protected void BindControls()
        {
            if(family != null)
            {

                rtbGroupName.Text = family.Name;

                gMembers.DataSource = family.Members.Select(m => new { m.Person.FullName, m.Person.Age, m.Person.PhotoUrl, RoleType = m.GroupRole.Name } ).ToList();
                gMembers.DataBind();

                gAddresses.DataSource = family.GroupLocations.ToList();
                gAddresses.DataBind();

            }
        }

        protected void lbSubmit_Click( object sender, EventArgs e )
        {
            if ( Page.IsValid )
            {
                var rockContext = new RockContext();

                rockContext.WrapTransaction( () =>
                {
                    if(family != null)
                    {
                        family.Name = rtbGroupName.Text;
                        rockContext.SaveChanges();

                        if ( person != null )
                        {
                            if ( !String.IsNullOrWhiteSpace( GetAttributeValue( "PersonDetailsPage" ) ) )
                            {
                                NavigateToLinkedPage( "PersonDetailsPage", new Dictionary<string, string>() { { "PersonId", person.Id.ToString() } } );
                            }
                            else
                            {
                                NavigateToCurrentPage( new Dictionary<string, string>() { { "PersonId", person.Id.ToString() } } );
                            }
                        }
                        else
                        {
                            if ( !String.IsNullOrWhiteSpace( GetAttributeValue( "PersonDetailsPage" ) ) )
                            {
                                NavigateToLinkedPage( "PersonDetailsPage" );
                            }
                            else
                            {
                                NavigateToCurrentPage( );
                            }
                        }
                    }
                } );
            }
        }

        protected void lbRequestChange_Click( object sender, EventArgs e )
        {
            var pars = new Dictionary<string, string>();

            var wfType = new WorkflowTypeService( rContext ).Get( GetAttributeValue( "RequestChangeWorkflow" ).AsGuid() );
            if ( wfType != null )
            {
                pars.Add( "WorkflowTypeId", wfType.Id.ToString() );
            }

            if ( family != null )
            {
                pars.Add( "GroupId", family.Id.ToString() );
            }

            NavigateToLinkedPage( "WorkflowEntryPage", pars );
        }
    }
}