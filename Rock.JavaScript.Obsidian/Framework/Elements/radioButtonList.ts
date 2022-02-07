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
import { Guid } from "../Util/guid";
import { ListItem } from "../ViewModels";
import RockFormField from "./rockFormField";

export default defineComponent({
    name: "RadioButtonList",
    components: {
        RockFormField
    },
    props: {
        options: {
            type: Array as PropType<ListItem[]>,
            default: []
        },
        modelValue: {
            type: String as PropType<string>,
            default: ""
        },
        repeatColumns: {
            type: Number as PropType<number>,
            default: 0
        },
        horizontal: {
            type: Boolean as PropType<boolean>,
            default: false
        }
    },
    emits: [
        "update:modelValue"
    ],
    data() {
        return {
            internalValue: ""
        };
    },
    computed: {
        containerClasses (): string {
            const classes: string[] = [];

            if (this.repeatColumns > 0) {
                classes.push(`in-columns in-columns-${this.repeatColumns}`);
            }

            if (this.horizontal) {
                classes.push("rockradiobuttonlist-horizontal");
            }
            else {
                classes.push("rockradiobuttonlist-vertical");
            }

            return classes.join(" ");
        }
    },
    methods: {
        getOptionUniqueId(uniqueId: Guid, option: ListItem): string {
            const key = option.value.replace(" ", "-");

            return `${uniqueId}-${key}`;
        }
    },
    watch: {
        internalValue() {
            this.$emit("update:modelValue", this.internalValue);
        },
        modelValue: {
            immediate: true,
            handler() {
                this.internalValue = this.modelValue;
            }
        }
    },
    template: `
<RockFormField formGroupClasses="rock-radio-button-list" #default="{uniqueId}" name="radiobuttonlist" v-model="internalValue">
    <div class="control-wrapper">
        <div class="controls rockradiobuttonlist" :class="containerClasses">
            <span>
                <template v-if="horizontal">
                    <label v-for="option in options" class="radio-inline" :for="getOptionUniqueId(uniqueId, option)">
                        <input :id="getOptionUniqueId(uniqueId, option)" :name="uniqueId" type="radio" :value="option.value" v-model="internalValue" />
                        <span class="label-text">{{option.text}}</span>
                    </label>
                </template>
                <template v-else>
                    <div v-for="option in options" class="radio">
                        <label :for="getOptionUniqueId(uniqueId, option)">
                            <input :id="getOptionUniqueId(uniqueId, option)" :name="uniqueId" type="radio" :value="option.value" v-model="internalValue" />
                            <span class="label-text">{{option.text}}</span>
                        </label>
                    </div>
                </template>
            </span>
        </div>
    </div>
</RockFormField>`
});
