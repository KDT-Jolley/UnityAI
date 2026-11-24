using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace MemoryGame
{
    public class Card : MonoBehaviour, IPointerClickHandler
    {
        public int SymbolId { get; private set; }
        public bool IsFaceUp { get; private set; }
        public bool IsMatched { get; private set; }
        public event Action<Card> OnClicked;

        private RectTransform rect;
        private GameObject backObj;
        private GameObject frontObj;
        private Image backImage;
        private Image frontImage;

        private static readonly string[] Symbols =
            { "あ", "い", "う", "え", "お", "か", "き", "く", "け", "こ", "さ", "し", "す", "せ", "そ", "た" };

        private const float UI_SCALE = 90f; // 1 world unit = 90 px

        public static Card Create(Vector3 pos, float size, int symbolId)
        {
            var root = GameManager.Instance.CardRoot;
            var go = new GameObject("Card_" + symbolId);
            go.transform.SetParent(root, false);
            var card = go.AddComponent<Card>();
            card.SymbolId = symbolId;
            card.BuildUI(size * UI_SCALE, Symbols[symbolId % Symbols.Length]);
            card.rect.anchoredPosition = pos * UI_SCALE;
            return card;
        }

        private void BuildUI(float pxSize, string symbol)
        {
            rect = gameObject.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(pxSize, pxSize);
            rect.localScale = Vector3.one;

            var white = GameManager.WhiteSprite;

            backObj = new GameObject("Back");
            backObj.transform.SetParent(transform, false);
            backImage = backObj.AddComponent<Image>();
            backImage.sprite = white;
            backImage.color = new Color(0.18f, 0.28f, 0.55f);
            FitChild(backObj, pxSize);

            frontObj = new GameObject("Front");
            frontObj.transform.SetParent(transform, false);
            frontImage = frontObj.AddComponent<Image>();
            frontImage.sprite = white;
            frontImage.color = new Color(0.95f, 0.93f, 0.85f);
            FitChild(frontObj, pxSize);

            var textObj = new GameObject("Symbol");
            textObj.transform.SetParent(frontObj.transform, false);
            var text = textObj.AddComponent<Text>();
            text.text = symbol;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = Mathf.RoundToInt(pxSize * 0.5f);
            text.color = new Color(0.1f, 0.1f, 0.12f);
            text.alignment = TextAnchor.MiddleCenter;
            FitChild(textObj, pxSize);

            frontObj.SetActive(false);
        }

        private static void FitChild(GameObject go, float pxSize)
        {
            var rt = go.GetComponent<RectTransform>();
            if (rt == null) rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (IsMatched) return;
            OnClicked?.Invoke(this);
        }

        public void FlipFaceUp()
        {
            if (IsFaceUp) return;
            IsFaceUp = true;
            StopAllCoroutines();
            StartCoroutine(FlipRoutine(true));
        }

        public void FlipFaceDown()
        {
            if (!IsFaceUp) return;
            IsFaceUp = false;
            StopAllCoroutines();
            StartCoroutine(FlipRoutine(false));
        }

        private System.Collections.IEnumerator FlipRoutine(bool toFaceUp)
        {
            float half = GameManager.Instance.cardFlipSpeed * 0.5f;
            float t = 0f;
            while (t < half)
            {
                t += Time.deltaTime;
                float s = Mathf.Lerp(1f, 0.05f, t / half);
                rect.localScale = new Vector3(s, 1f, 1f);
                yield return null;
            }
            backObj.SetActive(!toFaceUp);
            frontObj.SetActive(toFaceUp);
            t = 0f;
            while (t < half)
            {
                t += Time.deltaTime;
                float s = Mathf.Lerp(0.05f, 1f, t / half);
                rect.localScale = new Vector3(s, 1f, 1f);
                yield return null;
            }
            rect.localScale = Vector3.one;
        }

        public void SetMatched()
        {
            IsMatched = true;
            frontImage.color = new Color(0.45f, 0.8f, 0.5f);
            var imgs = GetComponentsInChildren<Image>();
            foreach (var img in imgs) img.raycastTarget = false;
        }
    }
}
