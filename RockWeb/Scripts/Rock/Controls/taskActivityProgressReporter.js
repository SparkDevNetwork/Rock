(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.taskActivityProgressReporter = (function () {
        const TaskActivityProgressReporter = function (options) {
            if (!Rock.RealTime) {
                throw new Error("realtime.js must be included first.");
            }

            if (!options.controlId) {
                throw new Error("controlId is required.");
            }

            this.controlId = options.controlId;
            this.connectionId = undefined;

            this.connect();
        }

        TaskActivityProgressReporter.prototype.getTaskId = function () {
            const control = document.getElementById(this.controlId);

            if (!control) {
                return null;
            }

            return control.getAttribute("data-task-id");
        };

        TaskActivityProgressReporter.prototype.connect = async function () {
            this.topic = await Rock.RealTime.getTopic("Rock.RealTime.Topics.TaskActivityProgressTopic");

            this.topic.on("taskStarted", this.onTaskStarted.bind(this));
            this.topic.on("taskCompleted", this.onTaskCompleted.bind(this));
            this.topic.on("updateTaskProgress", this.onUpdateTaskProgress.bind(this));

            const $connectionId = $(`#${this.controlId} [id$='_hfConnectionId']`);

            $connectionId.val(this.topic.connectionId || "");
        };

        TaskActivityProgressReporter.prototype.onTaskStarted = function (status) {
            // Intentionally left blank for now.
        };

        TaskActivityProgressReporter.prototype.onTaskCompleted = function (status) {
            const taskId = this.getTaskId();

            if (status.taskId !== taskId) {
                return;
            }

            const $preparing = $(`#${this.controlId} .js-preparing`);
            const $progress = $(`#${this.controlId} .js-progress-div`);
            const $results = $(`#${this.controlId} .js-results`);

            $results.removeClass("alert-danger").removeClass("alert-warning").removeClass("alert-success");

            if (status.errors && status.errors.length > 0) {
                $results.addClass("alert-danger");
            }
            else if (status.warnings && status.warnings.length > 0) {
                $results.addClass("alert-warning");
            }
            else {
                $results.addClass("alert-success");
            }

            $results.text(status.message).slideDown();
            $preparing.slideUp();
            $progress.slideUp();
        };

        TaskActivityProgressReporter.prototype.onUpdateTaskProgress = function (progress) {
            const taskId = this.getTaskId();

            if (progress.taskId !== taskId) {
                return;
            }

            const $preparing = $(`#${this.controlId} .js-preparing`);
            const $progress = $(`#${this.controlId} .js-progress-div`);
            const $bar = $(`#${this.controlId} .js-progress-bar`);

            $bar.prop("aria-valuenow", progress.completionPercentage);
            $bar.prop("aria-valuemax", "100");
            $bar.css("width", `${progress.completionPercentage}%`);

            if (progress.message) {
                $bar.text(progress.message);
            }
            else {
                $bar.text(`${progress.completionPercentage}%`);
            }

            $progress.slideDown();
            $preparing.slideUp();
        };

        const reporters = {};

        const exports = {
            initialize(options) {
                if (!options.controlId) {
                    throw new Error("id is required.");
                }

                if (reporters[options.controlId]) {
                    return;
                }

                reporters[options.controlId] = new TaskActivityProgressReporter(options);
            }
        };

        return exports;
    }());
}(jQuery));
