using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TournamentSelection : GASelectionAlgorithmBase<float>
{
    [Range(1,100)]
    public int TournamentSize;

    public DNA<float> TournamentWinner()
    {
        float maxFitness = int.MinValue;
        DNA<float> tournamentWinner=null;
        for (int i = 0; i < TournamentSize; i++)
        {
            int randomIndex= Helpers.Random.Next(0, _geneticAglorithm.Population.Count-1);

            DNA<float> contestant = this._geneticAglorithm.Population[randomIndex];
            if (contestant.Fitness > maxFitness)
            {
                maxFitness = contestant.Fitness;
                tournamentWinner = contestant;
            }
        }
        return tournamentWinner;
    }



    public override DNA<float> SelectionStrategy()
    {
        return TournamentWinner();
    }
}
