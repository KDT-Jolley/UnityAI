using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using MemoryGame;

// Batch-mode verification: builds the board the same way the runtime bootstrap
// does and checks card count / symbol pairing, then exits.
public static class MemoryGameVerify
{
    [MenuItem("Tools/Verify MemoryGame")]
    public static void Verify()
    {
        var go = new GameObject("VerifyGame");
        try
        {
            var gm = go.AddComponent<GameManager>();
            var hud = go.AddComponent<GameHud>();
            gm.StartNewGame();

            var cards = go.GetComponentsInChildren<Card>();
            var counts = new Dictionary<int, int>();
            foreach (var c in cards)
            {
                if (counts.ContainsKey(c.SymbolId)) counts[c.SymbolId]++;
                else counts[c.SymbolId] = 1;
            }

            bool pairsOk = counts.Count == gm.GetTotalPairs();
            foreach (var kv in counts)
                if (kv.Value != 2) pairsOk = false;

            bool hudOk = gm.CanvasRoot != null &&
                         go.GetComponentsInChildren<Text>().Length > 0;

            Debug.Log($"VERIFY_RESULT cards={cards.Length} expected={gm.rows * gm.cols} " +
                      $"pairsOk={pairsOk} hudOk={hudOk}");
        }
        catch (System.Exception e)
        {
            Debug.LogError("VERIFY_FAILED " + e);
        }
        finally
        {
            Object.DestroyImmediate(go);
        }

        EditorApplication.Exit(0);
    }
}
