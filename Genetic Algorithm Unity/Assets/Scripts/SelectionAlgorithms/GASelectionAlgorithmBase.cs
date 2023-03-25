using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GASelectionAlgorithmBase<T> : MonoBehaviour
{
    protected GeneticAglorithm<T> _geneticAglorithm;

    protected int _lastGenerationIndex = -1;
    public void SetGeneticAlgorthm(GeneticAglorithm<T> ga)
    {
        _geneticAglorithm = ga;
    }

    public bool IsNewGeneration(int generationIndex) => _lastGenerationIndex != generationIndex;


    public abstract DNA<T> SelectionStrategy();
}
