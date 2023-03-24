using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class RouleteWheelSelection : GASelectionAlgorithmBase<float>
{
    public List<float> RouletteDistibutions;

    void Start()
    {
        this.RouletteDistibutions = new List<float>();
    }

    public float BiggestPercentageOfDistribution = 0;
    public float SmallestPercentageOfDistribution = float.MaxValue;


    void CalculateRouletteDistributions()
    {
        float previousFitness = 0;
        BiggestPercentageOfDistribution = 0;
        SmallestPercentageOfDistribution = 1;
        RouletteDistibutions.Clear();
        for (int i = 0; i < _geneticAglorithm.Population.Count; i++)
        {
           float fitness= _geneticAglorithm.Population[i].Fitness/ _geneticAglorithm.FitnessSum;

           if (fitness > BiggestPercentageOfDistribution)
           {
                BiggestPercentageOfDistribution=fitness;
           }

           if (fitness < SmallestPercentageOfDistribution)
           {
               SmallestPercentageOfDistribution = fitness;
           }


           RouletteDistibutions.Add(previousFitness+fitness);
           previousFitness = previousFitness + fitness;

        }


    }

    DNA<float> PickFromRoulette(Random random)
    {
        float ran = (float)random.NextDouble();
        for (int i = 0; i < RouletteDistibutions.Count; i++)
        {
            if (ran <= RouletteDistibutions[i])
            {
                return _geneticAglorithm.Population[i];
            }
        }

        return _geneticAglorithm.Population[RouletteDistibutions.Count - 1];
    }

    public override DNA<float> SelectionStrategy()
    {
        if (IsNewGeneration(_geneticAglorithm.Generation))
        {
            CalculateRouletteDistributions();
            _lastGenerationIndex = _geneticAglorithm.Generation;
        }

        return PickFromRoulette(new Random());
    }
}
