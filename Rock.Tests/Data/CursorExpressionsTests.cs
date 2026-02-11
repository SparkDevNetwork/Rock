using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Model;

namespace Rock.Tests.Data
{
    [TestClass]
    public class CursorExpressionsTests
    {
        [TestMethod]
        public void IntegerAscending()
        {
            var campuses = new List<Campus>
            {
                new Campus
                {
                    Id = 1,
                    CreatedDateTime = new DateTime( 2024, 1, 1 ),
                },
                new Campus
                {
                    Id = 2,
                    CreatedDateTime = null,
                },
                new Campus
                {
                    Id = 3,
                    CreatedDateTime = null,
                },
                new Campus
                {
                    Id = 4,
                    CreatedDateTime = null,
                },
            };

            var set1 = campuses.OrderBy( c => c.Id ).ToList();

            Assert.AreEqual( 1, set1[0].Id );
            Assert.AreEqual( 2, set1[1].Id );
            Assert.AreEqual( 3, set1[2].Id );
            Assert.AreEqual( 4, set1[3].Id );

            var expr = CursorExpressions.BuildCursorPredicate<Campus>( new List<CursorOrderInfo>
            {
                new CursorOrderInfo
                {
                    KeySelector = (Expression<Func<Campus, int>>)(c => c.Id),
                    PropertyPath = "Id",
                    Descending = false
                },
            }, new Dictionary<string, object>
            {
                ["Id"] = set1[1].Id,
            } );

            var str = expr.ToString();
            var set2 = campuses.AsQueryable().OrderBy( c => c.Id ).Where( expr ).ToList();

            Assert.AreEqual( set1[2].Id, set2[0].Id );
            Assert.AreEqual( set1[3].Id, set2[1].Id );
        }

        [TestMethod]
        public void IntegerDescending()
        {
            var campuses = new List<Campus>
            {
                new Campus
                {
                    Id = 1,
                    CreatedDateTime = new DateTime( 2024, 1, 1 ),
                },
                new Campus
                {
                    Id = 2,
                    CreatedDateTime = null,
                },
                new Campus
                {
                    Id = 3,
                    CreatedDateTime = null,
                },
                new Campus
                {
                    Id = 4,
                    CreatedDateTime = null,
                },
            };

            var set1 = campuses.OrderByDescending( c => c.Id ).ToList();

            Assert.AreEqual( 4, set1[0].Id );
            Assert.AreEqual( 3, set1[1].Id );
            Assert.AreEqual( 2, set1[2].Id );
            Assert.AreEqual( 1, set1[3].Id );

            var expr = CursorExpressions.BuildCursorPredicate<Campus>( new List<CursorOrderInfo>
            {
                new CursorOrderInfo
                {
                    KeySelector = (Expression<Func<Campus, int>>)(c => c.Id),
                    PropertyPath = "Id",
                    Descending = true
                },
            }, new Dictionary<string, object>
            {
                ["Id"] = set1[1].Id,
            } );

            var str = expr.ToString();
            var set2 = campuses.AsQueryable().OrderByDescending( c => c.Id ).Where( expr ).ToList();

            Assert.AreEqual( set1[2].Id, set2[0].Id );
            Assert.AreEqual( set1[3].Id, set2[1].Id );
        }

