using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class MainSceneTests
{
    [UnityTest]
    public IEnumerator MainLoadsWithVisibleHudAtDifferentWindowSizes()
    {
        yield return SceneManager.LoadSceneAsync("Main");
        Assert.That(GameObject.Find("GameRoot"), Is.Not.Null);
        Assert.That(GameObject.Find("WorldView"), Is.Not.Null);
        Assert.That(Camera.main, Is.Not.Null);
        Assert.That(Camera.main.orthographic, Is.True);

        var canvas = GameObject.Find("HUD").GetComponent<Canvas>();
        var scaler = canvas.GetComponent<CanvasScaler>();
        Assert.That(canvas.renderMode, Is.EqualTo(RenderMode.ScreenSpaceOverlay));
        Assert.That(scaler.uiScaleMode, Is.EqualTo(CanvasScaler.ScaleMode.ScaleWithScreenSize));

        int originalWidth = Screen.width;
        int originalHeight = Screen.height;
        foreach (var size in new[] { new Vector2Int(1280, 720), new Vector2Int(800, 600), new Vector2Int(1920, 1080) })
        {
            Screen.SetResolution(size.x, size.y, FullScreenMode.Windowed);
            yield return null;
            yield return null;
#if !UNITY_EDITOR
            Assert.That(Screen.width, Is.EqualTo(size.x));
            Assert.That(Screen.height, Is.EqualTo(size.y));
#endif
            Canvas.ForceUpdateCanvases();
            var hudRect = (RectTransform)canvas.transform;
            var top = (RectTransform)GameObject.Find("TopBar").transform;
            var bottom = (RectTransform)GameObject.Find("StatusBar").transform;
            Assert.That(top.anchorMin, Is.EqualTo(new Vector2(0, 1)));
            Assert.That(top.anchorMax, Is.EqualTo(Vector2.one));
            Assert.That(bottom.anchorMin, Is.EqualTo(Vector2.zero));
            Assert.That(bottom.anchorMax, Is.EqualTo(new Vector2(1, 0)));
            Assert.That(top.rect.width, Is.EqualTo(hudRect.rect.width).Within(1));
            Assert.That(bottom.rect.width, Is.EqualTo(hudRect.rect.width).Within(1));
            foreach (var label in canvas.GetComponentsInChildren<Text>())
            {
                Assert.That(label.text, Is.Not.Empty);
                Assert.That(label.font, Is.Not.Null);
                Assert.That(label.color.a, Is.GreaterThan(0));
                var corners = new Vector3[4];
                label.rectTransform.GetWorldCorners(corners);
                foreach (var corner in corners)
                {
                    var local = hudRect.InverseTransformPoint(corner);
                    Assert.That(local.x, Is.InRange(hudRect.rect.xMin - 1, hudRect.rect.xMax + 1), label.name);
                    Assert.That(local.y, Is.InRange(hudRect.rect.yMin - 1, hudRect.rect.yMax + 1), label.name);
                }
            }
            // Keep rendered evidence with the test results for visual inspection.
            string capture = Path.Combine(Application.temporaryCachePath, $"Main-{size.x}x{size.y}.png");
            ScreenCapture.CaptureScreenshot(capture);
            for (int frame = 0; frame < 10; frame++)
                yield return null;
            if (File.Exists(capture))
                TestContext.WriteLine("HUD screenshot: " + capture);
        }
        Screen.SetResolution(originalWidth, originalHeight, FullScreenMode.Windowed);
        LogAssert.NoUnexpectedReceived();
    }
}
