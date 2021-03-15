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

import { defineComponent, PropType } from 'vue';
import { RegistrantInfo } from '../RegistrationEntry';
import Registrant from './Registrant';

export default defineComponent({
    name: 'Event.RegistrationEntry.Registrant',
    components: {
        Registrant
    },
    props: {
        registrants: {
            type: Array as PropType<RegistrantInfo[]>,
            required: true
        }
    },
    data() {
        return {
            currentRegistrantIndex: 0
        };
    },
    template: `
<div class="registrationentry-registrant">
    <h1>{{currentRegistrantTitle}}</h1>
    <ProgressBar :percent="completionPercentInt" />

    <Registrant v-for="(r, i) in registrants" v-if="currentRegistrantIndex === i" :currentRegistrantIndex="i" :key="r.Guid" />
</div>`
});