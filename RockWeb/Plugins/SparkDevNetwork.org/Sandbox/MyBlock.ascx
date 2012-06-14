<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MyBlock.ascx.cs" Inherits="RockWeb.Blocks.MyBlock" %>
<script>
    $(document).ready(function () {

        //attach a jQuery live event to the button
        $('#getjson-button').live('click', function () {
            alert('calling rest');
            $.ajax({
                type: "PUT",
                contentType: "application/json",
                dataType: "json",
                data: '{ "Street1":"14224 N 184th ave", "City":"Surprise", "State":"AZ", "Zip":"85388" }',
                url: 'http://localhost:6229/RockWeb/api/Crm/Address/Geocode',
                success: function (data, status, xhr) {
                    alert(data.Street1)
                },
                error: function (xhr, status, error) {
                    alert(status + ' ' + error + ' ' + xhr.responseText);
                }
            });
        });

        $('#getpagemethod-button').live('click', function () {
            alert('calling pagemethod');
            PageMethods.GetPerson(1, OnSucceeded, OnFailed);
        });

        function OnSucceeded(data, userContext, methodName) {
            alert(data.FirstName + ' ' + data.LastName);
        }

        function OnFailed(error, userContext, methodName) {
            alert(error);
        }

    });
    
</script>


<p>MY BLOCK CONTROL</p>
<%= ThemePath %><br />
<asp:literal ID="lPersonName" runat="server"></asp:literal><br />
<asp:literal ID="lBlockDetails" runat="server"></asp:literal><br />
<asp:literal ID="lBlockTime" runat="server"></asp:literal><br />
<asp:literal ID="lItemCache" runat="server"></asp:literal><br />
<asp:Literal ID="lItemTest" runat="server"></asp:Literal><br />
<asp:Literal ID="lParentGroups" runat="server"></asp:Literal><br />
<asp:Literal ID="lChildGroups" runat="server"></asp:Literal><br />
<asp:UpdatePanel ID="pnlAttributeValues" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
        <asp:Panel ID="pnlAttribute" runat="server">
            This DIV's bgcolor is from an attribute value <br />
            Best sci-fi movie: <asp:Literal ID="lMovie" runat="server"></asp:Literal>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
<asp:Button ID="bFlushItemCache" runat="server" Text="Flush Item Cache" onclick="bFlushItemCache_Click" />

<br />
<br />
<a href="#" id="getjson-button">Get JSON Data</a> (must be logged in)<br />
<a href="#" id="getpagemethod-button">Get PageMethod Data</a> (must be logged in)

<br />
<br />

<asp:UpdatePanel ID="upDialog" runat="server">
<ContentTemplate>

<asp:Button ID="btnShowDialog" runat="server" Text="Show Dialog" />
<asp:Button ID="btnShowDialog2" runat="server" Text="Show Dialog 2" />

<Rock:ModalDialog ID="mdTest" runat="server" TargetControlID="btnShowDialog2" Title="Test Dialog">
<Content>
    This will be the content
    <asp:TextBox ID="tbContent" runat="server"></asp:TextBox>
</Content>
</Rock:ModalDialog>

</ContentTemplate>
</asp:UpdatePanel>
