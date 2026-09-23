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

/**
 * The styles added to the framed page for zone chrome, the empty state, and
 * the drop placeholder, written against Rock's CSS variables.
 */
export const builderStyles = `
.pagebuilder-zone {
    position: relative;
    border-radius: var(--rounded-large);
}

.pagebuilder-dragging .pagebuilder-zone,
.pagebuilder-zone.pagebuilder-zone-is-empty {
    outline: 1px solid var(--color-primary);
    outline-offset: -1px;
}

.pagebuilder-zone.pagebuilder-zone-is-empty {
    background-color: var(--color-interface-softest);
}

.pagebuilder-zone.pagebuilder-zone-over {
    background-color: var(--color-primary-soft);
}

.pagebuilder-zone-chip {
    display: none;
    position: absolute;
    top: 0;
    left: 0;
    z-index: 1;
    align-items: center;
    gap: var(--spacing-tiny);
    padding: var(--spacing-tiny) 6px;
    font-family: var(--font-family-sans);
    font-size: var(--font-size-xsmall);
    font-weight: var(--font-weight-bold);
    line-height: normal;
    color: var(--color-interface-softest);
    background-color: var(--color-primary);
    border-radius: var(--rounded-medium) 0 var(--rounded-medium) 0;
    pointer-events: none;
}

.pagebuilder-dragging .pagebuilder-zone-chip,
.pagebuilder-zone-is-empty > .pagebuilder-zone-chip {
    display: flex;
}

.pagebuilder-zone-chip-icon {
    font-size: var(--font-size-regular);
}

.pagebuilder-zone-empty {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: var(--spacing-xlarge) var(--spacing-medium);
    font-family: var(--font-family-sans);
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
    color: var(--color-interface-medium);
    background-color: var(--color-interface-softer);
    border-radius: var(--rounded-full);
}

.pagebuilder-zone-empty-title {
    margin-top: var(--spacing-medium);
    font-size: var(--font-size-h5);
    font-weight: var(--font-weight-medium);
    line-height: var(--line-height-normal);
    color: var(--color-interface-stronger);
}

.pagebuilder-zone-empty-text {
    margin-top: var(--spacing-xsmall);
    font-size: var(--font-size-regular);
    line-height: var(--line-height-normal);
    color: var(--color-interface-medium);
}

.pagebuilder-placeholder {
    display: flex;
    align-items: center;
    margin: var(--spacing-tiny) 0;
    pointer-events: none;
}

.pagebuilder-placeholder-line {
    flex-grow: 1;
    height: 3px;
    background-color: var(--color-primary);
}

.pagebuilder-placeholder-pill {
    padding: var(--spacing-tiny) var(--spacing-medium);
    font-family: var(--font-family-sans);
    font-size: var(--font-size-xsmall);
    line-height: 1;
    color: var(--color-interface-softest);
    background-color: var(--color-primary);
    border-radius: var(--rounded-full);
}

.canvas-module {
    display: flow-root;
    position: relative;
    cursor: pointer;
}

.pagebuilder-selected {
    position: relative;
    outline: 2px solid var(--color-info-tint);
    outline-offset: -2px;
    border-radius: var(--rounded-medium);
}

.pagebuilder-selection-chip {
    position: absolute;
    top: 0;
    left: 0;
    z-index: 2;
    display: flex;
    align-items: center;
    gap: var(--spacing-tiny);
    padding: var(--spacing-tiny) 6px;
    font-family: var(--font-family-sans);
    font-size: var(--font-size-xsmall);
    font-weight: var(--font-weight-bold);
    line-height: normal;
    color: var(--color-interface-softest);
    background-color: var(--color-info-strong);
    border-radius: var(--rounded-medium) 0 var(--rounded-medium) 0;
    pointer-events: none;
}

.pagebuilder-selection-chip-icon {
    font-size: var(--font-size-h5);
}

.pagebuilder-selection-controls {
    position: absolute;
    top: var(--spacing-small);
    right: var(--spacing-small);
    z-index: 2;
    display: flex;
    flex-direction: column;
    gap: var(--spacing-large);
}

.pagebuilder-selection-actions {
    display: flex;
    flex-direction: column;
    gap: var(--spacing-xsmall);
}

.pagebuilder-selection-control {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 28px;
    height: 28px;
    padding: 0;
    font-size: var(--font-size-small);
    line-height: 1;
    color: var(--color-interface-strong);
    background-color: var(--color-interface-softer);
    border: 1px solid var(--color-interface-soft);
    border-radius: var(--rounded-small);
    cursor: pointer;
}

.pagebuilder-selection-drag {
    height: 40px;
    background-color: var(--color-interface-softest);
    cursor: grab;
}

.pagebuilder-selection-delete {
    color: var(--color-danger-strong);
    background-color: var(--color-danger-soft);
    border-color: transparent;
}
`;

/** The names of the Rock CSS variables the builder styles depend on. */
export const builderStyleVariableNames: string[] = Array.from(new Set(
    (builderStyles.match(/var\(--[a-z0-9-]+\)/g) ?? []).map(reference => reference.slice("var(".length, -1))
));
