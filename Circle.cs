using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(EdgeCollider2D))]

public class Circle : MonoBehaviour {

    private LineRenderer line;
    private EdgeCollider2D edgeCollider;

    private List<Vector3> points;
    private List<Vector2> edgeColliderPoints;
    private List<Vector2> innerCircleEdgeColliderPoints;

    private float radius;

    [SerializeField]
    private int segments = 360;

    [SerializeField]
    private float circleSpeed = 0.2f;

    [SerializeField]
    private EdgeCollider2D innerCircleEdgeCollider;

    [SerializeField]
    private GameManager gm;

    private int currentCircleSegment = 0;
    private int segmentLength = 0;
    private int desiredSegmentLength = 0;
    private bool isSpinning = true;
    private int betweenCircles = 0;

    void Start ()
    {
        points = new List<Vector3>();
        edgeColliderPoints = new List<Vector2>();
        innerCircleEdgeColliderPoints = new List<Vector2>();

        float screenWidth = Vector3.Distance(Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, 1)), Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 1)));
        radius = screenWidth / 5;

        line = gameObject.GetComponent<LineRenderer>();
        edgeCollider = gameObject.GetComponent<EdgeCollider2D>();

        line.useWorldSpace = false;
        line.startWidth = 0.15f;

        line.SetPosition(0, new Vector3(Mathf.Sin(0 * 360f / segments) * radius, Mathf.Cos(0 * 360f / segments) * radius, -9));
        var rad = Mathf.Deg2Rad * (1 * 360f / segments);
        line.SetPosition(1, new Vector3(Mathf.Sin(rad) * radius, Mathf.Cos(rad) * (radius), -9));
        DrawInnerCircleCollider(0.2f);
        StartCoroutine(IncreaseCircleSize(0.2f, true));
    }
	
	void Update ()
    {
        edgeCollider.points = edgeColliderPoints.ToArray();
        innerCircleEdgeCollider.points = innerCircleEdgeColliderPoints.ToArray();
    }

    private void DrawInnerCircleCollider(float percentOfCircle)
    {
        int pointCount = segments + 1;

        for (int i = 0; i < pointCount; i++)
        {
            var rad = Mathf.Deg2Rad * (i * 360f / segments);
            Vector3 nextPosition = new Vector3(Mathf.Sin(rad) * radius, Mathf.Cos(rad) * (radius), -9);
            innerCircleEdgeColliderPoints.Add(new Vector2(nextPosition.x, nextPosition.y));
        }
    }

    public IEnumerator IncreaseCircleSize(float percentOfCircle, bool constructor)
    {
        int numOfSegments = (int)Mathf.Floor(segments * percentOfCircle);
        if (numOfSegments > segments)
        {
            numOfSegments = segments;
        }
        
        for (int i = 2; i < numOfSegments; i++)
        {
            var rad = Mathf.Deg2Rad * (i * 360f / segments);
            Vector3 nextPosition = new Vector3(Mathf.Sin(rad) * radius, Mathf.Cos(rad) * radius, -9);
            edgeColliderPoints.Add(new Vector2(nextPosition.x, nextPosition.y));
            points.Add(nextPosition);
            innerCircleEdgeColliderPoints.RemoveAt(0);
            yield return null;
            line.positionCount = points.Count;
            line.SetPosition(points.Count - 1, nextPosition);
            if(constructor)
            {
                currentCircleSegment++;
            }
        }

        if(constructor)
        {
            segmentLength = currentCircleSegment;
            desiredSegmentLength = currentCircleSegment;
            StartCoroutine(SpinCircle());
        }
    }

    public IEnumerator SpinCircle()
    {
        while(isSpinning)
        {
            Vector2 firstCollisionPoint = edgeColliderPoints[0];
            innerCircleEdgeColliderPoints.RemoveAt(0);
            if (desiredSegmentLength <= segmentLength)
            {
                points.RemoveAt(0);
                edgeColliderPoints.RemoveAt(0);
                
                innerCircleEdgeColliderPoints.Add(firstCollisionPoint);
            }
            else
            {
                segmentLength++;
            }
            var rad = Mathf.Deg2Rad * (currentCircleSegment * 360f / segments);
            Vector3 nextPosition = new Vector3(Mathf.Sin(rad) * radius, Mathf.Cos(rad) * radius, -9);
            
            yield return new WaitForSeconds(circleSpeed);

            //only add a new point if desired length is or above the current length
            if (desiredSegmentLength >= segmentLength)
            {
                edgeColliderPoints.Add(new Vector2(nextPosition.x, nextPosition.y));
                points.Add(nextPosition);
                currentCircleSegment++;
            }
            else
            {
                segmentLength--;
            }

            line.positionCount = points.Count;
            line.SetPositions(points.ToArray());

            if (segmentLength == 0)
            {
                isSpinning = false;
            }
        }
    }

    public void increaseCircleLength()
    {
        if (isSpinning)
        {
            int potentialDesiredSegementLength = desiredSegmentLength + 10;
            if (potentialDesiredSegementLength > segments)
            {
                potentialDesiredSegementLength = desiredSegmentLength + (segments - desiredSegmentLength);
            }

            if (desiredSegmentLength == segments)
            {
                isSpinning = false;
                this.gameObject.SetActive(false);
            }
            desiredSegmentLength = potentialDesiredSegementLength;
        } 
        else
        {
            betweenCircles++;
        }
    }

    public void resetCircleLength(float percentOfCircle)
    {
        desiredSegmentLength = (int)Mathf.Floor(segments * percentOfCircle);
    }

    public void removeCircle()
    {
        desiredSegmentLength = 0;
    }
}
