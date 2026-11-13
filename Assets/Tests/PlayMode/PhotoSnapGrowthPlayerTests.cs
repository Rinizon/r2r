using System.Collections;
using InfrastructureTycoon.Simulation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class PhotoSnapGrowthPlayerTests
{
    [UnityTest]
    public IEnumerator SceneGrowthAndPostGrowthSaveRoundTripDoNotReplayTheMilestone()
    {
        yield return SceneManager.LoadSceneAsync("Main");
        var root = GameObject.Find("GameRoot").GetComponent<InfrastructureTycoon.GameRoot>();
        var source = root.Session;
        source.Clock.Speed = SimulationSpeed.Paused;
        foreach (var offer in source.Catalog.Offers)
        {
            source.TryAcceptOffer(offer.Id, out var contract, out _);
            source.TryAssignContract(contract.Id, source.Hardware[0].Id, out _);
        }
        int applied = 0;
        source.MilestoneApplied += (_, __) => applied++;
        source.Clock.Speed = SimulationSpeed.Normal;
        source.Clock.Advance(118);
        source.Clock.Speed = SimulationSpeed.Paused;
        yield return null;
        Assert.That(applied, Is.EqualTo(1));
        Assert.That(source.AcceptedContracts[0].EffectiveRequirements.CpuCores, Is.EqualTo(2));
        Assert.That(source.Hardware[0].Resources.Used.CpuCores, Is.EqualTo(3));
        Assert.That(GameObject.Find("CashLabel").GetComponent<Text>().text, Is.EqualTo("CASH  $470"));
        string json = SessionSaveJson.ToJson(SessionSaveJson.Capture(source, Application.version));
        Assert.That(SessionSaveJson.TryLoad(json, source.Catalog, out var loaded, out var reason), Is.True, reason);
        int replayed = 0;
        loaded.MilestoneApplied += (_, __) => replayed++;
        loaded.Clock.Speed = SimulationSpeed.Normal;
        loaded.Clock.Advance(62);
        Assert.That(replayed, Is.Zero);
        Assert.That(loaded.AcceptedContracts[0].AppliedMilestoneIds.Count, Is.EqualTo(1));
        Assert.That(loaded.AcceptedContracts[0].PaidMonths, Is.EqualTo(3));
        Assert.That(loaded.Cash, Is.EqualTo(630m));
        Assert.That(source.Cash, Is.EqualTo(470m));
        LogAssert.NoUnexpectedReceived();
    }
}
