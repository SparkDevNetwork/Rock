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
import { escapeHtml } from "@Obsidian/Utility/stringUtils";

export const enum ConfigurationKey {
    FileName = "fileName",
    MimeType = "mimeType",
    FilePath = "filePath",
    FileGuid = "fileGuid",
    BinaryFileType = "binaryFileType"
}

/** Matches a plain video MIME type, such as video/mp4, with no parameters. */
const videoMimeTypePattern = /^video\/[a-z0-9][a-z0-9.+-]{0,126}$/i;

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

    public override getHtmlValue(value: string, configurationValues: Record<string, string>, isEscaped: boolean = false): string {
        const filePath = configurationValues[ConfigurationKey.FilePath];
        const mimeType = configurationValues[ConfigurationKey.MimeType];
        const fileGuid = configurationValues[ConfigurationKey.FileGuid];

        /*
            10/16/2026 - MSE

            The MIME type comes from the Content-Type the client sent with the
            upload, and this HTML is displayed with v-html. Only a plain video
            type is placed in the type attribute, and every value is encoded.

            Reason: The stored MimeType was rendered as raw HTML.
        */
        const safeMimeType = mimeType && videoMimeTypePattern.test(mimeType) ? mimeType : "";

        const html = `<video
        src='${escapeHtml(filePath ?? "")}?guid=${escapeHtml(fileGuid ?? "")}'
        class='js-media-video'
        type='${escapeHtml(safeMimeType)}'
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

        if (isEscaped) {
            return escapeHtml(html);
        }

        return html;
    }

    public override getCondensedHtmlValue(value: string, configurationValues: Record<string, string>, isEscaped: boolean = false): string {
        const fileGuid = configurationValues[ConfigurationKey.FileGuid];

        /*
            10/16/2026 - MSE

            The file name comes from the upload and this HTML is displayed
            with v-html, so every value is encoded.

            Reason: The file name was rendered as raw HTML.
        */
        const html = `<a href="/GetFile.ashx?guid=${escapeHtml(fileGuid ?? "")}">${escapeHtml(value ?? "")}</a>`;

        if (isEscaped) {
            return escapeHtml(html);
        }

        return html;
    }
}
