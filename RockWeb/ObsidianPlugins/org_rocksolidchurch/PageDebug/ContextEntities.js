Obsidian.Blocks['org_rocksolidchurch.PageDebug.ContextEntities'] = {
    name: 'org_rocksolidchurch.PageDebug.ContextEntities',
    components: {
        PaneledBlockTemplate: Obsidian.Templates.PaneledBlockTemplate
    },
    data() {
        return {
        };
    },
    computed: {
        contextEntities() {
            return this.$store.state.contextEntities;
        }
    },
    template:
`<PaneledBlockTemplate>
    <template slot="title">
        <i class="fa fa-pizza-slice"></i>
        Context Entities
    </template>
    <template>
        <dl>
            <template v-for="(entity, key) of contextEntities">
                <dt>{{key}}</dt>
                <dd>{{entity.FullName || entity.Name || entity.Title || entity.Id}}</dd>
            </template>
            <p v-if="!Object.keys(contextEntities).length">
                There are no context entities.
            </p>
        </dl>
    </template>
</PaneledBlockTemplate>`
};
