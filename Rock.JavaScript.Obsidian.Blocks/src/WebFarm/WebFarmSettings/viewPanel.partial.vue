<!-- Copyright by the Spark Development Network; Licensed under the Rock Community License -->
<template>
    <fieldset>

        <Alert v-if="isInMemoryTransport" alertType="warning">The Web Farm will not function correctly with the In-Memory bus transport. Please configure a different bus transport before using the Web Farm.</Alert>

        <ValueDetailList :modelValue="topValues" />

        <div class="row">
            <div class="col-md-6">
                <ValueDetailList :modelValue="leftSideValues" />
            </div>

            <div class="col-md-6">
                <ValueDetailList :modelValue="rightSideValues" />
            </div>
        </div>

        <AttributeValuesContainer :modelValue="attributeValues" :attributes="attributes" :numberOfColumns="2" />
    </fieldset>
</template>

<script setup lang="ts">
    import { computed, PropType, ref } from "vue";
    import AttributeValuesContainer from "@Obsidian/Controls/attributeValuesContainer";
    import ValueDetailList from "@Obsidian/Controls/valueDetailList";
    import Alert from "@Obsidian/Controls/alert.vue";
    import { ValueDetailListItemBuilder } from "@Obsidian/Core/Controls/valueDetailListItemBuilder";
    import { ValueDetailListItem } from "@Obsidian/Types/Controls/valueDetailListItem";
    import { WebFarmSettingsBag } from "@Obsidian/ViewModels/Blocks/WebFarm/WebFarmSettings/webFarmSettingsBag";
    import { WebFarmSettingsDetailOptionsBag } from "@Obsidian/ViewModels/Blocks/WebFarm/WebFarmSettings/webFarmSettingsDetailOptionsBag";

    const props = defineProps({
        modelValue: {
            type: Object as PropType<WebFarmSettingsBag | null>,
            required: false
        },

        options: {
            type: Object as PropType<WebFarmSettingsDetailOptionsBag>,
            required: true
        }
    });

    // #region Values

    const attributes = ref(props.modelValue?.attributes ?? {});
    const attributeValues = ref(props.modelValue?.attributeValues ?? {});
    const isInMemoryTransport = ref(props.modelValue?.isInMemoryTransport);

    // #endregion

    // #region Computed Values

    /** The values to display full-width at the top of the block. */
    const topValues = computed((): ValueDetailListItem[] => {
        const valueBuilder = new ValueDetailListItemBuilder();

        if (!props.modelValue) {
            return valueBuilder.build();
        }

        return valueBuilder.build();
    });

    /** The values to display at half-width on the left side of the block. */
    const leftSideValues = computed((): ValueDetailListItem[] => {
        const valueBuilder = new ValueDetailListItemBuilder();

        if (!props.modelValue) {
            return valueBuilder.build();
        }

        if (props.modelValue.webFarmKey) {
            valueBuilder.addTextValue("Key", props.modelValue.webFarmKey)
        } else {
            valueBuilder.addTextValue("Key", "None");
        }

        if (props.modelValue.lowerPollingLimit) {
            valueBuilder.addTextValue("Min Polling Limit", `${props.modelValue.lowerPollingLimit} seconds`)
        }

        if (props.modelValue.upperPollingLimit) {
            valueBuilder.addTextValue("Max Polling Limit", `${props.modelValue.upperPollingLimit} seconds`)
        }

        if (props.modelValue.minimumPollingDifference) {
            valueBuilder.addTextValue("Min Polling Difference", `${props.modelValue.minimumPollingDifference} seconds`)
        }

        if (props.modelValue.maxPollingWaitSeconds) {
            valueBuilder.addTextValue("Max Polling Limit", `${props.modelValue.maxPollingWaitSeconds} seconds`)
        }

        return valueBuilder.build();
    });

    /** The values to display at half-width on the left side of the block. */
    const rightSideValues = computed((): ValueDetailListItem[] => {
        const valueBuilder = new ValueDetailListItemBuilder();

        if (!props.modelValue) {
            return valueBuilder.build();
        }

        let nodes = "<div class='row'>";

        if (props.modelValue.nodes) {

            for (const node of props.modelValue.nodes) {
                nodes += `
<div class="col-sm-6 col-md-6 col-lg-4">
    <div class="card card-node ${!node.isActive ? "bg-disabled" : ""}">
        <div class="indicator ${node.isActive ? "bg-success" : ""}${node.isUnresponsive ? " bg-danger" : ""}"></div>
        <div class="card-header">
            <span class="server-meta" title='Polling Interval: ${node.currentLeadershipPollingIntervalSeconds}'>
                <i class="fa fa-${node.isActive ? "server" : "exclamation-triangle"}"></i>
                <span class="node-name text-truncate">
                    ${node.nodeName}
                </span>
            </span>
            ${node.isLeader ? "<span class='node-type-icon' title='Leader'><i class='fa fa-user-tie'></i></span>" : ""}
            ${node.isCurrentJobRunner ? "<span class='node-type-icon' title='Job Runner'><i class='fa fa-cog'></i></span>" : ""}
        </div>
        <div class="card-body p-0" style="height:88px;">
            <span id="spanLastSeen" runat="server" class="label label-danger rounded-pill position-absolute m-2" style="bottom:0;right:0;">
                ${node.humanReadableLastSeen}
            </span>
            <!-- Chart goes here -->
        </div>
    </div>
</div>
`
            }

            nodes += "</div>"
            valueBuilder.addHtmlValue("Nodes", nodes);
        }

        return valueBuilder.build();
    });

    // #endregion

    // #region Functions

    // #endregion

    // #region Event Handlers

    // #endregion
</script>
