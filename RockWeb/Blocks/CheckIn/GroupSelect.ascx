<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupSelect.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.GroupSelect" %>

<script type="text/javascript">
    Sys.Application.add_load(function () {
        $('a.btn-checkin-select').click(function () {
            $(this).siblings().attr('onclick', 'return false;');
        });
    });
</script>

<asp:UpdatePanel ID="upContent" runat="server">
<ContentTemplate>

    <Rock:ModalAlert ID="maWarning" runat="server" />

    <div class="checkin-header">
        <h1><asp:Literal ID="lTitle" runat="server" /><div class="checkin-sub-title"><asp:Literal ID="lSubTitle" runat="server"></asp:Literal></div></h1>
    </div>
                
    <div class="checkin-body">

        <div class="checkin-scroll-panel">
            <div class="scroller">

                <div class="control-group checkin-body-container">
                    <label class="control-label">Select Group</label>
                    <div class="controls">
                        <asp:Repeater ID="rSelection" runat="server" OnItemCommand="rSelection_ItemCommand">
                            <ItemTemplate>
                                <Rock:BootstrapButton ID="lbSelect" runat="server" Text='<%# Container.DataItem.ToString() %>' CommandArgument='<%# Eval("Group.Id") %>' CssClass="btn btn-primary btn-large btn-block btn-checkin-select" DataLoadingText="Loading..." />
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </div>

            </div>
        </div>

    </div>        

     <div class="row-fluid checkin-footer">   
        <div class="checkin-actions">
            <asp:LinkButton CssClass="btn btn-default" ID="lbBack" runat="server" OnClick="lbBack_Click" Text="Back" />
            <asp:LinkButton CssClass="btn btn-default" ID="lbCancel" runat="server" OnClick="lbCancel_Click" Text="Cancel" />
        </div>
    </div>

</ContentTemplate>
</asp:UpdatePanel>
