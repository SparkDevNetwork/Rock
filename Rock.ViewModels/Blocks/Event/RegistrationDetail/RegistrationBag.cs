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

namespace Rock.ViewModels.Blocks.Event.RegistrationDetail
{
    /// <summary>
    /// The item details for the Registration Detail block. Editable properties are
    /// filled for both view and edit; view-only properties are filled only for view.
    /// </summary>
    public class RegistrationBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the person alias of the registrar. The value is the
        /// person alias unique identifier.
        /// </summary>
        public ListItemBag PersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the first name of the registrar.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name of the registrar.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the email address the confirmation is sent to.
        /// </summary>
        public string ConfirmationEmail { get; set; }

        /// <summary>
        /// Gets or sets the campus of the registration.
        /// </summary>
        public ListItemBag Campus { get; set; }

        /// <summary>
        /// Gets or sets the target group that registrants are placed in.
        /// </summary>
        public ListItemBag Group { get; set; }

        /// <summary>
        /// Gets or sets the discount code.
        /// </summary>
        public string DiscountCode { get; set; }

        /// <summary>
        /// Gets or sets the discount percentage as a whole percentage (for example 10 for 10%).
        /// A null or zero value means no percentage discount.
        /// </summary>
        public decimal? DiscountPercentage { get; set; }

        /// <summary>
        /// Gets or sets the discount amount applied to each registrant.
        /// A null or zero value means no amount discount.
        /// </summary>
        public decimal? DiscountAmount { get; set; }

        /// <summary>
        /// Gets or sets the discount codes that can be selected. Only set for edit.
        /// </summary>
        public List<RegistrationDiscountOptionBag> DiscountCodeOptions { get; set; }

        /// <summary>
        /// Gets or sets the target groups that can be selected. Only set for edit.
        /// When empty the target group cannot be changed.
        /// </summary>
        public List<ListItemBag> GroupOptions { get; set; }

        /// <summary>
        /// Gets or sets the display name of the registrar. Only set for view.
        /// </summary>
        public string RegistrarName { get; set; }

        /// <summary>
        /// Gets or sets the URL of the person profile of the registrar, or null when the
        /// registration has no registrar person. Only set for view.
        /// </summary>
        public string RegistrarProfileUrl { get; set; }

        /// <summary>
        /// Gets or sets the name of the target group. Only set for view.
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// Gets or sets the URL of the target group detail page. Only set for view.
        /// </summary>
        public string GroupUrl { get; set; }

        /// <summary>
        /// Gets or sets the formatted discount percentage, or null when there is none. Only set for view.
        /// </summary>
        public string DiscountPercentageText { get; set; }

        /// <summary>
        /// Gets or sets the formatted discount amount, or null when there is none. Only set for view.
        /// </summary>
        public string DiscountAmountText { get; set; }

        /// <summary>
        /// Gets or sets the message explaining that the registration cannot be edited,
        /// or null when it can. Only set for view.
        /// </summary>
        public string ReadOnlyMessage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the cost and balance labels are shown.
        /// Only set for view.
        /// </summary>
        public bool IsCostLabelVisible { get; set; }

        /// <summary>
        /// Gets or sets the formatted total cost after discounts. Only set for view.
        /// </summary>
        public string CostText { get; set; }

        /// <summary>
        /// Gets or sets the formatted total amount paid. Only set for view.
        /// </summary>
        public string TotalPaidText { get; set; }

        /// <summary>
        /// Gets or sets the formatted balance due. Only set for view.
        /// </summary>
        public string BalanceDueText { get; set; }

        /// <summary>
        /// Gets or sets the label type used to display the balance due
        /// (success, warning or danger). Only set for view.
        /// </summary>
        public string BalanceDueLabelType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the registration has an active payment plan.
        /// Only set for view.
        /// </summary>
        public bool IsPaymentPlanActive { get; set; }

        /// <summary>
        /// Gets or sets the rows of the cost summary. Only set for view.
        /// </summary>
        public List<RegistrationFeeRowBag> FeeRows { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the discounted amount column of the
        /// cost summary is shown. Only set for view.
        /// </summary>
        public bool IsDiscountedAmountColumnVisible { get; set; }

        /// <summary>
        /// Gets or sets the payments made for this registration. Only set for view.
        /// </summary>
        public List<RegistrationPaymentRowBag> Payments { get; set; }

        /// <summary>
        /// Gets or sets the summary of the active payment plan, or null when there is
        /// no active payment plan. Only set for view.
        /// </summary>
        public RegistrationPaymentPlanSummaryBag PaymentPlan { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a manual payment can be added.
        /// Only set for view.
        /// </summary>
        public bool IsAddManualPaymentVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the process new payment button is shown.
        /// Only set for view.
        /// </summary>
        public bool IsProcessPaymentVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the process new payment button is
        /// disabled because the gateway does not support it. Only set for view.
        /// </summary>
        public bool IsProcessPaymentDisabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the message explaining that the
        /// registration must be linked to an individual before a payment can be processed
        /// is shown. Only set for view.
        /// </summary>
        public bool IsLinkToIndividualMessageVisible { get; set; }

        /// <summary>
        /// Gets or sets the registrants of this registration. Only set for view.
        /// </summary>
        public List<RegistrantPanelBag> Registrants { get; set; }

        /// <summary>
        /// Gets or sets the attribute definitions used by the registrant field values,
        /// keyed by the attribute unique identifier, or by a per value key when the view configuration depends on the value. Only set for view.
        /// </summary>
        public Dictionary<Guid, PublicAttributeBag> RegistrantAttributes { get; set; }

        /// <summary>
        /// Gets or sets the name of the registration template. Only set for view.
        /// </summary>
        public string RegistrationTemplateName { get; set; }

        /// <summary>
        /// Gets or sets the URL of the registration template page. Only set for view.
        /// </summary>
        public string RegistrationTemplateUrl { get; set; }

        /// <summary>
        /// Gets or sets the name of the registration instance. Only set for view.
        /// </summary>
        public string RegistrationInstanceName { get; set; }

        /// <summary>
        /// Gets or sets the URL of the registration instance page. Only set for view.
        /// </summary>
        public string RegistrationInstanceUrl { get; set; }

        /// <summary>
        /// Gets or sets the display name of the registration. Only set for view.
        /// </summary>
        public string RegistrationName { get; set; }
    }
}
