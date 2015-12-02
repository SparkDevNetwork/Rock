<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ImportPeople.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.LifeGroupFinder.ImportPeople" %>
<asp:Panel ID="pnlContent" runat="server">
    <asp:Wizard ID="Wizard1" runat="server" ActiveStepIndex="0"
        Height="370px" Width="651px" HeaderText="Import People to Group"
        OnFinishButtonClick="Wizard1_FinishButtonClick"
        OnNextButtonClick="Wizard1_NextButtonClick">
        <WizardSteps>

            <asp:WizardStep runat="server" Title="Upload List Data" ID="WizardStep1">
                <div>
                    <h3>Step 1 - Choose List</h3>
                    Upload a file (csv) containing the following three (3) required, ordered fields:<br />
                    <asp:BulletedList ID="BulletedList1" runat="server">
                        <asp:ListItem>first name</asp:ListItem>
                        <asp:ListItem>last name</asp:ListItem>
                        <asp:ListItem>email address (required)</asp:ListItem>
                    </asp:BulletedList>
                </div>
                <table>
                    <tr>
                        <td align="right" width="20%">
                            <asp:Label ID="Label1" runat="server" Text="CSV file"></asp:Label>
                        </td>
                        <td>
                            <asp:FileUpload ID="fuUpload" runat="server"/>
                        </td>
                        <td>
                            <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server"
                                ControlToValidate="fuUpload" ErrorMessage="Please select a file to upload">*</asp:RequiredFieldValidator>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="3">
                            <asp:Label ID="lblErrorMessage" runat="server"
                                Text="There was a problem with the file you are trying to upload:"
                                Visible="False"></asp:Label>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="3">
                            <asp:Label ID="lblErrors" runat="server" Visible="False"></asp:Label>
                        </td>
                    </tr>
                </table>
            </asp:WizardStep>

            <asp:WizardStep ID="WizardStep2" runat="server" Title="Resolve People">
                <div >
                    <h3>Step  2 - Resolve Multiple Matching Records</h3>
                    <p>
                        Resolve
                        <asp:Label ID="lblRemainingRecords" runat="server" Text="TBD"></asp:Label>
                        remaining ambiguous matches
                    </p>
                </div>
                <br />
                <table>
                    <tr>
                        <td align="left">
                            <asp:Label ID="lblRawLineData" runat="server" Text=""></asp:Label></td>
                    </tr>
                    <tr>
                        <td>
                            <asp:RadioButtonList ID="rblPeople" runat="server"></asp:RadioButtonList>
                        </td>
                    </tr>
                </table>

                <hr />
                <p>
                    <i>* Ready to import
                    <asp:Label ID="lblReadyToImportPeople" runat="server" Text="TBD"></asp:Label>
                        people.</i>
                </p>
                <p>
                    <b>Note:</b>
                    There are
                    <asp:Label ID="lblUnmatchedItems" runat="server" Text="TBD"></asp:Label>
                    unmatched (new) items.<br />
                    There were
                    <asp:Label ID="lblCleanMatches" runat="server" Text="TBD"></asp:Label>
                    clean matches.<br />
                </p>
            </asp:WizardStep>

            <asp:WizardStep runat="server" Title="Select Group" ID="WizardStep3">
                <div>
                    <h3>Step 3 - Select Group</h3>
                    <br />
                    Select a group to import the people into.<br />
                </div>
                <br />
                <table>
                    <tr>
                        <td>
                            <Rock:RockDropDownList ID="ddlGroupType" runat="server" Label="Group Type" OnSelectedIndexChanged="ddlGroupType_SelectedIndexChanged" AutoPostBack="true" />
                            <Rock:RockDropDownList ID="ddlGroupRole" runat="server" Label="Group Member Role" />
                            <Rock:RockDropDownList ID="ddlGroupMemberStatus" runat="server" Label="Group Member Status" />
                            <Rock:RockDropDownList ID="ddlGroup" runat="server" Label="Group"></Rock:RockDropDownList>
                        </td>
                        <td>&nbsp;</td>
                    </tr>
                </table>
            </asp:WizardStep>

            <asp:WizardStep runat="server" StepType="Finish" Title="Summary">
                <div>
                    <h3>&nbsp;Summary</h3>
                    <div runat="server" id="divAlert" class="alert alert-warning">
                        <p>
                            WARNING! This tool is configured to Add unmatched people as new records to the database.
                    That means it will add them as new records to the database and then add them to the group.
                        </p>
                        <p>
                            However you can check this checkbox to disable it temporarily:<br />
                            <asp:CheckBox runat="server" ID="cbDisableAutoAdd" />
                            (check to disable automatic adding)
                        </p>
                    </div>
                    <br />
                    <asp:Label ID="lblSummary" runat="server"></asp:Label>
                    <br />
                </div>
            </asp:WizardStep>

            <asp:WizardStep runat="server" StepType="Complete" Title="Complete">
                <div>
                    <h3>Complete</h3>
                    <br />
                    <asp:Label ID="lblComplete" runat="server"
                        Text="Finished!"></asp:Label>
                    <br />
                    <br />
                    <asp:Label ID="lblCompleteMsg" runat="server"></asp:Label>
                </div>
            </asp:WizardStep>

        </WizardSteps>
    </asp:Wizard>
</asp:Panel>


