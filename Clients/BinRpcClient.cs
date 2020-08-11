using CreativeCoders.Core;
using CreativeCoders.Net.XmlRpc;
using CreativeCoders.Net.XmlRpc.Client;
using CreativeCoders.Net.XmlRpc.Definition;
using CreativeCoders.Net.XmlRpc.Exceptions;
using CreativeCoders.Net.XmlRpc.Model;
using CreativeCoders.Net.XmlRpc.Model.Values.Converters;
using HomeMaticBinRpc.Converters;
using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace HomeMaticBinRpc.Clients
{
    public class BinRpcClient : IXmlRpcClient
    {
        #region Members
        private readonly IRequestBuilder requestBuilder = new RequestBuilder(new DataToXmlRpcValueConverter());
        private readonly IDataToXmlRpcValueConverter converter = new DataToXmlRpcValueConverter();
        #endregion

        #region Properties
        public string Url { get; set; }
        public Encoding XmlEncoding { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string HttpContentType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        #endregion

        #region Constructors

        public BinRpcClient(string url)
        {
            Url = url;
        }

        #endregion

        #region Public Methods
        public Task ExecuteAsync(string methodName, params object[] parameters)
        {
            return InvokeAsync(methodName, parameters);
        }

        public Task<XmlRpcResponse> InvokeAsync(string methodName, params object[] parameters)
        {
            var request = requestBuilder.Build(methodName, parameters);
            return SendRequestAsync(request);
        }

        public Task<T> InvokeAsync<T>(string methodName, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public Task<XmlRpcResponse> InvokeExAsync(string methodName, object[] parameters)
        {
            return InvokeAsync(methodName, parameters);
        }

        public Task<T> InvokeExAsync<T>(string methodName, object[] parameters)
        {
            throw new NotImplementedException();
        }

        public Task<T> InvokeExAsync<T, TInvoke>(string methodName, object[] parameters, IMethodResultConverter resultConverter)
        {
            throw new NotImplementedException();
        }

        public async Task<XmlRpcResponse> SendRequestAsync(XmlRpcRequest request)
        {
            Ensure.IsNotNull(request, "request");

            using var encoder = new BinRpcDataEncoder();
            foreach (var call in request.Methods)
            {
                encoder.EncodeRequest(call.Name, call.Parameters.Select(x => x.Data).ToArray());
            }

            var uri = new Uri(Url);
            using var tcpClient = new TcpClient(uri.Host, uri.Port);
            var ns = tcpClient.GetStream();
            await encoder.Write(ns);

            var xmlRpcResponse = await ReadResponseAsync(ns);

            var fault = xmlRpcResponse.Results.FirstOrDefault(x => x.IsFaulted);
            if (fault != null)
            {
                throw new FaultException(fault.FaultCode, fault.FaultString);
            }
            return xmlRpcResponse;
        }
        #endregion

        #region Private Methods

        private Task<XmlRpcResponse> ReadResponseAsync(Stream stream)
        {
            var decoder = new BinRpcDataDecoder(stream);
            var message = decoder.DecodeMessage();

            var error = message as HomematicMessageError;

            var methodResult = error != null
                ? new XmlRpcMethodResult(error.FaultCode, error.FaultString)
                : new XmlRpcMethodResult(converter.Convert(((HomematicMessageResponse)message).Response));
            var xmlRpc = new XmlRpcResponse(new XmlRpcMethodResult[] { methodResult }, false);

            return Task.FromResult(xmlRpc);
        }

        #endregion
    }
}
