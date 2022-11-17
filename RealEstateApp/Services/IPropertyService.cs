using RealEstateApp.Models;

namespace RealEstateApp.Services
{
    public interface IPropertyService
    {
        List<Agent> GetAgents();
        List<Property> GetProperties();
        void SaveProperty(Property property);
    }
}
