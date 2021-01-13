import { defineComponent } from '../Vendor/Vue/vue.js';

export default defineComponent({
    name: 'JavaScriptAnchor',
    template: `
<a href="javascript:void(0);"><slot /></a>`
});
