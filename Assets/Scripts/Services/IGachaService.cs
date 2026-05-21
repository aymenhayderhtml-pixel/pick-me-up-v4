using PickMeUp.Data;

namespace PickMeUp.Services
{
    public interface IGachaService
    {
        GachaPullResult Pull(GameSaveData saveData);
    }
}