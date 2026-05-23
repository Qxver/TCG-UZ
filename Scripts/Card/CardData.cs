using Godot;

[GlobalClass]
public partial class CardData : Resource{
	[Export] public string Id = "";
	[Export] public string Name = "";
	[Export] public string Description = "";
	[Export] public Texture2D Portrait;
	[Export] public int Rarity = 1;
	[Export] public int Attack = 1;
	[Export] public int Health = 1;
	[Export] public int Cost = 1;
}
