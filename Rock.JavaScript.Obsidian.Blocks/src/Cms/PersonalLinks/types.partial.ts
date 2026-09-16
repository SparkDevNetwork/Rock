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
    ManageLinksPage = "ManageLinksPage"
}

export const enum PanelState {
    View = "View",
    AddSection = "AddSection",
    AddLink = "AddLink"
}

export type QuickReturnItem = {
    type: string;
    typeOrder: number;
    createdDateTime: string;
    itemName: string;
    url: string;
};

export type QuickReturnGroup = {
    type: string;
    items: QuickReturnItem[];
};
