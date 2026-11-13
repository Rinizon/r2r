using System;
using System.Collections;
using System.IO;
using InfrastructureTycoon.Simulation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class Phase3SavePlayerTests
{
    private string directory;

    [UnityTearDown]
    public IEnumerator CleanUp()
    {
        if (directory != null && Directory.Exists(directory))
        {
            foreach (var file in Directory.GetFiles(directory)) File.Delete(file);
            Directory.Delete(directory);
        }
        yield return null;
    }

    [UnityTest]
    public IEnumerator LegacySlotLoadsWithoutRewriteThenExplicitSaveWritesVersionTwo()
    {
        yield return SceneManager.LoadSceneAsync("Main");
        var root = GameObject.Find("GameRoot").GetComponent<InfrastructureTycoon.GameRoot>();
        root.Clock.Speed = SimulationSpeed.Paused;
        directory = Path.Combine(Application.temporaryCachePath, "r2r-phase3-schema-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, "company-v1.json");
        root.ConfigureSaveSlot(new SessionSaveSlot(path));
        foreach (var offer in root.Session.Catalog.Offers)
        {
            root.Session.TryAcceptOffer(offer.Id, out var contract, out _);
            root.Session.TryAssignContract(contract.Id, root.Session.Hardware[0].Id, out _);
        }
        root.Clock.Speed = SimulationSpeed.Normal;
        root.Clock.Advance(118);
        root.Clock.Speed = SimulationSpeed.Paused;
        var legacy = SessionSaveJson.Capture(root.Session, "phase2-player-fixture");
        legacy.SaveVersion = 1;
        foreach (var hardware in legacy.Hardware) hardware.Purchases = null;
        foreach (var contract in legacy.Contracts) contract.AppliedMilestoneIds = null;
        string original = SessionSaveJson.ToJson(legacy);
        File.WriteAllText(path, original);
        Click("LoadGame");
        yield return null;
        Assert.That(File.ReadAllText(path), Is.EqualTo(original), "Migration must not rewrite the slot during Load.");
        Assert.That(root.Session.Cash, Is.EqualTo(470m));
        Assert.That(root.Session.Hardware[0].Resources.Used.CpuCores, Is.EqualTo(3));
        Assert.That(root.Session.AcceptedContracts[0].AppliedMilestoneIds.Count, Is.EqualTo(1));
        Click("SaveGame");
        yield return null;
        Assert.That(File.ReadAllText(path), Does.Contain("\"save_version\":2"));
        Click("LoadGame");
        yield return null;
        Assert.That(root.Session.Cash, Is.EqualTo(470m));
        Assert.That(root.Session.Hardware[0].Resources.Used.CpuCores, Is.EqualTo(3));
        root.Clock.Speed = SimulationSpeed.Normal;
        root.Clock.Advance(62);
        root.Clock.Speed = SimulationSpeed.Paused;
        yield return null;
        Assert.That(root.Session.Cash, Is.EqualTo(630m));
        Assert.That(root.Session.BillingHistory.Count, Is.EqualTo(3));
        Assert.That(root.Session.AcceptedContracts[0].PaidMonths, Is.EqualTo(3));
        LogAssert.NoUnexpectedReceived();
    }

    private static void Click(string name)
    {
        var button = GameObject.Find(name).GetComponent<Button>();
        Assert.That(button.interactable, Is.True);
        button.onClick.Invoke();
    }
}
