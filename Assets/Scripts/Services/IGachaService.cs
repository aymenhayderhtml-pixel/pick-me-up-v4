using System.Collections.Generic;
using PickMeUp.Data;

namespace PickMeUp.Services
{
    public interface IGachaService
    {
        List<HeroInstance> PullStandard(int count);
        List<HeroInstance> PullPremium(int count);

        bool CanAffordStandard(int count);
        bool CanAffordPremium(int count);
        
        int GetPityCount(int bannerId);
        
        // NEW: Exposed for the UI Guarantee Badge
        bool IsPremiumGuaranteed(); 
    }
}