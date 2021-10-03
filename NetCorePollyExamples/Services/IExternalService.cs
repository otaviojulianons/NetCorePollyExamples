using Refit;
using System.Threading.Tasks;

namespace NetCorePollyExamples.Services
{
    public interface IExternalService
    {
        [Get("/external/slow")]
        Task<string> Slow();

        [Get("/external/unstable")]
        Task<string> Unstable();
    }
}
