import { defineComponent } from '../Vendor/Vue/vue.js';
import { Form } from '../Vendor/VeeValidate/vee-validate.js';
import RockValidation from './RockValidation.js';

export default defineComponent({
    name: 'RockForm',
    components: {
        Form,
        RockValidation
    },
    data() {
        return {
            errorsToDisplay: [],
            submitCount: 0
        };
    },
    methods: {
        emitSubmit(payload) {
            this.$emit('submit', payload);
        },
        getErrorsToDisplay(errors, submitCount: number) {
            if (submitCount !== this.submitCount) {
                this.submitCount = submitCount;
                this.errorsToDisplay = errors;
            }

            return this.errorsToDisplay;
        }
    },
    template: `
<Form as="" #default="{errors, handleSubmit, submitCount}">
    <RockValidation v-if="submitCount" :errors="getErrorsToDisplay(errors, submitCount)" />
    <form @submit="handleSubmit($event, emitSubmit)">
        <slot />
    </form>
</Form>`
});