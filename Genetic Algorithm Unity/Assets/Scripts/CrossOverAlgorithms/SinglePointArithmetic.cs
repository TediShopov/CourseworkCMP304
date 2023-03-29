
public class SinglePointArithmetic : FloatBasedRecombination
{
    public override DNA<float> Recombine(DNA<float> parent, DNA<float> otherParent)
    {
        DNA<float> child = new DNA<float>(parent);
        //Pick k form range Kmin Kmax
        int K = GetRandomK();
        float A = GetRandomA();

        //only one index will be arithmetically recombined
        ArithmeticRecombination(child.Genes, K, K + 1, parent.Genes, otherParent.Genes, A);
        return child;
    }
}
