﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AbilityLevelSelect.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.AbilityLevelSelect" %>

<script type="text/javascript">
    Sys.Application.add_load(function () {
        $('a.btn-checkin-select').click(function () {
            $(this).siblings().attr('onclick', 'return false;');
        });
    });
</script>

<asp:UpdatePanel ID="upContent" runat="server">
<ContentTemplate>

    <script>

        /**
        * Technique to prevent clicking multiple times on a submit button.
        * Attempting to prevent multiple postbacks.
        * @param {DOM Object} el The DOM element that fired the event.
        * @returns {bool}
        */
        function disableButton(el) {
            el.className += " disabled";
            if (el.getAttribute('requestSent') !== 'true') {
                el.setAttribute('requestSent', 'true');
                $(el).append(' <i class="fa fa-refresh fa-spin"></i> ');
                return true;
            } else {
                el.disabled = true;
                return false;
            }
        }

    </script>

    <Rock:ModalAlert ID="maWarning" runat="server" />


    <div class="checkin-header">
        <h1><asp:Literal ID="lPersonName" runat="server"></asp:Literal></h1>
    </div>
                
    <div class="checkin-body">

        <div class="checkin-scroll-panel">
            <div class="scroller">

                <asp:Panel ID="pnlNoOptions" runat="server" Visible="false">
                    <h1>Sorry</h1>
                    <h4>There are currently not any available locations that the selected individual(s) can check into.</h4>
                    <div class="actions">
                        <asp:LinkButton CssClass="btn btn-primary btn-checkin" ID="btnNoOptionOk" runat="server" OnClick="btnNoOptionOk_Click" Text="Ok" />
                    </div>
                </asp:Panel>

                <div id="divAbilityLevel" runat="server" class="control-group checkin-body-container">
                    <label class="control-label">Select Ability Level</label>
                    <div class="controls">
                        <asp:Repeater ID="rSelection" runat="server" OnItemCommand="rSelection_ItemCommand" OnItemDataBound="rSelection_ItemDataBound">
                            <ItemTemplate>
                                <asp:LinkButton ID="lbSelect" runat="server" Text='<%# Container.DataItem.ToString() %>' CommandArgument='<%# Eval("Guid").ToString().ToUpper() %>' OnClientClick="disableButton(this);" CssClass="btn btn-primary btn-large btn-block btn-checkin-select" />
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
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
