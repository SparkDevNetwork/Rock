using System;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

using Rock.Security;

namespace Rock.Tests.UnitTests.Performance.Benchmarks.Security
{
    /// <summary>
    /// Performs some basic performance tests on the Rock.Security.Encryption
    /// class. TL;DR; It's fast.
    /// </summary>
    [MemoryDiagnoser( false )]
    [Attributes.OperationsPerSecondColumn]
    [GroupBenchmarksBy( BenchmarkLogicalGroupRule.ByCategory )]
    [CategoriesColumn]
    public class EncryptionBenchmarks
    {
        #region Test Data

        private readonly string _shortText = "Lorem ipsum dolor sit amet, consectetur adipiscing elit.";

        private readonly string _mediumText = @"Lorem ipsum dolor sit amet, consectetur adipiscing elit.
Aliquam eu diam massa.
Quisque sollicitudin quis purus pharetra elementum.
Nunc elementum risus quis aliquet eleifend.
Donec dignissim, lorem at placerat ornare, lectus urna mattis nisi, sed ornare orci quam venenatis justo.
Sed sit amet fermentum arcu, nec ultrices nisi. Aliquam vitae vehicula enim.
Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos.
Aliquam erat volutpat.
Sed arcu ligula, porta eu porttitor efficitur, ornare eu magna.
Praesent tellus sapien, scelerisque vitae orci non, lobortis tempor odio.";

        private readonly string _longText = @"Lorem ipsum dolor sit amet, consectetur adipiscing elit.
Aliquam eu diam massa.
Quisque sollicitudin quis purus pharetra elementum.
Nunc elementum risus quis aliquet eleifend.
Donec dignissim, lorem at placerat ornare, lectus urna mattis nisi, sed ornare orci quam venenatis justo.
Sed sit amet fermentum arcu, nec ultrices nisi.
Aliquam vitae vehicula enim.
Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos.
Aliquam erat volutpat.
Sed arcu ligula, porta eu porttitor efficitur, ornare eu magna.
Praesent tellus sapien, scelerisque vitae orci non, lobortis tempor odio.

Ut sed imperdiet sem.
Cras vel elit at nibh congue luctus at vel velit.
Ut elementum consectetur leo sit amet sollicitudin.
Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia curae; In hac habitasse platea dictumst.
Nulla id lectus semper, consectetur massa vitae, placerat odio.
Nam sed est mattis, hendrerit sem et, iaculis nulla.

Orci varius natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus.
Nullam fringilla, enim sed auctor faucibus, odio libero tincidunt tellus, vel dignissim erat justo non nisi.
Nullam mauris nibh, iaculis et tellus sed, bibendum lobortis lectus.
Cras neque felis, interdum at dapibus vitae, efficitur non sem.
Pellentesque blandit arcu nec lobortis placerat.
Curabitur euismod interdum nulla in cursus.
Vivamus tellus velit, luctus vitae ante in, faucibus gravida purus.
Maecenas erat mi, tincidunt vel libero ac, laoreet faucibus dui.

Duis vestibulum ipsum et dui placerat, a mollis nisl molestie.
Proin id eleifend massa, et commodo leo.
Duis finibus in dui ut interdum.
Donec enim mi, auctor non consectetur at, tristique eu felis.
Phasellus urna erat, laoreet a pharetra quis, facilisis et est.
In a sagittis lectus.
Proin vitae pulvinar risus.
Praesent ut mi sagittis, facilisis tellus ac, auctor ante.
Phasellus commodo enim velit, eget imperdiet velit gravida vitae.
Phasellus mauris massa, ultrices vitae tortor et, aliquet sodales dui.
Mauris fermentum tempus eleifend.
Nunc rutrum feugiat lectus et luctus.
Vestibulum suscipit tincidunt tempor.
Fusce pellentesque ante vestibulum sem fermentum, vitae tempus sapien lobortis.

Aenean commodo imperdiet nibh.
Nullam accumsan ipsum eros, sit amet finibus purus molestie quis.
Aliquam sit amet ipsum sem. Integer vulputate semper elit sed mattis.
Maecenas aliquam neque id dictum mollis.
Vivamus in eleifend mauris, a pellentesque purus.
Nunc nec lectus eget ante aliquet sollicitudin et at mi.
Morbi vitae orci eleifend, suscipit diam et, aliquam eros.
Maecenas nec massa accumsan risus blandit aliquet at in justo.
Aenean venenatis nibh vel nisi bibendum, in lobortis ipsum porttitor.
Duis pellentesque augue magna, varius luctus odio consequat quis.
Sed in est luctus, consequat quam sed, pretium augue.
Donec ultricies tincidunt massa, eget sodales augue molestie vel.
Vivamus vel nulla vitae massa ullamcorper pharetra.";

