# Reference Data

This folder contains large reference exports used by hook analysis and build-time tooling.

## Files

- [assembly-hooks.txt.gz](./assembly-hooks.txt.gz)
- [modder-hooks.ffm.txt.gz](./modder-hooks.ffm.txt.gz)

## Decompress

Use standard gzip tools when you need local plaintext files:

```bash
gzip -dk assembly-hooks.txt.gz
gzip -dk modder-hooks.ffm.txt.gz
```

PowerShell alternative:

```powershell
$inputPath = ".\\assembly-hooks.txt.gz"
$outputPath = ".\\assembly-hooks.txt"
$in = [System.IO.File]::OpenRead($inputPath)
$out = [System.IO.File]::Create($outputPath)
$gzip = New-Object System.IO.Compression.GzipStream($in, [System.IO.Compression.CompressionMode]::Decompress)
$gzip.CopyTo($out)
$gzip.Dispose(); $out.Dispose(); $in.Dispose()
```

## Build Pipeline Usage

- Hook alias and reference tooling should read `.gz` directly when possible.
- If plaintext is required, scripts should decompress on demand and avoid committing extracted `.txt` files.
- Keep all reference artifacts in `docs/references/`.
