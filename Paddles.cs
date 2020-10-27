using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paddles : MonoBehaviour
{
    private List<LineRenderer> lines = new List<LineRenderer>();

    [SerializeField]
    private GameObject[] paddles;

    private GameManager gameManager;

    private List<EdgeCollider2D> edgeColliders = new List<EdgeCollider2D>();
    private List<int> startingIdx = new List<int>();
    private List<int> currentIdx = new List<int>();
    private List<int> secondIdx = new List<int>();
    private List<int> lastIdx = new List<int>();
    private List<List<Vector3>> points = new List<List<Vector3>>();
    private List<List<Vector2>> colliderPoints = new List<List<Vector2>>();
    private List<Vector3> paddlePath = new List<Vector3>();
    private List<Vector3> safetyPath = new List<Vector3>();

    //This will help find the first last idx when changing directions
    private int lineIdxLength = 1;
    //guides which way to increment path array
    private int direction = 1;
    private int numOfCornerPoints;
    private int[] cornerIdx = { 0, 0, 0, 0, 0, 0, 0, 0 };
    private int numPaddles = 2;

    private bool ready = false;
    private bool pathReady = false;

    private float unitDistance;
    private float lineDistance;
    private float defaultLength = 0f;

    public void Init(GameManager gameManager, float lineWidth, float unitDistance)
    {
        this.gameManager = gameManager;
        float vert_h = Vector3.Distance(gameManager.pathVertices[0], gameManager.pathVertices[1]);
        float horz_l = Vector3.Distance(gameManager.pathVertices[2], gameManager.pathVertices[3]);
        defaultLength = (vert_h + horz_l) * 0.5f; //was 0.36
        this.unitDistance = unitDistance;

        // Assign Values for Unfair Gamemode
        if (GameController.script.GetGameMode() == GameController.GameMode.Unfair)
        {
            numPaddles = 1;
            paddles[1].SetActive(false);
            defaultLength *= 2;
        }

        //create path for paddles to follow strictly
        PopulatePaddlePath();

        SetDefaults();

        foreach (LineRenderer line in lines)
        {
            line.useWorldSpace = false;
            line.numCapVertices = 15;
            line.startWidth = lineWidth;
            line.positionCount = 1;
        }

        pathReady = true;
        gameManager.BeginGame();
    }

    private void PopulatePaddlePath()
    {
        numOfCornerPoints = getNumberOfPointsForCorners();

        for (int i = 0; i < gameManager.pathVertices.Length; ++i)
        {
            cornerIdx[i] = paddlePath.Count;
            //Draw rounded corner
            if (i % 2 != 0)
            {
                DrawRoundedCorner(i);
            }
            //Draw Straight Line
            else
            {
                Vector3 currentPosition = gameManager.pathVertices[i];
                Vector3 currentSafetyPosition = gameManager.safetyVertices[i];

                Vector3 directionVector = Vector3.Normalize(gameManager.pathVertices[i + 1] - gameManager.pathVertices[i]);

                float sideLength = Vector3.Distance(currentPosition, gameManager.pathVertices[i + 1]);

                while (Vector3.Distance(gameManager.pathVertices[i], currentPosition) <= sideLength)
                {
                    paddlePath.Add(currentPosition);
                    safetyPath.Add(currentSafetyPosition);

                    currentPosition = currentPosition + (directionVector * unitDistance);
                    currentSafetyPosition = currentSafetyPosition + (directionVector * unitDistance);
                }
            }
        }
    }

    public void MovePaddles()
    {
        if (!pathReady)
            return;


        AddNextPoint();

        if (!ready)
            RenderInitialLine();
    }

    private void RenderInitialLine()
    {
        for (int i = 0; i < numPaddles; i++)
        {
            lines[i].positionCount = points[i].Count;
            lines[i].SetPositions(points[i].ToArray());
        }

        lineDistance += unitDistance;
        lineIdxLength++;

        if (lineDistance >= defaultLength)
        {
            lastIdx[0] = startingIdx[0];
            lastIdx[1] = startingIdx[1];

            gameManager.AddBall(new Vector3(0, 0));
            ready = true;
        }
    }

    private void DrawRoundedCorner(int startingVertex)
    {
        float degDiff = 180f;

        if (startingVertex == 3)
        {
            degDiff = 90f;
        }
        else if (startingVertex == 5)
        {
            degDiff = 0f;
        }
        else if (startingVertex == 7)
        {
            degDiff = 270f;
        }

        int endingVertex = (startingVertex + 1) % gameManager.pathVertices.Length;
        Vector3 startingPoint = gameManager.pathVertices[startingVertex];
        Vector3 endingPoint = gameManager.pathVertices[endingVertex];

        float centerX = Mathf.Abs(startingPoint.x) < Mathf.Abs(endingPoint.x) ? startingPoint.x : endingPoint.x;
        float centerY = Mathf.Abs(startingPoint.y) < Mathf.Abs(endingPoint.y) ? startingPoint.y : endingPoint.y;

        Vector3 center = new Vector3(centerX, centerY, 0);
        float radius = Vector3.Distance(center, startingPoint);

        for (int i = numOfCornerPoints; i >= 0; i--)
        {
            var rad = Mathf.Deg2Rad * ((i * 90f / numOfCornerPoints) + degDiff);
            Vector3 newPoint = new Vector3((Mathf.Sin(rad) * radius) + center.x, (Mathf.Cos(rad) * radius) + center.y, 0);
            paddlePath.Add(newPoint);
        }

        //Safety Path Corner Generation
        Vector3 safetyStartingPoint = gameManager.safetyVertices[startingVertex];
        Vector3 safetyEndingPoint = gameManager.safetyVertices[endingVertex];

        float safetyCenterX = Mathf.Abs(safetyStartingPoint.x) < Mathf.Abs(safetyEndingPoint.x) ? safetyStartingPoint.x : safetyEndingPoint.x;
        float safetyCenterY = Mathf.Abs(safetyStartingPoint.y) < Mathf.Abs(safetyEndingPoint.y) ? safetyStartingPoint.y : safetyEndingPoint.y;

        Vector3 safetyCenter = new Vector3(safetyCenterX, safetyCenterY, 0);
        float safetyRadius = Vector3.Distance(safetyCenter, safetyStartingPoint);

        for (int i = numOfCornerPoints; i >= 0; i--)
        {
            var rad = Mathf.Deg2Rad * ((i * 90f / numOfCornerPoints) + degDiff);
            Vector3 newPoint = new Vector3((Mathf.Sin(rad) * safetyRadius) + safetyCenter.x, (Mathf.Cos(rad) * radius) + safetyCenter.y, 0);
            safetyPath.Add(newPoint);
        }
    }

    private void AddNextPoint()
    {
        for (int i = 0; i < numPaddles; i++)
        {
            // Calculate the next point for each paddle
            secondIdx[i] = currentIdx[i];

            currentIdx[i] = (currentIdx[i] + direction) % paddlePath.Count;
            currentIdx[i] = currentIdx[i] < 0 ? paddlePath.Count - 1 : currentIdx[i];

            lastIdx[i] = (lastIdx[i] + direction) % paddlePath.Count;
            lastIdx[i] = lastIdx[i] < 0 ? paddlePath.Count - 1 : lastIdx[i];
            
            points[i].Add(paddlePath[currentIdx[i]]);
            colliderPoints[i].Add(safetyPath[currentIdx[i]]);
            colliderPoints[i][colliderPoints[i].Count - 2] = paddlePath[secondIdx[i]];

            // Remove the last point of the paddle
            if (ready)
            {
                points[i].RemoveAt(0);
                colliderPoints[i].RemoveAt(0);
                colliderPoints[i][0] = safetyPath[lastIdx[i]];
            }

            // Render the new line
            edgeColliders[i].points = colliderPoints[i].ToArray();
            lines[i].SetPositions(points[i].ToArray());
        }
    }

    public void ChangeDirection()
    {
        direction = direction * -1;
        for (int i = 0; i < numPaddles; i++)
        {
            int tmp = currentIdx[i];
            currentIdx[i] = lastIdx[i];
            lastIdx[i] = tmp;

            points[i].Reverse();
            colliderPoints[i].Reverse();
        }
    }

    private int getNumberOfPointsForCorners()
    {
        Vector3 startingPoint = gameManager.pathVertices[1];
        Vector3 endingPoint = gameManager.pathVertices[2];
        Vector3 centerPoint = new Vector3(endingPoint.x, startingPoint.y, 0);

        float radius = Vector3.Distance(centerPoint, endingPoint);

        float distance = Mathf.PI * radius / 2;
        return (int)(distance / unitDistance);
    }

    public void SetDefaults()
    {
        direction = 1;
        lineDistance = 0f;
        lineIdxLength = 1;
        ready = false;

        lines = new List<LineRenderer>();
        edgeColliders = new List<EdgeCollider2D>();
        points = new List<List<Vector3>>();
        colliderPoints = new List<List<Vector2>>();
        startingIdx = new List<int>();
        currentIdx = new List<int>();
        secondIdx = new List<int>();
        lastIdx = new List<int>();

        for (int i = 0; i < paddles.Length; i++)
        {
            edgeColliders.Add(paddles[i].GetComponentInChildren<EdgeCollider2D>());
            lines.Add(paddles[i].gameObject.GetComponent<LineRenderer>());
            currentIdx.Add(cornerIdx[(i + 4) % 4 == 0 ? 0 : 4]);
            startingIdx.Add(currentIdx[i]);
            lastIdx.Add(0);
            secondIdx.Add(0);

            points.Add(new List<Vector3>() { paddlePath[currentIdx[i]] });
            colliderPoints.Add(new List<Vector2> { paddlePath[currentIdx[i]] });
        }
    }

    public int GetDirection()
    {
        return direction;
    }
}