using MelonLoader;
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using FrikaMF;

namespace AssetExporter
{
    internal sealed class FrameworkDependencyTestMod
    {
        private readonly RuntimeHookService runtimeHookService;

        public FrameworkDependencyTestMod(RuntimeHookService runtimeHookService)
        {
            this.runtimeHookService = runtimeHookService ?? throw new ArgumentNullException(nameof(runtimeHookService));
        }

        public void Initialize()
        {
            ModFramework.Events.Subscribe<HookScanCompletedEvent>(OnHookScanCompleted);
            ModFramework.Events.Subscribe<HookInstallCompletedEvent>(OnHookInstallCompleted);
            ModFramework.Events.Subscribe<HookTriggeredEvent>(OnHookTriggered);

            FrameworkLog.Info("testmod", "FrameworkDependencyTestMod initialized (F6=Hook healthcheck, F7=+1 money mutation)");
        }

        public void Tick()
        {
            if (Keyboard.current == null)
                return;

            if (Keyboard.current.f6Key.wasPressedThisFrame)
                RunHealthcheck();

            if (Keyboard.current.f7Key.wasPressedThisFrame)
                ApplyTestMutation();
        }

        public void RunHealthcheck()
        {
            const int maxHooks = 200;

            HookScanResult scan = runtimeHookService.ScanCandidates(maxHooks);
            HookInstallResult install = runtimeHookService.ScanAndInstall(maxHooks);

            FrameworkLog.Info("healthcheck", $"Candidates={scan.Candidates.Count}, installed={install.Installed}, failed={install.Failed}");

            int previewCount = Math.Min(scan.Candidates.Count, 10);
            for (int index = 0; index < previewCount; index++)
                FrameworkLog.Debug("healthcheck", $"Hookable[{index + 1}/{previewCount}]: {scan.Candidates[index]}");

            if (install.Errors.Count > 0)
            {
                int errorPreviewCount = Math.Min(install.Errors.Count, 10);
                for (int index = 0; index < errorPreviewCount; index++)
                        FrameworkLog.Warn("healthcheck", $"Not hookable[{index + 1}/{errorPreviewCount}]: {install.Errors[index]}");
            }

        }

        private static void ApplyTestMutation()
        {
            float previousMoney = FrikaMF.GameHooks.GetPlayerMoney();
            float updatedMoney = previousMoney + 1f;

            FrikaMF.GameHooks.SetPlayerMoney(updatedMoney);
            FrameworkLog.Info("testmod", $"Money mutation applied: {previousMoney} -> {updatedMoney}");
        }

        private static void OnHookTriggered(HookTriggeredEvent hookTriggeredEvent)
        {
            if (hookTriggeredEvent == null)
                return;

            if (hookTriggeredEvent.TriggerCount <= 2)
                FrameworkLog.Debug("healthcheck", $"Hook trigger observed: {hookTriggeredEvent.MethodName} (count={hookTriggeredEvent.TriggerCount})");
        }

        private static void OnHookScanCompleted(HookScanCompletedEvent hookScanCompletedEvent)
        {
            if (hookScanCompletedEvent == null)
                return;

            FrameworkLog.Debug("healthcheck", $"HookScanCompletedEvent: {hookScanCompletedEvent.CandidatesFound} candidates");
        }

        private static void OnHookInstallCompleted(HookInstallCompletedEvent hookInstallCompletedEvent)
        {
            if (hookInstallCompletedEvent == null)
                return;

            FrameworkLog.Debug("healthcheck", $"HookInstallCompletedEvent: installed={hookInstallCompletedEvent.Installed}, failed={hookInstallCompletedEvent.Failed}");
        }
    }
}
