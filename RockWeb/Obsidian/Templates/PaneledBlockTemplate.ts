import { defineComponent } from '../Vendor/Vue/vue.js';

// Provides a generic Rock Block structure
export default defineComponent({
    name: 'PaneledBlockTemplate',
    template:
`<div class="panel panel-block">
    <div class="panel-heading rollover-container">
        <h1 class="panel-title pull-left">
            <slot name="title" />
        </h1>
        <slot name="titleAside" />
    </div>
    <div class="panel-body">
        <div class="block-content">
            <slot />
        </div>
    </div>
</div>`
});
