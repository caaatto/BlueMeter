# Comprehensive Queue Pop Logging - Complete Capture System

## ✅ Enhanced Logging Now Active

All packet logging is now **massively enhanced** to capture EVERYTHING needed to find the exact queue pop packet.

---

## 📋 Log Files Generated

When `DataStorageV2.EnableQueueDetectionLogging = true` (default), the following logs are created in `BlueMeter.WPF/bin/Release/net8.0-windows/logs/`:

### 1. **queue-detection-summary.log** ⭐ NEW - START HERE!
**Purpose**: Real-time queue pop detection summary

**Content**:
```
[21:59:13.456] RETURN BURST: 3 messages in last 2s
[21:59:13.457] RETURN BURST: 4 messages in last 2s
[21:59:13.458] RETURN BURST: 5 messages in last 2s
[21:59:13.459] ★★★ QUEUE POP DETECTED! ★★★ 5 Return messages triggered alert
```

**Use**: Quick verification that queue pop was detected and WHEN it happened

---

### 2. **call-messages.log** ⭐ NEW - QUEUE REQUESTS!
**Purpose**: All client→server RPC calls (including queue/matchmaking requests)

**Content**:
```
[21:59:10.123] Call - ServiceUuid: 0x????????????????, StubId: 0x12345678, MethodId: 0x000000AB (171), PayloadSize: 42 bytes
  Payload: 0A 1B 2C 3D ...
[21:59:11.234] [SPAN] Call - ServiceUuid: 0x????????????????, StubId: 0x12345679, MethodId: 0x000000AC (172), PayloadSize: 24 bytes
  Payload: 1A 2B 3C 4D ...
```

**Use**:
- Find the Call message that initiated the queue request
- Identify matchmaking service UUID
- Match StubId from Call to corresponding Return message
- **CRITICAL**: The Call that triggers queue should appear ~few seconds BEFORE the Return burst

---

### 3. **return-messages.log** (Enhanced)
**Purpose**: All server→client RPC responses

**Content**:
```
[21:59:13.456] Return StubId: 0x12345678, PayloadSize: 165 bytes
  Payload: 00 00 00 86 00 00 00 00 0A 9A 01 08 84 80 01 ...
[21:59:13.457] Return StubId: 0x12345679, PayloadSize: 42 bytes
  Payload: 00 00 00 91 00 00 00 00 0A 08 08 01 10 27 ...
```

**Use**:
- See the Return message burst that triggers queue pop
- Match StubId back to the Call message
- Analyze payload to identify unique queue pop response pattern

---

### 4. **all-messages.log** (Existing)
**Purpose**: All message types at the top level

**Content**:
```
[21:59:13.456] [SPAN] MessageType: Return (0x3), Compressed: False
[21:59:13.457] [SPAN] MessageType: Return (0x3), Compressed: False
[21:59:13.458] MessageType: Notify (0x2), Compressed: False
```

**Use**: See message flow and timing, verify message types

---

### 5. **all-notify-messages.log** (Existing)
**Purpose**: All Notify messages (both combat and non-combat services)

**Content**:
```
[21:59:13.456] ServiceUuid: 0x0000000063335342, MethodId: 0x0000002E (46), Combat: true
[21:59:14.123] ServiceUuid: 0x????????????????, MethodId: 0x???????? (???), Combat: false
```

**Use**: See all game state notifications

---

### 6. **non-combat-services.log** (Existing)
**Purpose**: Only non-combat service Notify messages (e.g., matchmaking, social, UI)

**Content**:
```
[21:59:14.123] NON-COMBAT SERVICE - ServiceUuid: 0x????????????????, MethodId: 0x???????? (???)
```

**Use**:
- Identify matchmaking service notifications
- May contain "match found" notification
- **NOTE**: May be empty if matchmaking uses Call/Return instead of Notify

---

### 7. **charteam-messages.log** (Existing)
**Purpose**: CharTeam messages (TeamMatching method, 0x2B)

**Content**:
```
[21:59:13.456] CharTeam - TeamId: 1764107160800, LeaderId: 1764107161099, TeamNum: NULL, IsMatching: NULL, MemberCount: 0, CharIds: []
```

**Use**:
- See team/party updates
- **NOTE**: IsMatching field is always NULL, not useful for detection
- Check if MemberCount or CharIds change during queue pop

---

### 8. **unknown-messages.log** ⭐ NEW
**Purpose**: Any message types we don't handle

**Content**:
```
[21:59:13.456] UNKNOWN MessageType: 7 (0x7), Compressed: False
[21:59:13.457] [SPAN] UNKNOWN MessageType: 8 (0x8), Compressed: True
```

**Use**: Discover new message types that might be queue-related

---

## 🎯 How to Find the Queue Pop Packet

### Step 1: Queue and Note the Exact Time

1. Clear old logs (optional):
   ```bash
   rm BlueMeter.WPF/bin/Release/net8.0-windows/logs/*.log
   ```

