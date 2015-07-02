<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ExternalConnectionOpportunityDetail.ascx.cs" Inherits="RockWeb.Blocks.Involvement.ExternalConnectionOpportunityDetail" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
    }
</script>

<asp:UpdatePanel ID="upnlOpportunityDetail" runat="server">
    <ContentTemplate>
        <Rock:RockLiteral ID="lResponseMessage" runat="server" Visible="false" />
        <h3>
            <asp:Literal ID="lIcon" runat="server" />
            <asp:Literal ID="lTitle" runat="server" />
        </h3>
        <asp:Panel ID="pnlDetails" runat="server">
            <div class="panel panel-block">
                <div class="panel-body">

                    <p class="description">
                        <asp:Literal ID="lDescription" runat="server"></asp:Literal>
                    </p>

                    <asp:PlaceHolder ID="phAttributes" runat="server"></asp:PlaceHolder>

                    <div class="actions">
                        <asp:LinkButton ID="btnConnect" runat="server" AccessKey="m" Text="Connect" CssClass="btn btn-primary" OnClick="btnConnect_Click" CausesValidation="false" />
                        <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                    </div>
                </div>
            </div>

        </asp:Panel>

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog ID="dlgConnectionRequest" runat="server" OnSaveClick="dlgConnectionRequest_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="Request" Title="Connect">
            <Content>
                <Rock:NotificationBox ID="nbInvalidMessage" runat="server" NotificationBoxType="Danger" />
                <Rock:NotificationBox ID="NotificationBox1" runat="server" NotificationBoxType="Danger" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbFirstName" runat="server" Label="First Name" ValidationGroup="Request" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbLastName" runat="server" Label="Last Name" ValidationGroup="Request" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:EmailBox ID="tbEmail" runat="server" Label="Email" ValidationGroup="Request" />
                    </div>
                    <div class="col-md-6">
                        <Rock:PhoneNumberBox ID="pnHome" runat="server" Label="Home Phone" ValidationGroup="Request" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:PhoneNumberBox ID="pnMobile" runat="server" Label="Mobile Phone" ValidationGroup="Request" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlCampus" runat="server" Label="Campus" ValidationGroup="Request" />
                    </div>
                </div>

                <Rock:RockTextBox ID="tbNote" runat="server" Label="Note" TextMode="MultiLine" Rows="4" ValidationGroup="Request" />

            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>
