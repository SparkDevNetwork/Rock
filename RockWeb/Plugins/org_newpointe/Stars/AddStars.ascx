<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AddStars.ascx.cs" Inherits="RockWeb.Plugins.org_newpointe.Stars.AddStars" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        
        <asp:Panel ID="pnlSearch" CssClass="panel panel-block" runat="server" >
            
                        <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-user"></i> Add Stars</h1>
            </div>
           
            <div class="panel-body">
                
                <div class="col-md-12">
        
      <!--  <Rock:PersonPicker runat="server" ID="ppPerson" Required="False" Label="Person"/> -->
                    
                    <Rock:RockDropDownList runat="server" ID="ddlStars" Help="Select what the stars are for" Label="Stars" DataValueField="Id"/>
                    
                  <!--  <Rock:RockTextBox runat="server" ID="tbValue" CssClass="input-width-md" Label="Stars"/> -->
       
             <Rock:BootstrapButton runat="server" OnClick="btnSaveStars_OnClick" Text="Save" CssClass="btn btn-primary"></Rock:BootstrapButton>

                </div>  </div>
            </asp:Panel>
        
    </ContentTemplate>
</asp:UpdatePanel>
