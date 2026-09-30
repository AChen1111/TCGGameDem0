using System;
using System.Linq;
using System.Reflection;
using AChen.Activities;
using AChen.Configuration;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class ActivityPrefabTests
{
    const string Folder = "Assets/UI/Prefab/Hall/Activities/";
    static T[] All<T>(GameObject root) where T : Component => Resources.FindObjectsOfTypeAll<T>()
        .Where(x => x.transform == root.transform || x.transform.IsChildOf(root.transform)).ToArray();
    [TestCase("ActivityWindow", false)]
    [TestCase("ActivityPopupWindow", true)]
    public void Window_prefabs_use_existing_layer_queue_and_serialized_bindings(string name, bool popup)
    {
        var root = AssetDatabase.LoadAssetAtPath<GameObject>(Folder + name + ".prefab"); Assert.That(root, Is.Not.Null);
        var window = All<AWindowController>(root).Single(); Assert.That(window.IsPopup, Is.EqualTo(popup));
        Assert.That(window.WindowPriority, Is.EqualTo(WindowPriority.Enqueue)); Assert.That(window.DestroyOnClose, Is.True);
        Assert.That(new SerializedObject(window).FindProperty("m_BtnClose").objectReferenceValue, Is.Not.Null);
        Assert.That(All<UiScreenGenerator>(root).Single(), Is.Not.Null);
        foreach (var component in All<MonoBehaviour>(root).Where(c => c is ActivityWindow or ActivityPopupWindow or ActivityDetailView)) AssertReferences(component);
    }
    [TestCase("ActivityListItem")]
    [TestCase("ActivityRewardItem")]
    public void Reusable_item_prefabs_have_all_required_references(string name)
    {
        var root = AssetDatabase.LoadAssetAtPath<GameObject>(Folder + name + ".prefab"); Assert.That(root, Is.Not.Null);
        foreach (var component in All<MonoBehaviour>(root).Where(c => c is ActivityListItem or ActivityRewardItem)) AssertReferences(component);
    }
    [Test]
    public void Lobby_settings_register_both_window_prefabs()
    {
        var settings = AssetDatabase.LoadAssetAtPath<UISettings>("Assets/UI/Prefab/Hall/UISetting.asset");
        foreach (string name in new[] { "ActivityWindow", "ActivityPopupWindow" })
            Assert.That(settings.ScreensToRegister, Does.Contain(AssetDatabase.LoadAssetAtPath<GameObject>(Folder + name + ".prefab")));
    }
    static void AssertReferences(MonoBehaviour component)
    {
        var serialized = new SerializedObject(component);
        foreach (var field in component.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic).Where(x => x.IsDefined(typeof(SerializeField), true)))
        {
            var property = serialized.FindProperty(field.Name);
            if (typeof(UnityEngine.Object).IsAssignableFrom(field.FieldType)) Assert.That(property.objectReferenceValue, Is.Not.Null, component.GetType().Name + "." + field.Name);
            if (field.FieldType == typeof(Sprite[]))
            { Assert.That(property.arraySize, Is.EqualTo(5)); for (int i = 0; i < property.arraySize; i++) Assert.That(property.GetArrayElementAtIndex(i).objectReferenceValue, Is.Not.Null); }
        }
    }
    [Test]
    public void Existing_loading_prefab_binds_activity_controls()
    {
        var root = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/UI/Prefab/Common/LoadIN.prefab");
        var view = All<SceneTransitionOverlayView>(root).Single();
        AssertReferences(view);
        var fields = new SerializedObject(view);
        Assert.That(fields.FindProperty("m_retry").objectReferenceValue, Is.Not.Null);
        Assert.That(fields.FindProperty("m_progress").objectReferenceValue, Is.Not.Null);
        Assert.That(fields.FindProperty("m_status").objectReferenceValue, Is.Not.Null);
    }
    [Test]
    public void Started_activity_requires_loading_even_when_personal_condition_is_locked()
    {
        var now = DateTimeOffset.Parse("2026-10-01T00:00:00+08:00");
        var config = new ActivityConfiguration(null);
        var master = new ActivityMasterRow { ActivityId = "future", IsEnabled = true, ScheduleMode = 1, StartsAt = now, EndsAt = now.AddDays(1) };
        var before = new ActivityIndexResponse { ReleaseId = "same", ServerTime = now.AddSeconds(-1), Activities = { new ActivityIndexItem { Master = master, Eligible = false } } };
        config.Commit(before);
        var after = new ActivityIndexResponse { ReleaseId = "same", ServerTime = now, Activities = before.Activities };
        Assert.That(config.RequiresLoading(after), Is.True);
        master.IsEnabled = false;
        Assert.That(master.IsOpen(now), Is.False);
        Assert.That(config.RequiresLoading(after), Is.False);
    }
    [Test]
    public void Invalid_snapshot_does_not_partially_replace_existing_activities()
    {
        var manager = new ActivityManager(null); var first = Snapshot("first", ActivityType.Gift); Install(manager, first);
        var invalid = Snapshot("next", ActivityType.Gift); invalid.ServerTime = first.ServerTime.AddSeconds(1);
        invalid.Activities.Add(new ActivitySnapshot { Definition = new ActivityDefinition { Id = "first", Type = ActivityType.Notice, NameKey = "activity.daily_gold.name" } });
        Assert.Throws<TargetInvocationException>(() => Install(manager, invalid));
        Assert.That(manager.Snapshot, Is.SameAs(first)); Assert.That(manager.Items.Single().Definition.Id, Is.EqualTo("first"));
    }
    [Test]
    public void Older_response_cannot_replace_newer_snapshot()
    {
        var manager = new ActivityManager(null); var first = Snapshot("first", ActivityType.Gift); Install(manager, first);
        var delayed = Snapshot("delayed", ActivityType.Gift); delayed.ServerTime = first.ServerTime.AddSeconds(-1); Install(manager, delayed);
        Assert.That(manager.Snapshot, Is.SameAs(first));
    }
    static ActivityListResponse Snapshot(string id, ActivityType type) => new ActivityListResponse { ServerTime = DateTimeOffset.UtcNow,
        Activities = { new ActivitySnapshot { Definition = new ActivityDefinition { Id = id, Type = type, NameKey = "activity.daily_gold.name" } } } };
    static void Install(ActivityManager manager, ActivityListResponse snapshot) => typeof(ActivityManager).GetMethod("Install", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(manager, new object[] { snapshot });
}
