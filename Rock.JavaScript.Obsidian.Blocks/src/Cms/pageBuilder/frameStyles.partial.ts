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
    09/23/26 - JMH

    The framed page can belong to any site and any theme, so it cannot be relied on to define Rock's
    admin CSS variables. The builder copies the values it needs from its own document into
    builder-specific variables on the framed page, which keeps the builder chrome looking like
    Rock rather than like the site being edited.

    Reason: Keeps builder chrome consistent regardless of the framed page's theme.
*/

/** Maps each builder variable set on the framed page to the Rock variable it copies from the builder's document. */
export const builderThemeVariables: Record<string, string> = {
    "--pagebuilder-accent": "--color-info-strong",
    "--pagebuilder-zone": "--color-interface-medium",
    "--pagebuilder-muted-text": "--color-interface-strong",
    "--pagebuilder-surface": "--color-interface-softest",
    "--pagebuilder-surface-soft": "--color-interface-softer",
    "--pagebuilder-font-family": "--font-family-sans"
};

/** The styles added to the framed page for zone chrome, the empty state, and the drop placeholder. */
export const builderStyles = `
.pagebuilder-zone {
    position: relative;
    min-height: 64px;
    outline: 1px dashed var(--pagebuilder-zone);
    outline-offset: -1px;
}

.pagebuilder-zone > .zone-content {
    padding-top: 20px;
}

.pagebuilder-zone.pagebuilder-zone-over {
    outline: 2px solid var(--pagebuilder-accent);
    outline-offset: -2px;
}

.pagebuilder-zone-chip {
    position: absolute;
    top: 0;
    left: 0;
    z-index: 1;
    padding: 2px 8px;
    font-family: var(--pagebuilder-font-family);
    font-size: 11px;
    line-height: 1.4;
    color: var(--pagebuilder-surface);
    background-color: var(--pagebuilder-zone);
    pointer-events: none;
}

.pagebuilder-zone-empty {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    gap: 8px;
    padding: 48px 16px;
    font-family: var(--pagebuilder-font-family);
    text-align: center;
    pointer-events: none;
}

.pagebuilder-zone-empty-icon {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 64px;
    height: 64px;
    font-size: 32px;
    line-height: 1;
    color: var(--pagebuilder-muted-text);
    background-color: var(--pagebuilder-surface-soft);
    border-radius: 50%;
}

.pagebuilder-zone-empty-title {
    font-size: 16px;
}

.pagebuilder-zone-empty-text {
    font-size: 14px;
    color: var(--pagebuilder-muted-text);
}

.pagebuilder-placeholder {
    display: flex;
    align-items: center;
    margin: 2px 0;
    pointer-events: none;
}

.pagebuilder-placeholder-line {
    flex-grow: 1;
    height: 3px;
    background-color: var(--pagebuilder-accent);
}

.pagebuilder-placeholder-pill {
    padding: 4px 18px;
    font-family: var(--pagebuilder-font-family);
    font-size: 10px;
    line-height: 1;
    color: var(--pagebuilder-surface);
    background-color: var(--pagebuilder-accent);
    border-radius: 9999px;
}

.pagebuilder-canvas {
    display: flow-root;
    position: relative;
}
`;
