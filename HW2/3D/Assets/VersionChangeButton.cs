using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VersionChangeButton : MonoBehaviour
{
    public TextMeshProUGUI textObject;
    bool isRigidTransformationScaled = false;


    // Start is called before the first frame update
    void Start()
    {
        textObject = GameObject.Find("VersionText").GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ButtonPressed()
    {
        if (isRigidTransformationScaled)
        {
            isRigidTransformationScaled = false;
            textObject.text = "Current Version: Rigid transformation";
        }
        else
        {
            isRigidTransformationScaled = true;
            textObject.text = "Current Version: Rigid transformation up to a global scale";
        }
        
    }
}
