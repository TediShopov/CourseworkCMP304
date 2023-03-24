using System;

public class DNA<T> {
	public T[] Genes { get; private set; }
	public float Fitness { get; private set; }

	private Random random;
	private Func<T> getRandomGene;
	private Func<int, float> fitnessFunction;
    private Func<DNA<T>, DNA<T>, DNA<T>> crossoverFunction;


	public DNA(int size, Random random, Func<T> getRandomGene, Func<int, float> fitnessFunction, Func<DNA<T>, DNA<T>, DNA<T>> crossoverFunction= null, bool shouldInitGenes = true)
	{
		Genes = new T[size];
		this.random = random;
		this.getRandomGene = getRandomGene;
		this.fitnessFunction = fitnessFunction;
        this.crossoverFunction = crossoverFunction;

		if(shouldInitGenes)
		{
			for(int i = 0; i < Genes.Length; i++)
			{
				Genes[i] = getRandomGene();
			}
		}
	}

    public DNA(DNA<T> dna)
    {
        Genes = new T[dna.Genes.Length];
        this.random = dna.random;
        this.getRandomGene = dna.getRandomGene;
        this.fitnessFunction = dna.fitnessFunction;
        //if (shouldInitGenes)
        //{
        //    for (int i = 0; i < Genes.Length; i++)
        //    {
        //        Genes[i] = getRandomGene();
        //    }
        //}
    }


	public float CalculateFitness(int index)
	{
		Fitness = fitnessFunction(index);

		return Fitness;
	}


	public void Mutate(float mutationRate)
	{
		for(int i = 0; i < Genes.Length; i++)
		{
			if(random.NextDouble() < mutationRate)
			{
				Genes[i] = getRandomGene();
			}
		}
	}
}