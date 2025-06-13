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
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";

export enum ContentArticleItemType {
    Text = 0,
    Section = 1,
    Video = 2,
    Note = 3
}

export type ContentArticleItem = {
    type: ContentArticleItemType;

    uniqueId: Guid;

    hasBeenGraded?: boolean | null;

    order: number;

    // pointsEarned?: number | null;

    // response?: string | null;

    // answers?: string[] | null;

    // correctAnswer?: string | null;

    // helpText?: string | null;

    // question?: string | null;

    // answerBoxRows?: number | null;

    // maxCharacterCount?: number | null;

    // pointsPossible?: number | null;

    // questionWeight?: number | null;

    text?: string | null; // Text

    title?: string | null; // section

    summary?: string | null; // section

    video?: ListItemBag | null; // video

    label?: string | null; // Note

    inputRows?: number | null; // Note

    isRequired?: boolean | null // Note

    note?: string | null // Note
};
