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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Event.RegistrationInstanceDetail
{
    /// <summary>
    /// The bag that represents a registration instance for view and edit.
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Utility.EntityBagBase" />
    public class RegistrationInstanceBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the registration instance name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the registration instance is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the HTML details shown on the detail page and in confirmation emails.
        /// </summary>
        public string Details { get; set; }

        /// <summary>
        /// Gets or sets the date and time when registration opens.
        /// </summary>
        public DateTimeOffset? StartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the date and time when registration closes.
        /// </summary>
        public DateTimeOffset? EndDateTime { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of attendees allowed for this instance. A null value means unlimited.
        /// </summary>
        public int? MaxAttendees { get; set; }

        /// <summary>
        /// Gets or sets the optional workflow type that is launched when a new registration is completed.
        /// </summary>
        public ListItemBag RegistrationWorkflowType { get; set; }

        /// <summary>
        /// Gets or sets the defined value used as the record source when creating new person records during registration.
        /// </summary>
        public ListItemBag RegistrantRecordSource { get; set; }

        /// <summary>
        /// Gets or sets the per-registrant cost when the template allows cost to be set on the instance.
        /// </summary>
        public decimal? Cost { get; set; }

        /// <summary>
        /// Gets or sets the minimum initial payment required per registrant.
        /// </summary>
        public decimal? MinimumInitialPayment { get; set; }

        /// <summary>
        /// Gets or sets the default payment amount per registrant.
        /// </summary>
        public decimal? DefaultPayment { get; set; }

        /// <summary>
        /// Gets or sets the financial account to receive registration payments.
        /// </summary>
        public ListItemBag Account { get; set; }

        /// <summary>
        /// Gets or sets the contact person alias. The picker emits a <c>PersonAlias.Guid</c>, not a <c>Person.Guid</c>.
        /// </summary>
        public ListItemBag ContactPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the contact phone number.
        /// </summary>
        public string ContactPhone { get; set; }

        /// <summary>
        /// Gets or sets the contact email address.
        /// </summary>
        public string ContactEmail { get; set; }

        /// <summary>
        /// Gets or sets the date and time at which a reminder should be sent.
        /// </summary>
        public DateTimeOffset? SendReminderDateTime { get; set; }

        /// <summary>
        /// Gets or sets the payment deadline date. Only shown when the template allows payment plans.
        /// </summary>
        public DateTimeOffset? PaymentDeadlineDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the reminder has been sent.
        /// </summary>
        public bool ReminderSent { get; set; }

        /// <summary>
        /// Gets or sets the registration instructions shown at the beginning of registration.
        /// </summary>
        public string RegistrationInstructions { get; set; }

        /// <summary>
        /// Gets or sets the additional reminder details included in the reminder notification.
        /// </summary>
        public string AdditionalReminderDetails { get; set; }

        /// <summary>
        /// Gets or sets the additional confirmation details appended to the template confirmation.
        /// </summary>
        public string AdditionalConfirmationDetails { get; set; }

        /// <summary>
        /// Gets or sets the session timeout length in minutes. Only relevant when MaxAttendees is set.
        /// </summary>
        public int? TimeoutLengthMinutes { get; set; }

        /// <summary>
        /// Gets or sets the session timeout threshold as a percentage of remaining spots. Only relevant when MaxAttendees is set.
        /// </summary>
        public int? TimeoutThreshold { get; set; }

        /// <summary>
        /// Gets or sets the external gateway merchant identifier used by redirection gateways.
        /// </summary>
        public string ExternalGatewayMerchantId { get; set; }

        /// <summary>
        /// Gets or sets the external gateway fund identifier used by redirection gateways.
        /// </summary>
        public string ExternalGatewayFundId { get; set; }

        /// <summary>
        /// Gets or sets the list of available gateway merchants for redirection gateways.
        /// </summary>
        public List<ListItemBag> GatewayMerchants { get; set; }

        /// <summary>
        /// Gets or sets the list of available gateway funds for the selected merchant.
        /// </summary>
        public List<ListItemBag> GatewayFunds { get; set; }

        /// <summary>
        /// Gets or sets the display label for the gateway merchant field, as supplied by the redirection gateway component.
        /// </summary>
        public string GatewayMerchantFieldLabel { get; set; }

        /// <summary>
        /// Gets or sets the display label for the gateway fund field, as supplied by the redirection gateway component.
        /// </summary>
        public string GatewayFundFieldLabel { get; set; }

        /// <summary>
        /// Gets or sets the registration template name. Used for the wizard header label and the template highlight label.
        /// </summary>
        public string RegistrationTemplateName { get; set; }

        /// <summary>
        /// Gets or sets the registration template identifier key. The Save action reads this on a
        /// copied instance (where the page URL carries no <c>RegistrationTemplateId</c> parameter)
        /// to preserve the source instance's template scope on the new record.
        /// </summary>
        public string RegistrationTemplateIdKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the template allows payment plans. Drives visibility of the payment deadline field.
        /// </summary>
        public bool IsPaymentPlanAllowed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the template sets cost on the instance. Drives visibility of the cost fields.
        /// </summary>
        public bool IsSetCostOnInstance { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the template has a financial gateway configured. Drives visibility of the account picker.
        /// </summary>
        public bool IsFinancialGatewayConfigured { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the template's financial gateway is a redirection gateway. Drives visibility of merchant/fund pickers.
        /// </summary>
        public bool IsRedirectionGateway { get; set; }

        /// <summary>
        /// Gets or sets the precomputed status text ("Open" or "Closed").
        /// </summary>
        public string StatusText { get; set; }

        /// <summary>
        /// Gets or sets the highlight label type for the status label ("success" when open, otherwise "type").
        /// </summary>
        public string StatusLabelType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the "Send Payment Reminders" shortcut should be shown on the view panel.
        /// </summary>
        public bool CanSendPaymentReminder { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether at least one registration has an active payment plan.
        /// Used by the delete confirmation prompt to show the additional payment-plan warning.
        /// </summary>
        public bool HasActivePaymentPlans { get; set; }

        /// <summary>
        /// Gets or sets the group placement links shown on the view panel.
        /// </summary>
        public List<RegistrationInstanceGroupPlacementBag> GroupPlacements { get; set; }
    }
}
