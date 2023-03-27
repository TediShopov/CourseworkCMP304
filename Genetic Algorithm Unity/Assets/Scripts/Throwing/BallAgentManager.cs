using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class BallAgentManager 
{
	public List<GameObject> BallAgents;
    public Vector3 BallStartPosition;
    public float MaxImpulse;
    public GameObject TargetToSet;
    public float SecondsAlive = 10.0f;

    public ShotPhenotypeRepresentation Phenotype;
	public BallAgentManager(uint numberOfBalls,Vector3 startPosition, float maxImpulse,GameObject AgentPrefab)
    {
        this.MaxImpulse=maxImpulse;
        ;
        BallStartPosition = startPosition;
        BallAgents = new List<GameObject>();
        for (int i = 0; i < numberOfBalls; i++)
        {
            var ballAdd = GameObject.Instantiate(AgentPrefab, startPosition, new Quaternion());
            ballAdd.GetComponent<AgentThrowableBall>().Target = GameObject.FindGameObjectWithTag("Target");
            BallAgents.Add(ballAdd);
		}
    }

	public bool AreAllBallsFinished()
	{
		foreach (GameObject agent in BallAgents)
		{
            AgentThrowableBall ballScript = agent.GetComponent<AgentThrowableBall>();
			//All balls have to have hit a surfac eor the ring of the basket
			if (ballScript && ballScript.IsAcitve)
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
            var ball = ballObj.GetComponent<AgentThrowableBall>();
           
            ball.ResetBall();
            ball.transform.position = BallStartPosition;
            ball.SecondBeforeDisable = SecondsAlive;
            ball.Target = TargetToSet;
			
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
        AgentThrowableBall ballScript = agent.GetComponent<AgentThrowableBall>();
        if (ballScript)
        {
            ballScript.Throw();
        }
    }


    void UpdateThrowImpulse(GameObject agent, DNA<float> dna)
    {
        AgentThrowableBall script = agent.GetComponent<AgentThrowableBall>();
        if (script)
        {
            Phenotype.DecodeGenes(dna.Genes);
            script.ThrowImpulse = Phenotype.ShotImpulse;
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
