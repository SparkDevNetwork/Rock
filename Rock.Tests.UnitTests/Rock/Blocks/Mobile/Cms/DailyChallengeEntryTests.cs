using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Model;

using static Rock.Blocks.Types.Mobile.Cms.DailyChallengeEntry;

namespace Rock.Tests.UnitTests.Rock.Blocks.Mobile.Cms
{
    [TestClass]
    public class DailyChallengeEntryTests
    {
        private static Guid _fiveDayChallengeGuid = new Guid( "CF0A95D9-B57A-4AE8-8554-2D5FD9A3A8D0" );

        private static Guid _fiveDayChallengeDayOneGuid = new Guid( "20EFEFA7-18A9-4BDF-9CC0-E9643E08C8B9" );
        private static Guid _fiveDayChallengeDayOneItemOneGuid = new Guid( "E8D3F14B-FFD3-4B2B-B51D-5A2E5050DD0B" );
        private static Guid _fiveDayChallengeDayOneItemTwoGuid = new Guid( "352A9078-63C4-4BA4-BC35-B01EB7C27860" );
        private static Guid _fiveDayChallengeDayOneItemThreeGuid = new Guid( "77EC8E11-454E-48AC-A88D-0F5FE044A390" );

        private static Guid _fiveDayChallengeDayTwoGuid = new Guid( "E1487D35-56A0-4992-90FC-5C731F8F304D" );
        private static Guid _fiveDayChallengeDayTwoItemOneGuid = new Guid( "B4D6F92A-493F-414E-9A91-2A2A7ED500F8" );
        private static Guid _fiveDayChallengeDayTwoItemTwoGuid = new Guid( "1731DF9E-C700-48F7-A2A5-CA607683E0CE" );
        private static Guid _fiveDayChallengeDayTwoItemThreeGuid = new Guid( "95C921B4-B3C7-4CB5-B036-ECC2C4663985" );

        private static Guid _fiveDayChallengeDayThreeGuid = new Guid( "0AC576B0-B138-45F7-A221-21CAB555D013" );
        private static Guid _fiveDayChallengeDayThreeItemOneGuid = new Guid( "98017EF5-4019-4835-B830-DFDB4FD11B21" );
        private static Guid _fiveDayChallengeDayThreeItemTwoGuid = new Guid( "DECD3D53-E57B-4C18-A640-D478B8F355A8" );
        private static Guid _fiveDayChallengeDayThreeItemThreeGuid = new Guid( "3E453608-FC41-43BF-8C72-D0787F819438" );

        private static Guid _fiveDayChallengeDayFourGuid = new Guid( "F1D90A1E-796C-4168-B791-4120D8046E47" );
        private static Guid _fiveDayChallengeDayFourItemOneGuid = new Guid( "FD6089CA-0B0B-4974-9189-E5642046B211" );
        private static Guid _fiveDayChallengeDayFourItemTwoGuid = new Guid( "340FDDFC-D177-4CBC-A5C7-AE8B0A015B02" );

        private static Guid _fiveDayChallengeDayFiveGuid = new Guid( "00C5E61A-AB9D-42A9-9CC2-A03828727FE7" );
        private static Guid _fiveDayChallengeDayFiveItemOneGuid = new Guid( "B6D88E30-E706-4384-A422-DEC653BD0849" );

