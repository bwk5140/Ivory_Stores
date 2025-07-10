using MethaWebsite.Data;
using Stripe;

namespace MethaWebsite.Services
{
    public class ProductRatingService
    {
        private readonly double rating;
        private const string FilledStarImage = "star-fill.svg";
        private const string ThreeQuarterStarImage = "star-three-quarter.svg";
        private const string HalfStarImage = "star-half.svg";
        private const string QuarterStarImage = "star-one-quarter.svg";
        private const string EmptyStarImage = "star.svg";


        public List<string> GetStarImages(double rating)
        {
            var stars = new List<string>();
            double remaining = rating;

            for (int i = 0; i < 5; i++)
            {
                if (remaining >= 1.0)
                {
                    stars.Add(FilledStarImage);
                    remaining -= 1.0;
                }
                else if (remaining >= 0.75)
                {
                    stars.Add(ThreeQuarterStarImage);
                    remaining = 0;
                }
                else if (remaining >= 0.5)
                {
                    stars.Add(HalfStarImage);
                    remaining = 0;
                }
                else if (remaining >= 0.25)
                {
                    stars.Add(QuarterStarImage);
                    remaining = 0;
                }
                else
                {
                    stars.Add(EmptyStarImage);
                }
            }

            return stars;
        }
    }
}
