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
import { defineComponent } from "vue";
import { getFieldEditorProps } from "./utils";
import { toNumberOrNull } from "../Services/number";
import Rating from "../Elements/rating";
import { ConfigurationValueKey, RatingValue } from "./ratingField";

export const EditComponent = defineComponent({
    name: "RatingField.Edit",

    components: {
        Rating
    },

    props: getFieldEditorProps(),

    data() {
        return {
            /** The current rating value. */
            internalValue: 0
        };
    },

    computed: {
        maxRating(): number {
            const maxRatingConfig = this.configurationValues[ConfigurationValueKey.MaxRating];

            return toNumberOrNull(maxRatingConfig) || 5;
        },

    },

    watch: {
        /**
         * Watch for changes to internalValue and emit the new value out to
         * the consuming component.
         */
        internalValue(): void {
            const ratingValue: RatingValue = {
                value: this.internalValue,
                maxValue: this.maxRating
            };

            this.$emit("update:modelValue", JSON.stringify(ratingValue));
        },

        /**
         * Watch for changes to modelValue which indicate the component
         * using us has given us a new value to work with.
         */
        modelValue: {
            immediate: true,
            handler(): void {
                try {
                    const ratingValue = JSON.parse(this.modelValue ?? "") as RatingValue;
                    this.internalValue = ratingValue.value ?? 0;
                }
                catch {
                    this.internalValue = 0;
                }
            }
        }
    },
    template: `
<Rating v-model="internalValue" :maxRating="maxRating" />
`
});
