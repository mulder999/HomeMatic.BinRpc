using CreativeCoders.HomeMatic.XmlRpc;
using CreativeCoders.HomeMatic.XmlRpc.Client;
using CreativeCoders.Net.XmlRpc.Definition;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeMaticBinRpc.Clients
{
    public interface IHomeMaticXmlRpcApiExt : IHomeMaticXmlRpcApi
    {
        [Flags]
        public enum DeleteFlags : int
        {
            Reset = 0x01,
            Force = 0x02,
            Defer = 0x04,
        }

        [XmlRpcMethod("system.listMethods")]
        Task<IEnumerable<string>> ListMethods();

        [XmlRpcMethod("system.methodHelp")]
        Task<string> MethodHelp(string methodName);
        
        [XmlRpcMethod("system.multicall")]
        Task<IEnumerable<object>> MultiCall(RpcCall[] calls);

        [XmlRpcMethod("deleteDevice")]
        Task DeleteDevice(string address, DeleteFlags flags);

        [XmlRpcMethod("reportValueUsage")]
        Task<bool> ReportValueUsage(string address, string validId, int refCounter);

        [XmlRpcMethod("listReplaceableDevices")]
        Task<IEnumerable<DeviceDescription>> ListReplaceableDevices(string newDeviceAddress);
    }
}
