using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography.X509Certificates;
using System.Threading;


[System.Serializable]
public class TestStrategyContainer<T> 
{
    public int StartIndex = 0;
    public int EndIndex = 10;
    public GameObject ContainerOfStrategies;

    public T this[int i]
    {
        get
        {
            if (i < StartIndex)
            {
                return _strategies[StartIndex];
            }
            else if (i >= EndIndex)
            {
                return _strategies[EndIndex-1];
            }
            else
            {
                return _strategies[i];
            }
        }
    }

  

    public void SetStrategies()
    {
        
        foreach (var selectionAlgorithm in ContainerOfStrategies.GetComponentsInChildren<T>(true))
            _strategies.Add(selectionAlgorithm);

        if (this.EndIndex> _strategies.Count)
        {
            this.EndIndex = _strategies.Count;
        }
    }



    private List<T> _strategies = new List<T>();

   
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


struct GASetupData
{
    public int selectionIndex;
    public int phenotypeIndex;
    public int obstacleCourseIndex;
    public int mutationIndex;
    public int populationIndex;
    public int crossoverIndex;
    public bool IsSetup;
}


//Sets the properties of each new genetic algorithms, as well as the sample sizes of the test performed.
// Stored the defined data in predefined struct and fires and event when GA is completed.
public class GATestRunner : MonoBehaviour
{
    public Throwing ThrowingGA;


    public TestStrategyContainer<GASelectionAlgorithmBase<float>> SelectionStrategies;

    public TestStrategyContainer<ShotPhenotypeRepresentation> PhenotypeRepresentations;

    public TestStrategyContainer<FloatBasedRecombination> CrossoverStrategies;

    public TestStrategyContainer<ObstacleCourse> ObstacleCourses;

    public TestVariableSet MutationRateTestSet;
    public TestVariableSet PopulationTestVariableSet;

    public string NameOfTestRun;

    // Start is called before the first frame update
    void Start()
    {
        SelectionStrategies.SetStrategies();
        PhenotypeRepresentations.SetStrategies();
        CrossoverStrategies.SetStrategies();
        ObstacleCourses.SetStrategies();

        newGaSetupData.IsSetup = true;

        this.ThrowingGA.OnGenerationEnd += AppendGeneration;
        this.ThrowingGA.OnAlgorithmTerminated += WriteToCsvFile;
        this.ThrowingGA.OnAlgorithmTerminated += AlgorithmTerminated;
         
    }

    // Update is called once per frame
    void Update()
    {
        if (newGaSetupData.IsSetup==false)
        {
            SetupNewGA();
        }
    }

    public Thread TestThread;

    public void StartTests()
    {
        TestThread = new Thread(RunAlgorithms);
        this.ThrowingGA.onButtonClickUnique();
        TestThread.Start();
    }

    public void OnDestroy()
    {
        breakThread = true;
    }

    bool breakThread=false;
    bool waitingForGAFinish=false;
    
    private GASetupData newGaSetupData;
    private void RunAlgorithms()
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

                                if (breakThread)
                                {
                                    return;
                                }

                                newGaSetupData.selectionIndex = selectionIndex;
                                newGaSetupData.phenotypeIndex=phenotypeIndex;
                                newGaSetupData.crossoverIndex=crossoverIndex;
                                newGaSetupData.obstacleCourseIndex=obstacleCourseIndex;
                                newGaSetupData.mutationIndex=mutationIndex;
                                newGaSetupData.populationIndex = populationIndex;
                                newGaSetupData.IsSetup = false;

                                //Block till unity thread sets up the genetic algorthm
                                
                                while (!newGaSetupData.IsSetup)
                                {
                                    if (breakThread)
                                    {
                                        return;
                                    }
                                }

                                

                                waitingForGAFinish = true;
                                while (waitingForGAFinish)
                                {
                                    if (breakThread)
                                    {
                                        return;
                                    }
                                }


                            }
                        }



                    }

                }
            }


        }
    }

    private void SetupNewGA()
    {
        ThrowingGA.gameObject.SetActive(false);

        ThrowingGA.SelectionAlgorithm = SelectionStrategies[newGaSetupData.selectionIndex];
        ThrowingGA.Phenotype = PhenotypeRepresentations[newGaSetupData.phenotypeIndex];
        ThrowingGA.ObstacleCourse = ObstacleCourses[newGaSetupData.obstacleCourseIndex].gameObject;
        ThrowingGA.mutationRate = MutationRateTestSet[newGaSetupData.mutationIndex];
        ThrowingGA.BallCount = (uint)Math.Ceiling(PopulationTestVariableSet[newGaSetupData.populationIndex]);
        ThrowingGA.CrossoverAlgorithm = CrossoverStrategies[newGaSetupData.crossoverIndex];
        newGaSetupData.IsSetup = true;

        ThrowingGA.gameObject.SetActive(true);
        //this.ThrowingGA.onButtonClickUnique();

    }


    //private void DoOnFinish()
    //{
    //  //this.ThrowingGA.Set
    //  WriteToCsvFile();
    //}
    // Set the variable "delimiter" to ", ".
    string delimiter = ", ";

    // Set the path and filename variable "path", filename being MyTest.csv in this example.
    // Change SomeGuy for your username.
    string path => @"Tests\" + NameOfTestRun + ".csv";
    private void AppendGeneration()
    {
        string appendText="";

        foreach (var dna in this.ThrowingGA.GeneticAglorithm.Population)
        {
            appendText += dna.Fitness.ToString() + delimiter;
        }

        appendText += "\n";
        File.AppendAllText(path,appendText);
    }

    private void AlgorithmTerminated()
    {
        waitingForGAFinish = false;

    }

    private void WriteToCsvFile()
    {
        

       

        // This text is added only once to the file.
        if (!File.Exists(path))
        {
            // Create a file to write to.
            string createText = this.ThrowingGA.SelectionAlgorithm.gameObject.name + delimiter
                + this.ThrowingGA.CrossoverAlgorithm.gameObject.name + delimiter
                + this.ThrowingGA.Phenotype.gameObject.name + delimiter
                + this.ThrowingGA.ObstacleCourse.gameObject.name + delimiter
                + this.ThrowingGA.mutationRate + delimiter
                + this.ThrowingGA.BallCount + delimiter;
            createText += "\n";
            File.WriteAllText(path, createText);
        }

        // This text is always added, making the file longer over time
        // if it is not deleted.
        string appendText = this.ThrowingGA.SelectionAlgorithm.gameObject.name + delimiter
                                                                               + this.ThrowingGA.CrossoverAlgorithm.gameObject.name + delimiter
                                                                               + this.ThrowingGA.Phenotype.gameObject.name + delimiter
                                                                               + this.ThrowingGA.ObstacleCourse.gameObject.name + delimiter
                                                                               + this.ThrowingGA.mutationRate + delimiter 
                                                                               + this.ThrowingGA.BallCount + delimiter;
        appendText += "\n";

        File.AppendAllText(path, appendText);
    }
}
