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

import { Component } from "vue";
import { defineAsyncComponent } from "@Obsidian/Utility/component";
import { FieldTypeBase } from "./fieldType";

export const enum ConfigurationValueKey {
    FileName = "fileName",
    MimeType = "mimeType",
    FilePath = "filePath",
    FileGuid = "fileGuid",
    BinaryFileType = "binaryFileType"
}

// The edit component can be quite large, so load it only as needed.
const editComponent = defineAsyncComponent(async () => {
    return (await import("./videoFileFieldComponents")).EditComponent;
});

// The configuration component can be quite large, so load it only as needed.
const configurationComponent = defineAsyncComponent(async () => {
    return (await import("./videoFileFieldComponents")).ConfigurationComponent;
});

export class VideoFileFieldType extends FieldTypeBase {
    public override getEditComponent(): Component {
        return editComponent;
    }

    public override getConfigurationComponent(): Component {
        return configurationComponent;
    }

    public override getHtmlValue(value: string, configurationValues: Record<string, string>): string {
        const filePath = configurationValues[ConfigurationValueKey.FilePath];
        const mimeType = configurationValues[ConfigurationValueKey.MimeType];
        const fileGuid = configurationValues[ConfigurationValueKey.FileGuid];

        return `<video
        src='${filePath}?guid=${fileGuid}'
        class='js-media-video'
        type='${mimeType}'
        controls='controls'
        style='width:100%;height:100%;'
        width='100%'
        height='100%'
        preload='auto'
    >
    </video>

    <script>
        Rock.controls.mediaPlayer.initialize();
    </script>`;
    }

    public override getCondensedHtmlValue(value: string, configurationValues: Record<string, string>): string {
        const fileGuid = configurationValues[ConfigurationValueKey.FileGuid];
        return `<a href="/GetFile.ashx?guid=${fileGuid}">${value}</a>`;
    }
}
