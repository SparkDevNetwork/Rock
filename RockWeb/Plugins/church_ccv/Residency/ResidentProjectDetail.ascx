<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ResidentProjectDetail.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.Residency.ResidentProjectDetail" %>

<asp:UpdatePanel ID="upCompetencyPersonProjectDetail" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">

            <asp:HiddenField ID="hfCompetencyPersonProjectId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-folder-open-o"></i> <asp:Literal ID="lReadOnlyTitle" runat="server" /></h1>
            </div>
            <div class="panel-body">
                <fieldset id="fieldsetViewDetails" runat="server">
                    <div >
                        <Rock:NotificationBox ID="nbWarningMessage" ClientIDMode="Static" runat="server" NotificationBoxType="Warning" />
                        <div class="row">
                            <div class="col-md-12">
                                <asp:LinkButton ID="btnGrade" runat="server" Text="Grade" CssClass="btn btn-primary pull-right" OnClick="btnGrade_Click" />
                                <asp:Literal ID="lblMainDetails" runat="server" />
                            </div>
                        </div>
                    </div>
                </fieldset>
            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
