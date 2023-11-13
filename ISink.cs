namespace AudioPlayground
{
    interface ISink
    {
        void Write(double[] buffer);
    }
}