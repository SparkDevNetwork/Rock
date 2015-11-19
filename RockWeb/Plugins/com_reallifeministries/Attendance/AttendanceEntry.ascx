<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AttendanceEntry.ascx.cs" Inherits="com.reallifeministries.Attendance.AttendanceEntry" %>

<asp:UpdatePanel runat="server">
    <ContentTemplate>
        <% if (!String.IsNullOrEmpty(lblMessage.Text)) { %>
        <p class="alert alert-info">
            <i class="fa fa-info-circle"></i> <asp:label ID="lblMessage" runat="server"  />
        </p>
        <% } %>
        

        <asp:Panel runat="server" ID="pnlSearch" >
            <fieldset>
                <legend>Attendance Info</legend>
                <Rock:DatePicker runat="server"  id="dpAttendanceDate" Required="true" Label="Attended Date" />
                <Rock:CampusPicker ID="ddlCampus" runat="server" Label="Campus" />
            </fieldset>                      
            <fieldset>
                <legend>Search For Person</legend>                   
                <Rock:RockTextBox ID="tbPhoneNumber" runat="server" Label="Phone Number"  />
                <Rock:RockTextBox ID="tbName" runat="server" Label="Name" />
                <p>
                    <Rock:BootstrapButton ID="btnSearch" runat="server" CssClass="btn btn-lg btn-primary" Text="<i class='fa fa-search'></i> Search" OnClick="btnSearch_Click" />
                    <Rock:BootstrapButton ID="btnClear" runat="server" CssClass="btn btn-default" Text="Clear" OnClick="btnClear_Click" />  
                </p>
                
            </fieldset>
        </asp:Panel>
        <asp:Panel runat="server" ID="pnlResults" visible="false">
            <Rock:Grid runat="server" ID="gResults" AllowSorting="true" cssClass="people-results" DataKeyNames="Id" >
                <Columns>
                    <asp:TemplateField>
                        <HeaderTemplate>
                            <input type="checkbox" onclick="javascript:ToggleAllRows(this);"  />
                        </HeaderTemplate>
                        <ItemTemplate>
                            <asp:CheckBox runat="server" ID="chkSelect" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
                        <ItemTemplate>
                            <asp:LinkButton runat="server" ID="btnFamily" OnClick="btnFamily_Click" CommandArgument='<%# Eval("ID") %>'>
                                <i class="fa fa-group"></i>
                            </asp:LinkButton>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="FullNameReversed" HeaderText="Name" />
                    <asp:TemplateField HeaderText="Phone Number(s)">
                        <ItemTemplate>
                            <%# String.Join(", ",((ICollection<Rock.Model.PhoneNumber>) Eval("PhoneNumbers")).AsEnumerable()) %>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                
            </Rock:Grid>
            <p>
                <Rock:BootstrapButton ID="btnRecord" runat="server" Text="<i class='fa fa-save'></i> Record Attendance" CssClass="btn btn-primary btn-lg" OnClick="btnRecord_Click" />
            </p>
            
        </asp:Panel>


        
    </ContentTemplate>
</asp:UpdatePanel>
<script>
    jQuery(function ($) {
        
        var $sbCampusPicker = $(<%= ddlCampus.ClientID %>);
        var campusKey = 'attendance-campus_id';

        if (!$sbCampusPicker.val()) {
            $sbCampusPicker.val(sessionStorage.getItem(campusKey));
        }
        $sbCampusPicker.on('change', function (e) {
            sessionStorage.setItem(campusKey, $sbCampusPicker.val());
        });
    });

    function ToggleAllRows(chk) {
        var $gResults = $(".people-results");
        var $chk = $(chk)
        $gResults.find(':checkbox').not($chk).prop('checked', chk.checked);
    }
   
</script>
