using Godot;
using System;
using System.Collections.Generic;

public partial class OpenCardPack : Node
{
	[Export] public int CardsPerPack = 5;

	public List<CardData> OpenPack()
	{
		var allCards = CardDatabase.Instance.AllCards;
		var pulled = new List<CardData>();
		Random random = new();

		for (int i = 0; i < CardsPerPack; i++)
		{
			CardData card = DrawWeightedCard(allCards, random);
			CardCollection.Instance.AddCard(card);
			pulled.Add(card);
		}

		return pulled;
	}

	private int GetWeight(int rarity) => rarity switch
	{
		1 => 40,
		2 => 20,
		3 => 10,
		4 => 5,
		_ => 1
	};

	private CardData DrawWeightedCard(List<CardData> allCards, Random random)
	{
		var drawPool = allCards.FindAll(c => c.Id != "0");

		int totalWeight = 0;
		foreach (var card in drawPool)
			totalWeight += GetWeight(card.Rarity);

		int roll = random.Next(totalWeight);
		int cumulative = 0;

		foreach (var card in drawPool)
		{
			cumulative += GetWeight(card.Rarity);
			if (roll < cumulative)
				return card;
		}

		return drawPool[0];
	}
}
