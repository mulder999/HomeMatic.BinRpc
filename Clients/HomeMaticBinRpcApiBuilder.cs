using CreativeCoders.Core;
using CreativeCoders.DynamicCode.Proxying;
using CreativeCoders.HomeMatic.XmlRpc.Client;
using CreativeCoders.Net.Http;
using HomeMaticBinRpc.Proxy;
using System.Net.Http;

namespace HomeMaticBinRpc.Clients
{
    public class HomeMaticBinRpcApiBuilder : IHomeMaticXmlRpcApiBuilder
    {
        #region Members

        private string _url;

        #endregion

        public static HomeMaticBinRpcApiBuilder Create()
        {
            return new HomeMaticBinRpcApiBuilder();
        }

        public IHomeMaticXmlRpcApi Build()
        {
            var httpClient = new HttpClient();
            return new BinRpcProxyBuilder<IHomeMaticXmlRpcApi>(
                new ProxyBuilder<IHomeMaticXmlRpcApi>(), 
                new DelegateClassFactory<IHttpClient>(() => new HttpClientEx(httpClient)))
                .ForUrl(_url)
                .Build();
        }

        public IHomeMaticXmlRpcApiBuilder ForUrl(string url)
        {
            _url = url;
            return this;
        }
    }
}
