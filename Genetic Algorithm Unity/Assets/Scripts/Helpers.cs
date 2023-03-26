using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public static class Helpers 
{
    public static float ConvertFromRange(float _input_value_tobe_converted, float _input_range_min,
        float _input_range_max, float _output_range_min = 0.0f,
        float _output_range_max = 1.0f)
    {
        float diffOutputRange = Math.Abs((_output_range_max - _output_range_min));
        float diffInputRange = Math.Abs((_input_range_max - _input_range_min));
        float convFactor = (diffOutputRange / diffInputRange);
        return (_output_range_min + (convFactor * (_input_value_tobe_converted - _input_range_min)));
    }

    public static Random Random=new Random();

    public static float RandomFloat()
    {
        return (float)Random.NextDouble();
    }

    public static float RandomFloat(float min, float max)
    {
        return ConvertFromRange(RandomFloat(), 0, 1, min, max);
    }
}