        private static ContentChannel GetFiveDayChallengeChannel()
        {
            return new ContentChannel
            {
                Id = 1,
                Guid = _fiveDayChallengeGuid,
                Name = "Five Day Challenge",
                Items = new ContentChannelItem[]
                {
                    // Day 1
                    new ContentChannelItem
                    {
                        Id = 1,
                        Guid = _fiveDayChallengeDayOneGuid,
                        Title = "Day 1",
                        Content = "",
                        Order = 0,
                        ChildItems = new ContentChannelItemAssociation[]
                        {
                            new ContentChannelItemAssociation
                            {
                                Order = 0,
                                ChildContentChannelItem = new ContentChannelItem
                                {
                                    Id = 11,
                                    Guid = _fiveDayChallengeDayOneItemOneGuid,
                                    Title = "Day 1 Item 1: Pray",
                                    Content = "",
                                    Order = 0,
                                    Attributes = new Dictionary<string, global::Rock.Web.Cache.AttributeCache>(),
                                    AttributeValues = new Dictionary<string, global::Rock.Web.Cache.AttributeValueCache>()
                                }
                            },
                            new ContentChannelItemAssociation
                            {
                                Order = 1,
                                ChildContentChannelItem = new ContentChannelItem
                                {
                                    Id = 12,
                                    Guid = _fiveDayChallengeDayOneItemTwoGuid,
                                    Title = "Day 1 Item 2: Read Bible",
                                    Content = "",
                                    Order = 1,
                                    Attributes = new Dictionary<string, global::Rock.Web.Cache.AttributeCache>(),
                                    AttributeValues = new Dictionary<string, global::Rock.Web.Cache.AttributeValueCache>()
                                }
                            },
                            new ContentChannelItemAssociation
                            {
                                Order = 2,
                                ChildContentChannelItem = new ContentChannelItem
                                {
                                    Id = 13,
                                    Guid = _fiveDayChallengeDayOneItemThreeGuid,
                                    Title = "Day 1 Item 3: Exercise",
                                    Content = "",
                                    Order = 2,
                                    Attributes = new Dictionary<string, global::Rock.Web.Cache.AttributeCache>(),
                                    AttributeValues = new Dictionary<string, global::Rock.Web.Cache.AttributeValueCache>()
                                }
                            }
                        },
                        Attributes = new Dictionary<string, global::Rock.Web.Cache.AttributeCache>(),
                        AttributeValues = new Dictionary<string, global::Rock.Web.Cache.AttributeValueCache>()
                    },

                    // Day 2
                    new ContentChannelItem
                    {
                        Id = 2,
                        Guid = _fiveDayChallengeDayTwoGuid,
                        Title = "Day 2",
                        Content = "",
                        Order = 1,
                        ChildItems = new ContentChannelItemAssociation[]
                        {
                            new ContentChannelItemAssociation
                            {
                                Order = 0,
                                ChildContentChannelItem = new ContentChannelItem
                                {
                                    Id = 21,
                                    Guid = _fiveDayChallengeDayTwoItemOneGuid,
                                    Title = "Day 2 Item 1: Pray",
                                    Content = "",
                                    Order = 0,
                                    Attributes = new Dictionary<string, global::Rock.Web.Cache.AttributeCache>(),
                                    AttributeValues = new Dictionary<string, global::Rock.Web.Cache.AttributeValueCache>()
                                }
                            },
                            new ContentChannelItemAssociation
                            {
                                Order = 1,
                                ChildContentChannelItem = new ContentChannelItem
                                {
                                    Id = 22,
                                    Guid = _fiveDayChallengeDayTwoItemTwoGuid,
                                    Title = "Day 2 Item 2: Read Bible",
                                    Content = "",
                                    Order = 1,
                                    Attributes = new Dictionary<string, global::Rock.Web.Cache.AttributeCache>(),
                                    AttributeValues = new Dictionary<string, global::Rock.Web.Cache.AttributeValueCache>()
                                }
                            },
                            new ContentChannelItemAssociation
                            {
                                Order = 2,
                                ChildContentChannelItem = new ContentChannelItem
                                {
                                    Id = 23,
                                    Guid = _fiveDayChallengeDayTwoItemThreeGuid,
                                    Title = "Day 2 Item 3: Exercise",
                                    Content = "",
                                    Order = 2,
                                    Attributes = new Dictionary<string, global::Rock.Web.Cache.AttributeCache>(),
                                    AttributeValues = new Dictionary<string, global::Rock.Web.Cache.AttributeValueCache>()
                                }
                            }
                        },
                        Attributes = new Dictionary<string, global::Rock.Web.Cache.AttributeCache>(),
                        AttributeValues = new Dictionary<string, global::Rock.Web.Cache.AttributeValueCache>()
                    },

                    // Day 3
                    new ContentChannelItem
                    {
                        Id = 3,
                        Guid = _fiveDayChallengeDayThreeGuid,
                        Title = "Day 3",
                        Content = "",
                        Order = 2,
                        ChildItems = new ContentChannelItemAssociation[]
                        {
                            new ContentChannelItemAssociation
                            {
                                Order = 0,
                                ChildContentChannelItem = new ContentChannelItem
                                {
                                    Id = 31,
                                    Guid = _fiveDayChallengeDayThreeItemOneGuid,
                                    Title = "Day 3 Item 1: Pray",
                                    Content = "",
                                    Order = 0,
                                    Attributes = new Dictionary<string, global::Rock.Web.Cache.AttributeCache>(),
                                    AttributeValues = new Dictionary<string, global::Rock.Web.Cache.AttributeValueCache>()
                                }
                            },
                            new ContentChannelItemAssociation
                            {
                                Order = 1,
                                ChildContentChannelItem = new ContentChannelItem
                                {
                                    Id = 32,
                                    Guid = _fiveDayChallengeDayThreeItemTwoGuid,
                                    Title = "Day 3 Item 2: Read Bible",
                                    Content = "",
                                    Order = 1,
                                    Attributes = new Dictionary<string, global::Rock.Web.Cache.AttributeCache>(),
                                    AttributeValues = new Dictionary<string, global::Rock.Web.Cache.AttributeValueCache>()
                                }
                            },
                            new ContentChannelItemAssociation
                            {
                                Order = 2,
                                ChildContentChannelItem = new ContentChannelItem
                                {
                                    Id = 33,
                                    Guid = _fiveDayChallengeDayThreeItemThreeGuid,
                                    Title = "Day 3 Item 3: Exercise",
                                    Content = "",
                                    Order = 2,
                                    Attributes = new Dictionary<string, global::Rock.Web.Cache.AttributeCache>(),
                                    AttributeValues = new Dictionary<string, global::Rock.Web.Cache.AttributeValueCache>()
                                }
                            }
                        },
                        Attributes = new Dictionary<string, global::Rock.Web.Cache.AttributeCache>(),
                        AttributeValues = new Dictionary<string, global::Rock.Web.Cache.AttributeValueCache>()
                    },

                    // Day 4
                    new ContentChannelItem
                    {
                        Id = 4,
                        Guid = _fiveDayChallengeDayFourGuid,
                        Title = "Day 4",
                        Content = "",
                        Order = 3,
                        ChildItems = new ContentChannelItemAssociation[]
                        {
                            new ContentChannelItemAssociation
                            {
                                Order = 0,
                                ChildContentChannelItem = new ContentChannelItem
                                {
                                    Id = 41,
                                    Guid = _fiveDayChallengeDayFourItemOneGuid,
                                    Title = "Day 4 Item 1: Pray",
                                    Content = "",
                                    Order = 0,
                                    Attributes = new Dictionary<string, global::Rock.Web.Cache.AttributeCache>(),
                                    AttributeValues = new Dictionary<string, global::Rock.Web.Cache.AttributeValueCache>()
                                }
                            },
                            new ContentChannelItemAssociation
                            {
                                Order = 1,
                                ChildContentChannelItem = new ContentChannelItem
                                {
                                    Id = 42,
                                    Guid = _fiveDayChallengeDayFourItemTwoGuid,
                                    Title = "Day 4 Item 2: Read Bible",
                                    Content = "",
                                    Order = 1,
                                    Attributes = new Dictionary<string, global::Rock.Web.Cache.AttributeCache>(),
                                    AttributeValues = new Dictionary<string, global::Rock.Web.Cache.AttributeValueCache>()
                                }
                            }
                        },
                        Attributes = new Dictionary<string, global::Rock.Web.Cache.AttributeCache>(),
                        AttributeValues = new Dictionary<string, global::Rock.Web.Cache.AttributeValueCache>()
                    },

                    // Day 5
                    new ContentChannelItem
                    {
                        Id = 5,
                        Guid = _fiveDayChallengeDayFiveGuid,
                        Title = "Day 5",
                        Content = "",
                        Order = 4,
                        ChildItems = new ContentChannelItemAssociation[]
                        {
                            new ContentChannelItemAssociation
                            {
                                Order = 0,
                                ChildContentChannelItem = new ContentChannelItem
                                {
                                    Id = 51,
                                    Guid = _fiveDayChallengeDayFiveItemOneGuid,
                                    Title = "Day 5 Item 1: Pray",
                                    Content = "",
                                    Order = 0,
                                    Attributes = new Dictionary<string, global::Rock.Web.Cache.AttributeCache>(),
                                    AttributeValues = new Dictionary<string, global::Rock.Web.Cache.AttributeValueCache>()
                                }
                            }
                        },
                        Attributes = new Dictionary<string, global::Rock.Web.Cache.AttributeCache>(),
                        AttributeValues = new Dictionary<string, global::Rock.Web.Cache.AttributeValueCache>()
                    }
                }
            };
        }

