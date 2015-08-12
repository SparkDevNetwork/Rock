<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AccountabilityGroupTypeDetail.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Accountability.AccountabilityGroupTypeDetail" %>

<asp:UpdatePanel ID="upnlGroupList" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" runat="server">
            <asp:HiddenField ID="hfGroupTypeId" runat="server" />

            <div class="panel panel-block">

                <div class="panel-heading clearfix">
                    <h1 class="panel-title pull-left">
                        <asp:Literal ID="lReadOnlyTitle" runat="server" />
                    </h1>

                </div>

                <div class="panel-body">
                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                    <div id="pnlEditDetails" runat="server">

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:DataTextBox ID="dtbName" runat="server" SourceTypeName="Rock.Model.Group, Rock" PropertyName="Name" />
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-12">
                                <Rock:DataTextBox ID="dtbDescription" runat="server" SourceTypeName="Rock.Model.Group, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />
                            </div>
                        </div>
                  
                        <div class="actions">
                            <asp:LinkButton ID="lbSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="lbSave_Click" />
                            <asp:LinkButton ID="lbCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="lbCancel_Click" />
                        </div>

                    </div>

                    <fieldset id="fieldsetViewDetails" runat="server">

                        <p class="description">
                            <asp:Literal ID="lGroupDescription" runat="server"></asp:Literal>
                        </p>

                        <div class="actions">
                            <asp:LinkButton ID="lbEdit" runat="server" Text="Edit" CssClass="btn btn-primary" OnClick="lbEdit_Click" CausesValidation="false" />
                            <Rock:ModalAlert ID="maDeleteWarning" runat="server" />
                            <asp:LinkButton ID="lbDelete" runat="server" Text="Delete" CssClass="btn btn-link" OnClick="lbDelete_Click" CausesValidation="false" />
                        </div>

                    </fieldset>
                </div>
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
