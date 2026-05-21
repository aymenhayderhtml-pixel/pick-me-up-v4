// Assets/Scripts/Data/GameSaveData.cs
using System;
using System.Collections.Generic;

namespace PickMeUp.Data
{
    /// <summary>
    /// Tracks the player's permanent meta-progression and unlocked authority nodes.
    /// </summary>
    [Serializable]
    public class MasterAuthorityData
    {
        public int AuthorityLevel;
        public int TotalPointsEarned;
        public List<string> UnlockedNodeIds;

        public MasterAuthorityData()
        {
            UnlockedNodeIds = new List<string>();
        }
    }

    /// <summary>
    /// Entry for tracking pity counter per banner.
    /// </summary>
    [Serializable]
    public struct BannerPityEntry
    {
        public int BannerId;
        public int PullCount;
    }

    /// <summary>
    /// Entry for tracking guaranteed rate-up status per banner.
    /// </summary>
    [Serializable]
    public struct BannerGuarantee
    {
        public int BannerId;
        public bool IsGuaranteed;
    }

    /// <summary>
    /// Tracks the pity counters and guarantee states for various gacha banners.
    /// Uses serializable lists instead of dictionaries for JSON compatibility.
    /// </summary>
    [Serializable]
    public class GachaPityData
    {
        public List<BannerPityEntry> BannerPityCounters;
        public List<BannerGuarantee> GuaranteedRateUp;
        
        public GachaPityData()
        {
            BannerPityCounters = new List<BannerPityEntry>();
            GuaranteedRateUp = new List<BannerGuarantee>();
        }
    }

    /// <summary>
    /// Logs a single synthesis attempt for historical tracking and analytics.
    /// </summary>
    [Serializable]
    public struct SynthesisLogEntry
    {
        public string TargetHeroDefId;
        public int PreviousStar;
        public int NewStar;
        public bool Success;
        public string Timestamp;
    }

    /// <summary>
    /// The top-level serializable container for the entire game save.
    /// Handled by the ISaveLoadService for persistence.
    /// </summary>
    [Serializable]
    public class GameSaveData
    {
        #region Metadata

        public int SchemaVersion = 1;
        public string Timestamp;

        #endregion

        #region Roster & Progression

        public List<HeroInstance> HeroRoster;
        public int FloorProgress;
        public int HighestFloorCleared;
        public long TotalXPEarnedAllTime;

        #endregion

        #region Economy

        public int Gems;
        public int Gold;
        public int Tickets;

        #endregion

        #region Meta & Gacha

        public MasterAuthorityData Master;
        public GachaPityData Pity;
        public string LastSeed;
        public List<SynthesisLogEntry> SynthesisHistory;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new, default GameSaveData instance for a fresh profile.
        /// </summary>
        public GameSaveData()
        {
            HeroRoster = new List<HeroInstance>();
            Master = new MasterAuthorityData();
            Pity = new GachaPityData();
            SynthesisHistory = new List<SynthesisLogEntry>();
            Timestamp = DateTime.UtcNow.ToString("o");
        }

        #endregion
    }
}