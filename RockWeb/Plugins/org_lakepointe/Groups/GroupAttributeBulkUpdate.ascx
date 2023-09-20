<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupAttributeBulkUpdate.ascx.cs" Inherits="RockWeb.Plugins.org_lakepointe.Groups.GroupAttributeBulkUpdate" %>

<script type="text/javascript">
  Sys.WebForms.PageRequestManager.getInstance().add_endRequest(pageLoaded);

  function pageLoaded(sender, args) {
     window.scrollTo(0,0);
  }
</script>

<asp:UpdatePanel ID="upMain" runat="server">

    <ContentTemplate>
        <style type="text/css">
            .selectItem {
                width: 25px;
                float: left;
            }

            .attributeField {
                width: 100%;
            }

            .panel-footer {
                background-color: #f3f3f3 !important;
            }
        </style>
        <asp:HiddenField ID="hfSelectedFields" runat="server" />
        <Rock:NotificationBox ID="nbGroupAttributeUpdate" runat="server" Visible="false" NotificationBoxType="Success" />
        <asp:Panel ID="pnlGroupInfo" runat="server">
            <div class="row">
                <div class="col-sm-6">
                    <Rock:RockLiteral ID="lGroupName" runat="server" Label="Group Name" />
                </div>
                <div class="col-sm-6">
                    <Rock:RockLiteral ID="lGroupType" runat="server" Label="Group Type" />
                </div>
            </div>
            <div class="panel panel-block margin-t-md">
                <div class="panel-heading clearfix">
                    <h2 class="panel-title pull-left">
                        <i class="fal fa-edit"></i>Attributes
                    </h2>
                </div>
                <div class="panel-body">
                    <asp:PlaceHolder ID="phAttributes" runat="server"></asp:PlaceHolder>
                </div>
                <asp:Panel ID="pnlGroupInfoFooter" runat="server" CssClass="panel-footer"  Visible="false">
                    <div class="row">
                        <div class="col-sm-12" style="padding-bottom:10px;">
                            <Rock:RockCheckBox ID="cbUpdateChildGroups"  DisplayInline="true" runat="server" Style="padding-right:0px;" /><span class="control-label" >Update Child Groups</span>
                        </div>
                    </div>
                    <div class="actions row">
                        <div class="col-sm-12">
                            <Rock:BootstrapButton ID="btnSubmit" runat="server" Visible="true" CssClass="btn btn-primary" Text="Update Attributes" OnClick="btnSubmit_Click" />
                            <Rock:BootstrapButton ID="btnReset" runat="server" Visible="true" CssClass="btn btn-default" Text="Reset" OnClick="btnReset_Click" />
                        </div>
                    </div>
                </asp:Panel>
            </div>
        </asp:Panel>
        <asp:Panel ID="pnlConfirm" runat="server" Visible="false">
            <div class="panel panel-block margin-t-md">
                <div class="panel-heading clearfix">
                    <h2 class="panel-title pull-left">
                        <i class="fal fa-edit"></i>Update Group Attributes
                    </h2>
                </div>
                <div class="panel-body">
                    
                    <asp:PlaceHolder ID="phConfirm" runat="server"></asp:PlaceHolder>
                    
                </div>
                <div class="panel-footer">
                    <div class="actions">
                        <asp:LinkButton ID="btnBack" runat="server" Text="Back" CssClass="btn btn-link" OnClick="btnBack_Click" />
                        <asp:LinkButton ID="btnConfirm" runat="server" Text="Confirm" CssClass="btn btn-primary" OnClick="btnConfirm_Click" />
                    </div>
                </div>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
