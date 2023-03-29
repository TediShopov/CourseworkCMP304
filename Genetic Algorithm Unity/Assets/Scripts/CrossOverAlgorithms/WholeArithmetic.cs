
public class WholeArithmetic : FloatBasedRecombination
{
    public override DNA<float> Recombine(DNA<float> parent, DNA<float> otherParent)
    {
        DNA<float> child = new DNA<float>(parent);
        float A = GetRandomA();
        ArithmeticRecombination(child.Genes, 0, child.Genes.Length, parent.Genes, otherParent.Genes, A);
        return child;
    }
}