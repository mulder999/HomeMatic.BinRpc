using CreativeCoders.Net.XmlRpc.Definition;

namespace HomeMaticBinRpc.Clients
{
    public class RpcCall
    {
        [XmlRpcStructMember("methodname", Required = true)]
        public string MethodName { get; set; }
        [XmlRpcStructMember("params", Required = true)]
        public object[] Params { get; set; }
    }
}
