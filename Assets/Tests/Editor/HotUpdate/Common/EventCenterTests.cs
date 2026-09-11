using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using AChen.Events;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public sealed class EventCenterTests
{
    [Test]
    public void Dispatch_isolates_listener_failure_and_continues_synchronously()
    {
        var evt = new EventId<int>("Test.ListenerFailure." + Guid.NewGuid());
        int received = 0;
        Action<int> failing = _ => throw new InvalidOperationException("listener failed");
        Action<int> remaining = value => received = value;
        EventCenter.AddListener(evt, failing);
        EventCenter.AddListener(evt, remaining);
        try
        {
            LogAssert.Expect(LogType.Error, new Regex("事件回调失败.*Test.ListenerFailure"));
            Assert.DoesNotThrow(() => EventCenter.Dispatch(evt, 42));
            Assert.AreEqual(42, received);
        }
        finally
        {
            EventCenter.RemoveListener(evt, failing);
            EventCenter.RemoveListener(evt, remaining);
        }
    }

    [Test]
    public void Subscription_changes_during_dispatch_apply_to_the_next_dispatch()
    {
        var evt = new EventId("Test.SubscriptionSnapshot." + Guid.NewGuid());
        var calls = new List<string>();
        Action second = () => calls.Add("second");
        Action first = () =>
        {
            calls.Add("first");
            EventCenter.RemoveListener(evt, second);
        };
        EventCenter.AddListener(evt, first);
        EventCenter.AddListener(evt, second);
        try
        {
            EventCenter.Dispatch(evt);
            EventCenter.Dispatch(evt);
            CollectionAssert.AreEqual(new[] { "first", "second", "first" }, calls);
        }
        finally
        {
            EventCenter.RemoveListener(evt, first);
            EventCenter.RemoveListener(evt, second);
        }
    }

    [Test]
    public void Signature_mismatch_still_throws_at_the_publisher()
    {
        string name = "Test.Signature." + Guid.NewGuid();
        EventCenter.Dispatch(new EventId<int>(name), 1);
        Assert.Throws<InvalidOperationException>(() => EventCenter.Dispatch(new EventId<string>(name), "invalid"));
    }
}