        [TestMethod]
        public void CachedDailyChallenge_ItemOrderIsCorrect()
        {
            var dayItem = new ContentChannelItem
            {
                ChildItems = new ContentChannelItemAssociation[]
                {
                    new ContentChannelItemAssociation
                    {
                        Order = 0,
                        ChildContentChannelItem = new ContentChannelItem
                        {
                            Id = 1,
                            Order = 0,
                            Attributes = new Dictionary<string, global::Rock.Web.Cache.AttributeCache>(),
                            AttributeValues = new Dictionary<string, global::Rock.Web.Cache.AttributeValueCache>()
                        }
                    },
                    new ContentChannelItemAssociation
                    {
                        Order = 2,
                        ChildContentChannelItem = new ContentChannelItem
                        {
                            Id = 3,
                            Order = 1,
                            Attributes = new Dictionary<string, global::Rock.Web.Cache.AttributeCache>(),
                            AttributeValues = new Dictionary<string, global::Rock.Web.Cache.AttributeValueCache>()
                        }
                    },
                    new ContentChannelItemAssociation
                    {
                        Order = 1,
                        ChildContentChannelItem = new ContentChannelItem
                        {
                            Id = 2,
                            Order = 2,
                            Attributes = new Dictionary<string, global::Rock.Web.Cache.AttributeCache>(),
                            AttributeValues = new Dictionary<string, global::Rock.Web.Cache.AttributeValueCache>()
                        }
                    }
                },
                Attributes = new Dictionary<string, global::Rock.Web.Cache.AttributeCache>(),
                AttributeValues = new Dictionary<string, global::Rock.Web.Cache.AttributeValueCache>()
            };

            var dailyChallenge = new CachedDailyChallenge( dayItem );

            // Assert the initialized order is correct.
            var unorderedIds = dailyChallenge.ChallengeItems.Select( i => i.Id ).ToList();
            CollectionAssert.AreEqual( new List<int> { 1, 2, 3 }, unorderedIds );

            // Assert the order property is correct.
            var orderedIds = dailyChallenge.ChallengeItems.OrderBy( i => i.Order ).Select( i => i.Id ).ToList();
            CollectionAssert.AreEqual( new List<int> { 1, 2, 3 }, orderedIds );
        }

