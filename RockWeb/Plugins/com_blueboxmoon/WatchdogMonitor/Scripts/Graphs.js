(function () {
    Sys.Application.add_load(function () {
        var baseConfig = {
            type: 'line',
            data: {
                datasets: []
            },
            options: {
                responsive: true,
                title: {
                    display: true
                },
                maintainAspectRatio: false,
                animation: {
                    duration: 750
                },
                legend: {
                    position: 'right',
                    labels: {
                        boxWidth: 20
                    }
                },
                pan: {
                    enabled: true,
                    mode: 'x'
                },
                scales: {
                    yAxes: [{
                        scaleLabel: {
                            display: true
                        }
                    }],
                    xAxes: [{
                        type: 'time',
                        ticks: {
                            minRotation: 0,
                            maxRotation: 0
                        },
                        time: {
                            tooltipFormat: "MM/DD/YYYY h:mm a"
                        }
                    }]
                },
                tooltips: {
                    displayColors: false,
                    callbacks: {
                    }
                }
            }
        };

        var getDataset = function (rows, label, key, color, visible) {
            var dataset = { label: label, borderWidth: 1, fill: false, data: [], hidden: !visible };

            dataset.pointRadius = 1;
            dataset.pointHitRadius = 4;
            dataset.lineTension = 0;
            dataset.borderColor = color;
            dataset.backgroundColor = dataset.borderColor;

            rows.forEach((row) => {
                dataset.data.push({ x: row.DateTime, y: row[key] });
            });

            return dataset;
        };

        var getHourForRange = function (range) {
            if (range === '1h') {
                return 1;
            }
            else if (range === '8h') {
                return 8;
            }
            else if (range === '1d') {
                return 24;
            }
            else if (range === '1w') {
                return 168;
            }
            else if (range === '1m') {
                return 720;
            }
            else if (range === '1y') {
                return 8760;
            }
            else {
                return null;
            }
        };

        $('.js-service-check-chart').each(function () {
            var $self = $(this);
            var chart = null;
            var id = $self.data('id');
            var range = $self.data('range') || 'all';
            var hourRange;
            var defaultView = $self.data('default-view');
            var defaultRange;

            //
            // Setup the range buttons.
            //
            if (range === 'all') {
                var $btnDiv = $('<div class="text-center margin-b-md"><a href="#" class="js-timescale label label-default" data-offset="3600">1h</a> <a href="#" class="js-timescale label label-default" data-offset="28800">8h</a> <a href="#" class="js-timescale label label-default" data-offset="86400">1d</a> <a href="#" class="js-timescale label label-default" data-offset="604800">1w</a> <a href="#" class="js-timescale label label-default" data-offset="2592000">1m</a> <a href="#" class="js-timescale label label-default" data-offset="31536000">1y</a></div>');
                $self.append($btnDiv);
            }

            //
            // Determine the data range to retrieve.
            //
            hourRange = getHourForRange(range);

            //
            // Determine the default range to display.
            //
            if (defaultView) {
                defaultRange = getHourForRange(defaultView);
                if (!defaultRange) {
                    defaultRange = 1;
                }
            }
            else {
                defaultRange = hourRange;
            }
            defaultRange *= 3600;

            $self.find('.js-timescale[data-offset="' + defaultRange + '"]').removeClass('label-default').addClass('label-primary');

            var url = '/api/WatchdogServiceCheckHistory/GetByService?serviceCheckId=' + id;
            if (hourRange) {
                url = url + '&range=' + hourRange;
            }

            $.ajax({
                url: url,
                success: function (history) {
                    var config = JSON.parse(JSON.stringify(baseConfig));

                    config.options.title.text = history.Title;
                    config.options.scales.yAxes[0].scaleLabel.labelString = pluralize(history.Label);
                    config.options.scales.xAxes[0].time.max = Date.now();
                    config.options.scales.xAxes[0].time.min = config.options.scales.xAxes[0].time.max - defaultRange * 1000;

                    config.options.tooltips.callbacks.label = function (tooltipItem, data) {
                        var units = config.options.scales.yAxes[0].scaleLabel.labelString;

                        var average = Math.round(data.datasets[0].data[tooltipItem.index].y * 1000) / 1000;
                        var minimum = Math.round(data.datasets[1].data[tooltipItem.index].y * 1000) / 1000;
                        var maximum = Math.round(data.datasets[2].data[tooltipItem.index].y * 1000) / 1000;
                        return ['Average: ' + average + ' ' + pluralize(units, average),
                        'Minimum: ' + minimum + ' ' + pluralize(units, minimum),
                        'Maximum: ' + maximum + ' ' + pluralize(units, maximum)];
                    };

                    config.data.datasets.push(getDataset(history.Data, 'Avg', 'Avg', 'rgb(54, 162, 235)', true));
                    config.data.datasets.push(getDataset(history.Data, 'Min', 'Min', 'rgb(75, 192, 192)', false));
                    config.data.datasets.push(getDataset(history.Data, 'Max', 'Max', 'rgb(255, 159, 64)', false));

                    chart = new Chart($self.find('canvas').get(0).getContext('2d'), config);
                    $self.find('.js-service-check-loading').fadeOut(100);
                }
            });

            $self.find('.js-timescale').on('click', function (e) {
                e.preventDefault();

                if (chart !== null) {
                    $self.find('.js-timescale').removeClass('label-primary').addClass('label-default');
                    $(this).addClass('label-primary');

                    var axis = chart.config.options.scales.xAxes[0];
                    var center = (axis.time.max - axis.time.min) / 2 + axis.time.min;
                    var newRange = parseInt($(this).data('offset')) * 1000;

                    axis.time.min = center - newRange / 2;
                    axis.time.max = center + newRange / 2;

                    chart.update();
                }
            });
        });
    });
})();
