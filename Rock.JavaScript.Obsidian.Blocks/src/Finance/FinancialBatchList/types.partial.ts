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

import { BatchStatus } from "@Obsidian/Enums/Finance/batchStatus";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";

export const enum NavigationUrlKey {
    DetailPage = "DetailPage"
}

export const enum PreferenceKey {
    FilterDaysBack = "filter-days-back",

    FilterContainsSourceType = "filter-contains-source",

    FilterContainsTransactionType = "filter-contains-transaction-type",

    FilterAccounts = "filter-accounts"
}

export type Row = {
    idKey: string;

    id: number;

    name: string;

    note?: string | null;

    accounts: AccountAmount[];

    accountSystemCode?: string | null;

    controlAmount: number;

    controlItemCount?: number | null;

    campus?: string | null;

    status: BatchStatus;

    startDateTime?: string | null;

    remoteSettlementKey?: string | null;

    remoteSettlementAmount?: number | null;

    remoteSettlementAmountStatus: number;

    remoteSettlementUrl?: string | null;

    totalAmount: number;

    transactionCount: number;

    variance?: Variance;
};

export type AccountAmount = {
    idKey: string;

    name: string;

    amount: number;
};

export type AccountSummary = AccountAmount & {
    isOtherAccount: boolean;
};

export type Variance = {
    amount: number;

    count: number;
};

export type GridSettingsOptions = {
    daysBack: number;

    containsTransactionType?: string | null;

    containsSourceType?: string | null;

    accounts: ListItemBag[];
};