        [TestMethod]
        public void IsDayComplete_ReturnsTrueWithNoChallengeItemsDefined()
        {
            var dayData = new InteractionChallengeDayData();
            var challengeItems = new CachedChallengeItem[]
            {
            };

            Assert.IsTrue( IsDayComplete( dayData, challengeItems ) );
        }

        [TestMethod]
        public void IsDayComplete_ReturnsFalseWithNoDayData()
        {
            var dayItem = new ContentChannelItem
            {
                ChildItems = new ContentChannelItemAssociation[]
                {
                    new ContentChannelItemAssociation
                    {
                        Order = 0,
                        ChildContentChannelItem = new ContentChannelItem
                        {
                            Id = 1,
                            Order = 0,
                            Attributes = new Dictionary<string, global::Rock.Web.Cache.AttributeCache>(),
                            AttributeValues = new Dictionary<string, global::Rock.Web.Cache.AttributeValueCache>()
                        }
                    },
                },
                Attributes = new Dictionary<string, global::Rock.Web.Cache.AttributeCache>(),
                AttributeValues = new Dictionary<string, global::Rock.Web.Cache.AttributeValueCache>()
            };

            var dailyChallenge = new CachedDailyChallenge( dayItem );

            var dayData = new InteractionChallengeDayData();

            Assert.IsFalse( IsDayComplete( dayData, dailyChallenge.ChallengeItems ) );
        }

