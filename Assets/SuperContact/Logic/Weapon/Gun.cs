using UnityEngine;

public abstract class Gun : MonoBehaviour {
    public delegate void OnFireDelegate(float timestamp);
    public event OnFireDelegate onFire;
    protected void RaiseOnFireEvent(float timestamp) {
        if (onFire != null) {
            onFire(timestamp);
        }
    }

    public abstract void PullTrigger(float timestamp);
    public abstract void ReleaseTrigger(float timestamp);
}
