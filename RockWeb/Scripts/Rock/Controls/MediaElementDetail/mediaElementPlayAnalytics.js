(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.mediaElementPlayAnalytics = (function () {
        /**
         * Default configuration for the chart. Basically turn off everything
         * that we can so we get left with a bare bones chart that fits perfectly
         * inside the video thumbnail.
         */
        const baseChartConfig = {
            type: 'line',
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: false
                },
                scales: {
                    xAxes: [
                        {
                            display: false
                        }
                    ],
                    yAxes: [
                        {
                            id: 'y-axis-watched',
                            display: false,
                            ticks: {
                                min: 0,
                                max: 100
                            }
                        },
                        {
                            id: 'y-axis-rewatched',
                            display: false,
                            ticks: {
                                min: 0,
                                max: 100
                            }
                        }
                    ],
                },
                tooltips: {
                    callbacks: {
                        label: function (tooltipItem, data) {
                            let value = tooltipItem.value;
                            let label = data.datasets[tooltipItem.datasetIndex].label;
                            let valueSuffix = data.datasets[tooltipItem.datasetIndex].valueSuffix || '';

                            // If this is the engagement dataset do some special
                            // processing to get the actual percentage value.
                            if (tooltipItem.datasetIndex === 0) {
                                label = 'Engagement';
                                if (data.totalPlayCount > 0) {
                                    value = Math.round(value / data.totalPlayCount * 1000) / 10;
                                }
                                else {
                                    value = '0';
                                }

                                valueSuffix = '%';
                            }

                            // If this is the rewatched dataset, we need to
                            // subtract the two values to get the number of
                            // rewatched times.
                            else if (tooltipItem.datasetIndex === 1) {
                                value = data.datasets[1].data[tooltipItem.index] - data.datasets[0].data[tooltipItem.index];
                            }

                            return label + ': ' + value + valueSuffix;
                        }
                    }
                }

            }
        };

        /**
         * Takes the data calculated in the C# code and converts it into data
         * that can be used by the chart plug in.
         *
         * @param source The source data to be converted into ChartJS data.
         */
        const buildChartData = function (source) {
            let chartData = {
                labels: [],
                datasets: [],
                totalPlayCount: 0
            };

            // Shouldn't happen, but just in case...
            if (source.Duration === undefined) {
                chartData.datasets.push({ data: [] });
                chartData.datasets.push({ data: [] });
                return chartData;
            }

            chartData.totalPlayCount = source.PlayCount;

            // Build the labels as time offsets.
            for (var second = 0; second < source.Duration; second++) {
                let hour = Math.floor(second / 3600);
                let minute = Math.floor((second % 3600) / 60);
                let sec = second % 60;
                let time = '';

                if (hour > 0) {
                    time = hour + ':' + (minute < 10 ? '0' + minute : minute) + ':' + (sec < 10 ? '0' + sec : sec);
                }
                else {
                    time = minute + ':' + (sec < 10 ? '0' + sec : sec);
                }

                chartData.labels.push(time);
            }

            // Get the dataset for the number of individuals played each second.
            if (source.Watched !== undefined) {
                chartData.datasets.push({
                    label: 'Watched',
                    yAxisID: 'y-axis-watched',
                    data: source.Watched,
                    fill: 'origin',
                    borderColor: 'rgb(99, 179, 237)',
                    backgroundColor: 'rgba(99, 179, 237, 0.6)',
                    tension: 0,
                    pointRadius: 0,
                    pointHitRadius: 2
                });
            }
            else {
                chartData.datasets.push({ data: [] });
            }

            // Get the dataset for the total number of times each second was played.
            if (source.Rewatched !== undefined) {
                chartData.datasets.push({
                    label: 'Rewatched',
                    yAxisID: 'y-axis-rewatched',
                    data: source.Rewatched,
                    fill: '-1',
                    borderColor: 'rgb(72, 187, 120)',
                    backgroundColor: 'rgba(72, 187, 120, 0.6)',
                    tension: 0,
                    pointRadius: 0,
                    pointHitRadius: 2
                });
            }
            else {
                chartData.datasets.push({ data: [] });
            }

            return chartData;
        };

        /**
         * Updates the lower and upper limits of the chart to properly display
         * everything we need it to.
         *
         * @param chart The chart instance to be updated.
         */
        const updateChartLimits = function (chart) {
            var maxWatched = Math.max.apply(null, chart.data.datasets[1].data);

            chart.config.options.scales.yAxes[0].ticks.max = parseInt(maxWatched * 1.1);
            chart.config.options.scales.yAxes[1].ticks.max = parseInt(maxWatched * 1.1);
        };

        /**
         * Updates the chart from the data we got from the API.
         *
         * @param chart The chart instance to be updated.
         * @param data The data to use when building the chart.
         */
        const updateChartData = function (chart, data) {
            chart.data = buildChartData(data);
            updateChartLimits(chart);
            chart.update();
        };

        /**
         * Update the chart with data retrieved from the server.
         *
         * @param chart The chart to be updated.
         * @param options The configuration options.
         */
        const updateChart = function (chart, options) {
            $.get("/api/blocks/action/" + options.blockGuid + "/GetVideoMetricData?MediaElementGuid=" + options.mediaElementGuid)
                .then(function (data) {
                    if (data) {
                        if (options.averageWatchEngagementId) {
                            $("#" + options.averageWatchEngagementId).text(data.AverageWatchEngagement);
                        }

                        if (options.totalPlaysId) {
                            $("#" + options.totalPlaysId).text(data.PlayCount);
                        }

                        if (options.minutesWatchedId) {
                            $("#" + options.minutesWatchedId).text(data.MinutesWatched);
                        }

                        updateChartData(chart, data);
                    }
                    else {
                        // TODO: Display an error message to the user.
                    }
                });
        };

        const loadMoreIndividualPlays = async function ($panel, options, pageContext) {
            const data = {
                mediaElementGuid: options.mediaElementGuid,
                pageContext
            };

            const $btnLoadMore = $panel.find('.js-load-more');
            $btnLoadMore.html('<i class="fa fa-refresh fa-spin"></i> Load More');
            $btnLoadMore.addClass("disabled");

            const result = await $.ajax({
                type: "POST",
                url: "/api/blocks/action/" + options.blockGuid + "/LoadIndividualPlays",
                data: JSON.stringify(data),
                contentType: "application/json"
            });

            if (!result.Items || result.Items.length === 0) {
                $btnLoadMore.hide();
                return;
            } else {
                if (result.Items.length < 25) {
                    $btnLoadMore.hide();
                }
            }

            $btnLoadMore.removeClass("disabled");
            $btnLoadMore.data("context", result.NextPage);
            $btnLoadMore.text("Load More");

            for (var i = 0; i < result.Items.length; i++) {
                const item = result.Items[i];

                var $row = $('<div class="individual-play-row"></div>');
                var $date = $('<div class="individual-play-date"></div>');
                var $bar = $('<div class="individual-play-bar"></div>');
                var $person = $('<div class="individual-play-person"></div>');
                var $additional = $('<div class="position-relative additional-data"></div>');
                var $interaction = $('<div class="individual-play-interaction"></div>');
                var $chart = $('<div class="individual-play-chart"></div>');
                var $percent = $('<div class="individual-play-percent"></div>');
                $row.append($date, $bar);
                $bar.append($person, $additional, $chart, $percent);
                $additional.append($interaction);

                const date = new moment(item.DateTime);

                $row.data("date", date.format("MMM D, YYYY"));

                const lastDate = $panel.find(".individual-play-row").last().data("date");
                if ($row.data("date") !== lastDate) {
                    $date.html("<span>" + date.format("dddd") + "</span> <span>" + date.format("MMM D, YYYY") + "</span> <span>" + date.format("h:mm a") + "</span>");
                }
                else {
                    $date.html(date.format("h:mm a"));
                }

                $person.append(getPerson(item))

                if (item.Isp !== null || item.OperatingSystem !== null || item.Application !== null) {
                    $interaction.append(getInteractionData(item));
                } else {
                    $interaction.addClass("no-details");
                }

                $chart.append(getHeatMap(item.Data.WatchMap));
                $percent.text(Math.floor(item.Data.WatchedPercentage) + "%");

                $row.insertBefore($btnLoadMore);
            }

            initializeExpanderHoverEventListener();
        }

        const getInteractionData = function (item) {
            const sessionInfo = document.createElement("div");
            sessionInfo.setAttribute("class", "individual-play-interaction-session");

            const isp = document.createElement("span");
            isp.textContent = item.Isp;
            const operatingSystem = document.createElement("span");
            operatingSystem.textContent = item.OperatingSystem;
            const applciation = document.createElement("span");
            applciation.textContent = item.Application;

            sessionInfo.append(isp);
            sessionInfo.append(operatingSystem);
            sessionInfo.append(applciation);

            const expander = document.createElement("div");
            expander.setAttribute("class", "expander");
            expander.setAttribute("name", "expander");
            const expanderIcon = document.createElement("i");
            expanderIcon.setAttribute("class", "fa fa-chevron-right text-muted o-50");
            expander.setAttribute("name", "expander");
            expander.append(expanderIcon);

            return [sessionInfo, expander];
        }

        const initializeExpanderHoverEventListener = function () {
            let interactions = document.querySelectorAll(".individual-play-interaction:not(.no-details)");

            interactions.forEach(interaction => interaction.addEventListener("mouseover", event => {
                if (event.target.getAttribute("name") === "expander") {
                    let icon = event.target.querySelector(".fa-chevron-right");
                    if (icon) {
                        icon.classList.add("fa-flip-horizontal");
                    }

                    interaction.classList.add("expanded");
                }
            }));

            interactions.forEach(interaction => interaction.addEventListener("mouseleave", event => {
                if (interaction.querySelector(".fa-chevron-right")) {
                    let icon = interaction.querySelector(".fa-chevron-right");
                    icon.classList.remove("fa-flip-horizontal");

                    interaction.classList.remove("expanded");
                }
            }));
        }

        const getPerson = function (item) {
            const clientTypeIcons = {
                Mobile: "fa fa-mobile-alt",
                Desktop: "fa fa-desktop",
                Tablet: "fa fa-tablet-alt",
            }

            const clientType = document.createElement("i");
            clientType.setAttribute("class", clientTypeIcons[item.ClientType] + " fa-lg text-muted o-50 mx-2");
            clientType.setAttribute("title", item.ClientType);

            const interactions = document.createElement("span");
            if (item.InteractionsCount > 0) {
                interactions.setAttribute("class", "badge-circle badge-info flex-shrink-0 ml-1");
                interactions.setAttribute("title", "Interactions");
                interactions.textContent = item.InteractionsCount;
            }

            const personInfo = document.createElement("div");
            personInfo.setAttribute("class", "individual-play-person-info");

            const personDetail = document.createElement("span");
            personDetail.setAttribute("class", "individual-play-person-detail");

            const photo = document.createElement("div");
            const url = item.PhotoUrl + (item.PhotoUrl.indexOf("?") === -1 ? "?w=50" : "&w=50");
            photo.setAttribute("class", "photo-icon photo-round photo-round-xs flex-shrink-0 margin-r-sm");
            photo.setAttribute("style", "background-image: url('" + url + "'); background-size: cover; background-repeat: no-repeat;");

            const name = document.createElement("span");
            name.setAttribute("class", "text-truncate");
            name.textContent = item.FullName;

            const location = document.createElement("span");
            location.setAttribute("class", "individual-play-person-location");
            location.textContent = item.Location;

            personInfo.appendChild(personDetail);
            personDetail.appendChild(photo);
            personDetail.appendChild(name);
            personInfo.appendChild(location);


            return [personInfo, clientType, interactions];
        }

        const getHeatMap = function (watchMap) {
            const segments = watchMap.split(",");

            const svg = document.createElementNS("http://www.w3.org/2000/svg", "svg");

            if (segments.length === 0) {
                return svg;
            }

            let totalDuration = 0;
            for (var i = 0; i < segments.length; i++) {
                const segment = segments[i];
                totalDuration += parseInt(segment.substr(0, segment.length - 1));
            }

            svg.setAttribute("viewBox", "0 0 " + totalDuration + " 60");
            svg.setAttribute("preserveAspectRatio", "none");
            svg.setAttribute("version", "1.1");

            let x = 0;
            for (var i = 0; i < segments.length; i++) {
                const segment = segments[i];
                const duration = parseInt(segment.substr(0, segment.length - 1));
                const bit = parseInt(segment.substr(segment.length - 1, 1));

                if (bit === 0) {
                    x += duration;
                    continue;
                }

                const color = bit === 1 ? "#73c9f0" : "#34b0e9";

                const rect = document.createElementNS("http://www.w3.org/2000/svg", "rect");
                rect.setAttribute("x", x);
                rect.setAttribute("y", "0");
                rect.setAttribute("width", duration);
                rect.setAttribute("height", "60");
                rect.setAttribute("fill", color);

                svg.appendChild(rect);

                x += duration;
            }

            return svg;
        }

        const initializeChartAndMetrics = function (options) {
            // Chart is hidden.
            if ($("#" + options.chartId).length === 0) {
                return;
            }

            // Initialize the chart.
            const chart = new Chart(document.getElementById(options.chartId), baseChartConfig);

            const $chartContainer = $("#" + options.chartId);

            // Detect mouse hover events and update the position of the
            // video to match the position of the mouse as if it were a
            // scrub tool.
            if (options.player) {
                $chartContainer.on("mousemove", (ev) => {
                    var posX = ev.pageX - $chartContainer.offset().left;
                    var offsetPercent = posX / $chartContainer.width();

                    options.player.seek(options.player.duration * offsetPercent);
                });
            }

            updateChart(chart, options);
        }

        const initializeIndividualPlays = function (options) {
            if (!options.individualPlaysId) {
                return;
            }

            const $individualPlaysPanel = $("#" + options.individualPlaysId);
            const $btnLoadMore = $individualPlaysPanel.find(".js-load-more");

            $btnLoadMore.on("click", ev => {
                ev.preventDefault();

                if ($btnLoadMore.hasClass("disabled")) {
                    return;
                }

                const context = $btnLoadMore.data("context");

                loadMoreIndividualPlays($individualPlaysPanel, options, context);
            });

            loadMoreIndividualPlays($individualPlaysPanel, options, null);
        }

        var exports = {
            /**
             * Initialize the statistics block for dynamically building charts.
             *
             * @param options The configuration options to use.
             */
            initialize: function (options) {
                if (!options.chartId) {
                    throw 'chartId is required';
                }

                initializeChartAndMetrics(options);
                initializeIndividualPlays(options);
            },
        };

        return exports;
    }());
}(jQuery));
