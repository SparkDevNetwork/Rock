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
import { defineComponent, PropType } from "vue";
// LPC CODE
import { useStore } from "@Obsidian/PageState";

const store = useStore();

var yPos = 0;

/** Gets the lang parameter from the query string.
 * Returns "en" or "es". Defaults to "en" if invalid. */
function getLang(): string {
    var lang = typeof store.state.pageParameters["lang"] === 'string' ? store.state.pageParameters["lang"] : "";

    if (lang != "es") {
        lang = "en";
    }

    return lang;
}
// END LPC CODE

export type ItemWithPreAndPostHtml = {
    slotName: string;
    preHtml: string;
    postHtml: string;
    // LPC CODE
    isRequired: boolean;
    // END LPC CODE
};

export default defineComponent({
    name: "ItemsWithPreAndPostHtml",
    props: {
        items: {
            type: Array as PropType<ItemWithPreAndPostHtml[]>,
            required: true
        }
    },
    methods: {
        onDismiss: function () {
            this.$emit("dismiss");
        },
        // LPC CODE
        translateAddress: function () {
            // Translate placeholders on address fields
            var addressFields = document.querySelectorAll("[id^='rock-addresscontrol'] input[placeholder]");
            for (let i = 0; i < addressFields.length; i++) {
                if (addressFields[i].getAttribute("placeholder") == "Address Line 1") {
                    addressFields[i].setAttribute("placeholder", "Dirección Línea 1");
                }
                else if (addressFields[i].getAttribute("placeholder") == "Address Line 2") {
                    addressFields[i].setAttribute("placeholder", "Dirección Línea 2");
                }
                else if (addressFields[i].getAttribute("placeholder") == "City") {
                    addressFields[i].setAttribute("placeholder", "Ciudad");
                }
                else if (addressFields[i].getAttribute("placeholder") == "Zip") {
                    addressFields[i].setAttribute("placeholder", "Código Postal");
                }
            }
        },
        delayedTranslateAddress: async function () {
            // Call translateAddress() after delays to ensure that all elements that need to be translated are translated
            // soon after they are loaded
            this.translateAddress();
            await new Promise(f => setTimeout(f, 50));
            this.translateAddress();
            await new Promise(f => setTimeout(f, 100));
            this.translateAddress();
        },
        waitForElm: async function (selector: string) {
            return new Promise(resolve => {
                if (document.querySelector(selector)) {
                    return resolve(document.querySelector(selector));
                }

                const observer = new MutationObserver(mutations => {
                    if (document.querySelector(selector)) {
                        resolve(document.querySelector(selector));
                        observer.disconnect();
                    }
                });

                observer.observe(document.body, {
                    childList: true,
                    subtree: true
                });
            });
        }
    },
    mounted() {
        // Execute translateAddress() as soon as an element exists that can be translated
        if (getLang() == 'es') {
            this.waitForElm("[id^='rock - addresscontrol'] input[placeholder]").then((elm) => {
                this.delayedTranslateAddress();
            })
        }
    },
    beforeUpdate() {
        yPos = window.pageYOffset;
    },
    updated() {
        document.documentElement.scrollTop = yPos;
        if (getLang() == 'es') {
            this.$nextTick(() => {
                this.translateAddress();
            });
        }
    },
    // END LPC CODE
    computed: {
        augmentedItems(): Record<string, string>[] {
            return this.items.map(i => ({
                // MODIFIED LPC CODE (Do not revert! Reverting the change will break this block)
                slotName: `${i.slotName}`,
                // END MODIFIED LPC CODE
                innerSlotName: `inner-${i.slotName}`
            } as Record<string, string>));
        },
        innerTemplate(): string {
            if (!this.items.length) {
                return "<slot />";
            }

            // MODIFIED LPC CODE (modified from stock - check GitHub for history)
            const templateParts = this.items.map(i => {
                var preHtml = i.preHtml;
                var postHtml = i.postHtml;

                // Create a temporary empty element and put the preHtml inside it
                var el = document.createElement('div');
                el.innerHTML = preHtml;

                // Get the first element with the class 'es' from within the temporary element
                var es = el.getElementsByClassName("SpanishLabel")[0];

                // If there was an element in the preHtml with the class 'es'
                if (getLang() == "es" && es && es.textContent != "" && es.textContent != null) {
                    // If the field is required add the 'required-indicator'
                    // class to the label to generate a required bubble
                    if (i.isRequired) {
                        preHtml += "<label class='control-label required-indicator'>";
                    }
                    else {
                        preHtml += "<label class='control-label'>";
                    }
                    // Put the contents of the 'es' element into the label, close the label,
                    // and wrap the field in a div with the class 'hide-label' to hide the original label
                    preHtml += es.textContent + "</label><div class='hide-label'>";
                    postHtml += "</div>";
                }

                return `${preHtml}<slot name="inner-${i.slotName}" />${postHtml}`;
            });
            // END MODIFIED LPC CODE
            return templateParts.join("");
        },
        innerComponent(): Record<string, unknown> {
            return {
                name: "InnerItemsWithPreAndPostHtml",
                template: this.innerTemplate
            };
        }
    },
    template: `
<component :is="innerComponent">
    <template v-for="item in augmentedItems" :key="item.slotName" v-slot:[item.innerSlotName]>
        <slot :name="item.slotName" />
    </template>
</component>`
});
