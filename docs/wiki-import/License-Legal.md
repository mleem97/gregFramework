---
title: License & Legal EN
description: License model, disclaimers, and interoperability legal context.
sidebar_position: 200
tags:
  - audience:enduser
  - audience:moddev
  - audience:contributor
  - audience:sponsor
  - audience:gamedev
---

## License & Legal

- License terms: [`LICENSE.txt`](https://github.com/mleem97/gregFramework/blob/master/LICENSE.txt)
- FrikaMF is unofficial and community-driven.
- No affiliation with WASEKU.

## Disclaimer

Use at your own risk. Compatibility is not guaranteed across future game updates.

## Interoperability basis

- EU Software Directive Article 6
- German UrhG § 69e

## Compliance guidance

- No anti-cheat bypass intent.
- No unauthorized redistribution of protected content.
- No exploit-focused competitive abuse.

## Shutdown examples

### 🦀 Rust

```rust
#[no_mangle]
pub extern "C" fn mod_shutdown() {}
```

### 🔷 C\#

```csharp
public override void OnApplicationQuit()
{
    LoggerInstance.Msg("Framework shutdown");
}
```

