public struct Move
{
    public Card movedCard;
    public int originalStack;
    public bool cardBelowTurned;

    public Move(Card movedCard, int originalStack, bool cardBelowTurned)
    {
        this.movedCard = movedCard;
        this.originalStack = originalStack;
        this.cardBelowTurned = cardBelowTurned;
    }
}