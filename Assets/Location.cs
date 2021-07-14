using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Location : MonoBehaviour
{
    /// <summary>
    /// Location type is for example work, house, mines, etc.
    /// </summary>
    [SerializeField]
    private string _locationType;
    public string locationType
    {
        get => _locationType;
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Vector3 getWorldPosition()
    {
        return gameObject.transform.position;
    }
}
