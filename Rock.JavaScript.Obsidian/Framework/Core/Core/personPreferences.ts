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

import { RockDateTime } from "@Obsidian/Utility/rockDateTime";
import { useHttp } from "@Obsidian/Utility/http";
import mitt, { Emitter } from "mitt";
import { IPersonPreferenceCollection } from "@Obsidian/Types/Core/personPreferences";
import { PersonPreferenceValueBag } from "@Obsidian/ViewModels/Core/personPreferenceValueBag";
import { UpdatePersonPreferencesAccessedOptionsBag } from "@Obsidian/ViewModels/Rest/Utilities/updatePersonPreferencesAccessedOptionsBag";
import { UpdatePersonPreferencesOptionsBag } from "@Obsidian/ViewModels/Rest/Utilities/updatePersonPreferencesOptionsBag";

/**
 * The primary class to use when accessing person preferences. This handles
 * all the logic of last accessed tracking as well as properly saving and
 * updating preferences values.
 *
 * @private This is an internal implementation and should not be used directly
 * by plugins.
 */

export type PersonPreferenceEvents = {
    /** Called when the {@link IPersonPreferenceCollection} value has been saved. */
    preferenceSaved: PersonPreferenceValueBag[]
};

export class PersonPreferenceCollection implements IPersonPreferenceCollection {
    // #region Fields

    /** The entity type key that we have been scoped to. */
    readonly entityTypeKey?: string;

    /** The entity key that we have been scoped to. */
    readonly entityKey?: string;

    /**
     * The prefix that will be prepended to any keys passed to us by the
     * caller.
     */
    readonly prefix: string;

    /**
     * Will be `true` if this is a completely anonymous page load. In other
     * words, updates and saves will not be performed.
     */
    readonly anonymous: boolean;

    /**
     * The preferences and values we know about. Dictionary key is the
     * prefixed preference key.
     */
    preferences: Record<string, PersonPreferenceValueBag> = {};

    /**
     * A list of prefixed preference keys that have had their values updated.
     */
    updatedKeys: string[] = [];

    /**
     * A list of prefixed preference keys that need to have their last
     * accessed timestamp updated.
     */
    accessedKeys: string[] = [];

    /**
     * The timer that has been scheduled for updating the last accessed
     * timestamps for any keys that were viewed.
     */
    updateAccessedTimer?: number;

    /** The event emitter for all the grid events. */
    private emitter: Emitter<PersonPreferenceEvents> = mitt<PersonPreferenceEvents>();

    // #endregion

    // #region Constructors

    /**
     * Creates an empty collection that will not be saved to the database.
     */
    public constructor();

    /**
     * Creates a new collection of person preferences. This instance will handle
     * all the saving and last-accessed tracking logic.
     *
     * @param entityTypeKey The entity type key that these preferences are scoped to.
     * @param entityKey The entity key that these preferences are scoped to.
     * @param prefix The additional prefix that will be applied. This should not include the standard `block-##-` prefix.
     * @param anonymous If `true` then this will be considered an anonymous request and no saves will be performed.
     * @param preferences The preferences to initialize the collection with.
     */
    public constructor(entityTypeKey: string | undefined, entityKey: string | undefined, prefix: string, anonymous: boolean, preferences: PersonPreferenceValueBag[]);

    /**
     * Creates a new collection of person preferences. This instance will handle
     * all the saving and last-accessed tracking logic.
     *
     * @param entityTypeKey The entity type key that these preferences are scoped to.
     * @param entityKey The entity key that these preferences are scoped to.
     * @param prefix The additional prefix that will be applied. This should not include the standard `block-##-` prefix.
     * @param anonymous If `true` then this will be considered an anonymous request and no saves will be performed.
     * @param preferences The preferences to initialize the collection with.
     */
    public constructor(entityTypeKey?: string, entityKey?: string, prefix: string = "", anonymous: boolean = true, preferences: PersonPreferenceValueBag[] = []) {
        this.entityTypeKey = entityTypeKey;
        this.entityKey = entityKey;
        this.prefix = prefix ?? "";
        this.anonymous = anonymous;

        for (const bag of preferences) {
            if (bag.key && bag.value && bag.lastAccessedDateTime) {
                // Make a copy of the bag so changes from outside don't
                // pollute our data.
                this.preferences[bag.key] = {
                    ...bag
                };
            }
        }
    }

    // #endregion

    // #region IPersonPreferenceCollection

    public getValue(key: string): string {
        const now = RockDateTime.now();
        const preference = this.preferences[key];

        if (!preference) {
            return "";
        }

        const preferenceDate = RockDateTime.parseISO(preference.lastAccessedDateTime ?? "");

        if (!preferenceDate || preferenceDate.date.toMilliseconds() < now.date.toMilliseconds()) {
            preference.lastAccessedDateTime = now.toISOString();
            this.accessedKeys.push(key);

            if (!this.updateAccessedTimer) {
                this.updateAccessedTimer = window.setTimeout(() => this.updateAccessedKeys(), 2500);
            }
        }

        return preference.value ?? "";
    }