        private readonly string _shortEncryptedText = "EAAAALBE5hrDbqNlheT/ovuAiYwPkkajwXd7Tg3HCYGxDM+ulWPCIzngedyQGiA8ibXMJ9HO38T+XxyY1vTEq3sQ1I0LrKrlOcgt1ok7loPyI9iL";

        private readonly string _mediumEncryptedText = "EAAAANEqXL8GI9q57QPg1ar0SDmBJ0FbkxGoYXD33i18AagIZu6Ggu95Kmo3okPNBAuIyPby/hRUxcAq1qzyunJXcSt6g2N9EFaTZQ5q/R3rkTW4ZrVqHpAFhXnGwx/tBgGIqHu0xnD7NLRfiIFB9+mR+1cF+Wa6A4HUyX+G7IXzHxKsBkmoD0Oq/29jK0o7cwRYHhlY65sbRH9O2O+EuGQYnMNAF6GtBRi7m2JXI1Il033nUg8WensMhIlpy8KWd/n6o8lRWUr+C17IvEHm70rdkkCsvlIld3GYD5XsrZwnAt0PZMWHg/PtlluyGK1kvSfpbtn4DUlSjkb8pl4eoWyIZB/Uz+px/6rRS6q1x65DJcJIE7AQm0DfHj1GfCXZoLxoKhwKeuZki/tAP4wmrw5E0yjCLN/SzHEO1bGnW491ZI78s1/NutppjHAqAjn6jXYNOpiXe9i35nB/5GMhx1KRZ1lu1a77Bwi0ElFfIFEQO4tZ67Yl8zCy4E3yXkmU3JtKRvjEB0SgAsh+3lF30sNm0Eh18ZOHAlJdOqiODWHCpjPrilNnacTwj/hipnSJermN/lQGrLuoSn0bA/T7gt9IlCuqDvK7Oa1Qt6kWg/7+TUcYKTrbCjJpOeIISrZfDNojgshn1WGlg0dANwI17LSt/fzuHJQ5wMrSmOM7aVm054aE03TcjF667FrFsjM0nG2L+W+IGhNC/wPAGgpZ75yD6O73fAkJwc+eLadjHXUdoH+2xFec+Msy203n5Fv1MSoAk1rU1LoCuWc8lRJ5spPUKiBsrl1jD+/Ypz3UQCQ2bNNntBpWycC6RCihAAE/ko5+DGVNxjE5PXuTzui/kgsvPhg=";

