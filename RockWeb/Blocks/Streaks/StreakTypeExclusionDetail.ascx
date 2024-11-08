<%@ Control Language="C#" AutoEventWireup="true" CodeFile="StreakTypeExclusionDetail.ascx.cs" Inherits="RockWeb.Blocks.Streaks.StreakTypeExclusionDetail" %>

<asp:UpdatePanel ID="upEnrollmentDetail" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfIsEditMode" runat="server" />

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-calendar-times"></i>
                    <asp:Literal ID="lReadOnlyTitle" runat="server" />
                </h1>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">
                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                <asp:ValidationSummary ID="valEnrollmentDetail" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                <div id="pnlViewDetails" runat="server">
                    <div class="row">
                        <div class="col-lg-6">
                            <asp:Literal ID="lExclusionDescription" runat="server" />
                        </div>
                        <div class="col-lg-6">
                            <asp:Literal ID="lStreakData" runat="server" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" data-shortcut-key="e" ToolTip="Alt+e" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" CausesValidation="false" />
                        <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                        <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" CausesValidation="false" OnClick="btnDelete_Click" />
                    </div>
                </div>

                <div id="pnlEditDetails" runat="server">
                    <div class="row">
                        <div class="col-sm-6 col-md-4">
                            <Rock:LocationPicker ID="rlpLocation" runat="server" AllowedPickerModes="Named" SourceTypeName="Rock.Model.Streak, Rock" PropertyName="LocationId" Label="Location" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" data-shortcut-key="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" data-shortcut-key="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
