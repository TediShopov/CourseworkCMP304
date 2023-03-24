using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class BallAgentManager 
{
	public List<GameObject> BallAgents;
    public Vector3 BallStartPosition;
    public float MaxImpulse;
    public GameObject TargetToSet;
	public BallAgentManager(uint numberOfBalls,Vector3 startPosition, float maxImpulse,GameObject agentPrefab)
    {
        this.MaxImpulse=maxImpulse;
        ;
        BallStartPosition = startPosition;
        BallAgents = new List<GameObject>();
        for (int i = 0; i < numberOfBalls; i++)
        {
            var ballAdd = GameObject.Instantiate(agentPrefab, startPosition, new Quaternion());
            ballAdd.GetComponent<AgentThrowableBall>().Target = GameObject.FindGameObjectWithTag("Target");
            BallAgents.Add(GameObject.Instantiate(agentPrefab, startPosition, new Quaternion()));
		}
    }

	public bool AreAllBallsFinished()
	{
		foreach (GameObject agent in BallAgents)
		{
            AgentThrowableBall ballScript = agent.GetComponent<AgentThrowableBall>();
			//All balls have to have hit a surfac eor the ring of the basket
			if (ballScript && !ballScript.IsHitSurfaceOrScored)
			{
				return false;
			}
		}

		return true;
	}

	public void ResetBallPositions()
	{
        foreach (var ball in BallAgents)
        {
            ball.transform.position = BallStartPosition;
            ball.GetComponent<AgentThrowableBall>().ResetHitSurface();
            ball.GetComponent<AgentThrowableBall>().Target = TargetToSet;
        }
	}

	public void ThrowBalls()
	{
		foreach (GameObject agent in BallAgents)
		{
			AgentThrowableBall ballScript = agent.GetComponent<AgentThrowableBall>();
			if (ballScript)
			{
                ballScript.Throw();
			}
		}
	}

 

    public Vector3 CalculateThrowImpulse(float[] genes)
    {
        Vector3 throwImpulse=new Vector3();
        ShotInfo shotInfo = new ShotInfo(genes, MaxImpulse);
        throwImpulse = shotInfo.Rotation.normalized * Vector3.forward;
        throwImpulse *= shotInfo.InitialImpulse;
        return throwImpulse;
    }



    public void UpdateAgentThrowImpulse(List<DNA<float>> genomes)
	{

        for (int i = 0; i < genomes.Count; i++)
		{
			DNA<float> dna = genomes[i];
			GameObject agent = BallAgents[i];


			AgentThrowableBall script = agent.GetComponent<AgentThrowableBall>();
			if (script)
			{
				script.ThrowImpulse = CalculateThrowImpulse(dna.Genes);
			}
		}
	}

	

}
