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
import { LearningComponentBaseDefaults } from "./learningActivity";

export type FileUploadActivityConfiguration = {
    instructions: string;
    rubric: string;
    showRubricOnUpload: boolean;
    showRubricOnScoring: boolean;
};

export type FileUploadActivityCompletion = {
    file: ListItemBag;
};


export class FileUploadActivityDefaults
    extends LearningComponentBaseDefaults<FileUploadActivityConfiguration, FileUploadActivityCompletion> {
    constructor() {
        super();
        this.defaultConfig = {
            instructions: "",
            rubric: "",
            showRubricOnScoring: false,
            showRubricOnUpload: false
        };
        this.defaultCompletion = {
            file: {} as ListItemBag
        };
    }
}