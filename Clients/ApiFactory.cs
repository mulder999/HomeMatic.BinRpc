using CreativeCoders.DynamicCode.Proxying;
using CreativeCoders.Net.Http;
using CreativeCoders.Net.XmlRpc.Client;
using CreativeCoders.Net.XmlRpc.Proxy;
using CreativeCoders.Net.XmlRpc.Proxy.Analyzing;
using System.Net.Http;

namespace HomeMaticBinRpc.Clients
{
    public static class ApiFactory
    {

        public static T GetApi<T>(bool useXml, string url)
            where T : class
        {
            var client = useXml
                ? (IXmlRpcClient)new XmlRpcClient(new HttpClientEx(new HttpClient())) { Url = url }
                : new BinRpcClient(url);
            return new ProxyBuilder<T>()
                .Build(new XmlRpcProxyInterceptor<T>(client, new ApiAnalyzer<T>().Analyze()));
        }
    }
}
