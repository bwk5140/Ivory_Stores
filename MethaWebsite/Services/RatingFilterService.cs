using MethaWebsite.Data;

namespace MethaWebsite.Services
{
    public class RatingFilterService
    {
        public List<Product> FilterByRating(List<Product> products, List<Rating> ratings, int clickedIndex)
        {
            bool changed = false;
            var filtered = new List<Product>(products);

            if (!ratings[clickedIndex].Selected)
            {
                for (int i = clickedIndex; i >= 0; i--)
                {
                    if (!ratings[i].Selected)
                    {
                        if (!changed)
                            filtered = filtered.Where(p => p.Rating == i + 1).ToList();

                        ratings[i].Selected = true;
                        changed = true;
                    }
                }
            }
            else
            {
                for (int i = clickedIndex; i < ratings.Count; i++)
                {
                    if (ratings[i].Selected)
                    {
                        if (!changed && i > 0)
                            filtered = filtered.Where(p => p.Rating == i).ToList();

                        ratings[i].Selected = false;
                        changed = true;
                    }
                }
            }

            return filtered;
        }
        public List<Rating> FilterByRating(List<Rating> ratings, int clickedIndex)
        {
            bool changed = false;

            if (!ratings[clickedIndex].Selected)
            {
                for (int i = clickedIndex; i >= 0; i--)
                {
                    if (!ratings[i].Selected)
                    {
                        ratings[i].Selected = true;
                        changed = true;
                    }
                }
            }
            else
            {
                for (int i = clickedIndex; i < ratings.Count; i++)
                {
                    if (ratings[i].Selected)
                    {
                        ratings[i].Selected = false;
                        changed = true;
                    }
                }
            }
            return ratings;
        }
    }
}