        [TestMethod]
        public void IntegerAsLongAscending()
        {
            var campuses = new List<Campus>
            {
                new Campus
                {
                    Id = 1,
                    CreatedDateTime = new DateTime( 2024, 1, 1 ),
                },
                new Campus
                {
                    Id = 2,
                    CreatedDateTime = null,
                },
                new Campus
                {
                    Id = 3,
                    CreatedDateTime = null,
                },
                new Campus
                {
                    Id = 4,
                    CreatedDateTime = null,
                },
            };

            var set1 = campuses.OrderBy( c => c.Id ).ToList();

            Assert.AreEqual( 1, set1[0].Id );
            Assert.AreEqual( 2, set1[1].Id );
            Assert.AreEqual( 3, set1[2].Id );
            Assert.AreEqual( 4, set1[3].Id );

            var expr = CursorExpressions.BuildCursorPredicate<Campus>( new List<CursorOrderInfo>
            {
                new CursorOrderInfo
                {
                    KeySelector = (Expression<Func<Campus, int>>)(c => c.Id),
                    PropertyPath = "Id",
                    Descending = false
                },
            }, new Dictionary<string, object>
            {
                ["Id"] = ( long ) set1[1].Id,
            } );

            var str = expr.ToString();
            var set2 = campuses.AsQueryable().OrderBy( c => c.Id ).Where( expr ).ToList();

            Assert.AreEqual( set1[2].Id, set2[0].Id );
            Assert.AreEqual( set1[3].Id, set2[1].Id );
        }

        [TestMethod]
        public void IntegerAsLongDescending()
        {
            var campuses = new List<Campus>
            {
                new Campus
                {
                    Id = 1,
                    CreatedDateTime = new DateTime( 2024, 1, 1 ),
                },
                new Campus
                {
                    Id = 2,
                    CreatedDateTime = null,
                },
                new Campus
                {
                    Id = 3,
                    CreatedDateTime = null,
                },
                new Campus
                {
                    Id = 4,
                    CreatedDateTime = null,
                },
            };

            var set1 = campuses.OrderByDescending( c => c.Id ).ToList();

            Assert.AreEqual( 4, set1[0].Id );
            Assert.AreEqual( 3, set1[1].Id );
            Assert.AreEqual( 2, set1[2].Id );
            Assert.AreEqual( 1, set1[3].Id );

            var expr = CursorExpressions.BuildCursorPredicate<Campus>( new List<CursorOrderInfo>
            {
                new CursorOrderInfo
                {
                    KeySelector = (Expression<Func<Campus, int>>)(c => c.Id),
                    PropertyPath = "Id",
                    Descending = true
                },
            }, new Dictionary<string, object>
            {
                ["Id"] = ( long ) set1[1].Id,
            } );

            var str = expr.ToString();
            var set2 = campuses.AsQueryable().OrderByDescending( c => c.Id ).Where( expr ).ToList();

            Assert.AreEqual( set1[2].Id, set2[0].Id );
            Assert.AreEqual( set1[3].Id, set2[1].Id );
        }

        [TestMethod]
        public void FalseBoolDescending_ThenIntegerDescending()
        {
            var campuses = new List<Campus>
            {
                new Campus
                {
                    Id = 1,
                    CreatedDateTime = new DateTime( 2024, 1, 1 ),
                },
                new Campus
                {
                    Id = 2,
                    CreatedDateTime = null,
                },
                new Campus
                {
                    Id = 3,
                    CreatedDateTime = null,
                },
                new Campus
                {
                    Id = 4,
                    CreatedDateTime = null,
                },
            };

            var set1 = campuses.OrderByDescending( c => c.CreatedDateTime.HasValue ).ThenByDescending( c => c.Id ).ToList();

            Assert.AreEqual( 1, set1[0].Id );
            Assert.AreEqual( 4, set1[1].Id );
            Assert.AreEqual( 3, set1[2].Id );
            Assert.AreEqual( 2, set1[3].Id );

            var expr = CursorExpressions.BuildCursorPredicate<Campus>( new List<CursorOrderInfo>
            {
                new CursorOrderInfo
                {
                    KeySelector = (Expression<Func<Campus, bool>>)(c => c.CreatedDateTime.HasValue),
                    PropertyPath = "CreatedDateTime.HasValue",
                    Descending = true
                },
                new CursorOrderInfo
                {
                    KeySelector = (Expression<Func<Campus, int>>)(c => c.Id),
                    PropertyPath = "Id",
                    Descending = true
                },
            }, new Dictionary<string, object>
            {
                ["CreatedDateTime.HasValue"] = set1[1].CreatedDateTime.HasValue,
                ["Id"] = set1[1].Id,
            } );

            var str = expr.ToString();
            var set2 = campuses.AsQueryable().OrderByDescending( c => c.CreatedDateTime.HasValue ).ThenByDescending( c => c.Id ).Where( expr ).ToList();

            Assert.AreEqual( set1[2].Id, set2[0].Id );
            Assert.AreEqual( set1[3].Id, set2[1].Id );
        }

