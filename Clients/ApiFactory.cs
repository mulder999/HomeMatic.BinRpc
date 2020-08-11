using CreativeCoders.DynamicCode.Proxying;
using CreativeCoders.Net.Http;
using CreativeCoders.Net.XmlRpc.Client;
using CreativeCoders.Net.XmlRpc.Definition;
using CreativeCoders.Net.XmlRpc.Proxy;
using CreativeCoders.Net.XmlRpc.Proxy.Analyzing;
using CreativeCoders.Net.XmlRpc.Proxy.Specification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;

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

            var methods = GetMethods(typeof(T));
            var exceptionHandler = GetExceptionHandler<T>();

            return new ProxyBuilder<T>()
                .Build(new XmlRpcProxyInterceptor<T>(client,
                    new ApiStructure()
                    {
                        MethodInfos = methods.Select(m => new ApiMethodAnalyzer(m, exceptionHandler).Analyze())
                    }));
        }

        private static IEnumerable<MethodInfo> GetMethods(Type t)
        {
            IEnumerable<MethodInfo> methods = t.GetMethods();
            var types = t.GetInterfaces();
            foreach (var type in types)
            {
                methods = methods.Union(GetMethods(type));

            }
            return methods;
        }

        private static IMethodExceptionHandler GetExceptionHandler<T>()
        {
            var customAttribute = typeof(T).GetCustomAttribute<GlobalExceptionHandlerAttribute>();
            if (customAttribute == null)
            {
                return null;
            }
            return Activator.CreateInstance(customAttribute.ExceptionHandler) as IMethodExceptionHandler;
        }
    }
}
