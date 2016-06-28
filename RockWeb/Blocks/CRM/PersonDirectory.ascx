<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonDirectory.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDirectory" %>

<style>
    .person-directory-grid > div.row {
        padding:5px;
    }
    .person-directory-grid > div.row:nth-child(odd) {
        background-color:#f5f2ee;
    }
</style>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <script>
            Sys.Application.add_load( function () {
                $("div.photo-round").lazyload({
                    effect: "fadeIn"
                });
            });
        </script>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-users"></i> Directory</h1>
            </div>
            <div class="panel-body">

                <Rock:NotificationBox ID="nbValidation" runat="server" NotificationBoxType="Danger" />

                <div class="row">
                    <div class="col-md-4">
                        <Rock:RockTextBox ID="tbFirstName" runat="server" Label="First Name" />
                    </div>
                    <div class="col-md-4">
                        <Rock:RockTextBox ID="tbLastName" runat="server" Label="Last Name" />
                    </div>
                    <div class="col-md-4">
                        <div class="form-group">
                            <label class="control-label">&nbsp;</label>
                            <div class="control-wrapper">
                                <asp:LinkButton ID="lbSearch" runat="server" Text="Search" CssClass="btn btn-primary" OnClick="lbSearch_Click"  />
                                <asp:LinkButton ID="lbClear" runat="server" Text="Clear" CssClass="btn btn-link" OnClick="lbClear_Click" />
                            </div>
                        </div>
                    </div>
                </div>

                <ul class="list-inline">
                    <asp:PlaceHolder ID="phLetters" runat="server" />
                </ul>

                <div class="person-directory-grid">

                    <asp:Repeater ID="rptPeople" runat="server" >
                        <ItemTemplate>
                            <div class="row">
                                <asp:Literal ID="lPerson" runat="server" />
                                <asp:Literal ID="lAddress" runat="server" />
                                <asp:Literal ID="lPhones" runat="server" />
                                <asp:Literal ID="lBirthdateGender" runat="server" />
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>

                    <asp:Repeater ID="rptFamilies" runat="server" >
                        <ItemTemplate>
                            <div class="row">
                                <div class="col-md-3">
                                    <asp:Literal ID="lFamily" runat="server" />
                                </div>
                                <div class="col-md-9">
                                    <asp:Repeater ID="rptFamilyPeople" runat="server" >
                                        <ItemTemplate>
                                            <div class="row margin-b-sm">
                                                <asp:Literal ID="lPerson" runat="server" />
                                                <asp:Literal ID="lPhones" runat="server" />
                                                <asp:Literal ID="lBirthdateGender" runat="server" />
                                            </div>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>

                </div>

                <div class="actions margin-t-lg">
                    <asp:LinkButton ID="lbOptInOut" runat="server" Text="Opt Out" CssClass="btn btn-default btn-sm" OnClick="btnOptOutIn_Click" />
                </div>

            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
