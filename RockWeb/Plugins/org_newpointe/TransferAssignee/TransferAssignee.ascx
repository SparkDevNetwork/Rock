<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TransferAssignee.ascx.cs" Inherits="RockWeb.Plugins.org_newpointe.TransferAssignee.TransferAssignee" %>
 

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        
        <asp:Panel ID="pnlSearch" CssClass="panel panel-block" runat="server" >
            
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-user"></i> Transfer Assignees for Workflows and Connection Requests</h1>
            </div>
           
            <div class="panel-body">
                
                <div class="col-md-12">
                    
                    <Rock:NotificationBox ID="nbSuccess" runat="server" Title="Success!" Text="Workflows were transfered." NotificationBoxType="Success" visible="false" />
                    <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" Title="Warning" Visible="false" />

                    <Rock:PersonPicker runat="server" Label="From Person" ID="ppFrom" OnSelectPerson="ppFrom_OnSelect" Required="True" RequiredErrorMessage="Please select a From Person" />  
                    <asp:Label runat="server" ID="lbCount"></asp:Label><br/>
                    <br/>
                    <Rock:PersonPicker runat="server" Label="To Person" ID="ppTo" Visible="False" Required="True" RequiredErrorMessage="Please select a To Person"  />
            
                    <Rock:BootstrapButton ID="btnSaveWorkflows" runat="server" Text="Transfer Workflows" CssClass="btn btn-primary" OnClick="btnSaveWorkflows_Click" DataLoadingText="&lt;i class='fa fa-refresh fa-spin fa-2x'&gt;&lt;/i&gt; Updating Workflows" Visible="False" />
                    <Rock:BootstrapButton ID="btnSaveConnections" runat="server" Text="Transfer Connection Requests" CssClass="btn btn-info" OnClick="btnSaveConnections_Click" DataLoadingText="&lt;i class='fa fa-refresh fa-spin fa-2x'&gt;&lt;/i&gt; Updating Connections" Visible="False" />
                    

        
                    <br /><br /><br />
                    
                     <Rock:Grid ID="gWorkflows" runat="server" AllowSorting="true" DataKeyNames="Id"  >
                        <Columns>
                            <asp:BoundField DataField="Workflow.Id" HeaderText="Workflow Id" />
                            <asp:BoundField DataField="Id" HeaderText="Activity Id" />
                            <asp:BoundField DataField="Workflow.Name" HeaderText="Workflow Name" />
                            <asp:BoundField DataField="Workflow.Status" HeaderText="Workflow Status" />
                            <asp:BoundField DataField="ActivityType.Name" HeaderText="Activity" />
                        </Columns>
                    </Rock:Grid>
                    
                    <br /><br /><br />
                    
                    <Rock:Grid ID="gConnections" runat="server" AllowSorting="true" DataKeyNames="Id">
                        <Columns>
                            <asp:BoundField DataField="Id" HeaderText="Id" />
                            <asp:BoundField DataField="ConnectionOpportunity.Name" HeaderText="Opportunity" />
                            <asp:BoundField DataField="PersonAlias.Person.FullName" HeaderText="Name" />
                            <asp:BoundField DataField="ConnectionStatus.Name" HeaderText="Status" />
                            <asp:BoundField DataField="ConnectionState" HeaderText="State" />
                        </Columns>
                    </Rock:Grid>
                    

                    </div>
                
                </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>


