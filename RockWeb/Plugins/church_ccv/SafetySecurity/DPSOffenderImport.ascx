<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DPSOffenderImport.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.SafetySecurity.DPSOffenderImport" %>

<script src="/SignalR/hubs"></script>
<script type="text/javascript">
    $(function () {
        var proxy = $.connection.rockMessageHub;

        proxy.client.showLog = function () {
            $('.js-notification').fadeIn();
        }

        proxy.client.receiveNotification = function (message) {
            $('.js-notification').text(message);
        }

        $.connection.hub.start().done(function () {
            // hub started... do stuff here if you want to let the user know something
        });
    })
</script>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-flag"></i>&nbsp;DPS Offender Import</h1>
            </div>
            <div class="panel-body">


                <Rock:FileUploader ID="fuImport" runat="server" Label="Import File" OnFileUploaded="fuImport_FileUploaded" />
                <Rock:NotificationBox ID="nbResult" runat="server" Visible="false" />

                <asp:LinkButton ID="btnMatchAddresses" runat="server" CssClass="btn btn-action" Text="Match Addresses" OnClick="btnMatchAddresses_Click" />
                <asp:LinkButton ID="btnMatchPeople" runat="server" CssClass="btn btn-action" Text="Match People" OnClick="btnMatchPeople_Click" />

                <Rock:RockCheckBox ID="cbMatchZip" runat="server" Text="ZipCode" />
                <Rock:RockCheckBox ID="cbAge" runat="server" Text="Age" />
                <Rock:RockCheckBox ID="cbLimitToPotentialMatches" runat="server" Text="Limit to Potential Matches" />
                <Rock:RockCheckBox ID="cbLimitToLocationMatches" runat="server" Text="Limit to Location Matches" />

                <div class="js-notification"></div>

                <Rock:Grid ID="gDpsOffender" runat="server" DataKeyNames="Id" AllowSorting="true" OnRowDataBound="gDpsOffender_RowDataBound">
                    <Columns>
                        <Rock:PersonField DataField="Person" />
                        <asp:BoundField DataField="DpsLocation" HeaderText="DPS Location" />
                        <asp:BoundField DataField="FamiliesAtAddress" HeaderText="FamiliesAtAddress" SortExpression="FamiliesAtAddress" />
                        <Rock:RockLiteralField ID="lPersonMatches" HeaderText="Potential Matches" SortExpression="PotentialMatches" ItemStyle-Wrap="false" />
                        <asp:BoundField DataField="FirstName" HeaderText="First Name" SortExpression="FirstName" />
                        <asp:BoundField DataField="LastName" HeaderText="Last Name" SortExpression="LastName" />
                        <asp:BoundField DataField="Age" HeaderText="Age" SortExpression="Age" />
                        <asp:BoundField DataField="Height" HeaderText="Height" SortExpression="Height" />
                        <asp:BoundField DataField="Weight" HeaderText="Weight" SortExpression="Weight" />
                        <asp:BoundField DataField="Race" HeaderText="Race" SortExpression="Race" />
                        <asp:BoundField DataField="Gender" HeaderText="Gender" SortExpression="Gender" />
                        <asp:BoundField DataField="Hair" HeaderText="Hair Color" SortExpression="Hair" />
                        <asp:BoundField DataField="Eyes" HeaderText="Eye Color" SortExpression="Eyes" />
                        <asp:BoundField DataField="ResAddress" HeaderText="ResAddress" SortExpression="ResAddress" Visible="false" />
                        <asp:BoundField DataField="ResCity" HeaderText="ResCity" SortExpression="ResCity" Visible="false"/>
                        <asp:BoundField DataField="ResState" HeaderText="ResState" SortExpression="ResState" Visible="false"/>
                        <asp:BoundField DataField="ResZip" HeaderText="ResZip" SortExpression="ResZip" Visible="false"/>
                        <asp:BoundField DataField="Level" HeaderText="Level" SortExpression="Level" Visible="true"/>
                        <Rock:DateField DataField="DateConvicted" HeaderText="DateConvicted" SortExpression="DateConvicted" />
                        <asp:BoundField DataField="ConvictionState" HeaderText="ConvictionState" SortExpression="ConvictionState" />
                        <Rock:BoolField DataField="Absconder" HeaderText="Absconder" SortExpression="Absconder" />



                    </Columns>
                </Rock:Grid>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
