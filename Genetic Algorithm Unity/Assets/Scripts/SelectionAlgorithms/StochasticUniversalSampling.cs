using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;


public class StochasticUniversalSampling : GASelectionAlgorithmBase<float>
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
        if (_geneticAglorithm.FitnessSum == 0)
        {

            for (int i = 0; i < _geneticAglorithm.Population.Count; i++)
            {
                float fitness = 1.0f / _geneticAglorithm.Population.Count;
                RouletteDistibutions.Add(previousFitness + fitness);
                previousFitness = previousFitness + fitness;
            }

            return;

        }
        else
        {
            for (int i = 0; i < _geneticAglorithm.Population.Count; i++)
            {
                float fitness = _geneticAglorithm.Population[i].Fitness / _geneticAglorithm.FitnessSum;

                if (fitness > BiggestPercentageOfDistribution)
                {
                    BiggestPercentageOfDistribution = fitness;
                }

                if (fitness < SmallestPercentageOfDistribution)
                {
                    SmallestPercentageOfDistribution = fitness;
                }


                RouletteDistibutions.Add(previousFitness + fitness);
                previousFitness = previousFitness + fitness;

            }
        }



    }

    private List<int> StochasticSamplingSelections = new List<int>();

    //Members are always assumed to be all the collections. Return indexes of parents
    void GenerateStochasticSamplingSelections()
    {
        StochasticSamplingSelections.Clear();

        int members = this.RouletteDistibutions.Count;
        float maxR = 1.0f / (float)members;
        float r = Helpers.ConvertFromRange((float)new Random().NextDouble(), 0, 1, 0, maxR);


        int currentMember = 0;
        if (RouletteDistibutions[0] == Single.NaN)
        {
            return;
        }

        while (StochasticSamplingSelections.Count != RouletteDistibutions.Count /** 2*/)
        {
            if (currentMember >= RouletteDistibutions.Count || r < RouletteDistibutions[currentMember])
            {
                //Add the memeber two times since we are taking twice as many parents for sexual reproduction
                StochasticSamplingSelections.Add(currentMember);
                //StochasticSamplingSelections.Add(currentMember);

                r += maxR;

            }
            else
            {
                currentMember++;
            }
        }

        lastPickedIndex = StochasticSamplingSelections.Count - 1;
    }


    private int lastPickedIndex;
    DNA<float> PickFromRoulette(System.Random random)
    {
        if (lastPickedIndex < 0)
        {
            lastPickedIndex = StochasticSamplingSelections.Count - 1;
        }

        var pick = this.StochasticSamplingSelections[lastPickedIndex];
        lastPickedIndex--;
        // this.StochasticSamplingSelections.RemoveAt(StochasticSamplingSelections.Count - 1);

        return _geneticAglorithm.Population[pick];
    }

    public override DNA<float> SelectionStrategy()
    {
        if (IsNewGeneration(_geneticAglorithm.Generation))
        {
            CalculateRouletteDistributions();
            GenerateStochasticSamplingSelections();
            _lastGenerationIndex = _geneticAglorithm.Generation;
        }

        return PickFromRoulette(new Random());
    }
}


