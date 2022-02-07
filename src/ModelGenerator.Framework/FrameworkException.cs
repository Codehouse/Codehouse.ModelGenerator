﻿using System;

namespace ModelGenerator.Framework
{
    public class FrameworkException : Exception
    {
        public FrameworkException(string message) : base(message)
        {
        }

        public FrameworkException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}