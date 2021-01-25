<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonSelect.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.PersonSelect" %>

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

                <div class="control-group checkin-body-container">
                    <label class="control-label"><asp:Literal ID="lCaption" runat="server"></asp:Literal></label>
                    <asp:LinkButton CssClass="btn btn-link pull-right" ID="lbEditFamily" runat="server" OnClick="lbEditFamily_Click" Text="<i class='fa fa-pencil-alt'></i> Edit Family" />
                    <div class="controls">
                        <asp:Repeater ID="rSelection" runat="server" OnItemCommand="rSelection_ItemCommand" OnItemDataBound="rSelection_ItemDataBound">
                            <ItemTemplate>
                                <Rock:BootstrapButton ID="lbSelect" runat="server" CommandArgument='<%# Eval("Person.Id") %>' CssClass="btn btn-primary btn-large btn-block btn-checkin-select text-center" DataLoadingText="Loading..." />
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
