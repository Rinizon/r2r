using System.Collections;
using System.IO;
using System.Linq;
using InfrastructureTycoon;
using InfrastructureTycoon.Simulation;
using InfrastructureTycoon.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class HardwareStationTests
{
    [UnityTest]
    public IEnumerator HardwareStationGatesInteractionMovementAndReload()
    {
        yield return SceneManager.LoadSceneAsync("Main");
        var stationObject = GameObject.Find("HardwareStation");
        Assert.That(stationObject, Is.Not.Null);
        var station = GameObject.Find("HardwareStationInteraction").GetComponent<PcStationInteraction>();
        var pc = GameObject.Find("PcStationInteraction").GetComponent<PcStationInteraction>();
        var player = GameObject.Find("Player").GetComponent<Rigidbody2D>();
        player.GetComponent<PlayerKeyboardInput>().enabled = false;
        var movement = player.GetComponent<PlayerMovement>();
        var root = Object.FindFirstObjectByType<GameRoot>();
        root.Clock.Speed = SimulationSpeed.Paused;
        Assert.That(station.TryOpen(), Is.False);
        var reach = stationObject.transform.Find("Reach").GetComponent<CircleCollider2D>();
        var blocker = stationObject.transform.Find("Blocker").GetComponent<BoxCollider2D>();
        Assert.That(reach.isTrigger, Is.True);
        Assert.That(blocker.isTrigger, Is.False);
        player.position = (Vector2)stationObject.transform.position + Vector2.left * 2.5f;
        movement.SetDirection(Vector2.right);
        for (int i = 0; i < 60; i++) yield return new WaitForFixedUpdate();
        movement.SetDirection(Vector2.zero);
        yield return null;
        Assert.That(player.position.x, Is.LessThan(blocker.bounds.min.x));
        Assert.That(station.IsWithinReach, Is.True);
        Assert.That(station.TryOpen(), Is.True);
        Assert.That(pc.TryOpen(), Is.False);
        GameObject.Find("CloseHardwarePanel").GetComponent<Button>().onClick.Invoke();
        Assert.That(station.IsOpen, Is.False);
        Assert.That(station.TryOpen(), Is.True);
        station.HandleInput(true, true);
        Assert.That(station.IsOpen, Is.False);
        yield return null;
        Assert.That(station.TryOpen(), Is.True);
        player.position = new Vector2(0, -2);
        for (int i = 0; i < 2; i++) yield return new WaitForFixedUpdate();
        yield return null;
        Assert.That(station.IsOpen, Is.False);
        player.position = (Vector2)stationObject.transform.position + Vector2.left * 1.5f;
        for (int i = 0; i < 2; i++) yield return new WaitForFixedUpdate();
        yield return null;
        Assert.That(station.IsOpen, Is.False);
        Assert.That(station.TryOpen(), Is.True);
        var path = Path.Combine(Application.temporaryCachePath, "hardware-station-test.json");
        root.ConfigureSaveSlot(new SessionSaveSlot(path));
        var position = player.position;
        root.RequestSave();
        yield return null;
        root.RequestLoad();
        yield return null;
        Assert.That(station.IsOpen, Is.False);
        Assert.That(pc.IsOpen, Is.False);
        Assert.That(player.position, Is.EqualTo(position));
        File.Delete(path);
    }

    [UnityTest]
    public IEnumerator OverlappingReachSelectsOneStationAndPanelsFit()
    {
        yield return SceneManager.LoadSceneAsync("Main");
        var hardware = GameObject.Find("HardwareStationInteraction");
        Assert.That(hardware, Is.Not.Null);
        var station = hardware.GetComponent<PcStationInteraction>();
        var pc = GameObject.Find("PcStationInteraction").GetComponent<PcStationInteraction>();
        var player = GameObject.Find("Player").GetComponent<Rigidbody2D>();
        player.GetComponent<PlayerKeyboardInput>().enabled = false;
        player.position = new Vector2(-3.5f, 0.8f);
        // Deliberately overlap reach zones to exercise arbitration independently of placement.
        GameObject.Find("PCStation").transform.Find("Reach").GetComponent<CircleCollider2D>().radius = 10;
        for (int i = 0; i < 2; i++) yield return new WaitForFixedUpdate();
        yield return null;
        pc.HandleInput(true, false);
        station.HandleInput(true, false);
        Assert.That(station.IsOpen, Is.True, "Nearest reachable station wins regardless of update order.");
        Assert.That(pc.IsOpen, Is.False);
        foreach (var size in new[] { new Vector2Int(800,600), new Vector2Int(1280,720) })
        {
            Screen.SetResolution(size.x,size.y,FullScreenMode.Windowed);
            yield return null;
            yield return null;
            Canvas.ForceUpdateCanvases();
            var top = new Vector3[4];
            var bottom = new Vector3[4];
            ((RectTransform)GameObject.Find("TopBar").transform).GetWorldCorners(top);
            ((RectTransform)GameObject.Find("StatusBar").transform).GetWorldCorners(bottom);
            foreach (var text in GameObject.Find("HardwarePanel").GetComponentsInChildren<Text>())
            {
                var corners = new Vector3[4];
                text.rectTransform.GetWorldCorners(corners);
                foreach (var corner in corners)
                {
                    Assert.That(corner.x, Is.InRange(0,Screen.width));
                    Assert.That(corner.y, Is.InRange(bottom[1].y,top[0].y));
                }
            }
            foreach (string name in new[] { "PCStation", "HardwareStation" })
                foreach (var sprite in GameObject.Find(name).GetComponentsInChildren<SpriteRenderer>())
                    foreach (var corner in new[] {sprite.bounds.min,sprite.bounds.max})
                    {
                        var point = Camera.main.WorldToScreenPoint(corner);
                        Assert.That(point.x, Is.InRange(0,Screen.width));
                        Assert.That(point.y, Is.InRange(bottom[1].y,top[0].y));
                    }
#if !UNITY_EDITOR
            foreach (string name in new[] {"CloseHardwarePanel", "Pause", "SaveGame", "LoadGame"})
            {
                var button=GameObject.Find(name).GetComponent<Button>();
                var rect=(RectTransform)button.transform;
                var pointer=new PointerEventData(EventSystem.current) {position=RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(rect.rect.center))};
                var hits=new System.Collections.Generic.List<RaycastResult>();
                EventSystem.current.RaycastAll(pointer,hits);
                Assert.That(hits.First().gameObject,Is.EqualTo(button.gameObject));
            }
#endif
            ScreenCapture.CaptureScreenshot(Path.Combine(Application.temporaryCachePath,$"HardwarePanel-{size.x}x{size.y}.png"));
            for (int i=0;i<10;i++) yield return null;
        }
        station.Close();
        yield return null;
        ScreenCapture.CaptureScreenshot(Path.Combine(Application.temporaryCachePath,"HardwareGarage.png"));
        for(int i=0;i<10;i++) yield return null;
    }
}
