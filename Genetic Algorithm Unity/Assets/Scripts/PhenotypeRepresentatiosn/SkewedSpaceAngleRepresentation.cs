using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public float Center => (this.Min + this.Max)/2.0f;
    public float Distance => (this.Max - this.Min);

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

struct RangeModifer
{
    public float Max;
    public float Modifier;
}

public class SkewedSpaceAngleRepresentation : ShotPhenotypeRepresentation
{

    private GameObject ObstacleCoursePrefab;
    //By the rules of the game the ball always start at 0,0,0
    public Vector3 StartPosition = new Vector3(0, 0, 0);

   

    private Bounds _myBounds = new Bounds(Vector3.zero, new Vector3(0, 0, 0));
    public Vector2 YAngleRange = new Vector2();

    List<Collider> childColliders=new List<Collider>();

    private List<ScalarRange> TargetsInXSpace=new List<ScalarRange>();
    private List<ScalarRange> StrecthedTargetsSpace = new List<ScalarRange>();

    List<RangeModifer> rangeModifers=new List<RangeModifer>();

    void FilLRangeModifiers(in List<ScalarRange> targets, float emptySpaceWeight = 2.0f, float min = -90, float max = 90)
    {

        float distanceToMove;
        float modifer = 0;
        float from = 0;
        float to = 0;
        for (int i = -1; i < targets.Count; i++)
        {
            if (i < 0)
            {
                var rangeTwo = targets[i + 1];
                from = rangeTwo.Min;

                distanceToMove = (rangeTwo.Min - min) / emptySpaceWeight;
                rangeTwo.Min -= distanceToMove;
                to=rangeTwo.Min;
                rangeModifers.Add(new RangeModifer() {Max = rangeTwo.Center,Modifier = to/from });
            }
            else if (i >= targets.Count - 1)
            {
                var rangeOne = targets[i];
                from = rangeOne.Max;
                distanceToMove = (max - rangeOne.Max) / emptySpaceWeight;
                rangeOne.Max += distanceToMove;
                to = rangeOne.Max;
                rangeModifers.Add(new RangeModifer() { Max = max, Modifier = to / from });

            }
            else
            {
                var rangeOne = targets[i];
                var rangeTwo = targets[i + 1];

                distanceToMove = ((rangeTwo.Min - rangeOne.Max) / emptySpaceWeight) / 2.0f;

                float middleOfRanges = (rangeOne.Center + rangeTwo.Center) / 2.0f;
                float middleOfRange2 = rangeTwo.Center;
                //move upper bound
                from = rangeOne.Max;

                rangeOne.Max += distanceToMove;
                to = rangeOne.Max;

                float modifierLeft = to / from;

                //Move lowe bound
                from = rangeTwo.Min;
                rangeTwo.Min -= distanceToMove;
                to = rangeTwo.Min;


                float modifierRight = to / from;
                rangeModifers.Add(new RangeModifer() { Max = middleOfRanges, Modifier = modifierLeft });

                rangeModifers.Add(new RangeModifer() { Max = middleOfRange2, Modifier = modifierRight });


                

            }

        }
    }


    void StretchTargetSpace(in List<ScalarRange> targets, float emptySpaceWeight=2.0f,float min=-90, float max=90)
    {
       
        float distanceToMove;
        for (int i = -1; i < targets.Count; i++)
        {
            if (i < 0)
            {
                var rangeTwo = targets[i + 1];
                distanceToMove= ( rangeTwo.Min - min) / emptySpaceWeight;
                rangeTwo.Min -= distanceToMove;
            }
            else if (i >= targets.Count - 1)
            {
                var rangeOne = targets[i];
                distanceToMove = (max - rangeOne.Max) / emptySpaceWeight;
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

       
    }

     float GetYAngle(float val)
    {
        float toAngle = Helpers.ConvertFromRange(val,0, 1, this.YAngleRange.x, this.YAngleRange.y);
        return toAngle;

    }

    public override void DecodeGenes(float[] floatGenes)
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

    public override float[] EncodeGenes()
    {
        float[] encoded = new float[3];
        encoded[0] = Helpers.ConvertFromRange(this.Rotation.x, -90, 90, 0, 1);
        encoded[1] = Helpers.ConvertFromRange(this.Rotation.y, YAngleRange.x, YAngleRange.y, 0, 1);

        encoded[2] = this.InitialImpulse / MaxImpulse;
        return encoded;
    }

    List<ScalarRange> DeepCopyRanges( List<ScalarRange> other)
    {
        var toReturn = new List<ScalarRange>();
        foreach ( var range in other ) 
        {
            toReturn.Add( new ScalarRange(range.Min,range.Max) );
        }

        return toReturn;
    }

    void OnEnable()
    {
        this.ObstacleCoursePrefab = this.ThrowingGA.ObstacleCourse;
        GetBoundingBoxOfHirearchy(this.ObstacleCoursePrefab.transform);
        this.StrecthedTargetsSpace = DeepCopyRanges(this.TargetsInXSpace);
       



        ScalarRange range= GetAngleBoundsFromForwardVector(_myBounds);
        YAngleRange.x = range.Min;
        YAngleRange.y = range.Max;

        //var dirToXMin = (new Vector3(_myBounds.min.x, 0, _myBounds.min.z) - Vector3.zero).normalized;
        //var dirToXMax = (new Vector3(_myBounds.max.x, 0, _myBounds.max.z) - Vector3.zero).normalized;
        //YAngleRange.x = Vector3.SignedAngle(Vector3.forward, dirToXMin, Vector3.up);
        //YAngleRange.y = Vector3.SignedAngle(Vector3.forward, dirToXMax, Vector3.up);

        StretchTargetSpace(this.StrecthedTargetsSpace, 2, YAngleRange.x, YAngleRange.y);

        FilLRangeModifiers(this.StrecthedTargetsSpace, 2, YAngleRange.x, YAngleRange.y);
        var listOfModifiedRanges = DeepCopyRanges(this.TargetsInXSpace);
        foreach (var r in listOfModifiedRanges)
        {
            var a= rangeModifers.FirstOrDefault(x => x.Max >= r.Min);
            r.Min *= a.Modifier;
            var b = rangeModifers.FirstOrDefault(x => x.Max >= r.Max);
            r.Max *= b.Modifier;

        }


        //Check if equal
        bool isEqual = true;
        for (int i = 0; i < StrecthedTargetsSpace.Count; i++)
        {
            if (!listOfModifiedRanges[i].Equals(StrecthedTargetsSpace[i]))
            {
                isEqual = false;
            }
        }

        int z = 3;


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
