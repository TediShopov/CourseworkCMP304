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


    [Header("Text Labels")]
    [SerializeField] Text onGoingStatusText;
    [SerializeField] Text bestFitnessText;
    [SerializeField] Text overallBestFitnessText;

    [SerializeField] Text numGenText;

    [Header("Button Text")]
    [SerializeField] Text buttonText;

    [Header("Agent Prefab")]
    [SerializeField] public GameObject AgentPrefab;

    [SerializeField] public GameObject ObstacleCourse;



    public GeneticAglorithm<float> GeneticAglorithm;     // Genetic Aglorithm with each Gene being a float

    public ShotPhenotypeRepresentation Phenotype;
    public GASelectionAlgorithmBase<float> SelectionAlgorithm;
    public FloatBasedRecombination CrossoverAlgorithm;

    public float MaxImpulse = 15.0f;


    public float OverallBestFitnessAchieved = 0;
    public float[] OverallBestGenes = null;



    private BallAgentManager agentManager;      // Manager for handling a jumping agents we need to make
    private System.Random random;           // Random for the RNG
    private bool running = false;           // Flag for if the GeneticAglorithm is to run
    private bool _initializedFirstGeneration = false;


    private Vector3 _agentStartPosition = new Vector3(0, 0, 0);
    private GameObject _target;

    // Use this for initialization

    void OnDisable()
    {
      
        agentManager.Clear();
        Destroy(ObstacleCourse);
      
    }



    void OnEnable()
    {
        // Create the Random class
        random = new System.Random();

        // Create our Agent Manager and give them the height and width of grid along with agent prefab
        agentManager = new BallAgentManager(BallCount, _agentStartPosition, MaxImpulse, AgentPrefab);

        // Create genetic algorithm class
        GeneticAglorithm = new GeneticAglorithm<float>(
            agentManager.BallAgents.Count,
            3, random,
            GetRandomGene,
            GameScore,
            SelectionAlgorithm.SelectionStrategy,
            CrossoverAlgorithm.Recombine,
            mutationRate: mutationRate);
        SelectionAlgorithm.SetGeneticAlgorthm(GeneticAglorithm);

        ObstacleCourse = Instantiate(ObstacleCourse);
        this._target = GameObject.FindWithTag("Target");
        agentManager.TargetToSet = _target;

        //Try set phenotype
        var pheno = this.Phenotype;
        pheno.ThrowingGA = this;
        pheno.MaxImpulse = this.MaxImpulse;
        Phenotype.gameObject.SetActive(true);
        if (pheno != null)
        {

            agentManager.Phenotype = pheno;

        }

    }

    public float WeightOfClosesDistanceH = 20;
    public float ValueOfYClose = 15;


    private float GameScore(int index)
    {
        float score = 0;
        DNA<float> dna = GeneticAglorithm.Population[index];
        var ball = agentManager.BallAgents[index].GetComponent<AgentThrowableBall>();
        return GameScoreDebug(ball);
    }


    private float GameScoreDebug(AgentThrowableBall ballDebug)
    {
        float score = 0;
        float closeToOptimalDistance = 10 - ballDebug.ClosestDistanceReached;

        //if (ballDebug.IsHitTarget)
        //{
        //    closeToOptimalDistance = 1;
        //}
        //else
        //{
        //    closeToOptimalDistance = Math.Clamp(closeToOptimalDistance, 0, 10);
        //    closeToOptimalDistance = Helpers.ConvertFromRange(closeToOptimalDistance, 0, 10, 0, 1);
        //}

        //var targetPosition = _target.transform.position;

        //int hitMultiplier = ballDebug.ScoreModifiersHit + 1;

        //score += (closeToOptimalDistance * hitMultiplier) * (WeightOfClosesDistanceH);

        float hitMultiplier = Helpers.ConvertFromRange( ballDebug.ScoreModifiersHit + 1,0,4,0,1);
        //score = hitMultiplier * hitMultiplier;

        score = hitMultiplier * WeightOfClosesDistanceH;
        return score;
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

    void GenerationFinished()
    {
        Debug.Log($"Selection strategy of  generation {SelectionAlgorithm.gameObject.name}");
        this.gameObject.SetActive(false);

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
                agentManager.UpdateAgentThrowImpulse(GeneticAglorithm.Population);
                agentManager.ResetBalls();
                agentManager.ThrowBalls();
                _initializedFirstGeneration = true;
            }
            else
            {
                if (agentManager.AreAllBallsFinished())
                {
                    //WorstThrowDistance = GetWorstThrowDistance();
                    GeneticAglorithm.NewGeneration();
                    UpdateOverallBest();

                    if (GeneticAglorithm.Generation > MaxGenerations)
                    {
                        GenerationFinished();
                        return;
                    }
                    agentManager.UpdateAgentThrowImpulse(GeneticAglorithm.Population);
                    agentManager.ResetBalls();
                    agentManager.ThrowBalls();
                }
            }
        }
    }

    private float GetRandomGene()
    {
        return (float)random.NextDouble();
    }



    public float WeightOfAngle = 20.0f;
    public float WeightOfDistance = 20.0f;

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
        VisualizedBestShot = Instantiate(AgentPrefab);
        var ball = VisualizedBestShot.GetComponent<AgentThrowableBall>();
        this.agentManager.SetupShot(ball, OverallBestGenes);
        ball.Throw();
    }

    private Coroutine TestingFitnessCoroutine;
    IEnumerator TestFitnessAfter()
    {
        ResimulateThrow();
        yield return new WaitForSeconds(5.0f);
        GameScoreDebug(VisualizedBestShot.GetComponent<AgentThrowableBall>());
        StopCoroutine(TestingFitnessCoroutine);
        TestingFitnessCoroutine = null;
    }

    private void UpdateText()
    {

        if (onGoingStatusText)
        {
            onGoingStatusText.text = agentManager.AreAllBallsFinished().ToString();
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
