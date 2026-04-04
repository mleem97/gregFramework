<!-- markdownlint-disable MD022 MD032 -->

# Steamworks P2P Multiplayer Roadmap

## Goals

- Keep the authoritative server simulation on the host machine at all times.
- Minimize operating cost by preferring Steamworks P2P transport (no dedicated relay/server bills in normal sessions).
- Maintain continuous save-state convergence across all clients.
- Ensure multiplayer can recover from disconnects and out-of-order packets without permanent drift.

## Current Status (Repository Snapshot)

- Host-authoritative remote player correction exists in `FrikaMF/MultiplayerBridge.cs` (`AuthorityMode=server|host`, hard-snap drift correction).
- Transport supports relay and optional P2P exports (`mp_p2p_host`, `mp_p2p_connect`).
- Runtime now defaults to `TransportMode = "p2p"` and prefers P2P in `auto` mode.
- Continuous host-triggered save sync is implemented with periodic `SaveSystem.SaveGame()`.

## Phase 1 — Networking Baseline (Must-Have)
1. **Handshake & Session Contract**
   - Define protocol version, map/save hash, mod manifest hash, and host capability flags.
   - Reject joins on mismatch with explicit user-facing diagnostics.
2. **Reliable Message Channels**
   - Split traffic into channels: `critical` (reliable/ordered), `state` (snapshot/delta), `fx` (unreliable).
   - Add packet sequence IDs and anti-replay windows.
3. **Host Authority Enforcement**
   - Enforce host-only writes for economy, inventory, progression, and save commits.
   - Convert client writes into requests validated by host simulation.

## Phase 2 — Continuous Save-State Synchronization
1. **Deterministic State Snapshot Schema**
   - Introduce canonical serialized world-state schema (money, XP, reputation, devices, links, customer state, technicians).
   - Use stable ordering and deterministic numeric encoding.
2. **Delta + Checkpoint Replication**
   - Broadcast frequent deltas (e.g., 5–10 Hz) and periodic checkpoints (e.g., every 30–60s).
   - Keep rolling checkpoint history on host for late joins and resync.
3. **Drift Detection/Repair**
   - Maintain rolling state hash per client (`xxh3`/`sha256`) over canonical snapshot.
   - On mismatch: request full checkpoint, reapply host baseline, then resume delta stream.

## Phase 3 — Steamworks P2P-First Hardening
1. **NAT Traversal Strategy**
   - Use Steam Datagram Relay fallback only when direct P2P path fails.
   - Keep metrics for route type (`direct` vs `sdr`) and connection quality.
2. **Session Discovery**
   - Support invite-based lobby, room code join, and friend join.
   - Persist session metadata (host build, mod hash, region hints).
3. **Cost Control Policy**
   - Prefer direct P2P; avoid paid external infra.
   - Keep central infra optional and lightweight (only metadata/version checks if needed).

## Phase 4 — Production Multiplayer Features Still Needed
- **Late Join World Bootstrap**: full world snapshot streaming with post-load revalidation.
- **Authoritative Simulation Coverage**: currently player transform sync is present; full gameplay system sync still needs expansion.
- **Conflict Resolution Rules**: deterministic tie-breaking for simultaneous interactions.
- **Rollback/Resimulation**: bounded rollback window for critical race conditions.
- **Bandwidth Budgeting**: adaptive tick rates and compression under poor network.
- **Reconnect Resume**: client checkpoint resume token and host-side state ring buffer.
- **Security**: payload signing for critical commands, anti-cheat sanity checks, rate limiting.

## Phase 5 — QA, Telemetry, and Release
1. **Automated Multiplayer Test Matrix**
   - 2/4/8/16 player scenarios, host migration disabled (host authoritative), packet loss simulation.
2. **Desync Regression Suite**
   - Run deterministic replay with expected state hashes.
3. **Operational Telemetry**
   - Collect drift counters, resync frequency, join failure causes, and transport route distribution.
4. **Release Gates**
   - Block release unless `desync_rate`, `join_success_rate`, and `host_crash_recovery` thresholds are met.

## Immediate Next Implementation Tasks
1. Add canonical world snapshot DTO and host delta broadcaster in Rust multiplayer core.
2. Add client-side checkpoint apply pipeline and hash acknowledgement.
3. Add join-time compatibility checks: framework version, mod manifest hash, save schema version.
4. Add late-join bootstrap path with post-bootstrap hash validation.
5. Expose sync health panel in UI (`drift`, `resync`, `route`, `RTT`, `packet loss`).

## Non-Goals
- Dedicated paid server infrastructure is not required for baseline multiplayer.
- Host migration is intentionally out of scope for initial stable release.
