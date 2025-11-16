using System.Collections.ObjectModel;
using BlueMeter.Core.Analyze.Models;
using BlueMeter.Core.Data;
using BlueMeter.Core.Data.Models;

namespace BlueMeter.WPF.Data;

public interface IDataStorage : IDisposable
{
    PlayerInfo CurrentPlayerInfo { get; }

    ReadOnlyDictionary<long, PlayerInfo> ReadOnlyPlayerInfoDatas { get; }

    ReadOnlyDictionary<long, DpsData> ReadOnlyFullDpsDatas { get; }

    IReadOnlyList<DpsData> ReadOnlyFullDpsDataList { get; }

    ReadOnlyDictionary<long, DpsData> ReadOnlySectionedDpsDatas { get; }

    IReadOnlyList<DpsData> ReadOnlySectionedDpsDataList { get; }

    TimeSpan SectionTimeout { get; set; }

    bool IsServerConnected { get; set; }
    long CurrentPlayerUUID { get; set; }

    /// <summary>
    /// Current active boss/enemy being fought (0 = no active boss)
    /// </summary>
    long ActiveBossUuid { get; set; }

    /// <summary>
    /// Time when the active boss died (null = boss not dead yet)
    /// </summary>
    DateTime? BossDeathTime { get; set; }

    /// <summary>
    /// Registers a new boss fight when first damage is dealt to an enemy
    /// </summary>
    void RegisterBossEngagement(long enemyUuid);

    /// <summary>
    /// Registers boss death and starts the post-death timer
    /// </summary>
    void RegisterBossDeath(long enemyUuid);

    /// <summary>
    /// Checks if we should end the battle section (boss dead + delay passed)
    /// </summary>
    bool ShouldEndBattleSection();

    event ServerConnectionStateChangedEventHandler? ServerConnectionStateChanged;
    event PlayerInfoUpdatedEventHandler? PlayerInfoUpdated;
    event NewSectionCreatedEventHandler? NewSectionCreated;
    event BattleLogCreatedEventHandler? BattleLogCreated;
    event DpsDataUpdatedEventHandler? DpsDataUpdated;
    event DataUpdatedEventHandler? DataUpdated;
    event ServerChangedEventHandler? ServerChanged;

    void LoadPlayerInfoFromFile();
    void SavePlayerInfoToFile();
    Dictionary<long, PlayerInfoFileData> BuildPlayerDicFromBattleLog(List<BattleLog> battleLogs);
    void ClearAllDpsData();
    void ClearDpsData();
    void ClearCurrentPlayerInfo();
    void ClearPlayerInfos();
    void ClearAllPlayerInfos();
    void RaiseServerChanged(string currentServerStr, string prevServer);
    void SetPlayerLevel(long playerUid, int tmpLevel);
    bool EnsurePlayer(long playerUid);
    void SetPlayerHP(long playerUid, long hp);
    void SetPlayerMaxHP(long playerUid, long maxHp);
    void SetPlayerName(long playerUid, string playerName);
    void SetPlayerCombatPower(long playerUid, int combatPower);
    void SetPlayerProfessionID(long playerUid, int professionId);

    /// <summary>
    /// 添加战斗日志 (会自动创建日志分段)
    /// Public method for backwards compatibility - fires events immediately
    /// </summary>
    /// <param name="log">战斗日志</param>
    void AddBattleLog(BattleLog log);

    void SetPlayerRankLevel(long playerUid, int readInt32);
    void SetPlayerCritical(long playerUid, int readInt32);
    void SetPlayerLucky(long playerUid, int readInt32);
}

public delegate void ServerConnectionStateChangedEventHandler(bool serverConnectionState);
public delegate void PlayerInfoUpdatedEventHandler(PlayerInfo info);
public delegate void NewSectionCreatedEventHandler();
public delegate void BattleLogCreatedEventHandler(BattleLog battleLog);
public delegate void DpsDataUpdatedEventHandler();
public delegate void DataUpdatedEventHandler();
public delegate void ServerChangedEventHandler(string currentServer, string prevServer);
