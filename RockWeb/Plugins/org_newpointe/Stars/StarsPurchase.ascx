<%@ Control Language="C#" AutoEventWireup="true" CodeFile="StarsPurchase.ascx.cs" Inherits="RockWeb.Plugins.org_newpointe.Stars.StarsPurchase" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        
        <asp:Panel ID="pnlSearch" CssClass="panel panel-block" runat="server" >
            
                        <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-user"></i> Stars Purchase</h1>
            </div>
           
            <div class="panel-body">
                
                <div class="col-md-12">
        
        <Rock:PersonPicker runat="server" ID="ppPerson" Required="False" Label="Person"/>
                    
                    <Rock:RockTextBox runat="server" ID="tbValue" CssClass="input-width-md" Label="Stars"/>
       
             <Rock:BootstrapButton runat="server" OnClick="btnSaveStars_OnClick" Text="save" CssClass="btn btn-primary"></Rock:BootstrapButton>

                  
                    
                    
                    <Rock:Grid ID="gStars" runat="server" AllowSorting="true" AllowPaging="True" EmptyDataText="No Data Found" RowClickEnabled="True" OnRowSelected="gStars_OnRowSelected" >
                        <Columns>
                            <asp:BoundField DataField="Person" HeaderText="Person" />
                            <asp:BoundField DataField="Sum" HeaderText="Stars" />
                        </Columns>
                    </Rock:Grid>
                    
                      

                  </div>  </div>
            </asp:Panel>
        
        
        


    </ContentTemplate>
</asp:UpdatePanel>