        [TestMethod]
        public void IsDayComplete_ReturnsFalseWithPartialDayData()
        {
            var dayItem = new ContentChannelItem
            {
                ChildItems = new ContentChannelItemAssociation[]
                {
                    new ContentChannelItemAssociation
                    {
                        Order = 0,
                        ChildContentChannelItem = new ContentChannelItem
                        {
                            Id = 1,
                            Order = 0,
                            Attributes = new Dictionary<string, global::Rock.Web.Cache.AttributeCache>(),
                            AttributeValues = new Dictionary<string, global::Rock.Web.Cache.AttributeValueCache>()
                        }
                    },
                    new ContentChannelItemAssociation
                    {
                        Order = 1,
                        ChildContentChannelItem = new ContentChannelItem
                        {
                            Id = 2,
                            Order = 0,
                            Attributes = new Dictionary<string, global::Rock.Web.Cache.AttributeCache>(),
                            AttributeValues = new Dictionary<string, global::Rock.Web.Cache.AttributeValueCache>()
                        }
                    },
                },
                Attributes = new Dictionary<string, global::Rock.Web.Cache.AttributeCache>(),
                AttributeValues = new Dictionary<string, global::Rock.Web.Cache.AttributeValueCache>()
            };

            var dailyChallenge = new CachedDailyChallenge( dayItem );

            var dayData = new InteractionChallengeDayData();
            dayData.Items[1] = new InteractionChallengeItemData
            {
                IsComplete = true
            };

            Assert.IsFalse( IsDayComplete( dayData, dailyChallenge.ChallengeItems ) );
        }

        [TestMethod]
        public void IsDayComplete_ReturnsTrueWithCompletedDayData()
        {
            var dayItem = new ContentChannelItem
            {
                ChildItems = new ContentChannelItemAssociation[]
                {
                    new ContentChannelItemAssociation
                    {
                        Order = 0,
                        ChildContentChannelItem = new ContentChannelItem
                        {
                            Id = 1,
                            Order = 0,
                            Attributes = new Dictionary<string, global::Rock.Web.Cache.AttributeCache>(),
                            AttributeValues = new Dictionary<string, global::Rock.Web.Cache.AttributeValueCache>()
                        }
                    },
                    new ContentChannelItemAssociation
                    {
                        Order = 1,
                        ChildContentChannelItem = new ContentChannelItem
                        {
                            Id = 2,
                            Order = 0,
                            Attributes = new Dictionary<string, global::Rock.Web.Cache.AttributeCache>(),
                            AttributeValues = new Dictionary<string, global::Rock.Web.Cache.AttributeValueCache>()
                        }
                    },
                },
                Attributes = new Dictionary<string, global::Rock.Web.Cache.AttributeCache>(),
                AttributeValues = new Dictionary<string, global::Rock.Web.Cache.AttributeValueCache>()
            };

            var dailyChallenge = new CachedDailyChallenge( dayItem );

            var dayData = new InteractionChallengeDayData();
            dayData.Items[1] = new InteractionChallengeItemData
            {
                IsComplete = true
            };
            dayData.Items[2] = new InteractionChallengeItemData
            {
                IsComplete = true
            };

            Assert.IsTrue( IsDayComplete( dayData, dailyChallenge.ChallengeItems ) );
        }

        [TestMethod]
        public void GetMissedDays_ReturnsZeroDaysWhenNoRecentInteractions()
        {
            var dailyChallenge = new CachedChallenge( GetFiveDayChallengeChannel() );
            var recentInteractions = new List<Interaction>
            {
            };

            var missedDays = GetMissedDays( dailyChallenge, recentInteractions, 1 );

            Assert.AreEqual( 0, missedDays.Item1.Count );
        }

        [TestMethod]
        public void GetMissedDays_ReturnsZeroDaysWhenMissedTwoFullDaysWithOneDayBackfill()
        {
            var dailyChallenge = new CachedChallenge( GetFiveDayChallengeChannel() );
            var recentInteractions = new List<Interaction>
            {
                new Interaction
                {
                    InteractionDateTime = RockDateTime.Now.AddDays( -3 ),
                    EntityId = 1,
                    InteractionData = new InteractionChallengeDayData
                    {
                        IsComplete = true
                    }.ToJson()
                }
            };

            var missedDays = GetMissedDays( dailyChallenge, recentInteractions, 1 );

            Assert.AreEqual( 0, missedDays.Item1.Count );
        }

