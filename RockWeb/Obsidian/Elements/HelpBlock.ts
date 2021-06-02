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
import JavaScriptAnchor from './JavaScriptAnchor';

/** Displays a help block tool-tip. */
const HelpBlock = defineComponent( {
    name: 'HelpBlock',
    components: {
        JavaScriptAnchor
    },
    props: {
        text: {
            type: String as PropType<string>,
            required: true
        }
    },
    mounted ()
    {
        const jquery = window[ '$' ];
        jquery( this.$el ).tooltip();
    },
    template: `
<JavaScriptAnchor class="help" tabindex="-1" data-toggle="tooltip" data-placement="auto" data-container="body" data-html="true" title="" :data-original-title="text">
    <i class="fa fa-info-circle"></i>
</JavaScriptAnchor>`
} );

export default HelpBlock;
