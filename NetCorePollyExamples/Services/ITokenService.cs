using System.Threading.Tasks;

namespace NetCorePollyExamples.Services
{
    public interface ITokenService
    {
        Task<bool> Validate(string token);
    }
}
