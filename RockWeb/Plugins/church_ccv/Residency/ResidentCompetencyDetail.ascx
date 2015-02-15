<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ResidentCompetencyDetail.ascx.cs" Inherits="RockWeb.Plugins.com_ccvonline.Residency.ResidentCompetencyDetail" %>

<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">

            <asp:HiddenField ID="hfCompetencyPersonId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-code-fork"></i> <asp:Literal ID="lReadOnlyTitle" runat="server" /></h1>
            </div>
            <div class="panel-body">
                <div class="row">
                    <div class="col-md-6">
                        <dl>
                            <dt>Facilitator</dt>
                            <dd>
                                <asp:Literal runat="server" ID="lblFacilitator" /></dd>
                        </dl>
                    </div>

                    <div class="col-md-6">
                        <dl>
                            <dt>Description</dt>
                            <dd>
                                <asp:Literal runat="server" ID="lblDescription" />
                            </dd>
                        </dl>
                    </div>
                </div>
            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
