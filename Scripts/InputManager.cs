using Godot;

public partial class InputManager : Node2D
{
    [Export] public Deck CharacterDeck;
    [Export] public Deck StudentDeck;
    [Export] public CardManager cardManager;

    [Export] public int StartingHandSize = 4;

    private bool hasDrawnThisTurn = false;

    const uint cardLayerMask = 1;
    const uint deckLayerMask = 4;

    public override void _Ready()
    {
        if (cardManager == null)
        {
            cardManager = GetParent().GetNodeOrNull<CardManager>("CardManager");
            if (cardManager == null)
                GD.PrintErr("InputManager: CardManager not found!");
        }

        CallDeferred(MethodName.DrawStartingHand);
    }

    private void DrawStartingHand()
    {
        for (int i = 0; i < StartingHandSize; i++)
        {
            if (CharacterDeck != null)
            {
                CharacterDeck.DrawCard();
            }
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
            {
                RaycastAtCursor();
            }
            else if (mouseEvent.ButtonIndex == MouseButton.Left && !mouseEvent.Pressed)
            {
                if (cardManager != null && cardManager.IsDragging())
                    cardManager.DragEnded(cardManager.GetDraggedCard());
            }
        }
    }

    public void RaycastAtCursor()
{
    var card = cardManager?.RaycastCheckForCard();
    if (card != null)
    {
        cardManager.DragStarted(card);
        return;
    }

    var spaceState = GetWorld2D().DirectSpaceState;
    var parameters = new PhysicsPointQueryParameters2D
    {
        Position = GetGlobalMousePosition(),
        CollideWithAreas = true,
        CollisionMask = deckLayerMask
    };

    var result = spaceState.IntersectPoint(parameters);
    if (result.Count == 0) return;

    result[0].TryGetValue("collider", out var resultCollision);
    TryDrawFromDeck(((Area2D)resultCollision).GetParent<Deck>());
}

    private void TryDrawFromDeck(Deck deck)
    {
        if (hasDrawnThisTurn)
        {
            GD.Print("Already drew a card this turn!");
            return;
        }
        if (deck.DrawCard())
            hasDrawnThisTurn = true;
    }

    public void OnEndTurn()
    {
        hasDrawnThisTurn = false;
        GD.Print("Turn ended.");
    }
}