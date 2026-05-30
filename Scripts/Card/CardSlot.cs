using Godot;
using System;

public partial class CardSlot : Node2D
{
	[Export] public bool isPlayerSlot = true;
	public bool cardInside = false;
}
