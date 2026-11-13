using System.Collections;
using InfrastructureTycoon.Simulation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class SessionSavePlayerTests
{
    [UnityTest]
    public IEnumerator SceneSessionJsonRoundTripWorksInThePlayerWithoutReplacingLiveState()
    {
        yield return SceneManager.LoadSceneAsync("Main");
        var source = GameObject.Find("GameRoot").GetComponent<InfrastructureTycoon.GameRoot>().Session;
        source.Clock.Speed = SimulationSpeed.Paused;
        Assert.That(source.TryAcceptOffer("offer.photosnap", out var contract, out var reason), Is.True, reason);
        Assert.That(source.TryAssignContract(contract.Id, source.Hardware[0].Id, out reason), Is.True, reason);
        string json = SessionSaveJson.ToJson(SessionSaveJson.Capture(source, Application.version));
        Assert.That(SessionSaveJson.TryLoad(json, source.Catalog, out var loaded, out reason), Is.True, reason);
        Assert.That(loaded.Clock.DayProgress, Is.EqualTo(source.Clock.DayProgress));
        Assert.That(loaded.Clock.Speed, Is.EqualTo(SimulationSpeed.Paused));
        Assert.That(loaded.AcceptedContracts[0].Id, Is.EqualTo(contract.Id));
        loaded.Clock.Speed = SimulationSpeed.Normal;
        loaded.Clock.Advance(62);
        Assert.That(loaded.Cash, Is.EqualTo(230m));
        Assert.That(loaded.AcceptedContracts[0].PaidMonths, Is.EqualTo(1));
        Assert.That(source.Cash, Is.EqualTo(150m));
        LogAssert.NoUnexpectedReceived();
    }
}
