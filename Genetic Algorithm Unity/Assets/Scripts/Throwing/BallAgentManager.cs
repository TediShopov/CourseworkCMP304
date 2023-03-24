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
	public BallAgentManager(uint numberOfBalls,Vector3 startPosition, float maxImpulse,GameObject agentPrefab)
    {
        this.MaxImpulse=maxImpulse;
        ;
        BallStartPosition = startPosition;
        BallAgents = new List<GameObject>();
        for (int i = 0; i < numberOfBalls; i++)
        {
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

    //  public Vector3 CalculateThrowImpulse(float[] genes)
    //  {
    //      if (genes.Length%3 !=0)
    //      {
    //          throw new ArgumentException();
    //      }

    //      Vector3 throwImpulse = new Vector3();

    //      int rangeOfGenomesToVectorComponent = genes.Length / 3;


    ////15 -- >
    ////0  -4
    //for (int x = rangeOfGenomesToVectorComponent*0; x < rangeOfGenomesToVectorComponent * 1; x++)
    //      {
    //          throwImpulse.x += genes[x];
    //      }
    ////5--9
    //      for (int y = rangeOfGenomesToVectorComponent * 1; y < rangeOfGenomesToVectorComponent * 2; y++)
    //      {
    //          throwImpulse.y += genes[y];
    //      }
    ////10,11,12,13,14
    //      for (int z = rangeOfGenomesToVectorComponent * 2; z < rangeOfGenomesToVectorComponent * 3; z++)
    //      {
    //          throwImpulse.z += genes[z];
    //      }

    //      throwImpulse /= rangeOfGenomesToVectorComponent;


    //return throwImpulse;
    //  }

    //public Vector3 CalculateThrowImpulse(float[] genes)
    //   {
    //       if (genes.Length % 3 != 0)
    //       {
    //           throw new ArgumentException();
    //       }


    //       int rangeOfGenomesToVectorComponent = genes.Length / 3;

    //	Vector3 throwImpulse = new Vector3();


    //       //XZ direction - ordered by relevance
    //       float angleXZ = 0;
    //       for (int i = rangeOfGenomesToVectorComponent*0; i < rangeOfGenomesToVectorComponent; i++)
    //       {

    //		//
    //		//In -0.5 to 0.5 range
    //		float angleFromGenome = genes[i] - 0.5f;
    //		//Multiply by 360 / x angle
    //           angleFromGenome *= 360.0f / rangeOfGenomesToVectorComponent;
    //           angleXZ += angleFromGenome;
    //       }

    //	//XY direction degress - ordered by relevance [ from -360/x to 360 /x ] [ <- /2] [-< /2]
    //       float angleXy =0;

    //       for (int i = rangeOfGenomesToVectorComponent * 1; i < rangeOfGenomesToVectorComponent*2; i++)
    //       {

    //           //
    //           //In -0.5 to 0.5 range
    //           float angleFromGenome = genes[i] - 0.5f;
    //           //Multiply by 360 / x angle
    //           angleFromGenome *= 90.0f / rangeOfGenomesToVectorComponent;
    //           angleXy += angleFromGenome;
    //       }

    //	throwImpulse.x = Mathf.Cos(angleXZ) * Mathf.Cos(angleXy);
    //	throwImpulse.z = Mathf.Sin(angleXZ) * Mathf.Cos(angleXy);
    //	throwImpulse.y = Mathf.Sin(angleXy);


    //       float force = genes.Skip(rangeOfGenomesToVectorComponent*2).Take(rangeOfGenomesToVectorComponent).Sum()*5;

    //       throwImpulse *= force;

    //	return throwImpulse;
    //   }

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
