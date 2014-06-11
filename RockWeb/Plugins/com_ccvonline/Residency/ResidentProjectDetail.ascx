<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ResidentProjectDetail.ascx.cs" Inherits="RockWeb.Plugins.com_ccvonline.Residency.ResidentProjectDetail" %>

<asp:UpdatePanel ID="upCompetencyPersonProjectDetail" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">

            <asp:HiddenField ID="hfCompetencyPersonProjectId" runat="server" />

            <div class="banner">
                <h1>
                    <asp:Literal ID="lReadOnlyTitle" runat="server" />
                </h1>
            </div>

            <fieldset id="fieldsetViewDetails" runat="server">
                <div >
                    <Rock:NotificationBox ID="nbWarningMessage" ClientIDMode="Static" runat="server" NotificationBoxType="Warning" />
                    <asp:LinkButton ID="btnGrade" runat="server" Text="Grade" CssClass="btn btn-primary pull-right" OnClick="btnGrade_Click" />

                    <div class="row">
                        <div class="col-md-12">
                            <asp:Literal ID="lblMainDetails" runat="server" />
                        </div>
                    </div>
                </div>
            </fieldset>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
