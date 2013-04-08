//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class FundPicker : ItemPicker
    {
        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="fund">The fund.</param>
        public void SetValue( Fund fund )
        {
            if ( fund != null )
            {
                ItemId = fund.Id.ToString();
                var parentFundIds = string.Empty;
                var parentFund = fund.ParentFund;

                while ( parentFund != null )
                {
                    parentFundIds = parentFund.Id + "," + parentFundIds;
                    parentFund = parentFund.ParentFund;
                }

                InitialItemParentIds = parentFundIds.TrimEnd( new[] { ',' } );
                ItemName = fund.PublicName;
            }
            else
            {
                ItemId = Constants.None.IdValue;
                ItemName = Constants.None.TextHtml;
            }
        }

        /// <summary>
        /// Sets the value on select.
        /// </summary>
        protected override void SetValueOnSelect()
        {
            var item = new FundService().Get( int.Parse( ItemId ) );
            this.SetValue( item );
        }

        /// <summary>
        /// Gets the item rest URL.
        /// </summary>
        /// <value>
        /// The item rest URL.
        /// </value>
        public override string ItemRestUrl
        {
            get { return "~/api/funds/getchildren/"; }
        }

        /// <summary>
        /// Sets the name of the fund entity type.
        /// </summary>
        /// <value>
        /// The name of the fund entity type.
        /// </value>
        public string FundEntityTypeName
        {
            set { ItemRestUrlExtraParams = "/" + value + "/false"; }
        }
    }
}
