<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TimeSelect.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.TimeSelect" %>
<asp:UpdatePanel ID="upContent" runat="server">
<ContentTemplate>

    <Rock:ModalAlert ID="maWarning" runat="server" />

    <fieldset>
    <legend><asp:Literal ID="lTitle" runat="server" /></legend>

        <div class="control-group">
            <label class="control-label">Select Time(s)</label>
            <div class="controls btn-group time-select" data-toggle="buttons-checkbox">
                <asp:Repeater ID="rSelection" runat="server">
                    <ItemTemplate>
                        <button type="button" schedule-id='<%# Eval("Schedule.Id") %>' class="btn btn-primary btn-large"><%# Container.DataItem.ToString() %></button>
                    </ItemTemplate>
                </asp:Repeater>
                <asp:HiddenField ID="hfTimes" runat="server"></asp:HiddenField>
            </div>
        </div>
        
    </fieldset>

    <div class="actions">
        <asp:LinkButton CssClass="btn btn-primary" ID="lbSelect" runat="server" OnClientClick="return GetTimeSelection();" OnClick="lbSelect_Click" Text="Select" />
        <asp:LinkButton CssClass="btn btn-secondary" ID="lbBack" runat="server" OnClick="lbBack_Click" Text="Back" />
        <asp:LinkButton CssClass="btn btn-secondary" ID="lbCancel" runat="server" OnClick="lbCancel_Click" Text="Cancel" />
    </div>

</ContentTemplate>
</asp:UpdatePanel>
