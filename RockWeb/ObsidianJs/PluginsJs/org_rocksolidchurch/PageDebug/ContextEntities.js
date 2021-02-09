RegisterObsidianPlugin(
    [
        '/ObsidianJs/Generated/Templates/PaneledBlockTemplate.js',
        '/ObsidianJs/Generated/Util/Guid.js'
    ],
    function (
        PaneledBlockTemplateModule,
        GuidModule
    ) {
        return {
            name: 'org_rocksolidchurch.PageDebug.ContextEntities',
            components: {
                PaneledBlockTemplate: PaneledBlockTemplateModule.default
            },
            data() {
                return {
                    aGuid: GuidModule.newGuid()
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
            template: `
<PaneledBlockTemplate>
    <template v-slot:title>
        <i class="fa fa-pizza-slice"></i>
        Context Entities (JS Plugin)
    </template>
    <template v-slot:default>
        <dl>
            <dt>Random Guid</dt>
            <dd>{{aGuid}}</dd>
            <dt>Person (shortcut)</dt>
            <dd>{{contextPerson.FullName || '<none>'}}</dd>
            <dt>Group (shortcut)</dt>
            <dd>{{contextGroup.Name || '<none>'}}</dd>
            <dt>All Context Entities</dt>
            <dd>
                <ul>
            <template v-for="(entity, key) of contextEntities">
                <li>
                    <strong>{{key}}:</strong>
                    <template v-if="entity">
                        {{entity.FullName || entity.Name || entity.Title || entity.Id}}
                    </template>
                    <template v-else>
                        null
                    </template>
                </li>
            </template>
                </ul>
            </dd>
            <p v-if="!Object.keys(contextEntities).length">
                There are no context entities.
            </p>
        </dl>
    </template>
</PaneledBlockTemplate>`
        };
    });
