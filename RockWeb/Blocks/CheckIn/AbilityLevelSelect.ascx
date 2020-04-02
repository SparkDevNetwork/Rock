<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AbilityLevelSelect.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.AbilityLevelSelect" %>

<script type="text/javascript">
    Sys.Application.add_load(function () {
        $('a.btn-checkin-select').on('click', function () {
            $(this).siblings().attr('onclick', 'return false;');
        });
    });
</script>

<asp:UpdatePanel ID="upContent" runat="server">
<ContentTemplate>

    <Rock:ModalAlert ID="maWarning" runat="server" />


    <div class="checkin-header">
        <h1><asp:Literal ID="lTitle" runat="server"></asp:Literal></h1>
    </div>

    <div class="checkin-body">

        <div class="checkin-scroll-panel">
            <div class="scroller">

                <asp:Panel ID="pnlNoOptions" runat="server" Visible="false">
                    <h1><asp:Literal ID="lNoOptionTitle" runat="server" /></h1>
                    <h4><asp:Literal ID="lNoOptionCaption" runat="server" /></h4>
                    <div class="actions">
                        <asp:LinkButton CssClass="btn btn-primary btn-checkin" ID="btnNoOptionOk" runat="server" OnClick="btnNoOptionOk_Click" Text="Ok" />
                    </div>
                </asp:Panel>

                <div id="divAbilityLevel" runat="server" class="control-group checkin-body-container">
                    <label class="control-label"><asp:Literal ID="lCaption" runat="server" /></label>
                    <div class="controls">
                        <asp:Repeater ID="rSelection" runat="server" OnItemCommand="rSelection_ItemCommand" OnItemDataBound="rSelection_ItemDataBound">
                            <ItemTemplate>
                                <Rock:BootstrapButton
                                    ID="lbSelect"
                                    runat="server"
                                    Text='<%# Container.DataItem.ToString() %>'
                                    CommandArgument='<%# Eval("Guid").ToString().ToUpper() %>'
                                    CssClass="btn btn-primary btn-large btn-block btn-checkin-select"
                                    DataLoadingText="Loading..." />
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </div>

            </div>
        </div>

    </div>



    <div class="checkin-footer">
        <div class="checkin-actions">
            <asp:LinkButton CssClass="btn btn-default btn-back" ID="lbBack" runat="server" OnClick="lbBack_Click" Text="Back" />
            <asp:LinkButton CssClass="btn btn-default btn-cancel" ID="lbCancel" runat="server" OnClick="lbCancel_Click" Text="Cancel" />
        </div>
    </div>

</ContentTemplate>
</asp:UpdatePanel>
