using Godot;

public enum TurnState { PlayerTurn, EnemyTurn }

public partial class BattleManager : Node
{
    [Export] public InputManager InputManager;
    [Export] public EnemyHand EnemyHand;
    [Export] public EnemyDeck EnemyDeck;
    [Export] public Node2D EnemySlots;
    [Export] public Timer BattleTimer;
    [Export] public Button EndTurnButton;

    [Export] public int StartingCharacterCards = 5;
    [Export] public int StartingStudentCards = 0;

    public TurnState CurrentTurn { get; private set; } = TurnState.PlayerTurn;

    private const float EnemyThinkTime = 1.5f;

    public override void _Ready()
    {
        BattleTimer.Timeout += OnEnemyThinkFinished;

        for (int i = 0; i < StartingCharacterCards; i++)
            EnemyDeck.DrawCharacterCard();
        for (int i = 0; i < StartingStudentCards; i++)
            EnemyDeck.DrawStudentCard(InputManager.StudentDeck.GlobalPosition);

        for (int i = 0; i < StartingCharacterCards; i++)
            InputManager.CharacterDeck.DrawCard();
        for (int i = 0; i < StartingStudentCards; i++)
            InputManager.StudentDeck.DrawCard();

        StartPlayerTurn();
    }

    // ── Player turn ──────────────────────────────────────────────

    private void StartPlayerTurn()
    {
        CurrentTurn = TurnState.PlayerTurn;
        GD.Print("=== PLAYER TURN ===");
        InputManager.OnStartTurn();
        EndTurnButton.Disabled = false;
    }

    public void OnEndTurn()
    {
        if (CurrentTurn != TurnState.PlayerTurn) return;
        EndTurnButton.Disabled = true;
        InputManager.OnEndTurn();
        StartEnemyTurn();
    }

    // ── Enemy turn ───────────────────────────────────────────────

    private void StartEnemyTurn()
    {
        CurrentTurn = TurnState.EnemyTurn;
        GD.Print("=== ENEMY TURN ===");

        bool needsStudent = false;
        foreach (var card in EnemyHand.CardsInHand)
        {
            if (card.Data != null && card.Data.Cost > 0)
            {
                int cost = card.Data.Cost;
                int students = 0;
                foreach (var c in EnemyHand.CardsInHand)
                {
                    if (c.Data != null && c.Data.Cost == 0)
                        students++;
                }
                if (students < cost)
                {
                    needsStudent = true;
                    break;
                }
            }
        }

        if (needsStudent || EnemyDeck.RemainingCards == 0)
        {
            EnemyDeck.DrawStudentCard(InputManager.StudentDeck.GlobalPosition);
        }
        else
        {
            EnemyDeck.DrawCharacterCard();
        }

        BattleTimer.WaitTime = EnemyThinkTime;
        BattleTimer.OneShot = true;
        BattleTimer.Start();
    }

    private void OnEnemyThinkFinished()
    {
        PlayEnemyCard();
        StartPlayerTurn();
    }

    private void PlayEnemyCard()
    {
        EnemyCard cardToPlay = null;
        foreach (var card in EnemyHand.CardsInHand)
        {
            if (card.Data != null && card.Data.Cost > 0)
            {
                cardToPlay = card;
                break;
            }
        }

        if (cardToPlay == null)
        {
            GD.Print("Enemy: no character card to play.");
            return;
        }

        int cost = cardToPlay.Data.Cost;
        var students = new System.Collections.Generic.List<EnemyCard>();
        foreach (var card in EnemyHand.CardsInHand)
        {
            if (card != cardToPlay && card.Data != null && card.Data.Cost == 0)
                students.Add(card);
        }

        if (students.Count < cost)
        {
            GD.Print($"Enemy: can't afford '{cardToPlay.Data.Name}' (needs {cost}, has {students.Count} students).");
            return;
        }

        CardSlot freeSlot = null;
        foreach (Node child in EnemySlots.GetChildren())
        {
            if (child is CardSlot slot && slot.IsEmpty() && !slot.isPlayerSlot)
            {
                freeSlot = slot;
                break;
            }
        }

        if (freeSlot == null)
        {
            GD.Print("Enemy: no free slots.");
            return;
        }

        EnemyHand.RemoveCardFromHand(cardToPlay);

        for (int i = 0; i < cost; i++)
        {
            EnemyHand.RemoveCardFromHand(students[i]);
            students[i].QueueFree();
        }

        cardToPlay.SetFaceDown(false);
        cardToPlay.GlobalPosition = freeSlot.GlobalPosition - cardToPlay.PivotOffset;
        cardToPlay.isPlaced = true;
        freeSlot.cardInside = true;

        GD.Print($"Enemy played: {cardToPlay.Data.Name}");
    }
}