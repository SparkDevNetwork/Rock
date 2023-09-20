using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Model;

namespace RockWeb.Plugins.com_kfs.CheckIn
{
    [DisplayName("Check Out Person Select")]
    [Category("KFS > Check-in")]
    [Description("Lists people who match the selected family and provides option of selecting multiple people to check-out.")]

    [TextField( "Title", "Title to display. Use {0} for family name", false, "{0} Check Out", "Text", 5 )]
    [TextField( "Caption", "", false, "Select People", "Text", 6 )]
    [BooleanField( "Check Out disabled by attribute key", "Should check out be prevented by the following attribute key having a value.", false, "Prevent Checkout", 7, "CheckoutDisabled" )]
    [TextField( "Attribute Key", "", false, "PagerId", "Prevent Checkout", 8 )]
    [CodeEditorField( "Attribute Error Text", "Error message displayed when the attribute is not empty and a person attempts to check out.", defaultValue: "We're sorry, you cannot check out via this kiosk at this time, please see the info desk for more information.", category: "Prevent Checkout", order: 9 )]
    public partial class CheckOutPersonSelect : CheckInBlock
    {
        bool _hidePhotos = false;
        bool _checkoutdisabled = false;

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            rSelection.ItemDataBound += rSelection_ItemDataBound;

            string script = string.Format( @"
        function GetPersonSelection() {{
            var ids = '';
            $('div.checkin-person-list').find('i.fa-check-square').each( function() {{
                ids += $(this).closest('a').attr('person-id') + ',';
            }});
            if (ids == '') {{
                bootbox.alert('Please select at least one person');
                return false;
            }}
            else
            {{
                $('#{0}').button('loading')
                $('#{1}').val(ids);
                return true;
            }}
        }}

        $('a.btn-checkin-select').click( function() {{
            //$(this).toggleClass('btn-dimmed');
            $(this).find('i').toggleClass('fa-check-square').toggleClass('fa-square-o');
        }});

", lbSelect.ClientID, hfPeople.ClientID );
            ScriptManager.RegisterStartupScript( Page, Page.GetType(), "SelectPerson", script, true );
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            _checkoutdisabled = GetAttributeValue( "CheckoutDisabled" ).AsBoolean();

            RockPage.AddScriptLink( "~/Scripts/CheckinClient/checkin-core.js" );

            var bodyTag = this.Page.Master.FindControl( "bodyTag" ) as HtmlGenericControl;
            if ( bodyTag != null )
            {
                bodyTag.AddCssClass( "checkin-checkoutpersonselect-bg" );
            }

            if ( CurrentWorkflow == null || CurrentCheckInState == null )
            {
                NavigateToHomePage(); 
            }
            else
            {
                if ( !Page.IsPostBack )
                {
                    ClearSelection();

                    var family = CurrentCheckInState.CheckIn.CurrentFamily;
                    if ( family == null )
                    {
                        GoBack();
                    }

                    lTitle.Text = string.Format( GetAttributeValue( "Title" ), family.ToString() );
                    lCaption.Text = GetAttributeValue( "Caption" );

                    _hidePhotos = CurrentCheckInState.CheckInType.TypeOfCheckin == TypeOfCheckin.Individual || CurrentCheckInState.CheckInType.HidePhotos;

                    rSelection.DataSource = family.CheckOutPeople;
                    rSelection.DataBind();
                }
            }
        }

        private void rSelection_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                var pnlPhoto = e.Item.FindControl( "pnlPhoto" ) as Panel;
                pnlPhoto.Visible = !_hidePhotos;

                var pnlPerson = e.Item.FindControl( "pnlPerson" ) as Panel;
                pnlPerson.CssClass = ( _hidePhotos ? "col-md-11 col-sm-10 col-xs-8" : "col-md-10 col-sm-8 col-xs-6" ) + " family-personselect";
            }
        }

        /// <summary>
        /// Clear any previously selected people.
        /// </summary>
        private void ClearSelection()
        {
            foreach ( var family in CurrentCheckInState.CheckIn.Families )
            {
                foreach ( var person in family.CheckOutPeople )
                {
                    person.Selected = true;
                }
            }
        }

        protected void lbSelect_Click( object sender, EventArgs e )
        {
            if ( KioskCurrentlyActive )
            {
                var selectedPersonIds = hfPeople.Value.SplitDelimitedValues().AsIntegerList();

                var family = CurrentCheckInState.CheckIn.CurrentFamily;
                if ( family != null )
                {
                    var attributeExists = false;
                    foreach ( var person in family.CheckOutPeople )
                    {
                        var personRecord = person.Person;
                        personRecord.LoadAttributes();
                        var attributeVal = personRecord.GetAttributeValue( GetAttributeValue( "AttributeKey" ) );
                        if ( _checkoutdisabled && !string.IsNullOrWhiteSpace( attributeVal ) )
                        {
                            attributeExists = true;
                        }
                        else
                        {
                            person.Selected = selectedPersonIds.Contains( person.Person.Id );
                        }
                    }
                    if ( attributeExists )
                    {
                        string errorMsg = GetAttributeValue( "AttributeErrorText" );
                        maWarning.Show( errorMsg, Rock.Web.UI.Controls.ModalAlertType.Warning );
                    }
                    else
                    {
                        ProcessSelection( maWarning );
                    }
                }
            }
        }

        protected void lbBack_Click( object sender, EventArgs e )
        {
            GoBack();
        }

        protected void lbCancel_Click( object sender, EventArgs e )
        {
            CancelCheckin();
        }

        protected void ProcessSelection()
        {
            ProcessSelection( maWarning, false );
        }

        protected string GetCheckboxClass( bool selected )
        {
            return selected ? "fa fa-check-square fa-3x" : "fa fa-square-o fa-3x";
        }

        protected string GetPersonImageTag( object dataitem )
        {
            var person = dataitem as Person;
            if ( person != null )
            {
                return Person.GetPersonPhotoUrl( person, 200, 200 );
            }
            return string.Empty;
        }
    }
}