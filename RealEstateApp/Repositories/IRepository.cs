using RealEstateApp.Models;

namespace RealEstateApp.Repositories
{
    public interface IRepository
    {
        List<Agent> GetAgents();
        List<Property> GetProperties();
        void SaveProperty(Property property);
    }
}
