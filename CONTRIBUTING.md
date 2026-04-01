# 🤝 Contributing

Thank you for your interest in contributing to `DataCenterExporter`.

---

## 📌 Core Principles

- Contributions must serve a legitimate modding purpose.
- Do not add features or changes that support copyright infringement, asset theft, or unauthorized redistribution.
- Keep changes small, focused, and easy to review.

---

## 🛠 Local Development Workflow

1. Fork the repository and create a branch (`feature/...`, `fix/...`).
2. Implement your changes.
3. Build locally:

```sh
dotnet build DataCenterExporter.sln -v:minimal
```

4. Validate functionality (hotkeys `F8`, `F9`, `F10`).
5. Open a pull request with a clear description.

---

## 🧩 Coding Guidelines

- Keep the existing style in `Main.cs`.
- Do not add unnecessary dependencies.
- Preserve IL2CPP/MelonLoader compatibility.

---

## ✅ Pull Request Checklist

- [ ] Build is successful.
- [ ] Change is limited to the intended scope.
- [ ] README/docs updated where necessary.
- [ ] No ethically or legally problematic content.
