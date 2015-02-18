<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ResidentGradeRequest.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.Residency.ResidentGradeRequest" %>

<asp:UpdatePanel ID="updatePanel" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfCompetencyPersonProjectId" runat="server" />

        <asp:Panel ID="pnlGraderLogin" CssClass="panel panel-block" runat="server">

                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-lock"></i> <asp:Literal ID="lFacilitatorLoginTitle" runat="server" /></h1>
                </div>
                <div class="panel-body">

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

        <asp:Panel ID="pnlEmailRequest" CssClass="panel panel-block" runat="server">
            
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-envelope"></i> <asp:Literal ID="lblEmailRequestTitle" runat="server" /></h1>
            </div>
            <div class="panel-body">
                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="vgEmailRequest" />

                <Rock:RockDropDownList ID="ddlFacilitators" runat="server" Required="true" ValidationGroup="vgEmailRequest" Label="Facilitator" />

                <asp:Button ID="btnSendRequest" runat="server" Text="Send" CssClass="btn btn-primary" OnClick="btnSendRequest_Click" ValidationGroup="vgEmailRequest" />

                <Rock:NotificationBox ID="nbSendMessage" ClientIDMode="Static" runat="server" NotificationBoxType="Info" />
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
