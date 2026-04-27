// Assets/Scripts/Cards/CardBase.cs
using UnityEngine;

public abstract class CardBase : ScriptableObject, ICard
{
    [SerializeField] protected string _cardName;
    [SerializeField] protected string _description;

    public string CardName => _cardName;
    public string Description => _description;

    public virtual bool CanUse(GameManager gm, Player owner) => true;
    public abstract void Execute(GameManager gm, Player owner);
}