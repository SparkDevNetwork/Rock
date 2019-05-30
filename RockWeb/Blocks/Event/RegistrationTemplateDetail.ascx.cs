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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Attribute = Rock.Model.Attribute;

namespace RockWeb.Blocks.Event
{
    [DisplayName( "Registration Template Detail" )]
    [Category( "Event" )]
    [Description( "Displays the details of the given registration template." )]
    [CodeEditorField( "Default Confirmation Email", "The default Confirmation Email Template value to use for a new template", CodeEditorMode.Lava, CodeEditorTheme.Rock, 300, false, @"{{ 'Global' | Attribute:'EmailHeader' }}
{% capture currencySymbol %}{{ 'Global' | Attribute:'CurrencySymbol' }}{% endcapture %}

<h1>{{ RegistrationInstance.RegistrationTemplate.RegistrationTerm }} Confirmation: {{ RegistrationInstance.Name }}</h1>

{% assign registrants = Registration.Registrants | Where:'OnWaitList', false %}
{% assign registrantCount = registrants | Size %}
{% if registrantCount > 0 %}
	<p>
		The following {{ RegistrationInstance.RegistrationTemplate.RegistrantTerm | PluralizeForQuantity:registrantCount | Downcase }}
		{% if registrantCount > 1 %}have{% else %}has{% endif %} been registered for {{ RegistrationInstance.Name }}:
	</p>

	<ul>
	{% for registrant in registrants %}
		<li>

			<strong>{{ registrant.PersonAlias.Person.FullName }}</strong>

			{% if registrant.Cost > 0 %}
				- {{ currencySymbol }}{{ registrant.Cost | Format:'#,##0.00' }}
			{% endif %}

			{% assign feeCount = registrant.Fees | Size %}
			{% if feeCount > 0 %}
				<br/>{{ RegistrationInstance.RegistrationTemplate.FeeTerm | PluralizeForQuantity:registrantCount }}:
				<ul>
				{% for fee in registrant.Fees %}
					<li>
						{{ fee.RegistrationTemplateFee.Name }} {{ fee.Option }}
						{% if fee.Quantity > 1 %} ({{ fee.Quantity }} @ {{ currencySymbol }}{{ fee.Cost | Format:'#,##0.00' }}){% endif %}: {{ currencySymbol }}{{ fee.TotalCost | Format:'#,##0.00' }}
					</li>
				{% endfor %}
				</ul>
			{% endif %}

		</li>
	{% endfor %}
	</ul>
{% endif %}

{% assign waitlist = Registration.Registrants | Where:'OnWaitList', true %}
{% assign waitListCount = waitlist | Size %}
{% if waitListCount > 0 %}
    <p>
        The following {{ RegistrationInstance.RegistrationTemplate.RegistrantTerm | PluralizeForQuantity:registrantCount | Downcase }}
		{% if waitListCount > 1 %}have{% else %}has{% endif %} been added to the wait list for {{ RegistrationInstance.Name }}:
   </p>

    <ul>
    {% for registrant in waitlist %}
        <li>
            <strong>{{ registrant.PersonAlias.Person.FullName }}</strong>
        </li>
    {% endfor %}
    </ul>
{% endif %}

{% if Registration.TotalCost > 0 %}
<p>
    Total Cost: {{ currencySymbol }}{{ Registration.TotalCost | Format:'#,##0.00' }}<br/>
    {% if Registration.DiscountedCost != Registration.TotalCost %}
        Discounted Cost: {{ currencySymbol }}{{ Registration.DiscountedCost | Format:'#,##0.00' }}<br/>
    {% endif %}
    {% for payment in Registration.Payments %}
        Paid {{ currencySymbol }}{{ payment.Amount | Format:'#,##0.00' }} on {{ payment.Transaction.TransactionDateTime| Date:'M/d/yyyy' }}
        <small>(Acct #: {{ payment.Transaction.FinancialPaymentDetail.AccountNumberMasked }}, Ref #: {{ payment.Transaction.TransactionCode }})</small><br/>
    {% endfor %}

    {% assign paymentCount = Registration.Payments | Size %}

    {% if paymentCount > 1 %}
        Total Paid: {{ currencySymbol }}{{ Registration.TotalPaid | Format:'#,##0.00' }}<br/>
    {% endif %}

    Balance Due: {{ currencySymbol }}{{ Registration.BalanceDue | Format:'#,##0.00' }}
</p>
{% endif %}

<p>
    {{ RegistrationInstance.AdditionalConfirmationDetails }}
</p>

<p>
    If you have any questions please contact {{ RegistrationInstance.ContactPersonAlias.Person.FullName }} at {{ RegistrationInstance.ContactEmail }}.
</p>

{{ 'Global' | Attribute:'EmailFooter' }}", "", 0 )]
    [CodeEditorField( "Default Reminder Email", "The default Reminder Email Template value to use for a new template", CodeEditorMode.Lava, CodeEditorTheme.Rock, 300, false, @"{{ 'Global' | Attribute:'EmailHeader' }}
{% capture currencySymbol %}{{ 'Global' | Attribute:'CurrencySymbol' }}{% endcapture %}
{% capture externalSite %}{{ 'Global' | Attribute:'PublicApplicationRoot' }}{% endcapture %}
{% assign registrantCount = Registration.Registrants | Size %}

<h1>{{ RegistrationInstance.RegistrationTemplate.RegistrationTerm }} Reminder</h1>

<p>
    {{ RegistrationInstance.AdditionalReminderDetails }}
</p>

{% assign registrants = Registration.Registrants | Where:'OnWaitList', false %}
{% assign registrantCount = registrants | Size %}
{% if registrantCount > 0 %}
	<p>
		The following {{ RegistrationInstance.RegistrationTemplate.RegistrantTerm | PluralizeForQuantity:registrantCount | Downcase }}
		{% if registrantCount > 1 %}have{% else %}has{% endif %} been registered for {{ RegistrationInstance.Name }}:
	</p>

	<ul>
	{% for registrant in registrants %}
		<li>{{ registrant.PersonAlias.Person.FullName }}</li>
	{% endfor %}
	</ul>
{% endif %}

{% assign waitlist = Registration.Registrants | Where:'OnWaitList', true %}
{% assign waitListCount = waitlist | Size %}
{% if waitListCount > 0 %}
    <p>
        The following {{ RegistrationInstance.RegistrationTemplate.RegistrantTerm | PluralizeForQuantity:registrantCount | Downcase }}
		{% if waitListCount > 1 %}are{% else %}is{% endif %} still on the waiting list:
   </p>

    <ul>
    {% for registrant in waitlist %}
        <li>
            <strong>{{ registrant.PersonAlias.Person.FullName }}</strong>
        </li>
    {% endfor %}
    </ul>
{% endif %}

{% if Registration.BalanceDue > 0 %}
<p>
    This {{ RegistrationInstance.RegistrationTemplate.RegistrationTerm | Downcase  }} has a remaining balance
    of {{ currencySymbol }}{{ Registration.BalanceDue | Format:'#,##0.00' }}.
    You can complete the payment for this {{ RegistrationInstance.RegistrationTemplate.RegistrationTerm | Downcase }}
    using our <a href='{{ externalSite }}/Registration?RegistrationId={{ Registration.Id }}&rckipid={{ Registration.PersonAlias.Person | PersonTokenCreate }}'>
    online registration page</a>.
</p>
{% endif %}

<p>
    If you have any questions please contact {{ RegistrationInstance.ContactName }} at {{ RegistrationInstance.ContactEmail }}.
</p>

{{ 'Global' | Attribute:'EmailFooter' }}", "", 1 )]
    [CodeEditorField( "Default Success Text", "The success text default to use for a new template", CodeEditorMode.Lava, CodeEditorTheme.Rock, 300, false, @"{% capture currencySymbol %}{{ 'Global' | Attribute:'CurrencySymbol' }}{% endcapture %}

{% assign registrants = Registration.Registrants | Where:'OnWaitList', false %}
{% assign registrantCount = registrants | Size %}
{% if registrantCount > 0 %}
    <p>
        You have successfully registered the following
        {{ RegistrationInstance.RegistrationTemplate.RegistrantTerm | PluralizeForQuantity:registrantCount | Downcase }}
        for {{ RegistrationInstance.Name }}:
    </p>

    <ul>
    {% for registrant in registrants %}
        <li>

            <strong>{{ registrant.PersonAlias.Person.FullName }}</strong>

            {% if registrant.Cost > 0 %}
                - {{ currencySymbol }}{{ registrant.Cost | Format:'#,##0.00' }}
            {% endif %}

            {% assign feeCount = registrant.Fees | Size %}
            {% if feeCount > 0 %}
                <br/>{{ RegistrationInstance.RegistrationTemplate.FeeTerm | PluralizeForQuantity:registrantCount }}:
                <ul class='list-unstyled'>
                {% for fee in registrant.Fees %}
                    <li>
                        {{ fee.RegistrationTemplateFee.Name }} {{ fee.Option }}
                        {% if fee.Quantity > 1 %} ({{ fee.Quantity }} @ {{ currencySymbol }}{{ fee.Cost | Format:'#,##0.00' }}){% endif %}: {{ currencySymbol }}{{ fee.TotalCost | Format:'#,##0.00' }}
                    </li>
                {% endfor %}
                </ul>
            {% endif %}

        </li>
    {% endfor %}
    </ul>
{% endif %}

{% assign waitlist = Registration.Registrants | Where:'OnWaitList', true %}
{% assign waitListCount = waitlist | Size %}
{% if waitListCount > 0 %}
    <p>
        You have successfully added the following
        {{ RegistrationInstance.RegistrationTemplate.RegistrantTerm | PluralizeForQuantity:registrantCount | Downcase }}
        to the waiting list for {{ RegistrationInstance.Name }}:
    </p>

    <ul>
    {% for registrant in waitlist %}
        <li>
            <strong>{{ registrant.PersonAlias.Person.FullName }}</strong>
        </li>
    {% endfor %}
    </ul>
{% endif %}

{% if Registration.TotalCost > 0 %}
<p>
    Total Cost: {{ currencySymbol }}{{ Registration.TotalCost | Format:'#,##0.00' }}<br/>
    {% if Registration.DiscountedCost != Registration.TotalCost %}
        Discounted Cost: {{ currencySymbol }}{{ Registration.DiscountedCost | Format:'#,##0.00' }}<br/>
    {% endif %}
    {% for payment in Registration.Payments %}
        Paid {{ currencySymbol }}{{ payment.Amount | Format:'#,##0.00' }} on {{ payment.Transaction.TransactionDateTime| Date:'M/d/yyyy' }}
        <small>(Acct #: {{ payment.Transaction.FinancialPaymentDetail.AccountNumberMasked }}, Ref #: {{ payment.Transaction.TransactionCode }})</small><br/>
    {% endfor %}
    {% assign paymentCount = Registration.Payments | Size %}
    {% if paymentCount > 1 %}
        Total Paid: {{ currencySymbol }}{{ Registration.TotalPaid | Format:'#,##0.00' }}<br/>
    {% endif %}
    Balance Due: {{ currencySymbol }}{{ Registration.BalanceDue | Format:'#,##0.00' }}
</p>
{% endif %}

<p>
    A confirmation email has been sent to {{ Registration.ConfirmationEmail }}. If you have any questions
    please contact {{ RegistrationInstance.ContactPersonAlias.Person.FullName }} at {{ RegistrationInstance.ContactEmail }}.
</p>", "", 2 )]
    [CodeEditorField( "Default Payment Reminder Email", "The default Payment Reminder Email Template value to use for a new template", CodeEditorMode.Lava, CodeEditorTheme.Rock, 300, false, @"{{ 'Global' | Attribute:'EmailHeader' }}
{% capture currencySymbol %}{{ 'Global' | Attribute:'CurrencySymbol' }}{% endcapture %}
{% capture externalSite %}{{ 'Global' | Attribute:'PublicApplicationRoot' }}{% endcapture %}

<h1>{{ RegistrationInstance.RegistrationTemplate.RegistrationTerm }} Payment Reminder</h1>

<p>
    This {{ RegistrationInstance.RegistrationTemplate.RegistrationTerm | Downcase  }} for {{ RegistrationInstance.Name }} has a remaining balance
    of {{ currencySymbol }}{{ Registration.BalanceDue | Format:'#,##0.00' }}. The
    {{ RegistrationInstance.RegistrationTemplate.RegistrantTerm | Downcase | Pluralize  }} for this
    {{ RegistrationInstance.RegistrationTemplate.RegistrationTerm }} are below.
</p>

{% assign registrants = Registration.Registrants | Where:'OnWaitList', false %}
{% assign registrantCount = registrants | Size %}
{% if registrantCount > 0 %}
	<ul>
	{% for registrant in registrants %}
		<li>{{ registrant.PersonAlias.Person.FullName }}</li>
	{% endfor %}
	</ul>
{% endif %}

{% assign waitlist = Registration.Registrants | Where:'OnWaitList', true %}
{% assign waitListCount = waitlist | Size %}
{% if waitListCount > 0 %}
    <p>
        The following {{ RegistrationInstance.RegistrationTemplate.RegistrantTerm | PluralizeForQuantity:registrantCount | Downcase }}
		{% if waitListCount > 1 %}are{% else %}is{% endif %} still on the wait list:
   </p>

    <ul>
    {% for registrant in waitlist %}
        <li>
            <strong>{{ registrant.PersonAlias.Person.FullName }}</strong>
        </li>
    {% endfor %}
    </ul>
{% endif %}

<p>
    You can complete the payment for this {{ RegistrationInstance.RegistrationTemplate.RegistrationTerm | Downcase }}
    using our <a href='{{ externalSite }}/Registration?RegistrationId={{ Registration.Id }}&rckipid={{ Registration.PersonAlias.Person | PersonTokenCreate }}'>
    online registration page</a>.
</p>

<p>
    If you have any questions please contact {{ RegistrationInstance.ContactName }} at {{ RegistrationInstance.ContactEmail }}.
</p>

{{ 'Global' | Attribute:'EmailFooter' }}", "", 3 )]
    [CodeEditorField( "Default Wait List Transition Email", "The default Wait List Transition Email Template value to use for a new template", CodeEditorMode.Lava, CodeEditorTheme.Rock, 300, false, @"{{ 'Global' | Attribute:'EmailHeader' }}
{% capture currencySymbol %}{{ 'Global' | Attribute:'CurrencySymbol' }}{% endcapture %}
{% capture externalSite %}{{ 'Global' | Attribute:'PublicApplicationRoot' }}{% endcapture %}

<h1>{{ RegistrationInstance.Name }} Wait List Update</h1>

<p>
    {{ Registration.FirstName }}, the following individuals have been moved from the {{ RegistrationInstance.Name }} wait list to a full
    {{ RegistrationInstance.RegistrationTemplate.RegistrantTerm | Downcase }}.
</p>

<ul>
    {% for registrant in TransitionedRegistrants %}
        <li>{{ registrant.PersonAlias.Person.FullName }}</li>
    {% endfor %}
</ul>

{% if AdditionalFieldsNeeded %}
    <p>
        <strong>Addition information is needed in order to process this registration. Please visit the
        <a href='{{ externalSite }}/Registration?RegistrationId={{ Registration.Id }}&rckipid={{ Registration.PersonAlias.Person | PersonTokenCreate }}&StartAtBeginning=True'>
        online registration page</a> to complete the registration.</strong>
    </p>
{% endif %}

{% if Registration.BalanceDue > 0 %}
    <p>
        A balance of {{ currencySymbol }}{{ Registration.BalanceDue | Format:'#,##0.00' }} remains on this registration. You can complete the payment for this {{ RegistrationInstance.RegistrationTemplate.RegistrationTerm | Downcase }}
        using our <a href='{{ externalSite }}/Registration?RegistrationId={{ Registration.Id }}&rckipid={{ Registration.PersonAlias.Person | PersonTokenCreate }}'>
        online registration page</a>.
    </p>
{% endif %}

<p>
    If you have any questions please contact {{ RegistrationInstance.ContactName }} at {{ RegistrationInstance.ContactEmail }}.
</p>

{{ 'Global' | Attribute:'EmailFooter' }}", "", 3 )]
    public partial class RegistrationTemplateDetail : RockBlock
    {
        #region Properties

        private List<RegistrationTemplateForm> FormState { get; set; }

        private Dictionary<Guid, List<RegistrationTemplateFormField>> FormFieldsState { get; set; }

        private List<Guid> ExpandedForms = new List<Guid>();

        private List<RegistrationTemplateDiscount> DiscountState { get; set; }

        private List<RegistrationTemplateFee> FeeState { get; set; }

        private List<Attribute> RegistrationAttributesState { get; set; }

        /// <summary>
        /// The State of the RegistrationTemplateFeeItems in the Fees Dialog while it is being edited
        /// </summary>
        private List<RegistrationTemplateFeeItem> FeeItemsEditState { get; set; }

        private int? GridFieldsDeleteIndex { get; set; }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            string json = ViewState["FormState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                FormState = new List<RegistrationTemplateForm>();
            }
            else
            {
                FormState = JsonConvert.DeserializeObject<List<RegistrationTemplateForm>>( json );
            }

            json = ViewState["FormFieldsState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                FormFieldsState = new Dictionary<Guid, List<RegistrationTemplateFormField>>();
            }
            else
            {
                FormFieldsState = JsonConvert.DeserializeObject<Dictionary<Guid, List<RegistrationTemplateFormField>>>( json );
            }

            json = ViewState["RegistrationAttributesState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                RegistrationAttributesState = new List<Attribute>();
            }
            else
            {
                RegistrationAttributesState = JsonConvert.DeserializeObject<List<Attribute>>( json );
            }

            ExpandedForms = ViewState["ExpandedForms"] as List<Guid>;
            if ( ExpandedForms == null )
            {
                ExpandedForms = new List<Guid>();
            }

            json = ViewState["DiscountState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                DiscountState = new List<RegistrationTemplateDiscount>();
            }
            else
            {
                DiscountState = JsonConvert.DeserializeObject<List<RegistrationTemplateDiscount>>( json );
            }

            json = ViewState["FeeState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                FeeState = new List<RegistrationTemplateFee>();
            }
            else
            {
                FeeState = JsonConvert.DeserializeObject<List<RegistrationTemplateFee>>( json );
            }

            json = ViewState["FeeItemsEditState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                FeeItemsEditState = new List<RegistrationTemplateFeeItem>();
            }
            else
            {
                FeeItemsEditState = JsonConvert.DeserializeObject<List<RegistrationTemplateFeeItem>>( json );
            }

            BuildControls( false );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gRegistrationAttributes.DataKeyNames = new string[] { "Guid" };
            gRegistrationAttributes.Actions.ShowAdd = true;
            gRegistrationAttributes.Actions.AddClick += gRegistrationAttributes_AddClick;
            gRegistrationAttributes.EmptyDataText = Server.HtmlEncode( None.Text );
            gRegistrationAttributes.GridRebind += gRegistrationAttributes_GridRebind;
            gRegistrationAttributes.GridReorder += gRegistrationAttributes_GridReorder;

            SecurityField registrationAttributeSecurityField = gRegistrationAttributes.Columns.OfType<SecurityField>().FirstOrDefault();
            registrationAttributeSecurityField.EntityTypeId = EntityTypeCache.GetId<Attribute>() ?? 0;

            // assign discounts grid actions
            gDiscounts.DataKeyNames = new string[] { "Guid" };
            gDiscounts.Actions.ShowAdd = true;
            gDiscounts.Actions.AddClick += gDiscounts_AddClick;
            gDiscounts.GridRebind += gDiscounts_GridRebind;
            gDiscounts.GridReorder += gDiscounts_GridReorder;

            // assign fees grid actions
            gFees.DataKeyNames = new string[] { "Guid" };
            gFees.Actions.ShowAdd = true;
            gFees.Actions.AddClick += gFees_AddClick;
            gFees.GridRebind += gFees_GridRebind;
            gFees.GridReorder += gFees_GridReorder;

            btnSecurity.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.RegistrationTemplate ) ).Id;

            ddlRegistrarOption.Help = @"How should the registrar's information be collected?

<strong>Prompt For Registrar</strong>
Registrar information will be collected at the end.

<strong>Pre-fill First Registrant</strong>
The first registrant's information will be used to complete the registrar information form but can be changed if needed.

<strong>Use First Registrant</strong>
The first registrant's information will be used to complete the registrar information form and the form will not be displayed.  (If the first registrant's name and email is not provided the registrar information form will still display.)

<strong>Use Logged In Person</strong>
The logged-in person's information will be used to complete the registrar information form and the form will not be displayed.  (If the logged in person's email is not known, a prompt for email will be shown.)
";

            string deleteScript = @"
    $('a.js-delete-template').click(function( e ){
        e.preventDefault();
        Rock.dialogs.confirm('Are you sure you want to delete this registration template? All of the instances, and the registrations and registrants from each instance will also be deleted!', function (result) {
            if (result) {
                if ( $('input.js-has-registrations').val() == 'True' ) {
                    Rock.dialogs.confirm('This template has existing instances with existing registrations. Are you sure that you want to delete the template?<br/><small>(Payments will not be deleted, but they will no longer be associated with a registration.)</small>', function (result) {
                        if (result) {
                            window.location = e.target.href ? e.target.href : e.target.parentElement.href;
                        }
                    });
                } else {
                    window.location = e.target.href ? e.target.href : e.target.parentElement.href;
                }
            }
        });
    });
";
            ScriptManager.RegisterStartupScript( btnDelete, btnDelete.GetType(), "deleteInstanceScript", deleteScript, true );
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
                ShowDetail();
            }
            else
            {
                nbValidationError.Visible = false;

                ShowDialog();

                string postbackArgs = Request.Params["__EVENTARGUMENT"];
                if ( !string.IsNullOrWhiteSpace( postbackArgs ) )
                {
                    string[] nameValue = postbackArgs.Split( new char[] { ':' } );
                    if ( nameValue.Count() == 2 )
                    {
                        string[] values = nameValue[1].Split( new char[] { ';' } );
                        if ( values.Count() == 2 )
                        {
                            Guid guid = values[0].AsGuid();
                            int newIndex = values[1].AsInteger();

                            switch ( nameValue[0] )
                            {
                                case "re-order-form":
                                    {
                                        SortForms( guid, newIndex + 1 );
                                        break;
                                    }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the bread crumbs.
        /// </summary>
        /// <param name="pageReference">The page reference.</param>
        /// <returns></returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            int? registrationTemplateId = PageParameter( pageReference, "RegistrationTemplateId" ).AsIntegerOrNull();
            if ( registrationTemplateId.HasValue )
            {
                RegistrationTemplate registrationTemplate = GetRegistrationTemplate( registrationTemplateId.Value );
                if ( registrationTemplate != null )
                {
                    breadCrumbs.Add( new BreadCrumb( registrationTemplate.ToString(), pageReference ) );
                    return breadCrumbs;
                }
            }

            breadCrumbs.Add( new BreadCrumb( this.PageCache.PageTitle, pageReference ) );
            return breadCrumbs;
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
            };

            ViewState["FormState"] = JsonConvert.SerializeObject( FormState, Formatting.None, jsonSetting );
            ViewState["FormFieldsState"] = JsonConvert.SerializeObject( FormFieldsState, Formatting.None, jsonSetting );
            ViewState["RegistrationAttributesState"] = JsonConvert.SerializeObject( RegistrationAttributesState, Formatting.None, jsonSetting );
            ViewState["ExpandedForms"] = ExpandedForms;
            ViewState["DiscountState"] = JsonConvert.SerializeObject( DiscountState, Formatting.None, jsonSetting );
            ViewState["FeeState"] = JsonConvert.SerializeObject( FeeState, Formatting.None, jsonSetting );

            ViewState["FeeItemsEditState"] = JsonConvert.SerializeObject( FeeItemsEditState, Formatting.None, jsonSetting );

            return base.SaveViewState();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnPreRender( EventArgs e )
        {
            if ( pnlEditDetails.Visible )
            {
                var sameFamily = rblRegistrantsInSameFamily.SelectedValueAsEnum<RegistrantsSameFamily>();
                divCurrentFamilyMembers.Attributes["style"] = sameFamily == RegistrantsSameFamily.No ? "display:none" : "display:block";
            }

            base.OnPreRender( e );
        }

        #endregion

        #region Events

        #region Form Events

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var registrationTemplate = new RegistrationTemplateService( rockContext ).Get( hfRegistrationTemplateId.Value.AsInteger() );

            if ( registrationTemplate != null && ( UserCanEdit || registrationTemplate.IsAuthorized( Authorization.ADMINISTRATE, this.CurrentPerson ) ) )
            {
                LoadStateDetails( registrationTemplate, rockContext );
                ShowEditDetails( registrationTemplate, rockContext );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();

            var service = new RegistrationTemplateService( rockContext );
            var registrationTemplate = service.Get( hfRegistrationTemplateId.Value.AsInteger() );

            if ( registrationTemplate != null )
            {
                if ( !UserCanEdit && !registrationTemplate.IsAuthorized( Authorization.ADMINISTRATE, this.CurrentPerson ) )
                {
                    mdDeleteWarning.Show( "You are not authorized to delete this registration template.", ModalAlertType.Information );
                    return;
                }

                rockContext.WrapTransaction( () =>
                {
                    new RegistrationService( rockContext ).DeleteRange( registrationTemplate.Instances.SelectMany( i => i.Registrations ) );
                    new RegistrationInstanceService( rockContext ).DeleteRange( registrationTemplate.Instances );
                    service.Delete( registrationTemplate );
                    rockContext.SaveChanges();
                } );
            }

            // reload page
            var qryParams = new Dictionary<string, string>();
            NavigateToPage( RockPage.Guid, qryParams );
        }

        /// <summary>
        /// Handles the Click event of the btnCopy control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCopy_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var registrationTemplate = new RegistrationTemplateService( rockContext ).Get( hfRegistrationTemplateId.Value.AsInteger() );

            if ( registrationTemplate != null )
            {
                // Load the state objects for the source registration template
                LoadStateDetails( registrationTemplate, rockContext );

                // clone the registration template
                var newRegistrationTemplate = registrationTemplate.Clone( false );
                newRegistrationTemplate.CreatedByPersonAlias = null;
                newRegistrationTemplate.CreatedByPersonAliasId = null;
                newRegistrationTemplate.CreatedDateTime = RockDateTime.Now;
                newRegistrationTemplate.ModifiedByPersonAlias = null;
                newRegistrationTemplate.ModifiedByPersonAliasId = null;
                newRegistrationTemplate.ModifiedDateTime = RockDateTime.Now;
                newRegistrationTemplate.Id = 0;
                newRegistrationTemplate.Guid = Guid.NewGuid();
                newRegistrationTemplate.Name = registrationTemplate.Name + " - Copy";

                // Create temporary state objects for the new registration template
                var newFormState = new List<RegistrationTemplateForm>();
                var newFormFieldsState = new Dictionary<Guid, List<RegistrationTemplateFormField>>();
                var newDiscountState = new List<RegistrationTemplateDiscount>();
                var newFeeState = new List<RegistrationTemplateFee>();
                var newAttributeState = new List<Attribute>();

                foreach ( var form in FormState )
                {
                    var newForm = form.Clone( false );
                    newForm.RegistrationTemplateId = 0;
                    newForm.Id = 0;
                    newForm.Guid = Guid.NewGuid();
                    newFormState.Add( newForm );

                    if ( FormFieldsState.ContainsKey( form.Guid ) )
                    {
                        newFormFieldsState.Add( newForm.Guid, new List<RegistrationTemplateFormField>() );
                        foreach ( var formField in FormFieldsState[form.Guid] )
                        {
                            var newFormField = formField.Clone( false );
                            newFormField.RegistrationTemplateFormId = 0;
                            newFormField.Id = 0;
                            newFormField.Guid = Guid.NewGuid();
                            newFormFieldsState[newForm.Guid].Add( newFormField );

                            if ( formField.FieldSource != RegistrationFieldSource.PersonField )
                            {
                                newFormField.Attribute = formField.Attribute;
                            }

                            if ( formField.FieldSource == RegistrationFieldSource.RegistrantAttribute && formField.Attribute != null )
                            {
                                var newAttribute = formField.Attribute.Clone( false );
                                newAttribute.Id = 0;
                                newAttribute.Guid = Guid.NewGuid();
                                newAttribute.IsSystem = false;

                                newFormField.AttributeId = null;
                                newFormField.Attribute = newAttribute;

                                foreach ( var qualifier in formField.Attribute.AttributeQualifiers )
                                {
                                    var newQualifier = qualifier.Clone( false );
                                    newQualifier.Id = 0;
                                    newQualifier.Guid = Guid.NewGuid();
                                    newQualifier.IsSystem = false;
                                    newAttribute.AttributeQualifiers.Add( qualifier );
                                }
                            }
                        }
                    }
                }

                foreach ( var discount in DiscountState )
                {
                    var newDiscount = discount.Clone( false );
                    newDiscount.RegistrationTemplateId = 0;
                    newDiscount.Id = 0;
                    newDiscount.Guid = Guid.NewGuid();
                    newDiscountState.Add( newDiscount );
                }

                foreach ( var fee in FeeState )
                {
                    var newFee = fee.Clone( false );
                    newFee.RegistrationTemplateId = 0;
                    newFee.Id = 0;
                    newFee.Guid = Guid.NewGuid();
                    newFeeState.Add( newFee );
                    foreach ( var item in fee.FeeItems )
                    {
                        var feeItem = item.Clone( false );
                        feeItem.Id = 0;
                        feeItem.Guid = Guid.NewGuid();
                        newFee.FeeItems.Add( feeItem );
                    }
                }

                foreach ( var attribute in RegistrationAttributesState )
                {
                    var newAttribute = attribute.Clone( false );
                    newAttribute.EntityTypeQualifierValue = null;
                    newAttribute.Id = 0;
                    newAttribute.Guid = Guid.NewGuid();
                    newAttributeState.Add( newAttribute );
                }

                FormState = newFormState;
                FormFieldsState = newFormFieldsState;
                DiscountState = newDiscountState;
                FeeState = newFeeState;
                RegistrationAttributesState = newAttributeState;

                hfRegistrationTemplateId.Value = newRegistrationTemplate.Id.ToString();
                ShowEditDetails( newRegistrationTemplate, rockContext );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            ParseControls( true );

            var rockContext = new RockContext();

            // validate gateway
            int? gatewayId = fgpFinancialGateway.SelectedValueAsInt();
            if ( gatewayId.HasValue )
            {
                var financialGateway = new FinancialGatewayService( rockContext ).Get( gatewayId.Value );
                if ( financialGateway != null )
                {
                    if ( financialGateway.GetGatewayComponent() is Rock.Financial.IHostedGatewayComponent )
                    {
                        nbValidationError.Text = "Unsupported Gateway. Registration doesn't currently support Gateways that have a hosted payment interface.";
                        nbValidationError.Visible = true;
                        return;
                    }
                }
            }

            var service = new RegistrationTemplateService( rockContext );

            RegistrationTemplate registrationTemplate = null;

            int? registrationTemplateId = hfRegistrationTemplateId.Value.AsIntegerOrNull();
            if ( registrationTemplateId.HasValue )
            {
                registrationTemplate = service.Get( registrationTemplateId.Value );
            }

            bool newTemplate = false;
            if ( registrationTemplate == null )
            {
                newTemplate = true;
                registrationTemplate = new RegistrationTemplate();
            }

            RegistrationNotify notify = RegistrationNotify.None;
            foreach ( ListItem li in cblNotify.Items )
            {
                if ( li.Selected )
                {
                    notify = notify | ( RegistrationNotify ) li.Value.AsInteger();
                }
            }

            registrationTemplate.IsActive = cbIsActive.Checked;
            registrationTemplate.Name = tbName.Text;
            registrationTemplate.CategoryId = cpCategory.SelectedValueAsInt();
            registrationTemplate.GroupTypeId = gtpGroupType.SelectedGroupTypeId;
            registrationTemplate.GroupMemberRoleId = rpGroupTypeRole.GroupRoleId;
            registrationTemplate.GroupMemberStatus = ddlGroupMemberStatus.SelectedValueAsEnum<GroupMemberStatus>();
            registrationTemplate.RequiredSignatureDocumentTemplateId = ddlSignatureDocumentTemplate.SelectedValueAsInt();
            registrationTemplate.SignatureDocumentAction = cbDisplayInLine.Checked ? SignatureDocumentAction.Embed : SignatureDocumentAction.Email;
            registrationTemplate.WaitListEnabled = cbWaitListEnabled.Checked;
            registrationTemplate.RegistrarOption = ddlRegistrarOption.SelectedValueAsEnum<RegistrarOption>();

            registrationTemplate.RegistrationWorkflowTypeId = wtpRegistrationWorkflow.SelectedValueAsInt();
            registrationTemplate.Notify = notify;
            registrationTemplate.AddPersonNote = cbAddPersonNote.Checked;
            registrationTemplate.LoginRequired = cbLoginRequired.Checked;
            registrationTemplate.AllowExternalRegistrationUpdates = cbAllowExternalUpdates.Checked;
            registrationTemplate.AllowGroupPlacement = cbAllowGroupPlacement.Checked;
            registrationTemplate.AllowMultipleRegistrants = cbMultipleRegistrants.Checked;
            registrationTemplate.MaxRegistrants = nbMaxRegistrants.Text.AsInteger();
            registrationTemplate.RegistrantsSameFamily = rblRegistrantsInSameFamily.SelectedValueAsEnum<RegistrantsSameFamily>();
            registrationTemplate.ShowCurrentFamilyMembers = cbShowCurrentFamilyMembers.Checked;
            registrationTemplate.SetCostOnInstance = !tglSetCostOnTemplate.Checked;
            registrationTemplate.Cost = cbCost.Text.AsDecimal();
            registrationTemplate.MinimumInitialPayment = cbMinimumInitialPayment.Text.AsDecimalOrNull();
            registrationTemplate.DefaultPayment = cbDefaultPaymentAmount.Text.AsDecimalOrNull();
            registrationTemplate.FinancialGatewayId = fgpFinancialGateway.SelectedValueAsInt();
            registrationTemplate.BatchNamePrefix = txtBatchNamePrefix.Text;

            registrationTemplate.ConfirmationFromName = tbConfirmationFromName.Text;
            registrationTemplate.ConfirmationFromEmail = tbConfirmationFromEmail.Text;
            registrationTemplate.ConfirmationSubject = tbConfirmationSubject.Text;
            registrationTemplate.ConfirmationEmailTemplate = ceConfirmationEmailTemplate.Text;

            registrationTemplate.ReminderFromName = tbReminderFromName.Text;
            registrationTemplate.ReminderFromEmail = tbReminderFromEmail.Text;
            registrationTemplate.ReminderSubject = tbReminderSubject.Text;
            registrationTemplate.ReminderEmailTemplate = ceReminderEmailTemplate.Text;

            registrationTemplate.PaymentReminderFromName = tbPaymentReminderFromName.Text;
            registrationTemplate.PaymentReminderFromEmail = tbPaymentReminderFromEmail.Text;
            registrationTemplate.PaymentReminderSubject = tbPaymentReminderSubject.Text;
            registrationTemplate.PaymentReminderEmailTemplate = cePaymentReminderEmailTemplate.Text;
            registrationTemplate.PaymentReminderTimeSpan = nbPaymentReminderTimeSpan.Text.AsInteger();

            registrationTemplate.WaitListTransitionFromName = tbWaitListTransitionFromName.Text;
            registrationTemplate.WaitListTransitionFromEmail = tbWaitListTransitionFromEmail.Text;
            registrationTemplate.WaitListTransitionSubject = tbWaitListTransitionSubject.Text;
            registrationTemplate.WaitListTransitionEmailTemplate = ceWaitListTransitionEmailTemplate.Text;

            registrationTemplate.RegistrationTerm = string.IsNullOrWhiteSpace( tbRegistrationTerm.Text ) ? "Registration" : tbRegistrationTerm.Text;
            registrationTemplate.RegistrantTerm = string.IsNullOrWhiteSpace( tbRegistrantTerm.Text ) ? "Person" : tbRegistrantTerm.Text;
            registrationTemplate.FeeTerm = string.IsNullOrWhiteSpace( tbFeeTerm.Text ) ? "Additional Options" : tbFeeTerm.Text;
            registrationTemplate.DiscountCodeTerm = string.IsNullOrWhiteSpace( tbDiscountCodeTerm.Text ) ? "Discount Code" : tbDiscountCodeTerm.Text;
            registrationTemplate.RegistrationAttributeTitleStart = string.IsNullOrWhiteSpace( tbRegistrationAttributeTitleStart.Text ) ? "Registration Information" : tbRegistrationAttributeTitleStart.Text;
            registrationTemplate.RegistrationAttributeTitleEnd = string.IsNullOrWhiteSpace( tbRegistrationAttributeTitleEnd.Text ) ? "Registration Information" : tbRegistrationAttributeTitleEnd.Text;
            registrationTemplate.SuccessTitle = tbSuccessTitle.Text;
            registrationTemplate.SuccessText = ceSuccessText.Text;
            registrationTemplate.RegistrationInstructions = heInstructions.Text;
            if ( !Page.IsValid || !registrationTemplate.IsValid )
            {
                return;
            }

            foreach ( var form in FormState )
            {
                if ( !form.IsValid )
                {
                    return;
                }

                if ( FormFieldsState.ContainsKey( form.Guid ) )
                {
                    foreach ( var formField in FormFieldsState[form.Guid] )
                    {
                        if ( !formField.IsValid )
                        {
                            return;
                        }
                    }
                }
            }

            // Get the valid group member attributes
            var group = new Group();
            group.GroupTypeId = gtpGroupType.SelectedGroupTypeId ?? 0;
            var groupMember = new GroupMember();
            groupMember.Group = group;
            groupMember.LoadAttributes();
            var validGroupMemberAttributeIds = groupMember.Attributes.Select( a => a.Value.Id ).ToList();

            // Remove any group member attributes that are not valid based on selected group type
            foreach ( var fieldList in FormFieldsState.Select( s => s.Value ) )
            {
                foreach ( var formField in fieldList
                    .Where( a =>
                        a.FieldSource == RegistrationFieldSource.GroupMemberAttribute &&
                        a.AttributeId.HasValue &&
                        !validGroupMemberAttributeIds.Contains( a.AttributeId.Value ) )
                    .ToList() )
                {
                    fieldList.Remove( formField );
                }
            }

            // Perform Validation
            var validationErrors = new List<string>();
            if ( ( ( registrationTemplate.SetCostOnInstance ?? false ) || registrationTemplate.Cost > 0 || FeeState.Any() ) && !registrationTemplate.FinancialGatewayId.HasValue )
            {
                validationErrors.Add( "A Financial Gateway is required when the registration has a cost or additional fees or is configured to allow instances to set a cost." );
            }

            if ( registrationTemplate.WaitListEnabled && registrationTemplate.MaxRegistrants == 0 )
            {
                validationErrors.Add( "To enable a wait list you must provide a maximum number of registrants." );
            }

            if ( validationErrors.Any() )
            {
                nbValidationError.Visible = true;
                nbValidationError.Text = "<ul class='list-unstyled'><li>" + validationErrors.AsDelimited( "</li><li>" ) + "</li></ul>";
            }
            else
            {
                // Save the entity field changes to registration template
                if ( registrationTemplate.Id.Equals( 0 ) )
                {
                    service.Add( registrationTemplate );
                }

                rockContext.SaveChanges();

                var attributeService = new AttributeService( rockContext );
                var registrationTemplateFormService = new RegistrationTemplateFormService( rockContext );
                var registrationTemplateFormFieldService = new RegistrationTemplateFormFieldService( rockContext );
                var registrationTemplateDiscountService = new RegistrationTemplateDiscountService( rockContext );
                var registrationTemplateFeeService = new RegistrationTemplateFeeService( rockContext );
                var registrationTemplateFeeItemService = new RegistrationTemplateFeeItemService( rockContext );
                var registrationRegistrantFeeService = new RegistrationRegistrantFeeService( rockContext );

                var groupService = new GroupService( rockContext );

                // delete forms that aren't assigned in the UI anymore
                var formUiGuids = FormState.Select( f => f.Guid ).ToList();
                foreach ( var form in registrationTemplateFormService
                    .Queryable()
                    .Where( f =>
                        f.RegistrationTemplateId == registrationTemplate.Id &&
                        !formUiGuids.Contains( f.Guid ) ) )
                {
                    foreach ( var formField in form.Fields.ToList() )
                    {
                        form.Fields.Remove( formField );
                        registrationTemplateFormFieldService.Delete( formField );
                    }

                    registrationTemplateFormService.Delete( form );
                }

                // delete fields that aren't assigned in the UI anymore
                var fieldUiGuids = FormFieldsState.SelectMany( a => a.Value ).Select( f => f.Guid ).ToList();
                foreach ( var formField in registrationTemplateFormFieldService
                    .Queryable()
                    .Where( a =>
                        formUiGuids.Contains( a.RegistrationTemplateForm.Guid ) &&
                        !fieldUiGuids.Contains( a.Guid ) ) )
                {
                    registrationTemplateFormFieldService.Delete( formField );
                }

                // delete discounts that aren't assigned in the UI anymore
                var discountUiGuids = DiscountState.Select( u => u.Guid ).ToList();
                foreach ( var discount in registrationTemplateDiscountService
                    .Queryable()
                    .Where( d =>
                        d.RegistrationTemplateId == registrationTemplate.Id &&
                        !discountUiGuids.Contains( d.Guid ) ) )
                {
                    registrationTemplateDiscountService.Delete( discount );
                }

                // delete fees that aren't assigned in the UI anymore
                var feeUiGuids = FeeState.Select( u => u.Guid ).ToList();
                var deletedfees = registrationTemplateFeeService
                    .Queryable()
                    .Where( d =>
                        d.RegistrationTemplateId == registrationTemplate.Id &&
                        !feeUiGuids.Contains( d.Guid ) )
                    .ToList();

                var deletedFeeIds = deletedfees.Select( f => f.Id ).ToList();
                foreach ( var registrantFee in registrationRegistrantFeeService
                    .Queryable()
                    .Where( f => deletedFeeIds.Contains( f.RegistrationTemplateFeeId ) )
                    .ToList() )
                {
                    registrationRegistrantFeeService.Delete( registrantFee );
                }

                foreach ( var fee in deletedfees )
                {
                    registrationTemplateFeeService.Delete( fee );
                }

                int? registrationRegistrantEntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.RegistrationRegistrant ) ).Id;
                var registrationRegistrantAttributeQualifierColumn = "RegistrationTemplateId";
                var registrationRegistrantAttributeQualifierValue = registrationTemplate.Id.ToString();

                // Get the registrant attributes still in the UI
                var registrantAttributesUI = FormFieldsState
                    .SelectMany( s =>
                        s.Value.Where( a =>
                            a.FieldSource == RegistrationFieldSource.RegistrantAttribute &&
                            a.Attribute != null ) )
                    .Select( f => f.Attribute )
                    .ToList();
                var selectedAttributeGuids = registrantAttributesUI.Select( a => a.Guid );

                // Delete the registrant attributes that were removed from the UI
                var registrantAttributesDB = attributeService.GetByEntityTypeQualifier( registrationRegistrantEntityTypeId, registrationRegistrantAttributeQualifierColumn, registrationRegistrantAttributeQualifierValue, true );
                foreach ( var attr in registrantAttributesDB.Where( a => !selectedAttributeGuids.Contains( a.Guid ) ).ToList() )
                {
                    var canDeleteAttribute = true;
                    foreach ( var form in registrationTemplate.Forms )
                    {
                        // make sure other RegistrationTemplates aren't using this AttributeId (which could happen due to an old bug)
                        var formFieldsFromOtherRegistrationTemplatesUsingAttribute = registrationTemplateFormFieldService.Queryable().Where( a => a.AttributeId.Value == attr.Id && a.RegistrationTemplateForm.RegistrationTemplateId != registrationTemplate.Id ).Any();
                        if ( formFieldsFromOtherRegistrationTemplatesUsingAttribute )
                        {
                            canDeleteAttribute = false;
                        }
                    }

                    if ( canDeleteAttribute )
                    {
                        attributeService.Delete( attr );
                    }
                }

                rockContext.SaveChanges();

                // Save all of the registrant attributes still in the UI
                foreach ( var attr in registrantAttributesUI )
                {
                    Helper.SaveAttributeEdits( attr, registrationRegistrantEntityTypeId, registrationRegistrantAttributeQualifierColumn, registrationRegistrantAttributeQualifierValue, rockContext );
                }

                // add/updated forms/fields
                foreach ( var formUI in FormState )
                {
                    var form = registrationTemplate.Forms.FirstOrDefault( f => f.Guid.Equals( formUI.Guid ) );
                    if ( form == null )
                    {
                        form = new RegistrationTemplateForm();
                        form.Guid = formUI.Guid;
                        registrationTemplate.Forms.Add( form );
                    }

                    form.Name = formUI.Name;
                    form.Order = formUI.Order;

                    if ( FormFieldsState.ContainsKey( form.Guid ) )
                    {
                        foreach ( var formFieldUI in FormFieldsState[form.Guid] )
                        {
                            var formField = form.Fields.FirstOrDefault( a => a.Guid.Equals( formFieldUI.Guid ) );
                            if ( formField == null )
                            {
                                formField = new RegistrationTemplateFormField();
                                formField.Guid = formFieldUI.Guid;
                                form.Fields.Add( formField );
                            }

                            formField.AttributeId = formFieldUI.AttributeId;
                            if ( !formField.AttributeId.HasValue &&
                                formFieldUI.FieldSource == RegistrationFieldSource.RegistrantAttribute &&
                                formFieldUI.Attribute != null )
                            {
                                var attr = AttributeCache.Get( formFieldUI.Attribute.Guid, rockContext );
                                if ( attr != null )
                                {
                                    formField.AttributeId = attr.Id;
                                }
                            }

                            formField.FieldSource = formFieldUI.FieldSource;
                            formField.PersonFieldType = formFieldUI.PersonFieldType;
                            formField.IsInternal = formFieldUI.IsInternal;
                            formField.IsSharedValue = formFieldUI.IsSharedValue;
                            formField.ShowCurrentValue = formFieldUI.ShowCurrentValue;
                            formField.PreText = formFieldUI.PreText;
                            formField.PostText = formFieldUI.PostText;
                            formField.IsGridField = formFieldUI.IsGridField;
                            formField.IsRequired = formFieldUI.IsRequired;
                            formField.Order = formFieldUI.Order;
                            formField.ShowOnWaitlist = formFieldUI.ShowOnWaitlist;
                            formField.FieldVisibilityRules = formFieldUI.FieldVisibilityRules;
                        }
                    }
                }

                // add/updated discounts
                foreach ( var discountUI in DiscountState )
                {
                    var discount = registrationTemplate.Discounts.FirstOrDefault( a => a.Guid.Equals( discountUI.Guid ) );
                    if ( discount == null )
                    {
                        discount = new RegistrationTemplateDiscount();
                        discount.Guid = discountUI.Guid;
                        registrationTemplate.Discounts.Add( discount );
                    }

                    discount.Code = discountUI.Code;
                    discount.DiscountPercentage = discountUI.DiscountPercentage;
                    discount.DiscountAmount = discountUI.DiscountAmount;
                    discount.Order = discountUI.Order;
                    discount.MaxUsage = discountUI.MaxUsage;
                    discount.MaxRegistrants = discountUI.MaxRegistrants;
                    discount.MinRegistrants = discountUI.MinRegistrants;
                    discount.StartDate = discountUI.StartDate;
                    discount.EndDate = discountUI.EndDate;
                    discount.AutoApplyDiscount = discountUI.AutoApplyDiscount;
                }

                // add/updated fees
                foreach ( var feeUI in FeeState )
                {
                    var fee = registrationTemplate.Fees.FirstOrDefault( a => a.Guid.Equals( feeUI.Guid ) );
                    if ( fee == null )
                    {
                        fee = new RegistrationTemplateFee();
                        fee.Guid = feeUI.Guid;
                        registrationTemplate.Fees.Add( fee );
                    }

                    fee.Name = feeUI.Name;
                    fee.FeeType = feeUI.FeeType;

                    // delete any feeItems no longer defined
                    foreach ( var deletedFeeItem in fee.FeeItems.ToList().Where( a => !feeUI.FeeItems.Any( x => x.Guid == a.Guid ) ) )
                    {
                        registrationTemplateFeeItemService.Delete( deletedFeeItem );
                    }

                    // add any new feeItems
                    foreach ( var newFeeItem in feeUI.FeeItems.ToList().Where( a => !fee.FeeItems.Any( x => x.Guid == a.Guid ) ) )
                    {
                        newFeeItem.RegistrationTemplateFee = fee;
                        newFeeItem.RegistrationTemplateFeeId = fee.Id;
                        registrationTemplateFeeItemService.Add( newFeeItem );
                    }

                    // update feeItems to match
                    foreach ( var feeItem in fee.FeeItems )
                    {
                        var feeItemUI = feeUI.FeeItems.FirstOrDefault( x => x.Guid == feeItem.Guid );
                        feeItem.Order = feeItemUI.Order;
                        feeItem.Name = feeItemUI.Name;
                        feeItem.Cost = feeItemUI.Cost;
                        feeItem.MaximumUsageCount = feeItemUI.MaximumUsageCount;
                    }

                    fee.DiscountApplies = feeUI.DiscountApplies;
                    fee.AllowMultiple = feeUI.AllowMultiple;
                    fee.Order = feeUI.Order;
                    fee.IsActive = feeUI.IsActive;
                    fee.IsRequired = feeUI.IsRequired;
                }

                rockContext.SaveChanges();

                SaveAttributes( new Registration().TypeId, "RegistrationTemplateId", registrationTemplate.Id.ToString(), RegistrationAttributesState, rockContext );

                // If this is a new template, give the current user and the Registration Administrators role administrative
                // rights to this template, and staff, and staff like roles edit rights
                if ( newTemplate )
                {
                    registrationTemplate.AllowPerson( Authorization.ADMINISTRATE, CurrentPerson, rockContext );

                    var registrationAdmins = groupService.Get( Rock.SystemGuid.Group.GROUP_EVENT_REGISTRATION_ADMINISTRATORS.AsGuid() );
                    registrationTemplate.AllowSecurityRole( Authorization.ADMINISTRATE, registrationAdmins, rockContext );

                    var staffLikeUsers = groupService.Get( Rock.SystemGuid.Group.GROUP_STAFF_LIKE_MEMBERS.AsGuid() );
                    registrationTemplate.AllowSecurityRole( Authorization.EDIT, staffLikeUsers, rockContext );

                    var staffUsers = groupService.Get( Rock.SystemGuid.Group.GROUP_STAFF_MEMBERS.AsGuid() );
                    registrationTemplate.AllowSecurityRole( Authorization.EDIT, staffUsers, rockContext );
                }

                var qryParams = new Dictionary<string, string>();
                qryParams["RegistrationTemplateId"] = registrationTemplate.Id.ToString();
                NavigateToPage( RockPage.Guid, qryParams );
            }
        }

        /// <summary>
        /// Saves the attributes.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="qualifierColumn">The qualifier column.</param>
        /// <param name="qualifierValue">The qualifier value.</param>
        /// <param name="viewStateAttributes">The view state attributes.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SaveAttributes( int entityTypeId, string qualifierColumn, string qualifierValue, List<Attribute> viewStateAttributes, RockContext rockContext )
        {
            // Get the existing attributes for this entity type and qualifier value
            var attributeService = new AttributeService( rockContext );
            var attributes = attributeService.GetByEntityTypeQualifier( entityTypeId, qualifierColumn, qualifierValue, true );

            // Delete any of those attributes that were removed in the UI
            var selectedAttributeGuids = viewStateAttributes.Select( a => a.Guid );
            var attributesToDelete = attributes.Where( a => !selectedAttributeGuids.Contains( a.Guid ) ).ToList();
            foreach ( var attr in attributesToDelete )
            {
                attributeService.Delete( attr );
                rockContext.SaveChanges();
            }

            // Update the Attributes that were assigned in the UI
            foreach ( var attributeState in viewStateAttributes )
            {
                Helper.SaveAttributeEdits( attributeState, entityTypeId, qualifierColumn, qualifierValue, rockContext );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( hfRegistrationTemplateId.Value.Equals( "0" ) )
            {
                int? parentCategoryId = PageParameter( "ParentCategoryId" ).AsIntegerOrNull();
                if ( parentCategoryId.HasValue )
                {
                    // Cancelling on Add, and we know the parentCategoryId, so we are probably in treeview mode, so navigate to the current page
                    var qryParams = new Dictionary<string, string>();
                    qryParams["CategoryId"] = parentCategoryId.ToString();
                    NavigateToPage( RockPage.Guid, qryParams );
                }
                else
                {
                    // Cancelling on Add.  Return to Grid
                    NavigateToParentPage();
                }
            }
            else
            {
                // Cancelling on Edit.  Return to Details
                RegistrationTemplateService service = new RegistrationTemplateService( new RockContext() );
                RegistrationTemplate item = service.Get( int.Parse( hfRegistrationTemplateId.Value ) );
                ShowReadonlyDetails( item );
            }
        }

        #endregion

        #region Details Events

        /// <summary>
        /// Handles the CheckedChanged event of the cbMultipleRegistrants control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cbMultipleRegistrants_CheckedChanged( object sender, EventArgs e )
        {
            ParseControls();

            nbMaxRegistrants.Visible = cbMultipleRegistrants.Checked;

            BuildControls();
        }

        /// <summary>
        /// Handles the CheckedChanged event of the tglSetCost control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tglSetCost_CheckedChanged( object sender, EventArgs e )
        {
            SetCostVisibility();
        }

        #endregion

        #region Form Control Events

        /// <summary>
        /// Handles the Click event of the lbAddForm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddForm_Click( object sender, EventArgs e )
        {
            ParseControls();

            var form = new RegistrationTemplateForm();
            form.Guid = Guid.NewGuid();
            form.Order = FormState.Any() ? FormState.Max( a => a.Order ) + 1 : 0;
            FormState.Add( form );

            FormFieldsState.Add( form.Guid, new List<RegistrationTemplateFormField>() );

            ExpandedForms.Add( form.Guid );

            BuildControls( true, form.Guid );
        }

        /// <summary>
        /// Handles the DeleteFormClick event of the tfeForm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tfeForm_DeleteFormClick( object sender, EventArgs e )
        {
            ParseControls();

            var templateFormEditor = sender as RegistrationTemplateFormEditor;
            if ( templateFormEditor != null )
            {
                var form = FormState.Where( a => a.Guid == templateFormEditor.FormGuid ).FirstOrDefault();
                if ( form != null )
                {
                    if ( ExpandedForms.Contains( form.Guid ) )
                    {
                        ExpandedForms.Remove( form.Guid );
                    }

                    if ( FormFieldsState.ContainsKey( form.Guid ) )
                    {
                        FormFieldsState.Remove( form.Guid );
                    }

                    FormState.Remove( form );
                }
            }

            BuildControls( true );
        }

        /// <summary>
        /// Tfes the form_ add attribute click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void tfeForm_AddFieldClick( object sender, TemplateFormFieldEventArg e )
        {
            ParseControls();

            ShowRegistrantFormFieldEdit( e.FormGuid, Guid.NewGuid() );

            BuildControls( true );
        }

        /// <summary>
        /// Tfes the form filter field click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void tfeForm_FilterFieldClick( object sender, TemplateFormFieldEventArg e )
        {
            ParseControls();

            ShowFormFieldFilter( e.FormGuid, e.FormFieldGuid );

            BuildControls( true );
        }

        /// <summary>
        /// Tfes the form_ edit attribute click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void tfeForm_EditFieldClick( object sender, TemplateFormFieldEventArg e )
        {
            ParseControls();

            ShowRegistrantFormFieldEdit( e.FormGuid, e.FormFieldGuid );

            BuildControls( true );
        }

        /// <summary>
        /// Tfes the form_ reorder attribute click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void tfeForm_ReorderFieldClick( object sender, TemplateFormFieldEventArg e )
        {
            ParseControls();

            if ( FormFieldsState.ContainsKey( e.FormGuid ) )
            {
                SortFields( FormFieldsState[e.FormGuid], e.OldIndex, e.NewIndex );
                ReOrderFields( FormFieldsState[e.FormGuid] );
            }

            BuildControls( true, e.FormGuid );
        }

        /// <summary>
        /// Tfes the form_ delete attribute click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void tfeForm_DeleteFieldClick( object sender, TemplateFormFieldEventArg e )
        {
            ParseControls();

            if ( FormFieldsState.ContainsKey( e.FormGuid ) )
            {
                FormFieldsState[e.FormGuid].RemoveEntity( e.FormFieldGuid );
            }

            BuildControls( true, e.FormGuid );
        }

        /// <summary>
        /// Tfes the form_ rebind attribute click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void tfeForm_RebindFieldClick( object sender, TemplateFormFieldEventArg e )
        {
            ParseControls();

            BuildControls( true, e.FormGuid );
        }

        #endregion

        #region Field Dialog Events

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlFieldSource control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlFieldSource_SelectedIndexChanged( object sender, EventArgs e )
        {
            SetFieldDisplay();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the gtpGroupType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gtpGroupType_SelectedIndexChanged( object sender, EventArgs e )
        {
            rpGroupTypeRole.GroupTypeId = gtpGroupType.SelectedGroupTypeId ?? 0;
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgRegistrantFormField control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgRegistrantFormField_SaveClick( object sender, EventArgs e )
        {
            FieldSave();
            HideDialog();
            BuildControls( true );
        }

        /// <summary>
        /// Saves the form field
        /// </summary>
        private void FieldSave()
        {
            var formGuid = hfFormGuid.Value.AsGuid();

            if ( FormFieldsState.ContainsKey( formGuid ) )
            {
                var attributeForm = CreateFormField( formGuid );

                int? attributeId = null;

                switch ( attributeForm.FieldSource )
                {
                    case RegistrationFieldSource.PersonField:
                        {
                            attributeForm.ShowCurrentValue = cbUsePersonCurrentValue.Checked;
                            attributeForm.IsGridField = cbShowOnGrid.Checked;
                            attributeForm.IsRequired = cbRequireInInitialEntry.Checked;
                            break;
                        }

                    case RegistrationFieldSource.PersonAttribute:
                        {
                            attributeId = ddlPersonAttributes.SelectedValueAsInt();
                            attributeForm.ShowCurrentValue = cbUsePersonCurrentValue.Checked;
                            attributeForm.IsGridField = cbShowOnGrid.Checked;
                            attributeForm.IsRequired = cbRequireInInitialEntry.Checked;
                            break;
                        }

                    case RegistrationFieldSource.GroupMemberAttribute:
                        {
                            attributeId = ddlGroupTypeAttributes.SelectedValueAsInt();
                            attributeForm.ShowCurrentValue = false;
                            attributeForm.IsGridField = cbShowOnGrid.Checked;
                            attributeForm.IsRequired = cbRequireInInitialEntry.Checked;
                            break;
                        }

                    case RegistrationFieldSource.RegistrantAttribute:
                        {
                            Rock.Model.Attribute attribute = new Rock.Model.Attribute();
                            edtRegistrantAttribute.GetAttributeProperties( attribute );
                            attributeForm.Attribute = attribute;
                            attributeForm.Id = attribute.Id;
                            attributeForm.ShowCurrentValue = false;
                            attributeForm.IsGridField = attribute.IsGridColumn;
                            attributeForm.IsRequired = attribute.IsRequired;
                            break;
                        }
                }

                attributeForm.ShowOnWaitlist = cbShowOnWaitList.Checked;

                if ( attributeId.HasValue )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        var attribute = new AttributeService( rockContext ).Get( attributeId.Value );
                        if ( attribute != null )
                        {
                            attributeForm.Attribute = attribute.Clone( false );
                            attributeForm.Attribute.FieldType = attribute.FieldType.Clone( false );
                            attributeForm.Attribute.AttributeQualifiers = new List<AttributeQualifier>();

                            foreach ( var qualifier in attribute.AttributeQualifiers )
                            {
                                attributeForm.Attribute.AttributeQualifiers.Add( qualifier.Clone( false ) );
                            }

                            attributeForm.AttributeId = attribute.Id;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates the form field.
        /// </summary>
        /// <param name="formGuid">The form unique identifier.</param>
        /// <returns></returns>
        private RegistrationTemplateFormField CreateFormField( Guid formGuid )
        {
            var attributeGuid = hfAttributeGuid.Value.AsGuid();

            var attributeFormField = FormFieldsState[formGuid].FirstOrDefault( a => a.Guid.Equals( attributeGuid ) );
            if ( attributeFormField == null )
            {
                attributeFormField = new RegistrationTemplateFormField();
                attributeFormField.Order = FormFieldsState[formGuid].Any() ? FormFieldsState[formGuid].Max( a => a.Order ) + 1 : 0;
                attributeFormField.Guid = attributeGuid;
                FormFieldsState[formGuid].Add( attributeFormField );
            }

            attributeFormField.PreText = ceFormFieldPreHtml.Text;
            attributeFormField.PostText = ceFormFieldPostHtml.Text;
            attributeFormField.FieldSource = ddlFieldSource.SelectedValueAsEnum<RegistrationFieldSource>();
            if ( ddlPersonField.Visible )
            {
                attributeFormField.PersonFieldType = ddlPersonField.SelectedValueAsEnum<RegistrationPersonFieldType>();
            }

            attributeFormField.IsInternal = cbInternalField.Checked;
            attributeFormField.IsSharedValue = cbCommonValue.Checked;

            return attributeFormField;
        }

        #endregion

        #region Discount Events

        /// <summary>
        /// Handles the AddClick event of the gDiscounts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gDiscounts_AddClick( object sender, EventArgs e )
        {
            ParseControls();

            ShowDiscountEdit( Guid.NewGuid() );

            BuildControls();
        }

        /// <summary>
        /// Handles the Edit event of the gDiscounts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gDiscounts_Edit( object sender, RowEventArgs e )
        {
            ParseControls();

            ShowDiscountEdit( e.RowKeyValue.ToString().AsGuid() );

            BuildControls();
        }

        /// <summary>
        /// Handles the GridReorder event of the gDiscounts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gDiscounts_GridReorder( object sender, GridReorderEventArgs e )
        {
            ParseControls();

            var movedItem = DiscountState.Where( a => a.Order == e.OldIndex ).FirstOrDefault();
            if ( movedItem != null )
            {
                if ( e.NewIndex < e.OldIndex )
                {
                    // Moved up
                    foreach ( var otherItem in DiscountState.Where( a => a.Order < e.OldIndex && a.Order >= e.NewIndex ) )
                    {
                        otherItem.Order = otherItem.Order + 1;
                    }
                }
                else
                {
                    // Moved Down
                    foreach ( var otherItem in DiscountState.Where( a => a.Order > e.OldIndex && a.Order <= e.NewIndex ) )
                    {
                        otherItem.Order = otherItem.Order - 1;
                    }
                }

                movedItem.Order = e.NewIndex;
            }

            int order = 0;
            DiscountState.OrderBy( d => d.Order ).ToList().ForEach( d => d.Order = order++ );

            BuildControls();
        }

        /// <summary>
        /// Handles the Delete event of the gDiscounts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gDiscounts_Delete( object sender, RowEventArgs e )
        {
            ParseControls();

            var discountGuid = e.RowKeyValue.ToString().AsGuid();
            var discount = DiscountState.FirstOrDefault( f => f.Guid.Equals( e.RowKeyValue.ToString().AsGuid() ) );
            if ( discount != null )
            {
                DiscountState.Remove( discount );

                int order = 0;
                DiscountState.OrderBy( f => f.Order ).ToList().ForEach( f => f.Order = order++ );
            }

            BuildControls();
        }

        /// <summary>
        /// Handles the GridRebind event of the gDiscounts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gDiscounts_GridRebind( object sender, EventArgs e )
        {
            BindDiscountsGrid();
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgDiscount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgDiscount_SaveClick( object sender, EventArgs e )
        {
            RegistrationTemplateDiscount discount = null;
            var discountGuid = hfDiscountGuid.Value.AsGuidOrNull();
            if ( discountGuid.HasValue )
            {
                discount = DiscountState.Where( f => f.Guid.Equals( discountGuid.Value ) ).FirstOrDefault();
            }

            if ( discount == null )
            {
                discount = new RegistrationTemplateDiscount();
                discount.Guid = Guid.NewGuid();
                discount.Order = DiscountState.Any() ? DiscountState.Max( d => d.Order ) + 1 : 0;
                DiscountState.Add( discount );
            }

            discount.Code = tbDiscountCode.Text;
            if ( rblDiscountType.SelectedValue == "Amount" )
            {
                discount.DiscountPercentage = 0.0m;
                discount.DiscountAmount = cbDiscountAmount.Text.AsDecimal();
            }
            else
            {
                discount.DiscountPercentage = nbDiscountPercentage.Text.AsDecimal() * 0.01m;
                discount.DiscountAmount = 0.0m;
            }

            discount.MaxUsage = nbDiscountMaxUsage.Text.AsIntegerOrNull();
            discount.MaxRegistrants = nbDiscountMaxRegistrants.Text.AsIntegerOrNull();
            discount.MinRegistrants = nbDiscountMinRegistrants.Text.AsIntegerOrNull();
            discount.StartDate = drpDiscountDateRange.LowerValue;
            discount.EndDate = drpDiscountDateRange.UpperValue;
            discount.AutoApplyDiscount = cbcAutoApplyDiscount.Checked;

            HideDialog();

            hfDiscountGuid.Value = string.Empty;

            BuildControls();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the rblDiscountType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rblDiscountType_SelectedIndexChanged( object sender, EventArgs e )
        {
            if ( rblDiscountType.SelectedValue == "Amount" )
            {
                nbDiscountPercentage.Visible = false;
                cbDiscountAmount.Visible = true;
            }
            else
            {
                nbDiscountPercentage.Visible = true;
                cbDiscountAmount.Visible = false;
            }
        }

        #endregion

        #region Fee Events

        /// <summary>
        /// Handles the AddClick event of the gFees control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gFees_AddClick( object sender, EventArgs e )
        {
            ParseControls();

            ShowFeeEdit( Guid.NewGuid() );

            BuildControls();
        }

        /// <summary>
        /// Handles the Edit event of the gFees control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gFees_Edit( object sender, RowEventArgs e )
        {
            ParseControls();

            ShowFeeEdit( e.RowKeyValue.ToString().AsGuid() );
        }

        /// <summary>
        /// Handles the GridReorder event of the gFees control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gFees_GridReorder( object sender, GridReorderEventArgs e )
        {
            ParseControls();

            var movedItem = FeeState.Where( a => a.Order == e.OldIndex ).FirstOrDefault();
            if ( movedItem != null )
            {
                if ( e.NewIndex < e.OldIndex )
                {
                    // Moved up
                    foreach ( var otherItem in FeeState.Where( a => a.Order < e.OldIndex && a.Order >= e.NewIndex ) )
                    {
                        otherItem.Order = otherItem.Order + 1;
                    }
                }
                else
                {
                    // Moved Down
                    foreach ( var otherItem in FeeState.Where( a => a.Order > e.OldIndex && a.Order <= e.NewIndex ) )
                    {
                        otherItem.Order = otherItem.Order - 1;
                    }
                }

                movedItem.Order = e.NewIndex;
            }

            int order = 0;
            FeeState.OrderBy( f => f.Order ).ToList().ForEach( f => f.Order = order++ );

            BuildControls();
        }

        /// <summary>
        /// Handles the Delete event of the gFees control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gFees_Delete( object sender, RowEventArgs e )
        {
            ParseControls();

            var feeGuid = e.RowKeyValue.ToString().AsGuid();
            var fee = FeeState.FirstOrDefault( f => f.Guid.Equals( e.RowKeyValue.ToString().AsGuid() ) );
            if ( fee != null )
            {
                FeeState.Remove( fee );

                int order = 0;
                FeeState.OrderBy( f => f.Order ).ToList().ForEach( f => f.Order = order++ );
            }

            BuildControls();
        }

        /// <summary>
        /// Handles the GridRebind event of the gFees control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gFees_GridRebind( object sender, EventArgs e )
        {
            BindFeesGrid();
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgFee control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgFee_SaveClick( object sender, EventArgs e )
        {
            RegistrationTemplateFee fee = null;
            var feeGuid = hfFeeGuid.Value.AsGuidOrNull();
            if ( feeGuid.HasValue )
            {
                fee = FeeState.Where( f => f.Guid.Equals( feeGuid.Value ) ).FirstOrDefault();
            }

            if ( fee == null )
            {
                fee = new RegistrationTemplateFee();
                fee.Guid = Guid.NewGuid();
                fee.Order = FeeState.Any() ? FeeState.Max( d => d.Order ) + 1 : 0;
                FeeState.Add( fee );
            }

            fee.Name = tbFeeName.Text;
            fee.FeeType = rblFeeType.SelectedValueAsEnum<RegistrationFeeType>();
            fee.AllowMultiple = cbAllowMultiple.Checked;
            fee.DiscountApplies = cbDiscountApplies.Checked;
            fee.IsActive = cbFeeIsActive.Checked;
            fee.IsRequired = cbFeeIsRequired.Checked;

            // set the FeeItems to what they are in the UI
            fee.FeeItems = new List<RegistrationTemplateFeeItem>();

            if ( fee.FeeType == RegistrationFeeType.Single )
            {
                RegistrationTemplateFeeItem registrationTemplateFeeItem = new RegistrationTemplateFeeItem();
                registrationTemplateFeeItem.Guid = hfFeeItemSingleGuid.Value.AsGuid();
                registrationTemplateFeeItem.Name = fee.Name;
                registrationTemplateFeeItem.Cost = cbFeeItemSingleCost.Value ?? 0.00M;
                registrationTemplateFeeItem.MaximumUsageCount = nbFeeItemSingleMaximumUsageCount.Text.AsIntegerOrNull();
                fee.FeeItems.Add( registrationTemplateFeeItem );
            }
            else
            {
                fee.FeeItems = GetFeeItemsFromUI();

                if ( !ValidateFeeItemUIValues() )
                {
                    return;
                }
            }

            hfFeeGuid.Value = string.Empty;
            HideDialog();
            BuildControls();
        }

        /// <summary>
        /// Validates the fee item UI values.
        /// </summary>
        /// <returns></returns>
        private bool ValidateFeeItemUIValues()
        {
            var result = true;
            foreach ( var item in rptFeeItemsMultiple.Items.OfType<RepeaterItem>() )
            {
                RegistrationTemplateFeeItem registrationTemplateFeeItem = new RegistrationTemplateFeeItem();
                var nbFeeItemWarning = item.FindControl( "nbFeeItemWarning" ) as NotificationBox;
                var tbFeeItemName = item.FindControl( "tbFeeItemName" ) as RockTextBox;
                var cbFeeItemCost = item.FindControl( "cbFeeItemCost" ) as CurrencyBox;
                var nbMaximumUsageCount = item.FindControl( "nbMaximumUsageCount" ) as NumberBox;
                var pnlFeeItemNameContainer = item.FindControl( "pnlFeeItemNameContainer" ) as Panel;
                if ( tbFeeItemName.Text.IsNullOrWhiteSpace() )
                {
                    result = false;
                    pnlFeeItemNameContainer.AddCssClass( "has-error" );
                    nbFeeItemWarning.Text = "Option is required.";
                    nbFeeItemWarning.NotificationBoxType = NotificationBoxType.Danger;
                    nbFeeItemWarning.Visible = true;
                }
                else
                {
                    pnlFeeItemNameContainer.RemoveCssClass( "has-error" );
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the fee item UI values.
        /// </summary>
        /// <returns></returns>
        private List<RegistrationTemplateFeeItem> GetFeeItemsFromUI()
        {
            var feeItemOrder = 0;
            var feeItems = new List<RegistrationTemplateFeeItem>();
            foreach ( var item in rptFeeItemsMultiple.Items.OfType<RepeaterItem>() )
            {
                RegistrationTemplateFeeItem registrationTemplateFeeItem = new RegistrationTemplateFeeItem();
                var hfFeeItemGuid = item.FindControl( "hfFeeItemGuid" ) as HiddenField;
                var tbFeeItemName = item.FindControl( "tbFeeItemName" ) as RockTextBox;
                var cbFeeItemCost = item.FindControl( "cbFeeItemCost" ) as CurrencyBox;
                var nbMaximumUsageCount = item.FindControl( "nbMaximumUsageCount" ) as NumberBox;

                registrationTemplateFeeItem.Guid = hfFeeItemGuid.Value.AsGuid();
                registrationTemplateFeeItem.Order = feeItemOrder++;
                registrationTemplateFeeItem.Name = tbFeeItemName.Text;
                registrationTemplateFeeItem.Cost = cbFeeItemCost.Value ?? 0.00M;
                registrationTemplateFeeItem.MaximumUsageCount = nbMaximumUsageCount.Text.AsIntegerOrNull();
                feeItems.Add( registrationTemplateFeeItem );
            }

            return feeItems;
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the rblFeeType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rblFeeType_SelectedIndexChanged( object sender, EventArgs e )
        {
            var feeItems = FeeItemsEditState;
            BindFeeItemsControls( feeItems, rblFeeType.SelectedValueAsEnum<RegistrationFeeType>() );
        }

        #endregion

        #endregion

        #region Show Details

        /// <summary>
        /// Gets the registration template.
        /// </summary>
        /// <param name="registrationTemplateId">The registration template identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private RegistrationTemplate GetRegistrationTemplate( int registrationTemplateId, RockContext rockContext = null )
        {
            string key = string.Format( "RegistrationTemplate:{0}", registrationTemplateId );
            RegistrationTemplate registrationTemplate = RockPage.GetSharedItem( key ) as RegistrationTemplate;
            if ( registrationTemplate == null )
            {
                rockContext = rockContext ?? new RockContext();
                registrationTemplate = new RegistrationTemplateService( rockContext )
                    .Queryable( "GroupType.Roles" )
                    .AsNoTracking()
                    .FirstOrDefault( i => i.Id == registrationTemplateId );
                RockPage.SaveSharedItem( key, registrationTemplate );
            }

            return registrationTemplate;
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        private void ShowDetail()
        {
            int? registrationTemplateId = PageParameter( "RegistrationTemplateId" ).AsIntegerOrNull();
            int? parentCategoryId = PageParameter( "ParentCategoryId" ).AsIntegerOrNull();

            if ( !registrationTemplateId.HasValue )
            {
                pnlDetails.Visible = false;
                return;
            }

            var rockContext = new RockContext();

            RegistrationTemplate registrationTemplate = null;
            if ( registrationTemplateId.HasValue )
            {
                registrationTemplate = GetRegistrationTemplate( registrationTemplateId.Value, rockContext );
            }

            if ( registrationTemplate == null )
            {
                registrationTemplate = new RegistrationTemplate();
                registrationTemplate.Id = 0;
                registrationTemplate.IsActive = true;
                registrationTemplate.CategoryId = parentCategoryId;
                registrationTemplate.ConfirmationFromName = "{{ RegistrationInstance.ContactPersonAlias.Person.FullName }}";
                registrationTemplate.ConfirmationFromEmail = "{{ RegistrationInstance.ContactEmail }}";
                registrationTemplate.ConfirmationSubject = "{{ RegistrationInstance.Name }} Confirmation";
                registrationTemplate.ConfirmationEmailTemplate = GetAttributeValue( "DefaultConfirmationEmail" );
                registrationTemplate.ReminderFromName = "{{ RegistrationInstance.ContactPersonAlias.Person.FullName }}";
                registrationTemplate.ReminderFromEmail = "{{ RegistrationInstance.ContactEmail }}";
                registrationTemplate.ReminderSubject = "{{ RegistrationInstance.Name }} Reminder";
                registrationTemplate.ReminderEmailTemplate = GetAttributeValue( "DefaultReminderEmail" );
                registrationTemplate.Notify = RegistrationNotify.None;
                registrationTemplate.SuccessTitle = "Congratulations {{ Registration.FirstName }}";
                registrationTemplate.SuccessText = GetAttributeValue( "DefaultSuccessText" );
                registrationTemplate.PaymentReminderEmailTemplate = GetAttributeValue( "DefaultPaymentReminderEmail" );
                registrationTemplate.PaymentReminderFromEmail = "{{ RegistrationInstance.ContactEmail }}";
                registrationTemplate.PaymentReminderFromName = "{{ RegistrationInstance.ContactPersonAlias.Person.FullName }}";
                registrationTemplate.PaymentReminderSubject = "{{ RegistrationInstance.Name }} Payment Reminder";
                registrationTemplate.WaitListTransitionEmailTemplate = GetAttributeValue( "DefaultWaitListTransitionEmail" );
                registrationTemplate.WaitListTransitionFromEmail = "{{ RegistrationInstance.ContactEmail }}";
                registrationTemplate.WaitListTransitionFromName = "{{ RegistrationInstance.ContactPersonAlias.Person.FullName }}";
                registrationTemplate.WaitListTransitionSubject = "{{ RegistrationInstance.Name }} Wait List Update";
                registrationTemplate.AllowMultipleRegistrants = true;
                registrationTemplate.MaxRegistrants = 10;
                registrationTemplate.GroupMemberStatus = GroupMemberStatus.Active;
            }

            pnlDetails.Visible = true;
            hfRegistrationTemplateId.Value = registrationTemplate.Id.ToString();

            // render UI based on Authorized
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;

            // User must have 'Edit' rights to block, or 'Administrate' rights to template
            if ( !UserCanEdit && !registrationTemplate.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) )
            {
                readOnly = true;
                nbEditModeMessage.Heading = "Information";
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( RegistrationTemplate.FriendlyTypeName );
            }

            if ( readOnly )
            {
                btnEdit.Visible = false;
                btnDelete.Visible = false;
                btnCopy.Visible = false;
                btnSecurity.Visible = false;
                ShowReadonlyDetails( registrationTemplate );
            }
            else
            {
                btnEdit.Visible = true;
                btnDelete.Visible = true;

                btnCopy.ToolTip = "Copy " + registrationTemplate.Name;
                btnCopy.Visible = true;

                btnSecurity.Title = "Secure " + registrationTemplate.Name;
                btnSecurity.EntityId = registrationTemplate.Id;

                if ( registrationTemplate.Id > 0 )
                {
                    SetHasRegistrations( registrationTemplate.Id, rockContext );
                    ShowReadonlyDetails( registrationTemplate );
                }
                else
                {
                    LoadStateDetails( registrationTemplate, rockContext );
                    ShowEditDetails( registrationTemplate, rockContext );
                }
            }
        }

        /// <summary>
        /// Loads the state details.
        /// </summary>
        /// <param name="registrationTemplate">The registration template.</param>
        /// <param name="rockContext">The rock context.</param>
        private void LoadStateDetails( RegistrationTemplate registrationTemplate, RockContext rockContext )
        {
            if ( registrationTemplate != null )
            {
                // If no forms, add at one
                if ( !registrationTemplate.Forms.Any() )
                {
                    var form = new RegistrationTemplateForm();
                    form.Guid = Guid.NewGuid();
                    form.Order = 0;
                    form.Name = "Default Form";
                    registrationTemplate.Forms.Add( form );
                }

                var defaultForm = registrationTemplate.Forms.First();

                // Add first name field if it doesn't exist
                if ( !defaultForm.Fields
                    .Any( f =>
                        f.FieldSource == RegistrationFieldSource.PersonField &&
                        f.PersonFieldType == RegistrationPersonFieldType.FirstName ) )
                {
                    var formField = new RegistrationTemplateFormField();
                    formField.FieldSource = RegistrationFieldSource.PersonField;
                    formField.PersonFieldType = RegistrationPersonFieldType.FirstName;
                    formField.IsGridField = true;
                    formField.IsRequired = true;
                    formField.ShowOnWaitlist = true;
                    formField.PreText = @"<div class='row'><div class='col-md-6'>";
                    formField.PostText = "    </div>";
                    formField.Order = defaultForm.Fields.Any() ? defaultForm.Fields.Max( f => f.Order ) + 1 : 0;
                    defaultForm.Fields.Add( formField );
                }

                // Add last name field if it doesn't exist
                if ( !defaultForm.Fields
                    .Any( f =>
                        f.FieldSource == RegistrationFieldSource.PersonField &&
                        f.PersonFieldType == RegistrationPersonFieldType.LastName ) )
                {
                    var formField = new RegistrationTemplateFormField();
                    formField.FieldSource = RegistrationFieldSource.PersonField;
                    formField.PersonFieldType = RegistrationPersonFieldType.LastName;
                    formField.IsGridField = true;
                    formField.IsRequired = true;
                    formField.ShowOnWaitlist = true;
                    formField.PreText = "    <div class='col-md-6'>";
                    formField.PostText = @"    </div></div>";
                    formField.Order = defaultForm.Fields.Any() ? defaultForm.Fields.Max( f => f.Order ) + 1 : 0;
                    defaultForm.Fields.Add( formField );
                }

                FormState = new List<RegistrationTemplateForm>();
                FormFieldsState = new Dictionary<Guid, List<RegistrationTemplateFormField>>();
                foreach ( var form in registrationTemplate.Forms.OrderBy( f => f.Order ) )
                {
                    FormState.Add( form.Clone( false ) );
                    FormFieldsState.Add( form.Guid, form.Fields.ToList() );
                }

                DiscountState = registrationTemplate.Discounts.OrderBy( a => a.Order ).ToList();
                FeeState = registrationTemplate.Fees.OrderBy( a => a.Order ).ToList();
                var attributeService = new AttributeService( rockContext );
                RegistrationAttributesState = attributeService.GetByEntityTypeId( new Registration().TypeId, true ).AsQueryable()
                    .Where( a =>
                        a.EntityTypeQualifierColumn.Equals( "RegistrationTemplateId", StringComparison.OrdinalIgnoreCase ) &&
                        a.EntityTypeQualifierValue.Equals( registrationTemplate.Id.ToString() ) )
                    .OrderBy( a => a.Order )
                    .ThenBy( a => a.Name )
                    .ToList();

            }
            else
            {
                FormState = new List<RegistrationTemplateForm>();
                FormFieldsState = new Dictionary<Guid, List<RegistrationTemplateFormField>>();
                DiscountState = new List<RegistrationTemplateDiscount>();
                FeeState = new List<RegistrationTemplateFee>();
                RegistrationAttributesState = new List<Attribute>();
            }
        }

        /// <summary>
        /// Sets the has registrations.
        /// </summary>
        /// <param name="registrationTemplateId">The registration template identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SetHasRegistrations( int registrationTemplateId, RockContext rockContext )
        {
            hfHasRegistrations.Value = new RegistrationInstanceService( rockContext )
                .Queryable().AsNoTracking()
                .Any( i =>

                    i.RegistrationTemplateId == registrationTemplateId &&
                    i.Registrations.Any( r => !r.IsTemporary ) ).ToString();
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="registrationTemplate">The registration template.</param>
        /// <param name="rockContext">The rock context.</param>
        private void ShowEditDetails( RegistrationTemplate registrationTemplate, RockContext rockContext )
        {
            if ( registrationTemplate.Id == 0 )
            {
                lReadOnlyTitle.Text = ActionTitle.Add( RegistrationTemplate.FriendlyTypeName ).FormatAsHtmlTitle();
                hlInactive.Visible = false;
                hlType.Visible = false;
            }
            else
            {
                pwDetails.Expanded = false;
            }

            pdAuditDetails.Visible = false;
            SetEditMode( true );

            LoadDropDowns( rockContext );

            cbIsActive.Checked = registrationTemplate.IsActive;
            tbName.Text = registrationTemplate.Name;
            cpCategory.SetValue( registrationTemplate.CategoryId );

            gtpGroupType.SelectedGroupTypeId = registrationTemplate.GroupTypeId;
            rpGroupTypeRole.GroupTypeId = registrationTemplate.GroupTypeId ?? 0;
            rpGroupTypeRole.GroupRoleId = registrationTemplate.GroupMemberRoleId;
            ddlGroupMemberStatus.SetValue( registrationTemplate.GroupMemberStatus.ConvertToInt() );
            ddlSignatureDocumentTemplate.SetValue( registrationTemplate.RequiredSignatureDocumentTemplateId );
            cbDisplayInLine.Checked = registrationTemplate.SignatureDocumentAction == SignatureDocumentAction.Embed;
            wtpRegistrationWorkflow.SetValue( registrationTemplate.RegistrationWorkflowTypeId );
            ddlRegistrarOption.SetValue( registrationTemplate.RegistrarOption.ConvertToInt() );

            foreach ( ListItem li in cblNotify.Items )
            {
                RegistrationNotify notify = ( RegistrationNotify ) li.Value.AsInteger();
                li.Selected = ( registrationTemplate.Notify & notify ) == notify;
            }

            cbWaitListEnabled.Checked = registrationTemplate.WaitListEnabled;
            cbAddPersonNote.Checked = registrationTemplate.AddPersonNote;
            cbLoginRequired.Checked = registrationTemplate.LoginRequired;
            cbAllowExternalUpdates.Checked = registrationTemplate.AllowExternalRegistrationUpdates;
            cbAllowGroupPlacement.Checked = registrationTemplate.AllowGroupPlacement;
            cbMultipleRegistrants.Checked = registrationTemplate.AllowMultipleRegistrants;
            nbMaxRegistrants.Visible = registrationTemplate.AllowMultipleRegistrants;
            nbMaxRegistrants.Text = registrationTemplate.MaxRegistrants == 0 ? string.Empty : registrationTemplate.MaxRegistrants.ToString();
            rblRegistrantsInSameFamily.SetValue( registrationTemplate.RegistrantsSameFamily.ConvertToInt() );
            cbShowCurrentFamilyMembers.Checked = registrationTemplate.ShowCurrentFamilyMembers;
            tglSetCostOnTemplate.Checked = !registrationTemplate.SetCostOnInstance.HasValue || !registrationTemplate.SetCostOnInstance.Value;
            cbCost.Text = registrationTemplate.Cost.ToString();
            cbMinimumInitialPayment.Text = registrationTemplate.MinimumInitialPayment.HasValue ? registrationTemplate.MinimumInitialPayment.Value.ToString( "N2" ) : string.Empty;
            cbDefaultPaymentAmount.Text = registrationTemplate.DefaultPayment.HasValue ? registrationTemplate.DefaultPayment.Value.ToString( "N2" ) : string.Empty;
            fgpFinancialGateway.SetValue( registrationTemplate.FinancialGatewayId );
            txtBatchNamePrefix.Text = registrationTemplate.BatchNamePrefix;
            SetCostVisibility();

            tbConfirmationFromName.Text = registrationTemplate.ConfirmationFromName;
            tbConfirmationFromEmail.Text = registrationTemplate.ConfirmationFromEmail;
            tbConfirmationSubject.Text = registrationTemplate.ConfirmationSubject;
            ceConfirmationEmailTemplate.Text = registrationTemplate.ConfirmationEmailTemplate;

            tbReminderFromName.Text = registrationTemplate.ReminderFromName;
            tbReminderFromEmail.Text = registrationTemplate.ReminderFromEmail;
            tbReminderSubject.Text = registrationTemplate.ReminderSubject;
            ceReminderEmailTemplate.Text = registrationTemplate.ReminderEmailTemplate;

            tbPaymentReminderFromName.Text = registrationTemplate.PaymentReminderFromName;
            tbPaymentReminderFromEmail.Text = registrationTemplate.PaymentReminderFromEmail;
            tbPaymentReminderSubject.Text = registrationTemplate.PaymentReminderSubject;
            cePaymentReminderEmailTemplate.Text = registrationTemplate.PaymentReminderEmailTemplate;
            nbPaymentReminderTimeSpan.Text = registrationTemplate.PaymentReminderTimeSpan.ToString();

            tbWaitListTransitionFromName.Text = registrationTemplate.WaitListTransitionFromName;
            tbWaitListTransitionFromEmail.Text = registrationTemplate.WaitListTransitionFromEmail;
            tbWaitListTransitionSubject.Text = registrationTemplate.WaitListTransitionSubject;
            ceWaitListTransitionEmailTemplate.Text = registrationTemplate.WaitListTransitionEmailTemplate;

            tbRegistrationTerm.Text = registrationTemplate.RegistrationTerm;
            tbRegistrantTerm.Text = registrationTemplate.RegistrantTerm;
            tbFeeTerm.Text = registrationTemplate.FeeTerm;
            tbDiscountCodeTerm.Text = registrationTemplate.DiscountCodeTerm;

            tbRegistrationAttributeTitleStart.Text = registrationTemplate.RegistrationAttributeTitleStart;
            tbRegistrationAttributeTitleEnd.Text = registrationTemplate.RegistrationAttributeTitleEnd;

            tbSuccessTitle.Text = registrationTemplate.SuccessTitle;
            ceSuccessText.Text = registrationTemplate.SuccessText;
            heInstructions.Text = registrationTemplate.RegistrationInstructions;
            var defaultForm = FormState.FirstOrDefault();
            BuildControls( true, defaultForm.Guid );
            BindRegistrationAttributesGrid();
        }

        /// <summary>
        /// Sets the cost visibility.
        /// </summary>
        private void SetCostVisibility()
        {
            bool setCostOnTemplate = tglSetCostOnTemplate.Checked;
            cbCost.Visible = setCostOnTemplate;
            cbMinimumInitialPayment.Visible = setCostOnTemplate;
            cbDefaultPaymentAmount.Visible = setCostOnTemplate;
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="registrationTemplate">The registration template.</param>
        private void ShowReadonlyDetails( RegistrationTemplate registrationTemplate )
        {
            SetEditMode( false );

            hfRegistrationTemplateId.SetValue( registrationTemplate.Id );
            FormState = null;
            ExpandedForms = null;
            DiscountState = null;
            FeeState = null;

            pdAuditDetails.Visible = true;
            pdAuditDetails.SetEntity( registrationTemplate, ResolveRockUrl( "~" ) );

            lReadOnlyTitle.Text = registrationTemplate.Name.FormatAsHtmlTitle();
            hlInactive.Visible = registrationTemplate.IsActive == false;
            hlType.Visible = registrationTemplate.Category != null;
            hlType.Text = registrationTemplate.Category != null ? registrationTemplate.Category.Name : string.Empty;
            lGroupType.Text = registrationTemplate.GroupType != null ? registrationTemplate.GroupType.Name : string.Empty;
            lRequiredSignedDocument.Text = registrationTemplate.RequiredSignatureDocumentTemplate != null ? registrationTemplate.RequiredSignatureDocumentTemplate.Name : string.Empty;
            lRequiredSignedDocument.Visible = !string.IsNullOrWhiteSpace( lRequiredSignedDocument.Text );
            lWorkflowType.Text = registrationTemplate.RegistrationWorkflowType != null ? registrationTemplate.RegistrationWorkflowType.Name : string.Empty;
            lWorkflowType.Visible = !string.IsNullOrWhiteSpace( lWorkflowType.Text );
            rcwRegistrantFormsSummary.Label = string.Format( "<strong>Registrant Forms</strong> ({0}) <i class='fa fa-caret-down'></i>", registrationTemplate.Forms.Count() );
            lRegistrantFormsSummary.Text = string.Empty;

            if ( registrationTemplate.Forms.Any() )
            {
                StringBuilder formsSummaryTextBuilder = new StringBuilder();
                foreach ( var form in registrationTemplate.Forms.OrderBy( a => a.Order ) )
                {
                    string formTextFormat = @"<br/><strong>{0}</strong>{1}";
                    StringBuilder formFieldTextBuilder = new StringBuilder();

                    foreach ( var formField in form.Fields.OrderBy( a => a.Order ) )
                    {
                        string formFieldName = ( formField.Attribute != null ) ? formField.Attribute.Name : formField.PersonFieldType.ConvertToString();
                        string fieldTypeName = ( formField.Attribute != null ) ? FieldTypeCache.GetName( formField.Attribute.FieldTypeId ) : string.Empty;
                        formFieldTextBuilder.AppendFormat(
                            @"<div class='row'>
                                <div class='col-sm-1'></div>
                                <div class='col-sm-4'>{0}</div>
                                <div class='col-sm-3'>{1}</div>
                                <div class='col-sm-4'>{2}</div>
                            </div>",
                            formFieldName,
                            fieldTypeName,
                            formField.FieldSource.ConvertToString() );
                    }

                    formsSummaryTextBuilder.AppendFormat( formTextFormat, form.Name, formFieldTextBuilder.ToString() );
                }

                lRegistrantFormsSummary.Text = formsSummaryTextBuilder.ToString();
            }
            else
            {
                lRegistrantFormsSummary.Text = "<div>" + None.TextHtml + "</div>";
            }

            var registrationAttributeNameList = new AttributeService( new RockContext() ).GetByEntityTypeId( new Registration().TypeId, true ).AsQueryable()
                .Where( a =>
                    a.EntityTypeQualifierColumn.Equals( "RegistrationTemplateId", StringComparison.OrdinalIgnoreCase ) &&
                    a.EntityTypeQualifierValue.Equals( registrationTemplate.Id.ToString() ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToAttributeCacheList();

            rcwRegistrationAttributesSummary.Visible = registrationAttributeNameList.Any();
            rcwRegistrationAttributesSummary.Label = string.Format( "<strong>Registration Attributes</strong> ({0}) <i class='fa fa-caret-down'></i>", registrationTemplate.Forms.Count() );

            StringBuilder registrationAttributeTextBuilder = new StringBuilder();
            foreach ( var registrationAttribute in registrationAttributeNameList )
            {
                registrationAttributeTextBuilder.AppendFormat(
                        @"<div class='row'>
                                <div class='col-sm-1'></div>
                                <div class='col-sm-4'>{0}</div>
                                <div class='col-sm-7'>{1}</div>
                            </div>",
                        registrationAttribute.Name,
                        registrationAttribute.FieldType.Name );
            }

            lRegistrationAttributesSummary.Text = registrationAttributeTextBuilder.ToString();

            if ( registrationTemplate.SetCostOnInstance ?? false )
            {
                lCost.Text = "Set on Instance";
                lMinimumInitialPayment.Text = "Set on Instance";
                lDefaultPaymentAmount.Text = "Set on Instance";
            }
            else
            {
                lCost.Text = registrationTemplate.Cost.FormatAsCurrency();
                lMinimumInitialPayment.Visible = registrationTemplate.MinimumInitialPayment.HasValue;
                lMinimumInitialPayment.Text = registrationTemplate.MinimumInitialPayment.HasValue ?
                    registrationTemplate.MinimumInitialPayment.Value.FormatAsCurrency() : string.Empty;
                lDefaultPaymentAmount.Visible = registrationTemplate.DefaultPayment.HasValue;
                lDefaultPaymentAmount.Text = registrationTemplate.DefaultPayment.HasValue ?
                    registrationTemplate.DefaultPayment.Value.FormatAsCurrency() : string.Empty;
            }

            rFees.DataSource = registrationTemplate.Fees.OrderBy( f => f.Order ).ToList();
            rFees.DataBind();
        }

        /// <summary>
        /// Adds the "is-inactive" css class if the item is not active.
        /// </summary>
        /// <param name="isActive">The is active.</param>
        /// <returns></returns>
        protected string FormatInactiveRow( string isActive )
        {
            try
            {
                if ( bool.Parse( isActive ) == false )
                {
                    return "class=\"row\"";
                }
                else
                {
                    return "class=\"row is-inactive\"";
                }
            }
            catch ( Exception )
            {
                // If there is a problem with this then just show the row as active.
                return "class=\"row\"";
            }
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            fieldsetViewDetails.Visible = !editable;

            this.HideSecondaryBlocks( editable );
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns( RockContext rockContext )
        {
            gtpGroupType.GroupTypes = new GroupTypeService( rockContext )
                .Queryable().AsNoTracking()
                .Where( t => t.ShowInNavigation )
                .OrderBy( t => t.Order )
                .ThenBy( t => t.Name )
                .ToList();
            ddlGroupMemberStatus.BindToEnum<GroupMemberStatus>();
            rblRegistrantsInSameFamily.BindToEnum<RegistrantsSameFamily>();

            ddlFieldSource.BindToEnum<RegistrationFieldSource>();

            ddlPersonField.BindToEnum<RegistrationPersonFieldType>( sortAlpha: true );
            ddlPersonField.Items.Remove( ddlPersonField.Items.FindByValue( "0" ) );
            ddlPersonField.Items.Remove( ddlPersonField.Items.FindByValue( "1" ) );

            rblFeeType.BindToEnum<RegistrationFeeType>();

            ddlSignatureDocumentTemplate.Items.Clear();
            ddlSignatureDocumentTemplate.Items.Add( new ListItem() );
            foreach ( var documentType in new SignatureDocumentTemplateService( rockContext )
                .Queryable().AsNoTracking()
                .OrderBy( t => t.Name ) )
            {
                ddlSignatureDocumentTemplate.Items.Add( new ListItem( documentType.Name, documentType.Id.ToString() ) );
            }
        }

        #endregion

        #region Parse/Build Controls

        /// <summary>
        /// Parses the controls.
        /// </summary>
        /// <param name="expandInvalid">if set to <c>true</c> [expand invalid].</param>
        private void ParseControls( bool expandInvalid = false )
        {
            ExpandedForms = new List<Guid>();
            FormState = new List<RegistrationTemplateForm>();

            int order = 0;
            foreach ( var formEditor in phForms.ControlsOfTypeRecursive<RegistrationTemplateFormEditor>() )
            {
                var form = formEditor.GetForm( expandInvalid );
                form.Order = order++;

                FormState.Add( form );
                if ( formEditor.Expanded )
                {
                    ExpandedForms.Add( form.Guid );
                }
            }
        }

        /// <summary>
        /// Builds the controls.
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        /// <param name="activeFormGuid">The active form unique identifier.</param>
        private void BuildControls( bool setValues = false, Guid? activeFormGuid = null )
        {
            phForms.Controls.Clear();

            if ( FormState != null )
            {
                var orderedForms = FormState.OrderBy( f => f.Order ).ToList();
                var defaultFormGuid = orderedForms.First().Guid;
                Panel pnlDefaultForm = new Panel() { CssClass = "js-default-form" };
                Panel pnlOptionalForms = new Panel() { CssClass = "js-optional-form-list" };
                phForms.Controls.Add( pnlDefaultForm );
                phForms.Controls.Add( pnlOptionalForms );
                foreach ( var form in orderedForms )
                {
                    Panel formParent;
                    if ( form.Guid == defaultFormGuid )
                    {
                        formParent = pnlDefaultForm;
                    }
                    else
                    {
                        formParent = pnlOptionalForms;
                    }

                    BuildFormControl( formParent, setValues, form, activeFormGuid, defaultFormGuid, false );
                }
            }

            BindDiscountsGrid();
            BindFeesGrid();
        }

        /// <summary>
        /// Builds the form control.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        /// <param name="form">The form.</param>
        /// <param name="activeFormGuid">The active form unique identifier.</param>
        /// <param name="defaultFormGuid">The default form unique identifier.</param>
        /// <param name="showInvalid">if set to <c>true</c> [show invalid].</param>
        private void BuildFormControl( Control parentControl, bool setValues, RegistrationTemplateForm form, Guid? activeFormGuid, Guid defaultFormGuid, bool showInvalid )
        {
            var registrationTemplateFormEditor = new RegistrationTemplateFormEditor();
            registrationTemplateFormEditor.ID = form.Guid.ToString( "N" );
            parentControl.Controls.Add( registrationTemplateFormEditor );

            // if this is the default form, don't let it get deleted. Also, there is some special logic to disable deleting FirstName,LastName fields on default form.
            bool isDefaultForm = form.Guid == defaultFormGuid;
            registrationTemplateFormEditor.IsDeleteEnabled = !isDefaultForm;
            registrationTemplateFormEditor.IsDefaultForm = isDefaultForm;

            registrationTemplateFormEditor.ValidationGroup = btnSave.ValidationGroup;
            registrationTemplateFormEditor.DeleteFieldClick += tfeForm_DeleteFieldClick;
            registrationTemplateFormEditor.ReorderFieldClick += tfeForm_ReorderFieldClick;
            registrationTemplateFormEditor.FilterFieldClick += tfeForm_FilterFieldClick;
            registrationTemplateFormEditor.EditFieldClick += tfeForm_EditFieldClick;
            registrationTemplateFormEditor.RebindFieldClick += tfeForm_RebindFieldClick;
            registrationTemplateFormEditor.DeleteFormClick += tfeForm_DeleteFormClick;
            registrationTemplateFormEditor.AddFieldClick += tfeForm_AddFieldClick;

            registrationTemplateFormEditor.SetForm( form );

            registrationTemplateFormEditor.BindFieldsGrid( FormFieldsState[form.Guid] );

            if ( setValues )
            {
                registrationTemplateFormEditor.Expanded = ExpandedForms.Contains( form.Guid );

                if ( !registrationTemplateFormEditor.Expanded && showInvalid && !form.IsValid )
                {
                    registrationTemplateFormEditor.Expanded = true;
                }

                if ( !registrationTemplateFormEditor.Expanded )
                {
                    registrationTemplateFormEditor.Expanded = activeFormGuid.HasValue && activeFormGuid.Equals( form.Guid );
                }
            }
        }

        #endregion

        #region Registrant Forms/FieldFilter Methods

        /// <summary>
        /// Shows the form field filter.
        /// </summary>
        /// <param name="formGuid">The form unique identifier.</param>
        /// <param name="formFieldGuid">The form field unique identifier.</param>
        private void ShowFormFieldFilter( Guid formGuid, Guid formFieldGuid )
        {
            if ( FormFieldsState.ContainsKey( formGuid ) )
            {
                ShowDialog( dlgFieldFilter );

                hfFormGuidFilter.Value = formGuid.ToString();
                hfFormFieldGuidFilter.Value = formFieldGuid.ToString();
                var formField = FormFieldsState[formGuid].FirstOrDefault( a => a.Guid == formFieldGuid );
                var otherFormFields = FormFieldsState[formGuid].Where( a => a != formField && a.Attribute != null ).ToList();

                fvreFieldVisibilityRulesEditor.ValidationGroup = dlgFieldFilter.ValidationGroup;
                fvreFieldVisibilityRulesEditor.FieldName = formField.ToString();
                fvreFieldVisibilityRulesEditor.ComparableAttributes = otherFormFields.Select( a => a.Attribute ).ToDictionary( k => k.Guid, v => v );
                fvreFieldVisibilityRulesEditor.SetFieldVisibilityRules( formField.FieldVisibilityRules );
            }

            BuildControls( true );
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgFieldFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgFieldFilter_SaveClick( object sender, EventArgs e )
        {
            Guid formGuid = hfFormGuidFilter.Value.AsGuid();
            Guid formFieldGuid = hfFormFieldGuidFilter.Value.AsGuid();
            var formField = FormFieldsState[formGuid].FirstOrDefault( a => a.Guid == formFieldGuid );
            formField.FieldVisibilityRules = fvreFieldVisibilityRulesEditor.GetFieldVisibilityRules();

            HideDialog();

            BuildControls( true );
        }

        #endregion

        #region Registrant Forms Form/Field Methods

        /// <summary>
        /// Shows the registrant form field edit.
        /// </summary>
        /// <param name="formGuid">The form unique identifier.</param>
        /// <param name="formFieldGuid">The form field unique identifier.</param>
        private void ShowRegistrantFormFieldEdit( Guid formGuid, Guid formFieldGuid )
        {
            if ( FormFieldsState.ContainsKey( formGuid ) )
            {
                ShowDialog( dlgRegistrantFormField );

                var fieldList = FormFieldsState[formGuid];

                RegistrationTemplateFormField formField = fieldList.FirstOrDefault( a => a.Guid.Equals( formFieldGuid ) );
                if ( formField == null )
                {
                    lFieldSource.Visible = false;
                    ddlFieldSource.Visible = true;
                    formField = new RegistrationTemplateFormField();
                    formField.Guid = formFieldGuid;
                    formField.FieldSource = RegistrationFieldSource.PersonAttribute;
                }
                else
                {
                    lFieldSource.Text = formField.FieldSource.ConvertToString();
                    lFieldSource.Visible = true;
                    ddlFieldSource.Visible = false;
                }

                ceFormFieldPreHtml.Text = formField.PreText;
                ceFormFieldPostHtml.Text = formField.PostText;
                ddlFieldSource.SetValue( formField.FieldSource.ConvertToInt() );
                ddlPersonField.SetValue( formField.PersonFieldType.ConvertToInt() );
                lPersonField.Text = formField.PersonFieldType.ConvertToString();

                ddlPersonAttributes.Items.Clear();
                var person = new Person();
                person.LoadAttributes();
                foreach ( var attr in person.Attributes
                    .OrderBy( a => a.Value.Name )
                    .Select( a => a.Value ) )
                {
                    if ( attr.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                    {
                        var listItem = new ListItem( attr.Name, attr.Id.ToString() );
                        listItem.Attributes.Add( "title", string.Format( "{0} - {1}", attr.Id.ToString(), attr.Key ) );
                        ddlPersonAttributes.Items.Add( listItem );
                    }
                }

                ddlGroupTypeAttributes.Items.Clear();
                var group = new Group();
                group.GroupTypeId = gtpGroupType.SelectedGroupTypeId ?? 0;
                var groupMember = new GroupMember();
                groupMember.Group = group;
                groupMember.LoadAttributes();
                foreach ( var attr in groupMember.Attributes
                    .OrderBy( a => a.Value.Name )
                    .Select( a => a.Value ) )
                {
                    if ( attr.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                    {
                        ddlGroupTypeAttributes.Items.Add( new ListItem( attr.Name, attr.Id.ToString() ) );
                    }
                }

                var attribute = new Attribute();
                attribute.FieldTypeId = FieldTypeCache.Get( Rock.SystemGuid.FieldType.TEXT ).Id;

                if ( formField.FieldSource == RegistrationFieldSource.PersonAttribute )
                {
                    ddlPersonAttributes.SetValue( formField.AttributeId );
                }
                else if ( formField.FieldSource == RegistrationFieldSource.GroupMemberAttribute )
                {
                    ddlGroupTypeAttributes.SetValue( formField.AttributeId );
                }
                else if ( formField.FieldSource == RegistrationFieldSource.RegistrantAttribute )
                {
                    if ( formField.Attribute != null )
                    {
                        attribute = formField.Attribute;
                    }
                }

                edtRegistrantAttribute.SetAttributeProperties( attribute, typeof( RegistrationTemplate ) );

                cbInternalField.Checked = formField.IsInternal;
                cbShowOnWaitList.Checked = formField.FieldSource != RegistrationFieldSource.GroupMemberAttribute && formField.ShowOnWaitlist;
                cbShowOnGrid.Checked = formField.IsGridField;
                cbRequireInInitialEntry.Checked = formField.IsRequired;
                cbUsePersonCurrentValue.Checked = formField.ShowCurrentValue;
                cbCommonValue.Checked = formField.IsSharedValue;

                hfFormGuid.Value = formGuid.ToString();
                hfAttributeGuid.Value = formFieldGuid.ToString();

                lPersonField.Visible = formField.FieldSource == RegistrationFieldSource.PersonField && (
                    formField.PersonFieldType == RegistrationPersonFieldType.FirstName ||
                    formField.PersonFieldType == RegistrationPersonFieldType.LastName );

                SetFieldDisplay();
            }

            BuildControls( true );
        }

        /// <summary>
        /// Sets the field display.
        /// </summary>
        private void SetFieldDisplay()
        {
            bool protectedField = lPersonField.Visible;

            ddlFieldSource.Enabled = !protectedField;

            cbInternalField.Enabled = !protectedField;
            cbCommonValue.Enabled = !protectedField;
            cbUsePersonCurrentValue.Enabled = true;

            cbRequireInInitialEntry.Enabled = !protectedField;
            cbShowOnGrid.Enabled = !protectedField;

            var fieldSource = ddlFieldSource.SelectedValueAsEnum<RegistrationFieldSource>();

            ddlPersonField.Visible = !protectedField && fieldSource == RegistrationFieldSource.PersonField;

            ddlPersonAttributes.Visible = fieldSource == RegistrationFieldSource.PersonAttribute;

            ddlGroupTypeAttributes.Visible = fieldSource == RegistrationFieldSource.GroupMemberAttribute;

            cbInternalField.Visible = true;
            cbCommonValue.Visible = true;
            cbUsePersonCurrentValue.Visible =
                fieldSource == RegistrationFieldSource.PersonAttribute ||
                fieldSource == RegistrationFieldSource.PersonField;

            cbShowOnGrid.Visible = fieldSource != RegistrationFieldSource.RegistrantAttribute;
            cbRequireInInitialEntry.Visible = fieldSource != RegistrationFieldSource.RegistrantAttribute;

            edtRegistrantAttribute.Visible = fieldSource == RegistrationFieldSource.RegistrantAttribute;

            cbShowOnWaitList.Visible = cbWaitListEnabled.Visible && cbWaitListEnabled.Checked;
            cbShowOnWaitList.Enabled = fieldSource != RegistrationFieldSource.GroupMemberAttribute;
        }

        /// <summary>
        /// Sorts the forms.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="newIndex">The new index.</param>
        private void SortForms( Guid guid, int newIndex )
        {
            ParseControls();

            Guid? activeFormGuid = null;

            var form = FormState.FirstOrDefault( a => a.Guid.Equals( guid ) );
            if ( form != null )
            {
                activeFormGuid = form.Guid;

                FormState.Remove( form );
                if ( newIndex >= FormState.Count() )
                {
                    FormState.Add( form );
                }
                else
                {
                    FormState.Insert( newIndex, form );
                }
            }

            int order = 0;
            foreach ( var item in FormState )
            {
                item.Order = order++;
            }

            BuildControls( true );
        }

        /// <summary>
        /// Sorts the fields.
        /// </summary>
        /// <param name="fieldList">The field list.</param>
        /// <param name="oldIndex">The old index.</param>
        /// <param name="newIndex">The new index.</param>
        private void SortFields( List<RegistrationTemplateFormField> fieldList, int oldIndex, int newIndex )
        {
            var movedItem = fieldList.Where( a => a.Order == oldIndex ).FirstOrDefault();
            if ( movedItem != null )
            {
                if ( newIndex < oldIndex )
                {
                    // Moved up
                    foreach ( var otherItem in fieldList.Where( a => a.Order < oldIndex && a.Order >= newIndex ) )
                    {
                        otherItem.Order = otherItem.Order + 1;
                    }
                }
                else
                {
                    // Moved Down
                    foreach ( var otherItem in fieldList.Where( a => a.Order > oldIndex && a.Order <= newIndex ) )
                    {
                        otherItem.Order = otherItem.Order - 1;
                    }
                }

                movedItem.Order = newIndex;
            }
        }

        /// <summary>
        /// Reorder fields.
        /// </summary>
        /// <param name="fieldList">The field list.</param>
        private void ReOrderFields( List<RegistrationTemplateFormField> fieldList )
        {
            fieldList = fieldList.OrderBy( a => a.Order ).ToList();
            int order = 0;
            fieldList.ForEach( a => a.Order = order++ );
        }

        #endregion

        #region Registration Attributes

        /// <summary>
        /// Binds the registration attributes grid.
        /// </summary>
        private void BindRegistrationAttributesGrid()
        {
            gRegistrationAttributes.AddCssClass( "attribute-grid" );

            // ensure Registration Attributes have order set
            int order = 0;
            RegistrationAttributesState.OrderBy( a => a.Order ).ToList().ForEach( a => a.Order = order++ );

            gRegistrationAttributes.DataSource = RegistrationAttributesState.OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList();
            gRegistrationAttributes.DataBind();
        }

        /// <summary>
        /// Handles the DataBound event of the gRegistrationAttributesCategory control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gRegistrationAttributesCategory_DataBound( object sender, RowEventArgs e )
        {
            var attribute = AttributeCache.Get( e.Row.DataItem as Rock.Model.Attribute );
            var lCategory = sender as Literal;
            lCategory.Text = attribute.Categories.Select( a => a.Name ).ToList().AsDelimited( "," );
        }

        /// <summary>
        /// Handles the AddClick event of the gRegistrationAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gRegistrationAttributes_AddClick( object sender, EventArgs e )
        {
            gRegistrationAttributes_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the Edit event of the gRegistrationAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gRegistrationAttributes_Edit( object sender, RowEventArgs e )
        {
            gRegistrationAttributes_ShowEdit( ( Guid ) e.RowKeyValue );
        }

        /// <summary>
        /// gs the registration attributes show edit.
        /// </summary>
        /// <param name="attributeGuid">The attribute unique identifier.</param>
        protected void gRegistrationAttributes_ShowEdit( Guid attributeGuid )
        {
            Attribute attribute;
            if ( attributeGuid.Equals( Guid.Empty ) )
            {
                attribute = new Attribute();
                attribute.FieldTypeId = FieldTypeCache.Get( Rock.SystemGuid.FieldType.TEXT ).Id;
                if ( hfRegistrationTemplateId.Value.AsInteger() > 0 )
                {
                    attribute.EntityTypeQualifierColumn = "RegistrationTemplateId";
                    attribute.EntityTypeQualifierValue = hfRegistrationTemplateId.Value;
                }

                edtRegistrationAttributes.ActionTitle = ActionTitle.Add( "attribute for " + tbName.Text + " registrations" );
            }
            else
            {
                attribute = RegistrationAttributesState.First( a => a.Guid.Equals( attributeGuid ) );
                edtRegistrationAttributes.ActionTitle = ActionTitle.Edit( "attribute for " + tbName.Text + " registrations" );
            }

            var reservedKeyNames = new List<string>();
            RegistrationAttributesState.Where( a => !a.Guid.Equals( attributeGuid ) ).Select( a => a.Key ).ToList().ForEach( a => reservedKeyNames.Add( a ) );
            edtRegistrationAttributes.ReservedKeyNames = reservedKeyNames.ToList();

            edtRegistrationAttributes.SetAttributeProperties( attribute, typeof( Registration ) );

            ShowDialog( dlgRegistrationAttribute );
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgRegistrationAttribute control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgRegistrationAttribute_SaveClick( object sender, EventArgs e )
        {
            Rock.Model.Attribute attribute = new Rock.Model.Attribute();
            edtRegistrationAttributes.GetAttributeProperties( attribute );

            // Controls will show warnings
            if ( !attribute.IsValid )
            {
                return;
            }

            if ( RegistrationAttributesState.Any( a => a.Guid.Equals( attribute.Guid ) ) )
            {
                attribute.Order = RegistrationAttributesState.Where( a => a.Guid.Equals( attribute.Guid ) ).FirstOrDefault().Order;
                RegistrationAttributesState.RemoveEntity( attribute.Guid );
            }
            else
            {
                attribute.Order = RegistrationAttributesState.Any() ? RegistrationAttributesState.Max( a => a.Order ) + 1 : 0;
            }

            RegistrationAttributesState.Add( attribute );

            BindRegistrationAttributesGrid();
            HideDialog();
        }

        /// <summary>
        /// Handles the GridReorder event of the gRegistrationAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        private void gRegistrationAttributes_GridReorder( object sender, GridReorderEventArgs e )
        {
            ReorderAttributeList( RegistrationAttributesState, e.OldIndex, e.NewIndex );
            BindRegistrationAttributesGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gRegistrationAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs"/> instance containing the event data.</param>
        private void gRegistrationAttributes_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindRegistrationAttributesGrid();
        }

        /// <summary>
        /// Handles the Delete event of the gRegistrationAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gRegistrationAttributes_Delete( object sender, RowEventArgs e )
        {
            Guid attributeGuid = ( Guid ) e.RowKeyValue;
            RegistrationAttributesState.RemoveEntity( attributeGuid );

            BindRegistrationAttributesGrid();
        }

        /// <summary>
        /// Reorders the attribute list.
        /// </summary>
        /// <param name="itemList">The item list.</param>
        /// <param name="oldIndex">The old index.</param>
        /// <param name="newIndex">The new index.</param>
        private void ReorderAttributeList( List<Attribute> itemList, int oldIndex, int newIndex )
        {
            var movedItem = itemList.Where( a => a.Order == oldIndex ).FirstOrDefault();
            if ( movedItem != null )
            {
                if ( newIndex < oldIndex )
                {
                    // Moved up
                    foreach ( var otherItem in itemList.Where( a => a.Order < oldIndex && a.Order >= newIndex ) )
                    {
                        otherItem.Order = otherItem.Order + 1;
                    }
                }
                else
                {
                    // Moved Down
                    foreach ( var otherItem in itemList.Where( a => a.Order > oldIndex && a.Order <= newIndex ) )
                    {
                        otherItem.Order = otherItem.Order - 1;
                    }
                }

                movedItem.Order = newIndex;
            }
        }

        #endregion Registration Attributes

        #region Discount Methods

        /// <summary>
        /// Binds the discounts grid.
        /// </summary>
        private void BindDiscountsGrid()
        {
            if ( DiscountState != null )
            {
                gDiscounts.DataSource = DiscountState.OrderBy( d => d.Order )
                    .Select( d => new
                    {
                        d.Guid,
                        d.Id,
                        d.Code,
                        Discount = d.DiscountAmount > 0 ?
                            d.DiscountAmount.FormatAsCurrency() :
                            d.DiscountPercentage.ToString( "P2" ),
                        Limits = d.DiscountLimitsString
                    } ).ToList();
                gDiscounts.DataBind();
            }
        }

        /// <summary>
        /// Shows the discount edit.
        /// </summary>
        /// <param name="discountGuid">The discount unique identifier.</param>
        private void ShowDiscountEdit( Guid discountGuid )
        {
            var discount = DiscountState.FirstOrDefault( d => d.Guid.Equals( discountGuid ) );
            if ( discount == null )
            {
                discount = new RegistrationTemplateDiscount();
            }

            hfDiscountGuid.Value = discount.Guid.ToString();
            tbDiscountCode.Text = discount.Code;
            nbDiscountPercentage.Text = ( discount.DiscountPercentage * 100.0m ).ToString( "N0" );
            cbDiscountAmount.Text = discount.DiscountAmount.ToString();

            if ( discount.DiscountAmount > 0 )
            {
                rblDiscountType.SetValue( "Amount" );
                nbDiscountPercentage.Visible = false;
                cbDiscountAmount.Visible = true;
            }
            else
            {
                rblDiscountType.SetValue( "Percentage" );
                nbDiscountPercentage.Visible = true;
                cbDiscountAmount.Visible = false;
            }

            nbDiscountMaxUsage.Text = discount.MaxUsage.HasValue ? discount.MaxUsage.ToString() : string.Empty;
            nbDiscountMaxRegistrants.Text = discount.MaxRegistrants.HasValue ? discount.MaxRegistrants.ToString() : string.Empty;
            nbDiscountMinRegistrants.Text = discount.MinRegistrants.HasValue ? discount.MinRegistrants.ToString() : string.Empty;
            drpDiscountDateRange.LowerValue = discount.StartDate;
            drpDiscountDateRange.UpperValue = discount.EndDate;
            cbcAutoApplyDiscount.Checked = discount.AutoApplyDiscount;

            ShowDialog( dlgDiscount );
        }

        #endregion Discount Methods

        #region Fee Methods

        /// <summary>
        /// Binds the fees grid.
        /// </summary>
        private void BindFeesGrid()
        {
            if ( FeeState != null )
            {
                gFees.DataSource = FeeState.OrderBy( f => f.Order )
                    .Select( f => new
                    {
                        f.Id,
                        f.Guid,
                        f.Name,
                        f.FeeType,
                        Cost = FormatFeeItems( f.FeeItems ),
                        f.AllowMultiple,
                        f.DiscountApplies,
                        f.IsActive,
                        f.IsRequired
                    } )
                    .ToList();
                gFees.DataBind();
            }
        }

        /// <summary>
        /// Shows the fee edit.
        /// </summary>
        /// <param name="feeGuid">The fee unique identifier.</param>
        private void ShowFeeEdit( Guid feeGuid )
        {
            var fee = FeeState.FirstOrDefault( d => d.Guid.Equals( feeGuid ) );
            if ( fee == null )
            {
                fee = new RegistrationTemplateFee();
            }

            // make a copy of FeeItems to FeeItemsEditState
            FeeItemsEditState = fee.FeeItems.ToList();

            hfFeeGuid.Value = fee.Guid.ToString();
            tbFeeName.Text = fee.Name;

            rblFeeType.SetValue( fee.FeeType.ConvertToInt() );
            if ( !fee.FeeItems.Any() )
            {
                fee.FeeItems.Add( new RegistrationTemplateFeeItem() );
            }

            BindFeeItemsControls( FeeItemsEditState, fee.FeeType );

            cbAllowMultiple.Checked = fee.AllowMultiple;
            cbDiscountApplies.Checked = fee.DiscountApplies;
            cbFeeIsActive.Checked = fee.IsActive;
            cbFeeIsRequired.Checked = fee.IsRequired;

            ShowDialog( dlgFee );
        }

        /// <summary>
        /// Bind the fee items controls.
        /// </summary>
        /// <param name="feeItems">The fee items.</param>
        /// <param name="registrationFeeType">Type of the registration fee.</param>
        private void BindFeeItemsControls( List<RegistrationTemplateFeeItem> feeItems, RegistrationFeeType registrationFeeType )
        {
            rcwFeeItemsSingle.Visible = ( registrationFeeType == RegistrationFeeType.Single );
            rcwFeeItemsMultiple.Visible = ( registrationFeeType == RegistrationFeeType.Multiple );

            if ( registrationFeeType == RegistrationFeeType.Single )
            {
                var singleFeeItem = feeItems.FirstOrDefault();
                if ( singleFeeItem == null )
                {
                    singleFeeItem = new RegistrationTemplateFeeItem();
                }

                hfFeeItemSingleGuid.Value = singleFeeItem.Guid.ToString();
                cbFeeItemSingleCost.Text = singleFeeItem.Cost.ToString();
                nbFeeItemSingleMaximumUsageCount.Text = singleFeeItem.MaximumUsageCount.ToString();
            }
            else
            {
                rptFeeItemsMultiple.DataSource = feeItems.ToList();
                rptFeeItemsMultiple.DataBind();
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptFeeItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptFeeItemsMultiple_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            RegistrationTemplateFeeItem registrationTemplateFeeItem = e.Item.DataItem as RegistrationTemplateFeeItem;
            if ( registrationTemplateFeeItem != null )
            {
                var hfFeeItemGuid = e.Item.FindControl( "hfFeeItemGuid" ) as HiddenField;
                var tbFeeItemName = e.Item.FindControl( "tbFeeItemName" ) as RockTextBox;
                var cbFeeItemCost = e.Item.FindControl( "cbFeeItemCost" ) as CurrencyBox;
                var nbMaximumUsageCount = e.Item.FindControl( "nbMaximumUsageCount" ) as NumberBox;

                hfFeeItemGuid.Value = registrationTemplateFeeItem.Guid.ToString();
                tbFeeItemName.Text = registrationTemplateFeeItem.Name;

                // if the Cost is 0 (vs 0.00M), set the text to blank since they haven't entered a value
                if ( registrationTemplateFeeItem.Cost.ToString() == "0" )
                {
                    cbFeeItemCost.Text = string.Empty;
                }
                else
                {
                    cbFeeItemCost.Text = registrationTemplateFeeItem.Cost.ToString();
                }

                nbMaximumUsageCount.Text = registrationTemplateFeeItem.MaximumUsageCount.ToString();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnDeleteFeeItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDeleteFeeItem_Click( object sender, EventArgs e )
        {
            var feeItems = GetFeeItemsFromUI();

            var hfFeeItemGuid = ( sender as Control ).NamingContainer.FindControl( "hfFeeItemGuid" ) as HiddenField;
            var feeItemGuid = hfFeeItemGuid.Value.AsGuid();
            var feeItem = feeItems.FirstOrDefault( a => a.Guid == feeItemGuid );
            if ( feeItem != null )
            {
                feeItems.Remove( feeItem );
            }

            BindFeeItemsControls( feeItems, rblFeeType.SelectedValueAsEnum<RegistrationFeeType>() );
        }

        /// <summary>
        /// Handles the Click event of the btnAddFeeItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnAddFeeItem_Click( object sender, EventArgs e )
        {
            var feeItems = GetFeeItemsFromUI();
            feeItems.Add( new RegistrationTemplateFeeItem() );
            BindFeeItemsControls( feeItems, rblFeeType.SelectedValueAsEnum<RegistrationFeeType>() );
        }

        /// <summary>
        /// Formats the fee items.
        /// </summary>
        /// <param name="feeItems">The fee items.</param>
        /// <returns></returns>
        protected string FormatFeeItems( ICollection<RegistrationTemplateFeeItem> feeItems )
        {
            List<string> feeItemsHtml = new List<string>();
            foreach ( var feeItem in feeItems )
            {
                string feeItemHtml = string.Format( "{0}-{1}", feeItem.Name, feeItem.Cost.FormatAsCurrency() );
                if ( feeItem.MaximumUsageCount.HasValue )
                {
                    feeItemHtml += " ( max: " + feeItem.MaximumUsageCount.Value.ToString() + " )";
                }

                feeItemsHtml.Add( feeItemHtml );
            }

            return feeItemsHtml.AsDelimited( ", " );
        }


        #endregion Fee Methods

        #region Dialog Methods

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="dialog">The dialog.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( ModalDialog dialog, bool setValues = false )
        {
            hfActiveDialogID.Value = dialog.ID;
            ShowDialog( setValues );
        }

        /// <summary>
        /// Shows the active dialog.
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( bool setValues = false )
        {
            var activeDialog = this.ControlsOfTypeRecursive<ModalDialog>().FirstOrDefault( a => a.ID == hfActiveDialogID.Value );
            if ( activeDialog != null )
            {
                activeDialog.Show();
            }
        }

        /// <summary>
        /// Hides the active dialog.
        /// </summary>
        private void HideDialog()
        {
            var activeDialog = this.ControlsOfTypeRecursive<ModalDialog>().FirstOrDefault( a => a.ID == hfActiveDialogID.Value );
            if ( activeDialog != null )
            {
                activeDialog.Hide();
            }

            hfActiveDialogID.Value = string.Empty;
        }

        #endregion Dialog Methods


    }
}
