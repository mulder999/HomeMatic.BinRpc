using CreativeCoders.Core;
using CreativeCoders.Core.IO;
using CreativeCoders.Net.Http;
using CreativeCoders.Net.XmlRpc;
using CreativeCoders.Net.XmlRpc.Client;
using CreativeCoders.Net.XmlRpc.Definition;
using CreativeCoders.Net.XmlRpc.Exceptions;
using CreativeCoders.Net.XmlRpc.Model;
using CreativeCoders.Net.XmlRpc.Model.Values.Converters;
using CreativeCoders.Net.XmlRpc.Reader;
using CreativeCoders.Net.XmlRpc.Reader.Values;
using CreativeCoders.Net.XmlRpc.Writer;
using CreativeCoders.Net.XmlRpc.Writer.Values;
using HomeMaticBinRpc.Converters;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HomeMaticBinRpc.Clients
{
    public class BinRpcClient : IXmlRpcClient
    {
        #region Members
        private readonly IRequestBuilder requestBuilder = new RequestBuilder(new DataToBinRpcValueConverter());
        private readonly IDataToXmlRpcValueConverter converter = new DataToXmlRpcValueConverter();
        private readonly IHttpClient _httpClient;
        #endregion

        #region Properties
        public string Url { get; set; }
        public Encoding XmlEncoding { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string HttpContentType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        #endregion

        #region Constructors

        public BinRpcClient(IHttpClient httpClient, string url)
        {
            _httpClient = httpClient;
            Url = url;
        }

        #endregion

        #region Public Methods
        public Task ExecuteAsync(string methodName, params object[] parameters)
        {
            throw new NotImplementedException();
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
            HttpRequestMessage httpRequest = await CreateHttpRequestAsync(request).ConfigureAwait(false);

            XmlRpcResponse xmlRpcResponse = await ReadResponseAsync(
                await _httpClient.SendRequestAsync(httpRequest, HttpCompletionOption.ResponseContentRead, CancellationToken.None)
                    .ConfigureAwait(false))
                .ConfigureAwait(false);
            if (xmlRpcResponse.Results.FirstOrDefault()?.IsFaulted ?? false)
            {
                throw new FaultException(xmlRpcResponse.Results.FirstOrDefault()?.FaultCode ?? 0, xmlRpcResponse.Results.FirstOrDefault()?.FaultString);
            }
            return xmlRpcResponse;
        }

        #endregion

        #region Private Methods

        private Task<HttpRequestMessage> CreateHttpRequestAsync(XmlRpcRequest request)
        {
            using var encoder = new BinRpcDataEncoder();

            foreach (var call in request.Methods)
            {
                encoder.EncodeRequest(call.Name, call.Parameters.Select(x => x.Data).ToArray());
            }

            var message = new HttpRequestMessage(HttpMethod.Post, Url)
            {
                Content = new ByteArrayContent(encoder.Buffer)
            };

            return Task.FromResult(message);
        }

        private async Task<XmlRpcResponse> ReadResponseAsync(HttpResponseMessage httpResponse)
        {
            var stream = await httpResponse.Content.ReadAsStreamAsync().ConfigureAwait(false);

            using var decoder = new BinRpcDataDecoder(stream);
            var message = (HomematicMessageResponse)decoder.DecodeMessage();
            var response = converter.Convert(message.Response);
            var methodResult = new XmlRpcMethodResult(response);
            return new XmlRpcResponse(new XmlRpcMethodResult[] { methodResult }, false);


            #endregion

        }
    }
}