        [TestMethod]
        public void TrueBoolDescending_ThenIntegerDescending()
        {
            var campuses = new List<Campus>
            {
                new Campus
                {
                    Id = 1,
                    CreatedDateTime = new DateTime( 2024, 1, 1 ),
                },
                new Campus
                {
                    Id = 2,
                    CreatedDateTime = null,
                },
                new Campus
                {
                    Id = 3,
                    CreatedDateTime = new DateTime( 2024, 2, 1 ),
                },
                new Campus
                {
                    Id = 4,
                    CreatedDateTime = null,
                },
            };

            var set1 = campuses.OrderByDescending( c => c.CreatedDateTime.HasValue ).ThenByDescending( c => c.Id ).ToList();

            Assert.AreEqual( 3, set1[0].Id );
            Assert.AreEqual( 1, set1[1].Id );
            Assert.AreEqual( 4, set1[2].Id );
            Assert.AreEqual( 2, set1[3].Id );

            var expr = CursorExpressions.BuildCursorPredicate<Campus>( new List<CursorOrderInfo>
            {
                new CursorOrderInfo
                {
                    KeySelector = (Expression<Func<Campus, bool>>)(c => c.CreatedDateTime.HasValue),
                    PropertyPath = "CreatedDateTime.HasValue",
                    Descending = true
                },
                new CursorOrderInfo
                {
                    KeySelector = (Expression<Func<Campus, int>>)(c => c.Id),
                    PropertyPath = "Id",
                    Descending = true
                },
            }, new Dictionary<string, object>
            {
                ["CreatedDateTime.HasValue"] = set1[1].CreatedDateTime.HasValue,
                ["Id"] = set1[1].Id,
            } );

            var str = expr.ToString();
            var set2 = campuses.AsQueryable().OrderByDescending( c => c.CreatedDateTime.HasValue ).ThenByDescending( c => c.Id ).Where( expr ).ToList();

            Assert.AreEqual( set1[2].Id, set2[0].Id );
            Assert.AreEqual( set1[3].Id, set2[1].Id );
        }

        [TestMethod]
        public void FalseBoolAscending_ThenIntegerDescending()
        {
            var campuses = new List<Campus>
            {
                new Campus
                {
                    Id = 1,
                    CreatedDateTime = new DateTime( 2024, 1, 1 ),
                },
                new Campus
                {
                    Id = 2,
                    CreatedDateTime = null,
                },
                new Campus
                {
                    Id = 3,
                    CreatedDateTime = null,
                },
                new Campus
                {
                    Id = 4,
                    CreatedDateTime = null,
                },
            };

            var set1 = campuses.OrderBy( c => c.CreatedDateTime.HasValue ).ThenByDescending( c => c.Id ).ToList();

            Assert.AreEqual( 4, set1[0].Id );
            Assert.AreEqual( 3, set1[1].Id );
            Assert.AreEqual( 2, set1[2].Id );
            Assert.AreEqual( 1, set1[3].Id );

            var expr = CursorExpressions.BuildCursorPredicate<Campus>( new List<CursorOrderInfo>
            {
                new CursorOrderInfo
                {
                    KeySelector = (Expression<Func<Campus, bool>>)(c => c.CreatedDateTime.HasValue),
                    PropertyPath = "CreatedDateTime.HasValue",
                    Descending = false,
                },
                new CursorOrderInfo
                {
                    KeySelector = (Expression<Func<Campus, int>>)(c => c.Id),
                    PropertyPath = "Id",
                    Descending = true,
                },
            }, new Dictionary<string, object>
            {
                ["CreatedDateTime.HasValue"] = set1[1].CreatedDateTime.HasValue,
                ["Id"] = set1[1].Id,
            } );

            var str = expr.ToString();
            var set2 = campuses.AsQueryable().OrderBy( c => c.CreatedDateTime.HasValue ).ThenByDescending( c => c.Id ).Where( expr ).ToList();

            Assert.AreEqual( set1[2].Id, set2[0].Id );
            Assert.AreEqual( set1[3].Id, set2[1].Id );
        }

