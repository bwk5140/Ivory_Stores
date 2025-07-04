using MethaWebsite.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Stripe;

namespace MethaWebsite.Services
{
    public class CheckoutService
    {
        public DayOfWeek? currentDay = DateTime.Now.DayOfWeek;
        public int CurrentMonth = DateTime.Now.Month;
        public int CurrentYear = DateTime.Now.Year;
        public int currentDate = DateTime.Now.Day;
        public double StandardShippingDays = 3;
        public double FreeShippingDays = 5;
        public double FastShippingDays = 1;
        public double Shipping { get; set; }
        public double FastShipping { get; set; }
        public double StandardShipping { get; set; }
        public string? ShippingType { get; set; }
        public double ShippingDistance { get; set; }
        public Dictionary<string, DayOfWeek>? ShippingDays = new();
        public Dictionary<string, string> ShippingMonths = new();
        public Dictionary<string, int> ShippingDates = new();
        public Dictionary<string, double> ShippingCosts = new();
        public readonly IStringLocalizer<CheckoutService> Loc;
        public string? FreeShipping { get; set; }
        public string? StandardShippng { get; set; }
        public string? FastShippng { get; set; }
        public Payment? PaymentMethod { get; set; }

        public CheckoutService(IStringLocalizer<CheckoutService> localizer)
        {
            Loc = localizer;
            ShippingDays["Free"] = DateTime.Now.AddDays(FreeShippingDays).DayOfWeek;
            ShippingMonths["Free"] = DateTime.Now.AddDays(FreeShippingDays).ToString("MMMM");
            ShippingDates["Free"] = DateTime.Now.AddDays(FreeShippingDays).Day;

            ShippingDays["Standard"] = DateTime.Now.AddDays(StandardShippingDays).DayOfWeek;
            ShippingMonths["Standard"] = DateTime.Now.AddDays(StandardShippingDays).ToString("MMMM");
            ShippingDates["Standard"] = DateTime.Now.AddDays(StandardShippingDays).Day;

            ShippingDays["Fast"] = DateTime.Now.AddDays(FastShippingDays).DayOfWeek;
            ShippingMonths["Fast"] = DateTime.Now.AddDays(FastShippingDays).ToString("MMMM");
            ShippingDates["Fast"] = DateTime.Now.AddDays(FastShippingDays).Day;

            FreeShipping = Loc["FreeDelivery"];
            StandardShippng = Loc["Standard"];
            FastShippng = Loc["Fast"];

            ShippingCosts[FreeShipping] = 0;
            ShippingCosts[StandardShippng] = 150;
            ShippingCosts[FastShippng] = 300;
            ShippingType = Loc["Standard"];
        }
        public double CalculateShippingCosts(string ShippingType)
        {
            return (ShippingDistance * 35) + ShippingCosts[ShippingType];
        }
    }
}
