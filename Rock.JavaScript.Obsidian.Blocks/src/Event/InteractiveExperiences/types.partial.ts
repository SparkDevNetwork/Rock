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

import { JoinExperienceResponseBag } from "@Obsidian/ViewModels/Event/InteractiveExperiences/joinExperienceResponseBag";

export interface IParticipantTopic {
    joinExperience(experienceToken: string): Promise<JoinExperienceResponseBag>;
    leaveExperience(occurrenceIdKey: string): Promise<void>;

    showAction(occurrenceIdKey: string, actionId: string, sendNotifications: boolean): Promise<void>;
    clearActions(occurrenceIdKey: string): Promise<void>;

    showVisualizer(occurrenceIdKey: string, actionId: string): Promise<void>;
    clearVisualizer(occurrenceIdKey: string): Promise<void>;

    pingExperience(occurrenceIdKey: string): Promise<void>;

    postResponse(occurrenceIdKey: string, actionId: string, answer: string): Promise<number>;

    getParticipantCount(occurrenceIdKey: string): Promise<number>;
}