        [TestMethod]
        public void TrueBoolAscending_ThenIntegerDescending()
        {
            var campuses = new List<Campus>
            {
                new Campus
                {
                    Id = 1,
                    CreatedDateTime = new DateTime( 2024, 1, 1 ),
                },
                new Campus
                {
                    Id = 2,
                    CreatedDateTime = new DateTime( 2024, 3, 1 ),
                },
                new Campus
                {
                    Id = 3,
                    CreatedDateTime = new DateTime( 2024, 2, 1 ),
                },
                new Campus
                {
                    Id = 4,
                    CreatedDateTime = null,
                },
            };

            var set1 = campuses.OrderBy( c => c.CreatedDateTime.HasValue ).ThenByDescending( c => c.Id ).ToList();

            Assert.AreEqual( 4, set1[0].Id );
            Assert.AreEqual( 3, set1[1].Id );
            Assert.AreEqual( 2, set1[2].Id );
            Assert.AreEqual( 1, set1[3].Id );

            var expr = CursorExpressions.BuildCursorPredicate<Campus>( new List<CursorOrderInfo>
            {
                new CursorOrderInfo
                {
                    KeySelector = (Expression<Func<Campus, bool>>)(c => c.CreatedDateTime.HasValue),
                    PropertyPath = "CreatedDateTime.HasValue",
                    Descending = false,
                },
                new CursorOrderInfo
                {
                    KeySelector = (Expression<Func<Campus, int>>)(c => c.Id),
                    PropertyPath = "Id",
                    Descending = true,
                },
            }, new Dictionary<string, object>
            {
                ["CreatedDateTime.HasValue"] = set1[1].CreatedDateTime.HasValue,
                ["Id"] = set1[1].Id,
            } );

            var str = expr.ToString();
            var set2 = campuses.AsQueryable().OrderBy( c => c.CreatedDateTime.HasValue ).ThenByDescending( c => c.Id ).Where( expr ).ToList();

            Assert.AreEqual( set1[2].Id, set2[0].Id );
            Assert.AreEqual( set1[3].Id, set2[1].Id );
        }

        [TestMethod]
        public void NullDateTimeAscending_ThenIntegerAscending()
        {
            var campuses = new List<Campus>
            {
                new Campus
                {
                    Id = 1,
                    CreatedDateTime = new DateTime( 2024, 1, 1 ),
                },
                new Campus
                {
                    Id = 2,
                    CreatedDateTime = null,
                },
                new Campus
                {
                    Id = 3,
                    CreatedDateTime = null,
                },
                new Campus
                {
                    Id = 4,
                    CreatedDateTime = null,
                },
            };

            var set1 = campuses.OrderBy( c => c.CreatedDateTime ).ThenBy( c => c.Id ).ToList();

            Assert.AreEqual( 2, set1[0].Id );
            Assert.AreEqual( 3, set1[1].Id );
            Assert.AreEqual( 4, set1[2].Id );
            Assert.AreEqual( 1, set1[3].Id );

            var expr = CursorExpressions.BuildCursorPredicate<Campus>( new List<CursorOrderInfo>
            {
                new CursorOrderInfo
                {
                    KeySelector = (Expression<Func<Campus, DateTime?>>)(c => c.CreatedDateTime),
                    PropertyPath = "CreatedDateTime",
                    Descending = false
                },
                new CursorOrderInfo
                {
                    KeySelector = (Expression<Func<Campus, int>>)(c => c.Id),
                    PropertyPath = "Id",
                    Descending = false
                },
            }, new Dictionary<string, object>
            {
                ["CreatedDateTime"] = set1[1].CreatedDateTime,
                ["Id"] = set1[1].Id,
            } );

            var str = expr.ToString();
            var set2 = campuses.AsQueryable().OrderBy( c => c.CreatedDateTime ).ThenBy( c => c.Id ).Where( expr ).ToList();

            Assert.AreEqual( set1[2].Id, set2[0].Id );
            Assert.AreEqual( set1[3].Id, set2[1].Id );
        }

