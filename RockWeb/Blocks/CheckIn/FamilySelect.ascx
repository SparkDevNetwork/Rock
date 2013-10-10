<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FamilySelect.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.FamilySelect" %>
<asp:UpdatePanel ID="upContent" runat="server">
<ContentTemplate>

    <Rock:ModalAlert ID="maWarning" runat="server" />

    <div class="row checkin-header">
        <div class="col-md-12">
            <h1>Families</h1>
        </div>
    </div>
                
    <div class="row checkin-body">
        <div class="col-md-12">
            <div class="control-group checkin-body-container">
                <label class="control-label">Select Your Family</label>
                <div class="controls">
                    <asp:Repeater ID="rSelection" runat="server" OnItemCommand="rSelection_ItemCommand">
                        <ItemTemplate>
                            <asp:LinkButton ID="lbSelect" runat="server" CommandArgument='<%# Eval("Group.Id") %>' CssClass="btn btn-primary btn-large btn-block btn-checkin-select"  OnClientClick="$(this).button('loading')"  ><%# Eval("Caption") %><span class="checkin-sub-title"><%# Eval("SubCaption") %></span></asp:LinkButton>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </div>
        </div>
    </div>


     <div class="checkin-footer">   
        <div class="checkin-actions">
            <asp:LinkButton CssClass="btn btn-default" ID="lbBack" runat="server" OnClick="lbBack_Click" Text="Back" />
            <asp:LinkButton CssClass="btn btn-default" ID="lbCancel" runat="server" OnClick="lbCancel_Click" Text="Cancel" />
        </div>
    </div>

</ContentTemplate>
</asp:UpdatePanel>