2. Launch BlueMeter and start packet capture

3. Queue for a dungeon/raid

4. **Write down the EXACT timestamp when queue pops** (e.g., 21:59:13)

5. Accept the queue

6. Stop capture after loading completes

---

### Step 2: Check Queue Detection Summary

**File**: `queue-detection-summary.log`

Look for the `★★★ QUEUE POP DETECTED! ★★★` line:

```
[21:59:13.459] ★★★ QUEUE POP DETECTED! ★★★ 5 Return messages triggered alert
```

This confirms:
- ✅ Detection is working
- ✅ Exact timestamp of detection
- ✅ Number of Return messages that triggered it

---

### Step 3: Analyze Call Messages (Queue Request)

**File**: `call-messages.log`

Look for Call messages **a few seconds BEFORE** the queue pop:

```
[21:59:10.123] Call - ServiceUuid: 0x????????????????, StubId: 0xABCD1234, MethodId: 0x000000AB
  Payload: ...
```

**What to look for**:
- Unique ServiceUuid (not the combat service 0x0000000063335342)
- Unique MethodId that appears only when queueing
- Note the StubId - this will match a Return message later

**Hypothesis**: This Call message is the "accept queue" or "check queue status" request

---

### Step 4: Analyze Return Messages (Queue Response)

**File**: `return-messages.log`

Look at the Return message burst around queue pop time:

```
[21:59:13.456] Return StubId: 0xABCD1234, PayloadSize: 165 bytes
  Payload: 00 00 00 86 00 00 00 00 0A 9A 01 08 84 80 01 ...
[21:59:13.457] Return StubId: 0x12345678, PayloadSize: 42 bytes
  Payload: 00 00 00 91 00 00 00 00 0A 08 08 01 10 27 ...
```

**What to look for**:
1. **Match StubId**: Find Return message with same StubId as the Call from Step 3
   - This Return is the response to the queue request!

2. **Unique Payload Pattern**: Look for unique byte sequences
   - First 4 bytes might be message type/ID
   - Could contain team member UUIDs
   - Could contain instance ID or dungeon ID

3. **Consistent Pattern**: Queue multiple times and verify the same pattern appears

**Hypothesis**: ONE of these Return messages specifically means "match found"

---

### Step 5: Check Non-Combat Services (Alternative)

**File**: `non-combat-services.log`

If this file exists, check for messages around queue pop time:

```
[21:59:14.123] NON-COMBAT SERVICE - ServiceUuid: 0x????????????????, MethodId: 0x????????
```

**What to look for**:
- ServiceUuid that appears only during queue pop
- MethodId that might mean "match found"

**Note**: This file may be empty if matchmaking uses Call/Return pattern instead of Notify

---

### Step 6: Cross-Reference with CharTeam

**File**: `charteam-messages.log`

Check if CharTeam messages change:

```
Before queue pop:
[21:59:12.456] CharTeam - TeamId: X, LeaderId: Y, MemberCount: 0, CharIds: []

After queue pop:
[21:59:14.456] CharTeam - TeamId: Z, LeaderId: Y, MemberCount: 5, CharIds: [123, 456, 789, ...]
```

**What to look for**:
- MemberCount increases (0 → 5 for 5-player dungeon)
- CharIds populated with team member UUIDs
- TeamId changes

**Note**: This happens AFTER queue pop, not during, so it's a confirmation signal, not the trigger

---

## 🔍 Analysis Workflow

### Workflow Diagram

```
User queues for dungeon
         ↓
[21:59:10] Call message sent (queue request)
    ServiceUuid: 0x????????????????
    StubId: 0xABCD1234
    MethodId: 0x000000AB
         ↓
Server matchmaking in progress...
         ↓
[21:59:13] MATCH FOUND!
         ↓
[21:59:13.456] Return message burst begins
    Return StubId: 0xABCD1234 ← Response to queue request
    Return StubId: 0x12345678
    Return StubId: 0x12345679
    ...
         ↓
[21:59:13.459] ★★★ QUEUE POP DETECTED! ★★★
         ↓
[21:59:14] CharTeam updated (MemberCount: 5)
         ↓
User accepts and loads into instance
```

---

## 🎯 Expected Findings

### Scenario A: Specific Return Message

**Most Likely**: ONE specific Return message indicates queue pop

```
Return StubId: 0xABCD1234, PayloadSize: 165 bytes
  Payload: 00 00 00 86 ...  ← First 4 bytes = 0x00000086 = message type?
```

**Detection Strategy**:
```csharp
if (returnMessage.StubId == QUEUE_ACCEPT_STUB_ID
    && payload.Length >= 4
    && BitConverter.ToUInt32(payload, 0) == QUEUE_POP_MESSAGE_TYPE)
{
    TriggerQueuePopDetected();
}
```

---