        [TestMethod]
        public void NonNullDateTimeAscending_ThenIntegerAscending()
        {
            var campuses = new List<Campus>
            {
                new Campus
                {
                    Id = 1,
                    CreatedDateTime = new DateTime( 2024, 1, 1 ),
                },
                new Campus
                {
                    Id = 2,
                    CreatedDateTime = new DateTime( 2024, 2, 1 ),
                },
                new Campus
                {
                    Id = 3,
                    CreatedDateTime = null,
                },
                new Campus
                {
                    Id = 4,
                    CreatedDateTime = new DateTime( 2023, 1, 1 ),
                },
            };

            var set1 = campuses.OrderBy( c => c.CreatedDateTime ).ThenBy( c => c.Id ).ToList();

            Assert.AreEqual( 3, set1[0].Id );
            Assert.AreEqual( 4, set1[1].Id );
            Assert.AreEqual( 1, set1[2].Id );
            Assert.AreEqual( 2, set1[3].Id );

            var expr = CursorExpressions.BuildCursorPredicate<Campus>( new List<CursorOrderInfo>
            {
                new CursorOrderInfo
                {
                    KeySelector = (Expression<Func<Campus, DateTime?>>)(c => c.CreatedDateTime),
                    PropertyPath = "CreatedDateTime",
                    Descending = false
                },
                new CursorOrderInfo
                {
                    KeySelector = (Expression<Func<Campus, int>>)(c => c.Id),
                    PropertyPath = "Id",
                    Descending = false
                },
            }, new Dictionary<string, object>
            {
                ["CreatedDateTime"] = set1[1].CreatedDateTime,
                ["Id"] = set1[1].Id,
            } );

            var str = expr.ToString();
            var set2 = campuses.AsQueryable().OrderBy( c => c.CreatedDateTime ).ThenBy( c => c.Id ).Where( expr ).ToList();

            Assert.AreEqual( set1[2].Id, set2[0].Id );
            Assert.AreEqual( set1[3].Id, set2[1].Id );
        }

        [TestMethod]
        public void NullDateTimeDescending_ThenIntegerAscending()
        {
            var campuses = new List<Campus>
            {
                new Campus
                {
                    Id = 1,
                    CreatedDateTime = new DateTime( 2024, 1, 1 ),
                },
                new Campus
                {
                    Id = 2,
                    CreatedDateTime = null,
                },
                new Campus
                {
                    Id = 3,
                    CreatedDateTime = null,
                },
                new Campus
                {
                    Id = 4,
                    CreatedDateTime = null,
                },
            };

            var set1 = campuses.OrderByDescending( c => c.CreatedDateTime ).ThenBy( c => c.Id ).ToList();

            Assert.AreEqual( 1, set1[0].Id );
            Assert.AreEqual( 2, set1[1].Id );
            Assert.AreEqual( 3, set1[2].Id );
            Assert.AreEqual( 4, set1[3].Id );

            var expr = CursorExpressions.BuildCursorPredicate<Campus>( new List<CursorOrderInfo>
            {
                new CursorOrderInfo
                {
                    KeySelector = (Expression<Func<Campus, DateTime?>>)(c => c.CreatedDateTime),
                    PropertyPath = "CreatedDateTime",
                    Descending = true
                },
                new CursorOrderInfo
                {
                    KeySelector = (Expression<Func<Campus, int>>)(c => c.Id),
                    PropertyPath = "Id",
                    Descending = false
                },
            }, new Dictionary<string, object>
            {
                ["CreatedDateTime"] = set1[1].CreatedDateTime,
                ["Id"] = set1[1].Id,
            } );

            var str = expr.ToString();
            var set2 = campuses.AsQueryable().OrderByDescending( c => c.CreatedDateTime ).ThenBy( c => c.Id ).Where( expr ).ToList();

            Assert.AreEqual( set1[2].Id, set2[0].Id );
            Assert.AreEqual( set1[3].Id, set2[1].Id );
        }

