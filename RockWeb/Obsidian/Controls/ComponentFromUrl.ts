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

import { Component, defineComponent, markRaw, PropType } from 'vue';
import Alert from '../Elements/Alert';
import LoadingIndicator from '../Elements/LoadingIndicator';

export default defineComponent({
    name: 'ComponentFromUrl',
    components: {
        LoadingIndicator,
        Alert
    },
    props: {
        url: {
            type: String as PropType<string>,
            required: true
        }
    },
    data() {
        return {
            control: null as Component | null,
            loading: true,
            error: ''
        };
    },
    async created() {
        if (!this.url) {
            this.error = `Could not load the control because no URL was provided`;
            this.loading = false;
            return;
        }

        try {
            const controlComponentModule = await import(this.url);
            const control = controlComponentModule ?
                (controlComponentModule.default || controlComponentModule) :
                null;

            if (control) {
                this.control = markRaw(control);
            }
        }
        catch (e) {
            console.error(e);
            this.error = `Could not load the control for '${this.url}'`;
        }
        finally {
            this.loading = false;

            if (!this.control) {
                this.error = `Could not load the control for '${this.url}'`;
            }
        }
    },
    template: `
<Alert v-if="error" alertType="danger">{{error}}</Alert>
<LoadingIndicator v-else-if="loading" />
<component v-else :is="control" />`
});
