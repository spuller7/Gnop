using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(EdgeCollider2D))]

public class MiddleLineRenderer : MonoBehaviour {

    enum Shape
    {
        VerticalLine, HorizontalLine, RandomLine, Circle, Square, Empty
    }

    Shape currentShape = Shape.Empty;
    Shape currentStatus = Shape.Empty;
    Shape nextShape;

    private int numTimes = 0;

    private LineRenderer line;
    EdgeCollider2D edgeCollider;
    List<Vector3> points = new List<Vector3>();
    List<Vector2> edgeColliderPoints = new List<Vector2>();
    private List<int> explosionPoints = new List<int>();

    private float percentOfScreen = 0.15f;
    private float maxRadius;

    private bool rotate = false;
    
    private Vector3 topLeft, bottomRight;

    [SerializeField]
    private GameObject explodeShapeEffect;

    [SerializeField]
    private Color disabledColor;

    [SerializeField]
    private Color enabledColor;

    private int rotationDirection = 1;

    private void Start()
    {
        line = gameObject.GetComponent<LineRenderer>();
        edgeCollider = gameObject.GetComponent<EdgeCollider2D>();
        line.useWorldSpace = false;
        line.numCapVertices = 15;
        line.startWidth = 0.1f;
        DisableLine();
        nextShape = GetNextShape();

        float screenWidth = Vector3.Distance(Camera.main.ScreenToWorldPoint(new Vector3(Screen.safeArea.xMax, 0, 1)), Camera.main.ScreenToWorldPoint(new Vector3(Screen.safeArea.x, 0, 1)));
        maxRadius = screenWidth * percentOfScreen;

        if (GameController.script.GetGameMode() == GameController.GameMode.Unfair || GameController.script.GetGameMode() == GameController.GameMode.Impossible)
            topLeft = Camera.main.ScreenToWorldPoint(new Vector3(Screen.safeArea.x + (Screen.safeArea.xMax / 4), Screen.safeArea.yMax - (Screen.safeArea.yMax / 4), 1));
        else
            topLeft = Camera.main.ScreenToWorldPoint(new Vector3(Screen.safeArea.x, Screen.safeArea.yMax, 1));

        topLeft += Vector3.right * 0.45f;
        topLeft += Vector3.down * 0.45f;

        if (GameController.script.GetGameMode() == GameController.GameMode.Unfair || GameController.script.GetGameMode() == GameController.GameMode.Impossible)
            bottomRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.safeArea.xMax - (Screen.safeArea.xMax / 4), Screen.safeArea.y + (Screen.safeArea.yMax / 4), 1));
        else
            bottomRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.safeArea.xMax, Screen.safeArea.y, 1));

        bottomRight += Vector3.left * 0.45f;
        bottomRight += Vector3.up * 0.45f;

        line.positionCount = 2;
    }

    private void Update()
    {
        if (rotate)
        {
            if (rotationDirection == 1)
            {
                transform.Rotate(0, 0, 25 * Time.deltaTime);
            }
            else
            {
                transform.Rotate(0, 0, -25 * Time.deltaTime);
            }
        }
    }

    private void DisableLine()
    {
        rotate = false;
        gameObject.transform.Rotate(0,0,0, Space.World);
        line.enabled = false;
        edgeCollider.enabled = false;
        points.Clear();
        edgeColliderPoints.Clear();
        line.positionCount = 0;

        this.gameObject.GetComponent<LineRenderer>().startColor = disabledColor;
        this.gameObject.GetComponent<LineRenderer>().endColor = disabledColor;
    }

    private void DrawLine()
    {
        line.enabled = true;
        numTimes++;
    }

    // Make line interactable with the ball
    public void EnableLine()
    {
        this.gameObject.GetComponent<LineRenderer>().startColor = enabledColor;
        this.gameObject.GetComponent<LineRenderer>().endColor = enabledColor;
        edgeCollider.enabled = true;
    }

    public bool DrawShape(bool gameEnded = false)
    {
        if (currentStatus != Shape.Empty)
        {
            DestroyShape(gameEnded);
            nextShape = GetNextShape();
            return false;
        }
        else if (GameController.script.GetGameMode() == GameController.GameMode.Impossible)
        {
            DrawLine();
            currentShape = Shape.RandomLine;
            currentStatus = Shape.RandomLine;
            StartCoroutine(DrawRandomLine());
            return true;
        }
        else if (GameController.script.GetGameMode() == GameController.GameMode.Unfair)
        {
            DrawLine();
            nextShape = Shape.RandomLine;
            currentShape = Shape.VerticalLine;
            currentStatus = Shape.VerticalLine;
            StartCoroutine(DrawRandomLine());
            return true;
        }
        else
        {
            DrawLine();

            if (nextShape == Shape.Square)
            {
                currentShape = Shape.Square;
                currentStatus = Shape.Square;
                DrawSquare();
            }
           /* else if (nextShape == Shape.Hexagon)
            {
                currentShape = Shape.Hexagon;
                currentStatus = Shape.Hexagon;
                DrawHexagon();
            }*/
            else if (nextShape == Shape.HorizontalLine)
            {
                currentShape = Shape.HorizontalLine;
                currentStatus = Shape.HorizontalLine;
                StartCoroutine(DrawHorizontalLine());
            }
            else if (nextShape == Shape.VerticalLine)
            {
                currentShape = Shape.VerticalLine;
                currentStatus = Shape.VerticalLine;
                StartCoroutine(DrawVerticalLine());
            }
            else if (nextShape == Shape.RandomLine)
            {
                currentShape = Shape.VerticalLine;
                currentStatus = Shape.VerticalLine;
                StartCoroutine(DrawRandomLine());
            }
            else
            {
                currentShape = Shape.Circle;
                currentStatus = Shape.Circle;
                DrawCircle();
            }

            return true;
        }
    }

    public void DestroyShape(bool gameEnded = false)
    {
        if (currentStatus != Shape.Empty)
        {
            if (currentStatus == Shape.HorizontalLine || currentStatus == Shape.VerticalLine)
            {
                StartCoroutine(DestroyLine());
                explosionPoints.Add(0);
            }
            else
            {
                Color curColor = this.gameObject.GetComponent<LineRenderer>().startColor;

                foreach (int v in explosionPoints)
                {
                    GameObject part = Instantiate(explodeShapeEffect);
                    part.transform.position = points[v];
                    var col = part.GetComponent<ParticleSystem>().colorOverLifetime;
                    col.color = curColor;
                }

                explosionPoints.Clear();
                points.Clear();
                DisableLine();

                currentStatus = Shape.Empty;
                transform.rotation = Quaternion.identity;
            }
        }

        if (GameController.script.GetGameMode() == GameController.GameMode.Impossible && !gameEnded)
        {
            DrawShape();
        }
    }

    public IEnumerator DestroyLine()
    {
        int lineSegments = 24;

        Vector3 center = new Vector3(0, 0, -9);

        Vector3 directionVectorLeft = (center - points[0]).normalized;
        Vector3 directionVectorRight = (center - points[1]).normalized;

        float lineRadius = Vector3.Distance(points[0], points[1]);
        float incrementDistance = lineRadius / lineSegments;

        //animated line generation
        for (int i = 0; i < lineSegments / 2; i++)
        {
            points[0] = points[0] + (directionVectorLeft * incrementDistance);
            points[1] = points[1] + (directionVectorRight * incrementDistance);

            edgeColliderPoints[0] = points[0];
            edgeColliderPoints[1] = points[1];

            line.SetPositions(points.ToArray());
            edgeCollider.points = edgeColliderPoints.ToArray();

            yield return new WaitForSeconds(0.01f);
        }

        currentStatus = Shape.Circle;
        DestroyShape();
    }

    private void DrawCircle()
    {
        StartCoroutine(DrawPolygon(24));
    }

    private void DrawTriangle()
    {
        StartCoroutine(DrawPolygon(3));
    }

    private void DrawSquare()
    {
        StartCoroutine(DrawPolygon(4));
    }

    private void DrawHexagon()
    {
        StartCoroutine(DrawPolygon(6));
    }


    private IEnumerator DrawPolygon(int numVertices)
    {
        int segments = numVertices;
        int lineSegments = 24 / numVertices;
        List<Vector3> vertices = new List<Vector3>();

        vertices.Add(new Vector3(0, maxRadius, -9));

        AddNextPosition(vertices[0]);
        AddNextPosition(vertices[0]);

        for (int i = 1; i < segments; i++)
        {
            var rad = Mathf.Deg2Rad * (i * 360f / segments);
            vertices.Add(new Vector3(Mathf.Sin(rad) * maxRadius, Mathf.Cos(rad) * maxRadius, -9));
        }

        float distance = Vector3.Distance(vertices[0], vertices[1]);
        float lineSegmentDistance = distance / lineSegments;

        for (int i = 0; i < segments; i++)
        {
            Vector3 directionVector = (vertices[(i + 1) % segments] - vertices[i]).normalized;

            for (int j = 0; j < lineSegments; j++)
            {
                if (j == lineSegments - 1)
                {
                    AddNextPosition(vertices[(i + 1) % segments]);
                }
                else
                {
                    AddNextPosition(points[points.Count - 1] + (directionVector * lineSegmentDistance));
                }

                if (i % 6 == 0)
                {
                    explosionPoints.Add(j);
                }

                yield return new WaitForSeconds(0.01f);
            }
        }

        //Rotate();
    }

    private void AddNextPosition(Vector3 nextPosition)
    {
        edgeColliderPoints.Add(new Vector2(nextPosition.x, nextPosition.y));
        points.Add(nextPosition);

        line.positionCount = points.Count;
        line.SetPosition(points.Count - 1, nextPosition);

        edgeCollider.points = edgeColliderPoints.ToArray();
        line.SetPositions(points.ToArray());
    }

    private IEnumerator DrawHorizontalLine()
    {
        int lineSegments = 24;

        Vector3 leftPoint = new Vector3(topLeft.x, 0, -9);
        Vector3 rightPoint = new Vector3(bottomRight.x, 0, -9);

        Vector3 center = new Vector3(0, 0, -9);

        Vector3 directionVectorLeft = (leftPoint - center).normalized;
        Vector3 directionVectorRight = (rightPoint - center).normalized;

        float lineRadius = Vector3.Distance(leftPoint, rightPoint);
        float incrementDistance = lineRadius / lineSegments;

        AddNextPosition(new Vector3(0,0,-9));
        AddNextPosition(new Vector3(0,0,-9));

        //animated line generation
        for (int i = 0; i < lineSegments / 2; i++)
        {
            points[0] = points[0] + (directionVectorLeft * incrementDistance);
            points[1] = points[1] + (directionVectorRight * incrementDistance);

            edgeColliderPoints[0] = points[0];
            edgeColliderPoints[1] = points[1];

            line.SetPositions(points.ToArray());
            edgeCollider.points = edgeColliderPoints.ToArray();

            yield return new WaitForSeconds(0.01f);
        }
    }

    private IEnumerator DrawVerticalLine()
    {
        int lineSegments = 24;

        Vector3 topPoint = new Vector3(0, topLeft.y, -9);
        Vector3 bottomPoint = new Vector3(0, bottomRight.y, -9);

        Vector3 center = new Vector3(0, 0, -9);

        Vector3 directionVectorTop = (topPoint - center).normalized;
        Vector3 directionVectorBottom = (bottomPoint - center).normalized;

        float lineRadius = Vector3.Distance(topPoint, bottomPoint);
        float incrementDistance = lineRadius / lineSegments;

        AddNextPosition(new Vector3(0, 0, -9));
        AddNextPosition(new Vector3(0, 0, -9));

        //animated line generation
        for (int i = 0; i < lineSegments / 2; i++)
        {
            points[0] = points[0] + (directionVectorTop * incrementDistance);
            points[1] = points[1] + (directionVectorBottom * incrementDistance);

            edgeColliderPoints[0] = points[0];
            edgeColliderPoints[1] = points[1];

            line.SetPositions(points.ToArray());
            edgeCollider.points = edgeColliderPoints.ToArray();

            yield return new WaitForSeconds(0.01f);
        }
    }

    private IEnumerator DrawRandomLine()
    {
        int lineSegments = 24;

        Vector3 topPoint = new Vector3();
        Vector3 bottomPoint = new Vector3();

        int side = Random.Range(0, 2);

        if (side == 1)
        {
            float yVal = Random.Range(topLeft.y, bottomRight.y);
            topPoint = new Vector3(topLeft.x, yVal, -9);
            bottomPoint = new Vector3(bottomRight.x, -yVal, -9);
        }
        else
        {
            float xVal = Random.Range(topLeft.x, bottomRight.x);
            topPoint = new Vector3(xVal, topLeft.y, -9);
            bottomPoint = new Vector3(-xVal, bottomRight.y, -9);
        }

        Vector3 center = new Vector3(0, 0, -9);

        Vector3 directionVectorTop = (topPoint - center).normalized;
        Vector3 directionVectorBottom = (bottomPoint - center).normalized;

        float lineRadius = Vector3.Distance(topPoint, bottomPoint);
        float incrementDistance = lineRadius / lineSegments;

        AddNextPosition(new Vector3(0, 0, -9));
        AddNextPosition(new Vector3(0, 0, -9));

        //animated line generation
        for (int i = 0; i < lineSegments / 2; i++)
        {
            points[0] = points[0] + (directionVectorTop * incrementDistance);
            points[1] = points[1] + (directionVectorBottom * incrementDistance);

            edgeColliderPoints[0] = points[0];
            edgeColliderPoints[1] = points[1];

            line.SetPositions(points.ToArray());
            edgeCollider.points = edgeColliderPoints.ToArray();

            yield return new WaitForSeconds(0.01f);
        }
    }

    private Shape GetNextShape()
    {
        if (numTimes < 5)
        {
            return (Shape)numTimes;
        }

        Shape nextShape = (Shape)Random.Range(0, 5);

        if (nextShape == currentShape)
        {
            return GetNextShape();
        }
        else
        {
            return nextShape;
        }
    }

    private void Rotate()
    {
        rotationDirection = Random.Range(0, 2);
        rotate = true;
    }
}
