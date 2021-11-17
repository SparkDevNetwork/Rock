System.register(["vue", "../Util/guid"], function (exports_1, context_1) {
    "use strict";
    var vue_1, guid_1, progressTrackerItem, ProgressTracker;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (guid_1_1) {
                guid_1 = guid_1_1;
            }
        ],
        execute: function () {
            progressTrackerItem = vue_1.defineComponent({
                name: "ProgressTrackerItem",
                props: {
                    isPast: {
                        type: Boolean,
                        required: true
                    },
                    isPresent: {
                        type: Boolean,
                        required: true
                    },
                    isFuture: {
                        type: Boolean,
                        required: true
                    },
                    isLast: {
                        type: Boolean,
                        required: true
                    },
                    item: {
                        type: Object,
                        required: true
                    }
                },
                template: `
<li class="progress-step progress-tracker-priority">
    <div v-if="isPast" class="progress-step-link">
        <div class="progress-tracker-icon">
            <i class="fas fa-check"></i>
        </div>
        <div class="progress-tracker-details">
            <span class="progress-tracker-title text-truncate">{{item.title}}</span>
            <p class="progress-tracker-subtitle text-truncate">{{item.subtitle}}</p>
        </div>
    </div>
    <div v-else-if="isPresent" class="progress-step-link">
        <div class="progress-tracker-icon current"></div>
        <div class="progress-tracker-details">
            <span class="progress-tracker-title text-truncate">{{item.title}}</span>
            <p class="progress-tracker-subtitle text-truncate">{{item.subtitle}}</p>
        </div>
    </div>
    <div v-else-if="isFuture" class="progress-step-link">
        <div class="progress-tracker-icon upcoming"></div>
        <div class="progress-tracker-details">
            <span class="progress-tracker-title text-truncate">{{item.title}}</span>
            <p class="progress-tracker-subtitle text-truncate">{{item.subtitle}}</p>
        </div>
    </div>
    <div v-if="!isLast" class="progress-tracker-arrow">
        <svg viewBox="0 0 22 80" fill="none" preserveAspectRatio="none">
            <path d="M0 -2L20 40L0 82" vector-effect="non-scaling-stroke" stroke="currentcolor" stroke-linejoin="round" />
        </svg>
    </div>
</li>
`
            });
            ProgressTracker = vue_1.defineComponent({
                name: "ProgressTracker",
                components: {
                    ProgressTrackerItem: progressTrackerItem
                },
                props: {
                    currentIndex: {
                        type: Number,
                        required: true
                    },
                    items: {
                        type: Array,
                        required: true
                    }
                },
                data() {
                    return {
                        guid: guid_1.newGuid(),
                        collapsedIndexes: []
                    };
                },
                computed: {
                    isCollapsed() {
                        return (index) => this.collapsedIndexes.indexOf(index) !== -1;
                    },
                    doNotCollapseIndexes() {
                        return [0, this.currentIndex - 1, this.currentIndex, this.currentIndex + 1, this.lastIndex];
                    },
                    lastIndex() {
                        return this.items.length - 1;
                    },
                    progressTrackerElementId() {
                        return `progress-tracker-${this.guid}`;
                    },
                    progressTrackerContainerElementId() {
                        return `progress-tracker-container-${this.guid}`;
                    },
                },
                methods: {
                    expandAndCollapseItemsBecauseOfWidth() {
                        this.collapsedIndexes = [];
                        this.$nextTick(() => this.collapseItemsBecauseOfWidth());
                    },
                    collapseItemsBecauseOfWidth() {
                        const progressTracker = document.getElementById(this.progressTrackerElementId);
                        const progressTrackerContainer = document.getElementById(this.progressTrackerContainerElementId);
                        const containerWidth = progressTrackerContainer === null || progressTrackerContainer === void 0 ? void 0 : progressTrackerContainer.clientWidth;
                        const childWidth = progressTracker === null || progressTracker === void 0 ? void 0 : progressTracker.scrollWidth;
                        if (!containerWidth || !childWidth || childWidth <= containerWidth) {
                            return;
                        }
                        const midPoint = this.lastIndex / 2;
                        if (this.currentIndex > midPoint) {
                            for (let i = 0; i <= this.lastIndex; i++) {
                                if (this.doNotCollapseIndexes.indexOf(i) !== -1) {
                                    continue;
                                }
                                if (this.isCollapsed(i)) {
                                    continue;
                                }
                                this.collapsedIndexes.push(i);
                                this.$nextTick(() => this.collapseItemsBecauseOfWidth());
                                return;
                            }
                        }
                        else {
                            for (let i = this.lastIndex; i >= 0; i--) {
                                if (this.doNotCollapseIndexes.indexOf(i) !== -1) {
                                    continue;
                                }
                                if (this.isCollapsed(i)) {
                                    continue;
                                }
                                this.collapsedIndexes.push(i);
                                this.$nextTick(() => this.collapseItemsBecauseOfWidth());
                                return;
                            }
                        }
                    }
                },
                watch: {
                    currentIndex: {
                        immediate: true,
                        handler() {
                            this.expandAndCollapseItemsBecauseOfWidth();
                        }
                    }
                },
                template: `
<nav class="progress-tracker" style="margin: 20px auto; max-width: 1200px; width:100%">
    <div :id="progressTrackerContainerElementId" class="progress-tracker-container d-none d-md-block">
        <ul :id="progressTrackerElementId" class="progress-steps">
            <template v-for="(item, index) in items" :key="item.key">
                <li v-if="isCollapsed(index)" class="progress-step progress-tracker-more">
                    <div class="progress-step-link">
                        <i class="fas fa-ellipsis-v"></i>
                    </div>
                    <div class="progress-tracker-arrow">
                        <svg viewBox="0 0 22 80" fill="none" preserveAspectRatio="none">
                            <path d="M0 -2L20 40L0 82" vector-effect="non-scaling-stroke" stroke="currentcolor" stroke-linejoin="round" />
                        </svg>
                    </div>
                </li>
                <ProgressTrackerItem
                    v-else
                    :item="item"
                    :isPast="index < currentIndex"
                    :isPresent="index === currentIndex"
                    :isFuture="index > currentIndex"
                    :isLast="index === lastIndex" />
            </template>
        </ul>
    </div>
    <slot name="aside" />
</nav>`
            });
            exports_1("default", ProgressTracker);
        }
    };
});
//# sourceMappingURL=progressTracker.js.map