Obsidian.Blocks.registerBlock({
    name: 'org_rocksolidchurch.PageDebug.ContextEntities',
    components: {
        PaneledBlockTemplate: Obsidian.Templates.PaneledBlockTemplate
    },
    data() {
        return {
        };
    },
    computed: {
        contextPerson() {
            return this.$store.getters.personContext || {};
        },
        contextGroup() {
            return this.$store.getters.groupContext || {};
        },
        contextEntities() {
            return this.$store.state.contextEntities;
        }
    },
    template:
`<PaneledBlockTemplate>
    <template v-slot:title>
        <i class="fa fa-pizza-slice"></i>
        Context Entities
    </template>
    <template v-slot:default>
        <dl>
            <dt>Person (shortcut)</dt>
            <dd>{{contextPerson.FullName}}</dd>
            <dt>Group (shortcut)</dt>
            <dd>{{contextGroup.Name}}</dd>
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
});
