using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Threading;
using System.Xml.Serialization;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Experimental.AI;
using UnityEngine.UI;
using static Throwing;
using Quaternion = UnityEngine.Quaternion;
using Random = System.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;


public class Throwing : MonoBehaviour
{
    [Header("Genetic Algorithm")]
    [Range(0, 150)]
    [SerializeField] public uint BallCount = 7;
    [SerializeField] public float mutationRate = 0.01f;
    //[SerializeField] int innerCount = 40;
    //[SerializeField] float innerScale = 400.0f;
    [SerializeField] public float timeScale = 1;
    [SerializeField] public int MaxGenerations = 150;

    public ShotPhenotypeRepresentation Phenotype;
    public GASelectionAlgorithmBase<float> SelectionAlgorithm;
    public FloatBasedRecombination CrossoverAlgorithm;
    public FitnessFunctionStrategyBase FitnessFunctionStrategy;
    [SerializeField] public GameObject ObstacleCourse;




    [Header("Text Labels")]
    [SerializeField] Text onGoingStatusText;
    [SerializeField] Text bestFitnessText;
    [SerializeField] Text overallBestFitnessText;

    [SerializeField] Text numGenText;

    [Header("Button Text")]
    [SerializeField] Text buttonText;

    [Header("Agent Prefab")]
    //[SerializeField] public GameObject AgentPrefab;




    public GeneticAglorithm<float> GeneticAglorithm;     // Genetic Aglorithm with each Gene being a float

  

    public float MaxImpulse = 15.0f;





    public BallAgentManager AgentManager;      // Manager for handling a jumping agents we need to make
    private System.Random random;           // Random for the RNG
    private bool running = false;           // Flag for if the GeneticAglorithm is to run
    private bool _initializedFirstGeneration = false;


    private Vector3 _agentStartPosition = new Vector3(0, 0, 0);
    private GameObject _target;

    public delegate void GenerationEnd();
    public GenerationEnd OnGenerationEnd;

    public delegate void AlgorithmTerminated();
    public AlgorithmTerminated OnAlgorithmTerminated;

    [Header("Debug")]
    public float OverallBestFitnessAchieved = 0;
    public float[] OverallBestGenes = null;


    // Use this for initialization

    void OnDisable()
    {
      
        AgentManager.Clear();
        DestroyImmediate(ObstacleCourse);
        _initializedFirstGeneration = false;

    }

   


    void OnEnable()
    {
       
        // Create the Random class
        random = new System.Random();

        // Create our Agent Manager and give them the height and width of grid along with agent prefab
        AgentManager = new BallAgentManager(BallCount, _agentStartPosition, MaxImpulse, FitnessFunctionStrategy.BallScript.gameObject);

        // Create genetic algorithm class
        GeneticAglorithm = new GeneticAglorithm<float>(
            AgentManager.BallAgents.Count,
            3, random,
            GetRandomGene,
            FitnessFunctionStrategy.FitnessFunctionByIndex,
            SelectionAlgorithm.SelectionStrategy,
            CrossoverAlgorithm.Recombine,
            mutationRate: mutationRate);
        SelectionAlgorithm.SetGeneticAlgorthm(GeneticAglorithm);
        FitnessFunctionStrategy.ThrowingGA = this;
        ObstacleCourse = Instantiate(ObstacleCourse);
        this._target = GameObject.FindWithTag("Target");
        AgentManager.TargetToSet = _target;

        //Try set phenotype
        var pheno = this.Phenotype;
        pheno.ThrowingGA = this;
        pheno.MaxImpulse = this.MaxImpulse;
        Phenotype.gameObject.SetActive(true);
        if (pheno != null)
        {

            AgentManager.Phenotype = pheno;

        }

    }

   

