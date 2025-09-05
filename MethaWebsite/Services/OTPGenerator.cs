using OneOf.Types;
using System.Security.Cryptography;

namespace MethaWebsite.Services
{
    public class OTPGenerator
    {
        private static string? OTP { get; set; }
        private static bool Used { get; set; } = false;
        private static string GenerateOTP()
        {
            // Generate a random number between 0 and 999999
            byte[] bytes = new byte[4];
            RandomNumberGenerator.Fill(bytes);
            int value = BitConverter.ToInt32(bytes, 0) & 0x7FFFFFFF; // Ensure non-negative
            int otp = value % 1000000; // Limit to 6 digits

            OTP = otp.ToString("D6"); // Pad with leading zeros if necessary
            Used = false;
            return OTP;
        }
        public static string GetOTP()
        {
            if (OTP is null || Used)
                OTP = GenerateOTP();
            return OTP;
        }
        public void SetUsage()
        {
            Used = true;
        }
        public bool GetUsage()
        {
            return Used;
        }
        public bool IsOTPNull()
        {
            var returnVal = OTP is null ? true : false;
            return returnVal;
        }
    }
}
