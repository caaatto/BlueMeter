using BlueProto;
using Microsoft.Extensions.Logging;
using BlueMeter.Core.Analyze.Models;
using BlueMeter.Core.Data.Models;
using BlueMeter.Core.Extends.BlueProto;
using BlueMeter.Core.Extends.System;
using BlueMeter.Core.Logging;
using BlueMeter.Core.Tools;
using BlueMeter.WPF.Data;
using Google.Protobuf;

namespace BlueMeter.Core.Analyze.V2.Processors;

/// <summary>
/// Processes delta info messages for damage and healing events.
/// </summary>
public abstract class BaseDeltaInfoProcessor(IDataStorage storage, ILogger? logger) : IMessageProcessor
{
    protected readonly IDataStorage _storage = storage;
    protected readonly ILogger? _logger = logger;

    public abstract void Process(byte[] payload);

    protected void ProcessAoiSyncDelta(AoiSyncDelta? delta)
    {
        if (delta == null) return;

        var targetUuidRaw = delta.Uuid;
        if (targetUuidRaw == 0) return;

        var isTargetPlayer = targetUuidRaw.IsUuidPlayerRaw();
        var targetUuid = targetUuidRaw.ShiftRight16();

        var attrCollection = delta.Attrs;
        if (attrCollection?.Attrs != null && isTargetPlayer)
        {
            _storage.EnsurePlayer(targetUuid);

            foreach (var attr in attrCollection.Attrs)
            {
                if (attr.Id == 0 || attr.RawData == null || attr.RawData.Length == 0) continue;
                var reader = new CodedInputStream(attr.RawData.ToByteArray());

                var attrType = (AttrType)attr.Id;
                switch (attrType)
                {
                    case AttrType.AttrName:
                        _storage.SetPlayerName(targetUuid, reader.ReadString());
                        break;
                    case AttrType.AttrProfessionId:
                        _storage.SetPlayerProfessionID(targetUuid, reader.ReadInt32());
                        break;
                    case AttrType.AttrFightPoint:
                        _storage.SetPlayerCombatPower(targetUuid, reader.ReadInt32());
                        break;
                    case AttrType.AttrLevel:
                        _storage.SetPlayerLevel(targetUuid, reader.ReadInt32());
                        break;
                    case AttrType.AttrRankLevel:
                        _storage.SetPlayerRankLevel(targetUuid, reader.ReadInt32());
                        break;
                    case AttrType.AttrCri:
                        _storage.SetPlayerCritical(targetUuid, reader.ReadInt32());
                        break;
                    case AttrType.AttrLucky:
                        _storage.SetPlayerLucky(targetUuid, reader.ReadInt32());
                        break;
                    case AttrType.AttrHp:
                        _storage.SetPlayerHP(targetUuid, reader.ReadInt32());
                        break;
                    case AttrType.AttrMaxHp:
                    case AttrType.AttrId:
                    case AttrType.AttrElementFlag:
                    case AttrType.AttrReductionLevel:
                    case AttrType.AttrReduntionId:
                    case AttrType.AttrEnergyFlag:
                        _ = reader.ReadInt32();
                        break;
                    default:
                        break;
                }
            }
        }

        var skillEffect = delta.SkillEffects;
        if (skillEffect?.Damages == null || skillEffect.Damages.Count == 0) return;

        var count = 0;
        var heals = 0;
        var crits = 0;

        foreach (var d in skillEffect.Damages)
        {
            var skillId = d.OwnerId;
            if (skillId == 0) continue;

            var attackerRaw = d.TopSummonerId != 0 ? d.TopSummonerId : d.AttackerUuid;
            if (attackerRaw == 0) continue;

            var isAttackerPlayer = attackerRaw.IsUuidPlayerRaw();
            var attackerUuid = attackerRaw.ShiftRight16();

            var damageSigned = d.HasValue ? d.Value : d.HasLuckyValue ? d.LuckyValue : 0L;
            if (damageSigned == 0) continue;

            var isDead = d.HasIsDead && d.IsDead;

            // Boss tracking: Register engagement when player attacks enemy
            if (isAttackerPlayer && !isTargetPlayer && d.Type != EDamageType.Heal)
            {
                _storage.RegisterBossEngagement(targetUuid);
            }

            // Boss tracking: Register death when enemy dies
            if (!isTargetPlayer && isDead)
            {
                _storage.RegisterBossDeath(targetUuid);
            }

            var (id, ticks) = IDGenerator.Next();
            _storage.AddBattleLog(new BattleLog
            {
                PacketID = id,
                TimeTicks = ticks,
                SkillID = skillId,
                AttackerUuid = attackerUuid,
                TargetUuid = targetUuid,
                Value = damageSigned,
                ValueElementType = (int)d.Property,
                DamageSourceType = (int)(d.HasDamageSource ? d.DamageSource : 0),
                IsAttackerPlayer = isAttackerPlayer,
                IsTargetPlayer = isTargetPlayer,
                IsLucky = d.LuckyValue != 0,
                IsCritical = (d.TypeFlag & 1) == 1,
                IsHeal = d.Type == EDamageType.Heal,
                IsMiss = d.HasIsMiss && d.IsMiss,
                IsDead = isDead
            });
            count++;
            if ((d.TypeFlag & 1) == 1) crits++;
            if (d.Type == EDamageType.Heal) heals++;
        }

        if (count > 0)
        {
            _logger?.LogTrace(CoreLogEvents.DeltaProcessed,
                "Delta processed: {Count} events (crit={Crit}, heal={Heal}) TargetPlayer={IsTargetPlayer}",
                count, crits, heals, isTargetPlayer);
        }
    }
}

public sealed class SyncToMeDeltaInfoProcessor(IDataStorage storage, ILogger? logger)
    : BaseDeltaInfoProcessor(storage, logger)
{
    public override void Process(byte[] payload)
    {
        _logger?.LogDebug(CoreLogEvents.SyncToMeDelta, nameof(SyncToMeDeltaInfoProcessor));
        var syncToMeDeltaInfo = SyncToMeDeltaInfo.Parser.ParseFrom(payload);
        var aoiSyncToMeDelta = syncToMeDeltaInfo.DeltaInfo;
        var uuid = aoiSyncToMeDelta.Uuid;
        if (uuid != 0 && _storage.CurrentPlayerUUID != uuid)
        {
            _storage.CurrentPlayerUUID = uuid;
        }

        var aoiSyncDelta = aoiSyncToMeDelta.BaseDelta;
        if (aoiSyncDelta == null) return;

        ProcessAoiSyncDelta(aoiSyncDelta);
    }
}

public sealed class SyncNearDeltaInfoProcessor(IDataStorage storage, ILogger? logger)
    : BaseDeltaInfoProcessor(storage, logger)
{
    public override void Process(byte[] payload)
    {
        _logger?.LogDebug(CoreLogEvents.SyncNearDelta, nameof(SyncNearDeltaInfoProcessor));
        var syncNearDeltaInfo = SyncNearDeltaInfo.Parser.ParseFrom(payload);
        if (syncNearDeltaInfo.DeltaInfos == null || syncNearDeltaInfo.DeltaInfos.Count == 0) return;

        foreach (var aoiSyncDelta in syncNearDeltaInfo.DeltaInfos)
        {
            ProcessAoiSyncDelta(aoiSyncDelta);
        }
    }
}
