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
import { getFieldTypeComponent } from '../Fields/Index.js';
import { Guid } from '../Util/Guid.js';
import { Component, defineComponent, PropType } from '../Vendor/Vue/vue.js';

// Import and assign TextField because it is the callback
import TextField from '../Fields/TextField.js';

// Import other field types so they are registered and available upon dynamic request
import '../Fields/BooleanField.js';
import '../Fields/DateField.js';

export default defineComponent({
    name: 'RockField',
    props: {
        fieldTypeGuid: {
            type: String as PropType<Guid>,
            required: true
        }
    },
    computed: {
        fieldComponent(): Component | null {
            const field = getFieldTypeComponent(this.fieldTypeGuid);

            if (!field) {
                // Fallback to text field
                return TextField.component;
            }

            return field;
        }
    },
    template: `
<component :is="fieldComponent" />`
});
