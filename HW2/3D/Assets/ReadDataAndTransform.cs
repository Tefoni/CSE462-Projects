using UnityEngine;
using System.IO;
using System.Collections;
using System.Drawing;
using System.Net;
using Color = UnityEngine.Color;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using MathNet.Numerics.LinearAlgebra;
using Accord.Math;
using System;
using Random = UnityEngine.Random;
using UnityEngine.UI;

public class ReadData : MonoBehaviour
{
    // Define the file path
    public string filePathP = "Assets/P_Points.txt";
    public string filePathQ = "Assets/Q_Points.txt";
    public Button changeVersionButton;
    private Accord.Math.Vector3[] objectPoints_P;
    private Accord.Math.Vector3[] objectPoints_Q;
    private Accord.Math.Vector3[] objectPoints_Transformation;
    public Accord.Math.Matrix3x3 rotation;
    public Accord.Math.Vector3 translation;
    private float scale;

    private bool isFinished = true;
    private float animationDuration = 5f;
    [SerializeField] private GameObject prefab;
    List<LineRenderer> lineRenderers = new List<LineRenderer>();
    List<GameObject> createdObjects = new List<GameObject>();
    public bool Rigid_transformation_up_to_a_global_scale = false;

    public void Start()
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        prefab = sphere;
        changeVersionButton.onClick.AddListener(ChangeVersion);
    }

    public void Update()
    {
    }
    public void ChangeVersion()
    {
        if(Rigid_transformation_up_to_a_global_scale)
        {
            Rigid_transformation_up_to_a_global_scale = false;
        }
        else
        {
            Rigid_transformation_up_to_a_global_scale= true;
        }
    }
    public void ButtonPressed()
    {
        if (isFinished)
        {
            isFinished = false;
            // Read object points from the  file
            objectPoints_P = ReadObjectPoints(filePathP);
            objectPoints_Q = ReadObjectPoints(filePathQ);
            // Apply your rotation and translation to get transformed points

            // For demonstration purposes, let's assume a simple transformation
            //rotation = Quaternion.Euler(0f, 0f, 90f);  // 45 degrees rotation around y-axis
            //translation = new UnityEngine.Vector3(0f, 0f, 0f);  // Translation along x-axis
            RigidTransform(objectPoints_P, objectPoints_Q);


            objectPoints_Transformation = ApplyTransformation(objectPoints_P,rotation,translation);

            // Visualize the original and transformed objects
            VisualizeObject(objectPoints_P, Color.blue);
            VisualizeObject(objectPoints_Q, Color.red);

            // Ensure the final position is set to the transformed state
            StartCoroutine(AnimateObject(objectPoints_P, objectPoints_Transformation, animationDuration, Color.black));
        }
        else
        {
            Debug.LogWarning("Wait for the transformation to finish");
        }

    }

    // Function to read object points from a file
    private Accord.Math.Vector3[] ReadObjectPoints(string path)
    {
        string[] lines = File.ReadAllLines(path);

        // Skip the first line (num_pts)
        int numPoints = int.Parse(lines[0]);
        Accord.Math.Vector3[] points = new Accord.Math.Vector3[numPoints];

        for (int i = 0; i < numPoints; i++)
        {
            string[] coordinates = lines[i + 1].Split(' ');
            float x = float.Parse(coordinates[0]);
            float y = float.Parse(coordinates[1]);
            float z = float.Parse(coordinates[2]);

            points[i] = new Accord.Math.Vector3(x, y, z);
        }

        return points;
    }

    // Function to calculate the rigid transformation between two sets of points
    private void RigidTransform(Accord.Math.Vector3[] points_P, Accord.Math.Vector3[] points_Q)
    {
        // Check if the input arrays are valid
        if (points_P.Length < 3 || points_Q.Length < 3)
        {
            Debug.LogError("Insufficient points for rigid transformation.");
            rotation = Accord.Math.Matrix3x3.Identity;
            translation = new Accord.Math.Vector3();
            return;
        }

        Accord.Math.Matrix3x3 bestRotation = Accord.Math.Matrix3x3.Identity;
        Accord.Math.Vector3 bestTranslation = new Accord.Math.Vector3(); ;
        UnityEngine.Matrix4x4 bestTransformation = UnityEngine.Matrix4x4.identity;
        float bestScale = 1.0f;

        int maxIterations = 10000;
        int bestInliers = 0;
        float minDistance = float.MaxValue;


        for (int i = 0; i < maxIterations; i++)
        {
            // Randomly select three corresponding points
            int indexPoint1_P = Random.Range(0, points_P.Length);
            int indexPoint2_P = Random.Range(0, points_P.Length);
            int indexPoint3_P = Random.Range(0, points_P.Length);

            int indexPoint1_Q = Random.Range(0, points_Q.Length);
            int indexPoint2_Q = Random.Range(0, points_Q.Length);
            int indexPoint3_Q = Random.Range(0, points_Q.Length);
            Accord.Math.Vector3 randomPoint1_P = points_P[indexPoint1_P];
            Accord.Math.Vector3 randomPoint2_P = points_P[indexPoint2_P];
            Accord.Math.Vector3 randomPoint3_P = points_P[indexPoint3_P];

            Accord.Math.Vector3 randomPoint1_Q = points_Q[indexPoint1_Q];
            Accord.Math.Vector3 randomPoint2_Q = points_Q[indexPoint2_Q];
            Accord.Math.Vector3 randomPoint3_Q = points_Q[indexPoint3_Q];


            // Calculate transformation matrix for each iteration
            Accord.Math.Vector3 iteration_translation = new Accord.Math.Vector3();
            Accord.Math.Matrix3x3 iteration_rotation = new Accord.Math.Matrix3x3();
            float iteration_scale;

            Accord.Math.Vector3 firstCentroid = new Accord.Math.Vector3();
            Accord.Math.Vector3 secondCentroid = new Accord.Math.Vector3();

            // Calculate first centroid
            firstCentroid.X = (randomPoint1_P.X + randomPoint2_P.X + randomPoint3_P.X) / 3;
            firstCentroid.Y = (randomPoint1_P.Y + randomPoint2_P.Y + randomPoint3_P.Y) / 3;
            firstCentroid.Z = (randomPoint1_P.Z + randomPoint2_P.Z + randomPoint3_P.Z) / 3;

            //Calculate second centroid
            secondCentroid.X = (randomPoint1_Q.X + randomPoint2_Q.X + randomPoint3_Q.X) / 3;
            secondCentroid.Y = (randomPoint1_Q.Y + randomPoint2_Q.Y + randomPoint3_Q.Y) / 3;
            secondCentroid.Z = (randomPoint1_Q.Z + randomPoint2_Q.Z + randomPoint3_Q.Z) / 3;

            // X matrix ( value - centroidValue)
            Accord.Math.Vector3 point1_P_related = new Accord.Math.Vector3((randomPoint1_P.X - firstCentroid.X), (randomPoint1_P.Y - firstCentroid.Y), (randomPoint1_P.Z - firstCentroid.Z) );
            Accord.Math.Vector3 point2_P_related = new Accord.Math.Vector3((randomPoint2_P.X - firstCentroid.X), (randomPoint2_P.Y - firstCentroid.Y), (randomPoint2_P.Z - firstCentroid.Z));
            Accord.Math.Vector3 point3_P_related = new Accord.Math.Vector3((randomPoint3_P.X - firstCentroid.X), (randomPoint3_P.Y - firstCentroid.Y), (randomPoint3_P.Z - firstCentroid.Z));
            Accord.Math.Matrix3x3 XMatrix = Accord.Math.Matrix3x3.CreateFromRows(point1_P_related, point2_P_related, point3_P_related);

            // Y matrix (value - centroidValue)
            Accord.Math.Vector3 point1_Q_related = new Accord.Math.Vector3((randomPoint1_Q.X - secondCentroid.X), (randomPoint1_Q.Y - secondCentroid.Y), (randomPoint1_Q.Z - secondCentroid.Z));
            Accord.Math.Vector3 point2_Q_related = new Accord.Math.Vector3((randomPoint2_Q.X - secondCentroid.X), (randomPoint2_Q.Y - secondCentroid.Y), (randomPoint2_Q.Z - secondCentroid.Z));
            Accord.Math.Vector3 point3_Q_related = new Accord.Math.Vector3((randomPoint3_Q.X - secondCentroid.X), (randomPoint3_Q.Y - secondCentroid.Y), (randomPoint3_Q.Z - secondCentroid.Z));
            Accord.Math.Matrix3x3 YMatrix = Accord.Math.Matrix3x3.CreateFromRows(point1_Q_related, point2_Q_related, point3_Q_related);


            // H is the familiar covariance matrix H = X*Y_Transpose
            Accord.Math.Matrix3x3 H = new Accord.Math.Matrix3x3(); 
            H = XMatrix*YMatrix.Transpose();
            H.SVD(out Accord.Math.Matrix3x3 U, out Accord.Math.Vector3 S, out Accord.Math.Matrix3x3 V);
            Accord.Math.Matrix3x3 R = new Accord.Math.Matrix3x3();
            R = V * U.Transpose();

            if (R.Determinant < 0)
            {
                R.SVD(out Accord.Math.Matrix3x3 newU, out Accord.Math.Vector3 newS, out Accord.Math.Matrix3x3 newV);
                newV.V20 *= -1;
                newV.V21 *= -1;
                newV.V22 *= -1;
                R = newV * newU.Transpose();
            }

            iteration_scale = (S.X + S.Y + S.Z) / 3f;
            iteration_translation = secondCentroid - R * firstCentroid;
            iteration_rotation = R;

            Accord.Math.Vector3[] transformedPoints = ApplyTransformation(points_P, iteration_rotation, iteration_translation);
            float distance = CountInliers(points_Q, transformedPoints);
            if(distance < minDistance)
            {
                minDistance = distance;
                bestScale = iteration_scale;
                bestTranslation = iteration_translation;
                bestRotation = iteration_rotation;
            }
        }
        scale = bestScale;
        translation = bestTranslation;
        rotation = bestRotation;
    }

    float CountInliers(Accord.Math.Vector3[] points_P, Accord.Math.Vector3[] transformedPoints)
    {
        float totalDistance = 0;

        for (int i = 0; i < points_P.Length; i++)
        {
            Accord.Math.Vector3 distanceVector = Accord.Math.Vector3.Subtract(points_P[i], transformedPoints[i]);
            totalDistance += distanceVector.Square;
        }

        return totalDistance;
    }



    // Function to apply rotation and translation to points
    Accord.Math.Vector3[] ApplyTransformation(Accord.Math.Vector3[] points, Accord.Math.Matrix3x3 rotation, Accord.Math.Vector3 translation)
    {
        Accord.Math.Vector3[] transformedPoints = new Accord.Math.Vector3[points.Length];

        for (int i = 0; i < points.Length; i++)
        {
            if (Rigid_transformation_up_to_a_global_scale)
            {
                transformedPoints[i].X = points[i].X*scale;
                transformedPoints[i].Y = points[i].Y * scale;
                transformedPoints[i].Z = points[i].Z * scale;
            }
            else
            {
                transformedPoints[i] = points[i];
            }
            transformedPoints[i] = rotation * transformedPoints[i];
            transformedPoints[i] = transformedPoints[i] + translation;
        }

        return transformedPoints;
    }

    // Function to visualize the object
    private void VisualizeObject(Accord.Math.Vector3[] points, Color color)
    {   

        for (var i = 0; i < points.Length; i++)
        {
            GameObject newObject = Instantiate(prefab, new UnityEngine.Vector3(points[i].X, points[i].Y, points[i].Z), Quaternion.identity);
            // Get the renderer component of the object
            Renderer renderer = newObject.GetComponent<Renderer>();

            // Check if the object has a renderer component
            if (renderer != null)
            {
                // Create a new material with the specified color
                Material material = new Material(Shader.Find("Standard"));
                material.color = color;

                // Assign the new material to the renderer
                renderer.material = material;
            }
            else
            {
                Debug.LogWarning("Renderer component not found on the instantiated object.");
            }


        }

    }
    private IEnumerator AnimateObject(Accord.Math.Vector3[] startPoints, Accord.Math.Vector3[] endPoints, float duration, Color color)
    {
        float elapsedTime = 0f;


        for (int i = 0; i < startPoints.Length; i++)
        {
            GameObject newObject = Instantiate(prefab,new UnityEngine.Vector3(startPoints[i].X, startPoints[i].Y, startPoints[i].Z), Quaternion.identity);

            newObject.AddComponent<LineRenderer>();
            lineRenderers.Add(newObject.GetComponent<LineRenderer>());
            // Get the renderer component of the object
            Renderer renderer = newObject.GetComponent<Renderer>();

            // Check if the object has a renderer component
            if (renderer != null)
            {
                // Create a new material with the specified color
                Material material = new Material(Shader.Find("Standard"));
                material.color = color;

                // Assign the new material to the renderer
                renderer.material = material;
            }
            else
            {
                Debug.LogWarning("Renderer component not found on the instantiated object.");
            }

            createdObjects.Add(newObject);


            // Initialize LineRenderer component
            lineRenderers[i].startWidth = 0.1f;
            lineRenderers[i].endWidth = 0.1f;
            lineRenderers[i].material = new Material(Shader.Find("Sprites/Default"));
            lineRenderers[i].startColor = Color.green;
            lineRenderers[i].endColor = Color.green;

        }


        while (elapsedTime < duration)
        {
            for (int i = 0; i < startPoints.Length; i++)
            {
                UnityEngine.Vector3 lerpedPosition = UnityEngine.Vector3.Lerp(new UnityEngine.Vector3(startPoints[i].X, startPoints[i].Y, startPoints[i].Z),new UnityEngine.Vector3(endPoints[i].X, endPoints[i].Y, endPoints[i].Z), elapsedTime / duration);
                createdObjects[i].transform.position = lerpedPosition;

                lineRenderers[i].SetPosition(0, new UnityEngine.Vector3(startPoints[i].X, startPoints[i].Y, startPoints[i].Z));
                lineRenderers[i].SetPosition(1, createdObjects[i].transform.position);
            }

            elapsedTime += Time.deltaTime;

            yield return null;
        }
        isFinished = true;
    }
}
