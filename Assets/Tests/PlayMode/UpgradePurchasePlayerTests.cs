using System.Collections;
using InfrastructureTycoon.Simulation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class UpgradePurchasePlayerTests
{
    [UnityTest]
    public IEnumerator PurchaseUpdatesCashAndPersistsItsNonemptyLedgerInThePlayer()
    {
        yield return SceneManager.LoadSceneAsync("Main");
        var root = GameObject.Find("GameRoot").GetComponent<InfrastructureTycoon.GameRoot>();
        root.Clock.Speed = SimulationSpeed.Paused;
        var pc = root.Session.Hardware[0];
        Assert.That(root.Session.TryPurchaseUpgrade("upgrade.starter-cpu", pc.Id, out var reason), Is.True, reason);
        yield return null;
        Assert.That(GameObject.Find("CashLabel").GetComponent<Text>().text, Is.EqualTo("CASH  $30"));
        string json = SessionSaveJson.ToJson(SessionSaveJson.Capture(root.Session, Application.version));
        Assert.That(SessionSaveJson.TryLoad(json, root.Session.Catalog, out var loaded, out reason), Is.True, reason);
        Assert.That(loaded.Hardware[0].Id, Is.EqualTo(pc.Id));
        Assert.That(loaded.Hardware[0].Resources.Capacity.CpuCores, Is.EqualTo(4));
        Assert.That(loaded.Hardware[0].PurchaseRecords.Count, Is.EqualTo(1));
        Assert.That(loaded.Hardware[0].PurchaseRecords[0].AmountPaid, Is.EqualTo(120m));
        Assert.That(loaded.Cash, Is.EqualTo(30m));
        Assert.That(loaded.OperatingProfit, Is.Zero);
        Assert.That(loaded.TryPurchaseUpgrade("upgrade.starter-cpu", pc.Id, out reason), Is.False);
        Assert.That(loaded.TryPurchaseUpgrade("upgrade.starter-ram", pc.Id, out reason), Is.False);
        Assert.That(loaded.Cash, Is.EqualTo(30m));
        Assert.That(root.Session.Cash, Is.EqualTo(30m));
        LogAssert.NoUnexpectedReceived();
    }
}
