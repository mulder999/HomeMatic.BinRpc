namespace HomeMaticBinRpc
{
    public enum BinRpcCommandType : byte
    {
        Method = 0x00,
        Response = 0x01,
        Error = 0xFF
    }
}
