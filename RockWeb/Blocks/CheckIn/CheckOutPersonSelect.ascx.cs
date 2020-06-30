// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
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
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Model;

namespace RockWeb.Blocks.CheckIn
{
    [DisplayName("Check Out Person Select")]
    [Category("Check-in")]
    [Description("Lists people who match the selected family and provides option of selecting multiple people to check-out.")]

    [TextField( "Title", "Title to display. Use {0} for family name", false, "{0} Check Out", "Text", 5 )]
    [TextField( "Caption", "", false, "Select People", "Text", 6 )]
    public partial class CheckOutPersonSelect : CheckInBlock
    {
        bool _hidePhotos = false;

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

        $('a.btn-checkin-select').on('click', function() {{
            $(this).toggleClass('active').find('i').toggleClass('fa-check-square').toggleClass('fa-square-o');
        }});

", lbSelect.ClientID, hfPeople.ClientID );
            ScriptManager.RegisterStartupScript( Page, Page.GetType(), "SelectPerson", script, true );
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

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
                    foreach ( var person in family.CheckOutPeople )
                    {
                        person.Selected = selectedPersonIds.Contains( person.Person.Id );
                    }

                    ProcessSelection( maWarning );
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

        protected string GetSelectedClass( bool selected )
        {
            return selected ? "active" : "";
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