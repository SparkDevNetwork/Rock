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

export const enum NavigationUrlKey {
    ParentPage = "ParentPage",
    RegistrantPage = "RegistrantPage",
    GroupMemberPage = "GroupMemberPage",
    TransactionDetailPage = "TransactionDetailPage",
    AuditPage = "AuditPage"
}

/** The tabs of the account section of the registration. */
export const enum AccountTab {
    Account = "Account",
    Fees = "Fees",
    Payments = "Payments"
}

/** The kind of payment form that is open on the payments tab. */
export const enum PaymentFormMode {
    None = "",
    Manual = "Manual",
    Hosted = "Hosted"
}

/**
 * Replaces the key placeholder of a navigation URL with an identifier key.
 *
 * @param url The navigation URL that contains a key placeholder.
 * @param key The identifier key to place in the URL.
 *
 * @returns The URL, or an empty string when there is no URL.
 */
export function getKeyedUrl(url: string | null | undefined, key: string | null | undefined): string {
    if (!url || !key) {
        return "";
    }

    return url.replace("((Key))", encodeURIComponent(key));
}
