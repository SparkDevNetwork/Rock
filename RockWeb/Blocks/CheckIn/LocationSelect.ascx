<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LocationSelect.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.LocationSelect" %>
<asp:UpdatePanel ID="upContent" runat="server">
<ContentTemplate>

    <Rock:ModalAlert ID="maWarning" runat="server" />

    <div class="row-fluid checkin-header">
        <div class="span12">
            <h1><asp:Literal ID="lGroupName" runat="server" /></h1>
        </div>
    </div>
                
    <div class="row-fluid checkin-body">
        <div class="span12">
            <div class="control-group checkin-body-container">
                <label class="control-label">Select Location</label>
                <div class="controls">
                    <asp:Repeater ID="rSelection" runat="server" OnItemCommand="rSelection_ItemCommand">
                        <ItemTemplate>
                            <asp:LinkButton ID="lbSelect" runat="server" CommandArgument='<%# Eval("Location.Id") %>' CssClass="btn btn-primary btn-large btn-block btn-checkin-select"><%# Container.DataItem.ToString() %><%# FormatCount( (int)Eval("Location.Id") ) %></asp:LinkButton>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </div>
        </div>
    </div>
        
     <div class="row-fluid checkin-footer">   
        <div class="checkin-actions">
            <asp:LinkButton CssClass="btn btn-secondary" ID="lbBack" runat="server" OnClick="lbBack_Click" Text="Back" />
            <asp:LinkButton CssClass="btn btn-secondary" ID="lbCancel" runat="server" OnClick="lbCancel_Click" Text="Cancel" />
        </div>
    </div>

</ContentTemplate>
</asp:UpdatePanel>
