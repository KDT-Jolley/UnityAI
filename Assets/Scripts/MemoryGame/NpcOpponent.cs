
using System.Collections.Generic;
using UnityEngine;

namespace MemoryGame
{
    public class NpcOpponent : MonoBehaviour
    {
        public enum Difficulty
        {
            Easy,
            Normal,
            Hard
        }

        [Header("NPC Settings")]
        public Difficulty difficulty = Difficulty.Normal;

        public float thinkDelay = 0.7f;
        public float flipPause = 0.9f;

        private GameManager gm;

        // Card object -> Symbol ID
        private readonly Dictionary<Card, int> known =
            new Dictionary<Card, int>();

        private void Awake()
        {
            gm = GameManager.Instance;
        }

        private void OnEnable()
        {
            if (gm == null)
            {
                gm = GameManager.Instance;
            }

            if (gm != null)
            {
                gm.OnCardRevealed += ObserveCard;
            }
        }

        private void OnDisable()
        {
            if (gm != null)
            {
                gm.OnCardRevealed -= ObserveCard;
            }
        }

        // Memory accuracy
        public float MemoryAccuracy
        {
            get
            {
                switch (difficulty)
                {
                    case Difficulty.Easy:
                        return 0.35f;

                    case Difficulty.Hard:
                        return 1f;

                    default:
                        return 0.7f;
                }
            }
        }

        // Reset NPC memory
        public void ResetMemory()
        {
            known.Clear();
        }

        // Remember revealed card
        private void ObserveCard(Card card)
        {
            if (card == null)
                return;

            if (known.ContainsKey(card))
                return;

            if (Random.value <= MemoryAccuracy)
            {
                known[card] = card.SymbolId;
            }
        }

        // Choose two cards
        public bool TryChoosePair(out Card a, out Card b)
        {
            a = null;
            b = null;

            if (gm == null || gm.AllCards == null)
                return false;

            List<Card> candidates = new List<Card>();

            // Find available cards
            foreach (Card c in gm.AllCards)
            {
                if (c != null && !c.IsMatched && !c.IsFaceUp)
                {
                    candidates.Add(c);
                }
            }

            if (candidates.Count < 2)
                return false;

            // 1. Find memorized pair
            for (int i = 0; i < candidates.Count; i++)
            {
                int si;

                if (!known.TryGetValue(candidates[i], out si))
                {
                    continue;
                }

                for (int j = i + 1; j < candidates.Count; j++)
                {
                    int sj;

                    if (known.TryGetValue(candidates[j], out sj)
                        && sj == si)
                    {
                        a = candidates[i];
                        b = candidates[j];

                        return true;
                    }
                }
            }

            // 2. Memorized card + random guess
            List<Card> knownCards = candidates.FindAll(
                c => known.ContainsKey(c)
            );

            if (knownCards.Count > 0 && Random.value < 0.75f)
            {
                Card selectedCard = knownCards[
                    Random.Range(0, knownCards.Count)
                ];

                a = selectedCard;

                List<Card> rest = candidates.FindAll(
                    c => c != selectedCard
                );

                if (rest.Count > 0)
                {
                    b = rest[Random.Range(0, rest.Count)];
                    return true;
                }
            }

            // 3. Two random cards
            int pick = Random.Range(0, candidates.Count);

            a = candidates[pick];

            candidates.RemoveAt(pick);

            b = candidates[Random.Range(0, candidates.Count)];

            return true;
        }
    }
}
