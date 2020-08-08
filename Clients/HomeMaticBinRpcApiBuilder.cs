using CreativeCoders.DynamicCode.Proxying;
using CreativeCoders.HomeMatic.XmlRpc.Client;
using HomeMaticBinRpc.Proxy;

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
            return new BinRpcProxyBuilder<IHomeMaticXmlRpcApi>(
                new ProxyBuilder<IHomeMaticXmlRpcApi>())
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
