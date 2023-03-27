using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UIElements.VisualElement;

public class BoundingBoxDebug : MonoBehaviour
{
    public GameObject ObstacleCoursePrefab;
    private Bounds _myBounds = new Bounds(Vector3.zero, new Vector3(0, 0, 0));

    void AddChildrenToBounds(Transform hierarchy)
    {

        for (int i = 0; i < hierarchy.childCount; i++)
        {
            var child = hierarchy.GetChild(i);
            Collider childCollider;
            if (child.TryGetComponent<Collider>(out childCollider))
            {
                _myBounds.Encapsulate(childCollider.bounds.min);
                _myBounds.Encapsulate(childCollider.bounds.max);
            }

            

            if (child.transform.childCount!=0)
            {
                AddChildrenToBounds(child);
            }
          
        }
    }

     bool SetCenterToTargetChildren(Transform hierarchy)
    {
        bool found=false;
        for (int i = 0; i < hierarchy.childCount; i++)
        {
            var child = hierarchy.GetChild(i);
            if (child.CompareTag("Target"))
            {
                _myBounds.center = child.transform.position;
                return true;
            }
            if (child.transform.childCount != 0)
            {
                found= SetCenterToTargetChildren(child);
            }

            if (found==true)
            {
                break;
            }
        }
        return found;
    }

    void Start()
    {
        //_myBounds.center=ObstacleCoursePrefab.transform.position;
        SetCenterToTargetChildren(this.ObstacleCoursePrefab.transform);
        AddChildrenToBounds(this.ObstacleCoursePrefab.transform);
    }

    void OnDrawGizmosSelected()
    {
        // Display the explosion radius when selected
        //Renderer.bounds.

        Gizmos.DrawWireCube(_myBounds.center, _myBounds.size);
    }
}
