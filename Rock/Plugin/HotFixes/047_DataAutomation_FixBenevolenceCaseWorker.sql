DECLARE @AttributeId int = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '7D78DD9F-F5D0-4660-A099-DEFDC70A6664')

UPDATE 
    [AttributeValue] 
SET 
    [Value] = REPLACE([Value],'<p>
        <strong>{{ caseworker.FullName }}</strong> <br />
        {{ Request.Location.FormattedHtmlAddress }} <br />
        {% if Request.HomePhoneNumber %}
            Home Phone: {{ Request.HomePhoneNumber }} <br />
        {% endif %}
        {% if Request.CellPhoneNumber %}
            Cell Phone: {{ Request.CellPhoneNumber }}
        {% endif %}
        {% if Request.WorkPhoneNumber %}
            {{ Request.WorkPhoneNumber }}
        {% endif %}
    </p>','<p>
        <strong>{{ caseworker.FullName }}</strong> <br />
        {{ caseworker | Address:''Home'' }} <br />
		{% assign CaseWorkerHome = caseworker | PhoneNumber:''Home'' %}
		{% assign CaseWorkerCell = caseworker | PhoneNumber:''Mobile'' %}
		{% assign CaseWorkerWork = caseworker | PhoneNumber:''Work'' %}
        {% if CaseWorkerHome %}
            Home Phone: {{ CaseWorkerHome }} <br />
        {% endif %}
        {% if CaseWorkerCell %}
            Cell Phone: {{ CaseWorkerCell }} <br />
        {% endif %}
        {% if CaseWorkerWork %}
            Work Phone: {{ CaseWorkerWork }}
        {% endif %}
    </p>') 
WHERE
	[AttributeId] = @AttributeId

UPDATE 
    [AttributeValue] 
SET 
    [Value] = REPLACE([Value],'<div class=""row"">
    <div class=""col-xs-12"">
        <h4 class=""visible-print-block"">Benevolence Request for {{ Request.FirstName }} {{ Request.LastName }}
        <br />
        <small>Requested: {{ Request.RequestDateTime | Date:''M/d/yyyy'' }} Status: <strong>{{ Request.RequestStatusValue.Value }}</strong></small></h4>
    </div>
</div>','<div class=""row"">
    <div class=""col-xs-12"">
        <h4 class=""visible-print-block"">Benevolence Request for {{ Request.FirstName }} {{ Request.LastName }}
        <br />
        <small>Requested: {{ Request.RequestDateTime | Date:''M/d/yyyy'' }} Status: <strong>{{ Request.RequestStatusValue.Value }}</strong></small></h4>
    </div>
</div>
<div class=""row"">
    <div class=""col-md-12"">
        <h4>Attributes:</h4>
        {% for attribute in Request.AttributeValues %}
            <p>{{ attribute.AttributeName }}: {{ attribute.ValueFormatted }}</p>
        {% endfor %}
    </div>
</div>')
WHERE 
	[AttributeId] = @AttributeId