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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Security
{
    /// <summary>
    /// The main Person Profile block the main information about a person 
    /// </summary>
    [DisplayName( "Account Detail" )]
    [Category( "Security" )]
    [Description( "Public block for user to manager their account" )]

    #region Block Attributes

    [LinkedPage( "Detail Page",
        Key = AttributeKey.DetailPage,
        Description = "Page to edit account details.",
        Order = 0 )]

    [BooleanField( "Show Home Address",
        Key = AttributeKey.ShowHomeAddress,
        Description = "Shows/hides the home address.",
        Order = 1 )]

    [GroupLocationTypeField( "Location Type",
        Key = AttributeKey.LocationType,
        Description = "The type of location that address should use.",
        GroupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY,
        DefaultValue = Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME,
        IsRequired = false,
        Order = 2 )]

    #endregion Block Attributes

    public partial class AccountDetail : RockBlock
    {
        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string ShowHomeAddress = "ShowHomeAddress";
            public const string LocationType = "LocationType";
        }

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddCSSLink( "~/Styles/fluidbox.css" );
            RockPage.AddScriptLink( "~/Scripts/imagesloaded.min.js" );
            RockPage.AddScriptLink( "~/Scripts/jquery.fluidbox.min.js" );

            if ( CurrentPerson != null )
            {
                lName.Text = CurrentPerson.FullName;

                // Setup Image
                var imgTag = new LiteralControl( Rock.Model.Person.GetPersonPhotoImageTag( CurrentPerson, 188, 188 ) );
                if ( CurrentPerson.PhotoId.HasValue )
                {
                    var imgLink = new HyperLink();
                    imgLink.Attributes.Add( "href", CurrentPerson.PhotoUrl );
                    phImage.Controls.Add( imgLink );
                    imgLink.Controls.Add( imgTag );
                }
                else
                {
                    phImage.Controls.Add( imgTag );
                }

                if ( CurrentPerson.BirthDate.HasValue )
                {
                    var dtf = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat;
                    string mdp = dtf.ShortDatePattern;
                    mdp = mdp.Replace( dtf.DateSeparator + "yyyy", "" ).Replace( "yyyy" + dtf.DateSeparator, "" );

                    string ageText = ( CurrentPerson.BirthYear.HasValue && CurrentPerson.BirthYear != DateTime.MinValue.Year ) ?
                        string.Format( "{0} yrs old ", CurrentPerson.BirthDate.Value.Age() ) : string.Empty;
                    lAge.Text = string.Format( "{0}<small>({1})</small><br/>", ageText, CurrentPerson.BirthDate.Value.ToMonthDayString() );
                }

                lGender.Text = CurrentPerson.Gender != Gender.Unknown ? CurrentPerson.Gender.ToString() : string.Empty;

                if ( CurrentPerson.PhoneNumbers != null )
                {
                    rptPhones.DataSource = CurrentPerson.PhoneNumbers.ToList();
                    rptPhones.DataBind();
                }

                lEmail.Text = CurrentPerson.Email;

                Guid? locationTypeGuid = GetAttributeValue( AttributeKey.LocationType ).AsGuidOrNull();
                if ( locationTypeGuid.HasValue )
                {
                    var addressTypeDv = DefinedValueCache.Get( locationTypeGuid.Value );

                    var familyGroupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuidOrNull();

                    if ( familyGroupTypeGuid.HasValue )
                    {
                        var familyGroupType = GroupTypeCache.Get( familyGroupTypeGuid.Value );

                        RockContext rockContext = new RockContext();
                        var address = new GroupLocationService( rockContext ).Queryable()
                                            .Where( l => l.Group.GroupTypeId == familyGroupType.Id
                                                 && l.GroupLocationTypeValueId == addressTypeDv.Id
                                                 && l.Group.Members.Any( m => m.PersonId == CurrentPerson.Id ) )
                                            .Select( l => l.Location )
                                            .FirstOrDefault();
                        if ( address != null )
                        {
                            lAddress.Text = string.Format( "<div class='margin-b-md'><small>{0} Address</small><br />{1}</div>", addressTypeDv.Value, address.FormattedHtmlAddress );
                        }
                    }
                }

                if ( GetAttributeValue( AttributeKey.ShowHomeAddress ).AsBoolean() )
                {
                    var homeAddress = CurrentPerson.GetHomeLocation();
                    if ( homeAddress != null )
                    {
                        
                    }
                }
            }
            else
            {
                pnlView.Visible = false;
                nbNotAuthorized.Visible = true;
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the lbEditPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEditPerson_Click( object sender, EventArgs e )
        {
            if ( CurrentPerson != null )
            {
                NavigateToLinkedPage( AttributeKey.DetailPage );
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Formats the phone number.
        /// </summary>
        /// <param name="countryCode">The country code.</param>
        /// <param name="number">The number.</param>
        /// <returns></returns>
        protected string FormatPhoneNumber( object countryCode, object number )
        {
            string cc = countryCode as string ?? string.Empty;
            string n = number as string ?? string.Empty;
            return PhoneNumber.FormattedNumber( cc, n );
        }

        #endregion

    }
}