        private readonly string _longEncryptedText = "EAAAACV0FooQcJVQrQZMjgylfw1KridnsQYUfvgn4w5vdPcDfdSDMMPrFjrfAnl8ocRLNCx6xVx5bq/tuwbTVpkKeECUcE5psE2k4nFGbKaLSfg5/BRHKaDhd9akhxEbZrb7FrLcQpUhLDobdiUCv+t/Ihe8gSrswsxhNdoW3OMBmo4BUHFBDbiQrCLqXhtR6nQMlZNSwmkXBBlpYyU+TJlrGCJ5SPz+8KbHRlQ0dwpUXM3D8HNX+cjj/YVUGJqtZ6M9WuFEVIna+5YpXGY3JxFeev2xBXN/C1xdJ8U9ZhzAyTpZ+HD/jg+uNIHTocKv8seEEi+pOLgXd5DzZvk0b4v8tW7cPr/ODgdONq33ZuGe9hxR/eC6qC5JVCeJy9fQEMGvq4/ZG/4AKgF7uVwNbxM1xrKnjOT7Ea0+cfh+UwuqRID/zP3JJBhUKaQJKWVvLx9YFaQFiQ5xP+SjeWNPPfymTGntk9imIAKmVOVS5/Yj2KG7GtK7m3CenBJO7DEdXmmmxHKibt5gpqmv6A0gYyGASjQpik/9Vjvd4LcXWDhsHSXxbO2/14MBopFRF/Nu//0tY6IsPvlkYPv7iId6gRkBD1G4K1vzkhB4mOYsciLHlId035m3vIeU7v+fhzTFGgd9aMfIVO+MV/7UvwwE+FkQXzHlC8QS4mjB6mZozhuMdqotgZ6eS16EYkzyCfHF+9Xv6rjJNq7qeSiy7b0MveobfCyMohFLZSANZOlK9bjNuBt62NEqdTTtfzVRQNRa4pFGT6T0Dmb15jlY8cic/FCdIKFtMhahZPu9PMqaNKKn/TxXEdDGECijTsitaS2aM+BcBTQtGaDWYmcv2lWzWnyLK2BZ9uvtG0YVSdgueG3ACEIvV496BLuI50U4Sgi8DSdFzz/+W8dbT8A4eRknkgZQke6Nuhz/DgqebdW49WFn1RHU690wwurkUhSH67ObGsh0W3/lYh3xE5nMQChspn4phdFcyEhKiJQX5NQPgPYU5daDZ8TbDTout2IAqtE8dtBXI+SS6rpNCWUWnqPov6jTn5N2N+WwuanSM266xhpX1GTKIrYbypxtKM4CqjA8kytd72ur945MGvImmFrX95pWDRurOQTC7XSFBElhmxPBaSlxO9pFdXxkwKgIZUK5QDp+t9lTSOs6FMSdRXad3fLnafMMNbD/ZWNLPJT8kleez227v6/gAnPOHKSTvfj1ydFeQIPXMXjWzMCEBsnwSwG+U3dARjDk2dvu/uDhpjvPH2tkqt2ZS0yBJ4T3bDCpkz+Cg6II0gABQbAqWEuy5caciR5CnlOxD+cuDEj71lQkV15EEIBNz45ixENQPANfGug87qugfmXAKmSR1NT130w5DloClF1M13qRLToSdwqhhZBTuu5/tGHlZxLbuLlPOpHy2M1s+Sackm8maXyfW7OnavHSfTfSzoKYoctRYD8NrQF8qQWjguxZMEZ+7LDCuA/sk8ANQCIF4mOxBQqWLxsLLxbtstSNPVKy8wLin6sb4p5iToAOrA8ABRaDYN5KAc0bOBBXiMl3gHDIIZgl94pLUQGpckvnSCeiePWdE6464JR8WV3lfdGphOjK7KEq2Mk562mMq8jzm4HDjI1MordA9poPkXYNuUJU6+sZ/mF32Hd+HvX4YXO2+DgQgqn9yozdDhTeCsbZk8EvMqdNiQAHPX6QcopFKG0DbkwKHs4CG+NrJZCQCohr/AYh1YhIlWWlUhcW3lrxaeW6ZuWhfplbLDNLPO9Lq5guhQIz2V7OhHb40GRfAkO//2xte7nfVjq/TerCZSuVgA3yu5CAuGBI7qV7Zf22mkAQsVM+ab457FOnIDpkQIzZ5JwTzgH4TdikwGSKDlifumsT35yR22MHmsuL0Ej0kL+Oow0QDwgJ/Siakvqh5AB9fmRpvRgyqT2OnjYf1EcuSBQYL2iwPkA8m6JjtAu60v+5A0Xy+BmcnE6abY+YHtNhK9FWBv1awsy7AqpVe6lCBWPsAZc+BcJO+ZsslIXkf98T/xl1ZRkxmjFnNM+mJxKzIzrnowvTTQPaqfNh2BEXFI0K/UCXMm3yO6YshdX6rWiMJXliOibQPS6uxs63bwTjTFikhNHrosZJg7BDFhu8HNzFRq8uyRQ3vwzGx0aWO+SpEc+sAF4Cc5ekzx1bjYHvHTkvbsbiGWOGd0Xcf4trRYoSZEMa/D4QoCQvn62j7zosuvrUdlAXb/pzdT6fJa8yiIXPHr0sEZJbi7kGB8K4vtY6mfIZ7y4UjH55ItxUfJZ6JlOKFEavHSfHdaMUQKdoBznI1VDQT71BZgcobQnNGY3SFv27YqZt/YVv4AMPd7vwIsNfucuWKKbbe6oYeGxe4kCqA9UtAH4A1Jb1NrFKRedV9kH2PHTTea5u7kRXV7yHuGkahOKvB8If0uncrqDxyIF+eczQoxPC1XiKZB8MwsnVM3N6RaVOOWsznr35fI4c9zKEcprvmzY22W2gVp6O3CGv88361nYDoWYP32vcEhkO88s77ApRd9dDZ/ofsQ7lWNS2ceaTW1sCUK1pXJ/lUiB4kmoMtJEMJ8tpwiuMPKbW1WMHDVDL0MKyrkpsZoV1TJSOblbDEg7wF8eBAk/oPdlLPyyVUJqrE05wC5GsTc7Lur+87zeeuZS5OaAZNjE/j3givujyXrBKd6g/wNCm8Jvnac/3I7EjheanVpLmVGbLaew82S56gjpzGYIc7H9wpN9mYNm9FS0nJmTu/Aby3388rFUiRN040ZltOYhkvhNZZeN7IckUEHGYoGAYsSYccvJQMSY/58BPmeD+T8ZCkDQrbtPEOI0s3HKuZ/HlT0dHrh2kGftROvG8NnXc6FwGCQIfUo/2RuSM9uBpbibM/U0+jduvY54kCuRSNwTW4NRni2907XsQt7OkBPfYKLNwv8nLIxYcRBXF7GKF1DOu06hv1IS//LxU8HUC+Lpv7nMvg/z2pDHjYZzrP7qa2A2ObCouU7NF4uwtYjzTIpa9frgNz/HkjATdBcnwUtC3DW2pU28K70fDnwmfQ0tJNpuSqjDJlEWxZczOrJS4E5j7w/QSzA8C+jRo2pvdxhlP/dXJoigm5QQSm9fXj1wcF58iudsEbuU5FvpavCdIjLgy6tgvFvKA0tygWGqziy+KFzE/kJ7veG8JgJZnWwpCKXM12IOdURAGofSkDpQpvY0P6qa0QHKm5Z7+3w86IEc3kyn3fT2eZZ/qWHeu1XHDNN9T23g6DcIb9Y/69yqdy+wySsuEZX2l86/KcQtOhCy+dHaovToV1gYRC1lqw4wPYGNBYJ7LwhWpxdsVKkdNtVGw7+wVR5GEHY/NA9GRobHGlUHVbdC8/bKwB2mxtIDUB3wt+tqUaO3x3dshw41Gp7dVW5uPR0eRMOEPtV3knt0+vL3QXWL3cLDJiYYMjUvuVOVXZT3B/xBwusZyn967yU0aCZHkKi+ZR4lFFAWIFg40k6d5KEX5is0tfZ4fkNZ4DZ39nzzyyhZos+eMtkmlnhTOiIF8MbiKMYBpK4GcwSmXzRfwkOWC1pYjE5gcvx2Wx1Mw5y5eSORs7bimAWU7iShVjvqfdWYZagLksNpinz2yrYpN5tiM2haacqagpJabKM12+u4cto+oVcfdjGcw/IBzlkbf1WzoUCTTVkXO7TWgCARO4oHDA9ZfwWrf2lAyBNOwoRczfgxDK9miPdjyJsv0W2+X3UrCY4Q85+b5gdYnXnVt3VUfxyqsLV72tDp/X9pSrVR6xydSAz6y967A7YvqEtwt855pU9cD8naa8nLM3fGuYnDn0sk+qYcSp3diAwB2gkV5/VA15PsJaexNxmF11bFL3kiLv8K0VpE5QWIF45SouWdpfqZ8wMasD/qoKM7cpYfvJvTpywR1OG3cntHHh5aSuTyUDv+0e4t7jcGhfzaFqgAtiCj4jYyKWJhTxn8XaV0mdfboWy+oMNr1MkW7e4o9rcFFxjBJGjwArpnwhF1WOTJPfWJCbIkSHONT+yRDI5gBfml885usdKZC8BOj3n6l/XtzLkusP9uAkihAyp0NnBvoMwAMcRvIh25MGWqCD7qnvg4NzXcn";

