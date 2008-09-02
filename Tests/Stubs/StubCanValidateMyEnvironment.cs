using System;
using System.Collections.Generic;
using System.Text;
using SvnBridge.Interfaces;

namespace Tests
{
    public class StubCanValidateMyEnvironment : ICanValidateMyEnvironment
    {
        public int ValidateEnvironment_CallCount;

        public void ValidateEnvironment()
        {
            ValidateEnvironment_CallCount++;
        }
    }
}
