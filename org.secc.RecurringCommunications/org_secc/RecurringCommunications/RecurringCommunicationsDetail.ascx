<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RecurringCommunicationsDetail.ascx.cs" Inherits="RockWeb.Plugins.org_secc.RecurringCommunications.RecurringCommunicationsDetail" %>
<asp:UpdatePanel runat="server" ID="upContent">
    <ContentTemplate>
        <div class="panel panel-default">
            <div class="panel-heading">
                <h3 class="panel-title">Recurring Communication</h3>
            </div>
            <asp:Panel runat="server" ID="pnlMain">
                <div class="panel-body">
                    <Rock:RockTextBox runat="server" ID="tbName" Label="Communication Name" Required="true" />
                    <Rock:DataViewItemPicker runat="server" ID="dvpDataview" Label="DataView" Required="true" />
                    <Rock:ScheduleBuilder runat="server" ID="sbScheduleBuilder" Label="Schedule" OnSaveSchedule="sbScheduleBuilder_SaveSchedule" />
                    <Rock:RockLiteral runat="server" ID="lScheduleDescription" Label="Schedule Description" Visible="false" />
                    <Rock:RockRadioButtonList runat="server" ID="rblCommunicationType" Label="Communication Type"
                        AutoPostBack="true" OnSelectedIndexChanged="rblCommunicationType_SelectedIndexChanged"
                        DataValueField="Key" DataTextField="Value" />
                    <Rock:RockDropDownList ID="ddlTransformTypes" runat="server" Label="Recipient Transformation" />

                    
                    <asp:Panel ID="pnlEmail" runat="server" CssClass="well form-well">
                        <fieldset>
                            <legend>Email Details</legend>
                            <Rock:RockTextBox runat="server" ID="tbFromName" Label="From Name" Required="true" />
                            <Rock:EmailBox runat="server" ID="tbFromEmail" Label="From Email" Required="true" />
                            <Rock:RockTextBox runat="server" ID="tbSubject" Label="Subject" Required="true" />
                            <Rock:HtmlEditor runat="server" ID="ceEmailBody" Label="Email Body" Required="true" Height="400" />

                        </fieldset>
                    </asp:Panel>
                    <asp:Panel runat="server" ID="pnlSMS" CssClass="well form-well">
                        <fieldset>
                            <legend>SMS Details</legend>
                            <Rock:DefinedValuePicker runat="server" ID="dvpPhoneNumber" Label="From Number" />
                            <Rock:RockTextBox runat="server" ID="tbSMSBody" Label="Text Message" TextMode="MultiLine"
                                Height="200" />
                        </fieldset>
                    </asp:Panel>
                    <asp:Panel runat="server" ID="pnlPushNofitication" CssClass="well form-well">
                        <fieldset>
                            <legend>Push Notification Details</legend>
                            <Rock:RockTextBox runat="server" ID="tbPushNotificationTitle" Label="Notification Title" />
                            <Rock:RockCheckBox runat="server" ID="cbPlaySound" Label="Play Sound" />
                            <Rock:RockTextBox runat="server" ID="tbPushNotificationBody" Label="Notification Message" TextMode="MultiLine"
                                Height="200" />
                    </asp:Panel>
                    </fieldset>
                    <Rock:BootstrapButton runat="server" ID="btnSave" CssClass="btn btn-primary" Text="Save" OnClick="btnSave_Click" />
                    <asp:LinkButton Text="Cancel" ID="btnCancel" runat="server" OnClick="btnCancel_Click" CausesValidation="false" />
                </div>
            </asp:Panel>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
