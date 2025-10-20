namespace Rock.AI.Agent.Classes.Entity
{
    internal class FinancialAccountResult : EntityResultBase
    {
        public string Name { get; set; }
        public string PublicDescription { get; set; }
        public bool IsTaxDeductible { get; set; }
        public System.Collections.Generic.List<FinancialAccountResult> Children { get; set; } = new System.Collections.Generic.List<FinancialAccountResult>();
        public string ParentAccountIdKey { get; set; }
    }
}
