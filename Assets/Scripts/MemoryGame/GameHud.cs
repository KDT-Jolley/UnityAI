using UnityEngine;
using UnityEngine.UI;

namespace MemoryGame
{
    // Builds the score bar, restart button and clear overlay programmatically.
    public class GameHud : MonoBehaviour
    {
        private GameManager gm;
        private Text scoreText;
        private Text statsText;
        private GameObject winPanel;
        private Text winStatsText;

        private void Awake()
        {
            gm = GameManager.Instance;
            BuildHud();
            gm.OnMatch += OnMatch;
            gm.OnGameComplete += OnGameComplete;
        }

        private void Update()
        {
            if (statsText != null)
                statsText.text = $"手数: {gm.Moves}   時間: {gm.Elapsed:F1}秒";
        }

        private void BuildHud()
        {
            var canvas = gm.CanvasRoot;

            MakeText("Title", canvas, "神経衰弱", 40,
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(40, -40),
                TextAnchor.UpperLeft, new Vector2(500, 56));

            scoreText = MakeText("Score", canvas, $"ペア: 0 / {gm.GetTotalPairs()}", 34,
                new Vector2(1, 1), new Vector2(1, 1), new Vector2(-40, -40),
                TextAnchor.UpperRight, new Vector2(500, 48));

            statsText = MakeText("Stats", canvas, "手数: 0   時間: 0.0秒", 24,
                new Vector2(1, 1), new Vector2(1, 1), new Vector2(-40, -92),
                TextAnchor.UpperRight, new Vector2(500, 34));

            MakeButton("RestartButton", canvas, "やり直し",
                new Vector2(0, 0), new Vector2(40, 40), new Vector2(200, 56), OnRestart);

            BuildWinPanel(canvas);
        }

        private void BuildWinPanel(Transform canvas)
        {
            var panelGO = new GameObject("WinPanel");
            panelGO.transform.SetParent(canvas, false);
            var bg = panelGO.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.78f);
            bg.raycastTarget = true; // block clicks on cards behind it
            var rt = panelGO.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            MakeText("WinTitle", panelGO.transform, "クリア!", 96,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 140),
                TextAnchor.MiddleCenter, new Vector2(800, 120));

            winStatsText = MakeText("WinStats", panelGO.transform, "", 36,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 20),
                TextAnchor.MiddleCenter, new Vector2(800, 60));

            MakeButton("PlayAgainButton", panelGO.transform, "もう一度あそぶ",
                new Vector2(0.5f, 0.5f), new Vector2(0, -120), new Vector2(280, 64), OnRestart);

            panelGO.SetActive(false);
            winPanel = panelGO;
        }

        private void OnMatch(int a, int b)
        {
            scoreText.text = $"ペア: {gm.GetMatchedPairs()} / {gm.GetTotalPairs()}";
        }

        private void OnGameComplete()
        {
            winStatsText.text = $"手数: {gm.Moves}   時間: {gm.Elapsed:F1}秒";
            winPanel.SetActive(true);
        }

        private void OnRestart()
        {
            winPanel.SetActive(false);
            gm.StartNewGame();
            scoreText.text = $"ペア: 0 / {gm.GetTotalPairs()}";
        }

        private static Text MakeText(string name, Transform parent, string content, int size,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, TextAnchor align, Vector2 sizeDelta)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<Text>();
            t.text = content;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = size;
            t.color = Color.white;
            t.alignment = align;
            t.raycastTarget = false;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = pos;
            rt.sizeDelta = sizeDelta;
            return t;
        }

        private static Button MakeButton(string name, Transform parent, string label,
            Vector2 anchor, Vector2 pos, Vector2 size, System.Action onClick)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = new Color(0.2f, 0.45f, 0.8f);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(() => onClick());

            var txtGO = new GameObject("Label");
            txtGO.transform.SetParent(go.transform, false);
            var t = txtGO.AddComponent<Text>();
            t.text = label;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = 26;
            t.color = Color.white;
            t.alignment = TextAnchor.MiddleCenter;
            t.raycastTarget = false;
            var trt = txtGO.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;
            return btn;
        }
    }
}
