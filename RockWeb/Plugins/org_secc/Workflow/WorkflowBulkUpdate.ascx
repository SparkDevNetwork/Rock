<%@ Control Language="C#" AutoEventWireup="true" CodeFile="WorkflowBulkUpdate.ascx.cs" Inherits="RockWeb.Plugins.org_secc.WorkFlowUpdate.WorkflowBulkUpdate" %>

<asp:UpdatePanel ID="upnlSettings" runat="server">
    <ContentTemplate>
        <asp:HiddenField runat="server" ID="hfCount" />
        <asp:HiddenField runat="server" ID="hfWorkflowTypeName" />

        <asp:Panel runat="server" ID="pnlDisplay">

            <Rock:NotificationBox ID="nbNotification" runat="server" NotificationBoxType="Info" Visible="true" />

            <Rock:PanelWidget runat="server" ID="pnlAttributes" Title="Attributes">
                <asp:Repeater runat="server" ID="rAttributes">
                    <ItemTemplate>
                        <b>
                            <asp:Literal Text='<%# Eval("Name") %>' runat="server" /></b>
                        <asp:HiddenField runat="server" ID="hfAttributeKey" Value='<%# Eval("Key") %>' />
                        <asp:CheckBox Text="Update Attribute Values" runat="server" ID="cbUpdate" />
                        <Rock:RockTextBox runat="server" ID="tbValue" Label="New Value" ValidateRequestMode="Disabled"
                            Help="Text or Lava to update the workflow attribute. <span class='tip tip-lava'></span>" />
                        <hr />
                    </ItemTemplate>
                </asp:Repeater>
                <Rock:NotificationBox runat="server" NotificationBoxType="Info" Text="Attributes are calculated and applied atomically. Attribute changes take place before any workflow updates." />
            </Rock:PanelWidget>
            <Rock:PanelWidget runat="server" ID="pnlWorkflow" Title="Workflow">
                <Rock:RockDropDownList runat="server" ID="ddlActivities" Label="Activate New Activity"
                    Help="Selecting an activity will add a new activity to the workflow. If an activity is activated the workflow will also be reactivated."
                    DataTextField="Name" DataValueField="Id" />
                <Rock:RockDropDownList runat="server" ID="ddlState" Label="Completion" Help="Change the state of the workflows to active or complete.">
                    <asp:ListItem Text="" Value="" />
                    <asp:ListItem Text="Mark Workflows Complete" Value="Complete" />
                    <asp:ListItem Text="Reactivate Workflows" Value="NotComplete" />
                </Rock:RockDropDownList>
                <Rock:RockTextBox runat="server" ID="tbStatus" Label="Workflow Status" ValidateRequestMode="Disabled"
                    Help="If not blank this will update the workflow's status. <span class='tip tip-lava'></span>" />
            </Rock:PanelWidget>
            <Rock:BootstrapButton runat="server" ID="btnContinue" CssClass="btn btn-primary" OnClick="btnContinue_Click" Text="Continue" />
        </asp:Panel>

        <asp:Panel runat="server" ID="pnlConfirmation" Visible="false">
            <Rock:NotificationBox runat="server" ID="nbConfirmation" NotificationBoxType="Warning" Title="Confirm Update" />
            <Rock:BootstrapButton runat="server" CssClass="btn btn-primary" Text="Update" ID="btnUpdate" OnClick="btnUpdate_Click" />
            <asp:LinkButton Text="Cancel" ID="btnCancel" runat="server" OnClick="btnCancel_Click" />
        </asp:Panel>

        <asp:Panel runat="server" ID="pnlDone" Visible="false">
            <Rock:NotificationBox runat="server" ID="nbDone" Text="Workflows updated." />
            <Rock:BootstrapButton runat="server" ID="btnDone" CssClass="btn btn-default" Text="Done" OnClick="btnDone_Click" />
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
