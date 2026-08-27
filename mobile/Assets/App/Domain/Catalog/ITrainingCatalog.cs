using System.Collections.Generic;
using System.Threading.Tasks;
using SurakshaAR.Domain.Training;

namespace SurakshaAR.Domain.Catalog
{
    public interface ITrainingCatalog
    {
        Task<ScenarioBundle> Get(string moduleId, int? version = null);

        Task<IReadOnlyList<ScenarioBundle>> List();
    }
}
