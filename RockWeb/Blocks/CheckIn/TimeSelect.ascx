<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TimeSelect.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.TimeSelect" %>
<asp:UpdatePanel ID="upContent" runat="server">
<ContentTemplate>

    <Rock:ModalAlert ID="maWarning" runat="server" />

    <div class="row checkin-header">
        <div class="col-md-12">
            <h1><asp:Literal ID="lTitle" runat="server" /><div class="checkin-sub-title"><asp:Literal ID="lSubTitle" runat="server"></asp:Literal></div></h1>
        </div>
    </div>
                
    <div class="row checkin-body">
        <div class="col-md-12">

            <div class="control-group checkin-time-select" style="margin: 0 auto">
                <h1>Select Time(s)</h1>
                <div class="controls checkin-timelist btn-group" data-toggle="buttons-checkbox">
                    <asp:Repeater ID="rSelection" runat="server">
                        <ItemTemplate>
                            <button type="button" schedule-id='<%# Eval("Schedule.Id") %>' class="btn btn-default btn-lg btn-checkbox">
                                <i class="icon-check-empty"></i>
                                
                                <div><%# Container.DataItem.ToString() %></div>
                            </button>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>

                <div class="checkin-actions">
                    <asp:LinkButton CssClass="btn btn-primary" ID="lbSelect" runat="server" OnClientClick="return GetTimeSelection();" OnClick="lbSelect_Click" Text="Check-in" />
                </div>

            </div>
            <asp:HiddenField ID="hfTimes" runat="server"></asp:HiddenField>
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
