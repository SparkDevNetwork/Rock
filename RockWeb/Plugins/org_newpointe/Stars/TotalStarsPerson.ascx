<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TotalStarsPerson.ascx.cs" Inherits="RockWeb.Plugins.org_newpointe.Stars.TotalStarsPerson" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        
        <asp:Panel ID="pnlSearch" CssClass="panel panel-block" runat="server" >
            
                        <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-star"></i> Stars for <%= Person.FullName %></h1>
            </div>
           
            <div class="panel-body">
                
                <div class="col-md-12">
                             <span style="font-size: 200%">
                    <Rock:HighlightLabel runat="server" Id="hlStarsThisMonth" IconCssClass="fa fa-star" LabelType="Primary" />
                    <Rock:HighlightLabel runat="server" Id="hlStarsLastMonth" IconCssClass="fa fa-star" LabelType="Warning" />
                    <Rock:HighlightLabel runat="server" Id="hlStarsTotal" IconCssClass="fa fa-star" LabelType="Success" />
                                 </span>
                      

                  </div>  </div>
            </asp:Panel>
        
        
        


    </ContentTemplate>
</asp:UpdatePanel>
