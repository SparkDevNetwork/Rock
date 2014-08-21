<%@ Control Language="C#" AutoEventWireup="true" CodeFile="WorkflowList.ascx.cs" Inherits="RockWeb.Blocks.WorkFlow.WorkflowList" %>

<asp:UpdatePanel ID="upnlSettings" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlWorkflowList" CssClass="panel panel-block" runat="server">

            <div class="panel-heading">
                <h1 class="panel-title"><asp:Literal ID="lHeadingIcon" runat="server" ><i class="fa fa-list"></i></asp:Literal> <asp:Literal ID="lGridTitle" runat="server" Text="Workflows" /></h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
            	    <Rock:GridFilter ID="gfWorkflows" runat="server">
                	    <Rock:RockTextBox ID="tbName" runat="server" Label="Name"></Rock:RockTextBox>
                	    <Rock:PersonPicker ID="ppInitiator" runat="server" Label="Initiator" />
                	    <Rock:RockTextBox ID="tbStatus" runat="server" Label="Status Text"></Rock:RockTextBox>
                	    <Rock:RockDropDownList ID="ddlState" runat="server" Label="State">
                    	    <asp:ListItem Text="All" Value="" />
                    	    <asp:ListItem Text="Active" Value="0" />
                   	     <asp:ListItem Text="Completed" Value="1" />
                 	    </Rock:RockDropDownList>
                	    <Rock:DateRangePicker ID="drpActivated" runat="server" Label="Activated" />
	                    <Rock:DateRangePicker ID="drpCompleted" runat="server" Label="Completed" />
                        <Rock:RockCheckBoxList ID="cblActiveStatus" runat="server" Label="Show Active Status" RepeatDirection="Horizontal">
                            <asp:ListItem Selected="True" Text="Active" Value="Active" />
                            <asp:ListItem Selected="True" Text="Inactive" Value="Inactive" />
                        </Rock:RockCheckBoxList>
	                </Rock:GridFilter>

	                <Rock:ModalAlert ID="mdGridWarning" runat="server" />

	                <Rock:Grid ID="gWorkflows" runat="server" AllowSorting="true" DisplayType="Full" OnRowSelected="gWorkflows_Edit">
	                    <Columns>
	                        <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
	                        <asp:BoundField DataField="Initiator" HeaderText="Initiated By" SortExpression="InitiatorPersonAlias.Person.FullName" />
                            <asp:BoundField DataField="Activities" HeaderText="Activities" />
	                    </Columns>
    	            </Rock:Grid>
                </div>

            </div> 

            

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
