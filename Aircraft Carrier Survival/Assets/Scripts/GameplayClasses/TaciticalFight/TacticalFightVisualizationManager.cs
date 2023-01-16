using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class TacticalFightVisualizationManager : MonoBehaviour
{
    public static TacticalFightVisualizationManager Instance;
    public float OnHoverMovementSpeed = 0.33f;
    public float OnAbillityUseMovementSpeed = 0.33f;
    private void Awake()
    {
        Instance = this;
    }

    public List<Sprite> FieldVisualizationsWhenIsClouded;
    public Sprite FieldOnHighLight;
    public Sprite FieldVisualizationAttackedByPlayer;
    public Sprite FieldVisualizationAttackcedByEnemy;
    public Sprite FieldVisualizationWhenEnemyMarkedOnIt;

    public Sprite DiagonalLeftLineFlightVisualizationNotBlocked;
    public Sprite DiagonalLeftLineFlightVisualizationBlocked;
    public Sprite DiagonalRightLineFlightVisualizationNotBlocked;
    public Sprite DiagonalRightLineFlightVisualizationBlocked;
    public Sprite VerticalLineFlightVisualizationNotBlocked;
    public Sprite VerticalLineFlightVisualizationBlocked;

    public Sprite CursorUpOnBlockedMovement;
    public Sprite CursorUpOnNotBlockedMovement;

    public Sprite CursorUpRightpOnBlockedMovement;
    public Sprite CursorUpRightOnNotBlockedMovement;

    public Sprite CursorDownRightpOnBlockedMovement;
    public Sprite CursorDownRightOnNotBlockedMovement;

    public Sprite CursorDownOnBlockedMovement;
    public Sprite CursorDownOnNotBlockedMovement;

    public Sprite CursorDownLeftOnBlockedMovement;
    public Sprite CursorDownLeftOnNotBlockedMovement;

    public Sprite CursorUpLeftOnBlockedMovement;
    public Sprite CursorUpLeftOnNotBlockedMovement;

    public Image FlightPathVisualizer;
    public Image FlightPathVisualizerCursor;

    public List<Sprite> DevastatorButtonSpriteVisualization;
    public List<Sprite> HelldiverButtonSpriteVisualization;
    public List<Sprite> WildcatButtonSpriteVisualization;

    public Sprite FullHealthCell;
    public Sprite EmptyHealthCell;

    public Sprite GetRandomCloudSprite()
    {
        return FieldVisualizationsWhenIsClouded[Random.Range(0, FieldVisualizationsWhenIsClouded.Count - 1)];
    }

    public void SetFlightVisualization(TacticalFightMapField mapFieldOnEnd, ETacticalFightUnitRotationState chosenRotation, bool isBlockedPath)
    {
        FlightPathVisualizer.gameObject.SetActive(true);

        switch (chosenRotation)
        {
            case (ETacticalFightUnitRotationState.Up):
                FlightPathVisualizer.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0);
                FlightPathVisualizerCursor.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0);

                if (isBlockedPath)
                {
                    FlightPathVisualizer.sprite = VerticalLineFlightVisualizationBlocked;
                    FlightPathVisualizerCursor.sprite = CursorDownOnBlockedMovement;
                }
                else
                {
                    FlightPathVisualizer.sprite = VerticalLineFlightVisualizationNotBlocked;
                    FlightPathVisualizerCursor.sprite = CursorDownOnNotBlockedMovement;
                }

                FlightPathVisualizer.GetComponent<RectTransform>().sizeDelta = new Vector2(4, 462);

                FlightPathVisualizerCursor.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -256);
                break;

            case (ETacticalFightUnitRotationState.RightUp):
                FlightPathVisualizer.GetComponent<RectTransform>().pivot = new Vector2(0, 0);
                FlightPathVisualizerCursor.GetComponent<RectTransform>().pivot = new Vector2(0, 0);

                if (isBlockedPath)
                {
                    FlightPathVisualizer.sprite = DiagonalRightLineFlightVisualizationBlocked;
                    FlightPathVisualizerCursor.sprite = CursorDownLeftOnBlockedMovement;
                }
                else
                {
                    FlightPathVisualizer.sprite = DiagonalRightLineFlightVisualizationNotBlocked;
                    FlightPathVisualizerCursor.sprite = CursorDownLeftOnNotBlockedMovement;
                }

                FlightPathVisualizer.GetComponent<RectTransform>().sizeDelta = new Vector2(842, 400);

                FlightPathVisualizerCursor.GetComponent<RectTransform>().anchoredPosition = new Vector2(-446,-215);
                break;

            case (ETacticalFightUnitRotationState.RightDown):
                FlightPathVisualizer.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
                FlightPathVisualizerCursor.GetComponent<RectTransform>().pivot = new Vector2(0, 1);

                if (isBlockedPath)
                {
                    FlightPathVisualizer.sprite = DiagonalLeftLineFlightVisualizationBlocked;
                    FlightPathVisualizerCursor.sprite = CursorUpLeftOnBlockedMovement;
                }
                else
                {
                    FlightPathVisualizer.sprite = DiagonalLeftLineFlightVisualizationNotBlocked;
                    FlightPathVisualizerCursor.sprite = CursorUpLeftOnNotBlockedMovement;
                }

                FlightPathVisualizer.GetComponent<RectTransform>().sizeDelta = new Vector2(842, 400);
                FlightPathVisualizerCursor.GetComponent<RectTransform>().anchoredPosition = new Vector2(-446, 215);
                break;

            case (ETacticalFightUnitRotationState.Down):
                FlightPathVisualizer.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1);
                FlightPathVisualizerCursor.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1);

                if (isBlockedPath)
                {
                    FlightPathVisualizer.sprite = VerticalLineFlightVisualizationBlocked;
                    FlightPathVisualizerCursor.sprite = CursorUpOnBlockedMovement;
                }
                else
                {
                    FlightPathVisualizer.sprite = VerticalLineFlightVisualizationNotBlocked;
                    FlightPathVisualizerCursor.sprite = CursorUpOnNotBlockedMovement;
                }

                FlightPathVisualizer.GetComponent<RectTransform>().sizeDelta = new Vector2(4, 462);
                FlightPathVisualizerCursor.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 256);

                break;

            case (ETacticalFightUnitRotationState.LeftDown):
                FlightPathVisualizer.GetComponent<RectTransform>().pivot = new Vector2(1, 1);
                FlightPathVisualizerCursor.GetComponent<RectTransform>().pivot = new Vector2(1, 1);

                if (isBlockedPath)
                {
                    FlightPathVisualizer.sprite = DiagonalRightLineFlightVisualizationBlocked;
                    FlightPathVisualizerCursor.sprite = CursorUpRightpOnBlockedMovement;
                }
                else
                {
                    FlightPathVisualizer.sprite = DiagonalRightLineFlightVisualizationNotBlocked;
                    FlightPathVisualizerCursor.sprite = CursorUpRightOnNotBlockedMovement;
                }

                FlightPathVisualizer.GetComponent<RectTransform>().sizeDelta = new Vector2(842,400);
                FlightPathVisualizerCursor.GetComponent<RectTransform>().anchoredPosition = new Vector2(446, 215);

                break;

            case (ETacticalFightUnitRotationState.LeftUp):
                FlightPathVisualizer.GetComponent<RectTransform>().pivot = new Vector2(1, 0);
                FlightPathVisualizerCursor.GetComponent<RectTransform>().pivot = new Vector2(1, 0);

                if (isBlockedPath)
                {
                    FlightPathVisualizer.sprite = DiagonalLeftLineFlightVisualizationBlocked;
                    FlightPathVisualizerCursor.sprite = CursorDownRightpOnBlockedMovement;
                }
                else
                {
                    FlightPathVisualizer.sprite = DiagonalLeftLineFlightVisualizationNotBlocked;
                    FlightPathVisualizerCursor.sprite = CursorDownRightOnNotBlockedMovement;
                }

                FlightPathVisualizer.GetComponent<RectTransform>().sizeDelta = new Vector2(842, 400);
                FlightPathVisualizerCursor.GetComponent<RectTransform>().anchoredPosition = new Vector2(446, -215);

                break;
        }

        FlightPathVisualizer.GetComponent<RectTransform>().anchoredPosition = mapFieldOnEnd.GetComponent<RectTransform>().anchoredPosition;
 
    }

    public void UnSetFlightVisualization()
    {
        FlightPathVisualizer.gameObject.SetActive(false);
    }
}
