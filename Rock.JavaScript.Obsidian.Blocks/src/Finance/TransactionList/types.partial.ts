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

import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { NumberRangeModelValue } from "@Obsidian/Types/Controls/numberRangeBox";

export const enum NavigationUrlKey {
    DetailPage = "DetailPage",
    BatchPage = "BatchPage"
}

export const ViewMode = {
    Transactions: "Transactions",
    Accounts: "Accounts"
} as const;

export const PreferenceKey = {
    ShowImages: "show-images",
    FilterDateRangeLower: "filter-date-range-lower",
    FilterDateRangeUpper: "filter-date-range-upper",
    FilterAmountRangeFrom: "filter-amount-range-from",
    FilterAmountRangeTo: "filter-amount-range-to",
    FilterCurrencyType: "filter-currency-type",
    FilterCreditCardType: "filter-credit-card-type",
    FilterTransactionCode: "filter-transaction-code",
    FilterForeignKey: "filter-foreign-key",
    FilterAccount: "filter-account",
    FilterTransactionType: "filter-transaction-type",
    FilterSourceType: "filter-source-type",
    FilterCampusOfBatch: "filter-campus-of-batch",
    FilterCampusOfAccount: "filter-campus-of-account",
    FilterPerson: "filter-person",
} as const;

export type AccountSummary = {
    name: string;
    amount: number;
};

export type GridSettingsOptions = {
    dateRangeLower?: string | null;
    dateRangeUpper?: string | null;
    amountRange?: NumberRangeModelValue;
    currencyType?: ListItemBag | null;
    creditCardType?: ListItemBag | null;
    sourceType?: ListItemBag | null;
    transactionCode?: string | null;
    foreignKey?: string | null;
    account?: ListItemBag;
    transactionType?: ListItemBag | null;
    campusOfBatch?: ListItemBag;
    campusOfAccount?: ListItemBag;
    person?: ListItemBag;
};
