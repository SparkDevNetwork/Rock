<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ResidentProjectDetail.ascx.cs" Inherits="RockWeb.Plugins.com_ccvonline.Residency.ResidentProjectDetail" %>

<!-- just to help do css intellisense at design time  -->
<link rel="stylesheet" type="text/css" href="~/CSS/bootstrap.min.css" visible="false" />

<asp:UpdatePanel ID="upCompetencyPersonProjectDetail" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">

            <asp:HiddenField ID="hfCompetencyPersonProjectId" runat="server" />

            <fieldset id="fieldsetViewDetails" runat="server">
                <div class="well">
                    <Rock:NotificationBox ID="nbWarningMessage" ClientIDMode="Static" runat="server" NotificationBoxType="Warning" />
                    <asp:LinkButton ID="btnGrade" runat="server" Text="Grade" CssClass="btn btn-primary pull-right" OnClick="btnGrade_Click" />

                    <div class="row-fluid">
                        <asp:Literal ID="lblMainDetails" runat="server" />
                    </div>
                </div>
            </fieldset>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
