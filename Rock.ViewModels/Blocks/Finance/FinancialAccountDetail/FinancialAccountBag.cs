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

namespace Rock.ViewModels.Blocks.Finance.FinancialAccountDetail
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Utility.EntityBagBase" />
    public class FinancialAccountBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the Account Type Rock.Model.DefinedValue for this FinancialAccount.
        /// </summary>
        public ListItemBag AccountTypeValue { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.Campus that this FinancialAccount is associated with.
        /// </summary>
        public ListItemBag Campus { get; set; }

        /// <summary>
        /// Gets or sets the user defined description of the FinancialAccount.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the closing/end date for this FinancialAccount. This is the last day that transactions can be posted to this account. If there is not a end date
        /// for this account, transactions can be posted for an indefinite period of time.  Ongoing FinancialAccounts will not have an end date.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets the General Ledger account code for this FinancialAccount.
        /// </summary>
        public string GlCode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if this FinancialAccount is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if this FinancialAccount is public.
        /// </summary>
        public bool? IsPublic { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if transactions posted to this FinancialAccount are tax-deductible.
        /// </summary>
        public bool IsTaxDeductible { get; set; }

        /// <summary>
        /// Gets or sets the (internal) Name of the FinancialAccount. This property is required.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the parent FinancialAccount.
        /// </summary>
        public ListItemBag ParentAccount { get; set; }

        /// <summary>
        /// Gets or sets the user defined public description of the FinancialAccount.
        /// </summary>
        public string PublicDescription { get; set; }

        /// <summary>
        /// Gets or sets the public name of the Financial Account.
        /// </summary>
        public string PublicName { get; set; }

        /// <summary>
        /// Gets or sets the opening date for this FinancialAccount. This is the first date that transactions can be posted to this account. 
        /// If there isn't a start date for this account, transactions can be posted as soon as the account is created until the Rock.Model.FinancialAccount.EndDate (if applicable).
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets or sets the URL which could be used to generate a link to a 'More Info' page
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the account participants.
        /// </summary>
        /// <value>
        /// The account participants.
        /// </value>
        public List<FinancialAccountParticipantBag> AccountParticipants { get; set; }

        /// <summary>
        /// Gets or sets the Image that can be used when displaying this Financial Account
        /// </summary>
        /// <value>
        /// The image binary file.
        /// </value>
        public ListItemBag ImageBinaryFile { get; set; }

        /// <summary>
        /// Gets or sets the image URL.
        /// </summary>
        /// <value>
        /// The image URL.
        /// </value>
        public string ImageUrl { get; set; }
    }
}
