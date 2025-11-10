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

import { CommunicationRecipientActivity } from "@Obsidian/Enums/Communication/communicationRecipientActivity";

export type RecipientTimelineActivity = {
    iconCssClass: string;
    iconLabelType: "default" | "success" | "info" | "warning" | "danger" | "custom",
    iconLabelCustomClass?: string | null,
    activity: string;
    activityDateTime: string;
    description?: string | null;
    tooltip?: string | null;
};

export const RecipientActivityIconCssClass: Record<CommunicationRecipientActivity, string> = {
    [CommunicationRecipientActivity.Pending]: "ti ti-hourglass",
    [CommunicationRecipientActivity.Cancelled]: "ti ti-ban",
    [CommunicationRecipientActivity.Sent]: "ti ti-send",
    [CommunicationRecipientActivity.Delivered]: "ti ti-circle-check",
    [CommunicationRecipientActivity.DeliveryFailed]: "ti ti-x",
    [CommunicationRecipientActivity.Opened]: "ti ti-eye",
    [CommunicationRecipientActivity.Clicked]: "ti ti-hand-finger",
    [CommunicationRecipientActivity.MarkedAsSpam]: "ti ti-trash",
    [CommunicationRecipientActivity.Unsubscribed]: "ti ti-circle-minus"
};

export const RecipientActivityIconLabelType: Record<CommunicationRecipientActivity, "default" | "success" | "info" | "warning" | "danger" | "custom"> = {
    [CommunicationRecipientActivity.Pending]: "default",
    [CommunicationRecipientActivity.Cancelled]: "warning",
    [CommunicationRecipientActivity.Sent]: "success",
    [CommunicationRecipientActivity.Delivered]: "success",
    [CommunicationRecipientActivity.DeliveryFailed]: "danger",
    [CommunicationRecipientActivity.Opened]: "info",
    [CommunicationRecipientActivity.Clicked]: "success",
    [CommunicationRecipientActivity.MarkedAsSpam]: "warning",
    [CommunicationRecipientActivity.Unsubscribed]: "danger"
};
