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

/*
    7/22/2026 - CLAUDE

    Exposes the Vue single-file-component compiler as an Obsidian library bundle
    (served at @Obsidian/Libs/vueCompilerSfc). It is loaded on demand only when the
    Obsidian Content block enters edit mode, so its weight never reaches a plain
    visitor. The package resolves to its self-contained browser build via its "module"
    field, so it bundles safely without a process.env replacement step.

    Reason: Edit-only, on-demand SFC compiler for the Obsidian Content block.
*/

export * from "@vue/compiler-sfc";
