using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class SceneManager : MonoBehaviour
{   
    public GameObject part1;
    public GameObject part2;
    public GameObject part3;
    public GameObject part3_movement;
    public GameObject part4;

    public GameObject part5_cube;
    public GameObject part5_sphere;
    public GameObject part5_cylinder;
    public GameObject part5_buttons;

    public GameObject part6;
    private float initialDistance;
    private Vector3 initialScale;
    private bool isAnimated = false;

    private ARTrackedImageManager _trackedImageManager;
    public GameObject[] AR_prefabs;
    private readonly Dictionary<string, GameObject> _instantiatedPrefabs = new Dictionary<string, GameObject>();  

    public int currentPart = 0;

    private void Awake()
    {
        _trackedImageManager = GetComponent<ARTrackedImageManager>();      
    }
    // Start is called before the first frame update
    void Start()
    {   
        ClearAll();
    }
    void OnEnable()
    {
        _trackedImageManager.trackedImagesChanged += TrackedImage;
    }
    private void OnDisable()
    {
        _trackedImageManager.trackedImagesChanged -= TrackedImage;
    }
    private void TrackedImage(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
        {
            var imageName = trackedImage.referenceImage.name;

            foreach (var prefab in AR_prefabs)
            {
                if(string.Compare(prefab.name, imageName, StringComparison.OrdinalIgnoreCase) == 0 && !_instantiatedPrefabs.ContainsKey(imageName))
                {
                    var newPrefab = Instantiate(prefab, trackedImage.transform);
                    newPrefab.SetActive(true);

                    _instantiatedPrefabs[imageName] = newPrefab;
                }
            }

        }
        foreach (var trackedImage in eventArgs.updated)
        {
            _instantiatedPrefabs[trackedImage.referenceImage.name].SetActive(trackedImage.trackingState == TrackingState.Tracking);     
        }

        foreach(var trackedImage in eventArgs.removed)
        {
            Destroy(_instantiatedPrefabs[trackedImage.referenceImage.name]);
            _instantiatedPrefabs.Remove(trackedImage.referenceImage.name);  
        }
    }
    // Update is called once per frame
    void Update()
    {   
        if(currentPart == 6)
        {
            if(Input.touchCount == 2)
            {
                var touchZero = Input.GetTouch(0);
                var touchOne = Input.GetTouch(1);

                if(touchZero.phase == TouchPhase.Ended || touchZero.phase == TouchPhase.Canceled ||
                    touchOne.phase == TouchPhase.Ended || touchOne.phase == TouchPhase.Canceled)
                {
                    return;
                }
                if(touchZero.phase == TouchPhase.Began || touchOne.phase == TouchPhase.Began)
                {
                    initialDistance = Vector2.Distance(touchZero.position, touchOne.position);
                    initialScale = part6.transform.localScale;
                }
                else // If touched zoom-in/ zoom-out
                {
                    var currentDistance = Vector2.Distance(touchZero.position,touchOne.position);
                    
                    var factor = currentDistance / initialDistance;
                    part6.transform.localScale = initialScale * factor;
                }
            }
            else if(Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);
                if(touch.phase == TouchPhase.Began)
                {
                    if (!isAnimated)
                    {
                        Animator animation = part6.GetComponent<Animator>();
                        animation.enabled = true;
                        isAnimated = true;
                    }
                    else
                    {
                        Animator animation = part6.GetComponent<Animator>();
                        animation.enabled = false;
                        isAnimated = false;
                    }
                }
            }
        }
    }
    public void PreviousStep()
    {

        if (currentPart > 1)
        {
            currentPart -= 1;
        }
        ClearAll();
        switch (currentPart)
        {
            case 1:
                Part1();
                break;
            case 2:
                Part2();
                break;
            case 3:
                Part3();
                break;
            case 4:
                Part4();
                break;
            case 5:
                Part5();
                break;
            case 6:
                Part6();
                break;
        }
    }

    public void NextStep()
    {
        if(currentPart < 6)
        {
            currentPart += 1;
        }
        ClearAll();
        switch (currentPart)
        {
            case 1:
                Part1();
                break;
            case 2:
                Part2();
                break;
            case 3:
                Part3();
                break;
            case 4:
                Part4();
                break;
            case 5:
                Part5();
                break;
            case 6:
                Part6();
                break;
        }
    }

    public void Part1()
    {
        part1.SetActive(true);
    }
    public void Part2()
    {   
        part2.SetActive(true);
        Animator animation = part2.GetComponent<Animator>();
        animation.enabled = true;

    }
    public void Part3()
    {   
        part3.SetActive(true);
        part3_movement.SetActive(true); 
    }

    public void Part3_Up()
    {
        SphereCollider sphere =  part3.GetComponentInChildren<SphereCollider>();
        sphere.transform.Translate(new Vector3(0,0.1f,0) );
    }
    public void Part3_Down()
    {
        SphereCollider sphere = part3.GetComponentInChildren<SphereCollider>();
        sphere.transform.Translate(new Vector3(0, -0.1f, 0));
    }
    public void Part3_Left()
    {
        SphereCollider sphere = part3.GetComponentInChildren<SphereCollider>();
        sphere.transform.Translate(new Vector3(-0.1f, 0, 0));
    }
    public void Part3_Right()
    {
        SphereCollider sphere = part3.GetComponentInChildren<SphereCollider>();
        sphere.transform.Translate(new Vector3(0.1f, 0, 0));
    }
    public void Part3_Inside()
    {
        SphereCollider sphere = part3.GetComponentInChildren<SphereCollider>();
        sphere.transform.Translate(new Vector3(0, 0,0.1f));
    }
    public void Part3_Outside()
    {
        SphereCollider sphere = part3.GetComponentInChildren<SphereCollider>();
        sphere.transform.Translate(new Vector3(0, 0,-0.1f));
    }
    public void Part4()
    {
        part4.SetActive(true);
    }

    public void Part5()
    {
        part5_buttons.SetActive(true);
    }
    public void Part5_Cube()
    {   
        part5_cylinder.SetActive(false);
        part5_sphere.SetActive(false);
        part5_cube.SetActive(true);  
    }
    public void Part5_Sphere()
    {
        part5_cylinder.SetActive(false);
        part5_sphere.SetActive(true);
        part5_cube.SetActive(false);
    }
    public void Part5_Cylinder()
    {
        part5_cylinder.SetActive(true);
        part5_sphere.SetActive(false);
        part5_cube.SetActive(false);
    }
    public void Part6()
    {
        part6.SetActive(true);
        Animator animation = part6.GetComponent<Animator>();
        animation.enabled = false;
    }
    public void ClearAll()
    {
        part1.SetActive(false);

        part2.SetActive(false);

        part3.SetActive(false);
        part3_movement.SetActive(false);

        part4.SetActive(false);

        part5_cube.SetActive(false);
        part5_cylinder.SetActive(false);
        part5_sphere.SetActive(false);
        part5_buttons.SetActive(false);

        part6.SetActive(false);
    }
}
