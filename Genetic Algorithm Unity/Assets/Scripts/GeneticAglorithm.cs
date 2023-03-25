using System;
using System.Collections.Generic;

public class GeneticAglorithm<T> {

	public List<DNA<T>> Population { get; private set; }
	public int Generation { get; private set; }
	public float BestFitness { get; private set; }
	public T[] BestGenes { get; private set; }


	public float MutationRate;

	private Random random;
	public float FitnessSum;



    private Func<DNA<T>> ChooseParent;

    private Func<DNA<T>, DNA<T>, DNA<T>> CrossoverFunc;
	public GeneticAglorithm(int populationSize, int dnaSize, Random random, Func<T> getRandomGene, Func<int, float> fitnessFunction, Func<DNA<T>> chooseParent, Func<DNA<T>, DNA<T>, DNA<T>> crossoverFunc, float mutationRate = 0.01f)
    {
        Generation = 1;
        MutationRate = mutationRate;
        Population = new List<DNA<T>>();
        this.random = random;
        this.ChooseParent = chooseParent;
        this.CrossoverFunc = crossoverFunc;
        BestGenes = new T[dnaSize];

        for (int i = 0; i < populationSize; i++)
        {
            Population.Add(new DNA<T>(dnaSize, random, getRandomGene, fitnessFunction, crossoverFunc,shouldInitGenes: true));
        }
    }

    public DNA<T> CrossoverDefault(DNA<T> current, DNA<T> otherParent)
    {
        DNA<T> child = new DNA<T>(current);

        for (int i = 0; i < child.Genes.Length; i++)
        {
            child.Genes[i] = random.NextDouble() < 0.5 ? current.Genes[i] : otherParent.Genes[i];
        }

        return child;
    }

	public void NewGeneration()
	{
		if(Population.Count <= 0) {
			return;
		}

		CalculateFitness();

		List<DNA<T>> newPopulation = new List<DNA<T>>();

		for(int i = 0; i < Population.Count; i++)
		{
			DNA<T> parent1 = ChooseParent();
			DNA<T> parent2 = ChooseParent();


            DNA<T> child = this.CrossoverFunc(parent1, parent2);

			child.Mutate(MutationRate);

			newPopulation.Add(child);
		}

		Population = newPopulation;

		Generation++;
	}

	public void CalculateFitness()
	{
		FitnessSum = 0;

		DNA<T> best = Population[0];

		for(int i = 0; i < Population.Count; i++)
		{
			FitnessSum += Population[i].CalculateFitness(i);

			if(Population[i].Fitness > best.Fitness)
			{
				best = Population[i];
			}
		}

		BestFitness = best.Fitness;
		best.Genes.CopyTo(BestGenes, 0);
	}

	//private DNA<T> ChooseParent()
	//{
	//	double randomNumber = random.NextDouble() * fitnessSum;

	//	for(int i = 0; i < Population.Count; i++)
	//	{
	//		if( randomNumber < Population[i].Fitness)
	//		{
	//			return Population[i];
	//		}

	//		randomNumber -= Population[i].Fitness;
	//	}

	//	return Population[random.Next(0, Population.Count)];
	//}
}
