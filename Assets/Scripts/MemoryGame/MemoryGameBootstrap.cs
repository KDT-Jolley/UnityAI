using UnityEngine;

namespace MemoryGame
{
    // Auto-starts the game in whatever scene is loaded, so you just press Play.
    // If the scene already has a GameManager, nothing happens.
    public static class MemoryGameBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void AutoStart()
        {
            if (GameManager.Instance != null) return;

            var go = new GameObject("MemoryGame");
            var gm = go.AddComponent<GameManager>();
            go.AddComponent<GameHud>();
            gm.StartNewGame();
        }
    }
}
