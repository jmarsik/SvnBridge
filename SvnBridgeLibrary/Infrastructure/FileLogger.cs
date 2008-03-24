using System;
using System.Reflection;
using SvnBridge.Exceptions;
using SvnBridge.Interfaces;
using System.IO;

namespace SvnBridge.Infrastructure
{
    public class FileLogger : ILogger, ICanValidateMyEnvironment
    {
        private static void Write(string level, Action<TextWriter> action)
        {
        	try
        	{
        		using (TextWriter tw = new StreamWriter(GetLogPath(level), true))
        		{
        			tw.WriteLine(level);
        			tw.WriteLine(DateTime.Now.ToString("yyyy MM dd hh:mm:ss:fff"));
        			action(tw);
        			tw.WriteLine("- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -");
					tw.Flush();
        		}
        	}
        	catch (Exception)
        	{
        		// we do not allow the logger to throw errors
        	}
        }

    	private static string GetLogPath(string level)
    	{
    		return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, level + ".log");
    	}

    	public void Error(string message, Exception exception)
        {
            Write("Error", delegate(TextWriter writer)
            {
                writer.WriteLine(message);
                writer.WriteLine(exception);
            });
        }

        public void Info(string message, Exception exception)
        {

            Write("Info", delegate(TextWriter writer)
            {
                writer.WriteLine(message);
                writer.WriteLine(exception);
            });
        }

    	public void ValidateEnvironment()
    	{
    		MethodInfo[] levels = typeof (FileLogger).GetMethods(BindingFlags.Public | BindingFlags.Instance);
    		foreach (MethodInfo info in levels)
    		{
				if (info.Name == "ValidateEnvironment")
					continue;
				if(info.DeclaringType==typeof(object))
					continue;
    			try
    			{
					using (TextWriter tw = new StreamWriter(GetLogPath(info.Name), true))
					{
					}
    			}
				catch(Exception)
				{
					throw new EnvironmentValidationException("Could not write log to: " + GetLogPath(info.Name));
				}
    		}
    	}
    }
}