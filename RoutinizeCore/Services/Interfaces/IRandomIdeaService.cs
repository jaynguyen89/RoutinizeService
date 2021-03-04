using System.Threading.Tasks;
using RoutinizeCore.Models;

namespace RoutinizeCore.Services.Interfaces {

    public interface IRandomIdeaService {

        Task<int?> InsertNewRandomIdea(RandomIdea randomIdea);
        
        Task<bool?> UpdateRandomIdea(RandomIdea randomIdea);
        
        Task<bool?> DoesRandomIdeaBelongToThisUser(int randomIdeaId, int userId);
        
        Task<bool?> DeleteRandomIdeaById(int randomIdeaId);
        
        Task<bool?> ArchiveRandomIdeaById(int randomIdeaId);
        
        Task<bool?> ReviveRandomIdeaById(int randomIdeaId);
        
        Task<bool?> DoAllIdeasBelongToThisUser(int[] ideaIds, int userId);
        
        Task<RandomIdea[]> GetRandomIdeasByUserId(int userId);
    }
}