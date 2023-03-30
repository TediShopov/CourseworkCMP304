using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.PlayerLoop;
using static UnityEngine.GraphicsBuffer;

public class BallAgentManager 
{
	public List<GameObject> BallAgents;
    public Vector3 BallStartPosition;
    public float MaxImpulse;
    public GameObject TargetToSet;
    public float SecondsAlive = 10.0f;

    public ShotPhenotypeRepresentation Phenotype;
    public GameObject VisualizedBestShot;

    void ResetBall(ThrowableBallBase ball)
    {
        ball.Reset();
        ball.transform.position = BallStartPosition;
    }

    public void Clear()
    {
        foreach (var ball in this.BallAgents)
        {
            GameObject.Destroy(ball);
        }
        this.BallAgents.Clear();
    }

    public void SetupShot(ThrowableBallBase ball,float[] genes)
    {
        ResetBall(ball);
        Phenotype.DecodeGenes(genes);
        ball.ShotImpulse = Phenotype.ShotImpulse;
        //ball.Throw();
    }

    public BallAgentManager(uint numberOfBalls,Vector3 startPosition, float maxImpulse,GameObject AgentPrefab)
    {
        this.MaxImpulse=maxImpulse;
        ;
        BallStartPosition = startPosition;
        BallAgents = new List<GameObject>();
        for (int i = 0; i < numberOfBalls; i++)
        {
            var ballAdd = GameObject.Instantiate(AgentPrefab, startPosition, new Quaternion());
            //ballAdd.GetComponent<ThrowableBallBase>().Target = GameObject.FindGameObjectWithTag("Target");
            BallAgents.Add(ballAdd);
		}
    }

	public bool AreAllBallsFinished()
	{
		foreach (GameObject agent in BallAgents)
		{
            ThrowableBallBase ballScript = agent.GetComponent<ThrowableBallBase>();
			//All balls have to have hit a surfac eor the ring of the basket
			if (ballScript && ballScript.IsActive)
			{
				return false;
			}
		}

		return true;
	}

	public void ResetBalls()
	{
        foreach (var ballObj in BallAgents)
        {
            var ball = ballObj.GetComponent<ThrowableBallBase>();
            ResetBall(ball);
        }
	}

	public void ThrowBalls()
	{
		foreach (GameObject agent in BallAgents)
        {
            ThrowBallObject(agent);
        }
	}

    public  void ThrowBallObject(GameObject agent)
    {
        ThrowableBallBase ballScript = agent.GetComponent<ThrowableBallBase>();
        if (ballScript)
        {
            ballScript.Throw();
        }
    }


    void UpdateThrowImpulse(GameObject agent, DNA<float> dna)
    {
        ThrowableBallBase script = agent.GetComponent<ThrowableBallBase>();
        if (script)
        {
            Phenotype.DecodeGenes(dna.Genes);
            script.ShotImpulse = Phenotype.ShotImpulse;
        }
    }



    public void UpdateAgentThrowImpulse(List<DNA<float>> genomes)
	{

        for (int i = 0; i < genomes.Count; i++)
		{
			DNA<float> dna = genomes[i];
			GameObject agent = BallAgents[i];
            UpdateThrowImpulse(agent,dna);
		}
	}

	

}