    public setValue(key: string, value: string): void {
        this.preferences[key] = {
            key,
            value,
            lastAccessedDateTime: RockDateTime.now().toISOString()
        };

        this.updatedKeys.push(key);
    }

    public getKeys(): string[] {
        return Object.keys(this.preferences).filter(k => k);
    }

    public containsKey(key: string): boolean {
        return !!this.preferences[key] && !!this.preferences[key].value;
    }

    public async save(): Promise<void> {
        const keys = this.updatedKeys;

        this.updatedKeys = [];

        if (!this.anonymous && keys.length > 0) {
            await this.saveUpdatedKeys(keys);
        }

        const preferenceToBeEmitted = Object.values(this.preferences)
            .map(p => ({
                ...p,
                key : this.getPrefixedKey(p.key ?? "")
            })) as PersonPreferenceValueBag[];
        this.emitter.emit("preferenceSaved", preferenceToBeEmitted);
    }

    public withPrefix(prefix: string): IPersonPreferenceCollection {
        const prefixedPreferences: PersonPreferenceValueBag[] = [];

        for (const key of Object.keys(this.preferences)) {
            if (!key.startsWith(prefix) || key.length == prefix.length) {
                continue;
            }

            const subkey = key.substring(prefix.length);

            prefixedPreferences.push({
                key: subkey,
                value: this.preferences[key].value,
                lastAccessedDateTime: this.preferences[key].lastAccessedDateTime
            });
        }

        const prefixedPreferenceCollection = new PersonPreferenceCollection(this.entityTypeKey,
            this.entityKey,
            `${this.prefix}${prefix}`,
            this.anonymous,
            prefixedPreferences);
        prefixedPreferenceCollection.setEmitter(this.emitter);
        return prefixedPreferenceCollection;
    }

    // #endregion

    // #region Functions

    /**
     * Gets the prefixed key. This is used during updates. It includes any
     * user defined prefix, but not the standard entity type prefixes.
     *
     * @param key The key that should be prefixed with our standard prefix.
     *
     * @returns A new key that has been prefixed.
     */
    private getPrefixedKey(key: string): string {
        return `${this.prefix}${key}`;
    }

    /**
     * Performs the actual update by calling the API on the server.
     *
     * @param updatedKeys The keys that have been updated.
     */
    private async saveUpdatedKeys(updatedKeys: string[]): Promise<void> {
        const updatedPreferences: Record<string, string> = {};

        for (const key of updatedKeys) {
            updatedPreferences[this.getPrefixedKey(key)] = this.preferences[key].value ?? "";
        }

        const args: UpdatePersonPreferencesOptionsBag = {
            preferences: updatedPreferences
        };

        const http = useHttp();
        let url: string;

        if (this.entityTypeKey && this.entityKey) {
            url = `/api/v2/Utilities/PersonPreferences/${this.entityTypeKey}/${this.entityKey}`;
        }
        else {
            url = "/api/v2/Utilities/PersonPreferences";
        }

        try {
            const response = await http.post<void>(url, undefined, args);

            if (!response.isSuccess) {
                console.error(response.errorMessage || "Unable to save person preferences.");
            }
        }
        catch (error) {
            console.error(error);
        }
    }

    /**
     * Updates the last accessed time stamps for all the keys that have been
     * accessed.
     */
    private updateAccessedKeys(): void {
        const keys = this.accessedKeys;

        this.accessedKeys = [];
        this.updateAccessedTimer = undefined;

        if (!this.anonymous && keys.length > 0) {
            this.postAccessedKeys(keys);
        }
    }


    on(event: keyof PersonPreferenceEvents, callback: (preference: PersonPreferenceValueBag[]) => void): void {
        this.emitter.on(event, callback);
    }

    off(event: keyof PersonPreferenceEvents, callback: (preference: PersonPreferenceValueBag[]) => void): void {
        this.emitter.off(event, callback);
    }

    setEmitter(emitter: Emitter<PersonPreferenceEvents>) : void  {
        this.emitter = emitter;
    }

    /**
     * Performs the actual API call to note that we accessed some keys.
     *
     * @param accessedKeys The keys that have been accessed.
     */
    private async postAccessedKeys(accessedKeys: string[]): Promise<void> {
        const prefixedKeys: string[] = [];

        for (const key of accessedKeys) {
            if (!prefixedKeys.includes(this.getPrefixedKey(key))) {
                prefixedKeys.push(this.getPrefixedKey(key));
            }
        }

        const args: UpdatePersonPreferencesAccessedOptionsBag = {
            keys: prefixedKeys
        };

        const http = useHttp();
        let url: string;

        if (this.entityTypeKey && this.entityKey) {
            url = `/api/v2/Utilities/PersonPreferencesAccessed/${this.entityTypeKey}/${this.entityKey}`;
        }
        else {
            url = "/api/v2/Utilities/PersonPreferencesAccessed";
        }

        try {
            const response = await http.post<void>(url, undefined, args);

            if (!response.isSuccess) {
                console.error(response.errorMessage || "Unable to save person preference accessed timestamps.");
            }
        }
        catch (error) {
            console.error(error);
        }
    }

    // #endregion
}
