using CreativeCoders.Core;
using CreativeCoders.DynamicCode.Proxying;
using CreativeCoders.Net.XmlRpc.Proxy;
using CreativeCoders.Net.XmlRpc.Proxy.Analyzing;
using HomeMaticBinRpc.Clients;
using System;
using System.Text;

namespace HomeMaticBinRpc.Proxy
{
    public class BinRpcProxyBuilder<T> : IXmlRpcProxyBuilder<T>
        where T : class
    {
        private readonly IProxyBuilder<T> _proxyBuilder;

        private string _url;

        public BinRpcProxyBuilder(IProxyBuilder<T> proxyBuilder)
        {
            Ensure.IsNotNull(proxyBuilder, "proxyBuilder");
            if (!typeof(T).IsInterface)
            {
                throw new ArgumentException("Generic type '" + typeof(T).Name + "' must be an interface");
            }
            _proxyBuilder = proxyBuilder;
        }

        public T Build()
        {
            return _proxyBuilder.Build(new XmlRpcProxyInterceptor<T>(
                new BinRpcClient(_url),
                new ApiAnalyzer<T>().Analyze()));
        }

        public IXmlRpcProxyBuilder<T> ForUrl(string url)
        {
            _url = url;
            return this;
        }

        public IXmlRpcProxyBuilder<T> UseEncoding(Encoding encoding)
        {
            return this;
        }

        public IXmlRpcProxyBuilder<T> UseEncoding(string encodingName)
        {
            return this;
        }

        public IXmlRpcProxyBuilder<T> WithContentType(string contentType)
        {
            return this;
        }
    }
}
