<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonDetail.ascx.cs" Inherits="RockWeb.Plugins.com_ccvonline.Residency.PersonDetail" %>

<asp:UpdatePanel ID="upResidencyPersonDetail" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">

            <asp:HiddenField ID="hfGroupId" runat="server" />
            <asp:HiddenField ID="hfPersonId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-user"></i> <asp:Literal ID="lReadOnlyTitle" runat="server" /></h1>
            </div>
            <div class="panel-body">

                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                <Rock:NotificationBox ID="nbWarning" runat="server" NotificationBoxType="Warning" />
                <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                <div id="pnlEditDetails" runat="server">

                    <div class="row">
                        <div class="col-md-12">

                            <Rock:PersonPicker ID="ppPerson" runat="server" Label="Select Resident" Required="true" />

                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>

                </div>

            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
