<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Staff.ascx.cs" Inherits="RockWeb.Plugins.org_newpointe.Staff.Staff" %>

<div class="container-fluid staff">
    <h2 class="text-center"><span><asp:Label runat="server" ID="lblGroupName"></asp:Label></span></h2>

    <hr />

    <div class="row">
        <asp:Repeater runat="server" ID="rptStaff" OnItemDataBound="rptStaff_ItemDataBound">
            <ItemTemplate>

                <div class="col-xs-6 col-sm-4 col-md-3 col-lg-2 text-center">
                    <asp:Image runat="server" CssClass="staff" ImageUrl='<%# Eval("PhotoUrl") %>' />
                    <p style="color: #8bc540; font-size:18px; letter-spacing: -1px;">
                        <asp:Label runat="server" Text='<%# Eval("Name") %>' />
                    </p>
                    <p class="small" style="margin-top: -10px; height: 40px; border-top:1px solid #efefef!important;">
                        <asp:Label runat="server" Text='<%# Eval("Position") %>' />
                    </p>
                </div>

            </ItemTemplate>
        </asp:Repeater>
    </div>
</div>