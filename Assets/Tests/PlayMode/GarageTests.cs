using System.Collections;
using InfrastructureTycoon.World;
using InfrastructureTycoon.Simulation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class GarageTests
{
    private float originalStep;
    private PlayerMovement movement;
    private Rigidbody2D body;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        originalStep = Time.fixedDeltaTime;
        yield return SceneManager.LoadSceneAsync("Main");
        var player = GameObject.Find("Player");
        Assert.That(player, Is.Not.Null);
        player.GetComponent<PlayerKeyboardInput>().enabled = false;
        movement = player.GetComponent<PlayerMovement>();
        body = player.GetComponent<Rigidbody2D>();
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Time.fixedDeltaTime = originalStep;
        if (movement != null) movement.SetDirection(Vector2.zero);
        yield return null;
    }

    [UnityTest]
    public IEnumerator MovementUsesElapsedPhysicsTimeAndNormalizesDiagonalSpeed()
    {
        var start = new Vector2(-4, -2);
        foreach (float step in new[] { 0.02f, 0.04f })
        {
            Time.fixedDeltaTime = step;
            foreach (var input in new[] { Vector2.right, Vector2.one })
            {
                body.position = start;
                movement.SetDirection(input);
                for (int i = 0; i < Mathf.RoundToInt(0.4f / step); i++)
                    yield return new WaitForFixedUpdate();
                Assert.That(Vector2.Distance(start, body.position), Is.EqualTo(1.2f).Within(0.04f));
                movement.SetDirection(Vector2.zero);
                body.linearVelocity = Vector2.zero;
            }
        }
    }

    [UnityTest]
    public IEnumerator WallsAndPcBlockMovementButReachTriggerDoesNot()
    {
        body.position = new Vector2(-5, -2);
        movement.SetDirection(Vector2.left);
        for (int i = 0; i < 40; i++) yield return new WaitForFixedUpdate();
        Assert.That(body.position.x, Is.GreaterThan(-5.7f));
        var pc = GameObject.Find("PCStation");
        var blocker = pc.transform.Find("Blocker").GetComponent<BoxCollider2D>();
        var reach = pc.transform.Find("Reach").GetComponent<CircleCollider2D>();
        Assert.That(blocker.isTrigger, Is.False);
        Assert.That(reach.isTrigger, Is.True);
        body.position = (Vector2)pc.transform.position + new Vector2(-2.5f, 0);
        body.linearVelocity = Vector2.zero;
        movement.SetDirection(Vector2.right);
        for (int i = 0; i < 60; i++) yield return new WaitForFixedUpdate();
        Assert.That(body.position.x, Is.LessThan(blocker.bounds.min.x));
        Assert.That(reach.OverlapPoint(body.position), Is.True);
        Assert.That(body.position.x, Is.GreaterThan(pc.transform.position.x - reach.radius));
    }

    [UnityTest]
    public IEnumerator WalkingStillWorksWhileSimulationIsPaused()
    {
        var clock = GameObject.Find("GameRoot").GetComponent<InfrastructureTycoon.GameRoot>().Clock;
        clock.Speed = SimulationSpeed.Paused;
        var date = clock.Date;
        double progress = clock.DayProgress;
        var start = body.position;
        movement.SetDirection(Vector2.right);
        for (int i = 0; i < 10; i++) yield return new WaitForFixedUpdate();
        Assert.That(body.position.x, Is.GreaterThan(start.x + 0.4f));
        Assert.That(clock.Date, Is.EqualTo(date));
        Assert.That(clock.DayProgress, Is.EqualTo(progress));
    }
}
