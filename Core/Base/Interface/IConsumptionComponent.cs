
using System.Collections.Generic;
using Pi.Core.Common;

namespace Pi.Core
{
    public interface IConsumptionComponent : IComponent
    {
        bool HasEnough(List<Pair<int>> needed);
        List<Pair<int>> GetDeficits(List<Pair<int>> needed);
    }
}