    void UpdateOverallBest()
    {
        if (GeneticAglorithm.BestFitness > OverallBestFitnessAchieved)
        {
            var bestGenes = GeneticAglorithm.BestGenes;
            OverallBestGenes = new float[bestGenes.Length];
            for (int i = 0; i < bestGenes.Length; i++)
            {
                OverallBestGenes[i] = bestGenes[i];
            }

            OverallBestFitnessAchieved = GeneticAglorithm.BestFitness;
        }
    }

   

    // Update is called once per frame
    void Update()
    {

        // Update time scale based on Editor value - do this every frame so we capture changes instantly
        Time.timeScale = timeScale;
        UpdateText();



        if (running)
        {
            if (!_initializedFirstGeneration)
            {
                AgentManager.UpdateAgentThrowImpulse(GeneticAglorithm.Population);
                AgentManager.ResetBalls();
                AgentManager.ThrowBalls();
                _initializedFirstGeneration = true;
            }
            else
            {
                if (AgentManager.AreAllBallsFinished())
                {
                    //WorstThrowDistance = GetWorstThrowDistance();

                    GeneticAglorithm.CalculateFitness();
                    if (this.OnGenerationEnd != null)
                    {
                        this.OnGenerationEnd.Invoke();
                    }

                    UpdateOverallBest();

                    if (GeneticAglorithm.Generation > MaxGenerations)
                    {
                        if (this.OnAlgorithmTerminated!=null)
                        {
                            this.OnAlgorithmTerminated.Invoke();
                            //running=false;
                        }
                        //this.gameObject.SetActive(false);
                        return;
                    }
                    GeneticAglorithm.NewGeneration();
                   
                    AgentManager.UpdateAgentThrowImpulse(GeneticAglorithm.Population);
                    AgentManager.ResetBalls();
                    AgentManager.ThrowBalls();
                }
            }
        }
    }

    private float GetRandomGene()
    {
        return (float)random.NextDouble();
    }

    public void onButtonClickUnique()
    {
        // Pause and Start the algorithm
        if (running)
        {
            running = false;
            if (buttonText)
            {
                buttonText.text = "Start";
            }
        }
        else
        {
            running = true;
            if (buttonText)
            {
                buttonText.text = "Pause";
            }
        }
    }

    private GameObject VisualizedBestShot = null;
    public void resimulateBestOverall()
    {
        if (TestingFitnessCoroutine == null)
        {
            TestingFitnessCoroutine = StartCoroutine(TestFitnessAfter());
        }

    }

    void ResimulateThrow()
    {
        if (VisualizedBestShot != null)
        {
            DestroyImmediate(VisualizedBestShot);
        }
        VisualizedBestShot = Instantiate(FitnessFunctionStrategy.BallScript.gameObject);
        var ball = VisualizedBestShot.GetComponent<ThrowableBallBase>();
        this.AgentManager.SetupShot(ball, OverallBestGenes);
        ball.Throw();
    }

    private Coroutine TestingFitnessCoroutine;
    IEnumerator TestFitnessAfter()
    {
        ResimulateThrow();
        yield return new WaitForSeconds(5.0f);
        FitnessFunctionStrategy.FitnessFunction(VisualizedBestShot.GetComponent<ThrowableBallBase>());
        //GameScoreDebug(VisualizedBestShot.GetComponent<AgentThrowableBall>());
        StopCoroutine(TestingFitnessCoroutine);
        TestingFitnessCoroutine = null;
    }

    private void UpdateText()
    {

        if (onGoingStatusText)
        {
            if (AgentManager.BallAgents.Count>0)
            {
                onGoingStatusText.text = AgentManager.AreAllBallsFinished().ToString();

            }
        }

        if (bestFitnessText)
        {
            bestFitnessText.text = GeneticAglorithm.BestFitness.ToString("F");
        }

        if (numGenText)
        {
            numGenText.text = GeneticAglorithm.Generation.ToString();
        }

        if (overallBestFitnessText)
        {
            overallBestFitnessText.text = $"Overall best fitness: {OverallBestFitnessAchieved}";
        }
    }
}
