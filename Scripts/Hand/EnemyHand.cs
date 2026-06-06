using Godot;
using System.Collections.Generic;

public partial class EnemyHand : Node2D
{
    [Export] public Node2D CardManager;

    const int cardWidth = 70;
    const int handYPosition = -40; // top of screen instead of 260

    float screenCenterX;

    private List<EnemyCard> cardsInHand = new();
    public IReadOnlyList<EnemyCard> CardsInHand => cardsInHand;

    public override void _Ready()
    {
        screenCenterX = GetViewportRect().Size.X / 2;
    }

    public void AddCardToHand(EnemyCard card)
    {
        if (!cardsInHand.Contains(card))
        {
            cardsInHand.Add(card);
            UpdateCardPositions();
        }
    }

    public void RemoveCardFromHand(EnemyCard card)
    {
        if (cardsInHand.Contains(card))
        {
            cardsInHand.Remove(card);
            UpdateCardPositions();
        }
    }

    // Call this from BattleManager when the enemy plays a card
    public EnemyCard TakeCard(int index = 0)
    {
        if (cardsInHand.Count == 0) return null;
        var card = cardsInHand[index];
        RemoveCardFromHand(card);
        return card;
    }

    private void UpdateCardPositions()
    {
        for (int i = 0; i < cardsInHand.Count; i++)
        {
            var newPos = new Vector2(CalculateCardX(i), handYPosition);
            cardsInHand[i].positionInHand = newPos;
            AnimateTo(cardsInHand[i], newPos);
        }
    }

    private void AnimateTo(EnemyCard card, Vector2 target)
    {
        if (!card.hasDrawnAnimPlayed)
        {
            card.hasDrawnAnimPlayed = true;
            var tween = GetTree().CreateTween();
            tween.TweenProperty(card, "position", target, 0.5f)
                 .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
        }
        else
        {
            var tween = GetTree().CreateTween();
            tween.TweenProperty(card, "position", target, 0.5f);
        }
    }

    private float CalculateCardX(int index)
    {
        if (cardsInHand.Count == 1)
            return screenCenterX - (cardWidth / 2f);

        float maxHandWidth = 500f;
        float spacing = cardWidth;
        if ((cardsInHand.Count - 1) * spacing > maxHandWidth)
            spacing = maxHandWidth / (cardsInHand.Count - 1);

        float totalWidth = (cardsInHand.Count - 1) * spacing;
        return screenCenterX - (totalWidth / 2f) + (index * spacing) - (cardWidth / 2f);
    }
}