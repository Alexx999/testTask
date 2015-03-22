using Microsoft.Owin.Security.DataProtection;

namespace TrackerWeb.Tests.Mocks
{
    public class TestDataProtection : IDataProtector
    {
        public byte[] Protect(byte[] userData)
        {
            return userData;
        }

        public byte[] Unprotect(byte[] protectedData)
        {
            return protectedData;
        }
    }
}