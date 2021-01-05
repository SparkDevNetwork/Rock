import { defineComponent, PropType } from '../Vendor/Vue/vue.js';

export default defineComponent({
    name: 'RockValidation',
    props: {
        errors: {
            type: Object as PropType<Record<string, string>>,
            required: true
        }
    },
    computed: {
        hasErrors(): boolean {
            return Object.keys(this.errors).length > 0;
        }
    },
    template: `
<div v-if="hasErrors" class="alert alert-validation">
    Please correct the following:
    <ul>
        <li v-for="(error, fieldLabel) of errors">
            <strong>{{fieldLabel}}</strong>
            {{error}}
        </li>
    </ul>
</div>`
});