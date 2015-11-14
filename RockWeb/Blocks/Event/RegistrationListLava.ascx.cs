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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI.Controls;
namespace RockWeb.Blocks.Event
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Registration List Lava" )]
    [Category( "Event" )]
    [Description( "List recent registrations using a Lava template." )]

    [CodeEditorField( "Lava Template", "Lava template to use to display content", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, @"
{% capture currencySymbol %}{{ 'Global' | Attribute:'CurrencySymbol' }}{% endcapture %}
{% capture externalSite %}{{ 'Global' | Attribute:'PublicApplicationRoot' }}{% endcapture %}

{% for registration in Registrations %}

    {% assign registrantCount = registration.Registrants | Size %}

    <p>
        The following {{ registration.RegistrationInstance.RegistrationTemplate.RegistrantTerm | PluralizeForQuantity:registrantCount | Downcase }}
        {% if registrantCount > 1 %}have{% else %}has{% endif %} been registered for {{ registration.RegistrationInstance.Name }}:
    </p>

    <p>
        {{ registration.RegistrationInstance.AdditionalReminderDetails }}
    </p>

    <ul>
    {% for registrant in registration.Registrants %}
        <li>{{ registrant.PersonAlias.Person.FullName }}</li>
    {% endfor %}
    </ul>

    {% if registration.BalanceDue > 0 %}
    <p>
        This {{ registration.RegistrationInstance.RegistrationTemplate.RegistrationTerm | Downcase  }} has a remaining balance 
        of {{ currencySymbol }}{{ registration.BalanceDue | Format:'#,##0.00' }}.
        You can complete the payment for this {{ registration.RegistrationInstance.RegistrationTemplate.RegistrationTerm | Downcase }}
        using our <a href='{{ externalSite }}/Registration?RegistrationInstanceId={{ registration.RegistrationInstanceId }}'>
        online registration page</a>.
    </p>
    {% endif %}

{% endfor %}
",
       "", 2, "LavaTemplate" )]

    [IntegerField( "Max Results", "The maximum number of results to display.", false, 100, order: 3 )]
    [SlidingDateRangeField( "Date Range", "Date range to limit by.", false, "", enabledSlidingDateRangeTypes: "Last,Previous,Current", order: 7 )]
    [BooleanField("Limit to registrations where money is still owed", "", true, "", 8, "LimitToOwed")]
    [BooleanField( "Enable Debug", "Show merge data to help you see what's available to you.", order: 9 )]
    
    public partial class RegistrationListLava : Rock.Web.UI.RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
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
                LoadContent();
            }
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            LoadContent();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the content.
        /// </summary>
        protected void LoadContent()
        {

            RockContext rockContext = new RockContext();

            var registrationService = new RegistrationService( rockContext );
            var qryRegistrations = registrationService.Queryable();

            // only show Active registrations
            qryRegistrations = qryRegistrations.Where( a => a.RegistrationInstance.IsActive == true );

            // limit to the current person
            int currentPersonId = this.CurrentPersonId ?? 0;
            qryRegistrations = qryRegistrations.Where( a => a.PersonAlias.PersonId == currentPersonId );

            

            // bring into a list so we can filter on non-database columns
            var registrationList  = qryRegistrations.ToList();

            if (this.GetAttributeValue("LimitToOwed").AsBooleanOrNull() ?? true)
            {
                registrationList = registrationList.Where( a=> a.BalanceDue != 0).ToList();
            }

            // filter by date range
            /*
            var requestDateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( GetAttributeValue( "DateRange" ) ?? "-1||" );
            if ( requestDateRange.Start != null )
            {
                registrationList = registrationList.Where( r => r.RegistrationInstance.Linkages.Where(a => a.EventItemOccurrenceId.HasValue).Any( a => !a.EventItemOccurrence.NextStartDateTime.HasValue || ( a.Item ) );
            }

            if ( requestDateRange.End != null )
            {
                registrationList = registrationList.Where( r => r.RegistrationInstance.StartDateTime < requestDateRange.End );
            }

            registrationList = registrationList.OrderBy( a => a.RegistrationInstance.StartDateTime );
             */

            List<Registration> hasDates = registrationList.Where(a => a.RegistrationInstance.Linkages.Any(x => x.EventItemOccurrenceId.HasValue && x.EventItemOccurrence.NextStartDateTime.HasValue)).ToList();
            hasDates = hasDates.OrderBy(a => a.RegistrationInstance.Linkages.OrderBy(b => b.EventItemOccurrence.NextStartDateTime).FirstOrDefault().EventItemOccurrence.NextStartDateTime).ToList();
            registrationList = hasDates;

            var noDates = registrationList.Where(a => !hasDates.Any(d => d.Id == a.Id)).OrderBy(x => x.RegistrationInstance.Name);

            registrationList.AddRange(noDates);
            

            int? maxResults = GetAttributeValue( "MaxResults" ).AsIntegerOrNull();
            if ( maxResults.HasValue && maxResults > 0 )
            {
                registrationList = registrationList.Take( maxResults.Value ).ToList();
            }

            var mergeFields = Rock.Web.Cache.GlobalAttributesCache.GetMergeFields( CurrentPerson );
            mergeFields.Add( "CurrentPerson", CurrentPerson );
            mergeFields.Add( "Registrations", registrationList );

            string template = GetAttributeValue( "LavaTemplate" );
            lContent.Text = template.ResolveMergeFields( mergeFields );

            // show debug info
            if ( GetAttributeValue( "EnableDebug" ).AsBoolean() && IsUserAuthorized( Authorization.EDIT ) )
            {
                lDebug.Visible = true;
                lDebug.Text = mergeFields.lavaDebugInfo();
            }
        }

        #endregion
    }
}