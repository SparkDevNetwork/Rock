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

import { Guid } from "@Obsidian/Types";
import { IEntity } from "@Obsidian/ViewModels/entity";
import { DebugTiming } from "@Obsidian/ViewModels/Utility/debugTiming";
import { PersonBag } from "@Obsidian/ViewModels/Entities/personBag";

export type State = {
    areSecondaryBlocksShown: boolean;
    currentPerson: PersonBag | null;
    isAnonymousVisitor: boolean;
    pageParameters: Record<string, unknown>;
    contextEntities: Record<string, IEntity>;
    pageId: number;
    pageGuid: Guid;
    executionStartTime: number;
    debugTimings: DebugTiming[],
    loginUrlWithReturnUrl: string
};