        #endregion


        [GlobalSetup]
        public void Setup()
        {
            if ( Encryption.DecryptString( _shortEncryptedText ) != _shortText )
            {
                throw new Exception( "Test data is not valid, short encrypted text does decrypt." );
            }

            if ( Encryption.DecryptString( _mediumEncryptedText ) != _mediumText )
            {
                throw new Exception( "Test data is not valid, medium encrypted text does decrypt." );
            }

            if ( Encryption.DecryptString( _longEncryptedText ) != _longText )
            {
                throw new Exception( "Test data is not valid, long encrypted text does decrypt." );
            }
        }

        [Benchmark( Baseline = true )]
        [BenchmarkCategory( "Encrypt" )]
        public string EncryptShortText() => Encryption.EncryptString( _shortText );

        [Benchmark]
        [BenchmarkCategory( "Encrypt" )]
        public string EncryptMediumText() => Encryption.EncryptString( _mediumText );

        [Benchmark]
        [BenchmarkCategory( "Encrypt" )]
        public string EncryptLongText() => Encryption.EncryptString( _longText );

        [Benchmark( Baseline = true )]
        [BenchmarkCategory( "Decrypt" )]
        public string DecryptShortText() => Encryption.DecryptString( _shortEncryptedText );

        [Benchmark]
        [BenchmarkCategory( "Decrypt" )]
        public string DecryptMediumText() => Encryption.DecryptString( _mediumEncryptedText );

        [Benchmark]
        [BenchmarkCategory( "Decrypt" )]
        public string DecryptLongText() => Encryption.DecryptString( _longEncryptedText );
    }
}
