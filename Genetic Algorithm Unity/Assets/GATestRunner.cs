using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;
using System.Security.Cryptography.X509Certificates;


[System.Serializable]
public class TestStrategyContainer<T>
{
    public int StartIndex=0;
    public int EndIndex=10;
    public GameObject ContainerOfStrategies;

    public T this[int i]
    {
        get
        {
            if (i < StartIndex)
            {
               return _strategies[StartIndex];
            }
            else if (i>EndIndex)
            {
                return _strategies[EndIndex];
            }
            else
            {
                return _strategies[i];
            }
        }
    }

    public void SetStrategies()
    {
        foreach (var selectionAlgorithm in ContainerOfStrategies.GetComponentsInChildren<T>())
            _strategies.Add(selectionAlgorithm);
    }

    private List<T> _strategies=new List<T>();

}

//Establishes set of discreet testing variables, bu given lower bound, increment and steps
[System.Serializable]
public class TestVariableSet
{
    public float LowerBound;
    public float Increment;
    public int Iterations;

    public float HigherBound => LowerBound + Iterations * Increment;

    //Gets the N-th iteration if one exist, if not gets the HigherBound
    public float this[int i]
    {
        get { return LowerBound + i * Increment; }
    }
}

//Sets the properties of each new genetic algorithms, as well as the sample sizes of the test performed.
// Stored the defined data in predefined struct and fires and event when GA is completed.
public class GATestRunner : MonoBehaviour
{
    public Throwing ThrowingGA;


    public TestStrategyContainer<GASelectionAlgorithmBase<float>> SelectionStrategies;

    public TestStrategyContainer<ShotPhenotypeRepresentation> PhenotypeRepresentations;

    public TestStrategyContainer<FloatBasedRecombination> CrossoverStrategies;

    public TestStrategyContainer<Transform> ObstacleCourses; 

    public TestVariableSet MutationRateTestSet;
    public TestVariableSet PopulationTestVariableSet;
    


    // Start is called before the first frame update
    void Start()
    {
      
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void RunAlgorithms()
    {
        //Phenotype representation loop
        for (int phenotypeIndex = PhenotypeRepresentations.StartIndex; phenotypeIndex < PhenotypeRepresentations.EndIndex; phenotypeIndex++)
        {
            //Selection Strategy loop
            for (int selectionIndex = SelectionStrategies.StartIndex; selectionIndex < SelectionStrategies.EndIndex; selectionIndex++)
            {
                //Crossover strategy loop

                for (int crossoverIndex = 0; crossoverIndex < CrossoverStrategies.EndIndex; crossoverIndex++)
                {

                    //Obstacle courses
                    for (int obstacleCourseIndex = 0; obstacleCourseIndex < this.ObstacleCourses.EndIndex; obstacleCourseIndex++)
                    {

                        //Mutation rate variable 
                        for (int mutationIndex = 0; mutationIndex < this.MutationRateTestSet.Iterations; mutationIndex++)
                        {


                            for (int populationIndex = 0; populationIndex < this.PopulationTestVariableSet.Iterations; populationIndex++)
                            {


                                ThrowingGA.SelectionAlgorithm = SelectionStrategies[selectionIndex];
                                ThrowingGA.Phenotype = PhenotypeRepresentations[phenotypeIndex];
                                ThrowingGA.ObstacleCourse = ObstacleCourses[obstacleCourseIndex].gameObject;
                                ThrowingGA.mutationRate = MutationRateTestSet[mutationIndex];
                                ThrowingGA.BallCount = (uint)Math.Ceiling(PopulationTestVariableSet[populationIndex]);
                                ThrowingGA.CrossoverAlgorithm = CrossoverStrategies[crossoverIndex];

                            }
                        }



                    }

                }
            }


        }
    }
}
