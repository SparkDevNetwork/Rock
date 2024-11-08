using System.Collections.Generic;
using System.ComponentModel;
using Rock.Attribute;
using Rock.Bus;
using Rock.Model;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Bus.BusStatus;

namespace Rock.Blocks.Bus
{
    [DisplayName( "Bus Status" )]
    [Category( "Bus" )]
    [Description( "Gives insight into the message bus." )]

    #region Block Attributes

    [LinkedPage(
        "Transport Select Page",
        Key = AttributeKey.TransportSelectPage,
        Description = "The page where the transport for the bus can be selected",
        DefaultValue = Rock.SystemGuid.Page.BUS_TRANSPORT,
        Order = 1 )]

    #endregion Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( "C472300C-781F-4D73-B530-8C9F8A9927D4" )]
    [Rock.SystemGuid.EntityTypeGuid( "9DFA8FD4-C3AA-440A-B1D6-1F8695C4AD5A" )]
    public class BusStatus : RockBlockType
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string TransportSelectPage = "TransportSelectPage";
        }
        private static class NavigationUrlKey
        {
            public const string TransportSelectPage = "TransportSelectPage";
        }

        #endregion Attribute Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new DetailBlockBox<BusStatusBag, BusStatusOptionsBag>();

            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();
            box.Entity = GetBusStatusData();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private BusStatusOptionsBag GetBoxOptions()
        {
            var options = new BusStatusOptionsBag();

            return options;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.TransportSelectPage] = this.GetLinkedPageUrl( AttributeKey.TransportSelectPage, "TransportSelectPage", "((Key))" )
            };
        }

        /// <summary>
        /// Gets the bus status data.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private BusStatusBag GetBusStatusData()
        {
            return new BusStatusBag
            {
                IsReady = RockMessageBus.IsReady(),
                TransportName = RockMessageBus.GetTransportName(),
                NodeName = RockMessageBus.NodeName,
                MessagesPerMinute = RockMessageBus.StatLog?.MessagesConsumedLastMinute,
                MessagesPerHour = RockMessageBus.StatLog?.MessagesConsumedLastHour,
                MessagesPerDay = RockMessageBus.StatLog?.MessagesConsumedLastDay
            };
        }

        #endregion Methods
    }
}