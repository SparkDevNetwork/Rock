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
import "@Obsidian/FieldTypes/index";
import { flushPromises, mount } from "@vue/test-utils";
import RockField from "@Obsidian/Controls/rockField.obs";
import { PublicAttributeBag } from "@Obsidian/ViewModels/Utility/publicAttributeBag";

describe("Issue 5604", () => {
    it("Defined Value field type includes blank item when required", async () => {
        const wrapper = mount(RockField, {
            props: {
                attribute: attribute,
                isCondensed: false,
                isEditMode: true,
                modelValue: ""
            }
        });

        await flushPromises();

        expect(wrapper.findComponent({name: "DropDownList"}).props("modelValue")).toBe("");
    });
});

const attribute: PublicAttributeBag = {
    "fieldTypeGuid": "59d5a94c-94a0-4630-b80a-bb25697d74c7",
    "attributeGuid": "4abf0bf2-49ba-4363-9d85-ac48a0f7e92a",
    "name": "Ability Level",
    "key": "AbilityLevel",
    "description": "The ability level of the child (used with children's check-in).",
    "isRequired": true,
    "order": 0,
    "categories": [],
    "configurationValues": {
        "definedtype": "7beef4d4-0860-4913-9a3d-857634d1bf7c",
        "allowmultiple": "False",
        "values": "[{\"value\":\"c4550426-ed87-4cb0-957e-c6e0bc96080f\",\"text\":\"Infant\",\"description\":\"The child is unable to crawl yet.\"},{\"value\":\"f78d64d3-6ba1-4eca-a9ec-058fbdf8e586\",\"text\":\"Crawling or Walking\",\"description\":\"The child is able to crawl or walk.\"},{\"value\":\"e6905502-4c23-4879-a60f-8c4ceb3ee2e9\",\"text\":\"Potty Trained\",\"description\":\"The child is also now potty trained.\"}]"
    },
    "preHtml": null,
    "postHtml": null
};
