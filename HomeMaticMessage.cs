namespace HomeMaticBinRpc
{
    public interface IHomeMaticMessage
    {
    }

    public class HomeMaticMessageRpc : IHomeMaticMessage
    {
        public string Method { get; set; }
        public object[] Parameters { get; set; }
    }

    public class HomematicMessageResponse : IHomeMaticMessage
    {
        public object Response { get; set; }
    }
}
