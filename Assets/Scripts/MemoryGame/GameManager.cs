using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MemoryGame
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        // UI parent that all cards are created under (set up by EnsureCanvas)
        public RectTransform CardRoot { get; private set; }
        public RectTransform CanvasRoot { get; private set; }

        public int Moves { get; private set; }
        public float Elapsed { get { return Time.time - startTime; } }
        private float startTime;

        private static Sprite whiteSprite;
        public static Sprite WhiteSprite
        {
            get
            {
                if (whiteSprite == null)
                {
                    var tex = new Texture2D(1, 1);
                    tex.SetPixel(0, 0, Color.white);
                    tex.Apply();
                    whiteSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
                }
                return whiteSprite;
            }
        }

        [Header("Game Settings")]
        public int rows = 4;
        public int cols = 4;
        public float cardFlipSpeed = 0.3f;
        public float matchDelay = 1.0f;
        public float mismatchDelay = 1.5f;

        private List<Card> allCards = new List<Card>();
        private List<Card> faceUpCards = new List<Card>();
        private int matchedPairs = 0;
        private int totalPairs;
        private bool inputLocked = false;

        // Events for UI
        public System.Action<int, int> OnMatch;
        public System.Action<int, int> OnMismatch;
        public System.Action OnGameComplete;
        public System.Action<Card> OnCardRevealed;

        // Read-only view of the board (used by NpcOpponent)
        public IReadOnlyList<Card> AllCards { get { return allCards; } }

        private void Awake()
        {
            Instance = this;
            if ((rows * cols) % 2 != 0) cols += 1; // pairs need an even cell count
            totalPairs = (rows * cols) / 2;
        }

        public void StartNewGame()
        {
            StopAllCoroutines(); // cancel any pending pair resolution
            EnsureCanvas();
            EnsureEventSystem();
            ClearBoard();
            CreateBoard();
            matchedPairs = 0;
            Moves = 0;
            startTime = Time.time;
            inputLocked = false;
        }

        private void EnsureCanvas()
        {
            if (CardRoot != null) return;

            var canvasGO = new GameObject("GameCanvas");
            canvasGO.transform.SetParent(transform, false);
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasRoot = canvasGO.transform as RectTransform;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGO.AddComponent<GraphicRaycaster>();

            var rootGO = new GameObject("CardsRoot");
            rootGO.transform.SetParent(canvasGO.transform, false);
            CardRoot = rootGO.AddComponent<RectTransform>();
        }

        private void EnsureEventSystem()
        {
            if (EventSystem.current != null) return;
            var esGO = new GameObject("EventSystem");
            esGO.transform.SetParent(transform, false);
            esGO.AddComponent<EventSystem>();
            esGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        private void ClearBoard()
        {
            foreach (var card in allCards)
            {
                if (card != null) Destroy(card.gameObject);
            }
            allCards.Clear();
            faceUpCards.Clear();
        }

        private void CreateBoard()
        {
            int totalCards = rows * cols;
            int symbolCount = totalCards / 2;

            // Create pairs of symbols
            List<int> symbols = new List<int>();
            for (int i = 0; i < symbolCount; i++)
            {
                symbols.Add(i);
                symbols.Add(i);
            }

            // Shuffle
            for (int i = symbols.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (symbols[i], symbols[j]) = (symbols[j], symbols[i]);
            }

            // Create cards in grid
            float cardSize = 1.5f;
            float spacing = 0.2f;
            float startX = -(cols - 1) * (cardSize + spacing) / 2f;
            float startY = (rows - 1) * (cardSize + spacing) / 2f;

            int index = 0;
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    Vector3 pos = new Vector3(
                        startX + c * (cardSize + spacing),
                        startY - r * (cardSize + spacing),
                        0f
                    );

                    var card = Card.Create(pos, cardSize, symbols[index]);
                    card.OnClicked += OnCardClicked;
                    allCards.Add(card);
                    index++;
                }
            }
        }

        private void OnCardClicked(Card card)
        {
            if (inputLocked) return;
            if (card.IsMatched || card.IsFaceUp) return;
            if (faceUpCards.Count >= 2) return;

            card.FlipFaceUp();
            faceUpCards.Add(card);

            if (faceUpCards.Count == 2)
            {
                inputLocked = true;
                StartCoroutine(ResolvePair());
            }
        }

        private IEnumerator ResolvePair()
        {
            yield return new WaitForSeconds(cardFlipSpeed + 0.5f);

            Card a = faceUpCards[0];
            Card b = faceUpCards[1];
            Moves++;

            if (a.SymbolId == b.SymbolId)
            {
                a.SetMatched();
                b.SetMatched();
                matchedPairs++;
                OnMatch?.Invoke(a.SymbolId, b.SymbolId);
                faceUpCards.Clear();

                if (matchedPairs >= totalPairs)
                {
                    OnGameComplete?.Invoke();
                }
            }
            else
            {
                OnMismatch?.Invoke(a.SymbolId, b.SymbolId);
                yield return new WaitForSeconds(mismatchDelay);
                a.FlipFaceDown();
                b.FlipFaceDown();
                faceUpCards.Clear();
            }

            inputLocked = false;
        }

        public int GetMatchedPairs() => matchedPairs;
        public int GetTotalPairs() => totalPairs;
    }
}
