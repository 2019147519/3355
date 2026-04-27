// Assets/Scripts/UI/CardSlotUI.cs
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardSlotUI : MonoBehaviour
{
    [SerializeField] private Image _cardArt;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _descText;
    [SerializeField] private Button _button;
    [SerializeField] private GameObject _emptyOverlay;

    private int _index;

    public void SetCard(CardBase card, int index, Action<int> onTap)
    {
        _index = index;
        _nameText.text = card.CardName;
        _descText.text = card.Description;
        _emptyOverlay.SetActive(false);
        _button.interactable = true;
        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() => onTap(_index));
    }

    public void SetEmpty()
    {
        _nameText.text = "";
        _descText.text = "";
        _emptyOverlay.SetActive(true);
        _button.interactable = false;
        _button.onClick.RemoveAllListeners();
    }
}