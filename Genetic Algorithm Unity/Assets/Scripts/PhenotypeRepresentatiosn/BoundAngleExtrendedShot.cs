using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundAngleExtrendedShot : MonoBehaviour, ShotPhenotypeRepresentation
{
    public Throwing ThrowingGA { get; set; }

    private GameObject ObstacleCoursePrefab;
    //By the rules of the game the ball always start at 0,0,0
    public Vector3 StartPosition = new Vector3(0, 0, 0);

    [HideInInspector] public Quaternion Rotation { get; set; }
    [HideInInspector] public float InitialImpulse { get; set; }

    public int MaxImpulse;

    private Bounds _myBounds = new Bounds(Vector3.zero, new Vector3(0, 0, 0));
    public Vector2 YAngleRange = new Vector2();
    public Vector2 XAngleRange = new Vector2();



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



            if (child.transform.childCount != 0)
            {
                AddChildrenToBounds(child);
            }

        }
    }

    bool SetCenterToTargetChildren(Transform hierarchy)
    {
        bool found = false;
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
                found = SetCenterToTargetChildren(child);
            }

            if (found == true)
            {
                break;
            }
        }
        return found;
    }

    void GetBoundingBoxOfHirearchy(Transform t)
    {
        SetCenterToTargetChildren(t);
        AddChildrenToBounds(t);
    }


    public Vector3 ShotImpulse => (this.Rotation.normalized * Vector3.forward) * this.InitialImpulse;
    public float MaxGenes => 3;



    public void DecodeGenes(float[] floatGenes)
    {

        float[] values = new float[floatGenes.Length];
        //X angle representation
        values[0] = Helpers.ConvertFromRange(floatGenes[1], 0, 1, -90, XAngleRange.x);
        //Limited Y anlge representation

        values[1] = Helpers.ConvertFromRange(floatGenes[0], 0, 1, this.YAngleRange.x, this.YAngleRange.y);

        values[2] = floatGenes[2];

        Vector3 eulerAngles = new Vector3(values[0], values[1], 0);
        this.Rotation = Quaternion.Euler(eulerAngles).normalized;
        this.InitialImpulse = values[2] * MaxImpulse;
    }

    public float[] EncodeGenes()
    {
        float[] encoded = new float[3];
        encoded[0] = Helpers.ConvertFromRange(this.Rotation.x, -90, XAngleRange.x, 0, 1);
        encoded[1] = Helpers.ConvertFromRange(this.Rotation.y, YAngleRange.x, YAngleRange.y, 0, 1);

        encoded[2] = this.InitialImpulse / MaxImpulse;
        return encoded;
    }

    void OnEnable()
    {
        this.ObstacleCoursePrefab = this.ThrowingGA.ObstacleCourse;
        GetBoundingBoxOfHirearchy(this.ObstacleCoursePrefab.transform);


        var dirToXMin = (new Vector3(_myBounds.min.x, 0, _myBounds.min.z) - Vector3.forward).normalized;
        var dirToXMax = (new Vector3(_myBounds.max.x, 0, _myBounds.min.z) - Vector3.forward).normalized;
        YAngleRange.x = Vector3.SignedAngle(Vector3.forward, dirToXMin, Vector3.up);
        YAngleRange.y = Vector3.SignedAngle(Vector3.forward, dirToXMax, Vector3.up);

        var dirToYMin = (new Vector3(0, _myBounds.min.y, _myBounds.min.z) - Vector3.forward).normalized;
        XAngleRange.x = Vector3.SignedAngle(Vector3.forward, dirToYMin, Vector3.right);
    }

    void OnDrawGizmos()
    {
        // Display the explosion radius when selected
        //Renderer.bounds.
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(_myBounds.center, _myBounds.size);
    }
}
