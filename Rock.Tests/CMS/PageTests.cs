//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.Collections.Generic;
using System.Linq;
using Xunit;
using Rock.Cms;

namespace Rock.Tests.Cms
    
    public class PageTests
        
        public class TheExportObjectMethod
            
            [Fact]
            public void ShouldCopyEntity()
                
                var page = new Page()      Name = "SomePage" };
                dynamic result = page.ExportObject();
                Assert.Equal( result.Name, page.Name );
            }

            [Fact]
            public void ShouldCopyPages()
                
                var children = new List<Page>()      new Page() };
                var parent = new Page()      Pages = children };
                dynamic result = parent.ExportObject();
                Assert.NotEmpty( result.Pages );
            }

            [Fact]
            public void ShouldCopyPagesRecursively()
                
                var parent = new Page();
                var child = new Page();
                var grandchild = new Page();
                parent.Pages = new List<Page>      child };
                child.Pages = new List<Page>      grandchild };
                dynamic result = parent.ExportObject();
                Assert.NotEmpty( parent.Pages );
                Assert.NotEmpty( parent.Pages.First().Pages );
            }

            [Fact]
            public void ShouldCopyBlocks()
                
                var page = new Page()      Blocks = new List<Block>() };
                page.Blocks.Add( new Block() );
                dynamic result = page.ExportObject();
                Assert.NotNull( result.Blocks );
                Assert.NotEmpty( result.Blocks );
            }

            [Fact]
            public void ShouldCopyPageRoutes()
                
                var page = new Page()      PageRoutes = new List<PageRoute>() };
                page.PageRoutes.Add( new PageRoute());
                dynamic result = page.ExportObject();
                Assert.NotNull( result.PageRoutes );
                Assert.NotEmpty( result.PageRoutes );
            }
        }

        public class TheExportJsonMethod
            
            [Fact]
            public void ShouldNotBeEmpty()
                
                var page = new Page();
                var result = page.ExportJson();
                Assert.NotEmpty(result);
            }

            [Fact]
            public void ShouldExportAsJson()
                
                var page = new Page()
                    
                    Title = "FooPage"
                };
                var result = page.ExportJson();
                Assert.Contains("\"Title\":\"FooPage\"", result);
            }

            [Fact]
            public void ShouldExportChildPages()
                
                var page = new Page()
                    
                    Title = "FooPage",
                    Pages = new List<Page>      new Page      Title = "BarPage" } }
                };
                var result = page.ExportJson();
                result = result.Substring( result.IndexOf( "\"Pages\":" ) + 7 );
                Assert.Contains("\"Title\":\"BarPage\"", result);
            }

            [Fact]
            public void ShouldExportChildPagesRecursively()
                
                var parent = new Page()      Title = "Parent" };
                var child = new Page()      Title = "Child" };
                var grandchild = new Page()      Title = "Grandchild" };
                parent.Pages = new List<Page>      child };
                child.Pages = new List<Page>      grandchild };
                var result = parent.ExportJson();
                Assert.Contains("\"Title\":\"Parent\"", result);
                Assert.Contains("\"Title\":\"Child\"", result);
                Assert.Contains("\"Title\":\"Grandchild\"", result);
            }
        }

        public class TheImportJsonMethod
            

        }
    }
}
