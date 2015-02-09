<%@ Control Language="C#" AutoEventWireup="true" CodeFile="WorkflowList.ascx.cs" Inherits="RockWeb.Blocks.WorkFlow.WorkflowList" %>

<asp:UpdatePanel ID="upnlSettings" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlWorkflowList" CssClass="panel panel-block" runat="server">

            <div class="panel-heading">
                <h1 class="panel-title"><asp:Literal ID="lHeadingIcon" runat="server" ><i class="fa fa-list"></i></asp:Literal> <asp:Literal ID="lGridTitle" runat="server" Text="Workflows" /></h1>
            </div>
            <div class="panel-body">

	            <Rock:ModalAlert ID="mdGridWarning" runat="server" />

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
	                        <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
	                        <Rock:RockBoundField DataField="Initiator" HeaderText="Initiated By" SortExpression="Initiator" />
                            <Rock:RockBoundField DataField="Activities" HeaderText="Activities" HtmlEncode="false" />
	                    </Columns>
    	            </Rock:Grid>
                </div>

            </div> 

            

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
