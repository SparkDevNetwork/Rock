<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SimplePrayerBlock.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Widgets.SimplePrayerBlock" %>
<asp:UpdatePanel ID="upAdd" runat="server">
<ContentTemplate>

<asp:Panel runat="server" CssClass="panel panel-block" ID="pnlForm">
    <div class="row">
 
  <div class="col-lg-6">
    <div class="input-group">
      <input type="text" ID="dtbRequest" class="form-control" runat="server" placeholder="Please pray that...">
        <span class="glyphicon glyphicon-ok form-control-feedback" aria-hidden="true"></span>
        <span id="inputSuccess2Status" class="sr-only">(success)</span>
      <span class="input-group-btn">
        <asp:LinkButton class="btn btn-default" type="button" id="lbComReq" runat="server" accesskey="s" onclick="btnComplete_Click" CssClass="btn btn-primary" causesvalidation="True">></asp:LinkButton>
      </span>
    </div><!-- /input-group -->
  </div><!-- /.col-lg-6 -->
</div><!-- /.row -->
</asp:Panel>





</ContentTemplate>
</asp:UpdatePanel>