        [TestMethod]
        public void GetMissedDays_ReturnsZeroDaysWhenMissedOnePartialDayAndOneFullDayWithOneDayBackfill()
        {
            var dailyChallenge = new CachedChallenge( GetFiveDayChallengeChannel() );
            var recentInteractions = new List<Interaction>
            {
                new Interaction
                {
                    InteractionDateTime = RockDateTime.Now.AddDays( -2 ),
                    EntityId = 1,
                    InteractionData = new InteractionChallengeDayData
                    {
                        IsComplete = false
                    }.ToJson()
                }
            };

            var missedDays = GetMissedDays( dailyChallenge, recentInteractions, 1 );

            Assert.AreEqual( 0, missedDays.Item1.Count );
        }

        [TestMethod]
        public void GetMissedDays_ReturnsOneDayWhenMissedOneDayWithOneDayBackfill()
        {
            var dailyChallenge = new CachedChallenge( GetFiveDayChallengeChannel() );
            var recentInteractions = new List<Interaction>
            {
                new Interaction
                {
                    InteractionDateTime = RockDateTime.Now.AddDays( -2 ),
                    EntityId = 1,
                    InteractionData = new InteractionChallengeDayData
                    {
                        IsComplete = true
                    }.ToJson()
                }
            };

            var missedDays = GetMissedDays( dailyChallenge, recentInteractions, 1 );

            Assert.AreEqual( 1, missedDays.Item1.Count );
        }

        [TestMethod]
        public void GetMissedDays_ReturnsTwoDaysWhenMissedOnePartialDayAndOneFullDayWithTwoDayBackfill()
        {
            var dailyChallenge = new CachedChallenge( GetFiveDayChallengeChannel() );
            var recentInteractions = new List<Interaction>
            {
                new Interaction
                {
                    InteractionDateTime = RockDateTime.Now.AddDays( -2 ),
                    EntityId = 1,
                    InteractionData = new InteractionChallengeDayData
                    {
                        IsComplete = false
                    }.ToJson()
                }
            };

            var missedDays = GetMissedDays( dailyChallenge, recentInteractions, 2 );

            Assert.AreEqual( 2, missedDays.Item1.Count );
        }

        [TestMethod]
        public void GetMissedDays_ReturnsTwoDaysWhenMissedTwoDaysWithTwoDayBackfill()
        {
            var dailyChallenge = new CachedChallenge( GetFiveDayChallengeChannel() );
            var recentInteractions = new List<Interaction>
            {
                new Interaction
                {
                    InteractionDateTime = RockDateTime.Now.AddDays( -3 ),
                    EntityId = 1,
                    InteractionData = new InteractionChallengeDayData
                    {
                        IsComplete = true
                    }.ToJson()
                }
            };

            var missedDays = GetMissedDays( dailyChallenge, recentInteractions, 2 );

            Assert.AreEqual( 2, missedDays.Item1.Count );
        }

        [TestMethod]
        public void GetCurrentDailyChallenge_ReturnsDayOneWhenMissedOneFullDayWithOneDayBackfill()
        {
            var cachedChallenge = new CachedChallenge( GetFiveDayChallengeChannel() );
            var recentInteractions = new List<Interaction>
            {
                new Interaction
                {
                    InteractionDateTime = RockDateTime.Now.AddDays( -2 ),
                    EntityId = 1,
                    InteractionData = new InteractionChallengeDayData
                    {
                        IsComplete = true,
                        Items = new Dictionary<int, InteractionChallengeItemData>
                        {
                            [11] = new InteractionChallengeItemData { IsComplete = true },
                            [12] = new InteractionChallengeItemData { IsComplete = true },
                            [13] = new InteractionChallengeItemData { IsComplete = true }
                        }
                    }.ToJson()
                }
            };

            var currentChallenge = GetCurrentDailyChallenge( cachedChallenge, recentInteractions, 1 );

            Assert.AreEqual( _fiveDayChallengeDayOneGuid, currentChallenge.Guid );
            Assert.AreEqual( 0, currentChallenge.ChallengeItemValues.Count );
        }