        [TestMethod]
        public void NonNullDateTimeDescending_ThenIntegerAscending()
        {
            var campuses = new List<Campus>
            {
                new Campus
                {
                    Id = 1,
                    CreatedDateTime = new DateTime( 2024, 1, 1 ),
                },
                new Campus
                {
                    Id = 2,
                    CreatedDateTime = new DateTime( 2024, 2, 1 ),
                },
                new Campus
                {
                    Id = 3,
                    CreatedDateTime = null,
                },
                new Campus
                {
                    Id = 4,
                    CreatedDateTime = new DateTime( 2023, 1, 1 ),
                },
            };

            var set1 = campuses.OrderByDescending( c => c.CreatedDateTime ).ThenBy( c => c.Id ).ToList();

            Assert.AreEqual( 2, set1[0].Id );
            Assert.AreEqual( 1, set1[1].Id );
            Assert.AreEqual( 4, set1[2].Id );
            Assert.AreEqual( 3, set1[3].Id );

            var expr = CursorExpressions.BuildCursorPredicate<Campus>( new List<CursorOrderInfo>
            {
                new CursorOrderInfo
                {
                    KeySelector = (Expression<Func<Campus, DateTime?>>)(c => c.CreatedDateTime),
                    PropertyPath = "CreatedDateTime",
                    Descending = true
                },
                new CursorOrderInfo
                {
                    KeySelector = (Expression<Func<Campus, int>>)(c => c.Id),
                    PropertyPath = "Id",
                    Descending = false
                },
            }, new Dictionary<string, object>
            {
                ["CreatedDateTime"] = set1[1].CreatedDateTime,
                ["Id"] = set1[1].Id,
            } );

            var str = expr.ToString();
            var set2 = campuses.AsQueryable().OrderByDescending( c => c.CreatedDateTime ).ThenBy( c => c.Id ).Where( expr ).ToList();

            Assert.AreEqual( set1[2].Id, set2[0].Id );
            Assert.AreEqual( set1[3].Id, set2[1].Id );
        }

