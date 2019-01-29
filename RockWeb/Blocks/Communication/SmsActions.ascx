<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SmsActions.ascx.cs" Inherits="RockWeb.Blocks.Communication.SmsActions" %>

<style>
.sms-container {
    min-height: 25px;
    border: 1px solid #ddd;
    background-color: #eee;
    padding: 12px;
}
    .sms-container > div {
        margin-bottom: 12px;
    }
    .sms-container > div:last-child {
        margin-bottom: 0px;
    }
.sms-action-component
{
    border: 1px solid #666;
    padding: 6px;
    height: 43px;
}
    .sms-action-component > .fa {
        border: 1px solid #888;
        padding: 2px;
    }

/*
-=-=- Dragula Classes
*/

/* Is added to the mirror image. It handles fixed positioning and z-index (and removes any prior margins on the element). Note
that the mirror image is appended to the mirrorContainer, not to its initial container. Keep that in mind when styling your
elements with nested rules, like .list .item { padding: 10px; }.*/
.gu-mirror {
  position: fixed !important;
  z-index: 9999 !important;
  margin: 0 !important;
  font-family: Arial, Helvetica, sans-serif;
  cursor: move;

  -ms-filter: "progid:DXImageTransform.Microsoft.Alpha(Opacity=80)";
  filter: alpha(opacity=80);
  opacity: .8;
}

/* Is a helper class to apply display: none to an element. */
.gu-hide {
  display: none !important;
}

/* Is added to the mirrorContainer element when dragging. You can use it to style the mirrorContainer while something is being dragged. */
.gu-unselectable {
  -webkit-user-select: none !important;
  -moz-user-select: none !important;
  -ms-user-select: none !important;
  user-select: none !important;
}
</style>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-sms"></i>&nbsp;SMS Actions</h1>
            </div>

            <div class="panel-body">
                <div class="row">
                    <div class="col-md-4">
                        <div class="js-sms-action-components sms-container">
                        <asp:Repeater ID="rptrComponents" runat="server" OnItemCommand="rptrComponents_ItemCommand">
                            <ItemTemplate>
                                <div class="sms-action-component">
                                    <i class="<%# Eval( "IconCssClass" ) %>"></i>
                                    <%# Eval( "Title" ) %> <%# Eval("Id") %>
                                    <div class="pull-right">
                                        <asp:LinkButton ID="lbAddComponent" runat="server" CssClass="btn btn-primary btn-xs" CommandName="AddComponent" CommandArgument='<%# Eval( "Id" ) %>'>
                                            <i class="fa fa-plus"></i>
                                        </asp:LinkButton>
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                        </div>
                    </div>

                    <div class="col-md-4">
                        <div class="js-sms-actions sms-container">

                        </div>
                    </div>

                    <div class="col-md-4">
                        Details
                    </div>
                </div>
            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>

<script>
Sys.Application.add_load(function () {
    dragula([$('.js-sms-action-components')[0], $('.js-sms-actions')[0]], {
        copy: true,
        revertOnSpill: true,
        removeOnSpill: true
    });
});
</script>