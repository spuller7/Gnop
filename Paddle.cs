using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(Rigidbody2D))]

public class Paddle : MonoBehaviour
{

    LineRenderer line;

    [SerializeField]
    private EdgeCollider2D edgeCollider;

    private GameManager gameManager;

    //private Vector3 originPoint;
    //private Vector3 currentPoint;
    private int currentIdx = 0;
    private int lastIdx = 0;

    //This will help find the first last idx when changing directions
    private int lineIdxLength = 1;
    
    private bool ready = false;

    //guides which way to increment path array
    private int direction = 1;

    private List<Vector3> paddlePath;
    private int[] cornerIdx = { 0, 0, 0, 0, 0, 0, 0, 0 };

    private List<Vector3> points;
    private List<Vector2> colliderPoints = new List<Vector2>();
    
    private float speed = 0;

    [SerializeField]
    private float pointSeperation = 60;
   
    private float unitDistance;
    private int numOfCornerPoints;
    private bool destroyingLine = false;

    public void Init(int startingCorner, GameManager gameManager, float lineWidth)
    {
        this.gameManager = gameManager;

        paddlePath = new List<Vector3>();
        points = new List<Vector3>();

        line = gameObject.GetComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.numCapVertices = 15;
        line.startWidth = lineWidth;

        //create path for paddles to follow strictly
        PopulatePaddlePath();

        gameManager.SetUnitDistance(unitDistance);

        line.positionCount = 1;
        currentIdx = cornerIdx[startingCorner];
        points.Add(paddlePath[currentIdx]);

        StartCoroutine(RenderInitialLine());
    }

    private void Update()
    {
        if (ready)
        {
            edgeCollider.points = colliderPoints.ToArray();
            line.SetPositions(points.ToArray());
            //speed = gameManager.GetSpeed();
        }

        if (destroyingLine)
        {
            if (points.Count <= 2)
            {
                gameManager.ContinueGame();
                Destroy(gameObject);
            }
        }
    }

    private void PopulatePaddlePath()
    {
        float distance = Vector3.Distance(gameManager.pathVertices[0], gameManager.pathVertices[1]);
        unitDistance = distance / pointSeperation;

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
                Vector3 directionVector = Vector3.Normalize(gameManager.pathVertices[i + 1] - gameManager.pathVertices[i]);

                float sideLength = Vector3.Distance(currentPosition, gameManager.pathVertices[i + 1]);

                while (Vector3.Distance(gameManager.pathVertices[i], currentPosition) <= sideLength)
                {
                    paddlePath.Add(currentPosition);
                    currentPosition = currentPosition + (directionVector * unitDistance);
                }
            }
        }
    }

    private IEnumerator DrawLine()
    {
        while (ready)
        {
            if (speed >= 1)
            {
                int nextIdx = GetNextIdx();

                points.RemoveAt(0);
                colliderPoints.RemoveAt(0);

                if (!destroyingLine)
                {
                    points.Add(paddlePath[nextIdx]);
                    colliderPoints.Add(paddlePath[nextIdx]);
                }

                yield return new WaitForSeconds(1 / (1000f * speed));
            }
            else
            {
                yield return new WaitForFixedUpdate();
            }
        }
    }

    private IEnumerator RenderInitialLine()
    {
        float lineDistance = 0f;
        int startingIndex = currentIdx;
        float defaultLength = Vector3.Distance(gameManager.pathVertices[0], gameManager.pathVertices[1]) * 0.55f;

        while (lineDistance < defaultLength)
        {
            int nextIdx = GetNextIdx();
            points.Add(paddlePath[nextIdx]);
            colliderPoints.Add(paddlePath[nextIdx]);
            line.positionCount = points.Count;
            line.positionCount = points.Count;
            line.SetPositions(points.ToArray());
            lineDistance += unitDistance;
            lineIdxLength++;

            yield return new WaitForSeconds(1 / 100f);
        }
        
        ready = true;
        lastIdx = startingIndex;
        gameManager.BeginGame();
        //StartCoroutine(DrawLine());
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
    }

    private int GetNextIdx()
    {
        currentIdx = currentIdx + direction;

        if (currentIdx == paddlePath.Count)
        {
            currentIdx = 0;
        }
        else if (currentIdx < 0)
        {
            currentIdx = paddlePath.Count - 1;
        }

        lastIdx = lastIdx + direction;
        if (lastIdx == paddlePath.Count)
        {
            lastIdx = 0;
        }
        else if (lastIdx < 0)
        {
            lastIdx = paddlePath.Count - 1;
        }

        return currentIdx;
    }

    public void AddPaddlePoint()
    {
        int nextIdx = GetNextIdx();

        points.RemoveAt(0);
        colliderPoints.RemoveAt(0);
        points.Add(paddlePath[nextIdx]);
        colliderPoints.Add(paddlePath[nextIdx]);
    }

    public void ChangeDirection(int dir)
    {
        direction = dir;

        int tmp = currentIdx;
        currentIdx = lastIdx;
        lastIdx = tmp;

        points.Reverse();
        colliderPoints.Reverse();
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

    public void DestroyLine()
    {
        destroyingLine = true;
    }
}