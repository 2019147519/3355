// Assets/Scripts/Cards/CardManager.cs
using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    [SerializeField] private List<CardBase> _deckTemplate;
    [SerializeField] private int _handSize = 3;

    private List<CardBase> _blackHand = new();
    private List<CardBase> _whiteHand = new();

    public IReadOnlyList<CardBase> GetHand(Player p)
        => p == Player.Black ? _blackHand : _whiteHand;

    public void DealInitialHand()
    {
        _blackHand.Clear(); _whiteHand.Clear();
        for (int i = 0; i < _handSize; i++)
        {
            _blackHand.Add(DrawRandom());
            _whiteHand.Add(DrawRandom());
        }
    }

    public bool UseCard(int index, Player owner, GameManager gm)
    {
        var hand = owner == Player.Black ? _blackHand : _whiteHand;
        if (index < 0 || index >= hand.Count) return false;

        var card = hand[index];
        if (!card.CanUse(gm, owner)) return false;

        card.Execute(gm, owner);
        hand.RemoveAt(index);
        hand.Add(DrawRandom()); // »Ì±â
        return true;
    }

    private CardBase DrawRandom()
        => _deckTemplate[Random.Range(0, _deckTemplate.Count)];
}
