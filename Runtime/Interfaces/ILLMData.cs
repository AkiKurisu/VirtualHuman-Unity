namespace Kurisu.VirtualHuman
{
    public interface ILLMData
    {
        bool Status { get; }
        string Response { get; }
    }
}
