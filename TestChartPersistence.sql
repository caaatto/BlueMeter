-- Run this SQL query against your BlueMeter database to check if chart data is being saved

-- 1. Check recent encounters
SELECT
    Id,
    EncounterId,
    BossName,
    StartTime,
    EndTime,
    PlayerCount,
    TotalDamage
FROM Encounters
ORDER BY StartTime DESC
LIMIT 5;

-- 2. Check if chart data exists for recent players
SELECT
    Id,
    EncounterId,
    NameSnapshot,
    TotalAttackDamage,
    DPS,
    -- Check if chart history JSON fields have data
    CASE
        WHEN DpsHistoryJson IS NULL THEN 'NULL'
        WHEN DpsHistoryJson = '' THEN 'EMPTY'
        WHEN LENGTH(DpsHistoryJson) > 0 THEN 'HAS DATA (' || LENGTH(DpsHistoryJson) || ' chars)'
    END as DpsHistoryStatus,
    CASE
        WHEN HpsHistoryJson IS NULL THEN 'NULL'
        WHEN HpsHistoryJson = '' THEN 'EMPTY'
        WHEN LENGTH(HpsHistoryJson) > 0 THEN 'HAS DATA (' || LENGTH(HpsHistoryJson) || ' chars)'
    END as HpsHistoryStatus
FROM PlayerEncounterStats
WHERE EncounterId IN (SELECT Id FROM Encounters ORDER BY StartTime DESC LIMIT 3)
ORDER BY TotalAttackDamage DESC;

-- 3. Sample one chart data JSON (first 500 chars)
SELECT
    NameSnapshot,
    SUBSTR(DpsHistoryJson, 1, 500) as DpsHistorySample
FROM PlayerEncounterStats
WHERE DpsHistoryJson IS NOT NULL
  AND LENGTH(DpsHistoryJson) > 0
LIMIT 1;
