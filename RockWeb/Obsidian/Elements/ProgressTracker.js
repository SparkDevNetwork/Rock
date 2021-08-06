System.register(["vue", "../Util/Guid"], function (exports_1, context_1) {
    "use strict";
    var vue_1, Guid_1, ProgressTrackerItem, ProgressTracker;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (Guid_1_1) {
                Guid_1 = Guid_1_1;
            }
        ],
        execute: function () {
            ProgressTrackerItem = vue_1.defineComponent({
                name: 'ProgressTrackerItem',
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
                template: "\n<li class=\"progress-step progress-tracker-priority\">\n    <div v-if=\"isPast\" class=\"progress-step-link\">\n        <div class=\"progress-tracker-icon\">\n            <i class=\"fas fa-check\"></i>\n        </div>\n        <div class=\"progress-tracker-details\">\n            <span class=\"progress-tracker-title text-truncate\">{{item.Title}}</span>\n            <p class=\"progress-tracker-subtitle text-truncate\">{{item.Subtitle}}</p>\n        </div>\n    </div>\n    <div v-else-if=\"isPresent\" class=\"progress-step-link\">\n        <div class=\"progress-tracker-icon current\"></div>\n        <div class=\"progress-tracker-details\">\n            <span class=\"progress-tracker-title text-truncate\">{{item.Title}}</span>\n            <p class=\"progress-tracker-subtitle text-truncate\">{{item.Subtitle}}</p>\n        </div>\n    </div>\n    <div v-else-if=\"isFuture\" class=\"progress-step-link\">\n        <div class=\"progress-tracker-icon upcoming\"></div>\n        <div class=\"progress-tracker-details\">\n            <span class=\"progress-tracker-title text-truncate\">{{item.Title}}</span>\n            <p class=\"progress-tracker-subtitle text-truncate\">{{item.Subtitle}}</p>\n        </div>\n    </div>\n    <div v-if=\"!isLast\" class=\"progress-tracker-arrow\">\n        <svg viewBox=\"0 0 22 80\" fill=\"none\" preserveAspectRatio=\"none\">\n            <path d=\"M0 -2L20 40L0 82\" vector-effect=\"non-scaling-stroke\" stroke=\"currentcolor\" stroke-linejoin=\"round\" />\n        </svg>\n    </div>\n</li>\n"
            });
            ProgressTracker = vue_1.defineComponent({
                name: 'ProgressTracker',
                components: {
                    ProgressTrackerItem: ProgressTrackerItem
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
                data: function () {
                    return {
                        guid: Guid_1.newGuid(),
                        collapsedIndexes: []
                    };
                },
                computed: {
                    isCollapsed: function () {
                        var _this = this;
                        return function (index) { return _this.collapsedIndexes.indexOf(index) !== -1; };
                    },
                    doNotCollapseIndexes: function () {
                        return [0, this.currentIndex - 1, this.currentIndex, this.currentIndex + 1, this.lastIndex];
                    },
                    lastIndex: function () {
                        return this.items.length - 1;
                    },
                    progressTrackerElementId: function () {
                        return "progress-tracker-" + this.guid;
                    },
                    progressTrackerContainerElementId: function () {
                        return "progress-tracker-container-" + this.guid;
                    },
                },
                methods: {
                    expandAndCollapseItemsBecauseOfWidth: function () {
                        var _this = this;
                        this.collapsedIndexes = [];
                        this.$nextTick(function () { return _this.collapseItemsBecauseOfWidth(); });
                    },
                    collapseItemsBecauseOfWidth: function () {
                        var _this = this;
                        var progressTracker = document.getElementById(this.progressTrackerElementId);
                        var progressTrackerContainer = document.getElementById(this.progressTrackerContainerElementId);
                        var containerWidth = progressTrackerContainer === null || progressTrackerContainer === void 0 ? void 0 : progressTrackerContainer.clientWidth;
                        var childWidth = progressTracker === null || progressTracker === void 0 ? void 0 : progressTracker.scrollWidth;
                        if (!containerWidth || !childWidth || childWidth <= containerWidth) {
                            return;
                        }
                        var midPoint = this.lastIndex / 2;
                        if (this.currentIndex > midPoint) {
                            for (var i = 0; i <= this.lastIndex; i++) {
                                if (this.doNotCollapseIndexes.indexOf(i) !== -1) {
                                    continue;
                                }
                                if (this.isCollapsed(i)) {
                                    continue;
                                }
                                this.collapsedIndexes.push(i);
                                this.$nextTick(function () { return _this.collapseItemsBecauseOfWidth(); });
                                return;
                            }
                        }
                        else {
                            for (var i = this.lastIndex; i >= 0; i--) {
                                if (this.doNotCollapseIndexes.indexOf(i) !== -1) {
                                    continue;
                                }
                                if (this.isCollapsed(i)) {
                                    continue;
                                }
                                this.collapsedIndexes.push(i);
                                this.$nextTick(function () { return _this.collapseItemsBecauseOfWidth(); });
                                return;
                            }
                        }
                    }
                },
                watch: {
                    currentIndex: {
                        immediate: true,
                        handler: function () {
                            this.expandAndCollapseItemsBecauseOfWidth();
                        }
                    }
                },
                template: "\n<nav class=\"progress-tracker\" style=\"margin: 20px auto; max-width: 1200px; width:100%\">\n    <div :id=\"progressTrackerContainerElementId\" class=\"progress-tracker-container d-none d-md-block\">\n        <ul :id=\"progressTrackerElementId\" class=\"progress-steps\">\n            <template v-for=\"(item, index) in items\" :key=\"item.key\">\n                <li v-if=\"isCollapsed(index)\" class=\"progress-step progress-tracker-more\">\n                    <div class=\"progress-step-link\">\n                        <i class=\"fas fa-ellipsis-v\"></i>\n                    </div>\n                    <div class=\"progress-tracker-arrow\">\n                        <svg viewBox=\"0 0 22 80\" fill=\"none\" preserveAspectRatio=\"none\">\n                            <path d=\"M0 -2L20 40L0 82\" vector-effect=\"non-scaling-stroke\" stroke=\"currentcolor\" stroke-linejoin=\"round\" />\n                        </svg>\n                    </div>\n                </li>\n                <ProgressTrackerItem\n                    v-else\n                    :item=\"item\"\n                    :isPast=\"index < currentIndex\"\n                    :isPresent=\"index === currentIndex\"\n                    :isFuture=\"index > currentIndex\"\n                    :isLast=\"index === lastIndex\" />\n            </template>\n        </ul>\n    </div>\n    <slot name=\"aside\" />\n</nav>"
            });
            exports_1("default", ProgressTracker);
        }
    };
});
//# sourceMappingURL=ProgressTracker.js.map