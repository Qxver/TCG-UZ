using Godot;

public partial class Card : Control
{
    [Signal] public delegate void CardOnHoverEnteredEventHandler(Card card);
    [Signal] public delegate void CardOnHoverExitedEventHandler(Card card);

    public bool isPlaced = false;
    public bool hasDrawnAnimPlayed = false;
    public Vector2 positionInHand;
    public CardData Data { get; private set; }

    public void SetFaceDown(bool faceDown)
    {
        var frontFace = GetNodeOrNull<Control>("FrontFace");
        var rearFace = GetNodeOrNull<Control>("RearFace");
        var stats = GetNodeOrNull<Control>("Stats");

        if (frontFace != null) frontFace.Visible = !faceDown;
        if (stats != null) stats.Visible = !faceDown;
        if (rearFace != null) rearFace.Visible = faceDown;
    }

    public override void _Ready()
    {
        GetParent<CardManager>().ConnectCardSignals(this);
    }

    public void Initialize(CardData data)
    {
        Data = data;

        var portrait = GetNodeOrNull<Sprite2D>("FrontFace/Portrait");
        if (portrait != null && data.Portrait != null)
            portrait.Texture = data.Portrait;

        var attackLabel = GetNodeOrNull<Label>("Stats/AttackLabel");
        var healthLabel = GetNodeOrNull<Label>("Stats/HealthLabel");
        var costLabel   = GetNodeOrNull<Label>("Stats/CostLabel");

        if (attackLabel != null) attackLabel.Text = data.Attack.ToString();
        if (healthLabel != null) healthLabel.Text = data.Health.ToString();
        if (costLabel   != null) costLabel.Text   = data.Cost.ToString();
    }

    public void OnGrabAreaEntered() => EmitSignal(SignalName.CardOnHoverEntered, this);
    public void OnGrabAreaExited()  => EmitSignal(SignalName.CardOnHoverExited, this);
}