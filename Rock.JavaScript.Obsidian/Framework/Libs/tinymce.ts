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

// This imports in this file follow https://github.com/tinymce/tinymce-react/discussions/182#discussioncomment-203737

// Import TinyMce
import "tinymce";

// Models
import "tinymce/models/dom";

// Default Icons
import "tinymce/icons/default";

// Theme (changing/customizing the theme is warned against in the TinyMCE docs: https://www.tiny.cloud/docs/tinymce/6/editor-theme/)
import "tinymce/themes/silver";

// Plugins (import any plugins needed)
//import "tinymce/plugins/code";
import "tinymce/plugins/image";
import "tinymce/plugins/autolink";
import "tinymce/plugins/lists";
import "tinymce/plugins/media";
import "tinymce/plugins/searchreplace";
import "tinymce/plugins/link";
import "tinymce/plugins/table";
// The help plugin requires that the en internationalization file be imported as well.
import "tinymce/plugins/help/js/i18n/keynav/en.js";
import "tinymce/plugins/help";

// CSS (import any skins needed)
// Use TinyMCE's default skin for the toolbar and editor container (does not style the editor content).
import "tinymce/skins/ui/oxide/skin.min.css";
import tinymce, { Editor, EditorEvent, Events, Ui } from "tinymce";
// Use Rock styles the content within the editor.
//import "tinymce/skins/content/default/content.css"; // Default styles for the editable content area.
//import "tinymce/skins/ui/oxide/content.min.css"; // Styles for the editable content area.

export { tinymce, Editor, EditorEvent, Events, Ui };