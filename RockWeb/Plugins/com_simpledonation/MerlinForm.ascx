<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MerlinForm.ascx.cs" Inherits="Plugins.com_simpledonation.SimpleDonationMerlinForm" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
		<asp:Panel ID="pnlDetails" runat="server" Visible="true">
            <asp:Literal runat="server" ID="merlinFormWarning"></asp:Literal>

            <Rock:Lava runat="server" ID="currentPersonCheck">
                {% if CurrentPerson %}
                    {% sql return:'sdPersonRow' %}
                        SELECT
                            TOP 1 sdp.SimpleDonationPersonId
                        FROM [FinancialPersonSavedAccount] fpsa
                        JOIN _com_simpledonation_SimpleDonationPerson sdp
                            ON sdp.PaymentMethodId = fpsa.ReferenceNumber
                        JOIN PersonAlias pa
                            ON pa.Id = fpsa.PersonAliasId
                        WHERE pa.PersonId = {{ CurrentPerson.Id }}
                        ORDER BY fpsa.CreatedDateTime DESC
                    {% endsql %}
                {% endif %}

                <script type="text/javascript">
                    var personRow = {% if CurrentPerson %}{{ sdPersonRow | First | ToJSON }}{% else %}  {
                  "SimpleDonationPersonId": "notLoggedIntoRock"
                }{% endif %};
                    var personId = personRow ? personRow['SimpleDonationPersonId'] : 'rockLoggedInNoSdPerson'

                    window.simpleDonationMerlinSettings = {
                        personId: personId,
                    };
                </script>
            </Rock:Lava>

            <asp:Literal runat="server" id="merlinFormControl"></asp:Literal>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>