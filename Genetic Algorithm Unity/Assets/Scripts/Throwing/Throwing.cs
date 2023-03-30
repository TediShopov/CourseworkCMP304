using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
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
    [SerializeField] Text bestOverallFitnesses;
    [SerializeField] Text bestOverallScores;

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
    //public float OverallBestFitnessAchieved = 0;
    //public float[] OverallBestGenes = null;

    public List<DNA<float>> TopNOverallBest=new List<DNA<float>>();
    public List<DNA<float>> ScoredNOverallBest = new List<DNA<float>>();

    [Range(1,50)]public int NumberOfOverallBestShotsToStore;

    void UpdateTopNOverallBestList()
    {
        //search through the wehole population
        float minimumAcquiredFitnessValue = 0;
        if (TopNOverallBest.Count > 0)
        {
            minimumAcquiredFitnessValue = TopNOverallBest.Min(x => x.Fitness);
        }

        foreach (var dna in this.GeneticAglorithm.Population)
        {
            if (dna.Fitness > minimumAcquiredFitnessValue)
            {
                TopNOverallBest.Add(dna);
            }
        }

        //Make sure sort is in ascending order
        TopNOverallBest.Sort((a, b) => b.Fitness.CompareTo(a.Fitness));
        TopNOverallBest = TopNOverallBest.Take(NumberOfOverallBestShotsToStore).ToList();
    }

    void UpdateScoredShotsTopNBestList()
    {
        //search through the wehole population
        float minimumAcquiredFitnessValue = 0;
        if (ScoredNOverallBest.Count > 0)
        {
            minimumAcquiredFitnessValue = ScoredNOverallBest.Min(x => x.Fitness);
        }


        for (int i = 0; i < this.AgentManager.BallAgents.Count; i++)
        {
            var throwable = this.AgentManager.BallAgents[i].GetComponent<ThrowableBallBase>();
            if (throwable.IsHitTarget)
            {
                var dnaOfThrowable = this.GeneticAglorithm.Population[i];
                if (dnaOfThrowable.Fitness > minimumAcquiredFitnessValue)
                {
                    ScoredNOverallBest.Add(dnaOfThrowable);
                }
            }
        }

       

        //Make sure sort is in ascending order
        ScoredNOverallBest.Sort((a, b) => b.Fitness.CompareTo(a.Fitness));
        ScoredNOverallBest = ScoredNOverallBest.Take(NumberOfOverallBestShotsToStore).ToList();
    }

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

                  //  UpdateOverallBest();
                    UpdateTopNOverallBestList();
                    UpdateScoredShotsTopNBestList();
                    if (GeneticAglorithm.Generation > MaxGenerations)
                    {
                        if (this.OnAlgorithmTerminated!=null)
                        {
                            this.OnAlgorithmTerminated.Invoke();
                            this.gameObject.SetActive(false);
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

    private List<GameObject> VisualizedShots = new List<GameObject>();
    public void resimulateBestOverall()
    {
        if (TestingFitnessCoroutine == null)
        {
            TestingFitnessCoroutine = StartCoroutine(TestFitnessAfter(TopNOverallBest));
        }

    }

    public void resimulateBestScored()
    {
        if (TestingFitnessCoroutine == null)
        {
            TestingFitnessCoroutine = StartCoroutine(TestFitnessAfter(ScoredNOverallBest));
        }

    }

    void ResimulateBestFitnessThrows(List<DNA<float>> collection)
    {
        if (VisualizedShots.Count != 0)
        {
            foreach (var shot in this.VisualizedShots)
            {
                DestroyImmediate(shot);
            }
        }

        foreach (var shotDna in collection)
        {
            var ballObj = Instantiate(FitnessFunctionStrategy.BallScript.gameObject);
            VisualizedShots.Add(ballObj);
            var ball = ballObj.GetComponent<ThrowableBallBase>();
            this.AgentManager.SetupShot(ball, shotDna.Genes);
            ball.Throw();
        }


      
    }

   

    private Coroutine TestingFitnessCoroutine;
    IEnumerator TestFitnessAfter(List<DNA<float>> collection)
    {
        ResimulateBestFitnessThrows(collection);
        yield return new WaitForSeconds(5.0f);
        //FitnessFunctionStrategy.FitnessFunction(VisualizedBestShot.GetComponent<ThrowableBallBase>());
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

        if (bestOverallFitnesses)
        {
            if (TopNOverallBest.Count!=0)
            {
                bestOverallFitnesses.text = "";


                int maxIterationCount = Math.Min(TopNOverallBest.Count, Math.Min(NumberOfOverallBestShotsToStore, 5));
                for (int i = 0; i < maxIterationCount; i++)
                {
                    bestOverallFitnesses.text += $"{TopNOverallBest[i].Fitness.ToString("0.00")},  ";
                }

               
            }
        }


        if (bestOverallScores)
        {
            if (ScoredNOverallBest.Count != 0)
            {
                bestOverallScores.text = "";


                int maxIterationCount = Math.Min(ScoredNOverallBest.Count, Math.Min(NumberOfOverallBestShotsToStore, 5));
                for (int i = 0; i < maxIterationCount; i++)
                {
                    bestOverallScores.text += $"{ScoredNOverallBest[i].Fitness.ToString("0.00")},  ";
                }


            }
        }
    }
}
