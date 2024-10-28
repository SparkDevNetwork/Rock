// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class AddNextGenCheckInPages : Rock.Migrations.RockMigration
    {
        #region Binary Data

        private const string CloudPrintLogo = "0x89504e470d0a1a0a0000000d494844520000012c0000012c0806000000797d8e75000000097048597300002138000021380145963160000000017352474200aece1ce90000000467414d410000b18f0bfc610500002dba494441547801ed9d0d7454e5b9ef9f904089894090af804a026804247ca958682d9042d52a2a706feb476b5a68b55dade25d955eed7109576bd7422cb0ecb9b6eb4045add073ae80e0b5686802e801a12aa0d8601492284a206802213110089cf7bf33c319c264f6bbbf66f69ef9ffd6da6b66f6ec489cccfce7ff3ceff33e8f08218410420821841042082184104208218410420821841042082184104208218410420821841042082184104208218410420821841042082184104208218410420821a41d69424802696e6ece0bdf6f6d6ded71e6cc991eb89f969676243d3dfd08ee1f3f7efc484e4ece1121290f058b78467d7d7d8fce9d3be79d3e7d7a9412a081eac85382641ceae91ea1c30ad51032757b44fd3776a9fbd5eaf6934e9d3a55676767ef1292f450b0886bc02d9d3a75ea162524a394901887c411f5ef6e829029018378bd47114b3e2858c43670501919197743a0948bba55ac3b26af812383137bf9e4c9939b5558592d24d050b088252052cac1cc51c7b7949b99280102e2a57ee74dea777f8eee2b9850b088296127a5eede1a34918a01f25fcf2967b89cce2b3850b048871c3b766ca2fa50dfa2ee168bffc23dd740ee4b1dcb95eb7a4e88afa16091f38050a99b4793c84de952adc2c579cc77f9170a16394b0a0b557b182efa140a16a150754c58b816b370d51f50b052180a953646a8c81c57e2a160a520a1d284792ad17cbf102b54b7b6b64e6298983828582946434343b1ba592449bcea1707162be15a42e18a3f14ac1441b9aabc8c8c8c6719feb906dcd6034ab45e161237285829807255d836f3acd05579c1e26eddba3d20242e50b0929820e4aa9a9a8e4b53e371e37e6363b3f11864657595ecec4ce37e9fbebed759e6b6e204052b490985806be2dd31211ab5878e48e5be1aa93d582f559507a5b2b2468954b3715e1708589f7e39c6eda0c1b9d2a74f0fc91f926bdcc7391f70047b2cb992e82d14ac2424b4a5668d242804dcfd5e95542981dabdbb4a3e50f7c3aec92bfaf4cd91fcc1fd64c4887cb97264be21628942bdeef3bb77ef3e4f882750b0928ca3478f22047c54e20804a9726f8d946dd829dbb6967b2e5066c07141b8aefdfa3019a16e131052beac42c41fb1d8d47d2858498412abc5f1cc57c149f945a462716561be144d19234553474b1c615ecb032858494028b90eb1ba5b3c06c2b476f5567965cd565f8b5434103a8e50e275fb0f26c7cb7551b45c8682157042bdaa367a9d5c0fb2504563dcf8a132edb6f18680790c45cb45285801261e62956c42d59e2b438ecb63e1a268b904052ba0782d56c92e54ed418ecbe35091a2e50214ac00e2b5582199bee4a95596eaa49cd025a7e779f75beaeb8cdbd6e666693dde2cf1e2fb4ab4eeb86bb2780445cb2114ac00d2d0d0801aab5bc565e0a4163fb94ab6bfb547dc0642943d688864f6bb58d233332533778071dba5474fad9f6f395267881804acb9e633757c2ecd073e37cebb0d92f3f7ff6aba576162f5dffffef771d3a74faf1562190a56c0f0aa7461dbd63db264e12ad7c23f0854f76123243bff3243a8d2bb668a1740c4205e47cbdf6f133175b80592f2b3efbd51dce6c48913e56fbef9e6248a9675285801c28ba25008d48ae7cbe49597b78a5320523dc78c33840a0e2a1140c01a2bf74addceedc6ad53e0b69e787296ebb9adfdfbf7ff6df8f0e1ff53dd6d12a20d052b20843a2eac11173974a85e9e98bfc2d846631738270854cfb1e3949b1a227e02e275b074bd215c4e42c7acecae72bbca6bc171b9c91b6fbcf1cc4d37ddf4a050b4b44917e27bb091b953a74eebd55dd776f922b1fe9b0797d94eac43a8fa7e6b8ae4dd7eb7e4148e392771ee179023eb3eac507a4f98a872651719e1a29d04fec99653b2e39d8fe58cfa7a7733af3570e0c0abbb76eddab069d3260c753d29c4140a96cfc18a607a7afa5bea6e3f7189b56bb6cac2dffd879c3c794aac122954dd2e1f2a9d323a4b10c8ec3fc0b1707df07e9572a547e4daf143c52d0a0b0b477efcf1c7072b2a2a3e128a96290c097d8e0a05d178af585c62c50b65f2d7bf94891df081ef5774bd6709f47852f7ee3f8c70d14ea8983f38d7c86bb9d5d6a6a6a6a6bca0a0e02175172e9aa215033a2c1f13eabf3e4f5cc2ae5821819e777bb1f4ba6642601c9519705c0817c157fb3fb1f2a372a4bed10811c75e75b991df72ca85175ed87be4c891275f7ae925e4b2f0cb9c1612150a964f09e5ad5c6b6b6c57ac067c77ba5c3af34e5fe6a89c821c17c25aac6c1e2ddf6d294c8468a114a468aacadf75c910a7e4e5e50dd9a5d8b76fdf19f5f03321516148e8538e1d3bb6d1ad811176c40a02957fe76cc389a402284845887878eb664b3fe76678180a0d1f5777df55c74742ce830ecb878442c139e20276c40a8e23ff07b393d2557544a7ce9d0db795def50239f6b17ea53f9c56c59efd86d3724a28346c52a1e157ea218a4a59eed00e0a96cf703314b42356fd8a6e900137dd9634b92aab645d9a27dd8716cab18ff6688788280d4101ee98ab2e13a78442c36d2a34cc96b6d09049f8083a09f115e9e9e9a864cf138720bf6245acb0f2875c155601531d84c1437ef24bed7d8e60dd9aadc6e194ce9d3b67cd9f3f7f86ba9ba58e7142ce810ecb4784dcd57271082ad817feeedfb5f70542ac86ccfea511129136c245a78d557be554e331ad9fc1ca217ac9f7ed9b234ee8d3a7cfc0d6d6d6f22d5bb6c0e2b5a8e34b210674583e0263b9c42110292b15ecc85341ac5225b96e85b3af8d857d914b16ae76a52dcfac59b36684ee8e9036b745840ecb378412edf78a439e5b56223bdffd58eb5a38abcb7ff6bfa46b9fbe42a283643cb61e1d797f87564ecb9820b4afc671121e09f890cbaa97b67c6695103a2c1fe1b80b4369c90eedae0be13030955602ed82f0d04a4e0b5b78dcc86745b82c7ca35c2e840ecb0f84dc55b1380079ab254fadd6ce5be57dbfd8e85345f408e7b4ea76fc43ce9c32df8359f1e17eb96e62a1a34af80897f5857ad84b1d9f4a8aaf1ad261f903c7ee6ae50b65dab913942ea0250cb186514c7bd76cad6b9b1a8fcbe285abc429112eab8bb4e5b3521a3aac04e386bb4228f8d7bf6cd4bab6e7986b64c04dd385d8a32d844e33560fcdc017081a000e1a9c2b7669e7b2b0fc98d205a5745809c68de1a72b35ebadf061a3583907b56abacd0a97fee955c76da7235c16b85252180a560241dd95d3fd822b2c848248b227436b183f80225b9dd712a1a1d3047c6e6eeeb08282820b420f9180ef23290a052b8184aada6d83447bd9861d5ad7226fc51541f7c06b89d75487752ecc765cb87061e43f96b22ecb795f0c720e8d8d8da34e9f3e3d4a857a03d50107651ca1a7f3c4457413ed6d1f2e6eb9719bde13be654ceb31cb67855dd6ed0ee61d8e1f3f1e7fc070163fecb2526eea0e1d9603d0be184973752c3a76ecd84e757b4689d54ef5d4b34aa4e6a9fbc5a1902f4f5c16ab3677b553eb5a8482c41b101aeae0d465618fe1dcb97323f74ea5a4cba260594409d3448cdb42bf2a15d2a10a199d15e6783585b923e0ae74c0aa204341efd075af6ee4b26eb8e186ab221ea6642e8b82a5019c5458a494306dc46c40b79aebd9016f7e5453eba09b6721f6e93d7ea25602dea9cb2a2c2cbcaedda994735914ac18c04da9638d7252558916a948b66d2dd7ca5d31d11e1f50058f7c9619c617cd7bf6b70446090be1b252aa7119052b0a21a132dc943a30c0d4ddb1bf0ed1a9bb6a9bc27c8d90f8a0ebb2d6ba1b1682024921285811b413aa89e243300055c75da1b091ee2a7ec065e97c41209477392ca460a51a28e0c4fc3f3f0b5598520b755724bef41cabd720d449f21d61e1d4a9537b479cc21ec39449bea7bc60a964fa1c95a3427d40b104009d643b570613031afde96cd9d9fd9eb3d6563ffef18fc7b63b75b1a408292b58705508ff54327d91f82c47d511bae1a0ee373d711f9d2e18f8d271d295f48a2bae18d6ee54bea4082929586157e5f7f0af3d3ae1209c95eec65ce23e1891a6937cc74aaf5d2eb9e492f6cdf753262c4c29c10ad5532d0f92ab8a44271c649fabc482e4bb4e0f784c35b24b943c164889b03065040b21205c951bed5c1201fa846b858363180e261a9dd5c2aaca1a47ab853367ce6cefb252222c4c09c15262352a9458cf9380a2e3ae100e5a99f042bc01ad94cd401129be84ec72d965970d6c770a6161d24fd7497ac1c2e664255668c719b8103092dd1a82c5dc953f4058a8f3b7a8722058fdfbf71f18e574d27f5b25757b1924d7d5cd22893308dd8c10ee60bdb2fe078dce0ab5eac0b76a4761409fbe3d8c76ba185a306850aee40fce355aebe23cd07973eb7cb393f800a76bd67606ae79da6de3c50ebd7af58a265848bc7f24494cd20a16362b63ff9fc40188d0b62de5ea0d58ad9c50a5ad256bfc4cf8e7b64724642162230af3b5fe9b0c07fd0326121ddeba39e63595fb0e8a5dc289f7929292c311a7937ec064520a169c553cc40a7551d8d707f7e3b4a36447c099956ea837bd0ef92b168bfa87ecfccb4caf315cb77adf6465d91b05a6c2c20bda9d0ae7b192764845d20956680a8d676120de606b576f95575c687beb267457fe02792c0c5e6d395217f33aa40ee0a0ed307efcf881cb972fffa4dd698485493b253aa9040bab81d2d650cf75fc2a54612858fe23b3ff0053c1aa3da8427d9ba9c7dcdcdcde514e63141805cbef84eaacd6880760eedfd23ffecd97421526333765b69305069dd1f6a8c712192d765089f7688215e8d570339242b050c11e2a5dc8131731c6bf2f5caddddd3391200421fe4227a7889563bb6466665e10e5748e24314921584aac90b3ca131741a3b5bfbe50e66b5715094342ffa1235848bcdba57bf7eed11c565227de032f58a15aab62710908d4bf3df3aaf6441a3f80cdb61c90ea3fbae45c647acd21075d1bba76ed7a41074f256ddbe4400b16f2566e962f2004fccd83cb1cb5fe080301e93e7c8464f6bbf86cc94178e5a83dadc79ba5a5beceb86d3ef0b934567d6cdc9a256cc370c3b33ff1fa4b242b2bab777575f5efd4caf8e14f14e5e5e59f9496967e52525282b0d0f99bd887a44980517f282497f2c405b0bcfcc4fc171d89158a05517fd373ec355a09573320628d957ba56ee776e3361a1042cc1d640d96ff686d6e96dd8ffd6fd3ebd6bdfeb8b84cb53a3675ead469d3c9932737e7e4e4544b92105887854a767151ace0acece4abf02d0a81c2b618b7f7f2198324d47f1b07c4eb60e97a43b8e0bcf0eff69e30d198d6c270d09f247021244f1dc518e4abf2bb9855b04b3d5e7eead4a9b54117af403aac5009832b4b774ec40a8281219a140cd211bb1ebedff41a0f1c5687a814ca2e752c0eaaf30aa4c35262e54ade2a9cb3b22a5608fd2e9d7127c330123830a15c1dcbe1bc544a65b90a1b97646767ef92801038c10a6dbd291687d8112b38294ca3d1199a49480030c24615326e5222b6a45bb76e2f8bcf09a2c372ecae205256570351e7947fd76cba2a927484661b4c5466a05a39af396af571adf8944035f00bb9ab3c7108eaacac8815da0e0ff90957e28835b04a6806fa9ff988bcd6d6d697d5e76c0df2c4e24382d671d4b1bb4205bb95a250848097cebc8389756219d4d5999195e5cbf7d5ad58d452c2b508dbdec4470446b0dc7057c85b61bb8d2e102bac0212628796fa2f4dafc9cef295c36a8f310eafb1b1d137835b82e4b01cbb2b2b49768a15718a96c3f25748188d3c95985fee17b71508c1522fd6ade2d05da1458c6edeaafbd011142be298e69acf4daf410bec8060b8ad44e7b6022158696969e6d5773140288856c63a20b17ee9ffb85308718a9e6005aa7d9551b01d6a3890107c2f585074a723e557aabc958ebb42621dfbf29860276ea0b34a88c948410393d3435be3e28eefebb0323232ee5682257681bbd25d151c70d374962e10d7301bf305faf4eb61e455c383559b1a9ba38e838313cbcace34725e7d5518996867862e292a5533b2b5b5f54739393971eb0ce17bc15262552c0e58a9b92a88f1e23a23c609d141271c04bf9d67bf438831bb7250ae5c5998afeef74b845bbbb553a74e792a0aba2d5efb127dbdf93962c4bc2de0ae7ef2c3a7b4ae1df6e0a37457c4350e6fd9249fbfeac988810e09cfb01c377ea88c18996f7b7c980daa95d39a140fd1f2b5c35262e5a8fe43d75da184816245dc44271c749bf00ccbd20d3b8cc74553c6c8e4a9a36d8f11b30092f11b95c1f05cb47c2d582a4e9ee8247fa5333c0242c5cdccc46d3a6ab8184f205c38e0bc6ebf6bb2e1ba3ccc7d85456bb497392ddf0a5668757094d844b7ee0aee8aab82c44d1a2b3fd62a1a8d17705e4b9e5a6508171cd7b795f3f248b8f2544e2becb43c112ddf963528b5b62d5660db5b7b4caf313a7a32d14e5ce668f96ef123b5a1ad690f3fb84c56fc457f8b9a15545434aaa5a5e55fc5a341187eaec3ba456c8265e1ed5bcd05cbed96c684003fb9ab6884856bb65a90daedc1cccdcccccc3bde7aebadff2b6de3c65cc5b78205a5169bec7e4fef8f80709010b7094a8aa136d4c4d28ba9e6c3870f9ffde28b2fce139745cbb7392c27f9ab6d6f959b5e8336c75c19245e80812487b76eb6f433e15170c638b828732631780495f31846121e09e716ebd66c956d2a2279e2c959aee6b6aebffefa9f4d9b366dcfba75ebfe5d5c1aeceacb3a2ca7f557b37fb8d034e17ee9cc3b99bf229e71b0f43563ca5134204699fd0748f7a185c62dbad95a7565284c85701d2d7fffec242537987def8d32edb6f1e2164d4d4d87bffded6fdff7cf7ffe73957a78521ce24b87a5c42a4f6c8262519dd541e6af8897a0db07665386674a8645aaf7f88986bb771a3642e4708487e8b689d76ea360d58978213c6c54e1e11d774d1637c0b0d73ffce10f3f9c3469123e94a5e250b47c29582a7f35d26efd55ed4173b10a5b6f42bc243c53321e84eb0971c07d41b8ea76fc43ec80843c3e47737e355ddc60ecd8b137cc9d3bf79d050b16e0c3b94d1ce0aba43b6aafd0284c8995edf6153ac5a2f866222459c1fb1b290f6c37b39bf628dbb043162f5c2d6ef1c0030fdc5b5050305cddbd521ce00bc10a09d5b3a1e1a8102bdb993fac7c9841c122a980d1dbcd8170b9295a080d172e5c886579c4b0b6f70a2554b0d072f5e8d1a38b4342552c2e7048237f95997bb110922a440a17f26a567053b4c68f1f7ffdd4a9537babbb63c466b943c2040b432520544ebb89b60745a366307f455211bcef87cd7d5425feaded9d8568b95119dfb973e7aca79f7efa1efc2aea28121bd5f0714fba23fccbc8c878d66917d18e686a32af4f49cf4c8ebd833f1b29c421cfbc2729071a5522caf8fcd5d5daf55c48c4a37160d194d1e284dcdcdc612a013f5425e0b11505e1e10e2b3f1f578775ecd8b189a8aff24aac804e4983555b4c48b281d5cb825fceb5f45958fac757a5b2b2469c326bd6ac19a1bb05eae863e567e32658e801ad846aa33848a81342dc0321a231d15c53b4906e7962de0ac7db78c22e2bf4f05ab1101a7a2e5848ac2b67b5113da08510e22bac8a1656e157bee03c9f15e1b2907c2fd0fd394f050bf92aaf43c0f6e8b485f5fb6e7a42e28955d1c2de43a75d1ee0b28a8b8b07861e42b0b4560d3d13ac90582104cc9338a23349978245c8b940b4f2ef9aadbd65085b789c72f7dd775f17fee7d5314ee7673c11ac448915c8ca327fc1b1ef8a10722ed8eb38e0bb7adb71aaf6d5184ecb09858585d71514145c107ad8573412f0ae0b5622c40af3dcd012b9adaf8fb97b6aa9ff520821e783d543dd3a2d4c53779280475dd6fcf9f3af8b3865ba6dc7d53a2c24d8e3215678914a5fdf21bb7757c907ef55597ed17467c611928aa0b1253a3f98757dc0aa215cd6ed0e3a3b8c1e3dfa2a75f35ae861d865d57674bdab82959191b14625d8f3c423d04914aafe81c3845ff3010a16211d81c26a6ce5d9bbf469d36b2158e89f6577062292ef080b2b2a2abe0a9d82cbea7019d2b59030546735513c00e11e9af2fd66ee32c76205e8b008890d7a76751f3ac2f43ab82c7c3e9dd02e2c84cbeab056d315c1c2be402feaace0a820544b9e5a6d7b9c7734b04ae85687464292156ce1d159352c2bb1dd1cd82014164632a8a36b1d0b1692ecea6691b8087252bf9df7a2e1a8dc14aa48b83d8790d8e80e19c6761d277559bd7af51a18b15a08d07e266af5bb63c10a25d95ddb6e63b8aa1f2c94ed1a7305edc25eee84e88196ce3ae88cd5eb08ac16de72cb2d03234ea12e2b6acf2c478285bc95b8b822b8e28532c355b93d7228127c6b70bc17217a20011fee1b1f8bd20dcef258dffce63787b53b15b5699d6dc14228e8562f2b08d4e285abe4af1e4da30d8344e290d9bf643f2c422ca0e3b2907c7712165e7ef9e543db9d42f2fdbcb0d07659830a059164771c0a42ac303a1b95b34e40db6363d6608f8b0c4142d56e24d166bd1142cc094ff931dbd286b07044a1bdeec7c86345398dffd84791276c09566363e3a8d3a74f178b439c8a55dbd26ba1519d4b3122c43b90f7351b0e5be9c074208f85f6c925252587234e232c742e5867ce9c71655570f193ab6c8915840a7928ce1624243ee84cb3468d244c88dd22d2c993270f6c275839d216169e9d656859b0d035d48d025124d8adae0452a808490ce1e9d46661215c96ddb0302f2faf57bb53582d84689dddaa6327e9eeb8401495b15613ecd8458e8439c58a90f883d5429df1784e72d103070ecc8b72fa9c0e0e961c1656069dba2b8c92b7d24bc7e8d373e7ecf392e88490f802c16aacda1bf39aaa7d07c52efdfaf58b9678b72f58a19541472c59b85abbcecae884c83204427c81d70eab6bd7ae1744399d13f9403b2444eb187573ab3800a1a0eee6658a1521fe22b3bff900e2460745df980e1de534f25867db276b0b967257102bdb75570805576ae6ad285684f80f9dfdb71852e184767b0ac39c7559da8295969676b738a0b464a7d64666ac4450ac08f11f48bcebd43b3a6958a012efd186519c15312dc14238e824d90e7755a6b9d708650b142b42fc49820ab4ad39ac5038681bb431d6515d54d3eab4b320842406b82c3374e62a74c4a851a37a45396d392474a422bab92b765120c4dfe8382c6c8476992ee13bba8235516cb2db82bb622848088982fe2a61a8a3689ed8a4d442ee8a10423ac06835632a582a7f354a1ca053774577454830d01942ac337ddd064658682a5869696923c526d808a9150e8ed59a524d084930669b9f81cef4f58ed8b56bd717b19e3715ac3367ced876583a65fa7056dcd04c4830d011ac3e7d5d1bf1108991c7d271587962139d96a9142b428281ce3c4fa7e160bb7e58e7a1e3b0f2c4263a3bb7d1188c10e27f5a9bbf32bda66f9f1cb1cbc993279bccaed1e9d660dbdfe9ec2b4aba647be9668917cf940a091245c12e8a366b2d03faf4b31f0e3634347c61764d4c87152a69b0058ac774dac8e8b4ac2084241e9d90307f50aed8e5e8d1a387cdae310b096dcba54e793e4b1908090e8d95e60e6bd060fb82a50c522cc132c2c5988295919161bf9dcc41f372060a1621c100ee4a6785f0ca91f6fab9838a8a8a4fccae713caa9e1092fcd4bdbbddf49a412a1cb43b31075456563a73588410028e96ef36bd26df413808162c58d0d118ad96f09d840a566bb3fd36148490f88070b0e588f9969c6bc70f15bb7cf9e597b1c2c1b3e50e3105ebd4a953b65b07f6d558ded48989092189e5f0964da6d7a060749c03c1aaaeae2e8ff17463f88e591d966dc1d2d94fa4b39192103fd129a34572f2ca25bbf77e49efdc22279bb3e58b7d23a5b9ae9f2423f88cd6edf887e975d77e7d983861fbf6edb1a62a9fad58350b09ed0b96525c9d049c8ed524c42f5c7cf5eb72d1e0f7e56bddea2523b349327b1e924bae2e911e03cb2519d149b683a2a9a3c509cf3efb6cac17f06c057a4cc1cac9c98160d916ad3e7dcdcbf41b2b3f1642820044a96bb7e8bb372062705fc984aebbc2e7fcca42fbe50c353535e5151515b1f6fde8e5b0405a5a5ab5d864d060739bac533d4b881fe83e605f87cf213cec16e3f92072b074bd560474fb0f268b1376eedcf98ec925b5e13b3a9b9f77894d749639ebde35577042fc407ac64993e793c761e9ba2b30c281bb027ffef39f6309d6399656a7acc17648a86313b152a853f24f48a26969ce8af9fc8963c9b37363efbf3dad755dd194318efa5fa19ca1a4a424d6a6e7dac8073a8265bbfd00f615e924de8fee795f08f13b5feeebb8f92e560b1b6b2f9164e0e0dfd76b2f86390d075f7ffdf5f526975813acd6d656db212180029b81b0903559c4efa074e18bbde78b16c46affdb532519683ef0b91c2c7b4deb5a8895d3eea2ab57af365b5e3d14f9c0b41f965a29ac6e6868a8169b9373504cb6eee5ad31af8158a1388d937388dfa9532eebd881c1929973483a6736ca574ac49aebfb4a3280bc55d55f966a5d8b95c169b78d1727ecd9b367b3493808b13a2771a8bb356793d864c4c87cadb0f0f096cd2c242581008eaa41891642c464112b980688959550d0c94667b06ad5aa374c2ef9acfd095dc172d446534789f1827dfad28b4208893f9ffebf17b54b8c90e6299ae2ac5014c9f6189b9dc39cf70b690996ca63bd2c0e8060e9a8315ab0ea2ea51242dc016275748f7937068050d069a21d6824db51ce705e8f772dc10a55bc6f129b609b8e6ebcfbf9ff5fcdd090903880a806e50b753bf54dc2ec9fdde838d1ded8d878f8de7bef350b072ba29dd46e2f93969616179765bc884b9fa66811e221f87c41ac74064b8481b3baf6ebf63b328459b76edd2a8dcb6aa39dd416ac53a74e3d270e37437f5fd34a86572b58ea4088fb60ff2ec4cacab6b8714aa86ebfcb7928a8e9ae2a254a3808b4052b14162e1707dca25c96ee2649bc98154f2fa0d322c4459072d9bbf40f96baa4206f35e7c119e206c5c5c58f695cd6e104664b1d4755f27dad38e4fe5f4dd75e0e356c2bc343421c035705037078abb5057f88d5134fce725cc20034eaae0092edb51d3d6949b094cbda240e92efa0af7a01be6f6195016255fee47cadae87849073c1e707e5427055563ba384c5ca69921d20147ce4914774725715b19ed499fc7c0eca65cd4f4f4f9f280e406878f8e011d30af8483e7f758d51f2907fd76c8e0723c40408d5e1ad9b6c6f7b7353ac0012ed1aee0a79abaa5817a4890d1a1a1a36aa9b89e290871f5c261fbc5f65f9e77a8eb9c6d8c6e34be18ae3a87a12303c1e550f616a3ef0991c2c7dcdd2ea5f7bdc162b8482e3c68dfb93c6a5dbc40bc1aaafaf9fa85cd6467108c6d943b4aa2a6bc40ed9f943a4e7d86ba4fbb04249ef6ade433e2e50b0484778205810a9a3ff7cdf10288ce272bab28ed54024d8ddc85901848248b46bbaab7526d7d8132ca05cd6b3eaa6581ce254b4c240bc32fb0f306e215e705f5d722e92b843c1221d6153b02042adcd5f851c54db0466e4a3d047cecd9908a8b372a3742192679e79e6a95ffffad7ef6a5c0ab16a32bbc8b660299795a75cd64e75d7b16f8468fd76fe8bb6c2c3440361ec39fa1ae9fd8d896d2e8f82453a420996d199e43f3719f958bf0c6031ca16d4eafd950e3b87b667f3e6cd2fdd7cf3cdab352e45dd95d6b40bdb82059468cd51a2b5485c62e50b65b2f22f651244327307c8909ffc52d2b7702f24894eeb846b2c176c7a8ddb2160180c96282828785ce352b8aa52d17057c0916001b712f0614a37ec3084abf690eda2fa84d1afe87ae927170821d13878e62bede6785ee395ab0216f256c034d11e8963c17233340c73e850bd215a651b764a904078386ccc4421241ae5ef6e4a78180827356dfa78edbdbd56b12856daa16018cb7558ed414752255a3f52a2b5465ca2afa1fe338ce67f41725bacc827b148a458792d54e0c489135f3dfcf0c3bfd7142b84801f88451c3bac302a34442e6b8e784090c2c45145d3859068ec2ad5c93fbb0b4670a14d79d1d4319e091580583df8e0838f2d5fbefc13cd1fc10668cbc93cd7040bb89dcf6a0f84abac64a7ecf6f16a22058b7444bc040b059f102888951739aaf6d8102b740bb4ecae80e3903092d6d6d6db3a75eab4312d2d6d9478405b6bd631468e0b25107e172f42e2019c93214e2a8572edf861ae55a8eb6043ac3058c2965801571d1640123e245a7912273e78af4a2a2b6b8c90b16a5f8d51d7d5d8d49c9010920e8b74845d870541423f39306850aeba9f29f983fb19b95e4c578fa7404582043b725616c4ca520943345c75582094849f144fd1c2370b0eaf99f69d7f1142bc64ddeb3aa54b89c7e26a20702c56c0527b195d205aa74f9f9e74e6cc996a21842415280a9d3469d24316c40ab305df148762053c112c40d12224f9c0761b54b05754547ca5f923102b38ab7a7101d743c24842e1e1e8a6a6a6ffe8d6addb1421dae8e43b66cdfda110eb2c5bf0bce935cc459e0b42c0c58b17ff5163966024ae8a15f054b040a817fc4d5bb66cf9d7112346cc164248a0a8acac7ce77bdffbde1f2db82ae0ba5801cf42c276b44c983061cef3cf3fff7fa0d44208f13d2859407b9851a346fddea2588513ecae8a1588976081a65ffce2170beeb9e79e87d081500821bee5edb7df5eff8d6f7ce33ecd5e5691782656209e82059a5e79e595bf8e1b37eef72b56acf823dd1621fe022b80f7dd77df434545452f5874550045a11841ef7835b0233ccf6145c150e07befbd5756af5e5dfed0430fdd3076ecd81b841092302054cb962d5b6531a91e89eded36564884600188d66b252525e350cb3175ead4f58f3df6d88ca143877adba59f10720e2e08153ecbe869552b7120de2161242dd2564cf601440b533566ce9c791ff35b84780b92e9a8a7c2e70d35550ec4ea33690b01e3225620510e2b12584924e8c628e11275fc4939ae55d3a74f1f366ddab419d9d9d9bd8510e20888545d5d5d35dcd4dab56b3fb1919f8a04250b6f898df6304ef1836001283544eb4a750c82e352c71b2acff5c6dcb97387ce9831e3ba4b2eb964582a89974ee1e235df1a2bc43aefbedb28a90044aab2b2f2ed9d3b77ee59b468d13b0e452a0c2633c3649c9404e017c1028885d12e15f672843ab2701276356c598b8b8b074e993265d8f0e1c3870e183060d8d7bef635365027240456ddf7efdf5fbe77efde4f9f7beeb9b72decf5d3012b80e859ee49b9822e7e12ac30687005d12a081d67411b8b502b0bc4cd86805d71c515bd95fbeaddbf7fff5eea18d8a54b972c0859e7ce9db32868249550effffbc57dc2fdabe296a78a851f050bc06ded9036fb698489d12e0a89976e2f1e8386868615420831c3574215c6af8215261c26e285eb50b80821ae80bcd487d216e57856fce904bf0b569848e1ea2311392e428823205218e783cf56bd242899ae4b50042b0c84ab2a7440b8d066b4af50bc2c73a255e435f52a7e79bcedbe97f4577f9dc9978a5cd825f675ef1f1679e750ecdfa7204764c20091afa50bb10f4469bfb4857b9f89cf452a92a0095624b5f2dff1b57a1b1b02860302d659484c367e2a72204ea61fffceda7d22770dedf81a88d59603624a85f200c7d4c7eb96c142f4c15f1a3929b47afa4c7c1aeee91064c18aa43e7454841e43c0b24207560a7b4a9b887511623898aa06892bc75a949b6b16b92833faf31f5a582c3fd0d8f6ff4097a5cd3a49129245b0da131630e22362857a2d1e87a5243948e45e429220e04cfa674b5c41fe2ad6bf59d84bb429e8497795aa50b05294c997b47df0e30184ca2ce754d85be4ea7eb113f310a9fcee2ae9de5f488a92ac21213101c200d1c2e117aeeadb7610d21174588490c040c1228404060a1621243050b008218181824508090c5c2574199d11f376e9d2f502e9d9ff52e97dc965929ec1dd474e683d75520e7ffab1d4d57c2a2dc7dd68c449e201052b40e08375b0f243397ab846868cb94e883d20567b77bc21cdc78e0a09160c092d9095d555fc003e6887f7ef15620f382b3f8a5556b63fde5f7e26151d56b53af2c406f98373e583f7abc40fd41db0d468f53cb019b92c8e1d1bdc22bf5b5b7b990b1d6c634718e847060dca15bb343535a5c414f594735869696947c4264553478b5f709a77d9f279f0c40aa0cb44d97e71845f735693a78e11bb34343450b092913367ceec129b144d1923f90ebe05fd4222dacbb849b8bd4c32d1a76f8e7a7fd9ff426c6e6e4e899583547458b6050b3c3cef0ef5e6ea2184b805c4ea89276789133efcf0c3724901524eb04e9d3ab5591cd057bdb97eabde5c4116ad44b4977113746c4896f632c88b3ee1c2fbe9b5d75eb33b6e3e50a45cd23d272767978af791c7b2fd0e81682d7dfe5752ba61879495ec94ca7d352ae9795c8204ba34fce701b50211b0957d086dd0dbcb60b57990122ae4ac9c84816130403534f22ee949c93a2c15162e56b9ac79e210e4b47078c1b4effc8b780956d96ec813d201eb5e7f5c8202a63dc7783ab0fddba3919275582a2c5c228424098f3cf2c8aa184f374a12919282a5c2428484cb859080b367cf9ecd2525255fc4b8a445928894ad746f6d6d9d2f84041c1377057c356ade29292b58ca6555ab5cd63c2124a06cdebcf9251377052858c90272594e0a490949145819bcf9e69bcd5a8320e16e7b67871f4969c1422eebf4e9d3b74992fd51497273e2c489af8a8b8b1fd3b8f490241929dfad01a1a1ca67dd26840484279f7cf2298d50107c204906dbcb88215a9b9468fd4808f1392b57ae7c66c182053a55ed95926435588082154289d672255a9384e121f12108031f7ffcf1c7eeb9e79e37357f24e9dc1560c7d108e0b4eaebeb4777ead469a35a41cc13427c405d5d5df54f7ffad3df6b868160b724a1bb027458ed404eab7bf7ee05bb77ef5e2a842418942e7ce73bdf79dc825841a892d25d013aace8b44c983061ce8d37def8eea38f3efafda143877e4b7cc83ddfbd5f4872820a7614855a102a70521da592c4d061754cd3dffef6b7d5e3c68dfbd3cc9933efc31b08b52f428847204f054775f5d557cfc6fbcea2588137244943c1306942ccc857c7b5e107c5c5c503a74c99326cf8f0e1433333332fb8f0c20b7b676767f71697f1ba5b03898dd7dd1af0e50781aaadadaddebb77efa71b366c2877d822669b3afc3170c04328587af45107e66a591a06d8d0d0b0426c42c14a2c4e04ab5bb76e7748fc4018086795545b703a8221a11e7833ac9724b7db2470d44bdbfb3225c40a50b0f48158ad9338adc0b06f7ce2408ff50080d20524d853ea4b9482651dbc51205c9566173a9915e7552753628e93716e71980f88f75df88bf3a4a418142c7be05b6dbbfcb77045fd964352556c32edb6f14931522c686028045e7bbb78341f10c2842fca97a4ed7d97b2a909d66139232c5c0071449fd081b9c459555555e53d7bf61c2836c0d8724c5359b766ab31eca2f610770c790942f0a2a9630cb1c29008bb1c3870c0e93008bca7d02514f929fcd16b43f78950b0dca43e7454844f8c1d3bb6a7bab9416c02d1bafd07938d830483eddbb7c7da98bc52882318127a486b6bebcb42528ab2b2b26a219e41c1f290d0b08b4d425282cacaca776c54a7130b50b03c86c32e5287152b56ac8ff1346bf85c8095ee7140ad1c6d54371385242dd86ad3bf7fff58bbd1d1aeb84c8823e8b0e2005d56f2f3f0c30fffdee4122ef3ba00052b0ea031a0ba592c242979fbedb7d76b6c5c4e99ed335e42c18a1370591c29967c2014fcf9cf7f6e36cc14b096ca05285871223c524c8956b590a4006285715b151515663b1af60b93eeae40c18a2368bfac446b12452bf884c54ab38ce12321ae40c18a3314ade083a11016c40ace8af92b97a06025809068a1250013f1010309768b4321de10e21a14ac04819c56b76edd1e58ba74e974f68af73f353535e5980b585454f48246ce2a0cf695b29cc14558389a78d0d9a168eedcb95fbfe38e3b6e183468d055427c03846ad9b265ab34a72d4782501095ef29d7b3ca4b2858fe204b1d45b89d3a756aafc99327e78d1b376e68fffefd077a35e4829c4f783044757575b972519f2c5ab4e81d0b6e2a128855ca75038d07142cff7056b48404198a9587a40bf10b081d3e53c7c5d2162692e081e2d04d42b1f20c0a96bf806861b61cfe2ebd84040924d8311bf0b810cf6048e85f303607b3101922fa1b7461c04008d65ac5010a96ff418ff802690b15897fa05025000a567080d3c2a00bac18f60c3da6fb8a0fe1c1101029d45521d7c872054208218410420821841042082184104208218410420821841042082184104208218410420821841042082184104208218410420871c47f017fe53d874c131a7d0000000049454e44ae426082";

        #endregion

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddProxyDeviceTypeUp();
            AddKioskPagesUp();
            AddLabelDesignerPagesUp();
            AddCloudPrintPagesUp();
            AddCloudPrintExternalApplicationUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddCloudPrintExternalApplicationDown();
            AddCloudPrintPagesDown();
            AddLabelDesignerPagesDown();
            AddKioskPagesDown();
            AddProxyDeviceTypeDown();
        }

        private void AddProxyDeviceTypeUp()
        {
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.DEVICE_TYPE,
                "Proxy",
                "A proxy device that handles communication between the server and another device.",
                SystemGuid.DefinedValue.DEVICE_TYPE_CLOUD_PRINT_PROXY,
                true );
        }

        private void AddProxyDeviceTypeDown()
        {
            // Intentionally not removing defined value.
        }

        private void AddKioskPagesUp()
        {
            // Add new check-in theme.
            Sql( @"
DECLARE @CheckInPurposeValueId INT = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '2bbb1a44-708e-4469-80de-4aae6227bef8')
DECLARE @ThemeId INT = (SELECT TOP 1 [Id] FROM [Theme] WHERE [Name] = 'NextGenCheckin')

IF @ThemeId IS NOT NULL
BEGIN
    UPDATE [Theme]
    SET
        [PurposeValueId] = @CheckInPurposeValueId
    WHERE [Id] = @ThemeId
END
ELSE
BEGIN
    INSERT INTO [Theme] ([Name], [RootPath], [IsActive], [IsSystem], [PurposeValueId], [Guid])
    VALUES ('NextGenCheckin', '/Themes/NextGenCheckin', 1, 1, @CheckInPurposeValueId, NEWID())
END

" );
            // Add new check-in site.
            RockMigrationHelper.AddSite( "Next-gen Check-in", "The Rock default check-in site for the next-generation check-in kiosk.", "NextGenCheckin", SystemGuid.Site.NEXT_GEN_CHECK_IN );

            Sql( $@"
UPDATE [Site]
SET [AllowIndexing] = 0
WHERE [Guid] = '{SystemGuid.Site.NEXT_GEN_CHECK_IN}'" );

            // Site:Next-gen Check-in
            RockMigrationHelper.AddLayout( "BFBB35BD-D0B0-459E-9329-B082CE4F253E", "Checkin", "Checkin", "", "BC067A3C-1257-4D19-BAD7-505FD28F916B" );

            RockMigrationHelper.AddPage( true, string.Empty, SystemGuid.Layout.NEXT_GEN_CHECK_IN_CHECKIN, "Next-gen Check-in", "", "7D1732D5-3957-475F-A259-4DB8261C2049", "" );

            Sql( @"
DECLARE @PageId INT = (SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '7D1732D5-3957-475F-A259-4DB8261C2049')

IF @PageId IS NOT NULL
BEGIN
    UPDATE [Site] SET [DefaultPageId] = @PageId WHERE [Guid] = 'BFBB35BD-D0B0-459E-9329-B082CE4F253E'
END" );

            // Add Page 
            //  Internal Name: Check-in Kiosk
            //  Site: Next-gen Check-in
            RockMigrationHelper.AddPage( true, "7D1732D5-3957-475F-A259-4DB8261C2049", "BC067A3C-1257-4D19-BAD7-505FD28F916B", "Check-in Kiosk", "", "AA049E71-9C99-4DC3-A62B-6F01807A2BF4", "" );

            // Add Page Route
            //   Page:Next-gen Check-in
            //   Route:nextgen-checkin/setup
            RockMigrationHelper.AddOrUpdatePageRoute( "7D1732D5-3957-475F-A259-4DB8261C2049", "nextgen-checkin/setup", "5127B9CC-A5AF-40DB-B04B-C1BBDDB496EB" );

            // Add Page Route
            //   Page:Check-in Kiosk
            //   Route:nextgen-checkin
            RockMigrationHelper.AddOrUpdatePageRoute( "AA049E71-9C99-4DC3-A62B-6F01807A2BF4", "nextgen-checkin", "D24328B9-DCDA-4F57-BB04-51DE93106BE4" );

            // Add Block 
            //  Block Name: Check-in Kiosk
            //  Page Name: Check-in Kiosk
            //  Layout: -
            //  Site: Next-gen Check-in
            RockMigrationHelper.AddBlock( true, "AA049E71-9C99-4DC3-A62B-6F01807A2BF4".AsGuid(), null, "BFBB35BD-D0B0-459E-9329-B082CE4F253E".AsGuid(), "A27FD0AA-67EE-44C3-9E5F-3289C6A210F3".AsGuid(), "Check-in Kiosk", "Main", @"", @"", 0, "06EA3C9C-D930-47B7-B200-C29968EB51EF" );

            // Add Block 
            //  Block Name: Check-in Kiosk Setup
            //  Page Name: Next-gen Check-in
            //  Layout: -
            //  Site: Next-gen Check-in
            RockMigrationHelper.AddBlock( true, "7D1732D5-3957-475F-A259-4DB8261C2049".AsGuid(), null, "BFBB35BD-D0B0-459E-9329-B082CE4F253E".AsGuid(), "D42352A2-C48D-443B-A51D-31EA4CE0C5A4".AsGuid(), "Check-in Kiosk Setup", "Main", @"", @"", 0, "CE181DB5-483B-4BA0-AF2F-7CD847B7CF2D" );
            
            // Add Block Attribute Value
            //   Block: Check-in Kiosk Setup
            //   BlockType: Check-in Kiosk Setup
            //   Category: Check-in
            //   Block Location: Page=Next-gen Check-in, Site=Next-gen Check-in
            //   Attribute: Kiosk Page
            /*   Attribute Value: aa049e71-9c99-4dc3-a62b-6f01807a2bf4 */
            RockMigrationHelper.AddBlockAttributeValue( "CE181DB5-483B-4BA0-AF2F-7CD847B7CF2D", "3BEAE186-4F7A-4F5A-AD37-8EDE2D939D36", @"aa049e71-9c99-4dc3-a62b-6f01807a2bf4" );

            // Add Block Attribute Value
            //   Block: Check-in Kiosk
            //   BlockType: Check-in Kiosk
            //   Category: Check-in
            //   Block Location: Page=Check-in Kiosk, Site=Next-gen Check-in
            //   Attribute: Promotions Content Channel
            /*   Attribute Value: a57bdbcd-fa77-4a6e-967d-1c5ace962587 */
            RockMigrationHelper.AddBlockAttributeValue( "06EA3C9C-D930-47B7-B200-C29968EB51EF", "5AED205E-1477-4C29-B7F6-99DE33FCB8EB", @"a57bdbcd-fa77-4a6e-967d-1c5ace962587" );

            // Add Block Attribute Value
            //   Block: Check-in Kiosk
            //   BlockType: Check-in Kiosk
            //   Category: Check-in
            //   Block Location: Page=Check-in Kiosk, Site=Next-gen Check-in
            //   Attribute: Setup Page
            /*   Attribute Value: 7d1732d5-3957-475f-a259-4db8261c2049 */
            RockMigrationHelper.AddBlockAttributeValue( "06EA3C9C-D930-47B7-B200-C29968EB51EF", "A380BDB9-96CF-4F90-A921-FED850C67B59", @"7d1732d5-3957-475f-a259-4db8261c2049" );
        }

        private void AddKioskPagesDown()
        {
            // Remove Block
            //  Name: Check-in Kiosk, from Page: Check-in Kiosk, Site: Next-gen Check-in
            //  from Page: Check-in Kiosk, Site: Next-gen Check-in
            RockMigrationHelper.DeleteBlock( "06EA3C9C-D930-47B7-B200-C29968EB51EF" );

            // Remove Block
            //  Name: Check-in Kiosk Setup, from Page: Next-gen Check-in, Site: Next-gen Check-in
            //  from Page: Next-gen Check-in, Site: Next-gen Check-in
            RockMigrationHelper.DeleteBlock( "CE181DB5-483B-4BA0-AF2F-7CD847B7CF2D" );

            // Delete Page 
            //  Internal Name: Check-in Kiosk
            //  Site: Next-gen Check-in
            //  Layout: Checkin
            RockMigrationHelper.DeletePage( "AA049E71-9C99-4DC3-A62B-6F01807A2BF4" );

            Sql( @"
DECLARE @PageId INT = (SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '7D1732D5-3957-475F-A259-4DB8261C2049')

IF @PageId IS NOT NULL
BEGIN
    UPDATE [Site] SET [DefaultPageId] = NULL WHERE [DefaultPageId] = @PageId
END" );

            // Delete Page 
            //  Internal Name: Next-gen Check-in
            //  Site: Next-gen Check-in
            //  Layout: Checkin
            RockMigrationHelper.DeletePage( "7D1732D5-3957-475F-A259-4DB8261C2049" );

            RockMigrationHelper.DeleteSite( SystemGuid.Site.NEXT_GEN_CHECK_IN );
        }

        private void AddLabelDesignerPagesUp()
        {
            // Add Page 
            //  Internal Name: Next-gen Labels
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "66C5DD58-094C-4FF9-9AFB-44801FCFCC2D", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Next-gen Labels", "", "FD2A703D-528E-4763-AB87-5CFEB2349259", "fa fa-print", "7c093a63-f2ac-4fe3-a826-8bf06d204ea2" );

            // Add Page 
            //  Internal Name: Label Detail
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "FD2A703D-528E-4763-AB87-5CFEB2349259", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Label Detail", "", "8DE681F3-0FE1-45B3-8CED-747E942BE135", "" );

            Sql( @"
UPDATE [Page]
SET [BreadCrumbDisplayName] = 0
WHERE [Guid] = '8DE681F3-0FE1-45B3-8CED-747E942BE135'" );

            // Add Page 
            //  Internal Name: Label Designer
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "8DE681F3-0FE1-45B3-8CED-747E942BE135", "c2467799-bb45-4251-8ee6-f0bf27201535", "Label Designer", "", "C165DF04-2217-41AD-95D6-AD3CDCE667FD", "" );

            // Add Page Route
            //   Page:Next-gen Labels
            //   Route:admin/checkin/nextgenlabels
            RockMigrationHelper.AddOrUpdatePageRoute( "FD2A703D-528E-4763-AB87-5CFEB2349259", "admin/checkin/nextgenlabels", "aed9acf2-6698-4868-8da6-81d1646933b1" );

            // Add Page Route
            //   Page:Label Detail
            //   Route:admin/checkin/nextgenlabels/{CheckInLabelId}
            RockMigrationHelper.AddOrUpdatePageRoute( "8DE681F3-0FE1-45B3-8CED-747E942BE135", "admin/checkin/nextgenlabels/{CheckInLabelId}", "372c87bf-8486-48e7-ab59-0903ee726165" );

            // Add Page Route
            //   Page:Label Designer
            //   Route:admin/checkin/nextgenlabels/{CheckInLabelId}/designer
            RockMigrationHelper.AddOrUpdatePageRoute( "C165DF04-2217-41AD-95D6-AD3CDCE667FD", "admin/checkin/nextgenlabels/{CheckInLabelId}/designer", "8f61cb27-c679-4e7e-8b8a-8e9c79df406b" );

            // Add Block 
            //  Block Name: Check-in Label List
            //  Page Name: Next-gen Labels
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "FD2A703D-528E-4763-AB87-5CFEB2349259".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "357014CB-376C-4957-A031-51A8371C3EBF".AsGuid(), "Check-in Label List", "Main", @"", @"", 0, "0C4CBE43-FCA5-44D0-BD83-9ABE0033DB47" );

            // Add Block 
            //  Block Name: Check-in Label Detail
            //  Page Name: Label Detail
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "8DE681F3-0FE1-45B3-8CED-747E942BE135".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "3299706F-2BB8-49DB-831B-86A2B282BB02".AsGuid(), "Check-in Label Detail", "Main", @"", @"", 0, "4EAF5330-9A59-40C1-ADF5-FAE27481BFE0" );

            // Add Block 
            //  Block Name: Label Designer
            //  Page Name: Label Designer
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "C165DF04-2217-41AD-95D6-AD3CDCE667FD".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "8C4AD18F-9F81-4145-8AD0-AB90E451D0D6".AsGuid(), "Label Designer", "Main", @"", @"", 0, "D6775798-2488-4C03-A2FE-B16E8765C28C" );

            // Add Block Attribute Value
            //   Block: Check-in Label List
            //   BlockType: Check-in Label List
            //   Category: Check-in > Configuration
            //   Block Location: Page=Next-gen Labels, Site=Rock RMS
            //   Attribute: Detail Page
            /*   Attribute Value: 8de681f3-0fe1-45b3-8ced-747e942be135 */
            RockMigrationHelper.AddBlockAttributeValue( "0C4CBE43-FCA5-44D0-BD83-9ABE0033DB47", "3C5684B3-C486-4FA9-BA05-19E02DC60B3D", @"8de681f3-0fe1-45b3-8ced-747e942be135" );

            // Add Block Attribute Value
            //   Block: Check-in Label Detail
            //   BlockType: Check-in Label Detail
            //   Category: Check-in > Configuration
            //   Block Location: Page=Label Detail, Site=Rock RMS
            //   Attribute: Designer Page
            /*   Attribute Value: c165df04-2217-41ad-95d6-ad3cdce667fd */
            RockMigrationHelper.AddBlockAttributeValue( "4EAF5330-9A59-40C1-ADF5-FAE27481BFE0", "0E94E533-87B1-4654-946B-03EB640F1D41", @"c165df04-2217-41ad-95d6-ad3cdce667fd" );
        }

        private void AddLabelDesignerPagesDown()
        {
            // Remove Block
            //  Name: Label Designer, from Page: Label Designer, Site: Rock RMS
            //  from Page: Label Designer, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "D6775798-2488-4C03-A2FE-B16E8765C28C" );

            // Remove Block
            //  Name: Check-in Label Detail, from Page: Label Detail, Site: Rock RMS
            //  from Page: Label Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "4EAF5330-9A59-40C1-ADF5-FAE27481BFE0" );

            // Remove Block
            //  Name: Check-in Label List, from Page: Next-gen Labels, Site: Rock RMS
            //  from Page: Next-gen Labels, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "0C4CBE43-FCA5-44D0-BD83-9ABE0033DB47" );

            // Delete Page 
            //  Internal Name: Label Designer
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "C165DF04-2217-41AD-95D6-AD3CDCE667FD" );

            // Delete Page 
            //  Internal Name: Label Detail
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "8DE681F3-0FE1-45B3-8CED-747E942BE135" );

            // Delete Page 
            //  Internal Name: Next-gen Labels
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "FD2A703D-528E-4763-AB87-5CFEB2349259" );
        }

        private void AddCloudPrintPagesUp()
        {
            // Add Page 
            //  Internal Name: Cloud Print
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "66C5DD58-094C-4FF9-9AFB-44801FCFCC2D", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Cloud Print", "", "8bbe9720-0b96-46ef-9fe5-ccad48e7abda", "fa fa-signal" );

            // Add Block 
            //  Block Name: Cloud Print Monitor
            //  Page Name: Cloud Print
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "8bbe9720-0b96-46ef-9fe5-ccad48e7abda".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "8F436A19-482A-41A7-AAB3-E5EC34D15D19".AsGuid(), "Cloud Print Monitor", "Main", @"", @"", 0, "decd55d7-fe1c-4563-8e9c-6bdd6966526b" );
        }

        private void AddCloudPrintPagesDown()
        {
            RockMigrationHelper.DeleteBlock( "decd55d7-fe1c-4563-8e9c-6bdd6966526b" );
            RockMigrationHelper.DeletePage( "8bbe9720-0b96-46ef-9fe5-ccad48e7abda" );
        }

        private void AddCloudPrintExternalApplicationUp()
        {
            Sql( $@"
DECLARE
	@BinaryFileId int
	,@BinaryFileTypeIdDefault int = (SELECT TOP 1 Id from [BinaryFileType] where [Guid] = '{Rock.SystemGuid.BinaryFiletype.DEFAULT}')
	,@StorageEntityTypeIdDatabase int = (SELECT TOP 1 Id FROM [EntityType] WHERE [Guid] = '{Rock.SystemGuid.EntityType.STORAGE_PROVIDER_DATABASE}')

-- Add logo.jpg
IF NOT EXISTS (SELECT * FROM [BinaryFile] WHERE [Guid] = '45ee3a13-1087-4167-8427-d7a1f8b905f5' )
BEGIN
INSERT INTO [BinaryFile] ([IsTemporary], [IsSystem], [BinaryFileTypeId], [FileName], [MimeType], [StorageEntityTypeId], [Guid])
			VALUES (0,0, @BinaryFileTypeIdDefault, 'logo.jpg', 'image/jpeg', @StorageEntityTypeIdDatabase, '45ee3a13-1087-4167-8427-d7a1f8b905f5')

SET @BinaryFileId = SCOPE_IDENTITY()

INSERT INTO [BinaryFileData] ([Id], [Content], [Guid])
  VALUES ( 
    @BinaryFileId
    ,{CloudPrintLogo}
    ,'45ee3a13-1087-4167-8427-d7a1f8b905f5'
    )
END
" );

            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.EXTERNAL_APPLICATION,
                "Rock Cloud Print (beta)",
                "The Rock Cloud Print service will connect to your Rock server and handle printing requests for check-in labels.",
                "d63ee996-bfec-42eb-8cda-7b5c7d6213cf",
                true );

            // Attribute Value: Icon
            RockMigrationHelper.UpdateDefinedValueAttributeValue( "d63ee996-bfec-42eb-8cda-7b5c7d6213cf",
                "c6e82af0-2128-492b-b5cb-7915630dda0b",
                "45ee3a13-1087-4167-8427-d7a1f8b905f5" );

            // Attribute Value: Vendor
            RockMigrationHelper.UpdateDefinedValueAttributeValue( "d63ee996-bfec-42eb-8cda-7b5c7d6213cf",
                "e9aae4d6-b4dc-4aa2-bd86-d63b2b4d26f3",
                "Spark Development Network" );

            // Attribute Value: Download Url
            RockMigrationHelper.UpdateDefinedValueAttributeValue( "d63ee996-bfec-42eb-8cda-7b5c7d6213cf",
                "e0af9b30-15ea-413b-bac4-25b286d91fd9",
                "https://storage.rockrms.com/externalapplications/sparkdevnetwork/rockcloudprint/1.0.0/Rock.CloudPrint.Installer.msi" );
        }

        private void AddCloudPrintExternalApplicationDown()
        {
            RockMigrationHelper.DeleteDefinedValue( "d63ee996-bfec-42eb-8cda-7b5c7d6213cf" );
        }
    }
}
