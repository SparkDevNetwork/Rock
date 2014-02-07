<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SystemConfiguration.ascx.cs" Inherits="RockWeb.Blocks.Administration.SystemConfiguration" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbMessage" runat="server" NotificationBoxType="Info" Title="Note" Visible="true" Text="Consider making these changes when everyone else is sleeping.  Once you save the changes, your website will be restarted."></Rock:NotificationBox>

        <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger"  />
        <fieldset>
            <dl>
                <dt>Time Zone</dt>
                <dd>
                    <Rock:RockDropDownList ID="ddTimeZone" runat="server" CausesValidation="false"></Rock:RockDropDownList>
                </dd>

                <dt>Max Upload File Size (MB) <small id="convertedGb"></small></dt>
                <dd>
                    <Rock:NumberBox ID="numbMaxSize" runat="server" NumberType="Integer" MinimumValue="1" MaximumValue="10000" AppendText="MB"></Rock:NumberBox>
                </dd>

                <dt>Max Allowed Content Length (MB) <small id="convertedGb2"></small></dt>
                <dd>
                    <Rock:NumberBox ID="numbMaxContentLength" runat="server" NumberType="Integer" MinimumValue="1" MaximumValue="10000"  AppendText="MB"></Rock:NumberBox>
                </dd>

            </dl>


        </fieldset>

        <div class="actions margin-t-lg">
            <Rock:BootstrapButton ID="bbtnSaveConfig" runat="server" CssClass="btn btn-primary" OnClick="bbtnSaveConfig_Click" Text="Save" DataLoadingText="Saving..."></Rock:BootstrapButton>
        </div>

        <script>
            $(function () {

                $("[id$=numbMaxSize]").keyup(function () {
                    var mb = $("[id$=numbMaxSize]").val();
                    $("[id$=convertedGb]").html( parseFloat( mb / 1024 ).toFixed(2) + " GB");
                });

                $("[id$=numbMaxContentLength]").keyup(function () {
                    var mb = $("[id$=numbMaxContentLength]").val();
                    $("[id$=convertedGb2]").html( parseFloat( mb / 1024 ).toFixed(2) + " GB");
                });

                /*
                $("[id$=bbtnSaveConfig]").on("click", function () {
                    bootbox.alert("The Rock application will be restarted. Press OK to reload and continue.", function () {
                        window.location.replace(location.href);
                        return true;
                    });
                });
                */
            });
        </script>

    </ContentTemplate>
</asp:UpdatePanel>
