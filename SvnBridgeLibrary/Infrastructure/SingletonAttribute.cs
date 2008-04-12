using System;

namespace SvnBridge.Infrastructure
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class SingletonAttribute : Attribute
	{ }
}