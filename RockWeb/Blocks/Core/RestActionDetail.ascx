<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RestActionDetail.ascx.cs" Inherits="RockWeb.Blocks.Administration.RestActionDetail" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <h4>
            <asp:Literal ID="ltlRestActionName" runat="server" />
        </h4>
        <Rock:HiddenFieldWithClass ID="hfUrl" runat="server" CssClass="rest-url" />
        <h2>
            <asp:Literal runat="server" ID="lUrlPreview" /></h2>
        <Rock:RockRadioButtonList ID="rblLoadAttributes" CssClass="js-load-attributes" runat="server" Label="LoadAttributes=">
            <asp:ListItem Text="false" Selected="True"/>
            <asp:ListItem Text="simple" />
            <asp:ListItem Text="expanded" />
        </Rock:RockRadioButtonList>
        <Rock:RockTextBox ID="tbPayload" runat="server" Label="Payload" TextMode="MultiLine" Rows="10" />
        <Rock:KeyValueList ID="lstParameterValues" runat="server" Label="Parameter Values" />
        <a id="btnPOST" class="btn btn-action" runat="server" href="javascript:doPost()">POST</a>
        <a id="btnDELETE" class="btn btn-action" runat="server" href="javascript:doDelete()">DELETE</a>
        <a id="btnPUT" class="btn btn-action" runat="server" href="javascript:doPut()">PUT</a>
        <a id="btnGET" class="btn btn-action" runat="server" href="javascript:doGet()">GET</a>

        <h4>Result</h4>
        <span class="js-from-rest-url"></span>
        <pre id="result-data">
        </pre>

        <script>
            function doPost() {
                $.ajax({
                    url: getRestUrl(),
                    type: 'POST',
                    contentType: 'application/json',
                    data: getPayload()
                }).done(handleDone).fail(handleFail);
            }

            function doDelete() {
                $.ajax({
                    url: getRestUrl(),
                    type: 'DELETE'
                }).done(handleDone).fail(handleFail);
            }

            function doPut() {
                $.ajax({
                    url: getRestUrl(),
                    type: 'PUT',
                    contentType: 'application/json',
                    data: getPayload()
                }).done(handleDone).fail(handleFail);
            }

            function doGet() {
                var restUrl = getRestUrl();
                $.ajax({
                    url: restUrl,
                    type: 'GET'
                }).done(handleDone).fail(handleFail);
            }

            function getRestUrl() {
                var restUrl = $('.rest-url').val();
                var $keys = $('.key-value-rows .key-value-key');
                $.each($keys, function (keyIndex) {
                    var $key = $($keys[keyIndex]);
                    var $value = $key.siblings('.key-value-value').first();
                    var keyTemplate = '{' + $key.val() + '}';
                    if (restUrl.indexOf(keyTemplate) < 0) {
                        restUrl = restUrl + (restUrl.indexOf("?") < 0 ? "?" : "&") + $key.val() + "=" + $value.val();
                    }
                    else {
                        restUrl = restUrl.replace(keyTemplate, $value.val());
                    }
                        
                });
                
                var loadAttributesOption = $('.js-load-attributes :checked').val();
                if ((loadAttributesOption || "false") != "false") {
                    restUrl = restUrl + (restUrl.indexOf("?") < 0 ? "?" : "&") + "loadAttributes=" + loadAttributesOption;
                }

                $('.js-from-rest-url').text(restUrl);
                return restUrl;
            }

            function handleFail(a) {
                $('#result-data').html('FAIL:' + a.status + '<br/>' + a.statusText + '<br/><br/>' + a.responseText);
            }

            function handleDone(resultData) {
                $('#result-data').html(JSON.stringify(resultData, null, 2));
            }

            function getPayload() {
                return $('#<%=tbPayload.ClientID %>').val();
            }

        </script>

    </ContentTemplate>
</asp:UpdatePanel>
