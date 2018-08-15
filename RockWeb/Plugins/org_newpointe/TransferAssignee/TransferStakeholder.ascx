<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TransferStakeholder.ascx.cs" Inherits="Plugins.org_newpointe.TransferAssignee.TransferStakeholder" %>
 

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        
        <asp:Panel ID="pnlSearch" CssClass="panel panel-block" runat="server" >
            
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-user"></i> Transfer Workflow Stakeholder</h1>
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
                 

        
                    <br /><br /><br />
                    
                     <Rock:Grid ID="gWorkflows" runat="server" AllowSorting="true" DataKeyNames="Id"  >
                        <Columns>

                            <asp:BoundField DataField="Id" HeaderText="Id" />
                        </Columns>
                    </Rock:Grid>
                    

                    

                    </div>
                
                </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>


