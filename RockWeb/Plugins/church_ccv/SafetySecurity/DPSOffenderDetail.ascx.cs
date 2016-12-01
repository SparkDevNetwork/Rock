using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using church.ccv.SafetySecurity.Model;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Plugins.church_ccv.SafetySecurity
{
    /// <summary>
    /// Displays the details of a Referral Agency.
    /// </summary>
    [DisplayName( "DPS Offender Detail" )]
    [Category( "CCV > Safety and Security" )]
    [Description( "Displays the details of a DPS Offender record." )]

    public partial class DPSOffenderDetail : RockBlock, IDetailBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( "DPSOffenderId" ).AsInteger() );
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            //
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="itemId">The item id value.</param>
        public void ShowDetail( int itemId )
        {
            var rockContext = new RockContext();

            var groupTypeFamilyId = GroupTypeCache.GetFamilyGroupType().Id;
            var qryPerson = new PersonService( rockContext ).Queryable();
            var qryGroupLocations = new GroupLocationService( rockContext ).Queryable();
            var groupLocationTypeValueHomeId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() ).Id;
            var groupTypeIdFamily = GroupTypeCache.GetFamilyGroupType().Id;
            var qryHomeAddress = new GroupLocationService( rockContext ).Queryable()
                .Where( a => a.GroupLocationTypeValueId == groupLocationTypeValueHomeId && a.Group.GroupTypeId == groupTypeIdFamily && !string.IsNullOrEmpty( a.Location.PostalCode ) );

            var dpsOffender = new DPSOffenderService( rockContext ).Get( itemId );
            bool matchZip = cbMatchZip.Checked;
            bool matchAge = cbAge.Checked;

            DateTime today = RockDateTime.Today;

            if ( dpsOffender != null )
            {
                hfDPSOffenderId.Value = dpsOffender.Id.ToString();
                hfDPSOffenderPersonAliasId.Value = dpsOffender.PersonAliasId.ToString();

                lDPSOffenderInfoCol1.Text = new DescriptionList()
                    .Add( "Name", string.Format( "{0} {1} {2}", dpsOffender.FirstName, dpsOffender.MiddleInitial, dpsOffender.LastName ) )
                    .Add( "Age", dpsOffender.Age )
                    .Add( "Gender", dpsOffender.Gender )
                    .Add( "Level", dpsOffender.Level )
                    .Html;

                lDPSOffenderInfoCol2.Text = new DescriptionList()
                    .Add( "Address", dpsOffender.DpsLocation )
                    .Add( "Date Convicted", dpsOffender.DateConvicted, "d" )
                    .Add( "Offense", dpsOffender.Offense )
                    .Add( "Absconder", dpsOffender.Absconder )
                    .Html;

                lDPSOffenderInfoCol3.Text = new DescriptionList()
                    .Add( "Hair/Eyes", string.Format( "{0}/{1}", dpsOffender.Hair, dpsOffender.Eyes ) )
                    .Add( "Race", dpsOffender.Race )
                    .Add( "Height", dpsOffender.Height )
                    .Add( "Weight", dpsOffender.Weight )
                    .Html;

                pnlDetails.Visible = true;
                var qryPotentialMatches = qryPerson
                    .Where( p => ( !p.BirthDate.HasValue || ( SqlFunctions.DateDiff( "year", p.BirthDate.Value, today ) > 17 ) ) )
                    .Where( p => p.FirstName.StartsWith( dpsOffender.FirstName.Substring( 0, 1 ) ) || p.NickName.StartsWith( dpsOffender.FirstName.Substring( 0, 1 ) ) )
                    .Where( p => dpsOffender.LastName == p.LastName )
                    .Where( p => ( p.Gender == Gender.Unknown ) || dpsOffender.Gender == ( p.Gender == Gender.Male ? "M" : "F" ) )
                    .Where( p => !matchZip || ( dpsOffender.ResZip != "0" && qryHomeAddress.Any( x => x.Group.Members.Any( gm => gm.PersonId == p.Id ) && x.Location.PostalCode.StartsWith( dpsOffender.ResZip ) ) ) )
                    .Where( p => !matchAge || !p.BirthDate.HasValue || ( p.BirthDate.HasValue && dpsOffender.Age.HasValue &&
                        SqlFunctions.DateDiff( "year", p.BirthDate.Value, today ) > ( dpsOffender.Age.Value - 3 ) && SqlFunctions.DateDiff( "year", p.BirthDate.Value, today ) < ( dpsOffender.Age.Value + 3 ) ) );

                var potentialMatchList = qryPotentialMatches.ToList();
                var fullNameMatches = potentialMatchList.Where( a => a.FirstName.Equals( dpsOffender.FirstName, StringComparison.OrdinalIgnoreCase )
                    || a.NickName.Equals( dpsOffender.FirstName, StringComparison.OrdinalIgnoreCase ) ).ToList();

                var fullNameAndAddressMatches = new List<Person>();
                foreach ( var person in fullNameMatches )
                {
                    var address = person.GetHomeLocation( new RockContext() );
                    if ( address != null && address.Id == dpsOffender.DpsLocationId )
                    {
                        fullNameAndAddressMatches.Add( person );
                    }
                }

                // put fullName and Address matches first
                var sortedList = fullNameAndAddressMatches;

                // put fullname matches next
                sortedList.AddRange( fullNameMatches.Where( a => !sortedList.Any( b => b.Id == a.Id ) ) );

                // any other matches last
                sortedList.AddRange( potentialMatchList.Where( a => !sortedList.Any( b => b.Id == a.Id ) ) );

                gPotentialMatches.DataSource = sortedList;
                gPotentialMatches.DataBind();
            }
        }

        /// <summary>
        /// Handles the RowDataBound event of the gPotentialMatches control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gPotentialMatches_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            Person person = e.Row.DataItem as Person;
            if ( person != null )
            {
                using ( var rockContext = new RockContext() )
                {
                    Literal lAddressInfo = e.Row.FindControl( "lAddressInfo" ) as Literal;
                    if ( lAddressInfo != null )
                    {
                        var homeAddress = person.GetHomeLocation( rockContext );
                        if ( homeAddress != null )
                        {
                            lAddressInfo.Text = homeAddress.ToString();
                        }
                    }

                    Literal lPersonImage = e.Row.FindControl( "lPersonImage" ) as Literal;
                    if ( lPersonImage != null )
                    {
                        lPersonImage.Text = Person.GetPersonPhotoImageTag( person, 100, 100 );
                    }

                    LinkButton btnIsConfirmMatch = e.Row.FindControl( "btnIsConfirmMatch" ) as LinkButton;
                    if ( btnIsConfirmMatch != null )
                    {
                        int? dpsOffenderPersonAliasId = hfDPSOffenderPersonAliasId.Value.AsIntegerOrNull();
                        if ( dpsOffenderPersonAliasId.HasValue && person.Aliases.Any( a => a.Id == dpsOffenderPersonAliasId.Value ) )
                        {
                            btnIsConfirmMatch.AddCssClass( "btn-danger" );
                            btnIsConfirmMatch.ToolTip = "Confirmed Match to DPS Offender";
                        }
                        else
                        {
                            btnIsConfirmMatch.RemoveCssClass( "btn-danger" );
                            btnIsConfirmMatch.ToolTip = "Not matched to DPS Offender";
                        }
                    }

                    LinkButton btnSetSOAttribute = e.Row.FindControl( "btnSetSOAttribute" ) as LinkButton;
                    if ( btnSetSOAttribute != null )
                    {
                        if ( person.AttributeValues == null )
                        {
                            person.LoadAttributes( rockContext );
                        }

                        if ( person.GetAttributeValue( "SO" ).AsBoolean() )
                        {
                            btnSetSOAttribute.AddCssClass( "btn-danger" );
                            btnSetSOAttribute.ToolTip = "Clear SO Attribute";
                        }
                        else
                        {
                            btnSetSOAttribute.RemoveCssClass( "btn-danger" );
                            btnSetSOAttribute.ToolTip = "Set SO Attribute";
                        }
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Handles the Click event of the btnIsConfirmMatch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnIsConfirmMatch_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();

            int dpsOffenderId = hfDPSOffenderId.Value.AsInteger();
            var dpsOffender = new DPSOffenderService( rockContext ).Get( dpsOffenderId );

            int personId = ( sender as LinkButton ).CommandArgument.AsInteger();
            var person = new PersonService( rockContext ).Get( personId );
            if ( dpsOffender != null && person != null )
            {
                if ( dpsOffender.PersonAliasId.HasValue )
                {
                    dpsOffender.PersonAliasId = null;
                }
                else
                {
                    dpsOffender.PersonAliasId = person.PrimaryAliasId;
                }

                rockContext.SaveChanges();

                ShowDetail( dpsOffenderId );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSetSOAttribute control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSetSOAttribute_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();

            int personId = ( sender as LinkButton ).CommandArgument.AsInteger();
            var person = new PersonService( rockContext ).Get( personId );
            if ( person != null )
            {
                if ( person.AttributeValues == null )
                {
                    person.LoadAttributes( rockContext );
                }

                if ( person.GetAttributeValue( "SO" ).AsBoolean() )
                {
                    person.SetAttributeValue( "SO", false.ToString() );
                }
                else
                {
                    person.SetAttributeValue( "SO", true.ToString() );
                }

                person.SaveAttributeValues( rockContext );

                ShowDetail( hfDPSOffenderId.Value.AsInteger() );
            }
        }

        /// <summary>
        /// Handles the CheckedChanged event of the cbMatchZip control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cbMatchZip_CheckedChanged( object sender, EventArgs e )
        {
            ShowDetail( hfDPSOffenderId.Value.AsInteger() );
        }

        /// <summary>
        /// Handles the CheckedChanged event of the cbAge control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cbAge_CheckedChanged( object sender, EventArgs e )
        {
            ShowDetail( hfDPSOffenderId.Value.AsInteger() );
        }
    }
}