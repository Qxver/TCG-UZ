using Godot;
using System;

public partial class Card : Control
{
	[Signal]
    public delegate void CardOnHoverEnteredEventHandler(Card card);

    [Signal]
    public delegate void CardOnHoverExitedEventHandler(Card card);

	public bool isPlaced = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetParent<CardManager>().ConnectCardSignals(this);
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void OnGrabAreaEntered()
	{
		EmitSignal(SignalName.CardOnHoverEntered, this);
	}

	public void OnGrabAreaExited()
	{
		EmitSignal(SignalName.CardOnHoverExited, this);
	}
}

