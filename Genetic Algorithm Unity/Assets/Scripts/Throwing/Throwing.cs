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


public class ShotInfo
{
    public Quaternion Rotation { get; set; }
	public float InitialImpulse { get;  set; }

    public float MaxImpulse { get; set; } = 1.0f;

    
     

    public ShotInfo(float[] FloatGenes, float maxImpulse)
    {
        this.MaxImpulse = maxImpulse;
        float[] values = new float[FloatGenes.Length];
        for (int i = 0; i < FloatGenes.Length-1; i++)
        {
            values[i] = Helpers.ConvertFromRange(FloatGenes[i],0,1,-90,90);
        }

        values[values.Length - 1] = FloatGenes[FloatGenes.Length - 1];


        //for (int i = 0; i < 3; i++)
        //{
        //    values[i] = Throwing.Helpers.ConvertFromRange(values[i], 0.0f, 1.0f, -180.0f, 180.0f);
        //}

        //Set the rotation using the first 4 values
        //this.Rotation = new Quaternion(FloatGenes[0], FloatGenes[1], FloatGenes[2], Math.Clamp(FloatGenes[3],0,1)).normalized;
        //  this.Rotation = new Quaternion(FloatGenes[0], FloatGenes[1], FloatGenes[2], FloatGenes[3]).normalized;
        Vector3 eulerAngles = new Vector3(values[0], values[1], 0);
        this.Rotation = Quaternion.Euler(eulerAngles).normalized;

        this.InitialImpulse = values[values.Length-1] * MaxImpulse;
        if (this.InitialImpulse> MaxImpulse)
        {
            int a = 3;
        }
    }

    public float[] GetGenes()
    {
        var toReturn = new List<float>();
        //toReturn.Add(this.Rotation.x);
        //toReturn.Add(this.Rotation.y);
        //toReturn.Add(this.Rotation.z);
        //toReturn.Add(this.Rotation.w);


        var euler = this.Rotation.eulerAngles;

        if (euler.x < 0 || euler.y <0 || euler.z < 0)
        {
            int a = 3;
        }


       toReturn.Add(Helpers.ConvertFromRange(euler.x,-90,90,0,1));
       toReturn.Add(Helpers.ConvertFromRange(euler.y, -90, 90, 0, 1));

        // toReturn.Add(euler.z / 360.0f);

        //toReturn.Add(this.Rotation.w);
        toReturn.Add(this.InitialImpulse/ MaxImpulse);
		return toReturn.ToArray();
    }
}

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
    [SerializeField] Text bestDistanceWeighed;
    [SerializeField] Text bestAngleWeighed;
    [SerializeField] Text bestJumpStrength;
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
            FFClosestPoint, 
            SelectionAlgorithm.SelectionStrategy,
            CrossoverAlgorithm.Recombine,
            mutationRate: mutationRate);
        SelectionAlgorithm.SetGeneticAlgorthm(GeneticAglorithm);

        ObstacleCourse = Instantiate(ObstacleCourse);
        this._target = GameObject.FindWithTag("Target");
        agentManager.TargetToSet = _target;
        agentManager.UpdateAgentThrowImpulse(GeneticAglorithm.Population);
        agentManager.ResetBalls();
        agentManager.ThrowBalls();
    }

    public float WeightOfClosesDistanceH = 20;
    public float ValueOfYClose = 15;
  

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


    // Update is called once per frame
    void Update()
	{

		// Update time scale based on Editor value - do this every frame so we capture changes instantly
		Time.timeScale = timeScale;
		UpdateText();
        

		if (running)
		{
            //_currentIterations++;
            //if (_currentIterations >= _maxIterations)
            //{
            //    running = false;
            //}

			// If the agents are currently jumping, stop the script if the ALL enter the DeadZone
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
         var bestGenes= GeneticAglorithm.BestGenes;
        
         var bestThrowInfo = new ShotInfo(bestGenes,MaxImpulse);
        // If the script has been passed a valid Text object then update that text
        if (onGoingStatusText)
		{
			onGoingStatusText.text = agentManager.AreAllBallsFinished().ToString();
		}

        if (bestAngleWeighed)
        {
            bestAngleWeighed.text = "Angle Of Best" + (bestThrowInfo.Rotation * Vector3.forward).ToString();
        }

        if (bestDistanceWeighed)
        {
            bestDistanceWeighed.text = "Impulse Of Best" + bestThrowInfo.InitialImpulse.ToString();
        }

        if (bestFitnessText)
		{
			bestFitnessText.text = GeneticAglorithm.BestFitness.ToString("F");
		}

		if (numGenText)
		{
			numGenText.text = GeneticAglorithm.Generation.ToString();
		}

		if (bestJumpStrength)
		{
			
            Vector3 throwImpulse = agentManager.CalculateThrowImpulse(GeneticAglorithm.BestGenes);

			bestJumpStrength.text = throwImpulse.ToString();
		}
	}
}