        [TestMethod]
        public void GetCurrentDailyChallenge_ReturnsDayTwoWhenMissedZeroDaysWithOneDayBackfill()
        {
            var cachedChallenge = new CachedChallenge( GetFiveDayChallengeChannel() );
            var recentInteractions = new List<Interaction>
            {
                new Interaction
                {
                    InteractionDateTime = RockDateTime.Now.AddDays( -1 ),
                    EntityId = 1,
                    InteractionData = new InteractionChallengeDayData
                    {
                        IsComplete = true
                    }.ToJson()
                }
            };

            var currentChallenge = GetCurrentDailyChallenge( cachedChallenge, recentInteractions, 1 );

            Assert.AreEqual( _fiveDayChallengeDayTwoGuid, currentChallenge.Guid );
            Assert.AreEqual( 0, currentChallenge.ChallengeItemValues.Count );
        }

        [TestMethod]
        public void GetCurrentDailyChallenge_ReturnsDayTwoWhenMissedZeroDaysAndPartialTodayWithOneDayBackfill()
        {
            var cachedChallenge = new CachedChallenge( GetFiveDayChallengeChannel() );
            var recentInteractions = new List<Interaction>
            {
                new Interaction
                {
                    InteractionDateTime = RockDateTime.Now.AddDays( -1 ),
                    EntityId = 1,
                    InteractionData = new InteractionChallengeDayData
                    {
                        IsComplete = true
                    }.ToJson()
                },
                new Interaction
                {
                    InteractionDateTime = RockDateTime.Now,
                    EntityId = 2,
                    InteractionData = new InteractionChallengeDayData
                    {
                        IsComplete = false,
                        Items = new Dictionary<int, InteractionChallengeItemData>
                        {
                            [21] = new InteractionChallengeItemData { IsComplete = true },
                            [22] = new InteractionChallengeItemData { IsComplete = false },
                            [23] = new InteractionChallengeItemData { IsComplete = false }
                        }
                    }.ToJson()
                }
            };

            var currentChallenge = GetCurrentDailyChallenge( cachedChallenge, recentInteractions, 1 );

            Assert.AreEqual( _fiveDayChallengeDayTwoGuid, currentChallenge.Guid );
            Assert.AreEqual( 3, currentChallenge.ChallengeItemValues.Count );
            Assert.IsTrue( currentChallenge.ChallengeItemValues[_fiveDayChallengeDayTwoItemOneGuid].IsComplete );
            Assert.IsFalse( currentChallenge.ChallengeItemValues[_fiveDayChallengeDayTwoItemTwoGuid].IsComplete );
            Assert.IsFalse( currentChallenge.ChallengeItemValues[_fiveDayChallengeDayTwoItemThreeGuid].IsComplete );
        }

        [TestMethod]
        public void GetCurrentDailyChallenge_ReturnsDayTwoWhenMissedZeroDaysAndCompletedTodayWithOneDayBackfill()
        {
            var cachedChallenge = new CachedChallenge( GetFiveDayChallengeChannel() );
            var recentInteractions = new List<Interaction>
            {
                new Interaction
                {
                    InteractionDateTime = RockDateTime.Now.AddDays( -1 ),
                    EntityId = 1,
                    InteractionData = new InteractionChallengeDayData
                    {
                        IsComplete = true
                    }.ToJson()
                },
                new Interaction
                {
                    InteractionDateTime = RockDateTime.Now,
                    EntityId = 2,
                    InteractionData = new InteractionChallengeDayData
                    {
                        IsComplete = true,
                        Items = new Dictionary<int, InteractionChallengeItemData>
                        {
                            [21] = new InteractionChallengeItemData { IsComplete = true },
                            [22] = new InteractionChallengeItemData { IsComplete = true },
                            [23] = new InteractionChallengeItemData { IsComplete = true }
                        }
                    }.ToJson()
                }
            };

            var currentChallenge = GetCurrentDailyChallenge( cachedChallenge, recentInteractions, 1 );

            Assert.AreEqual( _fiveDayChallengeDayTwoGuid, currentChallenge.Guid );
            Assert.AreEqual( 3, currentChallenge.ChallengeItemValues.Count );
            Assert.IsTrue( currentChallenge.ChallengeItemValues[_fiveDayChallengeDayTwoItemOneGuid].IsComplete );
            Assert.IsTrue( currentChallenge.ChallengeItemValues[_fiveDayChallengeDayTwoItemTwoGuid].IsComplete );
            Assert.IsTrue( currentChallenge.ChallengeItemValues[_fiveDayChallengeDayTwoItemThreeGuid].IsComplete );
        }
    }
}
