<%@ control language="C#" autoeventwireup="true" inherits="RockWeb.Blocks.Administration.RestActionDetail, RockWeb" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <h4>
            <asp:Literal ID="ltlRestActionName" runat="server" /></h4>
        <Rock:HiddenFieldWithClass ID="hfUrl" runat="server" CssClass="rest-url" />
        <Rock:HelpBlock runat="server" ID="hbUrlPreview" />
        <Rock:RockTextBox ID="tbPayload" runat="server" Label="Payload" TextMode="MultiLine" Rows="10" />
        <Rock:KeyValueList ID="lstParameterValues" runat="server" Label="Parameter Values" />
        <a id="btnPOST" class="btn btn-action" runat="server" href="javascript:doPost()">POST</a>
        <a id="btnDELETE" class="btn btn-action" runat="server" href="javascript:doDelete()">DELETE</a>
        <a id="btnPUT" class="btn btn-action" runat="server" href="javascript:doPut()">PUT</a>
        <a id="btnGET" class="btn btn-action" runat="server" href="javascript:doGet()">GET</a>

        <h4>Result</h4>
        <pre id="result-data">
        </pre>

        <script>
            function doPost() {
                var restUrl = $('.rest-url').val();
                var data = $('#<%=tbPayload.ClientID %>').val();
                $.post(restUrl, data, function (resultData, status, jgXHR) {
                    $('#result-data').val(resultData);
                }).fail(function (a, b, c, d) {
                    $('#result-data').html('FAIL:' + a.status + ', ' + a.statusText);
                });
            }

            function doDelete() {
                
                var restUrl = $('.rest-url').val();
                var $keys = $('.key-value-rows .key-value-key');
                $.each($keys, function (keyIndex) {
                    var key = $keys[keyIndex];
                    var $value = $(key).siblings('.key-value-value').first();
                    restUrl = restUrl.replace('{' + $(key).val() + '}', $value.val());
                });
                $.ajax({
                    url: restUrl,
                    type: 'DELETE'
                }).done(function (resultData, b, c, d) {
                    debugger
                    $('#result-data').val(resultData);
                }).fail(function (a, b, c, d) {
                    debugger
                    $('#result-data').html('FAIL:' + a.status + ', ' + a.statusText);
                });
            }

            function doPut() {
                var restUrl = $('.rest-url').val();
                $.ajax({
                    url: restUrl,
                    type: 'PUT',
                    data: $('#<%=tbPayload.ClientID %>').val()
                }).done(function (resultData) {
                    $('#result-data').val(resultData);
                }).fail(function (a, b, c, d) {
                    $('#result-data').html('FAIL:' + a.status + ', ' + a.statusText);
                });
            }

            function doGet() {
                var restUrl = $('.rest-url').val();
                var $keys = $('.key-value-rows .key-value-key');
                $.each($keys, function (keyIndex) {
                    var key = $keys[keyIndex];
                    var $value = $(key).siblings('.key-value-value').first();
                    restUrl = restUrl.replace('{' + $(key).val() + '}', $value.val());
                });
                $.ajax({
                    url: restUrl,
                    type: 'GET'
                }).done(function (resultData) {
                    $('#result-data').html(JSON.stringify(resultData, null, 2));
                }).fail(function (a, b, c, d) {
                    $('#result-data').html('FAIL:' + a.status + ', ' + a.statusText);
                });
            }

        </script>

    </ContentTemplate>
</asp:UpdatePanel>
