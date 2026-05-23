using Godot;
using System;

public partial class Card : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void OnGrabAreaEntered(){
		this.Scale = new Vector2(1.05f, 1.05f);
	}

	public void OnGrabAreaExited(){
		this.Scale = new Vector2(1f, 1f);
	}
}
