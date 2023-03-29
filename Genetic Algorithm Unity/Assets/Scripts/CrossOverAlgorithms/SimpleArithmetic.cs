

public class SimpleArithmetic : FloatBasedRecombination
{
    public override DNA<float> Recombine(DNA<float> parent, DNA<float> otherParent)
    {
        DNA<float> child = new DNA<float>(parent);
        //Pick k form range Kmin Kmax
        int K = GetRandomK();
        //Eveything before k is copied from parent one, but it doesnt need to be done again as child is already 100% copy of parent in this stage
        float A = GetRandomA(); ArithmeticRecombination(child.Genes, K, child.Genes.Length, parent.Genes, otherParent.Genes, A);
        return child;
    }
}
