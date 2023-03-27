using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements;


class ScalarRange : IComparable<ScalarRange>
{
    public float Min;
    public float Max;

    public ScalarRange(float one, float two)
    {
        if (one<=two)
        {
            this.Min = one; this.Max = two;

        }
        else
        {
            this.Min = two; this.Max = one;
        }
    }


    public bool Contains(float value)
    {
       return this.Min <= value && value <= this.Max;
    }

    public bool Contains(ScalarRange other)
    {
        return  this.Contains(other.Min) && this.Contains(other.Max);
    }

    public bool Intersect(ScalarRange other)
    {
       return this.Contains(other.Min) || this.Contains(other.Max) || other.Contains(this.Min) || other.Contains(this.Max);
    }

    public ScalarRange Union(ScalarRange other)
    {
       return new ScalarRange(Math.Min(this.Min, other.Min), Math.Max(this.Max, other.Max));
    }

    public bool Equals(ScalarRange other)
    {
        return this.Min == other.Min && this.Max == other.Max;
    }

    public static bool operator>(ScalarRange l, ScalarRange r)
    {
        return l.Min > r.Max;

    }
    public static bool operator<(ScalarRange l, ScalarRange r)
    {
       return l.Max < r.Min;
    }

    public int CompareTo(ScalarRange other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        if (this<other)
        {
            return -1;
        }
        else if(this.Equals(other))
        {
            return 0;
        }
        else
        {
            return 1;
        }
    }
}

public class SkewedSpaceAngleRepresentation : MonoBehaviour,ShotPhenotypeRepresentation
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

    List<Collider> childColliders=new List<Collider>();

    private List<ScalarRange> TargetsInXSpace=new List<ScalarRange>();
    private List<ScalarRange> StrecthedTargetsSpace = new List<ScalarRange>();

    void StretchTargetSpace(in List<ScalarRange> targets, float emptySpaceWeight=2.0f)
    {
       
        float distanceToMove;
        for (int i = 0; i < targets.Count; i++)
        {
            if (i < 0)
            {
                var rangeTwo = targets[i + 1];
                distanceToMove= (rangeTwo.Min) / emptySpaceWeight;
                rangeTwo.Min -= distanceToMove;
            }
            else if (i >= targets.Count - 1)
            {
                var rangeOne = targets[i];
                distanceToMove = (_myBounds.max.x - rangeOne.Max) / emptySpaceWeight;
                rangeOne.Max += distanceToMove;
            }
            else
            {
                var rangeOne = targets[i];
                var rangeTwo = targets[i + 1];

                 distanceToMove = ((rangeTwo.Min - rangeOne.Max) / emptySpaceWeight) / 2.0f;
                rangeOne.Max += distanceToMove;
                rangeTwo.Min -= distanceToMove;
            }

        }
    }


    void AddOrExtendTargetRange(ScalarRange otherRange)
    {
        ScalarRange updtedUnitedRange= otherRange;


        List<ScalarRange> rangesToRemove=new List<ScalarRange>();
        for (int i = 0; i < TargetsInXSpace.Count; i++)
        {
            var range = TargetsInXSpace[i];
            if (range.Intersect(updtedUnitedRange))
            {
                rangesToRemove.Add(range);
                updtedUnitedRange = updtedUnitedRange.Union(range);
            }
        }

        this.TargetsInXSpace.RemoveAll(x => rangesToRemove.Contains(x));
        

        //Doesnt intersect with any other range no update needed
        if (updtedUnitedRange == otherRange)
        {
            TargetsInXSpace.Add(updtedUnitedRange);
            TargetsInXSpace.Sort((rangeL, rangeR) => rangeL.CompareTo(rangeR));

        }
        //Intersected with one other range. Update other range and check again
        else
        {
            AddOrExtendTargetRange(updtedUnitedRange);
        }

     
    }


    private Vector3[] points=new Vector3[4];
    ScalarRange GetAngleBoundsFromForwardVector(Bounds bounds)
    {
        //points 
        points[0].x=bounds.min.x; points[0].z =bounds.min.z;
        points[1].x = bounds.min.x; points[1].z = bounds.max.z;
        points[2].x = bounds.max.x; points[2].z = bounds.min.z;
        points[3].x = bounds.max.x; points[3].z = bounds.max.z;

        float smallestAngle = float.MaxValue;
        float biggestAngle = float.MinValue;
        for (int i = 0; i < 4; i++)
        {

            var angle = Vector3.SignedAngle(Vector3.forward, points[i], Vector3.up);
            if (angle>biggestAngle)
            {
                biggestAngle=angle;
            }

            if (angle<smallestAngle)
            {
                smallestAngle=angle;
            }
        }
        return new ScalarRange(smallestAngle,biggestAngle);
    }

    void AddChildrenToBounds(Transform hierarchy)
    {

        for (int i = 0; i < hierarchy.childCount; i++)
        {
            var child = hierarchy.GetChild(i);
            Collider childCollider;
            if (child.TryGetComponent<Collider>(out childCollider))
            {
                childColliders.Add(childCollider);
                AddOrExtendTargetRange(GetAngleBoundsFromForwardVector(childCollider.bounds));
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

        this.StrecthedTargetsSpace = new List<ScalarRange>(this.TargetsInXSpace);
        //StretchTargetSpace(this.StrecthedTargetsSpace);
    }


    public Vector3 ShotImpulse => (this.Rotation.normalized * Vector3.forward) * this.InitialImpulse;
    public float MaxGenes => 3;



    public void DecodeGenes(float[] floatGenes)
    {

        float[] values = new float[floatGenes.Length];
        //X angle representation
        values[0] = Helpers.ConvertFromRange(floatGenes[1], 0, 1, -90, 90);
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
        encoded[0] = Helpers.ConvertFromRange(this.Rotation.x, -90, 90, 0, 1);
        encoded[1] = Helpers.ConvertFromRange(this.Rotation.y, YAngleRange.x, YAngleRange.y, 0, 1);

        encoded[2] = this.InitialImpulse / MaxImpulse;
        return encoded;
    }

    void OnEnable()
    {
        this.ObstacleCoursePrefab = this.ThrowingGA.ObstacleCourse;
        GetBoundingBoxOfHirearchy(this.ObstacleCoursePrefab.transform);


        var dirToXMin = (new Vector3(_myBounds.min.x, 0, _myBounds.min.z) - Vector3.zero).normalized;
        var dirToXMax = (new Vector3(_myBounds.max.x, 0, _myBounds.max.z) - Vector3.zero).normalized;
        YAngleRange.x = Vector3.SignedAngle(Vector3.forward, dirToXMin, Vector3.up);
        YAngleRange.y = Vector3.SignedAngle(Vector3.forward, dirToXMax, Vector3.up);


    }

    void OnDrawGizmos()
    {
        // Display the explosion radius when selected
        //Renderer.boundsg
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(_myBounds.center, _myBounds.size);


        Vector3 min, max, nMin, nMax;
        foreach (var range in this.TargetsInXSpace)
        {
            Gizmos.color = Color.red;
            //Quaternion rot= Quaternion.Euler(0,range.Min,0);
            min = Quaternion.Euler(0, range.Min, 0) * Vector3.forward * 10.0f;
            max = Quaternion.Euler(0, range.Max, 0) * Vector3.forward * 10.0f;


            //
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(Vector3.zero, min);
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(Vector3.zero, max);

        }

        foreach (var col in childColliders)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }
    }
}
