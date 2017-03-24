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
    using our <a href='{{ externalSite }}/Registration?RegistrationId={{ Registration.Id }}&rckipid={{ Registration.PersonAlias.Person.UrlEncodedKey }}'>
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
    using our <a href='{{ externalSite }}/Registration?RegistrationId={{ Registration.Id }}&rckipid={{ Registration.PersonAlias.Person.UrlEncodedKey }}'>
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
        <a href='{{ externalSite }}/Registration?RegistrationId={{ Registration.Id }}&rckipid={{ Registration.PersonAlias.Person.UrlEncodedKey }}&StartAtBeginning=True'>
        online registration page</a> to complete the registration.</strong>
    </p>
{% endif %}


{% if Registration.BalanceDue > 0 %}
    <p>
        A balance of {{ currencySymbol }}{{ Registration.BalanceDue | Format:'#,##0.00' }} remains on this regsitration. You can complete the payment for this {{ RegistrationInstance.RegistrationTemplate.RegistrationTerm | Downcase }}
        using our <a href='{{ externalSite }}/Registration?RegistrationId={{ Registration.Id }}&rckipid={{ Registration.PersonAlias.Person.UrlEncodedKey }}'>
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
        private List<Guid> ExpandedForms { get; set; }
        private List<RegistrationTemplateDiscount> DiscountState { get; set; }
        private List<RegistrationTemplateFee> FeeState { get; set; }
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

            BuildControls( false );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // attribute field grid actions
            gFields.DataKeyNames = new string[] { "Guid" };
            gFields.Actions.ShowAdd = true;
            gFields.Actions.AddClick += gFields_AddClick; ;
            gFields.GridRebind += gFields_GridRebind;
            gFields.RowDataBound += gFields_RowDataBound;
            gFields.GridReorder += gFields_GridReorder;
            GridFieldsDeleteIndex = gFields.GetColumnIndexByFieldType( typeof( DeleteField ) );

            // assign discounts grid actions
            gDiscounts.DataKeyNames = new string[] { "Guid" };
            gDiscounts.Actions.ShowAdd = true;
            gDiscounts.Actions.AddClick += gDiscounts_AddClick; ;
            gDiscounts.GridRebind += gDiscounts_GridRebind;
            gDiscounts.GridReorder += gDiscounts_GridReorder;

            // assign fees grid actions
            gFees.DataKeyNames = new string[] { "Guid" };
            gFees.Actions.ShowAdd = true;
            gFees.Actions.AddClick += gFees_AddClick;
            gFees.GridRebind += gFees_GridRebind;
            gFees.GridReorder += gFees_GridReorder;
            
            btnSecurity.EntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.RegistrationTemplate ) ).Id;

            string deleteScript = @"
    $('a.js-delete-template').click(function( e ){
        e.preventDefault();
        Rock.dialogs.confirm('Are you sure you want to delete this registration template? All of the instances, and the registrations and registrants from each instance will also be deleted!', function (result) {
            if (result) {
                if ( $('input.js-has-registrations').val() == 'True' ) {
                    Rock.dialogs.confirm('This template has existing instances with existing registrations. Are you really sure that you want to delete the template?<br/><small>(Payments will not be deleted, but they will no longer be associated with a registration.)</small>', function (result) {
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
                                        SortForms( guid, newIndex+1 );
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
            ViewState["ExpandedForms"] = ExpandedForms;
            ViewState["DiscountState"] = JsonConvert.SerializeObject( DiscountState, Formatting.None, jsonSetting );
            ViewState["FeeState"] = JsonConvert.SerializeObject( FeeState, Formatting.None, jsonSetting );

            return base.SaveViewState();
        }

        protected override void OnPreRender( EventArgs e )
        {
            if ( pnlEditDetails.Visible )
            {
                var sameFamily = rblRegistrantsInSameFamily.SelectedValueAsEnum<RegistrantsSameFamily>();
                divCurrentFamilyMembers.Attributes["style"] = sameFamily == RegistrantsSameFamily.Yes ? "display:block" : "display:none";
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
            var RegistrationTemplate = new RegistrationTemplateService( rockContext ).Get( hfRegistrationTemplateId.Value.AsInteger() );

            if ( RegistrationTemplate != null )
            {
                // Load the state objects for the source registration template
                LoadStateDetails( RegistrationTemplate, rockContext );

                // clone the registration template
                var newRegistrationTemplate = RegistrationTemplate.Clone( false );
                newRegistrationTemplate.CreatedByPersonAlias = null;
                newRegistrationTemplate.CreatedByPersonAliasId = null;
                newRegistrationTemplate.CreatedDateTime = RockDateTime.Now;
                newRegistrationTemplate.ModifiedByPersonAlias = null;
                newRegistrationTemplate.ModifiedByPersonAliasId = null;
                newRegistrationTemplate.ModifiedDateTime = RockDateTime.Now;
                newRegistrationTemplate.Id = 0;
                newRegistrationTemplate.Guid = Guid.NewGuid();
                newRegistrationTemplate.Name = RegistrationTemplate.Name + " - Copy";

                // Create temporary state objects for the new registration template
                var newFormState = new List<RegistrationTemplateForm>();
                var newFormFieldsState = new Dictionary<Guid, List<RegistrationTemplateFormField>>();
                var newDiscountState = new List<RegistrationTemplateDiscount>();
                var newFeeState = new List<RegistrationTemplateFee>();

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

                            if ( formField.FieldSource == RegistrationFieldSource.RegistrationAttribute && formField.Attribute != null )
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
                }

                FormState = newFormState;
                FormFieldsState = newFormFieldsState;
                DiscountState = newDiscountState;
                FeeState = newFeeState;

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
            var service = new RegistrationTemplateService( rockContext );

            RegistrationTemplate RegistrationTemplate = null;

            int? RegistrationTemplateId = hfRegistrationTemplateId.Value.AsIntegerOrNull();
            if ( RegistrationTemplateId.HasValue )
            {
                RegistrationTemplate = service.Get( RegistrationTemplateId.Value );
            }

            bool newTemplate = false;
            if ( RegistrationTemplate == null )
            {
                newTemplate = true;
                RegistrationTemplate = new RegistrationTemplate();
            }

            RegistrationNotify notify = RegistrationNotify.None;
            foreach( ListItem li in cblNotify.Items )
            {
                if ( li.Selected )
                {
                    notify = notify | (RegistrationNotify)li.Value.AsInteger();
                }
            }

            RegistrationTemplate.IsActive = cbIsActive.Checked;
            RegistrationTemplate.Name = tbName.Text;
            RegistrationTemplate.CategoryId = cpCategory.SelectedValueAsInt();
            RegistrationTemplate.GroupTypeId = gtpGroupType.SelectedGroupTypeId;
            RegistrationTemplate.GroupMemberRoleId = rpGroupTypeRole.GroupRoleId;
            RegistrationTemplate.GroupMemberStatus = ddlGroupMemberStatus.SelectedValueAsEnum<GroupMemberStatus>();
            RegistrationTemplate.RequiredSignatureDocumentTemplateId = ddlSignatureDocumentTemplate.SelectedValueAsInt();
            RegistrationTemplate.SignatureDocumentAction = cbDisplayInLine.Checked ? SignatureDocumentAction.Embed : SignatureDocumentAction.Email;
            RegistrationTemplate.WaitListEnabled = cbWaitListEnabled.Checked;

            RegistrationTemplate.RegistrationWorkflowTypeId = wtpRegistrationWorkflow.SelectedValueAsInt();
            RegistrationTemplate.Notify = notify;
            RegistrationTemplate.AddPersonNote = cbAddPersonNote.Checked;
            RegistrationTemplate.LoginRequired = cbLoginRequired.Checked;
            RegistrationTemplate.AllowExternalRegistrationUpdates = cbAllowExternalUpdates.Checked;
            RegistrationTemplate.AllowGroupPlacement = cbAllowGroupPlacement.Checked;
            RegistrationTemplate.AllowMultipleRegistrants = cbMultipleRegistrants.Checked;
            RegistrationTemplate.MaxRegistrants = nbMaxRegistrants.Text.AsInteger();
            RegistrationTemplate.RegistrantsSameFamily = rblRegistrantsInSameFamily.SelectedValueAsEnum<RegistrantsSameFamily>();
            RegistrationTemplate.ShowCurrentFamilyMembers = cbShowCurrentFamilyMembers.Checked;
            RegistrationTemplate.SetCostOnInstance = !tglSetCostOnTemplate.Checked;
            RegistrationTemplate.Cost = cbCost.Text.AsDecimal();
            RegistrationTemplate.MinimumInitialPayment = cbMinimumInitialPayment.Text.AsDecimalOrNull();
            RegistrationTemplate.FinancialGatewayId = fgpFinancialGateway.SelectedValueAsInt();
            RegistrationTemplate.BatchNamePrefix = txtBatchNamePrefix.Text;

            RegistrationTemplate.ConfirmationFromName = tbConfirmationFromName.Text;
            RegistrationTemplate.ConfirmationFromEmail = tbConfirmationFromEmail.Text;
            RegistrationTemplate.ConfirmationSubject = tbConfirmationSubject.Text;
            RegistrationTemplate.ConfirmationEmailTemplate = ceConfirmationEmailTemplate.Text;

            RegistrationTemplate.ReminderFromName = tbReminderFromName.Text;
            RegistrationTemplate.ReminderFromEmail = tbReminderFromEmail.Text;
            RegistrationTemplate.ReminderSubject = tbReminderSubject.Text;
            RegistrationTemplate.ReminderEmailTemplate = ceReminderEmailTemplate.Text;

            RegistrationTemplate.PaymentReminderFromName = tbPaymentReminderFromName.Text;
            RegistrationTemplate.PaymentReminderFromEmail = tbPaymentReminderFromEmail.Text;
            RegistrationTemplate.PaymentReminderSubject = tbPaymentReminderSubject.Text;
            RegistrationTemplate.PaymentReminderEmailTemplate = cePaymentReminderEmailTemplate.Text;
            RegistrationTemplate.PaymentReminderTimeSpan = nbPaymentReminderTimeSpan.Text.AsInteger();

            RegistrationTemplate.WaitListTransitionFromName = tbWaitListTransitionFromName.Text;
            RegistrationTemplate.WaitListTransitionFromEmail = tbWaitListTransitionFromEmail.Text;
            RegistrationTemplate.WaitListTransitionSubject = tbWaitListTransitionSubject.Text;
            RegistrationTemplate.WaitListTransitionEmailTemplate = ceWaitListTransitionEmailTemplate.Text;

            RegistrationTemplate.RegistrationTerm = string.IsNullOrWhiteSpace( tbRegistrationTerm.Text ) ? "Registration" : tbRegistrationTerm.Text;
            RegistrationTemplate.RegistrantTerm = string.IsNullOrWhiteSpace( tbRegistrantTerm.Text ) ? "Registrant" : tbRegistrantTerm.Text;
            RegistrationTemplate.FeeTerm = string.IsNullOrWhiteSpace( tbFeeTerm.Text ) ? "Additional Options" : tbFeeTerm.Text;
            RegistrationTemplate.DiscountCodeTerm = string.IsNullOrWhiteSpace( tbDiscountCodeTerm.Text ) ? "Discount Code" : tbDiscountCodeTerm.Text;
            RegistrationTemplate.SuccessTitle = tbSuccessTitle.Text;
            RegistrationTemplate.SuccessText = ceSuccessText.Text;

            if ( !Page.IsValid || !RegistrationTemplate.IsValid )
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
                    foreach( var formField in FormFieldsState[ form.Guid ])
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
            foreach( var fieldList in FormFieldsState.Select( s => s.Value ) )
            {
                foreach( var formField in fieldList
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
            if ( ( ( RegistrationTemplate.SetCostOnInstance ?? false ) || RegistrationTemplate.Cost > 0 || FeeState.Any() ) && !RegistrationTemplate.FinancialGatewayId.HasValue )
            {
                validationErrors.Add( "A Financial Gateway is required when the registration has a cost or additional fees or is configured to allow instances to set a cost." );
            }

            if ( RegistrationTemplate.WaitListEnabled && RegistrationTemplate.MaxRegistrants == 0 )
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
                if ( RegistrationTemplate.Id.Equals( 0 ) )
                {
                    service.Add( RegistrationTemplate );
                }
                rockContext.SaveChanges();

                var attributeService = new AttributeService( rockContext );
                var registrationTemplateFormService = new RegistrationTemplateFormService( rockContext );
                var registrationTemplateFormFieldService = new RegistrationTemplateFormFieldService( rockContext );
                var registrationTemplateDiscountService = new RegistrationTemplateDiscountService( rockContext );
                var registrationTemplateFeeService = new RegistrationTemplateFeeService( rockContext );
                var registrationRegistrantFeeService = new RegistrationRegistrantFeeService( rockContext );

                var groupService = new GroupService( rockContext );

                // delete forms that aren't assigned in the UI anymore
                var formUiGuids = FormState.Select( f => f.Guid ).ToList();
                foreach ( var form in registrationTemplateFormService
                    .Queryable()
                    .Where( f =>
                        f.RegistrationTemplateId == RegistrationTemplate.Id &&
                        !formUiGuids.Contains( f.Guid ) ) )
                {
                    foreach( var formField in form.Fields.ToList() )
                    {
                        form.Fields.Remove( formField );
                        registrationTemplateFormFieldService.Delete( formField );
                    }
                    registrationTemplateFormService.Delete( form );
                }

                // delete fields that aren't assigned in the UI anymore
                var fieldUiGuids = FormFieldsState.SelectMany( a => a.Value).Select( f => f.Guid ).ToList();
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
                        d.RegistrationTemplateId == RegistrationTemplate.Id &&
                        !discountUiGuids.Contains( d.Guid ) ) )
                {
                    registrationTemplateDiscountService.Delete( discount );
                }

                // delete fees that aren't assigned in the UI anymore
                var feeUiGuids = FeeState.Select( u => u.Guid ).ToList();
                var deletedfees = registrationTemplateFeeService
                    .Queryable()
                    .Where( d =>
                        d.RegistrationTemplateId == RegistrationTemplate.Id &&
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

                int? entityTypeId = EntityTypeCache.Read( typeof( Rock.Model.RegistrationRegistrant ) ).Id;
                var qualifierColumn = "RegistrationTemplateId";
                var qualifierValue = RegistrationTemplate.Id.ToString();

                // Get the registration attributes still in the UI
                var attributesUI = FormFieldsState
                    .SelectMany( s =>
                        s.Value.Where( a =>
                            a.FieldSource == RegistrationFieldSource.RegistrationAttribute &&
                            a.Attribute != null ) )
                    .Select( f => f.Attribute )
                    .ToList();
                var selectedAttributeGuids = attributesUI.Select( a => a.Guid );

                // Delete the registration attributes that were removed from the UI
                var attributesDB = attributeService.Get( entityTypeId, qualifierColumn, qualifierValue );
                foreach ( var attr in attributesDB.Where( a => !selectedAttributeGuids.Contains( a.Guid ) ).ToList() )
                {
                    var canDeleteAttribute = true;
                    foreach ( var form in RegistrationTemplate.Forms )
                    {
                        // make sure other RegistrationTemplates aren't using this AttributeId (which could happen due to an old bug)
                        var formFieldsFromOtherRegistrationTemplatesUsingAttribute = registrationTemplateFormFieldService.Queryable().Where( a => a.AttributeId.Value == attr.Id && a.RegistrationTemplateForm.RegistrationTemplateId != RegistrationTemplate.Id ).Any();
                        if ( formFieldsFromOtherRegistrationTemplatesUsingAttribute )
                        {
                            canDeleteAttribute = false;
                        }
                    }

                    if ( canDeleteAttribute )
                    {
                        attributeService.Delete( attr );
                        Rock.Web.Cache.AttributeCache.Flush( attr.Id );
                    }
                }

                rockContext.SaveChanges();

                // Save all of the registration attributes still in the UI
                foreach ( var attr in attributesUI )
                {
                    Helper.SaveAttributeEdits( attr, entityTypeId, qualifierColumn, qualifierValue, rockContext );
                }

                // add/updated forms/fields
                foreach ( var formUI in FormState )
                {
                    var form = RegistrationTemplate.Forms.FirstOrDefault( f => f.Guid.Equals( formUI.Guid ) );
                    if ( form == null )
                    {
                        form = new RegistrationTemplateForm();
                        form.Guid = formUI.Guid;
                        RegistrationTemplate.Forms.Add( form );
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
                                formFieldUI.FieldSource == RegistrationFieldSource.RegistrationAttribute &&
                                formFieldUI.Attribute != null )
                            {
                                var attr = AttributeCache.Read( formFieldUI.Attribute.Guid, rockContext );
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
                        }
                    }
                }

                // add/updated discounts
                foreach ( var discountUI in DiscountState )
                {
                    var discount = RegistrationTemplate.Discounts.FirstOrDefault( a => a.Guid.Equals( discountUI.Guid ) );
                    if ( discount == null )
                    {
                        discount = new RegistrationTemplateDiscount();
                        discount.Guid = discountUI.Guid;
                        RegistrationTemplate.Discounts.Add( discount );
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
                }

                // add/updated fees
                foreach ( var feeUI in FeeState )
                {
                    var fee = RegistrationTemplate.Fees.FirstOrDefault( a => a.Guid.Equals( feeUI.Guid ) );
                    if ( fee == null )
                    {
                        fee = new RegistrationTemplateFee();
                        fee.Guid = feeUI.Guid;
                        RegistrationTemplate.Fees.Add( fee );
                    }
                    fee.Name = feeUI.Name;
                    fee.FeeType = feeUI.FeeType;
                    fee.CostValue = feeUI.CostValue;
                    fee.DiscountApplies = feeUI.DiscountApplies;
                    fee.AllowMultiple = feeUI.AllowMultiple;
                    fee.Order = feeUI.Order;
                }

                rockContext.SaveChanges();

                AttributeCache.FlushEntityAttributes();

                // If this is a new template, give the current user and the Registration Administrators role administrative 
                // rights to this template, and staff, and staff like roles edit rights
                if ( newTemplate )
                {
                    RegistrationTemplate.AllowPerson( Authorization.ADMINISTRATE, CurrentPerson, rockContext );

                    var registrationAdmins = groupService.Get( Rock.SystemGuid.Group.GROUP_EVENT_REGISTRATION_ADMINISTRATORS.AsGuid() );
                    RegistrationTemplate.AllowSecurityRole( Authorization.ADMINISTRATE, registrationAdmins, rockContext );

                    var staffLikeUsers = groupService.Get( Rock.SystemGuid.Group.GROUP_STAFF_LIKE_MEMBERS.AsGuid() );
                    RegistrationTemplate.AllowSecurityRole( Authorization.EDIT, staffLikeUsers, rockContext );

                    var staffUsers = groupService.Get( Rock.SystemGuid.Group.GROUP_STAFF_MEMBERS.AsGuid() );
                    RegistrationTemplate.AllowSecurityRole( Authorization.EDIT, staffUsers, rockContext );
                }

                var qryParams = new Dictionary<string, string>();
                qryParams["RegistrationTemplateId"] = RegistrationTemplate.Id.ToString();
                NavigateToPage( RockPage.Guid, qryParams );
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

        #region Field Grid Events

        /// <summary>
        /// Handles the AddClick event of the gFields control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gFields_AddClick( object sender, EventArgs e )
        {
            ParseControls();

            if ( FormFieldsState.Any() )
            {
                ShowFormFieldEdit( FormFieldsState.First().Key, Guid.NewGuid() );
            }

            BuildControls();
        }

        /// <summary>
        /// Handles the Edit event of the gFields control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gFields_Edit( object sender, RowEventArgs e )
        {
            ParseControls();

            if ( FormFieldsState.Any() )
            {
                ShowFormFieldEdit( FormFieldsState.First().Key, e.RowKeyValue.ToString().AsGuid() );
            }

            BuildControls();
        }

        /// <summary>
        /// Handles the GridReorder event of the gFields control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gFields_GridReorder( object sender, GridReorderEventArgs e )
        {
            ParseControls();

            if ( FormFieldsState.Any() )
            {
                var keyValue = FormFieldsState.First();
                SortFields( keyValue.Value, e.OldIndex, e.NewIndex );
                ReOrderFields( keyValue.Value );
            }

            BuildControls();
        }

        /// <summary>
        /// Handles the Delete event of the gFields control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gFields_Delete( object sender, RowEventArgs e )
        {
            ParseControls();

            if ( FormFieldsState.Any() )
            {
                FormFieldsState.First().Value.RemoveEntity( e.RowKeyValue.ToString().AsGuid() );
            }

            BuildControls();
        }

        /// <summary>
        /// Handles the GridRebind event of the gFields control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gFields_GridRebind( object sender, EventArgs e )
        {
            BindFieldsGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gFields control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gFields_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow &&
                ( e.Row.Cells[1].Text == "First Name" || e.Row.Cells[1].Text == "Last Name" ) &&
                e.Row.Cells[2].Text == "Person Field" )
            {
                if ( GridFieldsDeleteIndex.HasValue )
                {
                    foreach ( var lb in e.Row.Cells[GridFieldsDeleteIndex.Value].ControlsOfTypeRecursive<LinkButton>() )
                    {
                        lb.Visible = false;
                    }
                }
            }
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
        void tfeForm_DeleteFormClick( object sender, EventArgs e )
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
        void tfeForm_AddFieldClick( object sender, TemplateFormFieldEventArg e )
        {
            ParseControls();

            ShowFormFieldEdit( e.FormGuid, Guid.NewGuid() );

            BuildControls( true );
        }

        /// <summary>
        /// Tfes the form_ edit attribute click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        void tfeForm_EditFieldClick( object sender, TemplateFormFieldEventArg e )
        {
            ParseControls();

            ShowFormFieldEdit( e.FormGuid, e.FormFieldGuid );

            BuildControls( true );
        }

        /// <summary>
        /// Tfes the form_ reorder attribute click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        void tfeForm_ReorderFieldClick( object sender, TemplateFormFieldEventArg e )
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
        void tfeForm_DeleteFieldClick( object sender, TemplateFormFieldEventArg e )
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
        void tfeForm_RebindFieldClick( object sender, TemplateFormFieldEventArg e )
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
        /// Handles the SaveClick event of the dlgField control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgField_SaveClick( object sender, EventArgs e )
        {
            var formGuid = hfFormGuid.Value.AsGuid();
            var attributeGuid = hfAttributeGuid.Value.AsGuid();

            if ( FormFieldsState.ContainsKey( formGuid ) )
            {
                var attributeForm = FormFieldsState[formGuid].FirstOrDefault( a => a.Guid.Equals( attributeGuid ) );
                if ( attributeForm == null )
                {
                    attributeForm = new RegistrationTemplateFormField();
                    attributeForm.Order = FormFieldsState[formGuid].Any() ? FormFieldsState[formGuid].Max( a => a.Order ) + 1 : 0;
                    attributeForm.Guid = attributeGuid;
                    FormFieldsState[formGuid].Add( attributeForm );
                }

                attributeForm.PreText = ceAttributePreText.Text;
                attributeForm.PostText = ceAttributePostText.Text;
                attributeForm.FieldSource = ddlFieldSource.SelectedValueAsEnum<RegistrationFieldSource>();
                if ( ddlPersonField.Visible )
                {
                    attributeForm.PersonFieldType = ddlPersonField.SelectedValueAsEnum<RegistrationPersonFieldType>();
                }

                attributeForm.IsInternal = cbInternalField.Checked;
                attributeForm.IsSharedValue = cbCommonValue.Checked;

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
                    case RegistrationFieldSource.RegistrationAttribute:
                        {
                            Rock.Model.Attribute attribute = new Rock.Model.Attribute();
                            edtRegistrationAttribute.GetAttributeProperties( attribute );
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
                            attributeForm.AttributeId = attribute.Id;
                        }
                    }
                }
            }

            HideDialog();

            BuildControls( true );
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
            if ( fee.FeeType == RegistrationFeeType.Single )
            {
                fee.CostValue = cCost.Text;
            }
            else
            {
                fee.CostValue = kvlMultipleFees.Value;
            }
            fee.AllowMultiple = cbAllowMultiple.Checked;
            fee.DiscountApplies = cbDiscountApplies.Checked;

            HideDialog();

            hfFeeGuid.Value = string.Empty;

            BuildControls();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the rblFeeType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rblFeeType_SelectedIndexChanged( object sender, EventArgs e )
        {
            var feeType = rblFeeType.SelectedValueAsEnum<RegistrationFeeType>();
            cCost.Visible = feeType == RegistrationFeeType.Single;
            kvlMultipleFees.Visible = feeType == RegistrationFeeType.Multiple;
        }

        #endregion

        #endregion

        #region Methods

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
                    LoadStateDetails(registrationTemplate, rockContext);
                    ShowEditDetails( registrationTemplate, rockContext );
                }
            }
        }

        /// <summary>
        /// Loads the state details.
        /// </summary>
        /// <param name="RegistrationTemplate">The registration template.</param>
        /// <param name="rockContext">The rock context.</param>
        private void LoadStateDetails( RegistrationTemplate RegistrationTemplate, RockContext rockContext )
        {
            if ( RegistrationTemplate != null )
            {
                // If no forms, add at one
                if ( !RegistrationTemplate.Forms.Any() )
                {
                    var form = new RegistrationTemplateForm();
                    form.Guid = Guid.NewGuid();
                    form.Order = 0;
                    form.Name = "Default Form";
                    RegistrationTemplate.Forms.Add( form );
                }

                var defaultForm = RegistrationTemplate.Forms.First();

                // Add first name field if it doesn't exist
                if ( !defaultForm.Fields
                    .Any( f => 
                        f.FieldSource == RegistrationFieldSource.PersonField &&
                        f.PersonFieldType == RegistrationPersonFieldType.FirstName ))
                {
                    var formField = new RegistrationTemplateFormField();
                    formField.FieldSource = RegistrationFieldSource.PersonField;
                    formField.PersonFieldType = RegistrationPersonFieldType.FirstName;
                    formField.IsGridField = true;
                    formField.IsRequired = true;
                    formField.ShowOnWaitlist = true;
                    formField.PreText = @"<div class='row'>
    <div class='col-md-6'>
";
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
                    formField.PostText = @"    </div>
</div>";
                    formField.Order = defaultForm.Fields.Any() ? defaultForm.Fields.Max( f => f.Order ) + 1 : 0;
                    defaultForm.Fields.Add( formField );
                }

                FormState = new List<RegistrationTemplateForm>();
                FormFieldsState = new Dictionary<Guid, List<RegistrationTemplateFormField>>();
                foreach ( var form in RegistrationTemplate.Forms.OrderBy( f => f.Order ) )
                {
                    FormState.Add( form.Clone( false ) );
                    FormFieldsState.Add( form.Guid, form.Fields.ToList() );
                }
                DiscountState = RegistrationTemplate.Discounts.OrderBy( a => a.Order ).ToList();
                FeeState = RegistrationTemplate.Fees.OrderBy( a => a.Order ).ToList();

            }
            else
            {
                FormState = new List<RegistrationTemplateForm>();
                FormFieldsState = new Dictionary<Guid, List<RegistrationTemplateFormField>>();
                DiscountState = new List<RegistrationTemplateDiscount>();
                FeeState = new List<RegistrationTemplateFee>();
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
        /// <param name="RegistrationTemplate">The registration template.</param>
        /// <param name="rockContext">The rock context.</param>
        private void ShowEditDetails( RegistrationTemplate RegistrationTemplate, RockContext rockContext )
        {
            if ( RegistrationTemplate.Id == 0 )
            {
                lReadOnlyTitle.Text = ActionTitle.Add( RegistrationTemplate.FriendlyTypeName ).FormatAsHtmlTitle();
                hlInactive.Visible = false;
                hlType.Visible = false;
            }
            else
            {
                pwDetails.Expanded = false;
            }

            SetEditMode( true );

            LoadDropDowns( rockContext );

            cbIsActive.Checked = RegistrationTemplate.IsActive;
            tbName.Text = RegistrationTemplate.Name;
            cpCategory.SetValue( RegistrationTemplate.CategoryId );

            gtpGroupType.SelectedGroupTypeId = RegistrationTemplate.GroupTypeId;
            rpGroupTypeRole.GroupTypeId = RegistrationTemplate.GroupTypeId ?? 0;
            rpGroupTypeRole.GroupRoleId = RegistrationTemplate.GroupMemberRoleId;
            ddlGroupMemberStatus.SetValue( RegistrationTemplate.GroupMemberStatus.ConvertToInt() );
            ddlSignatureDocumentTemplate.SetValue( RegistrationTemplate.RequiredSignatureDocumentTemplateId );
            cbDisplayInLine.Checked = RegistrationTemplate.SignatureDocumentAction == SignatureDocumentAction.Embed;
            wtpRegistrationWorkflow.SetValue( RegistrationTemplate.RegistrationWorkflowTypeId );

            foreach( ListItem li in cblNotify.Items )
            {
                RegistrationNotify notify = (RegistrationNotify)li.Value.AsInteger();
                li.Selected = ( RegistrationTemplate.Notify & notify ) == notify;
            }

            cbWaitListEnabled.Checked = RegistrationTemplate.WaitListEnabled;
            cbAddPersonNote.Checked = RegistrationTemplate.AddPersonNote;
            cbLoginRequired.Checked = RegistrationTemplate.LoginRequired;
            cbAllowExternalUpdates.Checked = RegistrationTemplate.AllowExternalRegistrationUpdates;
            cbAllowGroupPlacement.Checked = RegistrationTemplate.AllowGroupPlacement;
            cbMultipleRegistrants.Checked = RegistrationTemplate.AllowMultipleRegistrants;
            nbMaxRegistrants.Visible = RegistrationTemplate.AllowMultipleRegistrants;
            nbMaxRegistrants.Text = RegistrationTemplate.MaxRegistrants.ToString();
            rblRegistrantsInSameFamily.SetValue( RegistrationTemplate.RegistrantsSameFamily.ConvertToInt() );
            cbShowCurrentFamilyMembers.Checked = RegistrationTemplate.ShowCurrentFamilyMembers;
            tglSetCostOnTemplate.Checked = !RegistrationTemplate.SetCostOnInstance.HasValue || !RegistrationTemplate.SetCostOnInstance.Value;
            cbCost.Text = RegistrationTemplate.Cost.ToString();
            cbMinimumInitialPayment.Text = RegistrationTemplate.MinimumInitialPayment.HasValue ? RegistrationTemplate.MinimumInitialPayment.Value.ToString( "N2" ) : "";
            fgpFinancialGateway.SetValue( RegistrationTemplate.FinancialGatewayId );
            txtBatchNamePrefix.Text = RegistrationTemplate.BatchNamePrefix;
            SetCostVisibility();

            tbConfirmationFromName.Text = RegistrationTemplate.ConfirmationFromName;
            tbConfirmationFromEmail.Text = RegistrationTemplate.ConfirmationFromEmail;
            tbConfirmationSubject.Text = RegistrationTemplate.ConfirmationSubject;
            ceConfirmationEmailTemplate.Text = RegistrationTemplate.ConfirmationEmailTemplate;

            tbReminderFromName.Text = RegistrationTemplate.ReminderFromName;
            tbReminderFromEmail.Text = RegistrationTemplate.ReminderFromEmail;
            tbReminderSubject.Text = RegistrationTemplate.ReminderSubject;
            ceReminderEmailTemplate.Text = RegistrationTemplate.ReminderEmailTemplate;

            tbPaymentReminderFromName.Text = RegistrationTemplate.PaymentReminderFromName;
            tbPaymentReminderFromEmail.Text = RegistrationTemplate.PaymentReminderFromEmail;
            tbPaymentReminderSubject.Text = RegistrationTemplate.PaymentReminderSubject;
            cePaymentReminderEmailTemplate.Text = RegistrationTemplate.PaymentReminderEmailTemplate;
            nbPaymentReminderTimeSpan.Text = RegistrationTemplate.PaymentReminderTimeSpan.ToString();

            tbWaitListTransitionFromName.Text = RegistrationTemplate.WaitListTransitionFromName;
            tbWaitListTransitionFromEmail.Text = RegistrationTemplate.WaitListTransitionFromEmail;
            tbWaitListTransitionSubject.Text = RegistrationTemplate.WaitListTransitionSubject;
            ceWaitListTransitionEmailTemplate.Text = RegistrationTemplate.WaitListTransitionEmailTemplate;
            
            tbRegistrationTerm.Text = RegistrationTemplate.RegistrationTerm;
            tbRegistrantTerm.Text = RegistrationTemplate.RegistrantTerm;
            tbFeeTerm.Text = RegistrationTemplate.FeeTerm;
            tbDiscountCodeTerm.Text = RegistrationTemplate.DiscountCodeTerm;

            tbSuccessTitle.Text = RegistrationTemplate.SuccessTitle;
            ceSuccessText.Text = RegistrationTemplate.SuccessText;

            BuildControls( true );
        }

        /// <summary>
        /// Sets the cost visibility.
        /// </summary>
        private void SetCostVisibility()
        {
            bool setCostOnTemplate = tglSetCostOnTemplate.Checked;
            cbCost.Visible = setCostOnTemplate;
            cbMinimumInitialPayment.Visible = setCostOnTemplate;
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="RegistrationTemplate">The registration template.</param>
        private void ShowReadonlyDetails( RegistrationTemplate RegistrationTemplate )
        {
            SetEditMode( false );

            hfRegistrationTemplateId.SetValue( RegistrationTemplate.Id );
            FormState = null;
            ExpandedForms = null;
            DiscountState = null;
            FeeState = null;

            lReadOnlyTitle.Text = RegistrationTemplate.Name.FormatAsHtmlTitle();
            hlInactive.Visible = RegistrationTemplate.IsActive == false;
            hlType.Visible = RegistrationTemplate.Category != null;
            hlType.Text = RegistrationTemplate.Category != null ?
                RegistrationTemplate.Category.Name : string.Empty;
            lGroupType.Text = RegistrationTemplate.GroupType != null ?
                RegistrationTemplate.GroupType.Name : string.Empty;
            lRequiredSignedDocument.Text = RegistrationTemplate.RequiredSignatureDocumentTemplate != null ?
                RegistrationTemplate.RequiredSignatureDocumentTemplate.Name : string.Empty;
            lRequiredSignedDocument.Visible = !string.IsNullOrWhiteSpace( lRequiredSignedDocument.Text );
            lWorkflowType.Text = RegistrationTemplate.RegistrationWorkflowType != null ?
                RegistrationTemplate.RegistrationWorkflowType.Name : string.Empty;
            lWorkflowType.Visible = !string.IsNullOrWhiteSpace( lWorkflowType.Text );

            rcwForms.Label = string.Format( "<strong>Forms</strong> ({0}) <i class='fa fa-caret-down'></i>", RegistrationTemplate.Forms.Count() );
            if ( RegistrationTemplate.Forms.Any() )
            {
                foreach ( var form in RegistrationTemplate.Forms.OrderBy( a => a.Order ) )
                {
                    string formTextFormat = @"
            <br/><strong>{0}</strong>
            {1}
";

                    string attributeText = string.Empty;

                    foreach ( var formField in form.Fields.OrderBy( a => a.Order ) )
                    {
                        string formFieldName = ( formField.Attribute != null ) ?
                            formField.Attribute.Name : formField.PersonFieldType.ConvertToString();
                        string fieldTypeName = ( formField.Attribute != null ) ?
                            FieldTypeCache.GetName( formField.Attribute.FieldTypeId ) : "";
                        attributeText += string.Format( @"
            <div class='row'>
                <div class='col-sm-1'></div>
                <div class='col-sm-4'>{0}</div>
                <div class='col-sm-3'>{1}</div>
                <div class='col-sm-4'>{2}</div>
            </div>
", formFieldName, fieldTypeName, formField.FieldSource.ConvertToString() );
                    }

                    lFormsReadonly.Text += string.Format( formTextFormat, form.Name, attributeText );
                }
            }
            else
            {
                lFormsReadonly.Text = "<div>" + None.TextHtml + "</div>";
            }

            if ( RegistrationTemplate.SetCostOnInstance ?? false )
            {
                lCost.Text = "Set on Instance";
                lMinimumInitialPayment.Text = "Set on Instance";
            }
            else
            {
                lCost.Text = RegistrationTemplate.Cost.FormatAsCurrency();
                lMinimumInitialPayment.Visible = RegistrationTemplate.MinimumInitialPayment.HasValue;
                lMinimumInitialPayment.Text = RegistrationTemplate.MinimumInitialPayment.HasValue ? RegistrationTemplate.MinimumInitialPayment.Value.FormatAsCurrency() : "";
            }

            rFees.DataSource = RegistrationTemplate.Fees.OrderBy( f => f.Order ).ToList();
            rFees.DataBind();
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

            ddlPersonField.BindToEnum<RegistrationPersonFieldType>();
            ddlPersonField.Items.RemoveAt( 0 );
            ddlPersonField.Items.RemoveAt( 0 );

            rblFeeType.BindToEnum<RegistrationFeeType>();

            ddlSignatureDocumentTemplate.Items.Clear();
            ddlSignatureDocumentTemplate.Items.Add( new ListItem() );
            foreach( var documentType in new SignatureDocumentTemplateService( rockContext )
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
            FormState = FormState.Take(1).ToList();

            int order = 1;
            foreach ( var formEditor in phForms.Controls.OfType<RegistrationTemplateFormEditor>() )
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
                foreach ( var form in FormState.OrderBy( f => f.Order ).Skip( 1 ) )
                {
                    BuildFormControl( phForms, setValues, form, activeFormGuid );
                }
            }

            BindFieldsGrid();
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
        /// <param name="showInvalid">if set to <c>true</c> [show invalid].</param>
        private void BuildFormControl( Control parentControl, bool setValues, RegistrationTemplateForm form,
            Guid? activeFormGuid = null, bool showInvalid = false )
        {
            var control = new RegistrationTemplateFormEditor();
            control.ID = form.Guid.ToString( "N" );
            parentControl.Controls.Add( control );
            control.ValidationGroup = btnSave.ValidationGroup;

            control.DeleteFieldClick += tfeForm_DeleteFieldClick;
            control.ReorderFieldClick += tfeForm_ReorderFieldClick;
            control.EditFieldClick += tfeForm_EditFieldClick;
            control.RebindFieldClick += tfeForm_RebindFieldClick;
            control.DeleteFormClick += tfeForm_DeleteFormClick;
            control.AddFieldClick += tfeForm_AddFieldClick;

            control.SetForm( form );
            control.BindFieldsGrid( FormFieldsState[form.Guid] );

            if ( setValues )
            {
                control.Expanded = ExpandedForms.Contains( form.Guid );
                if ( !control.Expanded && showInvalid && !form.IsValid)
                {
                    control.Expanded = true;
                }

                if ( !control.Expanded )
                {
                    control.Expanded = activeFormGuid.HasValue && activeFormGuid.Equals( form.Guid );
                }
            }
        }

        #endregion

        #region Form/Field Methods

        /// <summary>
        /// Binds the fields grid.
        /// </summary>
        private void BindFieldsGrid()
        {
            if ( FormFieldsState != null && FormFieldsState.Any() )
            {
                gFields.DataSource = FormFieldsState.First().Value
                    .OrderBy( a => a.Order)
                    .Select( a => new
                    {
                        a.Id,
                        a.Guid,
                        Name = ( a.FieldSource != RegistrationFieldSource.PersonField && a.Attribute != null ) ?
                            a.Attribute.Name : a.PersonFieldType.ConvertToString(),
                        FieldSource = a.FieldSource.ConvertToString(),
                        FieldType = ( a.FieldSource != RegistrationFieldSource.PersonField && a.Attribute != null ) ? 
                            a.Attribute.FieldTypeId : 0,
                        a.IsInternal,
                        a.IsSharedValue,
                        a.ShowCurrentValue,
                        a.IsRequired,
                        a.IsGridField,
                        a.ShowOnWaitlist
                    } )
                    .ToList();
                gFields.DataBind();
            }
        }

        /// <summary>
        /// Shows the form field edit.
        /// </summary>
        /// <param name="formGuid">The form unique identifier.</param>
        /// <param name="formFieldGuid">The form field unique identifier.</param>
        private void ShowFormFieldEdit( Guid formGuid, Guid formFieldGuid )
        {
            if ( FormFieldsState.ContainsKey( formGuid ) )
            {
                ShowDialog( "Attributes" );

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

                ceAttributePreText.Text = formField.PreText;
                ceAttributePostText.Text = formField.PostText;
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
                        ddlPersonAttributes.Items.Add( new ListItem( attr.Name, attr.Id.ToString() ) );
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
                attribute.FieldTypeId = FieldTypeCache.Read( Rock.SystemGuid.FieldType.TEXT ).Id;

                if ( formField.FieldSource == RegistrationFieldSource.PersonAttribute )
                {
                    ddlPersonAttributes.SetValue( formField.AttributeId );
                }
                else if ( formField.FieldSource == RegistrationFieldSource.GroupMemberAttribute )
                {
                    ddlGroupTypeAttributes.SetValue( formField.AttributeId );
                }
                else if ( formField.FieldSource == RegistrationFieldSource.RegistrationAttribute )
                {
                    if ( formField.Attribute != null )
                    {
                        attribute = formField.Attribute;
                    }
                }

                edtRegistrationAttribute.SetAttributeProperties( attribute, typeof( RegistrationTemplate ) );

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

            cbShowOnGrid.Visible = fieldSource != RegistrationFieldSource.RegistrationAttribute;
            cbRequireInInitialEntry.Visible = fieldSource != RegistrationFieldSource.RegistrationAttribute;

            edtRegistrationAttribute.Visible = fieldSource == RegistrationFieldSource.RegistrationAttribute;

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
            var discount = DiscountState.FirstOrDefault( d => d.Guid.Equals( discountGuid ));
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

            ShowDialog( "Discounts" );
        }

        #endregion

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
                        Cost = FormatFeeCost( f.CostValue ),
                        f.AllowMultiple,
                        f.DiscountApplies
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
            var fee = FeeState.FirstOrDefault( d => d.Guid.Equals( feeGuid ));
            if ( fee == null )
            {
                fee = new RegistrationTemplateFee();
            }

            hfFeeGuid.Value = fee.Guid.ToString();
            tbFeeName.Text = fee.Name;

            rblFeeType.SetValue( fee.FeeType.ConvertToInt() );

            cCost.Visible = fee.FeeType == RegistrationFeeType.Single;
            cCost.Text = fee.CostValue;

            kvlMultipleFees.Visible = fee.FeeType == RegistrationFeeType.Multiple;
            kvlMultipleFees.Value = fee.CostValue;

            cbAllowMultiple.Checked = fee.AllowMultiple;
            cbDiscountApplies.Checked = fee.DiscountApplies;

            ShowDialog( "Fees" );
        }

        /// <summary>
        /// Formats the fee cost.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        protected string FormatFeeCost( string value )
        {
            var values = new List<string>();

            string[] nameValues = value.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );
            foreach ( string nameValue in nameValues )
            {
                string[] nameAndValue = nameValue.Split( new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries );
                if ( nameAndValue.Length == 2 )
                {
                    values.Add( string.Format( "{0}-{1}", nameAndValue[0], nameAndValue[1].AsDecimal().FormatAsCurrency() ) );
                }
                else
                {
                    values.Add( string.Format( "{0}", nameValue.AsDecimal().FormatAsCurrency() ) );
                }
            }

            return values.AsDelimited( ", " );
        }

        #endregion

        #region Dialog Methods

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="dialog">The dialog.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( string dialog, bool setValues = false )
        {
            hfActiveDialog.Value = dialog.ToUpper().Trim();
            ShowDialog( setValues );
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( bool setValues = false )
        {
            switch ( hfActiveDialog.Value )
            {
                case "ATTRIBUTES":
                    dlgField.Show();
                    break;
                case "DISCOUNTS":
                    dlgDiscount.Show();
                    break;
                case "FEES":
                    dlgFee.Show();
                    break;
            }
        }

        /// <summary>
        /// Hides the dialog.
        /// </summary>
        private void HideDialog()
        {
            switch ( hfActiveDialog.Value )
            {
                case "ATTRIBUTES":
                    dlgField.Hide();
                    break;
                case "DISCOUNTS":
                    dlgDiscount.Hide();
                    break;
                case "FEES":
                    dlgFee.Hide();
                    break;
            }

            hfActiveDialog.Value = string.Empty;
        }

        #endregion

        #endregion

}
}