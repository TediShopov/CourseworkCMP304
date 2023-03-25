using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
}
