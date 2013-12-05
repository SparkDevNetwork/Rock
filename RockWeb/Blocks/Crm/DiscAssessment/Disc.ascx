<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Disc.ascx.cs" Inherits="Rockweb.Blocks.Crm.Disc" %>
<div id="tabs">
    <ul id="navTabs" class="nav nav-tabs">
        <li class="active"><a data-toggle="tab" href="#instructions">Instructions</a></li>
        <li><a data-toggle="tab" href="#questions">Questions</a></li>
        <li><a data-toggle="tab" href="#results">Results</a></li>
    </ul>
    <div class="tab-content">
        <div id="instructions" class="tab-pane active">
            <h3>Welcome!</h3>
            <p>
                In this assessment you are given a series of questions, each containing four phrases.
            Select one phrase from each question that MOST describes you and one phrase that
            LEAST describes you.
            </p>
            <p>
                This assessment is environmentally sensitive, which means that you may score differently
            in different situations. In other words, you may act differently at home than you
            do on the job. So, as you complete the assessment you should focus on one environment
            for which you are seeking to understand yourself. For instance, if you are trying
            to understand yourself in marriage, you should only think of your responses to situations
            in the context of your marriage. On the other hand, if you want to know your behavioral
            needs on the job, then only think of how you would respond in the job context.
            </p>
            <p>
                One final thought as you give your responses. On these kinds of assessments, it
            is often best and easiest if you respond quickly and do not deliberate too long
            on each question. Your response on one question will not unduly influence your scores,
            so simply answer as quickly as possible and enjoy the process. Don't get too hung
            up, if none of the phrases describe you or if there are some phrases that seem too
            similar, just go with your instinct.
            </p>
            <p>
                When you are ready, click the 'Questions' tab above to proceed.
            </p>
        </div>
        <div id="questions" class="tab-pane">
            <p>
                The questions will auto-advance as you answer them.
            </p>
            <asp:Table ID="tblQuestions" runat="server">
            </asp:Table>
            <asp:Button ID="btnScoreTest" Text="Score Test" runat="server" OnClick="btnScoreTest_Click" />
        </div>
        <div id="results" class="tab-pane">
            <table border="1" cellpadding="1" cellspacing="1" class="table-bordered table-condensed">
                <thead>
                    <tr>
                        <th colspan="2" class="">Adaptive Behavior
                        </th>
                        <th colspan="2" class="">Natural Behavior
                        </th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td class="">D:
                        </td>
                        <td class="">
                            <asp:Label ID="lblABd" Text="" runat="server" />
                        </td>
                        <td class="">D:
                        </td>
                        <td class="">
                            <asp:Label ID="lblNBd" Text="" runat="server" />
                        </td>
                    </tr>
                    <tr class="">
                        <td class="">I:
                        </td>
                        <td class="">
                            <asp:Label ID="lblABi" Text="" runat="server" />
                        </td>
                        <td class="">I:
                        </td>
                        <td class="">
                            <asp:Label ID="lblNBi" Text="" runat="server" />
                        </td>
                    </tr>
                    <tr class="">
                        <td class="">S:
                        </td>
                        <td class="">
                            <asp:Label ID="lblABs" Text="" runat="server" />
                        </td>
                        <td class="">S:
                        </td>
                        <td class="">
                            <asp:Label ID="lblNBs" Text="" runat="server" />
                        </td>
                    </tr>
                    <tr class="">
                        <td class="">C:
                        </td>
                        <td class="">
                            <asp:Label ID="lblABc" Text="" runat="server" />
                        </td>
                        <td class="">C:
                        </td>
                        <td class="">
                            <asp:Label ID="lblNBc" Text="" runat="server" />
                        </td>
                    </tr>
                </tbody>
            </table>
            <asp:Button ID="btnSaveResults" Text="Save Results" runat="server" OnClick="btnSaveResults_Click" />
            <hr />
            <h2 class="">Your Saved DISC Assessment Scores
            </h2>
            <div class="">
                Saved on:
                <asp:Label ID="lblLastAssessmentDate" Text="" runat="server" />
            </div>
            <table border="0" cellpadding="1" cellspacing="1" class="table-bordered table-condensed">
                <thead>
                    <tr>
                        <th colspan="2" class="">Adaptive Behavior
                        </th>
                        <th colspan="2" class="">Natural Behavior
                        </th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td class="">D:
                        </td>
                        <td class="">
                            <asp:Label ID="lblPrevABd" Text="" runat="server" />
                        </td>
                        <td class="">D:
                        </td>
                        <td class="">
                            <asp:Label ID="lblPrevNBd" Text="" runat="server" />
                        </td>
                    </tr>
                    <tr class="">
                        <td class="">I:
                        </td>
                        <td class="">
                            <asp:Label ID="lblPrevABi" Text="" runat="server" />
                        </td>
                        <td class="">I:
                        </td>
                        <td class="">
                            <asp:Label ID="lblPrevNBi" Text="" runat="server" />
                        </td>
                    </tr>
                    <tr class="">
                        <td class="">S:
                        </td>
                        <td class="">
                            <asp:Label ID="lblPrevABs" Text="" runat="server" />
                        </td>
                        <td class="">S:
                        </td>
                        <td class="">
                            <asp:Label ID="lblPrevNBs" Text="" runat="server" />
                        </td>
                    </tr>
                    <tr class="">
                        <td class="">C:
                        </td>
                        <td class="">
                            <asp:Label ID="lblPrevABc" Text="" runat="server" />
                        </td>
                        <td class="">C:
                        </td>
                        <td class="">
                            <asp:Label ID="lblPrevNBc" Text="" runat="server" />
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>
    </div>
</div>