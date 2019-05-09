UPDATE [LavaShortCode] 
SET [Markup] = '{% javascript url:''~/Scripts/moment.min.js'' id:''moment''%}{% endjavascript %}
{% javascript url:''~/Scripts/Chartjs/Chart.min.js'' id:''chartjs''%}{% endjavascript %}

{% assign id = uniqueid %}
{% assign curvedlines = curvedlines | AsBoolean %}

{% assign dataitemCount = dataitems | Size -%}
{% if dataitemCount > 0 -%}
    {% assign fillColors = dataitems | Map:''fillcolor'' | Join:''", "'' | Prepend:''["'' | Append:''"]'' %}
    {% assign borderColors = dataitems | Map:''bordercolor'' | Join:''", "'' | Prepend:''["'' | Append:''"]'' %}
    {% assign firstDataItem = dataitems | First  %}

    {% capture seriesData -%}
    {
        label: ''{{ label }}'',
        fill: {{ filllinearea }}, 
        backgroundColor: {% if firstDataItem.fillcolor %}{{ fillColors }}{% else %}''{{ fillcolor }}''{% endif %},
        borderColor: {% if firstDataItem.bordercolor %}{{ borderColors }}{% else %}''{{ bordercolor }}''{% endif %},
        borderWidth: {{ borderwidth }},
        pointRadius: {{ pointradius }},
        pointBackgroundColor: ''{{ pointcolor }}'',
        pointBorderColor: ''{{ pointbordercolor }}'',
        pointBorderWidth: {{ pointborderwidth }},
        pointHoverBackgroundColor: ''{{ pointhovercolor }}'',
        pointHoverBorderColor: ''{{ pointhoverbordercolor }}'',
        pointHoverRadius: ''{{ pointhoverradius }}'',
        {% if borderdash != '''' -%} borderDash: {{ borderdash }},{% endif -%}
        {% if curvedlines == false -%} lineTension: 0,{% endif -%}
        data: {{ dataitems | Map:''value'' | Join:'','' | Prepend:''['' | Append:'']'' }},
    }
    {% endcapture -%}
    {% assign labels = dataitems | Map:''label'' | Join:''", "'' | Prepend:''"'' | Append:''"'' -%}
{% else -%}
    {% if labels == '''' -%}
        <div class="alert alert-warning">
            When using datasets you must provide labels on the shortcode to define each unit of measure.
            {% raw %}{[ chart labels:''Red, Green, Blue'' ... ]}{% endraw %}
        </div>
    {% else %}
        {% assign labelItems = labels | Split:'','' -%}
        {% assign labels = ''"''- %}
        {% for labelItem in labelItems -%}
            {% assign labelItem = labelItem | Trim %}
            {% assign labels = labels | Append:labelItem | Append:''","'' %}
        {% endfor -%}
        {% assign labels = labels | ReplaceLast:''","'',''"'' %}
    {% endif -%}
    {% assign seriesData = '''' -%}
    {% for dataset in datasets -%}
        {% if dataset.label -%} {% assign datasetLabel = dataset.label %} {% else -%} {% assign datasetLabel = '' '' %} {% endif -%}
        {% if dataset.fillcolor -%} {% assign datasetFillColor = dataset.fillcolor %} {% else -%} {% assign datasetFillColor = fillcolor %} {% endif -%}
        {% if dataset.filllinearea -%} {% assign datasetFillLineArea = dataset.filllinearea %} {% else -%} {% assign datasetFillLineArea = filllinearea %} {% endif -%}
        {% if dataset.bordercolor -%} {% assign datasetBorderColor = dataset.bordercolor %} {% else -%} {% assign datasetBorderColor = bordercolor %} {% endif -%}
        {% if dataset.borderwidth -%} {% assign datasetBorderWidth = dataset.borderwidth %} {% else -%} {% assign datasetBorderWidth = borderwidth %} {% endif -%}
        {% if dataset.pointradius -%} {% assign datasetPointRadius = dataset.pointradius %} {% else -%} {% assign datasetPointRadius = pointradius %} {% endif -%}
        {% if dataset.pointcolor -%} {% assign datasetPointColor = dataset.pointcolor %} {% else -%} {% assign datasetPointColor = pointcolor %} {% endif -%}
        {% if dataset.pointbordercolor -%} {% assign datasetPointBorderColor = dataset.pointbordercolor %} {% else -%} {% assign datasetPointBorderColor = pointbordercolor %} {% endif -%}
        {% if dataset.pointborderwidth -%} {% assign datasetPointBorderWidth = dataset.pointborderwidth %} {% else -%} {% assign datasetPointBorderWidth = pointborderwidth %} {% endif -%}
        {% if dataset.pointhovercolor -%} {% assign datasetPointHoverColor = dataset.pointhovercolor %} {% else -%} {% assign datasetPointHoverColor = pointhovercolor %} {% endif -%}
        {% if dataset.pointhoverbordercolor -%} {% assign datasetPointHoverBorderColor = dataset.pointhoverbordercolor %} {% else -%} {% assign datasetPointHoverBorderColor = pointhoverbordercolor %} {% endif -%}
        {% if dataset.pointhoverradius -%} {% assign datasetPointHoverRadius = dataset.pointhoverradius %} {% else -%} {% assign pointHoverRadius = pointhoverradius %} {% endif -%}
        
        {% capture itemData -%}
            {
                label: ''{{ datasetLabel }}'',
                fill: {{ filllinearea }}, // 1
                backgroundColor: ''{{ datasetFillColor }}'',
                borderColor: ''{{ datasetBorderColor }}'',
                borderWidth: {{ datasetBorderWidth }},
                pointRadius: {{ datasetPointRadius }},
                pointBackgroundColor: ''{{ datasetPointColor }}'',
                pointBorderColor: ''{{ datasetPointBorderColor }}'',
                pointBorderWidth: {{ datasetPointBorderWidth }},
                pointHoverBackgroundColor: ''{{ datasetPointHoverColor }}'',
                pointHoverBorderColor: ''{{ datasetPointHoverBorderColor }}'',
                pointHoverRadius: ''{{ pointhoverradius }}'',
                {% if dataset.borderdash and dataset.borderdash != '''' -%} borderDash: {{ dataset.borderdash }},{% endif -%}
                {% if dataset.curvedlines and dataset.curvedlines == ''false''-%} lineTension: 0,{% endif -%}
                data: [{{ dataset.data }}]
            },
        {% endcapture -%}

        {% assign seriesData = seriesData | Append:itemData -%}
    {% endfor -%}
    {% assign seriesData = seriesData | ReplaceLast:'','', '''' -%}
{% endif -%}

<div class="chart-container" style="position: relative; height:{{ chartheight }}; width:{{ chartwidth }}">
    <canvas id="chart-{{ id }}"></canvas>
</div>

<script>

var options = {
    maintainAspectRatio: false,
    legend: {
        position: ''{{ legendposition }}'',
        display: {{ legendshow }}
    },
    tooltips: {
        enabled: {{ tooltipshow }},
        backgroundColor: ''{{ tooltipbackgroundcolor }}'',
        bodyFontColor: ''{{ tooltipfontcolor }}'',
        titleFontColor: ''{{ tooltipfontcolor }}''
    }
    {% if xaxistype == ''time'' %}
        ,scales: {
        xAxes: [{
            type: "time",
            display: true,
            scaleLabel: {
                display: true,
                labelString: ''Date''
            }
        }],
        yAxes: [{
            display: true,
            scaleLabel: {
                display: true,
                labelString: ''value''
            }
        }]
    }
    {% elseif xaxistype == ''linearhorizontal0to100'' %}
        ,scales: {
        xAxes: [{
                ticks: {
                    min: 0,
                    max: 100
                }
        }],
        yAxes: [{
            gridLines: {
                display: false
              }
        }]
    }
    {% endif %}
};

var data = {
    labels: [{{ labels }}],
    datasets: [{{ seriesData }}],
    borderWidth: {{ borderwidth }}
};

Chart.defaults.global.defaultFontColor = ''{{ fontcolor }}'';
Chart.defaults.global.defaultFontFamily = "{{ fontfamily }}";

var ctx = document.getElementById(''chart-{{ id }}'').getContext(''2d'');
var chart = new Chart(ctx, {
    type: ''{{ type }}'',
    data: data,
    options: options
});    

</script>' WHERE [Guid] = '43819A34-4819-4507-8fEA-2E406b5474EA'


UPDATE [LavaShortCode] SET [Documentation] = REPLACE([Documentation], 'values are ''linear'' and ''time''.','values are ''linear'', ''time'' and ''linearhorizontal0to100''. The linearhorizontal0to100 option makes the horizontal axis scale from 0 to 100.') 
WHERE [Guid] = '43819A34-4819-4507-8fEA-2E406b5474EA'