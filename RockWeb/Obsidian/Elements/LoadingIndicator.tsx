import { defineComponent } from 'vue'

export default defineComponent({
    name: 'LoadingIndicator',
    render() {
        return (
            <div className="text-muted">
                Loading...
            </div>
        )
    }
});


/*
import { defineComponent } from "vue"

export default defineComponent({
    name: 'LoadingIndicator',
    data() {
        return {
            hello: 'world'
        };
    },
    methods: {
        sayHi() {
            this.hello = 3;
        }
    },
    template:
`<div class="text-muted">
    Loading...
</div>`
});
*/