        [TestMethod]
        public void StringAscending_ThenBoolAscending_ThenIntegerAscending()
        {
            var campuses = new List<Campus>
            {
                new Campus
                {
                    Id = 1,
                    Name = "Beta",
                    CreatedDateTime = new DateTime( 2024, 1, 1 ),
                },
                new Campus
                {
                    Id = 2,
                    Name = "Alpha",
                    CreatedDateTime = null,
                },
                new Campus
                {
                    Id = 3,
                    Name = "Beta",
                    CreatedDateTime = null,
                },
                new Campus
                {
                    Id = 4,
                    Name = "Alpha",
                    CreatedDateTime = null,
                },
            };

            var set1 = campuses.OrderBy( c => c.Name ).ThenBy( c => c.CreatedDateTime.HasValue ).ThenBy( c => c.Id ).ToList();

            Assert.AreEqual( 2, set1[0].Id );
            Assert.AreEqual( 4, set1[1].Id );
            Assert.AreEqual( 3, set1[2].Id );
            Assert.AreEqual( 1, set1[3].Id );

            var expr = CursorExpressions.BuildCursorPredicate<Campus>( new List<CursorOrderInfo>
            {
                new CursorOrderInfo
                {
                    KeySelector = (Expression<Func<Campus, string>>)(c => c.Name),
                    PropertyPath = "Name",
                    Descending = false,
                },
                new CursorOrderInfo
                {
                    KeySelector = (Expression<Func<Campus, bool>>)(c => c.CreatedDateTime.HasValue),
                    PropertyPath = "CreatedDateTime.HasValue",
                    Descending = false,
                },
                new CursorOrderInfo
                {
                    KeySelector = (Expression<Func<Campus, int>>)(c => c.Id),
                    PropertyPath = "Id",
                    Descending = false,
                },
            }, new Dictionary<string, object>
            {
                ["Name"] = set1[1].Name,
                ["CreatedDateTime.HasValue"] = set1[1].CreatedDateTime.HasValue,
                ["Id"] = set1[1].Id,
            } );

            var str = expr.ToString();
            var set2 = campuses.AsQueryable().OrderBy( c => c.Name ).ThenBy( c => c.CreatedDateTime.HasValue ).ThenBy( c => c.Id ).Where( expr ).ToList();

            Assert.AreEqual( set1[2].Id, set2[0].Id );
            Assert.AreEqual( set1[3].Id, set2[1].Id );
        }

        [TestMethod]
        public void StringDescending_ThenBoolAscending_ThenIntegerAscending()
        {
            var campuses = new List<Campus>
            {
                new Campus
                {
                    Id = 1,
                    Name = "Beta",
                    CreatedDateTime = new DateTime( 2024, 1, 1 ),
                },
                new Campus
                {
                    Id = 2,
                    Name = "Alpha",
                    CreatedDateTime = null,
                },
                new Campus
                {
                    Id = 3,
                    Name = "Beta",
                    CreatedDateTime = null,
                },
                new Campus
                {
                    Id = 4,
                    Name = "Alpha",
                    CreatedDateTime = null,
                },
            };

            var set1 = campuses.OrderByDescending( c => c.Name ).ThenBy( c => c.CreatedDateTime.HasValue ).ThenBy( c => c.Id ).ToList();

            Assert.AreEqual( 3, set1[0].Id );
            Assert.AreEqual( 1, set1[1].Id );
            Assert.AreEqual( 2, set1[2].Id );
            Assert.AreEqual( 4, set1[3].Id );

            var expr = CursorExpressions.BuildCursorPredicate<Campus>( new List<CursorOrderInfo>
            {
                new CursorOrderInfo
                {
                    KeySelector = (Expression<Func<Campus, string>>)(c => c.Name),
                    PropertyPath = "Name",
                    Descending = true,
                },
                new CursorOrderInfo
                {
                    KeySelector = (Expression<Func<Campus, bool>>)(c => c.CreatedDateTime.HasValue),
                    PropertyPath = "CreatedDateTime.HasValue",
                    Descending = false,
                },
                new CursorOrderInfo
                {
                    KeySelector = (Expression<Func<Campus, int>>)(c => c.Id),
                    PropertyPath = "Id",
                    Descending = false,
                },
            }, new Dictionary<string, object>
            {
                ["Name"] = set1[1].Name,
                ["CreatedDateTime.HasValue"] = set1[1].CreatedDateTime.HasValue,
                ["Id"] = set1[1].Id,
            } );

            var str = expr.ToString();
            var set2 = campuses.AsQueryable().OrderByDescending( c => c.Name ).ThenBy( c => c.CreatedDateTime.HasValue ).ThenBy( c => c.Id ).Where( expr ).ToList();

            Assert.AreEqual( set1[2].Id, set2[0].Id );
            Assert.AreEqual( set1[3].Id, set2[1].Id );
        }
    }
}
