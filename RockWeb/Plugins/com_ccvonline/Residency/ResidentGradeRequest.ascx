<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ResidentGradeRequest.ascx.cs" Inherits="RockWeb.Plugins.com_ccvonline.Residency.ResidentGradeRequest" %>

<asp:UpdatePanel ID="updatePanel" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfCompetencyPersonProjectId" runat="server" />

        <asp:Panel ID="pnlGraderLogin" runat="server">
            <div class="panel panel-default">
                <div class="panel-body">

                    <div class="banner">
                        <h1>
                            <asp:Literal ID="lFacilitatorLoginTitle" runat="server" />
                        </h1>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="vgLogin" />

                            <Rock:NotificationBox ID="nbLoginInstructions" runat="server" Text="The teacher of this competency or an authorized grader must login to grade this project." NotificationBoxType="Info" />

                            <Rock:RockTextBox ID="tbUserName" runat="server" Label="Username" Required="true" DisplayRequiredIndicator="false" ValidationGroup="vgLogin" />
                            <Rock:RockTextBox ID="tbPassword" runat="server" Label="Password" Required="true" DisplayRequiredIndicator="false" TextMode="Password" AutoCompleteType="Disabled" ValidationGroup="vgLogin" />

                            <Rock:NotificationBox ID="nbWarningMessage" ClientIDMode="Static" runat="server" NotificationBoxType="Warning" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:Button ID="btnLogin" runat="server" Text="Login" CssClass="btn btn-primary" OnClick="btnLogin_Click" ValidationGroup="vgLogin" />
                    </div>
                </div>
            </div>

        </asp:Panel>

        <asp:Panel ID="pnlEmailRequest" runat="server">
            <div class="banner">
                <h1>
                    <asp:Literal ID="lblEmailRequestTitle" runat="server" />
                </h1>
            </div>

            <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="vgEmailRequest" />

            <Rock:RockDropDownList ID="ddlFacilitators" runat="server" Required="true" ValidationGroup="vgEmailRequest" Label="Facilitator" />

            <asp:Button ID="btnSendRequest" runat="server" Text="Send" CssClass="btn btn-primary" OnClick="btnSendRequest_Click" ValidationGroup="vgEmailRequest" />

            <Rock:NotificationBox ID="nbSendMessage" ClientIDMode="Static" runat="server" NotificationBoxType="Info" />
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
