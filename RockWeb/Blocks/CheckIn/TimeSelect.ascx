<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TimeSelect.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.TimeSelect" %>
<asp:UpdatePanel ID="upContent" runat="server">
<ContentTemplate>

    <Rock:ModalAlert ID="maWarning" runat="server" />

    <div class="checkin-header">
        <h1><asp:Literal ID="lTitle" runat="server" /></h1>
    </div>

    <div class="checkin-body">

        <div class="checkin-scroll-panel">
            <div class="scroller">

                <div class="control-group checkin-time-select" style="margin: 0 auto">
                    <label class="control-label"><asp:Literal ID="lCaption" runat="server" /></label>
                    <div class="controls checkin-timelist btn-group" data-toggle="buttons-checkbox">
                        <asp:Repeater ID="rSelection" runat="server">
                            <ItemTemplate>
                                <button type="button" schedule-id='<%# Eval("Schedule.Id") %>' class='<%# "btn btn-default btn-lg btn-checkbox" + ((bool)Eval("PreSelected") ? " active" : "") %>'>
                                    <i class="fa fa-square-o"></i>

                                    <div><%# Container.DataItem.ToString() %></div>
                                </button>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>

                </div>
                <asp:HiddenField ID="hfTimes" runat="server"></asp:HiddenField>

            </div>
        </div>
    </div>



    <div class="checkin-footer">
        <div class="checkin-actions">
            <asp:LinkButton CssClass="btn btn-primary" ID="lbSelect" runat="server" OnClientClick="return GetTimeSelection();" OnClick="lbSelect_Click" />
            <asp:LinkButton CssClass="btn btn-default btn-back" ID="lbBack" runat="server" OnClick="lbBack_Click" Text="Back" />
            <asp:LinkButton CssClass="btn btn-default btn-cancel" ID="lbCancel" runat="server" OnClick="lbCancel_Click" Text="Cancel" />
        </div>
    </div>

</ContentTemplate>
</asp:UpdatePanel>
