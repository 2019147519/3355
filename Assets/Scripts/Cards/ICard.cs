// Assets/Scripts/Cards/ICard.cs
public interface ICard
{
    string CardName { get; }
    string Description { get; }
    bool CanUse(GameManager gm, Player owner);
    void Execute(GameManager gm, Player owner);
}