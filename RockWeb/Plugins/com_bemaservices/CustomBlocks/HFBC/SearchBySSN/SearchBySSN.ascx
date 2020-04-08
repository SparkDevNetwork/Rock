<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SearchBySSN.ascx.cs" Inherits="RockWeb.Plugins.com_bemadev.SearchBySSN.SearchBySSN" %>

<style>
    .btn-full-width {
        width: 100% !important;
    }
</style>
<div class="well">
    <h1><strong>SSN</strong> Search</h1>
	<p>Please enter the last 4 digits below to lookup a person</p>
</div>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-body">
                <div class="well">
                    <div class="row">
                        <div class="col-md-3">
                            <div class="input-group">
                                <Rock:RockTextBox runat="server" ID="inputbox_ssn" class="form-control" Placeholder="SSN#..." />
                                <span class="input-group-btn">
                                    <Rock:BootstrapButton ID="rbb_SearchBySSN" runat="server" CssClass="btn btn-primary" Text="Go!" OnClick="rbb_SearchBySSN_Click" />
                                </span>
                            </div>
                        </div>
                    </div>
                </div>
                <hr />
                <div class="row">
                    <div class="col-md-12">
                        <Rock:NotificationBox ID="nbNotice" runat="server" NotificationBoxType="Info" Visible="false"></Rock:NotificationBox>

                        <script>
                            Sys.Application.add_load(function () {
                                $("div.photo-round").lazyload({
                                    effect: "fadeIn"
                                });
                            });
                        </script>

                        <div class="grid">
                            <Rock:Grid ID="gPeople" runat="server" EmptyDataText="No People Found" AllowSorting="true" OnRowSelected="gPeople_RowSelected">
                                <Columns>
                                    <Rock:RockTemplateField HeaderText="Person" SortExpression="LastName,FirstName">
                                        <ItemTemplate>
                                            <asp:Literal ID="lPerson" runat="server" />
                                        </ItemTemplate>
                                    </Rock:RockTemplateField>
                                    <Rock:DateField
                                        DataField="BirthDate"
                                        HeaderText="Birthdate"
                                        SortExpression="BirthYear desc,BirthMonth desc,BirthDay desc"
                                        ColumnPriority="Desktop" />
                                    <Rock:RockBoundField
                                        ItemStyle-HorizontalAlign="Right"
                                        HeaderStyle-HorizontalAlign="Right"
                                        DataField="Age"
                                        HeaderText="Age"
                                        SortExpression="BirthYear desc,BirthMonth desc,BirthDay desc"
                                        ColumnPriority="Desktop" />
                                    <Rock:RockBoundField
                                        DataField="Gender"
                                        HeaderText="Gender"
                                        SortExpression="Gender"
                                        ColumnPriority="Desktop" />
                                    <Rock:RockTemplateField HeaderText="Spouse">
                                        <ItemTemplate>
                                            <asp:Literal ID="lSpouse" runat="server" />
                                        </ItemTemplate>
                                    </Rock:RockTemplateField>
                                    <Rock:DefinedValueField
                                        DataField="ConnectionStatusValueId"
                                        HeaderText="Connection Status"
                                        ColumnPriority="Tablet" />
                                    <Rock:DefinedValueField
                                        DataField="RecordStatusValueId"
                                        HeaderText="Record Status"
                                        ColumnPriority="Desktop" />
                                    <Rock:RockTemplateField HeaderText="Campus" ColumnPriority="Tablet">
                                        <ItemTemplate>
                                            <asp:Literal ID="lCampus" runat="server" />
                                        </ItemTemplate>
                                    </Rock:RockTemplateField>
                                    <Rock:RockLiteralField ID="lEnvelopeNumber" HeaderText="Envelope #" />
                                     <Rock:LinkButtonField OnClick="lbfStartEntry_Click" ID="lbfStartEntry" Text="Start Entry" CssClass="btn btn-success btn-lg btn-full-width"  />
                                </Columns>
                            </Rock:Grid>
                        </div>

                        <asp:Literal ID="lPerformance" runat="server" />
                    </div>
                </div>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
