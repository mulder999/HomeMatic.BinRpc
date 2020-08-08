using CreativeCoders.DynamicCode.Proxying;
using CreativeCoders.HomeMatic.XmlRpc.Client;
using HomeMaticBinRpc.Proxy;

namespace HomeMaticBinRpc.Clients
{
    public class HomeMaticBinRpcApiBuilder<T> : IHomeMaticXmlRpcApiBuilder
        where T : class, IHomeMaticXmlRpcApi
    {
        #region Members

        private string _url;

        #endregion

        public static HomeMaticBinRpcApiBuilder<T> Create()
        {
            return new HomeMaticBinRpcApiBuilder<T>();
        }

        public IHomeMaticXmlRpcApi Build()
        {
            return new BinRpcProxyBuilder<T>(
                new ProxyBuilder<T>())
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
