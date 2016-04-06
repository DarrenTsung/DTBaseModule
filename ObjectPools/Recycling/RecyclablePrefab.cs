using UnityEngine;
using UnityEngine.Events;

namespace DT {
  public class RecyclablePrefab : MonoBehaviour {
    public void Setup() {
      foreach (IRecycleSetupSubscriber subscriber in this._setupSubscribers) {
        subscriber.OnRecycleSetup();
      }
    }

    public void Cleanup() {
      foreach (IRecycleCleanupSubscriber subscriber in this._cleanupSubscribers) {
        subscriber.OnRecycleCleanup();
      }
    }

    public string prefabName;

    // PRAGMA MARK - Internal
    private IRecycleSetupSubscriber[] _setupSubscribers;
    private IRecycleCleanupSubscriber[] _cleanupSubscribers;

    private void Awake() {
      this._setupSubscribers = this.GetDepthSortedComponentsInChildren<IRecycleSetupSubscriber>(greatestDepthFirst: true);
      this._cleanupSubscribers = this.GetDepthSortedComponentsInChildren<IRecycleCleanupSubscriber>(greatestDepthFirst: true);
    }
  }
}