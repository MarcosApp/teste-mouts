namespace Ambev.DeveloperEvaluation.Domain.ValueObjects;

public class Rating
{
    public double Rate { get; private set; }
    public int Count { get; private set; }

    protected Rating() { }

    public Rating(double rate, int count)
    {
        if (rate < 0 || rate > 5)
            throw new ArgumentException("Rate must be between 0 and 5.");
        if (count < 0)
            throw new ArgumentException("Count must be non-negative.");

        Rate = rate;
        Count = count;
    }
}
