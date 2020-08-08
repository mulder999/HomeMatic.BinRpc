using CreativeCoders.HomeMatic.XmlRpc.Client;
using CreativeCoders.Net.XmlRpc.Definition;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeMaticBinRpc.Clients
{
    public interface IHomeMaticXmlRpcApiExt : IHomeMaticXmlRpcApi
    {
        [XmlRpcMethod("system.listMethods")]
        Task<IEnumerable<string>> ListMethods();
    }
}
