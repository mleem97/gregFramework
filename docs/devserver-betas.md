# DevServer API — beta channels (`gregframework.eu`)

This document defines the **intended** client contract for the FrikaMF **WorkshopUploader** “Betas” panel. The server may be implemented separately; keep URLs and tokens configurable.

## Base URL

- Default: `https://gregframework.eu`
- Override: user settings file next to the app (not committed to git).

## Endpoints (proposed)

| Method | Path | Purpose |
|--------|------|---------|
| `GET` | `/api/v1/betas` | List available beta channels (id, name, description). |
| `POST` | `/api/v1/betas/{id}/subscribe` | Enroll the current user (body: Steam ID or bearer token from OAuth). |
| `POST` | `/api/v1/betas/{id}/unsubscribe` | Leave a channel. |

## Authentication

- **Preferred:** Short-lived JWT after browser OAuth to `gregframework.eu`, stored in user settings.
- **Alternative:** Steam ID from Steamworks session in the WorkshopUploader process, plus server-side verification.

## Rate limiting and errors

Clients should show HTTP status and response body on failure; retry with backoff.