### Scenario B: Specific Call/Return Pair

**Alternative**: Specific Call MethodId + corresponding Return

```
Call:   MethodId: 0x000000AB (queue status check)
Return: StubId matches Call, payload contains match info
```

**Detection Strategy**:
```csharp
// Track Call StubIds
if (callMessage.MethodId == QUEUE_STATUS_METHOD_ID)
{
    _pendingQueueStubIds.Add(callMessage.StubId);
}

// Check Return
if (_pendingQueueStubIds.Contains(returnMessage.StubId))
{
    // Parse payload for match found indicator
    if (PayloadIndicatesMatchFound(returnMessage.Payload))
    {
        TriggerQueuePopDetected();
    }
}
```

---

### Scenario C: Non-Combat Service Notification

**Less Likely**: Separate Notify message for match found

```
NON-COMBAT SERVICE - ServiceUuid: 0x????????????????, MethodId: 0x000000CD
```

**Detection Strategy**:
```csharp
if (serviceUuid == MATCHMAKING_SERVICE_UUID
    && methodId == MATCH_FOUND_METHOD_ID)
{
    TriggerQueuePopDetected();
}
```

---

## 📊 Log File Quick Reference

| File | Purpose | Key Info | When to Check |
|------|---------|----------|---------------|
| `queue-detection-summary.log` | Queue pop alerts | Exact detection time | First - verify detection |
| `call-messages.log` | Client requests | Queue request Call | Find queue request before pop |
| `return-messages.log` | Server responses | Match StubId to Call | Find response during burst |
| `non-combat-services.log` | Non-combat notifications | Matchmaking service | Alternative detection method |
| `charteam-messages.log` | Team updates | MemberCount change | Confirmation after pop |
| `unknown-messages.log` | Unknown types | New message types | Discovery of unknowns |

---

## 🚀 Next Steps After Analysis

### 1. Identify the Pattern

After analyzing logs from 2-3 queue pops, identify:
- Consistent ServiceUuid/MethodId in Call messages
- Consistent StubId or payload pattern in Return messages
- Any unique non-combat service messages

### 2. Implement Specific Detection

Based on findings, implement targeted detection:

#### Option A: Specific Return Message
```csharp
// File: MessageAnalyzerV2.cs, ProcessReturnMsg()
if (stubId == QUEUE_POP_STUB_ID && payload.Length >= 4)
{
    var messageType = BitConverter.ToUInt32BE(payload, 0);
    if (messageType == QUEUE_POP_MESSAGE_TYPE)
    {
        dataStorageV2.TriggerQueuePopDetected();
        return; // Skip generic burst detection
    }
}
```

#### Option B: Call/Return Correlation
```csharp
// Track Call messages
private readonly HashSet<uint> _queueCallStubIds = new();

// In ProcessCallMsg()
if (serviceUuid == MATCHMAKING_SERVICE && methodId == QUEUE_CHECK_METHOD)
{
    _queueCallStubIds.Add(stubId);
}

// In ProcessReturnMsg()
if (_queueCallStubIds.Contains(stubId))
{
    _queueCallStubIds.Remove(stubId);
    // Parse payload for match found
    if (IsMatchFoundPayload(payload))
    {
        dataStorageV2.TriggerQueuePopDetected();
    }
}
```

#### Option C: Non-Combat Service
```csharp
// File: MessageAnalyzerV2.cs, ProcessNotifyMsg()
if (serviceUuid == MATCHMAKING_SERVICE_UUID)
{
    if (methodId == MATCH_FOUND_METHOD_ID)
    {
        dataStorageV2.TriggerQueuePopDetected();
    }
    return;
}
```

### 3. Test and Validate

1. Queue multiple times (different dungeon types)
2. Verify 100% detection rate
3. Check for false positives (run around without queueing)
4. Performance test (no lag during combat)

### 4. Replace Generic Burst Detection

Once specific detection is proven reliable:
1. Remove or disable `TrackReturnMessage()` burst detection
2. Keep as fallback safety net (lower priority)
3. Update documentation

---

## 🎯 Success Criteria

After implementation, the system should:

✅ Detect queue pop with specific packet, not pattern
✅ Trigger immediately when match is found
✅ Zero false positives during normal gameplay
✅ Work across all queue types (dungeon, raid, PvP)
✅ No performance impact

---

## 📝 Summary

**Current State**: Generic Return message burst detection (5+ messages in 2s)

**New Logging**:
- ✅ Call messages (queue requests)
- ✅ Enhanced Return messages (responses)
- ✅ Unknown message types
- ✅ Real-time burst detection summary

**Goal**: Find THE specific packet that means "match found"

**Next Action**:
1. Queue for dungeon
2. Note exact queue pop time
3. Analyze logs following this guide
4. Implement specific detection based on findings

---

**Status**: 🎯 Ready to Capture!
**All enhanced logging is now ACTIVE** - just queue and capture!
