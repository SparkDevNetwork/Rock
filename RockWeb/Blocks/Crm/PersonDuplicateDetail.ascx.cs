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
using System.Data.Entity;
using System.Linq;
using System.ServiceModel.Activities.Tracking.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Utility.Enums;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Crm
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Person Duplicate Detail" )]
    [Category( "CRM" )]
    [Description( "Shows records that are possible duplicates of the selected person" )]

    #region Block Attributes

    [DecimalField(
        "Confidence Score High",
        Key = AttributeKey.ConfidenceScoreHigh,
        Description = "The minimum confidence score required to be considered a likely match.",
        IsRequired = true,
        DefaultDecimalValue = 80.00,
        Order = 0 )]

    [DecimalField(
        "Confidence Score Low",
        Key = AttributeKey.ConfidenceScoreLow,
        Description = "The maximum confidence score required to be considered an unlikely match. Values lower than this will not be shown in the grid.",
        IsRequired = true,
        DefaultDecimalValue = 40.00,
        Order = 1 )]

    [BooleanField(
        "Include Inactive",
        Key = AttributeKey.IncludeInactive,
        Description = "Set to true to also include potential matches when both records are inactive.",
        DefaultBooleanValue = false,
        Order = 2 )]

    [BooleanField(
        "Include Businesses",
        Key = AttributeKey.IncludeBusinesses,
        Description = "Set to true to also include potential matches when either record is a Business.",
        DefaultBooleanValue = false,
        Order = 3 )]

    #endregion Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( "A65CF2F8-93A4-4AC6-9018-D7C6996D9017" )]
    public partial class PersonDuplicateDetail : RockBlock, ICustomGridColumns
    {
        #region Attribute Keys
        private static class AttributeKey
        {
            public const string ConfidenceScoreHigh = "ConfidenceScoreHigh";
            public const string ConfidenceScoreLow = "ConfidenceScoreLow";
            public const string IncludeInactive = "IncludeInactive";
            public const string IncludeBusinesses = "IncludeBusinesses";
        }
        #endregion Attribute Keys

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gList.Actions.ShowAdd = false;
            gList.DataKeyNames = new string[] { "PersonId" };
            gList.GridRebind += gList_GridRebind;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
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

        /// <summary>
        /// Handles the GridRebind event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gList_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the Confidence Score HTML include bootstrap label
        /// </summary>
        /// <param name="confidenceScore">The confidence score.</param>
        /// <returns></returns>
        public string GetConfidenceScoreColumnHtml( double? confidenceScore )
        {
            string css;

            if ( confidenceScore >= this.GetAttributeValue( AttributeKey.ConfidenceScoreHigh ).AsDoubleOrNull() )
            {
                css = "label label-success";
            }
            else if ( confidenceScore <= this.GetAttributeValue( AttributeKey.ConfidenceScoreLow ).AsDoubleOrNull() )
            {
                css = "label label-default";
            }
            else
            {
                css = "label label-warning";
            }

            if ( confidenceScore.HasValue )
            {
                return string.Format( "<span class='{0}'>{1}</span>", css, ( confidenceScore.Value / 100 ).ToString( "P" ) );
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the account protection profile column HTML.
        /// </summary>
        /// <param name="accountProtectionProfile">The account protection profile.</param>
        /// <returns></returns>
        public string GetAccountProtectionProfileColumnHtml( AccountProtectionProfile accountProtectionProfile )
        {
            var cssMap = new Dictionary<AccountProtectionProfile, string>
            {
                { AccountProtectionProfile.Extreme, "danger" },
                { AccountProtectionProfile.High, "primary" },
                { AccountProtectionProfile.Medium, "warning" },
                { AccountProtectionProfile.Low, "success" }
            };

            var css = $"label label-{cssMap[accountProtectionProfile]}";


            return $"<span class='{css}'>{accountProtectionProfile.ConvertToString()}</span>";
        }

        /// <summary>
        /// Gets the person view onclick.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        public string GetPersonViewOnClick( int personId )
        {
            var url = "/person/" + personId.ToString();

            // force the link to open a new scrollable,resizable browser window (and make it work in FF, Chrome and IE) http://stackoverflow.com/a/2315916/1755417
            return string.Format( "javascript: window.open('{0}', '_blank', 'scrollbars=1,resizable=1,toolbar=1'); return false;", url );
        }

        /// <summary>
        /// Gets the addresses.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public List<GroupLocation> GetGroupLocations( Person person )
        {
            return person.GetFamilies().Select( a => a.GroupLocations ).SelectMany( a => a ).OrderByDescending( a => a.IsMappedLocation ).ThenBy( a => a.Id ).ToList();
        }

        /// <summary>
        /// Gets the phone numbers.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public List<PhoneNumber> GetPhoneNumbers( Person person )
        {
            return person.PhoneNumbers.ToList();
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var hasMultipleCampuses = CampusCache.All().Count( c => c.IsActive ?? true ) > 1;
            if ( !hasMultipleCampuses )
            {
                var campustColumn = gList.Columns.OfType<RockBoundField>().First( a => a.DataField == "Campus" );
                campustColumn.Visible = false;
            }

            RockContext rockContext = new RockContext();
            var personDuplicateService = new PersonDuplicateService( rockContext );
            var personService = new PersonService( rockContext );
            int personId = this.PageParameter( "PersonId" ).AsInteger();
            int recordStatusInactiveId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() ).Id;
            int recordTypeBusinessId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() ).Id;

            //// select person duplicate records
            //// list duplicates that aren't confirmed as NotDuplicate and aren't IgnoreUntilScoreChanges. Also, don't include records where both the Person and Duplicate are inactive
            var qryPersonDuplicates = personDuplicateService.Queryable()
                .Where( a => a.PersonAlias.PersonId == personId && !a.IsConfirmedAsNotDuplicate && !a.IgnoreUntilScoreChanges );

            if ( this.GetAttributeValue( AttributeKey.IncludeInactive ).AsBoolean() == false )
            {
                qryPersonDuplicates = qryPersonDuplicates.Where( a => !( a.PersonAlias.Person.RecordStatusValueId == recordStatusInactiveId && a.DuplicatePersonAlias.Person.RecordStatusValueId == recordStatusInactiveId ) );
            }

            if ( this.GetAttributeValue( AttributeKey.IncludeBusinesses ).AsBoolean() == false )
            {
                qryPersonDuplicates = qryPersonDuplicates.Where( a => !( a.PersonAlias.Person.RecordTypeValueId == recordTypeBusinessId || a.DuplicatePersonAlias.Person.RecordTypeValueId == recordTypeBusinessId ) );
            }

            var qry = qryPersonDuplicates.Select( s => new
            {
                PersonId = s.DuplicatePersonAlias.Person.Id, // PersonId has to be the key field in the grid for the Merge button to work
                PersonDuplicateId = s.Id,
                FirstName = s.DuplicatePersonAlias.Person.FirstName,
                LastName = s.DuplicatePersonAlias.Person.LastName,
                Email = s.DuplicatePersonAlias.Person.Email,
                Gender = s.DuplicatePersonAlias.Person.Gender,
                Age = s.DuplicatePersonAlias.Person.Age,
                DuplicatePerson = s.DuplicatePersonAlias.Person,
                s.ConfidenceScore,
                IsComparePerson = true,
                Campus = s.DuplicatePersonAlias.Person.PrimaryCampus.Name
            } );

            double? confidenceScoreLow = GetAttributeValue( AttributeKey.ConfidenceScoreLow ).AsDoubleOrNull();
            if ( confidenceScoreLow.HasValue )
            {
                qry = qry.Where( a => a.ConfidenceScore >= confidenceScoreLow );
            }

            qry = qry.OrderByDescending( a => a.ConfidenceScore ).ThenBy( a => a.DuplicatePerson.LastName ).ThenBy( a => a.DuplicatePerson.FirstName );

            var gridList = qry.ToList();

            // put the person we are comparing the duplicates to at the top of the list
            var person = personService.Queryable().Include(p => p.PrimaryCampus).Where(p => p.Id == personId ).FirstOrDefault();
            if ( person != null )
            {
                gridList.Insert(
                    0,
                    new
                    {
                        PersonId = person.Id, // PersonId has to be the key field in the grid for the Merge button to work
                        PersonDuplicateId = 0,
                        FirstName = person.FirstName,
                        LastName = person.LastName,
                        Email = person.Email,
                        Gender = person.Gender,
                        Age = person.Age,
                        DuplicatePerson = person,
                        ConfidenceScore = ( double? ) null,
                        IsComparePerson = false,
                        Campus = person.PrimaryCampus?.Name
                    } );

                nbNoDuplicatesMessage.Visible = gridList.Count == 1;

                gList.DataSource = gridList;
                gList.DataBind();
            }
            else
            {
                ScriptManager.RegisterStartupScript( this, this.GetType(), "goBack", "history.go(-1);", true );
            }
        }

        #endregion

        /// <summary>
        /// Handles the RowDataBound event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gList_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var person = e.Row.DataItem.GetPropertyValue( "DuplicatePerson" );
                bool isComparePerson = ( bool ) e.Row.DataItem.GetPropertyValue( "IsComparePerson" );

                // If this is the main person for the compare, select them, but then hide the checkbox. 
                var row = e.Row;
                var cell = row.Cells[gList.Columns.OfType<SelectField>().First().ColumnIndex];
                var selectBox = cell.Controls[0] as CheckBox;
                selectBox.Visible = isComparePerson;
                selectBox.Checked = !isComparePerson;

                if ( !isComparePerson )
                {
                    row.AddCssClass( "grid-row-bold" );
                }

                // If this is the main person for the compare, hide the "not duplicate" button
                LinkButton btnNotDuplicate = e.Row.ControlsOfTypeRecursive<LinkButton>().FirstOrDefault( a => a.ID == "btnNotDuplicate" );
                btnNotDuplicate.Visible = isComparePerson;

                // If this is the main person for the compare, hide the "ignore duplicate" button
                LinkButton btnIgnoreDuplicate = e.Row.ControlsOfTypeRecursive<LinkButton>().FirstOrDefault( a => a.ID == "btnIgnoreDuplicate" );
                btnIgnoreDuplicate.Visible = isComparePerson;
            }
        }

        /// <summary>
        /// Handles the Click event of the btnNotDuplicate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnNotDuplicate_Click( object sender, EventArgs e )
        {
            RockContext rockContext = new RockContext();
            var personDuplicateService = new PersonDuplicateService( rockContext );
            int personDuplicateId = ( sender as LinkButton ).CommandArgument.AsInteger();
            var personDuplicate = personDuplicateService.Get( personDuplicateId );
            personDuplicate.IsConfirmedAsNotDuplicate = true;
            rockContext.SaveChanges();

            BindGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnIgnoreDuplicate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnIgnoreDuplicate_Click( object sender, EventArgs e )
        {
            RockContext rockContext = new RockContext();
            var personDuplicateService = new PersonDuplicateService( rockContext );
            int personDuplicateId = ( sender as LinkButton ).CommandArgument.AsInteger();
            var personDuplicate = personDuplicateService.Get( personDuplicateId );
            personDuplicate.IgnoreUntilScoreChanges = true;
            rockContext.SaveChanges();

            BindGrid();
        }
    }
}