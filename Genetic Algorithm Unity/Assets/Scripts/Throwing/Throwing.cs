using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Xml.Serialization;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
using Quaternion = UnityEngine.Quaternion;
using Random = System.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;


public class Throwing : MonoBehaviour
{
	[Header("Genetic Algorithm")]
	[Range(0,150)]
	[SerializeField] uint BallCount = 7;
	[SerializeField] float mutationRate = 0.01f;
	//[SerializeField] int innerCount = 40;
	//[SerializeField] float innerScale = 400.0f;
	[SerializeField] float timeScale = 1;

	

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

     private Vector3 _agentStartPosition=new Vector3(0,0,0) ;
     private GameObject _target;

    public GASelectionAlgorithmBase<float> SelectionAlgorithm;

    [Header(" Arithmetic Crossover Params")]
    //public float Alpha;
    public FloatBasedCrossoverAlgorithms.Type CrossoverAlgorithmType;
    public Vector2 ARange;
    public Vector2Int KRange;
    public FloatBasedRecombination CrossoverAlgorithm;


    public GeneticAglorithm<float> GeneticAglorithm;     // Genetic Aglorithm with each Gene being a float
	private BallAgentManager agentManager;      // Manager for handling a jumping agents we need to make
	private System.Random random;           // Random for the RNG
	private bool running = false;           // Flag for if the GeneticAglorithm is to run
	private float bestJump = 0;             // Store for the highest jump so far - Used for Mutation

    private int _maxIterations = 100;

	private int _currentIterations = 0;

    private float WorstThrowDistance = 1000;

    public float MaxImpulse = 15.0f;
    // Use this for initialization

    public float OverallBestFitnessAchieved = 0;
    public float[] OverallBestGenes=null;

    public GameObject PhenotypeGameObject;

    private FloatBasedRecombination InitCrossoverAlgorithm()
    {
        FloatBasedRecombination toReturn=null;
        //Get crossover algorithm 
        if (CrossoverAlgorithmType == FloatBasedCrossoverAlgorithms.Type.SinglePointArithmetic)
        {
            toReturn = new SingleArithmetic();
        }
        else if (CrossoverAlgorithmType == FloatBasedCrossoverAlgorithms.Type.SimplePointArithmetic)
        {
            toReturn = new SimpleArithmetic();

        }
        else if (CrossoverAlgorithmType == FloatBasedCrossoverAlgorithms.Type.WholeArithmetic)
        {
            toReturn = new WholeArithmetic();

        }

        toReturn.AlphaRange = ARange;
        toReturn.KRange = KRange;
        toReturn.Random=new Random();
        return toReturn;
    }

    void Start()
	{
		// Create the Random class
		random = new System.Random();

        // Create our Agent Manager and give them the height and width of grid along with agent prefab
        agentManager = new BallAgentManager(BallCount, _agentStartPosition, MaxImpulse, AgentPrefab);


        CrossoverAlgorithm = InitCrossoverAlgorithm();


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
        var pheno = PhenotypeGameObject.GetComponent<ShotPhenotypeRepresentation>();
        if (pheno!=null)
        {
            agentManager.Phenotype = pheno;

        }
        //agentManager.UpdateAgentThrowImpulse(GeneticAglorithm.Population);
        //agentManager.ResetBalls();
        //agentManager.ThrowBalls();
    }

    public float WeightOfClosesDistanceH = 20;
    public float ValueOfYClose = 15;


    private float GameScore(int index)
    {
        float score = 0;
        DNA<float> dna = GeneticAglorithm.Population[index];
        var ball = agentManager.BallAgents[index].GetComponent<AgentThrowableBall>();
        float closeToOptimalDistance = 10 - ball.ClosestDistanceReached;
        closeToOptimalDistance = Math.Clamp(closeToOptimalDistance, 0, 10);
        closeToOptimalDistance = Helpers.ConvertFromRange(closeToOptimalDistance, 0, 10, 0, 1);
      //  closeToOptimalDistance = Mathf.Pow(closeToOptimalDistance, 2.0f);


        var targetPosition = _target.transform.position;

        int hitMultiplier = ball.ScoreModifiersHit + 1;
        if (hitMultiplier>=2 && ball.IsHitTarget)
        {
            int a = 3;
        }

        if (ball.IsHitGround)
        {
            score = 0;
        }
       

        score += (closeToOptimalDistance*hitMultiplier) * (WeightOfClosesDistanceH);
        return score;
    }

    private float FFClosestPoint(int index)
    {
        float score = 0;
        DNA<float> dna = GeneticAglorithm.Population[index];
        var ball = agentManager.BallAgents[index].GetComponent<AgentThrowableBall>();
        float closeToOptimalDistance = 10 - ball.ClosestDistanceReached;
        closeToOptimalDistance = Math.Clamp(closeToOptimalDistance, 0, 10);
        closeToOptimalDistance = Helpers.ConvertFromRange(closeToOptimalDistance, 0, 20, 0, 1);
        closeToOptimalDistance = Mathf.Pow(closeToOptimalDistance, 2.0f);


        float scoreFromPassingY = 0.2f;
        var targetPosition = _target.transform.position;
        if (ball.BiggestYReached < targetPosition.y)
        {
            float worstDistance = MathF.Abs(_agentStartPosition.y - targetPosition.y);
            scoreFromPassingY =
                worstDistance - MathF.Abs(ball.transform.position.y - targetPosition.y);
            scoreFromPassingY = Math.Clamp(scoreFromPassingY, 0.2f, worstDistance);
            scoreFromPassingY = Helpers.ConvertFromRange(scoreFromPassingY, 0, worstDistance, 0, 1);
        }
        else
        {
            scoreFromPassingY = 1;
        }

        score += (closeToOptimalDistance * scoreFromPassingY) * (WeightOfClosesDistanceH + ValueOfYClose);
        return score;
    }

    private bool _initializedFirstGeneration = false;

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
        UpdateOverallBest();
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
                    bestJump = GeneticAglorithm.BestFitness;
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
