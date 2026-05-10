using UnityEngine;

public static class BackendRuntimeBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void BootstrapScene()
    {
        EnsureBackendManager();
    }

    private static void EnsureBackendManager()
    {
        if (BackendManager.Instance != null) return;
        var go = new GameObject("BackendManager");
        go.AddComponent<BackendManager>();
    }
}
