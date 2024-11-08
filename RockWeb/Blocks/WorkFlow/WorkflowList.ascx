<%@ Control Language="C#" AutoEventWireup="true" CodeFile="WorkflowList.ascx.cs" Inherits="RockWeb.Blocks.WorkFlow.WorkflowList" %>

<asp:UpdatePanel ID="upnlSettings" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbMessage" runat="server" NotificationBoxType="Warning" Visible="false" />

        <asp:Panel ID="pnlWorkflowList" CssClass="panel panel-block" runat="server">

            <div class="panel-heading">
                <h1 class="panel-title"><asp:Literal ID="lHeadingIcon" runat="server" ><i class="fa fa-list"></i></asp:Literal> <asp:Literal ID="lGridTitle" runat="server" Text="Workflows" /></h1>
            </div>
            <div class="panel-body">

	            <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                <Rock:NotificationBox ID="nbNoOccurrencesSelected" runat="server" NotificationBoxType="Warning" Text="Please select at least one occurrence to accept." Visible="false" />
                <div class="grid grid-panel">
            	    <Rock:GridFilter ID="gfWorkflows" runat="server">
                	    <Rock:RockTextBox ID="tbName" runat="server" Label="Name"></Rock:RockTextBox>
                	    <Rock:PersonPicker ID="ppInitiator" runat="server" Label="Initiator" />
                	    <Rock:RockTextBox ID="tbStatus" runat="server" Label="Status Text"></Rock:RockTextBox>
                	    <Rock:DateRangePicker ID="drpActivated" runat="server" Label="Activated" />
	                    <Rock:DateRangePicker ID="drpCompleted" runat="server" Label="Completed" />
                        <Rock:RockCheckBoxList ID="cblState" runat="server" Label="State" RepeatDirection="Horizontal">
                            <asp:ListItem Selected="True" Text="Active" Value="Active" />
                            <asp:ListItem Selected="True" Text="Completed" Value="Completed" />
                        </Rock:RockCheckBoxList>
                        <asp:PlaceHolder ID="phAttributeFilters" runat="server" />
	                </Rock:GridFilter>
	                <Rock:Grid ID="gWorkflows" runat="server" AllowSorting="true" DisplayType="Full" OnRowSelected="gWorkflows_Edit">
	                    <Columns>
                            <Rock:SelectField />
	                        <Rock:RockBoundField DataField="WorkflowId" HeaderText="Id" SortExpression="WorkflowId" />
	                        <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
	                        <Rock:PersonField DataField="Initiator" HeaderText="Initiated By" SortExpression="Initiator" />
                            <Rock:ListDelimitedField DataField="Activities" HeaderText="Activities" HtmlEncode="false" Delimiter="," />
	                    </Columns>
    	            </Rock:Grid>
                    <Rock:NotificationBox ID="nbResult" runat="server" Visible="false" CssClass="margin-b-none" Dismissable="true"></Rock:NotificationBox>
                </div>
            </div> 
        </asp:Panel>
        <Rock:ModalDialog ID="mdAlert" runat="server" Title="Information" SaveButtonText="OK" SaveButtonCausesValidation="false" OnSaveClick="mdAlert_OkClick" SaveButtonCssClass="btn btn-primary" CancelLinkVisible="false" CloseLinkVisible="false" Visible="false">
            <Content>
                <p>The Workflow items are scheduled to be deleted.</p>
            </Content>
        </Rock:ModalDialog>
    </ContentTemplate>
</asp:UpdatePanel>
