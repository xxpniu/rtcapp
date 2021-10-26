using System;

namespace RTC.XNet
{
    public class ResultException:Exception
    {
        public int Error { set; get; } = -1;
        
        
        public ResultException(int error)
        {
            Error = error;
        }

        public ResultException(int error, string message):base(message)
        {
             
        }

        public ResultException(string message) : base(message)
        {
        }
    }
}