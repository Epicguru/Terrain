
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PoolObject))]
public class GunSmoke : MonoBehaviour
{
    public PoolObject PoolObject
    {
        get
        {
            if (_po == null)
                _po = GetComponent<PoolObject>();
            return _po;
        }
    }
    private PoolObject _po;

    private struct SmokePoint
    {
        public Vector3 Position;
        public float Age;
    }

    public bool Emitting = true;
    public float FadeTime = 0.5f;
    public float LerpSpeed = 2f;

    public LineRenderer Line;
    [MyBox.PositiveValueOnly]
    public float NewPointsPerSecond = 5f;
    public float RiseSpeed = 0.3f;
    [MyBox.PositiveValueOnly]
    public int MaxPoints = 20;

    public float PerlinGlobalScale = 2f;
    public Vector3 PerlinAxisScale = Vector3.one;
    [Range(0.01f, 5f)]
    public float PerlinTimeCoefficient = 0.1f;
    [Range(0.01f, 5f)]
    public float PerlinPointCoefficient = 0.2f;

    private List<SmokePoint> points;
    private Vector3[] pointsArray;
    private float timer;
    private float alpha;
    private float alphaTimer;
    private Vector3 emitPos;
    private bool isFirstFrame = false;

    private void UponSpawn()
    {
        ClearPoints();
        isFirstFrame = true;
    }

    private void Update()
    {
        if (points == null)
            points = new List<SmokePoint>();

        emitPos = Vector3.Lerp(emitPos, transform.position, Time.deltaTime * LerpSpeed);

        if (Emitting)
        {
            alpha = 1f;
            alphaTimer = 0f;
        }
        else
        {
            ClearPoints();
            SnapEmissionPoint();
            alphaTimer += Time.deltaTime;
            alpha = 1f - Mathf.Clamp01(alphaTimer / FadeTime);
        }

        if (isFirstFrame)
        {
            SnapEmissionPoint();
            isFirstFrame = false;
        }

        timer += Time.deltaTime;
        if (NewPointsPerSecond != 0f && timer >= 1f / NewPointsPerSecond && Emitting)
        {
            timer = 0f;
            points.Add(new SmokePoint() { Position = emitPos });
            if (points.Count > MaxPoints)
                points.RemoveAt(0);
        }

        UpdatePoints();

        if (pointsArray == null || pointsArray.Length != points.Count)
            pointsArray = new Vector3[points.Count];

        for (int i = 0; i < points.Count; i++)
        {
            pointsArray[i] = points[i].Position;
        }

        if (Line.positionCount != pointsArray.Length)
            Line.positionCount = pointsArray.Length;
        Line.SetPositions(pointsArray);

        Line.material.SetFloat("_Alpha", alpha);
    }

    private void ClearPoints()
    {
        // Clear points so that when we start emitting again there isn't a strange trail from the 
        // last position to the new current position.
        if (points.Count > 0)
        {
            points.Clear();
            Line.positionCount = 0;
        }
    }

    private void SnapEmissionPoint()
    {
        emitPos = transform.position;
    }

    private void UpdatePoints()
    {
        for (int i = 0; i < points.Count; i++)
        {
            SmokePoint point = points[i];

            // Natural smoke rise.
            point.Position += Vector3.up * RiseSpeed * Time.deltaTime;

            // Perlin noise. Needs to be remapped from (0 to 1) to (-1 to 1).
            float noiseX = Mathf.PerlinNoise(point.Age * PerlinPointCoefficient, Time.time * PerlinTimeCoefficient);
            float noiseY = Mathf.PerlinNoise(point.Age * PerlinPointCoefficient + 10f, Time.time * PerlinTimeCoefficient);
            float noiseZ = Mathf.PerlinNoise(point.Age * PerlinPointCoefficient + 20f, Time.time * PerlinTimeCoefficient);
            noiseX = -1 + noiseX * 2f;
            noiseY = -1 + noiseY * 2f;
            noiseZ = -1 + noiseZ * 2f;

            Vector3 noiseVel = new Vector3(noiseX, noiseY, noiseZ);
            noiseVel = new Vector3(noiseVel.x * PerlinAxisScale.x, noiseVel.y * PerlinAxisScale.y, noiseVel.z * PerlinAxisScale.z) * PerlinGlobalScale;

            // Apply perlin noise. Simulates the random swirls and twists in smoke trails, such as from a candle, or in this case a gun.
            point.Position += noiseVel * Time.deltaTime;

            // Increase age.
            point.Age += Time.deltaTime;

            points[i] = point;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(emitPos, 0.01f);
    }
}
