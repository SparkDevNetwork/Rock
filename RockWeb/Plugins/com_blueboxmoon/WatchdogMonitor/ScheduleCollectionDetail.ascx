<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ScheduleCollectionDetail.ascx.cs" Inherits="RockWeb.Plugins.com_blueboxmoon.WatchdogMonitor.ScheduleCollectionDetail" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

        <asp:Panel ID="pnlDetails" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><asp:Literal ID="lName" runat="server" /></h1>
            </div>

            <div class="panel-body">
                <fieldset>
                    <dl>
                        <dt>Schedules</dt>
                        <dd><asp:Literal ID="lSchedules" runat="server" /></dd>
                    </dl>

                    <dl>
                        <dt>Description</dt>
                        <dd><asp:Literal ID="lDescription" runat="server" /></dd>
                    </dl>
                </fieldset>

                <div class="actions">
                    <asp:LinkButton ID="lbEdit" runat="server" Text="Edit" CssClass="btn btn-primary" OnClick="lbEdit_Click" />
                </div>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlEdit" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><asp:Literal ID="lEditTitle" runat="server" /></h1>
            </div>

            <div class="panel-body">
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbEditName" runat="server" Label="Name" MaxLength="100" Required="true" ValidationGroup="Edit" />
                    </div>
                </div>

                <Rock:RockTextBox ID="tbEditDescription" runat="server" Label="Description" ValidationGroup="Description" TextMode="MultiLine" />

                <Rock:Grid ID="gComponents" runat="server" RowItemText="Schedule" OnRowSelected="gComponents_RowSelected" OnGridRebind="gComponents_GridRebind">
                    <Columns>
                        <asp:BoundField DataField="ScheduleText" HeaderText="Schedule" />
                        <Rock:DeleteField OnClick="gComponentsDelete_Click" />
                    </Columns>
                </Rock:Grid>

                <div class="actions margin-t-md">
                    <asp:LinkButton ID="lbSave" runat="server" Text="Save" CssClass="btn btn-primary" ValidationGroup="Edit" OnClick="lbSave_Click" />
                    <asp:LinkButton ID="lbCancel" runat="server" Text="Cancel" CssClass="btn btn-link" ValidationGroup="Edit" CausesValidation="false" OnClick="lbCancel_Click" />
                </div>
            </div>
        </asp:Panel>

        <Rock:ModalDialog ID="mdlComponentEdit" runat="server" OnSaveClick="mdlEdit_SaveClick" ValidationGroup="ComponentEdit">
            <Content>
                <asp:HiddenField ID="hfRowId" runat="server" />
                <Rock:NotificationBox ID="nbComponentEditError" runat="server" NotificationBoxType="Danger" />
                <asp:PlaceHolder ID="phContents" runat="server" />
                <asp:LinkButton ID="lbEditCancel" runat="server" OnClick="lbEditCancel_Click" CssClass="hidden" Text="Cancel" ValidationGroup="ComponentEdit" CausesValidation="false" />
            </Content>
        </Rock:ModalDialog>
    </ContentTemplate>
</asp:UpdatePanel>
