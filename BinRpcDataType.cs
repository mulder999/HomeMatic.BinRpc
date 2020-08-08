namespace HomeMaticBinRpc
{
    public enum BinRpcDataType : int
    {
        Integer = 0x01,
        Bool = 0x02,
        String = 0x03,
        Double = 0x04,
        Array = 0x100,
        Struct = 0x101,
    }
}
