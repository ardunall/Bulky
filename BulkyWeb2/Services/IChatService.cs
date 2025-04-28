using System.Threading.Tasks;

namespace BulkyWeb2.Services
{
    public interface IChatService
    {
 
        Task<string> GetResponseFromOllama(string message);
    }
} 