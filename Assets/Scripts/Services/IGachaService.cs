// Assets/Scripts/Services/IGachaService.cs
using PickMeUp.Data;

namespace PickMeUp.Services
{
    public interface IGachaService
    {
        HeroInstance Pull(int bannerId);
        HeroInstance[] PullMultiple(int bannerId, int count);
        void TrackPity(int bannerId, bool gotLegendary);
        int GetPityCount(int bannerId);
    }
}