using UnityEngine;

public class RansacExample : MonoBehaviour
{
    public Vector3[] objectPoints_P;
    public Vector3[] objectPoints_Q;

    public void X()
    {
        // Example 3D points P and Q
        Vector3 pointP = new Vector3(1f, 2f, 3f);
        Vector3 pointQ = new Vector3(4f, 5f, 6f);

        // Calculate the translation vector
        Vector3 translation = pointQ - pointP;

        // Normalize the translation vector
        translation.Normalize();

        // Calculate the rotation quaternion
        Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, translation);

        // Create a transformation matrix from the rotation and translation
        Matrix4x4 transformationMatrix = Matrix4x4.TRS(translation, rotation, Vector3.one);

        // Print the transformation matrix
        Debug.Log("Transformation Matrix:\n" + transformationMatrix);
    }
    void Start()
    {
        int maxIterations = 1000;  // You may adjust this based on your scenario
        float inlierThreshold = 0.1f;  // Adjust based on your tolerance for outliers
        int minInliers = objectPoints_P.Length / 2;  // At least half of the points should be inliers

        Matrix4x4 bestTransformation = Matrix4x4.identity;
        int bestInliersCount = 0;

        for (int iteration = 0; iteration < maxIterations; iteration++)
        {
            // Randomly select three corresponding points
            Vector3[] randomPoints_P, randomPoints_Q;
            GetRandomCorrespondences(objectPoints_P, objectPoints_Q, out randomPoints_P, out randomPoints_Q);

            // Calculate the initial transformation (rotation and translation)
            Matrix4x4 transformation = EstimateTransformation(randomPoints_P, randomPoints_Q);

            // Apply the transformation to all points in objectPoints_P
            Vector3[] transformedPoints_P = ApplyTransformation(objectPoints_P, transformation);

            // Count inliers by checking the distance between transformedPoints_P and objectPoints_Q
            int inliersCount = CountInliers(transformedPoints_P, objectPoints_Q, inlierThreshold);

            // Update best transformation if the current one has more inliers
            if (inliersCount > bestInliersCount && inliersCount >= minInliers)
            {
                bestInliersCount = inliersCount;
                bestTransformation = transformation;
            }
        }

        // bestTransformation now contains the estimated rotation and translation
        Debug.Log("Best Transformation Matrix:\n" + bestTransformation);
    }

    void GetRandomCorrespondences(Vector3[] points_P, Vector3[] points_Q, out Vector3[] randomPoints_P, out Vector3[] randomPoints_Q)
    {
        // Randomly select three corresponding points
        randomPoints_P = new Vector3[3];
        randomPoints_Q = new Vector3[3];

        for (int i = 0; i < 3; i++)
        {
            int index = Random.Range(0, points_P.Length);
            randomPoints_P[i] = points_P[index];
            randomPoints_Q[i] = points_Q[index];
        }
    }

    Matrix4x4 EstimateTransformation(Vector3[] points_P, Vector3[] points_Q)
    {
        // Estimate rotation using Quaternion.LookRotation
        Quaternion rotation = Quaternion.LookRotation(AverageDirection(points_P), AverageDirection(points_Q));

        // Estimate translation using Vector3.Lerp
        Vector3 translation = Vector3.Lerp(CalculateCentroid(points_P), CalculateCentroid(points_Q), 0.5f);

        // Create the transformation matrix using TRS
        Matrix4x4 transformation = Matrix4x4.TRS(translation, rotation, Vector3.one);

        return transformation;
    }

    Vector3 AverageDirection(Vector3[] points)
    {
        Vector3 averageDirection = Vector3.zero;

        foreach (Vector3 point in points)
        {
            averageDirection += point.normalized;
        }

        return averageDirection.normalized;
    }

    Vector3 CalculateCentroid(Vector3[] points)
    {
        Vector3 centroid = Vector3.zero;

        foreach (Vector3 point in points)
        {
            centroid += point;
        }

        return centroid / points.Length;
    }

    Vector3[] ApplyTransformation(Vector3[] points, Matrix4x4 transformation)
    {
        Vector3[] transformedPoints = new Vector3[points.Length];

        for (int i = 0; i < points.Length; i++)
        {
            transformedPoints[i] = transformation.MultiplyPoint3x4(points[i]);
        }

        return transformedPoints;
    }

    int CountInliers(Vector3[] points_P, Vector3[] points_Q, float threshold)
    {
        int inliersCount = 0;

        for (int i = 0; i < points_P.Length; i++)
        {
            float distance = Vector3.Distance(points_P[i], points_Q[i]);

            if (distance < threshold)
            {
                inliersCount++;
            }
        }

        return inliersCount;
    }
}
