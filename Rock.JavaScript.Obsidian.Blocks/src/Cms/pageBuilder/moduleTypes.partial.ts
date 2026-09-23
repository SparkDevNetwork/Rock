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

/** A module type that can be dragged from the sidebar into the page. */
export type ModuleType = {
    /** Uniquely identifies the module type. */
    key: string;

    /** The name shown on the sidebar tile and on the selected module's chip. */
    name: string;

    /** The icon shown on the sidebar tile. */
    iconCssClass: string;

    /** The markup placed in the page when the module is dropped. */
    html: string;
};

/** The module types offered in the sidebar, in display order. */
export const moduleTypes: ModuleType[] = [
    {
        key: "accordion",
        name: "Accordion",
        iconCssClass: "ti ti-layout-navbar-collapse",
        html: `<h3>Accordion</h3>
<details><summary>An accordion title goes here?</summary><p>Accordion content goes here.</p></details>
<details><summary>An accordion title goes here?</summary><p>Accordion content goes here.</p></details>
<details><summary>An accordion title goes here?</summary><p>Accordion content goes here.</p></details>`
    },
    {
        key: "billboard",
        name: "Billboard",
        iconCssClass: "ti ti-presentation",
        html: `<div class="jumbotron"><h1>Billboard</h1><p>A large banner that introduces the page.</p></div>`
    },
    {
        key: "card",
        name: "Card",
        iconCssClass: "ti ti-id",
        html: `<div class="panel panel-default"><div class="panel-body"><h4>Card</h4><p>A short summary with a link to more.</p><a class="btn btn-default" href="#">View details</a></div></div>`
    },
    {
        key: "content",
        name: "Content",
        iconCssClass: "ti ti-file-text",
        html: `<h2>Content</h2><p>A block of formatted text.</p>`
    },
    {
        key: "video",
        name: "Video",
        iconCssClass: "ti ti-player-play",
        html: `<div class="well text-center"><i class="ti ti-player-play"></i> Video</div>`
    }
];
