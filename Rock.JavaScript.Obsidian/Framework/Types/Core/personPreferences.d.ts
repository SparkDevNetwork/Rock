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


type IPersonPreferenceValueBag = {
    /** Gets or sets the key of the preference value. */
    key?: string | null;

    /** Gets or sets the last accessed date time. */
    lastAccessedDateTime?: string | null;

    /** Gets or sets the preference value. */
    value?: string | null;
};

type PersonPreferenceChangedEvents =
    "preferenceSaved";

/**
 * Provides access to person preferences from inside a block.
 */
export interface IBlockPersonPreferencesProvider {
    /** The preference collection that is scoped to the current block. */
    blockPreferences: IPersonPreferenceCollection;

    /**
     * Gets a promise that resolves to the preference collection that is
     * not scoped. These are the global per-person preferences.
     *
     * @returns A promise that resolves to the {@link IPersonPreferenceCollection} that handles preference access.
     */
    getGlobalPreferences(): Promise<IPersonPreferenceCollection>;

    /**
     * Gets the preference collection that is scoped to the specified entity
     * identified by its {@link entityTypeKey} and {@link entityKey}.
     *
     * @param entityTypeKey The entity type to use when scoping preference values.
     * @param entityKey The entity to use when scoping preference values.
     *
     * @returns A promise that resolves to the {@link IPersonPreferenceCollection} that handles preference access.
     */
    getEntityPreferences(entityTypeKey: string, entityKey: string): Promise<IPersonPreferenceCollection>;
}

/**
 * The interface that handles all the logic of accessing person preferences
 * and keeping them in sync with the server.
 */
export interface IPersonPreferenceCollection {
    /**
     * Gets the preference value for the key.
     *
     * @param key The key whose value should be returned.
     *
     * @returns A string that represents the value. An empty string is
     * returned if the key was not found.
     */
    getValue(key: string): string;

    /**
     * Sets the preference value for the key.
     *
     * @param key The key whose value should be set.
     * @param value The new value. An empty string will delete the value.
     */
    setValue(key: string, value: string): void;

    /**
     * Gets all the keys currently in this collection.
     *
     * @returns An array of keys that exist in the collection.
     */
    getKeys(): string[];

    /**
     * Determines whether the specified key exists in the collection.
     *
     * @param key The key whose existence is to be checked.
     *
     * @returns True if the key exists and has a value, otherwise false.
     */
    containsKey(key: string): boolean;

    /**
     * Saves all the changes that have been made to the preference values.
     *
     * @returns A promise that indicates when the values have been saved.
     */
    save(): Promise<void>;

    /**
     * Gets a new {@link IPersonPreferenceCollection} with only the preferences
     * that start with the given prefix. The new collection can then be accessed
     * without including the prefix.
     *
     * @param prefix The prefix to filter preferences down by.
     *
     * @returns A new instance of {@link IPersonPreferenceCollection}.
     */
    withPrefix(prefix: string): IPersonPreferenceCollection;

    on(event: PersonPreferenceChangedEvents, callback: (preference: IPersonPreferenceValueBag[]) => void): void;

    off(event: PersonPreferenceChangedEvents, callback: (preference: IPersonPreferenceValueBag[]) => void): void;
}
