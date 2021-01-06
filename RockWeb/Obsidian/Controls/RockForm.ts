import { defineComponent } from '../Vendor/Vue/vue.js';
import { Form } from '../Vendor/VeeValidate/vee-validate.js';
import RockValidation from './RockValidation.js';

export default defineComponent({
    name: 'RockForm',
    components: {
        Form,
        RockValidation
    },
    methods: {
        emitSubmit(payload) {
            this.$emit('submit', payload);
        }
    },
    template: `
<Form as="" #default="{errors, handleSubmit, submitCount}">
    <RockValidation v-if="submitCount" :errors="errors" />
    <form @submit="handleSubmit($event, emitSubmit)">
        <slot />
    </form>
</Form>`
});