using System.Threading.Tasks;
namespace Kurisu.VirtualHuman
{
    public interface ILLMDriver
    {
        Task<ILLMData> ProcessLLM(string message);
    }
}
