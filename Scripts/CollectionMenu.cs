using Godot;
using System.Collections.Generic;

public partial class CollectionMenu : Control
{
    [Export] public PackedScene Card;
    [Export] public GridContainer Grid;

    private static readonly string LockedTexturePath = "res://Sprites/Card/card_locked.png";

    public void _on_back_button_pressed()
    {
        GetTree().ChangeSceneToFile("res://Scenes/MainMenu.tscn");
    }

    public override void _Ready()
    {
        Refresh();
    }

    public void Refresh()
    {
        foreach (Node child in Grid.GetChildren())
            child.QueueFree();

        if (CardDatabase.Instance == null) {
            GD.Print("CardDatabase instance is null.");
            return;
        }

        var allCards = new List<CardData>(CardDatabase.Instance.AllCards);
        allCards.Sort((a, b) =>
        {
            bool aNum = int.TryParse(a.Id, out int aId);
            bool bNum = int.TryParse(b.Id, out int bId);
            if (aNum && bNum) return aId.CompareTo(bId);
            return string.Compare(a.Id, b.Id, System.StringComparison.Ordinal);
        });

        var collection = CardCollection.Instance.GetAllCards();
        if (collection == null) {
            GD.Print("Collection is null.");
            return;
        }

        var lockedTexture = ResourceLoader.Load<Texture2D>(LockedTexturePath);

        foreach (var data in allCards)
        {
            bool isUnlocked = collection.TryGetValue(data.Id, out int amount) && amount > 0;

            var card = Card.Instantiate<Node>();
            Grid.AddChild(card);

            var portrait = card.GetNodeOrNull<Sprite2D>("FrontFace/Portrait");
            var stats = card.GetNodeOrNull<Control>("Stats");
            if (portrait != null)
            {
                portrait.Texture = isUnlocked ? data.Portrait : lockedTexture;
				stats.Visible = isUnlocked;
			}
			 else
			 {
				 GD.Print($"Nie można znaleźć węzła 'FrontFace/Portrait' dla karty {data.Id}");
            }

            var frontFace = card.GetNodeOrNull<CanvasItem>("FrontFace");
            if (frontFace != null && !isUnlocked)
            {
                frontFace.Modulate = new Color(0.4f, 0.4f, 0.4f, 1f);
            }
        }
    }
}
