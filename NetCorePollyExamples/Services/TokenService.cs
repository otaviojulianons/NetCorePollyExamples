using System;
using System.Threading.Tasks;

namespace NetCorePollyExamples.Services
{
    public class TokenService : ITokenService
    {

        public async Task<bool> Validate(string token)
        {
            switch (token)
            {
                case "valid":
                    return true;
                case "invalid":
                    return false;
                case "valid-slow":
                    await Task.Delay(5000);
                    return true;
                case "invalid-slow":
                    await Task.Delay(5000);
                    return false;
                case "exception":
                    throw new Exception("My Exception");
                default:
                    return false;
            }
        }
    }
}
